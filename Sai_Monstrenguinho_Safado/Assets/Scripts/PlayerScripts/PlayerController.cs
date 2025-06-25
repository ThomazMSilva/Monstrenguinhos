using UnityEngine;

namespace Assets.Scripts.PlayerScripts
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Animator playerAnim;

        [Space(8f)]
        [Header("Input")]
        [Space(8f)]

        [SerializeField] private string verticalInput = "Vertical";
        [SerializeField] private string horizontalInput = "Horizontal";
        [SerializeField] private string emoteInput = "Fire1";
        [SerializeField] private string interactionInput = "Fire2";
        [SerializeField] private string sprintInput = "Fire3";


        [Space(8f)]
        [Header("Variáveis de Movimento")]
        [Space(8f)]
        [SerializeField] private Rigidbody playerRB;
        [SerializeField] private Transform forwardAnchor;
        [SerializeField] private float speed = 2f;
        [SerializeField, Range(1.1f, 3f)] private float sprintMultiplier = 1.5f;
        [SerializeField] private float rotationSpeed = 10;

        [Space(8f)]
        [Header("Variaveis de Interacao")]
        [Space(8f)]
        [SerializeField] Transform interactionOrigin;
        [SerializeField] private float interactionHeight = 1f;
        [SerializeField] private LayerMask layerMask;

        private bool isInteracting;
        private bool isRunning;
        private bool m_interactionAxisDown;

        private float CurrentSpeedMultiplier => isRunning ? sprintMultiplier : 1;

        private Vector2 inputAxis;
        private Vector3 playerDirection;
        private Vector3 playerVelocity;
        private Vector3 isometricForward, isometricRight;

        private RaycastHit _hit;

        private Assets.Scripts.Interactibles.IInteractible selectedInteractible = null;

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
            isometricForward = forwardAnchor.forward;
            isometricForward.Normalize();

            isometricRight = forwardAnchor.right;
            isometricRight.Normalize();
        }

        private void HandleInput()
        {
            inputAxis.Set(Input.GetAxis(horizontalInput), Input.GetAxis(verticalInput));
            isRunning = Input.GetAxis(sprintInput) != 0;

            if(Input.GetAxisRaw(interactionInput) != 0)
            {
                if (!m_interactionAxisDown)
                {
                    m_interactionAxisDown = true;
                    playerAnim.SetTrigger("Action");
                    selectedInteractible?.Interact();
                }
            }
            if(Input.GetAxisRaw(interactionInput) == 0)
            {
                m_interactionAxisDown = false;
            }

            playerAnim.SetBool("Emoting_0", Input.GetAxisRaw(emoteInput) != 0);
        }
        
        private void HandleMovement()
        {
            if (isInteracting) return;

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
            if(Physics.Raycast(interactionOrigin.position, Vector3.down, out _hit, interactionHeight, layerMask))
            {
                TrySelectInteractible(_hit);
            }
            else
            {
                DeselectCurrentInteractible();
            }

            //Debug.DrawLine(interactionOrigin.position, interactionOrigin.position + (Vector3.down * interactionHeight), Color.magenta);
        }

        private void TrySelectInteractible(RaycastHit hit)
        {
            if (hit.transform.TryGetComponent<Assets.Scripts.Interactibles.IInteractible >(out var interactible))
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

        public void SetInteracting() => isInteracting = true;
        
        public void SetNotInteracting() => isInteracting = false;
    }

    [System.Serializable]
    public class HeldItem
    {

    }

    [System.Serializable]
    public abstract class Item
    {
        void Use(object sender) { Debug.Log("Usando item com metodo nao atribuido"); }
    }

    [System.Serializable]
    public class Hoe : Item
    {
        public void Use(object sender)
        {
            if (sender is Interactibles.IInteractible interactible)
            {
                interactible.Interact(this);
            }
        }
    }

    [System.Serializable]
    public class WateringCan : Item
    {
        [SerializeField] private float maximumWaterLevel;
        [SerializeField] private float drainAmount;
        private float currentWaterLevel;
        public float CurrentWaterLevel => currentWaterLevel;

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
            if (sender is Interactibles.IInteractible interactible)
            {
                interactible.Interact(this);
            }
            else UpdateWaterLevel(-drainAmount);
        }
    }
}