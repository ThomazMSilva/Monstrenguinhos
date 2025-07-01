using UnityEngine;

namespace Assets.Scripts.NPCScripts
{
    using DG.Tweening;
    using System.Linq;


    public class NPCBehaviour : Interactibles.Interactible
    {
        #region ATTRIBUTES
        
        #region ORDER_ATTRIBUTES
        [SerializeField] private Animator npcAnim;

        [Space(8f), Header("Pedidos"), Space(8f)]
        [SerializeField] private ClientAttributes attributes;
        [SerializeField] private Cinemachine.CinemachineVirtualCamera virtualCamera;
        [Space(8f)]
        [SerializeField] private GameObject orderDisplay;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform orderBackgroundPanel;
        [SerializeField] private UnityEngine.UI.LayoutGroup orderLayoutGroup;
        [SerializeField] private TMPro.TextMeshProUGUI orderTMP;

        [SerializeField] private float orderFadeDuration = .7f;
        [SerializeField] private System.Collections.Generic.List<Interactibles.CropType> deliveredCrops = new();

        [Space(8f)]
        
        [SerializeField] private UnityEngine.UI.Image orderImagePrefab;

        [SerializeField] CropSpritesReference cropSpritesReference;

        private System.Collections.Generic.List<UnityEngine.UI.Image> desiredCropImages = new();
        private System.Collections.Generic.List<Interactibles.CropType> desiredCrops = new();
        private int orderAmount;
        private float tolerance;
        private float elapsedTolerance;
        public string Name => attributes.ClientName;
        public UnityEngine.Events.UnityEvent OnSucceeded;
        public UnityEngine.Events.UnityEvent OnFailed;
        private Coroutine toleranceRoutine;
        #endregion

        #region NAVIGATION_ATTRIBUTES
        [Space(8f), Header("Navegação"), Space(8f)]
        [SerializeField] private UnityEngine.AI.NavMeshAgent npcNavigationAgent;
        [SerializeField] private float distanceThreshold = 1.1f;
        [SerializeField] private float navigationSpeed = 1f;
        [SerializeField] private float rotationDuration = .4f;
        private Vector3 originalPosition;
        private Vector3 targetDestination;
        private Quaternion targetRotation;
        private float currentDistance; //só pra debug no inspector. isso nao precisa ser guardado.
        #endregion

        private bool wasStoppedBeforePause;
        private bool wasRootMotionBeforePause;
        private bool isGamePaused;
        private WaitUntil waitUntilGameUnpauses;
        [HideInInspector] public UnityEngine.Events.UnityEvent<NPCBehaviour> onDestroy;

        #endregion

        #region PRIVATE_METHODS
        private void StartMovingToTarget(System.Action onFinish)
        {
            npcNavigationAgent.isStopped = false;
            npcNavigationAgent.SetDestination(targetDestination);
            npcAnim.SetFloat("velocityMagnitude", .12f);
            StartCoroutine(CheckPosition(onFinish));
        }

        private System.Collections.IEnumerator CheckPosition(System.Action onFinish)
        {
            while (true)
            {
                if (isGamePaused) yield return waitUntilGameUnpauses;

                currentDistance = (transform.position - targetDestination).sqrMagnitude;
                if (currentDistance < distanceThreshold)
                {
                    FinishMovingToTarget(onFinish);
                    break;
                }
                yield return null;
            }
        }

        private void FinishMovingToTarget(System.Action onFinish)
        {
            npcAnim.SetFloat("velocityMagnitude", 0f);
            npcNavigationAgent.isStopped = true;
            npcNavigationAgent.Warp(targetDestination);
            transform.DORotate(targetRotation.eulerAngles, rotationDuration)
                     .OnComplete(() => onFinish?.Invoke());

            //onFinish?.Invoke();
        }

        private System.Collections.IEnumerator CountTolerance()
        {
            elapsedTolerance = 0;
            while (elapsedTolerance < tolerance)
            {
                if (isGamePaused) yield return waitUntilGameUnpauses;

                elapsedTolerance += Time.deltaTime;
                yield return null;
            }
            OnFailed?.Invoke();
            ReturnHome();
            toleranceRoutine = null;
        }
        
        private bool IsDeliveryCorrect()
        {
            var desired = desiredCrops;
            var delivered = deliveredCrops;

            /*if (desired.Count != delivered.Count)
            {
                Debug.Log("Lista base tem tamanho diferente, nem calcula");
                return false; 
            }*/

            var groupedDesiredList = desired.GroupBy(d => d.ToString()).ToList();
            var groupedDeliveredList = delivered.GroupBy(d => d.ToString()).ToList();

            foreach(var group in groupedDesiredList)
            {
                Interactibles.CropType cropType = (Interactibles.CropType)System.Enum.Parse(typeof(Interactibles.CropType), group.Key);

                var groupDesiredImages = desiredCropImages
                    .Where(img => img.sprite == cropSpritesReference.sprites.FirstOrDefault(s => s.cropType == cropType).sprite).ToList();
                    //.FirstOrDefault();

                var deliveredInGroup = deliveredCrops.Where(c => c == cropType).ToList();

                Debug.Log($"Checando {cropType.ToString()} - desejadas: {group.Count()}; imagens achadas: {groupDesiredImages.Count}; entregues: {deliveredInGroup.Count}");

                for (int i = 0; i < deliveredInGroup.Count && i < groupDesiredImages.Count; i++)
                {
                    groupDesiredImages[i].color = Color.green;
                }

            }

            if (groupedDeliveredList.Count != groupedDesiredList.Count)
            {
                Debug.Log("Lista agrupada tem tamanho diferente, nem calcula");
                return false;
            }
            return true;
        }

        private void ReturnHome()
        {
            if(toleranceRoutine != null) StopCoroutine(toleranceRoutine);

            canvasGroup.DOFade(0, orderFadeDuration).OnComplete(() => orderDisplay.SetActive(false));

            var rends = GetComponentsInChildren<Renderer>();

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
        #endregion

        #region PUBLIC_METHODS
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

        public void OrderCrops()
        {
            canvasGroup.alpha = 0f;


            orderDisplay.SetActive(true);
            canvasGroup.DOFade(1, orderFadeDuration);

            for (int i = 0; i <= orderAmount; i++)
            {
                var desiredCrop = (Interactibles.CropType)
                    WeightedRandomByPercentage
                    (
                        1,
                        System.Enum.GetNames(typeof(Interactibles.CropType)).Length - 1,
                        (int) attributes.Preference,
                        attributes.PreferencePercentage
                    );

                var order = Instantiate(orderImagePrefab, orderLayoutGroup.transform);
                Sprite desiredCropSprite = cropSpritesReference.sprites.FirstOrDefault(s => s.cropType == desiredCrop).sprite;
                order.sprite = desiredCropSprite;
                
                desiredCropImages.Add(order);
                desiredCrops.Add(desiredCrop);
            }
            orderBackgroundPanel.sizeDelta = new(orderBackgroundPanel.sizeDelta.x, desiredCrops.Count);
            //orderTMP.text = TempDesiredAmountDebug();
            toleranceRoutine = StartCoroutine(CountTolerance());
        }

        public void DeliverCrops(System.Collections.Generic.List<Interactibles.CropAttributes> crops)
        {
            if (toleranceRoutine == null) return;

            System.Collections.Generic.List<Interactibles.CropType> cropTypes =
                crops.Select(c => c.CropName).ToList();

            deliveredCrops.AddRange(cropTypes);

            bool isCorrect = IsDeliveryCorrect();

            if (deliveredCrops.Count >= desiredCrops.Count)
            {
                if (isCorrect)
                {
                    Debug.Log($"{gameObject.name}: Eba entregou certo");
                    OnSucceeded?.Invoke();
                }

                else
                {
                    Debug.Log($"{gameObject.name}: Seu bosta entregou tudo errado");
                    OnFailed?.Invoke();
                }

                ReturnHome();
            }
            else Debug.Log($"{gameObject.name}: Ta faltando coisa, filho. Pedi mais, cade");
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
        #endregion

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

        private void PauseBehaviour(bool isPaused)
        {
            if (isGamePaused == isPaused) return;

            isGamePaused = isPaused;

            if (isPaused)
            {
                wasStoppedBeforePause = npcNavigationAgent.isStopped;
                wasRootMotionBeforePause = npcAnim.applyRootMotion;
                //npcNavigationAgent.speed = 0;
                npcNavigationAgent.velocity = Vector3.zero;
                npcNavigationAgent.isStopped = true;
                npcAnim.applyRootMotion = false;
                waitUntilGameUnpauses = new(() => !isGamePaused);
            }
            else
            {
                npcNavigationAgent.isStopped = wasStoppedBeforePause;
                npcNavigationAgent.speed = navigationSpeed;
                npcAnim.applyRootMotion = wasRootMotionBeforePause;

                if (!npcNavigationAgent.isStopped && npcNavigationAgent.destination != targetDestination)
                {
                    npcNavigationAgent.SetDestination(targetDestination);
                }
            }
        }

        private void Start() => GameManager.Instance.OnPause += PauseBehaviour;

        private void OnDestroy()
        {
            GameManager.Instance.OnPause -= PauseBehaviour;
            onDestroy?.Invoke(this);
        }
    }
}
