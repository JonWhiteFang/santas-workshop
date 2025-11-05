# Machine Framework Examples

This folder contains example implementations demonstrating proper inheritance patterns for the Machine Framework.

## Example Classes

### ExampleMiningDrill.cs
**Base Class**: `ExtractorBase`

Demonstrates:
- Continuous resource extraction
- Visual feedback (particles, animations, audio)
- Resource delivery to ResourceManager
- Proper base method calling
- State and power change handling

**Key Patterns**:
```csharp
// Always call base methods first
public override void Initialize(MachineData data)
{
    base.Initialize(data);
    // Custom initialization
}

// Override extraction callback
protected override void OnResourceExtracted()
{
    base.OnResourceExtracted();
    // Add resources to ResourceManager
}
```

### ExampleSmelter.cs
**Base Class**: `ProcessorBase`

Demonstrates:
- Recipe-based processing
- Input/output buffer management
- Visual effects (fire, glow, light)
- Recipe validation and switching
- Completion effects

**Key Patterns**:
```csharp
// Override completion callback
protected override void CompleteProcessing()
{
    base.CompleteProcessing();
    // Play completion effects
}

// Recipe validation
public override void SetRecipe(RecipeData recipe)
{
    // Validate recipe is available
    base.SetRecipe(recipe);
}
```

### ExampleToyAssembler.cs
**Base Class**: `AssemblerBase`

Demonstrates:
- Multi-input assembly
- Inventory management
- Component tracking
- Toy spawning
- Custom events

**Key Patterns**:
```csharp
// Override assembly completion
protected override void CompleteAssembly()
{
    base.CompleteAssembly();
    // Spawn toy visual
}

// Custom inventory handling
public override bool AddToInventory(string resourceId, int amount)
{
    bool success = base.AddToInventory(resourceId, amount);
    if (success)
    {
        OnInventoryChanged?.Invoke();
    }
    return success;
}
```

## Usage

These examples are **reference implementations** showing best practices. Use them as templates when creating your own machines:

1. **Copy the example** that matches your machine type
2. **Rename the class** to your machine name
3. **Customize the behavior** while maintaining the patterns
4. **Always call base methods** to preserve functionality

## Common Patterns

### 1. Always Call Base Methods

```csharp
// ✅ GOOD
public override void Initialize(MachineData data)
{
    base.Initialize(data); // Call base first
    // Custom initialization
}

// ❌ BAD
public override void Initialize(MachineData data)
{
    // Custom initialization
    // Forgot to call base!
}
```

### 2. Use showDebugInfo for Logging

```csharp
// ✅ GOOD
if (showDebugInfo)
{
    Debug.Log($"[MyMachine] {MachineId} extracted ore");
}

// ❌ BAD
Debug.Log($"[MyMachine] {MachineId} extracted ore"); // Always logs
```

### 3. Cache Component References

```csharp
// ✅ GOOD
private Animator animator;

protected override void Awake()
{
    base.Awake();
    animator = GetComponent<Animator>();
}

// ❌ BAD
private void UpdateAnimation()
{
    GetComponent<Animator>().SetBool("IsWorking", true); // Expensive!
}
```

### 4. Handle Null References

```csharp
// ✅ GOOD
if (ResourceManager.Instance != null)
{
    ResourceManager.Instance.AddResource(oreType, amount);
}

// ❌ BAD
ResourceManager.Instance.AddResource(oreType, amount); // May be null!
```

### 5. Clean Up in OnDestroy

```csharp
// ✅ GOOD
protected override void OnDestroy()
{
    // Stop effects
    if (particles != null && particles.isPlaying)
    {
        particles.Stop();
    }
    
    // Call base last
    base.OnDestroy();
}
```

## Testing Your Machine

1. **Create a test scene** or use `MachineFrameworkTest.unity`
2. **Add your machine prefab** to the scene
3. **Assign MachineData** in the Inspector
4. **Enable Show Debug Info** for detailed logging
5. **Play the scene** and verify functionality

## Documentation

For more detailed information, see:
- `Documentation/Systems/MACHINE_FRAMEWORK_GUIDE.md` - Developer guide
- `Documentation/Systems/MACHINE_DESIGNER_GUIDE.md` - Designer guide
- `.kiro/steering/game-design-patterns.md` - Architecture patterns

## Questions?

Check the troubleshooting section in the developer guide or review these examples for reference implementations.
