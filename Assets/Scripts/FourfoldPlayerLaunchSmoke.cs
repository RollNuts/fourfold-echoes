using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Product
{
    public sealed class FourfoldPlayerLaunchSmoke : MonoBehaviour
    {
        private const string SmokeArg = "--fourfoldLaunchSmoke";
        private const string PassPrefix = "FOURFOLD PLAYER SMOKE PASS";
        private const string FailPrefix = "FOURFOLD PLAYER SMOKE FAIL";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void MaybeRun()
        {
            if (!Requested())
            {
                return;
            }

            var runner = new GameObject("Fourfold Player Launch Smoke");
            DontDestroyOnLoad(runner);
            runner.AddComponent<FourfoldPlayerLaunchSmoke>();
        }

        private void Start()
        {
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            yield return null;
            yield return null;

            try
            {
                var scene = SceneManager.GetActiveScene();
                if (!scene.IsValid() || scene.name != FourfoldGameIds.UnitySceneTitle)
                {
                    throw new InvalidOperationException($"expected Title scene, got '{scene.name}'");
                }

                var titleHook = GameObject.Find("Title Runtime Hook");
                if (titleHook == null)
                {
                    throw new InvalidOperationException("Title Runtime Hook missing");
                }

                var titleController = titleHook.GetComponent<TitleSceneController>();
                if (titleController == null)
                {
                    throw new InvalidOperationException("TitleSceneController missing");
                }

                var progress = FourfoldProgressSave.Load();
                if (progress == null || progress.version != FourfoldProgressSave.CurrentVersion || !progress.settingsInitialized)
                {
                    throw new InvalidOperationException("progress save defaults failed to load");
                }

                Debug.Log($"{PassPrefix} scene={scene.name} saveVersion={progress.version}");
                Application.Quit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError($"{FailPrefix} {exception.Message}");
                Application.Quit(1);
            }
        }

        private static bool Requested()
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == SmokeArg)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
