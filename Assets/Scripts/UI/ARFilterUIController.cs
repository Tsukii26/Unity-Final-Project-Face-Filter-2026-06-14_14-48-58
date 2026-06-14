using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FaceFilter.Core;
using FaceFilter.AR;
using FaceFilter.Capture;

namespace FaceFilter.UI
{
    /// <summary>
    /// Bootstraps the AR Face Filter scene: builds the AR rig, the themed control
    /// overlay (filter carousel, toggle, camera switch, capture/share, settings, back)
    /// and wires every control to the underlying managers.
    /// </summary>
    public class ARFilterUIController : MonoBehaviour
    {
        private AppRig _rig;
        private IFilterController _filters;
        private ScreenshotCapture _capture;

        private Canvas _canvas;
        private GameObject _controls;     // hidden during screenshot capture
        private Text _filterNameLabel;
        private Text _statusLabel;
        private CanvasGroup _flash;

        private void Start()
        {
            AppSettings.ApplyQuality();
            UIFactory.EnsureEventSystem();

            _rig = AppRig.Build();
            _filters = _rig.Filters;
            _filters.FilterChanged += OnFilterChanged;

            _capture = gameObject.AddComponent<ScreenshotCapture>();

            BuildUI();
            UpdateFilterLabel();
        }

        private void OnDestroy()
        {
            if (_filters != null) _filters.FilterChanged -= OnFilterChanged;
            if (_rig != null && _rig.Root != null) Destroy(_rig.Root);
        }

        private void BuildUI()
        {
            _canvas = UIFactory.CreateCanvas("ARCanvas");
            _controls = new GameObject("Controls", typeof(RectTransform));
            var controlsRt = _controls.GetComponent<RectTransform>();
            controlsRt.SetParent(_canvas.transform, false);
            UIFactory.Stretch(controlsRt);

            BuildTopBar(controlsRt);
            BuildStatusHint(controlsRt);
            BuildBottomControls(controlsRt);
            BuildFlash(_canvas.transform);
        }

        private void BuildTopBar(Transform parent)
        {
            var bar = UIFactory.CreatePanel(parent, AppTheme.WithAlpha(AppTheme.Surface, 0.85f), "TopBar");
            bar.anchorMin = new Vector2(0, 1);
            bar.anchorMax = new Vector2(1, 1);
            bar.pivot = new Vector2(0.5f, 1);
            bar.sizeDelta = new Vector2(-2 * AppTheme.Padding, 150);
            bar.anchoredPosition = new Vector2(0, -AppTheme.Padding);

            var back = UIFactory.CreateIconButton(bar, "\u2039", () => SceneNavigator.Instance.GoTo(SceneNavigator.MainMenuScene), AppTheme.SurfaceRaised);
            Anchor(back.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(90, 0));
            back.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

            var barTitle = UIFactory.CreateText(bar, "AR Filters", AppTheme.FontSizeHeading, AppTheme.TextPrimary);
            UIFactory.Stretch(barTitle.rectTransform, 140);
            barTitle.fontStyle = FontStyle.Bold;

            var settings = UIFactory.CreateIconButton(bar, "\u2699", OpenSettings, AppTheme.SurfaceRaised);
            Anchor(settings.GetComponent<RectTransform>(), new Vector2(1, 0.5f), new Vector2(-90, 0));
            settings.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        }

        private void BuildStatusHint(Transform parent)
        {
            var hint = UIFactory.CreatePanel(parent, AppTheme.WithAlpha(AppTheme.Surface, 0.8f), "Hint");
            hint.anchorMin = new Vector2(0.5f, 1);
            hint.anchorMax = new Vector2(0.5f, 1);
            hint.pivot = new Vector2(0.5f, 1);
            hint.sizeDelta = new Vector2(720, 90);
            hint.anchoredPosition = new Vector2(0, -220);
            _statusLabel = UIFactory.CreateText(hint, "Point the camera at your face", AppTheme.FontSizeCaption, AppTheme.TextMuted);
            UIFactory.Stretch(_statusLabel.rectTransform, 16);
        }

        private void BuildBottomControls(Transform parent)
        {
            // Filter carousel: < [chip] >
            var carousel = UIFactory.CreateRow(parent, AppTheme.Spacing, "Carousel");
            var carRt = (RectTransform)carousel.transform;
            carRt.anchorMin = new Vector2(0.5f, 0);
            carRt.anchorMax = new Vector2(0.5f, 0);
            carRt.pivot = new Vector2(0.5f, 0);
            carRt.sizeDelta = new Vector2(900, 140);
            carRt.anchoredPosition = new Vector2(0, 320);

            var prev = UIFactory.CreateIconButton(carousel.transform, "\u2039", () => _filters.PreviousFilter(), AppTheme.Surface);
            prev.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);

            var chip = UIFactory.CreateButton(carousel.transform, "Filter", () => _filters.NextFilter(), AppTheme.SurfaceRaised);
            var chipRt = chip.GetComponent<RectTransform>();
            chipRt.sizeDelta = new Vector2(420, 120);
            _filterNameLabel = chip.GetComponentInChildren<Text>(); // chip doubles as the name label

            var next = UIFactory.CreateIconButton(carousel.transform, "\u203A", () => _filters.NextFilter(), AppTheme.Surface);
            next.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);

            // Main control row: toggle | CAPTURE | camera switch
            var row = UIFactory.CreateRow(parent, 60, "ControlRow");
            var rowRt = (RectTransform)row.transform;
            rowRt.anchorMin = new Vector2(0.5f, 0);
            rowRt.anchorMax = new Vector2(0.5f, 0);
            rowRt.pivot = new Vector2(0.5f, 0);
            rowRt.sizeDelta = new Vector2(900, 200);
            rowRt.anchoredPosition = new Vector2(0, 90);

            var toggle = UIFactory.CreateIconButton(row.transform, "\u25C9", () => { _filters.ToggleFilters(); UpdateFilterLabel(); }, AppTheme.Surface);
            toggle.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 130);

            var captureBtn = UIFactory.CreateIconButton(row.transform, "\u25CF", OnCapture, AppTheme.Primary);
            captureBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 190);

            var swap = UIFactory.CreateIconButton(row.transform, "\u21BB", OnSwapCamera, AppTheme.Surface);
            swap.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 130);
        }

        private void BuildFlash(Transform parent)
        {
            var flashCanvas = UIFactory.CreateCanvas("FlashCanvas", sortingOrder: 500);
            flashCanvas.transform.SetParent(parent, false);
            _flash = flashCanvas.gameObject.AddComponent<CanvasGroup>();
            UIFactory.CreateSolidFullScreen(flashCanvas.transform, Color.white, "Flash");
            _flash.alpha = 0;
            _flash.blocksRaycasts = false;
        }

        // --- behaviour ---

        private void Update()
        {
            if (_statusLabel == null || _rig == null) return;
            if (!_filters.FiltersEnabled)
            {
                _statusLabel.text = "Filters off";
            }
            else if (_rig.IsSimulation)
            {
                _statusLabel.text = "Simulation preview (no AR device)";
            }
            else if (_rig.FaceManager != null)
            {
                int count = _rig.FaceManager.trackables.count;
                if (count == 0)
                    _statusLabel.text = "Point the camera at your face";
                else
                    _statusLabel.text = count == 1 ? "Face tracked \u2713" : count + " faces tracked \u2713";
            }
        }

        private void OnSwapCamera()
        {
            if (_rig.CameraSwitcher != null) _rig.CameraSwitcher.Toggle();
        }

        private void OnFilterChanged(FilterEntry entry)
        {
            UpdateFilterLabel();
        }

        private void UpdateFilterLabel()
        {
            if (_filterNameLabel == null) return;
            var entry = _filters.Current;
            _filterNameLabel.text = _filters.FiltersEnabled ? (entry.Glyph + "  " + entry.DisplayName) : "Filters off";
        }

        private void OpenSettings()
        {
            SettingsPanel.Show(_canvas.transform, () => { _filters.ResetFilters(); UpdateFilterLabel(); });
        }

        private void OnCapture()
        {
            // Capture first (controls are hidden by ScreenshotCapture), then flash +
            // preview so the shutter flash is never baked into the saved image.
            _capture.Capture(_controls, result =>
            {
                StartCoroutine(FlashRoutine());
                if (result.Texture != null) SharePreview.Show(_canvas.transform, result.Texture, result.FilePath);
            });
        }

        private IEnumerator FlashRoutine()
        {
            _flash.alpha = 0.9f;
            while (_flash.alpha > 0)
            {
                _flash.alpha -= Time.deltaTime * 3f;
                yield return null;
            }
        }

        private static void Anchor(RectTransform rt, Vector2 anchor, Vector2 anchoredPos)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
        }
    }
}
