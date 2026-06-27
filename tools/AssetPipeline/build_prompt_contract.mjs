#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();
const schemaDir = path.join(repo, "artifacts/AssetPipeline/schema_v1");
const manifestPath = path.join(repo, "artifacts/Reports/fourfold-model-pack.json");
const outJsonl = path.join(repo, "artifacts/Reports/fourfold-asset-prompt-contract.jsonl");
const auditJson = path.join(repo, "artifacts/Reports/prompt-schema-v1-audit.json");
const auditMd = path.join(repo, "artifacts/Reports/prompt-schema-v1-audit.md");

const artDirectionName = "Folded Reliquary Miniatures";
const artDirectionId = "folded_reliquary";
const brandLineId = "folded_reliquary_miniatures";
const benchmarkPolicyId = "external_market_metrics_only";
const genreLockClause = [
  "Commercial-safe friendly compact action-adventure.",
  "Use rounded low folded plates, four-part inlays, chunky tabs, saturated readable color planes, and one functional signal-thread language.",
  "External games are benchmark metrics only; do not use named-title shapes, palettes, compositions, characters, props, logos, or style labels."
].join(" ");
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
  "tunic",
  "death's door",
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
const driftTerms = [
  "realistic pbr",
  "root",
  "crystal",
  "glass",
  "furnace",
  "crown",
  "plant",
  "flower",
  "pipe",
  "gear"
];

if (!fs.existsSync(schemaDir)) {
  fail(`Missing schema_v1 directory: ${path.relative(repo, schemaDir)}`);
}
if (!fs.existsSync(manifestPath)) {
  fail(`Missing model manifest: ${path.relative(repo, manifestPath)}`);
}

const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
const schemaRecords = readSchemaRecords(schemaDir);
const schemaByAssetId = new Map(schemaRecords.map((record) => [record.asset_id, record]));
const manifestByAssetId = new Map(manifest.assets.map((asset) => [asset.asset_id, asset]));
const contractRecords = manifest.assets.map((asset) => buildContractRecord(asset, schemaByAssetId.get(asset.asset_id)));
const schemaOnly = schemaRecords.filter((record) => !manifestByAssetId.has(record.asset_id));
const manifestOnly = manifest.assets.filter((asset) => !schemaByAssetId.has(asset.asset_id));
const rawRiskSummary = summarizeRisks(schemaRecords);

fs.mkdirSync(path.dirname(outJsonl), { recursive: true });
fs.writeFileSync(outJsonl, contractRecords.map((record) => JSON.stringify(record)).join("\n") + "\n");

const audit = {
  version: 1,
  art_direction_id: artDirectionId,
  art_direction_name: artDirectionName,
  schema_dir: rel(schemaDir),
  source_manifest: rel(manifestPath),
  contract_file: rel(outJsonl),
  schema_records: schemaRecords.length,
  manifest_assets: manifest.assets.length,
  matched_records: contractRecords.filter((record) => record.source_schema_file).length,
  schema_only: schemaOnly.map((record) => ({ asset_id: record.asset_id, file: rel(record.file) })),
  manifest_only: manifestOnly.map((asset) => ({ asset_id: asset.asset_id, name: asset.name })),
  raw_risk_summary: rawRiskSummary,
  direct_use_policy: "schema_v1 records are reference-only; use the sanitized prompt contract for any generation or vendor handoff.",
  external_reference_policy: "internal project files only; official store screenshots remain aggregate metric benchmarks and must not be used as image or style references.",
};
fs.writeFileSync(auditJson, JSON.stringify(audit, null, 2) + "\n");
fs.writeFileSync(auditMd, markdown(audit));

console.log(`Wrote ${rel(outJsonl)} (${contractRecords.length} records)`);
console.log(`Wrote ${rel(auditJson)}`);
console.log(`Wrote ${rel(auditMd)}`);

function readSchemaRecords(dir) {
  return fs.readdirSync(dir)
    .filter((file) => /^art_.*\.json$/.test(file))
    .sort()
    .map((file) => {
      const fullPath = path.join(dir, file);
      const record = JSON.parse(fs.readFileSync(fullPath, "utf8"));
      return { ...record, file: fullPath };
    });
}

function buildContractRecord(asset, raw) {
  const riskTags = raw ? detectRawRiskTags(raw) : ["no_schema_v1_record"];
  const requiredMaps = normalizeMaps(raw?.required_maps, asset);
  const targetFormats = normalizeFormats(raw?.target_formats);
  return {
    version: 1,
    asset_id: asset.asset_id,
    name: asset.name,
    asset_kind: asset.asset_kind,
    area_code: asset.area_code,
    source_schema_file: raw ? rel(raw.file) : null,
    raw_tool_suggestion: raw?.tool_recommendation ?? null,
    raw_direct_use_approved: false,
    raw_risk_tags: riskTags,
    raw_risk_resolution: riskTags.map((tag) => ({ tag, resolution: "sanitized_out_of_generation_prompt" })),
    sanitization_status: "sanitized_no_external_style_prompt",
    contract_status: "sanitized_contract_ready",
    art_direction_id: artDirectionId,
    art_direction_name: artDirectionName,
    brand_line_id: asset.brand_line_id ?? brandLineId,
    product_line_role: asset.product_line_role,
    genre_lock_clause: genreLockClause,
    benchmark_policy_id: benchmarkPolicyId,
    external_reference_policy_id: "repo_internal_only_no_external_game_imagery",
    visual_family_id: asset.visual_family_id,
    shape_family_id: asset.shape_family_id,
    required_shape_tokens: asset.required_shape_tokens,
    forbidden_drift_tokens: asset.forbidden_drift_tokens,
    required_readability_anchors: asset.required_readability_anchors,
    target_formats: targetFormats,
    required_maps: requiredMaps,
    texture_policy: texturePolicy(asset),
    triangle_budget_lod0: asset.triangle_budget_lod0,
    material_budget: asset.material_budget,
    collision_profile: asset.collision_profile,
    naming_rule: namingRule(asset, requiredMaps),
    canonical_prompt_ja: canonicalPrompt(asset, "ja"),
    canonical_prompt_en: canonicalPrompt(asset, "en"),
    negative_prompt: negativePrompt(asset),
    forbidden_prompt_hits: [],
    source_reference_policy: "Use only this repository's manifest, Blender generator, asset register, and approved internal previews for scale/readability checks. Do not use external game imagery, official store screenshots, or named-title moodboards as visual references.",
    qa_checklist: qaChecklist(asset, requiredMaps, targetFormats),
    commercial_safety: {
      source: "repository-authored or separately reviewed commercial-use tool output only",
      attribution: "none unless a future approved vendor/tool contract requires it",
      external_benchmark_use: "aggregate quality metrics only",
      human_review_required: true,
    },
  };
}

function canonicalPrompt(asset, language) {
  const role = asset.gameplay_role;
  const anchors = asset.required_readability_anchors.join(", ");
  const maps = materialRoles(asset).join(", ");
  if (language === "ja") {
    return [
      "Unity向け3Dアセット。",
      `アート軸: ${artDirectionName} / 折り目遺物の箱庭模型。`,
      "親しみやすい高彩度の色面、丸みのある低い折り板、分割象嵌、厚い欠けタブ、一本の機能シグナル糸で構成する。",
      `対象: ${asset.name}。用途: ${role}。`,
      `商品ライン内の役割: ${asset.product_line_role ?? asset.asset_kind}。形状ファミリー: ${asset.shape_family_id}。読み取りアンカー: ${anchors}。`,
      `必須形状トークン: ${(asset.required_shape_tokens ?? []).join(", ")}。`,
      `素材ロール: ${maps}。地域差は素材比率と摩耗状態だけで出し、別ジャンル化しない。`,
      `LOD0上限: ${asset.triangle_budget_lod0} tris、マテリアル上限: ${asset.material_budget}。`,
      `ピボット: ${asset.pivot_rule}、コライダー: ${asset.collision_profile}。`,
      "FBX納品前提。上面視/高め三人称で前後方向、機能、接地が読めること。外部作品の形状、配色、構図、固有モチーフ、スタイルラベルは参照しない。"
    ].join("");
  }
  return [
    "Create a Unity-ready 3D asset.",
    `Art direction: ${artDirectionName}, an original FOURFOLD product family of friendly folded miniature relics.`,
    "Build it from rounded low folded plates, split inlays, chunky tabs, saturated readable color planes, and one functional signal-thread language.",
    `Asset: ${asset.name}. Gameplay use: ${role}.`,
    `Product-line role: ${asset.product_line_role ?? asset.asset_kind}. Shape family: ${asset.shape_family_id}. Readability anchors: ${anchors}.`,
    `Required shape tokens: ${(asset.required_shape_tokens ?? []).join(", ")}.`,
    `Material roles: ${maps}. Regional variation must come from material ratio and wear state, not a separate genre.`,
    `LOD0 budget: ${asset.triangle_budget_lod0} tris. Material slot budget: ${asset.material_budget}.`,
    `Pivot: ${asset.pivot_rule}. Collision profile: ${asset.collision_profile}.`,
    "Deliver as FBX-ready game geometry. The front direction, function, and grounding must read from a top-down or high three-quarter gameplay camera. Do not use external game shapes, palettes, compositions, proprietary motifs, or named style labels as references."
  ].join(" ");
}

function negativePrompt(asset) {
  return [
    "no text labels",
    "no logos",
    "no watermark",
    "no external franchise references",
    "no named-game style imitation",
    "no averaged look from multiple games",
    "no friendly fantasy title lookalike",
    "no named JRPG or platformer trade dress",
    "no trade-dress mimicry",
    "no photoreal face scan",
    "no photoreal pores",
    "no gritty military palette",
    "no thin fragile noise",
    "no random extra props",
    "no mixed art styles",
    "no unreadable front direction",
    "no tall occluders that hide gameplay silhouettes",
    asset.asset_kind === "Detail" ? "no collider-driving geometry" : null,
  ].filter(Boolean).join(", ");
}

function qaChecklist(asset, requiredMaps, targetFormats) {
  return [
    `LOD pass: LOD0 <= ${asset.triangle_budget_lod0} tris and a reduced LOD silhouette remains readable from the top-down gameplay camera.`,
    `Format pass: deliver ${targetFormats.join("/")} only; texture maps: ${requiredMaps.join(", ")}.`,
    `Transform pass: scale matches ${asset.scale_meters}, pivot is ${asset.pivot_rule}, forward/read direction is obvious, and no negative scale is used.`,
    `Material pass: material slots <= ${asset.material_budget}; maps remain atlas-friendly and stylized, not photoreal surface noise.`,
    `Collision pass: generated Unity prefab uses ${asset.collision_profile}; visual detail and ground decals do not create navigation noise.`,
    `Art-direction pass: folded plinth/split inlay/chunky tab/signal thread language is visible where appropriate and the asset still belongs to ${artDirectionName}.`,
    "Commercial-safety pass: no external title, character, prop, logo, UI, palette set, composition, or trade-dress imitation is present.",
  ];
}

function namingRule(asset, requiredMaps) {
  const textureNames = requiredMaps.map((map) => `${asset.name}_${pascal(map)}.png`).join(", ");
  return `Model: ${asset.name}_LOD0.fbx; textures: ${textureNames}; prefab: ${path.basename(asset.unity_prefab)}`;
}

function materialRoles(asset) {
  if (!asset.material_role_usage) {
    return ["base", "dark", "accent", "signal"];
  }
  return [...new Set(Object.values(asset.material_role_usage))]
    .filter((role) => role !== "unmapped_generated_material")
    .sort();
}

function normalizeMaps(rawMaps, asset) {
  const maps = new Set((rawMaps?.length ? rawMaps : ["basecolor", "normal", "roughness", "ao"]).map((map) => map.toLowerCase()));
  if (asset.materials?.some((material) => /emissive|glow|signal|danger|relic|tool|ember/i.test(material))) {
    maps.add("emissive");
  }
  if (asset.materials?.some((material) => /metal|gold|iron/i.test(material))) {
    maps.add("metallic");
  }
  return [...maps].filter((map) => ["basecolor", "normal", "roughness", "metallic", "ao", "emissive"].includes(map));
}

function normalizeFormats(rawFormats) {
  const formats = new Set(rawFormats?.length ? rawFormats : ["FBX", "PNG"]);
  formats.add("FBX");
  formats.add("PNG");
  return [...formats].filter((format) => ["FBX", "GLB", "PNG"].includes(format)).sort();
}

function texturePolicy(asset) {
  if (asset.asset_kind === "Boss") {
    return "1024-2048 stylized atlas; no photoreal scan textures";
  }
  if (["Hero", "Combatant"].includes(asset.asset_kind)) {
    return "1024 stylized atlas; preserve top-down silhouette";
  }
  if (["Detail", "GroundDecal"].includes(asset.asset_kind)) {
    return "shared 512 detail atlas; low-frequency readable marks only";
  }
  return "shared 1024 stylized atlas; material roles match the area family";
}

function detectRawRiskTags(raw) {
  const searchable = JSON.stringify(raw).toLowerCase();
  const tags = [];
  for (const term of forbiddenTerms) {
    if (searchable.includes(term)) {
      tags.push(`forbidden_external_term:${term}`);
    }
  }
  if (searchable.includes("realistic pbr") || searchable.includes("リアル寄り")) {
    tags.push("photoreal_drift_risk");
  }
  const motifHits = driftTerms.filter((term) => searchable.includes(term));
  if (motifHits.length > 0) {
    tags.push(`regional_motif_may_override_folded_reliquary:${motifHits.join("|")}`);
  }
  if (searchable.includes("marketplace")) {
    tags.push("marketplace_license_review_required");
  }
  return tags.length ? tags : ["no_raw_prompt_risk_detected"];
}

function summarizeRisks(records) {
  const summary = {};
  for (const record of records) {
    for (const tag of detectRawRiskTags(record)) {
      summary[tag] = (summary[tag] ?? 0) + 1;
    }
  }
  return summary;
}

function markdown(audit) {
  const lines = [
    "# Prompt Schema v1 Audit",
    "",
    `Art direction: **${audit.art_direction_name}**`,
    "",
    `- Schema records: ${audit.schema_records}`,
    `- Manifest assets: ${audit.manifest_assets}`,
    `- Matched records: ${audit.matched_records}`,
    `- Contract file: \`${audit.contract_file}\``,
    "",
    "## Policy",
    "",
    `- ${audit.direct_use_policy}`,
    `- ${audit.external_reference_policy}`,
    "",
    "## Raw Risk Summary",
    "",
  ];
  for (const [tag, count] of Object.entries(audit.raw_risk_summary).sort()) {
    lines.push(`- ${tag}: ${count}`);
  }
  lines.push("", "## Schema Only", "");
  for (const item of audit.schema_only) {
    lines.push(`- ${item.asset_id}: \`${item.file}\``);
  }
  lines.push("", "## Manifest Only", "");
  for (const item of audit.manifest_only) {
    lines.push(`- ${item.asset_id}: ${item.name}`);
  }
  return lines.join("\n") + "\n";
}

function pascal(value) {
  return value.split(/[_-]/g).map((part) => part.charAt(0).toUpperCase() + part.slice(1)).join("");
}

function rel(value) {
  return path.resolve(value).replace(`${repo}${path.sep}`, "").replaceAll(path.sep, "/");
}

function fail(message) {
  console.error(`- ${message}`);
  process.exit(1);
}
