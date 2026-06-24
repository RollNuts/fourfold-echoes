# FOURFOLD ECHOES

Private commercial Unity project for FOURFOLD ECHOES.

FOURFOLD ECHOES is a fixed-angle 3D action RPG / hack-and-slash with Echo
Phase combat, environmental puzzle pressure, loot carry-home, and solo-first
co-op-ready structure.

This repository is private because it contains the commercial runtime path,
production technique, future assets, polish direction, and Steam-facing product
work. Public Veripsa dogfood should stay in the public demo repository; shipping
game work belongs here.

## Current Gate

Gate A: one playable Unity room.

- Move
- Attack
- Dodge
- Phase switch
- Ember altar interaction
- Enemy defeat
- Gate claim
- Procedural placeholder audio
- Fixed-angle 3D block room

## Store Readiness

Product presentation guidance for the current prototype lives in
`docs/store/README.md`. These docs support Steam-facing quality planning, but do
not claim the game is Steam-ready, demo-ready, or content-complete.

## Unity

Target editor: Unity 6.3 LTS `6000.3.18f1`.

The scene is generated from project-authored C# and Unity primitive geometry.
No third-party art, animation, audio, model, font, or texture asset is committed
in this first gate.

## Repository Hygiene

- `.gitignore` excludes Unity caches, local editor files, generated Gate A
  validation output, local evidence, and shipping artifacts.
- `.gitattributes` normalizes text/YAML Unity files and routes commercial binary
  asset formats through Git LFS when they are intentionally tracked.
- Track Unity `.meta` files beside every committed Unity asset. Missing or
  regenerated GUIDs should block review once imported assets exist.
- Asset intake and provenance rules live in `docs/ASSET_RIGHTS.md`.

## Unity MCP

Unity's official MCP bridge, provided through the Unity Assistant package, is
the preferred editor automation path. Third-party Unity MCP bridges are not
installed by default.

Do not enable Unity MCP or add generated client config until a security review
confirms:

- The project uses Unity's official `com.unity.ai.assistant` MCP path.
- The bridge binds to localhost only.
- External client connections require explicit approval in Unity settings.
- Generated MCP client config stays out of git.
- The tool is used for editor automation only, not as gameplay verification.

See `docs/MCP_SECURITY.md`.

## Veripsa Forge

The Unity path is managed through Veripsa Forge: AI agents edit stable game
specification files, GitHub and Veripsa Core coordinate PRs, and Unity produces
screenshots, logs, tests, and builds as evidence.

Start here:

- `docs/forge/FORGE_VISION.md`
- `docs/forge/FORGE_ARCHITECTURE.md`
- `docs/forge/CLI_MEDIATOR.md`
- `docs/forge/EDITOR_AUTOMATION.md`
- `docs/forge/MVP.md`

Local contract checks:

```sh
node tools/forge/check.mjs
tools/forge/forge inspect project
tools/forge/forge inspect scene scene.ashen_threshold
tools/forge/forge validate command commands/samples/run-room-spike.json
```

Unity mediator command:

```sh
tools/unity_forge_command.sh commands/samples/run-room-spike.json
```

## Generate Scene

Run the Gate A Unity validation:

```sh
tools/unity_gate_a.sh
```

This generates and validates `Assets/Scenes/AshenThresholdSpike.unity`.
Set `UNITY_EDITOR` if Unity is installed outside the default Hub path.

To capture the generated camera view as local evidence:

```sh
tools/unity_capture_gate_a.sh
```

## Build Gate A

Build the generated Gate A playable app from the terminal:

```sh
tools/unity_build_gate_a.sh
```

The default artifact is the ignored macOS standalone app at
`Build/GateA/macos/FourfoldEchoesGateA.app`. Use `--run` to open it after a
successful build:

```sh
tools/unity_build_gate_a.sh --run
```

Set `UNITY_EDITOR` if Unity is installed outside the default Hub path. Use
`FOURFOLD_BUILD_DIR` or `--output-dir` to place uncommitted build artifacts in a
different ignored/local directory. Windows can be requested with
`tools/unity_build_gate_a.sh --target windows`, but it depends on the Unity
Windows standalone module being installed and is not the verified Gate A path.

## Controls

- Move: WASD / arrows
- Attack: J / left click
- Dodge: Space, commits in the current facing direction and briefly grants
  invulnerability
- Phase: [ / ]
- Phase action: hold K near altar while Ember is active
- Claim gate: E / right click

The in-game HUD shows dodge/attack recovery, chain timing, altar heat, the
current objective, and the hollow strike tell when the enemy is winding up.
