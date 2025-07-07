using Assets.Scripts.GridScripts;
using DG.Tweening;
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
        [SerializeField] private string pauseInput = "Pause";
        private bool m_interactionAxisDown;
        private bool m_pauseAxisDown;
        private bool isRunning;
        private Vector2 inputAxis;
        #endregion

        #region MOVEMENT_ATTRIBUTES
        [Space(8f)]
        [Header("Variáveis de Movimento")]
        [Space(8f)]

        [SerializeField] private Rigidbody playerRB;
        [SerializeField] private UnityEngine.AI.NavMeshAgent playerNavigationAgent;
        [SerializeField] private Transform forwardAnchor;
        [SerializeField] private float speed = 4f;
        [SerializeField, Range(1.1f, 3f)] private float sprintMultiplier = 1.5f;
        [SerializeField] private float rotationSpeed = 10;

        [Space(8f)]

        [SerializeField] private bool stopWhenInteracting;
        [SerializeField] private bool hasInteractionCooldown;

        [Space(8f)]

        [SerializeField] private Transform pauseLocation;
        [SerializeField] private float aiSpeed = 2f;
        private float CurrentSpeedMultiplier => isRunning ? sprintMultiplier : 1;

        private bool IsAutomatic => playerNavigationAgent.enabled;
        private Coroutine automaticMovementRoutine;
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
        [SerializeField] private bool isGridSelectionActive;
        [SerializeField] private Vector3 gridSelectionSize = new(.8f, .8f, .8f);
        [SerializeField] private float gridSelectioVerticalOffset = .01f;

        [Space(8f)]
        [SerializeField] private HeldItem currentHeldTag;
        private Interactibles.Holdable currentHeldInteractible;
        private bool isInteracting;

        private Collider[] gridColliders = new Collider[8];
        private Vector3 gridPosition = Vector3.zero;

        private RaycastHit interactibleHit;

        private Interactibles.Interactible selectedInteractible = null;
        #endregion


        public UnityEngine.Events.UnityEvent OnPaused;
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
            if (GameManager.Instance.IsPaused) return;

            if (IsAutomatic) return;

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

                    bool canInteract = !(hasInteractionCooldown && isInteracting);

                    if(!GameManager.Instance.IsPaused && canInteract)
                        Interact();
                }
            }
            if(Input.GetAxisRaw(interactionInput) == 0)
            {
                m_interactionAxisDown = false;
            }

            if (Input.GetAxisRaw(pauseInput) != 0)
            {
                if (!m_pauseAxisDown)
                {
                    m_pauseAxisDown = true;
                    TriggerPause();
                }
            }
            if (Input.GetAxisRaw(pauseInput) == 0)
            {
                m_pauseAxisDown = false;
            }

            playerAnim.SetBool("Emoting_0", Input.GetAxisRaw(emoteInput) != 0);
        }

        private void HandleMovement()
        {
            if (stopWhenInteracting && isInteracting) return;

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
            Ray ray = new(interactionOrigin.position, Vector3.down);
            if (isGridSelectionActive)
            {
                if (Physics.Raycast(ray, out var gridHit, interactionHeight))
                {
                    gridPosition = BuildSystem.instance.SnappedPosition(gridHit.point);
                    int overlappedColliders = Physics.OverlapBoxNonAlloc
                    (
                        gridPosition + Vector3.up * ((gridSelectionSize.y * .5f) - gridSelectioVerticalOffset),
                        gridSelectionSize * .5F,
                        gridColliders,
                        Quaternion.identity,
                        interactibleLayerMask
                    );

                    if (overlappedColliders > 0)
                    {
                        var highestPriority = gridColliders[0].transform;
                        for(int i = 0; i < overlappedColliders; i++)
                        {
                            var overlappedTransform = gridColliders[i].transform;
                            if (overlappedTransform.position.y > highestPriority.position.y)
                            {
                                highestPriority = overlappedTransform;
                            }
                        }
                        TrySelectInteractible(highestPriority);
                    }
                    else DeselectCurrentInteractible();
                }
                else DeselectCurrentInteractible();
                return;
            }
            //Selecao normal, de raycast
            if(Physics.Raycast(ray, out interactibleHit, interactionHeight, interactibleLayerMask))
            {
                TrySelectInteractible(interactibleHit.transform);
            }
            else
            {
                DeselectCurrentInteractible();
            }

            //Debug.DrawLine(interactionOrigin.position, interactionOrigin.position + (Vector3.down * interactionHeight), Color.magenta);
        }

        private void TrySelectInteractible(Transform hit)
        {
            if (hit.TryGetComponent<Assets.Scripts.Interactibles.Interactible >(out var interactible))
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

        private void ActivatePauseScreen() => GameManager.Instance.SetMenuScreenActive(true);

        private System.Collections.IEnumerator MoveToDestinationAI(Transform target, System.Action actionBefore = null, System.Action actionAfter = null)
        {
            actionBefore?.Invoke();

            playerNavigationAgent.enabled = true;

            Vector3 direction = target.position - transform.position;
            direction.y = 0;
            var a = Quaternion.LookRotation(direction, Vector3.up);
            yield return transform.DORotate(a.eulerAngles, .7f).WaitForCompletion();

            playerNavigationAgent.speed = aiSpeed;

            playerNavigationAgent.SetDestination(target.position);

            playerAnim.SetFloat("velocityMagnitude", .15f);

            yield return new WaitUntil
            (
                () =>
                {
                    return !playerNavigationAgent.pathPending
                    && (playerNavigationAgent.remainingDistance <= playerNavigationAgent.stoppingDistance);
                }
            );

            playerNavigationAgent.Warp(target.position);

            playerNavigationAgent.enabled = false;

            actionAfter?.Invoke();

            automaticMovementRoutine = null;
        }
        #endregion

        #region PUBLIC_METHODS
        public void SetInteracting()
        {
            isInteracting = true;
        }

        public void SetNotInteracting()
        {
            isInteracting = false;
        }

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

        public void TriggerPause()
        {
            if (pauseLocation == null) return;

            if (!GameManager.Instance.IsPaused)
                //SetPauseFalse();
            //else
                SetNavigationTarget(pauseLocation);
        }

        public void SetPauseTrue() => GameManager.Instance.SetPause(true);
        
        public void SetPauseFalse() => GameManager.Instance.SetPause(false);

        public void SetNavigationTarget(Transform target)
        {
            if(automaticMovementRoutine != null) StopCoroutine(automaticMovementRoutine);
            
            automaticMovementRoutine = StartCoroutine
            (
                MoveToDestinationAI
                (   
                    target, 
                    SetPauseTrue,
                    () => 
                    { 
                        transform.DORotate(target.rotation.eulerAngles, .7f);
                        //SetPauseFalse();
                        ActivatePauseScreen();
                        OnPaused?.Invoke();
                    }
                )
            );
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

        private void OnDrawGizmos()
        {
            if (!isGridSelectionActive) return;

            Gizmos.DrawCube(gridPosition + Vector3.up * ((gridSelectionSize.y * .5f) - gridSelectioVerticalOffset), gridSelectionSize);
            Gizmos.color = Color.yellow;
        }
    }

}