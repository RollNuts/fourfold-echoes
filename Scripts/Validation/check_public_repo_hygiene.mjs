#!/usr/bin/env node
import { execFileSync } from "node:child_process";
import fs from "node:fs";

const tracked = execFileSync("git", ["ls-files", "-z"], { encoding: "utf8" })
  .split("\0")
  .filter(Boolean);
const untracked = execFileSync("git", ["ls-files", "--others", "--exclude-standard", "-z"], {
  encoding: "utf8"
})
  .split("\0")
  .filter(Boolean);
const candidateFiles = [...new Set([...tracked, ...untracked])];

const trackedPathRejects = [
  { name: "unity-library", pattern: /^Library\// },
  { name: "unity-logs", pattern: /^Logs\// },
  { name: "unity-user-settings", pattern: /^UserSettings\// },
  { name: "unity-temp", pattern: /^Temp\// },
  { name: "mac-ds-store", pattern: /(^|\/)\.DS_Store$/ }
];

const textRejects = [
  { name: "absolute-user-home-path", pattern: /\/Users\/[A-Za-z0-9._-]+(?:\/|$)/ },
  { name: "private-temp-path", pattern: /\/private\/tmp(?:\/|$)|\/var\/folders(?:\/|$)/ },
  { name: "local-file-url", pattern: /file:\/\/\/(?:Users|private|var)\// },
  { name: "private-key", pattern: /BEGIN (?:RSA |OPENSSH |EC |DSA )?PRIVATE KEY/ },
  { name: "github-token", pattern: /(?:ghp|github_pat)_[A-Za-z0-9_]{20,}/ },
  { name: "generic-api-key-assignment", pattern: /(?:API_KEY|ACCESS_TOKEN|SECRET_TOKEN|PRIVATE_TOKEN)\s*[=:]\s*['"]?[A-Za-z0-9_\-]{16,}/i },
  { name: "pem-file-reference", pattern: /[^\s]+\.pem(?:\s|$)/ }
];

const textExtensions = new Set([
  ".cs", ".js", ".mjs", ".json", ".yaml", ".yml", ".md", ".txt",
  ".csv", ".sh", ".xml", ".asset", ".meta", ".unity", ".prefab",
  ".mat", ".gitignore", ".gitattributes"
]);

const errors = [];

for (const file of candidateFiles) {
  for (const reject of trackedPathRejects) {
    if (reject.pattern.test(file)) {
      errors.push(`${file}: tracked forbidden local/generated path (${reject.name})`);
    }
  }

  if (!isTextCandidate(file)) continue;
  let text;
  try {
    text = fs.readFileSync(file, "utf8");
  } catch (error) {
    errors.push(`${file}: failed to read text candidate (${error.message})`);
    continue;
  }

  for (const reject of textRejects) {
    const match = reject.pattern.exec(text);
    if (match) {
      errors.push(`${file}: forbidden public-repo text (${reject.name})`);
    }
  }
}

if (errors.length > 0) {
  console.error("Public hygiene check failed:");
  for (const error of errors) {
    console.error(`- ${error}`);
  }
  process.exit(1);
}

console.log(
  `Public hygiene check passed: scanned ${candidateFiles.length} tracked/untracked files.`
);

function isTextCandidate(file) {
  if (file === "AGENTS.md" || file === "README.md") return true;
  if (file.includes("/Library/") || file.includes("/Temp/") || file.includes("/Logs/")) return false;
  const dot = file.lastIndexOf(".");
  if (dot < 0) return false;
  return textExtensions.has(file.slice(dot));
}
