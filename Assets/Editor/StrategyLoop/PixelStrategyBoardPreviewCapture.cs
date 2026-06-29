using System.IO;
using FourfoldEchoes.StrategyLoop;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FourfoldEchoes.Editor.StrategyLoop
{
    public static class PixelStrategyBoardPreviewCapture
    {
        private const int CaptureWidth = 1280;
        private const int CaptureHeight = 800;
        private const int PixelsPerUnit = 32;
        private static readonly string DefaultOutputPath = Path.Combine(
            Path.GetTempPath(),
            "fourfold-pixel-strategy-board-preview-20260629",
            "pixel-strategy-board-preview.png");

        public static void Capture()
        {
            Capture(DefaultOutputPath);
        }

        public static void Capture(string outputPath)
        {
            var state = PixelStrategyBoardPreviewFactory.CreateStreamerReadableSample();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = CreateCamera();
            CreateBoard(state);
            CreateHud(state);
            CaptureCamera(camera, outputPath);
            Debug.Log("Pixel strategy board preview captured: " + outputPath);
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Pixel Strategy Board Preview Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.078f, 0.084f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 4.8f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            return camera;
        }

        private static void CreateBoard(PixelStrategyBoardPreviewState state)
        {
            CreatePanel("Board Backplate", new Vector3(-2.35f, 0.05f, 0.4f), new Vector2(7.0f, 5.8f), new Color32(28, 40, 43, 255));
            CreateLabel("LOOP BOARD", new Vector3(-5.55f, 2.65f, -0.3f), 0.052f, new Color32(255, 239, 194, 255), TextAnchor.MiddleLeft);

            var origin = new Vector3(-5.25f, -2.1f, 0f);
            foreach (var cell in state.BuildCells())
            {
                var world = origin + new Vector3(cell.Cell.x, cell.Cell.y, 0f);
                CreateSprite("Board " + cell.Kind, CreateTileSprite(cell.Kind), world, 1);
            }

            DrawRouteLine(state, origin);

            foreach (var placement in state.Placements)
            {
                var world = origin + new Vector3(placement.Cell.x, placement.Cell.y, -0.08f);
                if (placement.Kind == PixelStrategyPlacementKind.Lair)
                {
                    CreateSprite("Lair Token", CreateLairToken(), world, 4);
                }
                else if (placement.Kind == PixelStrategyPlacementKind.RewardCache)
                {
                    CreateSprite("Reward Token", CreateRewardToken(), world, 4);
                }
            }

            CreateSprite("Hero Token", CreateHeroToken(), origin + new Vector3(state.HeroCell.x, state.HeroCell.y, -0.12f), 5);
            CreateSprite("Extract Token", CreateExtractToken(), origin + new Vector3(state.ExtractCell.x, state.ExtractCell.y, -0.10f), 5);
        }

        private static void DrawRouteLine(PixelStrategyBoardPreviewState state, Vector3 origin)
        {
            var routeObject = new GameObject("Route Loop Read");
            var line = routeObject.AddComponent<LineRenderer>();
            line.positionCount = state.Route.Count + 1;
            line.loop = false;
            line.startWidth = 0.06f;
            line.endWidth = 0.06f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = new Color(1f, 0.93f, 0.62f, 1f);
            line.endColor = line.startColor;

            for (var index = 0; index < state.Route.Count; index++)
            {
                var cell = state.Route.Cells[index];
                line.SetPosition(index, origin + new Vector3(cell.x, cell.y, -0.18f));
            }

            var first = state.Route.Cells[0];
            line.SetPosition(state.Route.Count, origin + new Vector3(first.x, first.y, -0.18f));
        }

        private static void CreateHud(PixelStrategyBoardPreviewState state)
        {
            CreatePanel("HUD Backplate", new Vector3(3.35f, 0.05f, 0.35f), new Vector2(4.25f, 5.8f), new Color32(248, 230, 180, 255));
            CreateLabel("EXTRACT READY", new Vector3(1.55f, 2.55f, -0.2f), 0.056f, new Color32(42, 37, 33, 255), TextAnchor.MiddleLeft);
            CreateLabel("Loop " + state.Run.CompletedLoops + "  Steps " + state.Run.StepsTaken, new Vector3(1.55f, 2.12f, -0.2f), 0.036f, new Color32(42, 37, 33, 255), TextAnchor.MiddleLeft);

            CreateMeter("Loot", state.Run.Loot, 12, new Vector3(1.55f, 1.45f, -0.1f), new Color32(246, 198, 75, 255));
            CreateMeter("Threat", state.Run.Threat, 20, new Vector3(1.55f, 0.75f, -0.1f), new Color32(232, 77, 63, 255));
            CreateMeter("Bag", state.Run.BagPressure, 12, new Vector3(1.55f, 0.05f, -0.1f), new Color32(88, 205, 126, 255));

            CreateLabel("Decision: " + state.Run.Decision, new Vector3(1.55f, -0.78f, -0.2f), 0.048f, new Color32(42, 37, 33, 255), TextAnchor.MiddleLeft);
            CreateLabel("Bag is hot. Bank it now.", new Vector3(1.55f, -1.18f, -0.2f), 0.034f, new Color32(42, 37, 33, 255), TextAnchor.MiddleLeft);
            CreateLabel("Route, lair, loot, hazard, exit.", new Vector3(1.55f, -1.55f, -0.2f), 0.034f, new Color32(42, 37, 33, 255), TextAnchor.MiddleLeft);
        }

        private static void CreateMeter(string label, int value, int max, Vector3 position, Color32 fillColor)
        {
            CreateLabel(label + " " + value + "/" + max, position + new Vector3(0f, 0.22f, -0.05f), 0.034f, new Color32(42, 37, 33, 255), TextAnchor.MiddleLeft);
            CreatePanel(label + " Meter Back", position + new Vector3(1.0f, 0f, 0f), new Vector2(1.8f, 0.22f), new Color32(32, 42, 46, 255));
            var width = 1.8f * Mathf.Clamp01((float)value / Mathf.Max(1, max));
            CreatePanel(label + " Meter Fill", position + new Vector3(0.1f + (width * 0.5f), 0f, -0.04f), new Vector2(width, 0.18f), fillColor);
        }

        private static void CreatePanel(string name, Vector3 position, Vector2 size, Color32 color)
        {
            var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            FillRect(texture, 0, 0, 8, 8, color);
            texture.Apply(false, false);

            var sprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), PixelsPerUnit);
            var spriteObject = new GameObject(name);
            spriteObject.transform.position = position;
            spriteObject.transform.localScale = new Vector3(size.x * 4f, size.y * 4f, 1f);
            var renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 0;
        }

        private static void CreateSprite(string name, Sprite sprite, Vector3 position, int sortingOrder)
        {
            var spriteObject = new GameObject(name);
            spriteObject.transform.position = position;
            var renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
        }

        private static void CreateLabel(string text, Vector3 position, float characterSize, Color32 color, TextAnchor anchor)
        {
            var textObject = new GameObject("Label " + text);
            textObject.transform.position = position;
            var textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.anchor = anchor;
            textMesh.alignment = TextAlignment.Left;
            textMesh.characterSize = characterSize;
            textMesh.fontSize = 64;
            textMesh.color = color;
            var renderer = textObject.GetComponent<MeshRenderer>();
            renderer.sortingOrder = 20;
        }

        private static Sprite CreateTileSprite(PixelStrategyBoardPreviewCellKind kind)
        {
            var texture = NewTexture(32, 32, new Color32(71, 119, 75, 255));
            switch (kind)
            {
                case PixelStrategyBoardPreviewCellKind.Route:
                    FillRect(texture, 0, 12, 32, 9, new Color32(185, 180, 122, 255));
                    FillRect(texture, 0, 21, 32, 4, new Color32(135, 137, 93, 255));
                    break;
                case PixelStrategyBoardPreviewCellKind.Lair:
                    FillRect(texture, 0, 0, 32, 32, new Color32(93, 102, 88, 255));
                    FillRect(texture, 7, 13, 18, 13, new Color32(30, 35, 38, 255));
                    FillRect(texture, 15, 17, 5, 5, new Color32(255, 217, 90, 255));
                    break;
                case PixelStrategyBoardPreviewCellKind.Hazard:
                    FillRect(texture, 0, 0, 32, 32, new Color32(97, 105, 92, 255));
                    DrawTriangle(texture, new Vector2Int(6, 26), new Vector2Int(15, 6), new Vector2Int(26, 26), new Color32(232, 77, 63, 255));
                    FillRect(texture, 14, 15, 3, 10, new Color32(255, 229, 150, 255));
                    break;
                case PixelStrategyBoardPreviewCellKind.Reward:
                    FillRect(texture, 0, 0, 32, 32, new Color32(185, 180, 122, 255));
                    DrawDiamond(texture, 16, 16, 11, new Color32(246, 198, 75, 255));
                    DrawDiamond(texture, 16, 16, 5, new Color32(157, 114, 255, 255));
                    break;
                case PixelStrategyBoardPreviewCellKind.Extract:
                    FillRect(texture, 0, 0, 32, 32, new Color32(31, 47, 52, 255));
                    FillRect(texture, 7, 12, 18, 14, new Color32(103, 220, 255, 255));
                    FillRect(texture, 12, 17, 8, 9, new Color32(21, 25, 28, 255));
                    break;
                default:
                    FillRect(texture, 4, 4, 5, 5, new Color32(144, 199, 101, 255));
                    FillRect(texture, 22, 7, 4, 4, new Color32(144, 199, 101, 255));
                    break;
            }

            DrawBorder(texture, 0, 0, 32, 32, new Color32(255, 242, 198, 145));
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Sprite CreateHeroToken()
        {
            var texture = NewTexture(48, 48, new Color32(0, 0, 0, 0));
            FillRect(texture, 17, 19, 16, 19, new Color32(57, 119, 198, 255));
            FillRect(texture, 21, 9, 9, 8, new Color32(239, 156, 102, 255));
            DrawTriangle(texture, new Vector2Int(18, 9), new Vector2Int(25, 3), new Vector2Int(35, 10), new Color32(246, 198, 75, 255));
            FillRect(texture, 33, 22, 5, 17, new Color32(103, 220, 255, 255));
            FillRect(texture, 13, 26, 5, 12, new Color32(232, 77, 63, 255));
            DrawBorder(texture, 16, 18, 18, 21, new Color32(48, 43, 39, 255));
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.42f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Sprite CreateLairToken()
        {
            var texture = NewTexture(48, 48, new Color32(0, 0, 0, 0));
            DrawDiamond(texture, 24, 26, 17, new Color32(127, 141, 113, 255));
            FillRect(texture, 28, 21, 6, 7, new Color32(255, 217, 90, 255));
            DrawTriangle(texture, new Vector2Int(18, 16), new Vector2Int(24, 7), new Vector2Int(30, 16), new Color32(183, 199, 132, 255));
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.42f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Sprite CreateRewardToken()
        {
            var texture = NewTexture(32, 32, new Color32(0, 0, 0, 0));
            DrawDiamond(texture, 16, 16, 12, new Color32(246, 198, 75, 255));
            DrawDiamond(texture, 16, 16, 6, new Color32(157, 114, 255, 255));
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Sprite CreateExtractToken()
        {
            var texture = NewTexture(32, 32, new Color32(0, 0, 0, 0));
            DrawDiamond(texture, 16, 16, 13, new Color32(103, 220, 255, 255));
            FillRect(texture, 12, 17, 8, 10, new Color32(21, 25, 28, 255));
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Texture2D NewTexture(int width, int height, Color32 color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            FillRect(texture, 0, 0, width, height, color);
            return texture;
        }

        private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color32 color)
        {
            for (var px = Mathf.Max(0, x); px < Mathf.Min(texture.width, x + width); px++)
            {
                for (var py = Mathf.Max(0, y); py < Mathf.Min(texture.height, y + height); py++)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }

        private static void DrawBorder(Texture2D texture, int x, int y, int width, int height, Color32 color)
        {
            FillRect(texture, x, y, width, 1, color);
            FillRect(texture, x, y + height - 1, width, 1, color);
            FillRect(texture, x, y, 1, height, color);
            FillRect(texture, x + width - 1, y, 1, height, color);
        }

        private static void DrawTriangle(Texture2D texture, Vector2Int a, Vector2Int b, Vector2Int c, Color32 color)
        {
            var minX = Mathf.Min(a.x, Mathf.Min(b.x, c.x));
            var maxX = Mathf.Max(a.x, Mathf.Max(b.x, c.x));
            var minY = Mathf.Min(a.y, Mathf.Min(b.y, c.y));
            var maxY = Mathf.Max(a.y, Mathf.Max(b.y, c.y));
            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    if (PointInTriangle(new Vector2(x, y), a, b, c))
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static bool PointInTriangle(Vector2 point, Vector2Int a, Vector2Int b, Vector2Int c)
        {
            var d1 = Sign(point, a, b);
            var d2 = Sign(point, b, c);
            var d3 = Sign(point, c, a);
            var hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
            var hasPos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(hasNeg && hasPos);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        private static void DrawDiamond(Texture2D texture, int centerX, int centerY, int radius, Color32 color)
        {
            for (var x = centerX - radius; x <= centerX + radius; x++)
            {
                for (var y = centerY - radius; y <= centerY + radius; y++)
                {
                    if (x >= 0 && y >= 0 && x < texture.width && y < texture.height &&
                        Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY) <= radius)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static void CaptureCamera(Camera camera, string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(CaptureWidth, CaptureHeight, 24);
            var texture = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
                texture.Apply();
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(renderTexture);
            }
        }
    }
}
