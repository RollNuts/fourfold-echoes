# FOURFOLD ECHOES Commercial Gap Map

Date: 2026-06-27

This map is based on the accepted `main` branch, not on dirty local lanes. The
current shipping direction is a Steam-first, buy-to-own, single-player top-down
classic action-adventure with one hub, three handcrafted regions, four bosses,
and one exploration tool. Historical Gate A, Echo Phase, loot, co-op-ready, and
Forge-only wording is treated as prototype context unless it is still visible in
accepted runtime evidence.

## Current Accepted Evidence

| Area | Evidence on `main` | Commercial read |
| --- | --- | --- |
| Playable runtime | `Assets/Scripts/FourfoldUnitySpikeController.cs` and `Assets/Editor/FourfoldUnitySpikeBuilder.cs` | Gate A can generate one playable room with move, attack, dodge, altar, gate, reward, HUD, VFX/audio feedback. |
| Enemy readability | `Assets/Scripts/Enemies/*`, `Assets/Tests/PlayMode/EnemyControllerPlayModeTests.cs`, DangerCircle VFX assets | Enemy AI, hit flash, defeat flash, and attack telegraph readability are accepted in isolated runtime/test paths. |
| Build and capture | `tools/unity_gate_a.sh`, `tools/unity_capture_gate_a.sh`, `tools/unity_build_gate_a.sh`, `FourfoldUnityBuild.cs` | Local Gate A generation, capture, and build tooling exist; Windows/Steam clean-launch evidence is not yet current. |
| Product docs | `README.md`, `docs/forge/MVP.md`, `reports/commercial-release-backlog-2026-06-27.md` | There is useful prototype and backlog context, but the current product source-of-truth docs named in AGENTS are absent from accepted `main`. |
| Asset rights | `docs/ASSET_RIGHTS.md`, generated primitive/procedural assets | Provenance discipline exists; store-ready no-placeholder asset acceptance is still open. |
| Dirty worktree | Main checkout contains many tracked production prefab, scene, script, report, and asset edits | Treat as other-agent work. Do not stage broadly or use as proof for accepted `main`. |

## Ordered Commercial Gaps

| Rank | Gap | Player-visible next move | MVP-safe constraint |
| --- | --- | --- | --- |
| P0-01 | Gate A must be controller-first from first glance, not only technically joystick-readable. | Done on `main`: concrete controller prompts are visible in the runtime HUD. | No input system swap, no new menu, no settings screen. |
| P0-02 | Accepted `main` still proves one Gate A room, not a 20-30 minute commercial slice. | Show an in-room next-route beacon after clear, then extend the same one-tool loop through one more accepted room beat. | No inventory, quest log, open world, or second tool. |
| P0-03 | Steam-first needs a current Windows clean-launch result. | Produce a sanitized Windows build smoke result or record the exact missing-module blocker. | Build/QA lane only; no gameplay changes. |
| P0-04 | Store capture cannot use ambiguous placeholder presentation. | Add a no-placeholder capture gate for current visible HUD, VFX, audio, and primitive art status. | Validator/report first; asset import only after provenance is clear. |
| P0-05 | Save/restart proof is not accepted on `main`. | Add focused local progress proof for the current playable loop. | Local save only; no cloud or platform service. |
| P0-06 | Product canon on `main` conflicts with current direction. | Land current compact action-adventure source docs. | Documentation lane only; do not change runtime scope. |
| P1-01 | Hub + three regions + four bosses are not accepted runtime evidence. | Build one hub-to-region transition proof after Gate A readability/build gaps are closed. | One hub, three regions, four bosses maximum. |
| P1-02 | Boss readability has no accepted first boss pattern proof. | Bind one boss/miniboss pattern with the existing telegraph language. | One boss lane at a time; no broad combat rewrite. |
| P1-03 | Store-facing art/audio readiness is incomplete. | Accept or replace one visible hero/tool/enemy/reward element with rights notes and preview evidence. | Asset-preview first; gameplay binding only after readiness. |

## Active Top Gap

This PR addresses P0-02. The accepted room should not end as a static
`Room complete` state; it should point the player toward the next room/region
beat without adding a new system. The smallest player-visible fix is a
Meshwright-guided, repository-authored low-poly route beacon that appears after
the reward is claimed.

## PR Operating Rule

Each follow-up PR should pick the highest remaining player-visible gap above,
then make one narrow runtime, scene, art, audio, or QA improvement. If a new
system appears necessary, choose a smaller expression inside the accepted MVP
instead of widening scope.
