using UnityEngine;
using SantasWorkshop.Data;
using SantasWorkshop.Core;

namespace SantasWorkshop.Machines.Examples
{
    /// <summary>
    /// Example implementation of a mining drill that extracts ore from resource nodes.
    /// This demonstrates proper inheritance from ExtractorBase.
    /// </summary>
    /// <remarks>
    /// Key Features:
    /// - Extracts resources from nearby resource nodes
    /// - Continuous production when powered
    /// - Visual feedback (drill animation, particles)
    /// - Automatic resource delivery to ResourceManager
    /// </remarks>
    public class ExampleMiningDrill : ExtractorBase
    {
        #region Serialized Fields

        [Header("Mining Drill Settings")]
        [Tooltip("Type of ore this drill extracts")]
        [SerializeField] private ResourceType oreType = ResourceType.IronOre;

        [Tooltip("Amount of ore extracted per cycle")]
        [SerializeField] private int orePerExtraction = 1;

        [Header("Visual Feedback")]
        [Tooltip("Particle system for drilling effect")]
        [SerializeField] private ParticleSystem drillingParticles;

        [Tooltip("Animator for drill animation")]
        [SerializeField] private Animator drillAnimator;

        [Tooltip("Audio source for drilling sound")]
        [SerializeField] private AudioSource drillAudio;

        #endregion

        #region Private Fields

        private bool isDrilling;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the mining drill with configuration data.
        /// Always call base.Initialize() first!
        /// </summary>
        public override void Initialize(MachineData data)
        {
            // IMPORTANT: Always call base.Initialize() first
            base.Initialize(data);

            // Custom initialization
            isDrilling = false;

            // Cache components if not assigned
            if (drillingParticles == null)
            {
                drillingParticles = GetComponentInChildren<ParticleSystem>();
            }

            if (drillAnimator == null)
            {
                drillAnimator = GetComponentInChildren<Animator>();
            }

            if (drillAudio == null)
            {
                drillAudio = GetComponent<AudioSource>();
            }

            // Log initialization (only when debug is enabled)
            if (showDebugInfo)
            {
                Debug.Log($"[ExampleMiningDrill] {MachineId} initialized for {oreType}");
            }
        }

        #endregion

        #region Resource Extraction

        /// <summary>
        /// Called when a resource unit has been extracted.
        /// Override to handle resource delivery.
        /// </summary>
        protected override void OnResourceExtracted()
        {
            // IMPORTANT: Call base method to maintain functionality
            base.OnResourceExtracted();

            // Add ore to ResourceManager
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(oreType, orePerExtraction);

                if (showDebugInfo)
                {
                    Debug.Log($"[ExampleMiningDrill] {MachineId} extracted {orePerExtraction}x {oreType}");
                }
            }
            else
            {
                Debug.LogWarning($"[ExampleMiningDrill] {MachineId} ResourceManager not found!");
            }

            // Play extraction effect
            PlayExtractionEffect();
        }

        /// <summary>
        /// Plays visual and audio effects for extraction.
        /// </summary>
        private void PlayExtractionEffect()
        {
            // Burst particles
            if (drillingParticles != null)
            {
                drillingParticles.Emit(10);
            }

            // Play sound
            if (drillAudio != null && drillAudio.clip != null)
            {
                drillAudio.PlayOneShot(drillAudio.clip);
            }
        }

        #endregion

        #region Visual Feedback

        /// <summary>
        /// Update visual elements based on machine state.
        /// Called automatically by base class.
        /// </summary>
        protected override void UpdateVisuals()
        {
            // IMPORTANT: Call base method first
            base.UpdateVisuals();

            // Update drilling state
            bool shouldDrill = currentState == MachineState.Working && isPowered;

            if (shouldDrill != isDrilling)
            {
                isDrilling = shouldDrill;

                // Update animator
                if (drillAnimator != null)
                {
                    drillAnimator.SetBool("IsDrilling", isDrilling);
                }

                // Update particles
                if (drillingParticles != null)
                {
                    if (isDrilling)
                    {
                        if (!drillingParticles.isPlaying)
                            drillingParticles.Play();
                    }
                    else
                    {
                        if (drillingParticles.isPlaying)
                            drillingParticles.Stop();
                    }
                }

                // Update audio
                if (drillAudio != null)
                {
                    if (isDrilling)
                    {
                        if (!drillAudio.isPlaying)
                            drillAudio.Play();
                    }
                    else
                    {
                        if (drillAudio.isPlaying)
                            drillAudio.Stop();
                    }
                }
            }
        }

        /// <summary>
        /// Handle state changes for visual feedback.
        /// </summary>
        protected override void OnStateChanged(MachineState oldState, MachineState newState)
        {
            // IMPORTANT: Call base method
            base.OnStateChanged(oldState, newState);

            // Custom state change handling
            if (showDebugInfo)
            {
                Debug.Log($"[ExampleMiningDrill] {MachineId} state: {oldState} → {newState}");
            }

            // Update visuals when state changes
            UpdateVisuals();
        }

        /// <summary>
        /// Handle power status changes.
        /// </summary>
        protected override void OnPowerStatusChanged(bool powered)
        {
            // IMPORTANT: Call base method
            base.OnPowerStatusChanged(powered);

            // Custom power change handling
            if (showDebugInfo)
            {
                Debug.Log($"[ExampleMiningDrill] {MachineId} power: {powered}");
            }

            // Update visuals when power changes
            UpdateVisuals();
        }

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Clean up when destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            // Stop effects
            if (drillingParticles != null && drillingParticles.isPlaying)
            {
                drillingParticles.Stop();
            }

            if (drillAudio != null && drillAudio.isPlaying)
            {
                drillAudio.Stop();
            }

            // IMPORTANT: Call base method last
            base.OnDestroy();
        }

        #endregion
    }
}
