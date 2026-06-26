using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldProductionP3ModelPackVerifier
    {
        private const string PrefabFolder = "Assets/Prefabs/Production/P3";
        private const string ModelFolder = "Assets/Art/Production/P3";
        private const int ExpectedPrefabCount = 28;

        public static void VerifyP3ModelPack()
        {
            if (!Directory.Exists(ModelFolder))
            {
                throw new InvalidOperationException($"P3 model folder is missing: {ModelFolder}");
            }

            if (!Directory.Exists(PrefabFolder))
            {
                throw new InvalidOperationException($"P3 prefab folder is missing: {PrefabFolder}");
            }

            var prefabPaths = Directory.GetFiles(PrefabFolder, "*.prefab", SearchOption.TopDirectoryOnly);
            Array.Sort(prefabPaths, StringComparer.Ordinal);
            if (prefabPaths.Length != ExpectedPrefabCount)
            {
                throw new InvalidOperationException($"P3 prefab count mismatch: expected {ExpectedPrefabCount}, found {prefabPaths.Length}");
            }

            foreach (var path in prefabPaths)
            {
                VerifyPrefab(path.Replace('\\', '/'));
            }

            Debug.Log($"FOURFOLD P3 model pack verifier passed: {prefabPaths.Length} prefabs validated.");
        }

        private static void VerifyPrefab(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException($"P3 prefab failed to load: {prefabPath}");
            }

            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"P3 prefab has no renderers: {prefabPath}");
            }

            foreach (var renderer in renderers)
            {
                if (renderer.sharedMaterial == null)
                {
                    throw new InvalidOperationException($"P3 prefab has missing material: {prefabPath}/{renderer.name}");
                }
            }

            var meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
            if (meshFilters.Length == 0)
            {
                throw new InvalidOperationException($"P3 prefab has no mesh filters: {prefabPath}");
            }

            foreach (var meshFilter in meshFilters)
            {
                if (meshFilter.sharedMesh == null)
                {
                    throw new InvalidOperationException($"P3 prefab has missing mesh: {prefabPath}/{meshFilter.name}");
                }
            }

            var bounds = CalculateBounds(renderers);
            if (bounds.size.y <= 0.05f || bounds.size.y > 8f || bounds.size.x > 8f || bounds.size.z > 8f)
            {
                throw new InvalidOperationException($"P3 prefab bounds look wrong: {prefabPath} {bounds.size}");
            }
        }

        private static Bounds CalculateBounds(Renderer[] renderers)
        {
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }
    }
}
