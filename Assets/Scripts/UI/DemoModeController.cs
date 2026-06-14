using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FaceFilter.Core;
using FaceFilter.AR;

namespace FaceFilter.UI
{
    /// <summary>
    /// "Demo Mode": automatically cycles through every filter on a timer with large,
    /// readable captions — ideal for recording a hands-free video walkthrough. Tap the
    /// screen to pause/resume; the back button returns to the menu.
    /// </summary>
    public class DemoModeController : MonoBehaviour
    {
        public float secondsPerFilter = 3.5f;

        private ARRigBuilder _rig;
        private FaceFilterManager _filters;
        private Text _nameLabel;
        private Text _counterLabel;
        private Image _progress;
        private bool _paused;
        private int _index;

        private void Start()
        {
            AppSettings.ApplyQuality();
            UIFactory.EnsureEventSystem();

            _rig = ARRigBuilder.Build();
            _filters = _rig.FilterManager;

            BuildUI();
            StartCoroutine(DemoRoutine());
        }

        private void OnDestroy()
        {
            if (_rig != null) Destroy(_rig.gameObject);
        }

        private void BuildUI()
        {
            var canvas = UIFactory.CreateCanvas("DemoCanvas");

            // Tap anywhere to pause/resume.
            var tap = UIFactory.CreateSolid(canvas.transform, new Color(0, 0, 0, 0), "TapCatcher").rectTransform;
            UIFactory.Stretch(tap);
            tap.GetComponent<Image>().raycastTarget = true;
            var tapBtn = tap.gameObject.AddComponent<Button>();
            tapBtn.transition = Selectable.Transition.None;
            tapBtn.onClick.AddListener(() => _paused = !_paused);

            // Top banner.
            var banner = UIFactory.CreatePanel(canvas.transform, AppTheme.WithAlpha(AppTheme.Surface, 0.85f), "Banner");
            banner.anchorMin = new Vector2(0.5f, 1);
            banner.anchorMax = new Vector2(0.5f, 1);
            banner.pivot = new Vector2(0.5f, 1);
            banner.sizeDelta = new Vector2(960, 200);
            banner.anchoredPosition = new Vector2(0, -AppTheme.Padding);

            var demoTag = UIFactory.CreateText(banner, "DEMO MODE", AppTheme.FontSizeCaption, AppTheme.Accent);
            demoTag.fontStyle = FontStyle.Bold;
            var tagRt = demoTag.rectTransform;
            tagRt.anchorMin = new Vector2(0, 1); tagRt.anchorMax = new Vector2(1, 1);
            tagRt.pivot = new Vector2(0.5f, 1); tagRt.sizeDelta = new Vector2(-40, 50); tagRt.anchoredPosition = new Vector2(0, -16);

            _nameLabel = UIFactory.CreateText(banner, "Filter", AppTheme.FontSizeHeading, AppTheme.TextPrimary);
            _nameLabel.fontStyle = FontStyle.Bold;
            UIFactory.Stretch(_nameLabel.rectTransform, 24);

            _counterLabel = UIFactory.CreateText(banner, "1 / 8", AppTheme.FontSizeCaption, AppTheme.TextMuted);
            var cRt = _counterLabel.rectTransform;
            cRt.anchorMin = new Vector2(0, 0); cRt.anchorMax = new Vector2(1, 0);
            cRt.pivot = new Vector2(0.5f, 0); cRt.sizeDelta = new Vector2(-40, 50); cRt.anchoredPosition = new Vector2(0, 16);

            // Progress bar under the banner.
            var track = UIFactory.CreatePanel(canvas.transform, AppTheme.Surface, "ProgressTrack");
            track.anchorMin = new Vector2(0.5f, 1); track.anchorMax = new Vector2(0.5f, 1);
            track.pivot = new Vector2(0.5f, 1);
            track.sizeDelta = new Vector2(960, 12); track.anchoredPosition = new Vector2(0, -AppTheme.Padding - 205);
            _progress = UIFactory.CreatePanel(track, AppTheme.Accent, "ProgressFill").GetComponent<Image>();
            var pRt = _progress.rectTransform;
            pRt.anchorMin = new Vector2(0, 0); pRt.anchorMax = new Vector2(0, 1);
            pRt.pivot = new Vector2(0, 0.5f); pRt.offsetMin = Vector2.zero; pRt.offsetMax = Vector2.zero;
            pRt.sizeDelta = new Vector2(0, 0);

            // Back button.
            var back = UIFactory.CreateButton(canvas.transform, "\u2039  Back", () => SceneNavigator.Instance.GoTo(SceneNavigator.MainMenuScene), AppTheme.SurfaceRaised);
            var bRt = back.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0); bRt.anchorMax = new Vector2(0.5f, 0);
            bRt.pivot = new Vector2(0.5f, 0); bRt.sizeDelta = new Vector2(420, AppTheme.ButtonHeight);
            bRt.anchoredPosition = new Vector2(0, 90);
        }

        private IEnumerator DemoRoutine()
        {
            // Start from the first real filter (skip "None").
            _index = 1;
            while (true)
            {
                ApplyCurrent();
                float t = 0f;
                while (t < secondsPerFilter)
                {
                    if (!_paused) t += Time.deltaTime;
                    SetProgress(t / secondsPerFilter);
                    yield return null;
                }
                _index++;
            }
        }

        private void ApplyCurrent()
        {
            _filters.SetFilter(_index);
            var entry = _filters.Current;
            if (_nameLabel != null) _nameLabel.text = entry.Glyph + "  " + entry.DisplayName;
            if (_counterLabel != null)
                _counterLabel.text = (_filters.CurrentIndex + 1) + " / " + FilterCatalog.Entries.Count + (_paused ? "   (paused)" : "");
        }

        private void SetProgress(float normalized)
        {
            if (_progress == null) return;
            var rt = _progress.rectTransform;
            rt.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1);
            if (_counterLabel != null)
                _counterLabel.text = (_filters.CurrentIndex + 1) + " / " + FilterCatalog.Entries.Count + (_paused ? "   (paused)" : "");
        }
    }
}
