#!/usr/bin/env node
import fs from "node:fs";
import os from "node:os";
import path from "node:path";

const REQUIRED_FILE_KINDS = new Set(["fbx", "material", "prefab", "mesh_qa", "preflight"]);
const TEXTURE_PREFIX = "texture:";
const CLASSIFICATION_ORDER = {
  "not_ready": 0,
  "preview_only": 1,
  "ready": 2,
  "gameplay_bind_ready": 3
};

const options = parseArgs(process.argv.slice(2));
if (options.selfTest) {
  runSelfTest();
  process.exit(0);
}

if (!options.manifestPath) {
  usage("Missing --manifest <path>.");
}

const manifest = readJson(options.manifestPath);
const assets = selectAssets(manifest.assets, options.assets);
if (assets.length === 0) {
  usage("No matching assets found in Meshwright delivery manifest.");
}

const results = assets.map(classifyAsset);
printReport(manifest, results, options.requireLevel);

const failed = results.filter((result) => {
  return CLASSIFICATION_ORDER[result.classification] < CLASSIFICATION_ORDER[options.requireLevel];
});

if (failed.length > 0) {
  process.exitCode = 2;
}

function parseArgs(args) {
  const parsed = {
    assets: [],
    manifestPath: "",
    requireLevel: "ready",
    selfTest: false
  };

  for (let index = 0; index < args.length; index += 1) {
    const arg = args[index];
    if (arg === "--self-test") {
      parsed.selfTest = true;
      continue;
    }
    if (arg === "--manifest") {
      parsed.manifestPath = takeValue(args, ++index, arg);
      continue;
    }
    if (arg === "--asset") {
      parsed.assets.push(takeValue(args, ++index, arg));
      continue;
    }
    if (arg === "--require") {
      const value = takeValue(args, ++index, arg);
      if (!(value in CLASSIFICATION_ORDER)) {
        usage(`Unsupported --require value: ${value}`);
      }
      parsed.requireLevel = value;
      continue;
    }
    usage(`Unknown argument: ${arg}`);
  }

  return parsed;
}

function takeValue(args, index, flag) {
  const value = args[index];
  if (!value || value.startsWith("--")) {
    usage(`Missing value for ${flag}.`);
  }
  return value;
}

function usage(message) {
  if (message) {
    console.error(message);
  }
  console.error(
    [
      "Usage:",
      "  node tools/AssetPipeline/check_meshwright_delivery_manifest.mjs --manifest <delivery-manifest.json>",
      "  [--asset <asset_id>] [--require not_ready|preview_only|ready|gameplay_bind_ready]",
      "",
      "Default gate requires ready. Use --require gameplay_bind_ready before binding generated",
      "characters, skeletons, mannequins, or enemy rigs into runtime gameplay."
    ].join("\n")
  );
  process.exit(1);
}

function readJson(filePath) {
  try {
    return JSON.parse(fs.readFileSync(filePath, "utf8"));
  } catch (error) {
    console.error(`Failed to read Meshwright delivery manifest: ${error.message}`);
    process.exit(1);
  }
}

function selectAssets(assets, selectedAssetIds) {
  if (!Array.isArray(assets)) {
    usage("Meshwright delivery manifest is missing an assets array.");
  }
  if (selectedAssetIds.length === 0) {
    return assets;
  }
  const selected = new Set(selectedAssetIds);
  return assets.filter((asset) => selected.has(asset.asset_id));
}

function classifyAsset(asset) {
  const files = Array.isArray(asset.files) ? asset.files : [];
  const presentKinds = new Set(files.filter((file) => file.exists).map((file) => file.kind));
  const missingRequired = Array.isArray(asset.missing_required) ? asset.missing_required : [];
  const missingKinds = new Set(missingRequired.map((file) => file.kind));
  const acceptance = asset.acceptance && typeof asset.acceptance === "object" ? asset.acceptance : {};

  const hasTexture = files.some((file) => file.exists && String(file.kind).startsWith(TEXTURE_PREFIX));
  const hasCorePreview =
    presentKinds.has("fbx") &&
    presentKinds.has("mesh_qa") &&
    presentKinds.has("preflight") &&
    hasTexture &&
    acceptance.mesh_qa_ready === true &&
    acceptance.unity_import_preflight_ready === true;
  const hasRuntimeFiles = presentKinds.has("material") && presentKinds.has("prefab");
  const missingCore = [...REQUIRED_FILE_KINDS].filter((kind) => !presentKinds.has(kind));
  const missing = [...new Set([...missingKinds, ...missingCore])].sort();

  let classification = "not_ready";
  const reasons = [];

  if (hasCorePreview && !hasRuntimeFiles) {
    classification = "preview_only";
    reasons.push("MeshQA, preflight, model, and textures are present, but runtime Material/Prefab is missing.");
  }

  if (hasCorePreview && hasRuntimeFiles && missingRequired.length === 0 && asset.status === "complete") {
    classification = "ready";
    reasons.push("Required Meshwright delivery files are present.");
  }

  if (classification === "ready" && isGameplayBindReady(asset, acceptance)) {
    classification = "gameplay_bind_ready";
    reasons.push("Gameplay binding evidence is explicit in the manifest.");
  }

  if (classification === "not_ready") {
    reasons.push("Core Meshwright evidence is missing or not marked ready.");
  }

  return {
    assetId: asset.asset_id || "unspecified",
    category: asset.category || "unspecified",
    status: asset.status || "unspecified",
    classification,
    missing,
    reasons
  };
}

function isGameplayBindReady(asset, acceptance) {
  if (acceptance.gameplay_binding_ready === true || asset.gameplay_binding_ready === true) {
    return true;
  }

  const category = String(asset.category || "");
  const isAnimatedRuntimeCategory = category === "characters" || category === "enemies";
  if (!isAnimatedRuntimeCategory) {
    return false;
  }

  return (
    acceptance.animator_controller_ready === true &&
    acceptance.root_motion_policy !== undefined &&
    acceptance.event_binding_ready === true
  );
}

function printReport(manifest, results, requiredLevel) {
  const summary = summarize(results);
  console.log("Meshwright delivery gate");
  console.log(`Manifest: ${manifest.manifest || "unspecified"}`);
  console.log(`Required classification: ${requiredLevel}`);
  console.log(
    `Summary: ${summary.gameplay_bind_ready} gameplay_bind_ready, ${summary.ready} ready, ` +
      `${summary.preview_only} preview_only, ${summary.not_ready} not_ready`
  );
  console.log("");
  console.log("| asset_id | category | manifest_status | classification | missing | reason |");
  console.log("| --- | --- | --- | --- | --- | --- |");
  for (const result of results) {
    console.log(
      `| ${escapeCell(result.assetId)} | ${escapeCell(result.category)} | ${escapeCell(result.status)} | ` +
        `${escapeCell(result.classification)} | ${escapeCell(result.missing.join(", ") || "-")} | ` +
        `${escapeCell(result.reasons.join(" "))} |`
    );
  }
}

function summarize(results) {
  return results.reduce(
    (acc, result) => {
      acc[result.classification] += 1;
      return acc;
    },
    {
      gameplay_bind_ready: 0,
      ready: 0,
      preview_only: 0,
      not_ready: 0
    }
  );
}

function escapeCell(value) {
  return String(value).replace(/\|/g, "\\|").replace(/\n/g, " ");
}

function runSelfTest() {
  const fixture = {
    manifest: "fixture.yaml",
    assets: [
      {
        asset_id: "preview_asset",
        category: "characters",
        status: "incomplete",
        missing_required: [
          { kind: "material", exists: false },
          { kind: "prefab", exists: false }
        ],
        acceptance: {
          mesh_qa_ready: true,
          unity_import_preflight_ready: true
        },
        files: [
          { kind: "fbx", exists: true },
          { kind: "mesh_qa", exists: true },
          { kind: "preflight", exists: true },
          { kind: "texture:BaseColor", exists: true }
        ]
      },
      {
        asset_id: "runtime_asset",
        category: "environment_props",
        status: "complete",
        missing_required: [],
        acceptance: {
          mesh_qa_ready: true,
          unity_import_preflight_ready: true
        },
        files: [
          { kind: "fbx", exists: true },
          { kind: "material", exists: true },
          { kind: "prefab", exists: true },
          { kind: "mesh_qa", exists: true },
          { kind: "preflight", exists: true },
          { kind: "texture:BaseColor", exists: true }
        ]
      },
      {
        asset_id: "bound_character",
        category: "characters",
        status: "complete",
        missing_required: [],
        acceptance: {
          animator_controller_ready: true,
          event_binding_ready: true,
          mesh_qa_ready: true,
          root_motion_policy: "disabled",
          unity_import_preflight_ready: true
        },
        files: [
          { kind: "fbx", exists: true },
          { kind: "material", exists: true },
          { kind: "prefab", exists: true },
          { kind: "mesh_qa", exists: true },
          { kind: "preflight", exists: true },
          { kind: "texture:BaseColor", exists: true }
        ]
      }
    ]
  };

  const tempPath = path.join(os.tmpdir(), `meshwright-delivery-${process.pid}.json`);
  fs.writeFileSync(tempPath, JSON.stringify(fixture, null, 2));
  const loaded = readJson(tempPath);
  fs.rmSync(tempPath, { force: true });
  const results = loaded.assets.map(classifyAsset);
  assertEqual(results[0].classification, "preview_only", "preview asset classification");
  assertEqual(results[1].classification, "ready", "runtime asset classification");
  assertEqual(results[2].classification, "gameplay_bind_ready", "bound character classification");
  console.log("Meshwright delivery gate self-test passed.");
}

function assertEqual(actual, expected, label) {
  if (actual !== expected) {
    throw new Error(`${label}: expected ${expected}, got ${actual}`);
  }
}
