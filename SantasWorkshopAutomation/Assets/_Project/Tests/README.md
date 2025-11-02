# Santa's Workshop Automation - Unit Tests

This directory contains unit tests for the core systems of Santa's Workshop Automation.

## Test Framework Setup

The tests use Unity Test Framework (UTF) with NUnit. The test assembly is configured to run in the Unity Editor.

### Assembly Definitions

The following assembly definitions have been created:

- **SantasWorkshop.Tests.asmdef**: Test assembly that references core game assemblies
- **SantasWorkshop.Core.asmdef**: Core systems (ResourceManager, GameManager)
- **SantasWorkshop.Machines.asmdef**: Machine framework (MachineBase, IMachine)
- **SantasWorkshop.Data.asmdef**: Data structures (ResourceStack, MachineData)
- **SantasWorkshop.Utilities.asmdef**: Utility classes (Singleton)

## Test Files

### ResourceManagerTests.cs

Tests for the ResourceManager singleton that handles all resource tracking, addition, and consumption.

**Test Coverage:**
- ✅ AddResource increases count correctly
- ✅ AddResources with array works properly
- ✅ Multiple additions accumulate correctly
- ✅ TryConsumeResources with sufficient resources returns true
- ✅ TryConsumeResources with insufficient resources returns false
- ✅ TryConsumeResources with multiple resources uses all-or-nothing logic
- ✅ HasResource returns correct values
- ✅ GetResourceCount returns correct values
- ✅ SetResourceCount sets values correctly
- ✅ Query methods (GetResourceTypeCount, GetAllResourceCounts) work
- ✅ ClearAllResources removes all resources
- ✅ LoadResourceCounts loads correctly

**Total Tests:** 25 tests covering all core ResourceManager functionality

### MachineBaseTests.cs

Tests for the MachineBase abstract class that provides common functionality for all machines.

**Test Coverage:**
- ✅ Initialize sets correct state (Idle)
- ✅ Initialize sets machine ID and power consumption
- ✅ Shutdown changes state to Offline
- ✅ Shutdown sets powered to false
- ✅ SetPowered changes power state correctly
- ✅ SetPowered(false) while Working changes to Idle
- ✅ Power state changes trigger OnPowerChanged
- ✅ State changes trigger OnStateChanged
- ✅ Tick is called and receives delta time
- ✅ Full lifecycle (Initialize → Tick → Shutdown) works

**Total Tests:** 20 tests covering machine lifecycle, state management, and power handling

## Running Tests

### In Unity Editor

1. Open Unity Test Runner: `Window → General → Test Runner`
2. Select the "EditMode" tab
3. Click "Run All" to run all tests
4. Click individual tests to run specific test cases

### Command Line

```powershell
# Run all tests
"C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe" `
  -runTests -batchmode -projectPath . `
  -testResults TestResults.xml `
  -testPlatform EditMode

# View results
Get-Content TestResults.xml
```

## Test Structure

Each test file follows this structure:

```csharp
[SetUp]
public void Setup()
{
    // Create test objects before each test
}

[TearDown]
public void Teardown()
{
    // Clean up test objects after each test
}

[Test]
public void TestName_Scenario_ExpectedResult()
{
    // Arrange: Set up test conditions
    // Act: Execute the code being tested
    // Assert: Verify the results
}
```

## Best Practices

1. **Isolation**: Each test is independent and doesn't rely on other tests
2. **Cleanup**: All test objects are destroyed in TearDown to prevent memory leaks
3. **Naming**: Tests use descriptive names following the pattern `MethodName_Scenario_ExpectedResult`
4. **Assertions**: Each test has clear assertions with descriptive messages
5. **Coverage**: Tests cover both success and failure cases

## Adding New Tests

To add tests for a new system:

1. Create a new test file in this directory (e.g., `PowerGridTests.cs`)
2. Add the namespace `SantasWorkshop.Tests`
3. Follow the existing test structure with SetUp/TearDown
4. Write tests following the Arrange-Act-Assert pattern
5. Run tests in Unity Test Runner to verify

## Continuous Integration

These tests can be integrated into a CI/CD pipeline:

```yaml
# Example GitHub Actions workflow
- name: Run Unity Tests
  uses: game-ci/unity-test-runner@v2
  with:
    projectPath: SantasWorkshopAutomation
    testMode: EditMode
```

## Test Results

All tests should pass before committing code. If tests fail:

1. Check the error message in Unity Test Runner
2. Review the test code and implementation
3. Fix the issue in the implementation or test
4. Re-run tests to verify the fix

## Notes

- Tests run in EditMode (Unity Editor) for fast iteration
- PlayMode tests can be added later for integration testing
- Mock objects are avoided in favor of testing real implementations
- Tests focus on core functional logic, not edge cases
