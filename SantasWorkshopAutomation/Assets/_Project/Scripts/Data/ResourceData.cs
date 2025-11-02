using UnityEngine;

namespace SantasWorkshop.Data
{
    /// <summary>
    /// ScriptableObject that defines a resource type in the game.
    /// Resources can be raw materials, refined goods, components, toys, or magical items.
    /// </summary>
    [CreateAssetMenu(fileName = "New Resource", menuName = "Santa's Workshop/Resource", order = 1)]
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
        [Range(1, 1000)]
        public int stackSize = 100;

        [Tooltip("Base value of this resource (for trading/scoring)")]
        public int baseValue = 1;

        [Header("Visual")]
        [Tooltip("Color tint for this resource in the UI")]
        public Color resourceColor = Color.white;

        /// <summary>
        /// Validates the resource data on enable.
        /// </summary>
        private void OnValidate()
        {
            // Auto-generate resourceId from asset name if empty
            if (string.IsNullOrEmpty(resourceId))
            {
                resourceId = name.ToLower().Replace(" ", "_");
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
