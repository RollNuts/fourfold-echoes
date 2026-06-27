# D-020 Playable Room Milestone

Date: 2026-06-27

## Goal Support

This pass moves `D020VerticalSlice` from a static evidence scene toward a
controllable Region 01 test room. It directly supports the first milestone:
movement, fixed top-down camera, normal attack, dodge, one enemy, one
ExplorationTool response, one shortcut response, one reward, core SFX evidence,
and automated smoke validation.

## Systems Touched

- D-020 editor smoke validation.
- D-020 validation report artifacts.
- Repository hygiene guidance in `AGENTS.md`.

## Files Changed

- `AGENTS.md`
- `Assets/Editor/FourfoldD020PlayableSmoke.cs`
- `artifacts/Reports/d020-playable-room-milestone.md`
- `artifacts/Reports/unity-product-validation.json`
- `artifacts/Reports/unity-product-validation.md`

## Implementation

- Extended `FourfoldD020PlayableSmoke.Run` to validate the fixed orthographic
  camera, reward chest readability, imported core SFX assets, enemy defeat after
  repeated normal attacks, and the inactive-to-active shortcut response after
  one ExplorationTool activation.
- Replaced the smoke's active-only scene lookup with an inactive-inclusive lookup
  so hidden shortcut responses can be verified before activation.
- Sanitized the repository guidance file by removing a personal absolute path.

## Tests

- `git diff --check -- <changed exact paths>`
  - Result: passed.
- `node Scripts/Validation/validate_repo.mjs`
  - Result: passed. Required reset files present: 52.
- `node <hygiene-check-script>`
  - Result: passed. Scanned tracked/untracked files: 206.
- Sanitization scan over changed exact paths
  - Result: passed. No personal local path, credential assignment, private key,
    credentialed URL, or database URL pattern found.
- Unity scene validation
  - Method: `FourfoldEchoes.Editor.FourfoldD020SliceSceneBuilder.ValidateGeneratedScene`
  - Result: passed, exit code 0.
- Unity playable smoke
  - Method: `FourfoldEchoes.Editor.FourfoldD020PlayableSmoke.Run`
  - Result: passed, exit code 0.

## Warnings

- Unity emitted non-failing CoreBusinessMetrics SQLite cache warnings.
- Unity emitted a non-failing licensing token update warning during shutdown.
- No compile errors or smoke failures were present in the final Unity results.

## Acceptance Conditions Covered

- Player movement: covered by `D020PlayerController.Tick` movement assertion.
- Fixed top-down camera: covered by orthographic MainCamera assertion.
- Normal attack: covered by attack count and hit count assertions.
- Dodge: covered by dodge movement and count assertions.
- One enemy: covered by `D020EnemyDummy` presence and defeat assertion.
- One ExplorationTool response: covered by solved `ExplorationNode`.
- One shortcut response: covered by hidden-before-use and active-after-use
  response target assertions.
- One reward: covered by readable `D020 Relic Chest` assertion.
- Non-placeholder core SFX: covered by imported generated audio clip assertions
  for attack, dodge, hit, tool, shortcut, and relic pickup.
- Automated smoke: covered by Unity `-executeMethod` smoke.

## Remaining Risk

- This is still a compact room proof, not the full 20-30 minute vertical slice.
- Reward is validated as a readable chest/relic object, not yet as full saveable
  reward progression.
- Core SFX are repository-authored generated pilot clips and still need final
  audio direction approval before market capture.

## Next Smallest Useful Task

Add a tiny reward pickup interaction to D020 so the player can claim the relic
after enemy defeat and shortcut activation, then persist that reward state in the
local progress flags.
