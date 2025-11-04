using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Stores grid cell occupation state with efficient sparse storage.
    /// Uses Dictionary-based storage for memory efficiency with large, mostly empty grids.
    /// Maintains bidirectional mapping between cells and occupants for fast queries.
    /// </summary>
    public class GridData
    {
        private readonly Dictionary<Vector3Int, GameObject> _occupiedCells;
        private readonly Dictionary<GameObject, List<Vector3Int>> _occupantToCells;
        private readonly int _width;
        private readonly int _height;

        public GridData(int width, int height)
        {
            _width = width;
            _height = height;
            _occupiedCells = new Dictionary<Vector3Int, GameObject>();
            _occupantToCells = new Dictionary<GameObject, List<Vector3Int>>();
        }

        /// <summary>
        /// Checks if a cell is occupied by any object.
        /// </summary>
        public bool IsCellOccupied(Vector3Int position)
        {
            return _occupiedCells.ContainsKey(position);
        }

        /// <summary>
        /// Marks a cell as occupied by the specified object.
        /// </summary>
        public void SetCellOccupied(Vector3Int position, GameObject occupant)
        {
            _occupiedCells[position] = occupant;

            if (!_occupantToCells.ContainsKey(occupant))
            {
                _occupantToCells[occupant] = new List<Vector3Int>();
            }

            if (!_occupantToCells[occupant].Contains(position))
            {
                _occupantToCells[occupant].Add(position);
            }
        }

        /// <summary>
        /// Marks a cell as free (unoccupied).
        /// </summary>
        public void SetCellFree(Vector3Int position)
        {
            if (_occupiedCells.TryGetValue(position, out GameObject occupant))
            {
                _occupiedCells.Remove(position);

                if (_occupantToCells.TryGetValue(occupant, out List<Vector3Int> cells))
                {
                    cells.Remove(position);

                    if (cells.Count == 0)
                    {
                        _occupantToCells.Remove(occupant);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the object occupying the specified cell.
        /// Returns null if cell is not occupied.
        /// </summary>
        public GameObject GetOccupant(Vector3Int position)
        {
            return _occupiedCells.TryGetValue(position, out GameObject occupant) ? occupant : null;
        }

        /// <summary>
        /// Gets all cells occupied by the specified object.
        /// </summary>
        public List<Vector3Int> GetCellsForOccupant(GameObject occupant)
        {
            return _occupantToCells.TryGetValue(occupant, out List<Vector3Int> cells) 
                ? new List<Vector3Int>(cells) 
                : new List<Vector3Int>();
        }

        /// <summary>
        /// Registers an occupant with a list of cells it occupies.
        /// </summary>
        public void RegisterOccupant(GameObject occupant, List<Vector3Int> cells)
        {
            foreach (var cell in cells)
            {
                SetCellOccupied(cell, occupant);
            }
        }

        /// <summary>
        /// Unregisters an occupant and frees all cells it occupied.
        /// </summary>
        public void UnregisterOccupant(GameObject occupant)
        {
            if (_occupantToCells.TryGetValue(occupant, out List<Vector3Int> cells))
            {
                foreach (var cell in cells.ToArray())
                {
                    SetCellFree(cell);
                }
            }
        }

        /// <summary>
        /// Checks if a position is within the grid boundaries.
        /// </summary>
        public bool IsWithinBounds(Vector3Int position)
        {
            return position.x >= 0 && position.x < _width &&
                   position.z >= 0 && position.z < _height;
        }

        /// <summary>
        /// Gets the grid width.
        /// </summary>
        public int Width => _width;

        /// <summary>
        /// Gets the grid height.
        /// </summary>
        public int Height => _height;
    }
}
