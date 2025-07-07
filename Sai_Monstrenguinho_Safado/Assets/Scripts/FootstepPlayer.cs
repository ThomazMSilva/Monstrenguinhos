using Assets.Scripts.ManagerScripts;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class FootstepPlayer : MonoBehaviour
    {
        private AudioManager audioManager;
        [SerializeField] private ParticleSystem runningParticles;
        private AudioClip runningAudioClip;
        private AudioClip walkingAudioClip;
        void Start()
        {

            audioManager = GameManager.Instance.AudioManager;
            walkingAudioClip = audioManager.AudioClips.CharacterWalkingAudioClip;
            runningAudioClip = audioManager.AudioClips.RunningAudioClip;
        }

        public void PlayWalkingEffects()
        {
            audioManager.PlayClip(walkingAudioClip);
        }

        public void PlayRunningEffects()
        {
            audioManager.PlayClip(runningAudioClip);
            //soltar particulas
            runningParticles.Play();
        }
    }
}