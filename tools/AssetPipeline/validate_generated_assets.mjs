#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();
const manifestPath = path.join(repo, "artifacts/Reports/fourfold-model-pack.json");
const promptContractPath = path.join(repo, "artifacts/Reports/fourfold-asset-prompt-contract.jsonl");
const visualBenchmarkPath = path.join(repo, "artifacts/Reports/visual-benchmark.json");
const consistencyJsonPath = path.join(repo, "artifacts/Reports/generated-asset-consistency.json");
const consistencyMdPath = path.join(repo, "artifacts/Reports/generated-asset-consistency.md");
const phaseArg = process.argv.find((arg) => arg.startsWith("--phase="));
const phase = phaseArg ? phaseArg.slice("--phase=".length) : "postimport";
const requireUnityImport = phase !== "preimport";
const errors = [];
const warnings = [];
const requiredMetaPaths = new Set();
const NAVIGATION_STATIC_BIT = 8;
const STATIC_RENDERING_BITS = 22;
const forbiddenTerms = [
  "final fantasy",
  "ff-style",
  "octopath",
  "hd-2d",
  "dragon quest",
  "maplestory",
  "maplestory2",
  "maple story",
  "akira toriyama",
  "square enix",
  "nintendo",
  "ファイナルファンタジー",
  "ドラクエ",
  "鳥山",
  "ff風",
  "dq風",
  "chocobo",
  "moogle",
  "cactuar",
  "tonberry"
];
const forbiddenPromptPatterns = [
  /\bin the style of\b/i,
  /\binspired by\b/i,
  /\bsimilar to\b/i,
  /\blike\s+[A-Z][A-Za-z0-9_-]+/i,
  /\baverage of\b/i,
  /\bmix of\b/i,
  /\bblend of\b/i,
  /\bfusion of\b/i,
  /〜風/,
  /っぽい/,
  /画風/,
  /作風/,
];

if (!fs.existsSync(manifestPath)) {
  fail(`Missing generated model manifest: ${path.relative(repo, manifestPath)}`);
}

const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
if (!Array.isArray(manifest.assets) || manifest.assets.length === 0) {
  fail("Generated model manifest has no assets.");
}
requiredRoot(manifest, "art_direction_id");
requiredRoot(manifest, "art_direction_name");
requiredRoot(manifest, "art_direction_contract");
requiredRoot(manifest, "brand_line_id");
requiredRoot(manifest, "product_line_contract");
requiredRoot(manifest, "genre_contract_id");
requiredRoot(manifest, "shape_grammar_id");
requiredRoot(manifest, "benchmark_policy_id");
requiredRoot(manifest, "benchmark_report_path");
requiredRoot(manifest, "commercial_safety_policy_id");
requiredRoot(manifest, "consistency_summary");
requiredRoot(manifest, "style_families");
requiredRoot(manifest, "external_benchmark_use");
requiredRoot(manifest, "comparison_scope");
requiredRoot(manifest, "human_review_required");
if (manifest.art_direction_contract?.contract_id !== manifest.art_direction_id) {
  errors.push("Manifest art_direction_contract.contract_id must match art_direction_id.");
}
if (!String(manifest.external_benchmark_use ?? "").includes("quality metrics only")) {
  errors.push("Manifest must restrict external benchmark use to quality metrics only.");
}
if (manifest.brand_line_id !== "folded_reliquary_miniatures") {
  errors.push(`Manifest brand_line_id must be folded_reliquary_miniatures, got ${manifest.brand_line_id}.`);
}
if (manifest.benchmark_policy_id !== "external_market_metrics_only") {
  errors.push(`Manifest benchmark_policy_id must be external_market_metrics_only, got ${manifest.benchmark_policy_id}.`);
}

const seen = new Set();
const signatures = new Map();
for (const asset of manifest.assets) {
  required(asset, "asset_id");
  required(asset, "name");
  required(asset, "builder");
  required(asset, "style");
  required(asset, "area_code");
  required(asset, "asset_kind");
  required(asset, "archetype");
  required(asset, "brand_line_id");
  required(asset, "product_line_role");
  required(asset, "product_line_role_key");
  required(asset, "visual_family_id");
  required(asset, "shape_family_id");
  required(asset, "required_shape_tokens");
  required(asset, "forbidden_drift_tokens");
  required(asset, "motif_limit_policy");
  required(asset, "genre_contract_status");
  required(asset, "style_gate_status");
  required(asset, "commercial_safety");
  required(asset, "benchmark_coverage");
  required(asset, "model_file");
  required(asset, "preview_file");
  required(asset, "source_file");
  required(asset, "unity_prefab");
  required(asset, "license");
  required(asset, "source_reference");
  required(asset, "material_budget_status");
  required(asset, "collision_profile");
  required(asset, "approved_overwrite_policy");

  if (seen.has(asset.name)) {
    errors.push(`Duplicate generated model name: ${asset.name}`);
  }
  seen.add(asset.name);

  if (!asset.name.startsWith("FE_")) {
    errors.push(`Generated model does not use FE_ naming: ${asset.name}`);
  }
  if (!/^FE_(CHAR|PROP|ENV|ENEMY|BOSS|RELIC)_[A-Za-z0-9_]+$/.test(asset.name)) {
    errors.push(`Generated model naming is outside FOURFOLD schema: ${asset.name}`);
  }
  validateAreaStyle(asset);

  for (const key of ["model_file", "preview_file", "source_file"]) {
    const value = asset[key];
    if (path.isAbsolute(value)) {
      errors.push(`${asset.name} uses an absolute ${key}: ${value}`);
      continue;
    }
    const fullPath = path.join(repo, value);
    if (!fs.existsSync(fullPath)) {
      errors.push(`${asset.name} missing ${key}: ${value}`);
    }
  }

  if (requireUnityImport) {
    validateUnityArtifacts(asset);
  }

  if (asset.license !== "repository-authored") {
    errors.push(`${asset.name} must be repository-authored, got license=${asset.license}`);
  }

  if (Number(asset.triangles_lod0) > Number(asset.triangle_budget_lod0)) {
    errors.push(`${asset.name} exceeds triangle budget: ${asset.triangles_lod0}/${asset.triangle_budget_lod0}`);
  }

  if (Number(asset.material_count) > Number(asset.material_budget)) {
    const message = `${asset.name} exceeds material budget: ${asset.material_count}/${asset.material_budget}`;
    if (asset.material_budget_status === "first_pass_warning" && asset.budget_exception_reason) {
      warnings.push(`${message}; atlas/material optimization required before production approval`);
    } else {
      errors.push(message);
    }
  } else if (asset.material_budget_status !== "within_budget") {
    errors.push(`${asset.name} material_budget_status must be within_budget when it is inside budget.`);
  }

  if (!Array.isArray(asset.required_readability_anchors) || asset.required_readability_anchors.length === 0) {
    errors.push(`${asset.name} has no required_readability_anchors; every model needs an explicit gameplay/readability job.`);
  }
  validateCollisionProfile(asset);
  validateGeometryContract(asset);
  validateMaterialRoles(asset);
  validateArtDirectionContract(asset);
  validateCommercialSafety(asset);

  const signature = `${asset.shape_family_id}|${asset.triangles_lod0}|${asset.materials?.join(",")}|${JSON.stringify(asset.primitive_counts ?? {})}`;
  const bucket = signatures.get(signature) ?? [];
  bucket.push(asset.name);
  signatures.set(signature, bucket);

  if (!String(asset.style_ip_clearance ?? manifest.style_ip_clearance ?? "").toLowerCase().includes("no protected")) {
    warnings.push(`${asset.name} inherits style clearance, but explicit protected-style wording was not found at asset level.`);
  }

  const searchable = JSON.stringify(asset).toLowerCase();
  for (const term of forbiddenTerms) {
    if (searchable.includes(term)) {
      errors.push(`${asset.name} contains forbidden protected-style term: ${term}`);
    }
  }
}

for (const names of signatures.values()) {
  if (names.length >= 4) {
    warnings.push(`Repeated geometry/material signature across ${names.length} assets: ${names.slice(0, 6).join(", ")}${names.length > 6 ? "..." : ""}`);
  }
}

if (requireUnityImport) {
  validateRequiredMetaGuids();
}

validatePromptContract(manifest);
const benchmarkSummary = validateBenchmarkReport();
const consistencySummary = buildConsistencySummary(manifest, benchmarkSummary);

if (errors.length > 0) {
  console.error(errors.map((line) => `- ${line}`).join("\n"));
  process.exit(1);
}

writeConsistencyReport(consistencySummary);

if (warnings.length > 0) {
  console.warn(warnings.map((line) => `- WARN ${line}`).join("\n"));
}

console.log(`FOURFOLD generated asset validation passed (${phase}): ${manifest.assets.length} models.`);

function requiredRoot(manifestValue, key) {
  if (!manifestValue[key]) {
    errors.push(`Manifest missing required field: ${key}`);
  }
}

function required(asset, key) {
  if (!asset[key]) {
    errors.push(`${asset.name ?? asset.asset_id ?? "unknown asset"} missing required field: ${key}`);
  }
}

function validateUnityArtifacts(asset) {
  const modelMeta = `${asset.model_file}.meta`;
  const materialMeta = asset.model_file.replace(/\.obj$/i, ".mtl.meta");
  const prefab = asset.unity_prefab;
  const prefabMeta = `${asset.unity_prefab}.meta`;
  if (materialMeta === asset.model_file) {
    errors.push(`${asset.name} model_file must be an OBJ path for generated Unity material meta validation: ${asset.model_file}`);
  }

  const paths = [
    [modelMeta, "model_file .meta"],
    [materialMeta, "material .meta"],
    [prefab, "generated prefab"],
    [prefabMeta, "prefab .meta"]
  ];
  let allPresent = true;
  for (const [relativePath, label] of paths) {
    if (!relativePath || path.isAbsolute(relativePath) || relativePath.includes("..")) {
      errors.push(`${asset.name} has invalid ${label} path: ${relativePath}`);
      allPresent = false;
      continue;
    }
    const fullPath = path.join(repo, relativePath);
    if (!fs.existsSync(fullPath)) {
      errors.push(`${asset.name} missing Unity ${label}: ${relativePath}`);
      allPresent = false;
    }
  }
  if (!allPresent) {
    return;
  }

  requiredMetaPaths.add(modelMeta);
  requiredMetaPaths.add(materialMeta);
  requiredMetaPaths.add(prefabMeta);

  const modelGuid = readMetaGuid(modelMeta, asset.name);
  readMetaGuid(materialMeta, asset.name);
  readMetaGuid(prefabMeta, asset.name);
  if (!modelGuid) {
    return;
  }

  validateModelMeta(asset, modelMeta);
  validatePrefabYaml(asset, prefab, modelGuid);
}

function validateRequiredMetaGuids() {
  const seenGuids = new Map();
  for (const relativePath of requiredMetaPaths) {
    const guid = readMetaGuid(relativePath, relativePath);
    if (!guid) {
      continue;
    }
    const existing = seenGuids.get(guid);
    if (existing && existing !== relativePath) {
      errors.push(`Unity .meta GUID collision: ${guid} is used by ${existing} and ${relativePath}`);
    }
    seenGuids.set(guid, relativePath);
  }
}

function readMetaGuid(relativePath, owner) {
  const fullPath = path.join(repo, relativePath);
  if (!fs.existsSync(fullPath)) {
    return null;
  }
  const match = fs.readFileSync(fullPath, "utf8").match(/^guid:\s*([0-9a-f]{32})\s*$/m);
  if (!match) {
    errors.push(`${owner} Unity .meta has no valid 32-char guid: ${relativePath}`);
    return null;
  }
  return match[1];
}

function validateModelMeta(asset, relativePath) {
  const text = fs.readFileSync(path.join(repo, relativePath), "utf8");
  const requiredLines = [
    "ModelImporter:",
    "    materialImportMode: 2",
    "    addColliders: 0",
    "    importCameras: 0",
    "    importLights: 0",
    "    isReadable: 0",
    "  importAnimation: 0"
  ];
  for (const line of requiredLines) {
    if (!text.includes(line)) {
      errors.push(`${asset.name} model meta is missing required importer setting "${line.trim()}": ${relativePath}`);
    }
  }
  const expectedSecondaryUv = asset.static_hint ? 1 : 0;
  const secondaryUv = numberAfter(text, "generateSecondaryUV:");
  if (secondaryUv !== expectedSecondaryUv) {
    errors.push(`${asset.name} model meta generateSecondaryUV=${secondaryUv}, expected ${expectedSecondaryUv}: ${relativePath}`);
  }
}

function validatePrefabYaml(asset, relativePath, modelGuid) {
  const text = fs.readFileSync(path.join(repo, relativePath), "utf8");
  if (!text.includes(`  m_Name: ${asset.name}\n`)) {
    errors.push(`${asset.name} prefab root m_Name is missing or mismatched: ${relativePath}`);
  }
  const guidHits = [...text.matchAll(/guid:\s*([0-9a-f]{32})/g)].map((match) => match[1]);
  if (guidHits.length === 0) {
    errors.push(`${asset.name} prefab has no model GUID references: ${relativePath}`);
  }
  const wrongGuids = [...new Set(guidHits.filter((guid) => guid !== modelGuid))];
  if (wrongGuids.length > 0) {
    errors.push(`${asset.name} prefab references GUIDs that do not match model meta ${modelGuid}: ${wrongGuids.join(", ")}`);
  }
  if (!text.includes(`m_SourcePrefab: {fileID: 100100000, guid: ${modelGuid}, type: 3}`)) {
    errors.push(`${asset.name} prefab m_SourcePrefab does not point at the generated OBJ model: ${relativePath}`);
  }
  if (!text.includes(`m_CorrespondingSourceObject: {fileID: -8679921383154817045, guid: ${modelGuid}, type: 3}`)) {
    errors.push(`${asset.name} prefab stripped Transform does not point at the generated OBJ root Transform: ${relativePath}`);
  }
  if (!text.includes("propertyPath: m_Name") || !text.includes("value: Visual")) {
    errors.push(`${asset.name} prefab must rename the model instance to Visual: ${relativePath}`);
  }

  const staticFlags = numberAfter(text, "m_StaticEditorFlags:");
  if (asset.static_hint) {
    if (!staticFlags) {
      errors.push(`${asset.name} prefab static_hint=true but m_StaticEditorFlags is ${staticFlags}: ${relativePath}`);
    }
    if ((staticFlags & STATIC_RENDERING_BITS) !== STATIC_RENDERING_BITS) {
      errors.push(`${asset.name} prefab static flags must include rendering static bits ${STATIC_RENDERING_BITS}, got ${staticFlags}: ${relativePath}`);
    }
  } else if (staticFlags !== 0) {
    errors.push(`${asset.name} prefab static_hint=false but m_StaticEditorFlags=${staticFlags}: ${relativePath}`);
  }
  if (asset.nav_blocking && (staticFlags & NAVIGATION_STATIC_BIT) === 0) {
    errors.push(`${asset.name} nav_blocking=true but NavigationStatic bit is not set: ${relativePath}`);
  }
  if (!asset.nav_blocking && (staticFlags & NAVIGATION_STATIC_BIT) !== 0) {
    errors.push(`${asset.name} nav_blocking=false but NavigationStatic bit is set: ${relativePath}`);
  }

  validatePrefabCollider(asset, relativePath, text);
}

function validatePrefabCollider(asset, relativePath, text) {
  const expected = expectedCollider(asset);
  const hasBox = text.includes("BoxCollider:");
  const hasCapsule = text.includes("CapsuleCollider:");
  if (!expected) {
    if (hasBox || hasCapsule) {
      errors.push(`${asset.name} collision_profile=${asset.collision_profile} must not include a Collider component: ${relativePath}`);
    }
    return;
  }

  if (expected.type === "capsule") {
    if (!hasCapsule || hasBox) {
      errors.push(`${asset.name} collision_profile=${asset.collision_profile} must include only CapsuleCollider: ${relativePath}`);
      return;
    }
    expectClose(asset, relativePath, "m_Radius", numberAfter(text, "m_Radius:"), expected.radius);
    expectClose(asset, relativePath, "m_Height", numberAfter(text, "m_Height:"), expected.height);
    const center = vectorAfter(text, "m_Center:");
    expectClose(asset, relativePath, "m_Center.y", center.y, expected.centerY);
    if (numberAfter(text, "m_IsTrigger:") !== 0) {
      errors.push(`${asset.name} actor/boss collider must not be a trigger: ${relativePath}`);
    }
    return;
  }

  if (!hasBox || hasCapsule) {
    errors.push(`${asset.name} collision_profile=${asset.collision_profile} must include only BoxCollider: ${relativePath}`);
    return;
  }
  const size = vectorAfter(text, "m_Size:");
  const center = vectorAfter(text, "m_Center:");
  expectClose(asset, relativePath, "m_Size.x", size.x, expected.size.x);
  expectClose(asset, relativePath, "m_Size.y", size.y, expected.size.y);
  expectClose(asset, relativePath, "m_Size.z", size.z, expected.size.z);
  expectClose(asset, relativePath, "m_Center.y", center.y, expected.centerY);
  const trigger = numberAfter(text, "m_IsTrigger:");
  if (trigger !== (expected.isTrigger ? 1 : 0)) {
    errors.push(`${asset.name} prefab trigger flag=${trigger}, expected ${expected.isTrigger ? 1 : 0}: ${relativePath}`);
  }
}

function expectedCollider(asset) {
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
      const boxSize = {
        x: Math.max(size.x, 0.35),
        y: Math.max(size.y, 0.35),
        z: Math.max(size.z, 0.35)
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
      return null;
  }
}

function colliderSize(asset) {
  const bounds = asset.bounds_m ?? {};
  const footprint = asset.footprint_m ?? {};
  return {
    x: positive(bounds.x, 0.9),
    y: positive(bounds.z, 0.8),
    z: positive(footprint.z, positive(bounds.y, 0.9))
  };
}

function positive(value, fallback) {
  const number = Number(value);
  return Number.isFinite(number) && number > 0 ? number : fallback;
}

function clamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}

function numberAfter(text, label) {
  const escaped = label.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  const match = text.match(new RegExp(`${escaped}\\s*(-?\\d+(?:\\.\\d+)?)`));
  return match ? Number(match[1]) : NaN;
}

function vectorAfter(text, label) {
  const escaped = label.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  const match = text.match(new RegExp(`${escaped}\\s*\\{x:\\s*(-?\\d+(?:\\.\\d+)?),\\s*y:\\s*(-?\\d+(?:\\.\\d+)?),\\s*z:\\s*(-?\\d+(?:\\.\\d+)?)\\}`));
  if (!match) {
    return { x: NaN, y: NaN, z: NaN };
  }
  return { x: Number(match[1]), y: Number(match[2]), z: Number(match[3]) };
}

function expectClose(asset, relativePath, field, actual, expected) {
  if (!Number.isFinite(actual)) {
    errors.push(`${asset.name} prefab missing numeric ${field}: ${relativePath}`);
    return;
  }
  if (Math.abs(actual - expected) > 0.011) {
    errors.push(`${asset.name} prefab ${field}=${actual}, expected ${Number(expected.toFixed(4))}: ${relativePath}`);
  }
}

function validateAreaStyle(asset) {
  const areaStyle = {
    HUB: "hub",
    R01: "r01",
    R02: "r02",
    R03: "r03",
    BOSS: "boss",
    COMMON: "common"
  };
  if (!Object.hasOwn(areaStyle, asset.area_code)) {
    errors.push(`${asset.name} uses unknown area_code=${asset.area_code}`);
    return;
  }
  const expected = areaStyle[asset.area_code];
  if (asset.category !== "Boss" && asset.name.includes(`_${asset.area_code}_`) && asset.style !== expected) {
    errors.push(`${asset.name} area_code=${asset.area_code} does not match style=${asset.style}`);
  }
  if (asset.name.includes("_COMMON_") && asset.area_code !== "COMMON") {
    errors.push(`${asset.name} has COMMON name but area_code=${asset.area_code}`);
  }
}

function validateCollisionProfile(asset) {
  const allowed = new Set([
    "capsule_actor",
    "boss_capsule",
    "trigger_pickup",
    "no_collider_equipment",
    "no_collider_visual_detail",
    "box_floor_thin",
    "box_boundary",
    "box_interactable",
    "box_set_dressing"
  ]);
  if (!allowed.has(asset.collision_profile)) {
    errors.push(`${asset.name} uses unknown collision_profile=${asset.collision_profile}`);
  }
  if (["Detail", "GroundDecal"].includes(asset.asset_kind) && asset.collision_profile !== "no_collider_visual_detail") {
    errors.push(`${asset.name} detail/decal assets must not create gameplay colliders.`);
  }
  if (asset.asset_kind === "Pickup" && asset.collision_profile !== "trigger_pickup") {
    errors.push(`${asset.name} pickup assets must use trigger_pickup.`);
  }
  if (asset.asset_kind === "Equipment" && asset.collision_profile !== "no_collider_equipment") {
    errors.push(`${asset.name} equipment assets must not create blocking scene colliders.`);
  }
  if (["shortcut_bridge", "crystal_bridge", "narrow_bridge"].includes(asset.builder) || String(asset.builder ?? "").startsWith("p3_block_") && String(asset.builder ?? "").includes("bridge")) {
    if (asset.asset_kind !== "Tile" || asset.collision_profile !== "box_floor_thin" || asset.nav_blocking) {
      errors.push(`${asset.name} bridge/shortcut route assets must be walkable Tile surfaces, not nav-blocking boundaries.`);
    }
  }
  if (asset.nav_blocking && asset.asset_kind !== "Boundary") {
    errors.push(`${asset.name} nav_blocking is only allowed on true Boundary assets.`);
  }
}

function validateGeometryContract(asset) {
  const height = Number(asset.height_m ?? asset.bounds_m?.z ?? 0);
  const partCount = Number(asset.part_count ?? 0);
  const thinPartCount = Number(asset.thin_part_count ?? 0);
  const smallPartRatio = Number(asset.small_part_ratio ?? 0);
  if (height <= 0) {
    errors.push(`${asset.name} has invalid height_m=${asset.height_m}`);
  }
  const builder = String(asset.builder ?? "");
  const isBlockFieldModule = builder.startsWith("block_") || builder.startsWith("p3_block_");
  if (asset.asset_kind === "Tile" && !isBlockFieldModule && height > 0.5) {
    errors.push(`${asset.name} tile height is too high for top-down navigation: ${height}m`);
  }
  if (asset.asset_kind === "Tile" && isBlockFieldModule && height > 1.4) {
    errors.push(`${asset.name} block-field tile is too tall for readable traversal grammar: ${height}m`);
  }
  if (asset.asset_kind === "GroundDecal" && height > 0.15) {
    errors.push(`${asset.name} ground decal is too tall: ${height}m`);
  }
  if (asset.asset_kind === "Detail" && height > 1.2) {
    errors.push(`${asset.name} detail prop is too tall for non-occluding detail: ${height}m`);
  }
  if (partCount > 0 && thinPartCount / partCount > 0.75) {
    warnings.push(`${asset.name} is mostly thin parts (${thinPartCount}/${partCount}); check top-down readability.`);
  }
  if (smallPartRatio > 0.55) {
    warnings.push(`${asset.name} small_part_ratio=${smallPartRatio}; check that detail is clustered, not noisy.`);
  }
}

function validateMaterialRoles(asset) {
  if (!asset.material_role_usage || typeof asset.material_role_usage !== "object") {
    errors.push(`${asset.name} missing material_role_usage.`);
    return;
  }
  const unmapped = Object.entries(asset.material_role_usage)
    .filter(([, role]) => role === "unmapped_generated_material")
    .map(([material]) => material);
  if (unmapped.length > 0) {
    warnings.push(`${asset.name} has unmapped generated materials: ${unmapped.join(", ")}`);
  }
}

function validateArtDirectionContract(asset) {
  if (asset.brand_line_id !== "folded_reliquary_miniatures") {
    errors.push(`${asset.name} brand_line_id must be folded_reliquary_miniatures.`);
  }
  if (!String(asset.visual_family_id ?? "").startsWith("folded_reliquary.")) {
    errors.push(`${asset.name} visual_family_id must stay inside folded_reliquary, got ${asset.visual_family_id}.`);
  }
  if (!String(asset.shape_family_id ?? "").startsWith("folded_reliquary.")) {
    errors.push(`${asset.name} shape_family_id must stay inside folded_reliquary, got ${asset.shape_family_id}.`);
  }
  if (!Array.isArray(asset.required_shape_tokens) || asset.required_shape_tokens.length < 2) {
    errors.push(`${asset.name} must declare at least two required_shape_tokens.`);
  }
  if (!Array.isArray(asset.forbidden_drift_tokens) || asset.forbidden_drift_tokens.length === 0) {
    errors.push(`${asset.name} must declare forbidden_drift_tokens.`);
  }
  if (Array.isArray(asset.missing_required_tokens) && asset.missing_required_tokens.length > 0) {
    errors.push(`${asset.name} is missing required art tokens: ${asset.missing_required_tokens.join(", ")}`);
  }
  if (Array.isArray(asset.forbidden_token_hits) && asset.forbidden_token_hits.length > 0) {
    errors.push(`${asset.name} has forbidden drift token hits: ${asset.forbidden_token_hits.join(", ")}`);
  }
  const policy = String(asset.motif_limit_policy ?? "").toLowerCase();
  if (!policy.includes("fold") && !policy.includes("product-line") && !policy.includes("reliquary")) {
    errors.push(`${asset.name} motif_limit_policy must keep the asset inside the folded-reliquary product line.`);
  }
}

function validateCommercialSafety(asset) {
  if (!asset.commercial_safety || typeof asset.commercial_safety !== "object") {
    errors.push(`${asset.name} missing commercial_safety object.`);
    return;
  }
  if (asset.commercial_safety.external_reference_used !== false) {
    errors.push(`${asset.name} commercial_safety.external_reference_used must be false.`);
  }
  const protectedHits = asset.commercial_safety.protected_term_hits;
  if (!Array.isArray(protectedHits) || protectedHits.length > 0) {
    errors.push(`${asset.name} has protected term hits in commercial_safety.`);
  }
  if (!String(asset.commercial_safety.external_benchmark_use ?? "").includes("aggregate quality metrics only")) {
    errors.push(`${asset.name} must limit external benchmark use to aggregate quality metrics only.`);
  }
}

function validatePromptContract(manifestValue) {
  if (!fs.existsSync(promptContractPath)) {
    warnings.push(`Prompt contract not found: ${path.relative(repo, promptContractPath)}; run tools/AssetPipeline/build_prompt_contract.mjs before external generation handoff.`);
    return;
  }

  const records = fs.readFileSync(promptContractPath, "utf8")
    .trim()
    .split(/\n+/)
    .filter(Boolean)
    .map((line, index) => {
      try {
        return JSON.parse(line);
      } catch (error) {
        errors.push(`Prompt contract line ${index + 1} is invalid JSON: ${error.message}`);
        return null;
      }
    })
    .filter(Boolean);
  const assetsById = new Map(manifestValue.assets.map((asset) => [asset.asset_id, asset]));
  const seenPromptIds = new Set();
  for (const record of records) {
    requiredPrompt(record, "asset_id");
    requiredPrompt(record, "name");
    requiredPrompt(record, "canonical_prompt_ja");
    requiredPrompt(record, "canonical_prompt_en");
    requiredPrompt(record, "negative_prompt");
    requiredPrompt(record, "art_direction_id");
    requiredPrompt(record, "brand_line_id");
    requiredPrompt(record, "genre_lock_clause");
    requiredPrompt(record, "benchmark_policy_id");
    requiredPrompt(record, "external_reference_policy_id");
    requiredPrompt(record, "shape_family_id");
    requiredPrompt(record, "required_shape_tokens");
    requiredPrompt(record, "forbidden_drift_tokens");
    requiredPrompt(record, "source_reference_policy");
    if (record.raw_direct_use_approved !== false) {
      errors.push(`${record.asset_id} prompt contract must keep raw_direct_use_approved=false.`);
    }
    if (record.art_direction_id !== manifestValue.art_direction_id) {
      errors.push(`${record.asset_id} prompt contract art_direction_id does not match manifest.`);
    }
    if (record.brand_line_id !== manifestValue.brand_line_id) {
      errors.push(`${record.asset_id} prompt contract brand_line_id does not match manifest.`);
    }
    if (record.benchmark_policy_id !== "external_market_metrics_only") {
      errors.push(`${record.asset_id} prompt contract benchmark_policy_id must be external_market_metrics_only.`);
    }
    if (record.forbidden_prompt_hits?.length > 0) {
      errors.push(`${record.asset_id} prompt contract has forbidden_prompt_hits.`);
    }
    const asset = assetsById.get(record.asset_id);
    if (!asset) {
      errors.push(`${record.asset_id} prompt contract has no matching manifest asset.`);
    } else {
      if (record.name !== asset.name) {
        errors.push(`${record.asset_id} prompt contract name=${record.name} does not match manifest name=${asset.name}.`);
      }
      if (record.shape_family_id !== asset.shape_family_id) {
        errors.push(`${record.asset_id} prompt contract shape_family_id does not match manifest.`);
      }
    }
    seenPromptIds.add(record.asset_id);
    const searchable = JSON.stringify(record).toLowerCase();
    for (const term of forbiddenTerms) {
      if (searchable.includes(term)) {
        errors.push(`${record.asset_id} prompt contract contains forbidden protected-style term: ${term}`);
      }
    }
    for (const pattern of forbiddenPromptPatterns) {
      if (pattern.test(JSON.stringify(record))) {
        errors.push(`${record.asset_id} prompt contract contains forbidden imitation pattern: ${pattern}`);
      }
    }
    const policy = String(record.source_reference_policy ?? "").toLowerCase();
    if (!policy.includes("do not use external game imagery")) {
      errors.push(`${record.asset_id} prompt contract must explicitly prohibit external game imagery references.`);
    }
  }
  for (const asset of manifestValue.assets) {
    if (!seenPromptIds.has(asset.asset_id)) {
      errors.push(`${asset.name} has no prompt contract record.`);
    }
  }
}

function validateBenchmarkReport() {
  if (!fs.existsSync(visualBenchmarkPath)) {
    warnings.push(`Visual benchmark report not found: ${path.relative(repo, visualBenchmarkPath)}; production approval remains blocked.`);
    return {
      present: false,
      verdict: "missing_visual_benchmark",
      overall_score: 0,
      production_approval_status: "blocked"
    };
  }
  const report = JSON.parse(fs.readFileSync(visualBenchmarkPath, "utf8"));
  const scope = String(report.comparison_scope ?? report.note ?? "").toLowerCase();
  if (!scope.includes("aggregate") && !scope.includes("benchmarks")) {
    errors.push("Visual benchmark report must state aggregate/metrics-only use.");
  }
  if (!Array.isArray(report.market_sources) || report.market_sources.length === 0) {
    errors.push("Visual benchmark report must include market_sources.");
  }
  if (!Array.isArray(report.downloaded_benchmarks) || report.downloaded_benchmarks.length === 0) {
    warnings.push("Visual benchmark report has no downloaded_benchmarks; external metric comparison is weak.");
  }
  const score = Number(report.score?.overall ?? 0);
  const productionApproved = score >= 0.80 && report.verdict !== "below_market_finish";
  return {
    present: true,
    source_count: report.market_sources?.length ?? 0,
    downloaded_image_count: report.downloaded_benchmarks?.length ?? 0,
    external_use: "aggregate_metrics_only",
    overall_score: score,
    verdict: report.verdict ?? "unknown",
    production_approval_status: productionApproved ? "candidate_needs_human_review" : "blocked",
    score: report.score ?? {}
  };
}

function buildConsistencySummary(manifestValue, benchmark) {
  const assets = manifestValue.assets;
  const forbiddenHitAssets = assets.filter((asset) => asset.forbidden_token_hits?.length > 0);
  const missingTokenAssets = assets.filter((asset) => asset.missing_required_tokens?.length > 0);
  const externalRefAssets = assets.filter((asset) => asset.commercial_safety?.external_reference_used !== false);
  const roleCounts = {};
  const styleCounts = {};
  for (const asset of assets) {
    roleCounts[asset.product_line_role] = (roleCounts[asset.product_line_role] ?? 0) + 1;
    styleCounts[asset.style] = (styleCounts[asset.style] ?? 0) + 1;
  }
  return {
    version: 1,
    asset_count: assets.length,
    genre_contract_id: manifestValue.genre_contract_id,
    brand_line_id: manifestValue.brand_line_id,
    shape_grammar_id: manifestValue.shape_grammar_id,
    benchmark_policy_id: manifestValue.benchmark_policy_id,
    genre_contract_passed: assets.length - missingTokenAssets.length,
    genre_contract_failed: missingTokenAssets.length,
    missing_required_token_assets: missingTokenAssets.map((asset) => asset.name),
    required_token_coverage_min: missingTokenAssets.length === 0 ? 1.0 : 0.0,
    forbidden_token_hits: forbiddenHitAssets.length,
    forbidden_token_hit_assets: forbiddenHitAssets.map((asset) => asset.name),
    protected_term_hits: 0,
    external_reference_used_in_prompts: externalRefAssets.length,
    raw_direct_use_approved_count: 0,
    product_line_role_counts: roleCounts,
    style_counts: styleCounts,
    benchmark,
    production_approval_status: benchmark.production_approval_status === "candidate_needs_human_review" ? "blocked_until_human_art_ip_review" : "blocked",
    source_reference_policy: "repository-authored generation and approved internal previews only; external store screenshots are aggregate metric benchmarks only."
  };
}

function writeConsistencyReport(summary) {
  fs.writeFileSync(consistencyJsonPath, JSON.stringify(summary, null, 2) + "\n");
  const lines = [
    "# Generated Asset Consistency",
    "",
    `- Asset count: ${summary.asset_count}`,
    `- Brand line: \`${summary.brand_line_id}\``,
    `- Genre contract passed: ${summary.genre_contract_passed}/${summary.asset_count}`,
    `- Forbidden token hit assets: ${summary.forbidden_token_hits}`,
    `- External reference used in prompts: ${summary.external_reference_used_in_prompts}`,
    `- Benchmark verdict: \`${summary.benchmark.verdict}\``,
    `- Benchmark overall score: ${summary.benchmark.overall_score}`,
    `- Production approval status: \`${summary.production_approval_status}\``,
    "",
    "## Product-Line Roles",
    "",
    "| Role | Count |",
    "| --- | ---: |",
    ...Object.entries(summary.product_line_role_counts).sort().map(([role, count]) => `| ${role} | ${count} |`),
    "",
    "## Policy",
    "",
    summary.source_reference_policy,
    ""
  ];
  fs.writeFileSync(consistencyMdPath, lines.join("\n"));
}

function requiredPrompt(record, key) {
  if (!record[key]) {
    errors.push(`${record.asset_id ?? "unknown prompt record"} missing required prompt contract field: ${key}`);
  }
}

function fail(message) {
  console.error(`- ${message}`);
  process.exit(1);
}
