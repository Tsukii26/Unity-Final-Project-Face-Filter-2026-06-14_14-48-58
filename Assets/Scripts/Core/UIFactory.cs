using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace FaceFilter.Core
{
    /// <summary>
    /// Builds themed uGUI elements entirely from code. Keeping the UI construction in
    /// one factory means the whole app shares the same modern, rounded, high-contrast
    /// styling and the scenes stay tiny (one bootstrap object instead of fragile YAML).
    /// </summary>
    public static class UIFactory
    {
        private static Sprite _roundedSprite;

        /// <summary>Procedurally generated rounded-rectangle sprite used for cards/buttons.</summary>
        public static Sprite RoundedSprite
        {
            get
            {
                if (_roundedSprite == null) _roundedSprite = GenerateRoundedSprite(96, 24);
                return _roundedSprite;
            }
        }

        public static Sprite CircleSprite
        {
            get { return GenerateCircleSprite(128); }
        }

        public static Canvas CreateCanvas(string name, int sortingOrder = 0)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static RectTransform CreatePanel(Transform parent, Color color, string name = "Panel")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = RoundedSprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            return rt;
        }

        public static RectTransform CreateFullScreen(Transform parent, Color color, string name = "FullScreen")
        {
            var rt = CreatePanel(parent, color, name);
            Stretch(rt);
            return rt;
        }

        /// <summary>Plain solid-color image (no sprite) — avoids sliced-sprite warnings for overlays.</summary>
        public static Image CreateSolid(Transform parent, Color color, string name = "Solid")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.GetComponent<RectTransform>().SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        public static Image CreateSolidFullScreen(Transform parent, Color color, string name = "Solid")
        {
            var img = CreateSolid(parent, color, name);
            Stretch(img.rectTransform);
            return img;
        }

        public static Text CreateText(Transform parent, string content, int fontSize, Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = AppTheme.Font;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = anchor;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        /// <summary>Creates a primary pill button with a label and click handler.</summary>
        public static Button CreateButton(Transform parent, string label, UnityAction onClick,
            Color? bg = null, Color? textColor = null)
        {
            var go = new GameObject("Button_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(0, AppTheme.ButtonHeight);

            var img = go.GetComponent<Image>();
            img.sprite = RoundedSprite;
            img.type = Image.Type.Sliced;
            img.color = bg ?? AppTheme.Primary;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            ApplyButtonColors(btn, img.color);
            if (onClick != null) btn.onClick.AddListener(onClick);

            var text = CreateText(rt, label, AppTheme.FontSizeButton, textColor ?? AppTheme.TextPrimary);
            text.fontStyle = FontStyle.Bold;
            Stretch(text.rectTransform);
            return btn;
        }

        /// <summary>Round icon button (used for the in-AR control bar). Label is a glyph/emoji.</summary>
        public static Button CreateIconButton(Transform parent, string glyph, UnityAction onClick, Color? bg = null)
        {
            var go = new GameObject("IconButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(AppTheme.IconButtonSize, AppTheme.IconButtonSize);

            var img = go.GetComponent<Image>();
            img.sprite = CircleSprite;
            img.type = Image.Type.Simple;
            img.color = bg ?? AppTheme.Surface;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            ApplyButtonColors(btn, img.color);
            if (onClick != null) btn.onClick.AddListener(onClick);

            var text = CreateText(rt, glyph, 56, AppTheme.TextPrimary);
            text.fontStyle = FontStyle.Bold;
            Stretch(text.rectTransform);
            return btn;
        }

        /// <summary>
        /// Horizontal layout. By default children keep their own RectTransform size
        /// (manual sizing). Set <paramref name="controlChildren"/> when you want the
        /// group to drive child widths via LayoutElement/flexibleWidth.
        /// </summary>
        public static HorizontalLayoutGroup CreateRow(Transform parent, float spacing, string name = "Row", bool controlChildren = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            go.GetComponent<RectTransform>().SetParent(parent, false);
            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = controlChildren;
            layout.childControlHeight = controlChildren;
            layout.childForceExpandWidth = controlChildren;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static VerticalLayoutGroup CreateColumn(Transform parent, float spacing, string name = "Column")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.GetComponent<RectTransform>().SetParent(parent, false);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static Toggle CreateToggle(Transform parent, string label, bool value, UnityAction<bool> onChanged)
        {
            var go = new GameObject("Toggle_" + label, typeof(RectTransform), typeof(Image), typeof(Toggle), typeof(HorizontalLayoutGroup));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(0, AppTheme.ButtonHeight);
            var bg = go.GetComponent<Image>();
            bg.sprite = RoundedSprite;
            bg.type = Image.Type.Sliced;
            bg.color = AppTheme.SurfaceRaised;

            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 0, 0);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.spacing = 24;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var labelText = CreateText(rt, label, AppTheme.FontSizeBody, AppTheme.TextPrimary, TextAnchor.MiddleLeft);
            var le = labelText.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            // Check box graphic.
            var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            var boxRt = box.GetComponent<RectTransform>();
            boxRt.SetParent(rt, false);
            boxRt.sizeDelta = new Vector2(72, 72);
            var boxImg = box.GetComponent<Image>();
            boxImg.sprite = RoundedSprite;
            boxImg.type = Image.Type.Sliced;
            boxImg.color = AppTheme.Background;

            var check = new GameObject("Check", typeof(RectTransform), typeof(Image));
            var checkRt = check.GetComponent<RectTransform>();
            checkRt.SetParent(boxRt, false);
            Stretch(checkRt, 12);
            var checkImg = check.GetComponent<Image>();
            checkImg.sprite = RoundedSprite;
            checkImg.type = Image.Type.Sliced;
            checkImg.color = AppTheme.Accent;

            var toggle = go.GetComponent<Toggle>();
            toggle.targetGraphic = bg;
            toggle.graphic = checkImg;
            toggle.isOn = value;
            if (onChanged != null) toggle.onValueChanged.AddListener(onChanged);
            return toggle;
        }

        /// <summary>Guarantees there is an EventSystem in the scene so UI is interactive.</summary>
        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
#if UNITY_2023_1_OR_NEWER
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;
#else
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null) return;
#endif
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public static void Stretch(RectTransform rt, float padding = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padding, padding);
            rt.offsetMax = new Vector2(-padding, -padding);
        }

        private static void ApplyButtonColors(Button btn, Color baseColor)
        {
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
        }

        // --- Procedural sprite generation (so the project needs no imported art) ---

        public static Sprite GenerateRoundedSprite(int size, int radius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = InsideRoundedRect(x, y, size, size, radius) ? Color.white : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            var border = new Vector4(radius, radius, radius, radius);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        public static Sprite GenerateCircleSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - r;
                    float dy = y + 0.5f - r;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(r - d);
                    pixels[y * size + x] = new Color(1, 1, 1, a);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static bool InsideRoundedRect(int x, int y, int w, int h, int radius)
        {
            int rx = Mathf.Min(radius, w / 2);
            int ry = Mathf.Min(radius, h / 2);
            int cxMin = rx, cxMax = w - rx - 1;
            int cyMin = ry, cyMax = h - ry - 1;
            int nx = x, ny = y;
            if (x < cxMin && y < cyMin) { nx = cxMin; ny = cyMin; }
            else if (x > cxMax && y < cyMin) { nx = cxMax; ny = cyMin; }
            else if (x < cxMin && y > cyMax) { nx = cxMin; ny = cyMax; }
            else if (x > cxMax && y > cyMax) { nx = cxMax; ny = cyMax; }
            else return true;
            float dx = x - nx, dy = y - ny;
            return (dx * dx + dy * dy) <= (rx * rx);
        }
    }
}
