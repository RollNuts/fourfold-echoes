# Core Systems

Status: canonical after D-020. This is a scope-control document, not a wish list.

この文書は D-020 のシステム境界を固定する。分類は「必須」「不要」「後回し」の 3 つだけとし、ここにない新規システムを追加提案しない。
D-020 は Steam 先行、買い切り、シングルプレイ専用、見下ろし視点、コンパクト、オープンワールドなし、探索ツール 1 つ、MVP 上限 1 ハブ/3 地域/4 ボスである。

## 必須

| System | Purpose | MVP Acceptance | Scope Guard |
| --- | --- | --- |
| Top-down movement | 基本操作感 | キーボードとコントローラーで即応し、押し出しや段差で破綻しない | 乗り物、登攀、広域移動は含めない |
| Top-down camera | 読みやすさ | プレイヤー、敵、進行方向、探索ツール対象が同時に読める | シネマティック専用カメラ群を作らない |
| Normal attack | 戦闘の主行動 | ヒット確認、硬直、敵反応が明確 | コンボツリーや装備ビルドを作らない |
| Dodge | 生存行動 | 入力、無敵/回避距離、復帰タイミングが読める | スタミナ経済や成長要素を足さない |
| Basic damage/death/retry | 失敗ループ | 被弾理由が分かり、短時間で再挑戦できる | ローグライト進行や抽出ロストを作らない |
| One exploration tool | 商品上の核 | reveal、activate、expose の範囲で部屋、近道、報酬、ボス opening に使える | 2 種目のツール、複数アビリティ体系を作らない |
| Exploration nodes | ツール反応対象 | ツール入力に反応し、道、対象、報酬、弱点を画面上で読ませる | クエスト条件やインベントリ条件を持たせない |
| Room controller | 部屋進行 | 入室、戦闘/ギミック、報酬、近道解放を制御する | ワールド配信や手続き生成を作らない |
| Enemy AI | 戦闘圧 | スライスでは通常敵 2 種までを読みやすく動かす | 大量の派生敵やハクスラ群戦闘にしない |
| Miniboss and boss framework | スライス証明 | ミニボス 1、ボス 1 を予兆と撃破イベント込みで通す | 最大 4 ボスを超える前提を作らない |
| Shortcut unlock | 冒険構造 | 近道 1 つでハブまたは既存地点へ折り返す | オープンワールドのファストトラベル網を作らない |
| Relic reward | 報酬感 | スライスで 2 つの取得/効果を、専用インベントリなしで伝える | 装備経済、クラフト素材、収集図鑑にしない |
| Minimal HUD | 可読性 | HP、ツール状態、プロンプト、ボス HP、ポーズ/設定だけを表示 | ミニマップ拡張、クエストログ、ビルド画面を作らない |
| Local save/load | 製品成立 | ボス撃破、近道、報酬、リージョン解放、設定を復元する | Steam API 直結やクラウド前提にしない |
| BGM/SFX routing | 操作と市場検証の品質 | 操作、攻撃、被弾、ツール、報酬、ボス、UI の cue が鳴る | 仮音を完成扱いしない |

## 不要

| System | Reason |
| --- | --- |
| Open-world streaming | コンパクトな手作り部屋構造と矛盾する |
| Echo Phase/world-state switching | 1 ツール習熟ではなく複数状態管理が主題になる |
| Hack-and-slash loot/build systems | 通常攻撃、回避、読みやすい敵配置から焦点が外れる |
| Extraction loop | シングルプレイ買い切りの王道アクションアドベンチャーと異なる |
| Inventory | UI、経済、報酬設計が MVP を膨らませる |
| Crafting | 部屋解決と戦闘より素材管理が前に出る |
| Quest log | 依頼量と物語構造の拡大を前提にする |
| Social systems | シングルプレイ専用の範囲外 |
| Multiplayer/co-op | 設計、同期、QA の負荷が D-020 に合わない |
| Live service/backend | 買い切り、ローカル進行、Steam 先行と合わない |
| Farming/fishing/base building | 別ジャンルの継続ループになる |
| Procedural world generation | 手作りの部屋読みと近道設計を弱める |
| Additional exploration tools | 探索ツール 1 つの習熟を薄める |
| Large map/minimap expansion | オープンワールド期待と移動量増加を招く |

## 後回し

| System | Revisit Only After | Constraint |
| --- | --- |
| Settings depth | 基本操作、音量、画面表示がスライスで安定した後 | 設定画面を製品級メニューに肥大化させない |
| Localization pipeline | UI テキストと用語が固定された後 | 翻訳対象を増やすための新 UI を作らない |
| Steam achievements | Steam リリース範囲が固定された後 | 実績のために収集要素を増やさない |
| Advanced accessibility | 基本入力、HUD、音声 cue、敵予兆が安定した後 | 基礎可読性の不足を設定で隠さない |
| Additional enemy variants | 通常敵 2 種、ミニボス 1、ボス 1 が面白さを証明した後 | D-020 上限とスライス優先を超えない |
| Additional relics | スライスの 2 報酬が価値を証明した後 | インベントリや装備経済に発展させない |
| Extra boss polish | ボス 1 の予兆、音、VFX、撃破演出が通った後 | 4 ボス上限を超えない |
| Additional region dressing | 1 ハブと第 1 リージョンの市場検証品質が出た後 | 地域数を 3 超にしない |

## Implementation Boundary

Code should be clean enough to extend, but not abstract enough to hide the game.
Prefer direct, testable components for the vertical slice. Introduce shared
abstractions only when two concrete rooms or enemies already need the same
behavior.

Any item outside the 必須 table must not be implemented as a new system during
the MVP. 後回し items are scheduling notes, not permission to expand D-020.
