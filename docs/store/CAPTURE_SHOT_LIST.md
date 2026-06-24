# Screenshot And Trailer Shot List

The goal is to make the prototype presentable without hiding that it is still a
prototype. Capture should show real player-facing progress, not empty rooms,
debug-only proof, or promises outside the build.

Fun-first rule: if the clip looks slow, symbolic, or chore-like, do not use it
to argue store readiness. Fix the play first.

## Screenshot Set

Target six screenshots. A PR can attach rough captures; a Steam-facing candidate
needs final resolution, stable framing, and no debug-only text.

| Shot | Purpose | Must show | Reject if |
| --- | --- | --- | --- |
| Room read | First impression of the fixed-angle room | Player, enemy, altar, gate, and navigable floor in one frame | The room looks empty, flat, or like an editor grid |
| Movement to contact | Basic ARPG pace | Player crossing the room quickly into a decision | The route looks tedious or empty |
| Enemy pressure | Fair threat readability | Enemy windup/lane, player position, dodge option | The threat is hidden, slow, or not worth reacting to |
| Counterattack | Combat payoff | Attack arc, hit burst, enemy reaction, clear hit timing | The hit has no weight or the silhouettes merge |
| Simple objective | Room state change | Player triggers one clear objective while combat remains primary | It reads as puzzle-symbol decoding or holding a button at a random prop |
| Gate claim | Room completion | Open/ready gate, claim badge, reward event | The finish looks like a generic door opening |

## Prototype Trailer Beat Sheet

Keep an internal prototype trailer to 35-50 seconds. Do not cut it like a launch
trailer. The point is to prove that the room has a sellable player loop.

1. 0-4s: fixed camera on the full room with the player already in motion.
2. 4-9s: fast movement into enemy pressure.
3. 9-16s: enemy tell appears, player dodges through or away from the strike.
4. 16-23s: counterattack lands with visible impact and enemy reaction.
5. 23-31s: one simple room objective changes state without slowing the fight.
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
- Do not let symbols, UI labels, or glowing markers become the main subject.
- The thumbnail test matters: player, threat, objective, and payoff should still
  read when the image is small.
- Do not crop out the gate or altar when the shot is meant to prove objective
  clarity.
