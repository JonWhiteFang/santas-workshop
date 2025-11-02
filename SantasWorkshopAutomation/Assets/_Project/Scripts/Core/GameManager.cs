using UnityEngine;
using UnityEngine.SceneManagement;
using SantasWorkshop.Utilities;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Game state enumeration for managing different game states.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused
    }

    /// <summary>
    /// Central game manager responsible for game state and scene management.
    /// Persists across scene loads using DontDestroyOnLoad.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        #region Fields

        [Header("Game State")]
        [SerializeField] private GameState _currentState = GameState.MainMenu;

        [Header("Scene Names")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";
        [SerializeField] private string _workshopSceneName = "Workshop";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current game state.
        /// </summary>
        public GameState CurrentState => _currentState;

        /// <summary>
        /// Gets whether the game is currently paused.
        /// </summary>
        public bool IsPaused => _currentState == GameState.Paused;

        /// <summary>
        /// Gets whether the game is currently playing.
        /// </summary>
        public bool IsPlaying => _currentState == GameState.Playing;

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the game state changes.
        /// </summary>
        public event System.Action<GameState> OnGameStateChanged;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            // Persist across scene loads
            if (Instance == this)
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log("[GameManager] Initialized and set to persist across scenes");
            }
        }

        private void Start()
        {
            // Initialize game state based on current scene
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == _workshopSceneName)
            {
                SetGameState(GameState.Playing);
            }
            else if (currentScene == _mainMenuSceneName)
            {
                SetGameState(GameState.MainMenu);
            }
        }

        private void Update()
        {
            // Handle pause input (ESC key)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_currentState == GameState.Playing)
                {
                    PauseGame();
                }
                else if (_currentState == GameState.Paused)
                {
                    ResumeGame();
                }
            }
        }

        #endregion

        #region Game State Management

        /// <summary>
        /// Sets the current game state and triggers the state changed event.
        /// </summary>
        /// <param name="newState">The new game state</param>
        public void SetGameState(GameState newState)
        {
            if (_currentState == newState)
            {
                return;
            }

            GameState previousState = _currentState;
            _currentState = newState;

            Debug.Log($"[GameManager] Game state changed: {previousState} -> {newState}");

            // Update time scale based on state
            Time.timeScale = _currentState == GameState.Paused ? 0f : 1f;

            // Trigger event
            OnGameStateChanged?.Invoke(_currentState);
        }

        /// <summary>
        /// Pauses the game.
        /// </summary>
        public void PauseGame()
        {
            if (_currentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
            }
        }

        /// <summary>
        /// Resumes the game from pause.
        /// </summary>
        public void ResumeGame()
        {
            if (_currentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
            }
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// Loads the main menu scene.
        /// </summary>
        public void LoadMainMenu()
        {
            Debug.Log("[GameManager] Loading Main Menu scene");
            SetGameState(GameState.MainMenu);
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        /// <summary>
        /// Loads the workshop scene and starts gameplay.
        /// </summary>
        public void LoadWorkshop()
        {
            Debug.Log("[GameManager] Loading Workshop scene");
            SetGameState(GameState.Playing);
            SceneManager.LoadScene(_workshopSceneName);
        }

        /// <summary>
        /// Loads a scene by name.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load</param>
        public void LoadScene(string sceneName)
        {
            Debug.Log($"[GameManager] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Loads a scene asynchronously.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load</param>
        public AsyncOperation LoadSceneAsync(string sceneName)
        {
            Debug.Log($"[GameManager] Loading scene asynchronously: {sceneName}");
            return SceneManager.LoadSceneAsync(sceneName);
        }

        #endregion

        #region Application Management

        /// <summary>
        /// Quits the application.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[GameManager] Quitting application");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}
