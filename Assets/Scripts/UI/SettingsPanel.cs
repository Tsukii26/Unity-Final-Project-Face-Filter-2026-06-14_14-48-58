using System;
using UnityEngine;
using UnityEngine.UI;
using FaceFilter.Core;

namespace FaceFilter.UI
{
    /// <summary>
    /// A reusable modal settings panel: render-quality selector, mirror-preview toggle,
    /// "reset filters" and "reset all" actions. Built from code via <see cref="UIFactory"/>
    /// so it shares the app theme and can be shown over any canvas.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        private Action _onResetFilters;
        private Text _qualityValue;

        public static SettingsPanel Show(Transform canvas, Action onResetFilters = null)
        {
            var overlayRt = UIFactory.CreateSolidFullScreen(canvas, AppTheme.Overlay, "SettingsOverlay").rectTransform;
            var panel = overlayRt.gameObject.AddComponent<SettingsPanel>();
            panel._onResetFilters = onResetFilters;
            panel.BuildContent(overlayRt);
            return panel;
        }

        private void BuildContent(RectTransform overlay)
        {
            // Dismiss when tapping the dimmed background.
            var bgButton = overlay.gameObject.AddComponent<Button>();
            bgButton.transition = Selectable.Transition.None;
            bgButton.onClick.AddListener(Close);

            var card = UIFactory.CreatePanel(overlay, AppTheme.Surface, "SettingsCard");
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(840, 1040);
            // Block click-through so taps inside the card don't dismiss it.
            card.gameObject.AddComponent<Button>().transition = Selectable.Transition.None;

            var column = UIFactory.CreateColumn(card, AppTheme.Spacing, "SettingsColumn");
            var colRt = (RectTransform)column.transform;
            UIFactory.Stretch(colRt, AppTheme.Padding);
            column.padding = new RectOffset(8, 8, 8, 8);
            column.childAlignment = TextAnchor.UpperCenter;

            var title = UIFactory.CreateText(colRt, "Settings", AppTheme.FontSizeHeading, AppTheme.TextPrimary);
            title.fontStyle = FontStyle.Bold;
            SetHeight(title.gameObject, 96);

            // Quality selector row.
            var qualityLabel = UIFactory.CreateText(colRt, "Render Quality", AppTheme.FontSizeBody, AppTheme.TextMuted, TextAnchor.MiddleLeft);
            SetHeight(qualityLabel.gameObject, 56);

            var qualityRow = UIFactory.CreateRow(colRt, AppTheme.Spacing, "QualityRow", controlChildren: true);
            qualityRow.childForceExpandWidth = false;
            SetHeight(qualityRow.gameObject, AppTheme.ButtonHeight);

            var minus = UIFactory.CreateButton(qualityRow.transform, "-", () => ChangeQuality(-1), AppTheme.SurfaceRaised);
            SetSize(minus.gameObject, 140, AppTheme.ButtonHeight);
            _qualityValue = UIFactory.CreateText(qualityRow.transform, AppSettings.QualityNames[AppSettings.QualityIndex],
                AppTheme.FontSizeBody, AppTheme.TextPrimary);
            var valLe = _qualityValue.gameObject.AddComponent<LayoutElement>();
            valLe.flexibleWidth = 1; valLe.preferredHeight = AppTheme.ButtonHeight;
            var plus = UIFactory.CreateButton(qualityRow.transform, "+", () => ChangeQuality(1), AppTheme.SurfaceRaised);
            SetSize(plus.gameObject, 140, AppTheme.ButtonHeight);

            // Mirror toggle.
            var mirror = UIFactory.CreateToggle(colRt, "Mirror preview", AppSettings.MirrorPreview,
                v => AppSettings.MirrorPreview = v);
            SetHeight(mirror.gameObject, AppTheme.ButtonHeight);

            // Reset filters.
            if (_onResetFilters != null)
            {
                var rf = UIFactory.CreateButton(colRt, "Reset Filters", () => { _onResetFilters(); }, AppTheme.SurfaceRaised);
                SetHeight(rf.gameObject, AppTheme.ButtonHeight);
            }

            // Reset all settings.
            var resetAll = UIFactory.CreateButton(colRt, "Reset All Settings", ResetAll, AppTheme.Danger);
            SetHeight(resetAll.gameObject, AppTheme.ButtonHeight);

            var spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.GetComponent<RectTransform>().SetParent(colRt, false);
            spacer.AddComponent<LayoutElement>().flexibleHeight = 1;

            var close = UIFactory.CreateButton(colRt, "Close", Close, AppTheme.Primary);
            SetHeight(close.gameObject, AppTheme.ButtonHeight);
        }

        private void ChangeQuality(int delta)
        {
            AppSettings.QualityIndex = Mathf.Clamp(AppSettings.QualityIndex + delta, 0, AppSettings.QualityNames.Length - 1);
            if (_qualityValue != null) _qualityValue.text = AppSettings.QualityNames[AppSettings.QualityIndex];
        }

        private void ResetAll()
        {
            AppSettings.ResetAll();
            if (_qualityValue != null) _qualityValue.text = AppSettings.QualityNames[AppSettings.QualityIndex];
        }

        public void Close()
        {
            Destroy(gameObject);
        }

        private static void SetHeight(GameObject go, float h)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.minHeight = h;
            le.preferredHeight = h;
        }

        private static void SetSize(GameObject go, float w, float h)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.minWidth = w;
            le.preferredWidth = w;
            le.flexibleWidth = 0;
            le.minHeight = h;
            le.preferredHeight = h;
        }
    }
}
