#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();
const manifestPath = path.join(repo, "artifacts/Reports/fourfold-model-pack.json");
const schemaDir = path.join(repo, "artifacts/AssetPipeline/schema_v1");
const promptContractPath = path.join(repo, "artifacts/Reports/fourfold-asset-prompt-contract.jsonl");
const benchmarkPath = path.join(repo, "artifacts/Reports/visual-benchmark.json");
const outJson = path.join(repo, "artifacts/Reports/commercial-asset-gate.json");
const outMd = path.join(repo, "artifacts/Reports/commercial-asset-gate.md");

const manifest = readJson(manifestPath);
const schemaRecords = readSchemaRecords(schemaDir);
const promptRecords = readJsonl(promptContractPath);
const benchmark = fs.existsSync(benchmarkPath) ? readJson(benchmarkPath) : null;

const manifestIssues = [];
for (const asset of manifest.assets ?? []) {
  if (asset.license !== "repository-authored") {
    manifestIssues.push(`${asset.asset_id}: non repository-authored license=${asset.license}`);
  }
  if (asset.commercial_safety?.external_reference_used !== false) {
    manifestIssues.push(`${asset.asset_id}: external_reference_used is not false`);
  }
  if (asset.commercial_safety?.protected_term_hits?.length) {
    manifestIssues.push(`${asset.asset_id}: protected term hits`);
  }
}

const promptIssues = [];
for (const record of promptRecords) {
  if (record.raw_direct_use_approved !== false) {
    promptIssues.push(`${record.asset_id}: raw_direct_use_approved must be false`);
  }
  if (record.forbidden_prompt_hits?.length) {
    promptIssues.push(`${record.asset_id}: forbidden prompt hits`);
  }
  if (!String(record.source_reference_policy ?? "").includes("Do not use external game imagery")) {
    promptIssues.push(`${record.asset_id}: weak source reference policy`);
  }
}

const thirdPartyCandidates = schemaRecords
  .filter((record) => record.buy_instead_of_generate === true || /marketplace/i.test(`${record.tool_recommendation} ${record.reference_strategy}`))
  .map((record) => ({
    asset_id: record.asset_id,
    tool_recommendation: record.tool_recommendation,
    status: "blocked_until_license_source_and_rework_review",
    required_evidence: [
      "vendor name/SKU or source URL",
      "commercial-use license",
      "redistribution and modification terms",
      "proof file stored in docs/Art or artifacts/Reports",
      "human art/IP review after rework into the project shape grammar"
    ]
  }));

const benchmarkScore = Number(benchmark?.score?.overall ?? 0);
const benchmarkThreshold = Number(benchmark?.production_metric_thresholds?.overall ?? 0.8);
const marketFinishPass = benchmarkScore >= benchmarkThreshold && benchmark?.verdict !== "below_market_finish";
const gate = {
  version: 1,
  generated_at: new Date().toISOString(),
  source_manifest: rel(manifestPath),
  schema_dir: rel(schemaDir),
  prompt_contract: rel(promptContractPath),
  manifest_asset_count: manifest.assets?.length ?? 0,
  schema_record_count: schemaRecords.length,
  prompt_contract_count: promptRecords.length,
  repository_authored_manifest_pass: manifestIssues.length === 0,
  prompt_contract_safety_pass: promptIssues.length === 0,
  manifest_issues: manifestIssues,
  prompt_issues: promptIssues,
  third_party_candidates: thirdPartyCandidates,
  third_party_candidate_count: thirdPartyCandidates.length,
  third_party_direct_use_approved_count: 0,
  benchmark: benchmark ? {
    report: rel(benchmarkPath),
    score_overall: benchmarkScore,
    threshold_overall: benchmarkThreshold,
    verdict: benchmark.verdict,
    market_finish_pass: marketFinishPass,
    comparison_scope: benchmark.comparison_scope
  } : {
    report: null,
    market_finish_pass: false,
    verdict: "missing_visual_benchmark"
  },
  gate_status: manifestIssues.length === 0 && promptIssues.length === 0 && marketFinishPass
    ? "commercial_safe_candidate_needs_human_review"
    : "blocked_until_market_finish_license_and_human_review",
  policy: [
    "Repository-authored procedural assets may proceed to internal prototype use after validation.",
    "Third-party or marketplace candidates are blocked until license/provenance evidence is captured.",
    "External market screenshots are aggregate metric benchmarks only and are never visual references.",
    "Market-facing approval remains blocked until benchmark finish and human art/IP review pass."
  ]
};

fs.writeFileSync(outJson, `${JSON.stringify(gate, null, 2)}\n`);
fs.writeFileSync(outMd, markdown(gate));
console.log(`Wrote ${rel(outJson)}`);
console.log(`Wrote ${rel(outMd)}`);

function readJson(file) {
  if (!fs.existsSync(file)) {
    fail(`Missing file: ${rel(file)}`);
  }
  return JSON.parse(fs.readFileSync(file, "utf8"));
}

function readJsonl(file) {
  if (!fs.existsSync(file)) {
    fail(`Missing file: ${rel(file)}`);
  }
  return fs.readFileSync(file, "utf8")
    .trim()
    .split(/\n+/)
    .filter(Boolean)
    .map((line) => JSON.parse(line));
}

function readSchemaRecords(dir) {
  if (!fs.existsSync(dir)) {
    fail(`Missing schema directory: ${rel(dir)}`);
  }
  return fs.readdirSync(dir)
    .filter((file) => /^art_.*\.json$/.test(file))
    .sort()
    .map((file) => JSON.parse(fs.readFileSync(path.join(dir, file), "utf8")));
}

function markdown(gate) {
  const lines = [
    "# Commercial Asset Gate",
    "",
    `- Manifest assets: ${gate.manifest_asset_count}`,
    `- Schema records: ${gate.schema_record_count}`,
    `- Prompt contract records: ${gate.prompt_contract_count}`,
    `- Repository-authored manifest pass: ${gate.repository_authored_manifest_pass}`,
    `- Prompt contract safety pass: ${gate.prompt_contract_safety_pass}`,
    `- Third-party candidates blocked: ${gate.third_party_candidate_count}`,
    `- Market benchmark verdict: \`${gate.benchmark.verdict}\``,
    `- Market benchmark score: ${gate.benchmark.score_overall ?? "n/a"} / ${gate.benchmark.threshold_overall ?? "n/a"}`,
    `- Gate status: \`${gate.gate_status}\``,
    "",
    "## Policy",
    "",
    ...gate.policy.map((item) => `- ${item}`),
    "",
    "## Third-Party Candidates",
    "",
  ];
  if (gate.third_party_candidates.length === 0) {
    lines.push("None.");
  } else {
    lines.push("| Asset | Tool | Status |");
    lines.push("| --- | --- | --- |");
    for (const item of gate.third_party_candidates) {
      lines.push(`| \`${item.asset_id}\` | ${item.tool_recommendation} | \`${item.status}\` |`);
    }
  }
  if (gate.manifest_issues.length > 0) {
    lines.push("", "## Manifest Issues", "", ...gate.manifest_issues.map((item) => `- ${item}`));
  }
  if (gate.prompt_issues.length > 0) {
    lines.push("", "## Prompt Issues", "", ...gate.prompt_issues.map((item) => `- ${item}`));
  }
  lines.push("");
  return lines.join("\n");
}

function rel(value) {
  return path.resolve(value).replace(`${repo}${path.sep}`, "").replaceAll(path.sep, "/");
}

function fail(message) {
  console.error(`- ${message}`);
  process.exit(1);
}
