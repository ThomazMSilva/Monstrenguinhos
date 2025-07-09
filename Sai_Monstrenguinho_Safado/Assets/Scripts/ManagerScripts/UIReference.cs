using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.ManagerScripts
{
    public class UIReference : MonoBehaviour
    {
        [SerializeField] private UINavigationManager uiManager;
        public void LoadScene(string scene)
        {
            GameManager.Instance.LoadScene(scene);
            SetMenuScreenActive(false);
        }

        [SerializeField] private GameObject optionsScreen;

        public void SetOptionsScreenActive(bool active) => uiManager.SetPanelActive(optionsScreen, active);

        [SerializeField] private GameObject creditsScreen;

        public void SetCreditsScreenActive(bool active) => uiManager.SetPanelActive(creditsScreen, active);

        [SerializeField] private GameObject controlsScreen;
        public void SetControlsScreenActive(bool active) => uiManager.SetPanelActive(controlsScreen, active);

        [SerializeField] private GameObject menuScreen;
        [SerializeField] private float draggingMenuOffset = 250f;
        [SerializeField] private float draggingMenuDuration = 1f;
        [SerializeField] private Ease draggingMenuEase = Ease.OutBounce;
        private Tween draggingMenuTween;

        public void SetMenuScreenActive(bool active) => uiManager.SetPanelActive(menuScreen, active);

        public void DragMenuScreen()
        {
            Vector3 originalPos = menuScreen.transform.position;
            Vector3 menuOffsetPosition = new(originalPos.x, originalPos.y - draggingMenuOffset, originalPos.z);

            menuScreen.transform.position = menuOffsetPosition;

            SetMenuScreenActive(true);

            draggingMenuTween = menuScreen.transform
                .DOMove(originalPos, draggingMenuDuration, true)
                .SetEase(draggingMenuEase);
            
        }

        [SerializeField] private GameObject tutorialScreen;
        public void SetTutorialScreenActive(bool active) => uiManager.SetPanelActive(tutorialScreen, active);

        public void QuitGame() => GameManager.Instance?.QuitGame();

        public void FadeScreen(CanvasGroup screen) => GameManager.Instance.Fade(screen);

        public void RestartGame() => GameManager.Instance.Restart();

        public void SetPause(bool pause) => GameManager.Instance.SetPause(pause);
    }
}