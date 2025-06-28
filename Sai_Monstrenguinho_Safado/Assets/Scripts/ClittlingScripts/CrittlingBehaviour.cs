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
        private Vector3 originalPosition;
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
                case BehaviourState.chasing:
                    Debug.Log($"{gameObject.name} call: chasing;");
                    agent.speed = chasingSpeed;
                    var closestPlot = Manager.GetClosestPlot(transform.position, cropPreferenceName);

                    if (closestPlot == null)
                    {
                        Debug.Log($"Monstrenguinho {gameObject.name} nao achou transform mais proximo");
                        break;
                    }
                    currentTargetedPlot = closestPlot;

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
            while (IsPlotPlanted())
            {
                eatingTimeElapsed += Time.deltaTime;
                
                if(eatingTimeElapsed > eatingTime)
                {
                    currentTargetedPlot.EmptyPlot();
                    CurrentState = BehaviourState.digesting;
                    yield break;
                }
                yield return null;
            }
            
            if (CurrentState != BehaviourState.digesting)
            {
                Debug.Log($"{gameObject.name} teve refeicao interrompida");
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

        public void ScareAway()
        {
            StopAllCoroutines();
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