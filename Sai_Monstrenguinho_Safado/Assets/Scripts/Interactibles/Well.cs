using Assets.Scripts.PlayerScripts;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class Well : Holdable
    {
        [SerializeField] private float maximumWaterLevel;
        [SerializeField] private float drainAmount;
        private float currentWaterLevel;
        public float CurrentWaterLevel => currentWaterLevel;

        public override void Interact(object sender = null)
        {
            base.Interact(sender);
        }

        public void UpdateMaximumWaterLevel(float newMaximum)
        {
            maximumWaterLevel = newMaximum;
            UpdateWaterLevel();
        }

        public void UpdateWaterLevel(float fillAmount = 0)
        {
            currentWaterLevel = Mathf.Clamp(currentWaterLevel + fillAmount, 0, maximumWaterLevel);
        }

        public void Use(object sender)
        {
            if (sender is Interactibles.Interactible interactible)
            {
                interactible.Interact(this);
            }
            else UpdateWaterLevel(-drainAmount);
        }
        
    }
}