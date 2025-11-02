using UnityEngine;

namespace SantasWorkshop.Data
{
    /// <summary>
    /// ScriptableObject that defines a machine type in the game.
    /// Contains configuration for machine behavior, costs, and requirements.
    /// </summary>
    [CreateAssetMenu(fileName = "New Machine", menuName = "Santa's Workshop/Machine", order = 3)]
    public class MachineData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique identifier for this machine")]
        public string machineId;

        [Tooltip("Display name shown to players")]
        public string displayName;

        [Tooltip("Description of what this machine does")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Icon representing this machine in the UI")]
        public Sprite icon;

        [Header("Machine Properties")]
        [Tooltip("Category of this machine")]
        public MachineCategory category = MachineCategory.Processor;

        [Tooltip("Prefab to instantiate when building this machine")]
        public GameObject prefab;

        [Tooltip("Power consumption in units per second")]
        [Range(0f, 1000f)]
        public float powerConsumption = 10f;

        [Tooltip("Processing speed multiplier (higher = faster)")]
        [Range(0.1f, 10f)]
        public float speedMultiplier = 1f;

        [Header("Building Cost")]
        [Tooltip("Resources required to build this machine")]
        public ResourceStack[] buildCost;

        [Tooltip("Time in seconds to construct this machine")]
        [Range(0.1f, 60f)]
        public float buildTime = 5f;

        [Header("Requirements")]
        [Tooltip("Research IDs required to unlock this machine")]
        public string[] requiredResearch;

        [Tooltip("Minimum tier/level required to build")]
        [Range(1, 10)]
        public int minimumTier = 1;

        [Header("Capacity")]
        [Tooltip("Input buffer size (for processors/assemblers)")]
        [Range(1, 100)]
        public int inputBufferSize = 10;

        [Tooltip("Output buffer size (for processors/assemblers)")]
        [Range(1, 100)]
        public int outputBufferSize = 10;

        [Header("Extractor Settings")]
        [Tooltip("Extraction rate for extractor machines (resources per second)")]
        [Range(0.1f, 100f)]
        public float extractionRate = 1f;

        [Tooltip("Extraction range for extractor machines")]
        [Range(1f, 50f)]
        public float extractionRange = 5f;

        [Header("Visual")]
        [Tooltip("Color tint for this machine in the UI")]
        public Color machineColor = Color.white;

        /// <summary>
        /// Validates the machine data on enable.
        /// </summary>
        private void OnValidate()
        {
            // Auto-generate machineId from asset name if empty
            if (string.IsNullOrEmpty(machineId))
            {
                machineId = name.ToLower().Replace(" ", "_");
            }

            // Ensure display name is set
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = name;
            }

            // Ensure build time is positive
            if (buildTime <= 0f)
            {
                buildTime = 0.1f;
            }
        }

        /// <summary>
        /// Checks if this machine is unlocked based on research.
        /// </summary>
        /// <param name="unlockedResearch">Array of unlocked research IDs</param>
        public bool IsUnlocked(string[] unlockedResearch)
        {
            if (requiredResearch == null || requiredResearch.Length == 0)
                return true; // No research requirements

            if (unlockedResearch == null || unlockedResearch.Length == 0)
                return false; // Has requirements but nothing unlocked

            foreach (var required in requiredResearch)
            {
                bool found = false;
                foreach (var unlocked in unlockedResearch)
                {
                    if (unlocked == required)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false; // Missing a required research
            }

            return true; // All requirements met
        }

        /// <summary>
        /// Returns a string representation of this machine.
        /// </summary>
        public override string ToString()
        {
            return $"{displayName} ({machineId})";
        }
    }
}
