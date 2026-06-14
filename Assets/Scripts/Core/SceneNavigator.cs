using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FaceFilter.Core
{
    /// <summary>
    /// Loads scenes with a smooth fade so transitions between the menu, AR and demo
    /// experiences feel polished. Survives scene loads as a singleton.
    /// </summary>
    public class SceneNavigator : MonoBehaviour
    {
        public const string MainMenuScene = "MainMenu";
        public const string ARScene = "ARFaceFilter";
        public const string DemoScene = "Demo";

        private static SceneNavigator _instance;
        public static SceneNavigator Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("SceneNavigator");
                    _instance = go.AddComponent<SceneNavigator>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private FadeTransition _fader;
        private bool _busy;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _fader = FadeTransition.Create(transform);
        }

        public void GoTo(string sceneName)
        {
            if (_busy) return;
            StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            _busy = true;
            yield return _fader.FadeOut();

            var op = SceneManager.LoadSceneAsync(sceneName);
            while (op != null && !op.isDone) yield return null;

            yield return null; // let the new scene's bootstrap build its UI
            yield return _fader.FadeIn();
            _busy = false;
        }
    }
}
