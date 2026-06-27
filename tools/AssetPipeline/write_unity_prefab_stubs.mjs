#!/usr/bin/env node
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();
const manifestPath = path.join(repo, "artifacts/Reports/fourfold-model-pack.json");
const reportJsonPath = path.join(repo, "artifacts/Reports/unity-prefab-stub-generation.json");
const reportMdPath = path.join(repo, "artifacts/Reports/unity-prefab-stub-generation.md");

const MODEL_ROOT_TRANSFORM_FILE_ID = "-8679921383154817045";
const MODEL_ROOT_GAMEOBJECT_FILE_ID = "919132149155446097";
const MODEL_SOURCE_PREFAB_FILE_ID = "100100000";
const STATIC_RENDERING_FLAGS = 22;
const STATIC_NAV_FLAGS = 30;

if (!fs.existsSync(manifestPath)) {
  console.error(`Missing manifest: ${path.relative(repo, manifestPath)}`);
  process.exit(1);
}

const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
if (!Array.isArray(manifest.assets) || manifest.assets.length === 0) {
  console.error("Manifest has no assets.");
  process.exit(1);
}

const stats = {
  asset_count: manifest.assets.length,
  model_meta_created: 0,
  model_meta_preserved: 0,
  material_meta_created: 0,
  material_meta_preserved: 0,
  prefab_created: 0,
  prefab_updated: 0,
  prefab_meta_created: 0,
  prefab_meta_preserved: 0,
  meta_settings_updated: 0,
  folder_meta_created: 0,
  folder_meta_preserved: 0,
  no_collider_prefabs: 0,
  trigger_prefabs: 0,
  static_prefabs: 0,
  nav_blocking_prefabs: 0,
  errors: []
};

for (const asset of manifest.assets) {
  try {
    writeAsset(asset);
  } catch (error) {
    stats.errors.push(`${asset.name ?? asset.asset_id ?? "unknown"}: ${error.message}`);
  }
}

writeReport();

if (stats.errors.length > 0) {
  console.error(stats.errors.map((line) => `- ${line}`).join("\n"));
  process.exit(1);
}

console.log(
  `FOURFOLD Unity prefab stubs generated: ${stats.asset_count} assets, ` +
  `${stats.prefab_created} created, ${stats.prefab_updated} updated.`
);

function writeAsset(asset) {
  assertRelativeAssetPath(asset.model_file, `${asset.name}.model_file`);
  assertRelativeAssetPath(asset.unity_prefab, `${asset.name}.unity_prefab`);

  const modelPath = path.join(repo, asset.model_file);
  if (!fs.existsSync(modelPath)) {
    throw new Error(`model file is missing: ${asset.model_file}`);
  }

  ensureFolderChain(path.dirname(asset.model_file));
  ensureFolderChain(path.dirname(asset.unity_prefab));

  const modelGuid = ensureMeta(
    `${asset.model_file}.meta`,
    "model",
    (guid) => modelMeta(guid, asset)
  );

  const materialPath = asset.model_file.replace(/\.obj$/i, ".mtl");
  if (materialPath === asset.model_file) {
    throw new Error(`model_file must be an OBJ path for stub generation: ${asset.model_file}`);
  }
  if (!fs.existsSync(path.join(repo, materialPath))) {
    throw new Error(`material file is missing: ${materialPath}`);
  }
  ensureMeta(`${materialPath}.meta`, "material", defaultImporterMeta);

  const prefabPath = path.join(repo, asset.unity_prefab);
  const existed = fs.existsSync(prefabPath);
  fs.writeFileSync(prefabPath, prefabYaml(asset, modelGuid));
  if (existed) {
    stats.prefab_updated += 1;
  } else {
    stats.prefab_created += 1;
  }
  ensureMeta(`${asset.unity_prefab}.meta`, "prefab", prefabImporterMeta);
}

function ensureMeta(relativeMetaPath, kind, factory) {
  assertRelativeAssetPath(relativeMetaPath, relativeMetaPath);
  const fullPath = path.join(repo, relativeMetaPath);
  const existing = readGuid(fullPath);
  if (existing) {
    const nextContent = factory(existing);
    const previousContent = fs.readFileSync(fullPath, "utf8");
    if (previousContent !== nextContent) {
      fs.writeFileSync(fullPath, nextContent);
      stats.meta_settings_updated += 1;
    }
    increment(`${kind}_meta_preserved`);
    return existing;
  }

  const guid = deterministicGuid(relativeMetaPath);
  fs.writeFileSync(fullPath, factory(guid));
  increment(`${kind}_meta_created`);
  return guid;
}

function ensureFolderChain(relativeDir) {
  const parts = relativeDir.split(/[\\/]+/).filter(Boolean);
  let current = "";
  for (const part of parts) {
    current = current ? `${current}/${part}` : part;
    const fullDir = path.join(repo, current);
    if (!fs.existsSync(fullDir)) {
      fs.mkdirSync(fullDir);
    }
    const metaPath = `${current}.meta`;
    const fullMetaPath = path.join(repo, metaPath);
    if (fs.existsSync(fullMetaPath)) {
      stats.folder_meta_preserved += 1;
      continue;
    }
    fs.writeFileSync(fullMetaPath, folderMeta(deterministicGuid(metaPath)));
    stats.folder_meta_created += 1;
  }
}

function readGuid(fullMetaPath) {
  if (!fs.existsSync(fullMetaPath)) {
    return null;
  }
  const match = fs.readFileSync(fullMetaPath, "utf8").match(/^guid:\s*([0-9a-f]{32})\s*$/m);
  if (!match) {
    throw new Error(`existing meta has no guid: ${path.relative(repo, fullMetaPath)}`);
  }
  return match[1];
}

function deterministicGuid(seed) {
  return crypto.createHash("md5").update(`fourfold-echoes:${seed}`).digest("hex");
}

function increment(key) {
  stats[key] = (stats[key] ?? 0) + 1;
}

function assertRelativeAssetPath(value, label) {
  if (!value || typeof value !== "string") {
    throw new Error(`${label} is empty`);
  }
  if (path.isAbsolute(value) || value.includes("..") || !value.startsWith("Assets/")) {
    throw new Error(`${label} must be a repo-relative Assets path, got: ${value}`);
  }
}

function prefabYaml(asset, modelGuid) {
  const ids = prefabIds(asset.name);
  const collider = colliderFor(asset);
  const sockets = socketObjectsFor(asset);
  const staticFlags = asset.static_hint ? (asset.nav_blocking ? STATIC_NAV_FLAGS : STATIC_RENDERING_FLAGS) : 0;
  if (asset.static_hint) stats.static_prefabs += 1;
  if (asset.nav_blocking) stats.nav_blocking_prefabs += 1;
  if (collider?.isTrigger) stats.trigger_prefabs += 1;
  if (!collider) stats.no_collider_prefabs += 1;

  const components = [
    `  - component: {fileID: ${ids.transform}}`,
    collider ? `  - component: {fileID: ${ids.collider}}` : null
  ].filter(Boolean).join("\n");

  return [
    "%YAML 1.1",
    "%TAG !u! tag:unity3d.com,2011:",
    `--- !u!1 &${ids.root}`,
    "GameObject:",
    "  m_ObjectHideFlags: 0",
    "  m_CorrespondingSourceObject: {fileID: 0}",
    "  m_PrefabInstance: {fileID: 0}",
    "  m_PrefabAsset: {fileID: 0}",
    "  serializedVersion: 6",
    "  m_Component:",
    components,
    "  m_Layer: 0",
    `  m_Name: ${asset.name}`,
    "  m_TagString: Untagged",
    "  m_Icon: {fileID: 0}",
    "  m_NavMeshLayer: 0",
    `  m_StaticEditorFlags: ${staticFlags}`,
    "  m_IsActive: 1",
    transformYaml(ids, sockets),
    collider ? colliderYaml(ids, collider) : null,
    ...sockets.flatMap((socket) => socketYaml(socket, ids)),
    prefabInstanceYaml(ids, modelGuid),
    strippedTransformYaml(ids, modelGuid),
    ""
  ].filter((line) => line !== null).join("\n");
}

function prefabIds(name) {
  const base = hashInt(name, 9000000000000n, 1000000000000n);
  return {
    root: `${base + 1n}`,
    transform: `${base + 2n}`,
    collider: `${base + 3n}`,
    prefabInstance: `${base + 4n}`,
    visualTransform: `${base + 5n}`
  };
}

function hashInt(seed, modulo, offset) {
  const hex = crypto.createHash("sha1").update(seed).digest("hex").slice(0, 14);
  return (BigInt(`0x${hex}`) % modulo) + offset;
}

function transformYaml(ids, sockets = []) {
  const children = [
    `  - {fileID: ${ids.visualTransform}}`,
    ...sockets.map((socket) => `  - {fileID: ${socket.transform}}`)
  ];
  return [
    `--- !u!4 &${ids.transform}`,
    "Transform:",
    "  m_ObjectHideFlags: 0",
    "  m_CorrespondingSourceObject: {fileID: 0}",
    "  m_PrefabInstance: {fileID: 0}",
    "  m_PrefabAsset: {fileID: 0}",
    `  m_GameObject: {fileID: ${ids.root}}`,
    "  serializedVersion: 2",
    "  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}",
    "  m_LocalPosition: {x: 0, y: 0, z: 0}",
    "  m_LocalScale: {x: 1, y: 1, z: 1}",
    "  m_ConstrainProportionsScale: 0",
    "  m_Children:",
    ...children,
    "  m_Father: {fileID: 0}",
    "  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}"
  ].join("\n");
}

function socketObjectsFor(asset) {
  if (!Array.isArray(asset.socket_plan)) {
    return [];
  }
  return asset.socket_plan
    .filter((socket) => socket && typeof socket.name === "string" && socket.position)
    .map((socket) => {
      const base = hashInt(`${asset.name}:${socket.name}`, 9000000000000n, 2000000000000n);
      return {
        gameObject: `${base + 1n}`,
        transform: `${base + 2n}`,
        name: socket.name,
        position: {
          x: Number(socket.position.x ?? 0),
          y: Number(socket.position.y ?? 0),
          z: Number(socket.position.z ?? 0)
        }
      };
    });
}

function socketYaml(socket, ids) {
  return [
    `--- !u!1 &${socket.gameObject}`,
    "GameObject:",
    "  m_ObjectHideFlags: 0",
    "  m_CorrespondingSourceObject: {fileID: 0}",
    "  m_PrefabInstance: {fileID: 0}",
    "  m_PrefabAsset: {fileID: 0}",
    "  serializedVersion: 6",
    "  m_Component:",
    `  - component: {fileID: ${socket.transform}}`,
    "  m_Layer: 0",
    `  m_Name: ${socket.name}`,
    "  m_TagString: Untagged",
    "  m_Icon: {fileID: 0}",
    "  m_NavMeshLayer: 0",
    "  m_StaticEditorFlags: 0",
    "  m_IsActive: 1",
    `--- !u!4 &${socket.transform}`,
    "Transform:",
    "  m_ObjectHideFlags: 0",
    "  m_CorrespondingSourceObject: {fileID: 0}",
    "  m_PrefabInstance: {fileID: 0}",
    "  m_PrefabAsset: {fileID: 0}",
    `  m_GameObject: {fileID: ${socket.gameObject}}`,
    "  serializedVersion: 2",
    "  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}",
    `  m_LocalPosition: {x: ${fmt(socket.position.x)}, y: ${fmt(socket.position.y)}, z: ${fmt(socket.position.z)}}`,
    "  m_LocalScale: {x: 1, y: 1, z: 1}",
    "  m_ConstrainProportionsScale: 0",
    "  m_Children: []",
    `  m_Father: {fileID: ${ids.transform}}`,
    "  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}"
  ].join("\n");
}

function colliderYaml(ids, collider) {
  if (collider.type === "capsule") {
    return [
      `--- !u!136 &${ids.collider}`,
      "CapsuleCollider:",
      ...colliderHeader(ids, collider.isTrigger),
      "  serializedVersion: 2",
      `  m_Radius: ${fmt(collider.radius)}`,
      `  m_Height: ${fmt(collider.height)}`,
      "  m_Direction: 1",
      `  m_Center: {x: 0, y: ${fmt(collider.centerY)}, z: 0}`
    ].join("\n");
  }

  return [
    `--- !u!65 &${ids.collider}`,
    "BoxCollider:",
    ...colliderHeader(ids, collider.isTrigger),
    "  serializedVersion: 3",
    `  m_Size: {x: ${fmt(collider.size.x)}, y: ${fmt(collider.size.y)}, z: ${fmt(collider.size.z)}}`,
    `  m_Center: {x: 0, y: ${fmt(collider.centerY)}, z: 0}`
  ].join("\n");
}

function colliderHeader(ids, isTrigger = false) {
  return [
    "  m_ObjectHideFlags: 0",
    "  m_CorrespondingSourceObject: {fileID: 0}",
    "  m_PrefabInstance: {fileID: 0}",
    "  m_PrefabAsset: {fileID: 0}",
    `  m_GameObject: {fileID: ${ids.root}}`,
    "  m_Material: {fileID: 0}",
    "  m_IncludeLayers:",
    "    serializedVersion: 2",
    "    m_Bits: 0",
    "  m_ExcludeLayers:",
    "    serializedVersion: 2",
    "    m_Bits: 0",
    "  m_LayerOverridePriority: 0",
    `  m_IsTrigger: ${isTrigger ? 1 : 0}`,
    "  m_ProvidesContacts: 0",
    "  m_Enabled: 1"
  ];
}

function prefabInstanceYaml(ids, modelGuid) {
  const transformTarget = `{fileID: ${MODEL_ROOT_TRANSFORM_FILE_ID}, guid: ${modelGuid}, type: 3}`;
  const gameObjectTarget = `{fileID: ${MODEL_ROOT_GAMEOBJECT_FILE_ID}, guid: ${modelGuid}, type: 3}`;
  const transformProperties = [
    ["m_LocalPosition.x", "-0"],
    ["m_LocalPosition.y", "0"],
    ["m_LocalPosition.z", "0"],
    ["m_LocalRotation.w", "1"],
    ["m_LocalRotation.x", "0"],
    ["m_LocalRotation.y", "-0"],
    ["m_LocalRotation.z", "-0"],
    ["m_LocalEulerAnglesHint.x", "0"],
    ["m_LocalEulerAnglesHint.y", "0"],
    ["m_LocalEulerAnglesHint.z", "0"]
  ];
  const modifications = transformProperties.flatMap(([property, value]) => [
    `    - target: ${transformTarget}`,
    `      propertyPath: ${property}`,
    `      value: ${value}`,
    "      objectReference: {fileID: 0}"
  ]);
  modifications.push(
    `    - target: ${gameObjectTarget}`,
    "      propertyPath: m_Name",
    "      value: Visual",
    "      objectReference: {fileID: 0}"
  );

  return [
    `--- !u!1001 &${ids.prefabInstance}`,
    "PrefabInstance:",
    "  m_ObjectHideFlags: 0",
    "  serializedVersion: 2",
    "  m_Modification:",
    "    serializedVersion: 3",
    `    m_TransformParent: {fileID: ${ids.transform}}`,
    "    m_Modifications:",
    ...modifications,
    "    m_RemovedComponents: []",
    "    m_RemovedGameObjects: []",
    "    m_AddedGameObjects: []",
    "    m_AddedComponents: []",
    `  m_SourcePrefab: {fileID: ${MODEL_SOURCE_PREFAB_FILE_ID}, guid: ${modelGuid}, type: 3}`
  ].join("\n");
}

function strippedTransformYaml(ids, modelGuid) {
  return [
    `--- !u!4 &${ids.visualTransform} stripped`,
    "Transform:",
    `  m_CorrespondingSourceObject: {fileID: ${MODEL_ROOT_TRANSFORM_FILE_ID}, guid: ${modelGuid}, type: 3}`,
    `  m_PrefabInstance: {fileID: ${ids.prefabInstance}}`,
    "  m_PrefabAsset: {fileID: 0}"
  ].join("\n");
}

function colliderFor(asset) {
  const profile = asset.collision_profile || "box_set_dressing";
  const size = colliderSize(asset);
  switch (profile) {
    case "capsule_actor": {
      const height = Math.max(size.y, 0.8);
      return {
        type: "capsule",
        height,
        radius: Math.max(Math.max(size.x, size.z) * 0.28, 0.18),
        centerY: height * 0.5
      };
    }
    case "boss_capsule": {
      const height = Math.max(size.y, 1.2);
      return {
        type: "capsule",
        height,
        radius: Math.max(Math.max(size.x, size.z) * 0.32, 0.5),
        centerY: height * 0.5
      };
    }
    case "trigger_pickup": {
      const boxSize = {
        x: Math.max(size.x, 0.5),
        y: Math.max(size.y, 0.5),
        z: Math.max(size.z, 0.5)
      };
      return { type: "box", size: boxSize, centerY: boxSize.y * 0.5, isTrigger: true };
    }
    case "box_floor_thin": {
      const boxSize = {
        x: Math.max(size.x, 0.25),
        y: clamp(size.y, 0.08, 0.35),
        z: Math.max(size.z, 0.25)
      };
      return { type: "box", size: boxSize, centerY: boxSize.y * 0.5 };
    }
    case "box_boundary":
    case "box_set_dressing": {
      const min = profile === "box_boundary" ? 0.35 : 0.35;
      const boxSize = {
        x: Math.max(size.x, min),
        y: Math.max(size.y, min),
        z: Math.max(size.z, min)
      };
      return { type: "box", size: boxSize, centerY: boxSize.y * 0.5 };
    }
    case "box_interactable": {
      const boxSize = {
        x: Math.max(size.x, 0.55),
        y: Math.max(size.y, 0.55),
        z: Math.max(size.z, 0.55)
      };
      return { type: "box", size: boxSize, centerY: boxSize.y * 0.5 };
    }
    case "no_collider_equipment":
    case "no_collider_visual_detail":
      return null;
    default:
      throw new Error(`unsupported collision_profile=${profile}`);
  }
}

function colliderSize(asset) {
  const bounds = asset.bounds_m ?? {};
  const footprint = asset.footprint_m ?? {};
  const x = positive(bounds.x, 0.9);
  const y = positive(bounds.z, 0.8);
  const z = positive(footprint.z, positive(bounds.y, 0.9));
  return { x, y, z };
}

function positive(value, fallback) {
  const number = Number(value);
  return Number.isFinite(number) && number > 0 ? number : fallback;
}

function clamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}

function fmt(value) {
  if (!Number.isFinite(value)) {
    throw new Error(`invalid numeric value: ${value}`);
  }
  return Number(value.toFixed(4)).toString();
}

function modelMeta(guid, asset) {
  const generateSecondaryUV = asset.static_hint ? 1 : 0;
  const normalMode = asset.asset_kind === "Detail" || asset.asset_kind === "GroundDecal" ? 1 : 0;
  return `fileFormatVersion: 2
guid: ${guid}
ModelImporter:
  serializedVersion: 24200
  internalIDToNameTable: []
  externalObjects: {}
  materials:
    materialImportMode: 2
    materialName: 0
    materialSearch: 1
    materialLocation: 1
  animations:
    legacyGenerateAnimations: 4
    bakeSimulation: 0
    resampleCurves: 1
    optimizeGameObjects: 0
    removeConstantScaleCurves: 0
    motionNodeName: 
    animationImportErrors: 
    animationImportWarnings: 
    animationRetargetingWarnings: 
    animationDoRetargetingWarnings: 0
    importAnimatedCustomProperties: 0
    importConstraints: 0
    animationCompression: 1
    animationRotationError: 0.5
    animationPositionError: 0.5
    animationScaleError: 0.5
    animationWrapMode: 0
    extraExposedTransformPaths: []
    extraUserProperties: []
    clipAnimations: []
    isReadable: 0
  meshes:
    lODScreenPercentages: []
    globalScale: 1
    meshCompression: 0
    addColliders: 0
    useSRGBMaterialColor: 1
    sortHierarchyByName: 1
    importPhysicalCameras: 0
    importVisibility: 0
    importBlendShapes: 1
    importCameras: 0
    importLights: 0
    nodeNameCollisionStrategy: 1
    fileIdsGeneration: 2
    swapUVChannels: 0
    generateSecondaryUV: ${generateSecondaryUV}
    useFileUnits: 1
    keepQuads: 0
    weldVertices: 1
    bakeAxisConversion: 0
    preserveHierarchy: 0
    skinWeightsMode: 0
    maxBonesPerVertex: 4
    minBoneWeight: 0.001
    optimizeBones: 1
    generateMeshLods: 0
    meshLodGenerationFlags: 0
    maximumMeshLod: -1
    meshOptimizationFlags: -1
    indexFormat: 0
    secondaryUVAngleDistortion: 8
    secondaryUVAreaDistortion: 15.000001
    secondaryUVHardAngle: 88
    secondaryUVMarginMethod: 1
    secondaryUVMinLightmapResolution: 40
    secondaryUVMinObjectScale: 1
    secondaryUVPackMargin: 4
    useFileScale: 1
    strictVertexDataChecks: 0
  tangentSpace:
    normalSmoothAngle: 60
    normalImportMode: ${normalMode}
    tangentImportMode: 3
    normalCalculationMode: 4
    legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes: 0
    blendShapeNormalImportMode: 1
    normalSmoothingSource: 0
  referencedClips: []
  importAnimation: 0
  humanDescription:
    serializedVersion: 3
    human: []
    skeleton: []
    armTwist: 0.5
    foreArmTwist: 0.5
    upperLegTwist: 0.5
    legTwist: 0.5
    armStretch: 0.05
    legStretch: 0.05
    feetSpacing: 0
    globalScale: 1
    rootMotionBoneName: 
    hasTranslationDoF: 0
    hasExtraRoot: 0
    skeletonHasParents: 1
  lastHumanDescriptionAvatarSource: {instanceID: 0}
  autoGenerateAvatarMappingIfUnspecified: 1
  animationType: 2
  humanoidOversampling: 1
  avatarSetup: 0
  addHumanoidExtraRootOnlyWhenUsingAvatar: 1
  importBlendShapeDeformPercent: 1
  remapMaterialsIfMaterialImportModeIsNone: 0
  additionalBone: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
`;
}

function defaultImporterMeta(guid) {
  return `fileFormatVersion: 2
guid: ${guid}
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
`;
}

function prefabImporterMeta(guid) {
  return `fileFormatVersion: 2
guid: ${guid}
PrefabImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
`;
}

function folderMeta(guid) {
  return `fileFormatVersion: 2
guid: ${guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
`;
}

function writeReport() {
  const report = {
    version: 1,
    generator: "tools/AssetPipeline/write_unity_prefab_stubs.mjs",
    manifest: "artifacts/Reports/fourfold-model-pack.json",
    note: "Deterministic Unity prefab/meta stubs for offline validation. Unity Editor import still remains the authoritative integration step when licensing is available.",
    static_flag_policy: {
      rendering_static_flags: STATIC_RENDERING_FLAGS,
      nav_blocking_static_flags: STATIC_NAV_FLAGS,
      nav_bit: 8
    },
    stats
  };
  fs.writeFileSync(reportJsonPath, JSON.stringify(report, null, 2) + "\n");
  fs.writeFileSync(reportMdPath, [
    "# Unity Prefab Stub Generation",
    "",
    `- Assets processed: ${stats.asset_count}`,
    `- Prefabs created: ${stats.prefab_created}`,
    `- Prefabs updated: ${stats.prefab_updated}`,
    `- Model metas created: ${stats.model_meta_created}`,
    `- Material metas created: ${stats.material_meta_created}`,
    `- Prefab metas created: ${stats.prefab_meta_created}`,
    `- Existing meta settings updated with preserved GUIDs: ${stats.meta_settings_updated}`,
    `- Static prefabs: ${stats.static_prefabs}`,
    `- Nav-blocking prefabs: ${stats.nav_blocking_prefabs}`,
    `- Trigger prefabs: ${stats.trigger_prefabs}`,
    `- No-collider prefabs: ${stats.no_collider_prefabs}`,
    "",
    "Unity Editor import was not executed by this script. These stubs exist so the repository can keep deterministic prefab references and run postimport-style validation while Unity licensing is unavailable.",
    ""
  ].join("\n"));
}
