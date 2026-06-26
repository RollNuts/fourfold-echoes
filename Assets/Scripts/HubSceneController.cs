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

        private Vector3 facing = Vector3.forward;
        private FourfoldProgressData progressData;
        private float resetHoldSeconds;
        private bool paused;

        public static bool LayoutFitsResolution(int screenWidth, int screenHeight, bool pauseOpen, out string reason)
        {
            reason = string.Empty;
            if (screenWidth < 960 || screenHeight < 540)
            {
                reason = $"resolution too small for hub HUD: {screenWidth}x{screenHeight}";
                return false;
            }

            var topPanel = new Rect(18f, 18f, Mathf.Min(720f, screenWidth - 36f), 128f);
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
            var pauseHeight = 132f;
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
            if (Pressed(pauseKey, gamepadPauseKey))
            {
                paused = !paused;
                resetHoldSeconds = 0f;
                return;
            }

            if (paused)
            {
                if (Pressed(resetKey, gamepadResetKey))
                {
                    TryReturnToTitle();
                }

                return;
            }

            MovePlayer(Time.deltaTime);
            UpdateCamera();
            UpdateResetInput(Time.deltaTime);

            if (Pressed(interactKey, gamepadInteractKey))
            {
                TryEnterD020Region();
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
            resetHoldSeconds = 0f;

            if (Application.isPlaying)
            {
                SceneManager.LoadScene(FourfoldGameIds.UnitySceneTitle);
            }

            return true;
        }

        public void ResetProgressForNewGame()
        {
            FourfoldProgressSave.DeleteAll();
            InitializeHubProgress();
            PlacePlayerAtHubSpawn();
            resetHoldSeconds = 0f;
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
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");
            var input = new Vector3(horizontal, 0f, vertical);
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private static bool Pressed(KeyCode keyboard, KeyCode gamepad)
        {
            return Input.GetKeyDown(keyboard) || Input.GetKeyDown(gamepad);
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private void OnGUI()
        {
            var cleared = progressData != null && progressData.regionD020Cleared;
            var clearCount = progressData == null ? 0 : progressData.d020ClearCount;
            var bestTime = progressData == null || progressData.d020BestClearTimeSeconds <= 0f
                ? "--"
                : Mathf.CeilToInt(progressData.d020BestClearTimeSeconds).ToString() + "s";
            GUI.Label(new Rect(18f, 18f, 520f, 24f), "HUB: Crossroads");
            GUI.Label(new Rect(18f, 42f, 520f, 24f), cleared ? "D-020 cleared. Re-enter to improve the run." : "Objective: enter D-020 and defeat the boss.");
            GUI.Label(new Rect(18f, 66f, 520f, 24f), CanEnterD020Region() ? "Press E / Y: Enter D-020" : "Move to the gold gate to start the run.");
            GUI.Label(new Rect(18f, 90f, 520f, 24f), $"D-020 clears: {clearCount}   Best: {bestTime}");
            GUI.Label(new Rect(18f, 114f, 680f, 24f), resetHoldSeconds > 0f ? "Keep holding reset to erase progress." : "Esc/Menu: Pause   Hold Backspace / Select: Reset save");

            if (!paused)
            {
                return;
            }

            var pauseWidth = Mathf.Min(520f, Screen.width - 48f);
            var pauseHeight = 132f;
            var pauseRect = new Rect((Screen.width - pauseWidth) * 0.5f, (Screen.height - pauseHeight) * 0.5f, pauseWidth, pauseHeight);
            GUI.Box(pauseRect, GUIContent.none);
            GUI.Label(new Rect(pauseRect.x + 24f, pauseRect.y + 20f, pauseWidth - 48f, 30f), "PAUSED");
            GUI.Label(new Rect(pauseRect.x + 24f, pauseRect.y + 56f, pauseWidth - 48f, 58f), "Esc/Menu resumes. Backspace/Select returns to title.");
        }
    }
}
