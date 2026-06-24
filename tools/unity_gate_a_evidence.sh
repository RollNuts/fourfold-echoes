#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-}"
FOURFOLD_EVIDENCE_DIR="${FOURFOLD_EVIDENCE_DIR:-/tmp/fourfold-evidence-harness}"

usage() {
  cat <<'USAGE'
Usage: tools/unity_gate_a_evidence.sh [--output-dir PATH]

Produces structured Gate A evidence from Unity batchmode:
  - gate-a-evidence.json
  - gate-a-main-camera.png
  - gate-a-objective-gate.png
  - unity.log

Environment:
  UNITY_EDITOR             Unity executable path. Defaults to Unity Hub 6000.3.18f1 when present.
  FOURFOLD_EVIDENCE_DIR    Evidence output directory. Defaults to /tmp/fourfold-evidence-harness.
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --output-dir)
      if [[ $# -lt 2 ]]; then
        echo "--output-dir requires a value." >&2
        exit 2
      fi
      FOURFOLD_EVIDENCE_DIR="$2"
      shift 2
      ;;
    --output-dir=*)
      FOURFOLD_EVIDENCE_DIR="${1#*=}"
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage >&2
      exit 2
      ;;
  esac
done

if [[ -z "$UNITY_EDITOR" ]]; then
  for candidate in \
    "/Applications/Unity/Hub/Editor/6000.3.18f1/Unity.app/Contents/MacOS/Unity" \
    /Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity; do
    if [[ -x "$candidate" ]]; then
      UNITY_EDITOR="$candidate"
      break
    fi
  done
fi

if [[ -z "$UNITY_EDITOR" || ! -x "$UNITY_EDITOR" ]]; then
  echo "Unity editor not found. Install Unity 6.3 LTS 6000.3.18f1 or set UNITY_EDITOR." >&2
  exit 2
fi

if [[ "$FOURFOLD_EVIDENCE_DIR" != /* ]]; then
  FOURFOLD_EVIDENCE_DIR="$PROJECT_ROOT/$FOURFOLD_EVIDENCE_DIR"
fi

mkdir -p "$FOURFOLD_EVIDENCE_DIR"

REPORT_PATH="$FOURFOLD_EVIDENCE_DIR/gate-a-evidence.json"
UNITY_LOG_PATH="$FOURFOLD_EVIDENCE_DIR/unity.log"

export FOURFOLD_EVIDENCE_DIR
export FOURFOLD_UNITY_LOG_PATH="$UNITY_LOG_PATH"
export FOURFOLD_PROJECT_ROOT="$PROJECT_ROOT"
export FOURFOLD_GIT_COMMIT="$(git -C "$PROJECT_ROOT" rev-parse HEAD 2>/dev/null || true)"
export FOURFOLD_GIT_BRANCH="$(git -C "$PROJECT_ROOT" branch --show-current 2>/dev/null || true)"

set +e
"$UNITY_EDITOR" \
  -batchmode \
  -quit \
  -projectPath "$PROJECT_ROOT" \
  -executeMethod FourfoldEchoes.Editor.FourfoldGateAEvidenceReport.Capture \
  -logFile "$UNITY_LOG_PATH"
UNITY_EXIT=$?
set -e

node - "$REPORT_PATH" "$UNITY_LOG_PATH" "$UNITY_EXIT" <<'NODE'
const fs = require("node:fs");
const path = require("node:path");

const reportPath = process.argv[2];
const logPath = process.argv[3];
const unityExitCode = Number(process.argv[4]);

function readReport() {
  if (!fs.existsSync(reportPath)) {
    return {
      schema_version: "fourfold.gate_a.evidence.v1",
      generated_at_utc: new Date().toISOString(),
      run_status: unityExitCode === 0 ? "report_missing" : "unity_failed_before_report",
      project_path: process.env.FOURFOLD_PROJECT_ROOT || "",
      evidence_directory: path.dirname(reportPath),
      git_commit: process.env.FOURFOLD_GIT_COMMIT || "",
      git_branch: process.env.FOURFOLD_GIT_BRANCH || "",
      scene_validation: {
        status: "not_run",
        message: "Unity did not write the editor evidence report."
      },
      captures: []
    };
  }

  return JSON.parse(fs.readFileSync(reportPath, "utf8"));
}

function scanUnityLog() {
  if (!fs.existsSync(logPath)) {
    return {
      status: "log_missing",
      unity_log_path: logPath,
      error_count: 0,
      sample_lines: [],
      unity_exit_code: unityExitCode
    };
  }

  const lines = fs.readFileSync(logPath, "utf8").split(/\r?\n/);
  const errorPattern = /\b(?:[A-Za-z]*Exception|Error|Assertion failed|Compilation failed|Build failed|Fatal error|Shader error)\b/i;
  const ignorePattern = /\b(?:totalErrors=0|0 errors|no compiler errors)\b/i;
  const environmentNoisePattern = /^(?:\[Licensing::(?:Client|Module)\] Error:|\[usbmuxd\] Error:|Curl error 42: Callback aborted$)/;
  const errorLikeMatches = lines
    .map((line) => line.trim())
    .filter((line) => line.length > 0 && errorPattern.test(line) && !ignorePattern.test(line));
  const ignoredEnvironmentMatches = errorLikeMatches.filter((line) => environmentNoisePattern.test(line));
  const actionableMatches = errorLikeMatches.filter((line) => !environmentNoisePattern.test(line));

  return {
    status: actionableMatches.length === 0
      ? (ignoredEnvironmentMatches.length === 0 ? "clean" : "clean_with_ignored_environment_noise")
      : "errors_found",
    unity_log_path: logPath,
    checked_at_utc: new Date().toISOString(),
    error_count: actionableMatches.length,
    sample_lines: actionableMatches.slice(0, 10).map((line) => line.slice(0, 240)),
    raw_error_like_count: errorLikeMatches.length,
    ignored_environment_error_count: ignoredEnvironmentMatches.length,
    ignored_environment_error_samples: ignoredEnvironmentMatches.slice(0, 10).map((line) => line.slice(0, 240)),
    unity_exit_code: unityExitCode
  };
}

const report = readReport();
report.console_error_scan = scanUnityLog();
fs.writeFileSync(reportPath, `${JSON.stringify(report, null, 2)}\n`);
NODE

if [[ "$UNITY_EXIT" -ne 0 ]]; then
  echo "Gate A evidence Unity run failed. Report: $REPORT_PATH" >&2
  echo "Unity log: $UNITY_LOG_PATH" >&2
  exit "$UNITY_EXIT"
fi

for screenshot in \
  "$FOURFOLD_EVIDENCE_DIR/gate-a-main-camera.png" \
  "$FOURFOLD_EVIDENCE_DIR/gate-a-objective-gate.png"; do
  if [[ ! -s "$screenshot" ]]; then
    echo "Expected non-empty evidence screenshot is missing: $screenshot" >&2
    exit 1
  fi
done

echo "Gate A evidence report: $REPORT_PATH"
echo "Gate A Unity log: $UNITY_LOG_PATH"
echo "Gate A screenshot: $FOURFOLD_EVIDENCE_DIR/gate-a-main-camera.png"
echo "Gate A screenshot: $FOURFOLD_EVIDENCE_DIR/gate-a-objective-gate.png"
