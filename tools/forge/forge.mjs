#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const root = path.resolve(new URL("../..", import.meta.url).pathname);
const allowedActions = new Set([
  "inspect_project",
  "inspect_scene",
  "plan_operation",
  "run_room_spike",
  "capture_scene",
  "run_playmode_tests"
]);

function readText(relativePath) {
  return fs.readFileSync(path.join(root, relativePath), "utf8");
}

function listFiles(relativePath) {
  const absolutePath = path.join(root, relativePath);
  if (!fs.existsSync(absolutePath)) {
    return [];
  }
  return fs.readdirSync(absolutePath).sort();
}

function inspectProject() {
  const project = readText("game-spec/project.yaml");
  const scenes = listFiles("game-spec/scenes");
  const entities = listFiles("game-spec/entities");
  const scenarios = listFiles("game-spec/scenarios");
  const summary = {
    projectSpec: "game-spec/project.yaml",
    hasFourfoldEchoes: project.includes("project.fourfold_echoes"),
    scenes,
    entities,
    scenarios
  };
  console.log(JSON.stringify(summary, null, 2));
}

function sceneIdToFile(sceneId) {
  if (!/^scene\.[a-z0-9_.-]+$/.test(sceneId ?? "")) {
    throw new Error("Scene ID must match scene.<stable_id>.");
  }

  return `${sceneId.replace(/^scene\./, "").replaceAll("_", "-")}.yaml`;
}

function inspectScene(sceneId) {
  if (!sceneId) {
    throw new Error("Missing scene ID.");
  }

  const sceneFile = sceneIdToFile(sceneId);
  const scenePath = path.join("game-spec/scenes", sceneFile);
  const sceneText = readText(scenePath);
  const entityFiles = listFiles("game-spec/entities");
  const scenarioFiles = listFiles("game-spec/scenarios");
  const summary = {
    sceneId,
    sceneSpec: scenePath,
    hasSceneId: sceneText.includes(sceneId),
    referencedEntityFiles: entityFiles,
    referencedScenarioFiles: scenarioFiles,
    unityGeneratedRoot: "unity/Assets/Generated/VeripsaForge"
  };

  console.log(JSON.stringify(summary, null, 2));
}

function validateCommand(commandPath) {
  if (!commandPath) {
    throw new Error("Missing command path.");
  }

  const fullPath = path.resolve(root, commandPath);
  if (!fullPath.startsWith(root + path.sep)) {
    throw new Error("Command path must stay inside the repository.");
  }

  const command = JSON.parse(fs.readFileSync(fullPath, "utf8"));
  const errors = [];

  if (!/^cmd\.[a-z0-9_.-]+$/.test(command.commandId ?? "")) {
    errors.push("commandId must match cmd.<stable_id>.");
  }
  if (!allowedActions.has(command.action)) {
    errors.push(`Unsupported action: ${command.action}`);
  }
  if (command.sceneId && !/^scene\.[a-z0-9_.-]+$/.test(command.sceneId)) {
    errors.push("sceneId must match scene.<stable_id>.");
  }
  if (command.seed !== undefined && (!Number.isInteger(command.seed) || command.seed < 0)) {
    errors.push("seed must be a non-negative integer.");
  }

  if (errors.length > 0) {
    console.error(JSON.stringify({ ok: false, errors }, null, 2));
    process.exit(1);
  }

  console.log(JSON.stringify({ ok: true, commandId: command.commandId, action: command.action }, null, 2));
}

function usage() {
  console.error(`Usage:
  tools/forge/forge inspect project
  tools/forge/forge inspect scene <scene_id>
  tools/forge/forge validate command <path>`);
  process.exit(2);
}

const [, , command, subcommand, arg] = process.argv;

try {
  if (command === "inspect" && subcommand === "project") {
    inspectProject();
  } else if (command === "inspect" && subcommand === "scene") {
    inspectScene(arg);
  } else if (command === "validate" && subcommand === "command") {
    validateCommand(arg);
  } else {
    usage();
  }
} catch (error) {
  console.error(error instanceof Error ? error.message : String(error));
  process.exit(1);
}
