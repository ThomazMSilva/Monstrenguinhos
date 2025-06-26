using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class SeedSource : Interactible
    {
        [SerializeField] private GameObject seedSeedHoldablePrefab;

        public override void Interact(object sender = null)
        {
            if (sender != null && sender is PlayerScripts.PlayerController player)
            {
                var seed = Instantiate(seedSeedHoldablePrefab, transform);
                var seedHoldable = seed.GetComponent<SeedHoldable>();

                seedHoldable.Interact(sender);
            }
        }
    }
}