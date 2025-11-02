# Design Document: Resource System

## Overview

This design document outlines the technical implementation of the Resource System for Santa's Workshop Automation. The Resource System is a foundational component that manages all resources in the game, from raw materials like wood and iron ore to finished toys. It provides centralized tracking, validation, and event-driven updates to ensure consistent resource management across all game systems.

The system is designed to be performant, supporting thousands of resource transactions per second while maintaining data integrity and providing real-time updates to UI and other dependent systems.

## Architecture

### System Components

```
┌─────────────────────────────────────────────────────────────┐
│                    Resource System                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────┐         ┌──────────────────┐        │
│  │ ResourceManager  │◄────────┤ ResourceDatabase │        │
│  │   (Singleton)    │         │  (Dictionary)    │        │
│  └────────┬─────────┘         └──────────────────┘        │
│           │                                                 │
│           │ manages                                         │
│           ▼                                                 │
│  ┌──────────────────┐         ┌──────────────────┐        │
│  │ Global Resource  │         │  Resource Data   │        │
│  │     Counts       │         │ (ScriptableObj)  │        │
│  │  (Dictionary)    │         └──────────────────┘        │
│  └──────────────────┘                                      │
│           │                                                 │
│           │ fires events                                    │
│           ▼                                                 │
│  ┌──────────────────┐                                      │
│  │ OnResourceChanged│                                      │
│  │     (Event)      │                                      │
│  └──────────────────┘                                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
         │                    │                    │
         ▼                    ▼                    ▼
   ┌─────────┐          ┌─────────┐         ┌─────────┐
   │Machines │          │   UI    │         │Logistics│
   └─────────┘          └─────────┘         └─────────┘
```

**Design Rationale**: 
- Centralized ResourceManager provides single source of truth for all resource counts
- Dictionary-based storage enables O(1) lookups and updates
- Event-driven architecture decouples resource changes from dependent systems
- ScriptableObject data enables designer-friendly resource configuration

### Data Flow

```
Resource Production Flow:
[Extractor Machine] → AddResource() → ResourceManager → OnResourceChanged → [UI Update]

Resource Consumption Flow:
[Processor Machine] → TryConsumeResources() → ResourceManager → OnResourceChanged → [UI Update]
                                                      ↓
                                              [Validation Check]
                                                      ↓
                                              [Success/Failure]
```

## Components and Interfaces

### ResourceData ScriptableObject

Defines the properties and configuration for each resource type:

```csharp
using UnityEngine;

namespace SantasWorkshop.Data
{
    [CreateAssetMenu(fileName = "NewResource", menuName = "Santa/Resource Data")]
    public class ResourceData : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this resource")]
        public string resourceId;
        
        [Tooltip("Display name shown in UI")]
        public string displayName;
        
        [TextArea(3, 5)]
        [Tooltip("Description of the resource")]
        public string description;

        [Header("Visual")]
        [Tooltip("Icon displayed in UI")]
        public Sprite icon;
        
        [Tooltip("3D model for items on conveyors")]
        public GameObject itemPrefab;
        
        [Tooltip("Color tint for the item")]
        public Color itemColor = Color.white;

        [Header("Properties")]
        [Tooltip("Category classification")]
        public ResourceCategory category;
        
        [Tooltip("Maximum stack size")]
        [Range(1, 10000)]
        public int stackSize = 100;
        
        [Tooltip("Weight for logistics calculations")]
        [Range(0.1f, 100f)]
        public float weight = 1f;
        
        [Tooltip("Base economic value")]
        public int baseValue = 10;

        [Header("Behavior")]
        [Tooltip("Can this resource be stored in containers?")]
        public bool canBeStored = true;
        
        [Tooltip("Can this resource be transported on conveyors?")]
        public bool canBeTransported = true;

        // Validation in editor
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                Debug.LogWarning($"ResourceData {name} has empty resourceId!");
            }
            
            if (stackSize <= 0)
            {
                Debug.LogWarning($"ResourceData {name} has invalid stackSize!");
                stackSize = 1;
            }
        }
    }

    public enum ResourceCategory
    {
        RawMaterial,    // Wood, Iron Ore, Coal
        Refined,        // Planks, Iron Ingots, Steel
        Component,      // Gears, Circuits, Paint
        ToyPart,        // Wheels, Arms, Bodies
        FinishedToy,    // Complete toys
        Magic,          // Magic Dust, Enchantments
        Energy          // Coal, Magic Crystals (for power)
    }
}
```

### ResourceStack Struct

Lightweight data structure for resource quantities:

```csharp
using System;

namespace SantasWorkshop.Data
{
    [Serializable]
    public struct ResourceStack
    {
        public string resourceId;
        public int amount;

        public ResourceStack(string id, int amt)
        {
            resourceId = id;
            amount = amt;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(resourceId) && amount > 0;
        }

        public override string ToString()
        {
            return $"{resourceId} x{amount}";
        }
    }
}
```

### ResourceManager Singleton

Core manager for all resource operations:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SantasWorkshop.Core
{
    public class ResourceManager : MonoBehaviour
    {
        // Singleton instance
        public static ResourceManager Instance { get; private set; }

        // Events
        public event Action<string, long> OnResourceChanged;
        public event Action OnResourceSystemInitialized;

        // Resource database (resourceId -> ResourceData)
        private Dictionary<string, ResourceData> _resourceDatabase;
        
        // Global resource counts (resourceId -> count)
        private Dictionary<string, long> _globalResourceCounts;
        
        // Resource capacity limits (resourceId -> capacity)
        private Dictionary<string, long> _resourceCapacities;

        // Initialization flag
        private bool _isInitialized = false;

        #region Lifecycle

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize collections
            _resourceDatabase = new Dictionary<string, ResourceData>();
            _globalResourceCounts = new Dictionary<string, long>();
            _resourceCapacities = new Dictionary<string, long>();
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Initialization

        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("ResourceManager already initialized!");
                return;
            }

            // Load all ResourceData assets from Resources folder
            ResourceData[] resources = Resources.LoadAll<ResourceData>("ResourceDefinitions");
            
            Debug.Log($"Loading {resources.Length} resource definitions...");

            foreach (var resource in resources)
            {
                if (string.IsNullOrWhiteSpace(resource.resourceId))
                {
                    Debug.LogError($"ResourceData {resource.name} has empty resourceId!");
                    continue;
                }

                if (_resourceDatabase.ContainsKey(resource.resourceId))
                {
                    Debug.LogError($"Duplicate resourceId: {resource.resourceId}. Using first occurrence.");
                    continue;
                }

                _resourceDatabase[resource.resourceId] = resource;
                _globalResourceCounts[resource.resourceId] = 0;
            }

            _isInitialized = true;
            Debug.Log($"ResourceManager initialized with {_resourceDatabase.Count} resources.");
            
            OnResourceSystemInitialized?.Invoke();
        }

        public void ResetToDefaults()
        {
            _resourceDatabase.Clear();
            _globalResourceCounts.Clear();
            _resourceCapacities.Clear();
            _isInitialized = false;
            Initialize();
        }

        #endregion

        #region Add Resources

        public bool AddResource(string resourceId, int amount)
        {
            if (!ValidateResourceOperation(resourceId, amount))
            {
                return false;
            }

            long capacity = GetResourceCapacity(resourceId);
            long currentCount = GetResourceCount(resourceId);
            long newCount = currentCount + amount;

            // Check capacity limit
            if (capacity > 0 && newCount > capacity)
            {
                long actualAdded = capacity - currentCount;
                if (actualAdded <= 0)
                {
                    Debug.LogWarning($"Cannot add {resourceId}: at capacity ({capacity})");
                    return false;
                }
                
                _globalResourceCounts[resourceId] = capacity;
                OnResourceChanged?.Invoke(resourceId, capacity);
                Debug.LogWarning($"Added only {actualAdded} {resourceId} (capacity limit reached)");
                return true;
            }

            _globalResourceCounts[resourceId] = newCount;
            OnResourceChanged?.Invoke(resourceId, newCount);
            return true;
        }

        public void AddResources(ResourceStack[] resources)
        {
            if (resources == null || resources.Length == 0)
            {
                Debug.LogWarning("AddResources called with null or empty array!");
                return;
            }

            foreach (var stack in resources)
            {
                AddResource(stack.resourceId, stack.amount);
            }
        }

        #endregion

        #region Consume Resources

        public bool TryConsumeResource(string resourceId, int amount)
        {
            if (!ValidateResourceOperation(resourceId, amount))
            {
                return false;
            }

            long currentCount = GetResourceCount(resourceId);
            
            if (currentCount < amount)
            {
                return false;
            }

            long newCount = currentCount - amount;
            _globalResourceCounts[resourceId] = newCount;
            OnResourceChanged?.Invoke(resourceId, newCount);
            return true;
        }

        public bool TryConsumeResources(ResourceStack[] resources)
        {
            if (resources == null || resources.Length == 0)
            {
                Debug.LogWarning("TryConsumeResources called with null or empty array!");
                return false;
            }

            // Validate all resources are available first (atomic operation)
            foreach (var stack in resources)
            {
                if (!HasResource(stack.resourceId, stack.amount))
                {
                    return false;
                }
            }

            // All resources available, consume them
            foreach (var stack in resources)
            {
                TryConsumeResource(stack.resourceId, stack.amount);
            }

            return true;
        }

        #endregion

        #region Query Resources

        public bool HasResource(string resourceId, int amount)
        {
            if (string.IsNullOrEmpty(resourceId) || amount < 0)
            {
                return false;
            }

            if (!_resourceDatabase.ContainsKey(resourceId))
            {
                return false;
            }

            return GetResourceCount(resourceId) >= amount;
        }

        public long GetResourceCount(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                return 0;
            }

            return _globalResourceCounts.TryGetValue(resourceId, out long count) ? count : 0;
        }

        public ResourceData GetResourceData(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                return null;
            }

            return _resourceDatabase.TryGetValue(resourceId, out ResourceData data) ? data : null;
        }

        public IEnumerable<ResourceData> GetAllResources()
        {
            return _resourceDatabase.Values;
        }

        public IEnumerable<ResourceData> GetResourcesByCategory(ResourceCategory category)
        {
            return _resourceDatabase.Values.Where(r => r.category == category);
        }

        #endregion

        #region Transfer Resources

        public bool TransferResource(string sourceId, string targetId, string resourceId, int amount)
        {
            // For now, transfer operates on global inventory
            // Future: Support named storage locations
            
            if (!HasResource(resourceId, amount))
            {
                return false;
            }

            if (!TryConsumeResource(resourceId, amount))
            {
                return false;
            }

            AddResource(resourceId, amount);
            return true;
        }

        #endregion

        #region Capacity Management

        public void SetResourceCapacity(string resourceId, long capacity)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                Debug.LogWarning("SetResourceCapacity called with empty resourceId!");
                return;
            }

            if (!_resourceDatabase.ContainsKey(resourceId))
            {
                Debug.LogWarning($"SetResourceCapacity: Unknown resourceId {resourceId}");
                return;
            }

            _resourceCapacities[resourceId] = capacity;
        }

        public long GetResourceCapacity(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                return 0;
            }

            return _resourceCapacities.TryGetValue(resourceId, out long capacity) ? capacity : 0;
        }

        #endregion

        #region Validation

        public bool ValidateResourceStack(ResourceStack stack)
        {
            if (string.IsNullOrEmpty(stack.resourceId))
            {
                return false;
            }

            if (stack.amount <= 0)
            {
                return false;
            }

            if (!_resourceDatabase.ContainsKey(stack.resourceId))
            {
                return false;
            }

            return true;
        }

        public bool ValidateResourceStacks(ResourceStack[] stacks)
        {
            if (stacks == null || stacks.Length == 0)
            {
                return false;
            }

            foreach (var stack in stacks)
            {
                if (!ValidateResourceStack(stack))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateResourceOperation(string resourceId, int amount)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                Debug.LogWarning("Resource operation with empty resourceId!");
                return false;
            }

            if (amount < 0)
            {
                Debug.LogWarning($"Resource operation with negative amount: {amount}");
                return false;
            }

            if (!_resourceDatabase.ContainsKey(resourceId))
            {
                Debug.LogWarning($"Unknown resourceId: {resourceId}");
                return false;
            }

            return true;
        }

        #endregion

        #region Reset

        public void ResetResources()
        {
            List<string> resourcesToReset = _globalResourceCounts
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var resourceId in resourcesToReset)
            {
                _globalResourceCounts[resourceId] = 0;
                OnResourceChanged?.Invoke(resourceId, 0);
            }

            Debug.Log("All resources reset to zero.");
        }

        #endregion

        #region Save/Load

        [Serializable]
        public struct ResourceSaveData
        {
            public ResourceEntry[] resources;
        }

        [Serializable]
        public struct ResourceEntry
        {
            public string resourceId;
            public long amount;
        }

        public ResourceSaveData GetSaveData()
        {
            List<ResourceEntry> entries = new List<ResourceEntry>();

            foreach (var kvp in _globalResourceCounts)
            {
                if (kvp.Value > 0)
                {
                    entries.Add(new ResourceEntry
                    {
                        resourceId = kvp.Key,
                        amount = kvp.Value
                    });
                }
            }

            return new ResourceSaveData
            {
                resources = entries.ToArray()
            };
        }

        public void LoadSaveData(ResourceSaveData saveData)
        {
            if (saveData.resources == null)
            {
                Debug.LogWarning("LoadSaveData called with null resources!");
                return;
            }

            // Reset all counts first
            foreach (var resourceId in _globalResourceCounts.Keys.ToList())
            {
                _globalResourceCounts[resourceId] = 0;
            }

            // Load saved counts
            foreach (var entry in saveData.resources)
            {
                if (_resourceDatabase.ContainsKey(entry.resourceId))
                {
                    _globalResourceCounts[entry.resourceId] = entry.amount;
                    OnResourceChanged?.Invoke(entry.resourceId, entry.amount);
                }
                else
                {
                    Debug.LogWarning($"LoadSaveData: Unknown resourceId {entry.resourceId}");
                }
            }

            Debug.Log($"Loaded {saveData.resources.Length} resource counts from save data.");
        }

        #endregion
    }
}
```

## Data Models

### Resource Categories

Resources are organized into seven categories:

1. **RawMaterial**: Extracted directly from resource nodes
   - Examples: Wood, Iron Ore, Coal, Stone
   - Properties: Low value, high volume, basic processing

2. **Refined**: Processed from raw materials
   - Examples: Planks, Iron Ingots, Steel, Plastic
   - Properties: Medium value, intermediate processing

3. **Component**: Crafted parts used in assembly
   - Examples: Gears, Circuits, Paint, Fabric
   - Properties: Higher value, specialized use

4. **ToyPart**: Specialized components for toys
   - Examples: Wheels, Arms, Bodies, Heads
   - Properties: Toy-specific, quality-graded

5. **FinishedToy**: Complete products ready for delivery
   - Examples: Wooden Train, Teddy Bear, Robot
   - Properties: High value, quality-graded, deliverable

6. **Magic**: Magical resources and enchantments
   - Examples: Magic Dust, Enchanted Crystals
   - Properties: Special effects, rare, powerful

7. **Energy**: Resources used for power generation
   - Examples: Coal, Magic Crystals
   - Properties: Consumed for electricity, efficiency-rated

### Resource Flow Example

```
Raw Material Flow:
[Iron Ore Node] → [Mining Drill] → AddResource("iron_ore", 1) → ResourceManager
                                                                        ↓
                                                              [Global Inventory]

Processing Flow:
[Smelter] → TryConsumeResource("iron_ore", 1) → ResourceManager → Success
                                                        ↓
                                              AddResource("iron_ingot", 1)
                                                        ↓
                                              [Global Inventory]

Assembly Flow:
[Assembler] → TryConsumeResources([
                  {resourceId: "iron_ingot", amount: 2},
                  {resourceId: "wood_plank", amount: 1}
              ]) → ResourceManager → Success
                                          ↓
                                AddResource("iron_gear", 1)
                                          ↓
                                [Global Inventory]
```

## Error Handling

### Validation Errors

The system validates all operations and provides clear error messages:

```csharp
// Invalid resource ID
AddResource("invalid_id", 10);
// Output: "Unknown resourceId: invalid_id"
// Returns: false

// Negative amount
AddResource("wood", -5);
// Output: "Resource operation with negative amount: -5"
// Returns: false

// Insufficient resources
TryConsumeResource("iron_ore", 100);
// When only 50 available
// Returns: false (no error message, expected behavior)

// Capacity exceeded
SetResourceCapacity("wood", 1000);
AddResource("wood", 1500);
// Output: "Added only 1000 wood (capacity limit reached)"
// Returns: true (partial success)
```

### Atomic Operations

Multi-resource operations are atomic - either all succeed or none:

```csharp
// Attempt to consume multiple resources
bool success = TryConsumeResources(new ResourceStack[]
{
    new ResourceStack("iron_ingot", 2),
    new ResourceStack("wood_plank", 1),
    new ResourceStack("paint_red", 1)
});

// If ANY resource is insufficient, NONE are consumed
// This prevents partial recipe execution
```

## Testing Strategy

### Unit Tests

Core functionality will be tested with Unity Test Framework:

```csharp
using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Core;
using SantasWorkshop.Data;

public class ResourceManagerTests
{
    private ResourceManager _resourceManager;
    private ResourceData _testResource;

    [SetUp]
    public void Setup()
    {
        // Create ResourceManager
        GameObject go = new GameObject("ResourceManager");
        _resourceManager = go.AddComponent<ResourceManager>();

        // Create test resource
        _testResource = ScriptableObject.CreateInstance<ResourceData>();
        _testResource.resourceId = "test_wood";
        _testResource.displayName = "Test Wood";
        _testResource.category = ResourceCategory.RawMaterial;
        _testResource.stackSize = 100;
    }

    [Test]
    public void AddResource_IncreasesCount()
    {
        // Arrange
        _resourceManager.Initialize();
        
        // Act
        bool result = _resourceManager.AddResource("test_wood", 50);
        
        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(50, _resourceManager.GetResourceCount("test_wood"));
    }

    [Test]
    public void TryConsumeResource_WithSufficientResources_ReturnsTrue()
    {
        // Arrange
        _resourceManager.Initialize();
        _resourceManager.AddResource("test_wood", 100);
        
        // Act
        bool result = _resourceManager.TryConsumeResource("test_wood", 50);
        
        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(50, _resourceManager.GetResourceCount("test_wood"));
    }

    [Test]
    public void TryConsumeResource_WithInsufficientResources_ReturnsFalse()
    {
        // Arrange
        _resourceManager.Initialize();
        _resourceManager.AddResource("test_wood", 30);
        
        // Act
        bool result = _resourceManager.TryConsumeResource("test_wood", 50);
        
        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(30, _resourceManager.GetResourceCount("test_wood"));
    }

    [Test]
    public void TryConsumeResources_Atomic_AllOrNothing()
    {
        // Arrange
        _resourceManager.Initialize();
        _resourceManager.AddResource("test_wood", 50);
        _resourceManager.AddResource("test_iron", 10);
        
        ResourceStack[] resources = new ResourceStack[]
        {
            new ResourceStack("test_wood", 30),
            new ResourceStack("test_iron", 20) // Insufficient
        };
        
        // Act
        bool result = _resourceManager.TryConsumeResources(resources);
        
        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(50, _resourceManager.GetResourceCount("test_wood")); // Unchanged
        Assert.AreEqual(10, _resourceManager.GetResourceCount("test_iron")); // Unchanged
    }

    [Test]
    public void SetResourceCapacity_LimitsAdditions()
    {
        // Arrange
        _resourceManager.Initialize();
        _resourceManager.SetResourceCapacity("test_wood", 100);
        
        // Act
        _resourceManager.AddResource("test_wood", 150);
        
        // Assert
        Assert.AreEqual(100, _resourceManager.GetResourceCount("test_wood"));
    }

    [Test]
    public void OnResourceChanged_FiresWhenResourceAdded()
    {
        // Arrange
        _resourceManager.Initialize();
        string firedResourceId = null;
        long firedAmount = 0;
        
        _resourceManager.OnResourceChanged += (id, amount) =>
        {
            firedResourceId = id;
            firedAmount = amount;
        };
        
        // Act
        _resourceManager.AddResource("test_wood", 50);
        
        // Assert
        Assert.AreEqual("test_wood", firedResourceId);
        Assert.AreEqual(50, firedAmount);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_resourceManager.gameObject);
        Object.DestroyImmediate(_testResource);
    }
}
```

### Integration Tests

Test resource flow through multiple systems:

- **Test_ResourceExtraction**: Mining drill extracts ore and adds to inventory
- **Test_ResourceProcessing**: Smelter consumes ore and produces ingots
- **Test_ResourceAssembly**: Assembler consumes multiple inputs and produces output
- **Test_ResourceUI**: UI updates when resources change
- **Test_SaveLoad**: Resources persist correctly through save/load

### Performance Tests

Validate system performance under load:

```csharp
[Test]
public void Performance_1000AddOperations_CompletesQuickly()
{
    _resourceManager.Initialize();
    
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    for (int i = 0; i < 1000; i++)
    {
        _resourceManager.AddResource("test_wood", 1);
    }
    
    stopwatch.Stop();
    
    Assert.Less(stopwatch.ElapsedMilliseconds, 100); // Should complete in <100ms
    Assert.AreEqual(1000, _resourceManager.GetResourceCount("test_wood"));
}
```

## Performance Considerations

### Dictionary-Based Storage

Using `Dictionary<string, long>` for resource counts provides:
- O(1) lookup time
- O(1) insertion time
- O(1) update time
- Minimal memory overhead

### Event Optimization

Events are invoked only when values change:
- No events fired for failed operations
- Single event per resource change
- Subscribers can batch updates

### Memory Efficiency

- ResourceData assets are loaded once and shared
- ResourceStack is a lightweight struct (16 bytes)
- Global counts use `long` (8 bytes) to support large quantities
- No garbage allocation during normal operations

### Scalability

The system is designed to handle:
- 100+ resource types
- 1000+ transactions per second
- Millions of units in inventory
- 100+ event subscribers

## Future Enhancements

### Phase 2 Additions

1. **Named Storage Locations**
   - Support for multiple storage containers
   - Transfer between specific locations
   - Per-container capacity limits

2. **Resource Reservations**
   - Reserve resources for pending operations
   - Prevent over-commitment
   - Timeout and cleanup

3. **Resource Quality**
   - Quality grades (S/A/B/C)
   - Quality-aware consumption
   - Quality tracking in inventory

4. **Resource Metadata**
   - Custom properties per resource instance
   - Enchantments and modifiers
   - Tracking and serialization

### Performance Optimizations

1. **Burst Compilation**
   - Convert hot paths to Burst-compiled jobs
   - Parallel resource updates
   - SIMD optimizations

2. **Spatial Partitioning**
   - Chunk-based resource tracking
   - Localized updates
   - Reduced global operations

3. **Event Batching**
   - Batch multiple changes into single event
   - Reduce UI update frequency
   - Configurable update rate

## Conclusion

The Resource System provides a robust, performant foundation for managing all resources in Santa's Workshop Automation. Key design decisions include:

- **Centralized Management**: Single ResourceManager singleton ensures consistency
- **Event-Driven Updates**: Decoupled architecture enables reactive UI and systems
- **Atomic Operations**: Multi-resource transactions are all-or-nothing
- **Designer-Friendly**: ScriptableObjects enable content creation without code
- **Performance-Focused**: Dictionary-based storage and minimal allocations
- **Extensible**: Clear interfaces for future enhancements

The system is ready for implementation and integration with machine systems, logistics, and UI.
