using UnityEngine;
using UnityEngine.SceneManagement;

namespace FourfoldEchoes.Product
{
    public sealed class HubSceneController : MonoBehaviour
    {
        [Header("Scene")]
        public Transform player;
        public Transform returnSpawn;
        public Transform d020RegionGate;
        public Camera fixedCamera;

        [Header("Region")]
        public string regionSceneName = FourfoldGameIds.UnitySceneD020VerticalSlice;

        [Header("Input")]
        public KeyCode interactKey = KeyCode.E;
        public KeyCode resetKey = KeyCode.Backspace;
        public KeyCode pauseKey = KeyCode.Escape;
        public KeyCode gamepadInteractKey = KeyCode.JoystickButton3;
        public KeyCode gamepadResetKey = KeyCode.JoystickButton6;
        public KeyCode gamepadPauseKey = KeyCode.JoystickButton9;

        private const float MoveSpeed = 4.8f;
        private const float InteractionRange = 2.1f;
        private const float ResetHoldDuration = 1.2f;
        private const float MinX = -6.0f;
        private const float MaxX = 6.0f;
        private const float MinZ = -5.0f;
        private const float MaxZ = 5.8f;
        private const int PauseResume = 0;
        private const int PauseSettings = 1;
        private const int PauseTitle = 2;
        private const int PauseMenuCount = 3;
        private const int MissionStart = 0;
        private const int MissionSettings = 1;
        private const int MissionBack = 2;
        private const int MissionMenuCount = 3;
        private const int SettingsCount = 5;
        private const float AxisRepeatDelay = 0.24f;

        private Vector3 facing = Vector3.forward;
        private FourfoldProgressData progressData;
        private float resetHoldSeconds;
        private bool paused;
        private bool settingsOpen;
        private bool missionBriefingOpen;
        private bool settingsOpenedFromMissionBriefing;
        private int selectedPauseIndex;
        private int selectedMissionIndex;
        private int selectedSettingIndex;
        private float axisRepeatTimer;

        public static bool LayoutFitsResolution(int screenWidth, int screenHeight, bool pauseOpen, out string reason)
        {
            reason = string.Empty;
            if (screenWidth < 960 || screenHeight < 540)
            {
                reason = $"resolution too small for hub HUD: {screenWidth}x{screenHeight}";
                return false;
            }

            var topPanel = new Rect(18f, 18f, Mathf.Min(760f, screenWidth - 36f), 190f);
            if (topPanel.xMax > screenWidth - 18f || topPanel.yMax > screenHeight - 18f)
            {
                reason = $"hub status text exceeds safe area at {screenWidth}x{screenHeight}: {topPanel}";
                return false;
            }

            if (!pauseOpen)
            {
                return true;
            }

            var pauseWidth = Mathf.Min(520f, screenWidth - 48f);
            var pauseHeight = 276f;
            var pauseRect = new Rect((screenWidth - pauseWidth) * 0.5f, (screenHeight - pauseHeight) * 0.5f, pauseWidth, pauseHeight);
            if (pauseRect.x < 24f || pauseRect.y < 24f || pauseRect.xMax > screenWidth - 24f || pauseRect.yMax > screenHeight - 24f)
            {
                reason = $"hub pause panel exceeds safe area at {screenWidth}x{screenHeight}: {pauseRect}";
                return false;
            }

            return true;
        }

        private void Awake()
        {
            if (player == null)
            {
                player = transform;
            }

            if (fixedCamera == null)
            {
                fixedCamera = Camera.main;
            }

            InitializeHubProgress();
            PlacePlayerAtHubSpawn();
            UpdateCamera();
        }

        private void Update()
        {
            axisRepeatTimer = Mathf.Max(0f, axisRepeatTimer - Time.unscaledDeltaTime);
            if (Pressed(pauseKey, gamepadPauseKey))
            {
                if (settingsOpen)
                {
                    CloseSettings();
                    return;
                }

                if (missionBriefingOpen)
                {
                    CloseMissionBriefing();
                    return;
                }

                paused = !paused;
                selectedPauseIndex = 0;
                resetHoldSeconds = 0f;
                return;
            }

            if (missionBriefingOpen)
            {
                UpdateMissionBriefingInput();
                return;
            }

            if (paused)
            {
                UpdatePauseInput();
                return;
            }

            MovePlayer(Time.deltaTime);
            UpdateCamera();
            UpdateResetInput(Time.deltaTime);

            if (Pressed(interactKey, gamepadInteractKey))
            {
                if (CanEnterD020Region())
                {
                    OpenMissionBriefing();
                }
            }
        }

        public void InitializeHubProgress()
        {
            progressData = FourfoldProgressSave.Load();
            progressData.currentScene = FourfoldGameIds.SceneHubCrossroads;
            progressData.hubUnlocked = true;
            progressData.regionD020Unlocked = true;
            progressData.lumenRodUnlocked = true;
            FourfoldProgressSave.Save(progressData);
        }

        public bool TryEnterD020Region()
        {
            if (!CanEnterD020Region())
            {
                return false;
            }

            missionBriefingOpen = false;
            settingsOpenedFromMissionBriefing = false;
            settingsOpen = false;
            paused = false;
            progressData = FourfoldProgressSave.Load();
            progressData.currentScene = FourfoldGameIds.SceneD020VerticalSlice;
            progressData.hubUnlocked = true;
            progressData.regionD020Unlocked = true;
            progressData.lumenRodUnlocked = true;
            FourfoldProgressSave.Save(progressData);

            if (Application.isPlaying)
            {
                SceneManager.LoadScene(regionSceneName);
            }

            return true;
        }

        public bool TryReturnToTitle()
        {
            progressData = FourfoldProgressSave.Load();
            progressData.currentScene = FourfoldGameIds.SceneHubCrossroads;
            progressData.hubUnlocked = true;
            progressData.regionD020Unlocked = true;
            progressData.lumenRodUnlocked = true;
            FourfoldProgressSave.Save(progressData);
            paused = false;
            settingsOpen = false;
            missionBriefingOpen = false;
            settingsOpenedFromMissionBriefing = false;
            resetHoldSeconds = 0f;

            if (Application.isPlaying)
            {
                SceneManager.LoadScene(FourfoldGameIds.UnitySceneTitle);
            }

            return true;
        }

        public void ResetProgressForNewGame()
        {
            var previousSettings = FourfoldProgressSave.Load();
            FourfoldProgressSave.DeleteAll();
            InitializeHubProgress();
            FourfoldProgressSave.CopySettings(previousSettings, progressData);
            FourfoldProgressSave.Save(progressData);
            PlacePlayerAtHubSpawn();
            resetHoldSeconds = 0f;
            settingsOpen = false;
            missionBriefingOpen = false;
            settingsOpenedFromMissionBriefing = false;
            paused = false;
        }

        public void OpenSettings()
        {
            progressData = FourfoldProgressSave.Load();
            paused = true;
            settingsOpen = true;
            settingsOpenedFromMissionBriefing = false;
            selectedSettingIndex = 0;
        }

        public void OpenMissionBriefing()
        {
            if (!CanEnterD020Region())
            {
                return;
            }

            progressData = FourfoldProgressSave.Load();
            missionBriefingOpen = true;
            settingsOpen = false;
            settingsOpenedFromMissionBriefing = false;
            paused = false;
            selectedMissionIndex = 0;
            resetHoldSeconds = 0f;
        }

        public void CloseMissionBriefing()
        {
            missionBriefingOpen = false;
            settingsOpen = false;
            settingsOpenedFromMissionBriefing = false;
            resetHoldSeconds = 0f;
        }

        public bool IsMissionBriefingOpen()
        {
            return missionBriefingOpen;
        }

        public void CloseSettings()
        {
            settingsOpen = false;
            if (settingsOpenedFromMissionBriefing)
            {
                paused = false;
                missionBriefingOpen = true;
                settingsOpenedFromMissionBriefing = false;
            }

            SaveSettings();
        }

        public void AdjustSelectedSetting(float delta)
        {
            progressData = FourfoldProgressSave.Load();
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

        public bool CanEnterD020Region()
        {
            if (player == null || d020RegionGate == null)
            {
                return false;
            }

            return FlatDistance(player.position, d020RegionGate.position) <= InteractionRange;
        }

        private void PlacePlayerAtHubSpawn()
        {
            if (player == null || returnSpawn == null)
            {
                return;
            }

            player.position = returnSpawn.position;
            player.rotation = returnSpawn.rotation;
        }

        private void MovePlayer(float deltaTime)
        {
            if (player == null)
            {
                return;
            }

            var input = ReadMoveInput();
            if (input.sqrMagnitude <= 0.001f)
            {
                return;
            }

            facing = input.normalized;
            var proposed = player.position + facing * MoveSpeed * deltaTime;
            player.position = new Vector3(
                Mathf.Clamp(proposed.x, MinX, MaxX),
                player.position.y,
                Mathf.Clamp(proposed.z, MinZ, MaxZ));
            player.rotation = Quaternion.LookRotation(facing, Vector3.up);
        }

        private void UpdateResetInput(float deltaTime)
        {
            if (Input.GetKey(resetKey) || Input.GetKey(gamepadResetKey))
            {
                resetHoldSeconds += deltaTime;
                if (resetHoldSeconds >= ResetHoldDuration)
                {
                    ResetProgressForNewGame();
                }

                return;
            }

            resetHoldSeconds = 0f;
        }

        private void UpdatePauseInput()
        {
            if (settingsOpen)
            {
                UpdateSettingsInput();
                return;
            }

            if (Pressed(KeyCode.UpArrow, KeyCode.W) || AxisPressed(1f))
            {
                selectedPauseIndex = Wrap(selectedPauseIndex - 1, PauseMenuCount);
            }
            else if (Pressed(KeyCode.DownArrow, KeyCode.S) || AxisPressed(-1f))
            {
                selectedPauseIndex = Wrap(selectedPauseIndex + 1, PauseMenuCount);
            }

            if (Pressed(interactKey, KeyCode.Return, gamepadInteractKey))
            {
                ActivatePauseSelection();
            }
        }

        private void UpdateMissionBriefingInput()
        {
            if (settingsOpen)
            {
                UpdateSettingsInput();
                return;
            }

            if (Pressed(KeyCode.UpArrow, KeyCode.W) || AxisPressed(1f))
            {
                selectedMissionIndex = Wrap(selectedMissionIndex - 1, MissionMenuCount);
            }
            else if (Pressed(KeyCode.DownArrow, KeyCode.S) || AxisPressed(-1f))
            {
                selectedMissionIndex = Wrap(selectedMissionIndex + 1, MissionMenuCount);
            }

            if (Pressed(resetKey, pauseKey, gamepadResetKey, gamepadPauseKey))
            {
                CloseMissionBriefing();
                return;
            }

            if (Pressed(interactKey, KeyCode.Return, gamepadInteractKey))
            {
                ActivateMissionSelection();
            }
        }

        private void UpdateSettingsInput()
        {
            if (Pressed(KeyCode.UpArrow, KeyCode.W) || AxisPressed(1f))
            {
                selectedSettingIndex = Wrap(selectedSettingIndex - 1, SettingsCount);
            }
            else if (Pressed(KeyCode.DownArrow, KeyCode.S) || AxisPressed(-1f))
            {
                selectedSettingIndex = Wrap(selectedSettingIndex + 1, SettingsCount);
            }

            if (Pressed(KeyCode.LeftArrow, KeyCode.A) || HorizontalAxisPressed(-1f))
            {
                AdjustSelectedSetting(-1f);
            }
            else if (Pressed(KeyCode.RightArrow, KeyCode.D) || HorizontalAxisPressed(1f))
            {
                AdjustSelectedSetting(1f);
            }

            if (Pressed(interactKey, KeyCode.Return, gamepadInteractKey) || Pressed(resetKey, gamepadResetKey))
            {
                CloseSettings();
            }
        }

        private void ActivatePauseSelection()
        {
            switch (selectedPauseIndex)
            {
                case PauseResume:
                    paused = false;
                    settingsOpen = false;
                    break;
                case PauseSettings:
                    OpenSettings();
                    break;
                case PauseTitle:
                    TryReturnToTitle();
                    break;
            }
        }

        private void ActivateMissionSelection()
        {
            switch (selectedMissionIndex)
            {
                case MissionStart:
                    TryEnterD020Region();
                    break;
                case MissionSettings:
                    progressData = FourfoldProgressSave.Load();
                    settingsOpen = true;
                    settingsOpenedFromMissionBriefing = true;
                    selectedSettingIndex = 0;
                    break;
                case MissionBack:
                    CloseMissionBriefing();
                    break;
            }
        }

        private void SaveSettings()
        {
            if (progressData == null)
            {
                progressData = FourfoldProgressSave.Load();
            }

            progressData.settingsInitialized = true;
            FourfoldProgressSave.Save(progressData);
        }

        private void UpdateCamera()
        {
            if (fixedCamera == null || player == null)
            {
                return;
            }

            fixedCamera.transform.position = new Vector3(0f, 9.4f, -8.7f);
            fixedCamera.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0.1f, 1.2f) - fixedCamera.transform.position, Vector3.up);
        }

        private static Vector3 ReadMoveInput()
        {
            var horizontal = SafeAxis("Horizontal");
            var vertical = SafeAxis("Vertical");
            var input = new Vector3(horizontal, 0f, vertical);
            return input.sqrMagnitude > 1f ? input.normalized : input;
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

        private static bool Pressed(params KeyCode[] keys)
        {
            for (var i = 0; i < keys.Length; i++)
            {
                if (keys[i] != KeyCode.None && Input.GetKeyDown(keys[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static int Wrap(int value, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            return (value % count + count) % count;
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private void OnGUI()
        {
            FourfoldRuntimeUi.DrawScreenWash();
            if (progressData == null)
            {
                progressData = FourfoldProgressSave.Load();
            }

            var uiScale = FourfoldRuntimeUi.SafeUiScale(progressData);
            var cleared = progressData != null && progressData.regionD020Cleared;
            var clearCount = progressData == null ? 0 : progressData.d020ClearCount;
            var relics = progressData == null ? 0 : (progressData.d020RewardClaimed ? 1 : 0) + (progressData.d020SecondRewardClaimed ? 1 : 0);
            var bestTime = progressData == null || progressData.d020BestClearTimeSeconds <= 0f
                ? "--"
                : Mathf.CeilToInt(progressData.d020BestClearTimeSeconds).ToString() + "s";
            var panel = new Rect(18f, 18f, Mathf.Min(760f, Screen.width - 36f), 190f);
            FourfoldRuntimeUi.DrawPanel(panel);
            var header = FourfoldRuntimeUi.SubheadStyle(Screen.height, uiScale);
            var body = FourfoldRuntimeUi.BodyStyle(Screen.height, uiScale);
            var muted = FourfoldRuntimeUi.MutedStyle(Screen.height, uiScale);
            GUI.Label(new Rect(panel.x + 18f, panel.y + 12f, panel.width - 36f, 34f), "HUB: Crossroads", header);
            GUI.Label(new Rect(panel.x + 18f, panel.y + 48f, panel.width - 36f, 42f), cleared ? "D-020 cleared. Re-enter to improve your time or test the build." : "Mission: enter D-020, defeat the boss, claim both relic rewards, and return to bank them.", body);
            FourfoldRuntimeUi.DrawChip(new Rect(panel.x + 18f, panel.y + 96f, 230f, 34f), $"Clears {clearCount}   Best {bestTime}", new Color(1.0f, 0.72f, 0.24f), muted);
            FourfoldRuntimeUi.DrawChip(new Rect(panel.x + 260f, panel.y + 96f, 220f, 34f), $"Relics banked {relics}/2", new Color(0.22f, 0.70f, 1.0f), muted);
            var prompt = CanEnterD020Region() ? "Press E / Y: Enter D-020" : "Move to the gold gate to start the run.";
            GUI.Label(new Rect(panel.x + 18f, panel.y + 142f, panel.width - 36f, 24f), prompt, body);
            if (progressData == null || progressData.showControlHints)
            {
                GUI.Label(new Rect(panel.x + 18f, panel.y + 166f, panel.width - 36f, 24f), resetHoldSeconds > 0f ? "Keep holding reset to erase progress." : "Esc/Menu: Pause   Hold Backspace / Select: Reset save", muted);
            }

            DrawObjectiveMarker(body);

            if (missionBriefingOpen)
            {
                DrawMissionBriefing(body, muted);
                return;
            }

            if (!paused)
            {
                return;
            }

            var pauseWidth = Mathf.Min(520f, Screen.width - 48f);
            var pauseHeight = 276f;
            var pauseRect = new Rect((Screen.width - pauseWidth) * 0.5f, (Screen.height - pauseHeight) * 0.5f, pauseWidth, pauseHeight);
            FourfoldRuntimeUi.DrawPanel(pauseRect);
            GUI.Label(new Rect(pauseRect.x + 24f, pauseRect.y + 18f, pauseWidth - 48f, 30f), settingsOpen ? "SETTINGS" : "PAUSED", header);

            if (settingsOpen)
            {
                DrawSettings(pauseRect, body, muted);
                return;
            }

            var labels = new[] { "Resume", "Settings", "Return to Title" };
            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(pauseRect.x + 24f, pauseRect.y + 62f + i * 40f, pauseWidth - 48f, 34f), labels[i], selectedPauseIndex == i, body);
            }

            GUI.Label(new Rect(pauseRect.x + 24f, pauseRect.y + 194f, pauseWidth - 48f, 58f), "Hub is safe. Use reset only with the long-hold command shown in the HUD.", muted);
        }

        private void DrawMissionBriefing(GUIStyle body, GUIStyle muted)
        {
            var width = Mathf.Min(640f, Screen.width - 48f);
            var height = 390f;
            var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            FourfoldRuntimeUi.DrawPanel(rect);
            var header = FourfoldRuntimeUi.SubheadStyle(Screen.height, FourfoldRuntimeUi.SafeUiScale(progressData));
            GUI.Label(new Rect(rect.x + 26f, rect.y + 18f, rect.width - 52f, 34f), settingsOpen ? "MISSION SETTINGS" : "D-020: FIRST ECHO RUN", header);

            if (settingsOpen)
            {
                DrawSettings(rect, body, muted);
                return;
            }

            GUI.Label(new Rect(rect.x + 26f, rect.y + 58f, rect.width - 52f, 52f), "Goal: enter the sealed room, break the boss, claim two relic skills, and return to the Hub to bank them.", body);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 26f, rect.y + 120f, rect.width - 52f, 34f), "Risk: death, retry, or title return drops unbanked relic rewards.", new Color(1.0f, 0.46f, 0.22f), muted);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 26f, rect.y + 164f, rect.width - 52f, 34f), "Reward: Lumen Edge and Lumen Ward become active only after returning.", new Color(0.22f, 0.70f, 1.0f), muted);
            FourfoldRuntimeUi.DrawDivider(rect.x + 26f, rect.y + 212f, rect.width - 52f);

            var labels = new[] { "Start Run", "Settings", "Back to Hub" };
            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 34f, rect.y + 228f + i * 38f, rect.width - 68f, 32f), labels[i], selectedMissionIndex == i, body);
            }

            GUI.Label(new Rect(rect.x + 34f, rect.y + rect.height - 46f, rect.width - 68f, 30f), "Move: arrows/stick   Confirm: E/Enter/Y   Back: Esc/Backspace/Menu", muted);
        }

        private void DrawSettings(Rect rect, GUIStyle body, GUIStyle muted)
        {
            progressData = FourfoldProgressSave.Load();
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
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 24f, rect.y + 58f + i * 34f, rect.width - 48f, 30f), labels[i], selectedSettingIndex == i, body);
            }

            GUI.Label(new Rect(rect.x + 24f, rect.y + 236f, rect.width - 48f, 28f), "Left/Right changes value. E/Enter/Y or Backspace/Select returns.", muted);
        }

        private void DrawObjectiveMarker(GUIStyle style)
        {
            if (d020RegionGate == null || player == null || missionBriefingOpen || paused)
            {
                return;
            }

            var camera = fixedCamera != null ? fixedCamera : Camera.main;
            if (camera == null)
            {
                return;
            }

            var targetWorldPosition = d020RegionGate.position + new Vector3(0f, 1.15f, 0f);
            var viewport = camera.WorldToViewportPoint(targetWorldPosition);
            var behindCamera = viewport.z < 0f;
            if (behindCamera)
            {
                viewport.x = 1f - viewport.x;
                viewport.y = 1f - viewport.y;
            }

            var offscreen = behindCamera || viewport.x < 0.10f || viewport.x > 0.90f || viewport.y < 0.12f || viewport.y > 0.88f;
            var screenX = Mathf.Clamp(viewport.x * Screen.width, 80f, Screen.width - 180f);
            var screenY = Mathf.Clamp((1f - viewport.y) * Screen.height, 74f, Screen.height - 82f);
            var distance = FlatDistance(player.position, d020RegionGate.position);
            var rect = new Rect(screenX - 58f, screenY - 18f, 168f, 38f);
            FourfoldRuntimeUi.DrawPanel(rect);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 7f, rect.width - 24f, rect.height - 10f), $"{(offscreen ? "NEXT >" : "NEXT")} D-020 {distance:0}m", style);
        }
    }
}
