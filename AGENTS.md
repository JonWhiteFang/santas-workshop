# AGENTS.md - Santa's Workshop Automation

**Last Updated**: November 5, 2025  
**Project**: Santa's Workshop Automation  
**Engine**: Unity 6.0 (6000.2.10f1)  
**Language**: C# 9.0+ (.NET Standard 2.1)

This file provides AI coding agents with the context, conventions, and instructions needed to work effectively on this Unity-based factory automation game project.

---

## Project Overview

Santa's Workshop Automation is a factory automation/management simulation game built in Unity 6. Players build and optimize Santa's automated workshop to produce high-quality toys before Christmas. The game features resource management, machine automation, power grids, logistics systems, and a branching tech tree.

**Key Systems**:
- Resource Management (✅ Complete - 28/28 tests passing)
- Grid & Placement System (✅ Complete)
- Machine Framework (🔄 In Progress - 29% complete)
- Power Grid System (⏳ Pending)
- Logistics System (⏳ Pending)
- Research & Tech Tree (⏳ Pending)
- Mission System (⏳ Pending)

---

## Quick Start

### Opening the Project

```powershell
# Unity 6.0 (6000.2.10f1) required
# Open Unity Hub → Open Project → Select SantasWorkshopAutomation folder
```

### Running Tests

```powershell
# In Unity Editor:
# Window → General → Test Runner → Run All

# Command-line (from project root):
"C:\Program Files\Unity\Hub\Editor\6000.2.10f1\Editor\Unity.exe" `
  -runTests -batchmode -projectPath . `
  -testResults TestResults.xml
```

### Building the Project

```powershell
# In Unity Editor:
# File → Build Settings → Select Platform → Build

# Command-line build:
"C:\Program Files\Unity\Hub\Editor\6000.2.10f1\Editor\Unity.exe" `
  -quit -batchmode -projectPath . `
  -buildWindows64Player "Builds/SantasWorkshop.exe"
```

---

## Project Structure

```
SantasWorkshopAutomation/
├── Assets/
│   ├── _Project/                    # Main project assets (underscore for top sorting)
│   │   ├── Scripts/                 # All C# scripts
│   │   │   ├── Core/                # Core systems (ResourceManager, GridManager, etc.)
│   │   │   ├── Machines/            # Machine framework and implementations
│   │   │   ├── Logistics/           # Conveyors, pipes, routing
│   │   │   ├── Research/            # Tech tree system
│   │   │   ├── Missions/            # Campaign and objectives
│   │   │   ├── UI/                  # UI controllers
│   │   │   ├── Data/                # ScriptableObjects and data structures
│   │   │   └── Utilities/           # Helper classes
│   │   ├── Prefabs/                 # Reusable game objects
│   │   ├── Scenes/                  # Unity scenes
│   │   ├── ScriptableObjects/       # Data assets (resources, recipes, machines)
│   │   ├── Materials/               # Unity materials
│   │   └── Settings/                # URP and project settings
│   ├── Art/                         # Visual assets (models, textures, animations)
│   ├── Audio/                       # Sound assets (music, SFX, VO)
│   ├── UI/                          # UI Toolkit assets (UXML, USS)
│   └── ThirdParty/                  # Third-party assets
├── .kiro/                           # Kiro AI assistant configuration
│   ├── steering/                    # Project steering documents
│   └── specs/                       # Feature specifications
└── Documentation/                   # Project documentation
```

**Key Locations**:
- Core systems: `Assets/_Project/Scripts/Core/`
- Machine implementations: `Assets/_Project/Scripts/Machines/`
- ScriptableObject schemas: `Assets/_Project/Scripts/Data/`
- Test scenes: `Assets/_Project/Scenes/TestScenes/`
- Steering docs: `.kiro/steering/`

---

## Coding Conventions

### Namespaces

All scripts use the `SantasWorkshop` namespace hierarchy:

```csharp
namespace SantasWorkshop.Core          // GameManager, ResourceManager, SaveLoadSystem
namespace SantasWorkshop.Machines      // All machine implementations
namespace SantasWorkshop.Logistics     // Conveyors, pipes, routing
namespace SantasWorkshop.Research      // Tech tree system
namespace SantasWorkshop.Missions      // Campaign and objectives
namespace SantasWorkshop.UI            // UI controllers
namespace SantasWorkshop.Data          // Data structures and ScriptableObjects
namespace SantasWorkshop.Utilities     // Helper classes and extensions
```

### Naming Conventions

```csharp
// Classes, Structs, Enums: PascalCase
public class ResourceManager { }
public struct ResourceStack { }
public enum MachineType { }

// Public fields, properties, methods: PascalCase
public int MaxHealth { get; set; }
public void ProcessRecipe() { }

// Private fields: _camelCase with underscore prefix
private int _currentHealth;
private Transform _transform;

// Parameters, local variables: camelCase
public void Initialize(int startingHealth)
{
    int calculatedValue = startingHealth * 2;
}

// Constants: UPPER_SNAKE_CASE or PascalCase
private const int MAX_MACHINES = 1000;
private const float GRID_CELL_SIZE = 1f;

// Serialized fields: camelCase (matches Inspector display)
[SerializeField] private float moveSpeed = 5f;
[SerializeField] private GameObject machinePrefab;
```

### File Organization

- **One class per file**: File name must match class name exactly
- **File location**: Place files in appropriate namespace folder
- **Regions**: Use `#region` to organize large classes
- **Comments**: XML documentation for public APIs

```csharp
/// <summary>
/// Manages all resources in the game including extraction, storage, and consumption.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    #region Singleton
    public static ResourceManager Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [SerializeField] private int defaultCapacity = 1000;
    #endregion

    #region Private Fields
    private Dictionary<ResourceType, int> _resources;
    #endregion

    #region Unity Lifecycle
    private void Awake() { }
    private void Start() { }
    #endregion

    #region Public Methods
    public bool HasResource(ResourceType type, int amount) { }
    #endregion

    #region Private Methods
    private void InitializeResources() { }
    #endregion
}
```

---

## Unity-Specific Guidelines

### MonoBehaviour Lifecycle

```csharp
// Awake: Initialize internal state, cache components
private void Awake()
{
    _rigidbody = GetComponent<Rigidbody>();
    _transform = transform;
}

// Start: Initialize logic that depends on other objects
private void Start()
{
    // Called after all Awake() calls
}

// Update: Frame-by-frame logic (input, movement)
private void Update()
{
    // Use Time.deltaTime for frame-rate independence
}

// FixedUpdate: Physics calculations
private void FixedUpdate()
{
    // Consistent timing for physics
}

// OnDisable/OnDestroy: Clean up
private void OnDisable()
{
    // Unsubscribe from events
}
```

### Component Caching

**Always cache component references in Awake()** to avoid expensive `GetComponent()` calls:

```csharp
// ❌ BAD: GetComponent every frame
private void Update()
{
    GetComponent<Rigidbody>().velocity = Vector3.zero;
}

// ✅ GOOD: Cache in Awake
private Rigidbody _rigidbody;
private void Awake()
{
    _rigidbody = GetComponent<Rigidbody>();
}
private void Update()
{
    _rigidbody.velocity = Vector3.zero;
}
```

### Serialization

```csharp
// ✅ Serialized: Visible in Inspector, saved with scene
[SerializeField] private int health = 100;
[SerializeField] private GameObject prefab;

// ✅ Public fields are automatically serialized
public float attackDamage = 25f;

// ❌ Not serialized: Private without [SerializeField]
private int score;

// ❌ Not serialized: Properties
public int Health { get; set; }

// ✅ Use attributes for better Inspector organization
[Header("Movement Settings")]
[Tooltip("Maximum movement speed in units per second")]
[Range(0f, 10f)]
[SerializeField] private float maxSpeed = 5f;
```

### ScriptableObjects

Use ScriptableObjects for data-driven design:

```csharp
[CreateAssetMenu(fileName = "NewResource", menuName = "Game/Resource")]
public class ResourceData : ScriptableObject
{
    public string resourceName;
    public Sprite icon;
    public int stackSize = 100;
}

// Reference in MonoBehaviour
public class Machine : MonoBehaviour
{
    [SerializeField] private ResourceData inputResource;
}
```

---

## Performance Best Practices

### Avoid Expensive Operations in Update

```csharp
// ❌ BAD: Expensive operations every frame
private void Update()
{
    GameObject player = GameObject.FindGameObjectWithTag("Player"); // SLOW
    Camera.main.transform.position = player.transform.position; // SLOW
}

// ✅ GOOD: Cache references
private Transform _playerTransform;
private Transform _cameraTransform;

private void Awake()
{
    _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    _cameraTransform = Camera.main.transform;
}

private void LateUpdate()
{
    _cameraTransform.position = _playerTransform.position;
}
```

### LINQ Usage

```csharp
// ❌ BAD: LINQ in Update (creates garbage)
private void Update()
{
    var activeMachines = machines.Where(m => m.IsActive).ToList();
}

// ✅ GOOD: Traditional loops for frequent operations
private void Update()
{
    for (int i = 0; i < machines.Count; i++)
    {
        if (machines[i].IsActive)
        {
            // Process machine
        }
    }
}

// ✅ OK: LINQ for initialization or infrequent operations
private void Start()
{
    var tierOneMachines = machines.Where(m => m.Tier == 1).ToList();
}
```

### Object Pooling

Use object pooling for frequently instantiated objects (items on conveyors):

```csharp
// See: Assets/_Project/Scripts/Utilities/ObjectPool.cs
public GameObject GetItem()
{
    return ItemPool.Instance.GetItem(resourceType, position);
}

public void ReturnItem(GameObject item)
{
    ItemPool.Instance.ReturnItem(item);
}
```

---

## Testing Guidelines

### Test Structure

Tests are located in `Assets/_Project/Scripts/Tests/`:

```
Tests/
├── EditMode/           # Edit mode tests (no Play mode required)
│   ├── Core/
│   │   ├── ResourceManagerTests.cs
│   │   └── GridManagerTests.cs
│   └── Machines/
└── PlayMode/           # Play mode tests (require scene)
    └── Integration/
```

### Writing Tests

```csharp
using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Core;

public class ResourceManagerTests
{
    private ResourceManager _resourceManager;

    [SetUp]
    public void SetUp()
    {
        // Create test object
        GameObject go = new GameObject("ResourceManager");
        _resourceManager = go.AddComponent<ResourceManager>();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up
        Object.DestroyImmediate(_resourceManager.gameObject);
    }

    [Test]
    public void AddResource_IncreasesResourceAmount()
    {
        // Arrange
        ResourceType type = ResourceType.IronOre;
        int amount = 10;

        // Act
        _resourceManager.AddResource(type, amount);

        // Assert
        Assert.AreEqual(10, _resourceManager.GetResourceAmount(type));
    }
}
```

### Running Tests

- **Unity Test Runner**: Window → General → Test Runner → Run All
- **Command-line**: See "Running Tests" section above
- **CI/CD**: Tests run automatically on push (see `.github/workflows/`)

---

## Common Tasks

### Adding a New Resource

1. Create ScriptableObject: `Assets/_Project/ScriptableObjects/Resources/`
2. Define in `ResourceType` enum: `Assets/_Project/Scripts/Data/ResourceType.cs`
3. Add icon and prefab references
4. Update resource database if needed

### Adding a New Machine

1. Create machine data: `Assets/_Project/ScriptableObjects/Machines/`
2. Create machine script: `Assets/_Project/Scripts/Machines/`
3. Inherit from appropriate base class (`ExtractorBase`, `ProcessorBase`, etc.)
4. Create prefab: `Assets/_Project/Prefabs/Machines/`
5. Add to machine database
6. Create recipes if needed

### Adding a New Recipe

1. Create ScriptableObject: `Assets/_Project/ScriptableObjects/Recipes/`
2. Define inputs and outputs
3. Set processing time and power consumption
4. Assign to appropriate machines

### Creating a Test Scene

1. Create scene: `Assets/_Project/Scenes/TestScenes/`
2. Add to `.gitignore` if temporary
3. Exclude from build settings (File → Build Settings → Scenes)

---

## Architecture Patterns

### Singleton Pattern

Used for managers (ResourceManager, GridManager, etc.):

```csharp
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

### Observer Pattern (Events)

Used for decoupled communication:

```csharp
// Define events
public static class GameEvents
{
    public static event Action<ResourceType, int> OnResourceChanged;
    
    public static void ResourceChanged(ResourceType type, int amount)
    {
        OnResourceChanged?.Invoke(type, amount);
    }
}

// Subscribe
private void OnEnable()
{
    GameEvents.OnResourceChanged += HandleResourceChanged;
}

private void OnDisable()
{
    GameEvents.OnResourceChanged -= HandleResourceChanged;
}
```

### State Machine Pattern

Used for machine behavior:

```csharp
public enum MachineState
{
    Idle,
    WaitingForInput,
    Processing,
    WaitingForOutput,
    NoPower
}

public abstract class MachineBase : MonoBehaviour
{
    protected MachineState _currentState;
    
    public void TransitionToState(MachineState newState)
    {
        _currentState = newState;
        // Handle state transition
    }
}
```

---

## Data Architecture

### ScriptableObject Schemas

All game data is defined in ScriptableObjects:

- **ResourceData**: Raw materials, refined goods, components, toys
- **RecipeData**: Input/output mappings for machines
- **MachineData**: Machine stats, costs, unlock requirements
- **ResearchNodeData**: Tech tree nodes with dependencies
- **MissionData**: Campaign missions and objectives
- **ToyData**: Finished toy products with quality grading

### Save File Format

Save files use JSON format with versioning:

```csharp
[System.Serializable]
public class SaveData
{
    public int version = 1;
    public string saveName;
    public DateTime saveTime;
    public ResourceSaveData resources;
    public MachineSaveData machines;
    // ... other systems
}
```

---

## Git Workflow

### Branch Strategy

- `main`: Stable, production-ready code
- `develop`: Integration branch for features
- `feature/*`: Feature branches (e.g., `feature/power-grid`)
- `bugfix/*`: Bug fix branches

### Commit Messages

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

**Examples**:
```
feat(machines): Add smelter machine implementation
fix(resources): Fix resource overflow bug
docs(readme): Update installation instructions
test(grid): Add grid placement tests
```

### Pull Request Guidelines

- Title format: `[<system>] <Title>` (e.g., `[Machines] Add smelter implementation`)
- Always run tests before committing: `Window → General → Test Runner → Run All`
- Update documentation if needed
- Link related issues: `Fixes #123`

---

## Debugging Tips

### Unity Console

```csharp
// Standard log
Debug.Log("Game started");

// Warning (yellow)
Debug.LogWarning("Low health!");

// Error (red)
Debug.LogError("Failed to load data");

// Conditional compilation (removed in builds)
#if UNITY_EDITOR
Debug.Log("Editor only");
#endif

// Rich text formatting
Debug.Log("<color=green>Success!</color>");

// Log with context (click to highlight object)
Debug.Log("Player spawned", gameObject);
```

### Unity Profiler

- Open: Window → Analysis → Profiler (Ctrl+7)
- Use Deep Profile for detailed analysis (warning: significant overhead)
- Focus on CPU, Memory, and Rendering modules

### Common Issues

**Issue**: `NullReferenceException` on component
**Solution**: Check if component is cached in `Awake()` and object exists

**Issue**: Performance drops with many machines
**Solution**: Use object pooling, spatial partitioning, and ECS for simulation

**Issue**: Save file not loading
**Solution**: Check version number and migration logic in `SaveLoadSystem`

---

## Documentation

### Steering Documents

Located in `.kiro/steering/`, these provide comprehensive guidance:

- **product.md**: Game concept, features, design pillars
- **tech.md**: Technologies, architecture, implementation status
- **structure.md**: Directory layout, file organization
- **unity-csharp-development.md**: C# best practices for Unity 6
- **game-design-patterns.md**: Architectural patterns
- **data-architecture.md**: ScriptableObject schemas, save formats

### System Documentation

Located in `Documentation/Systems/`:

- Resource system completion summary
- Grid & placement system summary
- Machine framework progress
- Integration test reports

### API Documentation

- XML documentation comments for public APIs
- IntelliSense support in IDE
- Generate docs with DocFX (future)

---

## Dependencies

### Unity Packages

- Universal RP (URP)
- UI Toolkit
- Burst Compiler
- Collections (for ECS)
- Mathematics (for Jobs)
- Input System
- Cinemachine
- TextMeshPro

### Third-Party Assets

- DOTween (animation tweening)
- Odin Inspector (enhanced editor tools) - Optional
- Addressables (asset management) - Future

---

## Build Configuration

### Development Build

```powershell
# In Unity Editor:
# File → Build Settings → Development Build (checked)
# Script Debugging (checked)
# Build
```

**Features**:
- Debug symbols enabled
- Profiler connection enabled
- Faster iteration time

### Release Build

```powershell
# In Unity Editor:
# File → Build Settings → Development Build (unchecked)
# IL2CPP scripting backend
# Code stripping: High
# Build
```

**Features**:
- Optimizations enabled
- Compressed assets
- Smaller file size

---

## Performance Targets

- **Frame Rate**: 60 FPS minimum on mid-range hardware
- **Factory Scale**: Support 1000+ active machines without degradation
- **Save/Load Time**: <5 seconds for typical factory
- **Memory Usage**: <4GB RAM for large factories

---

## Security & Best Practices

### Sensitive Data

- **Never commit**: API keys, passwords, personal data
- **Use**: `.gitignore` for sensitive files
- **Store**: Secrets in environment variables or secure vaults

### Code Review

- All code changes require review before merging
- Use pull requests for all changes
- Run tests before requesting review
- Address review comments promptly

---

## Contact & Support

### Project Maintainers

- Check `CONTRIBUTING.md` for contribution guidelines
- Check `README.md` for project overview

### Getting Help

1. Check steering documents in `.kiro/steering/`
2. Check system documentation in `Documentation/`
3. Search existing issues on GitHub
4. Create new issue with detailed description

---

## Quick Reference

### Common Commands

```powershell
# Open Unity project
# Unity Hub → Open Project → Select folder

# Run tests
# Window → General → Test Runner → Run All

# Build project
# File → Build Settings → Build

# Open Profiler
# Window → Analysis → Profiler (Ctrl+7)

# Open Test Runner
# Window → General → Test Runner (Ctrl+Alt+T)
```

### Key Files

- `Assets/_Project/Scripts/Core/ResourceManager.cs` - Resource management
- `Assets/_Project/Scripts/Core/GridManager.cs` - Grid system
- `Assets/_Project/Scripts/Core/PlacementController.cs` - Placement logic
- `Assets/_Project/Scripts/Machines/MachineBase.cs` - Machine base class
- `.kiro/steering/` - Steering documents
- `Documentation/` - System documentation

### Key Shortcuts

- `Ctrl+7`: Open Profiler
- `Ctrl+Alt+T`: Open Test Runner
- `Ctrl+Shift+B`: Build project
- `Ctrl+P`: Quick open asset
- `Ctrl+K`: Search in scene

---

**Last Updated**: November 5, 2025  
**Version**: 1.0  
**Maintained By**: Project Team

For more detailed information, see the steering documents in `.kiro/steering/` and system documentation in `Documentation/`.
