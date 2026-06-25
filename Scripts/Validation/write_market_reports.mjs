#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();
const reportsDir = path.join(repo, "artifacts/Reports");
fs.mkdirSync(reportsDir, { recursive: true });

const generatedAtUtc = new Date().toISOString();
const unityReport = readJson("artifacts/Reports/unity-product-validation.json");
const d020Screenshot = inspectPng("artifacts/Previews/d020-slice-camera.png");
const d020ToolRead = inspectPng("artifacts/Previews/d020-tool-node-read.png");
const d020RewardRead = inspectPng("artifacts/Previews/d020-reward-read.png");
const d020SilhouetteRead = inspectPng("artifacts/Previews/d020-silhouette-read.png");
const d020PlayableAttackRead = inspectPng("artifacts/Previews/d020-playable-attack-read.png");
const d020SecondGimmickRead = inspectPng("artifacts/Previews/d020-second-gimmick-room-read.png");
const d020SecondGimmickSolved = inspectPng("artifacts/Previews/d020-second-gimmick-solved.png");
const requiredVisualEvidence = [
  "artifacts/Previews/d020-slice-camera.png",
  "artifacts/Previews/d020-tool-node-read.png",
  "artifacts/Previews/d020-reward-read.png",
  "artifacts/Previews/d020-silhouette-read.png",
  "artifacts/Previews/d020-playable-attack-read.png",
  "artifacts/Previews/d020-second-gimmick-room-read.png",
  "artifacts/Previews/d020-second-gimmick-solved.png"
].map(inspectPng);
const buildArtifact = inspectPath("Build/D020Slice/macos/FourfoldEchoesD020Slice.app");
const d020Scene = inspectPath("Assets/Scenes/D020VerticalSlice.unity");
const d020ToolRuntime = inspectPath("Assets/Scripts/ExplorationTool.cs");
const d020PlayerRuntime = inspectPath("Assets/Scripts/D020PlayerController.cs");
const d020EnemyRuntime = inspectPath("Assets/Scripts/D020EnemyDummy.cs");
const audioRows = readCsv("docs/Audio/ASSET_REGISTER.csv");
const licenseText = readText("docs/Legal/LICENSES.md");
const audioFiles = findAudioFiles("Assets/Audio");
const audioFileSet = new Set(audioFiles);
const registeredCues = audioRows.map((row) => buildAudioCue(row, audioFileSet));
const audioFileAudit = buildAudioFileAudit(registeredCues, audioFiles);

const performanceSnapshot = {
  version: 1,
  generatedAtUtc,
  scope: "D-020 vertical slice evidence snapshot",
  caveat: "This is not a runtime profiler capture. It summarizes current scene, asset, and screenshot evidence until a real playthrough/profiler scenario exists.",
  targets: {
    pcBaseline: "1080p / 60fps",
    steamDeck: "target, not yet verified",
    ps5Readiness: "asset budgets and platform-safe structure only; no PS5 build claim"
  },
  unityMetrics: unityReport?.metrics ?? null,
  visualEvidence: requiredVisualEvidence,
  d020Screenshot,
  d020ToolRead,
  d020RewardRead,
  d020SilhouetteRead,
  d020PlayableAttackRead,
  d020SecondGimmickRead,
  d020SecondGimmickSolved,
  buildArtifact,
  d020Runtime: {
    scenePath: d020Scene.path,
    sceneExists: d020Scene.exists,
    toolRuntimePath: d020ToolRuntime.path,
    toolRuntimeExists: d020ToolRuntime.exists,
    playerRuntimePath: d020PlayerRuntime.path,
    playerRuntimeExists: d020PlayerRuntime.exists,
    enemyRuntimePath: d020EnemyRuntime.path,
    enemyRuntimeExists: d020EnemyRuntime.exists,
    status: d020Scene.exists && d020ToolRuntime.exists && d020PlayerRuntime.exists && d020EnemyRuntime.exists ? "two_gimmick_rooms_one_tool_smoke" : "missing"
  },
  knownGaps: [
    "No frame-time profiler sample has been captured yet.",
    "No Steam Deck measurement has been captured yet.",
    "No PS5 hardware or platform-module verification exists.",
    "D-020 slice is technical scene evidence, not store-ready art.",
    "Blender pilot assets and production art are intentionally outside this capture/build lane.",
    "No complete 30-minute vertical slice with hub, region, miniboss, boss, save/load, final SFX, and final music exists yet."
  ]
};

const audioInventory = {
  version: 1,
  generatedAtUtc,
  scope: "Audio register and imported asset inventory",
  importedAudioClipCount: unityReport?.metrics?.audioClipAssets ?? null,
  repositoryAudioFileCount: audioFiles.length,
  registeredCues,
  repositoryAudioFiles: audioFiles,
  missingRegisteredFiles: audioFileAudit.missingRegisteredFiles,
  unregisteredAudioFiles: audioFileAudit.unregisteredAudioFiles,
  licenseCoverage: summarizeAudioLicenseCoverage(audioRows, licenseText),
  knownGaps: buildAudioKnownGaps(registeredCues, audioFileAudit)
};

const finalStatus = {
  version: 1,
  generatedAtUtc,
  finalProductDefinition: "Steam-first, buy-to-play, single-player top-down classic action-adventure.",
  canonicalHook: "Leave one hub, explore three compact handcrafted regions, master one exploration tool, open shortcuts, earn relic rewards, and defeat four bosses.",
  currentEvidence: {
    unityValidationReport: "artifacts/Reports/unity-product-validation.json",
    d020SliceScene: d020Scene.exists ? d020Scene.path : null,
    d020SliceScreenshot: d020Screenshot.exists ? d020Screenshot.path : null,
    d020ToolScreenshot: d020ToolRead.exists ? d020ToolRead.path : null,
    d020RewardScreenshot: d020RewardRead.exists ? d020RewardRead.path : null,
    d020SilhouetteScreenshot: d020SilhouetteRead.exists ? d020SilhouetteRead.path : null,
    d020PlayableAttackScreenshot: d020PlayableAttackRead.exists ? d020PlayableAttackRead.path : null,
    d020SecondGimmickScreenshot: d020SecondGimmickRead.exists ? d020SecondGimmickRead.path : null,
    d020SecondGimmickSolvedScreenshot: d020SecondGimmickSolved.exists ? d020SecondGimmickSolved.path : null,
    visualEvidence: requiredVisualEvidence.filter((evidence) => evidence.exists).map((evidence) => evidence.path),
    performanceSnapshot: "artifacts/Reports/performance-snapshot.json",
    audioInventory: "artifacts/Reports/audio-inventory.json",
    d020ToolRuntime: d020ToolRuntime.exists ? d020ToolRuntime.path : null,
    d020PlayerRuntime: d020PlayerRuntime.exists ? d020PlayerRuntime.path : null,
    d020EnemyRuntime: d020EnemyRuntime.exists ? d020EnemyRuntime.path : null,
    buildArtifact: buildArtifact.exists ? buildArtifact.path : null
  },
  marketReadyStatus: "not_market_ready",
  blockers: [
    "No top-down hub/region/boss vertical slice exists.",
    "Production hero/tool/enemy/boss silhouettes are not approved.",
    "Final music and release-quality core SFX are not implemented.",
    "No measured frame-time profiler scenario exists.",
    "Steam screenshot set is not production-ready."
  ],
  nextHighestLeverageWork: [
    "Add a full Region 01 playable path that connects hub entry, two gimmick rooms, shortcut, reward, enemy pressure, and boss entry without adding systems.",
    "Replace pilot hero/tool/enemy with production-intent stylized silhouettes and turnaround evidence.",
    "Add a non-placeholder tool pulse SFX, target-hit SFX, attack hit SFX, enemy tell SFX, and discovery stinger.",
    "Extend the automated runtime smoke to cover SFX wiring and a build-level input replay.",
    "Capture a frame-time profiler sample for the current playable test scene."
  ]
};

writeJson("artifacts/Reports/performance-snapshot.json", performanceSnapshot);
writeMarkdown("artifacts/Reports/performance-snapshot.md", buildPerformanceMarkdown(performanceSnapshot));
writeJson("artifacts/Reports/audio-inventory.json", audioInventory);
writeMarkdown("artifacts/Reports/audio-inventory.md", buildAudioMarkdown(audioInventory));
writeJson("artifacts/Reports/final-status-report.json", finalStatus);
writeMarkdown("artifacts/Reports/final-status-report.md", buildStatusMarkdown(finalStatus));

console.log("[market-reports] wrote performance, audio inventory, and final status reports.");

function readJson(relativePath) {
  const fullPath = path.join(repo, relativePath);
  if (!fs.existsSync(fullPath)) return null;
  return JSON.parse(fs.readFileSync(fullPath, "utf8"));
}

function readText(relativePath) {
  const fullPath = path.join(repo, relativePath);
  if (!fs.existsSync(fullPath)) return "";
  return fs.readFileSync(fullPath, "utf8");
}

function writeJson(relativePath, value) {
  const fullPath = path.join(repo, relativePath);
  fs.mkdirSync(path.dirname(fullPath), { recursive: true });
  fs.writeFileSync(fullPath, `${JSON.stringify(value, null, 2)}\n`);
}

function writeMarkdown(relativePath, value) {
  const fullPath = path.join(repo, relativePath);
  fs.mkdirSync(path.dirname(fullPath), { recursive: true });
  fs.writeFileSync(fullPath, value);
}

function readCsv(relativePath) {
  const text = readText(relativePath).trim();
  if (!text) return [];
  const lines = text.split(/\r?\n/);
  const headers = splitCsvLine(lines.shift());
  return lines.filter(Boolean).map((line) => {
    const cells = splitCsvLine(line);
    return Object.fromEntries(headers.map((header, index) => [header, cells[index] ?? ""]));
  });
}

function splitCsvLine(line) {
  const cells = [];
  let cell = "";
  let quoted = false;
  for (let index = 0; index < line.length; index++) {
    const character = line[index];
    if (character === "\"") {
      quoted = !quoted;
      continue;
    }
    if (character === "," && !quoted) {
      cells.push(cell);
      cell = "";
      continue;
    }
    cell += character;
  }
  cells.push(cell);
  return cells;
}

function inspectPath(relativePath) {
  const fullPath = path.join(repo, relativePath);
  if (!fs.existsSync(fullPath)) {
    return { path: relativePath, exists: false, sizeBytes: 0 };
  }
  return {
    path: relativePath,
    exists: true,
    sizeBytes: pathSize(fullPath)
  };
}

function pathSize(fullPath) {
  const stat = fs.statSync(fullPath);
  if (stat.isFile()) return stat.size;
  if (!stat.isDirectory()) return 0;
  return fs.readdirSync(fullPath).reduce((total, entry) => total + pathSize(path.join(fullPath, entry)), 0);
}

function inspectPng(relativePath) {
  const fullPath = path.join(repo, relativePath);
  if (!fs.existsSync(fullPath)) {
    return { path: relativePath, exists: false };
  }
  const buffer = fs.readFileSync(fullPath);
  const isPng = buffer.length >= 24
    && buffer[0] === 0x89
    && buffer[1] === 0x50
    && buffer[2] === 0x4e
    && buffer[3] === 0x47;
  return {
    path: relativePath,
    exists: true,
    sizeBytes: buffer.length,
    width: isPng ? buffer.readUInt32BE(16) : null,
    height: isPng ? buffer.readUInt32BE(20) : null
  };
}

function findAudioFiles(relativePath) {
  const fullPath = path.join(repo, relativePath);
  const extensions = new Set([".aif", ".aiff", ".flac", ".mp3", ".ogg", ".wav"]);
  const files = [];
  if (!fs.existsSync(fullPath)) return files;

  walk(fullPath);
  return files.sort();

  function walk(directory) {
    for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
      const entryPath = path.join(directory, entry.name);
      if (entry.isDirectory()) {
        walk(entryPath);
        continue;
      }
      if (entry.isFile() && extensions.has(path.extname(entry.name).toLowerCase())) {
        files.push(toRepoPath(entryPath));
      }
    }
  }
}

function buildAudioCue(row, presentFiles) {
  const unityPath = normalizeUnityPath(row.unity_path);
  const hasUnityPath = isConcretePath(unityPath);
  const fileExists = hasUnityPath && presentFiles.has(unityPath);
  return {
    assetId: row.asset_id,
    displayName: row.display_name,
    category: row.category,
    gameplayRole: row.gameplay_role,
    priority: row.priority,
    status: row.status,
    unityPath,
    looping: row.looping,
    license: row.license,
    acceptanceStatus: row.acceptance_status,
    sourceStrategy: row.source_strategy,
    sourceFile: normalizeUnityPath(row.source_file),
    attribution: row.attribution,
    sourceReference: row.source_reference,
    notes: row.notes,
    fileExists,
    inventoryState: hasUnityPath ? (fileExists ? "registered_file_present" : "registered_file_missing") : "no_unity_path"
  };
}

function buildAudioFileAudit(cues, audioFilesValue) {
  const registeredPaths = new Set(cues.map((cue) => cue.unityPath).filter(isConcretePath));
  return {
    missingRegisteredFiles: cues
      .filter((cue) => isConcretePath(cue.unityPath) && !cue.fileExists)
      .map((cue) => ({
        assetId: cue.assetId,
        unityPath: cue.unityPath,
        status: cue.status
      })),
    unregisteredAudioFiles: audioFilesValue.filter((audioFile) => !registeredPaths.has(audioFile))
  };
}

function summarizeAudioLicenseCoverage(rows, licenseTextValue) {
  const registered = rows.length;
  const approved = rows.filter((row) => row.license && row.license !== "TBD").length;
  const imported = rows.filter((row) => isConcretePath(normalizeUnityPath(row.unity_path))).length;
  return {
    registered,
    licenseKnown: approved,
    imported,
    licenseDocumentMentionsAudio: /audio|sfx|music|pulse/i.test(licenseTextValue)
  };
}

function buildAudioKnownGaps(cues, audit) {
  const gaps = [];
  const needed = cues.filter((cue) => cue.status === "needed").map((cue) => cue.assetId);
  const prototype = cues.filter((cue) => cue.acceptanceStatus === "prototype_only" || cue.acceptanceStatus === "visual_review_only").map((cue) => cue.assetId);

  if (audit.missingRegisteredFiles.length > 0) {
    gaps.push(`${audit.missingRegisteredFiles.length} registered audio path(s) are missing on disk.`);
  }
  if (audit.unregisteredAudioFiles.length > 0) {
    gaps.push(`${audit.unregisteredAudioFiles.length} audio file(s) exist under Assets/Audio without an audio register row.`);
  }
  if (needed.length > 0) {
    gaps.push(`${needed.join(", ")} remain registered but not implemented.`);
  }
  if (prototype.length > 0) {
    gaps.push(`${prototype.length} generated prototype cue(s) are present for readability evidence only and are not release audio.`);
  }
  gaps.push("Generated/prototype SFX are asset inventory evidence only until D-020 gameplay events or scene AudioSources wire them in.");
  return gaps;
}

function buildPerformanceMarkdown(snapshot) {
  const metrics = snapshot.unityMetrics ?? {};
  const buildSizeMb = snapshot.buildArtifact.exists ? (snapshot.buildArtifact.sizeBytes / 1024 / 1024).toFixed(1) : "n/a";
  const screenshotSize = snapshot.d020Screenshot.exists ? `${snapshot.d020Screenshot.width}x${snapshot.d020Screenshot.height}` : "missing";
  return `# Performance Snapshot

Generated UTC: \`${snapshot.generatedAtUtc}\`

Scope: ${snapshot.scope}

${snapshot.caveat}

## Current Evidence

| Metric | Value |
| --- | ---: |
| Scene objects | ${metrics.sceneObjects ?? "n/a"} |
| Renderers | ${metrics.sceneRenderers ?? "n/a"} |
| LODGroups | ${metrics.sceneLodGroups ?? "n/a"} |
| Missing materials | ${metrics.sceneMissingMaterials ?? "n/a"} |
| Missing scripts | ${metrics.sceneMissingScripts ?? "n/a"} |
| Build artifact size | ${buildSizeMb} mb |
| D-020 screenshot | ${screenshotSize} |
| D-020 runtime proof | ${snapshot.d020Runtime?.status ?? "n/a"} |

## Visual Evidence

${snapshot.visualEvidence.map((evidence) => `- \`${evidence.path}\`: ${evidence.exists ? `${evidence.width}x${evidence.height}` : "missing"}`).join("\n")}

## Known Gaps

${snapshot.knownGaps.map((gap) => `- ${gap}`).join("\n")}
`;
}

function buildAudioMarkdown(inventory) {
  return `# Audio Inventory

Generated UTC: \`${inventory.generatedAtUtc}\`

Imported AudioClip count from Unity validation: \`${inventory.importedAudioClipCount ?? "unknown"}\`

## Registered Cues

| Asset | Category | Status | License | Unity Path | Acceptance |
| --- | --- | --- | --- | --- | --- |
${inventory.registeredCues.map((cue) => `| \`${cue.assetId}\` | ${cue.category} | ${cue.status} | ${cue.license} | ${cue.unityPath} | ${cue.acceptanceStatus} |`).join("\n")}

## File Audit

- Repository audio files: ${inventory.repositoryAudioFileCount}
- Registered paths missing on disk: ${inventory.missingRegisteredFiles.length}
- Unregistered audio files: ${inventory.unregisteredAudioFiles.length}

### Missing Registered Files

${buildMissingAudioMarkdown(inventory.missingRegisteredFiles)}

### Unregistered Audio Files

${buildUnregisteredAudioMarkdown(inventory.unregisteredAudioFiles)}

## License Coverage

- Registered cues: ${inventory.licenseCoverage.registered}
- Cues with known license: ${inventory.licenseCoverage.licenseKnown}
- Cues with Unity paths: ${inventory.licenseCoverage.imported}
- License document mentions audio: ${inventory.licenseCoverage.licenseDocumentMentionsAudio}

## Known Gaps

${inventory.knownGaps.map((gap) => `- ${gap}`).join("\n")}
`;
}

function buildMissingAudioMarkdown(files) {
  if (files.length === 0) return "- None";
  return files.map((file) => `- \`${file.assetId}\` -> \`${file.unityPath}\``).join("\n");
}

function buildUnregisteredAudioMarkdown(files) {
  if (files.length === 0) return "- None";
  return files.map((file) => `- \`${file}\``).join("\n");
}

function normalizeUnityPath(value) {
  return (value ?? "").trim().replaceAll("\\", "/");
}

function isConcretePath(value) {
  return Boolean(value) && value !== "TBD";
}

function toRepoPath(fullPath) {
  return path.relative(repo, fullPath).replaceAll(path.sep, "/");
}

function buildStatusMarkdown(status) {
  return `# Final Status Report

Generated UTC: \`${status.generatedAtUtc}\`

## Final Product Definition

${status.finalProductDefinition}

Canonical hook: ${status.canonicalHook}

## Current Evidence

| Evidence | Path |
| --- | --- |
| Unity validation | \`${status.currentEvidence.unityValidationReport}\` |
| D-020 slice scene | \`${status.currentEvidence.d020SliceScene ?? "missing"}\` |
| D-020 slice screenshot | \`${status.currentEvidence.d020SliceScreenshot ?? "missing"}\` |
| D-020 tool screenshot | \`${status.currentEvidence.d020ToolScreenshot ?? "missing"}\` |
| D-020 reward screenshot | \`${status.currentEvidence.d020RewardScreenshot ?? "missing"}\` |
| D-020 silhouette screenshot | \`${status.currentEvidence.d020SilhouetteScreenshot ?? "missing"}\` |
| D-020 playable attack screenshot | \`${status.currentEvidence.d020PlayableAttackScreenshot ?? "missing"}\` |
| D-020 second gimmick screenshot | \`${status.currentEvidence.d020SecondGimmickScreenshot ?? "missing"}\` |
| D-020 second gimmick solved screenshot | \`${status.currentEvidence.d020SecondGimmickSolvedScreenshot ?? "missing"}\` |
| Visual evidence shots | ${status.currentEvidence.visualEvidence?.length ?? 0} generated |
| Performance snapshot | \`${status.currentEvidence.performanceSnapshot}\` |
| Audio inventory | \`${status.currentEvidence.audioInventory}\` |
| D-020 tool runtime | \`${status.currentEvidence.d020ToolRuntime ?? "missing"}\` |
| D-020 player runtime | \`${status.currentEvidence.d020PlayerRuntime ?? "missing"}\` |
| D-020 enemy runtime | \`${status.currentEvidence.d020EnemyRuntime ?? "missing"}\` |
| Build artifact | \`${status.currentEvidence.buildArtifact ?? "missing"}\` |

## Market Ready Status

\`${status.marketReadyStatus}\`

## Blockers

${status.blockers.map((blocker) => `- ${blocker}`).join("\n")}

## Exact Next Highest-Leverage Work

${status.nextHighestLeverageWork.map((item) => `- ${item}`).join("\n")}
`;
}
