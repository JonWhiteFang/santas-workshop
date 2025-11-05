using System;
using System.Collections.Generic;
using UnityEngine;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Save data structure for a machine's complete state.
    /// Used for serialization and persistence between game sessions.
    /// </summary>
    [Serializable]
    public struct MachineSaveData
    {
        /// <summary>
        /// Unique identifier for this machine instance.
        /// </summary>
        public string machineId;
        
        /// <summary>
        /// Type name of the machine (used for instantiation).
        /// </summary>
        public string machineType;
        
        /// <summary>
        /// Current tier level of the machine.
        /// </summary>
        public int tier;
        
        /// <summary>
        /// Grid position of the machine.
        /// </summary>
        public Vector3Int gridPosition;
        
        /// <summary>
        /// Rotation of the machine (0-3 for 0°, 90°, 180°, 270°).
        /// </summary>
        public int rotation;
        
        /// <summary>
        /// Current operational state of the machine.
        /// </summary>
        public MachineState currentState;
        
        /// <summary>
        /// Current processing progress (0.0 to 1.0).
        /// </summary>
        public float processingProgress;
        
        /// <summary>
        /// ID of the currently active recipe (null if none).
        /// </summary>
        public string activeRecipeId;
        
        /// <summary>
        /// Save data for all input port buffers.
        /// </summary>
        public List<BufferSaveData> inputBuffers;
        
        /// <summary>
        /// Save data for all output port buffers.
        /// </summary>
        public List<BufferSaveData> outputBuffers;
        
        /// <summary>
        /// Whether the machine is enabled or disabled by the player.
        /// </summary>
        public bool isEnabled;
    }
    
    /// <summary>
    /// Save data structure for a port's buffer contents.
    /// </summary>
    [Serializable]
    public struct BufferSaveData
    {
        /// <summary>
        /// Unique identifier for the port.
        /// </summary>
        public string portId;
        
        /// <summary>
        /// Dictionary of resource IDs to amounts stored in the buffer.
        /// </summary>
        public Dictionary<string, int> contents;
    }
}
