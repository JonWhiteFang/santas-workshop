# Implementation Plan - Grid & Placement System

## Task List

- [ ] 1. Create core data structures and grid foundation
  - Create GridData class with Dictionary-based cell storage for efficient sparse grid management
  - Implement cell occupation tracking with bidirectional mapping (cell → occupant, occupant → cells)
  - Implement bounds checking methods to validate grid positions are within defined grid boundaries
  - Create GridConfiguration struct to store grid dimensions, cell size, and origin point
  - _Requirements: 1.1, 1.5_

- [ ] 2. Implement GridManager singleton
  - Create GridManager MonoBehaviour with singleton pattern for global grid access
  - Implement Awake() to initialize GridData with configurable width, height, and cell size
  - Implement WorldToGrid() method to convert world coordinates to integer grid positions
  - Implement GridToWorld() method to convert grid positions to world space coordinates
  - Implement IsCellAvailable() to check if a single grid cell is unoccupied and within bounds
  - Implement AreCellsAvailable() to check if multiple cells are available for multi-cell objects
  - Implement OccupyCell() and OccupyCells() to mark cells as occupied by a game object
  - Implement FreeCell() and FreeCells() to mark cells as available when objects are removed
  - Implement GetOccupant() to retrieve the game object occupying a specific cell
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 8.1, 8.2, 8.3, 8.5_

- [ ] 3. Create PlacementData ScriptableObject
  - Create PlacementData ScriptableObject with CreateAssetMenu attribute for designer-friendly creation
  - Add fields for object name, prefab reference, and grid size (Vector2Int for width/height)
  - Add pivot offset field (Vector3) to handle objects with non-centered pivots
  - Add canRotate boolean flag to control whether object supports rotation
  - Add PlacementLayer enum flags for future multi-layer support (Ground, Elevated, Underground)
  - Implement GetOccupiedCells() helper method that calculates all cells occupied by object at given position and rotation
  - Implement GetRotation() helper method that converts rotation state (0-3) to Quaternion
  - _Requirements: 8.1, 8.2, 8.4, 8.5_

- [ ] 4. Implement PlacementValidator static utility
  - Create PlacementValidator static class with ValidationResult enum (Valid, OutOfBounds, CellOccupied, InvalidTerrain, etc.)
  - Create ValidationInfo struct containing result, error message, and list of invalid cells
  - Implement ValidatePlacement() method that performs comprehensive validation for placement attempt
  - Implement CheckBounds() private method to verify all cells are within grid boundaries
  - Implement CheckOccupation() private method to verify all cells are unoccupied
  - Implement GetOccupiedCells() helper that calculates cell list considering rotation
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 8.2_

- [ ] 5. Create GhostPreview component
  - Create GhostPreview MonoBehaviour to manage visual preview during placement
  - Implement Initialize() to instantiate preview object from prefab and store material references
  - Implement UpdatePosition() to move ghost preview to specified world position with smooth updates
  - Implement UpdateRotation() to rotate ghost preview to specified quaternion rotation
  - Implement SetValid() to switch between valid (green) and invalid (red) materials based on validation
  - Implement ApplyMaterials() private method to recursively apply materials to all renderers in preview
  - Implement Destroy() to clean up preview object when placement mode exits
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 8.4_

- [ ] 6. Implement PlacementController
  - Create PlacementController MonoBehaviour to orchestrate placement mode and user interaction
  - Add serialized fields for rotation key (default: R), cancel key (default: Escape), and placement materials
  - Implement EnterPlacementMode() to initialize placement state, create ghost preview, and store placement data
  - Implement ExitPlacementMode() to destroy ghost preview and reset placement state
  - Implement Update() to handle per-frame ghost preview updates and input processing
  - Implement HandleRotationInput() to detect rotation key press and increment rotation state (0→1→2→3→0)
  - Implement HandleConfirmInput() to detect left mouse click and trigger placement confirmation
  - Implement HandleCancelInput() to detect cancel key or right mouse click and exit placement mode
  - Implement UpdateGhostPreview() to calculate grid position from cursor, update ghost position, and run validation
  - Implement ConfirmPlacement() to validate placement, instantiate actual object, update grid state, and exit placement mode
  - Implement RotateGhostPreview() to update rotation state and apply rotation to ghost preview
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 4.1, 4.2, 4.3, 4.4, 4.5, 5.1, 5.2, 5.3, 5.4, 5.5, 6.1, 6.2, 6.3, 6.4, 6.5, 7.1, 7.2, 7.3, 7.4, 7.5_

- [ ] 7. Create placement materials
  - Create valid placement material (URP/Lit) with green emission (0, 1, 0, 0.3) and 50% transparency
  - Create invalid placement material (URP/Lit) with red emission (1, 0, 0, 0.3) and 50% transparency
  - Configure both materials with appropriate render queue for transparency
  - Assign materials to PlacementController serialized fields in inspector
  - _Requirements: 3.3, 3.4_

- [ ] 8. Implement GridVisualizer
  - Create GridVisualizer MonoBehaviour to render grid lines for visual reference
  - Add serialized fields for grid line material, color (white with 0.2 alpha), and line width (0.02)
  - Implement Initialize() to generate LineRenderer components for all grid lines
  - Implement GenerateGridLines() to create horizontal and vertical lines at cell boundaries
  - Implement SetVisible() to toggle grid line visibility on/off
  - Integrate GridVisualizer into GridManager with toggle method
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5_

- [ ] 9. Create test scene and test objects
  - Create GridPlacementTest scene in Assets/_Project/Scenes/TestScenes/
  - Add GridManager to scene with configured grid size (50x50) and cell size (1.0)
  - Add PlacementController to scene with configured input keys and materials
  - Create test PlacementData assets for 1x1, 2x2, and 3x1 objects
  - Create simple cube prefabs for testing (1x1x1 Unity units)
  - Add camera to scene positioned for isometric view of grid
  - Add UI button to enter placement mode with test object
  - _Requirements: All requirements (integration testing)_

- [ ] 10. Implement placement audio feedback
  - Create audio source component on PlacementController
  - Add serialized fields for error sound, success sound, and rotation sound clips
  - Implement PlayErrorSound() with 0.1 second cooldown to prevent audio spam
  - Implement PlaySuccessSound() triggered on successful placement confirmation
  - Implement PlayRotationSound() triggered on rotation key press
  - Integrate audio playback into HandleRotationInput(), ConfirmPlacement(), and validation feedback
  - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5_

- [ ] 11. Add input system integration
  - Create input action asset for placement controls (confirm, cancel, rotate)
  - Configure mouse position input action for cursor tracking
  - Update PlacementController to use Input System instead of legacy Input
  - Add input action callbacks for confirm, cancel, and rotate actions
  - Test input responsiveness and ensure placement works with new input system
  - _Requirements: 4.1, 4.2, 5.1, 6.1, 7.1_

- [ ] 12. Integrate with existing systems
  - Add GridManager initialization to GameManager startup sequence
  - Create interface between PlacementController and future BuildMenuUI
  - Add placement event notifications (OnPlacementConfirmed, OnPlacementCancelled) for other systems
  - Document integration points for Resource System (cost checking) and Machine Framework (position assignment)
  - _Requirements: All requirements (system integration)_
