using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    [System.Serializable]
    public class CropAttributes
    {
        [SerializeField] private GameObject cropPrefab;
        public GameObject CropPrefab => cropPrefab;

        public Sprite cropStage1;
        public Sprite cropStage2;
        public Sprite cropStage3;

        [SerializeField] private string cropName = "fruit";
        public string CropName => cropName;

        [SerializeField] private float growthMultiplier = 1f;
        public float GrowthMultiplier => growthMultiplier;

        [SerializeField] private float growthTime = 10f;
        public float GrowthTime => growthTime;

        public float currentTime;
        public bool isWatered;
        public bool isReady;

        public CropAttributes(string name, float maxTime, float multiplier = 1f)
        {
            cropName = name;
            growthMultiplier = multiplier;
            growthTime = maxTime;
        }

        public CropAttributes(CropAttributes crop)
        {
            this.cropPrefab = crop.cropPrefab;
            this.cropName = crop.CropName;
            this.growthMultiplier = crop.GrowthMultiplier;
            this.growthTime = crop.GrowthTime;
            this.currentTime = crop.currentTime;
            this.isWatered = crop.isWatered;
            this.isReady = crop.isReady;
            this.cropStage1 = crop.cropStage1;
            this.cropStage2 = crop.cropStage2;
            this.cropStage3 = crop.cropStage3;
        }
    }
}