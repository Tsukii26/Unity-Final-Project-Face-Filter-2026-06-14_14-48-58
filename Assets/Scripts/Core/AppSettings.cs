using UnityEngine;

namespace FaceFilter.Core
{
    /// <summary>
    /// Lightweight persistent settings (backed by PlayerPrefs) shared across scenes:
    /// render quality, mirror preview, and last-used filter. Keeps the settings panel
    /// and AR scene decoupled.
    /// </summary>
    public static class AppSettings
    {
        private const string KeyQuality = "ff_quality";
        private const string KeyMirror = "ff_mirror";
        private const string KeyFilterIndex = "ff_filter";

        /// <summary>0 = Performance, 1 = Balanced, 2 = Quality.</summary>
        public static int QualityIndex
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(KeyQuality, 1), 0, 2); }
            set
            {
                PlayerPrefs.SetInt(KeyQuality, Mathf.Clamp(value, 0, 2));
                PlayerPrefs.Save();
                ApplyQuality();
            }
        }

        public static bool MirrorPreview
        {
            get { return PlayerPrefs.GetInt(KeyMirror, 1) == 1; }
            set { PlayerPrefs.SetInt(KeyMirror, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static int LastFilterIndex
        {
            get { return PlayerPrefs.GetInt(KeyFilterIndex, 0); }
            set { PlayerPrefs.SetInt(KeyFilterIndex, value); PlayerPrefs.Save(); }
        }

        public static readonly string[] QualityNames = { "Performance", "Balanced", "Quality" };

        public static void ApplyQuality()
        {
            switch (QualityIndex)
            {
                case 0:
                    Application.targetFrameRate = 60;
                    QualitySettings.SetQualityLevel(0, true);
                    QualitySettings.shadows = ShadowQuality.Disable;
                    break;
                case 2:
                    Application.targetFrameRate = 60;
                    QualitySettings.SetQualityLevel(Mathf.Max(0, QualitySettings.names.Length - 1), true);
                    QualitySettings.shadows = ShadowQuality.All;
                    break;
                default:
                    Application.targetFrameRate = 60;
                    QualitySettings.SetQualityLevel(Mathf.Clamp(1, 0, QualitySettings.names.Length - 1), true);
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    break;
            }
        }

        public static void ResetAll()
        {
            PlayerPrefs.DeleteKey(KeyQuality);
            PlayerPrefs.DeleteKey(KeyMirror);
            PlayerPrefs.DeleteKey(KeyFilterIndex);
            PlayerPrefs.Save();
            ApplyQuality();
        }
    }
}
