# Machine Framework Developer Guide

**Last Updated**: November 5, 2025  
**Version**: 1.0  
**Status**: Complete

This guide provides comprehensive documentation for the Machine Framework system, including how to create custom machines, work with recipes, and integrate with the game's core systems.

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Creating Custom Machines](#creating-custom-machines)
4. [Working with Recipes](#working-with-recipes)
5. [MachineData Configuration](#machinedata-configuration)
6. [State Machine System](#state-machine-system)
7. [Buffer Management](#buffer-management)
8. [Power Integration](#power-integration)
9. [Grid Integration](#grid-integration)
10. [Save/Load System](#saveload-system)
11. [Best Practices](#best-practices)
12. [Common Patterns](#common-patterns)
13. [Troubleshooting](#troubleshooting)

---

## Overview

The Machine Framework provides a flexible, extensible system for creating automated machines in Santa's Workshop Automation. All machines inherit from `MachineBase` and follow a consistent state machine pattern.

**Key Features**:
- State-based machine behavior (Idle, Working, Blocked, NoPower, Disabled)
- Recipe-based processing with input/output buffers
- Power consumption and efficiency calculations
- Grid-based placement and rotation
- Save/load support
- Event-driven architecture

**Base Classes**:
- `MachineBase` - Abstract base for all machines
- `ExtractorBase` - For resource extraction machines (mining drills, harvesters)
- `ProcessorBase` - For single-recipe processing machines (smelters, sawmills)
- `AssemblerBase` - For multi-input assembly machines (toy assemblers)

---

## Architecture

### Class Hierarchy

```
MonoBehaviour
└── MachineBase (abstract)
    ├── ExtractorBase (abstract)
    │   └── MiningDrill (concrete)
    ├── ProcessorBase (abstract)
    │   ├── Smelter (concrete)
    │   └── Sawmill (concrete)
    └── AssemblerBase (abstract)
        └── ToyAssembler (concrete)
```

### Core Components

1. **MachineBase**: Core machine logic, state machine, power management
2. **InputPort/OutputPort**: Buffer management for resources
3. **MachineData**: ScriptableObject configuration
4. **RecipeData**: ScriptableObject recipe definitions
5. **MachineState**: Enum defining machine states
6. **IPowerConsumer**: Interface for power grid integration

---

## Creating Custom Machines

### Step 1: Choose the Right Base Class

**ExtractorBase** - Use when:
- Machine extracts resources from the world (mining drills, harvesters)
- No input resources required
- Produces output resources continuously

**ProcessorBase** - Use when:
- Machine processes single-input recipes (smelters, refineries)
- Simple input → output transformation
- Uses queued buffer system

**AssemblerBase** - Use when:
- Machine combines multiple inputs (toy assemblers)
- Complex recipes with many ingredients
- Uses inventory-based system

### Step 2: Create the Machine Script

#### Example: Mining Drill (ExtractorBase)

```csharp
using UnityEngine;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Mining drill that extracts ore from resource nodes.
    /// </summary>
    public class MiningDrill : ExtractorBase
    {
        #region Serialized Fields
        
        [Header("Mining Drill Settings")]
        [SerializeField] private ResourceType oreType = ResourceType.IronOre;
        [SerializeField] private int orePerExtraction = 1;
        
        #endregion
        
        #region Initialization
        
        public override void Initialize(MachineData data)
        {
            base.Initialize(data);
            
            // Custom initialization
            if (showDebugInfo)
            {
                Debug.Log($"[MiningDrill] {MachineId} initialized for {oreType}");
            }
        }
        
        #endregion
        
        #region Resource Extraction
        
        protected override void OnResourceExtracted()
        {
            base.OnResourceExtracted();
            
            // Add ore to output buffer or ResourceManager
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(oreType, orePerExtraction);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[MiningDrill] {MachineId} extracted {orePerExtraction}x {oreType}");
                }
            }
        }
        
        #endregion
        
        #region Visual Feedback
        
        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            
            // Update drill animation based on state
            if (animator != null)
            {
                animator.SetBool("IsWorking", currentState == MachineState.Working);
            }
        }
        
        #endregion
    }
}
```

#### Example: Smelter (ProcessorBase)

```csharp
using UnityEngine;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Smelter that processes ore into ingots.
    /// </summary>
    public class Smelter : ProcessorBase
    {
        #region Serialized Fields
        
        [Header("Smelter Settings")]
        [SerializeField] private ParticleSystem fireEffect;
        [SerializeField] private Light furnaceLight;
        
        #endregion
        
        #region Initialization
        
        public override void Initialize(MachineData data)
        {
            base.Initialize(data);
            
            // Set default recipe if available
            if (data.availableRecipes != null && data.availableRecipes.Length > 0)
            {
                SetRecipe(data.availableRecipes[0]);
            }
        }
        
        #endregion
        
        #region Visual Feedback
        
        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            
            // Control fire effect based on state
            if (fireEffect != null)
            {
                if (currentState == MachineState.Working)
                {
                    if (!fireEffect.isPlaying)
                        fireEffect.Play();
                }
                else
                {
                    if (fireEffect.isPlaying)
                        fireEffect.Stop();
                }
            }
            
            // Control furnace light
            if (furnaceLight != null)
            {
                furnaceLight.enabled = currentState == MachineState.Working;
            }
        }
        
        #endregion
        
        #region Recipe Processing
        
        protected override void CompleteProcessing()
        {
            base.CompleteProcessing();
            
            // Play completion sound/effect
            if (audioSource != null && completionSound != null)
            {
                audioSource.PlayOneShot(completionSound);
            }
        }
        
        #endregion
    }
}
```

#### Example: Toy Assembler (AssemblerBase)

```csharp
using UnityEngine;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Toy assembler that combines multiple components into finished toys.
    /// </summary>
    public class ToyAssembler : AssemblerBase
    {
        #region Serialized Fields
        
        [Header("Toy Assembler Settings")]
        [SerializeField] private Transform assemblyPoint;
        [SerializeField] private float assemblyAnimationSpeed = 1f;
        
        #endregion
        
        #region Initialization
        
        public override void Initialize(MachineData data)
        {
            base.Initialize(data);
            
            // Initialize assembly point
            if (assemblyPoint == null)
            {
                assemblyPoint = transform;
            }
        }
        
        #endregion
        
        #region Assembly Logic
        
        protected override void CompleteAssembly()
        {
            base.CompleteAssembly();
            
            // Spawn toy visual at assembly point
            if (currentRecipe != null && currentRecipe.outputs != null)
            {
                foreach (var output in currentRecipe.outputs)
                {
                    SpawnToyVisual(output.resourceId, assemblyPoint.position);
                }
            }
        }
        
        private void SpawnToyVisual(string toyId, Vector3 position)
        {
            // TODO: Spawn toy prefab at position
            if (showDebugInfo)
            {
                Debug.Log($"[ToyAssembler] {MachineId} spawned toy: {toyId}");
            }
        }
        
        #endregion
        
        #region Visual Feedback
        
        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            
            // Animate assembly progress
            if (currentState == MachineState.Working && animator != null)
            {
                animator.SetFloat("AssemblyProgress", assemblyProgress);
                animator.speed = assemblyAnimationSpeed;
            }
        }
        
        #endregion
        
        #region Inventory Management
        
        public override bool AddToInventory(string resourceId, int amount)
        {
            bool success = base.AddToInventory(resourceId, amount);
            
            if (success)
            {
                // Update UI or visual indicators
                OnInventoryChanged?.Invoke();
            }
            
            return success;
        }
        
        // Custom event for inventory changes
        public event System.Action OnInventoryChanged;
        
        #endregion
    }
}
```

### Step 3: Override Virtual Methods

**Common Virtual Methods to Override**:

```csharp
// Initialization
public override void Initialize(MachineData data)
{
    base.Initialize(data);
    // Custom initialization
}

// Update logic (called every frame when enabled)
public override void Tick(float deltaTime)
{
    base.Tick(deltaTime);
    // Custom update logic
}

// Visual updates
protected override void UpdateVisuals()
{
    base.UpdateVisuals();
    // Update animations, particles, lights, etc.
}

// State change handling
protected override void OnStateChanged(MachineState oldState, MachineState newState)
{
    base.OnStateChanged(oldState, newState);
    // React to state changes
}

// Power status changes
protected override void OnPowerStatusChanged(bool powered)
{
    base.OnPowerStatusChanged(powered);
    // React to power changes
}
```

---

## Working with Recipes

### Creating Recipe ScriptableObjects

1. **In Unity Editor**: Right-click in Project window → Create → Game → Recipe
2. **Configure the recipe**:

```csharp
// Example: Iron Smelting Recipe
recipeId: "recipe_iron_smelting"
recipeName: "Iron Smelting"
description: "Smelt iron ore into iron ingots"

inputs:
  - resourceId: "iron_ore"
    amount: 1

outputs:
  - resourceId: "iron_ingot"
    amount: 1

processingTime: 2.0 (seconds)
powerConsumption: 20.0 (watts)
requiredTier: 1
```

### Recipe Validation

Recipes are automatically validated in the editor:

```csharp
private void OnValidate()
{
    // Check for empty inputs
    if (inputs == null || inputs.Length == 0)
    {
        Debug.LogWarning($"Recipe '{recipeName}' has no inputs!");
    }
    
    // Check for empty outputs
    if (outputs == null || outputs.Length == 0)
    {
        Debug.LogWarning($"Recipe '{recipeName}' has no outputs!");
    }
    
    // Check for valid processing time
    if (processingTime <= 0)
    {
        Debug.LogWarning($"Recipe '{recipeName}' has invalid processing time!");
    }
}
```

### Using Recipes in Code

```csharp
// Set recipe on a processor
ProcessorBase processor = GetComponent<ProcessorBase>();
processor.SetRecipe(ironSmeltingRecipe);

// Check if machine can process recipe
if (processor.CanProcess())
{
    // Machine will automatically process when conditions are met
}

// Get current recipe
RecipeData currentRecipe = processor.CurrentRecipe;
if (currentRecipe != null)
{
    Debug.Log($"Processing: {currentRecipe.recipeName}");
    Debug.Log($"Progress: {processor.ProcessingProgress * 100}%");
}
```

---

## MachineData Configuration

### Creating MachineData ScriptableObjects

1. **In Unity Editor**: Right-click → Create → Game → Machine Data
2. **Configure the machine**:

```csharp
// Example: Tier 1 Smelter
machineName: "Basic Smelter"
description: "Smelts ore into ingots"
icon: [Sprite reference]
gridSize: (2, 2) // 2x2 cells
tier: 1

baseProcessingSpeed: 1.0
basePowerConsumption: 20.0

inputPortCount: 1
outputPortCount: 1
bufferCapacity: 10

availableRecipes:
  - Iron Smelting Recipe
  - Copper Smelting Recipe

prefab: [Smelter prefab reference]
```

### Port Configuration

```csharp
// Ports are automatically initialized based on counts
// You can customize port positions in derived classes

protected override void InitializePorts()
{
    base.InitializePorts();
    
    // Customize input port positions
    if (inputPorts.Count > 0)
    {
        inputPorts[0].localPosition = new Vector3(-1f, 0.5f, 0f);
    }
    
    // Customize output port positions
    if (outputPorts.Count > 0)
    {
        outputPorts[0].localPosition = new Vector3(1f, 0.5f, 0f);
    }
}
```

---

## State Machine System

### Machine States

```csharp
public enum MachineState
{
    Idle,              // Machine is idle, waiting for work
    Working,           // Machine is actively processing
    Blocked,           // Machine is blocked (no inputs or full outputs)
    NoPower,           // Machine has no power
    Disabled,          // Machine is manually disabled
    Broken             // Machine is broken (future feature)
}
```

### State Transitions

```csharp
// Transition to a new state
SetState(MachineState.Working);

// States automatically transition based on conditions:
// - No power → NoPower
// - Disabled → Disabled
// - No inputs → Blocked
// - Processing → Working
// - Idle → Idle
```

### Handling State Changes

```csharp
protected override void OnStateChanged(MachineState oldState, MachineState newState)
{
    base.OnStateChanged(oldState, newState);
    
    switch (newState)
    {
        case MachineState.Working:
            // Start working animations/effects
            StartWorkingEffects();
            break;
            
        case MachineState.Blocked:
            // Show blocked indicator
            ShowBlockedIndicator();
            break;
            
        case MachineState.NoPower:
            // Show no power indicator
            ShowNoPowerIndicator();
            break;
    }
}
```

---

## Buffer Management

### Input Buffers

```csharp
// Add resource to input buffer
bool success = processor.AddInput(new ResourceStack
{
    resourceId = "iron_ore",
    amount = 5
});

// Check if can accept input
if (processor.CanAcceptInput)
{
    // Add input
}
```

### Output Buffers

```csharp
// Take output from buffer
if (processor.HasOutput)
{
    ResourceStack output = processor.TakeOutput();
    Debug.Log($"Received: {output.resourceId} x{output.amount}");
}
```

### Inventory System (Assemblers)

```csharp
// Add to inventory
assembler.AddToInventory("iron_gear", 2);
assembler.AddToInventory("copper_wire", 4);

// Check inventory
int gearCount = assembler.GetInventoryAmount("iron_gear");
Debug.Log($"Gears in inventory: {gearCount}");

// Check if has space
if (assembler.HasInventorySpace)
{
    // Add more items
}

// Clear inventory
assembler.ClearInventory();
```

---

## Power Integration

### Power Consumption

```csharp
// Power consumption is calculated automatically
float consumption = machine.PowerConsumption;

// Consumption is affected by:
// - Base power consumption (from MachineData)
// - Efficiency multiplier (based on tier)
// - Current state (only consumes when working)
```

### Power Status

```csharp
// Check if machine is powered
if (machine.IsPowered)
{
    // Machine can work
}

// Set power status (called by PowerGrid)
machine.SetPowered(true);

// React to power changes
protected override void OnPowerStatusChanged(bool powered)
{
    base.OnPowerStatusChanged(powered);
    
    if (powered)
    {
        Debug.Log("Power restored!");
    }
    else
    {
        Debug.Log("Power lost!");
    }
}
```

### IPowerConsumer Interface

```csharp
public interface IPowerConsumer
{
    float PowerConsumption { get; }
    bool IsPowered { get; }
    void SetPowered(bool powered);
}

// All machines implement this interface automatically
```

---

## Grid Integration

### Placement

```csharp
// Set grid position (called by PlacementController)
machine.SetGridPosition(new Vector3Int(5, 0, 10));

// Set rotation (0-3, representing 0°, 90°, 180°, 270°)
machine.SetRotation(1); // 90° rotation

// Get occupied cells
List<Vector3Int> cells = machine.GetOccupiedCells();
foreach (var cell in cells)
{
    Debug.Log($"Occupies cell: {cell}");
}
```

### Grid Size

```csharp
// Grid size is defined in MachineData
// Example: 2x2 machine occupies 4 cells

gridSize: (2, 2)

// Occupied cells for machine at (5, 0, 10):
// (5, 0, 10), (6, 0, 10), (5, 0, 11), (6, 0, 11)
```

### Rotation

```csharp
// Rotation affects visual orientation
// Grid cells are rotated around the origin cell

// Rotation 0 (0°):   [X][X]
//                    [X][X]

// Rotation 1 (90°):  [X][X]
//                    [X][X]

// Rotation 2 (180°): [X][X]
//                    [X][X]

// Rotation 3 (270°): [X][X]
//                    [X][X]
```

---

## Save/Load System

### Saving Machine State

```csharp
// Get save data (called by SaveLoadSystem)
MachineSaveData saveData = machine.GetSaveData();

// Save data includes:
// - Machine ID
// - Machine type
// - Tier
// - Grid position and rotation
// - Current state
// - Processing progress
// - Active recipe
// - Input/output buffers
// - Enabled state
```

### Loading Machine State

```csharp
// Load save data (called by SaveLoadSystem)
machine.LoadSaveData(saveData);

// All state is restored:
// - Position and rotation
// - Current state
// - Processing progress
// - Recipe
// - Buffers
```

### Custom Save Data

```csharp
// Override to save custom data
public override MachineSaveData GetSaveData()
{
    MachineSaveData data = base.GetSaveData();
    
    // Add custom data
    // (Store in a custom struct and serialize separately)
    
    return data;
}

// Override to load custom data
public override void LoadSaveData(MachineSaveData data)
{
    base.LoadSaveData(data);
    
    // Load custom data
}
```

---

## Best Practices

### 1. Always Call Base Methods

```csharp
// ✅ GOOD
public override void Initialize(MachineData data)
{
    base.Initialize(data); // Call base first
    // Custom initialization
}

// ❌ BAD
public override void Initialize(MachineData data)
{
    // Custom initialization
    // Forgot to call base!
}
```

### 2. Use showDebugInfo for Logging

```csharp
// ✅ GOOD
if (showDebugInfo)
{
    Debug.Log($"[MiningDrill] {MachineId} extracted ore");
}

// ❌ BAD
Debug.Log($"[MiningDrill] {MachineId} extracted ore"); // Always logs
```

### 3. Cache Component References

```csharp
// ✅ GOOD
private Animator animator;

protected override void Awake()
{
    base.Awake();
    animator = GetComponent<Animator>();
}

private void UpdateAnimation()
{
    if (animator != null)
    {
        animator.SetBool("IsWorking", true);
    }
}

// ❌ BAD
private void UpdateAnimation()
{
    GetComponent<Animator>().SetBool("IsWorking", true); // Expensive!
}
```

### 4. Validate Configuration

```csharp
// ✅ GOOD
public override void Initialize(MachineData data)
{
    base.Initialize(data);
    
    if (data.availableRecipes == null || data.availableRecipes.Length == 0)
    {
        Debug.LogWarning($"[Smelter] {MachineId} has no recipes!");
    }
}
```

### 5. Handle Null References

```csharp
// ✅ GOOD
if (ResourceManager.Instance != null)
{
    ResourceManager.Instance.AddResource(oreType, amount);
}

// ❌ BAD
ResourceManager.Instance.AddResource(oreType, amount); // May be null!
```

---

## Common Patterns

### Pattern 1: Continuous Production (Extractors)

```csharp
public override void Tick(float deltaTime)
{
    if (!isPowered)
    {
        SetState(MachineState.NoPower);
        return;
    }
    
    SetState(MachineState.Working);
    
    extractionProgress += extractionRate * deltaTime;
    
    if (extractionProgress >= 1f)
    {
        extractionProgress -= 1f;
        ProduceResource();
    }
}
```

### Pattern 2: Recipe-Based Processing (Processors)

```csharp
public override void Tick(float deltaTime)
{
    if (!isPowered || currentRecipe == null)
    {
        SetState(MachineState.Idle);
        return;
    }
    
    if (!CanProcess())
    {
        SetState(MachineState.Blocked);
        return;
    }
    
    SetState(MachineState.Working);
    
    processingProgress += deltaTime / currentRecipe.processingTime;
    
    if (processingProgress >= 1f)
    {
        processingProgress = 0f;
        CompleteProcessing();
    }
}
```

### Pattern 3: Multi-Input Assembly (Assemblers)

```csharp
public override void Tick(float deltaTime)
{
    if (!isPowered || currentRecipe == null)
    {
        SetState(MachineState.Idle);
        return;
    }
    
    CheckRequiredInputs();
    
    if (!hasRequiredInputs)
    {
        SetState(MachineState.Blocked);
        return;
    }
    
    SetState(MachineState.Working);
    
    assemblyProgress += deltaTime / currentRecipe.processingTime;
    
    if (assemblyProgress >= 1f)
    {
        assemblyProgress = 0f;
        CompleteAssembly();
    }
}
```

---

## Troubleshooting

### Machine Not Processing

**Symptoms**: Machine stays in Idle or Blocked state

**Possible Causes**:
1. No power: Check `IsPowered` property
2. No recipe: Check `CurrentRecipe` is not null
3. No inputs: Check input buffers have required resources
4. Output full: Check output buffers have space
5. Machine disabled: Check `isEnabled` is true

**Solution**:
```csharp
// Debug machine state
Debug.Log($"State: {machine.CurrentState}");
Debug.Log($"Powered: {machine.IsPowered}");
Debug.Log($"Recipe: {machine.CurrentRecipe?.recipeName ?? "none"}");
Debug.Log($"Can Process: {machine.CanProcess()}");
```

### State Not Changing

**Symptoms**: Machine stuck in one state

**Possible Causes**:
1. State transition logic not called
2. Conditions for transition not met
3. Override method not calling base

**Solution**:
```csharp
// Ensure base methods are called
public override void Tick(float deltaTime)
{
    base.Tick(deltaTime); // Important!
    // Custom logic
}
```

### Save/Load Not Working

**Symptoms**: Machine state not preserved after load

**Possible Causes**:
1. GetSaveData/LoadSaveData not implemented correctly
2. Custom data not serialized
3. Machine ID not preserved

**Solution**:
```csharp
// Verify save data
MachineSaveData data = machine.GetSaveData();
Debug.Log($"Saved ID: {data.machineId}");
Debug.Log($"Saved State: {data.currentState}");
Debug.Log($"Saved Progress: {data.processingProgress}");
```

### Performance Issues

**Symptoms**: Low FPS with many machines

**Possible Causes**:
1. Too many Debug.Log calls
2. Expensive operations in Tick()
3. Not using showDebugInfo flag

**Solution**:
```csharp
// Use showDebugInfo for all logging
if (showDebugInfo)
{
    Debug.Log("Debug message");
}

// Avoid expensive operations in Tick()
// Cache results, use coroutines for heavy work
```

---

## Summary

The Machine Framework provides a robust foundation for creating automated machines in Santa's Workshop Automation. Key takeaways:

1. **Choose the right base class**: ExtractorBase, ProcessorBase, or AssemblerBase
2. **Override virtual methods**: Initialize, Tick, UpdateVisuals, OnStateChanged
3. **Use recipes**: Define input/output relationships in ScriptableObjects
4. **Configure MachineData**: Set up machine properties, ports, and recipes
5. **Handle states**: React to state changes and power status
6. **Manage buffers**: Use input/output buffers or inventory system
7. **Integrate with systems**: Grid, power, save/load
8. **Follow best practices**: Call base methods, validate configuration, handle nulls

For more examples, see the test machines in `Assets/_Project/Scripts/Machines/`.

---

**Questions or Issues?**  
Check the troubleshooting section or review the example machines for reference implementations.
