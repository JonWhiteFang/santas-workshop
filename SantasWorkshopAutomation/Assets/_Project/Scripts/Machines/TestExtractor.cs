using UnityEngine;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Test implementation of an extractor machine for testing purposes.
    /// Provides basic functionality for testing the machine framework.
    /// </summary>
    public class TestExtractor : MachineBase
    {
        #region Serialized Fields
        
        [Header("Test Extractor Settings")]
        [Tooltip("Resource type to extract")]
        [SerializeField] private string extractedResourceId = "IronOre";
        
        [Tooltip("Amount of resource to extract per cycle")]
        [SerializeField] private int extractionAmount = 1;
        
        [Tooltip("Time between extractions in seconds")]
        [SerializeField] private float extractionInterval = 2f;
        
        #endregion
        
        #region Private Fields
        
        private float _timeSinceLastExtraction = 0f;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Gets the resource ID being extracted.
        /// </summary>
        public string ExtractedResourceId => extractedResourceId;
        
        /// <summary>
        /// Gets the amount extracted per cycle.
        /// </summary>
        public int ExtractionAmount => extractionAmount;
        
        #endregion
        
        #region Lifecycle Overrides
        
        protected override void Update()
        {
            if (!isEnabled) return;
            
            // Update extraction timer
            if (currentState == MachineState.Processing)
            {
                _timeSinceLastExtraction += Time.deltaTime;
                
                // Check if it's time to extract
                if (_timeSinceLastExtraction >= extractionInterval)
                {
                    _timeSinceLastExtraction = 0f;
                    ExtractResource();
                }
            }
            
            // Call base update for state machine
            base.Update();
        }
        
        #endregion
        
        #region State Machine Overrides
        
        protected override void OnEnterIdle()
        {
            base.OnEnterIdle();
            _timeSinceLastExtraction = 0f;
        }
        
        protected override void OnEnterProcessing()
        {
            base.OnEnterProcessing();
            _timeSinceLastExtraction = 0f;
        }
        
        protected override void UpdateIdle()
        {
            // Extractors don't need recipes, they just extract
            // Check if we have power and output space
            if (isPowered && CanAddToOutputBuffer(extractedResourceId, extractionAmount))
            {
                TransitionToState(MachineState.Processing);
            }
        }
        
        protected override void UpdateProcessing()
        {
            // Check if we still have power and output space
            if (!isPowered)
            {
                // Power loss is handled by SetPowered
                return;
            }
            
            if (!CanAddToOutputBuffer(extractedResourceId, extractionAmount))
            {
                TransitionToState(MachineState.WaitingForOutput);
            }
        }
        
        protected override void SetVisualState(MachineState state)
        {
            base.SetVisualState(state);
            
            // Add visual feedback based on state
            switch (state)
            {
                case MachineState.Idle:
                    // Dim lights, no effects
                    break;
                case MachineState.Processing:
                    // Bright lights, extraction effects
                    break;
                case MachineState.WaitingForOutput:
                    // Pulsing red indicator
                    break;
                case MachineState.NoPower:
                    // Flickering red light
                    break;
                case MachineState.Disabled:
                    // Greyed out
                    break;
            }
        }
        
        #endregion
        
        #region Extraction Logic
        
        /// <summary>
        /// Extracts resources and adds them to the output buffer.
        /// </summary>
        private void ExtractResource()
        {
            if (string.IsNullOrEmpty(extractedResourceId))
            {
                Debug.LogWarning($"TestExtractor {machineId} has no resource ID set!");
                return;
            }
            
            // Try to add to output buffer
            bool success = AddToOutputBuffer(extractedResourceId, extractionAmount);
            
            if (success)
            {
                Debug.Log($"TestExtractor {machineId} extracted {extractionAmount}x {extractedResourceId}");
            }
            else
            {
                Debug.LogWarning($"TestExtractor {machineId} failed to add resource to output buffer");
                TransitionToState(MachineState.WaitingForOutput);
            }
        }
        
        /// <summary>
        /// Sets the resource type to extract.
        /// </summary>
        /// <param name="resourceId">The resource ID to extract.</param>
        public void SetExtractedResource(string resourceId)
        {
            extractedResourceId = resourceId;
        }
        
        /// <summary>
        /// Sets the extraction amount per cycle.
        /// </summary>
        /// <param name="amount">The amount to extract.</param>
        public void SetExtractionAmount(int amount)
        {
            if (amount > 0)
            {
                extractionAmount = amount;
            }
        }
        
        /// <summary>
        /// Sets the extraction interval.
        /// </summary>
        /// <param name="interval">The time between extractions in seconds.</param>
        public void SetExtractionInterval(float interval)
        {
            if (interval > 0)
            {
                extractionInterval = interval;
            }
        }
        
        #endregion
        
        #region Validation
        
        protected override void ValidateConfiguration()
        {
            base.ValidateConfiguration();
            
            // Validate extractor-specific configuration
            if (string.IsNullOrEmpty(extractedResourceId))
            {
                Debug.LogWarning($"TestExtractor {machineId} has no extracted resource ID set!");
            }
            
            if (extractionAmount <= 0)
            {
                Debug.LogWarning($"TestExtractor {machineId} has invalid extraction amount: {extractionAmount}. Setting to 1.");
                extractionAmount = 1;
            }
            
            if (extractionInterval <= 0)
            {
                Debug.LogWarning($"TestExtractor {machineId} has invalid extraction interval: {extractionInterval}. Setting to 2.0s.");
                extractionInterval = 2f;
            }
            
            // Extractors should have at least one output port
            if (outputPorts.Count == 0)
            {
                Debug.LogError($"TestExtractor {machineId} has no output ports!");
            }
        }
        
        #endregion
        
        #region Debug
        
        public override string ToString()
        {
            return $"TestExtractor[{machineId}] State:{currentState} Resource:{extractedResourceId} Amount:{extractionAmount} Powered:{isPowered}";
        }
        
        #endregion
    }
}
