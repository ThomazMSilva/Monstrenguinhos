using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class Plot : Interactible
    {
        [SerializeField] private SpriteRenderer cropSpriteRenderer;
        public CropAttributes currentCrop;
        private Coroutine growCropRoutine;

        public override void Interact(object sender = null)
        {
            Debug.Log("Interagiu com terreno");
            if (sender != null && sender is PlayerScripts.PlayerController player)
            {
                if (player.HeldItem == null) return;
                Debug.Log($"objeto na mao: {player.HeldItem.itemTag}");

                switch (player.HeldItem.itemTag)
                {
                    case PlayerScripts.ItemTag.None:
                        HarvestCrop(player);
                        break;

                    case PlayerScripts.ItemTag.Bucket:
                        WaterSeed(player);
                        break;

                    case PlayerScripts.ItemTag.Box:
                        HarvestCrop(player);
                        break;

                    case PlayerScripts.ItemTag.Seed:
                        Debug.Log("Tentando plantar 2");
                        PlantSeed(player);
                        break;
                    default: break;
                }
            }
        }

        private void PlantSeed(PlayerScripts.PlayerController player)
        {
            if (currentCrop != null && !string.IsNullOrEmpty(currentCrop.CropName)) return;

            Debug.Log("Tentando plantar algo");

            if (player.HeldItem.transform.TryGetComponent<SeedHoldable>(out var heldSeed))
            {
                Debug.Log("Plantou " + heldSeed.cropAttributes.CropName);
                currentCrop = new(heldSeed.cropAttributes);
                cropSpriteRenderer.sprite = currentCrop.cropStage1;
                player.SetHeldItem(new());
                //player.SetHeldEmpty();
            }
            else
            {
                Debug.Log("nao tinha seedholdable");
            }
        }

        private void WaterSeed(PlayerScripts.PlayerController player)
        {
            if (currentCrop == null) return;
            growCropRoutine ??= StartCoroutine(GrowCrop(player));
        }

        private void HarvestCrop(PlayerScripts.PlayerController player)
        {
            if (currentCrop == null) return;
            if (currentCrop.isReady)
            {
                cropSpriteRenderer.sprite = null;
                var cropGO = Instantiate(currentCrop.CropPrefab);
                if (cropGO.TryGetComponent<Holdable>(out var cropHoldable))
                {
                    Debug.Log("Catou a crop");
                    cropHoldable.Interact(player);
                    currentCrop = null;
                }
            }
        }

        private System.Collections.IEnumerator GrowCrop(PlayerScripts.PlayerController player)
        {
            Debug.Log("Agüou a semente.");

            player.SetHeldItem(new());
            currentCrop.isWatered = true;
            cropSpriteRenderer.sprite = currentCrop.cropStage2
                ;
            while (currentCrop.isWatered && !currentCrop.isReady)
            {
                currentCrop.currentTime += Time.deltaTime * currentCrop.GrowthMultiplier;
                
                if (currentCrop.currentTime >= currentCrop.GrowthTime)
                {
                    currentCrop.isReady = true;
                    cropSpriteRenderer.sprite = currentCrop.cropStage3;
                } 

                if (currentCrop.isReady || !currentCrop.isWatered) break;

                yield return null;
            }
            growCropRoutine = null;
        }
        
    }
}