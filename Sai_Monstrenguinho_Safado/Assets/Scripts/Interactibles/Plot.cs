using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class Plot : Interactible
    {
        [SerializeField] private SpriteRenderer cropSpriteRenderer;
        public CropAttributes currentCrop;
        private Coroutine growCropRoutine;

        private ManagerScripts.AudioManager audioManager;
        private AudioClip wateringAudioClip;
        private AudioClip plantingAudioClip;


        public override void Interact(object sender = null)
        {
            if (sender != null && sender is PlayerScripts.PlayerController player)
            {
                if (player.HeldItem == null) return;

                switch (player.HeldItem.itemTag)
                {
                    case PlayerScripts.ItemTag.Bucket:
                        WaterSeed(player);
                        break;

                    case PlayerScripts.ItemTag.Seed:
                        PlantSeed(player);
                        break;

                    default:
                        HarvestCrop(player);
                        break;
                }
            }
        }

        private void PlantSeed(PlayerScripts.PlayerController player)
        {
            if (currentCrop != null && currentCrop.IsPlanted) return;

            if (player.HeldItem.transform.TryGetComponent<SeedHoldable>(out var heldSeed))
            {
                audioManager.PlayClip(plantingAudioClip);
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
            if (currentCrop == null || growCropRoutine != null || currentCrop.isReady) return;
            audioManager.PlayClip(wateringAudioClip);
            growCropRoutine = StartCoroutine(GrowCrop(player));
        }

        private void HarvestCrop(PlayerScripts.PlayerController player)
        {
            if (currentCrop == null) return;
            if (currentCrop.isReady)
            {
                var cropGO = Instantiate(currentCrop.CropPrefab);
                if (cropGO.TryGetComponent<Holdable>(out var cropHoldable))
                {
                    Debug.Log("Catou a crop");
                    cropHoldable.Interact(player);
                    EmptyPlot();
                }
            }
        }

        public void EmptyPlot()
        {
            cropSpriteRenderer.sprite = null;
            currentCrop = null;
        }

        private System.Collections.IEnumerator GrowCrop(PlayerScripts.PlayerController player)
        {
            //Debug.Log("Agüou a semente.");

            player.SetHeldItem(new());
            currentCrop.isWatered = true;
            cropSpriteRenderer.sprite = currentCrop.cropStage2;

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


        private void Start()
        {
            audioManager = GameManager.Instance.AudioManager;
            plantingAudioClip = audioManager.AudioClips.PickingSeedAudioClip;
            wateringAudioClip = audioManager.AudioClips.WateringAudioClip;
        }
    }
}