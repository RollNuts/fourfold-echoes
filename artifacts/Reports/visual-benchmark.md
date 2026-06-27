# Visual Market Benchmark

This compares the generated FOURFOLD scene against official store screenshots. These sources are benchmarks only, not prompts or derivative style instructions.

## Sources

- [TUNIC](https://store.steampowered.com/app/553420/TUNIC/): aggregate readability and chunky-form finish metrics only
- [Death's Door](https://store.steampowered.com/app/894020/Deaths_Door/): aggregate combat-readability, lighting, and enemy-tell metrics only
- [OCTOPATH TRAVELER II](https://store.steampowered.com/app/1971650/OCTOPATH_TRAVELER_II/): aggregate premium-finish and screenshot-density metrics only
- [DRAGON QUEST XI S](https://store.steampowered.com/app/1295510/DRAGON_QUEST_XI_S_Echoes_of_an_Elusive_Age__Definitive_Edition/): aggregate warm-readability and adventure-finish metrics only

## Metric Result

- Our scene: `artifacts/Previews/ProductionModelPack/FE_BENCHMARK_R01_GameplayScene.png`
- Internal grammar board: `artifacts/Previews/ProductionModelPack/FE_BENCHMARK_FoldedReliquaryGrammar.png`
- Verdict: `near_market_metric_range`
- Overall metric proximity: `0.87`
- Production approval status: `candidate_needs_human_art_ip_review`
- Downloaded benchmark images: `12`

| Metric | Our Scene | Market Mean | Market Range | Proximity |
| --- | ---: | ---: | ---: | ---: |
| brightness | 94.85 | 94.77 | 31.13 - 143.78 | 1.0 |
| contrast | 26.31 | 40.85 | 23.33 - 53.9 | 0.64 |
| edge_density | 14.71 | 18.43 | 7.34 - 46.21 | 0.8 |
| palette_bins_64 | 64.0 | 64.0 | 64.0 - 64.0 | 1.0 |
| saturation | 108.07 | 121.79 | 66.52 - 185.78 | 0.89 |

## Review

- The generated assets now cover the required model inventory, but this first pass is still blockout-to-style, not final market art.
- The next art iteration should raise material treatment, lighting, prop layering, and scene composition before Steam-facing capture.
- Do not copy benchmark character, monster, logo, or franchise-specific shapes. Use the metrics to pressure quality, not to imitate protected designs.
