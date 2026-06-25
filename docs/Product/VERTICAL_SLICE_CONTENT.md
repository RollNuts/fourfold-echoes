# Vertical Slice Content

Status: canonical after D-020.

## Slice Goal

The vertical slice must answer one question:

> Can a compact top-down action-adventure built around one exploration tool sell
> itself through 30 seconds of gameplay and 30 minutes of play?

## 必須コンテンツ

| Category | Required |
| --- | --- |
| Hub | 1 small hub with return point and region entrance |
| Exploration area | 1 region segment |
| Normal enemies | 2 types |
| Miniboss | 1 |
| Boss | 1 |
| Exploration tool | 1 |
| Shortcut | 1 opened shortcut |
| Gimmick rooms | 2 |
| Relic rewards | 2 |
| UI | health, tool state, prompt, boss health, pause/settings basics |
| Music | 2 tracks minimum: hub/exploration and boss/combat |
| SFX | minimum readable set for combat, tool, UI, reward, damage |
| Save/load | local progress save and reload |
| Runtime evidence | screenshots and short capture from actual Unity play |

## Trailer Value Requirements

- A silent 30-second clip must show hero, enemy, exploration tool, room problem,
  solution, reward, and boss threat.
- No gray-box capture is allowed for market validation.
- No placeholder sound is allowed for audio validation.
- UI must support the footage, not explain missing game clarity.

## Acceptance Checklist

- Player movement is readable from top-down camera.
- Basic attack, dodge, damage, and enemy tells are legible.
- The exploration tool has a clear model, VFX, SFX, and target reaction.
- Two gimmick rooms use the same tool differently without adding a second tool.
- One shortcut visibly changes navigation.
- Two relic rewards feel meaningful without adding inventory complexity.
- Boss uses readable attacks and one tool-related opening.
- Local save/load restores shortcut, boss, and reward flags.
- Slice has BGM and SFX in place, not silent placeholders.
- At least 8 screenshot candidates can be captured from runtime.

## Must Not Do

- Do not add another exploration tool.
- Do not add open-world travel.
- Do not add inventory, crafting, quest log, or social UI.
- Do not add multiplayer or online dependencies.
- Do not hide weak play behind cinematic-only scenes.
