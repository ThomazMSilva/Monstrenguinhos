using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class BucketSource : Interactible
    {

        [SerializeField] private GameObject bucketHoldablePrefab;

        private ManagerScripts.AudioManager audioManager;
        private AudioClip bucketAudioClip;

        public override void Interact(object sender = null)
        {
            if (sender != null && sender is PlayerScripts.PlayerController player)
            {
                audioManager.PlayClip(bucketAudioClip);

                var bucket = Instantiate(bucketHoldablePrefab, transform);
                var bucketHoldable = bucket.GetComponent<BucketHoldable>();

                bucketHoldable.Interact(sender);
            }
        }

        public void Start()
        {
            audioManager = GameManager.Instance.AudioManager;
            bucketAudioClip = GameManager.Instance.AudioManager.AudioClips.PickingBucketAudioClip;
        }

    }
}