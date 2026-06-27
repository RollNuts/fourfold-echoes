# Folded Reliquary Style Bible

Status: canonical product-line contract for production-intent 3D models.

## 商品ライン

**Folded Reliquary Miniatures / 折り目遺物の箱庭模型**

FOURFOLD ECHOES の3Dモデルは、ファンタジー世界の寄せ集めではなく、
同じ工房で作られた「折られた遺物模型」の商品ラインとして扱う。地域、
敵、報酬、道具、床、境界、ボスは別ジャンルではない。R01の根、R02の
炉、R03の水晶、ボスの王冠は、素材の摩耗状態や機能の差を示す言葉で
あり、画面の第一印象を別ブランドへ変えるための言葉ではない。

このラインの狙いは、現在売られているプレミアムインディー/JRPG系の
スクリーンショットと並べたときに、密度、明暗、彩度、シルエット、
接地、画面構成で見劣りしないこと。ただし外部作品のキャラクター、
敵、宝箱、UI、構図、配色セット、固有モチーフ、商標的な形状は参照しない。
外部作品は品質計測の物差しであって、デザインの材料ではない。

ユーザーの嗜好は「親しみやすい立体感、高彩度で濁らない色面、
奥行きのある暖色キー/寒色影、読みやすい丸み、装飾の階層化」として
吸収する。固有作品名や「〜風」は、プロンプト、manifest、仕様、
アセット名、コミットメッセージに入れない。

## 1文ジャンル定義

トップダウン/高め三人称で読む、低い折り石板・四分割象嵌・厚い欠け
タブ・一本の機能シグナル糸で統一された、商用安全で親しみやすい
箱庭遺物アクションアドベンチャー。

## 視覚スタイルロック

| Item | Rule |
| --- | --- |
| Camera read | Gameplay is top-down/high three-quarter first. Third-person beauty shots are secondary and must not change the model grammar. |
| Proportion | Heroic small-scale forms: compact torso, readable hands/tools, clear feet, large functional silhouettes. |
| Shape language | Rounded slab, trapezoid, droplet, socket, and folded tab are primary. Sharp spikes are reserved for elite danger and bosses. |
| Color | Saturated but not neon. Bright body colors need cool shadow support; black is reserved for deliberate hostile grounding. |
| Lighting | Warm key, cool ambient/fill, and restrained rim light. Flat gray Workbench captures are internal only. |
| Material | Stylized PBR intent: broad color planes, large bevel reads, sparse normal detail, no photoreal grime or scan noise. |
| Ornament density | Put detail on face/front, shoulder/socket, tool/weapon tips, reward/weak points. Keep legs and central mass cleaner. |
| VFX language | Arcs, ribbons, short signal lines, split rings, and outline glows. Smoke and sparks remain symbolic, not realistic. |
| Unity delivery | FBX-first, bottom-center pivots, atlas-friendly materials, clear LOD/readability budgets. |

## 絶対ルール

| Rule | Gate |
| --- | --- |
| Concept first | 重要な3Dモデルは、承認済みコンセプト絵とモデリングブリーフなしに制作しない。 |
| One product line | 全モデルが `folded_reliquary` の形状ファミリーに属する。地域語が主役にならない。 |
| Folded body first | 主形状は低い折り板、台座、割れた塊で作る。球、棒、円柱は主役にしない。 |
| Fourfold read | 四分割、ずれた45度前後の象嵌、欠けた縁のどれかを上面視で読ませる。 |
| Functional signal | 発光色は「道具、報酬、危険、ロック、ルート」の機能だけに使う。飾り発光は禁止。 |
| Regional restraint | 地域差は素材比率、摩耗、角の丸さ、シグナル色で出す。別ゲーム風の道具立てにしない。 |
| Chunk over noise | 細い蔓、針、ワイヤ、小粒の散布ではなく、厚みのある欠け、爪、段差で密度を作る。 |
| Top-down proof | 1280x800のゲームカメラで前後、接地、用途、危険/報酬が読めるまで完成扱いにしない。 |
| Market metric pressure | 外測のコントラスト、エッジ密度、彩度、明度を上げる。外部IPの形を借りて解決しない。 |

## モデルファミリー

| Family | Role | Required Motifs | Forbidden Drift |
| --- | --- | --- | --- |
| Toolbearer Relic | Hero and held/socketed tool | 完全形の折り板、胸/肩ソケット、前方向のシグナル糸 | MMO鎧、既存RPG主人公、顔/髪型での記号化 |
| Exploration Instrument | Central tool | 四分割の読み取り盤、欠けたレンズ枠、太い折り畳みヒンジ | 魔法杖、アンテナ、ただの球体コア |
| Broken Hostile Relic | Normal enemies | 壊れた折り塊、前方攻撃面、赤い危険亀裂 | 赤目の小型魔物、丸胴、手持ち棍棒 |
| Warden Relic | Bosses | 大型化した折り台座、四つの脅威アンカー、弱点ソケット | 既存ボスの輪郭、巨大なだけの小物、画面を隠す装飾 |
| Tool Receiver | Pedestals, relays, locks | 受け皿、入力スリット、アクティブ/解決の差分 | 別の道具に見える機械、文字依存の看板 |
| Route Surface | Floors, bridges, route edges | 低い板、欠けた縁、床の分割線 | 均一タイルの敷き詰め、ただの灰色床 |
| Low Boundary | Walls, gates, blockers | 低い折り壁、前後が読める切れ目 | 高壁、自然物だけの柵、カメラを塞ぐ柱 |
| Reward Reliquary | Chests and pickups | 小型遺物、蓋/受け皿、報酬シグナル | 汎用宝箱、既存RPG風の鍵/王冠/紋章 |
| Grounding Detail | Chips, marks, shadows | 低い象嵌、接地影、局所クラスタ | 単体の花畑、粒の散布、意味のない飾り |

## 地域差の扱い

| Area | Line Variant | What Changes | What Must Not Change |
| --- | --- | --- | --- |
| HUB | Polished Reliquary | ivory/warm gold/soft blue、清潔な折り縁、安全な低壁 | 折り台座、四分割、機能シグナル |
| R01 | Weathered Reliquary | moss/stone/tool signal、丸い欠け、低い成長タブ | 森セット、蔓ノイズ、独立した植物主役 |
| R02 | Scorched Reliquary | charcoal/rust/amber、曲がった厚タブ、熱で欠けた象嵌 | 工場セット、細い配管、歯車ノイズ |
| R03 | Cold Reliquary | dark/violet/crystal、鋭い欠け、冷たい分割インレイ | 水晶林、細い針、キラキラ散布 |
| BOSS | Broken Crown Reliquary | dark/gold/red、四方向危険面、弱点ソケット | 王冠そのもの、既存ラスボスの輪郭 |

## すぐ止めるもの

- 主要形状が球、円柱、棒だけで成立しているモデル。
- 地域名詞が主役になったモデル。花、根、パイプ、歯車、水晶、王冠は
  15%以下の補助記号に留める。
- 赤い目、丸い胴、棍棒、汎用宝箱、魔法杖、アンテナのように既視感で
  用途を説明するモデル。
- 均等にばら撒いた小物で密度を出す画面。密度は焦点の周囲に集める。
- 外部作品名を「雰囲気」「作風」「参考」として使うプロンプトや仕様。

## 合格条件

0. 重要な3Dモデルが `docs/Art/CONCEPT_FIRST_PIPELINE.md` の
   concept-first gate を通過している。
1. すべての生成モデルが manifest に `brand_line_id`,
   `product_line_role`, `required_shape_tokens`, `motif_limit_policy` を持つ。
2. Prompt contract が外部作品名を含まず、同じ商品ライン定義を含む。
3. Validator が地域差を「別ジャンル化」させる表現を警告または失敗にする。
4. Benchmark scene が `contrast >= 0.60 proximity`,
   `edge_density >= 0.60 proximity`, `saturation >= 0.55 proximity` に到達するまで
   Steam向け完成扱いにしない。
5. 人間レビューで「どの既存ゲームの何に似ているか」が説明できる形は却下する。
