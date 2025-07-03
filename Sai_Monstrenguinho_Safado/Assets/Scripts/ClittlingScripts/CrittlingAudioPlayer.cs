using Assets.Scripts.ManagerScripts;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.ClittlingScripts
{
    public class CrittlingAudioPlayer : MonoBehaviour
    {
        private AudioManager audioManager;
        private AudioClip ambientClip;
        private AudioClip ingestingClip;
        private AudioClip digestingClip;
        private AudioClip hitClip;
        private AudioClip fleeingClip;

        private void Start()
        {
            audioManager = GameManager.Instance.AudioManager;
            var clips = audioManager.AudioClips;
            ambientClip = clips.CrittlingAudioClip;
            ingestingClip = clips.CrittlingIngestingAudioClip;
            digestingClip = clips.CrittlingDigestingAudioClip;
            hitClip = clips.CrittlingHitAudioClip;
            fleeingClip = clips.CrittlingFleeingAudioClip;
        }
        public void PlayerIdleSound()
        {
            if (ambientClip == null) return;
            audioManager.PlayClip(ambientClip, transform);
        }

        public void PlayerDigestingSound() => audioManager.PlayClip(digestingClip, transform);
        public void PlayerIngestingSound() => audioManager.PlayClip(ingestingClip, transform);
        public void PlayerHitSound() => audioManager.PlayClip(hitClip, transform);
        public void PlayerFleeingSound() => audioManager.PlayClip(fleeingClip, transform);
    }
}