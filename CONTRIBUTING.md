# Contributing to Santa's Workshop Automation

Thank you for contributing to Santa's Workshop Automation! This document provides guidelines and best practices for development.

## Table of Contents

- [Code Style and Naming Conventions](#code-style-and-naming-conventions)
- [Architecture Overview](#architecture-overview)
- [Development Workflow](#development-workflow)
- [Best Practices](#best-practices)
- [Testing Guidelines](#testing-guidelines)

## Code Style and Naming Conventions

### C# Naming Conventions

Follow these naming conventions consistently throughout the codebase:

#### Classes, Structs, Enums, and Interfaces
- **Format**: PascalCase
- **Examples**:
  ```csharp
  public class ResourceManager { }
  public struct ResourceStack { }
  public enum MachineState { }
  public interface IMachine { }
  ```

#### Public Fields, Properties, and Methods
- **Format**: PascalCase
- **Examples**:
  ```csharp
  public int MaxHealth { get; set; }
  public float MoveSpeed = 5f;
  public void TakeDamage(int amount) { }
  ```

#### Private Fields
- **Format**: _camelCase (underscore prefix)
- **Examples**:
  ```csharp
  private int _currentHealth;
  private Transform _transform;
  private Rigidbody _rigidbody;
  ```

#### Serialized Fields
- **Format**: camelCase (no underscore, matches Inspector display)
- **Examples**:
  ```csharp
  [SerializeField] private float moveSpeed = 5f;
  [SerializeField] private GameObject enemyPrefab;
  ```

#### Parameters and Local Variables
- **Format**: camelCase
- **Examples**:
  ```csharp
  public void Initialize(int startingHealth)
  {
      int calculatedValue = startingHealth * 2;
  }
  ```

#### Constants
- **Format**: UPPER_SNAKE_CASE or PascalCase
- **Examples**:
  ```csharp
  private const int MAX_ENEMIES = 100;
  private const float GRAVITY_SCALE = 9.81f;
  ```

### Namespace Organization

All code must use the `SantasWorkshop` namespace with appropriate sub-namespaces:

```csharp
namespace SantasWorkshop.Core
{
    public class GameManager : MonoBehaviour { }
}

namespace SantasWorkshop.Machines
{
    public class MachineBase : MonoBehaviour { }
}

namespace SantasWorkshop.Data
{
    public class ResourceData : ScriptableObject { }
}
```

**Standard Sub-namespaces**:
- `SantasWorkshop.Core` - Core systems and managers
- `SantasWorkshop.Machines` - Machine framework and implementations
- `SantasWorkshop.Logistics` - Transport and routing systems
- `SantasWorkshop.Research` - Tech tree and progression
- `SantasWorkshop.Missions` - Campaign and objectives
- `SantasWorkshop.UI` - UI controllers and views
- `SantasWorkshop.Data` - ScriptableObjects and data structures
- `SantasWorkshop.Utilities` - Helper classes and extensions

### Code Organization

Use `#region` directives to organize large classes:

```csharp
public class MachineBase : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private MachineData machineData;
    #endregion

    #region Private Fields
    private MachineState _currentState;
    private float _powerConsumption;
    #endregion

    #region Properties
    public string MachineId => machineData.id;
    public MachineState State => _currentState;
    #endregion

    #region Unity Lifecycle
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    #endregion

    #region Public Methods
    public void Initialize(MachineData data) { }
    #endregion

    #region Private Methods
    private void UpdateState() { }
    #endregion
}
```

## Architecture Overview

Santa's Workshop Automation follows a **three-layer architecture** that separates concerns and enables parallel development:

### Layer 1: Render Layer

**Purpose**: Visual representation and presentation

**Responsibilities**:
- Rendering meshes, materials, and particles
- URP rendering pipeline configuration
- Camera systems (Cinemachine)
- Animation and VFX
- Lighting and post-processing

**Key Principles**:
- Read-only access to simulation state
- No game logic in render layer
- Performance-optimized rendering (GPU instancing, LOD)

### Layer 2: Simulation Layer

**Purpose**: Game state and logic

**Responsibilities**:
- Machine systems (extractors, processors, assemblers)
- Resource management and tracking
- Power grid simulation
- Logistics (conveyors, routing)
- Research and progression
- Mission system
- Save/load functionality

**Key Principles**:
- Single source of truth for game state
- Frame-rate independent calculations (use Time.deltaTime)
- Deterministic simulation for save/load consistency
- No direct UI updates from simulation

### Layer 3: UI Layer

**Purpose**: User interface and input handling

**Responsibilities**:
- UI Toolkit interfaces
- Async reads from simulation snapshots
- Input handling (new Input System)
- HUD and overlays
- Menus and dialogs

**Key Principles**:
- Read from cached simulation state (snapshots)
- Never block simulation with UI operations
- Use async/await for UI updates
- Input flows from UI → Simulation

### Layer Communication

```
┌─────────────────────────────────────┐
│       Render Layer                  │  ← Reads simulation state
├─────────────────────────────────────┤
│       Simulation Layer              │  ← Single source of truth
├─────────────────────────────────────┤
│       UI Layer                      │  ← Reads snapshots, sends input
└─────────────────────────────────────┘
```

**Rules**:
- Render and UI layers **read** from Simulation
- UI layer **sends input** to Simulation
- Simulation **never directly updates** Render or UI
- Use events and callbacks for cross-layer communication

## Development Workflow

### 1. Branch Strategy

- `main` - Stable, production-ready code
- `develop` - Integration branch for features
- `feature/<feature-name>` - Individual feature branches
- `bugfix/<bug-name>` - Bug fix branches

### 2. Commit Messages

Follow conventional commit format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**:
- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation changes
- `style` - Code style changes (formatting, no logic change)
- `refactor` - Code refactoring
- `perf` - Performance improvements
- `test` - Adding or updating tests
- `chore` - Maintenance tasks

**Examples**:
```
feat(machines): Add smelter machine implementation
fix(power): Resolve power grid calculation error
docs(readme): Update installation instructions
```

### 3. Pull Request Process

1. Create feature branch from `develop`
2. Implement changes following coding standards
3. Write/update tests for new functionality
4. Ensure all tests pass
5. Update documentation if needed
6. Create pull request to `develop`
7. Request code review from team member
8. Address review feedback
9. Merge after approval

## Best Practices

### Unity-Specific Guidelines

#### Component Caching

Always cache component references in `Awake()`:

```csharp
public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Animator _animator;
    private Transform _transform;

    private void Awake()
    {
        // Cache components once
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _transform = transform;
    }

    private void Update()
    {
        // Use cached references (fast)
        _rigidbody.velocity = Vector3.forward * 5f;
    }
}
```

**Never** use `GetComponent()` in `Update()`, `FixedUpdate()`, or `LateUpdate()`.

#### Serialization

Use `[SerializeField]` for private fields that need Inspector visibility:

```csharp
public class MachineController : MonoBehaviour
{
    // ✅ Good: Private with [SerializeField]
    [SerializeField] private float processingSpeed = 1f;
    [SerializeField] private GameObject outputPrefab;

    // ❌ Bad: Public fields (breaks encapsulation)
    public float processingSpeed = 1f;
    public GameObject outputPrefab;
}
```

#### ScriptableObjects for Data

Use ScriptableObjects for shared data and configuration:

```csharp
[CreateAssetMenu(fileName = "NewMachine", menuName = "Santa/Machine")]
public class MachineData : ScriptableObject
{
    public string machineId;
    public string displayName;
    public float powerConsumption;
    public ResourceStack[] buildCost;
}
```

**Benefits**:
- Share data between multiple objects
- Reduce memory usage
- Easy to modify in Inspector
- Version control friendly

### Performance Guidelines

#### Avoid Expensive Operations in Update

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

**Rules**:
- Never use `GameObject.Find()` or `FindGameObjectWithTag()` in Update
- Never use `Camera.main` in Update (cache it)
- Avoid `GetComponent()` in Update (cache it)
- Use `CompareTag()` instead of `gameObject.tag == "Player"`

#### Object Pooling

Reuse objects instead of instantiating/destroying:

```csharp
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 10;

    private Queue<GameObject> _pool = new();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        if (_pool.Count > 0)
        {
            GameObject obj = _pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(prefab);
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }
}
```

### Code Quality

#### XML Documentation

Document public APIs with XML comments:

```csharp
/// <summary>
/// Manages global resource tracking and consumption.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    /// <summary>
    /// Attempts to consume the specified resources.
    /// </summary>
    /// <param name="resources">Array of resources to consume</param>
    /// <returns>True if resources were consumed, false if insufficient</returns>
    public bool TryConsumeResources(ResourceStack[] resources)
    {
        // Implementation
    }
}
```

#### Error Handling

Always handle errors gracefully:

```csharp
public async Task<SaveResult> SaveGame(string saveName)
{
    try
    {
        SaveData data = GatherSaveData();
        string json = JsonUtility.ToJson(data, true);
        await File.WriteAllTextAsync(GetSavePath(saveName), json);
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
```

#### Event Cleanup

Always unsubscribe from events:

```csharp
public class HealthUI : MonoBehaviour
{
    private void OnEnable()
    {
        Player.OnHealthChanged += UpdateHealthBar;
    }

    private void OnDisable()
    {
        Player.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(int health)
    {
        // Update UI
    }
}
```

## Testing Guidelines

### Unit Tests

Write unit tests for core systems using Unity Test Framework:

```csharp
using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Core;

public class ResourceManagerTests
{
    private ResourceManager _resourceManager;

    [SetUp]
    public void Setup()
    {
        GameObject go = new GameObject();
        _resourceManager = go.AddComponent<ResourceManager>();
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
        _resourceManager.AddResources(resources);

        // Assert
        Assert.IsTrue(_resourceManager.HasResource("wood", 100));
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_resourceManager.gameObject);
    }
}
```

### Test Coverage

Focus on testing:
- Core game logic
- Resource management
- Machine state transitions
- Save/load functionality
- Edge cases and error conditions

### Integration Tests

Create test scenes for system integration:
- `TestScene_ResourceFlow` - Resource extraction → processing → assembly
- `TestScene_PowerGrid` - Power generation and distribution
- `TestScene_SaveLoad` - Save/load functionality

## Questions?

If you have questions about contributing, please reach out to the development team.

---

**Happy coding, and may your factory be merry and bright!** 🎄
