using UnityEngine;
using System;
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

        #region Events

        /// <summary>
        /// Event fired when the resource system has been initialized.
        /// </summary>
        public event Action OnResourceSystemInitialized;

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
        /// Called on the frame when a script is enabled.
        /// Initializes the resource system.
        /// </summary>
        private void Start()
        {
            Initialize();
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

        #region Initialization

        /// <summary>
        /// Initializes the resource system by loading all ResourceData assets from Resources/ResourceDefinitions folder.
        /// Registers each resource in the database and initializes global resource counts to zero.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("ResourceManager already initialized!");
                return;
            }

            // Load all ResourceData assets from Resources/ResourceDefinitions folder
            ResourceData[] resources = Resources.LoadAll<ResourceData>("ResourceDefinitions");

            Debug.Log($"Loading {resources.Length} resource definitions...");

            // Track duplicate detection
            HashSet<string> seenResourceIds = new HashSet<string>();

            foreach (var resource in resources)
            {
                // Validate resourceId is not empty
                if (string.IsNullOrWhiteSpace(resource.resourceId))
                {
                    Debug.LogError($"ResourceData {resource.name} has empty resourceId! Skipping.");
                    continue;
                }

                // Check for duplicate resourceId
                if (seenResourceIds.Contains(resource.resourceId))
                {
                    Debug.LogError($"Duplicate resourceId detected: {resource.resourceId}. Using first occurrence only.");
                    continue;
                }

                // Register resource in database
                _resourceDatabase[resource.resourceId] = resource;
                seenResourceIds.Add(resource.resourceId);

                // Initialize global resource count to zero
                _globalResourceCounts[resource.resourceId] = 0;
            }

            _isInitialized = true;
            Debug.Log($"ResourceManager initialized with {_resourceDatabase.Count} resources.");

            // Invoke initialization event
            OnResourceSystemInitialized?.Invoke();
        }

        #endregion
    }
}
