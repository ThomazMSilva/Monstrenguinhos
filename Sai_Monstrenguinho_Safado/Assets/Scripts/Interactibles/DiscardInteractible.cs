using Assets.Scripts.PlayerScripts;

namespace Assets.Scripts.Interactibles
{
    public class DiscardInteractible : Interactible
    {
        public override void Interact(object sender = null)
        {
            if (sender is PlayerController player && player.HeldItem.itemTag != ItemTag.None)
            {
                if (player.HeldItem.transform.TryGetComponent<Holdable>(out var held))
                    held.ReturnToStartingPoint();
                
                else 
                    player.SetHeldItem(new());
            }
        }
    }
}