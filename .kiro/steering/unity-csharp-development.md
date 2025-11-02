# Unity 6 and C# Development Guidelines

**Last Updated**: November 2, 2025  
**Unity Version**: 6.0 (6000.2.10f1)  
**C# Version**: C# 9.0+ (.NET Standard 2.1)

This steering document provides comprehensive guidelines for developing Unity projects using C# and Unity 6. Follow these patterns and best practices to ensure consistent, performant, and maintainable code.

---

## Unity 6 Fundamentals

### MonoBehaviour Lifecycle

Unity scripts inherit from `MonoBehaviour` and follow a specific execution order:

```csharp
using UnityEngine;

public class ExampleBehaviour : MonoBehaviour
{
    // Called when script instance is being loaded (before Start)
    private void Awake()
    {
        // Initialize references and setup that doesn't depend on other objects
        // Use for singleton patterns and component caching
    }

    // Called on the frame when a script is enabled (after Awake)
    private void Start()
    {
        // Initialize logic that depends on other objects being ready
        // Called only once, even if object is disabled/enabled
    }

    // Called every frame (frame-rate dependent)
    private void Update()
    {
        // Handle input, movement, and frame-by-frame logic
        // Use Time.deltaTime for frame-rate independent calculations
    }

    // Called at fixed intervals (physics updates)
    private void FixedUpdate()
    {
        // Handle physics calculations and Rigidbody operations
        // Consistent timing regardless of frame rate
    }

    // Called after all Update functions (useful for camera following)
    private void LateUpdate()
    {
        // Handle logic that needs to run after all Updates
        // Common for camera systems and final position adjustments
    }

    // Called when the object becomes disabled or inactive
    private void OnDisable()
    {
        // Unsubscribe from events, stop coroutines
    }

    // Called when the MonoBehaviour will be destroyed
    private void OnDestroy()
    {
        // Clean up resources, unsubscribe from static events
    }
}
```

**Key Rules**:
- Use `Awake()` for internal initialization and component caching
- Use `Start()` for initialization that depends on other objects
- Cache component references in `Awake()` to avoid repeated `GetComponent()` calls
- Use `FixedUpdate()` for physics, `Update()` for input and rendering logic
- Always clean up in `OnDisable()` and `OnDestroy()`

---

## C# Best Practices for Unity

### Modern C# Features (C# 9.0+)

Unity 6 supports modern C# features. Use them appropriately:

```csharp
// Null-coalescing assignment (C# 8.0)
private Rigidbody _rigidbody;
private void Awake()
{
    _rigidbody ??= GetComponent<Rigidbody>();
}

// Pattern matching with switch expressions (C# 8.0)
public string GetEnemyType(Enemy enemy) => enemy.Type switch
{
    EnemyType.Melee => "Warrior",
    EnemyType.Ranged => "Archer",
    EnemyType.Magic => "Mage",
    _ => "Unknown"
};

// Target-typed new expressions (C# 9.0)
private Vector3 spawnPosition = new(0, 10, 0);
private List<GameObject> enemies = new();

// Init-only properties (C# 9.0)
public class PlayerStats
{
    public int MaxHealth { get; init; }
    public float Speed { get; init; }
}

// Record types for data (C# 9.0)
public record PlayerData(string Name, int Level, float Experience);
```

**Guidelines**:
- Use null-coalescing operators to simplify null checks
- Prefer pattern matching over complex if-else chains
- Use init-only properties for immutable configuration data
- Consider record types for data transfer objects (DTOs)
- Avoid records for MonoBehaviour classes (they don't serialize well)

---

### Async/Await in Unity

Unity supports async/await for asynchronous operations. Use it carefully:

```csharp
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class AsyncExample : MonoBehaviour
{
    // Initialize Unity Services asynchronously
    private async void Awake()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Services initialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Async method with proper error handling
    public async Task<string> LoadDataAsync()
    {
        // Simulate async operation
        await Task.Delay(1000);
        return "Data loaded";
    }

    // Calling async methods
    public async void OnButtonClick()
    {
        string data = await LoadDataAsync();
        Debug.Log(data);
    }
}
```

**Critical Rules**:
- **NEVER** use `async void` except for Unity event handlers (like button clicks)
- Always use `async Task` or `async Task<T>` for methods you control
- Always wrap async operations in try-catch blocks
- Be aware that async methods continue after object destruction
- Use `CancellationToken` for long-running operations
- Prefer Coroutines for frame-based delays and Unity-specific timing

**When to use Async vs Coroutines**:
- **Async/Await**: Network calls, file I/O, Unity Services, database operations
- **Coroutines**: Frame-based delays, animations, sequences tied to game time

```csharp
// Coroutine example (preferred for Unity timing)
private IEnumerator SpawnEnemiesCoroutine()
{
    for (int i = 0; i < 10; i++)
    {
        SpawnEnemy();
        yield return new WaitForSeconds(2f); // Wait 2 seconds
    }
}

// Start coroutine
private void Start()
{
    StartCoroutine(SpawnEnemiesCoroutine());
}
```

---

### LINQ in Unity

LINQ is powerful but can cause performance issues and garbage collection. Use wisely:

```csharp
using System.Linq;
using UnityEngine;

public class LINQExample : MonoBehaviour
{
    private List<Enemy> enemies = new();

    // ❌ BAD: Creates garbage, slow in Update/FixedUpdate
    private void Update()
    {
        var aliveEnemies = enemies.Where(e => e.IsAlive).ToList();
        // Process enemies...
    }

    // ✅ GOOD: Use for initialization or infrequent operations
    private void Start()
    {
        var bossEnemies = enemies
            .Where(e => e.Type == EnemyType.Boss)
            .OrderByDescending(e => e.Health)
            .ToList();
    }

    // ✅ BETTER: Use traditional loops for frequent operations
    private void Update()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsAlive)
            {
                // Process enemy...
            }
        }
    }
}
```

**LINQ Guidelines**:
- **AVOID** LINQ in `Update()`, `FixedUpdate()`, or `LateUpdate()`
- **USE** LINQ for initialization, editor scripts, and infrequent operations
- **PREFER** traditional loops for performance-critical code
- Be aware that LINQ creates garbage (temporary allocations)
- Methods like `.ToList()`, `.ToArray()` allocate new collections

---

## Unity Component Patterns

### Component Caching

Always cache component references to avoid expensive `GetComponent()` calls:

```csharp
public class PlayerController : MonoBehaviour
{
    // Serialized fields (visible in Inspector)
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    // Cached components
    private Rigidbody _rigidbody;
    private Animator _animator;
    private Transform _transform;

    private void Awake()
    {
        // Cache components once
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _transform = transform; // 'transform' is already cached by Unity
    }

    private void Update()
    {
        // Use cached references
        float horizontal = Input.GetAxis("Horizontal");
        _rigidbody.velocity = new Vector3(horizontal * moveSpeed, _rigidbody.velocity.y, 0);
        _animator.SetFloat("Speed", Mathf.Abs(horizontal));
    }
}
```

**Component Caching Rules**:
- Cache in `Awake()` for internal components
- Use `[SerializeField]` for components assigned in Inspector
- Use `[RequireComponent(typeof(Rigidbody))]` to enforce dependencies
- Check for null before using cached components from other objects

---

### Serialization and Inspector

Unity serializes fields to save data and display in Inspector:

```csharp
using UnityEngine;

public class SerializationExample : MonoBehaviour
{
    // ✅ Serialized: Visible in Inspector, saved with scene
    [SerializeField] private int health = 100;
    [SerializeField] private GameObject enemyPrefab;
    
    // ✅ Public fields are automatically serialized
    public float attackDamage = 25f;

    // ❌ Not serialized: Private without [SerializeField]
    private int score;

    // ❌ Not serialized: Static fields
    public static int globalScore;

    // ❌ Not serialized: Properties
    public int Health { get; set; }

    // ✅ Header and Tooltip for better Inspector organization
    [Header("Movement Settings")]
    [Tooltip("Maximum movement speed in units per second")]
    [SerializeField] private float maxSpeed = 10f;

    [Range(0f, 1f)]
    [SerializeField] private float acceleration = 0.5f;

    // ✅ Hide public field from Inspector
    [HideInInspector] public bool isInitialized;
}
```

**Serialization Best Practices**:
- Use `[SerializeField]` for private fields you want in Inspector
- Prefer private serialized fields over public fields (encapsulation)
- Use `[Header]` and `[Tooltip]` for better Inspector organization
- Use `[Range]` for numeric values with min/max constraints
- Properties are NOT serialized (use backing fields)
- Use `[HideInInspector]` to hide public fields

---

### ScriptableObjects for Data

Use ScriptableObjects for shared data and configuration:

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName;
    public Sprite icon;

    [Header("Stats")]
    public int damage = 10;
    public float fireRate = 0.5f;
    public int magazineSize = 30;

    [Header("Effects")]
    public GameObject muzzleFlashPrefab;
    public AudioClip fireSound;

    // Methods can be added to ScriptableObjects
    public void Fire()
    {
        Debug.Log($"Firing {weaponName} for {damage} damage");
    }
}

// Using ScriptableObject in MonoBehaviour
public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponData currentWeapon;

    private void Start()
    {
        Debug.Log($"Equipped: {currentWeapon.weaponName}");
    }

    private void Fire()
    {
        currentWeapon.Fire();
    }
}
```

**ScriptableObject Benefits**:
- Share data between multiple objects without duplication
- Reduce memory usage (one instance, many references)
- Easy to create and modify in Inspector
- Persist between scenes
- Great for game configuration, item databases, enemy stats

---

## Unity Services Integration

### Initializing Unity Services

Always initialize Unity Services before using any Unity Gaming Services:

```csharp
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System;

public class ServicesInitializer : MonoBehaviour
{
    private async void Awake()
    {
        try
        {
            // Initialize Unity Services Core SDK
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services initialized");

            // Sign in anonymously
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
```

### Cloud Code Integration

Call Cloud Code scripts from Unity:

```csharp
using Unity.Services.CloudCode;
using System.Collections.Generic;

public class CloudCodeExample : MonoBehaviour
{
    // Call Cloud Code script endpoint
    public async void CallCloudCodeScript()
    {
        try
        {
            var arguments = new Dictionary<string, object>
            {
                { "playerLevel", 5 },
                { "itemId", "sword_001" }
            };

            var response = await CloudCodeService.Instance
                .CallEndpointAsync<ResponseType>("GetReward", arguments);

            Debug.Log($"Reward: {response.rewardAmount}");
        }
        catch (CloudCodeException e)
        {
            Debug.LogError($"Cloud Code Error: {e.Message}");
        }
    }

    // Response type matching Cloud Code return structure
    private class ResponseType
    {
        public int rewardAmount;
        public string rewardType;
    }
}
```

---

## Performance Best Practices

### Avoid Expensive Operations in Update

```csharp
public class PerformanceExample : MonoBehaviour
{
    // ❌ BAD: Expensive operations every frame
    private void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // SLOW
        Camera.main.transform.position = player.transform.position; // SLOW (Camera.main)
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
}
```

**Performance Rules**:
- **NEVER** use `GameObject.Find()` or `GameObject.FindGameObjectWithTag()` in Update
- **NEVER** use `Camera.main` in Update (cache it in Awake)
- **AVOID** `GetComponent()` in Update (cache in Awake)
- **USE** object pooling for frequently instantiated objects
- **USE** `CompareTag()` instead of `gameObject.tag == "Player"`

---

### Object Pooling

Reuse objects instead of instantiating/destroying:

```csharp
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 10;

    private Queue<GameObject> pool = new();

    private void Awake()
    {
        // Pre-instantiate objects
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // Pool exhausted, create new object
        return Instantiate(prefab);
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

---

## Input System

Unity 6 supports both old and new Input Systems. Prefer the new Input System:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private PlayerInputActions _inputActions;
    private Vector2 _moveInput;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
        _inputActions.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;
        _inputActions.Player.Jump.performed -= OnJump;
        _inputActions.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump!");
    }
}
```

---

## Naming Conventions

Follow these naming conventions for consistency:

```csharp
// Classes, Structs, Enums: PascalCase
public class PlayerController { }
public struct PlayerData { }
public enum EnemyType { Melee, Ranged, Magic }

// Public fields, properties, methods: PascalCase
public int MaxHealth { get; set; }
public void TakeDamage(int amount) { }

// Private fields: _camelCase with underscore prefix
private int _currentHealth;
private Transform _transform;

// Parameters, local variables: camelCase
public void Initialize(int startingHealth)
{
    int calculatedValue = startingHealth * 2;
}

// Constants: UPPER_SNAKE_CASE or PascalCase
private const int MAX_ENEMIES = 100;
private const float GRAVITY_SCALE = 9.81f;

// Serialized fields: camelCase (matches Inspector display)
[SerializeField] private float moveSpeed = 5f;
[SerializeField] private GameObject enemyPrefab;
```

---

## Common Patterns

### Singleton Pattern

Use for managers that should have only one instance:

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
```

**Singleton Guidelines**:
- Use sparingly (can make testing difficult)
- Good for: GameManager, AudioManager, InputManager
- Bad for: Everything else (prefer dependency injection)

---

### Event System

Use events for decoupled communication:

```csharp
using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Define event
    public static event Action<int> OnHealthChanged;
    public static event Action OnPlayerDied;

    private int _health = 100;

    public void TakeDamage(int amount)
    {
        _health -= amount;
        
        // Invoke event (null-conditional operator prevents null reference)
        OnHealthChanged?.Invoke(_health);

        if (_health <= 0)
        {
            OnPlayerDied?.Invoke();
        }
    }
}

// Subscriber
public class HealthUI : MonoBehaviour
{
    private void OnEnable()
    {
        Player.OnHealthChanged += UpdateHealthBar;
        Player.OnPlayerDied += ShowGameOver;
    }

    private void OnDisable()
    {
        Player.OnHealthChanged -= UpdateHealthBar;
        Player.OnPlayerDied -= ShowGameOver;
    }

    private void UpdateHealthBar(int health)
    {
        Debug.Log($"Health: {health}");
    }

    private void ShowGameOver()
    {
        Debug.Log("Game Over!");
    }
}
```

**Event Best Practices**:
- Always unsubscribe in `OnDisable()` or `OnDestroy()`
- Use `?.Invoke()` to safely invoke events
- Consider UnityEvents for Inspector-assignable events
- Use static events for global notifications

---

## Debugging and Logging

Use Unity's logging system effectively:

```csharp
using UnityEngine;

public class DebuggingExample : MonoBehaviour
{
    private void Start()
    {
        // Standard log
        Debug.Log("Game started");

        // Warning (yellow in console)
        Debug.LogWarning("Low health!");

        // Error (red in console)
        Debug.LogError("Failed to load data");

        // Conditional compilation (removed in builds)
        #if UNITY_EDITOR
        Debug.Log("This only appears in Editor");
        #endif

        // Rich text formatting
        Debug.Log("<color=green>Success!</color>");
        Debug.Log("<b>Bold text</b>");

        // Log with context (click to highlight object)
        Debug.Log("Player spawned", gameObject);
    }
}
```

---

## Testing Guidelines

### Unit Testing

Unity supports unit testing with Unity Test Framework:

```csharp
using NUnit.Framework;
using UnityEngine;

public class PlayerTests
{
    [Test]
    public void Player_TakeDamage_ReducesHealth()
    {
        // Arrange
        var player = new GameObject().AddComponent<Player>();
        int initialHealth = player.Health;

        // Act
        player.TakeDamage(10);

        // Assert
        Assert.AreEqual(initialHealth - 10, player.Health);
    }

    [Test]
    public void Player_TakeDamage_TriggersEvent()
    {
        // Arrange
        var player = new GameObject().AddComponent<Player>();
        bool eventTriggered = false;
        Player.OnHealthChanged += (health) => eventTriggered = true;

        // Act
        player.TakeDamage(10);

        // Assert
        Assert.IsTrue(eventTriggered);
    }
}
```

---

## Summary

This steering document covers the essential patterns and practices for Unity 6 development with C#. Key takeaways:

1. **Lifecycle**: Understand MonoBehaviour lifecycle and use appropriate methods
2. **Performance**: Cache references, avoid expensive operations in Update
3. **Async**: Use async/await for I/O, coroutines for Unity timing
4. **LINQ**: Avoid in Update loops, use for initialization
5. **Serialization**: Use [SerializeField] for Inspector visibility
6. **Patterns**: Use ScriptableObjects for data, events for communication
7. **Services**: Initialize Unity Services before use
8. **Naming**: Follow consistent naming conventions

Always prioritize readability, maintainability, and performance in your Unity projects.
