using System.Collections.Generic;
using System.IO;
using FourfoldEchoes.StrategyLoop;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FourfoldEchoes.Editor.StrategyLoop
{
    public static class PixelStrategySteamScreenPreviewCapture
    {
        private const int CaptureWidth = 1280;
        private const int CaptureHeight = 720;
        private const int PixelsPerUnit = 32;
        private const float BoardCellWidth = 0.58f;
        private const float BoardCellHeight = 0.42f;
        private static readonly string DefaultOutputPath = Path.Combine(
            Path.GetTempPath(),
            "fourfold-pixel-strategy-steam-screen-preview-20260629",
            "pixel-strategy-steam-screen-preview.png");

        public static void Capture()
        {
            Capture(DefaultOutputPath);
        }

        public static void Capture(string outputPath)
        {
            var state = PixelStrategySteamScreenPreviewFactory.CreateFirstSteamScreenSample();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = CreateCamera();
            CreateBackground();
            CreateEdgeHud(state);
            CreateBoard(state.Board);
            CreateChoiceImpact(state);
            CreateStarterRead(state);
            CreateDecisionCards(state.Cards);
            CaptureCamera(camera, outputPath);
            Debug.Log("Pixel strategy Steam screen preview captured: " + outputPath);
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Pixel Strategy Steam Screen Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(12, 18, 22, 255);
            camera.orthographic = true;
            camera.orthographicSize = 4.0f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            return camera;
        }

        private static void CreateBackground()
        {
            CreatePanel("Night Backdrop", new Vector3(0f, 0f, 1.0f), new Vector2(12.9f, 7.3f), new Color32(13, 22, 29, 255), 0);
            CreatePanel("Distant Ridge", new Vector3(-1.8f, 2.72f, 0.95f), new Vector2(9.2f, 0.48f), new Color32(22, 48, 58, 255), 1);
            CreatePanel("Low Ground", new Vector3(0.2f, -3.2f, 0.9f), new Vector2(12.9f, 0.9f), new Color32(19, 36, 22, 255), 1);

            for (var x = -6.0f; x <= 6.0f; x += 1.6f)
            {
                CreatePanel("Backdrop Grid V", new Vector3(x, 0f, 0.85f), new Vector2(0.015f, 7.3f), new Color32(36, 58, 64, 70), 2);
            }

            for (var y = -3.0f; y <= 3.0f; y += 1.2f)
            {
                CreatePanel("Backdrop Grid H", new Vector3(0f, y, 0.85f), new Vector2(12.9f, 0.015f), new Color32(36, 58, 64, 70), 2);
            }
        }

        private static void CreateEdgeHud(PixelStrategySteamScreenPreviewState state)
        {
            CreatePanel("Loop Bag HUD", new Vector3(-4.95f, 3.05f, 0.1f), new Vector2(2.65f, 0.55f), new Color32(16, 27, 34, 255), 12);
            CreatePanel("Loop Bag HUD Border", new Vector3(-4.95f, 3.05f, 0.05f), new Vector2(2.72f, 0.62f), new Color32(230, 207, 114, 255), 11);
            CreateLabel("LOOP", new Vector3(-6.08f, 3.12f, -0.2f), 0.034f, new Color32(232, 217, 156, 255), TextAnchor.MiddleLeft, 30);
            CreateLabel(state.LoopNumber.ToString("00"), new Vector3(-5.32f, 3.04f, -0.2f), 0.058f, new Color32(255, 241, 190, 255), TextAnchor.MiddleLeft, 30);
            CreateLabel("BAG", new Vector3(-4.54f, 3.12f, -0.2f), 0.034f, new Color32(232, 217, 156, 255), TextAnchor.MiddleLeft, 30);
            CreateLabel(state.BagValue.ToString(), new Vector3(-3.98f, 3.04f, -0.2f), 0.056f, new Color32(255, 215, 91, 255), TextAnchor.MiddleLeft, 30);

            CreatePanel("Pressure HUD Border", new Vector3(0f, 3.05f, 0.05f), new Vector2(4.75f, 0.62f), new Color32(230, 207, 114, 255), 11);
            CreatePanel("Pressure HUD", new Vector3(0f, 3.05f, 0.1f), new Vector2(4.62f, 0.55f), new Color32(16, 27, 34, 255), 12);
            CreateLabel("PRESSURE", new Vector3(-2.13f, 3.13f, -0.2f), 0.034f, new Color32(232, 217, 156, 255), TextAnchor.MiddleLeft, 30);
            CreatePanel("Pressure Bar Back", new Vector3(0.88f, 3.04f, -0.05f), new Vector2(2.45f, 0.18f), new Color32(38, 52, 58, 255), 20);
            CreatePanel("Pressure Bar Safe", new Vector3(0.15f, 3.04f, -0.08f), new Vector2(0.88f, 0.18f), new Color32(96, 190, 116, 255), 21);
            CreatePanel("Pressure Bar Risk", new Vector3(0.98f, 3.04f, -0.08f), new Vector2(0.78f, 0.18f), new Color32(232, 201, 74, 255), 21);
            CreatePanel("Pressure Bar Hot", new Vector3(1.8f, 3.04f, -0.08f), new Vector2(0.86f, 0.18f), new Color32(223, 81, 68, 255), 21);
            CreatePanel("Pressure Spike", new Vector3(2.36f, 3.04f, -0.1f), new Vector2(0.16f, 0.34f), new Color32(255, 105, 78, 255), 22);

            CreatePanel("Gate HUD Border", new Vector3(4.95f, 3.05f, 0.05f), new Vector2(3.35f, 0.62f), new Color32(95, 211, 231, 255), 11);
            CreatePanel("Gate HUD", new Vector3(4.95f, 3.05f, 0.1f), new Vector2(3.23f, 0.55f), new Color32(16, 27, 34, 255), 12);
            CreateLabel("GATE", new Vector3(3.56f, 3.13f, -0.2f), 0.034f, new Color32(232, 217, 156, 255), TextAnchor.MiddleLeft, 30);
            CreateLabel(state.GatePercent + "%", new Vector3(4.35f, 3.04f, -0.2f), 0.064f, new Color32(117, 231, 246, 255), TextAnchor.MiddleLeft, 30);
            CreateArrow(new Vector3(5.66f, 3.04f, -0.1f), new Color32(139, 244, 255, 255), 27, new Vector2(0.34f, 0.34f));
        }

        private static void CreateBoard(PixelStrategyBoardPreviewState state)
        {
            CreatePanel("Board Gold Rim", new Vector3(0f, 0.55f, 0.25f), new Vector2(11.65f, 4.48f), new Color32(240, 216, 121, 255), 5);
            CreatePanel("Board Outer", new Vector3(0f, 0.52f, 0.2f), new Vector2(11.42f, 4.25f), new Color32(13, 20, 24, 255), 6);
            CreatePanel("Board Inset", new Vector3(0.02f, 0.55f, 0.15f), new Vector2(10.28f, 3.78f), new Color32(30, 42, 42, 255), 7);
            CreatePanel("Board Playfield", new Vector3(0.02f, 0.58f, 0.1f), new Vector2(8.55f, 3.08f), new Color32(20, 29, 32, 255), 8);
            CreatePanel("Board Grass Cut", new Vector3(1.15f, -1.14f, 0.07f), new Vector2(8.5f, 0.48f), new Color32(30, 50, 30, 255), 9);

            var origin = new Vector3(-3.98f, -0.63f, 0f);
            foreach (var cell in state.BuildCells())
            {
                var world = CellToWorld(origin, cell.Cell);
                CreatePanel("Grid " + cell.Cell.x + "," + cell.Cell.y, world + new Vector3(0f, 0f, 0.03f), new Vector2(BoardCellWidth - 0.02f, BoardCellHeight - 0.02f), new Color32(19, 30, 32, 255), 10);

                if (cell.Kind == PixelStrategyBoardPreviewCellKind.Route ||
                    cell.Kind == PixelStrategyBoardPreviewCellKind.Extract ||
                    cell.Kind == PixelStrategyBoardPreviewCellKind.Reward ||
                    cell.Kind == PixelStrategyBoardPreviewCellKind.Hazard)
                {
                    CreatePanel("Route Tile " + cell.Cell.x + "," + cell.Cell.y, world + new Vector3(0f, 0f, -0.04f), new Vector2(BoardCellWidth, BoardCellHeight), new Color32(158, 142, 86, 255), 13);
                }
            }

            DrawRouteLine(state, origin, new Color32(255, 229, 131, 255), 0.07f, 24);
            DrawRouteLine(state, origin, new Color32(81, 61, 37, 255), 0.025f, 25);
            DrawPressureTrail(origin);

            foreach (var placement in state.Placements)
            {
                var world = CellToWorld(origin, placement.Cell) + new Vector3(0f, 0.02f, -0.18f);
                switch (placement.Kind)
                {
                    case PixelStrategyPlacementKind.Lair:
                        CreateSprite("Lair Token", CreateLairToken(), world + new Vector3(0f, 0.10f, 0f), 34, new Vector2(0.72f, 0.72f));
                        break;
                    case PixelStrategyPlacementKind.Hazard:
                        CreateSprite("Hazard Token", CreateHazardToken(), world, 33, new Vector2(0.58f, 0.58f));
                        break;
                    case PixelStrategyPlacementKind.RewardCache:
                        CreateSprite("Reward Token", CreateRewardToken(), world, 33, new Vector2(0.58f, 0.58f));
                        break;
                }
            }

            CreateSprite("Hero Token", CreateHeroToken(), CellToWorld(origin, state.HeroCell) + new Vector3(0f, 0.13f, -0.25f), 40, new Vector2(0.74f, 0.74f));
            CreateSprite("Extract Gate", CreateExtractToken(), CellToWorld(origin, state.ExtractCell) + new Vector3(0.08f, 0.08f, -0.24f), 39, new Vector2(0.74f, 0.74f));
            CreateSprite("Threat Boss", CreateBossThreatToken(), CellToWorld(origin, new Vector2Int(13, 2)) + new Vector3(0.36f, 0.17f, -0.22f), 38, new Vector2(0.88f, 0.88f));
            CreateTileHand();
        }

        private static void CreateStarterRead(PixelStrategySteamScreenPreviewState state)
        {
            CreatePanel("Starter Kit Border", new Vector3(-5.1f, -1.7f, 0.02f), new Vector2(2.1f, 0.72f), new Color32(54, 67, 74, 255), 42);
            CreatePanel("Starter Kit", new Vector3(-5.1f, -1.7f, 0f), new Vector2(2.0f, 0.63f), new Color32(16, 27, 34, 255), 43);
            CreateLabel("STARTER KIT", new Vector3(-5.95f, -1.5f, -0.2f), 0.026f, new Color32(232, 217, 156, 255), TextAnchor.MiddleLeft, 65);
            CreateLabel(state.StarterWeapon, new Vector3(-5.95f, -1.74f, -0.2f), 0.03f, new Color32(255, 241, 190, 255), TextAnchor.MiddleLeft, 65);
            CreateLabel(state.StarterCoins + "G", new Vector3(-4.48f, -1.74f, -0.2f), 0.036f, new Color32(255, 215, 91, 255), TextAnchor.MiddleLeft, 65);
            CreateSprite("Starter Stick Icon", CreateStickToken(), new Vector3(-4.98f, -1.72f, -0.18f), 66, new Vector2(0.42f, 0.42f));
        }

        private static Vector3 CellToWorld(Vector3 origin, Vector2Int cell)
        {
            return origin + new Vector3(cell.x * BoardCellWidth, cell.y * BoardCellHeight, 0f);
        }

        private static void CreateChoiceImpact(PixelStrategySteamScreenPreviewState state)
        {
            var origin = new Vector3(-3.98f, -0.63f, 0f);
            CreateOpenedGateRoute(origin, state.Impact.OpenedGateCells);

            foreach (var cell in state.Impact.SealedPressureCells)
            {
                CreateSealMark(CellToWorld(origin, cell) + new Vector3(0f, 0.02f, -0.28f));
            }

            CreatePanel("Impact Callout Border", new Vector3(2.92f, 0.52f, -0.22f), new Vector2(1.86f, 0.42f), new Color32(117, 231, 246, 255), 58);
            CreatePanel("Impact Callout", new Vector3(2.92f, 0.52f, -0.24f), new Vector2(1.76f, 0.33f), new Color32(16, 36, 44, 245), 59);
            CreateLabel(state.Impact.BoardCallout, new Vector3(2.15f, 0.53f, -0.35f), 0.025f, new Color32(225, 254, 255, 255), TextAnchor.MiddleLeft, 70);
        }

        private static void CreateOpenedGateRoute(Vector3 origin, IReadOnlyList<Vector2Int> cells)
        {
            var lineObject = new GameObject("Choice Impact Gate Cut");
            var line = lineObject.AddComponent<LineRenderer>();
            line.positionCount = cells.Count;
            line.startWidth = 0.12f;
            line.endWidth = 0.12f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = new Color32(117, 231, 246, 245);
            line.endColor = new Color32(117, 231, 246, 245);
            line.sortingOrder = 45;

            for (var index = 0; index < cells.Count; index++)
            {
                var cell = cells[index];
                line.SetPosition(index, CellToWorld(origin, cell) + new Vector3(0f, 0.02f, -0.3f));
                CreatePanel("Opened Gate Cell", CellToWorld(origin, cell) + new Vector3(0f, 0f, -0.29f), new Vector2(BoardCellWidth, BoardCellHeight), new Color32(39, 116, 129, 170), 43);
            }
        }

        private static void CreateSealMark(Vector3 position)
        {
            CreatePanel("Sealed Pressure Back", position, new Vector2(0.34f, 0.34f), new Color32(30, 20, 24, 230), 46);
            CreatePanel("Sealed Pressure Slash A", position + new Vector3(0f, 0f, -0.04f), new Vector2(0.44f, 0.08f), new Color32(139, 244, 255, 255), 47);
            CreatePanel("Sealed Pressure Slash B", position + new Vector3(0f, 0f, -0.05f), new Vector2(0.08f, 0.44f), new Color32(139, 244, 255, 255), 48);
        }

        private static void DrawRouteLine(PixelStrategyBoardPreviewState state, Vector3 origin, Color32 color, float width, int sortingOrder)
        {
            var routeObject = new GameObject("Steam Screen Route Line");
            var line = routeObject.AddComponent<LineRenderer>();
            line.positionCount = state.Route.Count + 1;
            line.startWidth = width;
            line.endWidth = width;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = color;
            line.endColor = color;
            line.sortingOrder = sortingOrder;

            for (var index = 0; index < state.Route.Count; index++)
            {
                line.SetPosition(index, CellToWorld(origin, state.Route.Cells[index]) + new Vector3(0f, 0f, -0.2f));
            }

            line.SetPosition(state.Route.Count, CellToWorld(origin, state.Route.Cells[0]) + new Vector3(0f, 0f, -0.2f));
        }

        private static void DrawPressureTrail(Vector3 origin)
        {
            var points = new List<Vector3>
            {
                CellToWorld(origin, new Vector2Int(11, 4)) + new Vector3(0f, 0.24f, -0.23f),
                CellToWorld(origin, new Vector2Int(10, 3)) + new Vector3(0f, 0.25f, -0.23f),
                CellToWorld(origin, new Vector2Int(9, 2)) + new Vector3(0f, 0.25f, -0.23f),
                CellToWorld(origin, new Vector2Int(8, 1)) + new Vector3(0f, 0.24f, -0.23f),
                CellToWorld(origin, new Vector2Int(6, 1)) + new Vector3(0f, 0.05f, -0.23f),
                CellToWorld(origin, new Vector2Int(4, 1)) + new Vector3(0f, 0.05f, -0.23f),
                CellToWorld(origin, new Vector2Int(2, 1)) + new Vector3(0f, 0.05f, -0.23f)
            };

            var trailObject = new GameObject("Pressure Trail");
            var line = trailObject.AddComponent<LineRenderer>();
            line.positionCount = points.Count;
            line.startWidth = 0.13f;
            line.endWidth = 0.09f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = new Color32(255, 82, 73, 230);
            line.endColor = new Color32(255, 82, 73, 120);
            line.sortingOrder = 29;

            for (var index = 0; index < points.Count; index++)
            {
                line.SetPosition(index, points[index]);
            }
        }

        private static void CreateTileHand()
        {
            CreatePanel("Tile Hand", new Vector3(-4.2f, 1.55f, -0.1f), new Vector2(1.85f, 0.74f), new Color32(16, 27, 34, 255), 35);
            CreatePanel("Tile Hand Border", new Vector3(-4.2f, 1.55f, -0.12f), new Vector2(1.94f, 0.83f), new Color32(46, 59, 64, 255), 34);
            CreateLabel("TILE HAND", new Vector3(-5.03f, 1.72f, -0.25f), 0.032f, new Color32(232, 217, 156, 255), TextAnchor.MiddleLeft, 50);
            CreatePanel("Hand Danger", new Vector3(-4.83f, 1.38f, -0.15f), new Vector2(0.38f, 0.22f), new Color32(223, 81, 68, 255), 50);
            CreatePanel("Hand Reward", new Vector3(-4.25f, 1.38f, -0.15f), new Vector2(0.38f, 0.22f), new Color32(241, 200, 77, 255), 50);
            CreatePanel("Hand Gate", new Vector3(-3.67f, 1.38f, -0.15f), new Vector2(0.38f, 0.22f), new Color32(89, 197, 214, 255), 50);
        }

        private static void CreateDecisionCards(IReadOnlyList<PixelStrategySteamScreenCard> cards)
        {
            var cardPositions = new[]
            {
                new Vector3(-4.35f, -2.9f, 0.05f),
                new Vector3(0f, -2.78f, 0.05f),
                new Vector3(4.35f, -2.9f, 0.05f)
            };

            for (var index = 0; index < cards.Count && index < cardPositions.Length; index++)
            {
                CreateDecisionCard(cards[index], cardPositions[index]);
            }
        }

        private static void CreateDecisionCard(PixelStrategySteamScreenCard card, Vector3 position)
        {
            var selected = card.Selected;
            var size = selected ? new Vector2(3.65f, 1.3f) : new Vector2(3.55f, 1.06f);
            var panelColor = selected ? new Color32(29, 41, 31, 255) : new Color32(21, 28, 34, 255);
            var borderColor = selected ? new Color32(116, 222, 134, 255) : new Color32(54, 67, 74, 255);

            if (selected)
            {
                CreatePanel("Selected Card Glow", position + new Vector3(0f, 0f, 0.12f), size + new Vector2(0.18f, 0.18f), new Color32(72, 170, 84, 155), 45);
            }

            CreatePanel("Decision Card Border", position + new Vector3(0f, 0f, 0.1f), size + new Vector2(0.1f, 0.1f), borderColor, 46);
            CreatePanel("Decision Card", position, size, panelColor, 47);

            var textColor = new Color32(255, 241, 190, 255);
            var rewardColor = card.Tone == PixelStrategySteamScreenCardTone.SafeSelected ? new Color32(117, 231, 246, 255) : new Color32(255, 215, 91, 255);
            var riskColor = card.Tone == PixelStrategySteamScreenCardTone.SafeSelected ? new Color32(132, 227, 148, 255) : new Color32(255, 115, 96, 255);

            CreateLabel(card.Option + "  " + card.Title, position + new Vector3(-size.x * 0.42f, size.y * 0.25f, -0.2f), 0.034f, textColor, TextAnchor.MiddleLeft, 65);
            CreateLabel(card.RewardText, position + new Vector3(-size.x * 0.42f, -0.02f, -0.2f), 0.036f, rewardColor, TextAnchor.MiddleLeft, 65);
            CreateLabel(card.RiskText, position + new Vector3(-size.x * 0.1f, -0.02f, -0.2f), 0.036f, riskColor, TextAnchor.MiddleLeft, 65);
            CreateLabel(card.FooterText, position + new Vector3(-size.x * 0.42f, -size.y * 0.34f, -0.2f), 0.019f, selected ? riskColor : new Color32(255, 183, 160, 255), TextAnchor.MiddleLeft, 65);

            if (card.Tone == PixelStrategySteamScreenCardTone.SafeSelected)
            {
                CreateArrow(position + new Vector3(1.22f, 0f, -0.18f), new Color32(139, 244, 255, 255), 66, new Vector2(0.46f, 0.46f));
            }
            else if (card.Tone == PixelStrategySteamScreenCardTone.Doom)
            {
                CreateSprite("Relic Card Icon", CreateRewardToken(), position + new Vector3(1.18f, 0.0f, -0.18f), 66, new Vector2(0.82f, 0.82f));
            }
            else
            {
                CreateSprite("Lair Card Icon", CreateHazardToken(), position + new Vector3(1.18f, 0.0f, -0.18f), 66, new Vector2(0.75f, 0.75f));
            }
        }

        private static void CreateArrow(Vector3 position, Color32 color, int sortingOrder, Vector2 scale)
        {
            var texture = NewTexture(96, 72, new Color32(0, 0, 0, 0));
            FillRect(texture, 8, 30, 54, 10, color);
            DrawTriangle(texture, new Vector2Int(52, 10), new Vector2Int(88, 36), new Vector2Int(52, 62), color);
            FillRect(texture, 46, 22, 16, 28, color);
            texture.Apply(false, false);
            CreateSprite("Arrow", Sprite.Create(texture, new Rect(0, 0, 96, 72), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect), position, sortingOrder, scale);
        }

        private static void CreatePanel(string name, Vector3 position, Vector2 size, Color32 color, int sortingOrder)
        {
            var texture = NewTexture(8, 8, color);
            texture.Apply(false, false);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), PixelsPerUnit);
            var spriteObject = new GameObject(name);
            spriteObject.transform.position = position;
            spriteObject.transform.localScale = new Vector3(size.x * 4f, size.y * 4f, 1f);
            var renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
        }

        private static void CreateSprite(string name, Sprite sprite, Vector3 position, int sortingOrder, Vector2 scale)
        {
            var spriteObject = new GameObject(name);
            spriteObject.transform.position = position;
            spriteObject.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            var renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
        }

        private static void CreateLabel(string text, Vector3 position, float characterSize, Color32 color, TextAnchor anchor, int sortingOrder)
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
            renderer.sortingOrder = sortingOrder;
        }

        private static Sprite CreateHeroToken()
        {
            var texture = NewTexture(48, 48, new Color32(0, 0, 0, 0));
            FillRect(texture, 16, 18, 17, 21, new Color32(86, 211, 186, 255));
            FillRect(texture, 20, 8, 11, 11, new Color32(246, 198, 75, 255));
            FillRect(texture, 23, 38, 8, 8, new Color32(215, 151, 62, 255));
            DrawBorder(texture, 15, 17, 19, 23, new Color32(17, 22, 27, 255));
            DrawBorder(texture, 19, 7, 13, 13, new Color32(17, 22, 27, 255));
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.42f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Sprite CreateLairToken()
        {
            var texture = NewTexture(64, 64, new Color32(0, 0, 0, 0));
            DrawDiamond(texture, 32, 34, 24, new Color32(55, 25, 36, 255));
            DrawTriangle(texture, new Vector2Int(14, 45), new Vector2Int(31, 11), new Vector2Int(51, 45), new Color32(255, 103, 88, 255));
            FillRect(texture, 20, 42, 28, 12, new Color32(22, 16, 22, 255));
            FillRect(texture, 25, 47, 5, 5, new Color32(255, 231, 165, 255));
            FillRect(texture, 40, 47, 5, 5, new Color32(255, 231, 165, 255));
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.42f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Sprite CreateBossThreatToken()
        {
            var texture = NewTexture(80, 80, new Color32(0, 0, 0, 0));
            DrawDiamond(texture, 40, 40, 30, new Color32(53, 25, 36, 255));
            DrawTriangle(texture, new Vector2Int(23, 55), new Vector2Int(42, 18), new Vector2Int(61, 55), new Color32(255, 103, 88, 255));
            FillRect(texture, 29, 55, 28, 12, new Color32(22, 16, 22, 255));
            FillRect(texture, 32, 61, 6, 6, new Color32(255, 231, 165, 255));
            FillRect(texture, 51, 61, 6, 6, new Color32(255, 231, 165, 255));
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, 80, 80), new Vector2(0.5f, 0.45f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Sprite CreateHazardToken()
        {
            var texture = NewTexture(48, 48, new Color32(0, 0, 0, 0));
            FillRect(texture, 7, 7, 34, 34, new Color32(42, 22, 28, 255));
            DrawTriangle(texture, new Vector2Int(13, 36), new Vector2Int(24, 12), new Vector2Int(37, 36), new Color32(255, 103, 88, 255));
            DrawBorder(texture, 7, 7, 34, 34, new Color32(17, 22, 27, 255));
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Sprite CreateStickToken()
        {
            var texture = NewTexture(48, 48, new Color32(0, 0, 0, 0));
            FillRect(texture, 12, 30, 26, 6, new Color32(91, 61, 37, 255));
            FillRect(texture, 31, 25, 6, 11, new Color32(126, 84, 45, 255));
            FillRect(texture, 9, 32, 7, 4, new Color32(173, 122, 63, 255));
            FillRect(texture, 12, 29, 26, 2, new Color32(214, 160, 89, 255));
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Sprite CreateRewardToken()
        {
            var texture = NewTexture(48, 48, new Color32(0, 0, 0, 0));
            DrawDiamond(texture, 24, 24, 18, new Color32(241, 200, 77, 255));
            DrawTriangle(texture, new Vector2Int(20, 26), new Vector2Int(25, 9), new Vector2Int(32, 26), new Color32(255, 242, 173, 255));
            DrawDiamond(texture, 24, 24, 7, new Color32(255, 230, 126, 255));
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Sprite CreateExtractToken()
        {
            var texture = NewTexture(56, 64, new Color32(0, 0, 0, 0));
            FillRect(texture, 8, 4, 40, 56, new Color32(18, 48, 68, 255));
            FillRect(texture, 14, 14, 28, 39, new Color32(89, 197, 214, 255));
            FillRect(texture, 8, 4, 5, 56, new Color32(134, 237, 255, 255));
            FillRect(texture, 43, 4, 5, 56, new Color32(134, 237, 255, 255));
            FillRect(texture, 17, 25, 24, 5, new Color32(229, 254, 255, 255));
            FillRect(texture, 17, 38, 24, 5, new Color32(229, 254, 255, 255));
            DrawBorder(texture, 8, 4, 40, 56, new Color32(17, 22, 27, 255));
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, 56, 64), new Vector2(0.5f, 0.45f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
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
            FillRect(texture, x, y, width, 2, color);
            FillRect(texture, x, y + height - 2, width, 2, color);
            FillRect(texture, x, y, 2, height, color);
            FillRect(texture, x + width - 2, y, 2, height, color);
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
