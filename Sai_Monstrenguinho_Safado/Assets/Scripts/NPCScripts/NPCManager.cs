using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.NPCScripts
{
    public class NPCManager : MonoBehaviour
    {
        #region ATTRIBUTES
        [SerializeField] private GameObject npcPrefab;
        [SerializeField] private Transform npcParent;

        [SerializeField] private Transform targetPositionParent;

        [Tooltip("As posições pra onde os npcs vão quando spawnam (a filazinha). Eles spawnam e são designados uma posição pra ir. \nVão na ordem da lista, então se uma posição x tá na frente da y no mundo, mas tá embaixo da y na lista, os npcs vão primeiro pra y.")]
        [SerializeField] private List<Transform> targetPositions = new();
        private float currentInterval;
        [Tooltip("Se chega no número máximo de npcs permitidos, vai checar a cada [esse tanto] de segundos se liberou. Depois de terminar todos os eventos, dá pra tirar isso aqui.")]
        [SerializeField] private float capCheckInterval = .5f;

        private Vector3 spawnOffsetDirection;
        [Tooltip("Se estão serializadas menos de duas posições de alvo para os npcs irem atrás, \nspawna novas a partir desse offset em relação à primeira posição.")]
        [SerializeField] private Vector3 defaultOffset = Vector3.forward;
        private WaitForSeconds waitForCapCheck;
        private List<NPCBehaviour> spawnedNPCs = new();
        private Dictionary<Transform, NPCBehaviour> positionsOccupationDict = new();

        [SerializeField] private int maximumFailedClients = 3;
        public int MaximumFailedClients => maximumFailedClients;

        private int totalFailedClients;
        public UnityEngine.Events.UnityEvent OnFailedClient;
        public UnityEngine.Events.UnityEvent OnSucceededClient;
        public UnityEngine.Events.UnityEvent OnLost;
        public UnityEngine.Events.UnityEvent OnWon;

        private GameManager game;
        private ManagerScripts.AudioManager audioManager;
        private AudioClip successClip, failureClip;

        private bool isGamePaused;
        private bool readyToPassStage;
        private WaitUntil waitUntilGameUnpauses;
        private WaitUntil waitUntilLastCLientFromStage;
        private Coroutine stageTimerRoutine;
        #endregion

        #region UNITY_METHODS
        //#if UNITY_EDITOR
        private void Update()
        {
            if (game.IsPaused) return;

            var client = game.CurrentStage.Clients;

            game.CurrentStage.StageTime += Time.deltaTime;

            if(client.ClientsRemaining <= 0)
            {
                client.TotalTimeWaitedWithNoClients += Time.deltaTime;
                if (client.ForceClientOnScreen)
                {
                    client.CurrentTimeWaitedWithNoClients += Time.deltaTime;
                }
            }

            if(client.ClientsRemaining > 0)
            {
                client.TotalTimeWaitedWithClients += Time.deltaTime;
                if (client.ForceClientOnScreen && client.CurrentTimeWaitedWithNoClients != 0)
                {
                    client.CurrentTimeWaitedWithNoClients = 0;
                }
            }
        }
//#endif
        private void Start()
        {
            if (!TryInitializeTargetPositions()) return;

            StartCoroutine(InitializeReferences());
        }

        private void OnDisable()
        {
            game.OnPause -= PauseBehaviour;
            game.OnStagePassed -= OnStagePassed;
            game.OnRestart -= Restart;
            game.LastStage.OnCompleted.RemoveListener(Win);
        }
        #endregion

        private bool TryInitializeTargetPositions()
        {
            game = GameManager.Instance;
            audioManager = game.AudioManager;
            successClip = audioManager.AudioClips.SucceessAudioClip;
            failureClip = audioManager.AudioClips.FailureAudioClip;

            if (targetPositions.Count < 1)
            {
                Debug.LogError("Não há posição de spawn serializada no NPCManager; Retornando Start()");
                return false;
            }

            int maxClients = 5;

            foreach (var attribute in game.StageAttributes)
            {
                if(attribute.Clients.SpawnCap > maxClients) maxClients = attribute.Clients.SpawnCap;
            }

            if (targetPositions.Count < maxClients)
            {
                //Inicializa cadeia se só tiver 1 ponto
                if (targetPositions.Count == 1)
                {
                    var secondPosition = new GameObject("TargetPosition (1)").transform;
                    secondPosition.SetParent(targetPositionParent);
                    secondPosition.SetPositionAndRotation
                    (
                        targetPositions[0].position + defaultOffset,
                        targetPositions[0].rotation
                    );

                    targetPositions.Add(secondPosition);
                }

                spawnOffsetDirection = targetPositions[^1].position - targetPositions[^2].position;

                //Pra não alterar a lista que tá referenciada no loop
                List<Transform> copyTargetPositions = new(targetPositions);

                for (int i = targetPositions.Count + 1; i <= maxClients; i++)
                {
                    var newPosition = new GameObject($"TargetPosition ({i - 1})").transform;
                    newPosition.SetParent(targetPositionParent);
                    newPosition.SetPositionAndRotation
                    (
                        targetPositions[^1].position + spawnOffsetDirection * (i - targetPositions.Count),
                        newPosition.rotation = targetPositions[^1].rotation
                    );

                    copyTargetPositions.Add(newPosition);
                }
                targetPositions = copyTargetPositions;
            }

            foreach (var position in targetPositions)
            {
                if (!positionsOccupationDict.ContainsKey(position))
                {
                    positionsOccupationDict.Add(position, null);
                }
            }

            return true;
        }

        private IEnumerator InitializeReferences()
        {
            while (GameManager.Instance.IsLoading) yield return null;

            CheckStartStageTimer();

            game.OnPause += PauseBehaviour;
            game.OnStagePassed += OnStagePassed;
            game.OnRestart += Restart;
            game.LastStage.OnCompleted.AddListener(Win);

            waitForCapCheck = new(capCheckInterval);

            StartCoroutine(SpawnRoutine());
        }

        #region SPAWN_LOGIC
        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                if(isGamePaused) yield return waitUntilGameUnpauses;

                var clients = game.CurrentStage.Clients;

                yield return WaitForSpawnCap(clients);

                yield return WaitForClientInterval(clients);
                
                yield return CheckStageProgression(clients);

                SpawnClient();
            }
        }
        
        private IEnumerator WaitForSpawnCap(StageAttributes.NpcAttributes clients)
        {
            if(isGamePaused) yield return waitUntilGameUnpauses;

            while (spawnedNPCs.Count >= clients.SpawnCap)
            {
                Debug.Log("Esperando spawn cap de cliente.");
                if (isGamePaused) yield return waitUntilGameUnpauses;
                yield return waitForCapCheck;
                if (readyToPassStage) yield break;
            }
        }

        private IEnumerator WaitForClientInterval(StageAttributes.NpcAttributes clients)
        {
            if (isGamePaused) yield return waitUntilGameUnpauses;

            clients.Interval = Random.Range(clients.MinInterval, clients.MaxInterval);

            clients.TimeRemaining = clients.Interval;

            while (clients.TimeRemaining > 0)
            {
                Debug.Log("Esperando intervalo de clientes.");
                if (isGamePaused) yield return waitUntilGameUnpauses;

                if (clients.ForceClientOnScreen && clients.CurrentTimeWaitedWithNoClients >= clients.waitToForce)
                {
                    clients.CurrentTimeWaitedWithNoClients = 0;
                    clients.TimeRemaining = 0;
                    yield break;
                }
                if (readyToPassStage) yield break;

                clients.TimeRemaining -= Time.deltaTime;
                yield return null;
            }
        }
        
        private IEnumerator CheckStageProgression(StageAttributes.NpcAttributes clients)
        {
            if (isGamePaused) yield return waitUntilGameUnpauses;

            if (readyToPassStage)
            {
                Debug.Log("Loop de spawn identificou que tá pronto pra passar de estágio.");
                if (clients.waitForStageToEndBeforeSpawning)
                {
                    while (clients.ClientsRemaining > 0)
                    {
                        Debug.Log("Esperando clientes do estagio irem embora, pra passar de estágio.");
                        clients.TimeTakenFromNextStage += Time.deltaTime;
                        yield return null;
                    }
                }
                readyToPassStage = false;

                game.CurrentStage.Conditions.SuccessfulClientsPassed = game.CurrentStage.Conditions.SuccessfulClientsToPass;
                game.CurrentStage.Conditions.TimeElapsed = game.CurrentStage.Conditions.TimeToPass;

                game.PassToStage(game.CurrentStage.nextStageID);
            }
        }

        private void SpawnClient()
        {
            if (game.CurrentStage.Clients.SpawnableClients.Count <= 0) return;
            var client = Instantiate(npcPrefab, transform.position, transform.rotation, npcParent).GetComponent<NPCBehaviour>();
            spawnedNPCs.Add(client);
            game.CurrentStage.Clients.SpawnedAmount += 1;
            game.CurrentStage.Clients.ClientsRemaining += 1;

            client.SetAttributes(game.CurrentStage.Clients.SpawnableClients[Random.Range(0, game.CurrentStage.Clients.SpawnableClients.Count)]);
            client.gameObject.name = $"Cliente ({spawnedNPCs.Count - 1}) - {client.Name}";

            Transform furthestAvailablePosition = targetPositions[0];

            for (int i = 0; i < targetPositions.Count; i++)
            {
                var position = targetPositions[i];
                if (positionsOccupationDict.ContainsKey(position) && positionsOccupationDict[position] == null)
                {
                    furthestAvailablePosition = position;
                    break;
                }
            }

            positionsOccupationDict[furthestAvailablePosition] = client;
            client.SetTarget(furthestAvailablePosition, client.OrderCrops);
            
            
            var current = game.CurrentStage;

            client.onDestroy.AddListener(RemoveClientFromList);  

            client.OnFailed.AddListener
            (
                () => 
                {
                    current.Clients.ClientsRemaining -= 1;
                    audioManager.PlayClip(failureClip, client.transform);
                    current.Conditions.FailedClientsPassed++;
                    totalFailedClients++;
                    OnFailedClient?.Invoke();
                    if(totalFailedClients >= maximumFailedClients)
                    {
                        Lose();
                    }
                }
            );
            
            client.OnSucceeded.AddListener
            (
                () =>
                {
                    current.Clients.ClientsRemaining -= 1;
                    audioManager.PlayClip(successClip, client.transform);
                    current.Conditions.SuccessfulClientsPassed++;
                    OnSucceededClient?.Invoke();
                    //Passagem de estagio na condição de "por sucesso"
                    if(current.Conditions.SuccessBased && current.Conditions.SuccessfulClientsPassed >= current.Conditions.SuccessfulClientsToPass)
                    {
                        current.OnCompleted?.Invoke();
                        //Tirar daqui se não quiser automático
                        //game.PassToStage(current.nextStageID);

                        readyToPassStage = true;
                    }
                }
            );
        }
        #endregion

        #region STAGE_LOGIC
        private IEnumerator StageTimer()
        {
            var conditions = game.CurrentStage.Conditions;
            conditions.TimeElapsed = 0;
            
            while (conditions.TimeElapsed < conditions.TimeToPass)
            {
                if (isGamePaused) yield return waitUntilGameUnpauses;

                conditions.TimeElapsed += Time.deltaTime;
                yield return null;
            }
            Debug.Log("Acabou o tempo do "+game.CurrentStage.stageName);
            readyToPassStage = true;
            stageTimerRoutine = null;
        }

        private void CheckStartStageTimer()
        {
            if (stageTimerRoutine != null) StopCoroutine(stageTimerRoutine);

            if(game.CurrentStage.Conditions.TimeBased)
                stageTimerRoutine = StartCoroutine(StageTimer());
        }
     
        public void OnStagePassed()
        {
            CheckStartStageTimer();
            game.CurrentStage.Clients.TimeRemaining = game.CurrentStage.Clients.Interval;
            
            /*if(game.CurrentStage.Clients.PreSpawn)
                SpawnClient();*/
        }
        #endregion

        private void RemoveClientFromList(NPCBehaviour client)
        {
            spawnedNPCs.Remove(client);
            foreach (var kvp in positionsOccupationDict)
            {
                if (kvp.Value != client) continue;
                
                positionsOccupationDict[kvp.Key] = null;
                break;
            }
        }
        
        private void ReturnAllClients(NPCBehaviour except = null)
        {
            bool exc = (except != null); 

            foreach (var client in spawnedNPCs)
            {
                if (exc && client == except) continue;
                client.ReturnHome();
            }
        }
     
        private void Restart()
        {
            ReturnAllClients(null);
        }

        private void PauseBehaviour(bool isPaused)
        {
            isGamePaused = isPaused;
            if(isPaused)
                waitUntilGameUnpauses = new(() => !isGamePaused);
        }
        
        public void Win() => OnWon?.Invoke();

        public void Lose() => OnLost?.Invoke();
    }
}
