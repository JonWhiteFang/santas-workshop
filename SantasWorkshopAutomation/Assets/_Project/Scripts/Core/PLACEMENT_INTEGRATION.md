# Placement System Integration Guide

**Last Updated**: November 4, 2025

This document describes how to integrate the Grid & Placement System with other game systems.

---

## Overview

The Grid & Placement System provides the foundation for spatial organization in Santa's Workshop Automation. This guide explains how to integrate it with other systems like the Resource System, Machine Framework, and Build Menu UI.

---

## Core Components

### GridManager
- **Location**: `Assets/_Project/Scripts/Core/GridManager.cs`
- **Purpose**: Manages the grid coordinate system and cell occupation state
- **Access**: `GridManager.Instance`

### PlacementController
- **Location**: `Assets/_Project/Scripts/Core/PlacementController.cs`
- **Purpose**: Handles placement mode, user input, and placement validation
- **Access**: Find in scene or reference directly

### PlacementData
- **Location**: `Assets/_Project/Scripts/Core/PlacementData.cs`
- **Purpose**: ScriptableObject defining placeable object properties
- **Access**: Create assets in `Assets/_Project/ScriptableObjects/Placement/`

---

## Integration Points

### 1. GameManager Integration

The GameManager initializes core systems on startup. The Grid & Placement System is automatically initialized when the Workshop scene loads.

**Implementation**:
```csharp
// In GameManager.Start()
private void InitializeCoreSystem()
{
    // GridManager is verified to exist in the scene
    if (GridManager.Instance == null)
    {
        Debug.LogWarning("GridManager not found in scene");
    }
    
    // PlacementController is verified to exist in the scene
    PlacementController placementController = FindFirstObjectByType<PlacementController>();
    if (placementController == null)
    {
        Debug.LogWarning("PlacementController not found in scene");
    }
}
```

**Requirements**:
- GridManager must be present in the Workshop scene
- PlacementController must be present in the Workshop scene
- Both should be initialized before other systems that depend on them

---

### 2. Build Menu UI Integration

The Build Menu UI will trigger placement mode when the player selects a building to place.

**Interface**:
```csharp
public class BuildMenuUI : MonoBehaviour
{
    [SerializeField] private PlacementController placementController;
    
    // Called when player clicks a building button
    public void OnBuildingSelected(PlacementData placementData)
    {
        if (placementController != null)
        {
            placementController.EnterPlacementMode(placementData);
        }
    }
    
    // Subscribe to placement events
    private void OnEnable()
    {
        if (placementController != null)
        {
            placementController.OnPlacementConfirmed += OnPlacementConfirmed;
            placementController.OnPlacementCancelled += OnPlacementCancelled;
        }
    }
    
    private void OnDisable()
    {
        if (placementController != null)
        {
            placementController.OnPlacementConfirmed -= OnPlacementConfirmed;
            placementController.OnPlacementCancelled -= OnPlacementCancelled;
        }
    }
    
    private void OnPlacementConfirmed(GameObject placedObject, Vector3Int gridPosition, int rotation)
    {
        // Close build menu or update UI
        Debug.Log($"Building placed at {gridPosition}");
    }
    
    private void OnPlacementCancelled()
    {
        // Update UI to show placement was cancelled
        Debug.Log("Placement cancelled");
    }
}
```

**Events**:
- `OnPlacementModeEntered(PlacementData)`: Triggered when entering placement mode
- `OnPlacementModeExited()`: Triggered when exiting placement mode
- `OnPlacementConfirmed(GameObject, Vector3Int, int)`: Triggered when placement succeeds
- `OnPlacementCancelled()`: Triggered when placement is cancelled

---

### 3. Resource System Integration

The Resource System should check if the player has enough resources before allowing placement.

**Interface**:
```csharp
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    
    // Check if player can afford the placement
    public bool CanAfford(ResourceAmount[] cost)
    {
        foreach (var resource in cost)
        {
            if (GetResourceAmount(resource.type) < resource.amount)
            {
                return false;
            }
        }
        return true;
    }
    
    // Deduct resources when placement is confirmed
    public void Spend(ResourceAmount[] cost)
    {
        foreach (var resource in cost)
        {
            DeductResource(resource.type, resource.amount);
        }
    }
    
    // Refund resources if placement is cancelled (optional)
    public void Refund(ResourceAmount[] cost)
    {
        foreach (var resource in cost)
        {
            AddResource(resource.type, resource.amount);
        }
    }
}

// Integration with PlacementController
public class PlacementResourceIntegration : MonoBehaviour
{
    [SerializeField] private PlacementController placementController;
    
    private void OnEnable()
    {
        placementController.OnPlacementModeEntered += OnPlacementModeEntered;
        placementController.OnPlacementConfirmed += OnPlacementConfirmed;
    }
    
    private void OnDisable()
    {
        placementController.OnPlacementModeEntered -= OnPlacementModeEntered;
        placementController.OnPlacementConfirmed -= OnPlacementConfirmed;
    }
    
    private void OnPlacementModeEntered(PlacementData placementData)
    {
        // Check if player can afford the building
        if (!ResourceManager.Instance.CanAfford(placementData.buildCost))
        {
            Debug.LogWarning("Not enough resources to build");
            placementController.ExitPlacementMode();
        }
    }
    
    private void OnPlacementConfirmed(GameObject placedObject, Vector3Int gridPosition, int rotation)
    {
        // Deduct resources
        PlacementData placementData = placedObject.GetComponent<PlacedObject>()?.PlacementData;
        if (placementData != null)
        {
            ResourceManager.Instance.Spend(placementData.buildCost);
        }
    }
}
```

**Future Enhancement**:
Add a `buildCost` field to `PlacementData` ScriptableObject:
```csharp
[Header("Cost")]
public ResourceAmount[] buildCost;
```

---

### 4. Machine Framework Integration

Machines need to know their grid position and rotation after placement.

**Interface**:
```csharp
public abstract class MachineBase : MonoBehaviour
{
    protected Vector3Int _gridPosition;
    protected int _rotation;
    
    // Called by PlacementController after placement
    public virtual void SetGridPosition(Vector3Int gridPosition)
    {
        _gridPosition = gridPosition;
    }
    
    public virtual void SetRotation(int rotation)
    {
        _rotation = rotation;
    }
    
    public Vector3Int GridPosition => _gridPosition;
    public int Rotation => _rotation;
}

// Integration component to attach to machine prefabs
public class PlacedObject : MonoBehaviour
{
    public PlacementData PlacementData { get; set; }
    public Vector3Int GridPosition { get; set; }
    public int Rotation { get; set; }
}

// Update PlacementController.ConfirmPlacement()
private void ConfirmPlacement()
{
    // ... existing code ...
    
    // Instantiate the actual object
    GameObject placedObject = Instantiate(
        _currentPlacementData.prefab,
        worldPosition,
        worldRotation
    );
    
    // Set grid position and rotation on machine
    var machine = placedObject.GetComponent<MachineBase>();
    if (machine != null)
    {
        machine.SetGridPosition(_currentGridPosition);
        machine.SetRotation(_currentRotation);
    }
    
    // Store placement data reference
    var placedObjectComponent = placedObject.AddComponent<PlacedObject>();
    placedObjectComponent.PlacementData = _currentPlacementData;
    placedObjectComponent.GridPosition = _currentGridPosition;
    placedObjectComponent.Rotation = _currentRotation;
    
    // ... rest of existing code ...
}
```

---

### 5. Save/Load Integration

The grid state and placed objects need to be saved and loaded.

**Interface**:
```csharp
[System.Serializable]
public class GridSaveData
{
    public List<PlacedObjectSaveData> placedObjects;
}

[System.Serializable]
public struct PlacedObjectSaveData
{
    public string placementDataId; // Reference to PlacementData asset
    public Vector3Int gridPosition;
    public int rotation;
}

public class SaveLoadSystem : MonoBehaviour
{
    public static SaveLoadSystem Instance { get; private set; }
    
    // Save grid state
    public GridSaveData SaveGridState()
    {
        GridSaveData saveData = new GridSaveData();
        saveData.placedObjects = new List<PlacedObjectSaveData>();
        
        // Find all placed objects in scene
        PlacedObject[] placedObjects = FindObjectsByType<PlacedObject>(FindObjectsSortMode.None);
        foreach (var placedObject in placedObjects)
        {
            saveData.placedObjects.Add(new PlacedObjectSaveData
            {
                placementDataId = placedObject.PlacementData.name,
                gridPosition = placedObject.GridPosition,
                rotation = placedObject.Rotation
            });
        }
        
        return saveData;
    }
    
    // Load grid state
    public void LoadGridState(GridSaveData saveData)
    {
        // Clear existing placed objects
        PlacedObject[] existingObjects = FindObjectsByType<PlacedObject>(FindObjectsSortMode.None);
        foreach (var obj in existingObjects)
        {
            Destroy(obj.gameObject);
        }
        
        // Clear grid state
        GridManager.Instance.ClearGrid();
        
        // Recreate placed objects
        foreach (var objectData in saveData.placedObjects)
        {
            // Load PlacementData asset
            PlacementData placementData = Resources.Load<PlacementData>($"Placement/{objectData.placementDataId}");
            if (placementData == null)
            {
                Debug.LogWarning($"Could not load PlacementData: {objectData.placementDataId}");
                continue;
            }
            
            // Calculate world position and rotation
            Vector3 worldPosition = GridManager.Instance.GridToWorld(objectData.gridPosition);
            Quaternion worldRotation = Quaternion.Euler(0, objectData.rotation * 90f, 0);
            worldPosition += worldRotation * placementData.pivotOffset;
            
            // Instantiate object
            GameObject placedObject = Instantiate(placementData.prefab, worldPosition, worldRotation);
            
            // Set grid position and rotation
            var machine = placedObject.GetComponent<MachineBase>();
            if (machine != null)
            {
                machine.SetGridPosition(objectData.gridPosition);
                machine.SetRotation(objectData.rotation);
            }
            
            // Store placement data reference
            var placedObjectComponent = placedObject.AddComponent<PlacedObject>();
            placedObjectComponent.PlacementData = placementData;
            placedObjectComponent.GridPosition = objectData.gridPosition;
            placedObjectComponent.Rotation = objectData.rotation;
            
            // Update grid state
            GridManager.Instance.OccupyCells(objectData.gridPosition, placementData.gridSize, placedObject);
        }
    }
}
```

---

## Usage Examples

### Example 1: Simple Building Placement

```csharp
public class SimpleBuildMenu : MonoBehaviour
{
    [SerializeField] private PlacementController placementController;
    [SerializeField] private PlacementData miningDrillData;
    
    public void OnMiningDrillButtonClicked()
    {
        placementController.EnterPlacementMode(miningDrillData);
    }
}
```

### Example 2: Placement with Resource Check

```csharp
public class BuildMenuWithResources : MonoBehaviour
{
    [SerializeField] private PlacementController placementController;
    
    public void OnBuildingButtonClicked(PlacementData placementData)
    {
        // Check resources
        if (!ResourceManager.Instance.CanAfford(placementData.buildCost))
        {
            ShowErrorMessage("Not enough resources!");
            return;
        }
        
        // Enter placement mode
        placementController.EnterPlacementMode(placementData);
    }
    
    private void OnEnable()
    {
        placementController.OnPlacementConfirmed += OnPlacementConfirmed;
    }
    
    private void OnDisable()
    {
        placementController.OnPlacementConfirmed -= OnPlacementConfirmed;
    }
    
    private void OnPlacementConfirmed(GameObject placedObject, Vector3Int gridPosition, int rotation)
    {
        // Deduct resources
        var placedObjectComponent = placedObject.GetComponent<PlacedObject>();
        if (placedObjectComponent != null)
        {
            ResourceManager.Instance.Spend(placedObjectComponent.PlacementData.buildCost);
        }
    }
}
```

### Example 3: Placement with Validation

```csharp
public class AdvancedPlacementValidator : MonoBehaviour
{
    [SerializeField] private PlacementController placementController;
    
    private void OnEnable()
    {
        placementController.OnPlacementModeEntered += OnPlacementModeEntered;
    }
    
    private void OnDisable()
    {
        placementController.OnPlacementModeEntered -= OnPlacementModeEntered;
    }
    
    private void OnPlacementModeEntered(PlacementData placementData)
    {
        // Custom validation logic
        if (placementData.requiresPower && !IsPowerAvailable())
        {
            ShowErrorMessage("No power available!");
            placementController.ExitPlacementMode();
            return;
        }
        
        if (placementData.requiresWater && !IsWaterNearby())
        {
            ShowErrorMessage("Must be placed near water!");
            placementController.ExitPlacementMode();
            return;
        }
    }
    
    private bool IsPowerAvailable()
    {
        // Check if power grid has capacity
        return true; // Placeholder
    }
    
    private bool IsWaterNearby()
    {
        // Check if water is nearby
        return true; // Placeholder
    }
}
```

---

## Testing Integration

### Unit Tests

```csharp
[Test]
public void PlacementController_EnterPlacementMode_TriggersEvent()
{
    // Arrange
    var placementController = new GameObject().AddComponent<PlacementController>();
    var placementData = ScriptableObject.CreateInstance<PlacementData>();
    bool eventTriggered = false;
    
    placementController.OnPlacementModeEntered += (data) => eventTriggered = true;
    
    // Act
    placementController.EnterPlacementMode(placementData);
    
    // Assert
    Assert.IsTrue(eventTriggered);
    Assert.IsTrue(placementController.IsInPlacementMode);
}

[Test]
public void PlacementController_ConfirmPlacement_TriggersEvent()
{
    // Arrange
    var placementController = new GameObject().AddComponent<PlacementController>();
    var placementData = ScriptableObject.CreateInstance<PlacementData>();
    GameObject placedObject = null;
    Vector3Int gridPosition = Vector3Int.zero;
    int rotation = 0;
    
    placementController.OnPlacementConfirmed += (obj, pos, rot) =>
    {
        placedObject = obj;
        gridPosition = pos;
        rotation = rot;
    };
    
    // Act
    placementController.EnterPlacementMode(placementData);
    // Simulate placement confirmation
    // ...
    
    // Assert
    Assert.IsNotNull(placedObject);
    Assert.AreEqual(Vector3Int.zero, gridPosition);
    Assert.AreEqual(0, rotation);
}
```

### Integration Tests

```csharp
[UnityTest]
public IEnumerator PlacementIntegration_BuildMenuToPlacement_WorksCorrectly()
{
    // Arrange
    var buildMenu = new GameObject().AddComponent<SimpleBuildMenu>();
    var placementController = new GameObject().AddComponent<PlacementController>();
    var placementData = ScriptableObject.CreateInstance<PlacementData>();
    
    // Act
    buildMenu.OnBuildingButtonClicked(placementData);
    yield return null;
    
    // Assert
    Assert.IsTrue(placementController.IsInPlacementMode);
}
```

---

## Future Enhancements

### Planned Features

1. **Undo/Redo System**
   - Track placement history
   - Allow undoing placements
   - Refund resources on undo

2. **Blueprint System**
   - Save building layouts as blueprints
   - Load and place entire blueprints
   - Share blueprints between saves

3. **Copy/Paste**
   - Copy existing building configurations
   - Paste with rotation and offset
   - Batch placement

4. **Drag-to-Place**
   - Drag to place multiple conveyors
   - Automatic routing
   - Smart connection

5. **Placement Constraints**
   - Proximity requirements (near power, water, etc.)
   - Terrain restrictions
   - Zoning rules

### Extension Points

- **IPlacementRule Interface**: Custom validation rules
- **IPlacementModifier Interface**: Modify placement behavior
- **PlacementData Inheritance**: Specialized placement types
- **Event System**: Additional placement events

---

## Troubleshooting

### Common Issues

1. **GridManager not found**
   - Ensure GridManager is in the scene
   - Check that GridManager.Instance is not null
   - Verify GridManager Awake() is called

2. **Placement not working**
   - Check that PlacementController is in the scene
   - Verify Input System is configured correctly
   - Check that PlacementData has a valid prefab

3. **Ghost preview not visible**
   - Check that materials are assigned
   - Verify camera is positioned correctly
   - Check that prefab has renderers

4. **Validation always fails**
   - Check grid bounds
   - Verify cell occupation state
   - Check PlacementValidator logic

---

## Summary

The Grid & Placement System provides a solid foundation for spatial organization with clear integration points for:

1. **GameManager**: System initialization
2. **Build Menu UI**: Triggering placement mode
3. **Resource System**: Cost checking and deduction
4. **Machine Framework**: Position and rotation assignment
5. **Save/Load System**: Persistence

Follow the interfaces and examples in this guide to integrate the placement system with your game systems.
