using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Manages the visual preview of an object during placement mode.
    /// Displays a semi-transparent ghost of the object with color-coded validation feedback.
    /// </summary>
    public class GhostPreview : MonoBehaviour
    {
        private GameObject _previewObject;
        private List<Renderer> _renderers = new List<Renderer>();
        private Material _validMaterial;
        private Material _invalidMaterial;
        private bool _isValid = true;

        /// <summary>
        /// Initializes the ghost preview with the specified prefab and materials.
        /// </summary>
        /// <param name="prefab">Prefab to create preview from</param>
        /// <param name="validMat">Material to use when placement is valid (green)</param>
        /// <param name="invalidMat">Material to use when placement is invalid (red)</param>
        public void Initialize(GameObject prefab, Material validMat, Material invalidMat)
        {
            if (prefab == null)
            {
                Debug.LogError("GhostPreview: Cannot initialize with null prefab");
                return;
            }

            _validMaterial = validMat;
            _invalidMaterial = invalidMat;

            // Instantiate preview object as child
            _previewObject = Instantiate(prefab, transform);
            _previewObject.name = "PreviewObject";

            // Disable any scripts on the preview object
            DisableScripts(_previewObject);

            // Disable colliders on the preview object
            DisableColliders(_previewObject);

            // Collect all renderers
            CollectRenderers(_previewObject);

            // Apply initial materials
            ApplyMaterials();
        }

        /// <summary>
        /// Updates the position of the ghost preview.
        /// </summary>
        /// <param name="worldPosition">World space position to move preview to</param>
        public void UpdatePosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        /// <summary>
        /// Updates the rotation of the ghost preview.
        /// </summary>
        /// <param name="rotation">Rotation to apply to preview</param>
        public void UpdateRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        /// <summary>
        /// Sets whether the current placement is valid and updates visual feedback.
        /// </summary>
        /// <param name="isValid">True if placement is valid, false otherwise</param>
        public void SetValid(bool isValid)
        {
            if (_isValid != isValid)
            {
                _isValid = isValid;
                ApplyMaterials();
            }
        }

        /// <summary>
        /// Applies the appropriate material (valid or invalid) to all renderers.
        /// </summary>
        private void ApplyMaterials()
        {
            Material materialToApply = _isValid ? _validMaterial : _invalidMaterial;

            if (materialToApply == null)
            {
                Debug.LogWarning("GhostPreview: Material is null, cannot apply");
                return;
            }

            foreach (var renderer in _renderers)
            {
                if (renderer != null)
                {
                    // Create material array with the ghost material for all slots
                    Material[] materials = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = materialToApply;
                    }
                    renderer.sharedMaterials = materials;
                }
            }
        }

        /// <summary>
        /// Recursively collects all Renderer components from the preview object.
        /// </summary>
        private void CollectRenderers(GameObject obj)
        {
            // Get renderer on this object
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
            }

            // Recursively collect from children
            foreach (Transform child in obj.transform)
            {
                CollectRenderers(child.gameObject);
            }
        }

        /// <summary>
        /// Recursively disables all MonoBehaviour scripts on the preview object.
        /// </summary>
        private void DisableScripts(GameObject obj)
        {
            // Disable all MonoBehaviour components except this one
            MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script != this)
                {
                    script.enabled = false;
                }
            }

            // Recursively disable scripts on children
            foreach (Transform child in obj.transform)
            {
                DisableScripts(child.gameObject);
            }
        }

        /// <summary>
        /// Recursively disables all Collider components on the preview object.
        /// </summary>
        private void DisableColliders(GameObject obj)
        {
            // Disable all colliders
            Collider[] colliders = obj.GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            // Recursively disable colliders on children
            foreach (Transform child in obj.transform)
            {
                DisableColliders(child.gameObject);
            }
        }

        /// <summary>
        /// Destroys the ghost preview and cleans up resources.
        /// </summary>
        public void DestroyPreview()
        {
            if (_previewObject != null)
            {
                Destroy(_previewObject);
                _previewObject = null;
            }

            _renderers.Clear();

            // Destroy this component's game object
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            // Clean up preview object if it still exists
            if (_previewObject != null)
            {
                Destroy(_previewObject);
            }

            _renderers.Clear();
        }
    }
}
