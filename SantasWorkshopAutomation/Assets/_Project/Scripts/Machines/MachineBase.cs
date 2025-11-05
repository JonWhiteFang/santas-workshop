using System;
using System.Collections.Generic;
using UnityEngine;
using SantasWorkshop.Data;
using SantasWorkshop.Core;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Abstract base class for all machines in the game.
    /// Provides common functionality for state management, power consumption,
    /// recipe processing, and grid integration.
    /// </summary>
    public abstract class MachineBase : MonoBehaviour, IPowerConsumer, IMachine
    {
        #region Constants
        
        private const float SPEED_MULTIPLIER_PER_TIER = 0.2f;
        private const float EFFICIENCY_MULTIPLIER_PER_TIER = 0.1f;
        private const float MIN_EFFICIENCY_MULTIPLIER = 0.5f;
        
        #endregion
        
        #region Serialized Fields
        
        [Header("Machine Identity")]
        [Tooltip("Unique identifier for this machine instance")]
        [SerializeField] protected string machineId;
        
        [Tooltip("Configuration data for this machine type")]
        [SerializeField] protected MachineData machineData;
        
        #endregion
        
        #region Save/Load
        
        /// <summary>
        /// Gets the save data for this machine.
        /// Captures all state needed to restore the machine later.
        /// </summary>
        /// <returns>A MachineSaveData struct containing all machine state.</returns>
        public virtual MachineSaveData GetSaveData()
        {
            return new MachineSaveData
            {
                machineId = machineId,
                machineType = GetType().Name,
                tier = tier,
                gridPosition = gridPosition,
                rotation = rotation,
                currentState = currentState,
                processingProgress = processingProgress,
                activeRecipeId = activeRecipe?.recipeId,
                inputBuffers = GetInputBufferSaveData(),
                outputBuffers = GetOutputBufferSaveData(),
                isEnabled = isEnabled
            };
        }
        
        /// <summary>
        /// Loads save data and restores the machine's state.
        /// Restores all fields, buffers, and transitions to the saved state.
        /// </summary>
        /// <param name="saveData">The save data to load from.</param>
        public virtual void LoadSaveData(MachineSaveData saveData)
        {
            machineId = saveData.machineId;
            tier = saveData.tier;
            gridPosition = saveData.gridPosition;
            rotation = saveData.rotation;
            processingProgress = saveData.processingProgress;
            isEnabled = saveData.isEnabled;
            
            // Recalculate multipliers based on loaded tier
            CalculateMultipliers();
            
            // Load recipe
            if (!string.IsNullOrEmpty(saveData.activeRecipeId))
            {
                // Find recipe in available recipes
                if (machineData != null && machineData.availableRecipes != null)
                {
                    activeRecipe = machineData.availableRecipes.Find(r => r != null && r.recipeId == saveData.activeRecipeId);
                    
                    if (activeRecipe == null)
                    {
                        Debug.LogWarning($"Machine {machineId} could not find saved recipe '{saveData.activeRecipeId}'!");
                    }
                    else
                    {
                        powerConsumption = activeRecipe.powerConsumption;
                    }
                }
            }
            else
            {
                activeRecipe = null;
                powerConsumption = 0f;
            }
            
            // Load buffers
            LoadInputBufferSaveData(saveData.inputBuffers);
            LoadOutputBufferSaveData(saveData.outputBuffers);
            
            // Update visual rotation
            UpdateVisualRotation();
            
            // Restore state (do this last after everything is loaded)
            TransitionToState(saveData.currentState);
            
            // If we were processing, restore the time remaining
            if (currentState == MachineState.Processing && activeRecipe != null)
            {
                float totalTime = activeRecipe.processingTime / speedMultiplier;
                processingTimeRemaining = totalTime * (1f - processingProgress);
            }
        }
        
        /// <summary>
        /// Gets save data for all input buffers.
        /// </summary>
        /// <returns>A list of BufferSaveData for each input port.</returns>
        protected virtual List<BufferSaveData> GetInputBufferSaveData()
        {
            List<BufferSaveData> data = new List<BufferSaveData>();
            foreach (var port in inputPorts)
            {
                if (port != null)
                {
                    data.Add(port.GetSaveData());
                }
            }
            return data;
        }
        
        /// <summary>
        /// Gets save data for all output buffers.
        /// </summary>
        /// <returns>A list of BufferSaveData for each output port.</returns>
        protected virtual List<BufferSaveData> GetOutputBufferSaveData()
        {
            List<BufferSaveData> data = new List<BufferSaveData>();
            foreach (var port in outputPorts)
            {
                if (port != null)
                {
                    data.Add(port.GetSaveData());
                }
            }
            return data;
        }
        
        /// <summary>
        /// Loads save data into input buffers.
        /// </summary>
        /// <param name="data">The list of BufferSaveData to load.</param>
        protected virtual void LoadInputBufferSaveData(List<BufferSaveData> data)
        {
            if (data == null) return;
            
            for (int i = 0; i < Mathf.Min(data.Count, inputPorts.Count); i++)
            {
                if (inputPorts[i] != null)
                {
                    inputPorts[i].LoadSaveData(data[i]);
                }
            }
        }
        
        /// <summary>
        /// Loads save data into output buffers.
        /// </summary>
        /// <param name="data">The list of BufferSaveData to load.</param>
        protected virtual void LoadOutputBufferSaveData(List<BufferSaveData> data)
        {
            if (data == null) return;
            
            for (int i = 0; i < Mathf.Min(data.Count, outputPorts.Count); i++)
            {
                if (outputPorts[i] != null)
                {
                    outputPorts[i].LoadSaveData(data[i]);
                }
            }
        }
        
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
        
        #region Cache Fields
        
        /// <summary>
        /// Cached input buffer totals for performance optimization.
        /// </summary>
        private Dictionary<string, int> _cachedInputTotals = new Dictionary<string, int>();
        
        /// <summary>
        /// Flag indicating if the input cache needs to be rebuilt.
        /// </summary>
        private bool _inputCacheDirty = true;
        
        /// <summary>
        /// Flag indicating if we're resuming processing after power loss.
        /// </summary>
        private bool _resumingProcessing = false;
        
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
        
        // SetPowered implementation is in the Power Management region
        
        #endregion
        
        #region IMachine Implementation
        
        /// <summary>
        /// Gets the unique identifier for this machine instance.
        /// </summary>
        string IMachine.MachineId => machineId;
        
        /// <summary>
        /// Gets the current operational state of the machine.
        /// </summary>
        MachineState IMachine.State => currentState;
        
        /// <summary>
        /// Initializes the machine with configuration data.
        /// Called once when the machine is first created or loaded.
        /// </summary>
        /// <param name="data">Machine configuration data</param>
        void IMachine.Initialize(MachineData data)
        {
            machineData = data;
            InitializeFromData();
        }
        
        /// <summary>
        /// Updates the machine's logic each frame or simulation tick.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last tick</param>
        void IMachine.Tick(float deltaTime)
        {
            if (!isEnabled) return;
            UpdateStateMachine();
        }
        
        /// <summary>
        /// Shuts down the machine and cleans up resources.
        /// Called when the machine is destroyed or disabled.
        /// </summary>
        void IMachine.Shutdown()
        {
            UnregisterFromPowerGrid();
            FreeGridCells();
        }
        
        #endregion
        
        #region Recipe Processing
        
        /// <summary>
        /// Validates a recipe for use with this machine.
        /// Checks all requirements and constraints.
        /// </summary>
        /// <param name="recipe">The recipe to validate.</param>
        /// <param name="error">Output parameter containing the error message if validation fails.</param>
        /// <returns>True if the recipe is valid, false otherwise.</returns>
        protected virtual bool ValidateRecipe(Recipe recipe, out string error)
        {
            error = null;
            
            if (recipe == null)
            {
                error = "Recipe is null";
                return false;
            }
            
            if (recipe.inputs == null || recipe.inputs.Length == 0)
            {
                error = $"Recipe '{recipe.recipeName}' has no inputs";
                return false;
            }
            
            if (recipe.outputs == null || recipe.outputs.Length == 0)
            {
                error = $"Recipe '{recipe.recipeName}' has no outputs";
                return false;
            }
            
            if (recipe.processingTime <= 0f)
            {
                error = $"Recipe '{recipe.recipeName}' has invalid processing time: {recipe.processingTime}";
                return false;
            }
            
            if (recipe.powerConsumption < 0f)
            {
                error = $"Recipe '{recipe.recipeName}' has negative power consumption: {recipe.powerConsumption}";
                return false;
            }
            
            if (recipe.requiredTier > tier)
            {
                error = $"Recipe '{recipe.recipeName}' requires tier {recipe.requiredTier}, but machine is tier {tier}";
                return false;
            }
            
            // Validate input resource IDs
            for (int i = 0; i < recipe.inputs.Length; i++)
            {
                if (string.IsNullOrEmpty(recipe.inputs[i].resourceId))
                {
                    error = $"Recipe '{recipe.recipeName}' has input at index {i} with empty resourceId";
                    return false;
                }
                if (recipe.inputs[i].amount <= 0)
                {
                    error = $"Recipe '{recipe.recipeName}' has input at index {i} with invalid amount: {recipe.inputs[i].amount}";
                    return false;
                }
            }
            
            // Validate output resource IDs
            for (int i = 0; i < recipe.outputs.Length; i++)
            {
                if (string.IsNullOrEmpty(recipe.outputs[i].resourceId))
                {
                    error = $"Recipe '{recipe.recipeName}' has output at index {i} with empty resourceId";
                    return false;
                }
                if (recipe.outputs[i].amount <= 0)
                {
                    error = $"Recipe '{recipe.recipeName}' has output at index {i} with invalid amount: {recipe.outputs[i].amount}";
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Sets the active recipe for this machine.
        /// Validates the recipe and cancels current processing if needed.
        /// </summary>
        /// <param name="recipe">The recipe to set as active.</param>
        public virtual void SetActiveRecipe(Recipe recipe)
        {
            if (recipe == null)
            {
                activeRecipe = null;
                powerConsumption = 0f;
                return;
            }
            
            // Validate recipe
            if (!ValidateRecipe(recipe, out string error))
            {
                Debug.LogWarning($"Machine {machineId}: Cannot set recipe - {error}");
                return;
            }
            
            // Validate recipe is available for this machine
            if (!IsRecipeAvailable(recipe))
            {
                Debug.LogWarning($"Machine {machineId}: Recipe '{recipe.recipeName}' is not in available recipes list");
                return;
            }
            
            // If currently processing, cancel and refund
            if (currentState == MachineState.Processing)
            {
                CancelProcessing();
            }
            
            activeRecipe = recipe;
            powerConsumption = recipe.powerConsumption;
            
            TransitionToState(MachineState.Idle);
        }
        
        /// <summary>
        /// Checks if the machine can process the given recipe.
        /// Validates power, enabled state, inputs, and output space.
        /// </summary>
        /// <param name="recipe">The recipe to check.</param>
        /// <returns>True if the recipe can be processed, false otherwise.</returns>
        protected virtual bool CanProcessRecipe(Recipe recipe)
        {
            if (recipe == null)
            {
                return false;
            }
            
            if (!isPowered)
            {
                return false;
            }
            
            if (!isEnabled)
            {
                return false;
            }
            
            // Check input availability
            if (!HasRequiredInputs(recipe))
            {
                return false;
            }
            
            // Check output space
            if (!HasOutputSpace(recipe))
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Checks if all required inputs for a recipe are available in the input buffers.
        /// </summary>
        /// <param name="recipe">The recipe to check.</param>
        /// <returns>True if all inputs are available, false otherwise.</returns>
        protected virtual bool HasRequiredInputs(Recipe recipe)
        {
            if (recipe == null || recipe.inputs == null || recipe.inputs.Length == 0)
                return false;
            
            // Rebuild cache once if dirty (amortized O(1) cost)
            if (_inputCacheDirty)
            {
                RebuildInputCache();
            }
            
            // Now use cached values for fast lookup
            foreach (var input in recipe.inputs)
            {
                if (!_cachedInputTotals.TryGetValue(input.resourceId, out int available) || available < input.amount)
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Checks if the output buffers have space for all outputs of a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to check.</param>
        /// <returns>True if there is space for all outputs, false otherwise.</returns>
        protected virtual bool HasOutputSpace(Recipe recipe)
        {
            if (recipe == null || recipe.outputs == null || recipe.outputs.Length == 0)
            {
                return true; // No outputs required, so space is available
            }
            
            foreach (var output in recipe.outputs)
            {
                if (string.IsNullOrEmpty(output.resourceId))
                {
                    Debug.LogWarning($"Machine {machineId}: Recipe '{recipe.recipeName}' has output with empty resourceId");
                    continue;
                }
                
                if (!CanAddToOutputBuffer(output.resourceId, output.amount))
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Checks if the output buffer has space for the active recipe's outputs.
        /// </summary>
        /// <returns>True if there is space, false otherwise.</returns>
        protected virtual bool HasOutputSpace()
        {
            if (activeRecipe == null) return true;
            return HasOutputSpace(activeRecipe);
        }
        
        /// <summary>
        /// Completes the current processing cycle.
        /// Consumes inputs, produces outputs, and checks if processing can continue.
        /// </summary>
        protected virtual void CompleteProcessing()
        {
            if (activeRecipe == null)
            {
                Debug.LogWarning($"Machine {machineId}: CompleteProcessing called with no active recipe");
                TransitionToState(MachineState.Idle);
                return;
            }
            
            // Consume inputs
            ConsumeInputs(activeRecipe);
            
            // Produce outputs
            ProduceOutputs(activeRecipe);
            
            // Fire completion event
            OnProcessingCompleted?.Invoke(activeRecipe);
            
            // Check if we can continue processing
            if (CanProcessRecipe(activeRecipe))
            {
                // Reset and continue
                processingProgress = 0f;
                processingTimeRemaining = activeRecipe.processingTime / speedMultiplier;
            }
            else
            {
                TransitionToState(MachineState.Idle);
            }
        }
        
        /// <summary>
        /// Cancels the current processing cycle.
        /// Resets progress without consuming inputs (they remain in buffers).
        /// </summary>
        protected virtual void CancelProcessing()
        {
            processingProgress = 0f;
            processingTimeRemaining = 0f;
        }
        
        /// <summary>
        /// Consumes input resources from the input buffers according to the recipe.
        /// </summary>
        /// <param name="recipe">The recipe whose inputs should be consumed.</param>
        protected virtual void ConsumeInputs(Recipe recipe)
        {
            if (recipe == null || recipe.inputs == null || recipe.inputs.Length == 0)
            {
                return;
            }
            
            foreach (var input in recipe.inputs)
            {
                if (string.IsNullOrEmpty(input.resourceId))
                {
                    Debug.LogWarning($"Machine {machineId}: Skipping input with empty resourceId in recipe '{recipe.recipeName}'");
                    continue;
                }
                
                if (input.amount <= 0)
                {
                    Debug.LogWarning($"Machine {machineId}: Skipping input '{input.resourceId}' with invalid amount {input.amount}");
                    continue;
                }
                
                bool success = RemoveFromInputBuffer(input.resourceId, input.amount);
                if (!success)
                {
                    Debug.LogError($"Machine {machineId}: Failed to consume {input.amount} of '{input.resourceId}' for recipe '{recipe.recipeName}'");
                }
            }
        }
        
        /// <summary>
        /// Produces output resources and adds them to the output buffers according to the recipe.
        /// </summary>
        /// <param name="recipe">The recipe whose outputs should be produced.</param>
        protected virtual void ProduceOutputs(Recipe recipe)
        {
            if (recipe == null || recipe.outputs == null || recipe.outputs.Length == 0)
            {
                return;
            }
            
            foreach (var output in recipe.outputs)
            {
                if (string.IsNullOrEmpty(output.resourceId))
                {
                    Debug.LogWarning($"Machine {machineId}: Skipping output with empty resourceId in recipe '{recipe.recipeName}'");
                    continue;
                }
                
                if (output.amount <= 0)
                {
                    Debug.LogWarning($"Machine {machineId}: Skipping output '{output.resourceId}' with invalid amount {output.amount}");
                    continue;
                }
                
                bool success = AddToOutputBuffer(output.resourceId, output.amount);
                if (!success)
                {
                    Debug.LogError($"Machine {machineId}: Failed to produce {output.amount} of '{output.resourceId}' for recipe '{recipe.recipeName}'. Output buffer may be full.");
                }
            }
        }
        
        /// <summary>
        /// Checks if a recipe is available for this machine.
        /// </summary>
        /// <param name="recipe">The recipe to check.</param>
        /// <returns>True if the recipe is in the machine's available recipes list, false otherwise.</returns>
        protected virtual bool IsRecipeAvailable(Recipe recipe)
        {
            if (machineData == null || recipe == null) return false;
            if (machineData.availableRecipes == null) return false;
            return machineData.availableRecipes.Contains(recipe);
        }
        
        /// <summary>
        /// Gets all recipes available for this machine.
        /// </summary>
        /// <returns>A list of available recipes.</returns>
        public virtual List<Recipe> GetAvailableRecipes()
        {
            if (machineData == null || machineData.availableRecipes == null)
            {
                return new List<Recipe>();
            }
            return new List<Recipe>(machineData.availableRecipes);
        }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Event fired when the machine transitions to a new state.
        /// Parameters: (oldState, newState)
        /// IMPORTANT: Always unsubscribe in OnDisable() or OnDestroy() to prevent memory leaks.
        /// Example:
        /// <code>
        /// private void OnEnable() { machine.OnStateChanged += HandleStateChange; }
        /// private void OnDisable() { machine.OnStateChanged -= HandleStateChange; }
        /// </code>
        /// </summary>
        public event Action<MachineState, MachineState> OnStateChanged;
        
        /// <summary>
        /// Event fired when the machine starts processing a recipe.
        /// Parameter: The recipe being processed.
        /// IMPORTANT: Always unsubscribe in OnDisable() or OnDestroy() to prevent memory leaks.
        /// </summary>
        public event Action<Recipe> OnProcessingStarted;
        
        /// <summary>
        /// Event fired when the machine completes processing a recipe.
        /// Parameter: The recipe that was completed.
        /// IMPORTANT: Always unsubscribe in OnDisable() or OnDestroy() to prevent memory leaks.
        /// </summary>
        public event Action<Recipe> OnProcessingCompleted;
        
        /// <summary>
        /// Event fired when the machine's power status changes.
        /// Parameter: True if powered, false if unpowered.
        /// IMPORTANT: Always unsubscribe in OnDisable() or OnDestroy() to prevent memory leaks.
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
        /// Clears event subscriptions, unregisters from power grid, and frees grid cells.
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Clear all event subscriptions to prevent memory leaks
            OnStateChanged = null;
            OnProcessingStarted = null;
            OnProcessingCompleted = null;
            OnPowerStatusChanged = null;
            
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
            // Speed increases by SPEED_MULTIPLIER_PER_TIER per tier
            speedMultiplier = 1f + (tier - 1) * SPEED_MULTIPLIER_PER_TIER;
            
            // Efficiency improves by EFFICIENCY_MULTIPLIER_PER_TIER per tier (reduces power consumption)
            efficiencyMultiplier = 1f - (tier - 1) * EFFICIENCY_MULTIPLIER_PER_TIER;
            efficiencyMultiplier = Mathf.Max(efficiencyMultiplier, MIN_EFFICIENCY_MULTIPLIER);
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
        /// Power status changes are handled in SetPowered() method.
        /// </summary>
        protected virtual void UpdateStateMachine()
        {
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
        /// Preserves progress if resuming from power loss.
        /// </summary>
        protected virtual void OnEnterProcessing()
        {
            if (activeRecipe == null)
            {
                Debug.LogError($"Machine {machineId} entered Processing state with no active recipe!");
                TransitionToState(MachineState.Idle);
                return;
            }
            
            // Only reset progress if starting fresh (not resuming from power loss)
            if (!_resumingProcessing)
            {
                processingProgress = 0f;
                processingTimeRemaining = activeRecipe.processingTime / speedMultiplier;
                OnProcessingStarted?.Invoke(activeRecipe);
            }
            // else: resuming from power loss, keep existing progress
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
                Debug.LogError($"Machine {machineId} is in Processing state with no active recipe!");
                TransitionToState(MachineState.Idle);
                return;
            }
            
            // Validate speed multiplier
            if (speedMultiplier <= 0f)
            {
                Debug.LogError($"Machine {machineId} has invalid speedMultiplier: {speedMultiplier}. Resetting to 1.0");
                speedMultiplier = 1f;
            }
            
            // Calculate total processing time
            float totalTime = activeRecipe.processingTime / speedMultiplier;
            if (totalTime <= 0f)
            {
                Debug.LogError($"Machine {machineId} has invalid processing time calculation. Recipe time: {activeRecipe.processingTime}, Speed multiplier: {speedMultiplier}");
                CompleteProcessing();
                return;
            }
            
            // Update progress
            float deltaTime = Time.deltaTime;
            processingTimeRemaining -= deltaTime;
            processingProgress = Mathf.Clamp01(1f - (processingTimeRemaining / totalTime));
            
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
        /// Override in derived classes to update visual indicators based on state.
        /// Base implementation provides logging for debugging.
        /// </summary>
        /// <param name="state">The state to visualize.</param>
        protected virtual void SetVisualState(MachineState state)
        {
            // Base implementation - derived classes should override to add visual feedback
            // Examples of visual feedback:
            // - Idle: Dim ambient light, no particles
            // - WaitingForInput: Pulsing yellow indicator
            // - Processing: Bright light, active particles, rotating parts
            // - WaitingForOutput: Pulsing red indicator
            // - NoPower: Flickering red light, warning icon
            // - Disabled: Greyed out, no effects
            
            Debug.Log($"Machine {machineId} visual state changed to {state}");
        }
        
        #endregion
        
        #region Buffer Management
        
        /// <summary>
        /// Invalidates the input buffer cache.
        /// Call this whenever input ports are modified externally (e.g., by logistics system).
        /// </summary>
        public void InvalidateInputCache()
        {
            _inputCacheDirty = true;
        }
        
        /// <summary>
        /// Adds resources to a specific input port.
        /// This is the preferred method for external systems to add resources.
        /// Automatically invalidates the input cache.
        /// </summary>
        /// <param name="portIndex">The index of the input port.</param>
        /// <param name="resourceId">The resource type to add.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if the resource was added successfully, false otherwise.</returns>
        public bool AddToInputPort(int portIndex, string resourceId, int amount)
        {
            if (portIndex < 0 || portIndex >= inputPorts.Count)
            {
                Debug.LogWarning($"Machine {machineId}: Invalid input port index {portIndex}");
                return false;
            }
            
            bool success = inputPorts[portIndex].AddResource(resourceId, amount);
            if (success)
            {
                InvalidateInputCache();
                
                // Check if we can now start processing
                if (currentState == MachineState.WaitingForInput && CanProcessRecipe(activeRecipe))
                {
                    TransitionToState(MachineState.Processing);
                }
            }
            return success;
        }
        
        /// <summary>
        /// Extracts resources from a specific output port.
        /// This is the preferred method for external systems (logistics) to extract resources.
        /// </summary>
        /// <param name="portIndex">The index of the output port.</param>
        /// <param name="resourceId">The resource type to extract.</param>
        /// <param name="amount">The amount to extract.</param>
        /// <returns>The actual amount extracted.</returns>
        public int ExtractFromOutputPort(int portIndex, string resourceId, int amount)
        {
            if (portIndex < 0 || portIndex >= outputPorts.Count)
            {
                Debug.LogWarning($"Machine {machineId}: Invalid output port index {portIndex}");
                return 0;
            }
            
            int extracted = outputPorts[portIndex].ExtractResource(resourceId, amount);
            
            // Check if we can now continue processing
            if (extracted > 0 && currentState == MachineState.WaitingForOutput && HasOutputSpace())
            {
                TransitionToState(MachineState.Idle);
            }
            
            return extracted;
        }
        
        /// <summary>
        /// Gets the total amount of a specific resource across all input ports.
        /// Uses caching for performance optimization.
        /// </summary>
        /// <param name="resourceId">The resource type to query.</param>
        /// <returns>The total amount of the resource in all input buffers.</returns>
        protected virtual int GetInputBufferAmount(string resourceId)
        {
            if (_inputCacheDirty)
            {
                RebuildInputCache();
            }
            
            return _cachedInputTotals.TryGetValue(resourceId, out int amount) ? amount : 0;
        }
        
        /// <summary>
        /// Rebuilds the input buffer cache by aggregating all resources from all ports.
        /// </summary>
        private void RebuildInputCache()
        {
            _cachedInputTotals.Clear();
            
            foreach (var port in inputPorts)
            {
                if (port == null) continue;
                
                var allResources = port.GetAllResources();
                foreach (var kvp in allResources)
                {
                    if (_cachedInputTotals.ContainsKey(kvp.Key))
                        _cachedInputTotals[kvp.Key] += kvp.Value;
                    else
                        _cachedInputTotals[kvp.Key] = kvp.Value;
                }
            }
            
            _inputCacheDirty = false;
        }
        
        /// <summary>
        /// Removes a specified amount of a resource from the input buffers.
        /// Removes from ports sequentially until the amount is satisfied.
        /// </summary>
        /// <param name="resourceId">The resource type to remove.</param>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>True if the full amount was removed, false otherwise.</returns>
        protected virtual bool RemoveFromInputBuffer(string resourceId, int amount)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                Debug.LogWarning($"Machine {machineId}: Attempted to remove resource with empty ID");
                return false;
            }
            
            if (amount <= 0)
            {
                Debug.LogWarning($"Machine {machineId}: Attempted to remove invalid amount {amount}");
                return false;
            }
            
            int remaining = amount;
            
            foreach (var port in inputPorts)
            {
                int removed = port.RemoveResource(resourceId, remaining);
                remaining -= removed;
                
                if (remaining <= 0) break;
            }
            
            // Invalidate cache after modification
            _inputCacheDirty = true;
            
            return remaining == 0;
        }
        
        /// <summary>
        /// Adds a specified amount of a resource to the output buffers.
        /// Adds to the first available output port with space.
        /// </summary>
        /// <param name="resourceId">The resource type to add.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if the resource was added successfully, false if no space available.</returns>
        protected virtual bool AddToOutputBuffer(string resourceId, int amount)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                Debug.LogWarning($"Machine {machineId}: Attempted to add resource with empty ID");
                return false;
            }
            
            if (amount <= 0)
            {
                Debug.LogWarning($"Machine {machineId}: Attempted to add invalid amount {amount}");
                return false;
            }
            
            // Try to add to first available output port
            foreach (var port in outputPorts)
            {
                if (port.CanAddResource(resourceId, amount))
                {
                    port.AddResource(resourceId, amount);
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Checks if a specified amount of a resource can be added to any output buffer.
        /// </summary>
        /// <param name="resourceId">The resource type to check.</param>
        /// <param name="amount">The amount to check.</param>
        /// <returns>True if there is space in at least one output port, false otherwise.</returns>
        protected virtual bool CanAddToOutputBuffer(string resourceId, int amount)
        {
            foreach (var port in outputPorts)
            {
                if (port.CanAddResource(resourceId, amount))
                {
                    return true;
                }
            }
            return false;
        }
        
        #endregion
        
        #region Power Management
        
        /// <summary>
        /// Sets the powered state of the machine.
        /// Called by the power grid when power availability changes.
        /// Handles state transitions immediately when power changes.
        /// </summary>
        /// <param name="powered">True if power is available, false otherwise.</param>
        public virtual void SetPowered(bool powered)
        {
            if (isPowered == powered) return;
            
            isPowered = powered;
            OnPowerStatusChanged?.Invoke(powered);
            
            // Handle state transitions immediately when power changes
            if (!powered && currentState != MachineState.NoPower && currentState != MachineState.Disabled)
            {
                _resumingProcessing = (currentState == MachineState.Processing);
                TransitionToState(MachineState.NoPower);
            }
            else if (powered && currentState == MachineState.NoPower)
            {
                TransitionToState(previousState);
                _resumingProcessing = false;
            }
        }
        
        /// <summary>
        /// Registers this machine with the power grid.
        /// Placeholder for future PowerGrid integration.
        /// </summary>
        protected virtual void RegisterWithPowerGrid()
        {
            // TODO: Integrate with PowerGrid system when implemented
            // PowerGrid.Instance?.RegisterConsumer(this);
        }
        
        /// <summary>
        /// Unregisters this machine from the power grid.
        /// Placeholder for future PowerGrid integration.
        /// </summary>
        protected virtual void UnregisterFromPowerGrid()
        {
            // TODO: Integrate with PowerGrid system when implemented
            // PowerGrid.Instance?.UnregisterConsumer(this);
        }
        
        #endregion
        
        #region Grid Integration
        
        /// <summary>
        /// Sets the grid position of this machine.
        /// </summary>
        /// <param name="position">The grid position to set.</param>
        public virtual void SetGridPosition(Vector3Int position)
        {
            gridPosition = position;
        }
        
        /// <summary>
        /// Sets the rotation of this machine.
        /// Clamps rotation to 0-3 and updates visual representation.
        /// </summary>
        /// <param name="rot">The rotation value (0-3 for 0°, 90°, 180°, 270°).</param>
        public virtual void SetRotation(int rot)
        {
            rotation = Mathf.Clamp(rot, 0, 3);
            UpdateVisualRotation();
        }
        
        /// <summary>
        /// Gets all grid cells occupied by this machine.
        /// </summary>
        /// <returns>A list of all occupied grid positions.</returns>
        public virtual List<Vector3Int> GetOccupiedCells()
        {
            List<Vector3Int> cells = new List<Vector3Int>();
            
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int z = 0; z < gridSize.y; z++)
                {
                    cells.Add(gridPosition + new Vector3Int(x, 0, z));
                }
            }
            
            return cells;
        }
        
        /// <summary>
        /// Frees the grid cells occupied by this machine.
        /// Calls GridManager to release the cells.
        /// </summary>
        protected virtual void FreeGridCells()
        {
            if (GridManager.Instance != null)
            {
                GridManager.Instance.FreeCells(gridPosition, gridSize);
            }
        }
        
        /// <summary>
        /// Updates the visual rotation of the machine.
        /// Rotates the transform based on the rotation value.
        /// </summary>
        protected virtual void UpdateVisualRotation()
        {
            transform.rotation = Quaternion.Euler(0, rotation * 90f, 0);
        }
        
        #endregion
        
        #region Enable/Disable
        
        /// <summary>
        /// Sets the enabled state of the machine.
        /// When disabled, the machine transitions to Disabled state and stops processing.
        /// When enabled, the machine transitions back to Idle state.
        /// </summary>
        /// <param name="enabled">True to enable the machine, false to disable it.</param>
        public virtual void SetEnabled(bool enabled)
        {
            if (isEnabled == enabled) return;
            
            isEnabled = enabled;
            
            if (!enabled)
            {
                TransitionToState(MachineState.Disabled);
            }
            else if (currentState == MachineState.Disabled)
            {
                TransitionToState(MachineState.Idle);
            }
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validates the machine's configuration.
        /// Checks for common errors like missing MachineData, invalid ports, and invalid recipes.
        /// Logs warnings for any issues found.
        /// </summary>
        protected virtual void ValidateConfiguration()
        {
            // Check if MachineData is assigned
            if (machineData == null)
            {
                Debug.LogError($"Machine {gameObject.name} has no MachineData assigned!");
                return;
            }
            
            // Check if machine has at least one port
            if (inputPorts.Count == 0 && outputPorts.Count == 0)
            {
                Debug.LogWarning($"Machine {machineId} has no input or output ports!");
            }
            
            // Validate input ports
            for (int i = 0; i < inputPorts.Count; i++)
            {
                if (inputPorts[i] == null)
                {
                    Debug.LogError($"Machine {machineId} has null input port at index {i}!");
                }
            }
            
            // Validate output ports
            for (int i = 0; i < outputPorts.Count; i++)
            {
                if (outputPorts[i] == null)
                {
                    Debug.LogError($"Machine {machineId} has null output port at index {i}!");
                }
            }
            
            // Check if active recipe is valid
            if (activeRecipe != null && !IsRecipeAvailable(activeRecipe))
            {
                Debug.LogWarning($"Machine {machineId} has invalid active recipe '{activeRecipe.recipeId}'! Clearing recipe.");
                activeRecipe = null;
            }
            
            // Validate available recipes
            if (machineData.availableRecipes == null || machineData.availableRecipes.Count == 0)
            {
                Debug.LogWarning($"Machine {machineId} has no available recipes!");
            }
            else
            {
                // Check for null recipes in the list
                for (int i = 0; i < machineData.availableRecipes.Count; i++)
                {
                    if (machineData.availableRecipes[i] == null)
                    {
                        Debug.LogWarning($"Machine {machineId} has null recipe at index {i} in available recipes!");
                    }
                }
            }
            
            // Validate grid size
            if (gridSize.x <= 0 || gridSize.y <= 0)
            {
                Debug.LogError($"Machine {machineId} has invalid grid size: {gridSize}!");
            }
            
            // Validate tier
            if (tier < 1)
            {
                Debug.LogWarning($"Machine {machineId} has invalid tier: {tier}! Setting to 1.");
                tier = 1;
                CalculateMultipliers();
            }
            
            // Validate power consumption
            if (powerConsumption < 0)
            {
                Debug.LogWarning($"Machine {machineId} has negative power consumption: {powerConsumption}! Setting to 0.");
                powerConsumption = 0;
            }
        }
        
        #endregion
        
        #region Debug Helpers
        
        /// <summary>
        /// Returns a string representation of this machine for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"Machine[{machineId}] State:{currentState} Tier:{tier} Progress:{processingProgress:P0} Powered:{isPowered}";
        }
        
        #endregion
    }
}
