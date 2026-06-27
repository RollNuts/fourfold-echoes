#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();
const schemaDir = path.join(repo, "artifacts/AssetPipeline/schema_v1");
const aggregateJson = path.join(schemaDir, "assets.schema_v1.json");
const aggregateJsonl = path.join(schemaDir, "assets.schema_v1.jsonl");
const dryRun = process.argv.includes("--dry-run");

if (!fs.existsSync(schemaDir)) {
  fail(`Missing schema directory: ${rel(schemaDir)}`);
}

const records = fs.readdirSync(schemaDir)
  .filter((file) => /^art_.*\.json$/.test(file))
  .sort()
  .map((file) => {
    const fullPath = path.join(schemaDir, file);
    return { file: fullPath, record: JSON.parse(fs.readFileSync(fullPath, "utf8")) };
  });

let changedCount = 0;
for (const item of records) {
  const before = JSON.stringify(item.record);
  harden(item.record);
  if (JSON.stringify(item.record) !== before) {
    changedCount += 1;
    if (!dryRun) {
      fs.writeFileSync(item.file, `${JSON.stringify(item.record, null, 2)}\n`);
    }
  }
}

if (!dryRun) {
  const allRecords = records.map((item) => item.record).sort((a, b) => a.asset_id.localeCompare(b.asset_id));
  fs.writeFileSync(aggregateJson, `${JSON.stringify(allRecords, null, 2)}\n`);
  fs.writeFileSync(aggregateJsonl, `${allRecords.map((record) => JSON.stringify(record)).join("\n")}\n`);
}

console.log(`Schema prompt hardening ${dryRun ? "dry-run " : ""}changed=${changedCount} records=${records.length}`);

function harden(record) {
  for (const key of ["prompt_ja", "prompt_en"]) {
    if (typeof record[key] !== "string") {
      continue;
    }
    record[key] = record[key]
      .replaceAll("低ポリ版とリアル寄りPBR版の両方で同じシルエットが成立すること。", "低ポリLODとstylized PBR素材の両方で同じ読みやすいシルエットが成立すること。")
      .replaceAll("It must work as both a low-poly version and a realistic PBR version with the same readable silhouette.", "It must preserve the same readable silhouette across low-poly LODs and stylized PBR materials.")
      .replaceAll("realistic PBR", "stylized PBR");
  }

  record.negative_prompt = appendCsv(record.negative_prompt, [
    "no external franchise references",
    "no named-game style imitation",
    "no averaged look from multiple games",
    "no trade-dress mimicry"
  ]);

  const policy = "External store screenshots are aggregate metric benchmarks only and must not be used as visual references.";
  if (!String(record.reference_strategy ?? "").includes(policy)) {
    record.reference_strategy = `${record.reference_strategy ?? ""} ${policy}`.trim();
  }
}

function appendCsv(value, additions) {
  const parts = String(value ?? "")
    .split(",")
    .map((part) => part.trim())
    .filter(Boolean);
  const lowered = new Set(parts.map((part) => part.toLowerCase()));
  for (const addition of additions) {
    if (!lowered.has(addition.toLowerCase())) {
      parts.push(addition);
      lowered.add(addition.toLowerCase());
    }
  }
  return parts.join(", ");
}

function rel(value) {
  return path.resolve(value).replace(`${repo}${path.sep}`, "").replaceAll(path.sep, "/");
}

function fail(message) {
  console.error(`- ${message}`);
  process.exit(1);
}
