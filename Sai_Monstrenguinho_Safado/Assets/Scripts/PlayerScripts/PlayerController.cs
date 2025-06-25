using UnityEngine;

namespace Assets.Scripts.PlayerScripts
{
    public class PlayerController : MonoBehaviour
    {
        #region ATTRIBUTES
        [SerializeField] private Animator playerAnim;

        #region INPUT_ATTRIBUTES
        [Space(8f)]
        [Header("Input")]
        [Space(8f)]

        [SerializeField] private string verticalInput = "Vertical";
        [SerializeField] private string horizontalInput = "Horizontal";
        [SerializeField] private string emoteInput = "Fire1";
        [SerializeField] private string interactionInput = "Fire2";
        [SerializeField] private string sprintInput = "Fire3";
        private bool m_interactionAxisDown;
        private bool isRunning;
        private Vector2 inputAxis;
        #endregion

        #region MOVEMENT_ATTRIBUTES
        [Space(8f)]
        [Header("Variáveis de Movimento")]
        [Space(8f)]

        [SerializeField] private Rigidbody playerRB;
        [SerializeField] private Transform forwardAnchor;
        [SerializeField] private float speed = 2f;
        [SerializeField, Range(1.1f, 3f)] private float sprintMultiplier = 1.5f;
        [SerializeField] private float rotationSpeed = 10;
        private float CurrentSpeedMultiplier => isRunning ? sprintMultiplier : 1;
        
        private Vector3 playerDirection;
        private Vector3 playerVelocity;
        private Vector3 isometricForward, isometricRight;
        #endregion

        #region INTERACTION_ATTRIBUTES
        [Space(8f)]
        [Header("Variaveis de Interacao")]
        [Space(8f)]

        [SerializeField] private Transform interactionOrigin;
        [SerializeField] private float interactionHeight = 1f;
        [SerializeField] private LayerMask layerMask;

        [Space(8f)]
        [SerializeField] private PlayerScripts.HeldItem currentHeldItem = new();
        private bool isInteracting;

        private RaycastHit _hit;

        private Interactibles.Interactible selectedInteractible = null;
        #endregion
        #endregion

        #region UNITY_METHODS
        private void Start()
        {
            currentHeldItem = new();
            CalculateIWorldAxis();
        }

        private void Update() => HandleInput();

        private void FixedUpdate()
        {
            HandleMovement();

            playerAnim.SetFloat("velocityMagnitude", inputAxis.magnitude * CurrentSpeedMultiplier);

            CheckForInteractible();
        }
        #endregion

        #region PRIVATE_METHODS
        private void CalculateIWorldAxis()
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
                    Interact();
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

        private void CheckForInteractible()
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
            if (hit.transform.TryGetComponent<Assets.Scripts.Interactibles.Interactible >(out var interactible))
            {
                if (selectedInteractible == interactible) return;

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
        
        private void Interact()
        {
            playerAnim.SetTrigger("Action");
            selectedInteractible?.Interact(this);
        }
        #endregion

        #region PUBLIC_METHODS
        public void SetInteracting() => isInteracting = true;
        
        public void SetNotInteracting() => isInteracting = false;

        public void SetHeldItem(HeldItem item) => currentHeldItem = item;

        public HeldItem HeldItem => currentHeldItem;

        public Transform InteractionOrigin => interactionOrigin;
        #endregion
    }

    #region ITEM_SCRIPTS
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
            if (sender is Interactibles.Interactible interactible)
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
            if (sender is Interactibles.Interactible interactible)
            {
                interactible.Interact(this);
            }
            else UpdateWaterLevel(-drainAmount);
        }
    }
    #endregion
}