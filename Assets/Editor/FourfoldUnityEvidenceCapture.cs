using System;
using System.IO;
using FourfoldEchoes.Product;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldUnityEvidenceCapture
    {
        private const int CaptureWidth = 1280;
        private const int CaptureHeight = 800;

        public static void CaptureGateA()
        {
            FourfoldUnitySpikeBuilder.BuildAndValidate();

            var camera = FindCamera();
            var outputDirectory = GetOutputDirectory();
            Directory.CreateDirectory(outputDirectory);
            var outputPath = Path.Combine(outputDirectory, "gate-a-camera.png");

            CaptureCamera(camera, outputPath);
            Debug.Log($"FOURFOLD Gate A camera evidence captured: {outputPath}");
        }

        public static void CaptureD020Slice()
        {
            FourfoldD020SliceSceneBuilder.BuildAndValidate();

            var camera = FindCamera();
            var outputDirectory = GetOutputDirectory();
            Directory.CreateDirectory(outputDirectory);
            var shortcutNode = FindSceneObject("D020 Exploration Tool Node")?.GetComponent<ExplorationNode>();
            var shortcutRoute = FindSceneObject("D020 Shortcut Route");
            var rewardLensNode = FindSceneObject("D020 Reward Lens Node")?.GetComponent<ExplorationNode>();
            var rewardLensResponse = FindSceneObject("D020 Reward Lens Response");
            var player = UnityEngine.Object.FindFirstObjectByType<D020PlayerController>();
            var enemy = UnityEngine.Object.FindFirstObjectByType<D020EnemyDummy>();
            var reward = UnityEngine.Object.FindFirstObjectByType<D020RelicReward>();
            var progressSave = UnityEngine.Object.FindFirstObjectByType<D020ProgressSave>();
            var hud = UnityEngine.Object.FindFirstObjectByType<D020HudController>();

            var outputPath = Path.Combine(outputDirectory, "d020-slice-camera.png");
            SetNodeSolved(shortcutNode, shortcutRoute, false);
            SetNodeSolved(rewardLensNode, rewardLensResponse, false);
            CaptureCamera(camera, outputPath);
            SetNodeSolved(shortcutNode, shortcutRoute, true);
            SetNodeSolved(rewardLensNode, rewardLensResponse, false);
            CaptureCameraFromPose(
                camera,
                Path.Combine(outputDirectory, "d020-tool-node-read.png"),
                new Vector3(2.9f, 7.1f, -5.2f),
                new Vector3(-2.35f, 0.35f, -0.1f),
                3.9f);
            SetNodeSolved(rewardLensNode, rewardLensResponse, true);
            CaptureCameraFromPose(
                camera,
                Path.Combine(outputDirectory, "d020-reward-lens-read.png"),
                new Vector3(6.35f, 6.9f, -5.30f),
                new Vector3(2.66f, 0.42f, -1.20f),
                3.55f);
            CaptureCameraFromPose(
                camera,
                Path.Combine(outputDirectory, "d020-reward-read.png"),
                new Vector3(5.6f, 6.4f, -4.8f),
                new Vector3(2.1f, 0.45f, -1.0f),
                3.8f);
            if (player != null && enemy != null)
            {
                enemy.ResetEnemy();
                player.ResetForSmoke(enemy.transform.position + new Vector3(0f, 0f, -0.95f));
                player.TryAttack();
                CaptureCameraFromPose(
                    camera,
                Path.Combine(outputDirectory, "d020-playable-attack-read.png"),
                    new Vector3(4.9f, 6.25f, -4.55f),
                    new Vector3(1.0f, 0.45f, -0.05f),
                    3.25f);
            }
            CaptureHudRewardSaveMoment(
                camera,
                Path.Combine(outputDirectory, "d020-hud-reward-save.png"),
                shortcutNode,
                shortcutRoute,
                rewardLensNode,
                rewardLensResponse,
                reward,
                enemy,
                progressSave,
                hud);

            Debug.Log($"FOURFOLD D-020 vertical slice camera evidence captured: {outputPath}");
        }

        private static void SetNodeSolved(ExplorationNode node, GameObject fallbackResponse, bool solved)
        {
            if (node != null)
            {
                node.SetSolved(solved);
                return;
            }

            if (fallbackResponse != null)
            {
                fallbackResponse.SetActive(solved);
            }
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
                throw new InvalidOperationException("Cannot capture scene evidence because no camera exists.");
            }

            return camera;
        }

        private static GameObject FindSceneObject(string name)
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var found = FindInChildren(roots[i].transform, name);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindInChildren(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindInChildren(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static string GetOutputDirectory()
        {
            var configuredDirectory = Environment.GetEnvironmentVariable("FOURFOLD_EVIDENCE_DIR");
            if (!string.IsNullOrWhiteSpace(configuredDirectory))
            {
                return configuredDirectory;
            }

            return Path.Combine(Path.GetTempPath(), "fourfold-echoes-evidence");
        }

        private static void CaptureHudRewardSaveMoment(
            Camera camera,
            string outputPath,
            ExplorationNode shortcutNode,
            GameObject shortcutRoute,
            ExplorationNode rewardLensNode,
            GameObject rewardLensResponse,
            D020RelicReward reward,
            D020EnemyDummy enemy,
            D020ProgressSave progressSave,
            D020HudController hud)
        {
            if (enemy != null)
            {
                enemy.ResetEnemy();
                while (!enemy.IsDefeated)
                {
                    enemy.TakeHit(1);
                }
            }

            SetNodeSolved(shortcutNode, shortcutRoute, true);
            SetNodeSolved(rewardLensNode, rewardLensResponse, true);

            if (reward != null)
            {
                reward.SetCollected(true);
            }

            if (progressSave != null)
            {
                progressSave.overrideFilePath = Path.Combine(Path.GetTempPath(), "fourfold-d020-capture-progress.json");
                if (!progressSave.SaveNow())
                {
                    throw new InvalidOperationException("D-020 HUD reward/save capture could not write progress state.");
                }
            }

            if (hud != null)
            {
                hud.showHud = true;
                hud.RefreshNow();
            }

            CaptureCameraFromPose(
                camera,
                outputPath,
                new Vector3(5.85f, 6.75f, -4.95f),
                new Vector3(2.2f, 0.55f, -1.05f),
                3.9f,
                hud);
        }

        private static void CaptureCamera(Camera camera, string outputPath)
        {
            CaptureCamera(camera, outputPath, null);
        }

        private static void CaptureCamera(Camera camera, string outputPath, D020HudController hudOverlay)
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
                if (hudOverlay != null)
                {
                    DrawHudOverlay(texture, hudOverlay);
                }

                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void CaptureCameraFromPose(Camera camera, string outputPath, Vector3 position, Vector3 target, float orthographicSize)
        {
            CaptureCameraFromPose(camera, outputPath, position, target, orthographicSize, null);
        }

        private static void CaptureCameraFromPose(Camera camera, string outputPath, Vector3 position, Vector3 target, float orthographicSize, D020HudController hudOverlay)
        {
            var previousPosition = camera.transform.position;
            var previousRotation = camera.transform.rotation;
            var previousOrthographicSize = camera.orthographicSize;

            try
            {
                camera.transform.position = position;
                camera.transform.rotation = Quaternion.LookRotation(target - position, Vector3.up);
                if (camera.orthographic)
                {
                    camera.orthographicSize = orthographicSize;
                }
                CaptureCamera(camera, outputPath, hudOverlay);
            }
            finally
            {
                camera.transform.position = previousPosition;
                camera.transform.rotation = previousRotation;
                camera.orthographicSize = previousOrthographicSize;
            }
        }

        private static void DrawHudOverlay(Texture2D texture, D020HudController hud)
        {
            hud.RefreshNow();
            FillRect(texture, 18, 18, 302, 132, new Color32(10, 14, 20, 224));
            DrawRect(texture, 18, 18, 302, 132, new Color32(230, 204, 124, 255));
            DrawText(texture, 34, 32, "D-020 ROOM", new Color32(240, 214, 132, 255), 3);
            DrawText(texture, 34, 62, hud.ToolRead, new Color32(234, 240, 248, 255), 2);
            DrawText(texture, 34, 84, hud.RewardRead, new Color32(234, 240, 248, 255), 2);
            DrawText(texture, 34, 106, hud.ProgressRead, new Color32(234, 240, 248, 255), 2);
            DrawText(texture, 34, 128, hud.PromptRead, new Color32(176, 224, 255, 255), 2);
            texture.Apply();
        }

        private static void FillRect(Texture2D texture, int x, int yTop, int width, int height, Color32 color)
        {
            for (var y = 0; y < height; y++)
            {
                for (var xOffset = 0; xOffset < width; xOffset++)
                {
                    SetPixelTopLeft(texture, x + xOffset, yTop + y, color);
                }
            }
        }

        private static void DrawRect(Texture2D texture, int x, int yTop, int width, int height, Color32 color)
        {
            for (var xOffset = 0; xOffset < width; xOffset++)
            {
                SetPixelTopLeft(texture, x + xOffset, yTop, color);
                SetPixelTopLeft(texture, x + xOffset, yTop + height - 1, color);
            }

            for (var y = 0; y < height; y++)
            {
                SetPixelTopLeft(texture, x, yTop + y, color);
                SetPixelTopLeft(texture, x + width - 1, yTop + y, color);
            }
        }

        private static void DrawText(Texture2D texture, int x, int yTop, string text, Color32 color, int scale)
        {
            var cursor = x;
            var upper = (text ?? string.Empty).ToUpperInvariant();
            for (var index = 0; index < upper.Length; index++)
            {
                var glyph = GetGlyph(upper[index]);
                if (glyph == null)
                {
                    cursor += 4 * scale;
                    continue;
                }

                for (var row = 0; row < glyph.Length; row++)
                {
                    for (var column = 0; column < glyph[row].Length; column++)
                    {
                        if (glyph[row][column] != '1')
                        {
                            continue;
                        }

                        FillRect(texture, cursor + column * scale, yTop + row * scale, scale, scale, color);
                    }
                }

                cursor += (glyph[0].Length + 1) * scale;
            }
        }

        private static string[] GetGlyph(char character)
        {
            switch (character)
            {
                case 'A': return new[] { "010", "101", "101", "111", "101" };
                case 'C': return new[] { "111", "100", "100", "100", "111" };
                case 'D': return new[] { "110", "101", "101", "101", "110" };
                case 'E': return new[] { "111", "100", "110", "100", "111" };
                case 'G': return new[] { "111", "100", "101", "101", "111" };
                case 'I': return new[] { "111", "010", "010", "010", "111" };
                case 'L': return new[] { "100", "100", "100", "100", "111" };
                case 'M': return new[] { "101", "111", "111", "101", "101" };
                case 'O': return new[] { "111", "101", "101", "101", "111" };
                case 'P': return new[] { "110", "101", "110", "100", "100" };
                case 'R': return new[] { "110", "101", "110", "101", "101" };
                case 'S': return new[] { "111", "100", "111", "001", "111" };
                case 'T': return new[] { "111", "010", "010", "010", "010" };
                case 'U': return new[] { "101", "101", "101", "101", "111" };
                case 'Y': return new[] { "101", "101", "010", "010", "010" };
                case '0': return new[] { "111", "101", "101", "101", "111" };
                case '1': return new[] { "010", "110", "010", "010", "111" };
                case '2': return new[] { "111", "001", "111", "100", "111" };
                case '-': return new[] { "000", "000", "111", "000", "000" };
                case '%': return new[] { "101", "001", "010", "100", "101" };
                case ' ': return new[] { "000", "000", "000", "000", "000" };
                default: return null;
            }
        }

        private static void SetPixelTopLeft(Texture2D texture, int x, int yTop, Color32 color)
        {
            if (x < 0 || x >= texture.width || yTop < 0 || yTop >= texture.height)
            {
                return;
            }

            texture.SetPixel(x, texture.height - 1 - yTop, color);
        }
    }
}
