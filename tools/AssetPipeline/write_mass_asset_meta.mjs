#!/usr/bin/env node
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();
const manifestPath = path.join(repo, "asset_manifest.json");
const reportPath = path.join(repo, "artifacts/Reports/mass-asset-meta-generation.json");

if (!fs.existsSync(manifestPath)) {
  console.error("Missing asset_manifest.json");
  process.exit(1);
}

const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
const stats = {
  assets: Array.isArray(manifest.assets) ? manifest.assets.length : 0,
  file_meta_created: 0,
  file_meta_preserved: 0,
  folder_meta_created: 0,
  folder_meta_preserved: 0,
  errors: []
};

for (const asset of manifest.assets ?? []) {
  try {
    writeAssetMetas(asset);
  } catch (error) {
    stats.errors.push(`${asset.contract_name ?? asset.asset_id ?? "unknown"}: ${error.message}`);
  }
}

fs.mkdirSync(path.dirname(reportPath), { recursive: true });
fs.writeFileSync(reportPath, `${JSON.stringify(stats, null, 2)}\n`);

if (stats.errors.length > 0) {
  console.error(stats.errors.map((line) => `- ${line}`).join("\n"));
  process.exit(1);
}

console.log(
  `Mass asset meta generated: ${stats.file_meta_created} created, ` +
  `${stats.file_meta_preserved} preserved, ${stats.folder_meta_created} folder meta created.`
);

function writeAssetMetas(asset) {
  const fileEntries = [];
  for (const value of Object.values(asset.final_files ?? {})) {
    fileEntries.push({ path: value, kind: inferKind(value, asset) });
  }
  for (const value of Object.values(asset.textures ?? {})) {
    fileEntries.push({ path: value, kind: inferKind(value, asset) });
  }

  for (const entry of fileEntries) {
    assertRelativeAssetPath(entry.path);
    const fullPath = path.join(repo, entry.path);
    if (!fs.existsSync(fullPath)) {
      throw new Error(`file is missing: ${entry.path}`);
    }
    ensureFolderChain(path.dirname(entry.path));
    ensureMeta(`${entry.path}.meta`, (guid) => metaFor(guid, entry.path, entry.kind, asset));
  }
}

function inferKind(relativePath, asset) {
  const lower = relativePath.toLowerCase();
  if (lower.endsWith(".fbx")) return "model";
  if (lower.endsWith(".wav")) return "audio";
  if (lower.endsWith(".anim")) return "animation";
  if (lower.endsWith(".png") || lower.endsWith(".tga")) {
    if (asset.category === "ui" || lower.includes("/ui/icons/")) return "ui_texture";
    if (asset.category === "vfx" || lower.includes("/vfx/")) return "vfx_texture";
    if (lower.endsWith("_n.png") || lower.endsWith("_n.tga")) return "normal_texture";
    if (lower.endsWith("_orm.png") || lower.endsWith("_orm.tga") || lower.endsWith("_msk.png") || lower.endsWith("_msk.tga")) {
      return "linear_texture";
    }
    return "color_texture";
  }
  return "default";
}

function metaFor(guid, relativePath, kind, asset) {
  switch (kind) {
    case "model":
      return modelMeta(guid, asset);
    case "audio":
      return audioMeta(guid);
    case "animation":
      return animationMeta(guid);
    case "ui_texture":
      return textureMeta(guid, { textureType: 8, mipmap: 0, srgb: 1, alpha: 1, spriteMode: 1 });
    case "vfx_texture":
      return textureMeta(guid, { textureType: 0, mipmap: 0, srgb: 1, alpha: 1, spriteMode: 0 });
    case "normal_texture":
      return textureMeta(guid, { textureType: 1, mipmap: 1, srgb: 0, alpha: 0, spriteMode: 0 });
    case "linear_texture":
      return textureMeta(guid, { textureType: 0, mipmap: 1, srgb: 0, alpha: 0, spriteMode: 0 });
    case "color_texture":
      return textureMeta(guid, { textureType: 0, mipmap: 1, srgb: 1, alpha: 0, spriteMode: 0 });
    default:
      return defaultMeta(guid);
  }
}

function ensureMeta(relativeMetaPath, factory) {
  assertRelativeAssetPath(relativeMetaPath);
  const fullPath = path.join(repo, relativeMetaPath);
  const existing = readGuid(fullPath);
  if (existing) {
    stats.file_meta_preserved += 1;
    return existing;
  }
  const guid = deterministicGuid(relativeMetaPath);
  fs.writeFileSync(fullPath, factory(guid));
  stats.file_meta_created += 1;
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
  if (!fs.existsSync(fullMetaPath)) return null;
  const match = fs.readFileSync(fullMetaPath, "utf8").match(/^guid:\s*([0-9a-f]{32})\s*$/m);
  if (!match) {
    throw new Error(`existing meta has no guid: ${path.relative(repo, fullMetaPath)}`);
  }
  return match[1];
}

function deterministicGuid(seed) {
  return crypto.createHash("md5").update(`fourfold-echoes:mass-assets:${seed}`).digest("hex");
}

function modelMeta(guid, asset) {
  const category = asset.category ?? "";
  const isStatic = category === "environment" || category === "prop";
  const animationType = category === "player" || category === "npc" ? 3 : (category === "enemy" || category === "elite" || category === "boss" ? 2 : 0);
  return `fileFormatVersion: 2
guid: ${guid}
ModelImporter:
  serializedVersion: 24200
  internalIDToNameTable: []
  externalObjects: {}
  materials:
    materialImportMode: 1
    materialName: 0
    materialSearch: 1
    materialLocation: 1
  animations:
    legacyGenerateAnimations: 4
    bakeSimulation: 0
    resampleCurves: 1
    optimizeGameObjects: 0
    removeConstantScaleCurves: 0
    animationCompression: 1
    animationRotationError: 0.5
    animationPositionError: 0.5
    animationScaleError: 0.5
    animationWrapMode: 0
    clipAnimations: []
    isReadable: 0
  meshes:
    globalScale: 1
    meshCompression: 0
    addColliders: 0
    importVisibility: 0
    importBlendShapes: ${category === "player" || category === "npc" ? 1 : 0}
    importCameras: 0
    importLights: 0
    generateSecondaryUV: ${isStatic ? 1 : 0}
    useFileScale: 1
    weldVertices: 1
    preserveHierarchy: 0
    maxBonesPerVertex: 4
  tangentSpace:
    normalImportMode: 0
    tangentImportMode: 3
    normalCalculationMode: 4
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
  animationType: ${animationType}
  avatarSetup: 0
  userData:
  assetBundleName:
  assetBundleVariant:
`;
}

function textureMeta(guid, options) {
  return `fileFormatVersion: 2
guid: ${guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: ${options.mipmap}
    sRGBTexture: ${options.srgb}
    linearTexture: ${options.srgb ? 0 : 1}
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
  isReadable: 0
  streamingMipmaps: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 1
  lightmap: 0
  compressionQuality: 50
  spriteMode: ${options.spriteMode}
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 100
  alphaUsage: ${options.alpha}
  alphaIsTransparency: ${options.alpha}
  textureType: ${options.textureType}
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  platformSettings: []
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID:
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
  userData:
  assetBundleName:
  assetBundleVariant:
`;
}

function audioMeta(guid) {
  return `fileFormatVersion: 2
guid: ${guid}
AudioImporter:
  externalObjects: {}
  serializedVersion: 8
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 1
    quality: 1
    conversionMode: 0
    preloadAudioData: 1
  platformSettingOverrides: {}
  forceToMono: 1
  normalize: 1
  loadInBackground: 0
  ambisonic: 0
  3D: 1
  userData:
  assetBundleName:
  assetBundleVariant:
`;
}

function animationMeta(guid) {
  return `fileFormatVersion: 2
guid: ${guid}
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 7400000
  userData:
  assetBundleName:
  assetBundleVariant:
`;
}

function defaultMeta(guid) {
  return `fileFormatVersion: 2
guid: ${guid}
DefaultImporter:
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

function assertRelativeAssetPath(value) {
  if (!value || typeof value !== "string") {
    throw new Error("asset path is empty");
  }
  if (path.isAbsolute(value) || value.includes("..") || !value.startsWith("Assets/")) {
    throw new Error(`asset path must be repo-relative under Assets/: ${value}`);
  }
}
