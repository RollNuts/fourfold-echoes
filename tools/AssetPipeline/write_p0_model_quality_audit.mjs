#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();
const manifestPath = path.join(repo, "artifacts/Reports/fourfold-model-pack.json");
const reportJsonPath = path.join(repo, "artifacts/Reports/p0-model-quality-audit.json");
const reportMdPath = path.join(repo, "artifacts/Reports/p0-model-quality-audit.md");

if (!fs.existsSync(manifestPath)) {
  throw new Error(`Missing manifest: ${path.relative(repo, manifestPath)}`);
}

const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
const p0 = manifest.assets.filter((asset) => asset.priority === "P0");
const rows = p0.map((asset) => {
  const threshold = thresholdFor(asset);
  const checks = {
    triangles: Number(asset.triangles_lod0) >= threshold.triangles,
    parts: Number(asset.part_count ?? asset.object_count ?? 0) >= threshold.parts,
    materials: Number(asset.material_count) >= threshold.materialsMin && Number(asset.material_count) <= Number(asset.material_budget),
    anchors: Array.isArray(asset.required_readability_anchors) && asset.required_readability_anchors.length >= threshold.anchors,
    original_license: asset.license === "repository-authored",
  };
  return {
    name: asset.name,
    asset_kind: asset.asset_kind,
    category: asset.category,
    builder: asset.builder,
    triangles_lod0: asset.triangles_lod0,
    part_count: asset.part_count ?? asset.object_count,
    material_count: asset.material_count,
    material_budget: asset.material_budget,
    required_threshold: threshold,
    checks,
    pass: Object.values(checks).every(Boolean),
  };
});

const failures = rows.filter((row) => !row.pass);
const report = {
  version: 1,
  generated_at: new Date().toISOString(),
  source_manifest: "artifacts/Reports/fourfold-model-pack.json",
  scope: "P0 generated models only",
  note: "This is a first-pass model-density and usability audit, not final art approval.",
  p0_asset_count: rows.length,
  pass_count: rows.length - failures.length,
  fail_count: failures.length,
  gate_status: failures.length === 0 ? "p0_model_density_gate_passed" : "p0_model_density_gate_failed",
  rows,
};

fs.mkdirSync(path.dirname(reportJsonPath), { recursive: true });
fs.writeFileSync(reportJsonPath, `${JSON.stringify(report, null, 2)}\n`);
fs.writeFileSync(reportMdPath, markdown(report));
console.log(`Wrote ${path.relative(repo, reportJsonPath)}`);
console.log(`Wrote ${path.relative(repo, reportMdPath)}`);
if (failures.length > 0) {
  console.log(`P0 quality audit has ${failures.length} failures; see report.`);
}

function thresholdFor(asset) {
  switch (asset.asset_kind) {
    case "Hero":
      return { triangles: 500, parts: 50, materialsMin: 4, anchors: 4 };
    case "ServiceNPC":
      return { triangles: 300, parts: 32, materialsMin: 4, anchors: 3 };
    case "Combatant":
      return { triangles: 230, parts: 24, materialsMin: 3, anchors: 3 };
    case "Boss":
      return { triangles: 320, parts: 34, materialsMin: 4, anchors: 4 };
    case "Equipment":
      return { triangles: 100, parts: 10, materialsMin: 3, anchors: 3 };
    case "Interactable":
      return { triangles: 120, parts: 12, materialsMin: 2, anchors: 3 };
    case "Pickup":
      return { triangles: 60, parts: 6, materialsMin: 1, anchors: 2 };
    case "Boundary":
      return { triangles: 60, parts: 5, materialsMin: 2, anchors: 2 };
    case "Tile":
      return { triangles: 48, parts: 4, materialsMin: 2, anchors: 2 };
    case "GroundDecal":
    case "Detail":
      return { triangles: 24, parts: 2, materialsMin: 1, anchors: 1 };
    default:
      return { triangles: 80, parts: 8, materialsMin: 1, anchors: 2 };
  }
}

function markdown(report) {
  const lines = [
    "# P0 Model Quality Audit",
    "",
    report.note,
    "",
    `- Gate status: \`${report.gate_status}\``,
    `- P0 assets: \`${report.p0_asset_count}\``,
    `- Passed: \`${report.pass_count}\``,
    `- Failed: \`${report.fail_count}\``,
    "",
    "| Asset | Kind | Tris | Parts | Materials | Status |",
    "| --- | --- | ---: | ---: | ---: | --- |",
  ];
  for (const row of report.rows) {
    lines.push(`| ${row.name} | ${row.asset_kind} | ${row.triangles_lod0} / ${row.required_threshold.triangles} | ${row.part_count} / ${row.required_threshold.parts} | ${row.material_count} / ${row.material_budget} | ${row.pass ? "pass" : "fail"} |`);
  }
  if (report.fail_count > 0) {
    lines.push("", "## Failures", "");
    for (const row of report.rows.filter((item) => !item.pass)) {
      const failed = Object.entries(row.checks).filter(([, pass]) => !pass).map(([key]) => key).join(", ");
      lines.push(`- ${row.name}: ${failed}`);
    }
  }
  return `${lines.join("\n")}\n`;
}
