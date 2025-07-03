using Assets.Scripts.PlayerScripts;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Interactibles
{
    public class BoxHoldable : Holdable
    {
        [UnityEngine.SerializeField] NPCScripts.CropSpritesReference spriteReferences;
        [UnityEngine.SerializeField] List<UnityEngine.UI.Image> imageTransform;
        
        public List<CropAttributes> storedCrops = new();

        public void AddCrop(CropAttributes crop)
        {
            if (storedCrops.Count >= imageTransform.Count) return;

            storedCrops.Add(crop);

            for(int i = 0; i < imageTransform.Count; i++)
            {
                if(i < storedCrops.Count)
                {
                    imageTransform[i].gameObject.SetActive(true);
                    imageTransform[i].sprite = spriteReferences.sprites
                        .Where(s => s.cropType == storedCrops[i].CropName)
                        .FirstOrDefault().sprite ?? null;

                    continue;
                }
                imageTransform[i].sprite = null;
                imageTransform[i].gameObject.SetActive(false);
            }
        }

        public void ClearCrops()
        {
            storedCrops.Clear();
            foreach (var crop in imageTransform)
            {
                crop.sprite = null;
                crop.gameObject.SetActive(false);
            }
        }

        public override void Interact(object sender = null)
        {
            //UnityEngine.Debug.Log("BoxHoldable interact call");
            if (sender is PlayerController player)
            {
                switch (player.HeldItem.itemTag)
                {
                    default:
                        PickUpItem(player);
                        break;
                    case ItemTag.Crop:
                        //UnityEngine.Debug.Log("crop tag");
                        if (player.HeldItem.transform.TryGetComponent<CropHoldable>(out  var cropHoldable))
                        {
                            //UnityEngine.Debug.Log("crop component in hand. Catando e deletando");
                            AddCrop(new(cropHoldable.cropAttributes));
                            player.SetHeldItem(new());
                            Destroy(cropHoldable.transform.gameObject);
                        }
                        break;
                }
            }
        }

        public override void ReturnToStartingPoint()
        {
            base.ReturnToStartingPoint();
            ClearCrops();
        }
    }
}