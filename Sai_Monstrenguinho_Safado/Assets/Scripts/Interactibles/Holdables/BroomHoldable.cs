using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class BroomHoldable : Holdable
    {
        [SerializeField] private bool canDrop;
        private ManagerScripts.AudioManager audioManager;
        private AudioClip hitClip;

        public override void DropItem()
        {
            audioManager.PlayClip(hitClip);
            if (!canDrop) return;
            base.DropItem();
        }

        public override void Start()
        {
            base.Start();
            audioManager = GameManager.Instance.AudioManager;
            hitClip = audioManager.AudioClips.PlayerHitAudioClip;
        }
    }
}