using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class SeedSource : Interactible
    {
        [SerializeField] private GameObject seedSeedHoldablePrefab;
        private ManagerScripts.AudioManager audioManager;
        private AudioClip seedAudioClip;

        public override void Interact(object sender = null)
        {
            if (sender != null && sender is PlayerScripts.PlayerController player)
            {
                audioManager.PlayClip(seedAudioClip);
                var seed = Instantiate(seedSeedHoldablePrefab, transform);
                var seedHoldable = seed.GetComponent<SeedHoldable>();

                seedHoldable.Interact(sender);
            }
        }

        private void Start()
        {
            audioManager = GameManager.Instance.AudioManager;
            seedAudioClip = audioManager.AudioClips.PickingSeedAudioClip;
        }
    }
}