using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;
using UnityEngine.Device;

namespace Assets.Scripts.ManagerScripts
{
    [System.Serializable]
    public class SceneLoader
    {
        [Space(8f), Header("Tela de Fade"), Space(8f)]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeTime = 0.5f;
        public float FadeTime => fadeTime;

        [Space(8f), Header("Gerenciamento de Cena"), Space(8f)]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private UnityEngine.UI.Image sceneLoadingBar;

        private MonoBehaviour _coroutineRunner;
        private Coroutine loadingSceneRoutine;

        public void Initialize(MonoBehaviour coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        public void StartLoadingScene(string sceneName)
        {
            if (_coroutineRunner == null)
            {
                Debug.LogError("SceneLoader not initialized with coroutine runner!");
                return;
            }

            if (loadingSceneRoutine != null) _coroutineRunner.StopCoroutine(loadingSceneRoutine);

            loadingSceneRoutine ??= _coroutineRunner.StartCoroutine(
                LoadScreen(
                    LoadSceneAsync(sceneName)
                )
            );
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            if (loadingSceneRoutine == null) yield break;

            loadingScreen.SetActive(true);
            Debug.Log("Loading information setado pra true");

            AsyncOperation loadScene = SceneManager.LoadSceneAsync(sceneName);

            while (!loadScene.isDone)
            {
                sceneLoadingBar.fillAmount = loadScene.progress;
                yield return null;
            }
            loadingScreen.SetActive(false);
            Debug.Log("Loading information setado pra false");
            loadingSceneRoutine = null;
        }

        private IEnumerator LoadScreen(object operation)
        {
            if (fadeCanvasGroup == null) yield break;

            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.gameObject.SetActive(true);
            yield return fadeCanvasGroup.DOFade(1, fadeTime).WaitForCompletion();

            if (operation != null)
            {
                if (operation is IEnumerator enumerator)
                {
                    yield return _coroutineRunner.StartCoroutine(enumerator);
                }
                else if (operation is System.Action action)
                {
                    action.Invoke();
                }
            }

            fadeCanvasGroup
                .DOFade(0, fadeTime)
                .OnComplete(() => fadeCanvasGroup.gameObject.SetActive(false));
        }

        

        public IEnumerator FadeScreen(CanvasGroup uiElement, float targetAlpha, float duration, bool active)
        {
            if (fadeCanvasGroup == null) yield break;
            Debug.Log($"Setando {uiElement.name} (estado {uiElement.gameObject.activeSelf}) pra alpha {targetAlpha} e estado ativo {active}");

            uiElement.alpha = active ? 0 : 1;
            Debug.Log("Inicializando pra alpha " + uiElement.alpha);
            uiElement.gameObject.SetActive(true);
            yield return uiElement.DOFade(targetAlpha, duration).WaitForCompletion();
            uiElement.gameObject.SetActive(active);
            Debug.Log($"Setando {uiElement.name} (estado {uiElement.gameObject.activeSelf}) pra alpha {targetAlpha} e estado ativo {active}");
        }
    }
}