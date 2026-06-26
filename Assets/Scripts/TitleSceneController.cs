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
        private const int SettingsCount = 5;
        private const float AxisRepeatDelay = 0.24f;

        private int selectedIndex;
        private bool settingsOpen;
        private int selectedSettingIndex;
        private float axisRepeatTimer;
        private FourfoldProgressData progressData;

        public string LastRequestedUnityScene { get; private set; } = string.Empty;

        public static bool LayoutFitsResolution(int screenWidth, int screenHeight, bool settingsOpen, out string reason)
        {
            reason = string.Empty;
            if (screenWidth < 960 || screenHeight < 540)
            {
                reason = $"resolution too small for product HUD: {screenWidth}x{screenHeight}";
                return false;
            }

            var width = Mathf.Min(760f, screenWidth - 48f);
            var height = 430f;
            var rect = new Rect((screenWidth - width) * 0.5f, (screenHeight - height) * 0.5f, width, height);
            if (rect.x < 24f || rect.y < 24f || rect.xMax > screenWidth - 24f || rect.yMax > screenHeight - 24f)
            {
                reason = $"title layout exceeds safe area at {screenWidth}x{screenHeight}: {rect}";
                return false;
            }

            var labelFont = Mathf.Clamp(screenHeight / 38, 18, 26);
            if (labelFont < 18)
            {
                reason = $"title menu font is too small at {screenWidth}x{screenHeight}: {labelFont}";
                return false;
            }

            return true;
        }

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
            var previousSettings = FourfoldProgressSave.Load();
            FourfoldProgressSave.DeleteAll();
            progressData = NewGameProgress();
            FourfoldProgressSave.CopySettings(previousSettings, progressData);
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
                case 2:
                    progressData.sfxVolume = Mathf.Clamp01(progressData.sfxVolume + step);
                    break;
                case 3:
                    progressData.uiScale = Mathf.Clamp(progressData.uiScale + step, 0.85f, 1.25f);
                    break;
                default:
                    progressData.showControlHints = !progressData.showControlHints;
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

        public string ContinueSummary()
        {
            LoadProgress();
            if (!FourfoldProgressSave.HasSaveFile())
            {
                return "Goal: enter D-020, defeat the boss, secure relics, and return to bank rewards.";
            }

            var location = progressData.currentScene == FourfoldGameIds.SceneD020VerticalSlice
                ? "D-020 run in progress"
                : "Hub";
            var relics = (progressData.d020RewardClaimed ? 1 : 0) + (progressData.d020SecondRewardClaimed ? 1 : 0);
            var best = progressData.d020BestClearTimeSeconds > 0f
                ? $" Best {Mathf.CeilToInt(progressData.d020BestClearTimeSeconds)}s."
                : string.Empty;
            var risk = progressData.currentScene == FourfoldGameIds.SceneD020VerticalSlice
                ? " Unreturned run rewards are still at risk."
                : string.Empty;
            return $"Continue: {location}. Clears {progressData.d020ClearCount}. Relics returned {relics}/2.{best}{risk}";
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
                selectedSettingIndex = Wrap(selectedSettingIndex - 1, SettingsCount);
            }
            else if (Pressed(downKey) || AxisPressed(1f))
            {
                selectedSettingIndex = Wrap(selectedSettingIndex + 1, SettingsCount);
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
                sfxVolume = 1f,
                uiScale = 1f,
                showControlHints = true
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
            var height = 430f;
            var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            FourfoldRuntimeUi.DrawScreenWash();
            FourfoldRuntimeUi.DrawPanel(rect);

            var uiScale = FourfoldRuntimeUi.SafeUiScale(progressData);
            var titleStyle = FourfoldRuntimeUi.HeaderStyle(Screen.height, uiScale);
            var labelStyle = FourfoldRuntimeUi.BodyStyle(Screen.height, uiScale);
            var subheadStyle = FourfoldRuntimeUi.SubheadStyle(Screen.height, uiScale);
            var mutedStyle = FourfoldRuntimeUi.MutedStyle(Screen.height, uiScale);

            GUI.Label(new Rect(rect.x + 40f, rect.y + 22f, width - 80f, 62f), "FOURFOLD ECHOES", titleStyle);
            GUI.Label(new Rect(rect.x + 42f, rect.y + 82f, width - 84f, 34f), "Boss-run fantasy action RPG", subheadStyle);
            GUI.Label(new Rect(rect.x + 42f, rect.y + 112f, width - 84f, 42f), "Prepare in the hub, enter D-020, defeat the boss, secure relic skills, then return before a failed run drops unbanked rewards.", mutedStyle);
            FourfoldRuntimeUi.DrawDivider(rect.x + 40f, rect.y + 158f, width - 80f);

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
            var mutedStyle = FourfoldRuntimeUi.MutedStyle(Screen.height);
            var labels = new[] { "New Game", FourfoldProgressSave.HasSaveFile() ? "Continue" : "Continue (starts new)", "Settings", "Quit" };
            for (var i = 0; i < labels.Length; i++)
            {
                var itemRect = new Rect(rect.x + 54f, rect.y + 178f + i * 36f, rect.width - 108f, 32f);
                FourfoldRuntimeUi.DrawSelectableRow(itemRect, labels[i], selectedIndex == i, style);
            }

            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 54f, rect.y + 330f, rect.width - 108f, 48f), ContinueSummary(), new Color(0.25f, 0.68f, 1.0f), mutedStyle);
            GUI.Label(new Rect(rect.x + 64f, rect.y + rect.height - 38f, rect.width - 128f, 28f), "Move: arrows/stick   Confirm: Enter/A   Back: Esc/B", mutedStyle);
        }

        private void DrawSettings(Rect rect, GUIStyle style)
        {
            LoadProgress();
            var labels = new[]
            {
                $"Master Volume {Mathf.RoundToInt(progressData.masterVolume * 100f)}%",
                $"Music Volume {Mathf.RoundToInt(progressData.musicVolume * 100f)}%",
                $"SFX Volume {Mathf.RoundToInt(progressData.sfxVolume * 100f)}%",
                $"UI Scale {Mathf.RoundToInt(progressData.uiScale * 100f)}%",
                $"Control Hints {(progressData.showControlHints ? "On" : "Off")}"
            };
            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 54f, rect.y + 150f + i * 36f, rect.width - 108f, 32f), labels[i], selectedSettingIndex == i, style);
            }

            GUI.Label(new Rect(rect.x + 64f, rect.y + rect.height - 54f, rect.width - 128f, 40f), "Left/Right changes value. Enter/A or Esc/B returns.", style);
        }
    }
}
