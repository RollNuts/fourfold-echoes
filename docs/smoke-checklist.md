# FOURFOLD ECHOES Smoke Checklist

Last updated: 2026-06-27

Run this before claiming a Unity slice is playable or suitable for evidence
capture. Use a clean editor play session. Do not rely on screenshots from scene
view.

## Setup

- [ ] Confirm Unity editor version is `6000.3.18f1`.
- [ ] Open `Assets/Scenes/ProductionCombatSlice.unity`.
- [ ] Confirm no compile errors are visible.
- [ ] Confirm Game view uses an actual runtime camera, not Scene view.
- [ ] Confirm editor console has no new errors after pressing Play.

## Production Combat Slice

- [ ] Player, two normal enemies, boss, gate, exploration node, and reward chest are visible from the top-down camera.
- [ ] Keyboard movement keeps the player inside the arena bounds.
- [ ] Basic attack hits nearby enemies and has readable feedback.
- [ ] Enemy contact/damage is understandable; the player can tell why damage happened.
- [ ] The boss starts locked while normal enemies and shortcut are unresolved.
- [ ] Using the exploration tool near the node reveals the shortcut route.
- [ ] The gate/reward state does not open before combat and shortcut conditions are met.
- [ ] Reward pad/chest are visible once the route is ready.
- [ ] Reset returns player, enemies, boss, shortcut, gate, and reward to their initial state.

## D-020 Tool Room

- [ ] Open `Assets/Scenes/D020VerticalSlice.unity`.
- [ ] Player, one exploration tool node, shortcut route location, enemies, and relic chest read clearly in one view.
- [ ] Using the exploration tool near the node visibly opens/reveals the shortcut.
- [ ] The same tool interaction is understandable without explanatory debug text.
- [ ] The room still communicates the compact one-tool adventure pitch in a 30-second silent capture.

## Input And Feel

- [ ] Keyboard fallback covers movement, attack, reset, and interaction/tool use.
- [ ] Controller pass is attempted if a controller is available.
- [ ] No control depends on a hidden editor-only shortcut.
- [ ] No action requires waiting through unclear recovery.
- [ ] Camera does not hide player, enemy, reward, route, or danger.

## Visual And Audio

- [ ] No pink/missing materials.
- [ ] No missing scripts in the loaded scene.
- [ ] No obvious gray-box object is being treated as final market evidence.
- [ ] Major player actions have SFX or clearly documented prototype audio.
- [ ] Reward/discovery feedback is audible and visible.
- [ ] Audio loops do not click or stack obviously after reset.

## Evidence

- [ ] Capture at least one screenshot showing player, enemy, tool target, route, reward, and boss/gate context.
- [ ] Capture one short clip from actual Play mode if the milestone claims runtime evidence.
- [ ] Record any failed checklist item with scene name, repro steps, expected result, and observed result.
