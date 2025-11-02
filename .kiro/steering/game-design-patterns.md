# Game Design Patterns - Santa's Workshop Automation

**Last Updated**: November 2, 2025  
**Purpose**: Architectural patterns and best practices for factory automation systems

This document defines the core design patterns used throughout Santa's Workshop Automation. These patterns ensure consistency, maintainability, and performance across all game systems.

---

## Core Architecture Patterns

### 1. Entity-Component-System (ECS) for Simulation

Use Unity's ECS for high-performance simulation of machines, items, and logistics.

```csharp
// Component: Pure data, no logic
public struct MachineComponent : IComponentData
{
    public Entity MachineType;
    public float ProductionProgress;
    public float PowerConsumption;
    public bool IsActive;
}

// System: Logic that operates on components
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class MachineProductionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        
        Entities
            .WithAll<MachineComponent>()
            .ForEach((ref MachineComponent machine) =>
            {
                if (machine.IsActive)
                {
                    machine.ProductionProgress += deltaTime;
                }
            })
            .ScheduleParallel();
    }
}
```

**When to use ECS**:
- ✅ Machines (hundreds to thousands of instances)
- ✅ Items on conveyors (high throughput)
- ✅ Power grid calculations
- ✅ Resource flow simulation
- ❌ UI (use MonoBehaviour)
- ❌ One-off managers (use MonoBehaviour singletons)

---

### 2. Command Pattern for Player Actions

All player actions (place machine, delete conveyor, etc.) use the Command pattern for undo/redo support.

```csharp
public interface ICommand
{
    void Execute();
    void Undo();
    bool CanExecute();
}

public class PlaceMachineCommand : ICommand
{
    private readonly MachineData _machineData;
    private readonly Vector3Int _gridPosition;
    private Entity _createdEntity;

    public PlaceMachineCommand(MachineData machineData, Vector3Int gridPosition)
    {
        _machineData = machineData;
        _gridPosition = gridPosition;
    }

    public bool CanExecute()
    {
        return GridManager.Instance.IsCellAvailable(_gridPosition) &&
               ResourceManager.Instance.CanAfford(_machineData.Cost);
    }

    public void Execute()
    {
        ResourceManager.Instance.Spend(_machineData.Cost);
        _createdEntity = MachineFactory.CreateMachine(_machineData, _gridPosition);
        GridManager.Instance.OccupyCell(_gridPosition, _createdEntity);
    }

    public void Undo()
    {
        ResourceManager.Instance.Refund(_machineData.Cost);
        MachineFactory.DestroyMachine(_createdEntity);
        GridManager.Instance.FreeCell(_gridPosition);
    }
}

// Command manager
public class CommandManager : MonoBehaviour
{
    private Stack<ICommand> _undoStack = new();
    private Stack<ICommand> _redoStack = new();

    public void ExecuteCommand(ICommand command)
    {
        if (command.CanExecute())
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear(); // Clear redo stack on new action
        }
    }

    public void Undo()
    {
        if (_undoStack.Count > 0)
        {
            ICommand command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
        }
    }

    public void Redo()
    {
        if (_redoStack.Count > 0)
        {
            ICommand command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }
    }
}
```

**Benefits**:
- Undo/redo functionality
- Action history for debugging
- Macro recording (automate repetitive tasks)
- Network synchronization (future multiplayer)

---

### 3. State Machine for Machine Behavior

Machines use a state machine pattern for clear, maintainable behavior.

```csharp
public enum MachineState
{
    Idle,
    WaitingForInput,
    Processing,
    WaitingForOutput,
    NoPower,
    Broken
}

public abstract class MachineBase : MonoBehaviour
{
    protected MachineState _currentState;
    protected Dictionary<MachineState, IMachineState> _states;

    protected virtual void Awake()
    {
        InitializeStates();
        TransitionToState(MachineState.Idle);
    }

    protected abstract void InitializeStates();

    public void TransitionToState(MachineState newState)
    {
        _states[_currentState]?.OnExit();
        _currentState = newState;
        _states[_currentState]?.OnEnter();
    }

    protected virtual void Update()
    {
        _states[_currentState]?.OnUpdate();
    }
}

// State interface
public interface IMachineState
{
    void OnEnter();
    void OnUpdate();
    void OnExit();
}

// Example state implementation
public class ProcessingState : IMachineState
{
    private readonly MachineBase _machine;
    private float _progress;

    public ProcessingState(MachineBase machine)
    {
        _machine = machine;
    }

    public void OnEnter()
    {
        _progress = 0f;
        _machine.PlayAnimation("Processing");
        _machine.PlaySound("MachineWorking");
    }

    public void OnUpdate()
    {
        _progress += Time.deltaTime / _machine.ProcessingTime;
        
        if (_progress >= 1f)
        {
            _machine.ProduceOutput();
            _machine.TransitionToState(MachineState.WaitingForOutput);
        }
    }

    public void OnExit()
    {
        _machine.StopAnimation("Processing");
    }
}
```

**Machine States**:
- **Idle**: No work to do, waiting for input
- **WaitingForInput**: Recipe selected, waiting for resources
- **Processing**: Actively producing
- **WaitingForOutput**: Product ready, output buffer full
- **NoPower**: Insufficient electricity
- **Broken**: Requires maintenance (future feature)

---

### 4. Observer Pattern for Events

Use events for decoupled communication between systems.

```csharp
// Event definitions
public static class GameEvents
{
    // Resource events
    public static event Action<ResourceType, int> OnResourceChanged;
    public static event Action<ResourceType> OnResourceDepleted;

    // Machine events
    public static event Action<Entity, MachineState> OnMachineStateChanged;
    public static event Action<Entity> OnMachineBuilt;
    public static event Action<Entity> OnMachineDestroyed;

    // Research events
    public static event Action<ResearchNode> OnResearchCompleted;
    public static event Action<ResearchNode> OnResearchStarted;

    // Mission events
    public static event Action<Mission> OnMissionCompleted;
    public static event Action<Objective> OnObjectiveUpdated;

    // Invoke methods
    public static void ResourceChanged(ResourceType type, int amount)
    {
        OnResourceChanged?.Invoke(type, amount);
    }

    public static void MachineStateChanged(Entity machine, MachineState newState)
    {
        OnMachineStateChanged?.Invoke(machine, newState);
    }
}

// Subscriber example
public class ResourceUI : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.OnResourceChanged += UpdateResourceDisplay;
        GameEvents.OnResourceDepleted += ShowDepletionWarning;
    }

    private void OnDisable()
    {
        GameEvents.OnResourceChanged -= UpdateResourceDisplay;
        GameEvents.OnResourceDepleted -= ShowDepletionWarning;
    }

    private void UpdateResourceDisplay(ResourceType type, int amount)
    {
        // Update UI
    }

    private void ShowDepletionWarning(ResourceType type)
    {
        // Show warning
    }
}
```

**Event Categories**:
- **Resource Events**: Inventory changes, depletion warnings
- **Machine Events**: State changes, construction, destruction
- **Research Events**: Tech unlocks, progress updates
- **Mission Events**: Objectives, completion, rewards
- **Power Events**: Grid changes, brownouts, blackouts

---

### 5. Factory Pattern for Machine Creation

Centralized machine creation ensures consistency and proper initialization.

```csharp
public class MachineFactory : MonoBehaviour
{
    public static MachineFactory Instance { get; private set; }

    [SerializeField] private MachineDatabase _machineDatabase;

    private EntityManager _entityManager;
    private Dictionary<MachineType, EntityArchetype> _archetypes;

    private void Awake()
    {
        Instance = this;
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        InitializeArchetypes();
    }

    private void InitializeArchetypes()
    {
        _archetypes = new Dictionary<MachineType, EntityArchetype>();

        // Create archetype for each machine type
        _archetypes[MachineType.Extractor] = _entityManager.CreateArchetype(
            typeof(MachineComponent),
            typeof(ExtractorComponent),
            typeof(PowerConsumerComponent),
            typeof(GridPositionComponent)
        );

        _archetypes[MachineType.Processor] = _entityManager.CreateArchetype(
            typeof(MachineComponent),
            typeof(ProcessorComponent),
            typeof(RecipeComponent),
            typeof(PowerConsumerComponent),
            typeof(GridPositionComponent)
        );

        // ... more archetypes
    }

    public Entity CreateMachine(MachineData data, Vector3Int gridPosition)
    {
        // Create entity with appropriate archetype
        Entity entity = _entityManager.CreateEntity(_archetypes[data.Type]);

        // Initialize components
        _entityManager.SetComponentData(entity, new MachineComponent
        {
            MachineType = data.EntityReference,
            ProductionProgress = 0f,
            PowerConsumption = data.PowerConsumption,
            IsActive = false
        });

        _entityManager.SetComponentData(entity, new GridPositionComponent
        {
            Position = gridPosition
        });

        // Create visual representation
        GameObject visual = Instantiate(data.Prefab, GridToWorld(gridPosition), Quaternion.identity);
        visual.GetComponent<MachineView>().Initialize(entity);

        // Register with managers
        PowerGrid.Instance.RegisterConsumer(entity, data.PowerConsumption);
        GridManager.Instance.OccupyCell(gridPosition, entity);

        // Fire event
        GameEvents.MachineBuilt(entity);

        return entity;
    }

    public void DestroyMachine(Entity entity)
    {
        // Unregister from managers
        PowerGrid.Instance.UnregisterConsumer(entity);
        
        var gridPos = _entityManager.GetComponentData<GridPositionComponent>(entity);
        GridManager.Instance.FreeCell(gridPos.Position);

        // Destroy visual
        // (handled by MachineView component listening to entity destruction)

        // Fire event
        GameEvents.MachineDestroyed(entity);

        // Destroy entity
        _entityManager.DestroyEntity(entity);
    }

    private Vector3 GridToWorld(Vector3Int gridPos)
    {
        return new Vector3(gridPos.x, 0, gridPos.z);
    }
}
```

**Factory Responsibilities**:
- Entity creation with correct archetype
- Component initialization
- Visual instantiation
- Manager registration (power, grid, etc.)
- Event firing

---

### 6. Object Pool for Items

Items on conveyors use object pooling to avoid allocation overhead.

```csharp
public class ItemPool : MonoBehaviour
{
    public static ItemPool Instance { get; private set; }

    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private int _initialPoolSize = 100;

    private Queue<GameObject> _pool = new();
    private HashSet<GameObject> _activeItems = new();

    private void Awake()
    {
        Instance = this;
        
        // Pre-instantiate pool
        for (int i = 0; i < _initialPoolSize; i++)
        {
            GameObject item = Instantiate(_itemPrefab);
            item.SetActive(false);
            _pool.Enqueue(item);
        }
    }

    public GameObject GetItem(ResourceType type, Vector3 position)
    {
        GameObject item;

        if (_pool.Count > 0)
        {
            item = _pool.Dequeue();
        }
        else
        {
            // Pool exhausted, create new item
            item = Instantiate(_itemPrefab);
        }

        item.transform.position = position;
        item.SetActive(true);
        
        var itemView = item.GetComponent<ItemView>();
        itemView.SetResourceType(type);

        _activeItems.Add(item);
        return item;
    }

    public void ReturnItem(GameObject item)
    {
        if (_activeItems.Remove(item))
        {
            item.SetActive(false);
            _pool.Enqueue(item);
        }
    }

    public void ReturnAllItems()
    {
        foreach (var item in _activeItems.ToList())
        {
            ReturnItem(item);
        }
    }
}
```

**Pooling Strategy**:
- Pre-allocate 100 items at startup
- Expand pool dynamically if needed
- Return items when consumed or destroyed
- Clear pool between scenes/saves

---

### 7. Grid System for Placement

All machines and logistics snap to a grid for clean placement and pathfinding.

```csharp
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private int _gridWidth = 100;
    [SerializeField] private int _gridHeight = 100;
    [SerializeField] private float _cellSize = 1f;

    private Dictionary<Vector3Int, Entity> _occupiedCells = new();
    private HashSet<Vector3Int> _reservedCells = new();

    private void Awake()
    {
        Instance = this;
    }

    public bool IsCellAvailable(Vector3Int gridPos)
    {
        return IsWithinBounds(gridPos) && 
               !_occupiedCells.ContainsKey(gridPos) &&
               !_reservedCells.Contains(gridPos);
    }

    public bool IsCellAvailable(Vector3Int gridPos, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (!IsCellAvailable(gridPos + new Vector3Int(x, 0, y)))
                    return false;
            }
        }
        return true;
    }

    public void OccupyCell(Vector3Int gridPos, Entity entity)
    {
        _occupiedCells[gridPos] = entity;
    }

    public void OccupyCells(Vector3Int gridPos, Vector2Int size, Entity entity)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                _occupiedCells[gridPos + new Vector3Int(x, 0, y)] = entity;
            }
        }
    }

    public void FreeCell(Vector3Int gridPos)
    {
        _occupiedCells.Remove(gridPos);
    }

    public Entity GetEntityAtCell(Vector3Int gridPos)
    {
        return _occupiedCells.TryGetValue(gridPos, out Entity entity) ? entity : Entity.Null;
    }

    public Vector3Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x / _cellSize),
            0,
            Mathf.FloorToInt(worldPos.z / _cellSize)
        );
    }

    public Vector3 GridToWorld(Vector3Int gridPos)
    {
        return new Vector3(
            gridPos.x * _cellSize + _cellSize * 0.5f,
            0,
            gridPos.z * _cellSize + _cellSize * 0.5f
        );
    }

    private bool IsWithinBounds(Vector3Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < _gridWidth &&
               gridPos.z >= 0 && gridPos.z < _gridHeight;
    }
}
```

**Grid Features**:
- Snap-to-grid placement
- Multi-cell occupation (large machines)
- Cell reservation (during placement preview)
- World ↔ Grid coordinate conversion
- Bounds checking

---

### 8. Recipe System

Recipes define input/output relationships for machines.

```csharp
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Game/Recipe")]
public class Recipe : ScriptableObject
{
    [Header("Basic Info")]
    public string recipeName;
    public Sprite icon;

    [Header("Inputs")]
    public ResourceAmount[] inputs;

    [Header("Outputs")]
    public ResourceAmount[] outputs;

    [Header("Processing")]
    public float processingTime = 1f;
    public float powerConsumption = 10f;

    [Header("Requirements")]
    public MachineType requiredMachineType;
    public int requiredMachineTier = 1;

    public bool CanProcess(Dictionary<ResourceType, int> availableResources)
    {
        foreach (var input in inputs)
        {
            if (!availableResources.TryGetValue(input.type, out int available) ||
                available < input.amount)
            {
                return false;
            }
        }
        return true;
    }

    public void ConsumeInputs(Dictionary<ResourceType, int> resources)
    {
        foreach (var input in inputs)
        {
            resources[input.type] -= input.amount;
        }
    }

    public void ProduceOutputs(Dictionary<ResourceType, int> resources)
    {
        foreach (var output in outputs)
        {
            if (resources.ContainsKey(output.type))
                resources[output.type] += output.amount;
            else
                resources[output.type] = output.amount;
        }
    }
}

[System.Serializable]
public struct ResourceAmount
{
    public ResourceType type;
    public int amount;
}
```

**Recipe Examples**:
- **Iron Smelting**: Iron Ore (1) → Iron Ingot (1), 2s, 20W
- **Toy Assembly**: Wood (2) + Paint (1) → Wooden Toy (1), 5s, 50W
- **Magic Infusion**: Toy (1) + Magic Dust (1) → Enchanted Toy (1), 10s, 100W

---

### 9. Spatial Partitioning for Performance

Use spatial partitioning to update only visible/relevant entities.

```csharp
public class SpatialPartitionSystem : SystemBase
{
    private const int CHUNK_SIZE = 16;
    private Dictionary<Vector2Int, List<Entity>> _chunks = new();

    protected override void OnUpdate()
    {
        // Clear chunks
        foreach (var chunk in _chunks.Values)
        {
            chunk.Clear();
        }

        // Partition entities into chunks
        Entities
            .WithAll<GridPositionComponent>()
            .ForEach((Entity entity, in GridPositionComponent gridPos) =>
            {
                Vector2Int chunkCoord = new Vector2Int(
                    gridPos.Position.x / CHUNK_SIZE,
                    gridPos.Position.z / CHUNK_SIZE
                );

                if (!_chunks.ContainsKey(chunkCoord))
                {
                    _chunks[chunkCoord] = new List<Entity>();
                }

                _chunks[chunkCoord].Add(entity);
            })
            .WithoutBurst()
            .Run();
    }

    public List<Entity> GetEntitiesInChunk(Vector2Int chunkCoord)
    {
        return _chunks.TryGetValue(chunkCoord, out var entities) ? entities : new List<Entity>();
    }

    public List<Entity> GetEntitiesNearPosition(Vector3Int gridPos, int radius)
    {
        List<Entity> nearbyEntities = new();
        Vector2Int centerChunk = new Vector2Int(gridPos.x / CHUNK_SIZE, gridPos.z / CHUNK_SIZE);

        int chunkRadius = Mathf.CeilToInt(radius / (float)CHUNK_SIZE);

        for (int x = -chunkRadius; x <= chunkRadius; x++)
        {
            for (int z = -chunkRadius; z <= chunkRadius; z++)
            {
                Vector2Int chunkCoord = centerChunk + new Vector2Int(x, z);
                if (_chunks.TryGetValue(chunkCoord, out var entities))
                {
                    nearbyEntities.AddRange(entities);
                }
            }
        }

        return nearbyEntities;
    }
}
```

**Performance Benefits**:
- Update only visible chunks
- Fast neighbor queries for logistics
- Efficient collision detection
- Scalable to large factories

---

### 10. Save/Load System

Versioned save system with forward compatibility.

```csharp
[System.Serializable]
public class SaveData
{
    public int version = 1;
    public string saveName;
    public DateTime saveTime;
    
    public ResourceSaveData resources;
    public MachineSaveData[] machines;
    public ResearchSaveData research;
    public MissionSaveData missions;
}

public class SaveLoadSystem : MonoBehaviour
{
    public static SaveLoadSystem Instance { get; private set; }

    private const int CURRENT_VERSION = 1;
    private const string SAVE_FOLDER = "Saves";

    private void Awake()
    {
        Instance = this;
    }

    public void SaveGame(string saveName)
    {
        SaveData data = new SaveData
        {
            version = CURRENT_VERSION,
            saveName = saveName,
            saveTime = DateTime.Now,
            resources = ResourceManager.Instance.GetSaveData(),
            machines = MachineManager.Instance.GetSaveData(),
            research = ResearchManager.Instance.GetSaveData(),
            missions = MissionManager.Instance.GetSaveData()
        };

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, SAVE_FOLDER, $"{saveName}.json");
        
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, json);

        Debug.Log($"Game saved: {path}");
    }

    public void LoadGame(string saveName)
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_FOLDER, $"{saveName}.json");

        if (!File.Exists(path))
        {
            Debug.LogError($"Save file not found: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Version migration
        if (data.version < CURRENT_VERSION)
        {
            data = MigrateSaveData(data);
        }

        // Load data into managers
        ResourceManager.Instance.LoadSaveData(data.resources);
        MachineManager.Instance.LoadSaveData(data.machines);
        ResearchManager.Instance.LoadSaveData(data.research);
        MissionManager.Instance.LoadSaveData(data.missions);

        Debug.Log($"Game loaded: {saveName}");
    }

    private SaveData MigrateSaveData(SaveData oldData)
    {
        // Handle version migrations
        // Example: v1 → v2 adds new fields with defaults
        return oldData;
    }
}
```

**Save System Features**:
- JSON format (human-readable, debuggable)
- Version tracking for migrations
- Modular save data (each manager handles its own)
- Persistent data path (survives game updates)

---

## Performance Patterns

### Burst Compilation for Hot Paths

Use Burst compiler for performance-critical systems.

```csharp
[BurstCompile]
public partial struct ConveyorUpdateJob : IJobEntity
{
    public float DeltaTime;

    public void Execute(ref ConveyorComponent conveyor, in GridPositionComponent gridPos)
    {
        if (conveyor.HasItem)
        {
            conveyor.ItemProgress += DeltaTime * conveyor.Speed;

            if (conveyor.ItemProgress >= 1f)
            {
                // Item reached end of conveyor
                conveyor.ItemProgress = 0f;
                conveyor.HasItem = false;
            }
        }
    }
}
```

### Job System for Parallelization

Use Job System for parallel processing.

```csharp
public partial class PowerGridSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var calculatePowerJob = new CalculatePowerConsumptionJob
        {
            DeltaTime = Time.DeltaTime
        };

        Dependency = calculatePowerJob.ScheduleParallel(Dependency);
    }
}

[BurstCompile]
public partial struct CalculatePowerConsumptionJob : IJobEntity
{
    public float DeltaTime;

    public void Execute(ref PowerConsumerComponent consumer, in MachineComponent machine)
    {
        if (machine.IsActive)
        {
            consumer.CurrentConsumption = machine.PowerConsumption;
        }
        else
        {
            consumer.CurrentConsumption = 0f;
        }
    }
}
```

---

## UI Patterns

### MVVM for UI Toolkit

Use Model-View-ViewModel pattern for UI.

```csharp
// Model: Game data
public class ResourceModel
{
    public ResourceType Type { get; set; }
    public int Amount { get; set; }
    public int Capacity { get; set; }
}

// ViewModel: UI logic and data binding
public class ResourceViewModel
{
    private ResourceModel _model;
    
    public string DisplayName => _model.Type.ToString();
    public string AmountText => $"{_model.Amount} / {_model.Capacity}";
    public float FillPercentage => (float)_model.Amount / _model.Capacity;

    public ResourceViewModel(ResourceModel model)
    {
        _model = model;
    }

    public void UpdateModel(ResourceModel newModel)
    {
        _model = newModel;
        OnPropertyChanged?.Invoke();
    }

    public event Action OnPropertyChanged;
}

// View: UI Toolkit visual element
public class ResourceView : VisualElement
{
    private Label _nameLabel;
    private Label _amountLabel;
    private ProgressBar _fillBar;
    private ResourceViewModel _viewModel;

    public ResourceView()
    {
        // Create UI elements
        _nameLabel = new Label();
        _amountLabel = new Label();
        _fillBar = new ProgressBar();

        Add(_nameLabel);
        Add(_amountLabel);
        Add(_fillBar);
    }

    public void BindViewModel(ResourceViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.OnPropertyChanged += UpdateView;
        UpdateView();
    }

    private void UpdateView()
    {
        _nameLabel.text = _viewModel.DisplayName;
        _amountLabel.text = _viewModel.AmountText;
        _fillBar.value = _viewModel.FillPercentage;
    }
}
```

---

## Summary

These patterns form the foundation of Santa's Workshop Automation:

1. **ECS**: High-performance simulation
2. **Command**: Undo/redo support
3. **State Machine**: Clear machine behavior
4. **Observer**: Decoupled event communication
5. **Factory**: Consistent entity creation
6. **Object Pool**: Efficient item management
7. **Grid System**: Snap-to-grid placement
8. **Recipe System**: Flexible production chains
9. **Spatial Partitioning**: Performance optimization
10. **Save/Load**: Persistent game state

Follow these patterns consistently for maintainable, performant code.
