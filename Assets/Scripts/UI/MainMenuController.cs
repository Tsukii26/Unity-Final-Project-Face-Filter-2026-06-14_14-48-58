using UnityEngine;
using UnityEngine.UI;
using FaceFilter.Core;

namespace FaceFilter.UI
{
    /// <summary>
    /// Bootstraps the Main Menu scene. The scene asset only contains a single GameObject
    /// with this component; the entire themed menu is built here at runtime.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        private void Start()
        {
            AppSettings.ApplyQuality();
            UIFactory.EnsureEventSystem();
            BuildUI();
        }

        private void BuildUI()
        {
            var canvas = UIFactory.CreateCanvas("MenuCanvas");

            // Gradient-ish background (solid base + accent glow card).
            UIFactory.CreateSolidFullScreen(canvas.transform, AppTheme.Background, "BG");

            var glow = UIFactory.CreatePanel(canvas.transform, AppTheme.WithAlpha(AppTheme.Primary, 0.18f), "Glow");
            glow.anchorMin = new Vector2(0.5f, 1f);
            glow.anchorMax = new Vector2(0.5f, 1f);
            glow.pivot = new Vector2(0.5f, 1f);
            glow.sizeDelta = new Vector2(1400, 900);
            glow.anchoredPosition = new Vector2(0, 200);

            // Header.
            var header = UIFactory.CreateColumn(canvas.transform, 8, "Header");
            var headerRt = (RectTransform)header.transform;
            headerRt.anchorMin = new Vector2(0.1f, 0.62f);
            headerRt.anchorMax = new Vector2(0.9f, 0.9f);
            headerRt.offsetMin = Vector2.zero;
            headerRt.offsetMax = Vector2.zero;

            var badge = UIFactory.CreateText(headerRt, "AR  •  FACE FILTER STUDIO", AppTheme.FontSizeCaption, AppTheme.Accent);
            badge.fontStyle = FontStyle.Bold;
            var title = UIFactory.CreateText(headerRt, "Face\nFilter Studio", AppTheme.FontSizeTitle, AppTheme.TextPrimary);
            title.fontStyle = FontStyle.Bold;
            UIFactory.CreateText(headerRt, "Try on glasses, masks, hats & more in real time.",
                AppTheme.FontSizeBody, AppTheme.TextMuted);

            // Button column.
            var column = UIFactory.CreateColumn(canvas.transform, AppTheme.Spacing, "MenuButtons");
            var colRt = (RectTransform)column.transform;
            colRt.anchorMin = new Vector2(0.12f, 0.1f);
            colRt.anchorMax = new Vector2(0.88f, 0.5f);
            colRt.offsetMin = Vector2.zero;
            colRt.offsetMax = Vector2.zero;
            column.childAlignment = TextAnchor.UpperCenter;
            column.childControlHeight = true;

            AddMenuButton(colRt, "Start AR Filters", AppTheme.Primary, () => SceneNavigator.Instance.GoTo(SceneNavigator.ARScene));
            AddMenuButton(colRt, "Demo Mode", AppTheme.Accent, () => SceneNavigator.Instance.GoTo(SceneNavigator.DemoScene));
            AddMenuButton(colRt, "Settings", AppTheme.SurfaceRaised, () => SettingsPanel.Show(canvas.transform));
#if !UNITY_WEBGL
            AddMenuButton(colRt, "Quit", AppTheme.Surface, Quit);
#endif

            // Footer.
            var footer = UIFactory.CreateText(canvas.transform, "Made with Unity AR Foundation", AppTheme.FontSizeCaption, AppTheme.TextMuted);
            var fRt = footer.rectTransform;
            fRt.anchorMin = new Vector2(0, 0);
            fRt.anchorMax = new Vector2(1, 0);
            fRt.pivot = new Vector2(0.5f, 0);
            fRt.sizeDelta = new Vector2(0, 60);
            fRt.anchoredPosition = new Vector2(0, 30);
        }

        private void AddMenuButton(Transform parent, string label, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var btn = UIFactory.CreateButton(parent, label, onClick, color);
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.minHeight = AppTheme.ButtonHeight;
            le.preferredHeight = AppTheme.ButtonHeight;
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
