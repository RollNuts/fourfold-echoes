# Skeleton Template Animation Plan

Status: active rig and animation planning note.

## Decision

Once skeleton templates are approved, enemy and character production becomes:

1. scale the template
2. swap or add parts
3. assign material/biome variants
4. reuse a small animation set
5. override only the attacks that need unique timing

This only works if every skeleton template defines joint landmarks, sockets,
movement type, collider profile, and animation clips before final art.

## What The Template Must Lock

Each skeleton template must define:

- front direction
- ground pivot
- height bands: small, medium, elite, boss
- main collider shape
- hitbox source points
- weak-point socket
- head socket
- left/right arm or limb sockets
- back socket
- weapon/tool socket when applicable
- VFX sockets: cast, hit, death, weak break
- movement mode
- minimum animation clip set

After that, most variants are size and part changes.

## Size Bands

| Band | Use | Scale Multiplier | Notes |
|---|---|---:|---|
| S | fodder, swarm, weak support | 0.75-0.90 | fast reads, low HP, small collider |
| M | normal enemy | 1.00 | baseline template |
| L | elite, blocker, charger | 1.20-1.45 | larger tells, stronger hit reactions |
| XL | miniboss | 1.70-2.30 | unique attacks, shared base locomotion only |
| Boss | arena boss | 2.50+ | separate phase sockets and camera framing |

## Movement Modes

| Mode | Templates | Animation Priority |
|---|---|---|
| Walk biped | small/heavy biped, golem | idle, walk, turn, attack, hit, death |
| Run/charge quadruped | quadruped, wyvern ground, rolling shell | idle, trot, charge windup, charge, skid, hit, death |
| Hover | floating caster, support caster | idle hover, drift, cast, recoil, death dissolve |
| Fly | winged flyer, dragon/wyvern air | idle flap, fly, dive, land, hit air, death fall |
| Crawl | insect, crab, plant root crawl | idle, crawl, lunge, burrow/brace, hit, death |
| Slither | serpent | idle coil, slither, strike, line attack, segment hit, death |
| Rooted | plant turret, trap, boss anchor | idle, aim, cast/spit, exposed, break |

## Shared Clip Minimums

Every non-boss enemy template needs:

- `idle_loop`
- `move_loop`
- `turn_in_place`
- `attack_a`
- `attack_b` or `special`
- `hit_light`
- `hit_heavy`
- `stagger`
- `death`
- `spawn`

Boss and miniboss templates add:

- `intro`
- `phase_shift`
- `telegraph_a`
- `telegraph_b`
- `weak_break`
- `rage_idle`
- `death_long`

## Socket Naming

Use these names consistently:

```text
SOCKET_Head
SOCKET_ChestCore
SOCKET_Back
SOCKET_WeakPoint
SOCKET_Mouth
SOCKET_AttackOrigin
SOCKET_LeftHand
SOCKET_RightHand
SOCKET_LeftWing
SOCKET_RightWing
SOCKET_Tail
SOCKET_Ground
SOCKET_Cast
SOCKET_HitVfx
```

Templates may omit sockets that do not apply, but names must not change.

## Variant Rule

Variant models should avoid changing the skeleton unless the gameplay job
requires it. Prefer:

- scale change
- head swap
- horn/ear/jaw swap
- wing swap
- tail swap
- shell/armor overlay
- weak-core color/shape swap
- weapon/tool socket swap
- material palette swap

Do not create a new skeleton just because the surface theme changes.

## Dragon/Wyvern Note

Dragon-like enemies need at least two templates:

- small wyvern: quadruped/wing hybrid, usable as elite or miniboss
- boss dragon: separate boss multi-anchor skeleton with phase sockets

A dragon is not just a scaled-up quadruped. It needs wing sockets, tail attack
origin, mouth attack origin, chest weak core, and ground/air movement modes.
