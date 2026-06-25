# Technical Architecture

## Goal

Support a Steam-first solo compact top-down classic action-adventure with one
exploration tool, controller-first feel, local save data, stylized 3D
presentation, and later console-portable architecture without introducing
network, live-service, large-world, or multi-system dependencies.

## Engine And Rendering

| Area | Direction | Status |
| --- | --- | --- |
| Engine | Unity 6 series | current |
| Render pipeline | keep current pipeline until a measured migration proves value | current/audit required |
| Input | controller-first action abstraction | required before playable slice acceptance |
| Asset loading | direct scene references first; Addressables only when production scale demands it | deferred |
| Exploration tool | one runtime tool with reactive nodes | canonical after D-020 |
| Physics | Unity 3D physics | current/prototype |
| Networking | none for v1 | canonical |

## Scene Structure

Production scene loading should stay simple until the vertical slice proves the
game loop:

- Bootstrap
- PersistentSystems
- Hub_Crossroads
- Region scene
- Boss arena scene
- UI

The first playable proof may use one scene, but the architecture must separate
persistent systems from content scenes so hub, region, and boss transitions do
not become hardcoded one-off logic.

## Core Runtime Boundaries

| Boundary | Rule |
| --- | --- |
| Save | gameplay calls save service; no direct file I/O in gameplay code |
| Platform | Steam/PS5/etc. go through platform service wrappers |
| Input | gameplay reads action abstraction, not raw device specifics |
| Exploration tool | gameplay calls `ExplorationTool`; world objects implement small reactive node behavior |
| UI | controller navigation first |
| Assets | production references stable prefab/material paths; generated evidence stays labeled as evidence |
| Audio | gameplay requests semantic cues, not raw clip paths where avoidable |

## Playable Base Requirements

- movement
- camera
- normal attack
- dodge
- one enemy
- hit detection
- hit feedback
- basic SFX wiring
- one exploration tool node that visibly changes the room

No content quantity is accepted before these feel good.

## Vertical Slice Requirements

- one hub
- one exploration area
- two gimmick rooms using the same tool differently
- one shortcut that opens and is saved
- two normal enemies
- one miniboss
- one boss
- two relic rewards without inventory complexity
- minimal UI
- BGM 2 tracks
- required SFX categories wired
- save abstraction path
- screenshot/capture path
- profiler sample

## Performance Budget Starting Point

| Target | Budget |
| --- | --- |
| PC baseline | 1080p / 60fps |
| Steam Deck | readable UI at 1280x800, measured before demo |
| Boss arena | stable frame pacing during VFX and boss tells |
| Materials | shared atlases/trim sheets where possible |
| LOD | required for outdoor landmarks and repeated props |
| Cameras | avoid high-cost multi-camera setups |

## Migration Notes

The current repository contains Built-in pipeline prototype evidence. Do not
silently claim URP, HDRP, Steam Deck, or console readiness. Any render-pipeline
migration requires:

1. package/config audit
2. material/shader conversion path
3. lighting comparison screenshots
4. build validation
5. performance snapshot
6. rollback plan

## Non-Goals

- multiplayer architecture
- server simulation
- MMO content streaming
- procedural infinite world
- large-world streaming
- second exploration tool
- inventory/crafting/quest/social frameworks
- PS5-specific SDK calls before partner access
- multi-state world services
