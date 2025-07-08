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
        public void SetMenuScreenActive(bool active) => uiManager.SetPanelActive(menuScreen, active);

        [SerializeField] private GameObject tutorialScreen;
        public void SetTutorialScreenActive(bool active) => uiManager.SetPanelActive(tutorialScreen, active);

        public void QuitGame() => GameManager.Instance?.QuitGame();

        public void FadeScreen(CanvasGroup screen) => GameManager.Instance.Fade(screen);

        public void RestartGame() => GameManager.Instance.Restart();

        public void SetPause(bool pause) => GameManager.Instance.SetPause(pause);
    }
}