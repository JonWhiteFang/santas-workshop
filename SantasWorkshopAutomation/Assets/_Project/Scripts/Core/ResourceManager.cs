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

        /// <summary>
        /// Event fired when a resource count changes.
        /// Parameters: resourceId, newAmount
        /// </summary>
        public event Action<string, long> OnResourceChanged;

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

        #region Add Resources

        /// <summary>
        /// Adds a specified amount of a resource to the global inventory.
        /// Respects capacity limits if set.
        /// </summary>
        /// <param name="resourceId">The unique identifier of the resource</param>
        /// <param name="amount">The amount to add (must be positive)</param>
        /// <returns>True if the resource was added successfully, false otherwise</returns>
        public bool AddResource(string resourceId, int amount)
        {
            // Validate the operation
            if (!ValidateResourceOperation(resourceId, amount))
            {
                return false;
            }

            // Get current count and capacity
            long currentCount = GetResourceCount(resourceId);
            long capacity = GetResourceCapacity(resourceId);
            long newCount = currentCount + amount;

            // Check capacity limit (0 means unlimited)
            if (capacity > 0 && newCount > capacity)
            {
                // Calculate how much we can actually add
                long actualAdded = capacity - currentCount;
                
                if (actualAdded <= 0)
                {
                    Debug.LogWarning($"Cannot add {amount} {resourceId}: at capacity ({capacity})");
                    return false;
                }

                // Add only up to capacity
                _globalResourceCounts[resourceId] = capacity;
                OnResourceChanged?.Invoke(resourceId, capacity);
                Debug.LogWarning($"Added only {actualAdded} {resourceId} (capacity limit reached)");
                return true;
            }

            // No capacity limit or within limit - add full amount
            _globalResourceCounts[resourceId] = newCount;
            OnResourceChanged?.Invoke(resourceId, newCount);
            return true;
        }

        /// <summary>
        /// Adds multiple resources to the global inventory.
        /// Processes each resource individually.
        /// </summary>
        /// <param name="resources">Array of resource stacks to add</param>
        public void AddResources(ResourceStack[] resources)
        {
            // Validate input
            if (resources == null || resources.Length == 0)
            {
                Debug.LogWarning("AddResources called with null or empty array!");
                return;
            }

            // Add each resource
            foreach (var stack in resources)
            {
                AddResource(stack.resourceId, stack.amount);
            }
        }

        #endregion

        #region Consume Resources

        /// <summary>
        /// Attempts to consume a specified amount of a resource from the global inventory.
        /// </summary>
        /// <param name="resourceId">The unique identifier of the resource</param>
        /// <param name="amount">The amount to consume (must be positive)</param>
        /// <returns>True if the resource was consumed successfully, false if insufficient resources</returns>
        public bool TryConsumeResource(string resourceId, int amount)
        {
            // Validate the operation
            if (!ValidateResourceOperation(resourceId, amount))
            {
                return false;
            }

            // Check if sufficient resources are available
            long currentCount = GetResourceCount(resourceId);
            
            if (currentCount < amount)
            {
                // Insufficient resources - return false without logging error
                return false;
            }

            // Sufficient resources available - consume them
            long newCount = currentCount - amount;
            _globalResourceCounts[resourceId] = newCount;
            
            // Invoke resource changed event
            OnResourceChanged?.Invoke(resourceId, newCount);
            
            return true;
        }

        /// <summary>
        /// Attempts to consume multiple resources from the global inventory atomically.
        /// All resources must be available or none will be consumed.
        /// </summary>
        /// <param name="resources">Array of resource stacks to consume</param>
        /// <returns>True if all resources were consumed successfully, false if any resource is insufficient</returns>
        public bool TryConsumeResources(ResourceStack[] resources)
        {
            // Validate input
            if (resources == null || resources.Length == 0)
            {
                Debug.LogWarning("TryConsumeResources called with null or empty array!");
                return false;
            }

            // First pass: Validate ALL resources are available (atomic check)
            foreach (var stack in resources)
            {
                // Validate the stack
                if (!ValidateResourceOperation(stack.resourceId, stack.amount))
                {
                    return false;
                }

                // Check if sufficient resources available
                long currentCount = GetResourceCount(stack.resourceId);
                if (currentCount < stack.amount)
                {
                    // Insufficient resources - abort entire operation
                    return false;
                }
            }

            // Second pass: All resources are available, consume them
            foreach (var stack in resources)
            {
                TryConsumeResource(stack.resourceId, stack.amount);
            }

            return true;
        }

        #endregion

        #region Transfer Resources

        /// <summary>
        /// Transfers a specified amount of a resource between storage locations.
        /// For initial implementation, operates on global inventory (sourceId/targetId reserved for future use).
        /// </summary>
        /// <param name="sourceId">The source storage identifier (reserved for future use)</param>
        /// <param name="targetId">The target storage identifier (reserved for future use)</param>
        /// <param name="resourceId">The unique identifier of the resource to transfer</param>
        /// <param name="amount">The amount to transfer</param>
        /// <returns>True if the transfer was successful, false otherwise</returns>
        public bool TransferResource(string sourceId, string targetId, string resourceId, int amount)
        {
            // For initial implementation, operate on global inventory
            // Future: Support named storage locations using sourceId/targetId

            // Validate resource availability
            if (!HasResource(resourceId, amount))
            {
                return false;
            }

            // Attempt to consume from source (global inventory)
            if (!TryConsumeResource(resourceId, amount))
            {
                return false;
            }

            // Add to target (global inventory)
            AddResource(resourceId, amount);

            return true;
        }

        #endregion

        #region Query Resources

        /// <summary>
        /// Checks if a specified amount of a resource is available in the global inventory.
        /// </summary>
        /// <param name="resourceId">The unique identifier of the resource</param>
        /// <param name="amount">The amount to check for</param>
        /// <returns>True if the resource is available in the specified amount, false otherwise</returns>
        public bool HasResource(string resourceId, int amount)
        {
            // Validate resourceId
            if (string.IsNullOrEmpty(resourceId))
            {
                return false;
            }

            // Validate amount is not negative
            if (amount < 0)
            {
                return false;
            }

            // Check if resource exists in database
            if (!_resourceDatabase.ContainsKey(resourceId))
            {
                return false;
            }

            // Check if sufficient resources available
            return GetResourceCount(resourceId) >= amount;
        }

        /// <summary>
        /// Gets the current count of a resource in the global inventory.
        /// </summary>
        /// <param name="resourceId">The unique identifier of the resource</param>
        /// <returns>The current count, or 0 if the resource doesn't exist</returns>
        public long GetResourceCount(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                return 0;
            }

            return _globalResourceCounts.TryGetValue(resourceId, out long count) ? count : 0;
        }

        /// <summary>
        /// Gets the ResourceData ScriptableObject for a specified resource.
        /// </summary>
        /// <param name="resourceId">The unique identifier of the resource</param>
        /// <returns>The ResourceData object, or null if the resource doesn't exist</returns>
        public ResourceData GetResourceData(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                return null;
            }

            return _resourceDatabase.TryGetValue(resourceId, out ResourceData data) ? data : null;
        }

        /// <summary>
        /// Gets all registered resources in the system.
        /// </summary>
        /// <returns>Collection of all ResourceData objects</returns>
        public IEnumerable<ResourceData> GetAllResources()
        {
            return _resourceDatabase.Values;
        }

        /// <summary>
        /// Gets all resources of a specific category.
        /// </summary>
        /// <param name="category">The resource category to filter by</param>
        /// <returns>Collection of ResourceData objects matching the category</returns>
        public IEnumerable<ResourceData> GetResourcesByCategory(ResourceCategory category)
        {
            foreach (var resource in _resourceDatabase.Values)
            {
                if (resource.category == category)
                {
                    yield return resource;
                }
            }
        }

        #endregion

        #region Capacity Management

        /// <summary>
        /// Sets the capacity limit for a resource.
        /// A capacity of 0 means unlimited.
        /// </summary>
        /// <param name="resourceId">The unique identifier of the resource</param>
        /// <param name="capacity">The capacity limit (0 for unlimited)</param>
        public void SetResourceCapacity(string resourceId, long capacity)
        {
            // Validate resourceId
            if (string.IsNullOrEmpty(resourceId))
            {
                Debug.LogWarning("SetResourceCapacity called with empty resourceId!");
                return;
            }

            // Check if resource exists in database
            if (!_resourceDatabase.ContainsKey(resourceId))
            {
                Debug.LogWarning($"SetResourceCapacity: Unknown resourceId {resourceId}");
                return;
            }

            // Set capacity
            _resourceCapacities[resourceId] = capacity;
        }

        /// <summary>
        /// Gets the capacity limit for a resource.
        /// </summary>
        /// <param name="resourceId">The unique identifier of the resource</param>
        /// <returns>The capacity limit, or 0 if unlimited</returns>
        public long GetResourceCapacity(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                return 0;
            }

            return _resourceCapacities.TryGetValue(resourceId, out long capacity) ? capacity : 0;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates a single resource stack.
        /// Checks if resourceId exists and amount is positive.
        /// </summary>
        /// <param name="stack">The resource stack to validate</param>
        /// <returns>True if the stack is valid, false otherwise</returns>
        public bool ValidateResourceStack(ResourceStack stack)
        {
            // Check resourceId is not empty
            if (string.IsNullOrEmpty(stack.resourceId))
            {
                return false;
            }

            // Check amount is positive
            if (stack.amount <= 0)
            {
                return false;
            }

            // Check resourceId exists in database
            if (!_resourceDatabase.ContainsKey(stack.resourceId))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates an array of resource stacks.
        /// All stacks must be valid for this method to return true.
        /// </summary>
        /// <param name="stacks">The array of resource stacks to validate</param>
        /// <returns>True if all stacks are valid, false otherwise</returns>
        public bool ValidateResourceStacks(ResourceStack[] stacks)
        {
            // Check array is not null or empty
            if (stacks == null || stacks.Length == 0)
            {
                return false;
            }

            // Validate each stack
            foreach (var stack in stacks)
            {
                if (!ValidateResourceStack(stack))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates a resource operation (add/consume).
        /// Checks if resourceId exists and amount is valid.
        /// </summary>
        /// <param name="resourceId">The resource identifier to validate</param>
        /// <param name="amount">The amount to validate</param>
        /// <returns>True if the operation is valid, false otherwise</returns>
        private bool ValidateResourceOperation(string resourceId, int amount)
        {
            // Check resourceId is not empty
            if (string.IsNullOrEmpty(resourceId))
            {
                Debug.LogWarning("Resource operation with empty resourceId!");
                return false;
            }

            // Check amount is not negative
            if (amount < 0)
            {
                Debug.LogWarning($"Resource operation with negative amount: {amount}");
                return false;
            }

            // Check resourceId exists in database
            if (!_resourceDatabase.ContainsKey(resourceId))
            {
                Debug.LogWarning($"Unknown resourceId: {resourceId}");
                return false;
            }

            return true;
        }

        #endregion
    }
}
