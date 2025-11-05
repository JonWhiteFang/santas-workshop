# Test Assets for Machine Framework

This directory contains test ScriptableObject assets used for testing the Machine Framework implementation.

## Contents

### Recipes
- **TestExtractorRecipe**: A simple recipe with no inputs that produces 1 iron ore. Used for testing extractor machines.
  - Inputs: None
  - Outputs: 1x iron_ore
  - Processing Time: 2 seconds
  - Power Consumption: 10W
  - Required Tier: 1

- **TestProcessorRecipe**: A simple recipe that converts iron ore to iron ingots. Used for testing processor machines.
  - Inputs: 1x iron_ore
  - Outputs: 1x iron_ingot
  - Processing Time: 3 seconds
  - Power Consumption: 20W
  - Required Tier: 1

### Machines
- **TestExtractorData**: Configuration for a test extractor machine.
  - Grid Size: 1x1
  - Tier: 1
  - Input Ports: 0
  - Output Ports: 1 (right side)
  - Buffer Capacity: 10
  - Available Recipes: TestExtractorRecipe

- **TestProcessorData**: Configuration for a test processor machine.
  - Grid Size: 1x1
  - Tier: 1
  - Input Ports: 1 (left side)
  - Output Ports: 1 (right side)
  - Buffer Capacity: 10
  - Available Recipes: TestProcessorRecipe

## Creating Test Assets

To create or recreate these test assets:

1. Open Unity Editor
2. Go to menu: **Santa → Create Test Assets → All Test Assets**

This will create all test recipes and machine data assets in the appropriate folders.

## Cleaning Test Assets

To remove all test assets:

1. Open Unity Editor
2. Go to menu: **Santa → Create Test Assets → Clean Test Assets**

This will delete the entire TestAssets folder and its contents.

## Usage in Tests

These assets are designed to be used in unit tests and integration tests for the Machine Framework:

```csharp
// Example usage in tests
[Test]
public void TestMachine_WithExtractorRecipe_ProducesOutput()
{
    // Load test assets
    var extractorData = AssetDatabase.LoadAssetAtPath<MachineData>(
        "Assets/_Project/ScriptableObjects/TestAssets/Machines/TestExtractorData.asset");
    
    // Create test machine
    var machine = CreateTestMachine(extractorData);
    
    // Test machine behavior
    // ...
}
```

## Port Configuration

### TestExtractor Port Layout
```
┌─────────┐
│         │
│  TEST   ├──► Output (right)
│  EXTR   │
│         │
└─────────┘
```

### TestProcessor Port Layout
```
┌─────────┐
│         │
◄──┤  TEST   ├──► Output (right)
│  PROC   │
│         │
└─────────┘
   Input (left)
```

## Notes

- These assets are for testing purposes only and should not be used in production gameplay.
- The recipes use simple resource IDs ("iron_ore", "iron_ingot") that should match the resource definitions in the Resource System.
- Port positions are configured for standard left-to-right flow (input on left, output on right).
- Buffer capacity is set to 10 for both machines, which is sufficient for testing basic functionality.

## Requirements Covered

This implementation covers the following requirements from the Machine Framework specification:

- **Requirement 2**: Recipe data structures with inputs, outputs, processing time, and power consumption
- **Requirement 3**: Machine data with port configurations
- **Requirement 14**: ScriptableObject-based configuration for designers

## Related Files

- Recipe Script: `Assets/_Project/Scripts/Data/Recipe.cs`
- MachineData Script: `Assets/_Project/Scripts/Data/MachineData.cs`
- Editor Script: `Assets/_Project/Scripts/Editor/CreateTestAssets.cs`
- Test Implementations: `Assets/_Project/Scripts/Machines/TestExtractor.cs`, `TestProcessor.cs`
