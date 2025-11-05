using UnityEngine;
using SantasWorkshop.Data;
using SantasWorkshop.Core;

namespace SantasWorkshop.Machines.Examples
{
    /// <summary>
    /// Example implementation of a mining drill that extracts ore from resource nodes.
    /// This demonstrates proper inheritance from ExtractorBase.
    /// </summary>
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

        protected override void Start()
        {
            base.Start();

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

            Debug.Log($"[ExampleMiningDrill] {MachineId} initialized for {oreType}");
        }

        #endregion

        #region Resource Extraction

        /// <summary>
        /// Called when a resource unit has been extracted.
        /// </summary>
        protected override void OnResourceExtracted()
        {
            base.OnResourceExtracted();

            // Add ore to ResourceManager
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(oreType.ToString(), orePerExtraction);
                Debug.Log($"[ExampleMiningDrill] {MachineId} extracted {orePerExtraction}x {oreType}");
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
        /// </summary>
        private void UpdateVisualEffects()
        {
            // Update drilling state
            bool shouldDrill = CurrentState == MachineState.Processing && IsPowered;

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

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        private new void Update()
        {
            base.Update();
            UpdateVisualEffects();
        }

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

            base.OnDestroy();
        }

        #endregion
    }
}
