using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FaceFilter.Capture
{
    /// <summary>
    /// Captures the current AR frame (camera background + 3D filters + minus the UI we
    /// hide during capture), saves it as a PNG and exposes the result so callers can
    /// preview, save-to-gallery and share it.
    /// </summary>
    public class ScreenshotCapture : MonoBehaviour
    {
        public struct Result
        {
            public Texture2D Texture;
            public string FilePath;
        }

        /// <summary>
        /// Captures the screen at end of frame. <paramref name="hideDuringCapture"/> is
        /// toggled off before the grab and back on after, so transient UI (buttons) is
        /// excluded from the saved image.
        /// </summary>
        public void Capture(GameObject hideDuringCapture, Action<Result> onComplete)
        {
            StartCoroutine(CaptureRoutine(hideDuringCapture, onComplete));
        }

        private IEnumerator CaptureRoutine(GameObject hideDuringCapture, Action<Result> onComplete)
        {
            bool wasActive = hideDuringCapture != null && hideDuringCapture.activeSelf;
            if (hideDuringCapture != null) hideDuringCapture.SetActive(false);

            yield return new WaitForEndOfFrame();

            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();

            if (hideDuringCapture != null) hideDuringCapture.SetActive(wasActive);

            string path = SaveToDisk(tex);
            TrySaveToGallery(path);

            if (onComplete != null) onComplete(new Result { Texture = tex, FilePath = path });
        }

        private string SaveToDisk(Texture2D tex)
        {
            try
            {
                byte[] png = tex.EncodeToPNG();
                string fileName = string.Format("FaceFilter_{0:yyyyMMdd_HHmmss}.png", DateTime.Now);
                string path = Path.Combine(Application.persistentDataPath, fileName);
                File.WriteAllBytes(path, png);
                Debug.Log("[ScreenshotCapture] Saved screenshot to " + path);
                return path;
            }
            catch (Exception e)
            {
                Debug.LogError("[ScreenshotCapture] Failed to save screenshot: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// On Android, copy the screenshot into the public gallery (DCIM) and notify the
        /// media scanner so it appears in Photos. No-op on other platforms.
        /// </summary>
        private void TrySaveToGallery(string sourcePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (string.IsNullOrEmpty(sourcePath)) return;
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    string fileName = Path.GetFileName(sourcePath);
                    string dcim = "/storage/emulated/0/DCIM/FaceFilter";
                    Directory.CreateDirectory(dcim);
                    string dest = Path.Combine(dcim, fileName);
                    File.Copy(sourcePath, dest, true);

                    using (var conn = new AndroidJavaClass("android.media.MediaScannerConnection"))
                    {
                        conn.CallStatic("scanFile", activity, new string[] { dest }, new string[] { "image/png" }, null);
                    }
                    Debug.Log("[ScreenshotCapture] Copied to gallery: " + dest);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ScreenshotCapture] Gallery save failed: " + e.Message);
            }
#endif
        }
    }
}
