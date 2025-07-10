using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class Plot : Interactible
    {
        [SerializeField] private SpriteRenderer cropSpriteRenderer;
        [SerializeField] private Material fadeMaterial;
        [SerializeField] private bool broomInteraction;
        private Material fadeMaterialCopy;
        private Material originalMaterial;
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

                    case PlayerScripts.ItemTag.Crop: 
                        break;

                    case PlayerScripts.ItemTag.Broom:
                        if (!broomInteraction) 
                            break;
                        else HarvestCrop(player); 
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
                if (triggersAnimation) player.TriggerAnimation();
                audioManager.PlayClip(plantingAudioClip);
                currentCrop = new(heldSeed.cropAttributes);
                cropSpriteRenderer.sprite = currentCrop.CropStage1;
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
            if (currentCrop == null || currentCrop.CropName == CropType.None || growCropRoutine != null || currentCrop.isReady) return;
            audioManager.PlayClip(wateringAudioClip);
            if (triggersAnimation) player.TriggerAnimation();
            growCropRoutine = StartCoroutine(GrowCrop(player));
        }

        private void HarvestCrop(PlayerScripts.PlayerController player)
        {
            if (currentCrop == null) return;
            if (currentCrop.isReady)
            {
                if (triggersAnimation) player.TriggerAnimation();
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
            StopShining();
        }

        private System.Collections.IEnumerator GrowCrop(PlayerScripts.PlayerController player)
        {
            //Debug.Log("Agüou a semente.");

            player.SetHeldItem(new());
            currentCrop.isWatered = true;
            cropSpriteRenderer.sprite = currentCrop.CropStage2;

            while (currentCrop.isWatered && !currentCrop.isReady)
            {
                currentCrop.currentTime += Time.deltaTime * currentCrop.GrowthMultiplier;
                
                if (currentCrop.currentTime >= currentCrop.GrowthTime)
                {
                    currentCrop.isReady = true;
                    cropSpriteRenderer.sprite = currentCrop.CropStage3;
                    StartShining();
                } 

                if (currentCrop.isReady || !currentCrop.isWatered) break;

                yield return null;
            }
            growCropRoutine = null;
        }

        private Tween fadeTween;
        private Color faceColor;
        [SerializeField] private float fadeDuration = .7f;
        [SerializeField, Range(0, 1)] private float fadeStrength = .7f;
        [SerializeField] private Ease fadeEase = Ease.InOutSine;

        private void StartShining()
        {
            fadeTween = DOTween.To(
                () => fadeMaterialCopy.GetColor("_FaceColor"),
                x => fadeMaterialCopy.SetColor("_FaceColor", x),
                new Color(faceColor.r, faceColor.g, faceColor.b, fadeStrength),
                fadeDuration
            )
            .SetEase(fadeEase)
            .SetLoops(-1, LoopType.Yoyo);

        }

        private void StopShining()
        {
            fadeTween?.Kill();
            faceColor.a = 0f;
            fadeMaterialCopy.SetColor("_FaceColor", faceColor);
            fadeTween = null;
        }

        private void Start()
        {
            audioManager = GameManager.Instance.AudioManager;
            plantingAudioClip = audioManager.AudioClips.PickingSeedAudioClip;
            wateringAudioClip = audioManager.AudioClips.WateringAudioClip;

            originalMaterial = cropSpriteRenderer.material;
            fadeMaterialCopy = new(fadeMaterial);
            faceColor = fadeMaterialCopy.GetColor("_FaceColor");
            faceColor.a = 0;
            fadeMaterialCopy.SetColor("_FaceColor", faceColor);
            cropSpriteRenderer.materials = new Material[] { originalMaterial, fadeMaterialCopy};

            
        }
    }
}