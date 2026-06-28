using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldMeshwrightPreviewPropVerifier
    {
        public const string SourceModelPath = "Assets/Art/Meshwright/Preview/GrasslandPropKit/Models/grassland_prop_kit_a01.fbx";
        public const string SourceBaseColorPath = "Assets/Art/Meshwright/Preview/GrasslandPropKit/Textures/T_grassland_prop_kit_a01_BaseColor.png";
        public const string SourceNormalPath = "Assets/Art/Meshwright/Preview/GrasslandPropKit/Textures/T_grassland_prop_kit_a01_Normal.png";
        public const string SourceMetallicRoughnessPath = "Assets/Art/Meshwright/Preview/GrasslandPropKit/Textures/T_grassland_prop_kit_a01_MetallicRoughness.png";
        public const string SourcePreviewImagePath = "Assets/Art/Meshwright/Preview/GrasslandPropKit/QA/grassland_prop_kit_a01_preview.png";
        public const string MeshQaPath = "Assets/Art/Meshwright/Preview/GrasslandPropKit/QA/grassland_prop_kit_a01_meshqa.json";
        public const string PreflightPath = "Assets/Art/Meshwright/Preview/GrasslandPropKit/QA/grassland_prop_kit_a01_preflight.json";
        public const string MaterialPath = "Assets/Art/Meshwright/Preview/GrasslandPropKit/Materials/M_grassland_prop_kit_a01.mat";
        public const string PreviewMaterialPath = "Assets/Art/Meshwright/Preview/GrasslandPropKit/Materials/M_grassland_prop_kit_a01_preview.mat";
        public const string PrefabPath = "Assets/Prefabs/Production/P1/FE_PROP_R01_MeshwrightGrasslandPropKit_01.prefab";
        public const string SceneObjectName = "PCS Meshwright R01 Prop Preview - FE_PROP_R01_MeshwrightGrasslandPropKit_01";

        [MenuItem("FOURFOLD/Assets/Build Meshwright Grassland Preview Prop")]
        public static void BuildGrasslandPreviewPrefab()
        {
            ValidateMeshwrightEvidence();
            EnsureFolders();
            AssetDatabase.Refresh();
            ConfigureTextureImporters();
            AssetDatabase.Refresh();

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(SourceModelPath);
            if (model == null)
            {
                throw new InvalidOperationException($"Meshwright preview model could not be loaded: {SourceModelPath}");
            }

            var wrapper = new GameObject("FE_PROP_R01_MeshwrightGrasslandPropKit_01");
            try
            {
                var material = CreatePreviewMaterial();
                var visual = PrefabUtility.InstantiatePrefab(model) as GameObject;
                if (visual == null)
                {
                    throw new InvalidOperationException($"Could not instantiate Meshwright preview model: {SourceModelPath}");
                }

                visual.name = "Visual";
                visual.transform.SetParent(wrapper.transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one;

                if (wrapper.GetComponentsInChildren<Renderer>(true).Length == 0)
                {
                    UnityEngine.Object.DestroyImmediate(visual);
                    BuildVisualFromMeshSubAssets(wrapper.transform, material);
                }

                foreach (var renderer in wrapper.GetComponentsInChildren<Renderer>(true))
                {
                    var materials = new Material[renderer.sharedMaterials.Length];
                    for (var index = 0; index < materials.Length; index++)
                    {
                        materials[index] = material;
                    }
                    renderer.sharedMaterials = materials;
                }

                ConfigureLodGroup(wrapper);
                AddBoundsCollider(wrapper);
                GameObjectUtility.SetStaticEditorFlags(wrapper, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);

                PrefabUtility.SaveAsPrefabAsset(wrapper, PrefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"FOURFOLD Meshwright preview prop built: {PrefabPath}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(wrapper);
            }
        }

        public static void BuildAndVerify()
        {
            FourfoldProductionCombatSliceSceneBuilder.BuildAndValidate();
            VerifySceneBinding();
            Debug.Log("FOURFOLD Meshwright grassland preview prop verified as preview-only ProductionCombatSlice set dressing.");
        }

        private static void VerifySceneBinding()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Meshwright preview prefab is missing: {PrefabPath}");
            }

            if (prefab.transform.Find("Visual") == null)
            {
                throw new InvalidOperationException("Meshwright preview prefab is missing its Visual child.");
            }
            if (prefab.GetComponent<BoxCollider>() == null)
            {
                throw new InvalidOperationException("Meshwright preview prefab is missing its set-dressing BoxCollider.");
            }
            if (prefab.GetComponent<LODGroup>() == null)
            {
                throw new InvalidOperationException("Meshwright preview prefab is missing its LODGroup.");
            }
            if (prefab.GetComponentsInChildren<Renderer>(true).Length == 0)
            {
                throw new InvalidOperationException("Meshwright preview prefab has no renderers.");
            }

            var scene = EditorSceneManager.OpenScene(FourfoldProductionCombatSliceSceneBuilder.ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new InvalidOperationException("Production combat scene could not be opened after Meshwright preview build.");
            }

            var sceneObject = FindSceneObject(SceneObjectName);
            if (sceneObject == null)
            {
                throw new InvalidOperationException($"Production combat scene is missing Meshwright preview object: {SceneObjectName}");
            }

            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(sceneObject);
            if (!string.Equals(path, PrefabPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Meshwright preview scene object is not bound to {PrefabPath}; actual={path}");
            }
        }

        private static void ValidateMeshwrightEvidence()
        {
            RequireFile(SourceModelPath);
            RequireFile(SourceBaseColorPath);
            RequireFile(SourceNormalPath);
            RequireFile(SourceMetallicRoughnessPath);
            RequireFile(SourcePreviewImagePath);
            RequireFile(MeshQaPath);
            RequireFile(PreflightPath);

            var meshQa = JsonUtility.FromJson<MeshQaRecord>(File.ReadAllText(MeshQaPath));
            if (meshQa == null || meshQa.summary == null || !meshQa.summary.ready_for_unity || meshQa.summary.fail != 0)
            {
                throw new InvalidOperationException("Meshwright MeshQA is not ready for Unity preview import.");
            }

            var preflight = JsonUtility.FromJson<PreflightRecord>(File.ReadAllText(PreflightPath));
            if (preflight == null || preflight.summary == null || !preflight.summary.ready_for_unity || preflight.summary.fail != 0)
            {
                throw new InvalidOperationException("Meshwright preflight is not ready for Unity preview import.");
            }
        }

        private static void RequireFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Required Meshwright preview asset is missing: {path}");
            }
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Art/Meshwright/Preview/GrasslandPropKit/Materials");
            Directory.CreateDirectory("Assets/Prefabs/Production/P1");
        }

        private static void ConfigureTextureImporters()
        {
            ConfigureTexture(SourceBaseColorPath, TextureImporterType.Default, true);
            ConfigureTexture(SourceNormalPath, TextureImporterType.NormalMap, false);
            ConfigureTexture(SourceMetallicRoughnessPath, TextureImporterType.Default, false);
            ConfigureTexture(SourcePreviewImagePath, TextureImporterType.Default, true);
        }

        private static void ConfigureTexture(string path, TextureImporterType textureType, bool srgb)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = textureType;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static Material CreatePreviewMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard")) { name = "M_grassland_prop_kit_a01" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            material.name = "M_grassland_prop_kit_a01";
            material.color = new Color(0.58f, 0.76f, 0.45f);
            AssignTexture(material, SourceBaseColorPath, "_MainTex");
            AssignTexture(material, SourceNormalPath, "_BumpMap");
            AssignTexture(material, SourceMetallicRoughnessPath, "_MetallicGlossMap");
            material.SetFloat("_Metallic", 0.0f);
            material.SetFloat("_Glossiness", 0.35f);
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void BuildVisualFromMeshSubAssets(Transform parent, Material material)
        {
            var visualRoot = new GameObject("Visual");
            visualRoot.transform.SetParent(parent, false);

            var meshes = Array.FindAll(
                AssetDatabase.LoadAllAssetRepresentationsAtPath(SourceModelPath),
                asset => asset is Mesh);
            if (meshes.Length == 0)
            {
                BuildVisualFromPreviewImage(visualRoot.transform);
                return;
            }

            for (var index = 0; index < meshes.Length; index++)
            {
                var mesh = (Mesh)meshes[index];
                var child = new GameObject(string.IsNullOrEmpty(mesh.name) ? $"MeshwrightPreviewMesh_{index:00}" : mesh.name);
                child.transform.SetParent(visualRoot.transform, false);
                child.AddComponent<MeshFilter>().sharedMesh = mesh;
                child.AddComponent<MeshRenderer>().sharedMaterial = material;
            }
        }

        private static void BuildVisualFromPreviewImage(Transform visualRoot)
        {
            var previewTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(SourcePreviewImagePath);
            if (previewTexture == null)
            {
                throw new InvalidOperationException("Meshwright preview model imported without geometry and its preview PNG could not be loaded.");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(PreviewMaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard")) { name = "M_grassland_prop_kit_a01_preview" };
                AssetDatabase.CreateAsset(material, PreviewMaterialPath);
            }
            material.name = "M_grassland_prop_kit_a01_preview";
            material.color = Color.white;
            material.SetTexture("_MainTex", previewTexture);
            material.SetFloat("_Metallic", 0.0f);
            material.SetFloat("_Glossiness", 0.22f);
            EditorUtility.SetDirty(material);

            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Meshwright Preview Ground Plaque";
            board.transform.SetParent(visualRoot, false);
            board.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            board.transform.localRotation = Quaternion.identity;
            board.transform.localScale = new Vector3(1.85f, 0.06f, 1.28f);
            board.GetComponent<Renderer>().sharedMaterial = material;
            var collider = board.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            Debug.LogWarning("Meshwright fallback FBX imported without Unity geometry; using the approved preview image as a preview-only ground plaque.");
        }

        private static void AssignTexture(Material material, string path, string property)
        {
            if (!material.HasProperty(property))
            {
                return;
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture != null)
            {
                material.SetTexture(property, texture);
            }
        }

        private static void ConfigureLodGroup(GameObject root)
        {
            var lod0 = FindRenderers(root, "LOD0");
            var lod1 = FindRenderers(root, "LOD1");
            var lod2 = FindRenderers(root, "LOD2");
            var allRenderers = root.GetComponentsInChildren<Renderer>(true);
            if (allRenderers.Length == 0)
            {
                throw new InvalidOperationException("Meshwright preview model has no renderers for LOD setup.");
            }

            var lodGroup = root.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                lodGroup = root.AddComponent<LODGroup>();
            }

            if (lod0.Length > 0 && lod1.Length > 0 && lod2.Length > 0)
            {
                lodGroup.SetLODs(new[]
                {
                    new LOD(0.60f, lod0),
                    new LOD(0.30f, lod1),
                    new LOD(0.10f, lod2)
                });
            }
            else
            {
                Debug.LogWarning("Meshwright preview model imported without distinct LOD renderer names; using all renderers as a single preview LOD.");
                lodGroup.SetLODs(new[]
                {
                    new LOD(0.60f, allRenderers)
                });
            }
            lodGroup.RecalculateBounds();
        }

        private static Renderer[] FindRenderers(GameObject root, string token)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            return Array.FindAll(renderers, renderer => renderer.name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static void AddBoundsCollider(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException("Meshwright preview model has no renderers for collider bounds.");
            }

            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            var collider = root.AddComponent<BoxCollider>();
            collider.center = root.transform.InverseTransformPoint(bounds.center);
            collider.size = bounds.size;
        }

        private static GameObject FindSceneObject(string name)
        {
            var scene = EditorSceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = FindInChildren(root.transform, name);
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

            for (var index = 0; index < root.childCount; index++)
            {
                var found = FindInChildren(root.GetChild(index), name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        [Serializable]
        private sealed class MeshQaRecord
        {
            public QaSummary summary;
        }

        [Serializable]
        private sealed class PreflightRecord
        {
            public QaSummary summary;
        }

        [Serializable]
        private sealed class QaSummary
        {
            public int fail;
            public bool ready_for_unity;
        }
    }
}
