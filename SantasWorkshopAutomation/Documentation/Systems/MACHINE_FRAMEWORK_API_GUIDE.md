# Machine Framework API Guide

**Last Updated**: November 5, 2025  
**Version**: 1.1 (Post-Fixes)

Quick reference guide for working with the Machine Framework after critical fixes.

---

## Quick Start

### Creating a New Machine Type

```csharp
using SantasWorkshop.Machines;
using SantasWorkshop.Data;

public class Smelter : MachineBase
{
    // Override state-specific behavior if needed
    protected override void OnEnterProcessing()
    {
        base.OnEnterProcessing();
        // Play smelting animation
        // Start particle effects
    }
    
    protected override void OnExitProcessing()
    {
        base.OnExitProcessing();
        // Stop effects
    }
}
```

---

## Working with Ports

### Adding Resources to Input Ports (Recommended)

```csharp
// ✅ GOOD: Use the public API
bool success = machine.AddToInputPort(portIndex: 0, resourceId: "iron_ore", amount: 10);

if (success)
{
    Debug.Log("Resources added successfully");
}
else
{
    Debug.Log("Failed to add resources (port full or invalid)");
}
```

**Benefits**:
- Automatic cache invalidation
- Automatic state transitions (WaitingForInput → Processing)
- Bounds checking
- Clear error messages

### Extracting Resources from Output Ports (Recommended)

```csharp
// ✅ GOOD: Use the public API
int extracted = machine.ExtractFromOutputPort(portIndex: 0, resourceId: "iron_ingot", amount: 5);

Debug.Log($"Extracted {extracted} iron ingots");

// Machine automatically transitions from WaitingForOutput → Idle if space becomes available
```

### Direct Port Access (Not Recommended)

```csharp
// ❌ AVOID: Direct port access
machine.inputPorts[0].AddResource("iron_ore", 10);

// Problems:
// - No cache invalidation
// - No state transitions
// - No bounds checking
// - Harder to debug

// If you must use direct access, invalidate cache manually:
machine.inputPorts[0].AddResource("iron_ore", 10);
machine.InvalidateInputCache(); // ⚠️ Don't forget this!
```

---

## Recipe Management

### Setting Active Recipe

```csharp
// Get recipe from MachineData
Recipe smelting = machineData.availableRecipes.Find(r => r.recipeId == "iron_smelting");

// Set as active (automatically validates)
machine.SetActiveRecipe(smelting);

// Clear recipe
machine.SetActiveRecipe(null);
```

**Automatic Validation Checks**:
- ✅ Recipe is not null
- ✅ Recipe has inputs and outputs
- ✅ Processing time is valid
- ✅ Power consumption is valid
- ✅ Machine tier meets requirements
- ✅ All resource IDs are valid
- ✅ All amounts are positive
- ✅ Recipe is in machine's available recipes list

### Checking Recipe Availability

```csharp
// Check if machine can process recipe right now
bool canProcess = machine.CanProcessRecipe(recipe);

// Check if recipe is in available recipes list
bool isAvailable = machine.IsRecipeAvailable(recipe);

// Get all available recipes
List<Recipe> recipes = machine.GetAvailableRecipes();
```

---

## State Management

### Current State

```csharp
MachineState state = machine.CurrentState;

switch (state)
{
    case MachineState.Idle:
        // Machine is ready but not processing
        break;
    case MachineState.WaitingForInput:
        // Machine needs input resources
        break;
    case MachineState.Processing:
        // Machine is actively working
        break;
    case MachineState.WaitingForOutput:
        // Output buffer is full
        break;
    case MachineState.NoPower:
        // Insufficient power
        break;
    case MachineState.Disabled:
        // Manually disabled by player
        break;
}
```

### State Transitions

```csharp
// Manual state transition (usually not needed)
machine.TransitionToState(MachineState.Idle);

// Listen for state changes
machine.OnStateChanged += (oldState, newState) =>
{
    Debug.Log($"Machine transitioned from {oldState} to {newState}");
};

// ⚠️ IMPORTANT: Always unsubscribe!
private void OnEnable()
{
    machine.OnStateChanged += HandleStateChange;
}

private void OnDisable()
{
    machine.OnStateChanged -= HandleStateChange;
}
```

---

## Processing Progress

### Monitoring Progress

```csharp
// Get current progress (0.0 to 1.0)
float progress = machine.ProcessingProgress;

// Get estimated time remaining (seconds)
float timeRemaining = machine.EstimatedTimeRemaining;

// Update UI
progressBar.value = progress;
timeText.text = $"{timeRemaining:F1}s";
```

### Processing Events

```csharp
// Listen for processing events
machine.OnProcessingStarted += (recipe) =>
{
    Debug.Log($"Started processing: {recipe.recipeName}");
};

machine.OnProcessingCompleted += (recipe) =>
{
    Debug.Log($"Completed processing: {recipe.recipeName}");
};

// ⚠️ IMPORTANT: Always unsubscribe in OnDisable()!
```

---

## Power Management

### Power Status

```csharp
// Check if machine has power
bool isPowered = machine.IsPowered;

// Get current power consumption
float consumption = machine.PowerConsumption;

// Listen for power changes
machine.OnPowerStatusChanged += (powered) =>
{
    if (powered)
        Debug.Log("Power restored");
    else
        Debug.Log("Power lost");
};
```

### Setting Power Status

```csharp
// Called by PowerGrid system
machine.SetPowered(true);  // Power available
machine.SetPowered(false); // Power lost

// Machine automatically transitions to/from NoPower state
// Processing progress is preserved during power loss
```

---

## Enable/Disable

### Manual Control

```csharp
// Enable machine
machine.SetEnabled(true);

// Disable machine
machine.SetEnabled(false);

// Check if enabled
bool isEnabled = machine.IsEnabled;
```

**Behavior**:
- Disabled machines transition to `Disabled` state
- Processing is paused (not cancelled)
- Re-enabling transitions back to previous state

---

## Grid Integration

### Position and Rotation

```csharp
// Set grid position
machine.SetGridPosition(new Vector3Int(5, 0, 10));

// Set rotation (0-3 for 0°, 90°, 180°, 270°)
machine.SetRotation(1); // 90° rotation

// Get occupied cells
List<Vector3Int> cells = machine.GetOccupiedCells();

// Get properties
Vector3Int position = machine.GridPosition;
Vector2Int size = machine.GridSize;
int rotation = machine.Rotation;
```

---

## Save/Load

### Saving Machine State

```csharp
// Get save data
MachineSaveData saveData = machine.GetSaveData();

// Serialize to JSON
string json = JsonUtility.ToJson(saveData);
```

### Loading Machine State

```csharp
// Deserialize from JSON
MachineSaveData saveData = JsonUtility.FromJson<MachineSaveData>(json);

// Load into machine
machine.LoadSaveData(saveData);

// Machine automatically:
// - Restores all state
// - Rebuilds buffers
// - Resumes processing if applicable
// - Transitions to saved state
```

---

## Common Patterns

### Logistics System Integration

```csharp
public class ConveyorBelt : MonoBehaviour
{
    private MachineBase targetMachine;
    private int targetPortIndex;
    
    public void DeliverResource(string resourceId, int amount)
    {
        // Use the public API
        bool success = targetMachine.AddToInputPort(targetPortIndex, resourceId, amount);
        
        if (success)
        {
            // Resource delivered successfully
            // Machine will automatically start processing if ready
        }
        else
        {
            // Port is full, try again later
            RetryDelivery(resourceId, amount);
        }
    }
    
    public void ExtractResource(string resourceId, int amount)
    {
        // Use the public API
        int extracted = targetMachine.ExtractFromOutputPort(targetPortIndex, resourceId, amount);
        
        if (extracted > 0)
        {
            // Resource extracted successfully
            // Machine will automatically continue processing if space available
            TransportResource(resourceId, extracted);
        }
    }
}
```

### UI Integration

```csharp
public class MachineUI : MonoBehaviour
{
    private MachineBase machine;
    
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text stateText;
    [SerializeField] private Text timeText;
    
    private void OnEnable()
    {
        machine.OnStateChanged += UpdateStateDisplay;
        machine.OnProcessingStarted += OnProcessingStarted;
        machine.OnProcessingCompleted += OnProcessingCompleted;
    }
    
    private void OnDisable()
    {
        machine.OnStateChanged -= UpdateStateDisplay;
        machine.OnProcessingStarted -= OnProcessingStarted;
        machine.OnProcessingCompleted -= OnProcessingCompleted;
    }
    
    private void Update()
    {
        if (machine.CurrentState == MachineState.Processing)
        {
            progressBar.value = machine.ProcessingProgress;
            timeText.text = $"{machine.EstimatedTimeRemaining:F1}s";
        }
    }
    
    private void UpdateStateDisplay(MachineState oldState, MachineState newState)
    {
        stateText.text = newState.ToString();
        
        // Update UI colors based on state
        switch (newState)
        {
            case MachineState.Processing:
                stateText.color = Color.green;
                break;
            case MachineState.NoPower:
                stateText.color = Color.red;
                break;
            case MachineState.WaitingForInput:
                stateText.color = Color.yellow;
                break;
            default:
                stateText.color = Color.white;
                break;
        }
    }
}
```

---

## Error Handling

### Common Errors and Solutions

#### "Machine is in Processing state with no active recipe"

**Cause**: State machine entered Processing without a valid recipe.

**Solution**: Always validate recipe before transitioning to Processing:
```csharp
if (machine.CanProcessRecipe(recipe))
{
    machine.TransitionToState(MachineState.Processing);
}
```

#### "Invalid speedMultiplier"

**Cause**: Speed multiplier became zero or negative.

**Solution**: Machine automatically resets to 1.0 and logs error. Check tier calculation logic.

#### "Failed to consume/produce resource"

**Cause**: Resource operation failed (buffer full/empty, invalid ID).

**Solution**: Check buffer capacity and resource IDs. Machine logs detailed error with context.

---

## Best Practices

### ✅ DO

- Use `AddToInputPort()` and `ExtractFromOutputPort()` for external resource management
- Always unsubscribe from events in `OnDisable()`
- Validate recipes before setting them as active
- Check `CanProcessRecipe()` before manual state transitions
- Use the provided events for UI updates
- Cache machine references in `Awake()`

### ❌ DON'T

- Don't access ports directly unless absolutely necessary
- Don't forget to call `InvalidateInputCache()` if you do access ports directly
- Don't subscribe to events without unsubscribing
- Don't manually transition to Processing without checking `CanProcessRecipe()`
- Don't modify machine state from multiple threads
- Don't use `GetComponent<MachineBase>()` in Update loops

---

## Performance Tips

### Cache Management

The input buffer cache is automatically managed, but you can optimize:

```csharp
// ✅ GOOD: Single call rebuilds cache once
bool canProcess = machine.CanProcessRecipe(recipe);

// ❌ BAD: Multiple calls might rebuild cache multiple times
bool hasInput1 = machine.HasRequiredInputs(recipe1);
bool hasInput2 = machine.HasRequiredInputs(recipe2);
bool hasInput3 = machine.HasRequiredInputs(recipe3);
```

### Event Subscriptions

```csharp
// ✅ GOOD: Subscribe once, unsubscribe in OnDisable
private void OnEnable()
{
    machine.OnStateChanged += HandleStateChange;
}

private void OnDisable()
{
    machine.OnStateChanged -= HandleStateChange;
}

// ❌ BAD: Subscribing multiple times
private void Update()
{
    machine.OnStateChanged += HandleStateChange; // Memory leak!
}
```

---

## Debugging

### Verbose Logging

Enable detailed logging in development builds:

```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    Debug.Log($"Machine {machine.MachineId}: {message}");
#endif
```

### Inspector Debugging

Add this to your machine script for runtime debugging:

```csharp
[Header("Debug Info")]
[SerializeField] private string debugState;
[SerializeField] private float debugProgress;
[SerializeField] private string debugRecipe;

private void Update()
{
    #if UNITY_EDITOR
    debugState = CurrentState.ToString();
    debugProgress = ProcessingProgress;
    debugRecipe = activeRecipe?.recipeName ?? "None";
    #endif
}
```

---

## Migration from Old API

If you have existing code using the old patterns:

### Before (Old)
```csharp
// Direct port access
machine.inputPorts[0].AddResource("iron_ore", 10);

// Manual cache invalidation (often forgotten)
// ... no cache invalidation ...

// Manual state checks
if (machine.currentState == MachineState.WaitingForInput)
{
    // Check if can process
    // Manually transition
}
```

### After (New)
```csharp
// Use public API
machine.AddToInputPort(0, "iron_ore", 10);

// Automatic cache invalidation
// Automatic state transitions
// All handled by the API!
```

---

## Summary

The Machine Framework provides a robust, validated, and performant system for factory automation. Key takeaways:

1. **Use the public API** for port operations
2. **Always unsubscribe** from events
3. **Let the framework handle** state transitions
4. **Trust the validation** - it's comprehensive
5. **Monitor events** for UI updates

For more details, see:
- `MachineBase.cs` - Full implementation
- `MACHINE_FRAMEWORK_FIXES_SUMMARY.md` - Recent improvements
- `game-design-patterns.md` - Architectural patterns

---

**Last Updated**: November 5, 2025  
**Status**: Production Ready ✅
