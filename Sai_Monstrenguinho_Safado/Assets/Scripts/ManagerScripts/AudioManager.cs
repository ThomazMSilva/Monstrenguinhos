using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.ManagerScripts
{
    [System.Serializable]
    public class AudioManager
    {
        [SerializeField] private string generalPrefs = "GeneralVolumePref";
        [SerializeField] private string musicPrefs = "MusicVolumePref";
        [SerializeField] private string sfxPrefs = "SFXVolumePref";
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        public AudioSource SfxSource => sfxSource;
        [SerializeField] private Slider generalSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        public float SfxSliderValue => sfxSlider != null ? sfxSlider.value : 1f;

        private MonoBehaviour _coroutineRunner;

        public void Initialize(MonoBehaviour coroutineRunner)
        {
            if(coroutineRunner == null)
            {
                Debug.LogError("Audio Source não teve MonoBehaviour serializado.");
            }

            _coroutineRunner = coroutineRunner;

            if (generalSlider != null)
            {
                if (!PlayerPrefs.HasKey(generalPrefs)) PlayerPrefs.SetFloat(generalPrefs, generalSlider.value);
                generalSlider.value = PlayerPrefs.GetFloat(generalPrefs);
                generalSlider.onValueChanged.AddListener(OnGeneralChanged);
            }

            if (musicSlider != null)
            {
                if (!PlayerPrefs.HasKey(musicPrefs)) PlayerPrefs.SetFloat(musicPrefs, musicSlider.value);
                musicSlider.value = PlayerPrefs.GetFloat(musicPrefs);
                musicSlider.onValueChanged.AddListener(OnMusicChanged);
            }

            if (sfxSlider != null)
            {
                if (!PlayerPrefs.HasKey(sfxPrefs)) PlayerPrefs.SetFloat(sfxPrefs, sfxSlider.value);
                sfxSlider.value = PlayerPrefs.GetFloat(sfxPrefs);
                sfxSlider.onValueChanged.AddListener(OnMusicChanged);
            }

        }

        public void PlayClipAtPoint(AudioClip clip, Transform pointTransform, MonoBehaviour instantiator)
        {
            AudioSource sfx = Object.Instantiate(sfxSource, pointTransform);

            sfx.volume = SfxSliderValue;
            sfx.PlayOneShot(clip);

            Object.Destroy(sfx.gameObject, clip.length);
        }

        private void OnGeneralChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(generalPrefs, value);
        }

        private void OnMusicChanged(float value)
        {
            musicSource.volume = value;
            PlayerPrefs.SetFloat(musicPrefs, value);
        }

        private void OnSfxChanged(float value)
        {
            sfxSource.volume = value;
            PlayerPrefs.SetFloat(sfxPrefs, value);
        }
    }
}