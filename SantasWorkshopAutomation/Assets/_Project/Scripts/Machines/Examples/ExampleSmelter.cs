using UnityEngine;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines.Examples
{
    /// <summary>
    /// Example implementation of a smelter that processes ore into ingots.
    /// This demonstrates proper inheritance from ProcessorBase.
    /// </summary>
    public class ExampleSmelter : ProcessorBase
    {
        #region Serialized Fields

        [Header("Smelter Visual Effects")]
        [Tooltip("Particle system for fire effect")]
        [SerializeField] private ParticleSystem fireEffect;

        [Tooltip("Light component for furnace glow")]
        [SerializeField] private Light furnaceLight;

        [Header("Audio")]
        [Tooltip("Sound played when smelting completes")]
        [SerializeField] private AudioClip completionSound;

        [Tooltip("Looping sound while smelting")]
        [SerializeField] private AudioClip smeltingLoop;

        #endregion

        #region Private Fields

        private AudioSource audioSource;

        #endregion

        #region Initialization

        protected override void Awake()
        {
            base.Awake();

            // Cache components
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        protected override void Start()
        {
            base.Start();

            // Set default recipe if available
            if (machineData != null && machineData.availableRecipes != null && machineData.availableRecipes.Count > 0)
            {
                SetRecipe(machineData.availableRecipes[0]);
                Debug.Log($"[ExampleSmelter] {MachineId} default recipe: {machineData.availableRecipes[0].recipeName}");
            }
        }

        #endregion

        #region Recipe Processing

        /// <summary>
        /// Called when processing completes.
        /// </summary>
        protected override void CompleteProcessing()
        {
            base.CompleteProcessing();

            // Play completion effects
            PlayCompletionEffects();

            Debug.Log($"[ExampleSmelter] {MachineId} completed: {currentRecipe?.recipeName ?? "unknown"}");
        }

        /// <summary>
        /// Plays visual and audio effects when smelting completes.
        /// </summary>
        private void PlayCompletionEffects()
        {
            // Play completion sound
            if (audioSource != null && completionSound != null)
            {
                audioSource.PlayOneShot(completionSound);
            }

            // Burst fire particles
            if (fireEffect != null)
            {
                fireEffect.Emit(20);
            }

            // Flash furnace light
            if (furnaceLight != null)
            {
                StartCoroutine(FlashLight());
            }
        }

        /// <summary>
        /// Flashes the furnace light briefly.
        /// </summary>
        private System.Collections.IEnumerator FlashLight()
        {
            float originalIntensity = furnaceLight.intensity;
            furnaceLight.intensity = originalIntensity * 2f;
            yield return new WaitForSeconds(0.2f);
            furnaceLight.intensity = originalIntensity;
        }

        #endregion

        #region Visual Feedback

        /// <summary>
        /// Update visual elements based on machine state.
        /// </summary>
        private void UpdateVisualEffects()
        {
            bool isWorking = CurrentState == MachineState.Processing && IsPowered;

            // Update fire effect
            if (fireEffect != null)
            {
                if (isWorking)
                {
                    if (!fireEffect.isPlaying)
                        fireEffect.Play();
                }
                else
                {
                    if (fireEffect.isPlaying)
                        fireEffect.Stop();
                }
            }

            // Update furnace light
            if (furnaceLight != null)
            {
                furnaceLight.enabled = isWorking;

                // Pulse light intensity based on processing progress
                if (isWorking)
                {
                    float pulse = Mathf.Sin(Time.time * 2f) * 0.2f + 0.8f;
                    furnaceLight.intensity = 2f * pulse;
                }
            }

            // Update audio
            UpdateAudio(isWorking);
        }

        /// <summary>
        /// Updates audio based on working state.
        /// </summary>
        private void UpdateAudio(bool isWorking)
        {
            if (audioSource == null || smeltingLoop == null)
                return;

            if (isWorking)
            {
                if (!audioSource.isPlaying || audioSource.clip != smeltingLoop)
                {
                    audioSource.clip = smeltingLoop;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else
            {
                if (audioSource.isPlaying && audioSource.clip == smeltingLoop)
                {
                    audioSource.Stop();
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
            
            // Update visuals every frame for smooth animations
            if (CurrentState == MachineState.Processing)
            {
                UpdateVisualEffects();
            }
        }

        /// <summary>
        /// Clean up when destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            // Stop effects
            if (fireEffect != null && fireEffect.isPlaying)
            {
                fireEffect.Stop();
            }

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            base.OnDestroy();
        }

        #endregion
    }
}
