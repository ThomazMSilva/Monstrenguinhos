using Assets.Scripts.NPCScripts;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.Interactibles
{
    [System.Serializable]
    public enum CropType
    {
        None = 0,
        Fruit = 1,
        Flower = 2,
        Vegetable = 3
    }

    [System.Serializable]
    public class CropAttributes
    {
        [SerializeField] private GameObject cropPrefab;
        [SerializeField] private CropSpritesReference spriteReferences;
        
        public GameObject CropPrefab => cropPrefab;

        public Sprite CropStage1
        {
            get
            {
                Debug.Log($"tentando referenciar croptype {cropName.ToString()}");
                var reference = spriteReferences.sprites.FirstOrDefault(reference => reference.cropType == cropName);
                Debug.Assert(reference != null, $"nao achou referencia com o mesmo tipo que croptype");
                string refName = spriteReferences.name;
                string refType = reference.cropType.ToString();
                return reference.stage1sprite;
            }
        }

        public Sprite CropStage2
        {
            get
            {
                return spriteReferences.sprites.FirstOrDefault(reference => reference.cropType == cropName).stage2sprite;
            }
        }

        public Sprite CropStage3
        {
            get
            {
                return spriteReferences.sprites.FirstOrDefault(reference => reference.cropType == cropName).stage3sprite;
            }
        }

        [SerializeField] private CropType cropName;
        public CropType CropName => cropName;

        [SerializeField] private float growthMultiplier = 1f;
        public float GrowthMultiplier => growthMultiplier;

        [SerializeField] private float growthTime = 10f;
        public float GrowthTime => growthTime;

        public float currentTime;
        public bool isWatered;
        public bool isReady;
        public bool IsPlanted => cropName != CropType.None;


        public CropAttributes(CropType name, float maxTime, float multiplier = 1f)
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
            this.spriteReferences = crop.spriteReferences;
            /*var reference = spriteReferences.sprites.FirstOrDefault(reference => reference.cropType == cropName);
            this.CropStage1 = reference.stage1sprite;
            this.CropStage2 = reference.stage2sprite;
            this.CropStage3 = reference.stage3sprite;*/
        }
    }
}