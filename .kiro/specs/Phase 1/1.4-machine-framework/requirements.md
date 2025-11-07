# Requirements Document: Machine Framework

## Introduction

This specification defines the Machine Framework for Santa's Workshop Automation, which provides the foundational architecture for all machines in the game including extractors, processors, assemblers, and utility buildings. The framework establishes a consistent state machine pattern, input/output port system, recipe processing, and power consumption interface that all machine types will inherit and extend.

## Glossary

- **Machine Framework**: The base architecture and abstract classes that define common machine behavior
- **Machine**: Any building that processes resources, generates power, or performs automated tasks
- **Machine State**: The current operational status of a machine (Idle, WaitingForInput, Processing, WaitingForOutput, NoPower, Disabled)
- **Input Port**: A connection point where a machine receives resources from conveyors or other machines
- **Output Port**: A connection point where a machine sends produced resources to conveyors or other machines
- **Recipe**: A data structure defining input resources, output resources, processing time, and power consumption
- **Processing**: The active state where a machine converts input resources into output resources
- **Power Consumer**: A machine that requires electrical power to operate
- **Power Interface**: The standardized connection between machines and the power grid system
- **Machine Base**: The abstract base class that all machines inherit from
- **State Machine**: The pattern used to manage machine behavior transitions
- **Input Buffer**: Temporary storage for resources waiting to be processed
- **Output Buffer**: Temporary storage for produced resources waiting to be transported
- **Machine Tier**: The upgrade level of a machine affecting speed, efficiency, and capabilities

## Requirements

### Requirement 1

**User Story:** As a developer, I want a base machine class with common functionality, so that all machines share consistent behavior and interfaces.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide an abstract MachineBase class that all machines inherit from
2. THE MachineBase class SHALL include fields for machineId, machineName, tier, gridPosition, and gridSize
3. THE MachineBase class SHALL provide virtual methods for Initialize, Update, OnStateEnter, OnStateExit, and OnDestroy
4. THE MachineBase class SHALL maintain a reference to the current MachineState
5. THE MachineBase class SHALL provide a TransitionToState method that handles state changes with proper enter/exit callbacks

### Requirement 2

**User Story:** As a developer, I want machines to use a state machine pattern, so that machine behavior is predictable and maintainable.

#### Acceptance Criteria

1. THE Machine Framework SHALL define a MachineState enum with values Idle, WaitingForInput, Processing, WaitingForOutput, NoPower, and Disabled
2. WHEN a machine transitions to a new state, THE Machine Framework SHALL invoke OnStateExit for the current state before changing
3. WHEN a machine transitions to a new state, THE Machine Framework SHALL invoke OnStateEnter for the new state after changing
4. THE Machine Framework SHALL prevent invalid state transitions by validating the target state
5. THE Machine Framework SHALL log state transitions for debugging purposes

### Requirement 3

**User Story:** As a developer, I want machines to have input and output ports, so that resources can flow between machines and conveyors.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide an InputPort class with fields for portId, position, acceptedResources, and currentBuffer
2. THE Machine Framework SHALL provide an OutputPort class with fields for portId, position, outputResource, and currentBuffer
3. WHEN a resource is delivered to an InputPort, THE Machine Framework SHALL add it to the port's buffer if space is available
4. WHEN a machine produces output, THE Machine Framework SHALL add it to the OutputPort buffer
5. THE Machine Framework SHALL provide methods to query port availability and buffer contents

### Requirement 4

**User Story:** As a developer, I want machines to process recipes, so that they can convert input resources into output resources.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide a Recipe class with fields for recipeId, inputs, outputs, processingTime, and powerConsumption
2. WHEN a machine starts processing, THE Machine Framework SHALL validate that all required input resources are available
3. WHILE processing, THE Machine Framework SHALL track progress as a percentage from 0 to 100
4. WHEN processing completes, THE Machine Framework SHALL consume input resources and produce output resources
5. THE Machine Framework SHALL support machines with multiple available recipes

### Requirement 5

**User Story:** As a developer, I want machines to consume power, so that the power grid system can manage electricity distribution.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide an IPowerConsumer interface with properties for PowerConsumption and IsPowered
2. THE MachineBase class SHALL implement IPowerConsumer
3. WHEN a machine is processing, THE Machine Framework SHALL report its power consumption to the power grid
4. WHEN a machine loses power, THE Machine Framework SHALL transition to the NoPower state
5. WHEN a machine regains power, THE Machine Framework SHALL transition back to its previous operational state

### Requirement 6

**User Story:** As a developer, I want machines to have input buffers, so that they can store resources waiting to be processed.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide an InputBuffer class with a configurable capacity
2. WHEN resources are added to an InputBuffer, THE Machine Framework SHALL check if capacity allows the addition
3. WHEN an InputBuffer is full, THE Machine Framework SHALL reject additional resource additions
4. THE Machine Framework SHALL provide methods to query buffer contents and available space
5. WHEN a machine consumes resources, THE Machine Framework SHALL remove them from the InputBuffer

### Requirement 7

**User Story:** As a developer, I want machines to have output buffers, so that they can store produced resources waiting for transport.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide an OutputBuffer class with a configurable capacity
2. WHEN a machine produces output, THE Machine Framework SHALL add it to the OutputBuffer if space is available
3. WHEN an OutputBuffer is full, THE Machine Framework SHALL transition the machine to WaitingForOutput state
4. THE Machine Framework SHALL provide methods to extract resources from the OutputBuffer
5. WHEN resources are extracted from an OutputBuffer, THE Machine Framework SHALL transition the machine back to operational state if it was waiting

### Requirement 8

**User Story:** As a developer, I want machines to validate recipe requirements, so that they only process when inputs are available.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide a CanProcessRecipe method that checks input availability
2. WHEN CanProcessRecipe is called, THE Machine Framework SHALL verify all required input resources are in the InputBuffer
3. WHEN CanProcessRecipe is called, THE Machine Framework SHALL verify the OutputBuffer has space for all outputs
4. WHEN CanProcessRecipe is called, THE Machine Framework SHALL verify the machine has sufficient power
5. THE Machine Framework SHALL return false from CanProcessRecipe if any requirement is not met

### Requirement 9

**User Story:** As a developer, I want machines to track processing progress, so that UI can display progress bars and time estimates.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide a ProcessingProgress property returning a float from 0.0 to 1.0
2. WHILE in Processing state, THE Machine Framework SHALL update ProcessingProgress based on elapsed time and recipe processing time
3. WHEN ProcessingProgress reaches 1.0, THE Machine Framework SHALL complete the recipe and produce outputs
4. THE Machine Framework SHALL reset ProcessingProgress to 0.0 when starting a new recipe
5. THE Machine Framework SHALL provide an EstimatedTimeRemaining property in seconds

### Requirement 10

**User Story:** As a developer, I want machines to support multiple tiers, so that players can upgrade machines for better performance.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide a Tier property with integer values starting from 1
2. THE Machine Framework SHALL provide a SpeedMultiplier property that scales with tier
3. WHEN calculating processing time, THE Machine Framework SHALL apply the SpeedMultiplier
4. THE Machine Framework SHALL provide an EfficiencyMultiplier property that reduces power consumption at higher tiers
5. THE Machine Framework SHALL support tier-specific visual variations through prefab variants

### Requirement 11

**User Story:** As a developer, I want machines to emit events, so that other systems can react to machine state changes.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide an OnStateChanged event with parameters for oldState and newState
2. THE Machine Framework SHALL provide an OnProcessingStarted event with the current recipe
3. THE Machine Framework SHALL provide an OnProcessingCompleted event with produced outputs
4. THE Machine Framework SHALL provide an OnPowerStatusChanged event with the new power status
5. THE Machine Framework SHALL invoke events after state changes are complete

### Requirement 12

**User Story:** As a developer, I want machines to integrate with the grid system, so that they occupy grid cells correctly.

#### Acceptance Criteria

1. THE MachineBase class SHALL store its GridPosition as a Vector3Int
2. THE MachineBase class SHALL store its GridSize as a Vector2Int
3. WHEN a machine is placed, THE Machine Framework SHALL register its occupied cells with the GridManager
4. WHEN a machine is destroyed, THE Machine Framework SHALL free its occupied cells in the GridManager
5. THE Machine Framework SHALL provide a GetOccupiedCells method returning all cells the machine occupies

### Requirement 13

**User Story:** As a developer, I want machines to support rotation, so that players can orient machines correctly.

#### Acceptance Criteria

1. THE MachineBase class SHALL store a Rotation property as an integer (0-3 for 0°, 90°, 180°, 270°)
2. THE Machine Framework SHALL provide a GetRotatedPortPosition method that calculates port positions based on rotation
3. WHEN rotation changes, THE Machine Framework SHALL update visual representation to match
4. THE Machine Framework SHALL adjust input/output port positions based on rotation
5. THE Machine Framework SHALL support rotation-specific grid cell occupation patterns

### Requirement 14

**User Story:** As a developer, I want machines to have configurable properties, so that designers can balance machine stats without code changes.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide a MachineData ScriptableObject with fields for machineName, tier, baseProcessingSpeed, basePowerConsumption, inputPortCount, outputPortCount, and bufferCapacity
2. THE MachineBase class SHALL initialize from a MachineData asset
3. THE Machine Framework SHALL support runtime property modifications for upgrades
4. THE Machine Framework SHALL validate MachineData properties in the Unity Editor
5. THE Machine Framework SHALL provide default values for all configurable properties

### Requirement 15

**User Story:** As a developer, I want machines to support save/load, so that machine state persists between game sessions.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide a GetSaveData method returning a MachineSaveData struct
2. THE MachineSaveData struct SHALL include machineId, machineType, tier, gridPosition, currentState, processingProgress, inputBufferContents, and outputBufferContents
3. THE Machine Framework SHALL provide a LoadSaveData method accepting a MachineSaveData struct
4. WHEN LoadSaveData is called, THE Machine Framework SHALL restore machine state including buffers and progress
5. THE Machine Framework SHALL handle version migration for save data compatibility

### Requirement 16

**User Story:** As a developer, I want machines to validate their configuration, so that invalid setups are caught early.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide a ValidateConfiguration method that checks for common errors
2. WHEN ValidateConfiguration is called, THE Machine Framework SHALL verify all required components are present
3. WHEN ValidateConfiguration is called, THE Machine Framework SHALL verify port configurations are valid
4. WHEN ValidateConfiguration is called, THE Machine Framework SHALL verify recipe references are not null
5. THE Machine Framework SHALL log warnings for invalid configurations in the Unity Editor

### Requirement 17

**User Story:** As a developer, I want machines to have visual feedback, so that players can see machine status at a glance.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide a SetVisualState method that updates visual indicators
2. WHEN a machine is processing, THE Machine Framework SHALL activate processing visual effects
3. WHEN a machine has no power, THE Machine Framework SHALL display a power warning indicator
4. WHEN a machine is waiting for input, THE Machine Framework SHALL display an input indicator
5. WHEN a machine is waiting for output, THE Machine Framework SHALL display an output indicator

### Requirement 18

**User Story:** As a developer, I want machines to support recipe selection, so that multi-recipe machines can switch between products.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide a SetActiveRecipe method accepting a Recipe parameter
2. WHEN SetActiveRecipe is called while processing, THE Machine Framework SHALL cancel current processing and refund inputs
3. WHEN SetActiveRecipe is called, THE Machine Framework SHALL validate the recipe is available for this machine
4. THE Machine Framework SHALL provide a GetAvailableRecipes method returning all recipes this machine can process
5. THE Machine Framework SHALL persist the active recipe selection through save/load

### Requirement 19

**User Story:** As a developer, I want machines to handle errors gracefully, so that invalid operations don't crash the game.

#### Acceptance Criteria

1. WHEN a machine receives an invalid resource type, THE Machine Framework SHALL log a warning and reject the resource
2. WHEN a machine attempts to process without sufficient inputs, THE Machine Framework SHALL transition to WaitingForInput state
3. WHEN a machine attempts to produce with a full output buffer, THE Machine Framework SHALL transition to WaitingForOutput state
4. WHEN a machine loses its recipe reference, THE Machine Framework SHALL transition to Disabled state and log an error
5. THE Machine Framework SHALL provide detailed error messages for debugging

### Requirement 20

**User Story:** As a developer, I want machines to support pause/resume, so that players can control factory operation.

#### Acceptance Criteria

1. THE Machine Framework SHALL provide a SetEnabled method accepting a boolean parameter
2. WHEN SetEnabled(false) is called, THE Machine Framework SHALL transition to Disabled state and pause processing
3. WHEN SetEnabled(true) is called, THE Machine Framework SHALL transition back to operational state and resume processing
4. WHILE disabled, THE Machine Framework SHALL not consume power or process recipes
5. THE Machine Framework SHALL preserve processing progress when paused and resume from the same point
