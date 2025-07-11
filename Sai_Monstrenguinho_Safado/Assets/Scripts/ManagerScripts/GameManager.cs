using UnityEngine;
using Assets.Scripts.ManagerScripts;
using Assets.Scripts.NPCScripts;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        private static readonly string prefabPath = "Prefabs/GameManager";

        private static GameManager _Instance;
        public static GameManager Instance
        {
            get
            {
                if (!_Instance)
                {
                    var prefab = Resources.Load<GameObject>(prefabPath);

                    var inScene = prefab ? Instantiate(prefab) : new GameObject("GameManager");

                    _Instance = inScene.GetComponentInChildren<GameManager>();
                    if (!_Instance) _Instance = inScene.AddComponent<GameManager>();

                    DontDestroyOnLoad(_Instance.transform.root.gameObject);
                }
                return _Instance;
            }
        }

        [SerializeField] private UINavigationManager uiManager;

        [Space(8f)]
        [SerializeField] private SceneLoader _sceneLoader;
        public SceneLoader SceneLoader => _sceneLoader;

        [Space(8f)]
        [SerializeField] private AudioManager _audioManager;
        public AudioManager AudioManager => _audioManager;


        [Space(8f)]
        [SerializeField] private bool isPaused;
        public bool IsPaused => isPaused;

        public bool IsLoading;

        [Space(8f)]
        [SerializeField] private StageSO stage;

        [SerializeField] private List<StageAttributes> stageAttributes;
        private List<StageAttributes> _stageAttributes;
        public List<StageAttributes> StageAttributes => _stageAttributes;

        private StageAttributes currentStageAttributes;
        public StageAttributes CurrentStage => currentStageAttributes;

        public StageAttributes LastStage => _stageAttributes.FirstOrDefault(s => s.isFinal);

        public void PassToStage(int stageID)
        {
            if (currentStageAttributes != null && stageID == currentStageAttributes.stageID) return;
            
            StageAttributes stageToGo = _stageAttributes.FirstOrDefault(s => s.stageID == stageID);

            if (stageToGo == null) return;
         
            currentStageAttributes = stageToGo;
            Debug.Log("Passou pra estagio "+stageID);
            OnStagePassed?.Invoke();
        }

        private void Awake() => InitializeReferences();

        private void InitializeReferences()
        {
            if (!InstanceInitializedCorrectly()) return;

            CloneStageAttributes();


            int firstStage = _stageAttributes.Min(s => s.stageID);
            PassToStage(firstStage);

            _audioManager.Initialize(this);
            _sceneLoader.Initialize(this);
        }

        private bool InstanceInitializedCorrectly()
        {
            if (_Instance != null)
            {
                Debug.LogError("Já tinha um GameManager na cena. Não continuando inicialização de " + _Instance.transform.root.gameObject);
                Destroy(gameObject);
                //return false;
            }
            _Instance = this;
            Debug.Log("inicializou instancia de gm");
            DontDestroyOnLoad(_Instance.transform.root.gameObject);
            return true;
        }

        private void CloneStageAttributes()
        {
            if (stage != null && stage.attributes != null)
            {
                _stageAttributes = new List<StageAttributes>();
                foreach (var original in stage.attributes)
                {
                    StageAttributes copy = new()
                    {
                        stageName = original.stageName,
                        stageID = original.stageID,
                        isTutorial = original.isTutorial,
                        isFinal = original.isFinal,
                        StageTime = original.StageTime,
                        nextStageID = original.nextStageID,
                        OnCompleted = original.OnCompleted
                    };

                    if (original.Clients != null)
                    {
                        copy.Clients = new StageAttributes.NpcAttributes()
                        {
                            SpawnableClients = new List<ClientAttributes>(original.Clients.SpawnableClients),
                            PreSpawn = original.Clients.PreSpawn,
                            MinInterval = original.Clients.MinInterval,
                            MaxInterval = original.Clients.MaxInterval,
                            Interval = original.Clients.Interval,
                            TimeRemaining = original.Clients.TimeRemaining,
                            ForceClientOnScreen = original.Clients.ForceClientOnScreen,
                            waitToForce = original.Clients.waitToForce,
                            waitForStageToEndBeforeSpawning = original.Clients.waitForStageToEndBeforeSpawning,
                            CurrentTimeWaitedWithNoClients = original.Clients.CurrentTimeWaitedWithNoClients,
                            TotalTimeWaitedWithNoClients = original.Clients.TotalTimeWaitedWithNoClients,
                            TotalTimeWaitedWithClients = original.Clients.TotalTimeWaitedWithClients,
                            TimeTakenFromNextStage = original.Clients.TimeTakenFromNextStage,
                            SpawnCap = original.Clients.SpawnCap,
                            SpawnedAmount = original.Clients.SpawnedAmount,
                            ClientsRemaining = original.Clients.ClientsRemaining,
                            ServiceTime = original.Clients.ServiceTime != null ? new List<float>(original.Clients.ServiceTime) : new List<float>(),
                            AverageServiceTime = original.Clients.AverageServiceTime
                        };
                    }

                    if (original.Crittlings != null)
                    {
                        copy.Crittlings = new StageAttributes.EnemyAttributes()
                        {
                            SpawnableCrittlings = original.Crittlings.SpawnableCrittlings != null ? new List<GameObject>(original.Crittlings.SpawnableCrittlings) : new List<GameObject>(),
                            MinInterval = original.Crittlings.MinInterval,
                            MaxInterval = original.Crittlings.MaxInterval,
                            Interval = original.Crittlings.Interval,
                            TimeRemaining = original.Crittlings.TimeRemaining,
                            TimeWaitedBetweenSpawns = original.Crittlings.TimeWaitedBetweenSpawns,
                            MinHorde = original.Crittlings.MinHorde,
                            MaxHorde = original.Crittlings.MaxHorde,
                            SpawnCap = original.Crittlings.SpawnCap,
                            SpawnedAmount = original.Crittlings.SpawnedAmount
                        };
                    }

                    if (original.Conditions != null)
                    {
                        copy.Conditions = new StageAttributes.StageConditions()
                        {
                            TimeBased = original.Conditions.TimeBased,
                            TimeToPass = original.Conditions.TimeToPass,
                            SuccessBased = original.Conditions.SuccessBased,
                            SuccessfulClientsToPass = original.Conditions.SuccessfulClientsToPass,
                            TimeElapsed = original.Conditions.TimeElapsed,
                            SuccessfulClientsPassed = original.Conditions.SuccessfulClientsPassed,
                            FailedClientsPassed = original.Conditions.FailedClientsPassed
                        };
                    }

                    _stageAttributes.Add(copy);
                }
            }
        }

        public void QuitGame() => Application.Quit();

        public void LoadScene(string sceneName)
        {
            _sceneLoader.StartLoadingScene(sceneName);
        }

        public void SetPause(bool pause)
        {
            isPaused = pause;
            OnPause?.Invoke(pause);
        }

        public void Restart()
        {
            foreach(var stage in _stageAttributes)
            {
                stage.Reset();
            }
            PassToStage(0);
            OnRestart?.Invoke();
        }

        private Coroutine fadeRoutine;
        public void Fade(CanvasGroup uiElement)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                Debug.Log("encerrando rotina de fade no GameGamanger");
            }

            bool active = uiElement.gameObject.activeSelf;

            System.Collections.IEnumerator routine = SceneLoader.FadeScreen(uiElement, active ? 0 : 1, SceneLoader.FadeTime, !active);
            
            fadeRoutine = StartCoroutine(routine);
        }

        public delegate void PauseDelegate(bool pause);
        public event PauseDelegate OnPause;
        public delegate void ConditionVoidDelegate();
        public event ConditionVoidDelegate OnStagePassed;
        public event ConditionVoidDelegate OnRestart;

        private void OnApplicationQuit() => Destroy(gameObject);
    }


}