#!/usr/bin/env node
import childProcess from "node:child_process";
import fs from "node:fs";
import path from "node:path";

const root = path.resolve(new URL("../..", import.meta.url).pathname);
const textRoots = [
  "README.md",
  "docs",
  "commands",
  "contracts",
  "game-spec",
  "Assets/Editor/Mediator",
  "tools/forge",
  "tools/unity_forge_command.sh",
  "artifacts",
  "events"
];
const jsonFiles = [
  "contracts/schemas/forge-command.schema.json",
  "contracts/schemas/forge-event.schema.json",
  "commands/samples/run-room-spike.json"
];
const forbidden = /\/Users\/|TOKEN|SECRET|PRIVATE KEY|pem|api[_-]?key|github_pat|ghp_|gho_/;

function run(command, args) {
  const result = childProcess.spawnSync(command, args, {
    cwd: root,
    encoding: "utf8",
    stdio: "pipe"
  });

  if (result.status !== 0) {
    process.stderr.write(result.stdout);
    process.stderr.write(result.stderr);
    throw new Error(`${command} ${args.join(" ")} failed`);
  }

  process.stdout.write(result.stdout);
}

function walk(relativePath) {
  const absolutePath = path.join(root, relativePath);
  if (!fs.existsSync(absolutePath)) {
    return [];
  }
  const stat = fs.statSync(absolutePath);
  if (stat.isFile()) {
    return [relativePath];
  }

  return fs.readdirSync(absolutePath).flatMap((entry) => {
    const child = path.join(relativePath, entry);
    const childStat = fs.statSync(path.join(root, child));
    if (childStat.isDirectory()) {
      return walk(child);
    }
    return [child];
  });
}

function parseJsonFiles() {
  for (const file of jsonFiles) {
    JSON.parse(fs.readFileSync(path.join(root, file), "utf8"));
  }
}

function scanTextFiles() {
  const matches = [];
  for (const file of textRoots.flatMap(walk)) {
    if (file === "tools/forge/check.mjs") {
      continue;
    }
    const absolutePath = path.join(root, file);
    if (!fs.statSync(absolutePath).isFile()) {
      continue;
    }
    const content = fs.readFileSync(absolutePath, "utf8");
    if (forbidden.test(content)) {
      matches.push(file);
    }
  }

  if (matches.length > 0) {
    throw new Error(`Forbidden private or secret-like text found in: ${matches.join(", ")}`);
  }
}

run("tools/forge/forge", ["inspect", "project"]);
run("tools/forge/forge", ["inspect", "scene", "scene.ashen_threshold"]);
run("tools/forge/forge", ["validate", "command", "commands/samples/run-room-spike.json"]);
parseJsonFiles();
scanTextFiles();
console.log("forge check ok");
