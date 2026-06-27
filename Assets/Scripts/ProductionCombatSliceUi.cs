using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FourfoldEchoes.Product
{
    [DisallowMultipleComponent]
    public sealed class ProductionCombatSliceUi : MonoBehaviour
    {
        public ProductionCombatSliceController controller;

        private enum ScreenState
        {
            None,
            Title,
            Playing,
            Paused,
            Retry,
            Complete
        }

        private static readonly Color OverlayBackground = new Color(0.02f, 0.025f, 0.03f, 0.78f);
        private static readonly Color PanelBackground = new Color(0.05f, 0.06f, 0.07f, 0.9f);
        private static readonly Color TextColor = new Color(0.92f, 0.9f, 0.84f, 1f);
        private static readonly Color MutedTextColor = new Color(0.68f, 0.72f, 0.68f, 1f);
        private static readonly Color AccentColor = new Color(0.92f, 0.67f, 0.28f, 1f);
        private static readonly Color ConfirmColor = new Color(0.34f, 0.78f, 0.48f, 1f);
        private static readonly Color WarningColor = new Color(0.86f, 0.34f, 0.26f, 1f);
        private static readonly Color BossColor = new Color(0.62f, 0.55f, 0.92f, 1f);
        private static readonly Color TrackColor = new Color(0.14f, 0.16f, 0.16f, 1f);
        private static readonly Color ButtonColor = new Color(0.13f, 0.15f, 0.16f, 1f);
        private static readonly Color SelectedButtonColor = new Color(0.22f, 0.24f, 0.22f, 1f);
        private static readonly Color BorderColor = new Color(0.26f, 0.29f, 0.28f, 1f);

        private const float NavigationRepeatSeconds = 0.18f;
        private UIDocument document;
        private PanelSettings panelSettings;
        private ThemeStyleSheet runtimeThemeStyleSheet;
        private VisualElement root;
        private VisualElement hud;
        private VisualElement titleOverlay;
        private VisualElement pauseOverlay;
        private VisualElement retryOverlay;
        private VisualElement completeOverlay;
        private VisualElement heroFill;
        private VisualElement wardensFill;
        private VisualElement bossFill;
        private VisualElement toolFill;
        private Label titleSaveLabel;
        private Label eventLabel;
        private Label statusLabel;
        private Label toolLabel;
        private Label saveLabel;
        private Label completeSaveLabel;
        private Button startButton;
        private readonly List<Button> titleButtons = new List<Button>();
        private readonly List<Button> pauseButtons = new List<Button>();
        private readonly List<Button> retryButtons = new List<Button>();
        private readonly List<Button> completeButtons = new List<Button>();
        private readonly Dictionary<Button, Action> buttonActions = new Dictionary<Button, Action>();
        private List<Button> activeButtons;
        private ScreenState activeScreen = ScreenState.None;
        private int selectedButtonIndex;
        private float navigationRepeatTimer;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<ProductionCombatSliceController>();
            }

            EnsureDocument();
        }

        private void OnEnable()
        {
            EnsureDocument();
        }

        private void Start()
        {
            EnsureBuilt();
            Refresh(true);
        }

        private void Update()
        {
            EnsureBuilt();
            HandleInput();
            Refresh(false);
        }

        private void OnDestroy()
        {
            if (panelSettings != null)
            {
                Destroy(panelSettings);
            }

            if (runtimeThemeStyleSheet != null)
            {
                Destroy(runtimeThemeStyleSheet);
            }
        }

        private void EnsureDocument()
        {
            if (document != null)
            {
                return;
            }

            var documentObject = new GameObject("PCS Runtime UI Document");
            documentObject.transform.SetParent(transform, false);

            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.name = "PCS Runtime UI Panel Settings";
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1920, 1080);
            panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panelSettings.match = 0.5f;
            panelSettings.sortingOrder = 50;
            runtimeThemeStyleSheet = CreateRuntimeThemeStyleSheet($"{panelSettings.name} Theme");
            panelSettings.themeStyleSheet = runtimeThemeStyleSheet;

            document = documentObject.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
        }

        private void EnsureBuilt()
        {
            if (root != null || document == null || document.rootVisualElement == null)
            {
                return;
            }

            root = document.rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1f;
            root.style.color = TextColor;

            hud = BuildHud();
            titleOverlay = BuildTitleOverlay();
            pauseOverlay = BuildPauseOverlay();
            retryOverlay = BuildRetryOverlay();
            completeOverlay = BuildCompleteOverlay();

            root.Add(hud);
            root.Add(titleOverlay);
            root.Add(pauseOverlay);
            root.Add(retryOverlay);
            root.Add(completeOverlay);
        }

        private VisualElement BuildHud()
        {
            var panel = new VisualElement { name = "PCS HUD" };
            panel.style.position = Position.Absolute;
            panel.style.left = 24f;
            panel.style.top = 20f;
            panel.style.width = 380f;
            panel.style.maxWidth = Length.Percent(88f);
            panel.style.paddingLeft = 16f;
            panel.style.paddingRight = 16f;
            panel.style.paddingTop = 14f;
            panel.style.paddingBottom = 14f;
            panel.style.backgroundColor = new Color(0.025f, 0.03f, 0.035f, 0.78f);
            SetBorder(panel, BorderColor, 1f, 6f);

            var title = MakeLabel("FOURFOLD ECHOES", 18, FontStyle.Bold);
            panel.Add(title);

            eventLabel = MakeLabel("Production slice ready", 14, FontStyle.Normal);
            eventLabel.style.color = MutedTextColor;
            eventLabel.style.marginTop = 4f;
            panel.Add(eventLabel);

            heroFill = AddMeter(panel, "Hero", ConfirmColor);
            wardensFill = AddMeter(panel, "Wardens", WarningColor);
            bossFill = AddMeter(panel, "Boss", BossColor);
            toolFill = AddMeter(panel, "Tool", AccentColor);

            toolLabel = MakeLabel("Tool ready", 13, FontStyle.Normal);
            toolLabel.style.color = MutedTextColor;
            toolLabel.style.marginTop = 8f;
            panel.Add(toolLabel);

            statusLabel = MakeLabel("Shortcut closed | Gate sealed | Reward waiting", 13, FontStyle.Normal);
            statusLabel.style.color = MutedTextColor;
            statusLabel.style.marginTop = 3f;
            panel.Add(statusLabel);

            saveLabel = MakeLabel("Local save ready", 12, FontStyle.Normal);
            saveLabel.style.color = MutedTextColor;
            saveLabel.style.marginTop = 6f;
            panel.Add(saveLabel);

            return panel;
        }

        private VisualElement BuildTitleOverlay()
        {
            var overlay = BuildOverlay("PCS Title Screen");
            var panel = BuildOverlayPanel();
            panel.Add(MakeLabel("FOURFOLD ECHOES", 42, FontStyle.Bold));
            panel.Add(MakeBodyLabel("Production Combat Slice"));
            panel.Add(MakeBodyLabel("Clear two wardens, open the shortcut with the Echo Tool, break the boss gate, and claim the reward."));
            panel.Add(MakeBodyLabel("Controller: Left Stick, South Button attack, North Button tool/claim, Menu pause. Keyboard: WASD, J / Mouse, E / Right Mouse, Esc or P."));
            titleSaveLabel = MakeSaveResumeLabel();
            panel.Add(titleSaveLabel);
            startButton = AddButton(panel, titleButtons, "Start Game", () => controller?.BeginRun());
            AddButton(panel, titleButtons, "Quit", Application.Quit);
            WireButtons(titleButtons);
            overlay.Add(panel);
            return overlay;
        }

        private VisualElement BuildPauseOverlay()
        {
            var overlay = BuildOverlay("PCS Pause Screen");
            var panel = BuildOverlayPanel();
            panel.Add(MakeLabel("Paused", 34, FontStyle.Bold));
            panel.Add(MakeBodyLabel("The run is held without advancing combat or exploration tool input."));
            AddButton(panel, pauseButtons, "Resume", () => controller?.SetPaused(false));
            AddButton(panel, pauseButtons, "Retry", () => controller?.RetryRun());
            AddButton(panel, pauseButtons, "Title", () => controller?.ReturnToTitle());
            WireButtons(pauseButtons);
            overlay.Add(panel);
            return overlay;
        }

        private VisualElement BuildRetryOverlay()
        {
            var overlay = BuildOverlay("PCS Retry Screen");
            var panel = BuildOverlayPanel();
            panel.Add(MakeLabel("Hero Down", 34, FontStyle.Bold));
            panel.Add(MakeBodyLabel("Restart the room from its initial state."));
            AddButton(panel, retryButtons, "Retry", () => controller?.RetryRun());
            AddButton(panel, retryButtons, "Title", () => controller?.ReturnToTitle());
            WireButtons(retryButtons);
            overlay.Add(panel);
            return overlay;
        }

        private VisualElement BuildCompleteOverlay()
        {
            var overlay = BuildOverlay("PCS Complete Screen");
            var panel = BuildOverlayPanel();
            panel.Add(MakeLabel("Reward Claimed", 34, FontStyle.Bold));
            panel.Add(MakeBodyLabel("The slice route is complete."));
            completeSaveLabel = MakeSaveResumeLabel("Progress saved. Returning later will restore the cleared reward.");
            panel.Add(completeSaveLabel);
            AddButton(panel, completeButtons, "Retry", () => controller?.RetryRun());
            AddButton(panel, completeButtons, "Title", () => controller?.ReturnToTitle());
            WireButtons(completeButtons);
            overlay.Add(panel);
            return overlay;
        }

        private static VisualElement BuildOverlay(string name)
        {
            var overlay = new VisualElement { name = name };
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0f;
            overlay.style.right = 0f;
            overlay.style.top = 0f;
            overlay.style.bottom = 0f;
            overlay.style.alignItems = Align.Center;
            overlay.style.justifyContent = Justify.Center;
            overlay.style.paddingLeft = 24f;
            overlay.style.paddingRight = 24f;
            overlay.style.backgroundColor = OverlayBackground;
            return overlay;
        }

        private static VisualElement BuildOverlayPanel()
        {
            var panel = new VisualElement();
            panel.style.width = Length.Percent(86f);
            panel.style.maxWidth = 680f;
            panel.style.paddingLeft = 28f;
            panel.style.paddingRight = 28f;
            panel.style.paddingTop = 26f;
            panel.style.paddingBottom = 26f;
            panel.style.backgroundColor = PanelBackground;
            panel.style.alignItems = Align.Stretch;
            SetBorder(panel, BorderColor, 1f, 8f);
            return panel;
        }

        private static Label MakeLabel(string text, int size, FontStyle fontStyle)
        {
            var label = new Label(text);
            label.style.fontSize = size;
            label.style.unityFontStyleAndWeight = fontStyle;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.color = TextColor;
            label.style.flexShrink = 1f;
            return label;
        }

        private static Label MakeBodyLabel(string text)
        {
            var label = MakeLabel(text, 16, FontStyle.Normal);
            label.style.color = MutedTextColor;
            label.style.marginTop = 8f;
            label.style.marginBottom = 12f;
            return label;
        }

        private static Label MakeSaveResumeLabel(string text = "No saved slice progress yet.")
        {
            var label = MakeLabel(text, 14, FontStyle.Bold);
            label.style.color = MutedTextColor;
            label.style.marginTop = 2f;
            label.style.marginBottom = 12f;
            label.style.paddingLeft = 12f;
            label.style.paddingRight = 12f;
            label.style.paddingTop = 8f;
            label.style.paddingBottom = 8f;
            label.style.backgroundColor = new Color(0.08f, 0.09f, 0.08f, 0.78f);
            SetBorder(label, BorderColor, 1f, 6f);
            return label;
        }

        private static VisualElement AddMeter(VisualElement parent, string labelText, Color fillColor)
        {
            var row = new VisualElement();
            row.style.marginTop = 9f;
            row.style.flexDirection = FlexDirection.Column;

            var label = MakeLabel(labelText, 12, FontStyle.Bold);
            label.style.color = MutedTextColor;
            row.Add(label);

            var track = new VisualElement();
            track.style.height = 12f;
            track.style.marginTop = 2f;
            track.style.backgroundColor = TrackColor;
            SetBorder(track, new Color(0f, 0f, 0f, 0.45f), 1f, 4f);

            var fill = new VisualElement();
            fill.style.height = Length.Percent(100f);
            fill.style.width = Length.Percent(100f);
            fill.style.backgroundColor = fillColor;
            track.Add(fill);
            row.Add(track);
            parent.Add(row);
            return fill;
        }

        private Button AddButton(VisualElement parent, List<Button> targetList, string text, Action clicked)
        {
            var button = new Button(clicked) { text = text };
            button.focusable = true;
            button.style.height = 46f;
            button.style.marginTop = 8f;
            button.style.fontSize = 17f;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.style.color = TextColor;
            button.style.backgroundColor = ButtonColor;
            SetBorder(button, BorderColor, 1f, 6f);
            parent.Add(button);
            targetList.Add(button);
            buttonActions[button] = clicked;
            return button;
        }

        private void WireButtons(List<Button> buttons)
        {
            for (var index = 0; index < buttons.Count; index++)
            {
                var selectedIndex = index;
                buttons[index].RegisterCallback<PointerEnterEvent>(_ => SelectButton(selectedIndex));
            }
        }

        private void HandleInput()
        {
            if (controller == null)
            {
                return;
            }

            navigationRepeatTimer = Mathf.Max(0f, navigationRepeatTimer - Time.unscaledDeltaTime);

            if (controller.State == ProductionCombatRunState.Playing && PausePressed())
            {
                controller.SetPaused(true);
                return;
            }

            if (controller.State == ProductionCombatRunState.Paused && (PausePressed() || CancelPressed()))
            {
                controller.SetPaused(false);
                return;
            }

            if (controller.State == ProductionCombatRunState.PlayerDown && Input.GetKeyDown(KeyCode.R))
            {
                controller.RetryRun();
                return;
            }

            if (TryReadNavigation(out var direction))
            {
                MoveSelection(direction);
            }

            if (SubmitPressed())
            {
                ActivateSelected();
            }
        }

        private void Refresh(bool forceScreen)
        {
            if (controller == null || root == null)
            {
                return;
            }

            var nextScreen = ToScreenState(controller.State);
            if (forceScreen || activeScreen != nextScreen)
            {
                SetActiveScreen(nextScreen);
            }

            if (eventLabel != null)
            {
                eventLabel.text = controller.LastEvent;
            }

            SetMeter(heroFill, controller.PlayerHealth01);
            SetMeter(wardensFill, controller.WardensHealth01);
            SetMeter(bossFill, controller.BossHealth01);
            SetMeter(toolFill, controller.ToolReady01);

            if (toolLabel != null)
            {
                toolLabel.text = controller.ToolReady01 >= 0.99f ? "Tool ready" : "Tool recovering";
            }

            if (statusLabel != null)
            {
                statusLabel.text = $"Shortcut {(controller.ShortcutOpen ? "open" : "closed")} | Gate {(controller.GateOpen ? "open" : "sealed")} | Reward {(controller.RewardClaimed ? "claimed" : "waiting")}";
            }

            if (saveLabel != null)
            {
                saveLabel.text = controller.SaveStatus;
                saveLabel.style.color = controller.SaveStatus.StartsWith("Save failed", StringComparison.Ordinal)
                    ? WarningColor
                    : MutedTextColor;
            }

            if (titleSaveLabel != null)
            {
                var hasProgress = HasSavedSliceProgress(controller.ShortcutOpen, controller.GateOpen, controller.RewardClaimed);
                titleSaveLabel.text = BuildTitleSaveLine(controller.ShortcutOpen, controller.GateOpen, controller.RewardClaimed, controller.SaveStatus);
                titleSaveLabel.style.color = controller.SaveStatus.StartsWith("Save failed", StringComparison.Ordinal)
                    ? WarningColor
                    : hasProgress
                        ? ConfirmColor
                        : MutedTextColor;
            }

            if (startButton != null)
            {
                startButton.text = BuildStartButtonText(controller.ShortcutOpen, controller.GateOpen, controller.RewardClaimed);
            }

            if (completeSaveLabel != null)
            {
                completeSaveLabel.text = BuildCompleteSaveLine(controller.SaveStatus);
                completeSaveLabel.style.color = controller.SaveStatus.StartsWith("Save failed", StringComparison.Ordinal)
                    || controller.SaveStatus == "Autosave off"
                        ? WarningColor
                        : ConfirmColor;
            }
        }

        internal static string BuildTitleSaveLine(bool shortcutOpen, bool gateOpen, bool rewardClaimed, string saveStatus)
        {
            if (!string.IsNullOrEmpty(saveStatus) && saveStatus.StartsWith("Save failed", StringComparison.Ordinal))
            {
                return "Local save is unavailable; progress will stay in memory for this run.";
            }

            if (rewardClaimed)
            {
                return "Saved reward claimed. Continue to review the completed slice.";
            }

            if (gateOpen)
            {
                return "Saved boss gate is open. Continue from the reward route.";
            }

            if (shortcutOpen)
            {
                return "Saved shortcut is open. Continue toward the wardens and boss gate.";
            }

            return "No saved slice progress yet.";
        }

        internal static string BuildStartButtonText(bool shortcutOpen, bool gateOpen, bool rewardClaimed)
        {
            return HasSavedSliceProgress(shortcutOpen, gateOpen, rewardClaimed)
                ? "Continue Saved Slice"
                : "Start Game";
        }

        public static string BuildCompleteSaveLine(string saveStatus)
        {
            if (!string.IsNullOrEmpty(saveStatus) && saveStatus.StartsWith("Save failed", StringComparison.Ordinal))
            {
                return "Clear state is held in memory only; local save did not finish.";
            }

            if (saveStatus == "Autosave off")
            {
                return "Autosave is off; this clear state is not written to disk.";
            }

            if (saveStatus == "Progress restored")
            {
                return "Saved clear restored. This reward state is already on disk.";
            }

            return "Progress saved. Returning later will restore the cleared reward.";
        }

        public static ThemeStyleSheet CreateRuntimeThemeStyleSheet(string name)
        {
            var theme = ScriptableObject.CreateInstance<ThemeStyleSheet>();
            theme.name = string.IsNullOrWhiteSpace(name) ? "PCS Runtime UI Theme" : name;
            theme.hideFlags = HideFlags.DontSave;
            return theme;
        }

        private static bool HasSavedSliceProgress(bool shortcutOpen, bool gateOpen, bool rewardClaimed)
        {
            return shortcutOpen || gateOpen || rewardClaimed;
        }

        private void SetActiveScreen(ScreenState screen)
        {
            activeScreen = screen;
            SetVisible(hud, screen != ScreenState.Title);
            SetVisible(titleOverlay, screen == ScreenState.Title);
            SetVisible(pauseOverlay, screen == ScreenState.Paused);
            SetVisible(retryOverlay, screen == ScreenState.Retry);
            SetVisible(completeOverlay, screen == ScreenState.Complete);

            if (screen == ScreenState.Title)
            {
                activeButtons = titleButtons;
            }
            else if (screen == ScreenState.Paused)
            {
                activeButtons = pauseButtons;
            }
            else if (screen == ScreenState.Retry)
            {
                activeButtons = retryButtons;
            }
            else if (screen == ScreenState.Complete)
            {
                activeButtons = completeButtons;
            }
            else
            {
                activeButtons = null;
            }

            selectedButtonIndex = 0;
            RefreshButtonSelection();
        }

        private static ScreenState ToScreenState(ProductionCombatRunState state)
        {
            switch (state)
            {
                case ProductionCombatRunState.Title:
                    return ScreenState.Title;
                case ProductionCombatRunState.Paused:
                    return ScreenState.Paused;
                case ProductionCombatRunState.PlayerDown:
                    return ScreenState.Retry;
                case ProductionCombatRunState.Completed:
                    return ScreenState.Complete;
                default:
                    return ScreenState.Playing;
            }
        }

        private void MoveSelection(int direction)
        {
            if (activeButtons == null || activeButtons.Count == 0)
            {
                return;
            }

            selectedButtonIndex = (selectedButtonIndex + direction + activeButtons.Count) % activeButtons.Count;
            RefreshButtonSelection();
        }

        private void SelectButton(int index)
        {
            if (activeButtons == null || index < 0 || index >= activeButtons.Count)
            {
                return;
            }

            selectedButtonIndex = index;
            RefreshButtonSelection();
        }

        private void ActivateSelected()
        {
            if (activeButtons == null || activeButtons.Count == 0)
            {
                return;
            }

            var button = activeButtons[Mathf.Clamp(selectedButtonIndex, 0, activeButtons.Count - 1)];
            if (buttonActions.TryGetValue(button, out var action))
            {
                action();
            }
        }

        private void RefreshButtonSelection()
        {
            if (activeButtons == null)
            {
                return;
            }

            for (var index = 0; index < activeButtons.Count; index++)
            {
                var button = activeButtons[index];
                var selected = index == selectedButtonIndex;
                button.style.backgroundColor = selected ? SelectedButtonColor : ButtonColor;
                SetBorder(button, selected ? AccentColor : BorderColor, selected ? 2f : 1f, 6f);
                if (selected)
                {
                    button.Focus();
                }
            }
        }

        private bool TryReadNavigation(out int direction)
        {
            direction = 0;
            if (activeButtons == null || activeButtons.Count == 0)
            {
                return false;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                direction = -1;
                return true;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                direction = 1;
                return true;
            }

            if (navigationRepeatTimer > 0f)
            {
                return false;
            }

            var vertical = Input.GetAxisRaw("Vertical");
            if (vertical > 0.5f)
            {
                direction = -1;
            }
            else if (vertical < -0.5f)
            {
                direction = 1;
            }

            if (direction == 0)
            {
                return false;
            }

            navigationRepeatTimer = NavigationRepeatSeconds;
            return true;
        }

        private static bool SubmitPressed()
        {
            return Input.GetKeyDown(KeyCode.Return)
                || Input.GetKeyDown(KeyCode.KeypadEnter)
                || Input.GetKeyDown(KeyCode.Space)
                || Input.GetKeyDown(KeyCode.J)
                || Input.GetKeyDown(KeyCode.JoystickButton0);
        }

        private static bool CancelPressed()
        {
            return Input.GetKeyDown(KeyCode.Escape)
                || Input.GetKeyDown(KeyCode.JoystickButton1);
        }

        private static bool PausePressed()
        {
            return Input.GetKeyDown(KeyCode.Escape)
                || Input.GetKeyDown(KeyCode.P)
                || Input.GetKeyDown(KeyCode.JoystickButton7);
        }

        private static void SetVisible(VisualElement element, bool visible)
        {
            if (element != null)
            {
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static void SetMeter(VisualElement fill, float value)
        {
            if (fill != null)
            {
                fill.style.width = Length.Percent(Mathf.Clamp01(value) * 100f);
            }
        }

        private static void SetBorder(VisualElement element, Color color, float width, float radius)
        {
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }
    }
}
