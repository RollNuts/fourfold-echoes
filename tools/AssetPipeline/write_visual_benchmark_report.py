#!/usr/bin/env python3
"""Write a market visual benchmark report using official store screenshots."""

from __future__ import annotations

import json
import hashlib
import math
import statistics
import sys
import urllib.request
from datetime import datetime, timezone
from pathlib import Path

from PIL import Image, ImageChops, ImageEnhance, ImageFilter, ImageStat


REPO = Path(__file__).resolve().parents[2]
BENCHMARK_DIR = REPO / "artifacts" / "Previews" / "MarketBenchmarks"
REPORT_JSON = REPO / "artifacts" / "Reports" / "visual-benchmark.json"
REPORT_MD = REPO / "artifacts" / "Reports" / "visual-benchmark.md"
OUR_SCENE = REPO / "artifacts" / "Previews" / "ProductionModelPack" / "FE_BENCHMARK_R01_GameplayScene.png"
GRAMMAR_BOARD = REPO / "artifacts" / "Previews" / "ProductionModelPack" / "FE_BENCHMARK_FoldedReliquaryGrammar.png"
METRIC_THRESHOLDS = {
    "overall": 0.80,
    "contrast": 0.60,
    "edge_density": 0.60,
    "saturation": 0.55,
}
REFRESH_BENCHMARKS = "--refresh-benchmarks" in sys.argv


APPS = {
    "TUNIC": {
        "appid": 553420,
        "url": "https://store.steampowered.com/app/553420/TUNIC/",
        "role": "aggregate readability and chunky-form finish metrics only",
    },
    "Death's Door": {
        "appid": 894020,
        "url": "https://store.steampowered.com/app/894020/Deaths_Door/",
        "role": "aggregate combat-readability, lighting, and enemy-tell metrics only",
    },
    "OCTOPATH TRAVELER II": {
        "appid": 1971650,
        "url": "https://store.steampowered.com/app/1971650/OCTOPATH_TRAVELER_II/",
        "role": "aggregate premium-finish and screenshot-density metrics only",
    },
    "DRAGON QUEST XI S": {
        "appid": 1295510,
        "url": "https://store.steampowered.com/app/1295510/DRAGON_QUEST_XI_S_Echoes_of_an_Elusive_Age__Definitive_Edition/",
        "role": "aggregate warm-readability and adventure-finish metrics only",
    },
}


def main() -> None:
    BENCHMARK_DIR.mkdir(parents=True, exist_ok=True)
    REPORT_JSON.parent.mkdir(parents=True, exist_ok=True)
    benchmark_images = []

    for title, app in APPS.items():
        images = fetch_screenshots(app["appid"], title)
        benchmark_images.extend(images)

    our_metrics = measure_image(OUR_SCENE)
    benchmark_metrics = [measure_image(path) for _, path in benchmark_images]
    summary = summarize(benchmark_metrics)
    score = score_against_market(our_metrics, summary)

    report = {
        "version": 1,
        "note": "Official store screenshots are used as market benchmarks, not as style prompts or derivative-asset instructions.",
        "art_direction_id": "folded_reliquary",
        "art_direction_name": "Folded Reliquary Miniatures",
        "brand_line_id": "folded_reliquary_miniatures",
        "benchmark_policy_id": "external_market_metrics_only",
        "downloaded_at": datetime.now(timezone.utc).isoformat(),
        "metrics_only_policy_ack": True,
        "production_metric_thresholds": METRIC_THRESHOLDS,
        "comparison_scope": "aggregate market finish metrics only; no shape, palette, composition, character, prop, logo, or trade-dress extraction",
        "our_scene": rel(OUR_SCENE),
        "grammar_board": rel(GRAMMAR_BOARD) if GRAMMAR_BOARD.exists() else None,
        "our_metrics": our_metrics,
        "market_summary": summary,
        "market_sources": [
            {"title": title, "url": app["url"], "role": app["role"], "appid": app["appid"]}
            for title, app in APPS.items()
        ],
        "downloaded_benchmarks": [{"title": title, "path": rel(path), "sha256": sha256(path)} for title, path in benchmark_images],
        "score": score,
        "verdict": market_verdict(score),
        "production_approval_status": production_approval_status(score),
    }
    REPORT_JSON.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    REPORT_MD.write_text(markdown(report), encoding="utf-8")
    print(f"Wrote {rel(REPORT_JSON)}")
    print(f"Wrote {rel(REPORT_MD)}")


def fetch_screenshots(appid: int, title: str) -> list[tuple[str, Path]]:
    cached = [BENCHMARK_DIR / f"{appid}_{index}.jpg" for index in range(1, 4)]
    if not REFRESH_BENCHMARKS and all(path.exists() for path in cached):
        return [(title, path) for path in cached]

    url = f"https://store.steampowered.com/api/appdetails?appids={appid}&filters=basic,screenshots"
    with urllib.request.urlopen(url, timeout=20) as response:
        payload = json.load(response)
    data = payload[str(appid)]["data"]
    images = []
    for index, screenshot in enumerate(data.get("screenshots", [])[:3], start=1):
        image_url = screenshot.get("path_thumbnail") or screenshot.get("path_full")
        out = BENCHMARK_DIR / f"{appid}_{index}.jpg"
        urllib.request.urlretrieve(image_url, out)
        images.append((title, out))
    return images


def measure_image(path: Path) -> dict[str, float]:
    image = Image.open(path).convert("RGB").resize((600, 338))
    stat = ImageStat.Stat(image)
    gray = image.convert("L")
    edges = gray.filter(ImageFilter.FIND_EDGES)
    edge_stat = ImageStat.Stat(edges)
    colors = image.convert("P", palette=Image.Palette.ADAPTIVE, colors=64).getcolors()
    saturation = ImageEnhance.Color(image).enhance(1.0).convert("HSV").split()[1]
    sat_stat = ImageStat.Stat(saturation)
    contrast = ImageStat.Stat(ImageChops.difference(gray, Image.new("L", gray.size, int(stat.mean[0]))))
    return {
        "brightness": round(sum(stat.mean) / 3, 2),
        "contrast": round(contrast.mean[0], 2),
        "edge_density": round(edge_stat.mean[0], 2),
        "palette_bins_64": float(len(colors or [])),
        "saturation": round(sat_stat.mean[0], 2),
    }


def summarize(metrics: list[dict[str, float]]) -> dict[str, dict[str, float]]:
    keys = metrics[0].keys()
    return {
        key: {
            "mean": round(statistics.mean(item[key] for item in metrics), 2),
            "low": round(min(item[key] for item in metrics), 2),
            "high": round(max(item[key] for item in metrics), 2),
        }
        for key in keys
    }


def score_against_market(ours: dict[str, float], market: dict[str, dict[str, float]]) -> dict[str, float]:
    score = {}
    for key, value in ours.items():
        target = market[key]["mean"]
        if math.isclose(target, 0):
            score[key] = 1.0
        else:
            score[key] = round(max(0.0, min(value / target, target / value if value else 0.0)), 2)
    score["overall"] = round(statistics.mean(score.values()), 2)
    return score


def market_verdict(score: dict[str, float]) -> str:
    if score["overall"] >= 0.85:
        return "near_market_metric_range"
    if score["overall"] >= 0.70:
        return "readable_but_below_market_finish"
    return "below_market_finish"


def production_approval_status(score: dict[str, float]) -> str:
    if score["overall"] < METRIC_THRESHOLDS["overall"]:
        return "blocked_metric_overall"
    for key in ("contrast", "edge_density", "saturation"):
        if score[key] < METRIC_THRESHOLDS[key]:
            return f"blocked_metric_{key}"
    return "candidate_needs_human_art_ip_review"


def markdown(report: dict) -> str:
    lines = [
        "# Visual Market Benchmark",
        "",
        "This compares the generated FOURFOLD scene against official store screenshots. These sources are benchmarks only, not prompts or derivative style instructions.",
        "",
        "## Sources",
        "",
    ]
    for source in report["market_sources"]:
        lines.append(f"- [{source['title']}]({source['url']}): {source['role']}")
    lines.extend([
        "",
        "## Metric Result",
        "",
        f"- Our scene: `{report['our_scene']}`",
        f"- Internal grammar board: `{report['grammar_board']}`",
        f"- Verdict: `{report['verdict']}`",
        f"- Overall metric proximity: `{report['score']['overall']}`",
        f"- Production approval status: `{report['production_approval_status']}`",
        f"- Downloaded benchmark images: `{len(report['downloaded_benchmarks'])}`",
        "",
        "| Metric | Our Scene | Market Mean | Market Range | Proximity |",
        "| --- | ---: | ---: | ---: | ---: |",
    ])
    for key, value in report["our_metrics"].items():
        market = report["market_summary"][key]
        lines.append(f"| {key} | {value} | {market['mean']} | {market['low']} - {market['high']} | {report['score'][key]} |")
    lines.extend([
        "",
        "## Review",
        "",
        "- The generated assets now cover the required model inventory, but this first pass is still blockout-to-style, not final market art.",
        "- The next art iteration should raise material treatment, lighting, prop layering, and scene composition before Steam-facing capture.",
        "- Do not copy benchmark character, monster, logo, or franchise-specific shapes. Use the metrics to pressure quality, not to imitate protected designs.",
    ])
    return "\n".join(lines) + "\n"


def rel(path: Path) -> str:
    return path.resolve().relative_to(REPO).as_posix()


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


if __name__ == "__main__":
    main()
