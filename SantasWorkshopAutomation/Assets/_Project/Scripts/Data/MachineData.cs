using UnityEngine;
using System.Collections.Generic;

namespace SantasWorkshop.Data
{
    /// <summary>
    /// Configuration data for a machine type.
    /// Defines all properties, stats, and capabilities of a machine.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMachineData", menuName = "Santa/Machine Data", order = 2)]
    public class MachineData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Display name of the machine")]
        public string machineName;
        
        [Tooltip("Description of what this machine does")]
        [TextArea(3, 5)]
        public string description;
        
        [Tooltip("Icon displayed in UI")]
        public Sprite icon;
        
        [Header("Grid Properties")]
        [Tooltip("Size of the machine in grid cells (width x depth)")]
        public Vector2Int gridSize = new Vector2Int(1, 1);
        
        [Tooltip("Tier level of this machine (affects speed and efficiency)")]
        [Min(1)]
        public int tier = 1;
        
        [Header("Performance")]
        [Tooltip("Base processing speed multiplier (1.0 = normal speed)")]
        [Min(0.1f)]
        public float baseProcessingSpeed = 1f;
        
        [Tooltip("Base power consumption in watts")]
        [Min(0f)]
        public float basePowerConsumption = 10f;
        
        [Header("Ports")]
        [Tooltip("Number of input ports for receiving resources")]
        [Min(0)]
        public int inputPortCount = 1;
        
        [Tooltip("Number of output ports for sending resources")]
        [Min(0)]
        public int outputPortCount = 1;
        
        [Tooltip("Local positions of input ports relative to machine center")]
        public Vector3[] inputPortPositions;
        
        [Tooltip("Local positions of output ports relative to machine center")]
        public Vector3[] outputPortPositions;
        
        [Header("Buffers")]
        [Tooltip("Maximum capacity of each input/output buffer")]
        [Min(1)]
        public int bufferCapacity = 10;
        
        [Header("Recipes")]
        [Tooltip("List of recipes this machine can process")]
        public List<Recipe> availableRecipes = new List<Recipe>();
        
        [Header("Visual")]
        [Tooltip("Prefab to instantiate for this machine")]
        public GameObject prefab;
        
        /// <summary>
        /// Validates the machine data configuration in the Unity Editor.
        /// Automatically initializes port position arrays and checks for errors.
        /// </summary>
        private void OnValidate()
        {
            // Initialize input port positions if needed
            if (inputPortPositions == null || inputPortPositions.Length != inputPortCount)
            {
                inputPortPositions = new Vector3[inputPortCount];
                
                // Default input ports to left side of machine
                for (int i = 0; i < inputPortCount; i++)
                {
                    float yOffset = inputPortCount > 1 ? (i - (inputPortCount - 1) * 0.5f) * 0.5f : 0f;
                    inputPortPositions[i] = new Vector3(-0.5f, 0.5f, yOffset);
                }
            }
            
            // Initialize output port positions if needed
            if (outputPortPositions == null || outputPortPositions.Length != outputPortCount)
            {
                outputPortPositions = new Vector3[outputPortCount];
                
                // Default output ports to right side of machine
                for (int i = 0; i < outputPortCount; i++)
                {
                    float yOffset = outputPortCount > 1 ? (i - (outputPortCount - 1) * 0.5f) * 0.5f : 0f;
                    outputPortPositions[i] = new Vector3(0.5f, 0.5f, yOffset);
                }
            }
            
            // Validate machine name
            if (string.IsNullOrEmpty(machineName))
            {
                Debug.LogWarning($"MachineData '{name}' has no machine name set");
            }
            
            // Validate grid size
            if (gridSize.x <= 0 || gridSize.y <= 0)
            {
                Debug.LogWarning($"MachineData '{machineName}' has invalid grid size. Setting to 1x1");
                gridSize = new Vector2Int(1, 1);
            }
            
            // Validate tier
            if (tier < 1)
            {
                Debug.LogWarning($"MachineData '{machineName}' has tier < 1. Setting to 1");
                tier = 1;
            }
            
            // Validate processing speed
            if (baseProcessingSpeed <= 0)
            {
                Debug.LogWarning($"MachineData '{machineName}' has processing speed <= 0. Setting to 1.0");
                baseProcessingSpeed = 1f;
            }
            
            // Validate power consumption
            if (basePowerConsumption < 0)
            {
                Debug.LogWarning($"MachineData '{machineName}' has negative power consumption. Setting to 0");
                basePowerConsumption = 0f;
            }
            
            // Validate buffer capacity
            if (bufferCapacity < 1)
            {
                Debug.LogWarning($"MachineData '{machineName}' has buffer capacity < 1. Setting to 10");
                bufferCapacity = 10;
            }
            
            // Validate prefab
            if (prefab == null)
            {
                Debug.LogWarning($"MachineData '{machineName}' has no prefab assigned");
            }
            
            // Validate recipes
            if (availableRecipes != null)
            {
                for (int i = availableRecipes.Count - 1; i >= 0; i--)
                {
                    if (availableRecipes[i] == null)
                    {
                        Debug.LogWarning($"MachineData '{machineName}' has null recipe at index {i}. Removing.");
                        availableRecipes.RemoveAt(i);
                    }
                }
            }
            
            // Warn if machine has no ports
            if (inputPortCount == 0 && outputPortCount == 0)
            {
                Debug.LogWarning($"MachineData '{machineName}' has no input or output ports. This may be intentional for utility machines.");
            }
        }
    }
}
