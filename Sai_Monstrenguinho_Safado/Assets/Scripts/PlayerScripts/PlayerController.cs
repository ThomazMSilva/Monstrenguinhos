using Assets.Scripts.GridScripts;
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
        [SerializeField] private Transform interactionShadow;
        [SerializeField] private float interactionHeight = 1f;
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private LayerMask interactibleLayerMask;

        [Space(8f)]
        [SerializeField] private HeldItem currentHeldTag;
        private Interactibles.Holdable currentHeldInteractible;
        private bool isInteracting;

        private RaycastHit interactibleHit;

        private Interactibles.Interactible selectedInteractible = null;
        #endregion
        #endregion

        #region UNITY_METHODS
        private void Start()
        {
            currentHeldTag = new();
            CalculateIWorldAxis();
        }

        private void Update() => HandleInput();

        private void FixedUpdate()
        {
            HandleMovement();

            playerAnim.SetFloat("velocityMagnitude", inputAxis.magnitude * CurrentSpeedMultiplier);

            CheckForInteractible();

            if(currentHeldInteractible != null)
            {
                DisplayShadow();
            }
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

        private void DisplayShadow()
        {
            if (Physics.Raycast(interactionOrigin.position, Vector3.down, out var groundHit, interactionHeight, groundLayerMask))
            {
                interactionShadow.position = BuildSystem.instance.SnappedPosition(groundHit.point);
                interactionShadow.gameObject.SetActive(true);
            }
            else interactionShadow.gameObject.SetActive(false);
        }

        private void CheckForInteractible()
        {
            if(Physics.Raycast(interactionOrigin.position, Vector3.down, out interactibleHit, interactionHeight, interactibleLayerMask))
            {

                TrySelectInteractible(interactibleHit);
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
            if (currentHeldInteractible == null && selectedInteractible == null) return;
            
            playerAnim.SetTrigger("Action");
            currentHeldInteractible?.DropItem();
            selectedInteractible?.Interact(this);
        }
        #endregion

        #region PUBLIC_METHODS
        public void SetInteracting() => isInteracting = true;
        
        public void SetNotInteracting() => isInteracting = false;

        public void SetHeldItem(HeldItem incomingTag)
        {
            //Isso aqui tá bizarro e triste, mudar depois
            bool hasTransform = currentHeldTag.transform != null;
            if (currentHeldTag.itemTag != ItemTag.None && hasTransform)
            {
                if (currentHeldTag.transform.TryGetComponent<Interactibles.Holdable>(out var held))
                {
                    held.ReturnToStartingPoint();
                    SetHeldEmpty();
                }
            }
            
            if (incomingTag.transform != null
                && incomingTag.transform.TryGetComponent<Interactibles.Holdable>(out var newHeld))
            {
                currentHeldInteractible = newHeld;
            }
            else
            {
                interactionShadow.gameObject.SetActive(false);
                currentHeldInteractible = null;
            }
            currentHeldTag = incomingTag;
        }

        public void SetHeldEmpty()
        {
            currentHeldTag = new();
            currentHeldInteractible = null;
            interactionShadow.gameObject.SetActive(false);
        }

        public HeldItem HeldItem => currentHeldTag;

        public Transform InteractionOrigin => interactionOrigin;

        public bool TryGetFromHeld<T>(out T component)
        {
            if (currentHeldTag.transform.TryGetComponent(out T t))
            {
                component = t;
                return true;
            }
            component = default;
            return false;
        }
        #endregion
    }

}