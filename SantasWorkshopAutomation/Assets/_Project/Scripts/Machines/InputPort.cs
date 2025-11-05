using System;
using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Represents an input port on a machine where resources are received.
    /// Manages a buffer of incoming resources with capacity limits.
    /// </summary>
    [Serializable]
    public class InputPort
    {
        /// <summary>
        /// Unique identifier for this port.
        /// </summary>
        public string portId;
        
        /// <summary>
        /// Local position of the port relative to the machine.
        /// </summary>
        public Vector3 localPosition;
        
        /// <summary>
        /// Maximum total capacity of this port's buffer.
        /// </summary>
        public int capacity;
        
        /// <summary>
        /// Internal buffer storing resources by resource ID.
        /// </summary>
        private Dictionary<string, int> buffer = new Dictionary<string, int>();
        
        /// <summary>
        /// Creates a new input port.
        /// </summary>
        /// <param name="id">Unique identifier for this port.</param>
        /// <param name="position">Local position relative to machine.</param>
        /// <param name="cap">Maximum buffer capacity.</param>
        public InputPort(string id, Vector3 position, int cap)
        {
            portId = id;
            localPosition = position;
            capacity = cap;
        }
        
        /// <summary>
        /// Checks if the port can accept the specified amount of a resource.
        /// </summary>
        /// <param name="resourceId">The resource type to check.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if there is sufficient space, false otherwise.</returns>
        public bool CanAcceptResource(string resourceId, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }
            
            int currentTotal = GetTotalAmount();
            return currentTotal + amount <= capacity;
        }
        
        /// <summary>
        /// Adds resources to the port's buffer.
        /// </summary>
        /// <param name="resourceId">The resource type to add.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if the resources were added successfully, false if capacity was exceeded.</returns>
        public bool AddResource(string resourceId, int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"InputPort {portId}: Attempted to add invalid amount {amount}");
                return false;
            }
            
            if (string.IsNullOrEmpty(resourceId))
            {
                Debug.LogWarning($"InputPort {portId}: Attempted to add resource with empty ID");
                return false;
            }
            
            if (!CanAcceptResource(resourceId, amount))
            {
                return false;
            }
            
            if (buffer.ContainsKey(resourceId))
            {
                buffer[resourceId] += amount;
            }
            else
            {
                buffer[resourceId] = amount;
            }
            
            return true;
        }
        
        /// <summary>
        /// Removes resources from the port's buffer.
        /// </summary>
        /// <param name="resourceId">The resource type to remove.</param>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>The actual amount removed (may be less than requested if insufficient resources).</returns>
        public int RemoveResource(string resourceId, int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }
            
            if (string.IsNullOrEmpty(resourceId))
            {
                return 0;
            }
            
            if (!buffer.ContainsKey(resourceId))
            {
                return 0;
            }
            
            int available = buffer[resourceId];
            int toRemove = Mathf.Min(available, amount);
            
            buffer[resourceId] -= toRemove;
            
            // Remove entry if amount reaches zero
            if (buffer[resourceId] <= 0)
            {
                buffer.Remove(resourceId);
            }
            
            return toRemove;
        }
        
        /// <summary>
        /// Gets the amount of a specific resource in the buffer.
        /// </summary>
        /// <param name="resourceId">The resource type to query.</param>
        /// <returns>The amount of the resource, or 0 if not present.</returns>
        public int GetResourceAmount(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                return 0;
            }
            
            return buffer.ContainsKey(resourceId) ? buffer[resourceId] : 0;
        }
        
        /// <summary>
        /// Gets the total amount of all resources in the buffer.
        /// </summary>
        /// <returns>The sum of all resource amounts.</returns>
        public int GetTotalAmount()
        {
            int total = 0;
            foreach (var kvp in buffer)
            {
                total += kvp.Value;
            }
            return total;
        }
        
        /// <summary>
        /// Gets the available space in the buffer.
        /// </summary>
        /// <returns>The remaining capacity.</returns>
        public int GetAvailableSpace()
        {
            return capacity - GetTotalAmount();
        }
        
        /// <summary>
        /// Gets a read-only view of all resources in the buffer.
        /// Used for efficient caching and iteration.
        /// </summary>
        /// <returns>Dictionary of resource IDs to amounts.</returns>
        public IReadOnlyDictionary<string, int> GetAllResources()
        {
            return buffer;
        }
        
        /// <summary>
        /// Gets save data for this port.
        /// </summary>
        /// <returns>A BufferSaveData struct containing the port's state.</returns>
        public BufferSaveData GetSaveData()
        {
            return new BufferSaveData
            {
                portId = portId,
                contents = new Dictionary<string, int>(buffer)
            };
        }
        
        /// <summary>
        /// Loads save data into this port.
        /// </summary>
        /// <param name="data">The save data to load.</param>
        public void LoadSaveData(BufferSaveData data)
        {
            buffer = new Dictionary<string, int>(data.contents);
        }
        
        /// <summary>
        /// Clears all resources from the buffer.
        /// </summary>
        public void Clear()
        {
            buffer.Clear();
        }
    }
}
