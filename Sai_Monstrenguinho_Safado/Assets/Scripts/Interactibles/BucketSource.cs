using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class BucketSource : Interactible
    {

        [SerializeField] private GameObject bucketHoldablePrefab;

        private ManagerScripts.AudioManager audioManager;
        private AudioClip bucketAudioClip;

        [SerializeField] private int maximumSpawnable = 1;
        private System.Collections.Generic.Queue<BucketHoldable> spawned = new();

        public override void Interact(object sender = null)
        {
            if (sender != null && sender is PlayerScripts.PlayerController player)
            {
                audioManager.PlayClip(bucketAudioClip);

                var bucket = Instantiate(bucketHoldablePrefab, transform);

                var bucketHoldable = bucket.GetComponent<BucketHoldable>();
                
                spawned.Enqueue(bucketHoldable);

                bucketHoldable.Interact(sender);
                
                if (spawned.Count > maximumSpawnable)
                {
                    var old = spawned.Dequeue();
                    if(old != null)
                        Destroy(old.gameObject);
                }
            }
        }

        public void Start()
        {
            audioManager = GameManager.Instance.AudioManager;
            bucketAudioClip = GameManager.Instance.AudioManager.AudioClips.PickingBucketAudioClip;
        }

    }
}