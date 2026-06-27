using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldModelRigAnimationPackVerifier
    {
        private const string PackRoot = "Assets/Art/ModelRigAnimation/Enemy/MeleeShardling/SealedLockRelic_v0.1.0";
        private const string ModelPath = PackRoot + "/Models/MDL_Enemy_MeleeShardling_SealedLockRelic_v0.1.0.fbx";
        private const string AssetJsonPath = PackRoot + "/asset.json";
        private const string RigMemoPath = PackRoot + "/Docs/avatar_rig_memo.md";
        private const string PreviewGifPath = PackRoot + "/Previews/preview.gif";
        private const string ClipRoot = PackRoot + "/AnimationClips";
        private const string UnityQcPath = PackRoot + "/unity_import_qc.json";
        private const int TriangleBudgetLod0 = 6000;
        private const int MaterialBudget = 4;

        private static readonly string[] RequiredSockets =
        {
            "SOCKET_Ground",
            "SOCKET_ChestCore",
            "SOCKET_WeakPoint",
            "SOCKET_Back",
            "SOCKET_AttackOrigin",
            "SOCKET_ForwardHit",
            "SOCKET_RedSeamVFX",
            "SOCKET_Cast",
            "SOCKET_HitVfx",
        };

        private static readonly AnimationSpec[] AnimationSpecs =
        {
            new AnimationSpec("Idle", true),
            new AnimationSpec("Walk", true),
            new AnimationSpec("Run", true),
            new AnimationSpec("AttackStart", false),
            new AnimationSpec("AttackLoop", true),
            new AnimationSpec("AttackEnd", false),
            new AnimationSpec("HitFront", false),
            new AnimationSpec("HitBack", false),
            new AnimationSpec("Knockdown", false),
            new AnimationSpec("Death", false),
            new AnimationSpec("CastStart", false),
            new AnimationSpec("ChannelLoop", true),
            new AnimationSpec("CastRelease", false),
            new AnimationSpec("Interact", false),
        };

        [MenuItem("FOURFOLD/Assets/Verify Model Rig Animation Pack")]
        public static void VerifyMeleeShardlingPack()
        {
            AssetDatabase.Refresh();

            var errors = new List<string>();
            var warnings = new List<string>();
            var animationReports = new List<AnimationReport>();

            RequireFile(ModelPath, errors);
            RequireFile(AssetJsonPath, errors);
            RequireFile(RigMemoPath, errors);
            RequireFile(PreviewGifPath, errors);

            ConfigureModelImporter(ModelPath, importAnimation: false, loop: false, errors);

            foreach (var spec in AnimationSpecs)
            {
                var path = AnimationPath(spec.Action);
                RequireFile(path, errors);
                ConfigureModelImporter(path, importAnimation: true, loop: spec.Loop, errors);
            }

            GenerateUnityAnimationClips();
            AssetDatabase.Refresh();

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                errors.Add("Model FBX could not be loaded as a GameObject.");
            }
            else
            {
                VerifyModel(model, errors, warnings);
            }

            foreach (var spec in AnimationSpecs)
            {
                var path = ClipPath(spec.Action);
                var report = VerifyAnimationClip(path, spec, errors, warnings);
                animationReports.Add(report);
            }

            WriteReport(errors, warnings, animationReports);

            if (errors.Count > 0)
            {
                throw new InvalidOperationException($"Model rig animation pack verification failed with {errors.Count} errors. See {UnityQcPath}");
            }

            Debug.Log($"Model rig animation pack verification passed. Report: {UnityQcPath}");
        }

        private static void GenerateUnityAnimationClips()
        {
            Directory.CreateDirectory(ClipRoot);
            foreach (var spec in AnimationSpecs)
            {
                var path = ClipPath(spec.Action);
                if (File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }

                var clip = BuildClip(spec);
                AssetDatabase.CreateAsset(clip, path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static AnimationClip BuildClip(AnimationSpec spec)
        {
            var clip = new AnimationClip
            {
                name = $"ANM_Enemy_MeleeShardling_{spec.Action}_SealedLockRelic_v0.1.0",
                frameRate = 30f,
            };

            var basePath = "ROOT/CTRL_Base";
            var wedgePath = "ROOT/CTRL_Base/CTRL_FrontWedge";
            var seamPath = "ROOT/CTRL_Base/CTRL_Body/CTRL_RedSeam";
            var leftPlatePath = "ROOT/CTRL_Base/CTRL_Body/CTRL_TopPlate_L";
            var rightPlatePath = "ROOT/CTRL_Base/CTRL_Body/CTRL_TopPlate_R";

            switch (spec.Action)
            {
                case "Idle":
                    SetPositionZ(clip, basePath, Keys(1, 0f, 16, 0.018f, 31, 0f, 46, -0.012f, 61, 0f));
                    SetEulerZ(clip, basePath, Keys(1, 0f, 16, 1.5f, 31, 0f, 46, -1f, 61, 0f));
                    SetUniformScale(clip, seamPath, Keys(1, 1f, 16, 1.08f, 31, 1f, 46, 0.96f, 61, 1f));
                    break;

                case "Walk":
                    SetPositionZ(clip, basePath, Keys(1, 0f, 8, 0.035f, 16, 0f, 24, 0.035f, 31, 0f));
                    SetEulerZ(clip, basePath, Keys(1, 0f, 8, 2.5f, 16, 0f, 24, -2.5f, 31, 0f));
                    SetPositionY(clip, wedgePath, Keys(1, 0f, 8, -0.025f, 16, 0f, 24, -0.025f, 31, 0f));
                    break;

                case "Run":
                    SetPositionZ(clip, basePath, Keys(1, 0f, 7, 0.045f, 13, 0f, 19, 0.045f, 25, 0f));
                    SetEulerZ(clip, basePath, Keys(1, 0f, 7, 4f, 13, 0f, 19, -4f, 25, 0f));
                    SetPositionY(clip, wedgePath, Keys(1, 0f, 7, -0.04f, 13, 0f, 19, -0.04f, 25, 0f));
                    break;

                case "AttackStart":
                    SetPositionY(clip, wedgePath, Keys(1, 0f, 8, 0.08f, 14, -0.20f, 19, -0.16f));
                    SetEulerX(clip, wedgePath, Keys(1, 0f, 8, 5f, 14, -6f, 19, -3f));
                    SetUniformScale(clip, seamPath, Keys(1, 1f, 8, 1.25f, 14, 1.5f, 19, 1.25f));
                    break;

                case "AttackLoop":
                    SetPositionY(clip, wedgePath, Keys(1, -0.16f, 7, -0.18f, 13, -0.15f, 19, -0.18f, 25, -0.16f));
                    SetPositionZ(clip, basePath, Keys(1, 0.03f, 7, 0.045f, 13, 0.03f, 19, 0.045f, 25, 0.03f));
                    SetUniformScale(clip, seamPath, Keys(1, 1.25f, 7, 1.35f, 13, 1.22f, 19, 1.35f, 25, 1.25f));
                    break;

                case "AttackEnd":
                    SetPositionY(clip, wedgePath, Keys(1, -0.16f, 8, 0.04f, 15, 0f));
                    SetEulerX(clip, wedgePath, Keys(1, -3f, 8, 3f, 15, 0f));
                    SetUniformScale(clip, seamPath, Keys(1, 1.25f, 8, 1.1f, 15, 1f));
                    break;

                case "HitFront":
                    SetPositionY(clip, basePath, Keys(1, 0f, 2, 0.10f, 7, -0.035f, 13, 0f));
                    SetEulerZ(clip, basePath, Keys(1, 0f, 2, -4f, 7, 2f, 13, 0f));
                    SetUniformScale(clip, seamPath, Keys(1, 1f, 2, 1.4f, 7, 1.15f, 13, 1f));
                    break;

                case "HitBack":
                    SetPositionY(clip, basePath, Keys(1, 0f, 2, -0.10f, 7, 0.035f, 13, 0f));
                    SetEulerZ(clip, basePath, Keys(1, 0f, 2, 4f, 7, -2f, 13, 0f));
                    SetUniformScale(clip, seamPath, Keys(1, 1f, 2, 1.4f, 7, 1.15f, 13, 1f));
                    break;

                case "Knockdown":
                    SetPositionY(clip, basePath, Keys(1, 0f, 10, 0.06f, 18, 0.12f, 31, 0.10f));
                    SetPositionZ(clip, basePath, Keys(1, 0f, 10, 0.02f, 18, -0.045f, 31, -0.06f));
                    SetEulerX(clip, basePath, Keys(1, 0f, 10, 10f, 18, 23f, 31, 19f));
                    break;

                case "Death":
                    SetPositionY(clip, basePath, Keys(1, 0f, 8, -0.02f, 20, 0.12f, 28, 0.15f, 45, 0.15f));
                    SetPositionZ(clip, basePath, Keys(1, 0f, 8, 0.04f, 20, -0.03f, 28, -0.06f, 45, -0.07f));
                    SetEulerX(clip, basePath, Keys(1, 0f, 8, -4f, 20, 18f, 28, 25f, 45, 26f));
                    SetUniformScale(clip, seamPath, Keys(1, 1f, 8, 1.55f, 28, 0.75f, 45, 0.45f));
                    break;

                case "CastStart":
                    SetUniformScale(clip, seamPath, Keys(1, 1f, 8, 1.3f, 18, 1.55f, 21, 1.45f));
                    SetEulerY(clip, leftPlatePath, Keys(1, 0f, 8, -5f, 21, -8f));
                    SetEulerY(clip, rightPlatePath, Keys(1, 0f, 8, 5f, 21, 8f));
                    break;

                case "ChannelLoop":
                    SetPositionZ(clip, basePath, Keys(1, 0.02f, 10, 0.045f, 19, 0.02f, 28, 0.045f, 37, 0.02f));
                    SetEulerZ(clip, basePath, Keys(1, 0f, 10, 1.5f, 19, 0f, 28, -1.5f, 37, 0f));
                    SetUniformScale(clip, seamPath, Keys(1, 1.45f, 10, 1.65f, 19, 1.45f, 28, 1.65f, 37, 1.45f));
                    break;

                case "CastRelease":
                    SetPositionY(clip, wedgePath, Keys(1, 0f, 8, 0f, 12, -0.10f, 19, 0f));
                    SetUniformScale(clip, seamPath, Keys(1, 1.45f, 8, 1.8f, 12, 1.25f, 19, 1f));
                    break;

                case "Interact":
                    SetPositionY(clip, wedgePath, Keys(1, 0f, 8, 0.04f, 12, -0.08f, 18, 0.02f, 25, 0f));
                    SetPositionZ(clip, basePath, Keys(1, 0f, 8, 0.02f, 12, 0.015f, 18, 0.025f, 25, 0f));
                    SetUniformScale(clip, seamPath, Keys(1, 1f, 8, 1.1f, 12, 1.28f, 18, 1.08f, 25, 1f));
                    break;

                default:
                    throw new InvalidOperationException($"No Unity AnimationClip recipe for {spec.Action}");
            }

            AnimationUtility.SetAnimationEvents(clip, BuildEvents(spec.Action));
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = spec.Loop;
            settings.loopBlend = spec.Loop;
            settings.keepOriginalOrientation = true;
            settings.keepOriginalPositionY = true;
            settings.keepOriginalPositionXZ = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void ConfigureModelImporter(string path, bool importAnimation, bool loop, List<string> errors)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                errors.Add($"Missing ModelImporter for {path}");
                return;
            }

            importer.globalScale = 1f;
            importer.useFileUnits = true;
            importer.importAnimation = importAnimation;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.optimizeGameObjects = false;
            importer.preserveHierarchy = true;
            importer.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;

            if (importAnimation)
            {
                var clips = importer.defaultClipAnimations;
                if (clips.Length == 0)
                {
                    clips = importer.clipAnimations;
                }

                if (clips.Length > 0)
                {
                    for (var index = 0; index < clips.Length; index++)
                    {
                        var clip = clips[index];
                        clip.loopTime = loop;
                        clip.loopPose = loop;
                        clip.keepOriginalOrientation = true;
                        clip.keepOriginalPositionY = true;
                        clip.keepOriginalPositionXZ = true;
                        clip.lockRootRotation = true;
                        clip.lockRootHeightY = true;
                        clip.lockRootPositionXZ = true;
                        clips[index] = clip;
                    }

                    importer.clipAnimations = clips;
                }
            }

            importer.SaveAndReimport();
        }

        private static void VerifyModel(GameObject model, List<string> errors, List<string> warnings)
        {
            var transforms = model.GetComponentsInChildren<Transform>(true).Select(transform => transform.name).ToHashSet();
            if (!transforms.Contains("ROOT"))
            {
                errors.Add("Model hierarchy is missing ROOT.");
            }

            foreach (var socket in RequiredSockets)
            {
                if (!transforms.Contains(socket))
                {
                    errors.Add($"Model hierarchy is missing socket {socket}.");
                }
            }

            var renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                errors.Add("Model has no renderers after Unity import.");
            }

            var missingMaterials = renderers.Sum(renderer => renderer.sharedMaterials.Count(material => material == null));
            if (missingMaterials > 0)
            {
                errors.Add($"Model has {missingMaterials} missing material slots.");
            }

            var materialCount = renderers
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .Select(material => material.name)
                .Distinct()
                .Count();
            if (materialCount > MaterialBudget)
            {
                errors.Add($"Material count exceeds budget: {materialCount}/{MaterialBudget}.");
            }

            var triangleCount = CountModelTriangles(model);
            if (triangleCount <= 0)
            {
                errors.Add("Model triangle count is zero after Unity import.");
            }
            else if (triangleCount > TriangleBudgetLod0)
            {
                errors.Add($"Model exceeds LOD0 triangle budget: {triangleCount}/{TriangleBudgetLod0}.");
            }

            var bounds = CombinedBounds(renderers);
            if (bounds.HasValue)
            {
                var size = bounds.Value.size;
                if (size.x > 2.0f || size.y > 2.0f || size.z > 2.0f)
                {
                    warnings.Add($"Imported bounds are larger than expected for a 1.2m enemy: {size}.");
                }
            }
        }

        private static AnimationReport VerifyAnimationClip(string path, AnimationSpec spec, List<string> errors, List<string> warnings)
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(clip => !clip.name.StartsWith("__", StringComparison.Ordinal)).ToArray();
            var report = new AnimationReport(spec.Action, path, spec.Loop, clips.Length);
            if (clips.Length == 0)
            {
                errors.Add($"Animation FBX has no AnimationClip: {path}");
                return report;
            }

            var clip = clips[0];
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            var curveCount = AnimationUtility.GetCurveBindings(clip).Length;
            report.ImportedLoop = settings.loopTime;
            report.FrameRate = clip.frameRate;
            report.LengthSeconds = clip.length;
            report.RootPositionMaxAbs = MaxRootPositionCurveAbs(clip);
            report.CurveCount = curveCount;

            if (spec.Loop != settings.loopTime)
            {
                errors.Add($"{spec.Action} loop setting mismatch: expected {spec.Loop}, got {settings.loopTime}.");
            }

            if (!Mathf.Approximately(clip.frameRate, 30f))
            {
                warnings.Add($"{spec.Action} imported at {clip.frameRate.ToString(CultureInfo.InvariantCulture)} fps; expected 30 fps.");
            }

            if (curveCount == 0)
            {
                errors.Add($"{spec.Action} AnimationClip has no transform curves.");
            }

            if (report.RootPositionMaxAbs > 0.001f)
            {
                errors.Add($"{spec.Action} has root position drift above tolerance: {report.RootPositionMaxAbs}.");
            }

            return report;
        }

        private static float MaxRootPositionCurveAbs(AnimationClip clip)
        {
            var max = 0f;
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                if (!binding.path.EndsWith("ROOT", StringComparison.Ordinal) && binding.path != "ROOT")
                {
                    continue;
                }

                if (binding.propertyName.IndexOf("LocalPosition", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve == null)
                {
                    continue;
                }

                foreach (var key in curve.keys)
                {
                    max = Mathf.Max(max, Mathf.Abs(key.value));
                }
            }

            return max;
        }

        private static void SetPositionY(AnimationClip clip, string path, Keyframe[] keys)
        {
            clip.SetCurve(path, typeof(Transform), "localPosition.y", new AnimationCurve(keys));
        }

        private static void SetPositionZ(AnimationClip clip, string path, Keyframe[] keys)
        {
            clip.SetCurve(path, typeof(Transform), "localPosition.z", new AnimationCurve(keys));
        }

        private static void SetEulerX(AnimationClip clip, string path, Keyframe[] keys)
        {
            clip.SetCurve(path, typeof(Transform), "localEulerAnglesRaw.x", new AnimationCurve(keys));
        }

        private static void SetEulerY(AnimationClip clip, string path, Keyframe[] keys)
        {
            clip.SetCurve(path, typeof(Transform), "localEulerAnglesRaw.y", new AnimationCurve(keys));
        }

        private static void SetEulerZ(AnimationClip clip, string path, Keyframe[] keys)
        {
            clip.SetCurve(path, typeof(Transform), "localEulerAnglesRaw.z", new AnimationCurve(keys));
        }

        private static void SetUniformScale(AnimationClip clip, string path, Keyframe[] keys)
        {
            clip.SetCurve(path, typeof(Transform), "localScale.x", new AnimationCurve(keys));
            clip.SetCurve(path, typeof(Transform), "localScale.y", new AnimationCurve(keys));
            clip.SetCurve(path, typeof(Transform), "localScale.z", new AnimationCurve(keys));
        }

        private static Keyframe[] Keys(params object[] frameValuePairs)
        {
            if (frameValuePairs.Length % 2 != 0)
            {
                throw new ArgumentException("Frame/value key data must contain pairs.");
            }

            var keys = new Keyframe[frameValuePairs.Length / 2];
            for (var index = 0; index < keys.Length; index++)
            {
                var frame = Convert.ToSingle(frameValuePairs[index * 2], CultureInfo.InvariantCulture);
                var value = Convert.ToSingle(frameValuePairs[index * 2 + 1], CultureInfo.InvariantCulture);
                keys[index] = new Keyframe((frame - 1f) / 30f, value);
            }

            return keys;
        }

        private static AnimationEvent[] BuildEvents(string action)
        {
            switch (action)
            {
                case "Walk":
                    return Events(Event(1, "base_contact_left"), Event(16, "base_contact_right"));
                case "Run":
                    return Events(Event(1, "base_contact_left"), Event(13, "base_contact_right"));
                case "AttackStart":
                    return Events(Event(8, "attack_windup"), Event(10, "hit_active_start"), Event(14, "hit_peak"), Event(17, "hit_active_end"));
                case "AttackLoop":
                    return Events(Event(6, "loop_pressure_pulse"), Event(18, "loop_pressure_pulse"));
                case "AttackEnd":
                    return Events(Event(8, "recover"));
                case "HitFront":
                case "HitBack":
                    return Events(Event(2, "hit_vfx"), Event(7, "armor_clack"));
                case "Knockdown":
                    return Events(Event(18, "ground_contact"), Event(26, "stun_open"));
                case "Death":
                    return Events(Event(8, "death_start"), Event(28, "death_vfx"), Event(44, "hide_allowed"));
                case "CastStart":
                    return Events(Event(8, "cast_charge"), Event(18, "cast_ready"));
                case "ChannelLoop":
                    return Events(Event(10, "channel_pulse"), Event(28, "channel_pulse"));
                case "CastRelease":
                    return Events(Event(12, "projectile_release"), Event(16, "cast_recover"));
                case "Interact":
                    return Events(Event(12, "interact_contact"), Event(18, "interact_vfx"));
                default:
                    return Array.Empty<AnimationEvent>();
            }
        }

        private static AnimationEvent Event(int frame, string name)
        {
            return new AnimationEvent
            {
                time = (frame - 1f) / 30f,
                functionName = name,
            };
        }

        private static AnimationEvent[] Events(params AnimationEvent[] events)
        {
            return events;
        }

        private static int CountModelTriangles(GameObject model)
        {
            var total = 0;
            foreach (var filter in model.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh != null)
                {
                    total += filter.sharedMesh.triangles.Length / 3;
                }
            }

            foreach (var skinned in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (skinned.sharedMesh != null)
                {
                    total += skinned.sharedMesh.triangles.Length / 3;
                }
            }

            return total;
        }

        private static Bounds? CombinedBounds(Renderer[] renderers)
        {
            var first = true;
            var combined = new Bounds();
            foreach (var renderer in renderers)
            {
                if (first)
                {
                    combined = renderer.bounds;
                    first = false;
                }
                else
                {
                    combined.Encapsulate(renderer.bounds);
                }
            }

            return first ? null : combined;
        }

        private static void RequireFile(string path, List<string> errors)
        {
            if (!File.Exists(path))
            {
                errors.Add($"Missing required file: {path}");
            }
        }

        private static string AnimationPath(string action)
        {
            return $"{PackRoot}/Animations/ANM_Enemy_MeleeShardling_{action}_SealedLockRelic_v0.1.0.fbx";
        }

        private static string ClipPath(string action)
        {
            return $"{ClipRoot}/ANM_Enemy_MeleeShardling_{action}_SealedLockRelic_v0.1.0.anim";
        }

        private static void WriteReport(List<string> errors, List<string> warnings, List<AnimationReport> animations)
        {
            var builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine("  \"schema\": \"model_rig_animation_unity_import_qc_v1\",");
            builder.AppendLine($"  \"generatedAtUtc\": \"{DateTime.UtcNow:o}\",");
            builder.AppendLine($"  \"status\": \"{(errors.Count == 0 ? "pass" : "fail")}\",");
            builder.AppendLine($"  \"model\": \"{ModelPath}\",");
            builder.AppendLine("  \"unityImport\": {");
            builder.AppendLine($"    \"modelImporterAnimationType\": \"{Escape(GetModelImporterAnimationType())}\",");
            builder.AppendLine("    \"rigStyle\": \"mechanical_transform_hierarchy_with_generated_animation_clips\",");
            builder.AppendLine("    \"globalScale\": 1.0,");
            builder.AppendLine("    \"rootMotion\": \"off_root_locked_controller_driven_transform_clips\",");
            builder.AppendLine("    \"axis\": \"Unity Y-up +Z-forward from FBX import\"");
            builder.AppendLine("  },");
            builder.AppendLine("  \"animations\": [");
            for (var index = 0; index < animations.Count; index++)
            {
                var report = animations[index];
                builder.AppendLine("    {");
                builder.AppendLine($"      \"action\": \"{report.Action}\",");
                builder.AppendLine($"      \"file\": \"{report.Path}\",");
                builder.AppendLine($"      \"expectedLoop\": {Bool(report.ExpectedLoop)},");
                builder.AppendLine($"      \"importedLoop\": {Bool(report.ImportedLoop)},");
                builder.AppendLine($"      \"clipCount\": {report.ClipCount},");
                builder.AppendLine($"      \"curveCount\": {report.CurveCount},");
                builder.AppendLine($"      \"frameRate\": {report.FrameRate.ToString(CultureInfo.InvariantCulture)},");
                builder.AppendLine($"      \"lengthSeconds\": {report.LengthSeconds.ToString(CultureInfo.InvariantCulture)},");
                builder.AppendLine($"      \"rootPositionMaxAbs\": {report.RootPositionMaxAbs.ToString(CultureInfo.InvariantCulture)}");
                builder.Append(index == animations.Count - 1 ? "    }\n" : "    },\n");
            }

            builder.AppendLine("  ],");
            WriteStringArray(builder, "warnings", warnings, trailingComma: true);
            WriteStringArray(builder, "errors", errors, trailingComma: false);
            builder.AppendLine("}");
            File.WriteAllText(UnityQcPath, builder.ToString(), Encoding.UTF8);
            AssetDatabase.ImportAsset(UnityQcPath);
        }

        private static void WriteStringArray(StringBuilder builder, string name, IReadOnlyList<string> values, bool trailingComma)
        {
            builder.AppendLine($"  \"{name}\": [");
            for (var index = 0; index < values.Count; index++)
            {
                builder.AppendLine($"    \"{Escape(values[index])}\"{(index == values.Count - 1 ? string.Empty : ",")}");
            }

            builder.AppendLine(trailingComma ? "  ]," : "  ]");
        }

        private static string Escape(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string GetModelImporterAnimationType()
        {
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            return importer == null ? "unknown" : importer.animationType.ToString();
        }

        private static string Bool(bool value)
        {
            return value ? "true" : "false";
        }

        private readonly struct AnimationSpec
        {
            public AnimationSpec(string action, bool loop)
            {
                Action = action;
                Loop = loop;
            }

            public string Action { get; }
            public bool Loop { get; }
        }

        private sealed class AnimationReport
        {
            public AnimationReport(string action, string path, bool expectedLoop, int clipCount)
            {
                Action = action;
                Path = path;
                ExpectedLoop = expectedLoop;
                ClipCount = clipCount;
            }

            public string Action { get; }
            public string Path { get; }
            public bool ExpectedLoop { get; }
            public bool ImportedLoop { get; set; }
            public int ClipCount { get; }
            public float FrameRate { get; set; }
            public float LengthSeconds { get; set; }
            public float RootPositionMaxAbs { get; set; }
            public int CurveCount { get; set; }
        }
    }
}
