using System;
using System.IO;
using UnityEngine;

namespace FaceFilter.Capture
{
    /// <summary>
    /// Minimal cross-platform "share image" helper. On Android it fires an
    /// ACTION_SEND intent via a FileProvider URI; in the Editor / other platforms it
    /// logs the path so the flow is still testable. iOS sharing requires a native
    /// plugin (see README) and falls back to a log here.
    /// </summary>
    public static class NativeShare
    {
        public static void ShareImage(string filePath, string message = "Made with my AR Face Filter app!")
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Debug.LogWarning("[NativeShare] No file to share at: " + filePath);
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            ShareAndroid(filePath, message);
#else
            Debug.Log("[NativeShare] (Editor/fallback) Would share image: " + filePath + " | " + message);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void ShareAndroid(string filePath, string message)
        {
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    string packageName = activity.Call<string>("getPackageName");
                    using (var fileObj = new AndroidJavaObject("java.io.File", filePath))
                    using (var providerClass = new AndroidJavaClass("androidx.core.content.FileProvider"))
                    {
                        AndroidJavaObject uri;
                        try
                        {
                            uri = providerClass.CallStatic<AndroidJavaObject>(
                                "getUriForFile", activity, packageName + ".fileprovider", fileObj);
                        }
                        catch
                        {
                            // Fallback to Uri.fromFile when no FileProvider is configured.
                            using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                                uri = uriClass.CallStatic<AndroidJavaObject>("fromFile", fileObj);
                        }

                        using (var intent = new AndroidJavaObject("android.content.Intent"))
                        {
                            intent.Call<AndroidJavaObject>("setAction", "android.intent.action.SEND");
                            intent.Call<AndroidJavaObject>("setType", "image/png");
                            intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.STREAM", uri);
                            intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.TEXT", message);
                            intent.Call<AndroidJavaObject>("addFlags", 1); // FLAG_GRANT_READ_URI_PERMISSION

                            using (var chooser = new AndroidJavaClass("android.content.Intent"))
                            using (var picker = chooser.CallStatic<AndroidJavaObject>("createChooser", intent, "Share your AR shot"))
                            {
                                activity.Call("startActivity", picker);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[NativeShare] Android share failed: " + e.Message);
            }
        }
#endif
    }
}
