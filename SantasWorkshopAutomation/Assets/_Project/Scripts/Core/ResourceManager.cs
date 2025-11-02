using UnityEngine;
using System.Collections.Generic;
using SantasWorkshop.Data;
using SantasWorkshop.Utilities;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Manages all resources in the game including tracking, extraction, and consumption.
    /// Singleton manager that persists across scenes.
    /// </summary>
    public class ResourceManager : Singleton<ResourceManager>
    {
        #region Fields

        [Header("Resource Database")]
        [SerializeField] private List<ResourceData> initialResources = new List<ResourceData>();

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        private Dictionary<string, ResourceData> resourceDatabase;
        private Dictionary<string, long> globalResourceCounts;

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when a resource count changes.
        /// Parameters: resourceId, newCount
        /// </summary>
        public event System.Action<string, long> OnResourceChanged;

        /// <summary>
        /// Event triggered when a new resource type is registered.
        /// Parameters: resourceId
        /// </summary>
        public event System.Action<string> OnResourceRegistered;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            if (Instance == this)
            {
                DontDestroyOnLoad(gameObject);
                InitializeResourceSystem();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the resource management system.
        /// </summary>
        private void InitializeResourceSystem()
        {
            resourceDatabase = new Dictionary<string, ResourceData>();
            globalResourceCounts = new Dictionary<string, long>();

            // Register initial resources
            foreach (var resource in initialResources)
            {
                if (resource != null)
                {
                    RegisterResource(resource);
                }
            }

            if (showDebugInfo)
            {
                Debug.Log($"[ResourceManager] Initialized with {resourceDatabase.Count} resource types");
            }
        }

        #endregion

        #region Resource Registration

        /// <summary>
        /// Registers a new resource type in the database.
        /// </summary>
        /// <param name="resource">The resource data to register</param>
        public void RegisterResource(ResourceData resource)
        {
            if (resource == null)
            {
                Debug.LogWarning("[ResourceManager] Attempted to register null resource");
                return;
            }

            if (resourceDatabase.ContainsKey(resource.resourceId))
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[ResourceManager] Resource already registered: {resource.resourceId}");
                }
                return;
            }

            resourceDatabase[resource.resourceId] = resource;

            // Initialize count to zero if not already present
            if (!globalResourceCounts.ContainsKey(resource.resourceId))
            {
                globalResourceCounts[resource.resourceId] = 0;
            }

            OnResourceRegistered?.Invoke(resource.resourceId);

            if (showDebugInfo)
            {
                Debug.Log($"[ResourceManager] Registered resource: {resource.resourceId}");
            }
        }

        /// <summary>
        /// Gets resource data by ID.
        /// </summary>
        public ResourceData GetResourceData(string resourceId)
        {
            return resourceDatabase.TryGetValue(resourceId, out ResourceData data) ? data : null;
        }

        #endregion

        #region Resource Management

        /// <summary>
        /// Adds resources to the global resource pool.
        /// </summary>
        /// <param name="resources">Array of resource stacks to add</param>
        public void AddResources(ResourceStack[] resources)
        {
            if (resources == null || resources.Length == 0)
                return;

            foreach (var stack in resources)
            {
                AddResource(stack.resourceId, stack.amount);
            }
        }

        /// <summary>
        /// Adds a specific amount of a resource to the global pool.
        /// </summary>
        /// <param name="resourceId">The resource identifier</param>
        /// <param name="amount">The amount to add</param>
        public void AddResource(string resourceId, int amount)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                Debug.LogWarning("[ResourceManager] Attempted to add resource with null/empty ID");
                return;
            }

            if (amount <= 0)
                return;

            if (!globalResourceCounts.ContainsKey(resourceId))
            {
                globalResourceCounts[resourceId] = 0;
            }

            globalResourceCounts[resourceId] += amount;

            OnResourceChanged?.Invoke(resourceId, globalResourceCounts[resourceId]);

            if (showDebugInfo)
            {
                Debug.Log($"[ResourceManager] Added {amount} of {resourceId}. New total: {globalResourceCounts[resourceId]}");
            }
        }

        /// <summary>
        /// Attempts to consume resources from the global pool.
        /// Returns true if successful, false if insufficient resources.
        /// </summary>
        /// <param name="resources">Array of resource stacks to consume</param>
        /// <returns>True if all resources were consumed, false otherwise</returns>
        public bool TryConsumeResources(ResourceStack[] resources)
        {
            if (resources == null || resources.Length == 0)
                return true;

            // First, check if we have enough of all resources
            foreach (var stack in resources)
            {
                if (!HasResource(stack.resourceId, stack.amount))
                {
                    if (showDebugInfo)
                    {
                        Debug.LogWarning($"[ResourceManager] Insufficient resources: {stack.resourceId} (need {stack.amount}, have {GetResourceCount(stack.resourceId)})");
                    }
                    return false;
                }
            }

            // If we have enough of everything, consume them
            foreach (var stack in resources)
            {
                ConsumeResource(stack.resourceId, stack.amount);
            }

            return true;
        }

        /// <summary>
        /// Consumes a specific amount of a resource from the global pool.
        /// Does not check if sufficient resources exist - use TryConsumeResources for safe consumption.
        /// </summary>
        private void ConsumeResource(string resourceId, int amount)
        {
            if (!globalResourceCounts.ContainsKey(resourceId))
            {
                Debug.LogError($"[ResourceManager] Attempted to consume untracked resource: {resourceId}");
                return;
            }

            globalResourceCounts[resourceId] -= amount;

            // Prevent negative counts
            if (globalResourceCounts[resourceId] < 0)
            {
                Debug.LogWarning($"[ResourceManager] Resource count went negative for {resourceId}. Clamping to 0.");
                globalResourceCounts[resourceId] = 0;
            }

            OnResourceChanged?.Invoke(resourceId, globalResourceCounts[resourceId]);

            if (showDebugInfo)
            {
                Debug.Log($"[ResourceManager] Consumed {amount} of {resourceId}. New total: {globalResourceCounts[resourceId]}");
            }
        }

        /// <summary>
        /// Checks if the global pool has at least the specified amount of a resource.
        /// </summary>
        /// <param name="resourceId">The resource identifier</param>
        /// <param name="amount">The amount to check for</param>
        /// <returns>True if sufficient resources exist, false otherwise</returns>
        public bool HasResource(string resourceId, int amount)
        {
            if (!globalResourceCounts.TryGetValue(resourceId, out long count))
            {
                return false;
            }

            return count >= amount;
        }

        /// <summary>
        /// Gets the current count of a specific resource.
        /// </summary>
        /// <param name="resourceId">The resource identifier</param>
        /// <returns>The current count, or 0 if the resource doesn't exist</returns>
        public long GetResourceCount(string resourceId)
        {
            return globalResourceCounts.TryGetValue(resourceId, out long count) ? count : 0;
        }

        /// <summary>
        /// Sets the count of a specific resource directly.
        /// Use with caution - prefer AddResource and TryConsumeResources for normal operations.
        /// </summary>
        public void SetResourceCount(string resourceId, long count)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                Debug.LogWarning("[ResourceManager] Attempted to set resource count with null/empty ID");
                return;
            }

            globalResourceCounts[resourceId] = count;
            OnResourceChanged?.Invoke(resourceId, count);

            if (showDebugInfo)
            {
                Debug.Log($"[ResourceManager] Set {resourceId} count to {count}");
            }
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Gets all resource IDs currently tracked.
        /// </summary>
        public IEnumerable<string> GetAllResourceIds()
        {
            return globalResourceCounts.Keys;
        }

        /// <summary>
        /// Gets the total number of different resource types being tracked.
        /// </summary>
        public int GetResourceTypeCount()
        {
            return globalResourceCounts.Count;
        }

        /// <summary>
        /// Gets a copy of all resource counts.
        /// </summary>
        public Dictionary<string, long> GetAllResourceCounts()
        {
            return new Dictionary<string, long>(globalResourceCounts);
        }

        #endregion

        #region Save/Load Support

        /// <summary>
        /// Clears all resource counts. Used when loading a save or starting a new game.
        /// </summary>
        public void ClearAllResources()
        {
            globalResourceCounts.Clear();

            if (showDebugInfo)
            {
                Debug.Log("[ResourceManager] All resources cleared");
            }
        }

        /// <summary>
        /// Loads resource counts from a dictionary. Used when loading a save.
        /// </summary>
        public void LoadResourceCounts(Dictionary<string, long> resourceCounts)
        {
            if (resourceCounts == null)
            {
                Debug.LogWarning("[ResourceManager] Attempted to load null resource counts");
                return;
            }

            globalResourceCounts = new Dictionary<string, long>(resourceCounts);

            if (showDebugInfo)
            {
                Debug.Log($"[ResourceManager] Loaded {globalResourceCounts.Count} resource counts");
            }

            // Trigger events for all loaded resources
            foreach (var kvp in globalResourceCounts)
            {
                OnResourceChanged?.Invoke(kvp.Key, kvp.Value);
            }
        }

        #endregion
    }
}
