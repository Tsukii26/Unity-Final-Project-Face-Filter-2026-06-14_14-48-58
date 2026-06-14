using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FaceFilter.Core
{
    /// <summary>A full-screen black overlay used to fade scene transitions in and out.</summary>
    public class FadeTransition : MonoBehaviour
    {
        private CanvasGroup _group;
        public float duration = 0.35f;

        public static FadeTransition Create(Transform parent)
        {
            var canvas = UIFactory.CreateCanvas("FadeCanvas", sortingOrder: 999);
            canvas.transform.SetParent(parent, false);

            var fader = canvas.gameObject.AddComponent<FadeTransition>();
            var group = canvas.gameObject.AddComponent<CanvasGroup>();
            fader._group = group;

            UIFactory.CreateSolidFullScreen(canvas.transform, Color.black, "Fade");
            group.alpha = 0f;
            group.blocksRaycasts = false;
            return fader;
        }

        public IEnumerator FadeOut() { yield return Fade(0f, 1f); }
        public IEnumerator FadeIn() { yield return Fade(1f, 0f); }

        private IEnumerator Fade(float from, float to)
        {
            _group.blocksRaycasts = true;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            _group.alpha = to;
            _group.blocksRaycasts = to > 0.5f;
        }
    }
}
