using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldMassAssetImporter
    {
        private const string ManifestPath = "asset_manifest.json";
        private const string PrefabRoot = "Assets/Prefabs/MassProduction";
        private const string ReportPath = "artifacts/Reports/mass-asset-unity-import.json";

        [MenuItem("FOURFOLD/Assets/Import Mass Asset Contract Batch")]
        public static void ImportMassAssetContractBatch()
        {
            if (!File.Exists(ManifestPath))
            {
                throw new FileNotFoundException($"Mass asset manifest is missing: {ManifestPath}");
            }

            AssetDatabase.Refresh();

            var manifest = JsonUtility.FromJson<MassAssetManifest>(File.ReadAllText(ManifestPath));
            if (manifest == null || manifest.assets == null || manifest.assets.Length == 0)
            {
                throw new InvalidOperationException("Mass asset manifest contains no assets.");
            }

            var created = 0;
            var skipped = 0;
            var errors = new List<string>();

            foreach (var asset in manifest.assets)
            {
                if (asset == null || asset.kind != "3d")
                {
                    skipped += 1;
                    continue;
                }

                try
                {
                    Import3dAsset(asset);
                    created += 1;
                }
                catch (Exception error)
                {
                    errors.Add($"{asset.contract_name}: {error.Message}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            WriteReport(created, skipped, errors);

            if (errors.Count > 0)
            {
                throw new InvalidOperationException($"Mass asset import failed for {errors.Count} assets. See {ReportPath}");
            }

            Debug.Log($"FOURFOLD mass asset contract imported: {created} prefabs created, {skipped} non-3D assets skipped.");
        }

        private static void Import3dAsset(MassAsset asset)
        {
            if (asset.final_files == null ||
                string.IsNullOrWhiteSpace(asset.final_files.lod0) ||
                string.IsNullOrWhiteSpace(asset.final_files.lod1) ||
                string.IsNullOrWhiteSpace(asset.final_files.lod2))
            {
                throw new InvalidOperationException("LOD0/1/2 FBX paths are required.");
            }

            var lod0 = LoadModel(asset.final_files.lod0);
            var lod1 = LoadModel(asset.final_files.lod1);
            var lod2 = LoadModel(asset.final_files.lod2);

            var root = new GameObject(asset.contract_name);
            try
            {
                var instances = new[]
                {
                    InstantiateLod(lod0, root.transform, "LOD0"),
                    InstantiateLod(lod1, root.transform, "LOD1"),
                    InstantiateLod(lod2, root.transform, "LOD2")
                };

                var lodGroup = root.AddComponent<LODGroup>();
                lodGroup.SetLODs(new[]
                {
                    new LOD(0.60f, GetRenderers(instances[0])),
                    new LOD(0.25f, GetRenderers(instances[1])),
                    new LOD(0.08f, GetRenderers(instances[2]))
                });
                lodGroup.RecalculateBounds();

                AddCollider(root, asset.category);
                ApplyStaticFlags(root, asset.category);

                var prefabPath = $"{PrefabRoot}/{SafeSegment(asset.category)}/{asset.contract_name}.prefab";
                Directory.CreateDirectory(Path.GetDirectoryName(prefabPath) ?? PrefabRoot);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static GameObject LoadModel(string path)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null)
            {
                throw new InvalidOperationException($"Model could not be loaded by Unity: {path}");
            }

            return model;
        }

        private static GameObject InstantiateLod(GameObject model, Transform parent, string name)
        {
            var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"Could not instantiate model for {name}.");
            }

            instance.name = name;
            instance.transform.SetParent(parent, false);
            return instance;
        }

        private static Renderer[] GetRenderers(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                throw new InvalidOperationException($"{root.name} has no renderers.");
            }

            return renderers;
        }

        private static void AddCollider(GameObject root, string category)
        {
            switch (category)
            {
                case "player":
                case "npc":
                case "enemy":
                case "elite":
                case "boss":
                {
                    var capsule = root.AddComponent<CapsuleCollider>();
                    capsule.height = category == "boss" ? 2.4f : 1.4f;
                    capsule.radius = category == "boss" ? 0.7f : 0.35f;
                    capsule.center = new Vector3(0f, capsule.height * 0.5f, 0f);
                    return;
                }

                case "equipment":
                    return;

                default:
                {
                    var box = root.AddComponent<BoxCollider>();
                    box.size = Vector3.one;
                    box.center = new Vector3(0f, 0.5f, 0f);
                    return;
                }
            }
        }

        private static void ApplyStaticFlags(GameObject root, string category)
        {
            if (category != "environment" && category != "prop")
            {
                GameObjectUtility.SetStaticEditorFlags(root, 0);
                return;
            }

            GameObjectUtility.SetStaticEditorFlags(
                root,
                StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
        }

        private static void WriteReport(int created, int skipped, List<string> errors)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? ".");
            var json = "{"
                + $"\"created_prefabs\":{created},"
                + $"\"skipped_non_3d\":{skipped},"
                + $"\"errors\":[{string.Join(",", errors.ConvertAll(error => $"\"{Escape(error)}\""))}]"
                + "}";
            File.WriteAllText(ReportPath, json + Environment.NewLine);
        }

        private static string SafeSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "uncategorized";
            }

            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return value;
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r");
        }

        [Serializable]
        private sealed class MassAssetManifest
        {
            public MassAsset[] assets;
        }

        [Serializable]
        private sealed class MassAsset
        {
            public string asset_id;
            public string contract_name;
            public string category;
            public string kind;
            public FinalFiles final_files;
        }

        [Serializable]
        private sealed class FinalFiles
        {
            public string lod0;
            public string lod1;
            public string lod2;
        }
    }
}
