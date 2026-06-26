# FOURFOLD ECHOES Agent Rules

This private repository is the commercial runtime path for FOURFOLD ECHOES.

## Protected Repositories

- Do not delete, rewrite, or repurpose the separate public Veripsa dogfood repository clone.
- `veripsa-games` is the public Veripsa dogfood repository and is not the
  shipping game source.
- Work for the commercial game belongs in this private `fourfold-echoes`
  repository.

## Current Product Direction

Treat this section as the current source of truth.

FOURFOLD ECHOES is a Steam-first, buy-to-own, single-player top-down classic
action-adventure.

The game is compact by design. It is not an open world. It is built around:

> a hub, three handcrafted regions, four bosses, and one visually clear
> exploration tool that turns repeated rules into mastery.

The MVP ceiling is strict:

- 1 hub
- 3 regions
- 4 bosses
- 1 exploration tool
- no inventory
- no crafting
- no quest log
- no social systems
- no open world
- no day/night cycle
- no fishing, farming, base building, MMO, co-op, PvP, live service, or gacha

Old references to Gate A, ProductReviewSandbox, Echo Phase world-state,
extraction, co-op-ready, hack-and-slash loot, compact open-world structure, or
older D020/D021 planning packs are historical. New work must use the D022 pack
only.

## Core Design Rules

- Add no new systems unless an accepted decision states what existing content is
  removed to pay for them.
- Depth comes from repeated use of one exploration tool, enemy placement,
  room layout, boss patterns, shortcuts, and player improvement.
- Art and audio are part of the core game, not late decoration.
- Do not end a milestone with gray boxes or placeholder sound when the milestone
  claims visual or market validation.
- Code may be extensible, but do not build broad abstractions before the MVP
  proves the game.

## Source Of Truth

Use these files first:

- `docs/Production/D022_TOPDOWN_ADVENTURE_MVP/00_IMPLEMENTATION_SPEC.md`
- `docs/Production/D022_TOPDOWN_ADVENTURE_MVP/01_CORE_SYSTEMS.md`
- `docs/Production/D022_TOPDOWN_ADVENTURE_MVP/02_ART_AUDIO_DIRECTION.md`
- `docs/Production/D022_TOPDOWN_ADVENTURE_MVP/03_VERTICAL_SLICE_AND_SCOPE.md`
- `docs/Production/D022_TOPDOWN_ADVENTURE_MVP/04_QA_RELEASE_STORE.md`

Older product, art, audio, production, QA, and marketing docs are historical
unless updated to explicitly point at the D022 pack.

## Technology Direction

- Unity 6.3 LTS (`6000.3.18f1`) is the current editor target.
- Keep runtime architecture portable to future console work.
- Controller-first.
- Local save only for v1.
- No networking dependency in v1.
- Platform-specific services go through wrappers when they become necessary.

## Required Output Shape

For substantial tasks, report:

1. how the task supports the final goal
2. systems touched
3. files added/changed
4. implementation
5. tests
6. acceptance conditions
7. next smallest useful task
