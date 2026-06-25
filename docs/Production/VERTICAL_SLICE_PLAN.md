# Vertical Slice Plan

Status: canonical after D-020.

## 完成条件チェックリスト

- [ ] Hub 1 exists and is visually styled.
- [ ] Exploration area 1 exists and is visually styled.
- [ ] Normal enemies 2 types are playable.
- [ ] Miniboss 1 is playable.
- [ ] Boss 1 is playable.
- [ ] Exploration tool 1 is modeled, animated, audible, and usable.
- [ ] Shortcut 1 opens and is saved.
- [ ] Gimmick rooms 2 use the same exploration tool in different ways.
- [ ] Relic rewards 2 are awarded without adding inventory complexity.
- [ ] Minimal UI exists: health, tool state, prompt, boss health, pause/settings.
- [ ] BGM 2 tracks are integrated.
- [ ] Minimum SFX set is integrated.
- [ ] Save/load works for progress flags.
- [ ] 30 minutes or less communicates the core fun.
- [ ] 30 seconds of silent footage communicates the game.
- [ ] No gray-box art remains in market-validation captures.
- [ ] No placeholder SFX remains in audio-validation captures.
- [ ] Optimization and bug table are updated.

## 実装順序

| Week | Goal | Deliverable |
| --- | --- | --- |
| 1 | Product reset and playable base | canonical docs, scene list, movement/camera/attack/dodge prototype |
| 2 | Exploration tool proof | `ExplorationTool`, `ExplorationNode`, one visual/audible route reaction |
| 3 | Room flow | two gimmick rooms, one shortcut, reward trigger, progress flags |
| 4 | Combat slice | two enemies, damage/death/retry, readable tells, combat SFX |
| 5 | Miniboss and boss | miniboss, boss with readable attacks, boss BGM, retry flow |
| 6 | Art pass | hub/region/enemy/tool/chest/gimmick art reaches style guide minimum |
| 7 | Audio/UI/save pass | two BGM tracks, required SFX, minimal UI, save/load regression |
| 8 | Market validation | Windows build, Steam Deck-oriented checks, screenshots, 45s trailer capture plan |

## 担当表

| Category | Responsibility |
| --- | --- |
| Lead Programmer | movement, camera, combat, room flow, save/load, build |
| Game Designer | one-tool rules, room layouts, boss pattern readability |
| Art Director | style guide, hero/tool/enemy/room readability, screenshot quality |
| Technical Artist | asset budgets, materials, VFX, Unity import validation |
| Audio Director | BGM, SFX, mix priority, no-placeholder gate |
| QA/Release | regression, Deck checks, save corruption, crash triage |
| Store Marketing | screenshot list, trailer storyboard, store copy honesty |

## リスク表

| Risk | Severity | Mitigation |
| --- | --- | --- |
| Tool is not fun enough | High | build two rooms around the same tool before adding anything else |
| Game reads as generic | High | lock one clear tool silhouette and regional color script early |
| Combat is too shallow | High | boss/miniboss must test movement, dodge, and tool timing |
| Art remains blockout | High | blockout cannot pass vertical-slice acceptance |
| Audio is delayed | High | tool/combat SFX required before room proof is accepted |
| Scope creeps | High | reject added systems; trade only content or polish tasks within D-020 caps |
| Steam Deck issues | Medium | design UI for 1280x800 and controller from week 1 |
| Save corruption | Medium | versioned save data and roundtrip tests before market capture |

## 市場検証可能性メモ

This slice can test market pull because it produces the same evidence a Steam
buyer sees first:

- 8 screenshots that show the hero, tool, room, enemy, boss, reward, shortcut,
  and region look.
- 45 seconds of trailer footage that explains the loop without narration.
- 20-30 minutes of hands-on play to judge whether one-tool mastery has depth.
- BGM and SFX present, so feel is not falsely postponed.

Market validation fails if players cannot describe the game as "top-down action
adventure built around one exploration tool" after watching footage.
