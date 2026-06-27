using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldProductValidator
    {
        private const string ReportJsonPath = "artifacts/Reports/unity-product-validation.json";
        private const string ReportMarkdownPath = "artifacts/Reports/unity-product-validation.md";

        public static void RunAll()
        {
            Directory.CreateDirectory("artifacts/Reports");

            var findings = new List<Finding>();
            var metrics = new Metrics();

            try
            {
                FourfoldD022ProductContractVerifier.VerifyD022Contract();
                findings.Add(Finding.Info("d022.contract", "D022 product contract validated: current top-down adventure MVP pack is present, AGENTS points to it, UI/UX layouts fit 1280x800/1080p, and stale player-facing copy is blocked."));
            }
            catch (Exception exception)
            {
                findings.Add(Finding.Error("d022.contract", "D022 product contract validation failed: " + exception.Message));
            }

            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                findings.Add(Finding.Info("render.pipeline", "Built-in render pipeline is active."));
            }
            else
            {
                findings.Add(Finding.Warn("render.pipeline", "A Scriptable Render Pipeline asset is active; docs currently assume Built-in until migration is proven."));
            }

            try
            {
                FourfoldSaveVerifier.VerifySaveRoundtripAndRecovery();
                findings.Add(Finding.Info("save.service", "Versioned local save validated with settings defaults, language preference, UI scale/control-hint preferences, settings preservation across reset, roundtrip persistence, backup recovery, and corrupt-save fallback."));
            }
            catch (Exception exception)
            {
                findings.Add(Finding.Error("save.service", "Versioned local save validation failed: " + exception.Message));
            }

            try
            {
                FourfoldD020SliceSceneBuilder.BuildAndValidate();
                FourfoldD020GameplayVerifier.VerifyExistingSceneCombatDefeatPath();
                FourfoldD020GameplayVerifier.VerifyExistingSceneDeathRetryAndTitlePath();
                FourfoldD020GameplayVerifier.VerifyExistingSceneFullProgressionLoop();
                FourfoldD020GameplayVerifier.VerifyExistingSceneFailureLoop();
                findings.Add(Finding.Info("r01.verdant_steps", "R01 Verdant Steps evidence path generated and validated with one exploration tool, sealed-route and shortcut interactions, two normal enemy types, elite guard, boss, boss tool-opening attack window, combat feedback text, basic-attack enemy defeat, enemy-hit failure, failure result/retry/hub-return UX, title return, shared pause/settings/language UX, objective marker, progression rail, dodge state HUD, reward-effect notice UX, confirmation before abandoning unsaved rewards, two distinct saved reward skills, Lumen Link combined-skill recovery, return gate, required SFX, two BGM clips, and hub-return reward persistence."));
            }
            catch (Exception exception)
            {
                findings.Add(Finding.Error("r01.verdant_steps", "R01 Verdant Steps scene generation, validation, combat, death/retry, title-return, or full-loop verification failed: " + exception.Message));
            }

            try
            {
                FourfoldProductionP3ModelPackVerifier.VerifyP3ModelPack();
                findings.Add(Finding.Info("art.production_p3", "Production P3 model pack imported and validated with 28 prefabs, renderer/mesh/material references, and sane bounds."));
            }
            catch (Exception exception)
            {
                findings.Add(Finding.Error("art.production_p3", "Production P3 model pack validation failed: " + exception.Message));
            }

            try
            {
                FourfoldHubSceneBuilder.BuildAndValidate();
                FourfoldHubGameplayVerifier.VerifyHubEnterRegionProgress();
                findings.Add(Finding.Info("hub.crossroads", "Hub Crossroads generated and validated as the playable hub with an R01 region gate, mission briefing/start confirmation, reward-skill synergy and loss-risk briefing, returned-run summary/replay UX, objective marker, progress initialization, pause/settings/language UX, reset confirmation, and return-to-title persistence."));
            }
            catch (Exception exception)
            {
                findings.Add(Finding.Error("hub.crossroads", "Hub Crossroads generation, validation, or entry verification failed: " + exception.Message));
            }

            try
            {
                FourfoldTitleSceneBuilder.BuildAndValidate();
                FourfoldTitleGameplayVerifier.VerifyTitleEntryFlow();
                findings.Add(Finding.Info("title.entry", "Title scene generated and validated with New Game overwrite confirmation, Continue resume-or-hub choice for in-progress runs, Settings volume/language persistence, Quit request, and Build Settings order Title -> HubCrossroads -> R01."));
            }
            catch (Exception exception)
            {
                findings.Add(Finding.Error("title.entry", "Title scene generation, validation, or entry-flow verification failed: " + exception.Message));
            }

            try
            {
                FourfoldSteamDeckReadinessVerifier.VerifyProductLoopReadiness();
                findings.Add(Finding.Info("steam_deck.readiness", "Title, Hub, and R01 validated for 1280x800/1080p HUD safe areas, legacy movement axes, and controller-critical bindings."));
            }
            catch (Exception exception)
            {
                findings.Add(Finding.Error("steam_deck.readiness", "Steam Deck/controller readiness validation failed: " + exception.Message));
            }

            if (ShouldIncludeLegacyGateA())
            {
                try
                {
                    FourfoldUnitySpikeBuilder.BuildAndValidate();
                    findings.Add(Finding.Info("prototype.gate_a", "Legacy Gate A prototype generated and validated. This proves only the technical harness."));
                }
                catch (Exception exception)
                {
                    findings.Add(Finding.Error("prototype.gate_a", "Legacy Gate A prototype generation or validation failed: " + exception.Message));
                }
            }
            else
            {
                findings.Add(Finding.Info("prototype.gate_a", "Legacy Gate A generation skipped. Set FOURFOLD_INCLUDE_LEGACY_GATE_A=1 to validate the old harness explicitly."));
            }

            ScanAssets(metrics, findings);
            ScanOpenScene(metrics, findings);
            WriteReports(metrics, findings);

            var errorCount = findings.FindAll(finding => finding.severity == "error").Count;
            var warnCount = findings.FindAll(finding => finding.severity == "warn").Count;
            Debug.Log($"FOURFOLD product validation wrote {ReportJsonPath} and {ReportMarkdownPath}; errors={errorCount}, warnings={warnCount}");

            if (errorCount > 0)
            {
                throw new InvalidOperationException($"FOURFOLD product validation failed with {errorCount} errors. See {ReportMarkdownPath}");
            }
        }

        private static void ScanAssets(Metrics metrics, List<Finding> findings)
        {
            metrics.materialAssets = AssetDatabase.FindAssets("t:Material").Length;
            metrics.textureAssets = AssetDatabase.FindAssets("t:Texture").Length;
            metrics.audioClipAssets = AssetDatabase.FindAssets("t:AudioClip").Length;
            metrics.meshAssets = AssetDatabase.FindAssets("t:Mesh").Length;
            metrics.prefabAssets = AssetDatabase.FindAssets("t:Prefab").Length;
            metrics.sceneAssets = AssetDatabase.FindAssets("t:Scene").Length;

            if (metrics.audioClipAssets == 0)
            {
                findings.Add(Finding.Warn("audio.assets", "No imported AudioClip assets found. Current sound is prototype procedural code only."));
            }

            if (metrics.prefabAssets == 0)
            {
                findings.Add(Finding.Warn("prefab.assets", "No prefab assets found. Production content is not prefabbed yet."));
            }

            if (metrics.textureAssets == 0)
            {
                findings.Add(Finding.Warn("texture.assets", "No production texture assets found. Store-quality art pass has not started."));
            }

            var prefabGuids = AssetDatabase.FindAssets("t:GameObject");
            var prefabsWithLod = 0;
            foreach (var guid in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (root != null && root.GetComponentsInChildren<LODGroup>(true).Length > 0)
                {
                    prefabsWithLod++;
                }
            }

            metrics.prefabsWithLod = prefabsWithLod;
            if (metrics.prefabAssets > 0 && prefabsWithLod == 0)
            {
                findings.Add(Finding.Warn("lod.prefabs", "Prefab assets exist but none include LODGroup components."));
            }
        }

        private static void ScanOpenScene(Metrics metrics, List<Finding> findings)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded)
            {
                findings.Add(Finding.Error("scene.active", "No active loaded scene found for validation."));
                return;
            }

            metrics.openScenePath = activeScene.path;
            foreach (var root in activeScene.GetRootGameObjects())
            {
                ScanGameObject(root, metrics, findings);
            }

            if (metrics.sceneRenderers == 0)
            {
                findings.Add(Finding.Warn("scene.renderers", "Active scene has no renderers. This is expected before a canonical product scene exists."));
            }

            if (metrics.sceneAudioSources == 0)
            {
                findings.Add(Finding.Warn("scene.audio_sources", "Active scene has no AudioSource components at validation time."));
            }

            if (metrics.sceneLodGroups == 0)
            {
                findings.Add(Finding.Warn("scene.lod", "Active scene has no LODGroup components. This is acceptable for the reset baseline but not for production outdoor assets."));
            }

            if (metrics.sceneMissingMaterials > 0)
            {
                findings.Add(Finding.Error("scene.materials", $"Active scene has {metrics.sceneMissingMaterials} missing material slots."));
            }

            if (metrics.sceneMissingScripts > 0)
            {
                findings.Add(Finding.Error("scene.scripts", $"Active scene has {metrics.sceneMissingScripts} missing script components."));
            }

            if (metrics.sceneNegativeScaleObjects > 0)
            {
                findings.Add(Finding.Warn("scene.scale", $"Active scene has {metrics.sceneNegativeScaleObjects} objects with negative scale."));
            }
        }

        private static void ScanGameObject(GameObject gameObject, Metrics metrics, List<Finding> findings)
        {
            metrics.sceneObjects++;
            metrics.sceneMissingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);

            var scale = gameObject.transform.localScale;
            if (scale.x < 0f || scale.y < 0f || scale.z < 0f)
            {
                metrics.sceneNegativeScaleObjects++;
            }

            if (Mathf.Approximately(scale.x, 0f) || Mathf.Approximately(scale.y, 0f) || Mathf.Approximately(scale.z, 0f))
            {
                findings.Add(Finding.Warn("scene.scale.zero", $"Object has zero scale axis: {GetPath(gameObject)}"));
            }

            foreach (var renderer in gameObject.GetComponents<Renderer>())
            {
                metrics.sceneRenderers++;
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null)
                    {
                        metrics.sceneMissingMaterials++;
                    }
                }

                var size = renderer.bounds.size;
                if (size.x > 100f || size.y > 100f || size.z > 100f)
                {
                    findings.Add(Finding.Warn("scene.bounds.large", $"Renderer bounds are unusually large on {GetPath(gameObject)}: {size}"));
                }
            }

            metrics.sceneAudioSources += gameObject.GetComponents<AudioSource>().Length;
            metrics.sceneLodGroups += gameObject.GetComponents<LODGroup>().Length;

            foreach (Transform child in gameObject.transform)
            {
                ScanGameObject(child.gameObject, metrics, findings);
            }
        }

        private static void WriteReports(Metrics metrics, List<Finding> findings)
        {
            File.WriteAllText(ReportJsonPath, BuildJson(metrics, findings));
            File.WriteAllText(ReportMarkdownPath, BuildMarkdown(metrics, findings));
            AssetDatabase.Refresh();
        }

        private static bool ShouldIncludeLegacyGateA()
        {
            return Environment.GetEnvironmentVariable("FOURFOLD_INCLUDE_LEGACY_GATE_A") == "1";
        }

        private static string BuildJson(Metrics metrics, List<Finding> findings)
        {
            var builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine("  \"version\": 1,");
            builder.AppendLine("  \"generatedAtUtc\": \"" + DateTime.UtcNow.ToString("o") + "\",");
            builder.AppendLine("  \"metrics\": {");
            builder.AppendLine($"    \"sceneObjects\": {metrics.sceneObjects},");
            builder.AppendLine($"    \"sceneRenderers\": {metrics.sceneRenderers},");
            builder.AppendLine($"    \"sceneAudioSources\": {metrics.sceneAudioSources},");
            builder.AppendLine($"    \"sceneLodGroups\": {metrics.sceneLodGroups},");
            builder.AppendLine($"    \"sceneMissingMaterials\": {metrics.sceneMissingMaterials},");
            builder.AppendLine($"    \"sceneMissingScripts\": {metrics.sceneMissingScripts},");
            builder.AppendLine($"    \"sceneNegativeScaleObjects\": {metrics.sceneNegativeScaleObjects},");
            builder.AppendLine($"    \"materialAssets\": {metrics.materialAssets},");
            builder.AppendLine($"    \"textureAssets\": {metrics.textureAssets},");
            builder.AppendLine($"    \"audioClipAssets\": {metrics.audioClipAssets},");
            builder.AppendLine($"    \"meshAssets\": {metrics.meshAssets},");
            builder.AppendLine($"    \"prefabAssets\": {metrics.prefabAssets},");
            builder.AppendLine($"    \"prefabsWithLod\": {metrics.prefabsWithLod},");
            builder.AppendLine($"    \"sceneAssets\": {metrics.sceneAssets}");
            builder.AppendLine("  },");
            builder.AppendLine("  \"findings\": [");
            for (var index = 0; index < findings.Count; index++)
            {
                var finding = findings[index];
                builder.Append("    {");
                builder.Append($"\"severity\": \"{Escape(finding.severity)}\", ");
                builder.Append($"\"code\": \"{Escape(finding.code)}\", ");
                builder.Append($"\"message\": \"{Escape(finding.message)}\"");
                builder.Append(index == findings.Count - 1 ? "}\n" : "},\n");
            }
            builder.AppendLine("  ]");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string BuildMarkdown(Metrics metrics, List<Finding> findings)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Unity Product Validation");
            builder.AppendLine();
            builder.AppendLine($"Generated UTC: `{DateTime.UtcNow:o}`");
            builder.AppendLine();
            builder.AppendLine("## Metrics");
            builder.AppendLine();
            builder.AppendLine("| Metric | Value |");
            builder.AppendLine("| --- | ---: |");
            builder.AppendLine($"| Active scene objects | {metrics.sceneObjects} |");
            builder.AppendLine($"| Renderers | {metrics.sceneRenderers} |");
            builder.AppendLine($"| AudioSources | {metrics.sceneAudioSources} |");
            builder.AppendLine($"| LODGroups | {metrics.sceneLodGroups} |");
            builder.AppendLine($"| Missing material slots | {metrics.sceneMissingMaterials} |");
            builder.AppendLine($"| Missing scripts | {metrics.sceneMissingScripts} |");
            builder.AppendLine($"| Negative scale objects | {metrics.sceneNegativeScaleObjects} |");
            builder.AppendLine($"| Material assets | {metrics.materialAssets} |");
            builder.AppendLine($"| Texture assets | {metrics.textureAssets} |");
            builder.AppendLine($"| AudioClip assets | {metrics.audioClipAssets} |");
            builder.AppendLine($"| Mesh assets | {metrics.meshAssets} |");
            builder.AppendLine($"| Prefab assets | {metrics.prefabAssets} |");
            builder.AppendLine($"| Prefabs with LODGroup | {metrics.prefabsWithLod} |");
            builder.AppendLine($"| Scene assets | {metrics.sceneAssets} |");
            builder.AppendLine();
            builder.AppendLine("## Findings");
            builder.AppendLine();
            foreach (var finding in findings)
            {
                builder.AppendLine($"- **{finding.severity}** `{finding.code}`: {finding.message}");
            }
            builder.AppendLine();
            builder.AppendLine("## Product Interpretation");
            builder.AppendLine();
            builder.AppendLine("This report validates technical hygiene only. D022 is the current product contract: Steam-first, buy-to-play, single-player, compact top-down classic action-adventure, one hub, three regions, four bosses, and one exploration tool. Title is the product entry point, HubCrossroads is the playable hub, and R01 Verdant Steps is the first playable evidence path for D022 player-facing language and UI/UX. Required product evidence includes title flow, hub objective marker, mission briefing, readable combat, exploration tool target response, boss clear, reward save-on-hub-return, local save, required SFX/BGM, pause/settings/language UX, and 1280x800 readability. Historical ProductReview, Gate A, D020, and D021 evidence are deliberately outside the active implementation lane unless explicitly migrated into D022.");
            return builder.ToString();
        }

        private static string GetPath(GameObject gameObject)
        {
            var path = gameObject.name;
            var current = gameObject.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        private static string Escape(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private sealed class Metrics
        {
            public string openScenePath;
            public int sceneObjects;
            public int sceneRenderers;
            public int sceneAudioSources;
            public int sceneLodGroups;
            public int sceneMissingMaterials;
            public int sceneMissingScripts;
            public int sceneNegativeScaleObjects;
            public int materialAssets;
            public int textureAssets;
            public int audioClipAssets;
            public int meshAssets;
            public int prefabAssets;
            public int prefabsWithLod;
            public int sceneAssets;
        }

        private sealed class Finding
        {
            public string severity;
            public string code;
            public string message;

            public static Finding Info(string code, string message)
            {
                return new Finding { severity = "info", code = code, message = message };
            }

            public static Finding Warn(string code, string message)
            {
                return new Finding { severity = "warn", code = code, message = message };
            }

            public static Finding Error(string code, string message)
            {
                return new Finding { severity = "error", code = code, message = message };
            }
        }
    }
}
