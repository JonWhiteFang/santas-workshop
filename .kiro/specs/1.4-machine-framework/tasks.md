# Implementation Plan: Machine Framework

This document outlines the implementation tasks for the Machine Framework system. Each task builds incrementally on previous work, ensuring a functional system at each step.

## Task List

- [x] 1. Create core data structures and enums


  - Create MachineState enum with all six states
  - Create IPowerConsumer interface with PowerConsumption, IsPowered properties and SetPowered method
  - Create ResourceStack struct if not already present from Resource System
  - _Requirements: 1, 2, 5_

- [x] 2. Implement Recipe ScriptableObject


  - Create Recipe class inheriting from ScriptableObject
  - Add fields for recipeId, recipeName, inputs, outputs, processingTime, powerConsumption, requiredTier
  - Implement OnValidate method to check for empty inputs/outputs
  - Create CreateAssetMenu attribute for designer workflow
  - _Requirements: 4, 14_

- [x] 3. Implement MachineData ScriptableObject


  - Create MachineData class inheriting from ScriptableObject
  - Add fields for machineName, description, icon, gridSize, tier, baseProcessingSpeed, basePowerConsumption
  - Add port configuration fields (inputPortCount, outputPortCount, port positions, bufferCapacity)
  - Add availableRecipes list and prefab reference
  - Implement OnValidate to initialize port position arrays
  - _Requirements: 14, 16_

- [x] 4. Implement InputPort class


  - Create InputPort class with portId, localPosition, capacity fields
  - Implement buffer storage using Dictionary<string, int>
  - Add CanAcceptResource method checking capacity
  - Add AddResource method with capacity validation
  - Add RemoveResource method returning amount actually removed
  - Add GetResourceAmount, GetTotalAmount, GetAvailableSpace query methods
  - Implement GetSaveData and LoadSaveData methods
  - _Requirements: 3, 6, 15_

- [x] 5. Implement OutputPort class



  - Create OutputPort class with portId, localPosition, capacity fields
  - Implement buffer storage using Dictionary<string, int>
  - Add CanAddResource method checking capacity
  - Add AddResource method with capacity validation
  - Add ExtractResource method for removing resources
  - Add GetResourceAmount, GetTotalAmount, GetAvailableSpace, HasResources query methods
  - Implement GetSaveData and LoadSaveData methods
  - _Requirements: 3, 7, 15_

- [ ] 6. Create save data structures
  - Create MachineSaveData struct with all necessary fields (machineId, machineType, tier, gridPosition, rotation, currentState, processingProgress, activeRecipeId, buffers, isEnabled)
  - Create BufferSaveData struct with portId and contents dictionary
  - Mark structs as Serializable
  - _Requirements: 15_

- [ ] 7. Implement MachineBase abstract class - Part 1: Core structure
  - Create MachineBase abstract class inheriting from MonoBehaviour and implementing IPowerConsumer
  - Add serialized fields for machineId and machineData
  - Add protected fields for grid integration (gridPosition, gridSize, rotation)
  - Add protected fields for state management (currentState, previousState)
  - Add protected lists for inputPorts and outputPorts
  - Add protected fields for recipe processing (activeRecipe, processingProgress, processingTimeRemaining)
  - Add protected fields for power (isPowered, powerConsumption)
  - Add protected fields for tier and multipliers (tier, speedMultiplier, efficiencyMultiplier)
  - Add isEnabled field
  - _Requirements: 1, 10, 12, 13_

- [ ] 8. Implement MachineBase - Part 2: Events and properties
  - Add event declarations (OnStateChanged, OnProcessingStarted, OnProcessingCompleted, OnPowerStatusChanged)
  - Implement public properties (MachineId, CurrentState, ProcessingProgress, EstimatedTimeRemaining, Tier, GridPosition, GridSize, Rotation)
  - Implement IPowerConsumer properties (PowerConsumption with efficiency calculation, IsPowered)
  - _Requirements: 1, 5, 9, 11_

- [ ] 9. Implement MachineBase - Part 3: Lifecycle methods
  - Implement Awake method (generate machineId if empty, call InitializeFromData)
  - Implement Start method (call RegisterWithPowerGrid, ValidateConfiguration)
  - Implement Update method (check isEnabled, call UpdateStateMachine)
  - Implement OnDestroy method (call UnregisterFromPowerGrid, FreeGridCells)
  - _Requirements: 1, 5, 12, 16_

- [ ] 10. Implement MachineBase - Part 4: Initialization
  - Implement InitializeFromData method (load from machineData, set tier, gridSize, powerConsumption, call CalculateMultipliers and InitializePorts)
  - Implement InitializePorts method (create InputPort and OutputPort instances from machineData configuration)
  - Implement CalculateMultipliers method (calculate speedMultiplier and efficiencyMultiplier based on tier)
  - _Requirements: 1, 10, 14_

- [ ] 11. Implement MachineBase - Part 5: State machine core
  - Implement TransitionToState method (call OnStateExit, change state, call OnStateEnter, fire OnStateChanged event, log transition)
  - Implement OnStateEnter method with switch statement calling specific enter methods
  - Implement OnStateExit method with switch statement calling specific exit methods
  - Implement UpdateStateMachine method (check power status, update current state)
  - Add virtual methods for state-specific behavior (OnEnterIdle, OnEnterWaitingForInput, OnEnterProcessing, OnEnterWaitingForOutput, OnEnterNoPower, OnEnterDisabled, OnExitProcessing)
  - _Requirements: 2, 5_

- [ ] 12. Implement MachineBase - Part 6: State update methods
  - Implement UpdateIdle method (check if can process, transition to Processing or WaitingForInput)
  - Implement UpdateWaitingForInput method (check if inputs available, transition to Processing)
  - Implement UpdateProcessing method (update progress, check completion, call CompleteProcessing)
  - Implement UpdateWaitingForOutput method (check if output space available, transition to Idle)
  - Implement OnEnterProcessing method (initialize progress, set time remaining, fire OnProcessingStarted event)
  - Implement OnExitProcessing method (reset progress and time remaining)
  - _Requirements: 2, 9_

- [ ] 13. Implement MachineBase - Part 7: Recipe processing
  - Implement SetActiveRecipe method (validate recipe, cancel current processing if needed, set activeRecipe, update powerConsumption, transition to Idle)
  - Implement CanProcessRecipe method (check power, enabled state, inputs, output space)
  - Implement HasRequiredInputs method (check all recipe inputs are in buffers)
  - Implement HasOutputSpace method (check all recipe outputs can fit in buffers)
  - Implement CompleteProcessing method (consume inputs, produce outputs, fire OnProcessingCompleted, check if can continue)
  - Implement CancelProcessing method (reset progress)
  - Implement ConsumeInputs and ProduceOutputs methods
  - Implement IsRecipeAvailable and GetAvailableRecipes methods
  - _Requirements: 4, 8, 18_

- [ ] 14. Implement MachineBase - Part 8: Buffer management
  - Implement GetInputBufferAmount method (sum across all input ports)
  - Implement RemoveFromInputBuffer method (remove from ports until amount satisfied)
  - Implement AddToOutputBuffer method (add to first available output port)
  - Implement CanAddToOutputBuffer method (check if any output port has space)
  - _Requirements: 6, 7_

- [ ] 15. Implement MachineBase - Part 9: Power management
  - Implement SetPowered method (update isPowered, fire OnPowerStatusChanged, transition to/from NoPower state)
  - Implement RegisterWithPowerGrid method (placeholder for future PowerGrid integration)
  - Implement UnregisterFromPowerGrid method (placeholder for future PowerGrid integration)
  - _Requirements: 5_

- [ ] 16. Implement MachineBase - Part 10: Grid integration
  - Implement SetGridPosition method (store gridPosition)
  - Implement SetRotation method (clamp rotation 0-3, call UpdateVisualRotation)
  - Implement GetOccupiedCells method (return list of all cells based on gridPosition and gridSize)
  - Implement FreeGridCells method (call GridManager to free cells)
  - Implement UpdateVisualRotation method (set transform rotation based on rotation value)
  - _Requirements: 12, 13_

- [ ] 17. Implement MachineBase - Part 11: Enable/Disable
  - Implement SetEnabled method (update isEnabled, transition to/from Disabled state)
  - _Requirements: 20_

- [ ] 18. Implement MachineBase - Part 12: Visual feedback
  - Implement SetVisualState method (virtual method for derived classes to override)
  - Add placeholder for visual indicator updates
  - _Requirements: 17_

- [ ] 19. Implement MachineBase - Part 13: Validation
  - Implement ValidateConfiguration method (check machineData not null, ports exist, active recipe valid)
  - Log warnings for invalid configurations
  - _Requirements: 16, 19_

- [ ] 20. Implement MachineBase - Part 14: Save/Load
  - Implement GetSaveData method (create MachineSaveData with all current state)
  - Implement LoadSaveData method (restore all state from MachineSaveData)
  - Implement GetInputBufferSaveData and GetOutputBufferSaveData helper methods
  - Implement LoadInputBufferSaveData and LoadOutputBufferSaveData helper methods
  - _Requirements: 15_

- [ ] 21. Create test machine implementations
  - Create TestExtractor class inheriting from MachineBase for testing
  - Create TestProcessor class inheriting from MachineBase for testing
  - Override necessary virtual methods for basic functionality
  - _Requirements: All_

- [ ] 22. Create test ScriptableObject assets
  - Create test Recipe assets (simple 1-input, 1-output recipes)
  - Create test MachineData assets for TestExtractor and TestProcessor
  - Configure port positions and buffer capacities
  - _Requirements: 2, 3, 14_

- [ ] 23. Create test scene
  - Create new test scene for Machine Framework
  - Add GridManager to scene (from Grid System spec)
  - Add ResourceManager to scene (from Resource System spec)
  - Add test machine instances
  - Configure test setup for manual testing
  - _Requirements: All_

- [ ] 24. Write unit tests for InputPort and OutputPort
  - Test AddResource with valid and invalid amounts
  - Test RemoveResource and ExtractResource
  - Test capacity limits
  - Test GetResourceAmount and GetTotalAmount
  - Test save/load functionality
  - _Requirements: 3, 6, 7_

- [ ] 25. Write unit tests for MachineBase state machine
  - Test TransitionToState changes state correctly
  - Test OnStateChanged event fires
  - Test state transition validation
  - Test power loss transitions to NoPower
  - Test power restore returns to previous state
  - _Requirements: 2, 5_

- [ ] 26. Write unit tests for recipe processing
  - Test CanProcessRecipe with sufficient/insufficient inputs
  - Test CompleteProcessing consumes inputs and produces outputs
  - Test SetActiveRecipe switches recipes correctly
  - Test processing progress tracking
  - Test recipe validation
  - _Requirements: 4, 8, 9, 18_

- [ ] 27. Write integration tests
  - Test full processing cycle (add inputs → process → extract outputs)
  - Test multi-recipe machine switching
  - Test save/load preserves machine state
  - Test grid integration (placement, rotation, cell occupation)
  - Test enable/disable functionality
  - _Requirements: All_

- [ ] 28. Performance testing
  - Test 100 machines updating per frame
  - Measure state machine update time
  - Measure buffer operation time
  - Verify performance targets (<0.1ms per machine)
  - _Requirements: All_

- [ ] 29. Create example machine prefabs
  - Create visual prefabs for TestExtractor and TestProcessor
  - Add basic 3D models or placeholder cubes
  - Add visual indicators for state (lights, particles)
  - Configure prefab references in MachineData assets
  - _Requirements: 17_

- [ ] 30. Integration with existing systems
  - Verify integration with ResourceManager for resource operations
  - Verify integration with GridManager for placement and cell occupation
  - Test machine placement through PlacementController (from Grid System spec)
  - Ensure machines register/unregister correctly
  - _Requirements: 12, 15_

- [ ] 31. Documentation and examples
  - Create example derived machine classes showing proper inheritance
  - Document virtual method override patterns
  - Create designer guide for creating MachineData assets
  - Document recipe creation workflow
  - _Requirements: All_

- [ ] 32. Final validation and cleanup
  - Run all tests and verify they pass
  - Check for memory leaks (event subscriptions)
  - Verify no null reference exceptions
  - Test edge cases (empty buffers, null recipes, etc.)
  - Clean up debug logs
  - _Requirements: All_
