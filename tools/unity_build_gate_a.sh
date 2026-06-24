#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-}"
FOURFOLD_BUILD_TARGET="${FOURFOLD_BUILD_TARGET:-macos}"
FOURFOLD_BUILD_DIR="${FOURFOLD_BUILD_DIR:-Build/GateA}"
RUN_AFTER_BUILD=0

usage() {
  cat <<'USAGE'
Usage: tools/unity_build_gate_a.sh [--target macos|windows] [--output-dir PATH] [--run]

Builds the generated Gate A playable artifact from the terminal.

Options:
  --target macos|windows   Build target. macos is verified first; windows requires Unity module support.
  --output-dir PATH        Build output root. Defaults to ignored Build/GateA.
  --run                    Open the macOS .app after a successful build.

Environment:
  UNITY_EDITOR             Unity executable path. Defaults to Unity Hub 6000.3.18f1 when present.
  FOURFOLD_BUILD_TARGET    Same as --target.
  FOURFOLD_BUILD_DIR       Same as --output-dir.
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --target)
      if [[ $# -lt 2 ]]; then
        echo "--target requires a value." >&2
        exit 2
      fi
      FOURFOLD_BUILD_TARGET="$2"
      shift 2
      ;;
    --target=*)
      FOURFOLD_BUILD_TARGET="${1#*=}"
      shift
      ;;
    --output-dir)
      if [[ $# -lt 2 ]]; then
        echo "--output-dir requires a value." >&2
        exit 2
      fi
      FOURFOLD_BUILD_DIR="$2"
      shift 2
      ;;
    --output-dir=*)
      FOURFOLD_BUILD_DIR="${1#*=}"
      shift
      ;;
    --run)
      RUN_AFTER_BUILD=1
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

if [[ "$FOURFOLD_BUILD_DIR" != /* ]]; then
  FOURFOLD_BUILD_DIR="$PROJECT_ROOT/$FOURFOLD_BUILD_DIR"
fi

case "$FOURFOLD_BUILD_TARGET" in
  macos|osx|standaloneosx)
    ARTIFACT_PATH="$FOURFOLD_BUILD_DIR/macos/FourfoldEchoesGateA.app"
    ;;
  windows|win64|standalonewindows64)
    ARTIFACT_PATH="$FOURFOLD_BUILD_DIR/windows/FourfoldEchoesGateA.exe"
    ;;
  *)
    echo "Unsupported build target: $FOURFOLD_BUILD_TARGET. Expected macos or windows." >&2
    exit 2
    ;;
esac

export FOURFOLD_BUILD_TARGET
export FOURFOLD_BUILD_DIR

"$UNITY_EDITOR" \
  -batchmode \
  -quit \
  -projectPath "$PROJECT_ROOT" \
  -executeMethod FourfoldEchoes.Editor.FourfoldUnityBuild.BuildGateA \
  -logFile - \
  --fourfoldBuildTarget "$FOURFOLD_BUILD_TARGET" \
  --fourfoldBuildDir "$FOURFOLD_BUILD_DIR"

if [[ ! -e "$ARTIFACT_PATH" ]]; then
  echo "Gate A build artifact missing after successful Unity command: $ARTIFACT_PATH" >&2
  exit 1
fi

ARTIFACT_SIZE="$(du -sh "$ARTIFACT_PATH" | awk '{print $1}')"
echo "Gate A build artifact: $ARTIFACT_PATH"
echo "Gate A build artifact size: $ARTIFACT_SIZE"

if [[ "$RUN_AFTER_BUILD" -eq 1 ]]; then
  case "$FOURFOLD_BUILD_TARGET" in
    macos|osx|standaloneosx)
      open "$ARTIFACT_PATH"
      ;;
    *)
      echo "--run is only supported for macOS builds from this script." >&2
      exit 3
      ;;
  esac
fi
