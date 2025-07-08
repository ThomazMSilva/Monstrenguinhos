using UnityEngine;
using Assets.Scripts.ManagerScripts;
using Assets.Scripts.NPCScripts;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts
{
    [System.Serializable]
    public class StageAttributes
    {
        public string stageName;
        public int stageID;
        public bool isTutorial;
        public bool isFinal;

        [System.Serializable]
        public class NpcAttributes
        {
            [Space(8f)]

            public List<ClientAttributes> SpawnableClients = new(3);

            [Space(8f), Header("Spawn"), Space(5f)]

            public bool PreSpawn = true;
            public float MinInterval = 50f;
            public float MaxInterval = 60f;
            private float currentInterval;
            public float Interval{ get => currentInterval; set { currentInterval = value; } }
            private float timeRemaining;
            public float TimeRemaining{ get => timeRemaining; set { timeRemaining = value; } }
            
            private float timeWaitedWithNoClients;
            public float TimeWaitedWithNoClients { get => timeWaitedWithNoClients; set => timeWaitedWithNoClients = value; }
            private float timeWaitedWithClients;
            public float TimeWaitedWithClients { get => timeWaitedWithClients; set => timeWaitedWithClients = value; }
            
            private float timeTakenFromNextStage;
            public float TimeTakenFromNextStage { get => timeTakenFromNextStage; set => timeTakenFromNextStage = value; }

            public int SpawnCap = 4;
            private int spawnedAmount;
            public int SpawnedAmount { get => spawnedAmount; set => spawnedAmount = value; }
            private int clientsCurrentlyActive;
            public int ClientsRemaining { get => clientsCurrentlyActive; set => clientsCurrentlyActive = value; }
            public List<float> serviceTime;
            public float AverageServiceTime;

            public float AverageTime() => serviceTime.Average();
        }

        [System.Serializable]
        public class EnemyAttributes
        {
            public List<GameObject> SpawnableCrittlings;
            [Space(8f), Header("Spawn"), Space(5f)]

            public float MinInterval = 15f;
            public float MaxInterval = 20f;
            private float currentInterval;
            public float Interval { get => currentInterval; set { currentInterval = value; } }
            private float timeRemaining;
            public float TimeRemaining { get => timeRemaining; set { timeRemaining = value; } }
            
            private float timeWaitedBetweenSpawns;
            public float TimeWaitedBetweenSpawns { get => timeWaitedBetweenSpawns; set => timeWaitedBetweenSpawns = value; }

            public int MinHorde = 1;
            public int MaxHorde = 2;
            public int SpawnCap = 6;
            private int spawnedAmount;
            public int SpawnedAmount { get => spawnedAmount; set => spawnedAmount = value; }
        }

        [System.Serializable]
        public class StageConditions
        {
            [Space(8f), Header("Condições de Passagem"), Space(8f)]

            public bool TimeBased;
            [Tooltip("Em segundos")]
            public float TimeToPass = 300;

            [Space(8f)]

            public bool SuccessBased = true;
            public int SuccessfulClientsToPass = 1;

            [Space(8f), Header("Estado da Condição"), Space(8f)]
            private float timeElapsed;
            public float TimeElapsed { get => timeElapsed; set { timeElapsed = value; } }
            private int successfulClientsPassed;
            public int SuccessfulClientsPassed { get => successfulClientsPassed; set { successfulClientsPassed = value; } }
            private int failedClientsPassed;
            public int FailedClientsPassed { get => failedClientsPassed; set { failedClientsPassed = value; } }
        }

        public NpcAttributes Clients;
        public EnemyAttributes Crittlings;
        public StageConditions Conditions;

        private float stageTime;
        public float StageTime { get => stageTime; set => stageTime = value; }

        [Space(8f)]

        public int nextStageID;
        public UnityEngine.Events.UnityEvent OnCompleted;

        public void Reset()
        {
            Conditions.TimeElapsed = 0;
            Conditions.SuccessfulClientsPassed = 0;
            Conditions.FailedClientsPassed = 0;
    }
    }

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
        [SerializeField] private List<StageAttributes> stageAttributes = new(1);
        public List<StageAttributes> StageAttributes => stageAttributes;

        private StageAttributes currentStageAttributes;
        public StageAttributes CurrentStage => currentStageAttributes;

        public StageAttributes LastStage => stageAttributes.FirstOrDefault(s => s.isFinal);

        public void PassToStage(int stageID)
        {
            if (currentStageAttributes != null && stageID == currentStageAttributes.stageID) return;
            
            StageAttributes stageToGo = stageAttributes.FirstOrDefault(s => s.stageID == stageID);

            if (stageToGo == null) return;
         
            currentStageAttributes = stageToGo;
            Debug.Log("Passou pra estagio "+stageID);
            OnStagePassed?.Invoke();
        }

        private void Awake() => InitializeReferences();

        private void InitializeReferences()
        {
            if (!InstanceInitializedCorrectly()) return;

            int firstStage = stageAttributes.Min(s => s.stageID);
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
            foreach(var stage in stageAttributes)
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