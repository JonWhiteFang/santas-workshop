using UnityEngine;
using System.Collections.Generic;
using SantasWorkshop.Data;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Manages all resources in the game including tracking, extraction, and consumption.
    /// Singleton manager that persists across scenes.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        #region Singleton

        /// <summary>
        /// Singleton instance of the ResourceManager.
        /// </summary>
        public static ResourceManager Instance { get; private set; }

        #endregion

        #region Private Fields

        // Resource database (resourceId -> ResourceData)
        private Dictionary<string, ResourceData> _resourceDatabase;

        // Global resource counts (resourceId -> count)
        private Dictionary<string, long> _globalResourceCounts;

        // Resource capacity limits (resourceId -> capacity)
        private Dictionary<string, long> _resourceCapacities;

        // Initialization flag
        private bool _isInitialized = false;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Implements singleton pattern and ensures persistence across scenes.
        /// </summary>
        private void Awake()
        {
            // Singleton pattern implementation
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize collections
            _resourceDatabase = new Dictionary<string, ResourceData>();
            _globalResourceCounts = new Dictionary<string, long>();
            _resourceCapacities = new Dictionary<string, long>();
        }

        /// <summary>
        /// Called when the MonoBehaviour will be destroyed.
        /// Cleans up the singleton instance reference.
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion
    }
}
