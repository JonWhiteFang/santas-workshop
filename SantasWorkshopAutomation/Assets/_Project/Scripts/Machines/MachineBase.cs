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
        
        #region Recipe Processing (Placeholder)
        
        /// <summary>
        /// Checks if the machine can process the given recipe.
        /// Implementation will be added in task 13.
        /// </summary>
        /// <param name="recipe">The recipe to check.</param>
        /// <returns>True if the recipe can be processed, false otherwise.</returns>
        protected virtual bool CanProcessRecipe(Recipe recipe)
        {
            // TODO: Implement in task 13
            return false;
        }
        
        /// <summary>
        /// Checks if the output buffer has space for the active recipe's outputs.
        /// Implementation will be added in task 13.
        /// </summary>
        /// <returns>True if there is space, false otherwise.</returns>
        protected virtual bool HasOutputSpace()
        {
            // TODO: Implement in task 13
            return true;
        }
        
        /// <summary>
        /// Completes the current processing cycle.
        /// Implementation will be added in task 13.
        /// </summary>
        protected virtual void CompleteProcessing()
        {
            // TODO: Implement in task 13
        }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Event fired when the machine transitions to a new state.
        /// Parameters: (oldState, newState)
        /// </summary>
        public event Action<MachineState, MachineState> OnStateChanged;
        
        /// <summary>
        /// Event fired when the machine starts processing a recipe.
        /// Parameter: The recipe being processed.
        /// </summary>
        public event Action<Recipe> OnProcessingStarted;
        
        /// <summary>
        /// Event fired when the machine completes processing a recipe.
        /// Parameter: The recipe that was completed.
        /// </summary>
        public event Action<Recipe> OnProcessingCompleted;
        
        /// <summary>
        /// Event fired when the machine's power status changes.
        /// Parameter: True if powered, false if unpowered.
        /// </summary>
        public event Action<bool> OnPowerStatusChanged;
        
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
        
        #region Lifecycle Methods
        
        /// <summary>
        /// Called when the script instance is being loaded.
        /// Generates machine ID if empty and initializes from MachineData.
        /// </summary>
        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(machineId))
            {
                machineId = System.Guid.NewGuid().ToString();
            }
            
            InitializeFromData();
        }
        
        /// <summary>
        /// Called on the frame when the script is enabled.
        /// Registers with power grid and validates configuration.
        /// </summary>
        protected virtual void Start()
        {
            RegisterWithPowerGrid();
            ValidateConfiguration();
        }
        
        /// <summary>
        /// Called every frame.
        /// Updates the state machine if the machine is enabled.
        /// </summary>
        protected virtual void Update()
        {
            if (!isEnabled) return;
            
            UpdateStateMachine();
        }
        
        /// <summary>
        /// Called when the MonoBehaviour will be destroyed.
        /// Unregisters from power grid and frees grid cells.
        /// </summary>
        protected virtual void OnDestroy()
        {
            UnregisterFromPowerGrid();
            FreeGridCells();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initializes the machine from its MachineData configuration.
        /// Loads tier, grid size, power consumption, and initializes ports.
        /// </summary>
        protected virtual void InitializeFromData()
        {
            if (machineData == null)
            {
                Debug.LogError($"Machine {gameObject.name} has no MachineData assigned!");
                return;
            }
            
            tier = machineData.tier;
            gridSize = machineData.gridSize;
            powerConsumption = machineData.basePowerConsumption;
            
            CalculateMultipliers();
            InitializePorts();
        }
        
        /// <summary>
        /// Initializes input and output ports from MachineData configuration.
        /// Creates port instances with configured positions and capacities.
        /// </summary>
        protected virtual void InitializePorts()
        {
            if (machineData == null) return;
            
            // Create input ports
            for (int i = 0; i < machineData.inputPortCount; i++)
            {
                Vector3 position = i < machineData.inputPortPositions.Length 
                    ? machineData.inputPortPositions[i] 
                    : new Vector3(-0.5f, 0.5f, 0);
                    
                inputPorts.Add(new InputPort(
                    $"{machineId}_input_{i}",
                    position,
                    machineData.bufferCapacity
                ));
            }
            
            // Create output ports
            for (int i = 0; i < machineData.outputPortCount; i++)
            {
                Vector3 position = i < machineData.outputPortPositions.Length 
                    ? machineData.outputPortPositions[i] 
                    : new Vector3(0.5f, 0.5f, 0);
                    
                outputPorts.Add(new OutputPort(
                    $"{machineId}_output_{i}",
                    position,
                    machineData.bufferCapacity
                ));
            }
        }
        
        /// <summary>
        /// Calculates speed and efficiency multipliers based on machine tier.
        /// Speed increases by 20% per tier, efficiency improves by 10% per tier.
        /// </summary>
        protected virtual void CalculateMultipliers()
        {
            // Speed increases by 20% per tier
            speedMultiplier = 1f + (tier - 1) * 0.2f;
            
            // Efficiency improves by 10% per tier (reduces power consumption)
            efficiencyMultiplier = 1f - (tier - 1) * 0.1f;
            efficiencyMultiplier = Mathf.Max(efficiencyMultiplier, 0.5f); // Min 50% consumption
        }
        
        #endregion
        
        #region State Machine
        
        /// <summary>
        /// Transitions the machine to a new state.
        /// Handles state exit, state change, state enter, and event firing.
        /// </summary>
        /// <param name="newState">The state to transition to.</param>
        public void TransitionToState(MachineState newState)
        {
            if (currentState == newState) return;
            
            MachineState oldState = currentState;
            
            // Exit current state
            OnStateExit(currentState);
            
            // Change state
            previousState = currentState;
            currentState = newState;
            
            // Enter new state
            OnStateEnter(newState);
            
            // Fire event
            OnStateChanged?.Invoke(oldState, newState);
            
            Debug.Log($"Machine {machineId} transitioned from {oldState} to {newState}");
        }
        
        /// <summary>
        /// Called when entering a new state.
        /// Routes to specific state enter methods based on the state.
        /// </summary>
        /// <param name="state">The state being entered.</param>
        protected virtual void OnStateEnter(MachineState state)
        {
            switch (state)
            {
                case MachineState.Idle:
                    OnEnterIdle();
                    break;
                case MachineState.WaitingForInput:
                    OnEnterWaitingForInput();
                    break;
                case MachineState.Processing:
                    OnEnterProcessing();
                    break;
                case MachineState.WaitingForOutput:
                    OnEnterWaitingForOutput();
                    break;
                case MachineState.NoPower:
                    OnEnterNoPower();
                    break;
                case MachineState.Disabled:
                    OnEnterDisabled();
                    break;
            }
            
            SetVisualState(state);
        }
        
        /// <summary>
        /// Called when exiting a state.
        /// Routes to specific state exit methods based on the state.
        /// </summary>
        /// <param name="state">The state being exited.</param>
        protected virtual void OnStateExit(MachineState state)
        {
            switch (state)
            {
                case MachineState.Processing:
                    OnExitProcessing();
                    break;
            }
        }
        
        /// <summary>
        /// Updates the state machine logic each frame.
        /// Checks power status and updates current state behavior.
        /// </summary>
        protected virtual void UpdateStateMachine()
        {
            // Check power status
            if (!isPowered && currentState != MachineState.NoPower && currentState != MachineState.Disabled)
            {
                TransitionToState(MachineState.NoPower);
                return;
            }
            
            if (isPowered && currentState == MachineState.NoPower)
            {
                TransitionToState(previousState);
                return;
            }
            
            // Update current state
            switch (currentState)
            {
                case MachineState.Idle:
                    UpdateIdle();
                    break;
                case MachineState.WaitingForInput:
                    UpdateWaitingForInput();
                    break;
                case MachineState.Processing:
                    UpdateProcessing();
                    break;
                case MachineState.WaitingForOutput:
                    UpdateWaitingForOutput();
                    break;
            }
        }
        
        /// <summary>
        /// Virtual method called when entering Idle state.
        /// Override in derived classes for custom behavior.
        /// </summary>
        protected virtual void OnEnterIdle() { }
        
        /// <summary>
        /// Virtual method called when entering WaitingForInput state.
        /// Override in derived classes for custom behavior.
        /// </summary>
        protected virtual void OnEnterWaitingForInput() { }
        
        /// <summary>
        /// Virtual method called when entering WaitingForOutput state.
        /// Override in derived classes for custom behavior.
        /// </summary>
        protected virtual void OnEnterWaitingForOutput() { }
        
        /// <summary>
        /// Virtual method called when entering NoPower state.
        /// Override in derived classes for custom behavior.
        /// </summary>
        protected virtual void OnEnterNoPower() { }
        
        /// <summary>
        /// Virtual method called when entering Disabled state.
        /// Override in derived classes for custom behavior.
        /// </summary>
        protected virtual void OnEnterDisabled() { }
        
        /// <summary>
        /// Called when entering Processing state.
        /// Initializes processing progress and fires OnProcessingStarted event.
        /// </summary>
        protected virtual void OnEnterProcessing()
        {
            if (activeRecipe == null)
            {
                Debug.LogError($"Machine {machineId} entered Processing state with no active recipe!");
                TransitionToState(MachineState.Idle);
                return;
            }
            
            processingProgress = 0f;
            processingTimeRemaining = activeRecipe.processingTime / speedMultiplier;
            
            OnProcessingStarted?.Invoke(activeRecipe);
        }
        
        /// <summary>
        /// Called when exiting Processing state.
        /// Resets processing progress and time remaining.
        /// </summary>
        protected virtual void OnExitProcessing()
        {
            processingProgress = 0f;
            processingTimeRemaining = 0f;
        }
        
        /// <summary>
        /// Updates Idle state behavior.
        /// Checks if processing can start and transitions accordingly.
        /// </summary>
        protected virtual void UpdateIdle()
        {
            // Check if we can start processing
            if (CanProcessRecipe(activeRecipe))
            {
                TransitionToState(MachineState.Processing);
            }
            else if (activeRecipe != null)
            {
                TransitionToState(MachineState.WaitingForInput);
            }
        }
        
        /// <summary>
        /// Updates WaitingForInput state behavior.
        /// Checks if inputs are now available and transitions to Processing.
        /// </summary>
        protected virtual void UpdateWaitingForInput()
        {
            // Check if inputs are now available
            if (CanProcessRecipe(activeRecipe))
            {
                TransitionToState(MachineState.Processing);
            }
        }
        
        /// <summary>
        /// Updates Processing state behavior.
        /// Updates progress and completes processing when finished.
        /// </summary>
        protected virtual void UpdateProcessing()
        {
            if (activeRecipe == null)
            {
                TransitionToState(MachineState.Idle);
                return;
            }
            
            // Update progress
            float deltaTime = Time.deltaTime;
            processingTimeRemaining -= deltaTime;
            processingProgress = 1f - (processingTimeRemaining / (activeRecipe.processingTime / speedMultiplier));
            
            // Check if processing is complete
            if (processingTimeRemaining <= 0f)
            {
                CompleteProcessing();
            }
        }
        
        /// <summary>
        /// Updates WaitingForOutput state behavior.
        /// Checks if output buffer has space and transitions to Idle.
        /// </summary>
        protected virtual void UpdateWaitingForOutput()
        {
            // Check if output buffer has space
            if (HasOutputSpace())
            {
                TransitionToState(MachineState.Idle);
            }
        }
        
        /// <summary>
        /// Sets the visual state of the machine.
        /// Override in derived classes to update visual indicators.
        /// Implementation will be added in task 18.
        /// </summary>
        /// <param name="state">The state to visualize.</param>
        protected virtual void SetVisualState(MachineState state)
        {
            // TODO: Implement in task 18
        }
        
        #endregion
        
        #region Power Management (Placeholder)
        
        /// <summary>
        /// Registers this machine with the power grid.
        /// Implementation will be added in task 15.
        /// </summary>
        protected virtual void RegisterWithPowerGrid()
        {
            // TODO: Implement in task 15
        }
        
        /// <summary>
        /// Unregisters this machine from the power grid.
        /// Implementation will be added in task 15.
        /// </summary>
        protected virtual void UnregisterFromPowerGrid()
        {
            // TODO: Implement in task 15
        }
        
        #endregion
        
        #region Grid Integration (Placeholder)
        
        /// <summary>
        /// Frees the grid cells occupied by this machine.
        /// Implementation will be added in task 16.
        /// </summary>
        protected virtual void FreeGridCells()
        {
            // TODO: Implement in task 16
        }
        
        #endregion
        
        #region Validation (Placeholder)
        
        /// <summary>
        /// Validates the machine's configuration.
        /// Implementation will be added in task 19.
        /// </summary>
        protected virtual void ValidateConfiguration()
        {
            // TODO: Implement in task 19
        }
        
        #endregion
    }
}
