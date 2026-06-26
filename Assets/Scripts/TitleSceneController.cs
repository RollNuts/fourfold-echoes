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
        private const int NewGameConfirmStart = 0;
        private const int NewGameConfirmCancel = 1;
        private const int NewGameConfirmCount = 2;
        private const int ContinueResumeRun = 0;
        private const int ContinueReturnHub = 1;
        private const int ContinueCancel = 2;
        private const int ContinueDecisionCount = 3;
        private const int SettingsCount = 6;
        private const float AxisRepeatDelay = 0.24f;

        private int selectedIndex;
        private bool newGameConfirmOpen;
        private int selectedNewGameConfirmIndex = NewGameConfirmCancel;
        private bool continueDecisionOpen;
        private int selectedContinueDecisionIndex;
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

            if (newGameConfirmOpen)
            {
                UpdateNewGameConfirmInput();
                return;
            }

            if (continueDecisionOpen)
            {
                UpdateContinueDecisionInput();
                return;
            }

            UpdateMenuInput();
        }

        public string RequestNewGame()
        {
            LoadProgress();
            if (FourfoldProgressSave.HasSaveFile())
            {
                newGameConfirmOpen = true;
                continueDecisionOpen = false;
                selectedNewGameConfirmIndex = NewGameConfirmCancel;
                return string.Empty;
            }

            return StartNewGame();
        }

        public string StartNewGame()
        {
            var previousSettings = FourfoldProgressSave.Load();
            FourfoldProgressSave.DeleteAll();
            progressData = NewGameProgress();
            FourfoldProgressSave.CopySettings(previousSettings, progressData);
            FourfoldProgressSave.Save(progressData);
            newGameConfirmOpen = false;
            return RequestScene(FourfoldGameIds.UnitySceneHubCrossroads);
        }

        public string ConfirmNewGameOverwrite()
        {
            newGameConfirmOpen = false;
            return StartNewGame();
        }

        public void CancelNewGameOverwrite()
        {
            newGameConfirmOpen = false;
            selectedNewGameConfirmIndex = NewGameConfirmCancel;
        }

        public bool IsNewGameConfirmationOpen()
        {
            return newGameConfirmOpen;
        }

        public string RequestContinueGame()
        {
            LoadProgress();
            if (!FourfoldProgressSave.HasSaveFile())
            {
                return StartNewGame();
            }

            if (progressData.currentScene == FourfoldGameIds.SceneD020VerticalSlice)
            {
                continueDecisionOpen = true;
                newGameConfirmOpen = false;
                selectedContinueDecisionIndex = ContinueResumeRun;
                return string.Empty;
            }

            return ContinueGame();
        }

        public string ContinueGame()
        {
            LoadProgress();
            if (!FourfoldProgressSave.HasSaveFile())
            {
                return StartNewGame();
            }

            continueDecisionOpen = false;
            return RequestScene(UnitySceneForProgress(progressData.currentScene));
        }

        public string ContinueRunFromTitleDecision()
        {
            continueDecisionOpen = false;
            return ContinueGame();
        }

        public string ReturnSavedRunToHub()
        {
            LoadProgress();
            if (!FourfoldProgressSave.HasSaveFile())
            {
                return StartNewGame();
            }

            progressData.currentScene = FourfoldGameIds.SceneHubCrossroads;
            progressData.hubUnlocked = true;
            progressData.regionD020Unlocked = true;
            progressData.lumenRodUnlocked = true;
            FourfoldProgressSave.Save(progressData);
            continueDecisionOpen = false;
            return RequestScene(FourfoldGameIds.UnitySceneHubCrossroads);
        }

        public void CancelContinueDecision()
        {
            continueDecisionOpen = false;
            selectedContinueDecisionIndex = ContinueResumeRun;
        }

        public bool IsContinueDecisionOpen()
        {
            return continueDecisionOpen;
        }

        public void OpenSettings()
        {
            LoadProgress();
            newGameConfirmOpen = false;
            continueDecisionOpen = false;
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
                case 4:
                    progressData.language = FourfoldLanguage.Toggle(progressData.language);
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
                return FourfoldLanguage.T(
                    progressData,
                    "New run: prepare in the hub, clear Region 01, defeat the boss, claim rewards, then return to save progress.",
                    "新規: ハブで準備し、地域01を攻略し、ボスを倒し、報酬を得てから帰還して進行を保存する。");
            }

            var location = progressData.currentScene == FourfoldGameIds.SceneD020VerticalSlice
                ? FourfoldLanguage.T(progressData, "Region attempt in progress", "地域攻略中")
                : FourfoldLanguage.T(progressData, "Hub", "ハブ");
            var relics = (progressData.d020RewardClaimed ? 1 : 0) + (progressData.d020SecondRewardClaimed ? 1 : 0);
            var best = progressData.d020BestClearTimeSeconds > 0f
                ? FourfoldLanguage.T(progressData, $" Best {Mathf.CeilToInt(progressData.d020BestClearTimeSeconds)}s.", $" 最速 {Mathf.CeilToInt(progressData.d020BestClearTimeSeconds)}秒。")
                : string.Empty;
            var risk = progressData.currentScene == FourfoldGameIds.SceneD020VerticalSlice
                ? FourfoldLanguage.T(progressData, " Resume the attempt or return to hub before starting again.", " 攻略を再開するか、再挑戦前にハブへ戻れる。")
                : string.Empty;
            return FourfoldLanguage.T(
                progressData,
                $"Save: {location}. Clears {progressData.d020ClearCount}. Saved reward skills {relics}/2.{best}{risk}",
                $"セーブ: {location}。クリア {progressData.d020ClearCount}。保存済み報酬スキル {relics}/2。{best}{risk}");
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

        private void UpdateNewGameConfirmInput()
        {
            if (Pressed(upKey) || AxisPressed(-1f))
            {
                selectedNewGameConfirmIndex = Wrap(selectedNewGameConfirmIndex - 1, NewGameConfirmCount);
            }
            else if (Pressed(downKey) || AxisPressed(1f))
            {
                selectedNewGameConfirmIndex = Wrap(selectedNewGameConfirmIndex + 1, NewGameConfirmCount);
            }

            if (Pressed(cancelKey) || Pressed(gamepadCancelKey))
            {
                CancelNewGameOverwrite();
                return;
            }

            if (Pressed(confirmKey) || Pressed(alternateConfirmKey) || Pressed(gamepadConfirmKey))
            {
                if (selectedNewGameConfirmIndex == NewGameConfirmStart)
                {
                    ConfirmNewGameOverwrite();
                }
                else
                {
                    CancelNewGameOverwrite();
                }
            }
        }

        private void UpdateContinueDecisionInput()
        {
            if (Pressed(upKey) || AxisPressed(-1f))
            {
                selectedContinueDecisionIndex = Wrap(selectedContinueDecisionIndex - 1, ContinueDecisionCount);
            }
            else if (Pressed(downKey) || AxisPressed(1f))
            {
                selectedContinueDecisionIndex = Wrap(selectedContinueDecisionIndex + 1, ContinueDecisionCount);
            }

            if (Pressed(cancelKey) || Pressed(gamepadCancelKey))
            {
                CancelContinueDecision();
                return;
            }

            if (!Pressed(confirmKey) && !Pressed(alternateConfirmKey) && !Pressed(gamepadConfirmKey))
            {
                return;
            }

            switch (selectedContinueDecisionIndex)
            {
                case ContinueResumeRun:
                    ContinueRunFromTitleDecision();
                    break;
                case ContinueReturnHub:
                    ReturnSavedRunToHub();
                    break;
                default:
                    CancelContinueDecision();
                    break;
            }
        }

        private void ActivateSelectedMenu()
        {
            switch (selectedIndex)
            {
                case MenuNewGame:
                    RequestNewGame();
                    break;
                case MenuContinue:
                    RequestContinueGame();
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
                showControlHints = true,
                language = FourfoldLanguage.English
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
            GUI.Label(new Rect(rect.x + 42f, rect.y + 82f, width - 84f, 34f), FourfoldLanguage.T(progressData, "Hub prep -> Region 01 run -> rewards -> hub result", "ハブ準備 -> 地域01攻略 -> 報酬 -> ハブ結果"), subheadStyle);
            GUI.Label(new Rect(rect.x + 42f, rect.y + 112f, width - 84f, 42f), FourfoldLanguage.T(progressData, "A compact fantasy action loop: use one exploration tool, read enemy tells, choose when to retry, and return to save what you earned.", "短く完結したファンタジーアクション: ひとつの探索ツールを使い、敵の予兆を読み、再挑戦を選び、得たものを帰還で保存する。"), mutedStyle);
            FourfoldRuntimeUi.DrawDivider(rect.x + 40f, rect.y + 158f, width - 80f);

            if (settingsOpen)
            {
                DrawSettings(rect, labelStyle);
            }
            else if (newGameConfirmOpen)
            {
                DrawNewGameConfirmation(rect, labelStyle, mutedStyle);
            }
            else if (continueDecisionOpen)
            {
                DrawContinueDecision(rect, labelStyle, mutedStyle);
            }
            else
            {
                DrawMenu(rect, labelStyle);
            }
        }

        private void DrawMenu(Rect rect, GUIStyle style)
        {
            var mutedStyle = FourfoldRuntimeUi.MutedStyle(Screen.height);
            var labels = new[]
            {
                FourfoldLanguage.T(progressData, "New Game", "新しく始める"),
                FourfoldProgressSave.HasSaveFile()
                    ? FourfoldLanguage.T(progressData, "Continue", "続きから")
                    : FourfoldLanguage.T(progressData, "Continue (starts new)", "続きから (新規開始)"),
                FourfoldLanguage.T(progressData, "Settings", "設定"),
                FourfoldLanguage.T(progressData, "Quit", "終了")
            };
            for (var i = 0; i < labels.Length; i++)
            {
                var itemRect = new Rect(rect.x + 54f, rect.y + 178f + i * 36f, rect.width - 108f, 32f);
                FourfoldRuntimeUi.DrawSelectableRow(itemRect, labels[i], selectedIndex == i, style);
            }

            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 54f, rect.y + 330f, rect.width - 108f, 48f), ContinueSummary(), new Color(0.25f, 0.68f, 1.0f), mutedStyle);
            GUI.Label(new Rect(rect.x + 64f, rect.y + rect.height - 38f, rect.width - 128f, 28f), FourfoldLanguage.T(progressData, "Move: arrows/stick   Confirm: Enter/A   Back: Esc/B", "移動: 矢印/スティック   決定: Enter/A   戻る: Esc/B"), mutedStyle);
        }

        private void DrawNewGameConfirmation(Rect rect, GUIStyle style, GUIStyle mutedStyle)
        {
            GUI.Label(new Rect(rect.x + 54f, rect.y + 178f, rect.width - 108f, 34f), FourfoldLanguage.T(progressData, "START NEW GAME?", "新しく始めますか？"), FourfoldRuntimeUi.SubheadStyle(Screen.height, FourfoldRuntimeUi.SafeUiScale(progressData)));
            GUI.Label(new Rect(rect.x + 54f, rect.y + 216f, rect.width - 108f, 54f), FourfoldLanguage.T(progressData, "This resets the product loop: clears, saved reward skills, best time, and failed-attempt count are erased. Audio, UI, and language settings are kept.", "製品ループをリセットする。クリア、保存済み報酬スキル、最速タイム、失敗回数を消去する。音量、UI、言語設定は保持される。"), style);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 54f, rect.y + 276f, rect.width - 108f, 34f), FourfoldLanguage.T(progressData, "Existing progress will be replaced.", "既存の進行は置き換えられる。"), new Color(1.0f, 0.46f, 0.22f), mutedStyle);

            var labels = new[]
            {
                FourfoldLanguage.T(progressData, "Start New Game", "新しく始める"),
                FourfoldLanguage.T(progressData, "Cancel", "キャンセル")
            };
            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 54f, rect.y + 326f + i * 34f, rect.width - 108f, 30f), labels[i], selectedNewGameConfirmIndex == i, style);
            }

            GUI.Label(new Rect(rect.x + 64f, rect.y + rect.height - 32f, rect.width - 128f, 24f), FourfoldLanguage.T(progressData, "Confirm: Enter/A   Cancel: Esc/B", "決定: Enter/A   キャンセル: Esc/B"), mutedStyle);
        }

        private void DrawContinueDecision(Rect rect, GUIStyle style, GUIStyle mutedStyle)
        {
            GUI.Label(new Rect(rect.x + 54f, rect.y + 176f, rect.width - 108f, 34f), FourfoldLanguage.T(progressData, "REGION ATTEMPT IN PROGRESS", "地域攻略中"), FourfoldRuntimeUi.SubheadStyle(Screen.height, FourfoldRuntimeUi.SafeUiScale(progressData)));
            GUI.Label(new Rect(rect.x + 54f, rect.y + 214f, rect.width - 108f, 54f), FourfoldLanguage.T(progressData, "Resume the current region attempt, or return to the hub to prepare before starting again. Saved hub progress stays saved.", "現在の地域攻略を再開するか、ハブへ戻って準備し直す。ハブ保存済みの進行はそのまま。"), style);

            var labels = new[]
            {
                FourfoldLanguage.T(progressData, "Resume Region", "地域を再開"),
                FourfoldLanguage.T(progressData, "Return to Hub", "ハブへ戻る"),
                FourfoldLanguage.T(progressData, "Cancel", "キャンセル")
            };
            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 54f, rect.y + 282f + i * 34f, rect.width - 108f, 30f), labels[i], selectedContinueDecisionIndex == i, style);
            }

            GUI.Label(new Rect(rect.x + 64f, rect.y + rect.height - 32f, rect.width - 128f, 24f), FourfoldLanguage.T(progressData, "Confirm: Enter/A   Cancel: Esc/B", "決定: Enter/A   キャンセル: Esc/B"), mutedStyle);
        }

        private void DrawSettings(Rect rect, GUIStyle style)
        {
            LoadProgress();
            var labels = new[]
            {
                $"{FourfoldLanguage.T(progressData, "Master Volume", "マスター音量")} {Mathf.RoundToInt(progressData.masterVolume * 100f)}%",
                $"{FourfoldLanguage.T(progressData, "Music Volume", "音楽音量")} {Mathf.RoundToInt(progressData.musicVolume * 100f)}%",
                $"{FourfoldLanguage.T(progressData, "SFX Volume", "効果音音量")} {Mathf.RoundToInt(progressData.sfxVolume * 100f)}%",
                $"{FourfoldLanguage.T(progressData, "UI Scale", "UIサイズ")} {Mathf.RoundToInt(progressData.uiScale * 100f)}%",
                $"{FourfoldLanguage.T(progressData, "Language", "言語")} {FourfoldLanguage.Label(progressData)}",
                $"{FourfoldLanguage.T(progressData, "Control Hints", "操作ヒント")} {(progressData.showControlHints ? FourfoldLanguage.T(progressData, "On", "表示") : FourfoldLanguage.T(progressData, "Off", "非表示"))}"
            };
            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 54f, rect.y + 150f + i * 36f, rect.width - 108f, 32f), labels[i], selectedSettingIndex == i, style);
            }

            GUI.Label(new Rect(rect.x + 64f, rect.y + rect.height - 54f, rect.width - 128f, 40f), FourfoldLanguage.T(progressData, "Left/Right changes value. Enter/A or Esc/B returns.", "左右で変更。Enter/A または Esc/B で戻る。"), style);
        }
    }
}
