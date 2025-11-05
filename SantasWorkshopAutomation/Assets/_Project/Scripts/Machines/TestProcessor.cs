using UnityEngine;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Test implementation of a processor machine for testing purposes.
    /// Provides basic functionality for testing the machine framework with recipes.
    /// </summary>
    public class TestProcessor : MachineBase
    {
        #region Serialized Fields
        
        [Header("Test Processor Settings")]
        [Tooltip("Whether to automatically select the first available recipe")]
        [SerializeField] private bool autoSelectRecipe = true;
        
        [Tooltip("Whether to show detailed debug logs")]
        [SerializeField] private bool verboseLogging = false;
        
        #endregion
        
        #region Lifecycle Overrides
        
        protected override void Start()
        {
            base.Start();
            
            // Auto-select first recipe if enabled
            if (autoSelectRecipe && machineData != null && machineData.availableRecipes != null && machineData.availableRecipes.Count > 0)
            {
                SetActiveRecipe(machineData.availableRecipes[0]);
                
                if (verboseLogging)
                {
                    Debug.Log($"TestProcessor {machineId} auto-selected recipe: {machineData.availableRecipes[0]?.recipeId}");
                }
            }
        }
        
        #endregion
        
        #region State Machine Overrides
        
        protected override void OnEnterIdle()
        {
            base.OnEnterIdle();
            
            if (verboseLogging)
            {
                Debug.Log($"TestProcessor {machineId} entered Idle state");
            }
        }
        
        protected override void OnEnterWaitingForInput()
        {
            base.OnEnterWaitingForInput();
            
            if (verboseLogging)
            {
                Debug.Log($"TestProcessor {machineId} entered WaitingForInput state");
            }
        }
        
        protected override void OnEnterProcessing()
        {
            base.OnEnterProcessing();
            
            if (verboseLogging)
            {
                Debug.Log($"TestProcessor {machineId} entered Processing state");
            }
        }
        
        protected override void OnEnterWaitingForOutput()
        {
            base.OnEnterWaitingForOutput();
            
            if (verboseLogging)
            {
                Debug.Log($"TestProcessor {machineId} entered WaitingForOutput state");
            }
        }
        
        protected override void OnEnterNoPower()
        {
            base.OnEnterNoPower();
            
            if (verboseLogging)
            {
                Debug.Log($"TestProcessor {machineId} entered NoPower state");
            }
        }
        
        protected override void OnEnterDisabled()
        {
            base.OnEnterDisabled();
            
            if (verboseLogging)
            {
                Debug.Log($"TestProcessor {machineId} entered Disabled state");
            }
        }
        
        protected override void SetVisualState(MachineState state)
        {
            base.SetVisualState(state);
            
            // Add visual feedback based on state
            switch (state)
            {
                case MachineState.Idle:
                    // Dim ambient light
                    break;
                case MachineState.WaitingForInput:
                    // Pulsing yellow indicator
                    break;
                case MachineState.Processing:
                    // Bright light, active particles, rotating parts
                    break;
                case MachineState.WaitingForOutput:
                    // Pulsing red indicator
                    break;
                case MachineState.NoPower:
                    // Flickering red light, warning icon
                    break;
                case MachineState.Disabled:
                    // Greyed out, no effects
                    break;
            }
        }
        
        #endregion
        
        #region Recipe Processing Overrides
        
        protected override void CompleteProcessing()
        {
            if (verboseLogging)
            {
                Debug.Log($"TestProcessor {machineId} completing recipe processing");
            }
            
            base.CompleteProcessing();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Manually adds a resource to the input buffer for testing.
        /// </summary>
        /// <param name="resourceId">The resource ID to add.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if the resource was added successfully.</returns>
        public bool AddInputResource(string resourceId, int amount)
        {
            if (inputPorts.Count == 0)
            {
                Debug.LogWarning($"TestProcessor {machineId} has no input ports!");
                return false;
            }
            
            // Try to add to first input port
            bool success = inputPorts[0].AddResource(resourceId, amount);
            
            if (success && verboseLogging)
            {
                Debug.Log($"TestProcessor {machineId} added {amount}x {resourceId} to input buffer");
            }
            
            return success;
        }
        
        /// <summary>
        /// Manually removes a resource from the output buffer for testing.
        /// </summary>
        /// <param name="resourceId">The resource ID to remove.</param>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>The actual amount removed.</returns>
        public int TakeOutputResource(string resourceId, int amount)
        {
            if (outputPorts.Count == 0)
            {
                Debug.LogWarning($"TestProcessor {machineId} has no output ports!");
                return 0;
            }
            
            // Try to remove from first output port
            int removed = outputPorts[0].ExtractResource(resourceId, amount);
            
            if (removed > 0 && verboseLogging)
            {
                Debug.Log($"TestProcessor {machineId} removed {removed}x {resourceId} from output buffer");
            }
            
            return removed;
        }
        
        /// <summary>
        /// Gets the amount of a specific resource in the input buffer.
        /// </summary>
        /// <param name="resourceId">The resource ID to check.</param>
        /// <returns>The amount of the resource in the input buffer.</returns>
        public int GetInputAmount(string resourceId)
        {
            return GetInputBufferAmount(resourceId);
        }
        
        /// <summary>
        /// Gets the amount of a specific resource in the output buffer.
        /// </summary>
        /// <param name="resourceId">The resource ID to check.</param>
        /// <returns>The amount of the resource in the output buffer.</returns>
        public int GetOutputAmount(string resourceId)
        {
            if (outputPorts.Count == 0) return 0;
            return outputPorts[0].GetResourceAmount(resourceId);
        }
        
        /// <summary>
        /// Clears all input buffers for testing.
        /// </summary>
        public void ClearInputBuffers()
        {
            foreach (var port in inputPorts)
            {
                port.Clear();
            }
            
            if (verboseLogging)
            {
                Debug.Log($"TestProcessor {machineId} cleared all input buffers");
            }
        }
        
        /// <summary>
        /// Clears all output buffers for testing.
        /// </summary>
        public void ClearOutputBuffers()
        {
            foreach (var port in outputPorts)
            {
                port.Clear();
            }
            
            if (verboseLogging)
            {
                Debug.Log($"TestProcessor {machineId} cleared all output buffers");
            }
        }
        
        /// <summary>
        /// Sets whether to show verbose logging.
        /// </summary>
        /// <param name="enabled">True to enable verbose logging.</param>
        public void SetVerboseLogging(bool enabled)
        {
            verboseLogging = enabled;
        }
        
        /// <summary>
        /// Forces the machine to check if it can start processing.
        /// Useful for testing state transitions.
        /// </summary>
        public void ForceCheckProcessing()
        {
            if (CurrentState == MachineState.Idle || CurrentState == MachineState.WaitingForInput)
            {
                UpdateIdle();
            }
        }
        
        #endregion
        
        #region Validation
        
        protected override void ValidateConfiguration()
        {
            base.ValidateConfiguration();
            
            // Validate processor-specific configuration
            if (inputPorts.Count == 0)
            {
                Debug.LogWarning($"TestProcessor {machineId} has no input ports!");
            }
            
            if (outputPorts.Count == 0)
            {
                Debug.LogWarning($"TestProcessor {machineId} has no output ports!");
            }
            
            if (machineData != null && (machineData.availableRecipes == null || machineData.availableRecipes.Count == 0))
            {
                Debug.LogWarning($"TestProcessor {machineId} has no available recipes!");
            }
        }
        
        #endregion
        
        #region Debug
        
        public override string ToString()
        {
            return $"TestProcessor[{machineId}] State:{CurrentState} Progress:{ProcessingProgress:P0} Powered:{IsPowered}";
        }
        
        #endregion
    }
}
