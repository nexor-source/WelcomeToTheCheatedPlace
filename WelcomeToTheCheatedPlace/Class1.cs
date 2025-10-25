using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TextAnchor = UnityEngine.TextAnchor;

namespace DarkPlaceRoomOverlay
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class RoomOverlayPlugin : BaseUnityPlugin
    {
        private const string ModGuid = "nexor.darkplace.roomoverlay";
        private const string ModName = "DarkPlaceRoomOverlay";
        private const string ModVersion = "0.1.0";

        private static readonly Harmony Harmony = new Harmony(ModGuid);

        internal static RoomOverlayPlugin Instance { get; private set; }
        internal static RoomsNode CurrentNode { get; private set; }
        internal static RoomsNode PreviousNode { get; private set; }

        private ConfigEntry<KeyboardShortcut> toggleKey;
        private bool overlayVisible = true;

        private readonly Button[] choiceButtons = new Button[4];
        private readonly Vector3[] worldCornersBuffer = new Vector3[4];
        private GUIStyle headerStyle;
        private GUIStyle tooltipStyle;
        private int lastStyleScreenHeight = -1;

        private void Awake()
        {
            Instance = this;
            toggleKey = Config.Bind("UI", "ToggleKey", new KeyboardShortcut(KeyCode.F8), "显示/隐藏房间信息面板的快捷键。");

            Harmony.PatchAll();
            Logger.LogInfo($"{ModName} {ModVersion} loaded");
        }

        private void Update()
        {
            if (toggleKey.Value.IsDown())
            {
                overlayVisible = !overlayVisible;
            }
        }

        private void OnDestroy()
        {
            Harmony.UnpatchSelf();
        }

        internal static void RegisterNode(RoomManager manager, RoomsNode node)
        {
            if (Instance == null || manager == null || node == null)
            {
                return;
            }

            PreviousNode = CurrentNode;
            CurrentNode = node;
            Instance.CacheChoiceButtons(manager);
        }

        private void CacheChoiceButtons(RoomManager manager)
        {
            choiceButtons[0] = manager.m_button1;
            choiceButtons[1] = manager.m_button2;
            choiceButtons[2] = manager.m_button3;
            choiceButtons[3] = manager.m_button4;
        }

        private void OnGUI()
        {
            if (!overlayVisible || CurrentNode == null)
            {
                return;
            }

            EnsureStyles();
            UpdateStyleMetrics();
            DrawHeader();
            DrawChoiceTooltip();
        }

        private void EnsureStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 15,
                    padding = new RectOffset(10, 10, 4, 4)
                };
            }

            if (tooltipStyle == null)
            {
                tooltipStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = true,
                    fontSize = 14,
                    padding = new RectOffset(8, 8, 6, 6)
                };
            }
        }

        private void UpdateStyleMetrics()
        {
            if (headerStyle == null || tooltipStyle == null)
            {
                return;
            }

            int screenHeight = Screen.height;
            if (screenHeight == lastStyleScreenHeight)
            {
                return;
            }

            float scale = Mathf.Clamp(screenHeight / 900f, 0.75f, 2.0f);

            headerStyle.fontSize = Mathf.RoundToInt(17f * scale);
            int horizontalPadding = Mathf.Clamp(Mathf.RoundToInt(10f * scale), 6, 22);
            int verticalPadding = Mathf.Clamp(Mathf.RoundToInt(4f * scale), 2, 12);
            headerStyle.padding = new RectOffset(horizontalPadding, horizontalPadding, verticalPadding, verticalPadding);

            tooltipStyle.fontSize = Mathf.RoundToInt(16f * scale);
            int tooltipHPadding = Mathf.Clamp(Mathf.RoundToInt(8f * scale), 4, 20);
            int tooltipTopPadding = Mathf.Clamp(Mathf.RoundToInt(6f * scale), 3, 16);
            tooltipStyle.padding = new RectOffset(tooltipHPadding, tooltipHPadding, tooltipTopPadding, tooltipTopPadding);

            lastStyleScreenHeight = screenHeight;
        }

        private void DrawHeader()
        {
            const float marginX = 8f;
            const float marginY = 6f;

            string areaName = string.IsNullOrEmpty(CurrentNode.ParentFolder) ? "未知区域" : CurrentNode.ParentFolder;
            string headerText = $"当前房间: {CurrentNode.name}   区域: {areaName}";
            GUIContent content = new GUIContent(headerText);
            Vector2 size = headerStyle.CalcSize(content);
            Rect rect = new Rect(marginX, marginY, size.x, size.y);
            GUI.Box(rect, content, headerStyle);
        }

        private void DrawChoiceTooltip()
        {
            Vector2 mouse = UnityEngine.Input.mousePosition;

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                Button button = choiceButtons[i];
                if (!IsButtonHoverable(button))
                {
                    continue;
                }

                RectTransform rectTransform = button.GetComponent<RectTransform>();
                Canvas canvas = button.GetComponentInParent<Canvas>();
                Camera canvasCamera = canvas != null ? canvas.worldCamera : null;

                if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mouse, canvasCamera))
                {
                    continue;
                }

                Rect guiRect = GetGuiRect(rectTransform, canvasCamera);
                string tooltipText = BuildTooltipText(i);

                if (string.IsNullOrEmpty(tooltipText))
                {
                    continue;
                }

                Vector2 tooltipSize = tooltipStyle.CalcSize(new GUIContent(tooltipText));
                float tooltipWidth = tooltipSize.x + 12f;
                float tooltipHeight = tooltipSize.y;

                float tooltipX = guiRect.xMin;
                float tooltipY = guiRect.yMin;


                Rect tooltipRect = new Rect(tooltipX, tooltipY, tooltipWidth, tooltipHeight);
                GUI.Box(tooltipRect, tooltipText, tooltipStyle);
                break;
            }
        }

        private bool IsButtonHoverable(Button button)
        {
            if (button == null)
            {
                return false;
            }

            if (!button.isActiveAndEnabled || !button.gameObject.activeInHierarchy)
            {
                return false;
            }

            return button.GetComponentInChildren<Text>() != null;
        }

        private Rect GetGuiRect(RectTransform rectTransform, Camera canvasCamera)
        {
            rectTransform.GetWorldCorners(worldCornersBuffer);
            Vector3 topLeft = RectTransformUtility.WorldToScreenPoint(canvasCamera, worldCornersBuffer[1]);
            Vector3 topRight = RectTransformUtility.WorldToScreenPoint(canvasCamera, worldCornersBuffer[2]);
            Vector3 bottomLeft = RectTransformUtility.WorldToScreenPoint(canvasCamera, worldCornersBuffer[0]);

            float width = topRight.x - topLeft.x;
            float height = topLeft.y - bottomLeft.y;
            float guiX = topLeft.x;
            float guiY = Screen.height - topLeft.y;

            return new Rect(guiX, guiY, width, height);
        }

        private string BuildTooltipText(int index)
        {
            string label;
            RoomsNode target;

            switch (index)
            {
                case 0:
                    label = CurrentNode.m_button1Text;
                    target = CurrentNode.m_button1Node;
                    break;
                case 1:
                    label = CurrentNode.m_button2Text;
                    target = CurrentNode.m_button2Node;
                    break;
                case 2:
                    label = CurrentNode.m_button3Text;
                    target = CurrentNode.m_button3Node;
                    break;
                case 3:
                    label = CurrentNode.m_button4Text;
                    target = CurrentNode.m_button4Node;
                    break;
                default:
                    return null;
            }

            if (string.IsNullOrEmpty(label) && target == null)
            {
                return null;
            }

            string targetName;
            string targetArea;

            if (target != null)
            {
                targetName = target.name;
                targetArea = string.IsNullOrEmpty(target.ParentFolder) ? "未知区域" : target.ParentFolder;
            }
            else if (!string.IsNullOrEmpty(CurrentNode.SceneName))
            {
                targetName = $"加载场景 {CurrentNode.SceneName}";
                targetArea = "-";
            }
            else
            {
                targetName = "暂无后续";
                targetArea = "-";
            }

            return $"→ {targetName} ({targetArea})";
        }
    }

    [HarmonyPatch(typeof(RoomManager))]
    internal static class RoomManagerShowNodePatch
    {
        [HarmonyPatch("ShowNode")]
        [HarmonyPostfix]
        private static void AfterShowNode(RoomManager __instance, RoomsNode node)
        {
            RoomOverlayPlugin.RegisterNode(__instance, node);
        }
    }
}
