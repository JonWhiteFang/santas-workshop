using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Central manager for grid operations, coordinate conversion, and cell state management.
    /// Singleton pattern provides global access to grid functionality.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Grid Configuration")]
        [SerializeField] private int gridWidth = 100;
        [SerializeField] private int gridHeight = 100;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;

        [Header("Grid Visualization")]
        [SerializeField] private bool enableGridVisualization = true;
        [SerializeField] private Material gridLineMaterial;

        private GridData _gridData;
        private GridConfiguration _configuration;
        private GridVisualizer _visualizer;

        /// <summary>
        /// Gets the current grid configuration.
        /// </summary>
        public GridConfiguration Configuration => _configuration;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize grid data
            _configuration = new GridConfiguration(gridWidth, gridHeight, cellSize, gridOrigin);
            _gridData = new GridData(gridWidth, gridHeight);

            // Initialize grid visualizer
            InitializeGridVisualizer();

            Debug.Log($"GridManager initialized: {gridWidth}x{gridHeight} grid with cell size {cellSize}");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #region Coordinate Conversion

        /// <summary>
        /// Converts world position to grid position.
        /// </summary>
        public Vector3Int WorldToGrid(Vector3 worldPosition)
        {
            Vector3 localPosition = worldPosition - gridOrigin;
            
            int x = Mathf.FloorToInt(localPosition.x / cellSize);
            int z = Mathf.FloorToInt(localPosition.z / cellSize);
            
            return new Vector3Int(x, 0, z);
        }

        /// <summary>
        /// Converts grid position to world position (cell center).
        /// </summary>
        public Vector3 GridToWorld(Vector3Int gridPosition)
        {
            float x = gridPosition.x * cellSize + cellSize * 0.5f;
            float z = gridPosition.z * cellSize + cellSize * 0.5f;
            
            return gridOrigin + new Vector3(x, 0, z);
        }

        #endregion

        #region Cell State Queries

        /// <summary>
        /// Checks if a single cell is available (unoccupied and within bounds).
        /// </summary>
        public bool IsCellAvailable(Vector3Int gridPosition)
        {
            return IsWithinBounds(gridPosition) && !_gridData.IsCellOccupied(gridPosition);
        }

        /// <summary>
        /// Checks if multiple cells are available for a multi-cell object.
        /// </summary>
        public bool AreCellsAvailable(Vector3Int gridPosition, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.y; z++)
                {
                    Vector3Int cellPos = gridPosition + new Vector3Int(x, 0, z);
                    if (!IsCellAvailable(cellPos))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if a position is within the grid boundaries.
        /// </summary>
        public bool IsWithinBounds(Vector3Int gridPosition)
        {
            return _gridData.IsWithinBounds(gridPosition);
        }

        #endregion

        #region Cell State Modification

        /// <summary>
        /// Marks a single cell as occupied by the specified object.
        /// </summary>
        public void OccupyCell(Vector3Int gridPosition, GameObject occupant)
        {
            if (!IsWithinBounds(gridPosition))
            {
                Debug.LogWarning($"Attempted to occupy cell outside bounds: {gridPosition}");
                return;
            }

            _gridData.SetCellOccupied(gridPosition, occupant);
        }

        /// <summary>
        /// Marks multiple cells as occupied by the specified object.
        /// </summary>
        public void OccupyCells(Vector3Int gridPosition, Vector2Int size, GameObject occupant)
        {
            List<Vector3Int> cells = new List<Vector3Int>();

            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.y; z++)
                {
                    Vector3Int cellPos = gridPosition + new Vector3Int(x, 0, z);
                    
                    if (!IsWithinBounds(cellPos))
                    {
                        Debug.LogWarning($"Attempted to occupy cell outside bounds: {cellPos}");
                        continue;
                    }

                    cells.Add(cellPos);
                }
            }

            _gridData.RegisterOccupant(occupant, cells);
        }

        /// <summary>
        /// Marks a single cell as free (unoccupied).
        /// </summary>
        public void FreeCell(Vector3Int gridPosition)
        {
            _gridData.SetCellFree(gridPosition);
        }

        /// <summary>
        /// Marks multiple cells as free (unoccupied).
        /// </summary>
        public void FreeCells(Vector3Int gridPosition, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.y; z++)
                {
                    Vector3Int cellPos = gridPosition + new Vector3Int(x, 0, z);
                    _gridData.SetCellFree(cellPos);
                }
            }
        }

        #endregion

        #region Cell Queries

        /// <summary>
        /// Gets the object occupying the specified cell.
        /// Returns null if cell is not occupied.
        /// </summary>
        public GameObject GetOccupant(Vector3Int gridPosition)
        {
            return _gridData.GetOccupant(gridPosition);
        }

        /// <summary>
        /// Gets all cells occupied by the specified object.
        /// </summary>
        public List<Vector3Int> GetOccupiedCells(GameObject occupant)
        {
            return _gridData.GetCellsForOccupant(occupant);
        }

        #endregion

        #region Grid Visualization

        /// <summary>
        /// Initializes the grid visualizer component.
        /// </summary>
        private void InitializeGridVisualizer()
        {
            if (!enableGridVisualization)
            {
                return;
            }

            // Add GridVisualizer component if not already present
            _visualizer = gameObject.GetComponent<GridVisualizer>();
            if (_visualizer == null)
            {
                _visualizer = gameObject.AddComponent<GridVisualizer>();
            }

            // Initialize with grid dimensions
            _visualizer.Initialize(gridWidth, gridHeight, cellSize);
            _visualizer.SetVisible(enableGridVisualization);
        }

        /// <summary>
        /// Toggles grid line visualization on or off.
        /// </summary>
        /// <param name="enabled">True to show grid lines, false to hide</param>
        public void SetGridVisualizationEnabled(bool enabled)
        {
            enableGridVisualization = enabled;
            
            if (_visualizer != null)
            {
                _visualizer.SetVisible(enabled);
            }
        }

        #endregion
    }
}
