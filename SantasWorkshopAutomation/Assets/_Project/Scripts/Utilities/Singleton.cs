using UnityEngine;

namespace SantasWorkshop.Utilities
{
    /// <summary>
    /// Generic singleton base class for MonoBehaviour singletons.
    /// Ensures only one instance exists and persists across scenes if needed.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// Gets the singleton instance. Creates one if it doesn't exist.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject($"{typeof(T).Name} (Singleton)");
                            _instance = singletonObject.AddComponent<T>();
                            Debug.Log($"[Singleton] Created new instance of {typeof(T)}");
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Called when the instance is created. Override to add initialization logic.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T)} detected. Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Called when the application quits to prevent singleton recreation.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        /// <summary>
        /// Called when the instance is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
            }
        }
    }
}
