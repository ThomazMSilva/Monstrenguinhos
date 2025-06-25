using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public enum LandState
    {
        Default,
        Rowed,
        Watered
    }

    public class LandPlot : Interactible
    {
        public LandState currentLandState = LandState.Default;

        public void RowLand()
        {
            if (currentLandState == LandState.Default)
            {
                currentLandState = LandState.Rowed;
            }
        }

        public void WaterLand()
        {
            if(currentLandState == LandState.Rowed)
            {
                currentLandState = LandState.Watered;
            }
        }

        public override void Interact(object sender = null)
        {
            Debug.Log("Interagiu com terreno");
            if (sender != null && sender is PlayerScripts.Item item)
            {

            }

        }
    }
}