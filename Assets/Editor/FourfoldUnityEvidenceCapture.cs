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
            var secondNode = FindSceneObject("D020 Second Exploration Tool Node")?.GetComponent<ExplorationNode>();
            var secondRoute = FindSceneObject("D020 Second Gimmick Route");

            var outputPath = Path.Combine(outputDirectory, "d020-slice-camera.png");
            if (shortcutNode != null)
            {
                shortcutNode.SetSolved(false);
            }
            else if (shortcutRoute != null)
            {
                shortcutRoute.SetActive(false);
            }
            CaptureCamera(camera, outputPath);
            if (shortcutNode != null)
            {
                shortcutNode.SetSolved(true);
            }
            else if (shortcutRoute != null)
            {
                shortcutRoute.SetActive(true);
            }

            if (secondNode != null)
            {
                secondNode.SetSolved(false);
            }
            else if (secondRoute != null)
            {
                secondRoute.SetActive(false);
            }

            CaptureCameraFromPose(
                camera,
                Path.Combine(outputDirectory, "d020-tool-node-read.png"),
                new Vector3(0.8f, 9.4f, -9.4f),
                new Vector3(-5.4f, 0.45f, -2.5f),
                5.6f);
            CaptureCameraFromPose(
                camera,
                Path.Combine(outputDirectory, "d020-reward-read.png"),
                new Vector3(13.8f, 9.4f, -0.5f),
                new Vector3(8.5f, 0.55f, 5.2f),
                5.8f);
            CaptureCameraFromPose(
                camera,
                Path.Combine(outputDirectory, "d020-combat-read.png"),
                new Vector3(9.0f, 10.2f, -7.3f),
                new Vector3(2.9f, 0.5f, 1.2f),
                6.2f);

            CaptureCameraFromPose(
                camera,
                Path.Combine(outputDirectory, "d020-second-tool-node-read.png"),
                new Vector3(14.4f, 9.4f, -9.7f),
                new Vector3(8.4f, 0.5f, -4.4f),
                5.2f);

            if (secondNode != null)
            {
                secondNode.SetSolved(true);
            }
            else if (secondRoute != null)
            {
                secondRoute.SetActive(true);
            }

            CaptureCameraFromPose(
                camera,
                Path.Combine(outputDirectory, "d020-second-reward-read.png"),
                new Vector3(15.9f, 9.6f, -10.1f),
                new Vector3(10.4f, 0.6f, -5.1f),
                4.8f);

            Debug.Log($"FOURFOLD D-020 vertical slice camera evidence captured: {outputPath}");
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
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void CaptureCameraFromPose(Camera camera, string outputPath, Vector3 position, Vector3 target, float orthographicSize)
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
                CaptureCamera(camera, outputPath);
            }
            finally
            {
                camera.transform.position = previousPosition;
                camera.transform.rotation = previousRotation;
                camera.orthographicSize = previousOrthographicSize;
            }
        }
    }
}
