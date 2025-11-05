using UnityEngine;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines.Examples
{
    /// <summary>
    /// Example implementation of a smelter that processes ore into ingots.
    /// This demonstrates proper inheritance from ProcessorBase.
    /// </summary>
    /// <remarks>
    /// Key Features:
    /// - Processes recipes with input/output buffers
    /// - Visual feedback (fire effects, furnace glow)
    /// - Recipe switching support
    /// - Automatic state management
    /// </remarks>
    public class ExampleSmelter : ProcessorBase
    {
        #region Serialized Fields

        [Header("Smelter Visual Effects")]
        [Tooltip("Particle system for fire effect")]
        [SerializeField] private ParticleSystem fireEffect;

        [Tooltip("Light component for furnace glow")]
        [SerializeField] private Light furnaceLight;

        [Tooltip("Material for hot metal glow")]
        [SerializeField] private Material glowMaterial;

        [Header("Audio")]
        [Tooltip("Sound played when smelting completes")]
        [SerializeField] private AudioClip completionSound;

        [Tooltip("Looping sound while smelting")]
        [SerializeField] private AudioClip smeltingLoop;

        #endregion

        #region Private Fields

        private AudioSource audioSource;
        private float glowIntensity;
        private Renderer furnaceRenderer;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the smelter with configuration data.
        /// </summary>
        public override void Initialize(MachineData data)
        {
            // IMPORTANT: Always call base.Initialize() first
            base.Initialize(data);

            // Cache components
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            furnaceRenderer = GetComponentInChildren<Renderer>();

            // Set default recipe if available
            if (data.availableRecipes != null && data.availableRecipes.Length > 0)
            {
                SetRecipe(data.availableRecipes[0]);

                if (showDebugInfo)
                {
                    Debug.Log($"[ExampleSmelter] {MachineId} default recipe: {data.availableRecipes[0].recipeName}");
                }
            }

            // Initialize visual state
            glowIntensity = 0f;
            UpdateVisuals();
        }

        #endregion

        #region Recipe Processing

        /// <summary>
        /// Called when processing completes.
        /// Override to add custom completion logic.
        /// </summary>
        protected override void CompleteProcessing()
        {
            // IMPORTANT: Call base method to handle resource consumption/production
            base.CompleteProcessing();

            // Play completion effects
            PlayCompletionEffects();

            if (showDebugInfo)
            {
                Debug.Log($"[ExampleSmelter] {MachineId} completed: {currentRecipe?.recipeName ?? "unknown"}");
            }
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
        protected override void UpdateVisuals()
        {
            // IMPORTANT: Call base method
            base.UpdateVisuals();

            bool isWorking = currentState == MachineState.Working && isPowered;

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

            // Update glow material
            if (furnaceRenderer != null && glowMaterial != null)
            {
                // Smoothly transition glow intensity
                float targetGlow = isWorking ? 1f : 0f;
                glowIntensity = Mathf.Lerp(glowIntensity, targetGlow, Time.deltaTime * 2f);

                // Apply glow to material
                if (glowMaterial.HasProperty("_EmissionColor"))
                {
                    Color emissionColor = Color.red * glowIntensity * 2f;
                    glowMaterial.SetColor("_EmissionColor", emissionColor);
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

        /// <summary>
        /// Handle state changes.
        /// </summary>
        protected override void OnStateChanged(MachineState oldState, MachineState newState)
        {
            // IMPORTANT: Call base method
            base.OnStateChanged(oldState, newState);

            if (showDebugInfo)
            {
                Debug.Log($"[ExampleSmelter] {MachineId} state: {oldState} → {newState}");
            }

            // Update visuals immediately on state change
            UpdateVisuals();
        }

        /// <summary>
        /// Handle power status changes.
        /// </summary>
        protected override void OnPowerStatusChanged(bool powered)
        {
            // IMPORTANT: Call base method
            base.OnPowerStatusChanged(powered);

            if (showDebugInfo)
            {
                Debug.Log($"[ExampleSmelter] {MachineId} power: {powered}");
            }

            // Update visuals immediately on power change
            UpdateVisuals();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the active recipe for this smelter.
        /// Validates that the recipe is available for this machine.
        /// </summary>
        public override void SetRecipe(RecipeData recipe)
        {
            // Validate recipe is available
            if (machineData != null && machineData.availableRecipes != null)
            {
                bool isAvailable = false;
                foreach (var availableRecipe in machineData.availableRecipes)
                {
                    if (availableRecipe == recipe)
                    {
                        isAvailable = true;
                        break;
                    }
                }

                if (!isAvailable)
                {
                    Debug.LogWarning($"[ExampleSmelter] {MachineId} recipe not available: {recipe?.recipeName ?? "null"}");
                    return;
                }
            }

            // IMPORTANT: Call base method to set recipe
            base.SetRecipe(recipe);

            if (showDebugInfo)
            {
                Debug.Log($"[ExampleSmelter] {MachineId} recipe changed: {recipe?.recipeName ?? "none"}");
            }
        }

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        private void Update()
        {
            // Update visuals every frame for smooth animations
            if (currentState == MachineState.Working)
            {
                UpdateVisuals();
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

            // IMPORTANT: Call base method last
            base.OnDestroy();
        }

        #endregion
    }
}
