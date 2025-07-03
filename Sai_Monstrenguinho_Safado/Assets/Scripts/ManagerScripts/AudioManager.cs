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
        [SerializeField, Range(0, 1)] private float spatialBlend = .8f;

        [Space(8f)]
        [SerializeField] private AudioReferences _audioReferences;
        public AudioReferences AudioClips => _audioReferences;

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
                AudioListener.volume = generalSlider.value;
                generalSlider.onValueChanged.AddListener(OnGeneralChanged);
            }

            if (musicSlider != null)
            {
                if (!PlayerPrefs.HasKey(musicPrefs)) PlayerPrefs.SetFloat(musicPrefs, musicSlider.value);
                musicSlider.value = PlayerPrefs.GetFloat(musicPrefs);
                musicSource.volume = musicSlider.value;
                musicSlider.onValueChanged.AddListener(OnMusicChanged);
            }

            if (sfxSlider != null)
            {
                if (!PlayerPrefs.HasKey(sfxPrefs)) PlayerPrefs.SetFloat(sfxPrefs, sfxSlider.value);
                sfxSlider.value = PlayerPrefs.GetFloat(sfxPrefs);
                sfxSource.volume = sfxSlider.value;
                sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            }

        }

        public void PlayClip(AudioClip clip, Transform pointTransform = null)
        {
            if(pointTransform == null)
            {
                //sfxSource.clip = clip;
                sfxSource.PlayOneShot(clip);
                //Debug.Log($"Playing {clip.name}");
                return;
            }
            //Debug.Log($"Playing spatial {clip.name}");

            AudioSource sfx = Object.Instantiate(sfxSource, pointTransform);

            sfx.volume = SfxSliderValue;
            sfx.spatialBlend = spatialBlend;
            sfx.spatialize = true;
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

    //[CreateAssetMenu(fileName="AudioReference", menuName="Audio References")]
    [System.Serializable]
    public class AudioReferences// : ScriptableObject
    {

        [SerializeField] private AudioClip failureAudioClip;
        public AudioClip FailureAudioClip => failureAudioClip;


        [SerializeField] private AudioClip successAudioClip;
        public AudioClip SucceessAudioClip => successAudioClip;

        [SerializeField] private AudioClip runningAudioClip;
        public AudioClip RunningAudioClip => runningAudioClip;
        
        [SerializeField] private AudioClip playerWalkingAudioClip;
        public AudioClip CharacterWalkingAudioClip => playerWalkingAudioClip;
        
        [SerializeField] private AudioClip crittlingAudioClip;
        public AudioClip CrittlingAudioClip => crittlingAudioClip;
        
        [SerializeField] private AudioClip crittlingFleeingAudioClip;
        public AudioClip CrittlingFleeingAudioClip => crittlingFleeingAudioClip;
        
        [SerializeField] private AudioClip crittlingHitAudioClip;
        public AudioClip CrittlingHitAudioClip => crittlingHitAudioClip;
        
        [SerializeField] private AudioClip crittlingDigestingAudioClip;
        public AudioClip CrittlingDigestingAudioClip => crittlingDigestingAudioClip;
        
        [SerializeField] private AudioClip crittlingIngestingAudioClip;
        public AudioClip CrittlingIngestingAudioClip => crittlingIngestingAudioClip;
        
        [SerializeField] private AudioClip pickingBucketAudioClip;
        public AudioClip PickingBucketAudioClip => pickingBucketAudioClip;
        
        [SerializeField] private AudioClip wateringAudioClip;
        public AudioClip WateringAudioClip => wateringAudioClip;

        [SerializeField] private AudioClip pickingSeedAudioClip;
        public AudioClip PickingSeedAudioClip => pickingSeedAudioClip;

        [SerializeField] private AudioClip playerHitAudioClip;
        public AudioClip PlayerHitAudioClip => playerHitAudioClip;
    }
}