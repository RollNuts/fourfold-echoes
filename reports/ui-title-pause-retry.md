# UI Report: title-pause-retry

## 1. Final Goal Support

This change supports the compact Steam-first vertical slice by replacing the IMGUI-only combat status readout with a runtime UI flow that covers title start, in-game HUD, pause, player-down retry, and completion/title return without adding inventory, quest, social, or settings scope.

`{{UI_SCOPE}}` was not expanded in the request, so the implemented scope is derived from the required test route: Title -> Game -> Pause -> Retry.

## 2. Systems Touched

- Runtime gameplay state: `ProductionCombatSliceController`
- Runtime UI: new `ProductionCombatSliceUi`
- Input: existing Legacy Input only
- Scene/prefab assets: none directly edited
- Reports: this file

## 3. Files Added/Changed

- Changed: `Assets/Scripts/ProductionCombatSliceController.cs`
- Added: `Assets/Scripts/ProductionCombatSliceUi.cs`
- Added: `Assets/Scripts/ProductionCombatSliceUi.cs.meta`
- Added: `reports/ui-title-pause-retry.md`

## 4. Implementation

- Added `ProductionCombatRunState` with `Title`, `Playing`, `Paused`, `PlayerDown`, and `Completed`.
- Added controller methods for UI-safe flow control: `BeginRun`, `RetryRun`, `ReturnToTitle`, `SetPaused`, and `TogglePause`.
- Kept screen ownership in `ProductionCombatSliceUi`; gameplay state remains in `ProductionCombatSliceController`.
- Auto-attaches `ProductionCombatSliceUi` from the existing runtime hook so the current scene YAML does not need to be regenerated.
- Added a programmatic UI Toolkit `UIDocument` and runtime `PanelSettings`.
- Added HUD meters for hero health, wardens, boss, and tool readiness.
- Added overlay screens for title, pause, retry, and reward completion.
- Disabled exploration-tool input outside `Playing` so pause/title states do not leak gameplay activation.
- Gated the old IMGUI overlay behind `showDebugOverlay`, defaulting it off.

## 5. Required Checks

- UI framework: UI Toolkit. `com.unity.ugui` was not present in `Packages/manifest.json` or PackageCache; `com.unity.modules.uielements` is present.
- EventSystem: no existing `EventSystem`, `StandaloneInputModule`, or `InputSystemUIInputModule` references were found. This implementation does not add a uGUI EventSystem; UI buttons are UI Toolkit, while keyboard/pad menu activation is handled through Legacy Input polling.
- Input System package: not present. No `com.unity.inputsystem` dependency and no `UnityEngine.InputSystem` usage found.
- Existing input: Legacy Input via `UnityEngine.Input` is already used by movement, combat, tool use, and retry. The UI keeps that path.
- Resolution/aspect: runtime `PanelSettings` uses `ScaleWithScreenSize`, `1920x1080` reference resolution, and width-constrained panels. HUD/overlays use percent max widths and wrapped labels for 16:9 and Steam Deck-oriented 16:10 targets.
- Variable text/localization tolerance: labels use wrapping and flexible width constraints. There is no localization table yet; direct strings are listed below.
- Screen vs game state: UI screen selection maps from controller state, but UI state is not used as gameplay authority.

## 6. Hardcoded Text

- `FOURFOLD ECHOES`
- `Production slice ready`
- `Hero`
- `Wardens`
- `Boss`
- `Tool`
- `Tool ready`
- `Tool recovering`
- `Shortcut closed | Gate sealed | Reward waiting`
- `Production Combat Slice`
- `A compact room proof for combat, the exploration tool, the boss gate, and reward retry flow.`
- `Start Game`
- `Quit`
- `Paused`
- `The run is held without advancing combat or exploration tool input.`
- `Resume`
- `Retry`
- `Title`
- `Hero Down`
- `Restart the room from its initial state.`
- `Reward Claimed`
- `The slice route is complete.`
- Controller event strings: `Production slice ready`, `Defeat the two wardens, reveal the shortcut, then break the boss gate`, `Retry started`, `Paused`, `Resumed`, `Hero down - choose Retry`, `Reward claimed`, plus pre-existing combat/status event strings.

## 7. Tests

- Passed: local C# compile check against Unity 6.3.18f1 runtime assemblies using a temporary `netstandard2.1` project.
  - Command: `dotnet build <temporary-ui-check-project>/FourfoldUiCheck.csproj --no-restore`
  - Result: 0 warnings, 0 errors.
- Attempted: Unity batch validation via `FourfoldProductionCombatSliceSceneBuilder.ValidateGeneratedScene`.
  - Result: blocked because the project is already open in Unity PID 29218.
- Attempted: open-Unity inbox validation command.
  - Result: inbox did not process during this run; queued commands created for this check were removed to avoid surprise later execution.
- Not fully verified in-editor: mouse clicking UI Toolkit buttons, keyboard/pad navigation in Play Mode, and live NullReference/missing-reference console pass.

## 8. Acceptance Conditions

- Title overlay appears before gameplay starts.
- `Start Game` enters the playable combat slice.
- HUD remains readable during gameplay.
- Pause can be entered/exited with keyboard/pad pause input.
- Retry restarts the room after player down or from pause.
- Return to title resets the room without changing scenes.
- Exploration tool input is disabled while title/pause/retry/completion overlays are active.
- No new package migration to Input System occurs.

## 9. Next Smallest Useful Task

Run the open Unity editor in Play Mode for `Assets/Scenes/ProductionCombatSlice.unity`, then verify mouse click, keyboard, and pad paths through Title -> Game -> Pause -> Retry while watching the console for NullReference and missing reference errors.
