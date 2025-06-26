using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class LandPlot : Interactible
    {
        public LandState currentLandState = LandState.Default;
        public Crop currentCrop;
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
                        HarvestCrop();
                        break;

                    case PlayerScripts.ItemTag.Bucket:
                        WaterSeed();
                        break;

                    case PlayerScripts.ItemTag.Box:
                        HarvestCrop();
                        break;

                    case PlayerScripts.ItemTag.Seed:
                        Debug.Log("Tentando plantar 2");
                        PlantSeed(player);
                        break;
                    default: break;
                }
            }
        }

        private void HarvestCrop()
        {
            if (currentCrop == null) return;
            if (currentCrop.isReady)
            {
                Debug.Log("Catou a crop");
                currentCrop = null;
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
            }
            else Debug.Log("nao tinha seedholdable");

        }

        private void WaterSeed()
        {
            if (currentCrop == null) return;
            growCropRoutine ??= StartCoroutine(GrowCrop());
        }

        private System.Collections.IEnumerator GrowCrop()
        {
            Debug.Log("Agüou a semente.");
            currentCrop.isWatered = true;

            while(currentCrop.isWatered && !currentCrop.isReady)
            {
                currentCrop.currentTime += Time.deltaTime * currentCrop.GrowthMultiplier;
                
                if (currentCrop.currentTime >= currentCrop.GrowthTime) 
                    currentCrop.isReady = true;

                if (currentCrop.isReady || !currentCrop.isWatered) break;

                yield return null;
            }
            growCropRoutine = null;
        }
        
    }

    [System.Serializable]
    public class Crop
    {
        [SerializeField] private string cropName = "fruit";
        public string CropName => cropName;

        [SerializeField] private float growthMultiplier = 1f;
        public float GrowthMultiplier => growthMultiplier;

        [SerializeField] private float growthTime = 10f;
        public float GrowthTime => growthTime;

        public  float currentTime;
        public  bool isWatered;
        public bool isReady;
            
        public Crop(string name, float maxTime, float multiplier = 1f)
        {
            cropName = name;
            growthMultiplier = multiplier;
            growthTime = maxTime;
        }

        public Crop(Crop crop)
        {
            cropName = crop.CropName;
            growthMultiplier = crop.GrowthMultiplier;
            growthTime = crop.GrowthTime;
        }
    }

    public enum LandState
    {
        Default,
        Watered
    }
}