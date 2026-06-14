using UnityEngine;

namespace FaceFilter.Core
{
    /// <summary>
    /// Central, single source of truth for the app's visual identity (the "reskin").
    /// Every UI element pulls its colors, sizing and font from here so the look stays
    /// consistent and can be re-themed from one place.
    /// </summary>
    public static class AppTheme
    {
        // Brand palette (modern dark "glassmorphism" inspired theme).
        public static readonly Color Background = new Color32(0x12, 0x14, 0x1C, 0xFF);
        public static readonly Color Surface = new Color32(0x1E, 0x21, 0x2E, 0xE6);
        public static readonly Color SurfaceRaised = new Color32(0x2A, 0x2E, 0x40, 0xF2);
        public static readonly Color Primary = new Color32(0x6C, 0x5C, 0xE7, 0xFF);   // indigo
        public static readonly Color PrimaryDark = new Color32(0x52, 0x44, 0xC2, 0xFF);
        public static readonly Color Accent = new Color32(0x00, 0xD2, 0xC6, 0xFF);     // teal
        public static readonly Color Danger = new Color32(0xFF, 0x57, 0x6B, 0xFF);
        public static readonly Color TextPrimary = new Color32(0xF5, 0xF6, 0xFA, 0xFF);
        public static readonly Color TextMuted = new Color32(0xA0, 0xA6, 0xB8, 0xFF);
        public static readonly Color Overlay = new Color32(0x00, 0x00, 0x00, 0x99);

        // Layout constants (in reference pixels @ 1080x1920).
        public const float CornerRadius = 24f;
        public const float ButtonHeight = 132f;
        public const float IconButtonSize = 132f;
        public const float Padding = 36f;
        public const float Spacing = 28f;

        public const int FontSizeTitle = 96;
        public const int FontSizeHeading = 56;
        public const int FontSizeBody = 40;
        public const int FontSizeButton = 44;
        public const int FontSizeCaption = 30;

        private static Font _font;

        /// <summary>
        /// Returns a usable runtime font across all Unity versions. Newer Unity ships
        /// "LegacyRuntime.ttf" instead of the old "Arial.ttf"; we fall back gracefully.
        /// </summary>
        public static Font Font
        {
            get
            {
                if (_font != null) return _font;
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", AppTheme.FontSizeBody);
                return _font;
            }
        }

        public static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }
    }
}
