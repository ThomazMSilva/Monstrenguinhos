using Assets.Scripts.PlayerScripts;
using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class DiscardInteractible : Interactible
    {
        [SerializeField] private Transform lidTransform;
        [SerializeField] private float animationDuration = .6f;
        [SerializeField] private Ease easeType = Ease.OutBounce;
        private Tween trashTween;
        private Vector3 closedLidRotation;
        private Vector3 openLidRotation;

        public void Start()
        {
            closedLidRotation = lidTransform.rotation.eulerAngles;
            openLidRotation = new
            (
                lidTransform.rotation.x, 
                lidTransform.rotation.y, 
                lidTransform.rotation.z + 60
            );
        }

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

        public override void Select(object sender = null)
        {
            base.Select(sender);
            trashTween?.Kill();
            trashTween = lidTransform
                .DORotate(openLidRotation, animationDuration, RotateMode.FastBeyond360)
                .SetEase(easeType);
        }

        public override void Deselect(object sender = null)
        {
            base.Deselect(sender);
            trashTween?.Kill();
            trashTween = lidTransform
                .DORotate(closedLidRotation, animationDuration, RotateMode.FastBeyond360)
                .SetEase(easeType);
        }
    }
}