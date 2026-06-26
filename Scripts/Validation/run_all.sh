#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.18f1/Unity.app/Contents/MacOS/Unity}"

cd "$ROOT"

node Scripts/Validation/check_public_repo_hygiene.mjs
node Scripts/Validation/validate_repo.mjs
node tools/forge/check.mjs

if [[ ! -x "$UNITY_EDITOR" ]]; then
  echo "Unity editor not found: $UNITY_EDITOR" >&2
  exit 1
fi

"$UNITY_EDITOR" \
  -batchmode \
  -quit \
  -projectPath "$ROOT" \
  -executeMethod FourfoldEchoes.Editor.FourfoldProductValidator.RunAll \
  -logFile -

tools/unity_capture_d020_slice.sh

node Scripts/Validation/write_market_reports.mjs

echo "FOURFOLD validation run complete."
