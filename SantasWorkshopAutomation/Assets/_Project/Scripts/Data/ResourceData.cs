using UnityEngine;

namespace SantasWorkshop.Data
{
    /// <summary>
    /// ScriptableObject that defines a resource type in the game.
    /// Resources can be raw materials, refined goods, components, toys, or magical items.
    /// </summary>
    [CreateAssetMenu(fileName = "NewResource", menuName = "Santa/Resource Data", order = 1)]
    public class ResourceData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique identifier for this resource")]
        public string resourceId;

        [Tooltip("Display name shown to players")]
        public string displayName;

        [Tooltip("Description of the resource")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Icon representing this resource in the UI")]
        public Sprite icon;

        [Header("Properties")]
        [Tooltip("Category of this resource")]
        public ResourceCategory category = ResourceCategory.RawMaterial;

        [Tooltip("Maximum stack size for this resource")]
        [Range(1, 10000)]
        public int stackSize = 100;

        [Tooltip("Weight for logistics calculations (in kg)")]
        [Range(0.1f, 100f)]
        public float weight = 1f;

        [Tooltip("Base value of this resource (for trading/scoring)")]
        public int baseValue = 10;

        [Header("Visual")]
        [Tooltip("3D model for items on conveyors")]
        public GameObject itemPrefab;

        [Tooltip("Color tint for the item")]
        public Color itemColor = Color.white;

        [Header("Behavior")]
        [Tooltip("Can this resource be stored in containers?")]
        public bool canBeStored = true;

        [Tooltip("Can this resource be transported on conveyors?")]
        public bool canBeTransported = true;

        /// <summary>
        /// Validates the resource data in the editor.
        /// Ensures resourceId is not empty and stackSize is greater than 0.
        /// </summary>
        private void OnValidate()
        {
            // Validate resourceId is not empty
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                Debug.LogWarning($"ResourceData {name} has empty resourceId!");
            }

            // Validate stackSize is greater than 0
            if (stackSize <= 0)
            {
                Debug.LogWarning($"ResourceData {name} has invalid stackSize! Setting to 1.");
                stackSize = 1;
            }

            // Ensure display name is set
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = name;
            }
        }

        /// <summary>
        /// Returns a string representation of this resource.
        /// </summary>
        public override string ToString()
        {
            return $"{displayName} ({resourceId})";
        }
    }
}
