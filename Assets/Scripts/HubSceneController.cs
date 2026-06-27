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
        public KeyCode gamepadConfirmKey = KeyCode.JoystickButton0;
        public KeyCode gamepadCancelKey = KeyCode.JoystickButton1;
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
        private const int PauseLoadout = 1;
        private const int PauseSettings = 2;
        private const int PauseTitle = 3;
        private const int PauseMenuCount = 4;
        private const int MissionStart = 0;
        private const int MissionLoadout = 1;
        private const int MissionSettings = 2;
        private const int MissionBack = 3;
        private const int MissionMenuCount = 4;
        private const int SummaryReplay = 0;
        private const int SummaryContinue = 1;
        private const int SummaryTitle = 2;
        private const int SummaryMenuCount = 3;
        private const int ResetConfirmReset = 0;
        private const int ResetConfirmCancel = 1;
        private const int ResetConfirmMenuCount = 2;
        private const int LoadoutEdge = 0;
        private const int LoadoutWard = 1;
        private const int LoadoutBack = 2;
        private const int LoadoutMenuCount = 3;
        private const int SettingsCount = 6;
        private const float AxisRepeatDelay = 0.24f;

        private Vector3 facing = Vector3.forward;
        private FourfoldProgressData progressData;
        private float resetHoldSeconds;
        private bool paused;
        private bool settingsOpen;
        private bool loadoutOpen;
        private bool missionBriefingOpen;
        private bool runSummaryOpen;
        private bool resetConfirmOpen;
        private bool settingsOpenedFromMissionBriefing;
        private bool loadoutOpenedFromMissionBriefing;
        private int selectedPauseIndex;
        private int selectedMissionIndex;
        private int selectedSummaryIndex = SummaryContinue;
        private int selectedResetIndex = ResetConfirmCancel;
        private int selectedLoadoutIndex;
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

            var topPanel = new Rect(18f, 18f, Mathf.Min(760f, screenWidth - 36f), 270f);
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
            var pauseHeight = 430f;
            var pauseRect = new Rect((screenWidth - pauseWidth) * 0.5f, (screenHeight - pauseHeight) * 0.5f, pauseWidth, pauseHeight);
            if (pauseRect.x < 24f || pauseRect.y < 24f || pauseRect.xMax > screenWidth - 24f || pauseRect.yMax > screenHeight - 24f)
            {
                reason = $"hub pause panel exceeds safe area at {screenWidth}x{screenHeight}: {pauseRect}";
                return false;
            }

            var resetWidth = Mathf.Min(560f, screenWidth - 48f);
            var resetHeight = 262f;
            var resetRect = new Rect((screenWidth - resetWidth) * 0.5f, (screenHeight - resetHeight) * 0.5f, resetWidth, resetHeight);
            if (resetRect.x < 24f || resetRect.y < 24f || resetRect.xMax > screenWidth - 24f || resetRect.yMax > screenHeight - 24f)
            {
                reason = $"hub reset confirmation panel exceeds safe area at {screenWidth}x{screenHeight}: {resetRect}";
                return false;
            }

            var summaryWidth = Mathf.Min(660f, screenWidth - 48f);
            var summaryHeight = 402f;
            var summaryRect = new Rect((screenWidth - summaryWidth) * 0.5f, (screenHeight - summaryHeight) * 0.5f, summaryWidth, summaryHeight);
            if (summaryRect.x < 24f || summaryRect.y < 24f || summaryRect.xMax > screenWidth - 24f || summaryRect.yMax > screenHeight - 24f)
            {
                reason = $"hub result panel exceeds safe area at {screenWidth}x{screenHeight}: {summaryRect}";
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
            FourfoldInputPrompts.ObserveFrameInput();
            axisRepeatTimer = Mathf.Max(0f, axisRepeatTimer - Time.unscaledDeltaTime);
            if (Pressed(pauseKey, gamepadPauseKey))
            {
                if (settingsOpen)
                {
                    CloseSettings();
                    return;
                }

                if (loadoutOpen)
                {
                    CloseLoadout();
                    return;
                }

                if (resetConfirmOpen)
                {
                    CloseResetConfirmation();
                    return;
                }

                if (missionBriefingOpen)
                {
                    CloseMissionBriefing();
                    return;
                }

                if (runSummaryOpen)
                {
                    DismissRunSummary();
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

            if (runSummaryOpen)
            {
                UpdateRunSummaryInput();
                return;
            }

            if (resetConfirmOpen)
            {
                UpdateResetConfirmationInput();
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
            runSummaryOpen = ShouldOpenRunSummary(progressData);
            selectedSummaryIndex = SummaryContinue;
        }

        public bool TryEnterD020Region()
        {
            if (!CanEnterD020Region())
            {
                return false;
            }

            return StartD020Region();
        }

        private bool StartD020Region()
        {
            missionBriefingOpen = false;
            runSummaryOpen = false;
            resetConfirmOpen = false;
            settingsOpenedFromMissionBriefing = false;
            loadoutOpenedFromMissionBriefing = false;
            loadoutOpen = false;
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
            loadoutOpen = false;
            missionBriefingOpen = false;
            runSummaryOpen = false;
            resetConfirmOpen = false;
            settingsOpenedFromMissionBriefing = false;
            loadoutOpenedFromMissionBriefing = false;
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
            loadoutOpen = false;
            missionBriefingOpen = false;
            runSummaryOpen = false;
            resetConfirmOpen = false;
            settingsOpenedFromMissionBriefing = false;
            loadoutOpenedFromMissionBriefing = false;
            paused = false;
        }

        public void OpenSettings()
        {
            progressData = FourfoldProgressSave.Load();
            paused = true;
            runSummaryOpen = false;
            resetConfirmOpen = false;
            loadoutOpen = false;
            settingsOpen = true;
            settingsOpenedFromMissionBriefing = false;
            loadoutOpenedFromMissionBriefing = false;
            selectedSettingIndex = 0;
        }

        public void OpenLoadout()
        {
            progressData = FourfoldProgressSave.Load();
            paused = true;
            runSummaryOpen = false;
            resetConfirmOpen = false;
            settingsOpen = false;
            loadoutOpen = true;
            settingsOpenedFromMissionBriefing = false;
            loadoutOpenedFromMissionBriefing = false;
            selectedLoadoutIndex = 0;
        }

        public void OpenMissionBriefing()
        {
            if (!CanEnterD020Region())
            {
                return;
            }

            progressData = FourfoldProgressSave.Load();
            missionBriefingOpen = true;
            runSummaryOpen = false;
            resetConfirmOpen = false;
            settingsOpen = false;
            loadoutOpen = false;
            settingsOpenedFromMissionBriefing = false;
            loadoutOpenedFromMissionBriefing = false;
            paused = false;
            selectedMissionIndex = 0;
            resetHoldSeconds = 0f;
        }

        public void CloseMissionBriefing()
        {
            missionBriefingOpen = false;
            settingsOpen = false;
            loadoutOpen = false;
            settingsOpenedFromMissionBriefing = false;
            loadoutOpenedFromMissionBriefing = false;
            resetHoldSeconds = 0f;
        }

        public bool IsMissionBriefingOpen()
        {
            return missionBriefingOpen;
        }

        public bool IsRunSummaryOpen()
        {
            return runSummaryOpen;
        }

        public void DismissRunSummary()
        {
            if (progressData == null)
            {
                progressData = FourfoldProgressSave.Load();
            }

            progressData.d020AcknowledgedClearCount = Mathf.Max(progressData.d020AcknowledgedClearCount, progressData.d020ClearCount);
            FourfoldProgressSave.Save(progressData);
            runSummaryOpen = false;
            selectedSummaryIndex = SummaryContinue;
        }

        public bool IsResetConfirmationOpen()
        {
            return resetConfirmOpen;
        }

        private void OpenResetConfirmation()
        {
            resetConfirmOpen = true;
            paused = false;
            missionBriefingOpen = false;
            runSummaryOpen = false;
            settingsOpen = false;
            loadoutOpen = false;
            selectedResetIndex = ResetConfirmCancel;
            resetHoldSeconds = 0f;
        }

        private void CloseResetConfirmation()
        {
            resetConfirmOpen = false;
            selectedResetIndex = ResetConfirmCancel;
            resetHoldSeconds = 0f;
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

        public void CloseLoadout()
        {
            loadoutOpen = false;
            if (loadoutOpenedFromMissionBriefing)
            {
                paused = false;
                missionBriefingOpen = true;
                loadoutOpenedFromMissionBriefing = false;
            }
        }

        public bool IsLoadoutOpen()
        {
            return loadoutOpen;
        }

        public bool ToggleLumenEdgeLoadout()
        {
            progressData = FourfoldProgressSave.Load();
            if (!progressData.d020RewardClaimed)
            {
                return false;
            }

            progressData.d020EdgeEquipped = !progressData.d020EdgeEquipped;
            progressData.d020LoadoutInitialized = true;
            FourfoldProgressSave.Save(progressData);
            return true;
        }

        public bool ToggleLumenWardLoadout()
        {
            progressData = FourfoldProgressSave.Load();
            if (!progressData.d020SecondRewardClaimed)
            {
                return false;
            }

            progressData.d020WardEquipped = !progressData.d020WardEquipped;
            progressData.d020LoadoutInitialized = true;
            FourfoldProgressSave.Save(progressData);
            return true;
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
                case 4:
                    progressData.language = FourfoldLanguage.Toggle(progressData.language);
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
                    OpenResetConfirmation();
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

            if (loadoutOpen)
            {
                UpdateLoadoutInput();
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

            if (Pressed(interactKey, KeyCode.Return, gamepadInteractKey, gamepadConfirmKey))
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

            if (loadoutOpen)
            {
                UpdateLoadoutInput();
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

            if (Pressed(resetKey, pauseKey, gamepadResetKey, gamepadPauseKey, gamepadCancelKey))
            {
                CloseMissionBriefing();
                return;
            }

            if (Pressed(interactKey, KeyCode.Return, gamepadInteractKey, gamepadConfirmKey))
            {
                ActivateMissionSelection();
            }
        }

        private void UpdateRunSummaryInput()
        {
            if (Pressed(KeyCode.UpArrow, KeyCode.W) || AxisPressed(1f))
            {
                selectedSummaryIndex = Wrap(selectedSummaryIndex - 1, SummaryMenuCount);
            }
            else if (Pressed(KeyCode.DownArrow, KeyCode.S) || AxisPressed(-1f))
            {
                selectedSummaryIndex = Wrap(selectedSummaryIndex + 1, SummaryMenuCount);
            }

            if (Pressed(resetKey, pauseKey, gamepadResetKey, gamepadPauseKey, gamepadCancelKey))
            {
                DismissRunSummary();
                return;
            }

            if (Pressed(interactKey, KeyCode.Return, gamepadInteractKey, gamepadConfirmKey))
            {
                ActivateRunSummarySelection();
            }
        }

        private void UpdateResetConfirmationInput()
        {
            if (Pressed(KeyCode.UpArrow, KeyCode.W) || AxisPressed(1f))
            {
                selectedResetIndex = Wrap(selectedResetIndex - 1, ResetConfirmMenuCount);
            }
            else if (Pressed(KeyCode.DownArrow, KeyCode.S) || AxisPressed(-1f))
            {
                selectedResetIndex = Wrap(selectedResetIndex + 1, ResetConfirmMenuCount);
            }

            if (Pressed(resetKey, pauseKey, gamepadResetKey, gamepadPauseKey, gamepadCancelKey))
            {
                CloseResetConfirmation();
                return;
            }

            if (Pressed(interactKey, KeyCode.Return, gamepadInteractKey, gamepadConfirmKey))
            {
                ActivateResetConfirmationSelection();
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

            if (Pressed(interactKey, KeyCode.Return, gamepadInteractKey, gamepadConfirmKey) || Pressed(resetKey, gamepadResetKey, gamepadCancelKey))
            {
                CloseSettings();
            }
        }

        private void UpdateLoadoutInput()
        {
            if (Pressed(KeyCode.UpArrow, KeyCode.W) || AxisPressed(1f))
            {
                selectedLoadoutIndex = Wrap(selectedLoadoutIndex - 1, LoadoutMenuCount);
            }
            else if (Pressed(KeyCode.DownArrow, KeyCode.S) || AxisPressed(-1f))
            {
                selectedLoadoutIndex = Wrap(selectedLoadoutIndex + 1, LoadoutMenuCount);
            }

            if (Pressed(resetKey, pauseKey, gamepadResetKey, gamepadPauseKey, gamepadCancelKey))
            {
                CloseLoadout();
                return;
            }

            if (Pressed(interactKey, KeyCode.Return, gamepadInteractKey, gamepadConfirmKey))
            {
                ActivateLoadoutSelection();
            }
        }

        private void ActivatePauseSelection()
        {
            switch (selectedPauseIndex)
            {
                case PauseResume:
                    paused = false;
                    settingsOpen = false;
                    loadoutOpen = false;
                    break;
                case PauseLoadout:
                    OpenLoadout();
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
                case MissionLoadout:
                    progressData = FourfoldProgressSave.Load();
                    loadoutOpen = true;
                    loadoutOpenedFromMissionBriefing = true;
                    settingsOpen = false;
                    selectedLoadoutIndex = 0;
                    break;
                case MissionSettings:
                    progressData = FourfoldProgressSave.Load();
                    settingsOpen = true;
                    settingsOpenedFromMissionBriefing = true;
                    loadoutOpen = false;
                    selectedSettingIndex = 0;
                    break;
                case MissionBack:
                    CloseMissionBriefing();
                    break;
            }
        }

        private void ActivateRunSummarySelection()
        {
            switch (selectedSummaryIndex)
            {
                case SummaryReplay:
                    DismissRunSummary();
                    StartD020Region();
                    break;
                case SummaryContinue:
                    DismissRunSummary();
                    break;
                case SummaryTitle:
                    DismissRunSummary();
                    TryReturnToTitle();
                    break;
            }
        }

        private void ActivateResetConfirmationSelection()
        {
            if (selectedResetIndex == ResetConfirmReset)
            {
                ResetProgressForNewGame();
                return;
            }

            CloseResetConfirmation();
        }

        private void ActivateLoadoutSelection()
        {
            progressData = FourfoldProgressSave.Load();
            switch (selectedLoadoutIndex)
            {
                case LoadoutEdge:
                    ToggleLumenEdgeLoadout();
                    break;
                case LoadoutWard:
                    ToggleLumenWardLoadout();
                    break;
                default:
                    CloseLoadout();
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

        private static bool ShouldOpenRunSummary(FourfoldProgressData data)
        {
            return data != null
                && data.regionD020Cleared
                && data.d020ReturnedToHub
                && data.d020ClearCount > data.d020AcknowledgedClearCount;
        }

        private static int SavedRewardCount(FourfoldProgressData data)
        {
            if (data == null)
            {
                return 0;
            }

            var count = data.d020RewardClaimed ? 1 : 0;
            if (data.d020SecondRewardClaimed)
            {
                count += 1;
            }

            return count;
        }

        private static int EquippedRewardCount(FourfoldProgressData data)
        {
            if (data == null)
            {
                return 0;
            }

            var count = data.d020RewardClaimed && data.d020EdgeEquipped ? 1 : 0;
            if (data.d020SecondRewardClaimed && data.d020WardEquipped)
            {
                count += 1;
            }

            return count;
        }

        private static string LoadoutSummary(FourfoldProgressData data)
        {
            var saved = SavedRewardCount(data);
            var equipped = EquippedRewardCount(data);
            return FourfoldLanguage.T(data, $"Loadout {equipped}/{saved} equipped", $"ロードアウト 装備 {equipped}/{saved}");
        }

        private static string LoadoutEffectText(FourfoldProgressData data)
        {
            if (data == null || EquippedRewardCount(data) == 0)
            {
                return FourfoldLanguage.T(data, "base build", "基礎ビルド");
            }

            if (data.d020EdgeEquipped && data.d020WardEquipped)
            {
                return FourfoldLanguage.T(data, "Lumen Link: stronger attacks, reduced damage taken, hit recovery", "Lumen Link: 攻撃強化、被ダメージ軽減、命中回復");
            }

            if (data.d020EdgeEquipped)
            {
                return FourfoldLanguage.T(data, "stronger attacks", "攻撃強化");
            }

            return FourfoldLanguage.T(data, "reduced damage taken", "被ダメージ軽減");
        }

        private static string LoadoutSynergyText(FourfoldProgressData data)
        {
            var edgeActive = data != null && data.d020RewardClaimed && data.d020EdgeEquipped;
            var wardActive = data != null && data.d020SecondRewardClaimed && data.d020WardEquipped;
            if (edgeActive && wardActive)
            {
                return FourfoldLanguage.T(data, "Current synergy: Lumen Link = Edge + Ward, stronger attacks, less damage, hit recovery.", "現在のシナジー: Lumen Link = Edge + Ward。攻撃強化、被ダメージ軽減、命中回復。");
            }

            if (edgeActive)
            {
                return FourfoldLanguage.T(data, "Current synergy: Edge only, stronger attacks; add Ward for Lumen Link recovery.", "現在のシナジー: Edgeのみで攻撃強化。Ward追加でLumen Link回復。");
            }

            if (wardActive)
            {
                return FourfoldLanguage.T(data, "Current synergy: Ward only, less damage; add Edge for Lumen Link recovery.", "現在のシナジー: Wardのみで被ダメージ軽減。Edge追加でLumen Link回復。");
            }

            if (SavedRewardCount(data) > 0)
            {
                return FourfoldLanguage.T(data, "Current synergy: none equipped; turn on saved skills for R01 bonuses.", "現在のシナジー: 装備なし。保存済みスキルをONにするとR01で発動。");
            }

            return FourfoldLanguage.T(data, "Current synergy: base kit; R01 rewards activate after a hub return.", "現在のシナジー: 基礎ビルド。R01報酬はハブ帰還後に有効。");
        }

        private static string MissionRewardRiskText(FourfoldProgressData data)
        {
            return FourfoldLanguage.T(data, "Loss risk: new R01 rewards save on hub return; fail or leave before return loses them.", "喪失リスク: R01新報酬はハブ帰還で保存。失敗や帰還前の離脱で失う。");
        }

        private static string LoadoutRowLabel(FourfoldProgressData data, int index)
        {
            switch (index)
            {
                case LoadoutEdge:
                    if (data != null && data.d020RewardClaimed)
                    {
                        return FourfoldLanguage.T(data, $"[{(data.d020EdgeEquipped ? "ON" : "OFF")}] Lumen Edge - attacks hit harder", $"[{(data.d020EdgeEquipped ? "ON" : "OFF")}] Lumen Edge - 攻撃が強くなる");
                    }

                    return FourfoldLanguage.T(data, "[LOCKED] Lumen Edge - clear reward 1", "[未取得] Lumen Edge - クリア報酬1");
                case LoadoutWard:
                    if (data != null && data.d020SecondRewardClaimed)
                    {
                        return FourfoldLanguage.T(data, $"[{(data.d020WardEquipped ? "ON" : "OFF")}] Lumen Ward - damage taken drops", $"[{(data.d020WardEquipped ? "ON" : "OFF")}] Lumen Ward - 被ダメージを軽減");
                    }

                    return FourfoldLanguage.T(data, "[LOCKED] Lumen Ward - clear reward 2", "[未取得] Lumen Ward - クリア報酬2");
                default:
                    return FourfoldLanguage.T(data, "Back", "戻る");
            }
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
            var rewards = SavedRewardCount(progressData);
            var equippedRewards = EquippedRewardCount(progressData);
            var bestTime = progressData == null || progressData.d020BestClearTimeSeconds <= 0f
                ? "--"
                : Mathf.CeilToInt(progressData.d020BestClearTimeSeconds).ToString() + "s";
            var canStartRegion = CanEnterD020Region();
            var panel = new Rect(18f, 18f, Mathf.Min(760f, Screen.width - 36f), 270f);
            FourfoldRuntimeUi.DrawPanel(panel);
            var header = FourfoldRuntimeUi.SubheadStyle(Screen.height, uiScale);
            var body = FourfoldRuntimeUi.BodyStyle(Screen.height, uiScale);
            var muted = FourfoldRuntimeUi.MutedStyle(Screen.height, uiScale);
            var hubPhase = cleared
                ? FourfoldLanguage.T(progressData, "RESULT", "結果")
                : canStartRegion
                    ? FourfoldLanguage.T(progressData, "READY", "準備完了")
                    : FourfoldLanguage.T(progressData, "PREPARE", "準備");
            GUI.Label(new Rect(panel.x + 18f, panel.y + 12f, panel.width - 36f, 34f), FourfoldLanguage.T(progressData, $"HUB: Crossroads - {hubPhase}", $"ハブ: 交差路 - {hubPhase}"), header);
            var hubStatus = cleared
                ? FourfoldLanguage.T(progressData, "Region 01 result is saved. Reward skills are active; replay is for time, mastery, or another clear.", "地域01の結果は保存済み。報酬スキルは有効。再挑戦はタイム、習熟、追加クリアのため。")
                : FourfoldLanguage.T(progressData, "Prepare here, start Region 01 at the gold gate, then return after a clear to save rewards.", "ここで準備し、金色のゲートから地域01を始める。クリア後に帰還すると報酬が保存される。");
            GUI.Label(new Rect(panel.x + 18f, panel.y + 48f, panel.width - 36f, 48f), hubStatus, body);
            FourfoldRuntimeUi.DrawChip(new Rect(panel.x + 18f, panel.y + 100f, 230f, 34f), FourfoldLanguage.T(progressData, $"Clears {clearCount}   Best {bestTime}", $"クリア {clearCount}   最速 {bestTime}"), new Color(1.0f, 0.72f, 0.24f), muted);
            FourfoldRuntimeUi.DrawChip(new Rect(panel.x + 260f, panel.y + 100f, 220f, 34f), FourfoldLanguage.T(progressData, $"Rewards saved {rewards}/2", $"保存報酬 {rewards}/2"), new Color(0.22f, 0.70f, 1.0f), muted);
            FourfoldRuntimeUi.DrawChip(new Rect(panel.x + 492f, panel.y + 100f, panel.width - 510f, 34f), FourfoldLanguage.T(progressData, $"Failures {(progressData == null ? 0 : progressData.d020FailureCount)}", $"失敗 {(progressData == null ? 0 : progressData.d020FailureCount)}"), new Color(1.0f, 0.46f, 0.22f), muted);
            FourfoldRuntimeUi.DrawChip(new Rect(panel.x + 18f, panel.y + 142f, panel.width - 36f, 34f), FourfoldLanguage.T(progressData, $"Equipped reward skills {equippedRewards}/{rewards}: {LoadoutEffectText(progressData)}", $"装備中報酬スキル {equippedRewards}/{rewards}: {LoadoutEffectText(progressData)}"), new Color(0.62f, 0.44f, 1.0f), muted);
            var prompt = canStartRegion
                ? FourfoldInputPrompts.HubStartReady(progressData)
                : FourfoldLanguage.T(progressData, "PREP: move to the gold gate when you are ready to start.", "準備: 開始できる状態になったら金色のゲートへ。");
            var next = cleared
                ? FourfoldLanguage.T(progressData, "RESULT OPTIONS: replay Region 01, compare best time, or return to title from Pause.", "結果の選択: 地域01再挑戦、最速タイム比較、またはポーズからタイトルへ。")
                : FourfoldLanguage.T(progressData, "RUN PLAN: tune loadout, open route, beat boss, claim skills, return.", "攻略手順: 装備調整、道を開く、ボス撃破、スキル獲得、帰還。");
            FourfoldRuntimeUi.DrawChip(new Rect(panel.x + 18f, panel.y + 184f, panel.width - 36f, 34f), prompt, canStartRegion ? new Color(0.34f, 0.90f, 0.52f) : new Color(0.25f, 0.68f, 1.0f), muted);
            GUI.Label(new Rect(panel.x + 18f, panel.y + 226f, panel.width - 36f, 24f), next, muted);
            if (progressData == null || progressData.showControlHints)
            {
                GUI.Label(new Rect(panel.x + 18f, panel.y + 250f, panel.width - 36f, 20f), FourfoldInputPrompts.HubHud(progressData, resetHoldSeconds > 0f), muted);
            }

            DrawObjectiveMarker(body);

            if (resetConfirmOpen)
            {
                DrawResetConfirmation(body, muted);
                return;
            }

            if (runSummaryOpen)
            {
                DrawRunSummary(body, muted);
                return;
            }

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
            var pauseHeight = loadoutOpen ? 430f : settingsOpen ? 360f : 340f;
            var pauseRect = new Rect((Screen.width - pauseWidth) * 0.5f, (Screen.height - pauseHeight) * 0.5f, pauseWidth, pauseHeight);
            FourfoldRuntimeUi.DrawPanel(pauseRect);
            var pauseTitle = settingsOpen
                ? FourfoldLanguage.T(progressData, "SETTINGS", "設定")
                : loadoutOpen
                    ? FourfoldLanguage.T(progressData, "LOADOUT", "ロードアウト")
                    : FourfoldLanguage.T(progressData, "PAUSED", "ポーズ");
            GUI.Label(new Rect(pauseRect.x + 24f, pauseRect.y + 18f, pauseWidth - 48f, 30f), pauseTitle, header);

            if (settingsOpen)
            {
                DrawSettings(pauseRect, body, muted);
                return;
            }

            if (loadoutOpen)
            {
                DrawLoadout(pauseRect, body, muted);
                return;
            }

            var labels = new[]
            {
                FourfoldLanguage.T(progressData, "Resume", "再開"),
                FourfoldLanguage.T(progressData, "Loadout", "ロードアウト"),
                FourfoldLanguage.T(progressData, "Settings", "設定"),
                FourfoldLanguage.T(progressData, "Return to Title", "タイトルへ戻る")
            };
            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(pauseRect.x + 24f, pauseRect.y + 62f + i * 40f, pauseWidth - 48f, 34f), labels[i], selectedPauseIndex == i, body);
            }

            GUI.Label(new Rect(pauseRect.x + 24f, pauseRect.y + 232f, pauseWidth - 48f, 78f), FourfoldLanguage.T(progressData, "Hub is the preparation screen: choose reward skills before starting the next region attempt.", "ハブは準備画面。次の地域攻略前に報酬スキルを選ぶ。"), muted);
        }

        private void DrawRunSummary(GUIStyle body, GUIStyle muted)
        {
            var width = Mathf.Min(660f, Screen.width - 48f);
            var height = 402f;
            var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            FourfoldRuntimeUi.DrawPanel(rect);
            var header = FourfoldRuntimeUi.SubheadStyle(Screen.height, FourfoldRuntimeUi.SafeUiScale(progressData));
            var bestTime = progressData == null || progressData.d020BestClearTimeSeconds <= 0f
                ? "--"
                : Mathf.CeilToInt(progressData.d020BestClearTimeSeconds).ToString() + "s";
            var clearCount = progressData == null ? 0 : progressData.d020ClearCount;
            var failures = progressData == null ? 0 : progressData.d020FailureCount;
            var rewards = progressData == null ? 0 : (progressData.d020RewardClaimed ? 1 : 0) + (progressData.d020SecondRewardClaimed ? 1 : 0);

            GUI.Label(new Rect(rect.x + 26f, rect.y + 18f, rect.width - 52f, 34f), FourfoldLanguage.T(progressData, "REGION CLEARED", "地域クリア"), header);
            GUI.Label(new Rect(rect.x + 26f, rect.y + 58f, rect.width - 52f, 50f), FourfoldLanguage.T(progressData, "Region 01 is complete. The hub received the result, and reward skills are saved for future attempts.", "地域01完了。ハブが結果を受け取り、報酬スキルは次回以降のため保存された。"), body);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 26f, rect.y + 116f, (rect.width - 64f) * 0.50f, 36f), FourfoldLanguage.T(progressData, $"Rewards active {rewards}/2", $"有効報酬 {rewards}/2"), new Color(0.22f, 0.70f, 1.0f), muted);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 38f + (rect.width - 64f) * 0.50f, rect.y + 116f, (rect.width - 64f) * 0.50f, 36f), FourfoldLanguage.T(progressData, $"Clears {clearCount}   Best {bestTime}", $"クリア {clearCount}   最速 {bestTime}"), new Color(1.0f, 0.72f, 0.24f), muted);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 26f, rect.y + 162f, rect.width - 52f, 36f), FourfoldLanguage.T(progressData, $"Failed attempts {failures}. Saved rewards remain active.", $"失敗 {failures}。保存済みの報酬は有効なまま。"), new Color(1.0f, 0.46f, 0.22f), muted);
            FourfoldRuntimeUi.DrawDivider(rect.x + 26f, rect.y + 214f, rect.width - 52f);

            var labels = new[]
            {
                FourfoldLanguage.T(progressData, "Replay Region 01", "地域01を再挑戦"),
                FourfoldLanguage.T(progressData, "Continue in Hub", "ハブで続ける"),
                FourfoldLanguage.T(progressData, "Return to Title", "タイトルへ戻る")
            };
            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 34f, rect.y + 230f + i * 38f, rect.width - 68f, 32f), labels[i], selectedSummaryIndex == i, body);
            }

            GUI.Label(new Rect(rect.x + 34f, rect.y + rect.height - 48f, rect.width - 68f, 30f), FourfoldInputPrompts.HubPanel(progressData), muted);
        }

        private void DrawResetConfirmation(GUIStyle body, GUIStyle muted)
        {
            var width = Mathf.Min(560f, Screen.width - 48f);
            var height = 262f;
            var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            FourfoldRuntimeUi.DrawPanel(rect);
            var header = FourfoldRuntimeUi.SubheadStyle(Screen.height, FourfoldRuntimeUi.SafeUiScale(progressData));
            GUI.Label(new Rect(rect.x + 26f, rect.y + 18f, rect.width - 52f, 34f), FourfoldLanguage.T(progressData, "RESET SAVE?", "セーブをリセット？"), header);
            GUI.Label(new Rect(rect.x + 26f, rect.y + 58f, rect.width - 52f, 58f), FourfoldLanguage.T(progressData, "This erases clears, saved reward skills, best time, and failure count. Audio/UI settings are kept.", "クリア、保存済み報酬スキル、最速タイム、失敗回数を消去する。音量/UI設定は保持される。"), body);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 26f, rect.y + 126f, rect.width - 52f, 34f), FourfoldLanguage.T(progressData, "This cannot be undone from inside the game.", "ゲーム内では取り消せない。"), new Color(1.0f, 0.46f, 0.22f), muted);

            var labels = new[] { FourfoldLanguage.T(progressData, "Reset Save", "リセットする"), FourfoldLanguage.T(progressData, "Cancel", "キャンセル") };
            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 34f, rect.y + 176f + i * 34f, rect.width - 68f, 30f), labels[i], selectedResetIndex == i, body);
            }

            GUI.Label(new Rect(rect.x + 34f, rect.y + rect.height - 32f, rect.width - 68f, 24f), FourfoldInputPrompts.HubConfirm(progressData), muted);
        }

        private void DrawMissionBriefing(GUIStyle body, GUIStyle muted)
        {
            var width = Mathf.Min(640f, Screen.width - 48f);
            var height = loadoutOpen ? 430f : 456f;
            var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            FourfoldRuntimeUi.DrawPanel(rect);
            var header = FourfoldRuntimeUi.SubheadStyle(Screen.height, FourfoldRuntimeUi.SafeUiScale(progressData));
            var title = settingsOpen
                ? FourfoldLanguage.T(progressData, "REGION SETTINGS", "地域設定")
                : loadoutOpen
                    ? FourfoldLanguage.T(progressData, "REGION LOADOUT", "地域ロードアウト")
                    : FourfoldLanguage.T(progressData, "REGION 01: VERDANT STEPS", "地域01: 緑陰の段丘");
            GUI.Label(new Rect(rect.x + 26f, rect.y + 18f, rect.width - 52f, 34f), title, header);

            if (settingsOpen)
            {
                DrawSettings(rect, body, muted);
                return;
            }

            if (loadoutOpen)
            {
                DrawLoadout(rect, body, muted);
                return;
            }

            GUI.Label(new Rect(rect.x + 26f, rect.y + 58f, rect.width - 52f, 52f), FourfoldLanguage.T(progressData, "Goal: use the exploration tool, defeat the boss, claim two reward skills, and return to the hub to save the result.", "目標: 探索ツールを使い、ボスを倒し、2つの報酬スキルを得て、ハブへ戻って結果を保存する。"), body);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 26f, rect.y + 120f, rect.width - 52f, 34f), FourfoldLanguage.T(progressData, $"Equipped reward skills {EquippedRewardCount(progressData)}/{SavedRewardCount(progressData)}: {LoadoutEffectText(progressData)}", $"装備中報酬スキル {EquippedRewardCount(progressData)}/{SavedRewardCount(progressData)}: {LoadoutEffectText(progressData)}"), new Color(0.62f, 0.44f, 1.0f), muted);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 26f, rect.y + 164f, rect.width - 52f, 34f), LoadoutSynergyText(progressData), new Color(0.62f, 0.44f, 1.0f), muted);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 26f, rect.y + 208f, rect.width - 52f, 34f), MissionRewardRiskText(progressData), new Color(1.0f, 0.46f, 0.22f), muted);
            FourfoldRuntimeUi.DrawDivider(rect.x + 26f, rect.y + 256f, rect.width - 52f);

            var labels = new[]
            {
                FourfoldLanguage.T(progressData, "Start Region", "地域へ入る"),
                FourfoldLanguage.T(progressData, "Loadout", "ロードアウト"),
                FourfoldLanguage.T(progressData, "Settings", "設定"),
                FourfoldLanguage.T(progressData, "Back to Hub", "ハブへ戻る")
            };
            for (var i = 0; i < labels.Length; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 34f, rect.y + 272f + i * 34f, rect.width - 68f, 30f), labels[i], selectedMissionIndex == i, body);
            }

            GUI.Label(new Rect(rect.x + 34f, rect.y + rect.height - 46f, rect.width - 68f, 30f), FourfoldInputPrompts.HubPanel(progressData), muted);
        }

        private void DrawLoadout(Rect rect, GUIStyle body, GUIStyle muted)
        {
            progressData = FourfoldProgressSave.Load();
            GUI.Label(new Rect(rect.x + 24f, rect.y + 58f, rect.width - 48f, 48f), FourfoldLanguage.T(progressData, "Equip saved reward skills before starting a region. Locked skills must be earned and saved by returning to the hub.", "地域へ入る前に、保存済み報酬スキルを装備する。未取得のスキルは、獲得してハブへ帰還すると保存される。"), body);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 24f, rect.y + 112f, rect.width - 48f, 34f), LoadoutSummary(progressData) + " - " + LoadoutEffectText(progressData), new Color(0.62f, 0.44f, 1.0f), muted);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 24f, rect.y + 154f, rect.width - 48f, 34f), LoadoutSynergyText(progressData), new Color(0.62f, 0.44f, 1.0f), muted);
            FourfoldRuntimeUi.DrawChip(new Rect(rect.x + 24f, rect.y + 196f, rect.width - 48f, 34f), MissionRewardRiskText(progressData), new Color(1.0f, 0.46f, 0.22f), muted);
            FourfoldRuntimeUi.DrawDivider(rect.x + 24f, rect.y + 246f, rect.width - 48f);

            for (var i = 0; i < LoadoutMenuCount; i++)
            {
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 32f, rect.y + 260f + i * 38f, rect.width - 64f, 34f), LoadoutRowLabel(progressData, i), selectedLoadoutIndex == i, body);
            }

            GUI.Label(new Rect(rect.x + 32f, rect.y + rect.height - 54f, rect.width - 64f, 42f), FourfoldLanguage.T(progressData, "Confirm toggles equipped saved skills. Locked rows are read-only.", "決定で保存済みスキルの装備を切り替える。未取得行は確認のみ。") + "\n" + FourfoldInputPrompts.HubConfirm(progressData), muted);
        }

        private void DrawSettings(Rect rect, GUIStyle body, GUIStyle muted)
        {
            progressData = FourfoldProgressSave.Load();
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
                FourfoldRuntimeUi.DrawSelectableRow(new Rect(rect.x + 24f, rect.y + 58f + i * 34f, rect.width - 48f, 30f), labels[i], selectedSettingIndex == i, body);
            }

            GUI.Label(new Rect(rect.x + 24f, rect.y + rect.height - 42f, rect.width - 48f, 28f), FourfoldInputPrompts.SharedSettings(progressData), muted);
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
            var prefix = offscreen
                ? FourfoldLanguage.T(progressData, "START >", "開始 >")
                : FourfoldLanguage.T(progressData, "START", "開始");
            GUI.Label(new Rect(rect.x + 12f, rect.y + 7f, rect.width - 24f, rect.height - 10f), $"{prefix} R01 {distance:0}m", style);
        }
    }
}
