using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FaceFilter.Capture
{
    public class ScreenshotCapture : MonoBehaviour
    {
        public struct Result
        {
            public Texture2D Texture;
            public string FilePath;
        }

        public void Capture(GameObject hideDuringCapture, Action<Result> onComplete)
        {
            StartCoroutine(CaptureRoutine(hideDuringCapture, onComplete));
        }

        private IEnumerator CaptureRoutine(GameObject hideDuringCapture, Action<Result> onComplete)
        {
            bool wasActive = hideDuringCapture != null && hideDuringCapture.activeSelf;

            if (hideDuringCapture != null)
                hideDuringCapture.SetActive(false);

            yield return new WaitForEndOfFrame();

            Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();

            if (hideDuringCapture != null)
                hideDuringCapture.SetActive(wasActive);

            string path = SaveToDisk(tex);
            TrySaveToGallery(path);

            onComplete?.Invoke(new Result
            {
                Texture = tex,
                FilePath = path
            });
        }

        private string SaveToDisk(Texture2D tex)
        {
            try
            {
                byte[] png = tex.EncodeToPNG();
                string fileName = $"FaceFilter_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string path = Path.Combine(Application.persistentDataPath, fileName);

                File.WriteAllBytes(path, png);

                Debug.Log("Screenshot saved to: " + path);
                return path;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save screenshot: " + e.Message);
                return null;
            }
        }

        private void TrySaveToGallery(string sourcePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (string.IsNullOrEmpty(sourcePath))
                return;

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
                        conn.CallStatic(
                            "scanFile",
                            activity,
                            new string[] { dest },
                            new string[] { "image/png" },
                            null
                        );
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Gallery save failed: " + e.Message);
            }
#endif
        }
    }
}