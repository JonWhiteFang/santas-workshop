# Design Document: Machine Framework

## Overview

The Machine Framework provides the foundational architecture for all machines in Santa's Workshop Automation. This system establishes a robust, extensible base that all machine types (extractors, processors, assemblers, utility buildings) inherit from. The framework implements a state machine pattern for predictable behavior, a port-based system for resource flow, recipe processing with validation, and power consumption interfaces.

The design emphasizes modularity, performance, and ease of extension. By providing a solid base class and clear interfaces, the framework enables rapid development of new machine types while maintaining consistency across the game.

## Architecture

### System Components

```
┌─────────────────────────────────────────────────────────────┐
│                    Machine Framework                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────┐         ┌──────────────────┐        │
│  │   MachineBase    │◄────────┤  MachineData     │        │
│  │  (Abstract)      │         │ (ScriptableObj)  │        │
│  └────────┬─────────┘         └──────────────────┘        │
│           │                                                 │
│           │ implements                                      │
│           ▼                                                 │
│  ┌──────────────────┐         ┌──────────────────┐        │
│  │ IPowerConsumer   │         │  State Machine   │        │
│  │   (Interface)    │         │    Pattern       │        │
│  └──────────────────┘         └──────────────────┘        │
│           │                            │                    │
│           │                            │                    │
│  ┌────────┴────────┐         ┌────────┴────────┐          │
│  │  Input/Output   │         │  Recipe System  │          │
│  │     Ports       │         │                 │          │
│  └─────────────────┘         └─────────────────┘          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
         │                    │                    │
         ▼                    ▼                    ▼
   ┌─────────┐          ┌─────────┐         ┌─────────┐
   │Extractor│          │Processor│         │Assembler│
   └─────────┘          └─────────┘         └─────────┘
```

### State Machine Flow

```
                    ┌──────────┐
                    │   Idle   │
                    └────┬─────┘
                         │
                         ▼
              ┌──────────────────┐
              │ WaitingForInput  │◄──────┐
              └────┬─────────────┘       │
                   │                     │
                   ▼                     │
              ┌──────────┐               │
              │Processing│               │
              └────┬─────┘               │
                   │                     │
                   ▼                     │
           ┌───────────────┐            │
           │WaitingForOutput│───────────┘
           └───────┬───────┘
                   │
                   ▼
              ┌─────────┐
              │  Idle   │
              └─────────┘

         Power Loss ──► NoPower ──► (Previous State)
         Disabled ────► Disabled ──► (Previous State)
```

## Components and Interfaces

### 1. MachineBase (Abstract MonoBehaviour)

The core abstract class that all machines inherit from:

```csharp
using UnityEngine;
using System;
using System.Collections.Generic;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    public abstract class MachineBase : MonoBehaviour, IPowerConsumer
    {
        // Identification
        [Header("Machine Identity")]
        [SerializeField] protected string machineId;
        [SerializeField] protected MachineData machineData;
        
        // Grid Integration
        protected Vector3Int gridPosition;
        protected Vector2Int gridSize;
        protected int rotation; // 0-3 for 0°, 90°, 180°, 270°
        
        // State Management
        protected MachineState currentState = MachineState.Idle;
        protected MachineState previousState = MachineState.Idle;
        
        // Ports
        protected List<InputPort> inputPorts = new List<InputPort>();
        protected List<OutputPort> outputPorts = new List<OutputPort>();
        
        // Recipe Processing
        protected Recipe activeRecipe;
        protected float processingProgress = 0f;
        protected float processingTimeRemaining = 0f;
        
        // Power
        protected bool isPowered = true;
        protected float powerConsumption = 0f;
        
        // Tier and Multipliers
        protected int tier = 1;
        protected float speedMultiplier = 1f;
        protected float efficiencyMultiplier = 1f;
        
        // Enabled State
        protected bool isEnabled = true;
        
        // Events
        public event Action<MachineState, MachineState> OnStateChanged;
        public event Action<Recipe> OnProcessingStarted;
        public event Action<Recipe> OnProcessingCompleted;
        public event Action<bool> OnPowerStatusChanged;
        
        // Properties
        public string MachineId => machineId;
        public MachineState CurrentState => currentState;
        public float ProcessingProgress => processingProgress;
        public float EstimatedTimeRemaining => processingTimeRemaining;
        public int Tier => tier;
        public Vector3Int GridPosition => gridPosition;
        public Vector2Int GridSize => gridSize;
        public int Rotation => rotation;
        
        // IPowerConsumer Implementation
        public float PowerConsumption => isPowered && currentState == MachineState.Processing 
            ? powerConsumption * efficiencyMultiplier 
            : 0f;
        public bool IsPowered => isPowered;
        
        #region Lifecycle
        
        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(machineId))
            {
                machineId = System.Guid.NewGuid().ToString();
            }
            
            InitializeFromData();
        }
        
        protected virtual void Start()
        {
            RegisterWithPowerGrid();
            ValidateConfiguration();
        }
        
        protected virtual void Update()
        {
            if (!isEnabled) return;
            
            UpdateStateMachine();
        }
        
        protected virtual void OnDestroy()
        {
            UnregisterFromPowerGrid();
            FreeGridCells();
        }
        
        #endregion
        
        #region Initialization
        
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
        
        protected virtual void InitializePorts()
        {
            // Create input ports
            for (int i = 0; i < machineData.inputPortCount; i++)
            {
                inputPorts.Add(new InputPort(
                    $"{machineId}_input_{i}",
                    machineData.inputPortPositions[i],
                    machineData.bufferCapacity
                ));
            }
            
            // Create output ports
            for (int i = 0; i < machineData.outputPortCount; i++)
            {
                outputPorts.Add(new OutputPort(
                    $"{machineId}_output_{i}",
                    machineData.outputPortPositions[i],
                    machineData.bufferCapacity
                ));
            }
        }
        
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
        
        protected virtual void OnStateExit(MachineState state)
        {
            switch (state)
            {
                case MachineState.Processing:
                    OnExitProcessing();
                    break;
            }
        }
        
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
        
        #endregion
        
        #region State Implementations
        
        protected virtual void OnEnterIdle() { }
        protected virtual void OnEnterWaitingForInput() { }
        protected virtual void OnEnterWaitingForOutput() { }
        protected virtual void OnEnterNoPower() { }
        protected virtual void OnEnterDisabled() { }
        
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
        
        protected virtual void OnExitProcessing()
        {
            processingProgress = 0f;
            processingTimeRemaining = 0f;
        }
        
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
        
        protected virtual void UpdateWaitingForInput()
        {
            // Check if inputs are now available
            if (CanProcessRecipe(activeRecipe))
            {
                TransitionToState(MachineState.Processing);
            }
        }
        
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
        
        protected virtual void UpdateWaitingForOutput()
        {
            // Check if output buffer has space
            if (HasOutputSpace())
            {
                TransitionToState(MachineState.Idle);
            }
        }
        
        #endregion
        
        #region Recipe Processing
        
        public virtual void SetActiveRecipe(Recipe recipe)
        {
            if (recipe == null)
            {
                activeRecipe = null;
                return;
            }
            
            // Validate recipe is available for this machine
            if (!IsRecipeAvailable(recipe))
            {
                Debug.LogWarning($"Recipe {recipe.recipeId} is not available for machine {machineId}");
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
        
        public virtual bool CanProcessRecipe(Recipe recipe)
        {
            if (recipe == null) return false;
            if (!isPowered) return false;
            if (!isEnabled) return false;
            
            // Check input availability
            if (!HasRequiredInputs(recipe)) return false;
            
            // Check output space
            if (!HasOutputSpace(recipe)) return false;
            
            return true;
        }
        
        protected virtual bool HasRequiredInputs(Recipe recipe)
        {
            foreach (var input in recipe.inputs)
            {
                int availableAmount = GetInputBufferAmount(input.resourceId);
                if (availableAmount < input.amount)
                {
                    return false;
                }
            }
            return true;
        }
        
        protected virtual bool HasOutputSpace(Recipe recipe)
        {
            foreach (var output in recipe.outputs)
            {
                if (!CanAddToOutputBuffer(output.resourceId, output.amount))
                {
                    return false;
                }
            }
            return true;
        }
        
        protected virtual bool HasOutputSpace()
        {
            if (activeRecipe == null) return true;
            return HasOutputSpace(activeRecipe);
        }
        
        protected virtual void CompleteProcessing()
        {
            if (activeRecipe == null) return;
            
            // Consume inputs
            ConsumeInputs(activeRecipe);
            
            // Produce outputs
            ProduceOutputs(activeRecipe);
            
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
        
        protected virtual void CancelProcessing()
        {
            // Refund inputs (they weren't consumed yet)
            processingProgress = 0f;
            processingTimeRemaining = 0f;
        }
        
        protected virtual void ConsumeInputs(Recipe recipe)
        {
            foreach (var input in recipe.inputs)
            {
                RemoveFromInputBuffer(input.resourceId, input.amount);
            }
        }
        
        protected virtual void ProduceOutputs(Recipe recipe)
        {
            foreach (var output in recipe.outputs)
            {
                AddToOutputBuffer(output.resourceId, output.amount);
            }
        }
        
        protected virtual bool IsRecipeAvailable(Recipe recipe)
        {
            if (machineData == null) return false;
            return machineData.availableRecipes.Contains(recipe);
        }
        
        public virtual List<Recipe> GetAvailableRecipes()
        {
            if (machineData == null) return new List<Recipe>();
            return new List<Recipe>(machineData.availableRecipes);
        }
        
        #endregion
        
        #region Buffer Management
        
        protected virtual int GetInputBufferAmount(string resourceId)
        {
            int total = 0;
            foreach (var port in inputPorts)
            {
                total += port.GetResourceAmount(resourceId);
            }
            return total;
        }
        
        protected virtual bool RemoveFromInputBuffer(string resourceId, int amount)
        {
            int remaining = amount;
            
            foreach (var port in inputPorts)
            {
                int removed = port.RemoveResource(resourceId, remaining);
                remaining -= removed;
                
                if (remaining <= 0) break;
            }
            
            return remaining == 0;
        }
        
        protected virtual bool AddToOutputBuffer(string resourceId, int amount)
        {
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
        
        public virtual void SetPowered(bool powered)
        {
            if (isPowered == powered) return;
            
            isPowered = powered;
            OnPowerStatusChanged?.Invoke(powered);
            
            if (!powered && currentState != MachineState.NoPower && currentState != MachineState.Disabled)
            {
                TransitionToState(MachineState.NoPower);
            }
            else if (powered && currentState == MachineState.NoPower)
            {
                TransitionToState(previousState);
            }
        }
        
        protected virtual void RegisterWithPowerGrid()
        {
            // TODO: Register with PowerGrid when implemented
        }
        
        protected virtual void UnregisterFromPowerGrid()
        {
            // TODO: Unregister from PowerGrid when implemented
        }
        
        #endregion
        
        #region Grid Integration
        
        public virtual void SetGridPosition(Vector3Int position)
        {
            gridPosition = position;
        }
        
        public virtual void SetRotation(int rot)
        {
            rotation = Mathf.Clamp(rot, 0, 3);
            UpdateVisualRotation();
        }
        
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
        
        protected virtual void FreeGridCells()
        {
            if (GridManager.Instance != null)
            {
                GridManager.Instance.FreeCells(gridPosition, gridSize);
            }
        }
        
        #endregion
        
        #region Enable/Disable
        
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
        
        #region Visual Feedback
        
        protected virtual void SetVisualState(MachineState state)
        {
            // Override in derived classes to update visual indicators
        }
        
        protected virtual void UpdateVisualRotation()
        {
            transform.rotation = Quaternion.Euler(0, rotation * 90f, 0);
        }
        
        #endregion
        
        #region Validation
        
        protected virtual void ValidateConfiguration()
        {
            if (machineData == null)
            {
                Debug.LogError($"Machine {gameObject.name} has no MachineData assigned!");
            }
            
            if (inputPorts.Count == 0 && outputPorts.Count == 0)
            {
                Debug.LogWarning($"Machine {machineId} has no input or output ports!");
            }
            
            if (activeRecipe != null && !IsRecipeAvailable(activeRecipe))
            {
                Debug.LogWarning($"Machine {machineId} has invalid active recipe!");
                activeRecipe = null;
            }
        }
        
        #endregion
        
        #region Save/Load
        
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
        
        public virtual void LoadSaveData(MachineSaveData saveData)
        {
            machineId = saveData.machineId;
            tier = saveData.tier;
            gridPosition = saveData.gridPosition;
            rotation = saveData.rotation;
            processingProgress = saveData.processingProgress;
            isEnabled = saveData.isEnabled;
            
            // Load recipe
            if (!string.IsNullOrEmpty(saveData.activeRecipeId))
            {
                // TODO: Load recipe from database
            }
            
            // Load buffers
            LoadInputBufferSaveData(saveData.inputBuffers);
            LoadOutputBufferSaveData(saveData.outputBuffers);
            
            // Restore state
            TransitionToState(saveData.currentState);
        }
        
        protected virtual List<BufferSaveData> GetInputBufferSaveData()
        {
            List<BufferSaveData> data = new List<BufferSaveData>();
            foreach (var port in inputPorts)
            {
                data.Add(port.GetSaveData());
            }
            return data;
        }
        
        protected virtual List<BufferSaveData> GetOutputBufferSaveData()
        {
            List<BufferSaveData> data = new List<BufferSaveData>();
            foreach (var port in outputPorts)
            {
                data.Add(port.GetSaveData());
            }
            return data;
        }
        
        protected virtual void LoadInputBufferSaveData(List<BufferSaveData> data)
        {
            for (int i = 0; i < Mathf.Min(data.Count, inputPorts.Count); i++)
            {
                inputPorts[i].LoadSaveData(data[i]);
            }
        }
        
        protected virtual void LoadOutputBufferSaveData(List<BufferSaveData> data)
        {
            for (int i = 0; i < Mathf.Min(data.Count, outputPorts.Count); i++)
            {
                outputPorts[i].LoadSaveData(data[i]);
            }
        }
        
        #endregion
    }
}
```

### 2. MachineState Enum

```csharp
namespace SantasWorkshop.Machines
{
    public enum MachineState
    {
        Idle,              // No work to do, ready to start
        WaitingForInput,   // Recipe selected, waiting for resources
        Processing,        // Actively converting inputs to outputs
        WaitingForOutput,  // Product ready, output buffer full
        NoPower,           // Insufficient electricity
        Disabled           // Manually disabled by player
    }
}
```

### 3. IPowerConsumer Interface

```csharp
namespace SantasWorkshop.Machines
{
    public interface IPowerConsumer
    {
        float PowerConsumption { get; }
        bool IsPowered { get; }
        void SetPowered(bool powered);
    }
}
```

### 4. InputPort Class

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Machines
{
    [System.Serializable]
    public class InputPort
    {
        public string portId;
        public Vector3 localPosition;
        public int capacity;
        
        private Dictionary<string, int> buffer = new Dictionary<string, int>();
        
        public InputPort(string id, Vector3 position, int cap)
        {
            portId = id;
            localPosition = position;
            capacity = cap;
        }
        
        public bool CanAcceptResource(string resourceId, int amount)
        {
            int currentTotal = GetTotalAmount();
            return currentTotal + amount <= capacity;
        }
        
        public bool AddResource(string resourceId, int amount)
        {
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
        
        public int RemoveResource(string resourceId, int amount)
        {
            if (!buffer.ContainsKey(resourceId))
            {
                return 0;
            }
            
            int available = buffer[resourceId];
            int toRemove = Mathf.Min(available, amount);
            
            buffer[resourceId] -= toRemove;
            
            if (buffer[resourceId] <= 0)
            {
                buffer.Remove(resourceId);
            }
            
            return toRemove;
        }
        
        public int GetResourceAmount(string resourceId)
        {
            return buffer.ContainsKey(resourceId) ? buffer[resourceId] : 0;
        }
        
        public int GetTotalAmount()
        {
            int total = 0;
            foreach (var kvp in buffer)
            {
                total += kvp.Value;
            }
            return total;
        }
        
        public int GetAvailableSpace()
        {
            return capacity - GetTotalAmount();
        }
        
        public BufferSaveData GetSaveData()
        {
            return new BufferSaveData
            {
                portId = portId,
                contents = new Dictionary<string, int>(buffer)
            };
        }
        
        public void LoadSaveData(BufferSaveData data)
        {
            buffer = new Dictionary<string, int>(data.contents);
        }
    }
}
```

### 5. OutputPort Class

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Machines
{
    [System.Serializable]
    public class OutputPort
    {
        public string portId;
        public Vector3 localPosition;
        public int capacity;
        
        private Dictionary<string, int> buffer = new Dictionary<string, int>();
        
        public OutputPort(string id, Vector3 position, int cap)
        {
            portId = id;
            localPosition = position;
            capacity = cap;
        }
        
        public bool CanAddResource(string resourceId, int amount)
        {
            int currentTotal = GetTotalAmount();
            return currentTotal + amount <= capacity;
        }
        
        public bool AddResource(string resourceId, int amount)
        {
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
        
        public int ExtractResource(string resourceId, int amount)
        {
            if (!buffer.ContainsKey(resourceId))
            {
                return 0;
            }
            
            int available = buffer[resourceId];
            int toExtract = Mathf.Min(available, amount);
            
            buffer[resourceId] -= toExtract;
            
            if (buffer[resourceId] <= 0)
            {
                buffer.Remove(resourceId);
            }
            
            return toExtract;
        }
        
        public int GetResourceAmount(string resourceId)
        {
            return buffer.ContainsKey(resourceId) ? buffer[resourceId] : 0;
        }
        
        public int GetTotalAmount()
        {
            int total = 0;
            foreach (var kvp in buffer)
            {
                total += kvp.Value;
            }
            return total;
        }
        
        public int GetAvailableSpace()
        {
            return capacity - GetTotalAmount();
        }
        
        public bool HasResources()
        {
            return buffer.Count > 0;
        }
        
        public BufferSaveData GetSaveData()
        {
            return new BufferSaveData
            {
                portId = portId,
                contents = new Dictionary<string, int>(buffer)
            };
        }
        
        public void LoadSaveData(BufferSaveData data)
        {
            buffer = new Dictionary<string, int>(data.contents);
        }
    }
}
```


### 6. Recipe Class

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Data
{
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "Santa/Recipe")]
    public class Recipe : ScriptableObject
    {
        [Header("Identification")]
        public string recipeId;
        public string recipeName;
        
        [Header("Inputs")]
        public ResourceStack[] inputs;
        
        [Header("Outputs")]
        public ResourceStack[] outputs;
        
        [Header("Processing")]
        public float processingTime = 1f;
        public float powerConsumption = 10f;
        
        [Header("Requirements")]
        public int requiredTier = 1;
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(recipeId))
            {
                recipeId = name;
            }
            
            if (inputs == null || inputs.Length == 0)
            {
                Debug.LogWarning($"Recipe {recipeName} has no inputs!");
            }
            
            if (outputs == null || outputs.Length == 0)
            {
                Debug.LogWarning($"Recipe {recipeName} has no outputs!");
            }
        }
    }
}
```

### 7. MachineData ScriptableObject

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace SantasWorkshop.Data
{
    [CreateAssetMenu(fileName = "NewMachineData", menuName = "Santa/Machine Data")]
    public class MachineData : ScriptableObject
    {
        [Header("Basic Info")]
        public string machineName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;
        
        [Header("Grid Properties")]
        public Vector2Int gridSize = new Vector2Int(1, 1);
        public int tier = 1;
        
        [Header("Performance")]
        public float baseProcessingSpeed = 1f;
        public float basePowerConsumption = 10f;
        
        [Header("Ports")]
        public int inputPortCount = 1;
        public int outputPortCount = 1;
        public Vector3[] inputPortPositions;
        public Vector3[] outputPortPositions;
        
        [Header("Buffers")]
        public int bufferCapacity = 10;
        
        [Header("Recipes")]
        public List<Recipe> availableRecipes = new List<Recipe>();
        
        [Header("Visual")]
        public GameObject prefab;
        
        private void OnValidate()
        {
            if (inputPortPositions == null || inputPortPositions.Length != inputPortCount)
            {
                inputPortPositions = new Vector3[inputPortCount];
                for (int i = 0; i < inputPortCount; i++)
                {
                    inputPortPositions[i] = new Vector3(-0.5f, 0.5f, 0);
                }
            }
            
            if (outputPortPositions == null || outputPortPositions.Length != outputPortCount)
            {
                outputPortPositions = new Vector3[outputPortCount];
                for (int i = 0; i < outputPortCount; i++)
                {
                    outputPortPositions[i] = new Vector3(0.5f, 0.5f, 0);
                }
            }
        }
    }
}
```

### 8. Save Data Structures

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Machines
{
    [Serializable]
    public struct MachineSaveData
    {
        public string machineId;
        public string machineType;
        public int tier;
        public Vector3Int gridPosition;
        public int rotation;
        public MachineState currentState;
        public float processingProgress;
        public string activeRecipeId;
        public List<BufferSaveData> inputBuffers;
        public List<BufferSaveData> outputBuffers;
        public bool isEnabled;
    }
    
    [Serializable]
    public struct BufferSaveData
    {
        public string portId;
        public Dictionary<string, int> contents;
    }
}
```

## Data Models

### Machine Hierarchy

```
MachineBase (Abstract)
├── ExtractorBase (Abstract)
│   ├── MiningDrill
│   ├── WoodHarvester
│   └── CoalMine
├── ProcessorBase (Abstract)
│   ├── Smelter
│   ├── Sawmill
│   └── Refinery
├── AssemblerBase (Abstract)
│   ├── BasicAssembler
│   ├── AdvancedAssembler
│   └── ToyAssembler
└── UtilityBase (Abstract)
    ├── StorageChest
    ├── PowerGenerator
    └── MagicInfuser
```

### State Transition Rules

```
Idle → WaitingForInput: Recipe selected but inputs not available
Idle → Processing: Recipe selected and all requirements met

WaitingForInput → Processing: Required inputs become available
WaitingForInput → Idle: Recipe deselected

Processing → WaitingForOutput: Processing complete but output buffer full
Processing → Idle: Processing complete and output buffer has space
Processing → NoPower: Power lost during processing (progress preserved)

WaitingForOutput → Idle: Output buffer space becomes available

NoPower → (Previous State): Power restored

Any State → Disabled: Player disables machine
Disabled → Idle: Player enables machine
```

## Error Handling

### Common Error Scenarios

1. **Missing MachineData**
   - Detection: Check in Awake()
   - Handling: Log error, disable machine, show warning in inspector
   
2. **Invalid Recipe**
   - Detection: Validate when setting active recipe
   - Handling: Log warning, reject recipe, maintain current state
   
3. **Buffer Overflow**
   - Detection: Check before adding resources
   - Handling: Reject addition, transition to WaitingForOutput if needed
   
4. **Power Loss During Processing**
   - Detection: Check in Update()
   - Handling: Transition to NoPower, preserve progress, resume when power returns
   
5. **Null Port References**
   - Detection: Validate in InitializePorts()
   - Handling: Log error, create default ports, continue operation

### Error Recovery

```csharp
protected virtual void HandleError(string errorMessage, ErrorSeverity severity)
{
    Debug.LogError($"Machine {machineId}: {errorMessage}");
    
    switch (severity)
    {
        case ErrorSeverity.Warning:
            // Continue operation
            break;
            
        case ErrorSeverity.Error:
            // Transition to safe state
            TransitionToState(MachineState.Idle);
            break;
            
        case ErrorSeverity.Critical:
            // Disable machine
            SetEnabled(false);
            break;
    }
}

public enum ErrorSeverity
{
    Warning,
    Error,
    Critical
}
```

## Testing Strategy

### Unit Tests

1. **State Machine Tests**
   ```csharp
   [Test]
   public void TransitionToState_ChangesState()
   {
       var machine = CreateTestMachine();
       machine.TransitionToState(MachineState.Processing);
       Assert.AreEqual(MachineState.Processing, machine.CurrentState);
   }
   
   [Test]
   public void TransitionToState_FiresEvent()
   {
       var machine = CreateTestMachine();
       bool eventFired = false;
       machine.OnStateChanged += (old, newState) => eventFired = true;
       
       machine.TransitionToState(MachineState.Processing);
       Assert.IsTrue(eventFired);
   }
   ```

2. **Recipe Processing Tests**
   ```csharp
   [Test]
   public void CanProcessRecipe_WithSufficientInputs_ReturnsTrue()
   {
       var machine = CreateTestMachine();
       var recipe = CreateTestRecipe();
       AddInputsToMachine(machine, recipe.inputs);
       
       Assert.IsTrue(machine.CanProcessRecipe(recipe));
   }
   
   [Test]
   public void CompleteProcessing_ConsumesInputs_ProducesOutputs()
   {
       var machine = CreateTestMachine();
       var recipe = CreateTestRecipe();
       AddInputsToMachine(machine, recipe.inputs);
       
       machine.SetActiveRecipe(recipe);
       machine.TransitionToState(MachineState.Processing);
       SimulateProcessing(machine, recipe.processingTime);
       
       Assert.AreEqual(0, machine.GetInputBufferAmount(recipe.inputs[0].resourceId));
       Assert.Greater(machine.GetOutputBufferAmount(recipe.outputs[0].resourceId), 0);
   }
   ```

3. **Buffer Tests**
   ```csharp
   [Test]
   public void InputPort_AddResource_IncreasesAmount()
   {
       var port = new InputPort("test", Vector3.zero, 10);
       port.AddResource("wood", 5);
       
       Assert.AreEqual(5, port.GetResourceAmount("wood"));
   }
   
   [Test]
   public void InputPort_AddResource_BeyondCapacity_ReturnsFalse()
   {
       var port = new InputPort("test", Vector3.zero, 10);
       port.AddResource("wood", 8);
       
       bool result = port.AddResource("wood", 5);
       Assert.IsFalse(result);
   }
   ```

4. **Power Tests**
   ```csharp
   [Test]
   public void SetPowered_False_TransitionsToNoPower()
   {
       var machine = CreateTestMachine();
       machine.TransitionToState(MachineState.Processing);
       
       machine.SetPowered(false);
       
       Assert.AreEqual(MachineState.NoPower, machine.CurrentState);
   }
   
   [Test]
   public void SetPowered_True_RestoresPreviousState()
   {
       var machine = CreateTestMachine();
       machine.TransitionToState(MachineState.Processing);
       machine.SetPowered(false);
       
       machine.SetPowered(true);
       
       Assert.AreEqual(MachineState.Processing, machine.CurrentState);
   }
   ```

### Integration Tests

1. **Full Processing Cycle**
   - Add inputs → Set recipe → Start processing → Complete → Extract outputs
   
2. **Multi-Recipe Machine**
   - Switch recipes → Validate cancellation → Resume with new recipe
   
3. **Power Grid Integration**
   - Register with power grid → Consume power → Handle brownouts
   
4. **Save/Load**
   - Save machine state → Load → Verify state restoration

### Performance Tests

```csharp
[Test]
public void Performance_100Machines_Update_CompletesQuickly()
{
    var machines = CreateTestMachines(100);
    
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    foreach (var machine in machines)
    {
        machine.Update();
    }
    
    stopwatch.Stop();
    
    Assert.Less(stopwatch.ElapsedMilliseconds, 16); // 60 FPS = 16ms per frame
}
```

## Performance Considerations

### Optimization Strategies

1. **State Machine Updates**
   - Only update machines in active states (Processing, WaitingForInput)
   - Skip updates for Idle and Disabled machines
   - Use spatial partitioning to update only visible machines

2. **Buffer Operations**
   - Use Dictionary for O(1) lookups
   - Cache total amounts to avoid recalculation
   - Batch buffer operations when possible

3. **Event Invocation**
   - Use null-conditional operator (?.) to avoid null checks
   - Consider event pooling for high-frequency events
   - Batch events when multiple changes occur

4. **Recipe Validation**
   - Cache validation results for current frame
   - Invalidate cache only on state changes
   - Use early-out checks (power, enabled state)

### Memory Management

- **Object Pooling**: Reuse port objects for machines with dynamic port counts
- **Struct Usage**: Use structs for save data to reduce heap allocations
- **Dictionary Sizing**: Pre-allocate dictionary capacity based on expected usage
- **Event Cleanup**: Unsubscribe from events in OnDestroy to prevent memory leaks

### Performance Targets

- **Update Time**: < 0.1ms per machine per frame
- **State Transition**: < 0.05ms
- **Recipe Validation**: < 0.02ms
- **Buffer Operation**: < 0.01ms
- **Save/Load**: < 1ms per machine

## Future Extensibility

### Planned Extensions

1. **Advanced Recipes**
   - Probabilistic outputs (e.g., 80% chance of bonus output)
   - Quality-based processing (higher quality inputs → better outputs)
   - Multi-stage recipes (intermediate steps)

2. **Machine Upgrades**
   - Module slots for upgrades (speed, efficiency, capacity)
   - Visual upgrades (particle effects, animations)
   - Unlock new recipes through upgrades

3. **Machine Networks**
   - Direct machine-to-machine connections (no conveyors)
   - Shared buffers between machines
   - Coordinated processing (assembly lines)

4. **Dynamic Recipes**
   - Player-created recipes
   - Recipe discovery through experimentation
   - Seasonal recipe variations

### Extension Points

```csharp
// Custom machine behaviors
public abstract class MachineBase
{
    protected virtual void OnCustomUpdate() { }
    protected virtual bool CustomValidation() { return true; }
    protected virtual void OnCustomStateEnter(MachineState state) { }
}

// Recipe modifiers
public interface IRecipeModifier
{
    float ModifyProcessingTime(float baseTime);
    float ModifyPowerConsumption(float basePower);
    ResourceStack[] ModifyOutputs(ResourceStack[] baseOutputs);
}

// Port extensions
public interface IAdvancedPort
{
    bool SupportsFiltering { get; }
    void SetResourceFilter(List<string> allowedResources);
    int GetPriority();
}
```

## Integration Points

### Resource System Integration

```csharp
// Consume resources from global inventory (for machines without input conveyors)
protected virtual bool ConsumeFromGlobalInventory(Recipe recipe)
{
    return ResourceManager.Instance.TryConsumeResources(recipe.inputs);
}

// Add outputs to global inventory (for machines without output conveyors)
protected virtual void AddToGlobalInventory(Recipe recipe)
{
    ResourceManager.Instance.AddResources(recipe.outputs);
}
```

### Grid System Integration

```csharp
// Register machine with grid on placement
public override void SetGridPosition(Vector3Int position)
{
    base.SetGridPosition(position);
    GridManager.Instance.OccupyCells(position, gridSize, gameObject);
}

// Get world position for port connections
public Vector3 GetPortWorldPosition(InputPort port)
{
    Vector3 localPos = RotateVector(port.localPosition, rotation);
    return GridManager.Instance.GridToWorld(gridPosition) + localPos;
}
```

### Power Grid Integration (Future)

```csharp
// Register as power consumer
protected override void RegisterWithPowerGrid()
{
    PowerGrid.Instance.RegisterConsumer(this);
}

// Update power consumption based on state
protected override void UpdateStateMachine()
{
    base.UpdateStateMachine();
    PowerGrid.Instance.UpdateConsumption(this, PowerConsumption);
}
```

## Visual Design

### State Indicators

```
Idle: Dim ambient light, no particles
WaitingForInput: Pulsing yellow indicator above machine
Processing: Bright light, active particles, rotating parts
WaitingForOutput: Pulsing red indicator, steam/smoke effects
NoPower: Flickering red light, warning icon
Disabled: Greyed out, no effects
```

### Port Visualization

```
Input Ports: Blue glowing connection point
Output Ports: Green glowing connection point
Active Transfer: Particle stream from port to conveyor
Full Buffer: Pulsing red glow on port
```

## Implementation Notes

### Initialization Order

1. Awake(): Create machine ID, initialize from MachineData
2. Start(): Register with managers (Power, Grid), validate configuration
3. First Update(): Begin state machine operation

### State Preservation

When power is lost during processing:
- Store current progress (processingProgress)
- Store previous state (previousState)
- On power restore, resume from stored progress

### Recipe Switching

When switching recipes mid-processing:
- Cancel current processing
- Refund consumed inputs (they weren't actually consumed yet)
- Clear processing progress
- Set new recipe
- Transition to Idle

### Multi-Cell Rotation

For machines larger than 1x1:
- Rotation affects which cells are occupied
- Port positions rotate around machine center
- Visual representation rotates to match

## Summary

The Machine Framework provides a robust, extensible foundation for all machines in Santa's Workshop Automation. Key design decisions include:

1. **State Machine Pattern**: Clear, predictable behavior with well-defined transitions
2. **Port-Based I/O**: Flexible resource flow supporting various connection patterns
3. **Recipe System**: Data-driven production with validation and progress tracking
4. **Power Integration**: Standardized interface for electricity consumption
5. **Buffer Management**: Separate input/output buffers with capacity limits
6. **Tier System**: Built-in support for machine upgrades and progression
7. **Event-Driven**: Decoupled communication with other systems
8. **Save/Load Support**: Complete state persistence
9. **Performance-Focused**: Optimized for hundreds of active machines
10. **Extensible**: Clear extension points for future features

The framework is ready for implementation and will serve as the base for all machine types in the game.
