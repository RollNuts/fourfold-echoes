#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";
import { execFileSync } from "node:child_process";

const repo = process.cwd();
const outDir = path.join(repo, "artifacts/Reports");

const lanes = [
  {
    id: "PR-A",
    name: "product-canon",
    order: 1,
    patterns: [
      /^AGENTS\.md$/,
      /^README\.md$/,
      /^docs\/DECISIONS\.md$/,
      /^docs\/Product\//,
      /^game-spec\/project\.yaml$/,
      /^game-spec\/(scenes|entities|scenarios)\/d020-/,
      /^game-spec\/(scenes|entities|scenarios)\/(ashen-threshold|gate-a-clear-room|gate-a)\.yaml$/,
      /^commands\/samples\/run-room-spike\.json$/
    ]
  },
  {
    id: "PR-B",
    name: "art-audio-direction",
    order: 2,
    patterns: [
      /^docs\/Art\//,
      /^docs\/Audio\//,
      /^docs\/ASSET_RIGHTS\.md$/,
      /^Assets\/Audio(\.meta|\/)/,
      /^tools\/AudioPipeline\//
    ]
  },
  {
    id: "PR-C",
    name: "production-release-plans",
    order: 3,
    patterns: [
      /^docs\/Production\//,
      /^docs\/QA\//,
      /^docs\/Marketing\//,
      /^docs\/Release\//,
      /^docs\/Legal\//,
      /^docs\/Tech\//
    ]
  },
  {
    id: "PR-D",
    name: "validation-sync",
    order: 4,
    patterns: [
      /^artifacts\/\.gitignore$/,
      /^Scripts\/Validation\/validate_repo\.mjs$/,
      /^Scripts\/Validation\/write_veripsa_split_report\.mjs$/,
      /^tools\/forge\/check\.mjs$/,
      /^commands\/samples\/inspect-d020-slice\.json$/,
      /^artifacts\/Reports\/\.gitkeep$/,
      /^artifacts\/Reports\/veripsa-current-split\.(json|md)$/
    ]
  },
  {
    id: "PR-E1",
    name: "d020-tool-runtime",
    order: 5,
    patterns: [
      /^Assets\/Scripts\/D020EnemyDummy\.cs(\.meta)?$/,
      /^Assets\/Scripts\/D020PlayerController\.cs(\.meta)?$/,
      /^Assets\/Scripts\/D020ProgressSave\.cs(\.meta)?$/,
      /^Assets\/Scripts\/ExplorationTool\.cs(\.meta)?$/,
      /^Assets\/Scripts\/ExplorationNode\.cs(\.meta)?$/
    ]
  },
  {
    id: "PR-E2",
    name: "d020-scene-evidence",
    order: 6,
    patterns: [
      /^Assets\/Art\.meta$/,
      /^Assets\/Art\/Generated\.meta$/,
      /^Assets\/Art\/Generated\/D020(\.meta|\/)/,
      /^Assets\/Editor\/FourfoldD020SliceSceneBuilder\.cs(\.meta)?$/,
      /^Assets\/Scenes\/D020VerticalSlice\.unity(\.meta)?$/,
      /^ProjectSettings\/EditorBuildSettings\.asset$/
    ]
  },
  {
    id: "PR-E3",
    name: "d020-capture-build",
    order: 7,
    patterns: [
      /^\.gitignore$/,
      /^Assets\/Editor\/FourfoldD020PlayableSmoke\.cs(\.meta)?$/,
      /^Assets\/Editor\/FourfoldUnityEvidenceCapture\.cs$/,
      /^Assets\/Editor\/FourfoldUnityBuild\.cs$/,
      /^Scripts\/Build\//,
      /^Scripts\/Validation\/run_all\.sh$/,
      /^Scripts\/Validation\/write_market_reports\.mjs$/,
      /^tools\/unity_capture_d020_slice\.sh$/,
      /^artifacts\/Previews\/(\.gitkeep|README\.md|d020-)/,
      /^artifacts\/Reports\/(audio-inventory|final-status-report|performance-snapshot|unity-product-validation)\.(json|md)$/
    ]
  },
  {
    id: "PR-F",
    name: "historical-proof-cleanup",
    order: 8,
    patterns: [
      /^Assets\/Scripts\/EchoPhaseState\.cs(\.meta)?$/,
      /^Assets\/Scripts\/FourfoldProductReviewController\.cs(\.meta)?$/,
      /^Assets\/Editor\/FourfoldProductReviewSceneBuilder\.cs(\.meta)?$/,
      /^Assets\/Editor\/FourfoldProductValidator\.cs(\.meta)?$/,
      /^Assets\/Scenes\/ProductReviewSandbox\.unity(\.meta)?$/,
      /^Assets\/Art\/Generated\/Materials(\.meta|\/ProductReview_)/,
      /^Assets\/Art\/Generated\/Meshes(\.meta|\/product_review_)/,
      /^Assets\/Art\/Generated\/Prefabs(\.meta|\/(BossReadTarget|EnemyReadTarget|PlayerReadTarget|RewardReadTarget|WorldLandmark)\.prefab)/,
      /^Assets\/Art\/Generated\/Textures(\.meta|\/product_review_)/,
      /^artifacts\/Previews\/(demo_path_contact_sheet|enemy_turnaround|hero_area_day|hero_area_night_or_alt_phase|landmark_sheet|phase_comparison|player_turnaround|product-review-camera|store_candidate_shots)\.png$/,
      /^tools\/unity_capture_product_review\.sh$/,
      /^events\/\.gitkeep$/
    ]
  },
  {
    id: "PR-G",
    name: "forge-mediator-sync",
    order: 9,
    patterns: [
      /^Assets\/Editor\/Mediator\/FourfoldForgeMediator\.cs$/,
      /^Assets\/Editor\/Mediator\/FourfoldForgeMenuItems\.cs$/,
      /^tools\/unity_forge_command\.sh$/,
      /^ProjectSettings\/PackageManagerSettings\.asset$/
    ]
  },
  {
    id: "PR-H",
    name: "asset-pipeline-pilot-optional",
    order: 10,
    patterns: [
      /^tools\/AssetPipeline\//,
      /^tools\/Blender\//,
      /^Assets\/Art\/Generated\/BlenderPilot(\.meta|\/)/,
      /^artifacts\/Previews\/BlenderPilot\//,
      /^artifacts\/Reports\/blender-pilot-assets\.json$/
    ]
  }
];

const coreObservations = [
  {
    source: "PR #17 codex/store-readiness-pack",
    verdict: "SUCCESS",
    implication: "Docs-only changes with a narrow path set are a Veripsa-friendly landing unit."
  },
  {
    source: "PR #14 codex/gate-a-evidence-harness",
    verdict: "NEUTRAL",
    implication:
      "New Unity editor/tool files were not in main's graph, so Core treated them as unknown. Split new C# runtime, editor scene generation, and capture/build tooling into separate PRs instead of landing them with docs."
  }
];

const porcelain = execFileSync("git", ["status", "--short", "--untracked-files=all"], {
  cwd: repo,
  encoding: "utf8"
});

const entries = porcelain
  .split("\n")
  .filter(Boolean)
  .map(parseStatusLine)
  .map((entry) => ({ ...entry, lane: classify(entry.path) }));

const grouped = Object.fromEntries(lanes.map((lane) => [lane.id, []]));
const unknown = [];

for (const entry of entries) {
  if (entry.lane) {
    grouped[entry.lane.id].push(entry);
  } else {
    unknown.push(entry);
  }
}

const report = {
  generatedUtc: new Date().toISOString(),
  coreSource: "GitHub Veripsa checks; local veripsa CLI unavailable",
  coreObservations,
  dirtyFileCount: entries.length,
  lanes: lanes.map((lane) => ({
    id: lane.id,
    name: lane.name,
    order: lane.order,
    files: grouped[lane.id].map((entry) => ({
      status: entry.status,
      path: entry.path
    }))
  })),
  unknown: unknown.map((entry) => ({
    status: entry.status,
    path: entry.path
  }))
};

fs.mkdirSync(outDir, { recursive: true });
fs.writeFileSync(
  path.join(outDir, "veripsa-current-split.json"),
  `${JSON.stringify(report, null, 2)}\n`
);
fs.writeFileSync(path.join(outDir, "veripsa-current-split.md"), renderMarkdown(report));

console.log(
  `[veripsa-split] wrote ${entries.length} dirty paths across ${lanes.length} lanes; unknown=${unknown.length}.`
);

function parseStatusLine(line) {
  const status = line.slice(0, 2);
  const rawPath = line.slice(3);
  const renameSeparator = " -> ";
  const filePath = rawPath.includes(renameSeparator)
    ? rawPath.slice(rawPath.indexOf(renameSeparator) + renameSeparator.length)
    : rawPath;
  return { status, path: filePath };
}

function classify(filePath) {
  return lanes.find((lane) => lane.patterns.some((pattern) => pattern.test(filePath))) ?? null;
}

function renderMarkdown(report) {
  const lines = [
    "# Veripsa Current Split Report",
    "",
    `Generated UTC: \`${report.generatedUtc}\``,
    "",
    `Core source: ${report.coreSource}`,
    "",
    "## Core-Derived Rules",
    "",
    ...report.coreObservations.flatMap((observation) => [
      `- ${observation.source}: Veripsa \`${observation.verdict}\`. ${observation.implication}`
    ]),
    "",
    `Dirty files: ${report.dirtyFileCount}`,
    "",
    "## Recommended Lanes",
    ""
  ];

  for (const lane of report.lanes) {
    lines.push(`### ${lane.id} - ${lane.name}`);
    lines.push("");
    if (lane.files.length === 0) {
      lines.push("- None");
    } else {
      for (const file of lane.files) {
        lines.push(`- \`${file.status.trim() || "M"}\` ${file.path}`);
      }
    }
    lines.push("");
  }

  lines.push("## Unknown / Needs Manual Lane");
  lines.push("");
  if (report.unknown.length === 0) {
    lines.push("- None");
  } else {
    for (const file of report.unknown) {
      lines.push(`- \`${file.status.trim() || "M"}\` ${file.path}`);
    }
  }
  lines.push("");
  lines.push("## Use");
  lines.push("");
  lines.push("- Keep PR-A through PR-D reviewable before any Unity scene/build lane lands.");
  lines.push("- Treat new Unity C# and generated scene paths as Veripsa UNKNOWN until main indexes them.");
  lines.push("- Do not ACK a Veripsa Pause without reading the overlapping files and recording why the D-020 lane is authoritative.");
  lines.push("");
  return `${lines.join("\n").trimEnd()}\n`;
}
