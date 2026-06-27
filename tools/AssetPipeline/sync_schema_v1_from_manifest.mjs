#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();
const schemaDir = path.join(repo, "artifacts/AssetPipeline/schema_v1");
const manifestPath = path.join(repo, "artifacts/Reports/fourfold-model-pack.json");
const aggregateJson = path.join(schemaDir, "assets.schema_v1.json");
const aggregateJsonl = path.join(schemaDir, "assets.schema_v1.jsonl");
const dryRun = process.argv.includes("--dry-run");

if (!fs.existsSync(schemaDir)) {
  fail(`Missing schema directory: ${rel(schemaDir)}`);
}
if (!fs.existsSync(manifestPath)) {
  fail(`Missing model manifest: ${rel(manifestPath)}`);
}

const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
const existing = readSchemaRecords();
const existingIds = new Set(existing.map((record) => record.asset_id));
const added = [];

for (const asset of manifest.assets) {
  if (existingIds.has(asset.asset_id)) {
    continue;
  }
  const record = schemaRecordFromAsset(asset);
  const file = path.join(schemaDir, `${safeFileName(asset.asset_id)}.json`);
  if (fs.existsSync(file)) {
    fail(`Refusing to overwrite existing schema file: ${rel(file)}`);
  }
  added.push({ file, record });
}

if (!dryRun) {
  for (const { file, record } of added) {
    fs.writeFileSync(file, `${JSON.stringify(record, null, 2)}\n`);
  }
}

const allRecords = readSchemaRecords()
  .concat(dryRun ? added.map((item) => item.record) : [])
  .sort((a, b) => a.asset_id.localeCompare(b.asset_id));

if (!dryRun) {
  fs.writeFileSync(aggregateJson, `${JSON.stringify(allRecords, null, 2)}\n`);
  fs.writeFileSync(aggregateJsonl, `${allRecords.map((record) => JSON.stringify(record)).join("\n")}\n`);
}

console.log(`Schema records existing=${existing.length} added=${added.length} total=${allRecords.length}`);
for (const item of added) {
  console.log(`+ ${rel(item.file)}`);
}
if (dryRun) {
  console.log("Dry run: no files written.");
}

function readSchemaRecords() {
  return fs.readdirSync(schemaDir)
    .filter((file) => /^art_.*\.json$/.test(file))
    .sort()
    .map((file) => JSON.parse(fs.readFileSync(path.join(schemaDir, file), "utf8")));
}

function schemaRecordFromAsset(asset) {
  const assetType = schemaAssetType(asset);
  const requiredMaps = requiredTextureMaps(asset);
  const formats = targetFormats(assetType);
  const japanese = [
    "Unity向け3D/2D制作スキーマ。",
    "世界観: Fourfold Echoes, original folded-reliquary miniature product line。",
    "外部作品の形状、配色、構図、固有モチーフ、作風ラベルを参照しない。",
    `対象: ${asset.name}。用途: ${asset.gameplay_role}。`,
    `役割: ${asset.product_line_role}。読み取り: ${(asset.required_readability_anchors ?? []).join(", ")}。`,
    `形状: ${(asset.required_shape_tokens ?? []).join(", ")} を優先し、${(asset.forbidden_drift_tokens ?? []).join(", ")} を避ける。`,
    `LOD0上限: ${asset.triangle_budget_lod0} tris。マテリアル上限: ${asset.material_budget}。`,
    `ピボット: ${asset.pivot_rule}。コライダー: ${asset.collision_profile}。`,
    "上面視/高め三人称で機能、前後方向、接地が読めること。"
  ].join("");
  const english = [
    "Unity-ready production schema for Fourfold Echoes.",
    "Use the original folded-reliquary miniature product line only.",
    "Do not reference external game shapes, palettes, compositions, proprietary motifs, or named style labels.",
    `Asset: ${asset.name}. Gameplay use: ${asset.gameplay_role}.`,
    `Role: ${asset.product_line_role}. Readability anchors: ${(asset.required_readability_anchors ?? []).join(", ")}.`,
    `Prioritize shape tokens: ${(asset.required_shape_tokens ?? []).join(", ")}. Avoid drift tokens: ${(asset.forbidden_drift_tokens ?? []).join(", ")}.`,
    `LOD0 budget: ${asset.triangle_budget_lod0} tris. Material budget: ${asset.material_budget}.`,
    `Pivot: ${asset.pivot_rule}. Collision: ${asset.collision_profile}.`,
    "Function, front direction, and grounding must read from top-down or high three-quarter gameplay camera."
  ].join(" ");

  return {
    asset_id: asset.asset_id,
    asset_type: assetType,
    priority: asset.priority,
    style: "stylized",
    buy_instead_of_generate: false,
    tool_recommendation: "InternalProceduralFirst",
    prompt_ja: japanese,
    prompt_en: english,
    negative_prompt: [
      "no text labels",
      "no logos",
      "no watermark",
      "no external franchise references",
      "no named-game style imitation",
      "no averaged look from multiple games",
      "no trade-dress mimicry",
      "no photoreal scan textures",
      "no thin fragile noise",
      "no random extra props",
      "no unreadable front direction"
    ].join(", "),
    reference_strategy: [
      "Use only repository-owned sources:",
      "artifacts/Reports/fourfold-model-pack.json,",
      "tools/AssetPipeline/generate_production_model_pack.py,",
      "tools/Blender/generate_fourfold_model_pack.py,",
      "and approved internal previews.",
      "External store screenshots are aggregate metric benchmarks only and must not be used as visual references."
    ].join(" "),
    target_formats: formats,
    required_maps: requiredMaps,
    unity_import_preset: unityImportPreset(asset, assetType),
    qa_checklist: qaChecklist(asset, requiredMaps, formats),
    naming_rule: namingRule(asset, requiredMaps, formats),
  };
}

function schemaAssetType(asset) {
  if (asset.asset_kind === "Hero" || asset.asset_kind === "Combatant" || asset.asset_kind === "Boss" || asset.asset_kind === "ServiceNPC") {
    return "character";
  }
  if (asset.asset_kind === "Equipment") {
    return "equipment";
  }
  if (asset.asset_kind === "Tile" || asset.asset_kind === "Boundary" || asset.category === "Environment") {
    return "environment";
  }
  if (asset.asset_kind === "GroundDecal") {
    return "decal";
  }
  return "prop";
}

function requiredTextureMaps(asset) {
  const maps = new Set(["basecolor", "normal", "roughness", "ao"]);
  const materialText = JSON.stringify(asset.materials ?? []).toLowerCase();
  if (/signal|tool|danger|relic|blue|ember|glow|crystal|violet|white/.test(materialText)) {
    maps.add("emissive");
  }
  if (/metal|gold|iron|steel/.test(materialText)) {
    maps.add("metallic");
  }
  if (asset.asset_kind === "GroundDecal") {
    maps.delete("normal");
    maps.delete("roughness");
  }
  return [...maps];
}

function targetFormats(assetType) {
  if (assetType === "decal") {
    return ["PNG"];
  }
  return ["FBX", "PNG"];
}

function unityImportPreset(asset, assetType) {
  if (assetType === "character") {
    return asset.asset_kind === "Hero" || asset.asset_kind === "ServiceNPC" ? "CharacterHumanoid" : "CharacterGeneric";
  }
  if (assetType === "equipment") {
    return "Equipment";
  }
  if (assetType === "environment") {
    return "Environment";
  }
  if (assetType === "decal") {
    return "SpriteOrDecal";
  }
  return "Prop";
}

function qaChecklist(asset, requiredMaps, formats) {
  const checks = [
    `LOD policy passes: LOD0 <= ${asset.triangle_budget_lod0} tris and role silhouette remains readable at gameplay camera distance.`,
    `Unity transform passes: scale matches ${asset.scale_meters}, pivot is ${asset.pivot_rule}, no negative scale.`,
    `Format pass: exported ${formats.join("/")} only; required maps: ${requiredMaps.join(", ")}.`,
    `Material pass: material slots <= ${asset.material_budget}; texture work stays stylized and atlas-friendly.`,
    `Collision pass: collider matches ${asset.collision_profile}.`,
    "Commercial-safety pass: no external title, character, prop, logo, palette set, composition, or trade-dress imitation."
  ];
  if (asset.required_readability_anchors?.length) {
    checks.splice(1, 0, `Readability pass: ${asset.required_readability_anchors.join(", ")} are visible in a 128 px preview.`);
  }
  return checks;
}

function namingRule(asset, requiredMaps, formats) {
  if (formats.length === 1 && formats[0] === "PNG") {
    return `Texture: ${asset.name}.png`;
  }
  const textures = requiredMaps.map((map) => `${asset.name}_${pascal(map)}.png`).join(", ");
  return `Model: ${asset.name}_LOD0.fbx; textures: ${textures}; prefab: ${path.basename(asset.unity_prefab)}`;
}

function safeFileName(assetId) {
  return assetId.toLowerCase().replace(/[^a-z0-9]+/g, "_").replace(/^_+|_+$/g, "");
}

function pascal(value) {
  return value.split(/[_-]/g).map((part) => `${part.charAt(0).toUpperCase()}${part.slice(1)}`).join("");
}

function rel(value) {
  return path.resolve(value).replace(`${repo}${path.sep}`, "").replaceAll(path.sep, "/");
}

function fail(message) {
  console.error(`- ${message}`);
  process.exit(1);
}
