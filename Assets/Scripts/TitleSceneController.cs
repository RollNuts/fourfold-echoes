using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Product
{
    public sealed class TitleSceneController : MonoBehaviour
    {
        public Camera titleCamera;

        [Header("Input")]
        public KeyCode upKey = KeyCode.UpArrow;
        public KeyCode downKey = KeyCode.DownArrow;
        public KeyCode leftKey = KeyCode.LeftArrow;
        public KeyCode rightKey = KeyCode.RightArrow;
        public KeyCode confirmKey = KeyCode.Return;
        public KeyCode alternateConfirmKey = KeyCode.Space;
        public KeyCode cancelKey = KeyCode.Escape;
        public KeyCode gamepadConfirmKey = KeyCode.JoystickButton0;
        public KeyCode gamepadCancelKey = KeyCode.JoystickButton1;

        private const int MenuNewGame = 0;
        private const int MenuContinue = 1;
        private const int MenuSettings = 2;
        private const int MenuQuit = 3;
        private const int MenuCount = 4;
        private const float AxisRepeatDelay = 0.24f;

        private int selectedIndex;
        private bool settingsOpen;
        private int selectedSettingIndex;
        private float axisRepeatTimer;
        private FourfoldProgressData progressData;

        public string LastRequestedUnityScene { get; private set; } = string.Empty;

        private void Awake()
        {
            if (titleCamera == null)
            {
                titleCamera = Camera.main;
            }

            LoadProgress();
        }

        private void Update()
        {
            axisRepeatTimer = Mathf.Max(0f, axisRepeatTimer - Time.unscaledDeltaTime);
            if (settingsOpen)
            {
                UpdateSettingsInput();
                return;
            }

            UpdateMenuInput();
        }

        public string StartNewGame()
        {
            FourfoldProgressSave.DeleteAll();
            progressData = NewGameProgress();
            FourfoldProgressSave.Save(progressData);
            return RequestScene(FourfoldGameIds.UnitySceneHubCrossroads);
        }

        public string ContinueGame()
        {
            LoadProgress();
            if (!FourfoldProgressSave.HasSaveFile())
            {
                return StartNewGame();
            }

            return RequestScene(UnitySceneForProgress(progressData.currentScene));
        }

        public void OpenSettings()
        {
            LoadProgress();
            settingsOpen = true;
            selectedSettingIndex = 0;
        }

        public void CloseSettings()
        {
            settingsOpen = false;
            SaveSettings();
        }

        public void AdjustSelectedSetting(float delta)
        {
            LoadProgress();
            var step = delta >= 0f ? 0.1f : -0.1f;
            switch (selectedSettingIndex)
            {
                case 0:
                    progressData.masterVolume = Mathf.Clamp01(progressData.masterVolume + step);
                    break;
                case 1:
                    progressData.musicVolume = Mathf.Clamp01(progressData.musicVolume + step);
                    break;
                default:
                    progressData.sfxVolume = Mathf.Clamp01(progressData.sfxVolume + step);
                    break;
            }

            SaveSettings();
        }

        public void QuitGame()
        {
            LastRequestedUnityScene = "quit";
            if (Application.isPlaying)
            {
                Application.Quit();
            }
        }

        private void UpdateMenuInput()
        {
            if (Pressed(upKey) || AxisPressed(-1f))
            {
                selectedIndex = Wrap(selectedIndex - 1, MenuCount);
            }
            else if (Pressed(downKey) || AxisPressed(1f))
            {
                selectedIndex = Wrap(selectedIndex + 1, MenuCount);
            }

            if (Pressed(confirmKey) || Pressed(alternateConfirmKey) || Pressed(gamepadConfirmKey))
            {
                ActivateSelectedMenu();
            }
        }

        private void UpdateSettingsInput()
        {
            if (Pressed(upKey) || AxisPressed(-1f))
            {
                selectedSettingIndex = Wrap(selectedSettingIndex - 1, 3);
            }
            else if (Pressed(downKey) || AxisPressed(1f))
            {
                selectedSettingIndex = Wrap(selectedSettingIndex + 1, 3);
            }

            if (Pressed(leftKey) || HorizontalAxisPressed(-1f))
            {
                AdjustSelectedSetting(-1f);
            }
            else if (Pressed(rightKey) || HorizontalAxisPressed(1f))
            {
                AdjustSelectedSetting(1f);
            }

            if (Pressed(cancelKey) || Pressed(gamepadCancelKey) || Pressed(confirmKey) || Pressed(gamepadConfirmKey))
            {
                CloseSettings();
            }
        }

        private void ActivateSelectedMenu()
        {
            switch (selectedIndex)
            {
                case MenuNewGame:
                    StartNewGame();
                    break;
                case MenuContinue:
                    ContinueGame();
                    break;
                case MenuSettings:
                    OpenSettings();
                    break;
                case MenuQuit:
                    QuitGame();
                    break;
            }
        }

        private string RequestScene(string unitySceneName)
        {
            LastRequestedUnityScene = unitySceneName;
            if (Application.isPlaying)
            {
                SceneManager.LoadScene(unitySceneName);
            }

            return unitySceneName;
        }

        private void LoadProgress()
        {
            progressData = FourfoldProgressSave.Load();
        }

        private void SaveSettings()
        {
            if (progressData == null)
            {
                progressData = NewGameProgress();
            }

            progressData.settingsInitialized = true;
            FourfoldProgressSave.Save(progressData);
        }

        private static FourfoldProgressData NewGameProgress()
        {
            return new FourfoldProgressData
            {
                currentScene = FourfoldGameIds.SceneHubCrossroads,
                hubUnlocked = true,
                regionD020Unlocked = true,
                lumenRodUnlocked = true,
                settingsInitialized = true,
                masterVolume = 1f,
                musicVolume = 1f,
                sfxVolume = 1f
            };
        }

        private static string UnitySceneForProgress(string sceneId)
        {
            switch (sceneId)
            {
                case FourfoldGameIds.SceneD020VerticalSlice:
                    return FourfoldGameIds.UnitySceneD020VerticalSlice;
                case FourfoldGameIds.SceneTitle:
                    return FourfoldGameIds.UnitySceneTitle;
                case FourfoldGameIds.SceneHubCrossroads:
                default:
                    return FourfoldGameIds.UnitySceneHubCrossroads;
            }
        }

        private bool AxisPressed(float sign)
        {
            var value = SafeAxis("Vertical");
            if (sign < 0f)
            {
                value = -value;
            }

            return value > 0.55f && ConsumeAxisRepeat();
        }

        private bool HorizontalAxisPressed(float sign)
        {
            var value = SafeAxis("Horizontal");
            if (sign < 0f)
            {
                value = -value;
            }

            return value > 0.55f && ConsumeAxisRepeat();
        }

        private bool ConsumeAxisRepeat()
        {
            if (axisRepeatTimer > 0f)
            {
                return false;
            }

            axisRepeatTimer = AxisRepeatDelay;
            return true;
        }

        private static float SafeAxis(string axisName)
        {
            try
            {
                return Input.GetAxisRaw(axisName);
            }
            catch (System.ArgumentException)
            {
                return 0f;
            }
        }

        private static bool Pressed(KeyCode key)
        {
            return key != KeyCode.None && Input.GetKeyDown(key);
        }

        private static int Wrap(int value, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            return (value % count + count) % count;
        }

        private void OnGUI()
        {
            var width = Mathf.Min(760f, Screen.width - 48f);
            var height = settingsOpen ? 360f : 330f;
            var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.Box(rect, GUIContent.none);

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.height / 18, 34, 62),
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1.0f, 0.86f, 0.50f) }
            };
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.height / 38, 18, 26),
                normal = { textColor = Color.white }
            };

            GUI.Label(new Rect(rect.x + 24f, rect.y + 20f, width - 48f, 70f), "FOURFOLD ECHOES", titleStyle);
            GUI.Label(new Rect(rect.x + 40f, rect.y + 92f, width - 80f, 42f), "Compact single-player action adventure", labelStyle);

            if (settingsOpen)
            {
                DrawSettings(rect, labelStyle);
            }
            else
            {
                DrawMenu(rect, labelStyle);
            }
        }

        private void DrawMenu(Rect rect, GUIStyle style)
        {
            var labels = new[] { "New Game", FourfoldProgressSave.HasSaveFile() ? "Continue" : "Continue (starts new)", "Settings", "Quit" };
            for (var i = 0; i < labels.Length; i++)
            {
                var prefix = selectedIndex == i ? "> " : "  ";
                GUI.Label(new Rect(rect.x + 64f, rect.y + 148f + i * 34f, rect.width - 128f, 30f), prefix + labels[i], style);
            }

            GUI.Label(new Rect(rect.x + 64f, rect.y + rect.height - 52f, rect.width - 128f, 28f), "Move: arrows/stick   Confirm: Enter/A   Back: Esc/B", style);
        }

        private void DrawSettings(Rect rect, GUIStyle style)
        {
            LoadProgress();
            var labels = new[]
            {
                $"Master Volume {Mathf.RoundToInt(progressData.masterVolume * 100f)}%",
                $"Music Volume {Mathf.RoundToInt(progressData.musicVolume * 100f)}%",
                $"SFX Volume {Mathf.RoundToInt(progressData.sfxVolume * 100f)}%"
            };
            for (var i = 0; i < labels.Length; i++)
            {
                var prefix = selectedSettingIndex == i ? "> " : "  ";
                GUI.Label(new Rect(rect.x + 64f, rect.y + 150f + i * 36f, rect.width - 128f, 32f), prefix + labels[i], style);
            }

            GUI.Label(new Rect(rect.x + 64f, rect.y + rect.height - 70f, rect.width - 128f, 52f), "Left/Right changes value. Enter/A or Esc/B returns.", style);
        }
    }
}
