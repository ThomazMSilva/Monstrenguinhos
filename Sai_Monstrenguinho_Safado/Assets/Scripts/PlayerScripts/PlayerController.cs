using Assets.Scripts.Interactibles;
using UnityEngine;

namespace Assets.Scripts.PlayerScripts
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Animator playerAnim;

        [Space(8f)]
        [Header("Variáveis de Movimento")]
        [Space(8f)]
        [SerializeField] private Rigidbody playerRB;
        [SerializeField] private Transform isometricAnchor;
        [SerializeField] private float speed = 2f;
        [SerializeField, Range(1.1f, 3f)] private float sprintMultiplier = 1.5f;
        [SerializeField] private float rotationSpeed = 10;

        [Space(8f)]
        [Header("Variaveis de Interacao")]
        [Space(8f)]
        [SerializeField] Transform interactionAnchor;
        [SerializeField] private float interactionHeight = 1f;
        [SerializeField] private LayerMask layerMask;

        private bool isRunning;
        private bool m_interactionAxisDown;

        private float CurrentSpeedMultiplier => isRunning ? sprintMultiplier : 1;

        private Vector2 inputAxis;
        private Vector3 playerDirection;
        private Vector3 playerVelocity;
        private Vector3 isometricForward, isometricRight;

        private RaycastHit _hit;

        private IInteractible selectedInteractible = null;

        private void Start() => CalculateIsometricAngle();

        private void Update() => HandleInput();

        private void FixedUpdate()
        {
            HandleMovement();

            playerAnim.SetFloat("velocityMagnitude", inputAxis.magnitude * CurrentSpeedMultiplier);

            CheckForInteraction();
        }

        private void CalculateIsometricAngle()
        {
            isometricForward = isometricAnchor.forward;
            isometricForward.Normalize();

            isometricRight = isometricAnchor.right;
            isometricRight.Normalize();
        }

        private void HandleInput()
        {
            inputAxis.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            isRunning = Input.GetAxis("Fire3") != 0;

            if(Input.GetAxisRaw("Fire2") != 0)
            {
                if (!m_interactionAxisDown)
                {
                    m_interactionAxisDown = true;
                    selectedInteractible?.Interact();
                }
            }
            if(Input.GetAxisRaw("Fire2") == 0)
            {
                m_interactionAxisDown = false;
            }
        }

        private void HandleMovement()
        {
            playerDirection = Vector3.ClampMagnitude(isometricRight * inputAxis.x + isometricForward * inputAxis.y, 1);
            playerVelocity = speed * CurrentSpeedMultiplier * playerDirection;

            if (playerDirection.sqrMagnitude <= 0) return;

            Quaternion targetRotation = Quaternion.LookRotation(playerVelocity.normalized);

            playerRB.Move
            (
                transform.position + (playerVelocity * Time.fixedDeltaTime),
                Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed)
            );
        }

        private void CheckForInteraction()
        {
            if(Physics.Raycast(interactionAnchor.position, Vector3.down, out _hit, interactionHeight, layerMask))
            {
                TrySelectInteractible(_hit);
            }
            else
            {
                DeselectCurrentInteractible();
            }

            Debug.DrawLine(interactionAnchor.position, interactionAnchor.position + (Vector3.down * interactionHeight), Color.magenta);
        }

        private void TrySelectInteractible(RaycastHit hit)
        {
            if (hit.transform.TryGetComponent<IInteractible>(out var interactible))
            {
                if (selectedInteractible != null && selectedInteractible != interactible)
                {
                    selectedInteractible.Deselect();
                }

                selectedInteractible = interactible;
                selectedInteractible.Select();
            }
            else
            {
                DeselectCurrentInteractible();
            }
        }

        private void DeselectCurrentInteractible()
        {
            selectedInteractible?.Deselect();
            selectedInteractible = null;
        }

    }

}