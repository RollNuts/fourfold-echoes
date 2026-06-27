using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldGeneratedModelPackImporter
    {
        private const string ManifestPath = "artifacts/Reports/fourfold-model-pack.json";

        [MenuItem("FOURFOLD/Assets/Import Generated Model Pack")]
        public static void ImportGeneratedModelPack()
        {
            if (!File.Exists(ManifestPath))
            {
                throw new FileNotFoundException($"Generated model manifest is missing: {ManifestPath}");
            }

            AssetDatabase.Refresh();

            var manifest = JsonUtility.FromJson<ModelPackManifest>(File.ReadAllText(ManifestPath));
            if (manifest == null || manifest.assets == null || manifest.assets.Length == 0)
            {
                throw new InvalidOperationException("Generated model manifest contains no assets.");
            }

            var created = 0;
            foreach (var asset in manifest.assets)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(asset.model_file);
                if (model == null)
                {
                    throw new InvalidOperationException($"Generated model could not be loaded by Unity: {asset.model_file}");
                }

                var wrapper = new GameObject(asset.name);
                var visual = PrefabUtility.InstantiatePrefab(model) as GameObject;
                if (visual == null)
                {
                    UnityEngine.Object.DestroyImmediate(wrapper);
                    throw new InvalidOperationException($"Could not instantiate generated model: {asset.model_file}");
                }

                visual.name = "Visual";
                visual.transform.SetParent(wrapper.transform, false);
                AddSimpleCollider(wrapper, asset);
                ApplyStaticFlags(wrapper, asset);

                var prefabFolder = Path.GetDirectoryName(asset.unity_prefab);
                if (string.IsNullOrEmpty(prefabFolder))
                {
                    UnityEngine.Object.DestroyImmediate(wrapper);
                    throw new InvalidOperationException($"Generated asset has invalid prefab path: {asset.unity_prefab}");
                }

                Directory.CreateDirectory(prefabFolder);
                PrefabUtility.SaveAsPrefabAsset(wrapper, asset.unity_prefab);
                UnityEngine.Object.DestroyImmediate(wrapper);
                created += 1;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"FOURFOLD generated model pack imported: {created} prefabs created from {ManifestPath}");
        }

        private static void AddSimpleCollider(GameObject root, ModelPackAsset asset)
        {
            var profile = string.IsNullOrEmpty(asset.collision_profile) ? "box_set_dressing" : asset.collision_profile;
            var size = ColliderSize(asset);
            switch (profile)
            {
                case "capsule_actor":
                {
                    var capsule = root.AddComponent<CapsuleCollider>();
                    capsule.height = Mathf.Max(size.y, 0.8f);
                    capsule.radius = Mathf.Max(Mathf.Max(size.x, size.z) * 0.28f, 0.18f);
                    capsule.center = new Vector3(0f, capsule.height * 0.5f, 0f);
                    return;
                }

                case "boss_capsule":
                {
                    var capsule = root.AddComponent<CapsuleCollider>();
                    capsule.height = Mathf.Max(size.y, 1.2f);
                    capsule.radius = Mathf.Max(Mathf.Max(size.x, size.z) * 0.32f, 0.5f);
                    capsule.center = new Vector3(0f, capsule.height * 0.5f, 0f);
                    return;
                }

                case "trigger_pickup":
                {
                    var box = root.AddComponent<BoxCollider>();
                    box.size = new Vector3(Mathf.Max(size.x, 0.5f), Mathf.Max(size.y, 0.5f), Mathf.Max(size.z, 0.5f));
                    box.center = new Vector3(0f, box.size.y * 0.5f, 0f);
                    box.isTrigger = true;
                    return;
                }

                case "no_collider_equipment":
                case "no_collider_visual_detail":
                    return;

                case "box_floor_thin":
                {
                    var box = root.AddComponent<BoxCollider>();
                    box.size = new Vector3(Mathf.Max(size.x, 0.25f), Mathf.Clamp(size.y, 0.08f, 0.35f), Mathf.Max(size.z, 0.25f));
                    box.center = new Vector3(0f, box.size.y * 0.5f, 0f);
                    return;
                }

                case "box_boundary":
                {
                    var box = root.AddComponent<BoxCollider>();
                    box.size = new Vector3(Mathf.Max(size.x, 0.35f), Mathf.Max(size.y, 0.35f), Mathf.Max(size.z, 0.35f));
                    box.center = new Vector3(0f, box.size.y * 0.5f, 0f);
                    return;
                }

                case "box_interactable":
                {
                    var box = root.AddComponent<BoxCollider>();
                    box.size = new Vector3(Mathf.Max(size.x, 0.55f), Mathf.Max(size.y, 0.55f), Mathf.Max(size.z, 0.55f));
                    box.center = new Vector3(0f, box.size.y * 0.5f, 0f);
                    return;
                }

                case "box_set_dressing":
                {
                    var box = root.AddComponent<BoxCollider>();
                    box.size = new Vector3(Mathf.Max(size.x, 0.35f), Mathf.Max(size.y, 0.35f), Mathf.Max(size.z, 0.35f));
                    box.center = new Vector3(0f, box.size.y * 0.5f, 0f);
                    return;
                }

                default:
                    throw new InvalidOperationException($"{asset.name} has unsupported collision_profile={profile}");
            }
        }

        private static Vector3 ColliderSize(ModelPackAsset asset)
        {
            var x = asset.bounds_m != null && asset.bounds_m.x > 0f ? asset.bounds_m.x : 0.9f;
            var y = asset.bounds_m != null && asset.bounds_m.z > 0f ? asset.bounds_m.z : 0.8f;
            var z = asset.footprint_m != null && asset.footprint_m.z > 0f
                ? asset.footprint_m.z
                : asset.bounds_m != null && asset.bounds_m.y > 0f
                    ? asset.bounds_m.y
                    : 0.9f;
            return new Vector3(x, y, z);
        }

        private static void ApplyStaticFlags(GameObject root, ModelPackAsset asset)
        {
            if (!asset.static_hint)
            {
                GameObjectUtility.SetStaticEditorFlags(root, 0);
                return;
            }

            var flags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic;
            if (asset.nav_blocking)
            {
                flags |= StaticEditorFlags.NavigationStatic;
            }

            GameObjectUtility.SetStaticEditorFlags(root, flags);
        }

        [Serializable]
        private sealed class ModelPackManifest
        {
            public ModelPackAsset[] assets;
        }

        [Serializable]
        private sealed class ModelPackAsset
        {
            public string name;
            public string category;
            public string asset_kind;
            public string builder;
            public string style;
            public string area_code;
            public string collision_profile;
            public bool static_hint;
            public bool nav_blocking;
            public BoundsM bounds_m;
            public FootprintM footprint_m;
            public string model_file;
            public string unity_prefab;
        }

        [Serializable]
        private sealed class BoundsM
        {
            public float x;
            public float y;
            public float z;
        }

        [Serializable]
        private sealed class FootprintM
        {
            public float x;
            public float z;
        }
    }
}
