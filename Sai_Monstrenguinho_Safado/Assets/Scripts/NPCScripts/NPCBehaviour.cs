using UnityEngine;

namespace Assets.Scripts.NPCScripts
{
    using DG.Tweening;

    public class NPCBehaviour : Interactibles.Interactible
    {
        #region ATTRIBUTES
        [SerializeField] private Animator npcAnim;

        [Space(8f), Header("Pedidos"), Space(8f)]
        [SerializeField] private ClientAttributes attributes;
        [SerializeField] private GameObject orderDisplay;
        [SerializeField] private UnityEngine.UI.Image orderBgIMG;
        [SerializeField] private TMPro.TextMeshProUGUI orderTMP;
        [SerializeField] private float orderFadeDuration = .7f;
        [SerializeField] private System.Collections.Generic.List<Interactibles.CropAttributes> deliveredCrops = new();
        private System.Collections.Generic.List<Interactibles.CropType> desiredCrops = new();
        private int orderAmount;
        private float tolerance;
        private float elapsedTolerance;

        [Space(8f), Header("Navegação"), Space(8f)]
        [SerializeField] private UnityEngine.AI.NavMeshAgent npcNavigationAgent;
        [SerializeField] private float distanceThreshold = 1.1f;
        [SerializeField] private float navigationSpeed = 1f;
        [SerializeField] private float rotationDuration = .4f;
        private Vector3 originalPosition;
        private Vector3 targetDestination;
        private Quaternion targetRotation;
        private float currentDistance; //só pra debug no inspector. isso nao precisa ser guardado.
        private Coroutine toleranceRoutine;


        [HideInInspector] public UnityEngine.Events.UnityEvent<NPCBehaviour> onDestroy;
        #endregion

        public void SetAttributes(ClientAttributes newAttributes)
        {
            originalPosition = transform.position;
            npcNavigationAgent.speed = navigationSpeed;
            attributes = newAttributes;
            tolerance = attributes.RandomizedToleranceTime;
            orderAmount = attributes.RandomizedOrderAmount;
        }

        public void SetTarget(Transform target, System.Action onFinish)
        {
            targetDestination = target.position;
            targetRotation = target.rotation;
            StartMovingToTarget(onFinish);
        }

        public void SetTarget(Vector3 position, Quaternion rotation, System.Action onFinish)
        {
            targetDestination = position;
            targetRotation = rotation;
            StartMovingToTarget(onFinish);
        }

        private void StartMovingToTarget(System.Action onFinish)
        {
            npcNavigationAgent.SetDestination(targetDestination);
            npcAnim.SetFloat("velocityMagnitude", .12f);
            StartCoroutine(CheckPosition(onFinish));
        }

        private void FinishMovingToTarget(System.Action onFinish)
        {
            npcAnim.SetFloat("velocityMagnitude", 0f);
            npcNavigationAgent.Warp(targetDestination);
            npcNavigationAgent.isStopped = true;
            transform.DORotate(targetRotation.eulerAngles, rotationDuration);

            onFinish?.Invoke();
        }

        private System.Collections.IEnumerator CheckPosition(System.Action onFinish)
        {
            while (true)
            {
                currentDistance = (transform.position - targetDestination).sqrMagnitude;
                if (currentDistance < distanceThreshold)
                {
                    FinishMovingToTarget(onFinish);
                    break;
                }
                yield return null;
            }
        }

        private System.Collections.IEnumerator CountTolerance()
        {
            elapsedTolerance = 0;
            while (elapsedTolerance < tolerance)
            {
                elapsedTolerance += Time.deltaTime;
                yield return null;
            }
            ReturnHome();
            toleranceRoutine = null;
        }

        public void OrderCrops()
        {
            orderDisplay.SetActive(true);
            orderBgIMG.DOFade(0, 0).OnComplete(() => orderBgIMG.DOFade(1, orderFadeDuration));
            orderTMP.DOFade(0, 0).OnComplete(() => orderTMP.DOFade(1, orderFadeDuration));


            for (int i = 0; i <= orderAmount; i++)
            {
                var desiredCrop = (Interactibles.CropType)
                    WeightedRandomByPercentage
                    (
                        1,
                        System.Enum.GetNames(typeof(Interactibles.CropType)).Length - 1,
                        (int) attributes.Preference,
                        75
                    );
                desiredCrops.Add(desiredCrop);
            }
            toleranceRoutine = StartCoroutine(CountTolerance());
        }

        public void DeliverCrops(System.Collections.Generic.List<Interactibles.CropAttributes> crops)
        {
            deliveredCrops.AddRange(crops);

            if(deliveredCrops.Count >= desiredCrops.Count)
            {
                ReturnHome();
            }
        }

        private void ReturnHome()
        {
            if(toleranceRoutine != null) StopCoroutine(toleranceRoutine);

            orderBgIMG.DOFade(0, orderFadeDuration);
            orderTMP.DOFade(0, orderFadeDuration).OnComplete(() => orderDisplay.SetActive(false));
            var rends = GetComponentsInChildren<Renderer>();

            Debug.Log("rends: "+ rends.Length);
            for (int i = 0; i < rends.Length; i++)
            {
                Renderer rend = rends[i];
                if (i == rends.Length - 1)
                {
                    
                    rend.material.DOFade(0, orderFadeDuration).OnComplete(() => Destroy(gameObject, 2f));
                    break;
                }
                rend.material.DOFade(0, orderFadeDuration * 3f);
            }

            SetTarget(originalPosition, Quaternion.identity, () => { });
        }

        public override void Interact(object sender = null)
        {
            if(sender != null && sender is PlayerScripts.PlayerController player)
            {
                switch (player.HeldItem.itemTag)
                {
                    case PlayerScripts.ItemTag.Box:
                        if (player.TryGetFromHeld<Interactibles.BoxHoldable>(out var box))
                        {
                            DeliverCrops(box.storedCrops);
                            box.ClearCrops();
                            box.ReturnToStartingPoint();
                            player.SetHeldEmpty();
                        }
                        break;
                    case PlayerScripts.ItemTag.Crop:
                        if (player.TryGetFromHeld<Interactibles.CropHoldable>(out var singleCrop))
                        {
                            System.Collections.Generic.List<Interactibles.CropAttributes> singleCropList = new() { singleCrop.cropAttributes };
                            DeliverCrops(singleCropList);
                            player.SetHeldEmpty();
                            singleCrop.ReturnToStartingPoint();
                        }
                        break;

                    default: break;
                }
            }
        }

        public static int WeightedRandomByPercentage(int minInlcusive, int maxInclusive, int preference, float percentage)
        {
            if (minInlcusive > maxInclusive)
            {
                (minInlcusive, maxInclusive) = (maxInclusive, minInlcusive);
            }

            percentage = Mathf.Clamp(percentage, 0f, 100f);

            if (minInlcusive == maxInclusive) return minInlcusive;
            if (preference < minInlcusive || preference > maxInclusive) return Random.Range(minInlcusive, maxInclusive + 1);

            float preferenceProbability = percentage / 100f;
            float randomValue = Random.value;

            if (randomValue < preferenceProbability)
            {
                return preference;
            }

            int totalOtherValues = maxInclusive - minInlcusive;
            //float otherValueProbability = (1f - preferenceProbability) / totalOtherValues;

            float adjustedRandom = (randomValue - preferenceProbability) / (1f - preferenceProbability);
            int selectedIndex = Mathf.FloorToInt(adjustedRandom * totalOtherValues);

            return selectedIndex < (preference - minInlcusive)
                ? minInlcusive + selectedIndex
                : minInlcusive + selectedIndex + 1;
        }

        private void OnDestroy() => onDestroy?.Invoke(this);
    }
}
