#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ARTIFACT_PATH="$PROJECT_ROOT/Build/FourfoldEchoes/macos/FourfoldEchoes.app"
LOG_PATH="$PROJECT_ROOT/artifacts/logs/player-smoke-product-loop.log"
REPORT_PATH="$PROJECT_ROOT/artifacts/Reports/player-smoke-product-loop.md"
TIMEOUT_SECONDS=45

usage() {
  cat <<'USAGE'
Usage: tools/unity_smoke_product_loop.sh [--artifact PATH] [--log PATH] [--report PATH] [--timeout SECONDS]

Launches the built product loop directly and verifies that the packaged player can
route Title -> HubCrossroads -> D020VerticalSlice while preserving the user's save.

Options:
  --artifact PATH      Built .app or .exe. Defaults to Build/FourfoldEchoes/macos/FourfoldEchoes.app.
  --log PATH           Player log output. Defaults to artifacts/logs/player-smoke-product-loop.log.
  --report PATH        Sanitized report output. Defaults to artifacts/Reports/player-smoke-product-loop.md.
  --timeout SECONDS    Watchdog timeout. Defaults to 45.
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --artifact)
      if [[ $# -lt 2 ]]; then
        echo "--artifact requires a value." >&2
        exit 2
      fi
      ARTIFACT_PATH="$2"
      shift 2
      ;;
    --artifact=*)
      ARTIFACT_PATH="${1#*=}"
      shift
      ;;
    --log)
      if [[ $# -lt 2 ]]; then
        echo "--log requires a value." >&2
        exit 2
      fi
      LOG_PATH="$2"
      shift 2
      ;;
    --log=*)
      LOG_PATH="${1#*=}"
      shift
      ;;
    --report)
      if [[ $# -lt 2 ]]; then
        echo "--report requires a value." >&2
        exit 2
      fi
      REPORT_PATH="$2"
      shift 2
      ;;
    --report=*)
      REPORT_PATH="${1#*=}"
      shift
      ;;
    --timeout)
      if [[ $# -lt 2 ]]; then
        echo "--timeout requires a value." >&2
        exit 2
      fi
      TIMEOUT_SECONDS="$2"
      shift 2
      ;;
    --timeout=*)
      TIMEOUT_SECONDS="${1#*=}"
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

if [[ "$ARTIFACT_PATH" != /* ]]; then
  ARTIFACT_PATH="$PROJECT_ROOT/$ARTIFACT_PATH"
fi

if [[ "$LOG_PATH" != /* ]]; then
  LOG_PATH="$PROJECT_ROOT/$LOG_PATH"
fi

if [[ "$REPORT_PATH" != /* ]]; then
  REPORT_PATH="$PROJECT_ROOT/$REPORT_PATH"
fi

if [[ ! "$TIMEOUT_SECONDS" =~ ^[0-9]+$ || "$TIMEOUT_SECONDS" -lt 5 ]]; then
  echo "--timeout must be an integer >= 5." >&2
  exit 2
fi

case "$ARTIFACT_PATH" in
  *.app)
    PLAYER_BIN="$ARTIFACT_PATH/Contents/MacOS/FourfoldEchoes"
    ;;
  *.exe)
    PLAYER_BIN="$ARTIFACT_PATH"
    ;;
  *)
    echo "Unsupported artifact path: $ARTIFACT_PATH. Expected .app or .exe." >&2
    exit 2
    ;;
esac

if [[ ! -x "$PLAYER_BIN" ]]; then
  echo "Built player executable is missing or not executable: $PLAYER_BIN" >&2
  exit 1
fi

mkdir -p "$(dirname "$LOG_PATH")"
mkdir -p "$(dirname "$REPORT_PATH")"
rm -f "$LOG_PATH"

"$PLAYER_BIN" \
  -batchmode \
  -logFile "$LOG_PATH" \
  --fourfoldLaunchSmoke &
PLAYER_PID=$!

deadline=$((SECONDS + TIMEOUT_SECONDS))
while kill -0 "$PLAYER_PID" 2>/dev/null; do
  if [[ "$SECONDS" -ge "$deadline" ]]; then
    kill "$PLAYER_PID" 2>/dev/null || true
    wait "$PLAYER_PID" 2>/dev/null || true
    echo "Product loop player smoke timed out after ${TIMEOUT_SECONDS}s. See $LOG_PATH" >&2
    exit 1
  fi
  sleep 1
done

wait "$PLAYER_PID" || PLAYER_EXIT=$?
PLAYER_EXIT="${PLAYER_EXIT:-0}"

if ! grep -q "FOURFOLD PLAYER SMOKE PASS" "$LOG_PATH"; then
  echo "Product loop player smoke did not find pass sentinel. Player exit=$PLAYER_EXIT. See $LOG_PATH" >&2
  exit 1
fi

if [[ "$PLAYER_EXIT" -ne 0 ]]; then
  echo "Product loop player smoke pass sentinel was present but player exit=$PLAYER_EXIT. See $LOG_PATH" >&2
  exit 1
fi

relpath() {
  local value="$1"
  case "$value" in
    "$PROJECT_ROOT"/*)
      printf '%s\n' "${value#"$PROJECT_ROOT"/}"
      ;;
    *)
      printf '%s\n' "$value"
      ;;
  esac
}

ARTIFACT_REL="$(relpath "$ARTIFACT_PATH")"
LOG_REL="$(relpath "$LOG_PATH")"
REPORT_REL="$(relpath "$REPORT_PATH")"
ARTIFACT_SIZE="$(du -sh "$ARTIFACT_PATH" | awk '{print $1}')"
GENERATED_UTC="$(date -u '+%Y-%m-%dT%H:%M:%SZ')"

cat > "$REPORT_PATH" <<REPORT
# Product Loop Player Route Smoke

Generated UTC: \`$GENERATED_UTC\`

- Result: PASS
- Artifact: \`$ARTIFACT_REL\`
- Artifact size: \`$ARTIFACT_SIZE\`
- Log: \`$LOG_REL\`
- Sentinel: \`FOURFOLD PLAYER SMOKE PASS\`

This report is intentionally sanitized for public commit. The raw player log is local evidence and is not committed.
The runtime smoke snapshots and restores the local save around its New Game route check.
REPORT

echo "Product loop player smoke passed: $ARTIFACT_REL"
echo "Product loop player smoke log: $LOG_REL"
echo "Product loop player smoke report: $REPORT_REL"
