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

## Unity

Target editor: Unity 6.3 LTS `6000.3.18f1`.

The scene is generated from project-authored C# and Unity primitive geometry.
No third-party art, animation, audio, model, font, or texture asset is committed
in this first gate.

## Unity MCP

This project includes `com.coplaydev.unity-mcp` pinned to `v9.7.3`.

After Unity resolves packages, open the editor and use:

`Window > MCP for Unity > Configure All Detected Clients`

Keep generated user-local MCP config out of git unless it is explicitly reviewed
as a portable project config.

## Generate Scene

Run the Gate A Unity validation:

```sh
tools/unity_gate_a.sh
```

This generates and validates `Assets/Scenes/AshenThresholdSpike.unity`.
Set `UNITY_EDITOR` if Unity is installed outside the default Hub path.

## Controls

- Move: WASD / arrows
- Attack: J / left click
- Dodge: Space
- Phase: [ / ]
- Phase action: hold K near altar while Ember is active
- Claim gate: E / right click
