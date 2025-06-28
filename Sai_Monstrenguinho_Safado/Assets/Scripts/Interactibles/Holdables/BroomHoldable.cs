using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class BroomHoldable : Holdable
    {
        [SerializeField] private bool canDrop;

        public override void DropItem()
        {
            if (!canDrop) return;
            base.DropItem();
        }
    }
}