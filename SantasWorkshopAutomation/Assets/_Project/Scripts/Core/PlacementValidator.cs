using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Static utility class for validating object placement on the grid.
    /// Performs comprehensive validation including bounds checking, occupation checking,
    /// and terrain validation.
    /// </summary>
    public static class PlacementValidator
    {
        /// <summary>
        /// Result of a placement validation check.
        /// </summary>
        public enum ValidationResult
        {
            Valid,
            OutOfBounds,
            CellOccupied,
            InvalidTerrain,
            InsufficientResources,
            RequirementNotMet
        }

        /// <summary>
        /// Detailed information about a validation result.
        /// </summary>
        public struct ValidationInfo
        {
            public ValidationResult result;
            public string message;
            public List<Vector3Int> invalidCells;

            public bool IsValid => result == ValidationResult.Valid;
        }

        /// <summary>
        /// Validates a placement attempt at the specified grid position.
        /// </summary>
        /// <param name="gridPosition">Base grid position for placement</param>
        /// <param name="placementData">Data defining the object to place</param>
        /// <param name="rotation">Rotation state (0-3 for 0°, 90°, 180°, 270°)</param>
        /// <returns>Validation information including result and error details</returns>
        public static ValidationInfo ValidatePlacement(
            Vector3Int gridPosition,
            PlacementData placementData,
            int rotation)
        {
            if (placementData == null)
            {
                return new ValidationInfo
                {
                    result = ValidationResult.RequirementNotMet,
                    message = "Placement data is null",
                    invalidCells = new List<Vector3Int>()
                };
            }

            // Get all cells that would be occupied by this placement
            List<Vector3Int> occupiedCells = GetOccupiedCells(gridPosition, placementData.gridSize, rotation);

            // Check bounds
            if (!CheckBounds(occupiedCells))
            {
                return new ValidationInfo
                {
                    result = ValidationResult.OutOfBounds,
                    message = "Placement is outside grid boundaries",
                    invalidCells = GetOutOfBoundsCells(occupiedCells)
                };
            }

            // Check occupation
            List<Vector3Int> occupiedInvalidCells = CheckOccupation(occupiedCells);
            if (occupiedInvalidCells.Count > 0)
            {
                return new ValidationInfo
                {
                    result = ValidationResult.CellOccupied,
                    message = "One or more cells are already occupied",
                    invalidCells = occupiedInvalidCells
                };
            }

            // All checks passed
            return new ValidationInfo
            {
                result = ValidationResult.Valid,
                message = "Placement is valid",
                invalidCells = new List<Vector3Int>()
            };
        }

        /// <summary>
        /// Checks if all cells are within grid boundaries.
        /// </summary>
        private static bool CheckBounds(List<Vector3Int> cells)
        {
            if (GridManager.Instance == null)
            {
                Debug.LogError("GridManager instance is null");
                return false;
            }

            foreach (var cell in cells)
            {
                if (!GridManager.Instance.IsWithinBounds(cell))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets list of cells that are out of bounds.
        /// </summary>
        private static List<Vector3Int> GetOutOfBoundsCells(List<Vector3Int> cells)
        {
            List<Vector3Int> outOfBoundsCells = new List<Vector3Int>();

            if (GridManager.Instance == null)
            {
                return outOfBoundsCells;
            }

            foreach (var cell in cells)
            {
                if (!GridManager.Instance.IsWithinBounds(cell))
                {
                    outOfBoundsCells.Add(cell);
                }
            }

            return outOfBoundsCells;
        }

        /// <summary>
        /// Checks if all cells are unoccupied.
        /// Returns list of occupied cells if any are found.
        /// </summary>
        private static List<Vector3Int> CheckOccupation(List<Vector3Int> cells)
        {
            List<Vector3Int> occupiedCells = new List<Vector3Int>();

            if (GridManager.Instance == null)
            {
                Debug.LogError("GridManager instance is null");
                return occupiedCells;
            }

            foreach (var cell in cells)
            {
                if (!GridManager.Instance.IsCellAvailable(cell))
                {
                    occupiedCells.Add(cell);
                }
            }

            return occupiedCells;
        }

        /// <summary>
        /// Calculates all grid cells that would be occupied by an object
        /// at the given position with the given size and rotation.
        /// </summary>
        /// <param name="basePosition">Bottom-left anchor position</param>
        /// <param name="size">Size in grid cells (width, height)</param>
        /// <param name="rotation">Rotation state (0-3)</param>
        /// <returns>List of all occupied cell positions</returns>
        public static List<Vector3Int> GetOccupiedCells(
            Vector3Int basePosition,
            Vector2Int size,
            int rotation)
        {
            List<Vector3Int> cells = new List<Vector3Int>();

            // Normalize rotation to 0-3 range
            rotation = rotation % 4;
            if (rotation < 0) rotation += 4;

            // Calculate occupied cells based on rotation
            // Rotation 0 (0°): Normal orientation
            // Rotation 1 (90°): Width and height swap, rotate around anchor
            // Rotation 2 (180°): Flip both axes
            // Rotation 3 (270°): Width and height swap, rotate around anchor

            switch (rotation)
            {
                case 0: // 0° - No rotation
                    for (int x = 0; x < size.x; x++)
                    {
                        for (int z = 0; z < size.y; z++)
                        {
                            cells.Add(new Vector3Int(basePosition.x + x, basePosition.y, basePosition.z + z));
                        }
                    }
                    break;

                case 1: // 90° - Rotate clockwise
                    for (int x = 0; x < size.y; x++)
                    {
                        for (int z = 0; z < size.x; z++)
                        {
                            cells.Add(new Vector3Int(basePosition.x + x, basePosition.y, basePosition.z + z));
                        }
                    }
                    break;

                case 2: // 180° - Flip both axes
                    for (int x = 0; x < size.x; x++)
                    {
                        for (int z = 0; z < size.y; z++)
                        {
                            cells.Add(new Vector3Int(basePosition.x + x, basePosition.y, basePosition.z + z));
                        }
                    }
                    break;

                case 3: // 270° - Rotate counter-clockwise
                    for (int x = 0; x < size.y; x++)
                    {
                        for (int z = 0; z < size.x; z++)
                        {
                            cells.Add(new Vector3Int(basePosition.x + x, basePosition.y, basePosition.z + z));
                        }
                    }
                    break;
            }

            return cells;
        }
    }
}
