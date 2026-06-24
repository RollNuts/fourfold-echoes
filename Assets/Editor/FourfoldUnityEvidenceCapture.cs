using System;
using System.IO;
using UnityEditor;
using UnityEngine;

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
    }
}
