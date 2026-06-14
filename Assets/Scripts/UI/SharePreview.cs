using UnityEngine;
using UnityEngine.UI;
using FaceFilter.Core;
using FaceFilter.Capture;

namespace FaceFilter.UI
{
    /// <summary>
    /// Modal that previews a freshly captured screenshot and offers Share / Done.
    /// The image is already saved to disk (and the gallery on Android) by
    /// <see cref="ScreenshotCapture"/>; this just confirms and enables sharing.
    /// </summary>
    public class SharePreview : MonoBehaviour
    {
        private string _filePath;

        public static SharePreview Show(Transform canvas, Texture2D texture, string filePath)
        {
            var overlay = UIFactory.CreateSolidFullScreen(canvas, AppTheme.Overlay, "SharePreview").rectTransform;
            var preview = overlay.gameObject.AddComponent<SharePreview>();
            preview._filePath = filePath;
            preview.Build(overlay, texture);
            return preview;
        }

        private void Build(RectTransform overlay, Texture2D texture)
        {
            var card = UIFactory.CreatePanel(overlay, AppTheme.Surface, "PreviewCard");
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(880, 1280);

            var column = UIFactory.CreateColumn(card, AppTheme.Spacing, "Column");
            UIFactory.Stretch((RectTransform)column.transform, AppTheme.Padding);
            column.childAlignment = TextAnchor.UpperCenter;
            column.childForceExpandHeight = false;

            var title = UIFactory.CreateText(column.transform, "Nice shot!", AppTheme.FontSizeHeading, AppTheme.TextPrimary);
            title.fontStyle = FontStyle.Bold;
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 90;

            // Image preview.
            var imgGo = new GameObject("Preview", typeof(RectTransform), typeof(RawImage));
            imgGo.GetComponent<RectTransform>().SetParent(column.transform, false);
            var raw = imgGo.GetComponent<RawImage>();
            raw.texture = texture;
            float aspect = texture.height == 0 ? 1f : (float)texture.width / texture.height;
            var le = imgGo.AddComponent<LayoutElement>();
            le.preferredHeight = 820;
            le.preferredWidth = 820 * aspect;
            var fitter = imgGo.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = aspect;

            var caption = UIFactory.CreateText(column.transform, "Saved to your gallery", AppTheme.FontSizeCaption, AppTheme.TextMuted);
            caption.gameObject.AddComponent<LayoutElement>().preferredHeight = 50;

            var row = UIFactory.CreateRow(column.transform, AppTheme.Spacing, "Actions", controlChildren: true);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = AppTheme.ButtonHeight;

            var done = UIFactory.CreateButton(row.transform, "Done", Close, AppTheme.SurfaceRaised);
            var doneLe = done.gameObject.AddComponent<LayoutElement>();
            doneLe.flexibleWidth = 1; doneLe.preferredHeight = AppTheme.ButtonHeight;
            var share = UIFactory.CreateButton(row.transform, "Share", OnShare, AppTheme.Primary);
            var shareLe = share.gameObject.AddComponent<LayoutElement>();
            shareLe.flexibleWidth = 1; shareLe.preferredHeight = AppTheme.ButtonHeight;
        }

        private void OnShare()
        {
            NativeShare.ShareImage(_filePath);
        }

        public void Close()
        {
            Destroy(gameObject);
        }
    }
}
