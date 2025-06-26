using Assets.Scripts.PlayerScripts;
using System.Collections.Generic;

namespace Assets.Scripts.Interactibles
{
    public class BoxHoldable : Holdable
    {
        public List<Crop> storedCrops = new();
        public void AddCrop(Crop crop) => storedCrops.Add(crop);
        public void ResetCrops() => storedCrops.Clear();

        public override void Interact(object sender = null)
        {
            UnityEngine.Debug.Log("BoxHoldable interact call");
            if (sender is PlayerController player)
            {
                switch (player.HeldItem.itemTag)
                {
                    default:
                        PickUpItem(player);
                        break;
                    case ItemTag.Crop:
                        UnityEngine.Debug.Log("crop tag");
                        if (player.HeldItem.transform.TryGetComponent<CropHoldable>(out  var cropHoldable))
                        {
                            UnityEngine.Debug.Log("crop component in hand. Catando e deletando");
                            AddCrop(new(cropHoldable.cropAttributes));
                            player.SetHeldItem(new());
                            Destroy(cropHoldable.transform.gameObject);
                        }
                        break;
                }
            }
        }
    }
}