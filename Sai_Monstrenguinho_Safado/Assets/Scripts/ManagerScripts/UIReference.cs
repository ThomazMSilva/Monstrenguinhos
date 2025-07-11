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
        [SerializeField] private UnityEngine.UI.Slider sfxSlider;
        [SerializeField] private UnityEngine.UI.Slider musicSlider;
        [SerializeField] private UnityEngine.UI.Slider generalSlider;

        private void Awake()
        {
            InitializeAudioSettings();
        }

        private void InitializeAudioSettings()
        {
            var audioManager = GameManager.Instance.AudioManager;
            if (generalSlider != null)
            {
                if (!PlayerPrefs.HasKey(audioManager.GeneralPrefs)) PlayerPrefs.SetFloat(audioManager.GeneralPrefs, generalSlider.value);
                generalSlider.onValueChanged.AddListener(OnGeneralChanged);
                generalSlider.value = PlayerPrefs.GetFloat(audioManager.GeneralPrefs);
            }

            if (musicSlider != null)
            {
                if (!PlayerPrefs.HasKey(audioManager.MusicPrefs)) PlayerPrefs.SetFloat(audioManager.MusicPrefs, musicSlider.value);
                musicSlider.onValueChanged.AddListener(OnMusicChanged);
                musicSlider.value = PlayerPrefs.GetFloat(audioManager.MusicPrefs);
            }

            if (sfxSlider != null)
            {
                if (!PlayerPrefs.HasKey(audioManager.SfxPrefs)) PlayerPrefs.SetFloat(audioManager.SfxPrefs, sfxSlider.value);
                sfxSlider.onValueChanged.AddListener(OnSfxChanged);
                sfxSlider.value = PlayerPrefs.GetFloat(audioManager.SfxPrefs);
            }
        }

        public void SetOptionsScreenActive(bool active) => uiManager.SetPanelActive(optionsScreen, active);

        public void OnSfxChanged(float value) => GameManager.Instance.AudioManager.OnSfxChanged(value);
        public void OnMusicChanged(float value) => GameManager.Instance.AudioManager.OnMusicChanged(value);
        public void OnGeneralChanged(float value) => GameManager.Instance.AudioManager.OnGeneralChanged(value);


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