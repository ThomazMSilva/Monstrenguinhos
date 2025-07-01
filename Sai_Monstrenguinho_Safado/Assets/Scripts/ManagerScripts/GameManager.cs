using UnityEngine;
using Assets.Scripts.ManagerScripts;

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

        [Space(8f)]
        [SerializeField] private SceneLoader _sceneLoader;
        public SceneLoader SceneLoader => _sceneLoader;

        [Space(8f)]
        [SerializeField] private AudioManager _audioManager;
        public AudioManager AudioManager => _audioManager;
        
        [Space(8f)]
        [SerializeField] private bool isPaused;
        public bool IsPaused => isPaused;

        private void Awake() => InitializeReferences();

        private void InitializeReferences()
        {
            if (!InstanceInitializedCorrectly()) return;

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
            DontDestroyOnLoad(_Instance.transform.root.gameObject);
            return true;
        }

        [SerializeField] private GameObject optionsScreen;

        public void SetOptionsScreenActive(bool active) => optionsScreen.SetActive(active);

        public void LoadScene(string sceneName) => _sceneLoader.StartLoadingScene(sceneName);

        public void SetPause(bool pause)
        {
            isPaused = pause;
            OnPause?.Invoke(pause);
        }

        public delegate void PauseDelegate(bool pause);
        public event PauseDelegate OnPause;
    }


}