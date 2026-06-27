# Commercial Progress: Save Progress Flags

Date: 2026-06-27

## 1. Final Goal Support

This pass reduces commercial save-risk for the active production slice. The MVP
requires local save/load, shortcut state, boss completion, and relic reward
flags without inventory or quest systems. Focused tests now pin the monotonic
relationship between those flags for `ProductionCombatSlice`.

## 2. Systems Touched

- Save progress mapping: `ProductionCombatSliceProgress`
- EditMode QA: production slice save flag tests
- Commercial planning: `reports/commercial-gap-map.md`

No runtime gameplay system, online dependency, inventory, crafting, quest log,
or social feature was added.

## 3. Files Added/Changed

- Added: `Assets/Tests/EditMode/ProductionCombatSliceProgressTests.cs`
- Added: `Assets/Tests/EditMode/ProductionCombatSliceProgressTests.cs.meta`
- Added: `reports/commercial-gap-map.md`
- Added: `reports/commercial-progress-save-progress-2026-06-27.md`

Existing unrelated dirty worktree files were preserved.

## 4. Implementation

- Added tests that prove a claimed reward implies both boss defeated and
  shortcut open.
- Added tests that prove a defeated boss implies shortcut open.
- Added tests that prove `Write` records the production slice scene id and
  writes monotonic shortcut, boss, and reward flags.
- Added a null-data guard test for `Read` and `Write`.
- Updated the commercial gap map so save progress conversion is no longer listed
  as untested; live scene save/load remains the next gap.

## 5. Tests

Static compile check:

```sh
dotnet build <temporary-save-progress-check-project>/FourfoldSaveProgressCheck.csproj --no-restore
```

Result:

- Passed.
- 0 warnings.
- 0 errors.

Unity Test Runner was not executed in this pass because this checkout is already
open in Unity, which previously blocked batchmode test execution. The new test
source compiles against Unity 6.3.18f1 runtime assemblies plus the Unity NUnit
package.

## 6. Acceptance Conditions

Accepted for this pass when:

- `ProductionCombatSliceProgress.Read` treats reward as a completed boss and
  shortcut.
- `ProductionCombatSliceProgress.Read` treats boss completion as an opened
  shortcut.
- `ProductionCombatSliceProgress.Write` stores scene, shortcut, boss, and reward
  flags without introducing inventory-like state.
- The new test source compiles.

## 7. Next Smallest Useful Task

Run EditMode and PlayMode tests through Unity, or close the active editor and run
batchmode. If Unity remains blocked, add deterministic public progression hooks
to `ProductionCombatSliceController` so the room loop can be tested without
simulating Legacy Input.
