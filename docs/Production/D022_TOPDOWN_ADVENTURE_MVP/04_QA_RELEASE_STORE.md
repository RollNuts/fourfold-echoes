# D022 QA Release And Store Plan

Official references:

- [Steam Store Assets](https://partner.steamgames.com/doc/store/assets)
- [Steam Release Process](https://partner.steamgames.com/doc/store/review_process)
- [Steam Trailers](https://partner.steamgames.com/doc/store/trailer)
- [Steam Deck Compatibility](https://partner.steamgames.com/doc/steamhardware/compat)

## Steam発売前チェックリスト

| Area | Pass Condition |
| --- | --- |
| Build | Windows build launches through Steam test path with version/SHA visible |
| Store | copy describes implemented features only |
| Screenshots | gameplay screenshots, 16:9, at least 1920x1080 source |
| Trailer | first 10 sec show actual gameplay and tool loop |
| Input | keyboard/mouse and controller can finish the slice |
| Save | new, continue, boss clear, death retry, corrupt fallback verified |
| Audio | no missing clips, no placeholder SFX in public capture |
| Legal | licenses, AI usage, credits, commercial rights recorded |
| Support | log path, contact, known issues, hotfix template ready |

## Steam Deckテスト表

| Test | Pass Condition |
| --- | --- |
| 1280x800 | HUD, boss HP, prompts, settings, result readable |
| Controller-only | title to clear to quit works without keyboard/mouse |
| Glyphs | controller prompts do not show keyboard-only instructions |
| Performance | 30-minute route holds target frame rate without audio drop |
| Suspend/resume | input, audio, save, and scene state survive resume |
| Offline | game starts, saves, and exits without network |
| Proton/Deck | Windows build launches with no fatal error |

## 回帰テスト表

| Area | Cases |
| --- | --- |
| Save | new, continue, death, boss clear, corrupt, version migration |
| Tool | valid target, invalid target, solved target, cooldown, reload |
| Combat | enemy 1, enemy 2, miniboss, boss, simultaneous death, 10 retries |
| UI | title, pause, settings, language, save feedback, boss HP |
| Input | keyboard, Xbox pad, DualSense, Deck, disconnect/reconnect |
| Resolution | 1280x800, 1920x1080, windowed, fullscreen |
| Audio | BGM 2, SFX categories, volume persistence, no missing clip |

## コンソール事前対応表

| Risk | Preemptive Action |
| --- | --- |
| Platform input differences | keep input abstraction and controller-first UI |
| Save APIs | isolate `SaveService` from direct platform calls |
| Achievements | emit internal progress events before Steam-specific APIs |
| UI readability | maintain 10-foot readability and 16:9/16:10 layouts |
| Performance | record frame, memory, load, and audio budgets |
| Marketing promise | do not promise console release until platform access exists |

## 30日運用計画

| Window | Work |
| --- | --- |
| Day 0-3 | P0 only: startup failure, save corruption, progression blocker, frequent crash |
| Day 4-7 | P1: controller/Deck blockers, boss retry, input lock, severe localization |
| Day 8-14 | stability patch, FAQ, known issues update, high P2 |
| Day 15-30 | small fixes, crash pattern review, no feature creep |

## ユーザー告知テンプレート

```text
FOURFOLD ECHOES is being built around one hub, one exploration tool, readable
rooms, boss openings, reward beats, and local save progress. Public footage and
store text will show only playable features.
```

```text
Known issue: [summary]
Workaround: [steps]
Priority: [P0-P3]
Next update: [build/date]
```

## バグ優先度表

| Priority | Definition |
| --- | --- |
| P0 | startup failure, save corruption, progression blocker, reproducible crash |
| P1 | controller-only failure, boss/tool softlock, major UI or audio blocker |
| P2 | readability issue, missing SFX, localization layout issue, rare crash |
| P3 | typo, minor visual issue, rare non-blocking animation/VFX mismatch |

## 短文紹介3案

1. JP: ひとつの探索ツールで道を開き、敵の予兆を読み、ボスの隙を作る見下ろし型アクションアドベンチャー。
   EN: Open paths, read enemy tells, and create boss openings with one exploration tool.
   ZH-Hans: 使用一个探索工具开辟道路、读懂敌人预兆，并制造首领破绽。

2. JP: 小さなハブから手作りの探索地へ向かい、近道、レリック、ボス攻略を味わう。
   EN: Leave a small hub, clear handcrafted rooms, earn relics, open a shortcut, and defeat a boss.
   ZH-Hans: 从小型据点出发，探索手工区域，取得遗物，开启捷径并击败首领。

3. JP: 広さではなく密度で勝負する、買い切りシングルプレイのコンパクトな冒険。
   EN: A compact buy-to-play single-player adventure built for density, not sprawl.
   ZH-Hans: 一款买断制单人紧凑冒险，重视密度而非规模。

## 長文紹介

FOURFOLD ECHOES is a compact buy-to-play single-player top-down
action-adventure for Steam. Start from a small hub, enter handcrafted regions,
use one exploration tool to solve rooms and open shortcuts, earn small reward
beats, and defeat readable enemies and bosses. The focus is density, clarity,
and mastery, not online systems, crafting, or open-world scale.

## タグ優先順

1. Action-Adventure
2. Singleplayer
3. Top-Down
4. Adventure
5. Action
6. Exploration
7. Fantasy
8. Controller
9. Puzzle
10. Stylized
11. Atmospheric
12. Indie

Avoid: Open World, Crafting, Survival, Co-op, Multiplayer, MMO, Loot, Live
Service, Souls-like.

## スクリーンショット計画

| # | Shot | Requirement |
| --- | --- | --- |
| 1 | hub and player | product UI, no graybox |
| 2 | exploration tool aim/use | target and effect clear |
| 3 | gimmick room before | problem readable |
| 4 | gimmick room after | route change readable |
| 5 | normal enemy tell | dodge lane clear |
| 6 | miniboss | larger threat readable |
| 7 | boss opening | tool creates opening |
| 8 | relic/return | reward and progress clear |

## トレーラー絵コンテ

### 45 seconds

| Time | Shot |
| ---: | --- |
| 0-5 | hub, player, gate, title hook |
| 5-10 | tool opens first route |
| 10-16 | enemy tell, dodge, basic attack |
| 16-22 | gimmick before/after |
| 22-28 | shortcut open and relic reward |
| 28-36 | miniboss pressure and boss intro |
| 36-42 | boss opening with tool |
| 42-45 | title, Steam wishlist |

### 75 seconds

| Time | Shot |
| ---: | --- |
| 0-8 | hub and compact adventure promise |
| 8-18 | tool language: ready, pulse, success, fail |
| 18-28 | two room transformations |
| 28-38 | enemy 1 and enemy 2 tells |
| 38-48 | shortcut and reward beat |
| 48-60 | miniboss and boss pattern |
| 60-70 | boss opening, defeat, return |
| 70-75 | title, demo/wishlist CTA |

## 告知文

Demo note:

```text
FOURFOLD ECHOES starts with a dense playable slice: one hub, one exploration
tool, two gimmick rooms, readable enemies, a miniboss, a boss, reward relics,
and local save/load. We will show only features that are actually playable.
```

Launch note:

```text
FOURFOLD ECHOES is a compact single-player top-down action-adventure about
mastering one exploration tool, opening routes, reading enemies, and defeating
bosses across handcrafted regions.
```

## 翻訳優先リスト

| Priority | Language | Scope |
| --- | --- | --- |
| 1 | Japanese | UI, store, support, announcements |
| 2 | English | Steam baseline, trailer, press, support |
| 3 | Simplified Chinese | store short/long copy, FAQ, key UI |
| 4 | Traditional Chinese | after demand is proven |

Priority glossary:

| JP | EN | ZH-Hans |
| --- | --- | --- |
| 探索ツール | exploration tool | 探索工具 |
| 近道 | shortcut | 捷径 |
| レリック | relic | 遗物 |
| 敵の予兆 | enemy tell | 敌人预兆 |
| ボスの隙 | boss opening | 首领破绽 |
