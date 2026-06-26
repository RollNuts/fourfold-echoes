#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ACTION="${1:-d020.build_and_validate}"
COMMAND_ID="${2:-queued.$(date +%Y%m%d%H%M%S)}"
INBOX="$PROJECT_ROOT/Temp/FourfoldForgeInbox/commands"

mkdir -p "$INBOX"

cat > "$INBOX/$(printf '%s' "$COMMAND_ID" | tr -c 'A-Za-z0-9_.-' '_').ready.json" <<JSON
{
  "commandId": "$COMMAND_ID",
  "action": "$ACTION"
}
JSON

echo "Queued Unity editor command: $ACTION"
