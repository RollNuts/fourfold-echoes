#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();

const requiredFiles = [
  "README.md",
  "tools/forge/check.mjs",
  "tools/forge/forge.mjs",
  "contracts/schemas/forge-command.schema.json",
  "contracts/schemas/forge-event.schema.json",
  "game-spec/project.yaml"
];

const errors = [];

for (const file of requiredFiles) {
  if (!fs.existsSync(path.join(repo, file))) {
    errors.push(`Missing required repo file: ${file}`);
  }
}

const packageJsonPath = path.join(repo, "package.json");
if (fs.existsSync(packageJsonPath)) {
  try {
    JSON.parse(fs.readFileSync(packageJsonPath, "utf8"));
  } catch (error) {
    errors.push(`Invalid package.json: ${error.message}`);
  }
}

for (const file of [
  "contracts/schemas/forge-command.schema.json",
  "contracts/schemas/forge-event.schema.json"
]) {
  const fullPath = path.join(repo, file);
  if (!fs.existsSync(fullPath)) continue;
  try {
    JSON.parse(fs.readFileSync(fullPath, "utf8"));
  } catch (error) {
    errors.push(`Invalid JSON in ${file}: ${error.message}`);
  }
}

const readme = readIfExists("README.md");
if (readme && !readme.includes("FOURFOLD ECHOES")) {
  errors.push("README.md must identify FOURFOLD ECHOES.");
}

const projectSpec = readIfExists("game-spec/project.yaml");
if (projectSpec && !projectSpec.includes("project.fourfold_echoes")) {
  errors.push("game-spec/project.yaml must identify project.fourfold_echoes.");
}

if (errors.length > 0) {
  console.error("FOURFOLD repo validation failed:");
  for (const error of errors) {
    console.error(`- ${error}`);
  }
  process.exit(1);
}

console.log(`FOURFOLD repo validation passed: ${requiredFiles.length} baseline files present.`);

function readIfExists(file) {
  const fullPath = path.join(repo, file);
  return fs.existsSync(fullPath) ? fs.readFileSync(fullPath, "utf8") : "";
}
