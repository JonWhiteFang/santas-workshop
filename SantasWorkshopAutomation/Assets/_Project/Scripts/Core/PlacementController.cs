using UnityEngine;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Manages placement mode state, user input, and orchestrates placement operations.
    /// Handles ghost preview updates, rotation, validation, and placement confirmation.
    /// </summary>
    public class PlacementController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyCode rotateKey = KeyCode.R;
        [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

        [Header("Materials")]
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;

        [Header("Camera")]
        [SerializeField] private Camera mainCamera;

        private bool _isInPlacementMode;
        private GhostPreview _currentGhostPreview;
        private PlacementData _currentPlacementData;
        private int _currentRotation; // 0, 1, 2, 3 for 0°, 90°, 180°, 270°
        private Vector3Int _currentGridPosition;
        private bool _lastValidationResult;

        /// <summary>
        /// Gets whether the controller is currently in placement mode.
        /// </summary>
        public bool IsInPlacementMode => _isInPlacementMode;

        private void Awake()
        {
            // Get main camera if not assigned
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (_isInPlacementMode)
            {
                HandleRotationInput();
                HandleConfirmInput();
                HandleCancelInput();
                UpdateGhostPreview();
            }
        }

        /// <summary>
        /// Enters placement mode with the specified placement data.
        /// </summary>
        /// <param name="placementData">Data defining the object to place</param>
        public void EnterPlacementMode(PlacementData placementData)
        {
            if (placementData == null)
            {
                Debug.LogError("PlacementController: Cannot enter placement mode with null placement data");
                return;
            }

            if (_isInPlacementMode)
            {
                ExitPlacementMode();
            }

            _isInPlacementMode = true;
            _currentPlacementData = placementData;
            _currentRotation = 0;
            _lastValidationResult = false;

            CreateGhostPreview();

            Debug.Log($"Entered placement mode for: {placementData.objectName}");
        }

        /// <summary>
        /// Exits placement mode and cleans up the ghost preview.
        /// </summary>
        public void ExitPlacementMode()
        {
            if (!_isInPlacementMode)
            {
                return;
            }

            _isInPlacementMode = false;
            DestroyGhostPreview();
            _currentPlacementData = null;
            _currentRotation = 0;

            Debug.Log("Exited placement mode");
        }

        /// <summary>
        /// Handles rotation input from the player.
        /// </summary>
        private void HandleRotationInput()
        {
            if (Input.GetKeyDown(rotateKey))
            {
                if (_currentPlacementData != null && _currentPlacementData.canRotate)
                {
                    RotateGhostPreview();
                }
            }
        }

        /// <summary>
        /// Handles placement confirmation input (left mouse click).
        /// </summary>
        private void HandleConfirmInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                ConfirmPlacement();
            }
        }

        /// <summary>
        /// Handles placement cancellation input (cancel key or right mouse click).
        /// </summary>
        private void HandleCancelInput()
        {
            if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1))
            {
                ExitPlacementMode();
            }
        }

        /// <summary>
        /// Updates the ghost preview position and validation state based on cursor position.
        /// </summary>
        private void UpdateGhostPreview()
        {
            if (_currentGhostPreview == null || mainCamera == null)
            {
                return;
            }

            // Get cursor position in world space
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPosition = ray.GetPoint(distance);

                // Convert to grid position
                _currentGridPosition = GridManager.Instance.WorldToGrid(worldPosition);

                // Convert back to world position (snapped to grid)
                Vector3 snappedWorldPosition = GridManager.Instance.GridToWorld(_currentGridPosition);

                // Update ghost preview position
                _currentGhostPreview.UpdatePosition(snappedWorldPosition);

                // Validate placement
                PlacementValidator.ValidationInfo validationInfo = PlacementValidator.ValidatePlacement(
                    _currentGridPosition,
                    _currentPlacementData,
                    _currentRotation
                );

                // Update ghost preview validation state
                _currentGhostPreview.SetValid(validationInfo.IsValid);
                _lastValidationResult = validationInfo.IsValid;
            }
        }

        /// <summary>
        /// Creates the ghost preview for the current placement data.
        /// </summary>
        private void CreateGhostPreview()
        {
            if (_currentPlacementData == null || _currentPlacementData.prefab == null)
            {
                Debug.LogError("PlacementController: Cannot create ghost preview with null prefab");
                return;
            }

            // Create ghost preview game object
            GameObject ghostObject = new GameObject("GhostPreview");
            _currentGhostPreview = ghostObject.AddComponent<GhostPreview>();

            // Initialize ghost preview
            _currentGhostPreview.Initialize(
                _currentPlacementData.prefab,
                validPlacementMaterial,
                invalidPlacementMaterial
            );

            // Set initial rotation
            _currentGhostPreview.UpdateRotation(GetRotationForState(_currentRotation));
        }

        /// <summary>
        /// Destroys the current ghost preview.
        /// </summary>
        private void DestroyGhostPreview()
        {
            if (_currentGhostPreview != null)
            {
                _currentGhostPreview.DestroyPreview();
                _currentGhostPreview = null;
            }
        }

        /// <summary>
        /// Confirms the current placement if validation passes.
        /// </summary>
        private void ConfirmPlacement()
        {
            if (_currentPlacementData == null)
            {
                return;
            }

            // Validate placement one final time
            PlacementValidator.ValidationInfo validationInfo = PlacementValidator.ValidatePlacement(
                _currentGridPosition,
                _currentPlacementData,
                _currentRotation
            );

            if (!validationInfo.IsValid)
            {
                Debug.LogWarning($"Placement failed: {validationInfo.message}");
                return;
            }

            // Calculate world position and rotation
            Vector3 worldPosition = GridManager.Instance.GridToWorld(_currentGridPosition);
            Quaternion worldRotation = GetRotationForState(_currentRotation);

            // Apply pivot offset if specified
            worldPosition += worldRotation * _currentPlacementData.pivotOffset;

            // Instantiate the actual object
            GameObject placedObject = Instantiate(
                _currentPlacementData.prefab,
                worldPosition,
                worldRotation
            );

            placedObject.name = _currentPlacementData.objectName;

            // Update grid state
            GridManager.Instance.OccupyCells(
                _currentGridPosition,
                _currentPlacementData.gridSize,
                placedObject
            );

            Debug.Log($"Placed {_currentPlacementData.objectName} at grid position {_currentGridPosition}");

            // Exit placement mode
            ExitPlacementMode();
        }

        /// <summary>
        /// Rotates the ghost preview to the next rotation state.
        /// </summary>
        private void RotateGhostPreview()
        {
            // Increment rotation state (0 -> 1 -> 2 -> 3 -> 0)
            _currentRotation = (_currentRotation + 1) % 4;

            // Update ghost preview rotation
            if (_currentGhostPreview != null)
            {
                _currentGhostPreview.UpdateRotation(GetRotationForState(_currentRotation));
            }

            Debug.Log($"Rotated to {_currentRotation * 90}°");
        }

        /// <summary>
        /// Converts a rotation state (0-3) to a Quaternion rotation.
        /// </summary>
        /// <param name="rotationState">Rotation state (0=0°, 1=90°, 2=180°, 3=270°)</param>
        /// <returns>Quaternion representing the rotation</returns>
        private Quaternion GetRotationForState(int rotationState)
        {
            return Quaternion.Euler(0, rotationState * 90f, 0);
        }

        private void OnDestroy()
        {
            // Clean up ghost preview if it exists
            if (_currentGhostPreview != null)
            {
                DestroyGhostPreview();
            }
        }
    }
}
