using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldGateAEvidenceReport
    {
        private const int CaptureWidth = 1280;
        private const int CaptureHeight = 800;
        private const string ScenePath = "Assets/Scenes/AshenThresholdSpike.unity";
        private const string ReportFileName = "gate-a-evidence.json";

        public static void Capture()
        {
            var outputDirectory = GetOutputDirectory();
            Directory.CreateDirectory(outputDirectory);

            var report = CreateBaseReport(outputDirectory);
            try
            {
                FourfoldUnitySpikeBuilder.BuildAndValidate();
                report.scene_validation.status = "passed";
                report.scene_validation.message = "FourfoldUnitySpikeBuilder.BuildAndValidate completed.";

                var mainCamera = FindCamera();
                var objectiveCamera = CreateObjectiveCamera(mainCamera);
                var captures = new[]
                {
                    new CaptureSpec
                    {
                        id = "main_camera",
                        purpose = "Fixed-angle Gate A room proof from the generated gameplay camera.",
                        file_name = "gate-a-main-camera.png",
                        camera = mainCamera
                    },
                    new CaptureSpec
                    {
                        id = "objective_gate_detail",
                        purpose = "Deterministic objective/gate readability view; editor-only camera, no gameplay state changes.",
                        file_name = "gate-a-objective-gate.png",
                        camera = objectiveCamera
                    }
                };

                report.captures = CaptureViews(outputDirectory, captures);
                report.gameplay_readability = EvaluateReadability(captures);
                report.run_status = "passed";
            }
            catch (Exception error)
            {
                report.run_status = "failed";
                if (report.scene_validation.status == "not_run")
                {
                    report.scene_validation.status = "failed";
                    report.scene_validation.message = error.Message;
                }
                report.error = error.ToString();
                throw;
            }
            finally
            {
                var reportPath = Path.Combine(outputDirectory, ReportFileName);
                File.WriteAllText(reportPath, JsonUtility.ToJson(report, true) + Environment.NewLine);
                Debug.Log($"FOURFOLD Gate A evidence report written: {reportPath}");
            }
        }

        private static EvidenceReport CreateBaseReport(string outputDirectory)
        {
            var projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return new EvidenceReport
            {
                schema_version = "fourfold.gate_a.evidence.v1",
                generated_at_utc = DateTime.UtcNow.ToString("o"),
                run_status = "not_run",
                project_path = projectPath,
                evidence_directory = Path.GetFullPath(outputDirectory),
                git_commit = GetMetadata("FOURFOLD_GIT_COMMIT", projectPath, "rev-parse", "HEAD"),
                git_branch = GetMetadata("FOURFOLD_GIT_BRANCH", projectPath, "branch", "--show-current"),
                unity_version = Application.unityVersion,
                unity_editor_path = EditorApplication.applicationPath,
                scene_path = ScenePath,
                scene_validation = new SceneValidation
                {
                    status = "not_run",
                    message = "Scene validation has not run yet."
                },
                build_target_support = BuildTargetSupportEvidence(),
                captures = new CaptureRecord[0],
                gameplay_readability = GameplayReadability.Empty(),
                console_error_scan = new ConsoleErrorScan
                {
                    status = "not_scanned",
                    unity_log_path = Environment.GetEnvironmentVariable("FOURFOLD_UNITY_LOG_PATH") ?? string.Empty,
                    error_count = 0,
                    sample_lines = new string[0],
                    unity_exit_code = -1
                },
                error = string.Empty
            };
        }

        private static BuildTargetEvidence[] BuildTargetSupportEvidence()
        {
            return new[]
            {
                BuildTargetEvidenceFor("macos", BuildTarget.StandaloneOSX),
                BuildTargetEvidenceFor("windows", BuildTarget.StandaloneWindows64)
            };
        }

        private static BuildTargetEvidence BuildTargetEvidenceFor(string label, BuildTarget target)
        {
            var supported = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, target);
            return new BuildTargetEvidence
            {
                target = label,
                unity_build_target = target.ToString(),
                supported = supported,
                note = supported
                    ? "Unity reports this standalone target as installed and supported in the current editor."
                    : "Unity reports this standalone target as missing or unsupported; install the target module before treating builds as verified."
            };
        }

        private static CaptureRecord[] CaptureViews(string outputDirectory, CaptureSpec[] captures)
        {
            var records = new List<CaptureRecord>();
            foreach (var capture in captures)
            {
                var path = Path.Combine(outputDirectory, capture.file_name);
                CaptureCamera(capture.camera, path);
                var fileInfo = new FileInfo(path);
                records.Add(new CaptureRecord
                {
                    id = capture.id,
                    purpose = capture.purpose,
                    path = Path.GetFullPath(path),
                    width = CaptureWidth,
                    height = CaptureHeight,
                    camera_name = capture.camera.name,
                    camera_position = Float3.From(capture.camera.transform.position),
                    camera_rotation_euler = Float3.From(capture.camera.transform.rotation.eulerAngles),
                    orthographic = capture.camera.orthographic,
                    orthographic_size = capture.camera.orthographicSize,
                    non_empty = fileInfo.Exists && fileInfo.Length > 0,
                    size_bytes = fileInfo.Exists ? fileInfo.Length : 0
                });
            }

            return records.ToArray();
        }

        private static GameplayReadability EvaluateReadability(CaptureSpec[] captures)
        {
            var player = EvaluateReadabilityItem(
                "player",
                new[] { "Echo Bearer Player" },
                captures,
                "Playable character body should be readable in the fixed-angle room.");
            var enemy = EvaluateReadabilityItem(
                "enemy",
                new[] { "Hollow Grunt" },
                captures,
                "Primary hostile should be visible enough to read combat pressure.");
            var weapon = EvaluateReadabilityItem(
                "weapon_attack_affordance",
                new[] { "Echo Blade Silhouette" },
                captures,
                "Blade silhouette is the current attack affordance in the generated primitive scene.");
            var objective = EvaluateReadabilityItem(
                "objective",
                new[] { "Ember Altar", "Claim Gate" },
                captures,
                "Gate A objective is represented by the altar path and claim gate.");
            var lootReward = EvaluateReadabilityItem(
                "loot_reward",
                new[] { "Claim Ready E Badge" },
                captures,
                "The claim/reward badge exists in scene but starts inactive until gameplay reaches gate-claim state.");
            var room = EvaluateReadabilityItem(
                "arpg_room_framing",
                new[] { "Block Diorama Terrain" },
                captures,
                "Room framing should show an ARPG-style fixed-angle combat space.");

            var allShown = player.shown_in_capture
                && enemy.shown_in_capture
                && weapon.shown_in_capture
                && objective.shown_in_capture
                && lootReward.shown_in_capture
                && room.shown_in_capture;

            return new GameplayReadability
            {
                status = allShown ? "passed" : "needs_product_review",
                note = allShown
                    ? "All product-facing readability checklist items are visible in the deterministic captures."
                    : "One or more product-facing readability checklist items are not visible in the deterministic captures; see item statuses.",
                player = player,
                enemy = enemy,
                weapon_attack_affordance = weapon,
                objective = objective,
                loot_reward = lootReward,
                arpg_room_framing = room
            };
        }

        private static ReadabilityItem EvaluateReadabilityItem(
            string id,
            string[] objectNames,
            CaptureSpec[] captures,
            string note)
        {
            var present = true;
            var shown = true;
            var captureIds = new List<string>();

            foreach (var objectName in objectNames)
            {
                var sceneObject = FindSceneObject(objectName);
                if (sceneObject == null)
                {
                    present = false;
                    shown = false;
                    continue;
                }

                var objectShown = false;
                foreach (var capture in captures)
                {
                    if (IsVisible(sceneObject, capture.camera))
                    {
                        objectShown = true;
                        if (!captureIds.Contains(capture.id))
                        {
                            captureIds.Add(capture.id);
                        }
                    }
                }

                if (!objectShown)
                {
                    shown = false;
                }
            }

            return new ReadabilityItem
            {
                id = id,
                present_in_scene = present,
                shown_in_capture = present && shown,
                status = !present ? "missing_scene_object" : shown ? "shown" : "not_visible_in_capture",
                evidence_objects = objectNames,
                visible_capture_ids = captureIds.ToArray(),
                note = note
            };
        }

        private static bool IsVisible(GameObject gameObject, Camera camera)
        {
            if (gameObject == null || camera == null || !gameObject.activeInHierarchy)
            {
                return false;
            }

            var renderers = gameObject.GetComponentsInChildren<Renderer>(false);
            foreach (var renderer in renderers)
            {
                if (renderer.enabled && renderer.gameObject.activeInHierarchy && IsBoundsVisible(renderer.bounds, camera))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsBoundsVisible(Bounds bounds, Camera camera)
        {
            var min = bounds.min;
            var max = bounds.max;
            var points = new[]
            {
                bounds.center,
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };

            foreach (var point in points)
            {
                var viewport = camera.WorldToViewportPoint(point);
                if (viewport.z > camera.nearClipPlane
                    && viewport.x >= 0f
                    && viewport.x <= 1f
                    && viewport.y >= 0f
                    && viewport.y <= 1f)
                {
                    return true;
                }
            }

            return false;
        }

        private static GameObject FindSceneObject(string name)
        {
            foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.name == name && candidate.scene.IsValid())
                {
                    return candidate;
                }
            }

            return null;
        }

        private static Camera FindCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            }

            if (camera == null)
            {
                throw new InvalidOperationException("Cannot capture Gate A evidence because no camera exists.");
            }

            return camera;
        }

        private static Camera CreateObjectiveCamera(Camera source)
        {
            var cameraObject = new GameObject("Gate A Evidence Objective Camera")
            {
                hideFlags = HideFlags.DontSave
            };
            cameraObject.transform.position = new Vector3(2.25f, 4.85f, -4.35f);
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(2.35f, 0.8f, 0f) - cameraObject.transform.position, Vector3.up);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.65f;
            camera.clearFlags = source.clearFlags;
            camera.backgroundColor = source.backgroundColor;
            camera.nearClipPlane = source.nearClipPlane;
            camera.farClipPlane = source.farClipPlane;
            return camera;
        }

        private static void CaptureCamera(Camera camera, string outputPath)
        {
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(CaptureWidth, CaptureHeight, 24);
            var texture = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
                texture.Apply();
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static string GetOutputDirectory()
        {
            var configuredDirectory = Environment.GetEnvironmentVariable("FOURFOLD_EVIDENCE_DIR");
            if (!string.IsNullOrWhiteSpace(configuredDirectory))
            {
                return configuredDirectory;
            }

            return Path.Combine(Path.GetTempPath(), "fourfold-evidence-harness");
        }

        private static string GetMetadata(string environmentVariable, string repoRoot, params string[] gitArgs)
        {
            var configuredValue = Environment.GetEnvironmentVariable(environmentVariable);
            if (!string.IsNullOrWhiteSpace(configuredValue))
            {
                return configuredValue.Trim();
            }

            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = string.Join(" ", gitArgs),
                        WorkingDirectory = repoRoot,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                process.Start();
                var value = process.StandardOutput.ReadToEnd().Trim();
                if (!process.WaitForExit(3000))
                {
                    process.Kill();
                    return string.Empty;
                }

                return process.ExitCode == 0 ? value : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private sealed class CaptureSpec
        {
            public string id;
            public string purpose;
            public string file_name;
            public Camera camera;
        }

        [Serializable]
        private sealed class EvidenceReport
        {
            public string schema_version;
            public string generated_at_utc;
            public string run_status;
            public string project_path;
            public string evidence_directory;
            public string git_commit;
            public string git_branch;
            public string unity_version;
            public string unity_editor_path;
            public string scene_path;
            public SceneValidation scene_validation;
            public BuildTargetEvidence[] build_target_support;
            public CaptureRecord[] captures;
            public GameplayReadability gameplay_readability;
            public ConsoleErrorScan console_error_scan;
            public string error;
        }

        [Serializable]
        private sealed class SceneValidation
        {
            public string status;
            public string message;
        }

        [Serializable]
        private sealed class BuildTargetEvidence
        {
            public string target;
            public string unity_build_target;
            public bool supported;
            public string note;
        }

        [Serializable]
        private sealed class CaptureRecord
        {
            public string id;
            public string purpose;
            public string path;
            public int width;
            public int height;
            public string camera_name;
            public Float3 camera_position;
            public Float3 camera_rotation_euler;
            public bool orthographic;
            public float orthographic_size;
            public bool non_empty;
            public long size_bytes;
        }

        [Serializable]
        private sealed class GameplayReadability
        {
            public string status;
            public string note;
            public ReadabilityItem player;
            public ReadabilityItem enemy;
            public ReadabilityItem weapon_attack_affordance;
            public ReadabilityItem objective;
            public ReadabilityItem loot_reward;
            public ReadabilityItem arpg_room_framing;

            public static GameplayReadability Empty()
            {
                return new GameplayReadability
                {
                    status = "not_checked",
                    note = "Readability checklist has not run yet.",
                    player = ReadabilityItem.Empty("player"),
                    enemy = ReadabilityItem.Empty("enemy"),
                    weapon_attack_affordance = ReadabilityItem.Empty("weapon_attack_affordance"),
                    objective = ReadabilityItem.Empty("objective"),
                    loot_reward = ReadabilityItem.Empty("loot_reward"),
                    arpg_room_framing = ReadabilityItem.Empty("arpg_room_framing")
                };
            }
        }

        [Serializable]
        private sealed class ReadabilityItem
        {
            public string id;
            public bool present_in_scene;
            public bool shown_in_capture;
            public string status;
            public string[] evidence_objects;
            public string[] visible_capture_ids;
            public string note;

            public static ReadabilityItem Empty(string id)
            {
                return new ReadabilityItem
                {
                    id = id,
                    present_in_scene = false,
                    shown_in_capture = false,
                    status = "not_checked",
                    evidence_objects = new string[0],
                    visible_capture_ids = new string[0],
                    note = "Not checked."
                };
            }
        }

        [Serializable]
        private sealed class ConsoleErrorScan
        {
            public string status;
            public string unity_log_path;
            public int error_count;
            public string[] sample_lines;
            public int unity_exit_code;
        }

        [Serializable]
        private sealed class Float3
        {
            public float x;
            public float y;
            public float z;

            public static Float3 From(Vector3 value)
            {
                return new Float3
                {
                    x = value.x,
                    y = value.y,
                    z = value.z
                };
            }
        }
    }
}
