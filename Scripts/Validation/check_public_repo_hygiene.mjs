#!/usr/bin/env node
import childProcess from "node:child_process";
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();
const trackedFiles = childProcess
  .execFileSync("git", ["ls-files", "-z"], { cwd: repo })
  .toString("utf8")
  .split("\0")
  .filter(Boolean);

const forbiddenTrackedPathParts = [
  ".codex/",
  ".cursor/",
  ".mcp.json",
  "Library/",
  "Temp/",
  "Logs/",
  "UserSettings/",
  "Build/",
  "Builds/",
  "CrashReports/",
  "MemoryCaptures/"
];

const forbiddenTrackedExtensions = new Set([
  ".key",
  ".keystore",
  ".p12",
  ".pem"
]);

const textExtensions = new Set([
  "",
  ".asset",
  ".cs",
  ".csv",
  ".gitattributes",
  ".gitignore",
  ".json",
  ".md",
  ".mjs",
  ".sh",
  ".txt",
  ".xml",
  ".yaml",
  ".yml"
]);

const secretLikePatterns = [
  { label: "macOS absolute user path", pattern: /\/Users\/[A-Za-z0-9._-]+/ },
  { label: "macOS private temp path", pattern: /\/private\/tmp\/[A-Za-z0-9._/-]+/ },
  { label: "macOS var folders temp path", pattern: /\/var\/folders\/[A-Za-z0-9._/-]+/ },
  { label: "Windows absolute user path", pattern: /[A-Z]:\\Users\\[A-Za-z0-9._-]+/ },
  { label: "private key block", pattern: /-----BEGIN [A-Z ]*PRIVATE KEY-----/ },
  { label: "GitHub classic token", pattern: /gh[pousr]_[A-Za-z0-9_]{20,}/ },
  { label: "GitHub fine-grained token", pattern: /github_pat_[A-Za-z0-9_]{20,}/ },
  { label: "AWS access key", pattern: /AKIA[0-9A-Z]{16}/ },
  { label: "OpenAI-style secret key", pattern: /sk-[A-Za-z0-9_-]{32,}/ },
  { label: "Slack bot token", pattern: /xox[baprs]-[A-Za-z0-9-]{20,}/ }
];

const allowlistedScannerFiles = new Set([
  "Scripts/Validation/check_public_repo_hygiene.mjs",
  "Scripts/Validation/validate_repo.mjs",
  "tools/forge/check.mjs"
]);

const errors = [];

for (const file of trackedFiles) {
  const normalized = file.replaceAll("\\", "/");
  for (const forbiddenPart of forbiddenTrackedPathParts) {
    if (normalized === forbiddenPart.slice(0, -1) || normalized.startsWith(forbiddenPart)) {
      errors.push(`Tracked local/private path is not public-safe: ${file}`);
    }
  }

  if (forbiddenTrackedExtensions.has(path.extname(file).toLowerCase())) {
    errors.push(`Tracked credential-like file extension is not public-safe: ${file}`);
  }

  if (allowlistedScannerFiles.has(normalized)) {
    continue;
  }

  const absolute = path.join(repo, file);
  if (!isTextCandidate(absolute)) {
    continue;
  }

  const content = fs.readFileSync(absolute, "utf8");
  for (const { label, pattern } of secretLikePatterns) {
    if (pattern.test(content)) {
      errors.push(`${label} found in tracked file: ${file}`);
    }
  }
}

if (errors.length > 0) {
  console.error(errors.map((error) => `- ${error}`).join("\n"));
  process.exit(1);
}

console.log(`public repo hygiene ok: scanned ${trackedFiles.length} tracked files.`);

function isTextCandidate(file) {
  const extension = path.extname(file).toLowerCase();
  if (!textExtensions.has(extension)) {
    return false;
  }

  const stat = fs.statSync(file);
  if (stat.size > 1024 * 1024) {
    return false;
  }

  const sample = fs.readFileSync(file);
  return !sample.includes(0);
}
