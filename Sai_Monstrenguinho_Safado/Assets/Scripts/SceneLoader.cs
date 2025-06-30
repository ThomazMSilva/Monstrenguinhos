using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    //criar singleton dinamico
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    [Space(8f), Header("Tela de Fade"), Space(8f)]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeTime = 0.5f;

    [Space(8f), Header("Gerenciamento de Cena"), Space(8f)]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] UnityEngine.UI.Image sceneLoadingBar;
    private Coroutine loadingSceneRoutine;

    public void StartLoadingScene(string sceneName)
    {
        loadingSceneRoutine ??= StartCoroutine
        (
            LoadScreen
            (
                LoadSceneAsync(sceneName)
            )
        );
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingSceneRoutine == null) yield break;

        loadingScreen.SetActive(true);

        AsyncOperation loadScene = SceneManager.LoadSceneAsync(sceneName);
        
        while (!loadScene.isDone)
        {
            sceneLoadingBar.fillAmount = loadScene.progress;
            yield return null;
        }

        loadingScreen.SetActive(false);
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
                yield return StartCoroutine(enumerator);
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
}
