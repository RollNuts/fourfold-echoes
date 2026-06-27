# FOURFOLD ECHOES MVP Acceptance Criteria

Status: MVP acceptance spec, 2026-06-27.

This document defines how the MVP in `docs/mvp-spec.md` is accepted. A feature
is not accepted because it exists in a scene once; it is accepted when it works
with controller-first input, saves or resets correctly, is readable from the
gameplay camera, has required feedback, and has evidence captured from the
current Unity project.

## Evidence Rule

Every accepted item must have a repo-local evidence path. Acceptable evidence
includes a Unity validation report, a manual QA note, a screenshot or capture
manifest, a committed scene or prefab path, a PlayMode/EditMode test result
when tests exist, and a short implementation note that identifies the owning
scene and component. Debug-only IMGUI, prototype-only generated primitives, or
old Gate A behavior may inform implementation, but they do not by themselves
accept the MVP.

The requested `reports/repo-audit.md` is absent from this checkout at the time
of writing. Acceptance therefore references the repo-local audit substitutes
listed in `docs/mvp-spec.md` until the requested report path exists.

## Product Scope Acceptance

| Spec area | Pass condition |
| --- | --- |
| Product promise | A tester can describe the build after 30 seconds of silent footage as a compact top-down action-adventure built around one exploration tool. If testers describe it as open world, loot, co-op, crafting, puzzle sandbox, or generic arena combat, the MVP fails product clarity. |
| Scope ceiling | The build contains no more than one hub, three regions, four bosses, and one exploration tool. It contains no inventory, crafting, quest log, social, multiplayer, backend, open-world, day/night, fishing, farming, base-building, live-service, or gacha behavior. |
| Package constraint | `Packages/manifest.json` does not gain new runtime package dependencies for MVP gameplay without a recorded product/technical decision. Built-in Unity modules already present may be used. |
| Current asset leverage | The first accepted vertical slice uses existing production prefabs and current generated/pilot assets before requesting new large imports. Any new large reusable asset group is routed to the asset-library repository process, not silently piled into this game repo. |

## Core Loop Acceptance

The core loop passes when a tester can start in the hub, enter a region, clear
at least three authored rooms, solve or activate one exploration-tool node,
open a shortcut, claim a reward, defeat a boss, return to safety, quit, reload,
and find the correct progress flags still applied. This must work without a
quest log or inventory screen.

For the vertical-slice milestone, the same loop must be proven in one Region 01
segment with two normal enemy reads, two tool-based room uses, one shortcut,
two relic rewards, one miniboss or miniboss-equivalent escalation, and one
boss. For the full MVP, the loop must repeat across three regions and end in a
final boss clear.

## Session Flow Acceptance

| Flow step | Pass condition | Evidence |
| --- | --- | --- |
| Title start | New Game, Continue, Settings, and Quit are reachable with controller and keyboard. Continue is disabled or safe when no save exists. | QA note and screenshot at 1280x800 and 1920x1080. |
| New game | A clean save is created and the player spawns at the hub return point with the exploration tool available only if the current design starts with it. | Save roundtrip note with save version and initial flags. |
| Hub entry | The hub contains a visible return point, save point, and region gate layout. It has no active enemy pressure and no quest/inventory UI. | Hub screenshot and scene/prefab references. |
| Region entry | Entering a region loads or activates a bounded authored route with camera, HUD, movement, combat, and tool input working. | Runtime capture and validation report. |
| Boss clear | Defeating a boss sets the correct flag, opens the correct exit or return, awards or reveals the intended reward, and unlocks the next gate when applicable. | Boss progression QA note and save reload check. |
| Quit and reload | Quit to title and app restart do not corrupt progress. Reloaded state places the player in a valid hub, checkpoint, or safe region state. | Save roundtrip and corruption fallback test notes. |

## Player Action Acceptance

| Action | Pass condition |
| --- | --- |
| Movement | The player can complete the slice using keyboard and controller movement. Movement feels responsive, clamps or collides correctly within room bounds, and never drifts through walls, gates, or hazards during normal play. |
| Facing | Attacks and tool use have an understandable facing or targeting rule. The player can intentionally hit the nearest visible enemy or node without needing debug text. |
| Normal attack | Attack has startup/read, active effect, recovery, hit confirm, whiff feedback, and cooldown. It damages valid enemies and does not damage locked or dead targets. |
| Dodge | Dodge moves in the intended direction, has visible recovery, prevents repeated spamming through cooldown, and can avoid at least one normal enemy and one boss attack when timed correctly. |
| Exploration tool | Tool pulse, valid hit, fail/no-target, cooldown, and solved-state visuals are all visible and audible. The tool cannot activate already solved nodes unless the room explicitly supports repeatable nodes. |
| Interact | Interact works for save stones, region gates, reward chests, and exits. Prompts appear only when valid and never require a mouse. |
| Pause | Pause reliably freezes gameplay state, focuses the pause menu, resumes cleanly, and can return to title without leaving input stuck. |

## Exploration Tool Acceptance

The tool passes MVP acceptance only when it is both a mechanic and a visual
identity. A valid node must have an idle read, an activation response, a solved
read, and a saved state when permanence is intended. A no-target pulse must
communicate failure without implying the tool is broken. The player must solve
two different rooms using the same input and same tool logic; the difference
must come from room layout, enemy pressure, target placement, or response
consequence rather than from a second ability.

| Node response | Pass condition |
| --- | --- |
| Reveal | A hidden or inactive route, bridge, reward object, or readable marker becomes visible and usable after pulse. The before/after state is obvious in a screenshot. |
| Activate | A gate, relay, pedestal, lock, or mechanism changes state after pulse and stays changed long enough for the player to act. Permanent activations are saved. |
| Expose | An enemy, boss, lock, or hazard exposes a vulnerable/readable state. The state uses the same node/tool language and does not create a new combat subsystem. |

## Room, Enemy, And Boss Acceptance

| Content | Pass condition |
| --- | --- |
| Room controller | Room entry, enemy activation, gate lock/unlock, reward spawn, shortcut open, and room-complete events are deterministic and reload into valid states. A room does not depend on global quest logic. |
| Normal melee enemy | The enemy approaches, telegraphs, strikes, recovers, takes damage, dies, and counts toward room clear. A tester can identify the tell before the hit. |
| Normal ranged enemy | The enemy produces a visible line or area threat, gives time to respond, applies damage only when the threat resolves, and reuses common health/death/room clear behavior. A full projectile system is not required for acceptance. |
| Miniboss | The miniboss uses larger health, clearer tells, and at least one cadence change while reusing normal combat primitives. It does not introduce new UI or a separate combat rule set. |
| Region boss | A boss has intro lock, boss health UI, two or three readable attacks, one tool-related opening or exposed state, defeat event, reward/exit event, and saved defeat flag. |
| Final boss | The final boss tests movement, dodge, attack timing, and tool recognition from previous regions. It clears the final flag and reaches a stable end state. |
| Obstacles | Gates, bridges, hazards, relays, locks, and chests change state visibly. A state change cannot leave the room impossible to complete after reload. |

## UI Acceptance

| UI surface | Pass condition |
| --- | --- |
| HUD | Health, tool state, context prompt, save indicator, and boss health when active are readable at 1280x800 and 1920x1080. HUD elements do not cover enemy tells, tool targets, or interaction objects. |
| Title menu | Menu navigation works without mouse, uses clear focus states, and has no marketing claims that exceed implemented content. |
| Pause/settings | Pause, resume, settings, quit to title, volume changes, and settings apply feedback work with controller. Settings are shallow but functional. |
| Prompts | Prompts are action-based and concise. They do not explain missing gameplay clarity and do not mention systems outside MVP. |
| Store capture mode | Debug labels, prototype HUD text, editor-only indicators, and placeholder markers can be hidden for screenshot/trailer capture. |

## Art And Audio Acceptance

Art passes when every market-validation capture uses production-intent
stylized 3D assets, top-down readable silhouettes, clear regional identity, and
no gray-box or primitive-only stand-ins for the hero, tool, enemies, boss,
shortcut, or reward. Existing generated production prefabs can be used for
internal gameplay acceptance, but public/store capture requires human art/IP
review and approval because `generated-asset-consistency` currently blocks
production approval until that review.

Audio passes when required gameplay cues fire from gameplay events rather than
only existing as files. The vertical slice requires tool pulse, target hit,
fail, shortcut/mechanism, normal swing, enemy hit confirm, dodge, player
damage, enemy tell, enemy death, reward/discovery, UI select/confirm/back/error
or equivalent basics, save cue, boss tell/impact/transition/defeat, one
hub/exploration BGM loop, and one boss/combat BGM loop. Generated pilot SFX can
prove timing, but market validation requires accepted non-placeholder cues.

## Save And Failure Acceptance

| Case | Pass condition |
| --- | --- |
| Player death | Health reaching zero triggers a quick retry from the last valid checkpoint or room start. Already saved boss, shortcut, region, and relic flags are not lost. |
| Room reset | Retrying a room resets active enemies and temporary hazards while preserving permanent solved nodes and claimed rewards when those were saved. |
| Save roundtrip | Boss defeated, shortcut opened, region unlocked, relic claimed, tool owned, current scene or hub spawn, and settings reload correctly after app restart. |
| Save corruption | Missing, old-version, or truncated save data fails safely into New Game or a documented fallback without crashing. |
| Soft-lock prevention | A solved node, opened gate, defeated boss, claimed reward, or unlocked region cannot reload into a physically blocked or impossible state. |

## Technical Acceptance

The MVP build must launch from a clean Windows install target and remain
controller-first. Steam Deck is a compatibility target, so 1280x800 readability
and controller-only completion are required before public demo or store
capture. Built-in Render Pipeline remains accepted unless a separate measured
migration decision exists. LOD warnings from the current validation are
acceptable for the reset baseline only; MVP outdoor landmarks and repeated
props need either LOD coverage or a documented performance exception with a
frame-time sample.

The product validator must report no missing scripts, no missing material
slots, no negative-scale issues that break rendering/collisions, and no
unexpected networking or backend dependency. Any warning that affects
store-facing performance, visual quality, save reliability, input, or
progression must be triaged before acceptance.

## Nice-To-Have Acceptance Gate

Nice-to-have work is accepted into the product only after the vertical slice
passes the core loop, art, audio, save, and controller checks above. A
nice-to-have proposal must identify the MVP problem it solves, the footage or
play value it improves, the implementation and QA cost, and the existing task
or content it removes. If it adds a second tool, inventory, quest log,
open-world behavior, online service, complex dynamic music, or unrelated genre
loop, it fails the gate by default.
