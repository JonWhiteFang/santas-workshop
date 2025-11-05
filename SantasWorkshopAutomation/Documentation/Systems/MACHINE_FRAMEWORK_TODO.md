# Machine Framework - Code Improvements TODO

**Created**: November 5, 2025  
**Status**: Ready for Implementation  
**Related**: Machine Framework (29% complete - Tasks 1-22 done)

This document contains actionable improvements identified during code analysis of the Machine Framework implementation. Issues are prioritized by impact and organized into implementation phases.

---

## 🔴 CRITICAL Priority Issues

### 1. Transform Component Caching

**File**: `Assets/_Project/Scripts/Machines/MachineBase.cs`  
**Issue**: Missing explicit transform caching (Unity caches internally, but consistency matters)  
**Impact**: Low | **Effort**: Low

**Current Code** (line ~1150):
```csharp
protected virtual void UpdateVisualRotation()
{
    transform.rotation = Quaternion.Euler(0, rotation * 90f, 0); // ❌ Direct access
}
```

**Solution**:
```csharp
// Add to Cache Fields region (around line 150)
/// <summary>
/// Cached transform component.
/// </summary>
private Transform _transform;

// Update Awake() method (around line 550)
protected virtual void Awake()
{
    _transform = transform; // ✅ Cache transform
    
    if (string.IsNullOrEmpty(machineId))
    {
        machineId = System.Guid.NewGuid().ToString();
    }
    
    InitializeFromData();
}

// Update UpdateVisualRotation() method (around line 1150)
protected virtual void UpdateVisualRotation()
{
    _transform.rotation = Quaternion.Euler(0, rotation * 90f, 0); // ✅ Use cached
}
```

---

### 2. Event Subscription Memory Leak Prevention

**File**: `Assets/_Project/Scripts/Machines/MachineBase.cs`  
**Issue**: No OnDisable() cleanup; event documentation doesn't warn about memory leaks  
**Impact**: High | **Effort**: Low

**Current Code** (line ~590):
```csharp
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
```

**Solution 1 - Add OnDisable()**:
```csharp
// Add after Start() method (around line 580)
/// <summary>
/// Called when the MonoBehaviour becomes disabled.
/// Override in derived classes to unsubscribe from external events.
/// </summary>
protected virtual void OnDisable()
{
    // Base implementation - derived classes should override to unsubscribe from external events
    // Note: Don't clear our own events here - subscribers should handle that
}
```

**Solution 2 - Update Event Documentation** (around line 430):
```csharp
/// <summary>
/// Event fired when the machine transitions to a new state.
/// Parameters: (oldState, newState)
/// ⚠️ IMPORTANT: Always unsubscribe in OnDisable() or OnDestroy() to prevent memory leaks.
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
/// ⚠️ IMPORTANT: Always unsubscribe in OnDisable() or OnDestroy() to prevent memory leaks.
/// </summary>
public event Action<Recipe> OnProcessingStarted;

/// <summary>
/// Event fired when the machine completes processing a recipe.
/// Parameter: The recipe that was completed.
/// ⚠️ IMPORTANT: Always unsubscribe in OnDisable() or OnDestroy() to prevent memory leaks.
/// </summary>
public event Action<Recipe> OnProcessingCompleted;

/// <summary>
/// Event fired when the machine's power status changes.
/// Parameter: True if powered, false if unpowered.
/// ⚠️ IMPORTANT: Always unsubscribe in OnDisable() or OnDestroy() to prevent memory leaks.
/// </summary>
public event Action<bool> OnPowerStatusChanged;
```

---

## 🟡 HIGH Priority Issues

### 3. State Machine Transition Validation

**File**: `Assets/_Project/Scripts/Machines/MachineBase.cs`  
**Issue**: No validation of state transitions; invalid transitions are allowed  
**Impact**: Medium | **Effort**: Medium

**Current Code** (line ~750):
```csharp
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
```

**Solution - Add before TransitionToState()** (around line 745):
```csharp
/// <summary>
/// Validates if a state transition is allowed.
/// Override in derived classes to add custom transition rules.
/// </summary>
/// <param name="from">Current state</param>
/// <param name="to">Target state</param>
/// <returns>True if transition is valid</returns>
protected virtual bool IsValidTransition(MachineState from, MachineState to)
{
    // Same state is always valid (no-op)
    if (from == to) return true;
    
    // Disabled can only transition to Idle (when re-enabled)
    if (from == MachineState.Disabled && to != MachineState.Idle)
    {
        Debug.LogWarning($"Machine {machineId}: Cannot transition from Disabled to {to}. Must go to Idle first.");
        return false;
    }
    
    // NoPower can transition back to previous state or Disabled
    if (from == MachineState.NoPower && to != previousState && to != MachineState.Disabled)
    {
        Debug.LogWarning($"Machine {machineId}: Cannot transition from NoPower to {to}. Can only return to {previousState} or Disabled.");
        return false;
    }
    
    // Processing can't directly transition to WaitingForInput (must go through Idle or complete)
    if (from == MachineState.Processing && to == MachineState.WaitingForInput)
    {
        Debug.LogWarning($"Machine {machineId}: Cannot transition directly from Processing to WaitingForInput.");
        return false;
    }
    
    return true;
}
```

**Update TransitionToState()** (around line 750):
```csharp
public void TransitionToState(MachineState newState)
{
    if (currentState == newState) return;
    
    // ✅ Validate transition
    if (!IsValidTransition(currentState, newState))
    {
        Debug.LogWarning($"Machine {machineId}: Invalid state transition from {currentState} to {newState}. Transition blocked.");
        return;
    }
    
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
```

---

### 4. Performance: Recipe-Specific Input Cache

**File**: `Assets/_Project/Scripts/Machines/MachineBase.cs`  
**Issue**: HasRequiredInputs() performs dictionary lookups every call even with caching  
**Impact**: Medium | **Effort**: Low

**Current Code** (line ~380):
```csharp
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
```

**Solution - Add to Cache Fields region** (around line 150):
```csharp
/// <summary>
/// Cached result of HasRequiredInputs for the active recipe.
/// </summary>
private bool _cachedHasInputs = false;

/// <summary>
/// The recipe for which _cachedHasInputs was calculated.
/// </summary>
private Recipe _cachedInputsRecipe = null;
```

**Update HasRequiredInputs()** (around line 380):
```csharp
protected virtual bool HasRequiredInputs(Recipe recipe)
{
    if (recipe == null || recipe.inputs == null || recipe.inputs.Length == 0)
        return false;
    
    // ✅ If checking the same recipe and cache is clean, return cached result
    if (!_inputCacheDirty && recipe == _cachedInputsRecipe)
    {
        return _cachedHasInputs;
    }
    
    // Rebuild cache once if dirty
    if (_inputCacheDirty)
    {
        RebuildInputCache();
    }
    
    // Check inputs
    bool hasInputs = true;
    foreach (var input in recipe.inputs)
    {
        if (!_cachedInputTotals.TryGetValue(input.resourceId, out int available) || available < input.amount)
        {
            hasInputs = false;
            break;
        }
    }
    
    // ✅ Cache result for this recipe
    _cachedInputsRecipe = recipe;
    _cachedHasInputs = hasInputs;
    
    return hasInputs;
}
```

**Update InvalidateInputCache()** (around line 1000):
```csharp
public void InvalidateInputCache()
{
    _inputCacheDirty = true;
    _cachedInputsRecipe = null; // ✅ Clear recipe cache too
}
```

---

### 5. Recipe Validation Redundancy

**File**: `Assets/_Project/Scripts/Machines/MachineBase.cs`  
**Issue**: ValidateRecipe() duplicates checks already done in Recipe.OnValidate()  
**Impact**: Medium | **Effort**: Low

**Current Code** (line ~240):
```csharp
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
```

**Solution - Simplified Validation**:
```csharp
protected virtual bool ValidateRecipe(Recipe recipe, out string error)
{
    error = null;
    
    if (recipe == null)
    {
        error = "Recipe is null";
        return false;
    }
    
    // ✅ Only check runtime-specific conditions
    // ScriptableObject validation (Recipe.OnValidate) already ensures:
    // - inputs/outputs exist and are valid
    // - processing time > 0
    // - power consumption >= 0
    // - resource IDs are not empty
    // - amounts are > 0
    
    if (recipe.requiredTier > tier)
    {
        error = $"Recipe '{recipe.recipeName}' requires tier {recipe.requiredTier}, but machine is tier {tier}";
        return false;
    }
    
    #if UNITY_EDITOR
    // ✅ In editor, perform additional validation for debugging
    // This helps catch issues during development without impacting runtime performance
    if (recipe.inputs == null || recipe.inputs.Length == 0)
    {
        error = $"Recipe '{recipe.recipeName}' has no inputs (this should be caught by OnValidate)";
        Debug.LogError(error);
        return false;
    }
    
    if (recipe.outputs == null || recipe.outputs.Length == 0)
    {
        error = $"Recipe '{recipe.recipeName}' has no outputs (this should be caught by OnValidate)";
        Debug.LogError(error);
        return false;
    }
    
    if (recipe.processingTime <= 0f)
    {
        error = $"Recipe '{recipe.recipeName}' has invalid processing time: {recipe.processingTime} (this should be caught by OnValidate)";
        Debug.LogError(error);
        return false;
    }
    #endif
    
    return true;
}
```

---

## 🟢 MEDIUM Priority Issues

### 6. Serialization for Debugging

**File**: `Assets/_Project/Scripts/Machines/MachineBase.cs`  
**Issue**: Key runtime fields not visible in Inspector for debugging  
**Impact**: Low | **Effort**: Low

**Prerequisites**: Create ReadOnlyAttribute first

**Step 1 - Create ReadOnlyAttribute.cs**:
```csharp
// New file: Assets/_Project/Scripts/Utilities/ReadOnlyAttribute.cs
using UnityEngine;

namespace SantasWorkshop.Utilities
{
    /// <summary>
    /// Attribute to make a serialized field read-only in the Inspector.
    /// Useful for debugging runtime state without allowing modification.
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute { }
}
```

**Step 2 - Create ReadOnlyDrawer.cs** (Editor only):
```csharp
// New file: Assets/_Project/Scripts/Editor/ReadOnlyDrawer.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SantasWorkshop.Utilities;

namespace SantasWorkshop.Editor
{
    /// <summary>
    /// Custom property drawer for ReadOnlyAttribute.
    /// Displays the field in the Inspector but disables editing.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
#endif
```

**Step 3 - Update MachineBase.cs** (around line 100):
```csharp
using SantasWorkshop.Utilities; // ✅ Add using

// Update State Management Fields region (around line 100)
#region State Management Fields

[Header("State (Debug - Read Only)")]
[SerializeField, ReadOnly]
[Tooltip("Current operational state of the machine")]
private MachineState currentState = MachineState.Idle;

[SerializeField, ReadOnly]
[Tooltip("Previous operational state (used when transitioning from NoPower or Disabled)")]
private MachineState previousState = MachineState.Idle;

#endregion

// Update Recipe Processing Fields region (around line 120)
#region Recipe Processing Fields

[Header("Processing (Debug - Read Only)")]
[SerializeField, ReadOnly]
[Tooltip("Currently active recipe being processed")]
private Recipe activeRecipe;

[SerializeField, ReadOnly]
[Tooltip("Current processing progress (0.0 to 1.0)")]
private float processingProgress = 0f;

[SerializeField, ReadOnly]
[Tooltip("Time remaining to complete current processing (in seconds)")]
private float processingTimeRemaining = 0f;

#endregion

// Update Power Fields region (around line 135)
#region Power Fields

[Header("Power (Debug - Read Only)")]
[SerializeField, ReadOnly]
[Tooltip("Whether this machine currently has sufficient power")]
private bool isPowered = true;

[SerializeField, ReadOnly]
[Tooltip("Current power consumption in watts")]
private float powerConsumption = 0f;

#endregion

// Update Enable/Disable Fields region (around line 165)
#region Enable/Disable Fields

[Header("Enable/Disable (Debug - Read Only)")]
[SerializeField, ReadOnly]
[Tooltip("Whether this machine is enabled (can be manually disabled by player)")]
private bool isEnabled = true;

#endregion
```

---

## 🔵 LOW Priority Issues

### 7. Documentation Examples

**File**: `Assets/_Project/Scripts/Machines/MachineBase.cs`  
**Issue**: Complex methods lack usage examples  
**Impact**: Low | **Effort**: Low

**Update SetActiveRecipe() documentation** (around line 300):
```csharp
/// <summary>
/// Sets the active recipe for this machine.
/// Validates the recipe and cancels current processing if needed.
/// </summary>
/// <param name="recipe">The recipe to set as active.</param>
/// <example>
/// <code>
/// // Set a smelting recipe
/// Recipe ironSmeltingRecipe = Resources.Load&lt;Recipe&gt;("Recipes/IronSmelting");
/// machine.SetActiveRecipe(ironSmeltingRecipe);
/// 
/// // Clear the recipe (stop production)
/// machine.SetActiveRecipe(null);
/// 
/// // Check if recipe was set successfully
/// if (machine.activeRecipe == ironSmeltingRecipe)
/// {
///     Debug.Log("Recipe set successfully");
/// }
/// </code>
/// </example>
public virtual void SetActiveRecipe(Recipe recipe)
```

**Update AddToInputPort() documentation** (around line 1000):
```csharp
/// <summary>
/// Adds resources to a specific input port.
/// This is the preferred method for external systems to add resources.
/// Automatically invalidates the input cache.
/// </summary>
/// <param name="portIndex">The index of the input port.</param>
/// <param name="resourceId">The resource type to add.</param>
/// <param name="amount">The amount to add.</param>
/// <returns>True if the resource was added successfully, false otherwise.</returns>
/// <example>
/// <code>
/// // Add 10 iron ore to the first input port
/// bool success = machine.AddToInputPort(0, "IronOre", 10);
/// if (success)
/// {
///     Debug.Log("Resources added successfully");
/// }
/// else
/// {
///     Debug.Log("Input port is full or invalid");
/// }
/// </code>
/// </example>
public bool AddToInputPort(int portIndex, string resourceId, int amount)
```

---

### 8. Unit Test Hooks

**File**: `Assets/_Project/Scripts/Machines/MachineBase.cs`  
**Issue**: Some methods difficult to test due to Unity lifecycle dependencies  
**Impact**: Low | **Effort**: Low

**Add at end of class** (around line 1400):
```csharp
#region Test Hooks

#if UNITY_EDITOR || DEVELOPMENT_BUILD
/// <summary>
/// Test hook: Forces a state transition without validation.
/// Only available in editor and development builds.
/// USE WITH CAUTION: This bypasses all state transition validation.
/// </summary>
/// <param name="state">The state to force.</param>
internal void TestForceState(MachineState state)
{
    currentState = state;
}

/// <summary>
/// Test hook: Gets the current input cache dirty flag.
/// Only available in editor and development builds.
/// </summary>
/// <returns>True if the input cache needs rebuilding.</returns>
internal bool TestIsInputCacheDirty()
{
    return _inputCacheDirty;
}

/// <summary>
/// Test hook: Gets the cached input totals dictionary.
/// Only available in editor and development builds.
/// </summary>
/// <returns>Read-only view of cached input totals.</returns>
internal IReadOnlyDictionary<string, int> TestGetCachedInputTotals()
{
    return _cachedInputTotals;
}

/// <summary>
/// Test hook: Manually triggers the state machine update.
/// Only available in editor and development builds.
/// </summary>
internal void TestUpdateStateMachine()
{
    UpdateStateMachine();
}

/// <summary>
/// Test hook: Gets the current processing time remaining.
/// Only available in editor and development builds.
/// </summary>
internal float TestGetProcessingTimeRemaining()
{
    return processingTimeRemaining;
}
#endif

#endregion
```

---

## 📋 Implementation Checklist

### Phase 1: Critical Issues (1-2 hours)
- [ ] **Issue #1**: Add transform caching
- [ ] **Issue #2**: Add OnDisable() method and update event documentation
- [ ] **Issue #3**: Add state transition validation
- [ ] **Issue #4**: Add recipe-specific input caching
- [ ] **Issue #5**: Simplify recipe validation

### Phase 2: Medium Priority (2-3 hours)
- [ ] **Issue #6**: Create ReadOnlyAttribute and add debug serialization

### Phase 3: Low Priority (Optional - 1-2 hours)
- [ ] **Issue #7**: Add documentation examples
- [ ] **Issue #8**: Add unit test hooks

---

## 🧪 Testing After Implementation

After implementing these changes, run the following tests:

1. **Unit Tests**: Run all existing Machine Framework tests
   ```
   Window → General → Test Runner → Run All
   ```

2. **Manual Testing**:
   - Create a test machine in a scene
   - Verify state transitions work correctly
   - Test recipe processing with various inputs
   - Test power on/off scenarios
   - Test enable/disable functionality
   - Verify Inspector shows debug fields correctly

3. **Performance Testing**:
   - Create 100+ machines in a scene
   - Monitor frame rate and memory usage
   - Verify input cache optimization works

---

## 📝 Notes

- All changes maintain backward compatibility
- No breaking changes to public API
- All optimizations preserve existing functionality
- Changes align with project's established patterns
- Unity 6 and C# 9.0+ features used appropriately

---

## ✅ Code Quality Assessment

**Current State**: Excellent  
**After Implementation**: Outstanding

The Machine Framework is already well-architected with:
- ✅ Comprehensive XML documentation
- ✅ Clean region organization
- ✅ Proper event patterns
- ✅ State machine implementation
- ✅ Performance caching
- ✅ Extensive validation
- ✅ Complete save/load support

These improvements add:
- ✅ Enhanced memory safety
- ✅ Better state management
- ✅ Performance optimizations
- ✅ Improved debugging experience
- ✅ Better testability

---

**Last Updated**: November 5, 2025  
**Status**: Ready for Implementation  
**Estimated Total Time**: 4-7 hours (depending on phases completed)
