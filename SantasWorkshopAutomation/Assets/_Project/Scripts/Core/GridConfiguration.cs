using UnityEngine;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Configuration data for the grid system.
    /// Stores grid dimensions, cell size, and origin point.
    /// </summary>
    [System.Serializable]
    public struct GridConfiguration
    {
        public int width;
        public int height;
        public float cellSize;
        public Vector3 origin;

        public GridConfiguration(int width, int height, float cellSize, Vector3 origin)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.origin = origin;
        }
    }
}
