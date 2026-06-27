#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-}"
TEST_MODE="PlayMode"
RESULTS_DIR="$PROJECT_ROOT/artifacts/unity-tests-local"
EXIT_GRACE_SECONDS="${FOURFOLD_TEST_EXIT_GRACE_SECONDS:-30}"
RUN_TIMEOUT_SECONDS="${FOURFOLD_TEST_TIMEOUT_SECONDS:-1800}"

usage() {
  cat <<'USAGE'
Usage: tools/unity_run_tests.sh [--mode EditMode|PlayMode] [--results-dir PATH]

Runs Unity Test Runner in batchmode and fails if the expected XML is missing.
Do not add -quit here; Unity Test Framework exits after the run.

Options:
  --mode EditMode|PlayMode  Test mode to run. Defaults to PlayMode.
  --results-dir PATH        Output directory for XML and Unity log.

Environment:
  UNITY_EDITOR              Unity executable path. Defaults to the editor
                            version in ProjectSettings/ProjectVersion.txt when
                            installed through Unity Hub.
  FOURFOLD_TEST_EXIT_GRACE_SECONDS
                            Seconds to wait after Test Runner completion before
                            stopping a still-live Unity process. Defaults to 30.
  FOURFOLD_TEST_TIMEOUT_SECONDS
                            Overall timeout in seconds. Defaults to 1800.
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --mode)
      if [[ $# -lt 2 ]]; then
        echo "--mode requires a value." >&2
        exit 2
      fi
      TEST_MODE="$2"
      shift 2
      ;;
    --mode=*)
      TEST_MODE="${1#*=}"
      shift
      ;;
    --results-dir)
      if [[ $# -lt 2 ]]; then
        echo "--results-dir requires a value." >&2
        exit 2
      fi
      RESULTS_DIR="$2"
      shift 2
      ;;
    --results-dir=*)
      RESULTS_DIR="${1#*=}"
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

case "$TEST_MODE" in
  EditMode|PlayMode)
    ;;
  editmode)
    TEST_MODE="EditMode"
    ;;
  playmode)
    TEST_MODE="PlayMode"
    ;;
  *)
    echo "Unsupported test mode: $TEST_MODE. Expected EditMode or PlayMode." >&2
    exit 2
    ;;
esac

if [[ -z "$UNITY_EDITOR" ]]; then
  EDITOR_VERSION="$(awk '/m_EditorVersion:/ { print $2; exit }' "$PROJECT_ROOT/ProjectSettings/ProjectVersion.txt")"
  if [[ -n "$EDITOR_VERSION" ]]; then
    CANDIDATE="/Applications/Unity/Hub/Editor/$EDITOR_VERSION/Unity.app/Contents/MacOS/Unity"
    if [[ -x "$CANDIDATE" ]]; then
      UNITY_EDITOR="$CANDIDATE"
    fi
  fi
fi

if [[ -z "$UNITY_EDITOR" ]]; then
  for candidate in /Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity; do
    if [[ -x "$candidate" ]]; then
      UNITY_EDITOR="$candidate"
      break
    fi
  done
fi

if [[ -z "$UNITY_EDITOR" || ! -x "$UNITY_EDITOR" ]]; then
  echo "Unity editor not found. Set UNITY_EDITOR or install the ProjectVersion editor." >&2
  exit 2
fi

if [[ "$RESULTS_DIR" != /* ]]; then
  RESULTS_DIR="$PROJECT_ROOT/$RESULTS_DIR"
fi

mkdir -p "$RESULTS_DIR"

RESULTS_XML="$RESULTS_DIR/${TEST_MODE}-results.xml"
UNITY_LOG="$RESULTS_DIR/unity-${TEST_MODE}.log"

rm -f "$RESULTS_XML" "$UNITY_LOG"

test_run_completed() {
  [[ -s "$RESULTS_XML" ]] \
    && grep -q "<test-run" "$RESULTS_XML" \
    && grep -q "Test run completed. Exiting with code" "$UNITY_LOG"
}

"$UNITY_EDITOR" \
  -batchmode \
  -projectPath "$PROJECT_ROOT" \
  -runTests \
  -testPlatform "$TEST_MODE" \
  -testResults "$RESULTS_XML" \
  -logFile "$UNITY_LOG" &

UNITY_PID=$!
UNITY_STATUS=
STARTED_AT=$SECONDS
COMPLETED_AT=

while kill -0 "$UNITY_PID" 2>/dev/null; do
  if (( SECONDS - STARTED_AT > RUN_TIMEOUT_SECONDS )); then
    echo "Unity $TEST_MODE test run timed out after ${RUN_TIMEOUT_SECONDS}s." >&2
    kill "$UNITY_PID" 2>/dev/null || true
    sleep 2
    kill -9 "$UNITY_PID" 2>/dev/null || true
    wait "$UNITY_PID" 2>/dev/null || true
    exit 124
  fi

  if test_run_completed; then
    if [[ -z "$COMPLETED_AT" ]]; then
      COMPLETED_AT=$SECONDS
    elif (( SECONDS - COMPLETED_AT >= EXIT_GRACE_SECONDS )); then
      echo "Unity $TEST_MODE test run completed but the editor stayed alive; stopping spawned Unity process." >&2
      kill "$UNITY_PID" 2>/dev/null || true
      sleep 2
      kill -9 "$UNITY_PID" 2>/dev/null || true
      wait "$UNITY_PID" 2>/dev/null || true
      UNITY_STATUS=0
      break
    fi
  fi

  sleep 2
done

if [[ -z "$UNITY_STATUS" ]]; then
  set +e
  wait "$UNITY_PID"
  UNITY_STATUS=$?
  set -e
fi

if [[ "$UNITY_STATUS" -ne 0 && ! test_run_completed ]]; then
  echo "Unity $TEST_MODE test command failed with exit code $UNITY_STATUS." >&2
  echo "Unity log: $UNITY_LOG" >&2
  exit "$UNITY_STATUS"
fi

if [[ ! -s "$RESULTS_XML" ]]; then
  echo "Unity Test Runner did not write results XML: $RESULTS_XML" >&2
  echo "Unity log: $UNITY_LOG" >&2
  exit 1
fi

if ! grep -q "<test-run" "$RESULTS_XML"; then
  echo "Unity Test Runner results XML is not an NUnit test-run document: $RESULTS_XML" >&2
  exit 1
fi

if ! grep -q '<test-run[^>]*result="Passed"' "$RESULTS_XML"; then
  echo "Unity $TEST_MODE tests did not pass. See results XML: $RESULTS_XML" >&2
  exit 1
fi

echo "Unity $TEST_MODE results: $RESULTS_XML"
echo "Unity $TEST_MODE log: $UNITY_LOG"
