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
        private WaitUntil waitUntilGameUnpauses;
        private Coroutine stageTimerRoutine;
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

        private void SpawnClient()
        {
            var client = Instantiate(npcPrefab, transform.position, transform.rotation, npcParent).GetComponent<NPCBehaviour>();
            spawnedNPCs.Add(client);
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
            
            client.onDestroy.AddListener(RemoveClientFromList);
            
            var current = game.CurrentStage;

            client.OnFailed.AddListener
            (
                () => 
                {
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
                    audioManager.PlayClip(successClip, client.transform);
                    current.Conditions.SuccessfulClientsPassed++;
                    OnSucceededClient?.Invoke();
                    //Passagem de estagio na condição de "por sucesso"
                    if(current.Conditions.SuccessBased && current.Conditions.SuccessfulClientsPassed >= current.Conditions.SuccessfulClientsToPass)
                    {
                        current.OnCompleted?.Invoke();
                        //Tirar daqui se não quiser automático
                        game.PassToStage(current.nextStageID);
                    }
                }
            );
        }

        private void RemoveClientFromList(NPCBehaviour client)
        {
            spawnedNPCs.Remove(client);
            foreach(var kvp in positionsOccupationDict)
            {
                if (kvp.Value != client) continue;
                
                positionsOccupationDict[kvp.Key] = null;
                break;
            }
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                if(isGamePaused) yield return waitUntilGameUnpauses;

                while (spawnedNPCs.Count >= game.CurrentStage.Clients.SpawnCap)
                {
                    if (isGamePaused) yield return waitUntilGameUnpauses;
                    yield return waitForCapCheck;
                }

                game.CurrentStage.Clients.Interval = Random.Range(game.CurrentStage.Clients.MinInterval, game.CurrentStage.Clients.MaxInterval);

                game.CurrentStage.Clients.TimeRemaining = game.CurrentStage.Clients.Interval;

                while (game.CurrentStage.Clients.TimeRemaining > 0)
                {
                    if (isGamePaused) yield return waitUntilGameUnpauses;
                    game.CurrentStage.Clients.TimeRemaining -= Time.deltaTime;
                    yield return null;
                }

                SpawnClient();
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

        private void PauseBehaviour(bool isPaused)
        {
            isGamePaused = isPaused;
            if(isPaused)
                waitUntilGameUnpauses = new(() => !isGamePaused);
        }
        
        private void Restart()
        {
            ReturnAllClients(null);
        }
        
        private void CheckStartStageTimer()
        {
            if (stageTimerRoutine != null) StopCoroutine(stageTimerRoutine);

            if(game.CurrentStage.Conditions.TimeBased)
                stageTimerRoutine = StartCoroutine(StageTimer());
        }

        private IEnumerator StageTimer()
        {
            game.CurrentStage.Conditions.TimeElapsed = 0;
            
            while (game.CurrentStage.Conditions.TimeElapsed < game.CurrentStage.Conditions.TimeToPass)
            {
                if (isGamePaused) yield return waitUntilGameUnpauses;

                game.CurrentStage.Conditions.TimeElapsed += Time.deltaTime;
                yield return null;
            }
            Debug.Log("Acabou o tempo do "+game.CurrentStage.stageName);
            game.PassToStage(game.CurrentStage.nextStageID);
            stageTimerRoutine = null;
        }

        public void Win() => OnWon?.Invoke();

        public void Lose() => OnLost?.Invoke();

        public void OnStagePassed()
        {
            CheckStartStageTimer();
            game.CurrentStage.Clients.TimeRemaining = game.CurrentStage.Clients.Interval;
            
            /*if(game.CurrentStage.Clients.PreSpawn)
                SpawnClient();*/
        }

        private void Start()
        {
            if (!TryInitializeTargetPositions()) return;

            CheckStartStageTimer();

            game.OnPause += PauseBehaviour;
            game.OnStagePassed += OnStagePassed;
            game.OnRestart += Restart;
            game.LastStage.OnCompleted.AddListener(Win);

            waitForCapCheck = new(capCheckInterval);

            StartCoroutine(SpawnRoutine());
        }

        private void OnDisable()
        {
            game.OnPause -= PauseBehaviour;
            game.OnStagePassed -= OnStagePassed;
            game.OnRestart -= Restart;
            game.LastStage.OnCompleted.RemoveListener(Win);
        }
    }
}
