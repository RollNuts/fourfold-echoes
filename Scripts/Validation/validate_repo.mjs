#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const repo = process.cwd();

const requiredFiles = [
  "AGENTS.md",
  "docs/Product/MVP_BLUEPRINT.md",
  "docs/Product/PROJECT_SPEC.md",
  "docs/Product/DEVELOPMENT_CHARTER.md",
  "docs/Product/SCOPE_BOUNDARIES.md",
  "docs/Product/CORE_SYSTEMS.md",
  "docs/Product/VERTICAL_SLICE_CONTENT.md",
  "docs/Product/INTENT_AUDIT.md",
  "docs/Product/FINAL_PRODUCT_TABLE.md",
  "docs/Product/RISKS_AND_MITIGATIONS.md",
  "docs/Product/REPO_TIMELINE_AUDIT.md",
  "docs/Product/MARKET_TARGET.md",
  "docs/Product/CANONICAL_PRODUCT_SPEC.md",
  "docs/Product/STEAM_ONE_PAGER.md",
  "docs/Product/KILL_LIST.md",
  "docs/Art/ART_BIBLE.md",
  "docs/Art/COMPACT_ACTION_ART_DIRECTION.md",
  "docs/Art/ASSET_REGISTER.csv",
  "docs/Art/AI_GENERATION_QUEUE.md",
  "docs/Art/LICENSES.md",
  "docs/Art/ASSET_PIPELINE.md",
  "docs/Audio/AUDIO_BIBLE.md",
  "docs/Audio/COMPACT_ACTION_AUDIO_DIRECTION.md",
  "docs/Audio/ASSET_REGISTER.csv",
  "docs/Audio/SFX_REGISTER.csv",
  "docs/Audio/MUSIC_BRIEF.md",
  "docs/Audio/AUDIO_PIPELINE.md",
  "docs/Production/VERTICAL_SLICE_PLAN.md",
  "docs/Production/SCOPE_CONTROL.md",
  "docs/Production/VERIPSA_LANES.md",
  "docs/Production/REQUIRED_CHECKS.md",
  "docs/Tech/TECHNICAL_ARCHITECTURE.md",
  "docs/Tech/OPEN_WORLD_SYSTEMS.md",
  "docs/Tech/PHASE_STATE_SYSTEM.md",
  "docs/Tech/ASSET_PIPELINE.md",
  "docs/Tech/PS5_READINESS_CHECKLIST.md",
  "docs/Tech/PERFORMANCE_BUDGET.md",
  "docs/Tech/PLATFORM_NOTES.md",
  "docs/Release/STEAM_READINESS.md",
  "docs/Release/DEMO_SCOPE.md",
  "docs/Release/SUPPORT.md",
  "docs/Marketing/STEAM_STORE_PLAN.md",
  "docs/QA/VALIDATION_REPORT.md",
  "docs/QA/MANUAL_QA_CHECKLIST.md",
  "docs/QA/STEAM_RELEASE_PLAN.md",
  "docs/Legal/LICENSES.md",
  "docs/Legal/AI_USAGE_POLICY.md",
  "game-spec/project.yaml",
  "game-spec/scenes/d020-vertical-slice.yaml",
  "game-spec/entities/d020-vertical-slice.yaml",
  "game-spec/scenarios/d020-tool-room-read.yaml",
  "commands/samples/inspect-d020-slice.json",
  "commands/samples/build-d020-slice.json",
  "commands/samples/capture-d020-slice.json",
  "Scripts/Validation/write_veripsa_split_report.mjs",
  "Scripts/Validation/check_public_repo_hygiene.mjs",
  "tools/queue_unity_editor_command.sh"
];

const secretLikePatterns = [
  /API_KEY\s*=\s*['"]?[A-Za-z0-9_\-]{12,}/,
  /BEGIN (RSA |OPENSSH )?PRIVATE KEY/,
  /ghp_[A-Za-z0-9_]{20,}/,
  /github_pat_[A-Za-z0-9_]{20,}/
];

const rejectedLegacyFiles = [
  "game-spec/entities/gate-a.yaml",
  "game-spec/scenarios/gate-a-clear-room.yaml",
  "game-spec/scenes/ashen-threshold.yaml",
  "commands/samples/run-room-spike.json",
  "game-spec/entities/product-review.yaml",
  "game-spec/scenarios/product-review-visual-read.yaml",
  "game-spec/scenes/product-reset-sandbox.yaml",
  "commands/samples/inspect-product-reset.json"
];

const errors = [];
for (const file of requiredFiles) {
  if (!fs.existsSync(path.join(repo, file))) {
    errors.push(`Missing required product reset file: ${file}`);
  }
}

for (const file of rejectedLegacyFiles) {
  if (fs.existsSync(path.join(repo, file))) {
    errors.push(`Rejected legacy Gate A Game IR file still exists: ${file}`);
  }
}

const canonicalTextChecks = [
  {
    file: "AGENTS.md",
    required: ["single-player top-down classic", "1 exploration tool", "not an open world"]
  },
  {
    file: "docs/Product/MVP_BLUEPRINT.md",
    required: ["1 hub", "3 regions", "4 bosses", "1 exploration tool", "## 新規プロジェクトのフォルダ構成"]
  },
  {
    file: "game-spec/project.yaml",
    required: ["scene.d020_vertical_slice", "one_exploration_tool_mastery", "open_world", "multiple_exploration_tools"]
  },
  {
    file: "game-spec/scenes/d020-vertical-slice.yaml",
    required: ["scene.d020_vertical_slice", "canonical: true", "D020VerticalSlice.unity"]
  }
];

for (const check of canonicalTextChecks) {
  const fullPath = path.join(repo, check.file);
  if (!fs.existsSync(fullPath)) continue;
  const text = fs.readFileSync(fullPath, "utf8");
  for (const required of check.required) {
    if (!text.includes(required)) {
      errors.push(`Canonical text check failed: ${check.file} missing "${required}"`);
    }
  }
}

const requiredSectionChecks = [
  {
    file: "docs/Product/MVP_BLUEPRINT.md",
    headings: [
      "## 仕様固定メモ",
      "## 新規プロジェクトのフォルダ構成",
      "## シーン一覧",
      "## スクリプト責務一覧",
      "## データ設計",
      "## 実装順",
      "## スコープ外一覧"
    ]
  },
  {
    file: "docs/Product/CORE_SYSTEMS.md",
    headings: ["## 必須", "## 不要", "## 後回し"]
  },
  {
    file: "docs/Art/COMPACT_ACTION_ART_DIRECTION.md",
    headings: [
      "## アートピラー",
      "## 禁止事項",
      "## 予算表",
      "## 命名規則",
      "## 地域別ルック表",
      "## 最低品質基準",
      "## 制作フロー"
    ]
  },
  {
    file: "docs/Audio/COMPACT_ACTION_AUDIO_DIRECTION.md",
    headings: [
      "## オーディオピラー",
      "## 必須SE一覧",
      "## BGM一覧",
      "## 探索ツール音設計",
      "## 実装優先順位",
      "## マイルストーン完成条件"
    ]
  },
  {
    file: "docs/Production/VERTICAL_SLICE_PLAN.md",
    headings: [
      "## 完成条件チェックリスト",
      "## 実装順序",
      "## 担当表",
      "## リスク表",
      "## 市場検証可能性メモ"
    ]
  },
  {
    file: "docs/Production/SCOPE_CONTROL.md",
    headings: [
      "## 変更管理テンプレート",
      "## 上限定義",
      "## 却下基準",
      "## 週次レビュー表",
      "## 3分類表"
    ]
  },
  {
    file: "docs/QA/STEAM_RELEASE_PLAN.md",
    headings: [
      "## Steam発売前チェックリスト",
      "## Steam Deckテスト表",
      "## 回帰テスト表",
      "## コンソール事前対応表",
      "## 30日運用計画",
      "## ユーザー告知テンプレート",
      "## バグ優先度表"
    ]
  },
  {
    file: "docs/Marketing/STEAM_STORE_PLAN.md",
    headings: [
      "## 短文紹介3案",
      "## 長文紹介",
      "## タグ優先順",
      "## スクリーンショット計画",
      "## トレーラー絵コンテ",
      "## 告知文",
      "## 翻訳優先リスト"
    ]
  }
];

for (const check of requiredSectionChecks) {
  const fullPath = path.join(repo, check.file);
  if (!fs.existsSync(fullPath)) continue;
  const text = fs.readFileSync(fullPath, "utf8");
  for (const heading of check.headings) {
    if (!text.includes(heading)) {
      errors.push(`Required objective section missing: ${check.file} needs "${heading}"`);
    }
  }
}

const staleScopeChecks = [
  "README.md",
  "docs/Tech/TECHNICAL_ARCHITECTURE.md",
  "docs/Product/MVP_BLUEPRINT.md",
  "docs/Product/CORE_SYSTEMS.md",
  "docs/Product/PROJECT_SPEC.md",
  "docs/Product/CANONICAL_PRODUCT_SPEC.md",
  "docs/Production/VERTICAL_SLICE_PLAN.md",
  "docs/Production/SCOPE_CONTROL.md",
  "docs/Marketing/STEAM_STORE_PLAN.md",
  "docs/Art/COMPACT_ACTION_ART_DIRECTION.md",
  "docs/Audio/COMPACT_ACTION_AUDIO_DIRECTION.md",
  "docs/QA/VALIDATION_REPORT.md",
  "docs/QA/STEAM_RELEASE_PLAN.md",
  "game-spec/project.yaml",
  "game-spec/scenes/d020-vertical-slice.yaml",
  "game-spec/entities/d020-vertical-slice.yaml",
  "game-spec/scenarios/d020-tool-room-read.yaml",
  "tools/forge/check.mjs",
  "docs/Art/ASSET_PIPELINE.md",
  "docs/Art/PROJECT_ART_AUDIT.md",
  "docs/Art/ASSET_REGISTER.csv",
  "docs/Audio/ASSET_REGISTER.csv",
  "docs/Audio/SFX_REGISTER.csv",
  "docs/Audio/AUDIO_PIPELINE.md",
  "docs/Legal/LICENSES.md",
  "docs/ASSET_RIGHTS.md",
  "tools/Blender/README.md",
  "tools/Blender/generate_pilot_assets.py",
  "tools/AssetPipeline/README.md"
];

const forbiddenCurrentScopePhrases = [
  "compact 3D open-world",
  "open-world action adventure",
  "Echo Phase world changes",
  "canonical after D-019",
  "at least two Echo Phases",
  "scene.product_reset_sandbox",
  "phase_comparison",
  "four phase states",
  "Boss Rush",
  "Tools/Blender",
  "Tools/AssetPipeline",
  "inspect-product-reset",
  "phase_variant",
  "boss phase readability",
  "Echo Phase markers",
  "Docs/Art",
  "Docs/Audio",
  "Docs/Legal",
  "Artifacts/Previews",
  "AI_USE.md"
];

for (const file of staleScopeChecks) {
  const fullPath = path.join(repo, file);
  if (!fs.existsSync(fullPath)) continue;
  const text = fs.readFileSync(fullPath, "utf8");
  for (const phrase of forbiddenCurrentScopePhrases) {
    if (text.includes(phrase)) {
      errors.push(`Stale D-019/current-scope phrase in ${file}: "${phrase}"`);
    }
  }
}

const marketingForbiddenClaims = [
  "Open World",
  "MMO",
  "Survival",
  "Crafting",
  "Souls-like",
  "Co-op",
  "Multiplayer",
  "Live Service",
  "Social",
  "Inventory",
  "Loot",
  "Quest",
  "Open-Ended"
];

{
  const file = "docs/Marketing/STEAM_STORE_PLAN.md";
  const fullPath = path.join(repo, file);
  if (fs.existsSync(fullPath)) {
    const text = fs.readFileSync(fullPath, "utf8");
    for (const phrase of marketingForbiddenClaims) {
      if (!text.includes(phrase)) {
        errors.push(`Marketing hard-exclude list must include "${phrase}" in ${file}`);
      }
    }
  }
}

const requiredD020SfxIds = [
  "audio.sfx.ui_select",
  "audio.sfx.ui_confirm",
  "audio.sfx.ui_back",
  "audio.sfx.ui_error",
  "audio.sfx.ui_pause",
  "audio.sfx.footstep_common",
  "audio.sfx.player_dodge",
  "audio.sfx.player_landing",
  "audio.sfx.normal_swing",
  "audio.sfx.enemy_hit_confirm",
  "audio.sfx.shield_armor_hit",
  "audio.sfx.player_damage",
  "audio.sfx.enemy_death",
  "audio.sfx.enemy_notice",
  "audio.sfx.enemy_tell",
  "audio.sfx.enemy_attack",
  "audio.sfx.enemy_damage",
  "audio.sfx.boss_intro_hit",
  "audio.sfx.boss_tell",
  "audio.sfx.boss_impact",
  "audio.sfx.boss_transition",
  "audio.sfx.boss_defeat",
  "audio.sfx.tool_equip_ready",
  "audio.sfx.tool_pulse",
  "audio.sfx.tool_near_response",
  "audio.sfx.tool_target_hit",
  "audio.sfx.tool_fail",
  "audio.sfx.tool_cooldown_ready",
  "audio.sfx.pedestal_wake",
  "audio.sfx.mechanism_move",
  "audio.sfx.shortcut_open",
  "audio.sfx.chest_open",
  "audio.sfx.relic_appear",
  "audio.sfx.pickup",
  "audio.sfx.discovery_stinger",
  "audio.sfx.system_save",
  "audio.sfx.system_load_continue",
  "audio.sfx.settings_apply"
];

const requiredD020MusicIds = [
  "audio.music.bgm_hub",
  "audio.music.bgm_region01",
  "audio.music.bgm_region02",
  "audio.music.bgm_region03",
  "audio.music.bgm_normal_combat",
  "audio.music.bgm_boss"
];

validateAudioRegisterCoverage({
  file: "docs/Audio/ASSET_REGISTER.csv",
  requiredIds: [...requiredD020SfxIds, ...requiredD020MusicIds],
  label: "audio asset register"
});

validateAudioRegisterCoverage({
  file: "docs/Audio/SFX_REGISTER.csv",
  requiredIds: requiredD020SfxIds,
  label: "SFX register"
});

for (const file of walk(repo)) {
  if (file.includes(`${path.sep}.git${path.sep}`)) continue;
  if (file.includes(`${path.sep}Library${path.sep}`)) continue;
  if (file.includes(`${path.sep}Temp${path.sep}`)) continue;
  if (file.includes(`${path.sep}Build${path.sep}`)) continue;
  if (!isTextCandidate(file)) continue;
  const text = fs.readFileSync(file, "utf8");
  for (const pattern of secretLikePatterns) {
    if (pattern.test(text)) {
      errors.push(`Forbidden secret-like pattern ${pattern} in ${path.relative(repo, file)}`);
    }
  }
}

if (errors.length > 0) {
  console.error(errors.map((line) => `- ${line}`).join("\n"));
  process.exit(1);
}

console.log(`FOURFOLD repo validation passed: ${requiredFiles.length} required reset files present.`);

function* walk(dir) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      yield* walk(full);
    } else if (entry.isFile()) {
      yield full;
    }
  }
}

function isTextCandidate(file) {
  return /\.(md|txt|json|yaml|yml|cs|mjs|js|sh|csv|asset|meta|gitignore|gitattributes)$/i.test(file);
}

function validateAudioRegisterCoverage({ file, requiredIds, label }) {
  const fullPath = path.join(repo, file);
  if (!fs.existsSync(fullPath)) return;
  const rows = parseCsv(fs.readFileSync(fullPath, "utf8"));
  const seen = new Map();

  for (const row of rows) {
    const id = row.asset_id;
    if (!id) {
      errors.push(`${label} row is missing asset_id in ${file}`);
      continue;
    }
    if (seen.has(id)) {
      errors.push(`${label} has duplicate asset_id "${id}" in ${file}`);
    }
    seen.set(id, row);
  }

  for (const id of requiredIds) {
    const row = seen.get(id);
    if (!row) {
      errors.push(`${label} missing required D-020 cue: ${id}`);
      continue;
    }
    if (row.acceptance_status === "historical_only") {
      errors.push(`${label} marks required D-020 cue as historical_only: ${id}`);
    }
    if (row.acceptance_status === "not_started" && row.status !== "needed") {
      errors.push(`${label} not_started D-020 cue must use status=needed: ${id}`);
    }
  }
}

function parseCsv(text) {
  const rows = [];
  let row = [];
  let field = "";
  let inQuotes = false;

  for (let i = 0; i < text.length; i += 1) {
    const char = text[i];
    const next = text[i + 1];

    if (inQuotes) {
      if (char === '"' && next === '"') {
        field += '"';
        i += 1;
      } else if (char === '"') {
        inQuotes = false;
      } else {
        field += char;
      }
      continue;
    }

    if (char === '"') {
      inQuotes = true;
    } else if (char === ",") {
      row.push(field);
      field = "";
    } else if (char === "\n") {
      row.push(field);
      rows.push(row);
      row = [];
      field = "";
    } else if (char !== "\r") {
      field += char;
    }
  }

  if (field.length > 0 || row.length > 0) {
    row.push(field);
    rows.push(row);
  }

  const [headers, ...dataRows] = rows;
  if (!headers) return [];

  return dataRows
    .filter((dataRow) => dataRow.some((value) => value.trim().length > 0))
    .map((dataRow) => Object.fromEntries(headers.map((header, index) => [header, dataRow[index] ?? ""])));
}
