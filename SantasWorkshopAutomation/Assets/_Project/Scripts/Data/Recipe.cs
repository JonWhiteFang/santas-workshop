using UnityEngine;

namespace SantasWorkshop.Data
{
    /// <summary>
    /// Defines a recipe for converting input resources into output resources.
    /// Used by machines to determine what they can produce.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "Santa/Recipe", order = 1)]
    public class Recipe : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this recipe")]
        public string recipeId;
        
        [Tooltip("Display name for this recipe")]
        public string recipeName;
        
        [Header("Inputs")]
        [Tooltip("Resources required to process this recipe")]
        public ResourceStack[] inputs;
        
        [Header("Outputs")]
        [Tooltip("Resources produced by this recipe")]
        public ResourceStack[] outputs;
        
        [Header("Processing")]
        [Tooltip("Time in seconds to complete this recipe")]
        [Min(0.1f)]
        public float processingTime = 1f;
        
        [Tooltip("Power consumption in watts while processing")]
        [Min(0f)]
        public float powerConsumption = 10f;
        
        [Header("Requirements")]
        [Tooltip("Minimum machine tier required to process this recipe")]
        [Min(1)]
        public int requiredTier = 1;
        
        /// <summary>
        /// Validates the recipe configuration in the Unity Editor.
        /// Logs warnings for common configuration errors.
        /// </summary>
        private void OnValidate()
        {
            // Auto-generate recipe ID from asset name if not set
            if (string.IsNullOrEmpty(recipeId))
            {
                recipeId = name;
            }
            
            // Validate inputs
            if (inputs == null || inputs.Length == 0)
            {
                Debug.LogWarning($"Recipe '{recipeName}' has no inputs! Recipes should have at least one input resource.");
            }
            else
            {
                // Check for invalid input amounts
                for (int i = 0; i < inputs.Length; i++)
                {
                    if (inputs[i].amount <= 0)
                    {
                        Debug.LogWarning($"Recipe '{recipeName}' has input at index {i} with amount <= 0");
                    }
                    if (string.IsNullOrEmpty(inputs[i].resourceId))
                    {
                        Debug.LogWarning($"Recipe '{recipeName}' has input at index {i} with empty resourceId");
                    }
                }
            }
            
            // Validate outputs
            if (outputs == null || outputs.Length == 0)
            {
                Debug.LogWarning($"Recipe '{recipeName}' has no outputs! Recipes should produce at least one output resource.");
            }
            else
            {
                // Check for invalid output amounts
                for (int i = 0; i < outputs.Length; i++)
                {
                    if (outputs[i].amount <= 0)
                    {
                        Debug.LogWarning($"Recipe '{recipeName}' has output at index {i} with amount <= 0");
                    }
                    if (string.IsNullOrEmpty(outputs[i].resourceId))
                    {
                        Debug.LogWarning($"Recipe '{recipeName}' has output at index {i} with empty resourceId");
                    }
                }
            }
            
            // Validate processing time
            if (processingTime <= 0)
            {
                Debug.LogWarning($"Recipe '{recipeName}' has processing time <= 0. Setting to minimum 0.1s");
                processingTime = 0.1f;
            }
            
            // Validate power consumption
            if (powerConsumption < 0)
            {
                Debug.LogWarning($"Recipe '{recipeName}' has negative power consumption. Setting to 0");
                powerConsumption = 0f;
            }
        }
    }
}
