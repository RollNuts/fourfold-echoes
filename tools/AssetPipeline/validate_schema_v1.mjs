#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();
const schemaDir = path.join(repo, "artifacts/AssetPipeline/schema_v1");
const aggregateJson = path.join(schemaDir, "assets.schema_v1.json");
const aggregateJsonl = path.join(schemaDir, "assets.schema_v1.jsonl");
const manifestPath = path.join(repo, "artifacts/Reports/fourfold-model-pack.json");
const errors = [];
const warnings = [];

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
const imitationPatterns = [
  /\bin the style of\b/i,
  /\binspired by\b/i,
  /\bsimilar to\b/i,
  /\baverage of\b/i,
  /\bmix of\b/i,
  /\bblend of\b/i,
  /\bfusion of\b/i,
  /〜風/,
  /っぽい/,
  /画風/
];
const requiredFields = [
  "asset_id",
  "asset_type",
  "priority",
  "style",
  "buy_instead_of_generate",
  "tool_recommendation",
  "prompt_ja",
  "prompt_en",
  "negative_prompt",
  "reference_strategy",
  "target_formats",
  "required_maps",
  "unity_import_preset",
  "qa_checklist",
  "naming_rule"
];

if (!fs.existsSync(schemaDir)) {
  fail(`Missing schema directory: ${rel(schemaDir)}`);
}
if (!fs.existsSync(aggregateJson)) {
  fail(`Missing aggregate schema JSON: ${rel(aggregateJson)}`);
}
if (!fs.existsSync(aggregateJsonl)) {
  fail(`Missing aggregate schema JSONL: ${rel(aggregateJsonl)}`);
}

const recordsWithFiles = readIndividualRecords();
const ids = new Set();
for (const { file, record } of recordsWithFiles) {
  validateRecord(file, record);
  if (ids.has(record.asset_id)) {
    errors.push(`Duplicate schema asset_id: ${record.asset_id}`);
  }
  ids.add(record.asset_id);
}

validateAggregates(recordsWithFiles.map((item) => item.record));
validateManifestCoverage(recordsWithFiles.map((item) => item.record));

if (errors.length > 0) {
  console.error(errors.map((line) => `- ${line}`).join("\n"));
  process.exit(1);
}
if (warnings.length > 0) {
  console.warn(warnings.map((line) => `- WARN ${line}`).join("\n"));
}
console.log(`Schema v1 validation passed: ${recordsWithFiles.length} records.`);

function readIndividualRecords() {
  return fs.readdirSync(schemaDir)
    .filter((file) => /^art_.*\.json$/.test(file))
    .sort()
    .map((file) => {
      const fullPath = path.join(schemaDir, file);
      return { file: fullPath, record: JSON.parse(fs.readFileSync(fullPath, "utf8")) };
    });
}

function validateRecord(file, record) {
  for (const field of requiredFields) {
    if (record[field] === undefined || record[field] === null || record[field] === "") {
      errors.push(`${rel(file)} missing required field: ${field}`);
    }
  }
  if (!/^art\.[a-z0-9_.]+$/.test(record.asset_id ?? "")) {
    errors.push(`${rel(file)} has invalid asset_id: ${record.asset_id}`);
  }
  if (!Array.isArray(record.target_formats) || record.target_formats.length === 0) {
    errors.push(`${rel(file)} target_formats must be a non-empty array.`);
  }
  if (!Array.isArray(record.required_maps)) {
    errors.push(`${rel(file)} required_maps must be an array.`);
  }
  if (!Array.isArray(record.qa_checklist) || record.qa_checklist.length < 4) {
    errors.push(`${rel(file)} qa_checklist must contain at least 4 checks.`);
  }
  const text = JSON.stringify(record).toLowerCase();
  for (const term of forbiddenTerms) {
    if (text.includes(term)) {
      errors.push(`${rel(file)} contains forbidden protected-style term: ${term}`);
    }
  }
  for (const pattern of imitationPatterns) {
    if (pattern.test(JSON.stringify(record))) {
      errors.push(`${rel(file)} contains imitation/averaging prompt pattern: ${pattern}`);
    }
  }
  if (/\bna\s+tris\b/i.test(JSON.stringify(record)) || /<=\s*na\b/i.test(JSON.stringify(record))) {
    errors.push(`${rel(file)} contains non-numeric triangle budget text.`);
  }
  if (!String(record.negative_prompt ?? "").toLowerCase().includes("no external franchise")) {
    warnings.push(`${rel(file)} negative_prompt does not explicitly include no external franchise references.`);
  }
  if (!String(record.reference_strategy ?? "").toLowerCase().includes("external") || !String(record.reference_strategy ?? "").toLowerCase().includes("aggregate")) {
    warnings.push(`${rel(file)} reference_strategy should explicitly restrict external screenshots to aggregate metrics only.`);
  }
  if (record.target_formats?.length === 1 && record.target_formats[0] === "PNG" && /\b(fbx|glb|model:)\b/i.test(record.naming_rule ?? "")) {
    errors.push(`${rel(file)} is PNG-only but naming_rule still references model/FBX/GLB output.`);
  }
  if (String(record.prompt_en ?? "").toLowerCase().includes("realistic pbr") || String(record.prompt_ja ?? "").includes("リアル寄り")) {
    warnings.push(`${rel(file)} has photoreal/PBR drift wording; sanitized prompt contract must be used for generation.`);
  }
  if (String(record.reference_strategy ?? "").toLowerCase().includes("marketplace")) {
    warnings.push(`${rel(file)} mentions marketplace sourcing; license proof and art-direction rework are required before use.`);
  }
}

function validateAggregates(individualRecords) {
  const individualIds = individualRecords.map((record) => record.asset_id).sort();
  const aggregateRecords = JSON.parse(fs.readFileSync(aggregateJson, "utf8"));
  const jsonIds = aggregateRecords.map((record) => record.asset_id).sort();
  const jsonlRecords = fs.readFileSync(aggregateJsonl, "utf8")
    .trim()
    .split(/\n+/)
    .filter(Boolean)
    .map((line, index) => {
      try {
        return JSON.parse(line);
      } catch (error) {
        errors.push(`Invalid JSONL line ${index + 1}: ${error.message}`);
        return null;
      }
    })
    .filter(Boolean);
  const jsonlIds = jsonlRecords.map((record) => record.asset_id).sort();
  if (JSON.stringify(individualIds) !== JSON.stringify(jsonIds)) {
    errors.push("assets.schema_v1.json does not match individual art_*.json records. Run sync_schema_v1_from_manifest.mjs.");
  }
  if (JSON.stringify(individualIds) !== JSON.stringify(jsonlIds)) {
    errors.push("assets.schema_v1.jsonl does not match individual art_*.json records. Run sync_schema_v1_from_manifest.mjs.");
  }
}

function validateManifestCoverage(records) {
  if (!fs.existsSync(manifestPath)) {
    warnings.push(`Model manifest not found, skipping coverage: ${rel(manifestPath)}`);
    return;
  }
  const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
  const schemaIds = new Set(records.map((record) => record.asset_id));
  const missing = manifest.assets.filter((asset) => !schemaIds.has(asset.asset_id));
  if (missing.length > 0) {
    errors.push(`Schema is missing ${missing.length} manifest assets: ${missing.slice(0, 12).map((asset) => asset.asset_id).join(", ")}${missing.length > 12 ? "..." : ""}`);
  }
}

function rel(value) {
  return path.resolve(value).replace(`${repo}${path.sep}`, "").replaceAll(path.sep, "/");
}

function fail(message) {
  console.error(`- ${message}`);
  process.exit(1);
}
