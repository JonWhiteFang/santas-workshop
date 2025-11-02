# Design Document: Project Setup & Architecture

## Overview

This design document outlines the technical implementation for establishing Santa's Workshop Automation as a Unity 2022+ LTS project with Universal Render Pipeline. The design focuses on creating a scalable, maintainable foundation that supports the game's three-layer architecture (Render, Simulation, UI) and accommodates a 14-18 month development timeline with multiple developers.

The project will follow industry-standard Unity conventions while incorporating modern C# patterns and performance optimizations through Burst, Jobs, and ECS where appropriate.

## Architecture

### Three-Layer Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      RENDER LAYER                           │
│  - Visual representation (meshes, materials, particles)     │
│  - URP rendering pipeline                                   │
│  - Camera systems (Cinemachine)                             │
│  - Animation and VFX                                        │
│  - Lighting and post-processing                             │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                    SIMULATION LAYER                         │
│  - Game state and logic                                     │
│  - Machine systems (extractors, processors, assemblers)     │
│  - Resource management                                      │
│  - Power grid simulation                                    │
│  - Logistics (conveyors, routing)                          │
│  - Research and progression                                 │
│  - Mission system                                           │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                        UI LAYER                             │
│  - UI Toolkit interfaces                                    │
│  - Async reads from simulation snapshots                    │
│  - Input handling (new Input System)                        │
│  - HUD and overlays                                         │
│  - Menus and dialogs                                        │
└─────────────────────────────────────────────────────────────┘
```

**Design Rationale**: Separating concerns into three distinct layers allows:
- Independent development and testing of each layer
- Performance optimization (UI reads cached state, doesn't block simulation)
- Clear ownership boundaries for team members
- Easier debugging and profiling

### Core Singleton Managers

The project uses a minimal set of singleton managers for cross-cutting concerns:

```csharp
// Core managers (singletons)
- GameManager: Game state, scene management, lifecycle
- ResourceManager: Resource tracking, extraction, consumption
- PowerGridManager: Electricity generation and distribution
- ResearchManager: Tech tree progression
- MissionManager: Campaign and objectives
- SaveLoadManager: Persistence system
```

**Design Rationale**: Singletons are used sparingly and only for truly global systems. Most game logic uses dependency injection or component-based patterns to maintain testability.

## Components and Interfaces

### Machine Framework

All machines in the game inherit from a common base class hierarchy:

```csharp
namespace SantasWorkshop.Machines
{
    // Core interface
    public interface IMachine
    {
        string MachineId { get; }
        MachineState State { get; }
        void Initialize(MachineData data);
        void Tick(float deltaTime);
        void Shutdown();
    }

    // Base abstract class
    public abstract class MachineBase : MonoBehaviour, IMachine
    {
        [SerializeField] protected MachineData machineData;
        
        protected MachineState currentState;
        protected float powerConsumption;
        protected bool isPowered;
        
        public string MachineId => machineData.id;
        public MachineState State => currentState;
        
        public virtual void Initialize(MachineData data)
        {
            machineData = data;
            currentState = MachineState.Idle;
        }
        
        public abstract void Tick(float deltaTime);
        
        public virtual void Shutdown()
        {
            currentState = MachineState.Offline;
        }
        
        protected virtual void OnPowerChanged(bool powered)
        {
            isPowered = powered;
        }
    }

    // Specialized base classes
    public abstract class ExtractorBase : MachineBase
    {
        protected ResourceNode targetNode;
        protected float extractionRate;
        
        public override void Tick(float deltaTime)
        {
            if (!isPowered || targetNode == null) return;
            // Extraction logic
        }
    }

    public abstract class ProcessorBase : MachineBase
    {
        protected Recipe currentRecipe;
        protected Queue<ResourceStack> inputBuffer;
        protected Queue<ResourceStack> outputBuffer;
        
        public override void Tick(float deltaTime)
        {
            if (!isPowered || currentRecipe == null) return;
            // Processing logic
        }
    }

    public abstract class AssemblerBase : MachineBase
    {
        protected Recipe currentRecipe;
        protected Dictionary<string, int> inputInventory;
        protected float assemblyProgress;
        
        public override void Tick(float deltaTime)
        {
            if (!isPowered || currentRecipe == null) return;
            // Assembly logic
        }
    }
}
```

**Design Rationale**: 
- Interface defines contract for all machines
- Abstract base class provides common functionality (power, state, lifecycle)
- Specialized base classes (Extractor, Processor, Assembler) add category-specific behavior
- Concrete implementations (MiningDrill, Smelter, etc.) focus on unique logic

### ScriptableObject Data Architecture

All game data uses ScriptableObjects for designer-friendly configuration:

```csharp
namespace SantasWorkshop.Data
{
    // Resource definition
    [CreateAssetMenu(fileName = "New Resource", menuName = "Santa/Resource")]
    public class ResourceData : ScriptableObject
    {
        public string resourceId;
        public string displayName;
        public Sprite icon;
        public ResourceCategory category;
        public int stackSize = 100;
    }

    // Recipe definition
    [CreateAssetMenu(fileName = "New Recipe", menuName = "Santa/Recipe")]
    public class RecipeData : ScriptableObject
    {
        public string recipeId;
        public string displayName;
        public ResourceStack[] inputs;
        public ResourceStack[] outputs;
        public float processingTime;
        public float powerRequired;
    }

    // Machine configuration
    [CreateAssetMenu(fileName = "New Machine", menuName = "Santa/Machine")]
    public class MachineData : ScriptableObject
    {
        public string machineId;
        public string displayName;
        public GameObject prefab;
        public MachineCategory category;
        public float powerConsumption;
        public ResourceStack[] buildCost;
        public string[] requiredResearch;
    }

    // Research node
    [CreateAssetMenu(fileName = "New Research", menuName = "Santa/Research")]
    public class ResearchData : ScriptableObject
    {
        public string researchId;
        public string displayName;
        public string description;
        public ResearchBranch branch;
        public int researchPointCost;
        public string[] prerequisites;
        public string[] unlocks; // Machine IDs, recipe IDs, etc.
    }
}
```

**Design Rationale**:
- Designers can create and modify game content without code changes
- Data is shared across instances (memory efficient)
- Easy to balance and iterate on game content
- Version control friendly (text-based assets)

### Resource Management System

```csharp
namespace SantasWorkshop.Core
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }
        
        private Dictionary<string, ResourceData> resourceDatabase;
        private Dictionary<string, long> globalResourceCounts;
        
        public void RegisterResource(ResourceData resource)
        {
            resourceDatabase[resource.resourceId] = resource;
        }
        
        public bool TryConsumeResources(ResourceStack[] resources)
        {
            // Check availability
            foreach (var stack in resources)
            {
                if (!HasResource(stack.resourceId, stack.amount))
                    return false;
            }
            
            // Consume
            foreach (var stack in resources)
            {
                globalResourceCounts[stack.resourceId] -= stack.amount;
            }
            
            return true;
        }
        
        public void AddResources(ResourceStack[] resources)
        {
            foreach (var stack in resources)
            {
                if (!globalResourceCounts.ContainsKey(stack.resourceId))
                    globalResourceCounts[stack.resourceId] = 0;
                    
                globalResourceCounts[stack.resourceId] += stack.amount;
            }
        }
        
        public bool HasResource(string resourceId, int amount)
        {
            return globalResourceCounts.TryGetValue(resourceId, out long count) 
                && count >= amount;
        }
    }
}
```

## Data Models

### Core Data Structures

```csharp
namespace SantasWorkshop.Data
{
    // Resource stack (amount of a resource)
    [System.Serializable]
    public struct ResourceStack
    {
        public string resourceId;
        public int amount;
        
        public ResourceStack(string id, int amt)
        {
            resourceId = id;
            amount = amt;
        }
    }

    // Machine state enum
    public enum MachineState
    {
        Offline,      // Not powered or disabled
        Idle,         // Powered but not working
        Working,      // Actively processing
        Blocked,      // Output full or input empty
        Error         // Configuration or runtime error
    }

    // Resource categories
    public enum ResourceCategory
    {
        RawMaterial,  // Wood, stone, iron ore
        Refined,      // Planks, iron ingots, plastic
        Component,    // Gears, circuits, fabric
        Toy,          // Final products
        Magic         // Magical resources
    }

    // Machine categories
    public enum MachineCategory
    {
        Extractor,    // Mining drills, harvesters
        Processor,    // Smelters, sawmills, refineries
        Assembler,    // Assembly machines
        Logistics,    // Conveyors, splitters, storage
        Power,        // Generators
        Utility       // Research labs, inspection stations
    }

    // Research branches
    public enum ResearchBranch
    {
        Automation,
        Energy,
        Materials,
        Toys,
        Logistics,
        ElfManagement,
        Magic,
        Aesthetics
    }
}
```

### Save Data Schema

```csharp
namespace SantasWorkshop.Data
{
    [System.Serializable]
    public class SaveData
    {
        public int version = 1;
        public string saveName;
        public long saveTimestamp;
        
        // Game state
        public int currentYear;
        public int currentMonth;
        public float gameTime;
        
        // Resources
        public Dictionary<string, long> resourceCounts;
        
        // Machines
        public List<MachineInstanceData> machines;
        
        // Research
        public List<string> unlockedResearch;
        
        // Missions
        public List<MissionProgressData> missionProgress;
        
        // Prestige
        public int prestigeLevel;
        public List<string> prestigeUnlocks;
    }

    [System.Serializable]
    public class MachineInstanceData
    {
        public string machineId;
        public Vector3 position;
        public Quaternion rotation;
        public string currentRecipeId;
        public MachineState state;
        public Dictionary<string, object> customData;
    }
}
```

## Error Handling

### Machine Error States

Machines can enter error states for various reasons:

```csharp
public enum MachineError
{
    None,
    NoPower,
    NoInput,
    OutputFull,
    InvalidRecipe,
    MissingComponent
}

public class MachineBase : MonoBehaviour
{
    protected MachineError currentError = MachineError.None;
    
    protected void SetError(MachineError error)
    {
        if (currentError != error)
        {
            currentError = error;
            OnErrorChanged?.Invoke(error);
            
            if (error != MachineError.None)
            {
                currentState = MachineState.Error;
                Debug.LogWarning($"Machine {MachineId} error: {error}");
            }
        }
    }
    
    protected void ClearError()
    {
        SetError(MachineError.None);
    }
}
```

### Save/Load Error Handling

```csharp
public class SaveLoadManager : MonoBehaviour
{
    public async Task<SaveResult> SaveGame(string saveName)
    {
        try
        {
            SaveData data = GatherSaveData();
            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath(saveName);
            
            await File.WriteAllTextAsync(path, json);
            
            return new SaveResult { Success = true };
        }
        catch (Exception ex)
        {
            Debug.LogError($"Save failed: {ex.Message}");
            return new SaveResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message 
            };
        }
    }
    
    public async Task<LoadResult> LoadGame(string saveName)
    {
        try
        {
            string path = GetSavePath(saveName);
            
            if (!File.Exists(path))
            {
                return new LoadResult 
                { 
                    Success = false, 
                    ErrorMessage = "Save file not found" 
                };
            }
            
            string json = await File.ReadAllTextAsync(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            // Version check
            if (data.version > CURRENT_SAVE_VERSION)
            {
                return new LoadResult 
                { 
                    Success = false, 
                    ErrorMessage = "Save file from newer version" 
                };
            }
            
            ApplySaveData(data);
            
            return new LoadResult { Success = true, Data = data };
        }
        catch (Exception ex)
        {
            Debug.LogError($"Load failed: {ex.Message}");
            return new LoadResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message 
            };
        }
    }
}
```

## Testing Strategy

### Unit Testing

Core systems will have unit tests using Unity Test Framework:

```csharp
using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Core;

public class ResourceManagerTests
{
    private ResourceManager resourceManager;
    
    [SetUp]
    public void Setup()
    {
        GameObject go = new GameObject();
        resourceManager = go.AddComponent<ResourceManager>();
    }
    
    [Test]
    public void AddResources_IncreasesCount()
    {
        // Arrange
        var resources = new ResourceStack[] 
        { 
            new ResourceStack("wood", 100) 
        };
        
        // Act
        resourceManager.AddResources(resources);
        
        // Assert
        Assert.IsTrue(resourceManager.HasResource("wood", 100));
    }
    
    [Test]
    public void TryConsumeResources_WithSufficientResources_ReturnsTrue()
    {
        // Arrange
        resourceManager.AddResources(new ResourceStack[] 
        { 
            new ResourceStack("wood", 100) 
        });
        
        // Act
        bool result = resourceManager.TryConsumeResources(new ResourceStack[] 
        { 
            new ResourceStack("wood", 50) 
        });
        
        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(resourceManager.HasResource("wood", 50));
    }
    
    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(resourceManager.gameObject);
    }
}
```

### Integration Testing

Test scenes will verify system interactions:

- **TestScene_ResourceFlow**: Test resource extraction → processing → assembly chain
- **TestScene_PowerGrid**: Test power generation and distribution
- **TestScene_SaveLoad**: Test save/load functionality with various game states

### Manual Testing Checklist

After project setup, verify:
- [ ] Project opens without errors in Unity 2022+ LTS
- [ ] URP is active and rendering correctly
- [ ] All required packages are installed
- [ ] Folder structure matches design
- [ ] MainMenu and Workshop scenes load
- [ ] Git repository is initialized with correct .gitignore
- [ ] Git LFS is configured for binary files
- [ ] README.md is complete and accurate
- [ ] Build settings are configured for Windows
- [ ] Development build can be created successfully

## Project Structure Implementation

### Directory Creation Order

1. Create root Assets/_Project directory
2. Create Scripts subdirectories (Core, Machines, Logistics, etc.)
3. Create asset directories (Prefabs, Scenes, ScriptableObjects, etc.)
4. Create Art directories (Models, Textures, Animations, VFX)
5. Create Audio directories (Music, SFX, VO)
6. Create UI directories (UXML, USS, Assets)

### Initial Files to Create

**Core Scripts:**
- `GameManager.cs` - Game state and lifecycle
- `ResourceManager.cs` - Resource tracking
- `MachineBase.cs` - Base machine class
- `IMachine.cs` - Machine interface

**Data Scripts:**
- `ResourceData.cs` - Resource ScriptableObject
- `RecipeData.cs` - Recipe ScriptableObject
- `MachineData.cs` - Machine ScriptableObject
- `ResearchData.cs` - Research ScriptableObject

**Utility Scripts:**
- `Singleton.cs` - Generic singleton pattern
- `Extensions.cs` - C# extension methods

**Scenes:**
- `MainMenu.unity` - Entry point scene
- `Workshop.unity` - Main gameplay scene
- `TestScenes/TestScene_Empty.unity` - Template test scene

**Documentation:**
- `README.md` - Project overview and setup
- `CONTRIBUTING.md` - Development guidelines
- `.gitignore` - Git exclusions
- `.gitattributes` - Git LFS configuration

## Performance Considerations

### Initial Setup

The initial project setup focuses on establishing patterns that support future optimization:

1. **Namespace Organization**: Clear namespaces (SantasWorkshop.*) enable easy profiling
2. **Component Caching**: Base classes cache Transform and other components in Awake()
3. **Object Pooling Ready**: Machine framework designed for pooling (Initialize/Shutdown pattern)
4. **Async-Ready**: Managers use async/await for I/O operations (save/load)

### Future Optimization Hooks

The architecture includes hooks for future performance work:

- **Burst/Jobs**: Machine.Tick() can be converted to IJob for parallel processing
- **ECS**: Machine data structures designed to migrate to ECS if needed
- **Spatial Partitioning**: ResourceManager can add spatial indexing for large factories
- **GPU Instancing**: Prefab structure supports instanced rendering

## Conclusion

This design establishes a solid foundation for Santa's Workshop Automation. The three-layer architecture, machine framework, and ScriptableObject data system provide flexibility for the 14-18 month development timeline while maintaining performance and maintainability.

Key design decisions:
- **Three-layer architecture** separates concerns and enables parallel development
- **Machine framework** provides consistent patterns for all factory buildings
- **ScriptableObjects** enable designer-friendly content creation
- **Minimal singletons** maintain testability while providing global access where needed
- **Async/await** for I/O operations improves responsiveness
- **Clear namespaces** organize code and enable easy profiling

The project is now ready for implementation of the task list.
