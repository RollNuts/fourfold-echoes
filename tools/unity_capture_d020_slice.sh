#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-}"
FOURFOLD_EVIDENCE_DIR="${FOURFOLD_EVIDENCE_DIR:-$PROJECT_ROOT/artifacts/Previews}"
export FOURFOLD_EVIDENCE_DIR

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

"$UNITY_EDITOR" \
  -batchmode \
  -quit \
  -projectPath "$PROJECT_ROOT" \
  -executeMethod FourfoldEchoes.Editor.FourfoldUnityEvidenceCapture.CaptureD020Slice \
  -logFile -

echo "D-020 vertical slice camera evidence: $FOURFOLD_EVIDENCE_DIR/d020-slice-camera.png"
echo "D-020 HUD reward/save evidence: $FOURFOLD_EVIDENCE_DIR/d020-hud-reward-save.png"
