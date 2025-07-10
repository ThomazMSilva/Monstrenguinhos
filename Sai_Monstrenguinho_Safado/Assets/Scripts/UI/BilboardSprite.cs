using UnityEngine;

namespace Assets.Scripts
{
    public class BilboardSprite : MonoBehaviour
    {
        private Transform cameraTransform;

        private void Awake() => cameraTransform = Camera.main.transform;

        private void LateUpdate() => transform.forward = cameraTransform.forward;
    }
}