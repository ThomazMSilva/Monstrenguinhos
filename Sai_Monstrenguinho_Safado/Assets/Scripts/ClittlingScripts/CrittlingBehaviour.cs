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
        [SerializeField] private SpriteRenderer spriteRenderer;
        public BehaviourState CurrentState;

        [Space(8f)]
        [Header("Movement")]
        [Space(8f)]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private string cropPreferenceName = "Berinjela";
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

        private Coroutine digestRoutine;
        private Coroutine eatRoutine;
        private bool debugIsEating;
        private Coroutine roamingRoutine;
        private Vector3 originalPosition;
        private Tween punchScaleTween;
        #endregion

        #region UNITY_METHODS
        private void Start() => originalPosition = transform.position;
        private void OnDestroy() => Manager.RemoveCritterFromList(this);
        private void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;
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
                        ScareAway();
                    }
                }
            }
        }

        public void UpdateBehaviour()
        {
            switch (CurrentState)
            {
                case BehaviourState.roaming:
                    Debug.Log($"{gameObject.name} call: roaming;");
                    agent.speed = digestingSpeed;
                    var closestPlotRoaming = Manager.GetClosestPlot(transform.position, cropPreferenceName);

                    if (closestPlotRoaming == null)
                    {
                        Debug.Log($"Monstrenguinho {gameObject.name} nao achou transform mais proximo");
                        punchScaleTween ??= spriteRenderer.DOBlendableColor(Color.magenta, .04f)
                            .OnComplete
                            (() => 
                                { 
                                    spriteRenderer.
                                    DOBlendableColor(Color.white, .04f)
                                    .OnComplete(() => punchScaleTween = null); 
                                }
                            );
                        break;
                    }
                    currentTargetedPlot = closestPlotRoaming;
                    CurrentState = BehaviourState.chasing;
                    break;

                case BehaviourState.chasing:
                    Debug.Log($"{gameObject.name} call: chasing;");
                    agent.speed = chasingSpeed;

                    var closestPlotChasing = Manager.GetClosestPlot(transform.position, cropPreferenceName);
                    if (closestPlotChasing == null)
                    {
                        Debug.Log($"Monstrenguinho {gameObject.name} nao achou transform mais proximo");
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
                    Debug.Log($"{gameObject.name} call: digesting;");
                    digestRoutine ??= StartCoroutine(Digest());
                    break;

                case BehaviourState.fleeing:
                    Debug.Log($"{gameObject.name} call: fleeing;");
                    agent.SetDestination(originalPosition);
                    break;
            }
        }

        private System.Collections.IEnumerator EatCrop()
        {
            agent.speed = eatingSpeed;
            eatingTimeElapsed = 0;
            debugIsEating = true;
            while (IsPlotPlanted())
            {
                eatingTimeElapsed += Time.deltaTime;
                
                if(eatingTimeElapsed > eatingTime)
                {
                    currentTargetedPlot.EmptyPlot();
                    eatRoutine = null;
                    debugIsEating = false;
                    CurrentState = BehaviourState.digesting;
                    yield break;
                }
                yield return null;
            }
            
            if (CurrentState != BehaviourState.digesting)
            {
                Debug.Log($"{gameObject.name} teve refeicao interrompida");
                debugIsEating = false;
                eatRoutine = null;
                CurrentState = BehaviourState.chasing;
            }

            currentTargetedPlot = null;
            eatRoutine = null;
        }

        private System.Collections.IEnumerator Digest()
        {
            agent.speed = digestingSpeed;
            yield return new WaitForSeconds(digestingTime);
            CurrentState = BehaviourState.chasing;
            digestRoutine = null;
        }

        /*private System.Collections.IEnumerator Roam()
        {

        }*/

        public void ScareAway()
        {
            if(eatRoutine != null) StopCoroutine(eatRoutine);
            if (digestRoutine != null) StopCoroutine(digestRoutine);
            GetComponent<Collider>().enabled = false;
            agent.speed = chasingSpeed;
            CurrentState = BehaviourState.fleeing;
            spriteRenderer.DOFade(0, fadeOutTime).OnComplete(() => Destroy(gameObject, 1f));
        }

        private bool InPlotRange()
        {
            //Debug.Log($"Distancia {gameObject.name}: "+(currentTargetedPlot.transform.position - transform.position).sqrMagnitude);
            return (currentTargetedPlot.transform.position - transform.position).sqrMagnitude < destinationThreshold;
        }

        private bool IsPlotPlanted()
        {
            return currentTargetedPlot.currentCrop != null 
                && !string.IsNullOrEmpty(currentTargetedPlot.currentCrop.CropName);
        }
    }
}