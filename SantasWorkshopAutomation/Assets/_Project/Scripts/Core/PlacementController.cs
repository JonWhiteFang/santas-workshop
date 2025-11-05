using UnityEngine;
using UnityEngine.InputSystem;
using System;
using SantasWorkshop.Data;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Manages placement mode state, user input, and orchestrates placement operations.
    /// Handles ghost preview updates, rotation, validation, and placement confirmation.
    /// Uses Unity's new Input System for input handling.
    /// </summary>
    public class PlacementController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private bool useLegacyInput = false; // Toggle for backward compatibility

        [Header("Materials")]
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;

        [Header("Camera")]
        [SerializeField] private Camera mainCamera;

        [Header("Audio")]
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip rotationSound;
        [SerializeField] private float audioVolume = 0.5f;

        private AudioSource _audioSource;
        private float _lastErrorSoundTime;
        private const float ERROR_SOUND_COOLDOWN = 0.1f;

        // Input System
        private PlacementInputActions _inputActions;
        private Vector2 _mousePosition;

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

        #region Events

        /// <summary>
        /// Event triggered when an object is successfully placed.
        /// Parameters: placedObject, gridPosition, rotation
        /// </summary>
        public event Action<GameObject, Vector3Int, int> OnPlacementConfirmed;

        /// <summary>
        /// Event triggered when placement is cancelled.
        /// </summary>
        public event Action OnPlacementCancelled;

        /// <summary>
        /// Event triggered when entering placement mode.
        /// Parameters: placementData
        /// </summary>
        public event Action<PlacementData> OnPlacementModeEntered;

        /// <summary>
        /// Event triggered when exiting placement mode.
        /// </summary>
        public event Action OnPlacementModeExited;

        #endregion

        private void Awake()
        {
            // Get main camera if not assigned
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            // Create audio source component
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.volume = audioVolume;

            // Initialize Input System
            if (!useLegacyInput)
            {
                _inputActions = new PlacementInputActions();
                _inputActions.Placement.Confirm.performed += OnConfirmPerformed;
                _inputActions.Placement.Cancel.performed += OnCancelPerformed;
                _inputActions.Placement.Rotate.performed += OnRotatePerformed;
                _inputActions.Placement.MousePosition.performed += OnMousePositionPerformed;
            }
        }

        private void Update()
        {
            if (_isInPlacementMode)
            {
                // Handle legacy input if enabled
                if (useLegacyInput)
                {
                    HandleRotationInputLegacy();
                    HandleConfirmInputLegacy();
                    HandleCancelInputLegacy();
                }

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

            // Enable input actions
            if (!useLegacyInput)
            {
                _inputActions.Placement.Enable();
            }

            CreateGhostPreview();

            // Trigger event
            OnPlacementModeEntered?.Invoke(placementData);

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

            // Disable input actions
            if (!useLegacyInput)
            {
                _inputActions.Placement.Disable();
            }

            // Trigger event
            OnPlacementModeExited?.Invoke();

            Debug.Log("Exited placement mode");
        }

        /// <summary>
        /// Input System callback for confirm action.
        /// </summary>
        private void OnConfirmPerformed(InputAction.CallbackContext context)
        {
            if (_isInPlacementMode)
            {
                ConfirmPlacement();
            }
        }

        /// <summary>
        /// Input System callback for cancel action.
        /// </summary>
        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            if (_isInPlacementMode)
            {
                // Trigger cancellation event
                OnPlacementCancelled?.Invoke();
                ExitPlacementMode();
            }
        }

        /// <summary>
        /// Input System callback for rotate action.
        /// </summary>
        private void OnRotatePerformed(InputAction.CallbackContext context)
        {
            if (_isInPlacementMode && _currentPlacementData != null && _currentPlacementData.canRotate)
            {
                RotateGhostPreview();
                PlayRotationSound();
            }
        }

        /// <summary>
        /// Input System callback for mouse position updates.
        /// </summary>
        private void OnMousePositionPerformed(InputAction.CallbackContext context)
        {
            _mousePosition = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// Handles rotation input from the player (legacy input).
        /// </summary>
        private void HandleRotationInputLegacy()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (_currentPlacementData != null && _currentPlacementData.canRotate)
                {
                    RotateGhostPreview();
                    PlayRotationSound();
                }
            }
        }

        /// <summary>
        /// Handles placement confirmation input (left mouse click) (legacy input).
        /// </summary>
        private void HandleConfirmInputLegacy()
        {
            if (Input.GetMouseButtonDown(0))
            {
                ConfirmPlacement();
            }
        }

        /// <summary>
        /// Handles placement cancellation input (cancel key or right mouse click) (legacy input).
        /// </summary>
        private void HandleCancelInputLegacy()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                // Trigger cancellation event
                OnPlacementCancelled?.Invoke();
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
            Vector2 screenPosition = useLegacyInput ? (Vector2)Input.mousePosition : _mousePosition;
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
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
                bool currentValidationResult = validationInfo.IsValid;
                _currentGhostPreview.SetValid(currentValidationResult);

                // Play error sound if validation changed from valid to invalid
                if (_lastValidationResult && !currentValidationResult)
                {
                    PlayErrorSound();
                }

                _lastValidationResult = currentValidationResult;
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
                PlayErrorSound();
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

            // Play success sound
            PlaySuccessSound();

            // Trigger placement confirmed event
            OnPlacementConfirmed?.Invoke(placedObject, _currentGridPosition, _currentRotation);

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

        /// <summary>
        /// Plays the error sound with cooldown to prevent audio spam.
        /// </summary>
        private void PlayErrorSound()
        {
            if (errorSound == null || _audioSource == null)
            {
                return;
            }

            // Check cooldown
            float currentTime = Time.time;
            if (currentTime - _lastErrorSoundTime < ERROR_SOUND_COOLDOWN)
            {
                return;
            }

            _audioSource.PlayOneShot(errorSound, audioVolume);
            _lastErrorSoundTime = currentTime;
        }

        /// <summary>
        /// Plays the success sound when placement is confirmed.
        /// </summary>
        private void PlaySuccessSound()
        {
            if (successSound == null || _audioSource == null)
            {
                return;
            }

            _audioSource.PlayOneShot(successSound, audioVolume);
        }

        /// <summary>
        /// Plays the rotation sound when the object is rotated.
        /// </summary>
        private void PlayRotationSound()
        {
            if (rotationSound == null || _audioSource == null)
            {
                return;
            }

            _audioSource.PlayOneShot(rotationSound, audioVolume);
        }

        private void OnDestroy()
        {
            // Clean up ghost preview if it exists
            if (_currentGhostPreview != null)
            {
                DestroyGhostPreview();
            }

            // Clean up input actions
            if (_inputActions != null)
            {
                _inputActions.Placement.Confirm.performed -= OnConfirmPerformed;
                _inputActions.Placement.Cancel.performed -= OnCancelPerformed;
                _inputActions.Placement.Rotate.performed -= OnRotatePerformed;
                _inputActions.Placement.MousePosition.performed -= OnMousePositionPerformed;
                _inputActions.Dispose();
            }
        }
    }
}
