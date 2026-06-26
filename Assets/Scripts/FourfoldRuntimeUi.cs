using UnityEngine;

namespace FourfoldEchoes.Product
{
    public static class FourfoldRuntimeUi
    {
        private static readonly Color PanelColor = new Color(0.035f, 0.032f, 0.045f, 0.86f);
        private static readonly Color LineColor = new Color(1.0f, 0.76f, 0.28f, 0.82f);
        private static Texture2D whiteTexture;

        public static GUIStyle HeaderStyle(int screenHeight)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(screenHeight / 22, 28, 48),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(1.0f, 0.86f, 0.52f) }
            };
        }

        public static GUIStyle SubheadStyle(int screenHeight)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(screenHeight / 36, 18, 28),
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                wordWrap = true
            };
        }

        public static GUIStyle BodyStyle(int screenHeight)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(screenHeight / 46, 15, 22),
                normal = { textColor = new Color(0.92f, 0.93f, 0.98f) },
                wordWrap = true
            };
        }

        public static GUIStyle MutedStyle(int screenHeight)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(screenHeight / 54, 13, 18),
                normal = { textColor = new Color(0.70f, 0.75f, 0.82f) },
                wordWrap = true
            };
        }

        public static void DrawScreenWash()
        {
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.015f, 0.016f, 0.024f, 0.24f));
        }

        public static void DrawPanel(Rect rect)
        {
            DrawRect(rect, PanelColor);
            DrawRect(new Rect(rect.x, rect.y, rect.width, 4f), LineColor);
            GUI.Box(rect, GUIContent.none);
        }

        public static void DrawChip(Rect rect, string text, Color color, GUIStyle style)
        {
            DrawRect(rect, new Color(color.r, color.g, color.b, 0.22f));
            DrawRect(new Rect(rect.x, rect.y, 4f, rect.height), color);
            GUI.Box(rect, GUIContent.none);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 4f, rect.width - 18f, rect.height - 8f), text, style);
        }

        public static void DrawBar(Rect rect, float value01, Color color, string label, GUIStyle style)
        {
            DrawRect(rect, new Color(0f, 0f, 0f, 0.40f));
            var fill = Mathf.Clamp01(value01);
            DrawRect(new Rect(rect.x, rect.y, rect.width * fill, rect.height), new Color(color.r, color.g, color.b, 0.62f));
            GUI.Box(rect, GUIContent.none);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 2f, rect.width - 20f, rect.height - 4f), label, style);
        }

        public static void DrawDivider(float x, float y, float width)
        {
            DrawRect(new Rect(x, y, width, 1f), new Color(1f, 1f, 1f, 0.16f));
        }

        private static void DrawRect(Rect rect, Color color)
        {
            if (whiteTexture == null)
            {
                whiteTexture = Texture2D.whiteTexture;
            }

            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previous;
        }
    }
}
