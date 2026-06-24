# CLI File-Contract Mediator

The stable Forge bridge is a CLI plus file contract.

Unity is invoked through official editor command line behavior:

```sh
Unity \
  -batchmode \
  -quit \
  -projectPath "$PROJECT_ROOT" \
  -executeMethod FourfoldEchoes.Editor.Mediator.FourfoldForgeMediator.Run \
  --commandFile commands/samples/run-room-spike.json \
  -logFile artifacts/logs/run-room-spike-unity.log
```

The command method reads one JSON command, performs allowed operations, writes
JSONL events and artifacts, then exits with a meaningful code.

## Contract Paths

```text
commands/
events/
artifacts/
  screenshots/
  video/
  logs/
  test-results/
```

Generated artifacts are not source of truth. They are review evidence.

## Command Example

```json
{
  "commandId": "cmd.0001",
  "action": "run_room_spike",
  "sceneId": "scene.ashen_threshold",
  "seed": 1234,
  "evidence": {
    "screenshot": true,
    "video": false,
    "playmodeLog": true
  }
}
```

## Event Example

```json
{
  "event": "room_clear",
  "commandId": "cmd.0001",
  "semanticId": "entity.object.ember_altar",
  "status": "ok",
  "artifacts": [
    "artifacts/screenshots/gate-a-camera.png",
    "artifacts/logs/playmode.log"
  ]
}
```

## Allowed Actions For MVP

- `inspect_project`
- `inspect_scene`
- `plan_operation`
- `run_room_spike`
- `capture_scene`
- `run_playmode_tests`

## Deny By Default

The mediator must reject:

- Asset deletion.
- Manual asset modification.
- Package installation.
- Project upgrade.
- Arbitrary method execution.
- External network calls.
- Writes outside approved artifact and generated paths.

## MCP Relationship

Official Unity MCP may later call the mediator, but the mediator remains the
stable interface. MCP is a transport, not the product logic.
