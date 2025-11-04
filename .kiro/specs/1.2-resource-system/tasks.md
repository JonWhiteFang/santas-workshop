# Implementation Plan: Resource System

## Overview

This implementation plan breaks down the Resource System into discrete, manageable coding tasks. Each task builds incrementally on previous work, ensuring the system can be tested and validated at each step.

---

## Task List

- [x] 1. Create resource data structures and enums





  - Create ResourceCategory enum with all seven categories (RawMaterial, Refined, Component, ToyPart, FinishedToy, Magic, Energy)
  - Create ResourceStack struct with resourceId, amount fields, IsValid() method, and ToString() override
  - Add XML documentation comments for all public members
  - Place in SantasWorkshop.Data namespace
  - _Requirements: 1.1, 1.2, 1.3_

- [x] 2. Create ResourceData ScriptableObject



  - Create ResourceData class inheriting from ScriptableObject
  - Add all required fields: resourceId, displayName, description, icon, category, stackSize, weight, itemPrefab, itemColor, baseValue, canBeStored, canBeTransported
  - Add [CreateAssetMenu] attribute with path "Santa/Resource Data"
  - Implement OnValidate() method to check resourceId is not empty and stackSize > 0
  - Add [Header], [Tooltip], [Range], and [TextArea] attributes for better Inspector UX
  - Place in SantasWorkshop.Data namespace
  - _Requirements: 1.1, 1.4, 1.5_

- [x] 3. Create ResourceManager singleton class structure



  - Create ResourceManager class inheriting from MonoBehaviour
  - Implement singleton pattern with Instance property and Awake() lifecycle
  - Add DontDestroyOnLoad() to persist across scenes
  - Implement OnDestroy() to clean up Instance reference
  - Add private dictionaries: _resourceDatabase, _globalResourceCounts, _resourceCapacities
  - Add _isInitialized flag
  - Place in SantasWorkshop.Core namespace
  - _Requirements: 2.1, 2.2, 2.4, 2.5_

- [x] 4. Implement resource database initialization




  - Create Initialize() method that loads ResourceData assets from Resources/ResourceDefinitions folder
  - Use Resources.LoadAll<ResourceData>() to find all resource definitions
  - Register each ResourceData in _resourceDatabase dictionary indexed by resourceId
  - Initialize _globalResourceCounts to zero for each registered resource
  - Add duplicate resourceId detection with error logging
  - Add OnResourceSystemInitialized event and invoke after initialization
  - Call Initialize() from Start() lifecycle method
  - _Requirements: 2.2, 2.3, 13.1, 13.2, 13.3, 13.4, 13.5_

- [x] 5. Implement AddResource and AddResources methods





  - Create AddResource(string resourceId, int amount) method
  - Validate resourceId exists and amount is positive using ValidateResourceOperation()
  - Check capacity limits using GetResourceCapacity()
  - Update _globalResourceCounts dictionary
  - Invoke OnResourceChanged event with resourceId and new count
  - Handle capacity overflow by adding only up to limit
  - Create AddResources(ResourceStack[] resources) method that calls AddResource() for each stack
  - Add null/empty array validation
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 7.1, 7.2, 7.3, 7.4, 7.5, 14.3_

- [ ] 6. Implement TryConsumeResource and TryConsumeResources methods
  - Create TryConsumeResource(string resourceId, int amount) method
  - Validate resourceId exists and amount is positive
  - Check if sufficient resources available using GetResourceCount()
  - If sufficient, decrease _globalResourceCounts and invoke OnResourceChanged event
  - Return true on success, false on failure (no error logging for insufficient resources)
  - Create TryConsumeResources(ResourceStack[] resources) method
  - Validate ALL resources are available before consuming ANY (atomic operation)
  - If all available, call TryConsumeResource() for each stack
  - Return true only if all consumptions succeed
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 6.1, 6.2, 6.3, 6.4, 6.5_

- [ ] 7. Implement resource query methods
  - Create HasResource(string resourceId, int amount) method that checks if count >= amount
  - Return false for invalid resourceId or negative amount
  - Create GetResourceCount(string resourceId) method that returns current count from _globalResourceCounts
  - Return 0 for invalid or unknown resourceId
  - Create GetResourceData(string resourceId) method that returns ResourceData from _resourceDatabase
  - Return null for invalid or unknown resourceId
  - Create GetAllResources() method returning IEnumerable<ResourceData> of all registered resources
  - Create GetResourcesByCategory(ResourceCategory category) method using LINQ Where() filter
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 8.1, 8.2, 8.3, 8.4, 8.5_

- [ ] 8. Implement resource validation methods
  - Create ValidateResourceStack(ResourceStack stack) method
  - Check resourceId is not null/empty, amount > 0, and resourceId exists in database
  - Return true only if all checks pass
  - Create ValidateResourceStacks(ResourceStack[] stacks) method
  - Check array is not null/empty
  - Call ValidateResourceStack() for each stack
  - Return true only if all stacks are valid
  - Create private ValidateResourceOperation(string resourceId, int amount) helper method
  - Check resourceId not empty, amount >= 0, and resourceId exists in database
  - Log warnings for validation failures
  - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5_

- [ ] 9. Implement capacity management methods
  - Create SetResourceCapacity(string resourceId, long capacity) method
  - Validate resourceId exists in database
  - Store capacity in _resourceCapacities dictionary
  - Log warning for invalid resourceId
  - Create GetResourceCapacity(string resourceId) method
  - Return capacity from _resourceCapacities or 0 if not set (unlimited)
  - Return 0 for invalid resourceId
  - _Requirements: 14.1, 14.2, 14.4, 14.5_

- [ ] 10. Implement resource transfer method
  - Create TransferResource(string sourceId, string targetId, string resourceId, int amount) method
  - For initial implementation, operate on global inventory (sourceId/targetId reserved for future use)
  - Validate resource availability using HasResource()
  - Call TryConsumeResource() to remove from source
  - Call AddResource() to add to target
  - Return true on success, false on failure
  - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5_

- [ ] 11. Implement reset methods
  - Create ResetResources() method
  - Find all resources with non-zero counts using LINQ Where()
  - Set each count to zero in _globalResourceCounts
  - Invoke OnResourceChanged event for each reset resource
  - Preserve _resourceDatabase registration
  - Create ResetToDefaults() method
  - Clear all dictionaries (_resourceDatabase, _globalResourceCounts, _resourceCapacities)
  - Set _isInitialized to false
  - Call Initialize() to reload resource database
  - _Requirements: 12.1, 12.2, 12.3, 12.4, 12.5_

- [ ] 12. Implement save/load support
  - Create ResourceSaveData struct with ResourceEntry[] array field
  - Create ResourceEntry struct with resourceId string and amount long fields
  - Mark both structs with [Serializable] attribute
  - Create GetSaveData() method that returns ResourceSaveData
  - Iterate _globalResourceCounts and include only non-zero counts in save data
  - Create LoadSaveData(ResourceSaveData saveData) method
  - Reset all counts to zero first
  - Restore counts from save data for each entry
  - Validate resourceId exists in database before restoring
  - Invoke OnResourceChanged events for all restored resources
  - Log warning for unknown resourceIds in save data
  - _Requirements: 15.1, 15.2, 15.3, 15.4, 15.5_

- [ ] 13. Create sample ResourceData assets
  - Create Resources/ResourceDefinitions folder in Assets/_Project
  - Create 5-10 sample ResourceData assets covering different categories:
    - RawMaterial: Wood, IronOre, Coal
    - Refined: WoodPlank, IronIngot
    - Component: IronGear, Paint
    - FinishedToy: WoodenTrain
  - Set unique resourceId for each (e.g., "wood", "iron_ore", "wood_plank")
  - Set appropriate displayName, category, stackSize, and baseValue
  - Leave icon and itemPrefab null for now (will be added in art phase)
  - _Requirements: 1.1, 2.2_

- [ ] 14. Create ResourceManager GameObject in Workshop scene
  - Open Workshop.unity scene
  - Create empty GameObject named "ResourceManager"
  - Add ResourceManager component
  - Position at (0, 0, 0)
  - Save scene
  - _Requirements: 2.1_

- [ ] 15. Create test scene for resource system
  - Create TestScenes/TestScene_ResourceSystem.unity
  - Add ResourceManager GameObject
  - Add test UI canvas with buttons for Add/Consume/Query operations
  - Add text display for resource counts
  - Create simple test script to trigger ResourceManager methods
  - _Requirements: Testing Strategy_

- [ ] 16. Write unit tests for ResourceManager
  - Create Tests/Runtime/ResourceManagerTests.cs
  - Write test for AddResource increases count
  - Write test for TryConsumeResource with sufficient resources returns true
  - Write test for TryConsumeResource with insufficient resources returns false
  - Write test for TryConsumeResources atomic behavior (all-or-nothing)
  - Write test for SetResourceCapacity limits additions
  - Write test for OnResourceChanged event fires correctly
  - Write test for HasResource validation
  - Write test for GetResourceData returns correct data
  - Write test for ResetResources clears all counts
  - Write test for save/load preserves resource counts
  - _Requirements: Testing Strategy_

- [ ] 17. Integration testing and validation
  - Run all unit tests and verify they pass
  - Open TestScene_ResourceSystem and manually test operations
  - Verify ResourceManager initializes correctly on scene load
  - Test adding resources updates counts correctly
  - Test consuming resources validates availability
  - Test capacity limits work as expected
  - Test events fire when resources change
  - Test save/load preserves state correctly
  - Check for any errors or warnings in console
  - Profile performance with 1000+ operations
  - _Requirements: All requirements_

---

## Implementation Notes

### Namespace Organization
All code should use appropriate namespaces:
- `SantasWorkshop.Data` for ResourceData, ResourceStack, enums
- `SantasWorkshop.Core` for ResourceManager

### Coding Standards
- Use PascalCase for public members
- Use _camelCase for private fields
- Add XML documentation comments for all public APIs
- Use [SerializeField] for private fields that need Inspector visibility
- Follow Unity lifecycle method order: Awake, Start, Update, OnDestroy

### Testing Approach
- Unit tests focus on core logic in isolation
- Integration tests verify system interactions
- Manual testing validates user-facing behavior
- Performance tests ensure scalability

### Dependencies
- Task 1-2: No dependencies (data structures)
- Task 3-4: Depends on Task 1-2 (needs data structures)
- Task 5-12: Depends on Task 3-4 (needs ResourceManager structure)
- Task 13-14: Can be done in parallel with Task 5-12
- Task 15-17: Depends on all previous tasks (testing)

### Estimated Effort
- Tasks 1-4: 2-3 hours (foundation)
- Tasks 5-12: 4-6 hours (core functionality)
- Tasks 13-14: 1 hour (sample data and scene setup)
- Tasks 15-17: 2-3 hours (testing and validation)
- **Total: 9-13 hours** (approximately 2 days)

---

## Success Criteria

The Resource System implementation is complete when:

✅ All 17 tasks are marked complete
✅ All unit tests pass
✅ ResourceManager initializes without errors
✅ Resources can be added, consumed, and queried correctly
✅ Events fire when resources change
✅ Capacity limits work as expected
✅ Save/load preserves resource state
✅ No errors or warnings in console during normal operation
✅ Performance is acceptable (1000+ operations per second)
✅ Code follows project coding standards and conventions

---

## Next Steps After Completion

Once the Resource System is complete, the following systems can be built:

1. **Grid & Placement System (Task 1.3)**: Can be developed in parallel
2. **Machine Framework (Task 1.4)**: Depends on Resource System for input/output
3. **Time & Simulation Manager (Task 1.5)**: Can be developed in parallel
4. **Conveyor Belt System (Task 2.1)**: Depends on Resource System for item transport

The Resource System is a foundational component that many other systems will depend on, so thorough testing and validation is critical before proceeding.
