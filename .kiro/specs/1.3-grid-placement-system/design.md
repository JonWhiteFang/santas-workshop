# Design Document - Grid & Placement System

## Overview

The Grid & Placement System provides a robust foundation for spatial organization in Santa's Workshop Automation. This system consists of two primary components: the Grid System (data layer) and the Placement System (interaction layer). The Grid System manages the underlying tile-based coordinate space, while the Placement System handles player interaction, validation, and visual feedback during object placement.

The design follows Unity best practices with a clear separation between data (grid state), logic (validation and placement), and presentation (visual feedback). The system is designed to be performant, supporting hundreds of placed objects while maintaining 60 FPS, and extensible to support future features like underground layers, elevated platforms, and dynamic grid modifications.

## Architecture

### System Components

```
┌─────────────────────────────────────────────────────────────┐
│                    Placement System                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Placement  │  │    Ghost     │  │   Placement  │      │
│  │  Controller  │──│   Preview    │  │  Validator   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│         │                  │                  │              │
└─────────┼──────────────────┼──────────────────┼──────────────┘
          │                  │                  │
          ▼                  ▼                  ▼
┌─────────────────────────────────────────────────────────────┐
│                      Grid System                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │     Grid     │  │     Grid     │  │     Grid     │      │
│  │   Manager    │──│     Data     │  │  Visualizer  │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────────┐
│                   Placed Objects                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Machines   │  │  Conveyors   │  │  Buildings   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
```

### Data Flow

1. **Placement Initiation**: Player selects object → PlacementController enters Placement Mode → GhostPreview instantiated
2. **Position Update**: Cursor moves → PlacementController converts to Grid Position → GhostPreview updates position
3. **Validation**: GhostPreview position changes → PlacementValidator checks GridData → Visual feedback updated
4. **Confirmation**: Player confirms → PlacementValidator validates → Object created → GridData updated → Placement Mode exits
5. **Cancellation**: Player cancels → GhostPreview destroyed → Placement Mode exits

## Components and Interfaces

### 1. GridManager (Singleton MonoBehaviour)

**Responsibility**: Central manager for grid operations, coordinate conversion, and cell state management.

```csharp
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Configuration")]
    [SerializeField] private int gridWidth = 100;
    [SerializeField] private int gridHeight = 100;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

    private GridData _gridData;
    private GridVisualizer _visualizer;

    // Initialization
    private void Awake();
    
    // Coordinate Conversion
    public Vector3Int WorldToGrid(Vector3 worldPosition);
    public Vector3 GridToWorld(Vector3Int gridPosition);
    
    // Cell State Queries
    public bool IsCellAvailable(Vector3Int gridPosition);
    public bool AreCellsAvailable(Vector3Int gridPosition, Vector2Int size);
    public bool IsWithinBounds(Vector3Int gridPosition);
    
    // Cell State Modification
    public void OccupyCell(Vector3Int gridPosition, GameObject occupant);
    public void OccupyCells(Vector3Int gridPosition, Vector2Int size, GameObject occupant);
    public void FreeCell(Vector3Int gridPosition);
    public void FreeCells(Vector3Int gridPosition, Vector2Int size);
    
    // Cell Queries
    public GameObject GetOccupant(Vector3Int gridPosition);
    public List<Vector3Int> GetOccupiedCells(GameObject occupant);
    
    // Grid Visualization
    public void SetGridVisualizationEnabled(bool enabled);
}
```

**Key Design Decisions**:
- Singleton pattern for global access (acceptable for core systems)
- Cell size of 1.0 Unity units for simplicity
- Grid origin at world (0, 0, 0) by default, configurable for future multi-grid support
- Y-coordinate always 0 for 2D grid (future: support multiple Y levels)

### 2. GridData (Data Structure)

**Responsibility**: Stores grid cell occupation state and provides efficient lookups.

```csharp
public class GridData
{
    private Dictionary<Vector3Int, GameObject> _occupiedCells;
    private Dictionary<GameObject, List<Vector3Int>> _occupantToCells;
    private int _width;
    private int _height;

    public GridData(int width, int height);
    
    // Cell State
    public bool IsCellOccupied(Vector3Int position);
    public void SetCellOccupied(Vector3Int position, GameObject occupant);
    public void SetCellFree(Vector3Int position);
    
    // Occupant Tracking
    public GameObject GetOccupant(Vector3Int position);
    public List<Vector3Int> GetCellsForOccupant(GameObject occupant);
    public void RegisterOccupant(GameObject occupant, List<Vector3Int> cells);
    public void UnregisterOccupant(GameObject occupant);
    
    // Bounds Checking
    public bool IsWithinBounds(Vector3Int position);
    
    // Serialization Support (for save/load)
    public GridDataSaveFormat ToSaveFormat();
    public void LoadFromSaveFormat(GridDataSaveFormat saveData);
}
```

**Key Design Decisions**:
- Dictionary-based storage for sparse grids (efficient for large, mostly empty grids)
- Bidirectional mapping (cell → occupant, occupant → cells) for fast queries
- Separate bounds checking for validation
- Save/load support for persistence

### 3. PlacementController (MonoBehaviour)

**Responsibility**: Manages placement mode state, user input, and orchestrates placement operations.

```csharp
public class PlacementController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode rotateKey = KeyCode.R;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;
    
    [Header("Prefabs")]
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    
    private bool _isInPlacementMode;
    private GameObject _currentGhostPreview;
    private PlacementData _currentPlacementData;
    private int _currentRotation; // 0, 1, 2, 3 for 0°, 90°, 180°, 270°
    
    // Placement Mode Control
    public void EnterPlacementMode(PlacementData placementData);
    public void ExitPlacementMode();
    public bool IsInPlacementMode { get; }
    
    // Update Loop
    private void Update();
    
    // Input Handling
    private void HandleRotationInput();
    private void HandleConfirmInput();
    private void HandleCancelInput();
    
    // Ghost Preview Management
    private void UpdateGhostPreview();
    private void CreateGhostPreview();
    private void DestroyGhostPreview();
    
    // Placement Execution
    private void ConfirmPlacement();
    private void CancelPlacement();
    
    // Rotation
    private void RotateGhostPreview();
    private Quaternion GetRotationForState(int rotationState);
}
```

**Key Design Decisions**:
- Single active placement at a time (no multi-select)
- Rotation in 90-degree increments (0-3 states)
- Ghost preview updates every frame for smooth feedback
- Input handling in Update() for responsiveness

### 4. PlacementData (ScriptableObject)

**Responsibility**: Defines the properties of an object that can be placed.

```csharp
[CreateAssetMenu(fileName = "NewPlacementData", menuName = "Game/Placement Data")]
public class PlacementData : ScriptableObject
{
    [Header("Basic Info")]
    public string objectName;
    public GameObject prefab;
    
    [Header("Grid Properties")]
    public Vector2Int gridSize = new Vector2Int(1, 1);
    public Vector3 pivotOffset = Vector3.zero; // Offset from grid position to object pivot
    
    [Header("Placement Rules")]
    public bool canRotate = true;
    public bool requiresFlatGround = true;
    public PlacementLayer allowedLayers = PlacementLayer.Ground;
    
    [Header("Visual")]
    public Material ghostMaterial;
    
    // Helper Methods
    public List<Vector3Int> GetOccupiedCells(Vector3Int basePosition, int rotation);
    public Quaternion GetRotation(int rotationState);
}

[System.Flags]
public enum PlacementLayer
{
    Ground = 1 << 0,
    Elevated = 1 << 1,
    Underground = 1 << 2
}
```

**Key Design Decisions**:
- ScriptableObject for data-driven design (designers can create new placeable objects)
- Grid size supports multi-cell objects (1x1, 2x2, 3x1, etc.)
- Pivot offset allows flexible object origins
- Placement layers for future vertical building support

### 5. PlacementValidator (Static Utility)

**Responsibility**: Validates placement attempts and provides detailed feedback.

```csharp
public static class PlacementValidator
{
    public enum ValidationResult
    {
        Valid,
        OutOfBounds,
        CellOccupied,
        InvalidTerrain,
        InsufficientResources,
        RequirementNotMet
    }
    
    public struct ValidationInfo
    {
        public ValidationResult result;
        public string message;
        public List<Vector3Int> invalidCells;
    }
    
    // Primary Validation
    public static ValidationInfo ValidatePlacement(
        Vector3Int gridPosition,
        PlacementData placementData,
        int rotation);
    
    // Specific Checks
    private static bool CheckBounds(Vector3Int position, Vector2Int size);
    private static bool CheckOccupation(Vector3Int position, Vector2Int size);
    private static bool CheckTerrain(Vector3Int position, Vector2Int size);
    
    // Helper Methods
    private static List<Vector3Int> GetOccupiedCells(
        Vector3Int basePosition,
        Vector2Int size,
        int rotation);
}
```

**Key Design Decisions**:
- Static utility class (no state, pure validation logic)
- Detailed validation results for UI feedback
- Extensible validation rules (terrain, resources, requirements)
- Returns list of invalid cells for visual highlighting

### 6. GhostPreview (MonoBehaviour)

**Responsibility**: Visual representation of object during placement, with validation feedback.

```csharp
public class GhostPreview : MonoBehaviour
{
    private GameObject _previewObject;
    private List<Renderer> _renderers;
    private Material _validMaterial;
    private Material _invalidMaterial;
    private bool _isValid;
    
    // Initialization
    public void Initialize(GameObject prefab, Material validMat, Material invalidMat);
    
    // Position and Rotation
    public void UpdatePosition(Vector3 worldPosition);
    public void UpdateRotation(Quaternion rotation);
    
    // Validation Feedback
    public void SetValid(bool isValid);
    
    // Material Management
    private void ApplyMaterials();
    private void SetMaterialsRecursive(GameObject obj, Material material);
    
    // Cleanup
    public void Destroy();
}
```

**Key Design Decisions**:
- Separate component for ghost preview logic
- Material override for validation feedback (green/red tint)
- Recursive material application for complex prefabs
- Smooth position updates (no lerping, instant snap for precision)

### 7. GridVisualizer (MonoBehaviour)

**Responsibility**: Renders grid lines for visual reference (optional feature).

```csharp
public class GridVisualizer : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Material gridLineMaterial;
    [SerializeField] private Color gridLineColor = new Color(1, 1, 1, 0.2f);
    [SerializeField] private float lineWidth = 0.02f;
    
    private LineRenderer[] _gridLines;
    private bool _isVisible;
    
    // Initialization
    public void Initialize(int width, int height, float cellSize);
    
    // Visibility Control
    public void SetVisible(bool visible);
    
    // Grid Line Generation
    private void GenerateGridLines();
    private void CreateHorizontalLines();
    private void CreateVerticalLines();
    
    // Cleanup
    private void OnDestroy();
}
```

**Key Design Decisions**:
- LineRenderer for grid lines (simple, performant)
- Optional feature (can be toggled off)
- Generated once at initialization (static grid)
- Semi-transparent lines (don't obscure gameplay)

## Data Models

### Grid Cell State

```csharp
public struct GridCellState
{
    public bool isOccupied;
    public GameObject occupant;
    public PlacementLayer layer;
}
```

### Placement Context

```csharp
public struct PlacementContext
{
    public PlacementData placementData;
    public Vector3Int gridPosition;
    public int rotation;
    public Vector3 worldPosition;
    public Quaternion worldRotation;
}
```

### Grid Configuration

```csharp
[System.Serializable]
public struct GridConfiguration
{
    public int width;
    public int height;
    public float cellSize;
    public Vector3 origin;
}
```

## Error Handling

### Validation Errors

1. **Out of Bounds**: Position outside grid boundaries
   - **Handling**: Display error message, prevent placement, highlight invalid area
   
2. **Cell Occupied**: Target cells already contain objects
   - **Handling**: Display error message, show occupied cells, suggest alternative location
   
3. **Invalid Terrain**: Terrain doesn't meet placement requirements
   - **Handling**: Display error message, explain terrain requirement
   
4. **Insufficient Resources**: Player lacks resources to build (future integration)
   - **Handling**: Display error message, show required resources

### Runtime Errors

1. **Null Reference**: Missing GridManager or PlacementController
   - **Handling**: Log error, fail gracefully, prevent placement
   
2. **Invalid Prefab**: PlacementData references null or invalid prefab
   - **Handling**: Log error, prevent placement mode entry
   
3. **Coordinate Overflow**: Grid position exceeds int bounds
   - **Handling**: Clamp to valid range, log warning

## Testing Strategy

### Unit Tests

1. **GridManager Tests**
   - Coordinate conversion (world ↔ grid)
   - Bounds checking
   - Cell occupation tracking
   - Multi-cell occupation

2. **PlacementValidator Tests**
   - Single-cell validation
   - Multi-cell validation
   - Rotation validation
   - Edge case handling (corners, boundaries)

3. **GridData Tests**
   - Cell state management
   - Occupant tracking
   - Bidirectional mapping integrity

### Integration Tests

1. **Placement Flow Tests**
   - Enter placement mode → move cursor → confirm placement
   - Enter placement mode → rotate → confirm placement
   - Enter placement mode → cancel placement
   - Place multi-cell object
   - Attempt invalid placement

2. **Grid Visualization Tests**
   - Toggle grid visibility
   - Grid lines render correctly
   - Grid lines update with camera

### Manual Testing

1. **Visual Feedback**
   - Ghost preview appears correctly
   - Valid/invalid materials apply correctly
   - Rotation visual updates smoothly
   - Grid lines render at correct positions

2. **Edge Cases**
   - Place at grid boundaries
   - Place overlapping objects (should fail)
   - Rotate near boundaries
   - Place large objects (3x3, 5x5)

3. **Performance**
   - Place 100+ objects
   - Toggle grid visualization with many objects
   - Update ghost preview at 60 FPS

## Performance Considerations

### Optimization Strategies

1. **Sparse Grid Storage**
   - Use Dictionary instead of 2D array (memory efficient for large, sparse grids)
   - Only store occupied cells
   - Expected memory: ~100 bytes per occupied cell

2. **Ghost Preview Updates**
   - Update position only when cursor moves to new grid cell (not every frame)
   - Cache validation results for current cell
   - Revalidate only on position or rotation change

3. **Grid Visualization**
   - Generate grid lines once at initialization
   - Use object pooling for line renderers (if dynamic grid needed)
   - Cull grid lines outside camera frustum (future optimization)

4. **Validation Caching**
   - Cache validation results for current placement context
   - Invalidate cache on position, rotation, or grid state change
   - Expected speedup: 10-100x for repeated validations

### Performance Targets

- **Grid Operations**: < 0.1ms per operation (coordinate conversion, validation)
- **Ghost Preview Update**: < 0.5ms per frame
- **Placement Confirmation**: < 1ms (including grid update and object instantiation)
- **Grid Visualization**: < 2ms per frame (when enabled)
- **Memory Usage**: < 10MB for 100x100 grid with 1000 placed objects

## Future Extensibility

### Planned Extensions

1. **Multi-Layer Grids**
   - Support for underground and elevated layers
   - Layer-specific placement rules
   - Visual indicators for current layer

2. **Dynamic Grid Modification**
   - Terrain deformation
   - Expandable grid boundaries
   - Grid cell type variations (water, rock, etc.)

3. **Advanced Placement Features**
   - Copy/paste building layouts
   - Blueprint system
   - Undo/redo for placement
   - Drag-to-place for conveyors

4. **Placement Constraints**
   - Proximity requirements (e.g., must be near power source)
   - Terrain slope restrictions
   - Zoning rules (residential, industrial, etc.)

### Extension Points

- **IPlacementRule Interface**: Custom validation rules
- **IGridModifier Interface**: Dynamic grid modifications
- **PlacementData Inheritance**: Specialized placement types
- **Event System**: Placement events for other systems to react

## Integration Points

### Resource System Integration

```csharp
// Check if player has resources to build
public bool CanAffordPlacement(PlacementData data)
{
    return ResourceManager.Instance.CanAfford(data.buildCost);
}

// Deduct resources on placement
private void OnPlacementConfirmed(PlacementContext context)
{
    ResourceManager.Instance.Spend(context.placementData.buildCost);
}
```

### Machine Framework Integration

```csharp
// Notify machine of grid position on placement
private void OnPlacementConfirmed(PlacementContext context)
{
    var machine = placedObject.GetComponent<MachineBase>();
    if (machine != null)
    {
        machine.SetGridPosition(context.gridPosition);
        machine.SetRotation(context.rotation);
    }
}
```

### Save/Load Integration

```csharp
// Save grid state
public GridSaveData GetSaveData()
{
    return new GridSaveData
    {
        occupiedCells = _gridData.ToSaveFormat(),
        gridConfiguration = new GridConfiguration
        {
            width = gridWidth,
            height = gridHeight,
            cellSize = cellSize,
            origin = gridOrigin
        }
    };
}

// Load grid state
public void LoadSaveData(GridSaveData saveData)
{
    _gridData.LoadFromSaveFormat(saveData.occupiedCells);
    // Recreate placed objects from save data
}
```

## Visual Design

### Ghost Preview Materials

```
Valid Placement Material:
- Base Color: White (1, 1, 1, 1)
- Emission: Green (0, 1, 0, 0.3)
- Transparency: 50%
- Shader: URP/Lit with transparency

Invalid Placement Material:
- Base Color: White (1, 1, 1, 1)
- Emission: Red (1, 0, 0, 0.3)
- Transparency: 50%
- Shader: URP/Lit with transparency
```

### Grid Line Appearance

```
Grid Lines:
- Color: White (1, 1, 1, 0.2)
- Width: 0.02 Unity units
- Material: Unlit/Color with transparency
- Render Queue: Transparent
```

## Implementation Notes

### Coordinate System

- **World Space**: Unity's standard 3D coordinate system (float precision)
- **Grid Space**: Integer coordinates (x, y, z) where y is always 0 for ground level
- **Conversion**: `gridX = floor(worldX / cellSize)`, `worldX = gridX * cellSize + cellSize/2`

### Rotation Handling

- **Rotation States**: 0 (0°), 1 (90°), 2 (180°), 3 (270°)
- **Quaternion Conversion**: `Quaternion.Euler(0, rotationState * 90, 0)`
- **Cell Calculation**: Rotate grid size vector by rotation state to get occupied cells

### Multi-Cell Objects

- **Anchor Point**: Bottom-left cell (minimum x, minimum z)
- **Size**: Width (x-axis) and Height (z-axis) in grid cells
- **Occupied Cells**: All cells from anchor to (anchor + size - 1)

Example: 2x3 object at (5, 0, 10) occupies cells:
- (5, 0, 10), (6, 0, 10)
- (5, 0, 11), (6, 0, 11)
- (5, 0, 12), (6, 0, 12)

## Summary

The Grid & Placement System provides a solid foundation for spatial organization in Santa's Workshop Automation. The design emphasizes:

1. **Clarity**: Clear separation between data, logic, and presentation
2. **Performance**: Efficient data structures and caching strategies
3. **Extensibility**: Designed for future features (multi-layer, dynamic grids)
4. **Usability**: Intuitive visual feedback and validation
5. **Integration**: Clean interfaces for other systems (resources, machines, save/load)

The system is ready for implementation with well-defined components, interfaces, and data models. The testing strategy ensures reliability, and performance considerations guarantee smooth gameplay even with hundreds of placed objects.
