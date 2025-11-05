# How to Create Test Assets

## Quick Start

1. **Open Unity Editor** (Unity 6.0 or later)

2. **Create All Test Assets**:
   - Go to menu: `Santa → Create Test Assets → All Test Assets`
   - This will create both test recipes and machine data in one step

3. **Verify Assets Created**:
   - Check `Assets/_Project/ScriptableObjects/TestAssets/Recipes/` for recipe assets
   - Check `Assets/_Project/ScriptableObjects/TestAssets/Machines/` for machine data assets

## Individual Asset Creation

If you only need specific assets:

### Create Only Recipes
- Menu: `Santa → Create Test Assets → Test Recipes`
- Creates: TestExtractorRecipe.asset, TestProcessorRecipe.asset

### Create Only Machine Data
- Menu: `Santa → Create Test Assets → Test Machine Data`
- Creates: TestExtractorData.asset, TestProcessorData.asset
- **Note**: Recipes must be created first, as machine data references them

## Cleaning Up

To remove all test assets:
- Menu: `Santa → Create Test Assets → Clean Test Assets`
- This deletes the entire TestAssets folder

## What Gets Created

### TestExtractorRecipe.asset
```
Recipe ID: test_extractor_recipe
Name: Test Extractor Recipe
Inputs: (none)
Outputs: 1x iron_ore
Processing Time: 2 seconds
Power: 10W
Tier: 1
```

### TestProcessorRecipe.asset
```
Recipe ID: test_processor_recipe
Name: Test Processor Recipe
Inputs: 1x iron_ore
Outputs: 1x iron_ingot
Processing Time: 3 seconds
Power: 20W
Tier: 1
```

### TestExtractorData.asset
```
Name: Test Extractor
Grid Size: 1x1
Tier: 1
Input Ports: 0
Output Ports: 1 (right side at x=0.5, y=0.5, z=0)
Buffer Capacity: 10
Recipes: TestExtractorRecipe
```

### TestProcessorData.asset
```
Name: Test Processor
Grid Size: 1x1
Tier: 1
Input Ports: 1 (left side at x=-0.5, y=0.5, z=0)
Output Ports: 1 (right side at x=0.5, y=0.5, z=0)
Buffer Capacity: 10
Recipes: TestProcessorRecipe
```

## Using in Tests

### Loading Assets in Test Code

```csharp
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using SantasWorkshop.Data;

[Test]
public void TestMachineWithAssets()
{
    // Load test machine data
    MachineData extractorData = AssetDatabase.LoadAssetAtPath<MachineData>(
        "Assets/_Project/ScriptableObjects/TestAssets/Machines/TestExtractorData.asset");
    
    Assert.IsNotNull(extractorData, "TestExtractorData should exist");
    Assert.AreEqual(1, extractorData.availableRecipes.Count, "Should have 1 recipe");
    
    // Load test recipe
    Recipe recipe = extractorData.availableRecipes[0];
    Assert.IsNotNull(recipe, "Recipe should not be null");
    Assert.AreEqual("test_extractor_recipe", recipe.recipeId);
}
```

### Creating Test Machines

```csharp
[Test]
public void CreateTestMachine()
{
    // Load machine data
    MachineData data = AssetDatabase.LoadAssetAtPath<MachineData>(
        "Assets/_Project/ScriptableObjects/TestAssets/Machines/TestExtractorData.asset");
    
    // Create GameObject with machine component
    GameObject go = new GameObject("TestMachine");
    TestExtractor machine = go.AddComponent<TestExtractor>();
    
    // Initialize with test data
    // (Implementation depends on your machine initialization code)
    
    // Test machine behavior
    // ...
    
    // Cleanup
    Object.DestroyImmediate(go);
}
```

## Troubleshooting

### "Menu item not found"
- Make sure the Editor script is in an `Editor` folder
- Check that the script compiled without errors
- Restart Unity Editor if needed

### "Recipe not found" warning
- Create recipes before creating machine data
- Use "All Test Assets" menu option to create everything in order

### Assets not appearing
- Check Unity Console for errors
- Refresh Asset Database: Right-click in Project window → Refresh
- Check that folders were created correctly

### Port positions incorrect
- Port positions are in local space relative to machine center
- Default positions: Input left (-0.5, 0.5, 0), Output right (0.5, 0.5, 0)
- Modify positions in the Editor script if needed

## Customization

To modify the test assets, edit the `CreateTestAssets.cs` script:

```csharp
// Location: Assets/_Project/Scripts/Editor/CreateTestAssets.cs

// Example: Change processing time
extractorRecipe.processingTime = 5f; // Change from 2f to 5f

// Example: Add more outputs
extractorRecipe.outputs = new ResourceStack[]
{
    new ResourceStack { resourceId = "iron_ore", amount = 2 }, // Changed from 1 to 2
    new ResourceStack { resourceId = "coal", amount = 1 }      // Added second output
};

// Example: Change port positions
extractorData.outputPortPositions = new Vector3[]
{
    new Vector3(0f, 0.5f, 0.5f) // Move to front instead of right
};
```

After modifying the script:
1. Save the file
2. Run "Clean Test Assets" to remove old assets
3. Run "All Test Assets" to create new assets with your changes

## Best Practices

1. **Always create recipes before machine data** - Machine data references recipes
2. **Use "All Test Assets"** for initial setup - Ensures correct order
3. **Clean before recreating** - Prevents duplicate or stale assets
4. **Verify in Inspector** - Check created assets in Unity Inspector to confirm settings
5. **Keep test assets separate** - Don't mix with production assets

## Integration with Tests

These test assets are designed to work with:
- Unit tests for MachineBase class
- Integration tests for machine processing
- Port and buffer tests
- Recipe validation tests
- State machine tests

See the test files in `Assets/_Project/Scripts/Tests/` for examples.
