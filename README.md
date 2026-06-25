# FOURFOLD ECHOES

Private commercial Unity project for FOURFOLD ECHOES.

FOURFOLD ECHOES is being reset toward a Steam-first, single-player,
premium-indie compact top-down classic action-adventure. The canonical direction
is a focused fantasy game built around one exploration tool, three compact
handcrafted regions, four bosses, readable combat, shortcuts, relic rewards, and
strong stylized art/audio.

This repository is private because it contains the commercial runtime path,
production technique, future assets, polish direction, and Steam-facing product
work. Public Veripsa dogfood should stay in the public demo repository; shipping
game work belongs here.

## Current Canon

The current product target is documented in:

- `docs/Product/CANONICAL_PRODUCT_SPEC.md`
- `docs/Product/MVP_BLUEPRINT.md`
- `docs/Product/MARKET_TARGET.md`
- `docs/Product/CORE_SYSTEMS.md`
- `docs/Product/VERTICAL_SLICE_CONTENT.md`
- `docs/Product/SCOPE_BOUNDARIES.md`

`docs/Product/REPO_TIMELINE_AUDIT.md`, Gate A, ProductReviewSandbox, Echo Phase,
and compact-open-world references are historical context unless D-020 documents
explicitly re-accept them. D-020 is the current source of truth.

## Historical Prototypes

Older Gate A and ProductReviewSandbox work is historical prototype evidence.
It is not the product core, not the current control contract, and not the visual
target. Use `docs/Product/REPO_TIMELINE_AUDIT.md` when legacy context is needed.

## Unity Direction

Target editor: Unity 6.3 LTS `6000.3.18f1`.

The current product scene target is `scene.d020_vertical_slice` in `game-spec`.
It is the first canonical D-020 proof target, not the old Gate A room.

The next runtime proof must become a controllable Region 01 test room with
movement, camera, normal attack, dodge, one enemy, the single exploration tool,
one shortcut response, one reward, and non-placeholder core SFX.

## Repository Hygiene

- `.gitignore` excludes Unity caches, local editor files, generated validation
  output, local evidence, and shipping artifacts.
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

The Forge documents are retained as prototype infrastructure. They may be
generalized later, but product work must not bend itself around old prototype
shape.

Start here:

- `docs/forge/FORGE_VISION.md`
- `docs/forge/FORGE_ARCHITECTURE.md`
- `docs/forge/CLI_MEDIATOR.md`
- `docs/forge/EDITOR_AUTOMATION.md`
- `docs/forge/MVP.md`

Local contract checks that are valid for this canon lane:

```sh
node tools/forge/check.mjs
tools/forge/forge inspect project
tools/forge/forge inspect scene scene.d020_vertical_slice
```

Legacy prototype commands must not be advertised from this canonical README.
If a later maintenance task needs them, document that task in a dedicated
historical tooling note instead of turning the old prototype back into the
project entrypoint.
