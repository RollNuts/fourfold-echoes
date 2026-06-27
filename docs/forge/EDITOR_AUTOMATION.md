# Editor Automation

FOURFOLD uses two Unity automation layers.

## Stable Layer

The stable layer is CLI plus file contract:

```text
commands/*.json
  -> Unity -batchmode -executeMethod
  -> events/*.jsonl
  -> artifacts/screenshots, logs, test-results
```

This layer is the PR gate and CI path. It must work without MCP.

Current command entry:

```text
FourfoldEchoes.Editor.Mediator.FourfoldForgeMediator.Run
```

Current playable build entry:

```text
tools/unity_build_gate_a.sh
  -> FourfoldEchoes.Editor.FourfoldUnityBuild.BuildGateA
  -> FourfoldUnitySpikeBuilder.BuildAndValidate
  -> Build/GateA/
```

The historical Gate A wrapper still supports macOS and Windows when the
matching Unity standalone module is available. Current commercial slice build
automation should prefer Windows because Steam Windows is the first shipping
target.

## Acceleration Layer

Local Unity MCP can accelerate day-to-day editor work once reviewed.

Allowed use:

- Inspect current scene and console logs.
- Execute reviewed `Tools/FOURFOLD/...` menu items.
- Run repository-owned commands.
- Trigger compile, tests, capture, and validation.

Not allowed use:

- Treat MCP output as proof without artifacts.
- Let MCP edit manual assets without a PR-visible command.
- Commit generated MCP client config.
- Install or enable a third-party bridge without a separate security review.

## MenuItem Policy

Menu items are thin wrappers only.

They may call:

- `FourfoldForgeMediator.Run`
- `FourfoldUnitySpikeBuilder.BuildAndValidate`
- focused validation/capture methods

They must not contain hidden gameplay logic, broad file deletion, package
installation, or arbitrary command execution.

## Agent Worktrees

Each AI agent should use a separate git worktree and therefore a separate Unity
`Library/` directory. Do not share Unity `Library/` between active agents.

Shared history and coordination happen through:

- Git branches.
- Pull requests.
- Veripsa Core.
- Recorded evidence artifacts.

Shared import speed, if needed later, should use a Unity cache service rather
than committing or copying `Library/`.

## Visual Proof

Player-visible changes require fixed evidence:

- Fixed scene.
- Fixed camera.
- Fixed resolution.
- Fixed seed.
- Captured screenshot or replay.
- Console error status.

Editor viewport screenshots can help discussion, but they are not a merge gate.
