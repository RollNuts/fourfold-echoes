# Veripsa Forge Vision

Veripsa Forge is an AI-native game development bridge.

It is not a replacement game engine. Unity remains the renderer, physics
runtime, animation runtime, audio runtime, asset importer, playtest
environment, and build system.

Forge changes the control plane:

```text
Terminal / AI agents
  -> Game IR and operation manifests
  -> GitHub pull requests
  -> Veripsa Core coordination
  -> main as desired state
  -> Unity Adapter reconciliation
  -> scene, prefab, component, capture, test, build evidence
  -> GitHub checks and PR comments
```

The core bet:

> GitHub is the shared brain. Unity is the execution and rendering data plane.

AI agents should edit stable, reviewable game definitions rather than clicking
Unity Editor UI or writing `.unity` / `.prefab` YAML directly.

The director-facing experience should not require learning Unity. The expected
review loop is:

```text
PR summary
  -> screenshot / clip / playable build
  -> audio and input notes
  -> approve, reject, or request a change
```

Unity can be open on a build machine or developer workstation, but the product
owner should be able to complete most review cycles from generated evidence and
playable artifacts.

The director's job is taste and product judgment:

- Look at the screen.
- Listen to the sound.
- Touch the playable artifact.
- Say what feels right or wrong.

Forge should absorb the operational burden around Unity setup, generated scenes,
tests, screenshots, replay capture, and PR evidence.

## Principles

- GitHub `main` is the desired-state source of truth.
- Every change lands through PRs.
- Veripsa Core manages coordination, not correctness.
- Unity Adapter verifies correctness by running Unity.
- Forge owns generated Unity regions only.
- Human-made art, animation, lighting, and final prefab work stay in manual
  regions unless explicitly imported by reference.
- Evidence beats self-report: screenshots, tests, replays, logs, and builds.
- A human reviewer should not need Unity Editor knowledge to judge game feel.
- The first target is Unity only.
- The first customer is FOURFOLD ECHOES.

## Non-Goals

- Replacing Unity.
- Building a full engine-neutral abstraction before the game proves itself.
- Automating arbitrary Unity GUI clicks as the main path.
- Letting AI overwrite manual art or hand-authored scene work.
- Treating MCP success as proof that the game feels good.
