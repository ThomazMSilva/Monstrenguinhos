using Assets.Scripts.PlayerScripts;

namespace Assets.Scripts.Interactibles
{
    public class CropHoldable : Holdable
    {
        public CropHoldable() => cropAttributes = default;
        
        public CropHoldable(CropAttributes crop) => cropAttributes = crop;

        public CropAttributes cropAttributes;

        public override void Interact(object sender = null)
        {
            if (sender != null && sender is PlayerController player)
            {
                //UnityEngine.Debug.Log("CropHoldable interact call");
                switch (player.HeldItem.itemTag)
                {
                    case ItemTag.Box:
                        //UnityEngine.Debug.Log("box tag");
                        if (player.TryGetFromHeld<BoxHoldable>(out var box))
                        {
                            //UnityEngine.Debug.Log("box component");
                            box.AddCrop(new(cropAttributes));
                            //player.SetHeldItem(new());
                            Destroy(gameObject, 1);
                        }
                        break;
                    default: 
                        PickUpItem(player); 
                        break;
                }
            }
        }
    }
}