using System;
using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Represents an output port on a machine where produced resources are sent.
    /// Manages a buffer of outgoing resources with capacity limits.
    /// </summary>
    [Serializable]
    public class OutputPort
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
        /// Creates a new output port.
        /// </summary>
        /// <param name="id">Unique identifier for this port.</param>
        /// <param name="position">Local position relative to machine.</param>
        /// <param name="cap">Maximum buffer capacity.</param>
        public OutputPort(string id, Vector3 position, int cap)
        {
            portId = id;
            localPosition = position;
            capacity = cap;
        }
        
        /// <summary>
        /// Checks if the port can add the specified amount of a resource.
        /// </summary>
        /// <param name="resourceId">The resource type to check.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if there is sufficient space, false otherwise.</returns>
        public bool CanAddResource(string resourceId, int amount)
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
                Debug.LogWarning($"OutputPort {portId}: Attempted to add invalid amount {amount}");
                return false;
            }
            
            if (string.IsNullOrEmpty(resourceId))
            {
                Debug.LogWarning($"OutputPort {portId}: Attempted to add resource with empty ID");
                return false;
            }
            
            if (!CanAddResource(resourceId, amount))
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
        /// Extracts resources from the port's buffer.
        /// This is typically called by the logistics system to remove resources for transport.
        /// </summary>
        /// <param name="resourceId">The resource type to extract.</param>
        /// <param name="amount">The amount to extract.</param>
        /// <returns>The actual amount extracted (may be less than requested if insufficient resources).</returns>
        public int ExtractResource(string resourceId, int amount)
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
            int toExtract = Mathf.Min(available, amount);
            
            buffer[resourceId] -= toExtract;
            
            // Remove entry if amount reaches zero
            if (buffer[resourceId] <= 0)
            {
                buffer.Remove(resourceId);
            }
            
            return toExtract;
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
        /// Checks if the buffer contains any resources.
        /// </summary>
        /// <returns>True if there are resources in the buffer, false if empty.</returns>
        public bool HasResources()
        {
            return buffer.Count > 0 && GetTotalAmount() > 0;
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
