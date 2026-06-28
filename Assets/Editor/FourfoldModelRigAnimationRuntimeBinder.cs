using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Editor
{
    public static class FourfoldModelRigAnimationRuntimeBinder
    {
        private const string PackRoot = "Assets/Art/ModelRigAnimation/Enemy/MeleeShardling/SealedLockRelic_v0.1.0";
        private const string ModelPath = PackRoot + "/Models/MDL_Enemy_MeleeShardling_SealedLockRelic_v0.1.0.fbx";
        private const string ClipRoot = PackRoot + "/AnimationClips";
        private const string RuntimeRoot = PackRoot + "/Runtime";
        private const string ControllerPath = RuntimeRoot + "/Animator/AC_Enemy_MeleeShardling_SealedLockRelic_v0.1.0.controller";
        private const string PrefabPath = RuntimeRoot + "/Prefabs/PF_Enemy_MeleeShardling_SealedLockRelic_v0.1.0.prefab";
        private const string ScenePath = RuntimeRoot + "/Scenes/SCN_Enemy_MeleeShardling_SealedLockRelic_RuntimePreview_v0.1.0.unity";
        private const string RuntimeQcPath = RuntimeRoot + "/runtime_binding_qc.json";

        private static readonly string[] Actions =
        {
            "Idle",
            "Walk",
            "Run",
            "AttackStart",
            "AttackLoop",
            "AttackEnd",
            "HitFront",
            "HitBack",
            "Knockdown",
            "Death",
            "CastStart",
            "ChannelLoop",
            "CastRelease",
            "Interact",
        };

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

        [MenuItem("FOURFOLD/Assets/Build And Verify Melee Shardling Runtime Binding")]
        public static void BuildAndVerifyMeleeShardlingRuntimeBinding()
        {
            BuildMeleeShardlingRuntimeBinding();
            VerifyMeleeShardlingRuntimeBinding();
        }

        [MenuItem("FOURFOLD/Assets/Build Melee Shardling Runtime Binding")]
        public static void BuildMeleeShardlingRuntimeBinding()
        {
            AssetDatabase.Refresh();
            Directory.CreateDirectory(RuntimeRoot + "/Animator");
            Directory.CreateDirectory(RuntimeRoot + "/Prefabs");
            Directory.CreateDirectory(RuntimeRoot + "/Scenes");

            var controller = BuildAnimatorController();
            BuildPrefab(controller);
            BuildPreviewScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("FOURFOLD/Assets/Verify Melee Shardling Runtime Binding")]
        public static void VerifyMeleeShardlingRuntimeBinding()
        {
            AssetDatabase.Refresh();

            var errors = new List<string>();
            var warnings = new List<string>();

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                errors.Add($"Missing AnimatorController: {ControllerPath}");
            }
            else
            {
                VerifyController(controller, errors);
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                errors.Add($"Missing runtime prefab: {PrefabPath}");
            }
            else
            {
                VerifyPrefab(prefab, controller, errors, warnings);
            }

            if (!File.Exists(ScenePath))
            {
                errors.Add($"Missing runtime preview scene: {ScenePath}");
            }
            else
            {
                VerifyPreviewScene(errors);
            }

            WriteRuntimeReport(errors, warnings);

            if (errors.Count > 0)
            {
                throw new InvalidOperationException($"Melee Shardling runtime binding verification failed with {errors.Count} errors. See {RuntimeQcPath}");
            }

            Debug.Log($"Melee Shardling runtime binding verification passed. Report: {RuntimeQcPath}");
        }

        private static AnimatorController BuildAnimatorController()
        {
            if (File.Exists(ControllerPath))
            {
                AssetDatabase.DeleteAsset(ControllerPath);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var layer = controller.layers[0];
            var stateMachine = layer.stateMachine;
            stateMachine.name = "MeleeShardlingRuntime";

            for (var index = 0; index < Actions.Length; index++)
            {
                var action = Actions[index];
                var clip = LoadClip(action);
                var state = stateMachine.AddState(action, new Vector3(260f * (index % 4), 80f * (index / 4), 0f));
                state.motion = clip;
                state.writeDefaultValues = true;
                state.speed = 1f;

                if (action == "Idle")
                {
                    stateMachine.defaultState = state;
                }
            }

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void BuildPrefab(RuntimeAnimatorController controller)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                throw new InvalidOperationException($"Model could not be loaded: {ModelPath}");
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            if (instance == null)
            {
                throw new InvalidOperationException($"Model could not be instantiated: {ModelPath}");
            }

            try
            {
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                instance.name = "PF_Enemy_MeleeShardling_SealedLockRelic_v0.1.0";
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;

                var animator = instance.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = instance.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.updateMode = AnimatorUpdateMode.Normal;

                var renderers = instance.GetComponentsInChildren<Renderer>(true);
                var bounds = CombinedBounds(renderers);
                var bodyCollider = instance.GetComponent<BoxCollider>();
                if (bodyCollider == null)
                {
                    bodyCollider = instance.AddComponent<BoxCollider>();
                }

                bodyCollider.isTrigger = false;
                bodyCollider.center = instance.transform.InverseTransformPoint(bounds.center);
                bodyCollider.size = bounds.size + new Vector3(0.08f, 0.08f, 0.08f);

                AddForwardHitbox(instance);

                PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath, out var success);
                if (!success)
                {
                    throw new InvalidOperationException($"Failed to save runtime prefab: {PrefabPath}");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static void BuildPreviewScene()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Prefab could not be loaded: {PrefabPath}");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "SCN_Enemy_MeleeShardling_SealedLockRelic_RuntimePreview_v0.1.0";

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Preview_Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(2.2f, 1f, 2.2f);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.name = "Preview_PF_Enemy_MeleeShardling_SealedLockRelic_v0.1.0";
            instance.transform.position = Vector3.zero;

            var keyLight = new GameObject("Preview_KeyLight");
            var light = keyLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            keyLight.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            var cameraObject = new GameObject("Preview_Camera");
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.transform.position = new Vector3(0f, -3.2f, 2.1f);
            cameraObject.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.045f, 0.052f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 20f;
            camera.fieldOfView = 38f;
            camera.tag = "MainCamera";

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void AddForwardHitbox(GameObject root)
        {
            var socket = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(transform => transform.name == "SOCKET_ForwardHit");
            if (socket == null)
            {
                throw new InvalidOperationException("Prefab is missing SOCKET_ForwardHit.");
            }

            var hitbox = new GameObject("HITBOX_ForwardPreview");
            hitbox.transform.SetParent(socket, false);
            hitbox.transform.localPosition = Vector3.zero;
            hitbox.transform.localRotation = Quaternion.identity;
            hitbox.transform.localScale = Vector3.one;

            var collider = hitbox.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = Vector3.zero;
            collider.size = new Vector3(0.6f, 0.38f, 0.32f);
        }

        private static void VerifyController(AnimatorController controller, List<string> errors)
        {
            var layer = controller.layers.FirstOrDefault();
            if (layer.stateMachine == null)
            {
                errors.Add("AnimatorController has no base state machine.");
                return;
            }

            var states = layer.stateMachine.states.Select(child => child.state).ToArray();
            var stateNames = states.Select(state => state.name).ToHashSet();
            foreach (var action in Actions)
            {
                if (!stateNames.Contains(action))
                {
                    errors.Add($"AnimatorController is missing state {action}.");
                    continue;
                }

                var state = states.First(item => item.name == action);
                if (state.motion == null)
                {
                    errors.Add($"AnimatorController state {action} has no motion.");
                }
            }

            if (layer.stateMachine.defaultState == null || layer.stateMachine.defaultState.name != "Idle")
            {
                errors.Add("AnimatorController default state must be Idle.");
            }
        }

        private static void VerifyPrefab(GameObject prefab, RuntimeAnimatorController expectedController, List<string> errors, List<string> warnings)
        {
            var animator = prefab.GetComponent<Animator>();
            if (animator == null)
            {
                errors.Add("Runtime prefab is missing Animator.");
            }
            else
            {
                if (animator.runtimeAnimatorController != expectedController)
                {
                    errors.Add("Runtime prefab AnimatorController reference is incorrect.");
                }

                if (animator.applyRootMotion)
                {
                    errors.Add("Runtime prefab must keep root motion disabled.");
                }
            }

            if (prefab.GetComponent<BoxCollider>() == null)
            {
                errors.Add("Runtime prefab is missing body BoxCollider.");
            }

            var transforms = prefab.GetComponentsInChildren<Transform>(true).Select(transform => transform.name).ToHashSet();
            foreach (var socket in RequiredSockets)
            {
                if (!transforms.Contains(socket))
                {
                    errors.Add($"Runtime prefab is missing socket {socket}.");
                }
            }

            var hitbox = prefab.GetComponentsInChildren<Transform>(true).FirstOrDefault(transform => transform.name == "HITBOX_ForwardPreview");
            if (hitbox == null)
            {
                errors.Add("Runtime prefab is missing HITBOX_ForwardPreview.");
            }
            else
            {
                var collider = hitbox.GetComponent<BoxCollider>();
                if (collider == null || !collider.isTrigger)
                {
                    errors.Add("HITBOX_ForwardPreview must have a trigger BoxCollider.");
                }
            }

            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                errors.Add("Runtime prefab has no renderers.");
            }

            var missingMaterials = renderers.Sum(renderer => renderer.sharedMaterials.Count(material => material == null));
            if (missingMaterials > 0)
            {
                errors.Add($"Runtime prefab has {missingMaterials} missing material slots.");
            }

            var bounds = CombinedBounds(renderers);
            if (bounds.size.magnitude <= 0.01f)
            {
                errors.Add("Runtime prefab bounds are effectively zero.");
            }
            else if (bounds.size.magnitude > 4.0f)
            {
                warnings.Add($"Runtime prefab bounds are larger than expected: {bounds.size}.");
            }
        }

        private static void VerifyPreviewScene(List<string> errors)
        {
            var previousScene = SceneManager.GetActiveScene().path;
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();
            if (!roots.Any(root => root.name == "Preview_PF_Enemy_MeleeShardling_SealedLockRelic_v0.1.0"))
            {
                errors.Add("Preview scene is missing the runtime prefab instance.");
            }

            if (!roots.Any(root => root.GetComponent<Camera>() != null))
            {
                errors.Add("Preview scene is missing a camera.");
            }

            if (!roots.Any(root => root.GetComponent<Light>() != null))
            {
                errors.Add("Preview scene is missing a light.");
            }

            if (!string.IsNullOrEmpty(previousScene) && previousScene != ScenePath)
            {
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
            }
        }

        private static AnimationClip LoadClip(string action)
        {
            var path = $"{ClipRoot}/ANM_Enemy_MeleeShardling_{action}_SealedLockRelic_v0.1.0.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                throw new InvalidOperationException($"Missing generated animation clip: {path}");
            }

            return clip;
        }

        private static Bounds CombinedBounds(Renderer[] renderers)
        {
            var activeRenderers = renderers.Where(renderer => renderer != null).ToArray();
            if (activeRenderers.Length == 0)
            {
                return new Bounds(Vector3.zero, Vector3.zero);
            }

            var bounds = activeRenderers[0].bounds;
            for (var index = 1; index < activeRenderers.Length; index++)
            {
                bounds.Encapsulate(activeRenderers[index].bounds);
            }

            return bounds;
        }

        private static void WriteRuntimeReport(IReadOnlyCollection<string> errors, IReadOnlyCollection<string> warnings)
        {
            Directory.CreateDirectory(RuntimeRoot);
            var builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine("  \"schema\": \"model_rig_animation_runtime_binding_qc_v1\",");
            builder.AppendLine("  \"asset_id\": \"modelrig.enemy.meleeshardling.sealedlockrelic.v0_1_0\",");
            builder.AppendLine($"  \"generated_at\": \"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\",");
            builder.AppendLine($"  \"status\": \"{(errors.Count == 0 ? "pass" : "fail")}\",");
            builder.AppendLine("  \"runtime_binding\": {");
            builder.AppendLine($"    \"animator_controller\": \"{ControllerPath}\",");
            builder.AppendLine($"    \"prefab\": \"{PrefabPath}\",");
            builder.AppendLine($"    \"preview_scene\": \"{ScenePath}\",");
            builder.AppendLine("    \"root_motion\": \"off\",");
            builder.AppendLine("    \"default_state\": \"Idle\",");
            builder.AppendLine($"    \"animation_state_count\": {Actions.Length.ToString(CultureInfo.InvariantCulture)},");
            builder.AppendLine("    \"forward_hitbox\": \"SOCKET_ForwardHit/HITBOX_ForwardPreview\"");
            builder.AppendLine("  },");
            builder.AppendLine("  \"errors\": [");
            AppendJsonArray(builder, errors, "    ");
            builder.AppendLine("  ],");
            builder.AppendLine("  \"warnings\": [");
            AppendJsonArray(builder, warnings, "    ");
            builder.AppendLine("  ]");
            builder.AppendLine("}");
            File.WriteAllText(RuntimeQcPath, builder.ToString(), new UTF8Encoding(false));
            AssetDatabase.ImportAsset(RuntimeQcPath);
        }

        private static void AppendJsonArray(StringBuilder builder, IEnumerable<string> values, string indent)
        {
            var array = values.ToArray();
            for (var index = 0; index < array.Length; index++)
            {
                var suffix = index == array.Length - 1 ? string.Empty : ",";
                builder.AppendLine($"{indent}\"{Escape(array[index])}\"{suffix}");
            }
        }

        private static string Escape(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
