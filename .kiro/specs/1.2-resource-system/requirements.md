# Requirements Document: Resource System

## Introduction

This specification defines the Resource System for Santa's Workshop Automation, which manages all resources in the game including raw materials, refined goods, components, and finished toys. The system provides centralized tracking, storage, transfer logic, and validation for resource operations across all game systems including machines, logistics, and player inventory.

## Glossary

- **Resource System**: The centralized system responsible for tracking, storing, and managing all game resources
- **Resource**: Any material, component, or item that can be extracted, processed, stored, or consumed in the game
- **Resource Category**: A classification grouping for resources (RawMaterial, Refined, Component, Toy, Magic, Energy)
- **Resource Stack**: A data structure representing a specific resource type and quantity
- **Resource Manager**: The singleton manager component responsible for global resource operations
- **Resource Data**: A ScriptableObject defining the properties and configuration of a resource type
- **Resource Database**: A collection of all ResourceData assets indexed by resource ID
- **Global Resource Count**: The total quantity of a specific resource available across the entire workshop
- **Resource Transfer**: The operation of moving resources between storage locations or systems
- **Resource Validation**: The process of checking resource availability before consumption

## Requirements

### Requirement 1

**User Story:** As a developer, I want resource data structures defined in ScriptableObjects, so that designers can create and configure resources without modifying code.

#### Acceptance Criteria

1. THE Resource System SHALL provide a ResourceData ScriptableObject with fields for resourceId, displayName, description, icon, category, stackSize, weight, itemPrefab, itemColor, baseValue, canBeStored, and canBeTransported
2. THE Resource System SHALL provide a ResourceCategory enum with values RawMaterial, Refined, Component, ToyPart, FinishedToy, Magic, and Energy
3. THE Resource System SHALL provide a ResourceStack struct with fields for resourceId and amount
4. WHEN a ResourceData asset is created, THE Resource System SHALL validate that resourceId is unique and not empty
5. WHEN a ResourceData asset is created, THE Resource System SHALL validate that stackSize is greater than zero

### Requirement 2

**User Story:** As a developer, I want a ResourceManager singleton, so that I can access resource operations from any game system.

#### Acceptance Criteria

1. THE Resource System SHALL provide a ResourceManager singleton component accessible via ResourceManager.Instance
2. WHEN the ResourceManager is initialized, THE Resource System SHALL load all ResourceData assets from the Resources/ResourceDefinitions folder into a resource database
3. WHEN the ResourceManager is initialized, THE Resource System SHALL initialize global resource counts to zero for all registered resources
4. WHEN multiple ResourceManager instances exist, THE Resource System SHALL destroy duplicate instances and preserve only the first instance
5. WHEN the ResourceManager is destroyed, THE Resource System SHALL set the Instance property to null

### Requirement 3

**User Story:** As a developer, I want to add resources to the global inventory, so that extracted or produced resources are tracked correctly.

#### Acceptance Criteria

1. THE Resource System SHALL provide an AddResource method accepting a resourceId string and amount integer
2. WHEN AddResource is called with a valid resourceId, THE Resource System SHALL increase the global resource count by the specified amount
3. WHEN AddResource is called with an invalid resourceId, THE Resource System SHALL log a warning and return false
4. WHEN AddResource is called with a negative amount, THE Resource System SHALL log a warning and return false
5. WHEN a resource count changes, THE Resource System SHALL invoke an OnResourceChanged event with the resourceId and new amount

### Requirement 4

**User Story:** As a developer, I want to consume resources from the global inventory, so that machines and systems can use resources for production.

#### Acceptance Criteria

1. THE Resource System SHALL provide a TryConsumeResource method accepting a resourceId string and amount integer
2. WHEN TryConsumeResource is called, THE Resource System SHALL check if sufficient resources are available before consumption
3. WHEN sufficient resources are available, THE Resource System SHALL decrease the global resource count by the specified amount and return true
4. WHEN insufficient resources are available, THE Resource System SHALL not modify the resource count and return false
5. WHEN a resource is consumed, THE Resource System SHALL invoke an OnResourceChanged event with the resourceId and new amount

### Requirement 5

**User Story:** As a developer, I want to check resource availability, so that I can validate operations before attempting them.

#### Acceptance Criteria

1. THE Resource System SHALL provide a HasResource method accepting a resourceId string and amount integer
2. WHEN HasResource is called with a valid resourceId, THE Resource System SHALL return true if the global count is greater than or equal to the specified amount
3. WHEN HasResource is called with an invalid resourceId, THE Resource System SHALL return false
4. WHEN HasResource is called with a negative amount, THE Resource System SHALL return false
5. THE Resource System SHALL provide a GetResourceCount method accepting a resourceId string and returning the current global count

### Requirement 6

**User Story:** As a developer, I want to consume multiple resources atomically, so that recipes requiring multiple inputs are validated correctly.

#### Acceptance Criteria

1. THE Resource System SHALL provide a TryConsumeResources method accepting an array of ResourceStack structs
2. WHEN TryConsumeResources is called, THE Resource System SHALL validate that all required resources are available before consuming any
3. WHEN all required resources are available, THE Resource System SHALL consume all resources atomically and return true
4. WHEN any required resource is insufficient, THE Resource System SHALL not consume any resources and return false
5. WHEN resources are consumed, THE Resource System SHALL invoke OnResourceChanged events for each affected resource

### Requirement 7

**User Story:** As a developer, I want to add multiple resources at once, so that machine outputs with multiple products are handled efficiently.

#### Acceptance Criteria

1. THE Resource System SHALL provide an AddResources method accepting an array of ResourceStack structs
2. WHEN AddResources is called, THE Resource System SHALL add each resource in the array to the global inventory
3. WHEN any resourceId in the array is invalid, THE Resource System SHALL log a warning for that resource and continue processing remaining resources
4. WHEN resources are added, THE Resource System SHALL invoke OnResourceChanged events for each affected resource
5. THE Resource System SHALL complete the AddResources operation in a single frame

### Requirement 8

**User Story:** As a developer, I want to retrieve resource data by ID, so that I can access resource properties for UI display and game logic.

#### Acceptance Criteria

1. THE Resource System SHALL provide a GetResourceData method accepting a resourceId string
2. WHEN GetResourceData is called with a valid resourceId, THE Resource System SHALL return the corresponding ResourceData ScriptableObject
3. WHEN GetResourceData is called with an invalid resourceId, THE Resource System SHALL return null
4. THE Resource System SHALL provide a GetAllResources method returning a collection of all registered ResourceData assets
5. THE Resource System SHALL provide a GetResourcesByCategory method accepting a ResourceCategory and returning matching ResourceData assets

### Requirement 9

**User Story:** As a developer, I want resource change events, so that UI and other systems can react to inventory changes.

#### Acceptance Criteria

1. THE Resource System SHALL provide an OnResourceChanged event with parameters resourceId string and newAmount long
2. WHEN a resource count changes via AddResource, THE Resource System SHALL invoke OnResourceChanged with the updated values
3. WHEN a resource count changes via TryConsumeResource, THE Resource System SHALL invoke OnResourceChanged with the updated values
4. WHEN multiple resources change via AddResources or TryConsumeResources, THE Resource System SHALL invoke OnResourceChanged for each affected resource
5. THE Resource System SHALL invoke OnResourceChanged events after the resource count has been updated

### Requirement 10

**User Story:** As a developer, I want resource validation utilities, so that I can ensure data integrity throughout the game.

#### Acceptance Criteria

1. THE Resource System SHALL provide a ValidateResourceStack method accepting a ResourceStack struct
2. WHEN ValidateResourceStack is called, THE Resource System SHALL return true if the resourceId exists and amount is positive
3. WHEN ValidateResourceStack is called with an invalid resourceId, THE Resource System SHALL return false
4. WHEN ValidateResourceStack is called with a non-positive amount, THE Resource System SHALL return false
5. THE Resource System SHALL provide a ValidateResourceStacks method accepting an array of ResourceStack structs and returning true only if all stacks are valid

### Requirement 11

**User Story:** As a developer, I want resource transfer operations, so that resources can move between storage locations.

#### Acceptance Criteria

1. THE Resource System SHALL provide a TransferResource method accepting sourceId string, targetId string, resourceId string, and amount integer
2. WHEN TransferResource is called, THE Resource System SHALL validate that the source has sufficient resources before transfer
3. WHEN the source has sufficient resources, THE Resource System SHALL decrease the source count and increase the target count by the specified amount
4. WHEN the source has insufficient resources, THE Resource System SHALL not modify any counts and return false
5. WHEN a transfer completes, THE Resource System SHALL invoke OnResourceChanged events for both source and target

### Requirement 12

**User Story:** As a developer, I want to reset the resource system, so that I can clear state between game sessions or for testing.

#### Acceptance Criteria

1. THE Resource System SHALL provide a ResetResources method with no parameters
2. WHEN ResetResources is called, THE Resource System SHALL set all global resource counts to zero
3. WHEN ResetResources is called, THE Resource System SHALL invoke OnResourceChanged events for all resources that had non-zero counts
4. WHEN ResetResources is called, THE Resource System SHALL preserve the resource database registration
5. THE Resource System SHALL provide a ResetToDefaults method that resets counts and reloads the resource database

### Requirement 13

**User Story:** As a developer, I want resource system initialization to be explicit, so that I can control when resources are loaded.

#### Acceptance Criteria

1. THE Resource System SHALL provide an Initialize method that loads the resource database
2. WHEN Initialize is called, THE Resource System SHALL scan the Resources/ResourceDefinitions folder for ResourceData assets
3. WHEN Initialize is called, THE Resource System SHALL register each found ResourceData asset in the resource database indexed by resourceId
4. WHEN Initialize finds duplicate resourceIds, THE Resource System SHALL log an error and use the first occurrence
5. WHEN Initialize completes, THE Resource System SHALL invoke an OnResourceSystemInitialized event

### Requirement 14

**User Story:** As a developer, I want resource capacity limits, so that storage systems can enforce maximum quantities.

#### Acceptance Criteria

1. THE Resource System SHALL provide a SetResourceCapacity method accepting a resourceId string and capacity long
2. WHEN SetResourceCapacity is called, THE Resource System SHALL store the capacity limit for the specified resource
3. WHEN AddResource would exceed the capacity limit, THE Resource System SHALL add only up to the capacity and return the amount actually added
4. THE Resource System SHALL provide a GetResourceCapacity method accepting a resourceId string and returning the current capacity limit
5. WHERE no capacity is set for a resource, THE Resource System SHALL treat the capacity as unlimited

### Requirement 15

**User Story:** As a developer, I want resource system save/load support, so that resource counts persist between game sessions.

#### Acceptance Criteria

1. THE Resource System SHALL provide a GetSaveData method returning a ResourceSaveData struct
2. WHEN GetSaveData is called, THE Resource System SHALL include all non-zero resource counts in the save data
3. THE Resource System SHALL provide a LoadSaveData method accepting a ResourceSaveData struct
4. WHEN LoadSaveData is called, THE Resource System SHALL restore all resource counts from the save data
5. WHEN LoadSaveData is called, THE Resource System SHALL invoke OnResourceChanged events for all restored resources
