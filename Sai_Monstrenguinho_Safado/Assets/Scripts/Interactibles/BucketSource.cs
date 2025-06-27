using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class BucketSource : Interactible
    {

        [SerializeField] private GameObject bucketHoldablePrefab;

        public override void Interact(object sender = null)
        {
            if (sender != null && sender is PlayerScripts.PlayerController player)
            {
                var bucket = Instantiate(bucketHoldablePrefab, transform);
                var bucketHoldable = bucket.GetComponent<BucketHoldable>();

                bucketHoldable.Interact(sender);
            }
        }

       /* private void HarvestCrop(PlayerScripts.PlayerController player)
        {
            if (currentCrop.isReady)
            {
                var cropGO = Instantiate(bucketHoldablePrefab);
                if (cropGO.TryGetComponent<Holdable>(out var bucketHoldable))
                {
                    Debug.Log("Catou a crop");
                    bucketHoldable.Interact(player);
                    bucketHoldable = null;

                }
            }
        }*/
    }
}