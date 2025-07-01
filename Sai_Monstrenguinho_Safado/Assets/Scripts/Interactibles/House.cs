namespace Assets.Scripts.Interactibles
{
    public class House : Interactible
    {
        public override void Interact(object sender = null)
        {
            if(sender != null && sender is PlayerScripts.PlayerController player)
            {
                player.TriggerPause();
                Deselect();
            }
        }
    }
}