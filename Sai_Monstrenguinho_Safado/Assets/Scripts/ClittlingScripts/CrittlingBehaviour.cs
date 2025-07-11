using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.ClittlingScripts
{
    public class CrittlingBehaviour : Interactibles.Interactible
    {
        #region ATTRIBUTES
        [System.Serializable]
        public enum BehaviourState
        {
            roaming,
            chasing,
            eating,
            digesting,
            fleeing
        }


        public CrittlingsManager Manager;
        [SerializeField] private Animator crittlingAnim;
        [SerializeField] private SpriteRenderer spriteRenderer;
        public BehaviourState CurrentState;
        [SerializeField] private bool randomizeColor;
        [SerializeField] private int hitsToFlee = 1;
        private int hitsTaken;

        [Space(8f)]
        [Header("Movement")]
        [Space(8f)]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Interactibles.CropType cropPreferenceName = (Interactibles.CropType) 1;
        [SerializeField] private float destinationThreshold = .1f;
        [SerializeField] private float chasingSpeed = 3f;
        [SerializeField] private float fleeingSpeed = 7f;
        [SerializeField] private float eatingSpeed = 0f;
        [SerializeField] private float digestingSpeed = .01f;
        
        [Space(8f)]
        [Header("Timers")]
        [Space(8f)]
        [SerializeField] private float eatingTime = 7f;
        private float eatingTimeElapsed = 0;
        [SerializeField] private float digestingTime = 3f;
        [SerializeField] private float fadeOutTime = 2f;
        private Interactibles.Plot currentTargetedPlot;

        private bool debugIsEating;
        private bool isFacingRight = false;
        private bool IsFacingRight 
        {
            get
            {
                float currentVelocity = agent.velocity.x;
                
                if (currentVelocity == 0) return isFacingRight;

                isFacingRight = currentVelocity > 0;
                return isFacingRight;
            }
        }
        private Vector3 originalPosition;
        private Color spawnColor = Color.white;
        private Coroutine digestRoutine;
        private Coroutine eatRoutine;
        private Tween punchScaleTween;



        private bool isGamePaused;
        private WaitUntil waitUntilHameUnpauses;


        private bool InPlotRange()
        {
            //Debug.Log($"Distancia {gameObject.name}: "+(currentTargetedPlot.transform.position - transform.position).sqrMagnitude);
            return (currentTargetedPlot.transform.position - transform.position).sqrMagnitude < destinationThreshold;
        }

        private bool IsPlotPlanted()
        {
            return currentTargetedPlot.currentCrop != null 
                && (currentTargetedPlot.currentCrop.IsPlanted);
        }
        #endregion

        #region UNITY_METHODS
        private void Start()
        {
            PauseBehaviour(GameManager.Instance.IsPaused);
            GameManager.Instance.OnPause += PauseBehaviour;

            originalPosition = transform.position;
            if (randomizeColor)
            {
                spawnColor = Random.ColorHSV(0, 1, 0, 1, .7f, 1, 1, 1);
                spriteRenderer.material.color = spawnColor;
            }
            else
            {
                spawnColor = spriteRenderer.material.color;
            }
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnPause -= PauseBehaviour;
            Manager.RemoveCritterFromList(this);
        }

        private void PauseBehaviour(bool isPaused)
        {
            isGamePaused = isPaused;
            if (isPaused)
            {
                agent.isStopped = true;
                waitUntilHameUnpauses = new(() => !isGamePaused);
            }
            else
            {
                agent.isStopped = false;
                switch (CurrentState)
                {
                    case BehaviourState.chasing:
                        agent.speed = chasingSpeed;
                        break;
                    case BehaviourState.fleeing:
                        agent.speed = fleeingSpeed;
                        break;
                    case BehaviourState.digesting:
                        agent.speed = digestingSpeed;
                        break;
                    default:
                        agent.speed = 0;
                        break;
                }
            }
        }
        #endregion

        public override void Interact(object sender = null)
        {
            if (sender != null && sender is PlayerScripts.PlayerController player)
            {
                if (player.HeldItem.itemTag == PlayerScripts.ItemTag.Broom)
                {
                    if (player.TryGetFromHeld<Interactibles.BroomHoldable>(out var broom))
                    {
                        crittlingAnim.SetTrigger("hit");
                        hitsTaken++;

                        if(hitsTaken >= hitsToFlee)
                            ScareAway();
                    }
                }
            }
        }

        public void UpdateBehaviour()
        {
            spriteRenderer.flipX = !IsFacingRight;
            switch (CurrentState)
            {
                case BehaviourState.roaming:
                    //Debug.Log($"{gameObject.name} call: roaming;");
                    crittlingAnim.SetFloat("velocity", 0);
                    agent.speed = digestingSpeed;
                    var closestPlotRoaming = Manager.GetClosestPlot(transform.position, cropPreferenceName);

                    if (closestPlotRoaming == null)
                    {
                        //Debug.Log($"Monstrenguinho {gameObject.name} nao achou transform mais proximo");
                        punchScaleTween ??= spriteRenderer.DOBlendableColor(Color.magenta, .04f)
                            .OnComplete
                            (() => 
                                { 
                                    spriteRenderer.
                                    DOBlendableColor(spawnColor, .04f)
                                    .OnComplete(() => punchScaleTween = null); 
                                }
                            );
                        break;
                    }
                    currentTargetedPlot = closestPlotRoaming;
                    CurrentState = BehaviourState.chasing;
                    break;

                case BehaviourState.chasing:
                    //Debug.Log($"{gameObject.name} call: chasing;");

                    crittlingAnim.SetFloat("velocity", 1);

                    agent.speed = chasingSpeed;

                    var closestPlotChasing = Manager.GetClosestPlot(transform.position, cropPreferenceName);
                    if (closestPlotChasing == null)
                    {
                        //Debug.Log($"Monstrenguinho {gameObject.name} nao achou transform mais proximo");
                        CurrentState = BehaviourState.roaming;
                        break;
                    }
                    currentTargetedPlot = closestPlotChasing;
                    agent.SetDestination(currentTargetedPlot.transform.position);

                    if (InPlotRange()) CurrentState = BehaviourState.eating;

                    break;

                case BehaviourState.eating:
                    Debug.Log($"{gameObject.name} call: eating;");
                    eatRoutine ??= StartCoroutine(EatCrop());
                    break;

                case BehaviourState.digesting:
                    //Debug.Log($"{gameObject.name} call: digesting;");
                    digestRoutine ??= StartCoroutine(Digest());
                    break;

                case BehaviourState.fleeing:
                    //Debug.Log($"{gameObject.name} call: fleeing;");
                    agent.SetDestination(originalPosition);
                    break;
            }
        }

        public void ScareAway()
        {
            if (eatRoutine != null)
            {
                StopCoroutine(eatRoutine);
                crittlingAnim.SetBool("eating", false);
            }

            if (digestRoutine != null)
            {
                crittlingAnim.SetBool("digesting", false);
                StopCoroutine(digestRoutine);
            }

            crittlingAnim.SetBool("fleeing", true);

            GetComponent<Collider>().enabled = false;
            agent.speed = chasingSpeed;
            CurrentState = BehaviourState.fleeing;
            spriteRenderer.DOFade(0, fadeOutTime).OnComplete(() => Destroy(gameObject, 1f));
        }


        #region ROUTINES
        private System.Collections.IEnumerator EatCrop()
        {
            agent.speed = eatingSpeed;
            eatingTimeElapsed = 0;
            debugIsEating = true;

            crittlingAnim.SetBool("eating", true);

            while (IsPlotPlanted())
            {
                if (isGamePaused)
                {
                    crittlingAnim.SetBool("eating", false);
                    yield return waitUntilHameUnpauses;
                    crittlingAnim.SetBool("eating", true);
                }

                eatingTimeElapsed += Time.deltaTime;
                
                if(eatingTimeElapsed > eatingTime)
                {
                    currentTargetedPlot.EmptyPlot();
                    eatRoutine = null;
                    debugIsEating = false;
                    crittlingAnim.SetBool("eating", false);
                    CurrentState = BehaviourState.digesting;
                    yield break;
                }
                yield return null;
            }

            if (isGamePaused) yield return waitUntilHameUnpauses;

            if (CurrentState != BehaviourState.digesting)
            {
                Debug.Log($"{gameObject.name} teve refeicao interrompida");
                debugIsEating = false;
                crittlingAnim.SetBool("eating", false);
                eatRoutine = null;
                CurrentState = BehaviourState.chasing;
            }

            currentTargetedPlot = null;
            eatRoutine = null;
        }

        private System.Collections.IEnumerator Digest()
        {
            agent.speed = digestingSpeed;
            crittlingAnim.SetBool("digesting", true);

            float digestTimeRemaining = digestingTime;

            while (digestTimeRemaining > 0)
            {
                if (isGamePaused)
                {
                    crittlingAnim.SetBool("digesting", false);
                    yield return waitUntilHameUnpauses;
                    crittlingAnim.SetBool("digesting", true);
                }
                else
                {
                    digestTimeRemaining -= Time.deltaTime;
                }
                yield return null;
            }


            crittlingAnim.SetBool("digesting", false);
            CurrentState = BehaviourState.chasing;
            digestRoutine = null;
        }
        #endregion
    }
}