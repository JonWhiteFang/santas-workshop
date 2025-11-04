using UnityEngine;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Renders grid lines for visual reference during placement.
    /// Optional feature that can be toggled on/off.
    /// </summary>
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Material gridLineMaterial;
        [SerializeField] private Color gridLineColor = new Color(1f, 1f, 1f, 0.2f);
        [SerializeField] private float lineWidth = 0.02f;

        private GameObject _gridLinesContainer;
        private LineRenderer[] _horizontalLines;
        private LineRenderer[] _verticalLines;
        private bool _isVisible = true;
        private int _width;
        private int _height;
        private float _cellSize;

        /// <summary>
        /// Initializes the grid visualizer with the specified dimensions.
        /// </summary>
        /// <param name="width">Grid width in cells</param>
        /// <param name="height">Grid height in cells</param>
        /// <param name="cellSize">Size of each cell in Unity units</param>
        public void Initialize(int width, int height, float cellSize)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;

            // Create container for all grid lines
            _gridLinesContainer = new GameObject("GridLines");
            _gridLinesContainer.transform.SetParent(transform);
            _gridLinesContainer.transform.localPosition = Vector3.zero;

            GenerateGridLines();
        }

        /// <summary>
        /// Toggles grid line visibility.
        /// </summary>
        /// <param name="visible">True to show grid lines, false to hide</param>
        public void SetVisible(bool visible)
        {
            _isVisible = visible;
            if (_gridLinesContainer != null)
            {
                _gridLinesContainer.SetActive(visible);
            }
        }

        /// <summary>
        /// Generates all grid lines (horizontal and vertical).
        /// </summary>
        private void GenerateGridLines()
        {
            CreateHorizontalLines();
            CreateVerticalLines();
        }

        /// <summary>
        /// Creates horizontal grid lines (parallel to X-axis).
        /// </summary>
        private void CreateHorizontalLines()
        {
            // Create height + 1 horizontal lines (including boundaries)
            _horizontalLines = new LineRenderer[_height + 1];

            for (int z = 0; z <= _height; z++)
            {
                GameObject lineObj = new GameObject($"HorizontalLine_{z}");
                lineObj.transform.SetParent(_gridLinesContainer.transform);
                lineObj.transform.localPosition = Vector3.zero;

                LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
                ConfigureLineRenderer(lineRenderer);

                // Set line positions
                float zPos = z * _cellSize;
                Vector3 startPos = new Vector3(0f, 0.01f, zPos); // Slightly above ground to prevent z-fighting
                Vector3 endPos = new Vector3(_width * _cellSize, 0.01f, zPos);

                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, endPos);

                _horizontalLines[z] = lineRenderer;
            }
        }

        /// <summary>
        /// Creates vertical grid lines (parallel to Z-axis).
        /// </summary>
        private void CreateVerticalLines()
        {
            // Create width + 1 vertical lines (including boundaries)
            _verticalLines = new LineRenderer[_width + 1];

            for (int x = 0; x <= _width; x++)
            {
                GameObject lineObj = new GameObject($"VerticalLine_{x}");
                lineObj.transform.SetParent(_gridLinesContainer.transform);
                lineObj.transform.localPosition = Vector3.zero;

                LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
                ConfigureLineRenderer(lineRenderer);

                // Set line positions
                float xPos = x * _cellSize;
                Vector3 startPos = new Vector3(xPos, 0.01f, 0f); // Slightly above ground to prevent z-fighting
                Vector3 endPos = new Vector3(xPos, 0.01f, _height * _cellSize);

                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, endPos);

                _verticalLines[x] = lineRenderer;
            }
        }

        /// <summary>
        /// Configures a LineRenderer with the appropriate settings.
        /// </summary>
        /// <param name="lineRenderer">LineRenderer to configure</param>
        private void ConfigureLineRenderer(LineRenderer lineRenderer)
        {
            lineRenderer.material = gridLineMaterial;
            lineRenderer.startColor = gridLineColor;
            lineRenderer.endColor = gridLineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.useWorldSpace = false;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.alignment = LineAlignment.TransformZ;
        }

        /// <summary>
        /// Cleans up grid lines when the component is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (_gridLinesContainer != null)
            {
                Destroy(_gridLinesContainer);
            }
        }
    }
}
