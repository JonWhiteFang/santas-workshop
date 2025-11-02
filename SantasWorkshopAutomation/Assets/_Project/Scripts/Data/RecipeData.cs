using UnityEngine;

namespace SantasWorkshop.Data
{
    /// <summary>
    /// ScriptableObject that defines a crafting recipe.
    /// Recipes specify input resources, output resources, processing time, and power requirements.
    /// </summary>
    [CreateAssetMenu(fileName = "New Recipe", menuName = "Santa's Workshop/Recipe", order = 2)]
    public class RecipeData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique identifier for this recipe")]
        public string recipeId;

        [Tooltip("Display name shown to players")]
        public string displayName;

        [Tooltip("Description of what this recipe produces")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Icon representing this recipe in the UI")]
        public Sprite icon;

        [Header("Recipe Configuration")]
        [Tooltip("Resources required as input")]
        public ResourceStack[] inputs;

        [Tooltip("Resources produced as output")]
        public ResourceStack[] outputs;

        [Tooltip("Time in seconds to process this recipe")]
        [Range(0.1f, 300f)]
        public float processingTime = 1f;

        [Tooltip("Power required to process this recipe (units per second)")]
        [Range(0f, 1000f)]
        public float powerRequired = 10f;

        [Header("Requirements")]
        [Tooltip("Machine categories that can use this recipe")]
        public MachineCategory[] allowedMachineCategories;

        [Tooltip("Research IDs required to unlock this recipe")]
        public string[] requiredResearch;

        [Header("Quality")]
        [Tooltip("Base quality multiplier for outputs (affects toy grading)")]
        [Range(0.5f, 2f)]
        public float qualityMultiplier = 1f;

        /// <summary>
        /// Validates the recipe data on enable.
        /// </summary>
        private void OnValidate()
        {
            // Auto-generate recipeId from asset name if empty
            if (string.IsNullOrEmpty(recipeId))
            {
                recipeId = name.ToLower().Replace(" ", "_");
            }

            // Ensure display name is set
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = name;
            }

            // Ensure processing time is positive
            if (processingTime <= 0f)
            {
                processingTime = 0.1f;
            }
        }

        /// <summary>
        /// Checks if this recipe can be used by a specific machine category.
        /// </summary>
        public bool IsAllowedForMachine(MachineCategory category)
        {
            if (allowedMachineCategories == null || allowedMachineCategories.Length == 0)
                return true; // No restrictions

            foreach (var allowed in allowedMachineCategories)
            {
                if (allowed == category)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a string representation of this recipe.
        /// </summary>
        public override string ToString()
        {
            return $"{displayName} ({recipeId})";
        }
    }
}
