using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class SeedSource : Interactible
    {
        [SerializeField] private GameObject seedSeedHoldablePrefab;
        private ManagerScripts.AudioManager audioManager;
        private AudioClip seedAudioClip;

        [SerializeField] private bool limitedSpawn;
        [SerializeField] private int maximumSpawnable = 6;
        private System.Collections.Generic.Queue<SeedHoldable> spawned = new();

        public override void Interact(object sender = null)
        {
            if (sender != null && sender is PlayerScripts.PlayerController player)
            {
                audioManager.PlayClip(seedAudioClip);

                var seed = Instantiate(seedSeedHoldablePrefab, transform);

                var seedHoldable = seed.GetComponent<SeedHoldable>();

                seedHoldable.Interact(sender);

                if (!limitedSpawn) return;

                spawned.Enqueue(seedHoldable);

                if (spawned.Count > maximumSpawnable)
                {
                    var old = spawned.Dequeue();
                    if (old != null)
                        Destroy(old.gameObject);
                }
            }
        }

        private void Start()
        {
            audioManager = GameManager.Instance.AudioManager;
            seedAudioClip = audioManager.AudioClips.PickingSeedAudioClip;
        }
    }
}