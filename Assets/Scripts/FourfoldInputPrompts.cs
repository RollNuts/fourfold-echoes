using UnityEngine;

namespace FourfoldEchoes.Product
{
    public static class FourfoldInputPrompts
    {
        private const float RecentInputWindow = 8f;
        private static float lastKeyboardTime = -100f;
        private static float lastGamepadTime = -100f;

        public static void ObserveFrameInput()
        {
            if (GamepadButtonDown())
            {
                lastGamepadTime = Time.unscaledTime;
            }

            if (KeyboardButtonDown())
            {
                lastKeyboardTime = Time.unscaledTime;
            }
        }

        public static string TitleMenu(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Move: D-pad/Left Stick   Confirm: A   Back: B", "移動: 十字キー/左スティック   決定: A   戻る: B")
                : FourfoldLanguage.T(data, "Move: arrows   Confirm: Enter   Back: Esc", "移動: 矢印   決定: Enter   戻る: Esc");
        }

        public static string TitleConfirm(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Confirm: A   Cancel: B", "決定: A   キャンセル: B")
                : FourfoldLanguage.T(data, "Confirm: Enter   Cancel: Esc", "決定: Enter   キャンセル: Esc");
        }

        public static string TitleSettings(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Left/Right changes value. A or B returns.", "左右で変更。A または B で戻る。")
                : FourfoldLanguage.T(data, "Left/Right changes value. Enter or Esc returns.", "左右で変更。Enter または Esc で戻る。");
        }

        public static string HubHud(FourfoldProgressData data, bool resetHolding)
        {
            if (resetHolding)
            {
                return PreferGamepad()
                    ? FourfoldLanguage.T(data, "Keep holding Select to open reset confirmation.", "Selectを押し続けるとリセット確認を開く。")
                    : FourfoldLanguage.T(data, "Keep holding Backspace to open reset confirmation.", "Backspaceを押し続けるとリセット確認を開く。");
            }

            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Menu: Pause   Hold Select: Reset confirmation", "Menu: ポーズ   Select長押し: リセット確認")
                : FourfoldLanguage.T(data, "Esc: Pause   Hold Backspace: Reset confirmation", "Esc: ポーズ   Backspace長押し: リセット確認");
        }

        public static string HubPanel(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Move: D-pad/Left Stick   Confirm: A or Y   Close: B or Menu", "移動: 十字キー/左スティック   決定: A または Y   閉じる: B または Menu")
                : FourfoldLanguage.T(data, "Move: arrows or WASD   Confirm: E or Enter   Close: Esc or Backspace", "移動: 矢印 または WASD   決定: E または Enter   閉じる: Esc または Backspace");
        }

        public static string HubConfirm(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Confirm: A or Y   Cancel: B or Menu", "決定: A または Y   キャンセル: B または Menu")
                : FourfoldLanguage.T(data, "Confirm: E or Enter   Cancel: Esc or Backspace", "決定: E または Enter   キャンセル: Esc または Backspace");
        }

        public static string HubStartReady(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "START READY: press Y to review Region 01 and begin.", "開始可能: Y で地域01を確認して開始。")
                : FourfoldLanguage.T(data, "START READY: press E to review Region 01 and begin.", "開始可能: E で地域01を確認して開始。");
        }

        public static string SharedSettings(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Left/Right changes value. A, Y, B, or Select returns.", "左右で変更。A、Y、B、Selectで戻る。")
                : FourfoldLanguage.T(data, "Left/Right changes value. E or Enter or Backspace returns.", "左右で変更。E または Enter または Backspace で戻る。");
        }

        public static string RegionControls(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Move Left Stick/D-pad   Attack A   Dodge B   Tool X   Interact Y   Pause Menu", "移動 左スティック/十字キー   攻撃 A   回避 B   ツール X   調べる Y   ポーズ Menu")
                : FourfoldLanguage.T(data, "Move WASD or arrows   Attack Space   Dodge Shift   Tool Q   Interact E   Pause Esc", "移動 WASD または 矢印   攻撃 Space   回避 Shift   ツール Q   調べる E   ポーズ Esc");
        }

        public static string RegionBossToolReady(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Boss close: press X to expose an opening, then attack with A.", "ボス接近: Xで隙を作り、Aで攻撃。")
                : FourfoldLanguage.T(data, "Boss close: press Q to expose an opening, then attack with Space.", "ボス接近: Qで隙を作り、Spaceで攻撃。");
        }

        public static string RegionBossOpeningActive(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Boss open: attack with A before the window closes.", "ボスに隙あり: 閉じる前にAで攻撃。")
                : FourfoldLanguage.T(data, "Boss open: attack with Space before the window closes.", "ボスに隙あり: 閉じる前にSpaceで攻撃。");
        }

        public static string RegionFailure(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Move: D-pad/Left Stick   Confirm: A or Y   Start retries", "移動: 十字キー/左スティック   決定: A または Y   Startで再挑戦")
                : FourfoldLanguage.T(data, "Move: arrows or WASD   Confirm: E or Enter   R retries", "移動: 矢印 または WASD   決定: E または Enter   Rで再挑戦");
        }

        public static string RegionRetryConfirm(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Press Start, A, or Y to retry from the start.", "Start、A、Yで最初から再挑戦。")
                : FourfoldLanguage.T(data, "Press R or E to retry from the start.", "R または E で最初から再挑戦。");
        }

        public static string RegionTitleConfirm(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Press Select, A, or Y to leave for title.", "Select、A、Yでタイトルへ。")
                : FourfoldLanguage.T(data, "Press Backspace or E to leave for title.", "Backspace または E でタイトルへ。");
        }

        public static string RegionReplayObjective(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "RESULT: hub return saved this clear. Press Start to replay the region.", "結果: ハブ帰還で今回のクリアを保存済み。Startで地域を再挑戦。")
                : FourfoldLanguage.T(data, "RESULT: hub return saved this clear. Press R to replay the region.", "結果: ハブ帰還で今回のクリアを保存済み。Rで地域を再挑戦。");
        }

        public static string RegionReturnGateObjective(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "CLEAR: return to hub to save rewards. Press Y at the return gate.", "クリア: 報酬保存のためハブへ帰還。帰還ゲートでY。")
                : FourfoldLanguage.T(data, "CLEAR: return to hub to save rewards. Press E at the return gate.", "クリア: 報酬保存のためハブへ帰還。帰還ゲートでE。");
        }

        public static string RegionRetryObjective(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "FAILED: saved hub progress remains. Press Start to retry the region.", "失敗: ハブ保存済みの進行は保持。Startで地域を再挑戦。")
                : FourfoldLanguage.T(data, "FAILED: saved hub progress remains. Press R to retry the region.", "失敗: ハブ保存済みの進行は保持。Rで地域を再挑戦。");
        }

        public static string RegionClaimEdgeObjective(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Step 3/6: claim Lumen Edge with Y.", "手順3/6: YでLumen Edgeを獲得。")
                : FourfoldLanguage.T(data, "Step 3/6: claim Lumen Edge with E.", "手順3/6: EでLumen Edgeを獲得。");
        }

        public static string RegionClaimWardObjective(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "Step 5/6: claim Lumen Ward with Y.", "手順5/6: YでLumen Wardを獲得。")
                : FourfoldLanguage.T(data, "Step 5/6: claim Lumen Ward with E.", "手順5/6: EでLumen Wardを獲得。");
        }

        public static string RegionPendingExitCancel(FourfoldProgressData data)
        {
            return PreferGamepad()
                ? FourfoldLanguage.T(data, "B or Menu cancels.", "B または Menu でキャンセル。")
                : FourfoldLanguage.T(data, "Esc cancels.", "Escでキャンセル。");
        }

        private static bool PreferGamepad()
        {
            if (lastGamepadTime > lastKeyboardTime)
            {
                return true;
            }

            if (lastKeyboardTime > 0f && Time.unscaledTime - lastKeyboardTime < RecentInputWindow)
            {
                return false;
            }

            var names = Input.GetJoystickNames();
            if (names == null)
            {
                return false;
            }

            for (var i = 0; i < names.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(names[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool GamepadButtonDown()
        {
            return Input.GetKeyDown(KeyCode.JoystickButton0)
                || Input.GetKeyDown(KeyCode.JoystickButton1)
                || Input.GetKeyDown(KeyCode.JoystickButton2)
                || Input.GetKeyDown(KeyCode.JoystickButton3)
                || Input.GetKeyDown(KeyCode.JoystickButton6)
                || Input.GetKeyDown(KeyCode.JoystickButton7)
                || Input.GetKeyDown(KeyCode.JoystickButton9);
        }

        private static bool KeyboardButtonDown()
        {
            return Input.GetKeyDown(KeyCode.W)
                || Input.GetKeyDown(KeyCode.A)
                || Input.GetKeyDown(KeyCode.S)
                || Input.GetKeyDown(KeyCode.D)
                || Input.GetKeyDown(KeyCode.UpArrow)
                || Input.GetKeyDown(KeyCode.DownArrow)
                || Input.GetKeyDown(KeyCode.LeftArrow)
                || Input.GetKeyDown(KeyCode.RightArrow)
                || Input.GetKeyDown(KeyCode.Return)
                || Input.GetKeyDown(KeyCode.Space)
                || Input.GetKeyDown(KeyCode.E)
                || Input.GetKeyDown(KeyCode.Q)
                || Input.GetKeyDown(KeyCode.R)
                || Input.GetKeyDown(KeyCode.LeftShift)
                || Input.GetKeyDown(KeyCode.RightShift)
                || Input.GetKeyDown(KeyCode.Escape)
                || Input.GetKeyDown(KeyCode.Backspace);
        }
    }
}
