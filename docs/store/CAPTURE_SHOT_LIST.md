# Screenshot And Trailer Shot List

The goal is to make the prototype presentable without hiding that it is still a
prototype. Capture should show real player-facing progress, not empty rooms,
debug-only proof, or promises outside the build.

## Screenshot Set

Target six screenshots. A PR can attach rough captures; a Steam-facing candidate
needs final resolution, stable framing, and no debug-only text.

| Shot | Purpose | Must show | Reject if |
| --- | --- | --- | --- |
| Room read | First impression of the fixed-angle room | Player, enemy, altar, gate, and navigable floor in one frame | The room looks empty, flat, or like an editor grid |
| Phase identity | Color and silhouette difference between phases | Player in a non-default phase near phase markers or combat feedback | The phase difference is only a HUD label |
| Enemy tell | Fair threat readability | Enemy windup ring/lane, player position, dodge option | The tell is hidden behind effects or unreadable at thumbnail size |
| Counterattack | Combat payoff | Attack arc, hit burst, enemy reaction, phase color | The player and enemy silhouettes merge |
| Altar heat | Objective state change | Player near altar, visible heat/prompt, gate still closed | It reads as holding a button at a random prop |
| Gate claim | Room completion | Open/ready gate, claim badge, reward event | The finish looks like a generic door opening |

## Prototype Trailer Beat Sheet

Keep an internal prototype trailer to 35-50 seconds. Do not cut it like a launch
trailer. The point is to prove that the room has a sellable player loop.

1. 0-4s: fixed camera on the full room with the player already in motion.
2. 4-9s: phase switch while moving toward danger.
3. 9-16s: enemy tell appears, player dodges through or away from the strike.
4. 16-23s: counterattack lands, enemy reacts, chain or phase feedback is visible.
5. 23-31s: altar heat builds and changes the room state.
6. 31-40s: gate opens, enemy is cleared or already down, claim becomes visible.
7. 40-50s: reward claim closes the loop, then cut before lingering on rough edges.

## Capture Standards

- Capture live gameplay when possible. Staged editor camera moves are not a
  substitute for readable player input.
- Keep the fixed-angle composition. Do not sell a camera behavior the game does
  not have.
- Show UI only when it helps prove player readability. Debug wording, raw event
  logs, and tool-like instructions should be removed or minimized before any
  public-facing asset.
- Capture at moments of action. Idle character lineup shots do not sell the loop.
- The thumbnail test matters: player, threat, objective, and payoff should still
  read when the image is small.
- Do not crop out the gate or altar when the shot is meant to prove objective
  clarity.
