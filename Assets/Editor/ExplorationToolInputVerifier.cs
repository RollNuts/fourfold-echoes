using System;
using FourfoldEchoes.Product;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public static class ExplorationToolInputVerifier
    {
        public static void VerifyInputContract()
        {
            var gameObject = new GameObject("Exploration Tool Input Contract");
            try
            {
                var tool = gameObject.AddComponent<ExplorationTool>();

                Expect(ExplorationTool.DefaultUseKey == KeyCode.E, "Default keyboard input should be E.");
                Expect(ExplorationTool.DefaultControllerUseButton == KeyCode.JoystickButton3, "Default controller input should be North Button.");
                Expect(ExplorationTool.DefaultMouseUseButton == 1, "Default mouse input should be right mouse button.");
                Expect(ExplorationTool.IsDefaultUseKey(KeyCode.E), "E should be accepted as a default use key.");
                Expect(ExplorationTool.IsDefaultUseKey(KeyCode.JoystickButton3), "North Button should be accepted as a default use key.");
                Expect(!ExplorationTool.IsDefaultUseKey(KeyCode.JoystickButton0), "South Button should remain attack-only.");

                Expect(tool.AcceptsUseKey(KeyCode.E), "Configured tool should accept E.");
                Expect(tool.AcceptsUseKey(KeyCode.JoystickButton3), "Configured tool should accept North Button.");
                Expect(!tool.AcceptsUseKey(KeyCode.JoystickButton0), "Configured tool should not accept South Button.");
                Expect(tool.AcceptsMouseButton(1), "Configured tool should accept right mouse button.");
                Expect(!tool.AcceptsMouseButton(0), "Configured tool should not accept left mouse button.");

                tool.mouseUseButton = -1;
                Expect(!tool.AcceptsMouseButton(1), "Negative mouse button should disable mouse use.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }

            Debug.Log("FOURFOLD exploration tool input contract verified.");
        }

        private static void Expect(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
