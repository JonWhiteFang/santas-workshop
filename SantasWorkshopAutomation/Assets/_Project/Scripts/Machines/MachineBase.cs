using System;
using System.Collections.Generic;
using UnityEngine;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Abstract base class for all machines in the game.
    /// Provides common functionality for state management, power consumption,
    /// recipe processing, and grid integration.
    /// </summary>
    public abstract class MachineBase : MonoBehaviour, IPowerConsumer
    {
        #region Serialized Fields
        
        [Header("Machine Identity")]
        [Tooltip("Unique identifier for this machine instance")]
        [SerializeField] protected string machineId;
        
        [Tooltip("Configuration data for this machine type")]
        [SerializeField] protected MachineData machineData;
        
        #endregion
        
        #region Grid Integration Fields
        
        /// <summary>
        /// Position of this machine on the grid.
        /// </summary>
        protected Vector3Int gridPosition;
        
        /// <summary>
        /// Size of this machine in grid cells (width x depth).
        /// </summary>
        protected Vector2Int gridSize;
        
        /// <summary>
        /// Rotation of the machine (0-3 for 0°, 90°, 180°, 270°).
        /// </summary>
        protected int rotation;
        
        #endregion
        
        #region State Management Fields
        
        /// <summary>
        /// Current operational state of the machine.
        /// </summary>
        protected MachineState currentState = MachineState.Idle;
        
        /// <summary>
        /// Previous operational state (used when transitioning from NoPower or Disabled).
        /// </summary>
        protected MachineState previousState = MachineState.Idle;
        
        #endregion
        
        #region Port Fields
        
        /// <summary>
        /// List of input ports for receiving resources.
        /// </summary>
        protected List<InputPort> inputPorts = new List<InputPort>();
        
        /// <summary>
        /// List of output ports for sending produced resources.
        /// </summary>
        protected List<OutputPort> outputPorts = new List<OutputPort>();
        
        #endregion
        
        #region Recipe Processing Fields
        
        /// <summary>
        /// Currently active recipe being processed.
        /// </summary>
        protected Recipe activeRecipe;
        
        /// <summary>
        /// Current processing progress (0.0 to 1.0).
        /// </summary>
        protected float processingProgress = 0f;
        
        /// <summary>
        /// Time remaining to complete current processing (in seconds).
        /// </summary>
        protected float processingTimeRemaining = 0f;
        
        #endregion
        
        #region Power Fields
        
        /// <summary>
        /// Whether this machine currently has sufficient power.
        /// </summary>
        protected bool isPowered = true;
        
        /// <summary>
        /// Current power consumption in watts.
        /// </summary>
        protected float powerConsumption = 0f;
        
        #endregion
        
        #region Tier and Multiplier Fields
        
        /// <summary>
        /// Current tier level of this machine (affects performance).
        /// </summary>
        protected int tier = 1;
        
        /// <summary>
        /// Speed multiplier based on tier (higher tier = faster processing).
        /// </summary>
        protected float speedMultiplier = 1f;
        
        /// <summary>
        /// Efficiency multiplier based on tier (higher tier = lower power consumption).
        /// </summary>
        protected float efficiencyMultiplier = 1f;
        
        #endregion
        
        #region Enable/Disable Fields
        
        /// <summary>
        /// Whether this machine is enabled (can be manually disabled by player).
        /// </summary>
        protected bool isEnabled = true;
        
        #endregion
        
        #region IPowerConsumer Implementation
        
        /// <summary>
        /// Gets the current power consumption in watts.
        /// Returns 0 if the machine is not actively consuming power.
        /// </summary>
        public virtual float PowerConsumption => isPowered && currentState == MachineState.Processing 
            ? powerConsumption * efficiencyMultiplier 
            : 0f;
        
        /// <summary>
        /// Gets whether the machine currently has sufficient power.
        /// </summary>
        public bool IsPowered => isPowered;
        
        /// <summary>
        /// Sets the powered state of the machine.
        /// Called by the power grid when power availability changes.
        /// </summary>
        /// <param name="powered">True if power is available, false otherwise.</param>
        public virtual void SetPowered(bool powered)
        {
            // Implementation will be added in later tasks
        }
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Gets the unique identifier for this machine instance.
        /// </summary>
        public string MachineId => machineId;
        
        /// <summary>
        /// Gets the current operational state of the machine.
        /// </summary>
        public MachineState CurrentState => currentState;
        
        /// <summary>
        /// Gets the current processing progress (0.0 to 1.0).
        /// </summary>
        public float ProcessingProgress => processingProgress;
        
        /// <summary>
        /// Gets the estimated time remaining to complete processing (in seconds).
        /// </summary>
        public float EstimatedTimeRemaining => processingTimeRemaining;
        
        /// <summary>
        /// Gets the current tier level of this machine.
        /// </summary>
        public int Tier => tier;
        
        /// <summary>
        /// Gets the grid position of this machine.
        /// </summary>
        public Vector3Int GridPosition => gridPosition;
        
        /// <summary>
        /// Gets the size of this machine in grid cells.
        /// </summary>
        public Vector2Int GridSize => gridSize;
        
        /// <summary>
        /// Gets the rotation of this machine (0-3).
        /// </summary>
        public int Rotation => rotation;
        
        #endregion
    }
}
