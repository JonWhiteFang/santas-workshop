using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Data
{
    /// <summary>
    /// ScriptableObject defining properties of an object that can be placed on the grid.
    /// Designer-friendly data asset for creating new placeable objects.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPlacementData", menuName = "Game/Placement Data")]
    public class PlacementData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Display name of the object")]
        public string objectName;
        
        [Tooltip("Prefab to instantiate when placed")]
        public GameObject prefab;

        [Header("Grid Properties")]
        [Tooltip("Size in grid cells (width x height)")]
        public Vector2Int gridSize = new Vector2Int(1, 1);
        
        [Tooltip("Offset from grid position to object pivot")]
        public Vector3 pivotOffset = Vector3.zero;

        [Header("Placement Rules")]
        [Tooltip("Can this object be rotated during placement?")]
        public bool canRotate = true;
        
        [Tooltip("Requires flat ground for placement?")]
        public bool requiresFlatGround = true;
        
        [Tooltip("Allowed placement layers")]
        public PlacementLayer allowedLayers = PlacementLayer.Ground;

        [Header("Visual")]
        [Tooltip("Optional custom material for ghost preview")]
        public Material ghostMaterial;

        /// <summary>
        /// Calculates all cells occupied by this object at the given position and rotation.
        /// </summary>
        /// <param name="basePosition">Bottom-left anchor position</param>
        /// <param name="rotation">Rotation state (0-3 for 0°, 90°, 180°, 270°)</param>
        /// <returns>List of occupied cell positions</returns>
        public List<Vector3Int> GetOccupiedCells(Vector3Int basePosition, int rotation)
        {
            List<Vector3Int> cells = new List<Vector3Int>();
            Vector2Int rotatedSize = GetRotatedSize(rotation);

            for (int x = 0; x < rotatedSize.x; x++)
            {
                for (int z = 0; z < rotatedSize.y; z++)
                {
                    Vector3Int cellOffset = RotateOffset(new Vector3Int(x, 0, z), rotation);
                    cells.Add(basePosition + cellOffset);
                }
            }

            return cells;
        }

        /// <summary>
        /// Converts rotation state (0-3) to Quaternion.
        /// </summary>
        /// <param name="rotationState">Rotation state (0=0°, 1=90°, 2=180°, 3=270°)</param>
        /// <returns>Quaternion rotation</returns>
        public Quaternion GetRotation(int rotationState)
        {
            return Quaternion.Euler(0, rotationState * 90f, 0);
        }

        /// <summary>
        /// Gets the grid size after applying rotation.
        /// </summary>
        private Vector2Int GetRotatedSize(int rotation)
        {
            // For 90° and 270° rotations, swap width and height
            if (rotation == 1 || rotation == 3)
            {
                return new Vector2Int(gridSize.y, gridSize.x);
            }
            return gridSize;
        }

        /// <summary>
        /// Rotates a cell offset based on rotation state.
        /// </summary>
        private Vector3Int RotateOffset(Vector3Int offset, int rotation)
        {
            switch (rotation)
            {
                case 0: // 0°
                    return offset;
                
                case 1: // 90° clockwise
                    return new Vector3Int(offset.z, 0, gridSize.x - 1 - offset.x);
                
                case 2: // 180°
                    return new Vector3Int(gridSize.x - 1 - offset.x, 0, gridSize.y - 1 - offset.z);
                
                case 3: // 270° clockwise
                    return new Vector3Int(gridSize.y - 1 - offset.z, 0, offset.x);
                
                default:
                    return offset;
            }
        }

        private void OnValidate()
        {
            // Ensure grid size is at least 1x1
            if (gridSize.x < 1) gridSize.x = 1;
            if (gridSize.y < 1) gridSize.y = 1;

            // Warn if prefab is missing
            if (prefab == null)
            {
                Debug.LogWarning($"PlacementData '{name}' has no prefab assigned!");
            }
        }
    }
}
