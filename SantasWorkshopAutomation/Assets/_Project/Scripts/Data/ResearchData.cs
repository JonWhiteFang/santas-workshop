using UnityEngine;

namespace SantasWorkshop.Data
{
    /// <summary>
    /// ScriptableObject that defines a research node in the tech tree.
    /// Research unlocks new machines, recipes, and gameplay features.
    /// </summary>
    [CreateAssetMenu(fileName = "New Research", menuName = "Santa's Workshop/Research", order = 4)]
    public class ResearchData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique identifier for this research")]
        public string researchId;

        [Tooltip("Display name shown to players")]
        public string displayName;

        [Tooltip("Description of what this research unlocks")]
        [TextArea(3, 6)]
        public string description;

        [Tooltip("Icon representing this research in the UI")]
        public Sprite icon;

        [Header("Research Properties")]
        [Tooltip("Research branch this belongs to")]
        public ResearchBranch branch = ResearchBranch.Automation;

        [Tooltip("Research points required to complete")]
        [Range(1, 10000)]
        public int researchPointCost = 100;

        [Tooltip("Time in seconds to complete this research")]
        [Range(1f, 3600f)]
        public float researchTime = 60f;

        [Header("Tech Tree")]
        [Tooltip("Research IDs that must be completed before this one")]
        public string[] prerequisites;

        [Tooltip("Position in the tech tree UI (X, Y)")]
        public Vector2 treePosition;

        [Tooltip("Tier/level of this research (for visual organization)")]
        [Range(1, 10)]
        public int tier = 1;

        [Header("Unlocks")]
        [Tooltip("Machine IDs unlocked by this research")]
        public string[] unlockedMachines;

        [Tooltip("Recipe IDs unlocked by this research")]
        public string[] unlockedRecipes;

        [Tooltip("Feature flags unlocked by this research")]
        public string[] unlockedFeatures;

        [Header("Bonuses")]
        [Tooltip("Global speed multiplier bonus (e.g., 0.1 = +10% speed)")]
        [Range(0f, 2f)]
        public float speedBonus = 0f;

        [Tooltip("Global efficiency multiplier bonus (e.g., 0.1 = +10% efficiency)")]
        [Range(0f, 2f)]
        public float efficiencyBonus = 0f;

        [Tooltip("Power consumption reduction (e.g., 0.1 = -10% power usage)")]
        [Range(0f, 0.5f)]
        public float powerReduction = 0f;

        [Header("Visual")]
        [Tooltip("Color for this research node in the tech tree")]
        public Color nodeColor = Color.cyan;

        /// <summary>
        /// Validates the research data on enable.
        /// </summary>
        private void OnValidate()
        {
            // Auto-generate researchId from asset name if empty
            if (string.IsNullOrEmpty(researchId))
            {
                researchId = name.ToLower().Replace(" ", "_");
            }

            // Ensure display name is set
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = name;
            }

            // Ensure costs are positive
            if (researchPointCost <= 0)
            {
                researchPointCost = 1;
            }

            if (researchTime <= 0f)
            {
                researchTime = 1f;
            }
        }

        /// <summary>
        /// Checks if this research can be started based on completed research.
        /// </summary>
        /// <param name="completedResearch">Array of completed research IDs</param>
        public bool CanResearch(string[] completedResearch)
        {
            if (prerequisites == null || prerequisites.Length == 0)
                return true; // No prerequisites

            if (completedResearch == null || completedResearch.Length == 0)
                return false; // Has prerequisites but nothing completed

            foreach (var prerequisite in prerequisites)
            {
                bool found = false;
                foreach (var completed in completedResearch)
                {
                    if (completed == prerequisite)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false; // Missing a prerequisite
            }

            return true; // All prerequisites met
        }

        /// <summary>
        /// Gets the total number of items unlocked by this research.
        /// </summary>
        public int GetTotalUnlocks()
        {
            int count = 0;

            if (unlockedMachines != null)
                count += unlockedMachines.Length;

            if (unlockedRecipes != null)
                count += unlockedRecipes.Length;

            if (unlockedFeatures != null)
                count += unlockedFeatures.Length;

            return count;
        }

        /// <summary>
        /// Checks if this research provides any bonuses.
        /// </summary>
        public bool HasBonuses()
        {
            return speedBonus > 0f || efficiencyBonus > 0f || powerReduction > 0f;
        }

        /// <summary>
        /// Returns a string representation of this research.
        /// </summary>
        public override string ToString()
        {
            return $"{displayName} ({researchId}) - {branch}";
        }
    }
}
