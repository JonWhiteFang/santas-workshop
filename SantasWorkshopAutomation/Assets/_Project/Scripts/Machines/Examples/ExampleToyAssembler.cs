using UnityEngine;
using System.Collections.Generic;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines.Examples
{
    /// <summary>
    /// Example implementation of a toy assembler that combines multiple components.
    /// This demonstrates proper inheritance from AssemblerBase.
    /// </summary>
    /// <remarks>
    /// Key Features:
    /// - Combines multiple input resources
    /// - Inventory-based resource management
    /// - Visual assembly animation
    /// - Toy spawning at completion
    /// </remarks>
    public class ExampleToyAssembler : AssemblerBase
    {
        #region Serialized Fields

        [Header("Toy Assembler Settings")]
        [Tooltip("Transform where assembled toys appear")]
        [SerializeField] private Transform assemblyPoint;

        [Tooltip("Speed of assembly animation")]
        [SerializeField] private float assemblyAnimationSpeed = 1f;

        [Header("Visual Effects")]
        [Tooltip("Particle system for assembly sparkles")]
        [SerializeField] private ParticleSystem assemblySparkles;

        [Tooltip("Animator for assembly arm")]
        [SerializeField] private Animator assemblyArmAnimator;

        [Header("Audio")]
        [Tooltip("Sound played during assembly")]
        [SerializeField] private AudioClip assemblySound;

        [Tooltip("Sound played when toy is completed")]
        [SerializeField] private AudioClip completionSound;

        #endregion

        #region Private Fields

        private AudioSource audioSource;
        private Dictionary<string, GameObject> componentVisuals;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the toy assembler with configuration data.
        /// </summary>
        public override void Initialize(MachineData data)
        {
            // IMPORTANT: Always call base.Initialize() first
            base.Initialize(data);

            // Initialize assembly point
            if (assemblyPoint == null)
            {
                // Create assembly point if not assigned
                GameObject assemblyPointObj = new GameObject("AssemblyPoint");
                assemblyPointObj.transform.SetParent(transform);
                assemblyPointObj.transform.localPosition = Vector3.up * 1.5f;
                assemblyPoint = assemblyPointObj.transform;
            }

            // Cache audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Initialize component visuals dictionary
            componentVisuals = new Dictionary<string, GameObject>();

            // Set default recipe if available
            if (data.availableRecipes != null && data.availableRecipes.Length > 0)
            {
                SetRecipe(data.availableRecipes[0]);

                if (showDebugInfo)
                {
                    Debug.Log($"[ExampleToyAssembler] {MachineId} default recipe: {data.availableRecipes[0].recipeName}");
                }
            }
        }

        #endregion

        #region Assembly Logic

        /// <summary>
        /// Called when assembly completes.
        /// Override to add custom completion logic.
        /// </summary>
        protected override void CompleteAssembly()
        {
            // IMPORTANT: Call base method to handle resource consumption
            base.CompleteAssembly();

            // Spawn toy visual
            if (currentRecipe != null && currentRecipe.outputs != null)
            {
                foreach (var output in currentRecipe.outputs)
                {
                    SpawnToyVisual(output.resourceId, assemblyPoint.position);
                }
            }

            // Play completion effects
            PlayCompletionEffects();

            if (showDebugInfo)
            {
                Debug.Log($"[ExampleToyAssembler] {MachineId} completed: {currentRecipe?.recipeName ?? "unknown"}");
            }
        }

        /// <summary>
        /// Spawns a visual representation of the completed toy.
        /// </summary>
        private void SpawnToyVisual(string toyId, Vector3 position)
        {
            // TODO: Load toy prefab from resources or database
            // For now, create a simple placeholder

            GameObject toy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toy.name = $"Toy_{toyId}";
            toy.transform.position = position;
            toy.transform.localScale = Vector3.one * 0.3f;

            // Add random color
            Renderer renderer = toy.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Random.ColorHSV();
            }

            // Add physics for fun
            Rigidbody rb = toy.AddComponent<Rigidbody>();
            rb.mass = 0.1f;

            // Destroy after a few seconds (in real game, this would go to logistics)
            Destroy(toy, 5f);

            if (showDebugInfo)
            {
                Debug.Log($"[ExampleToyAssembler] {MachineId} spawned toy: {toyId}");
            }
        }

        /// <summary>
        /// Plays visual and audio effects when assembly completes.
        /// </summary>
        private void PlayCompletionEffects()
        {
            // Play completion sound
            if (audioSource != null && completionSound != null)
            {
                audioSource.PlayOneShot(completionSound);
            }

            // Burst sparkles
            if (assemblySparkles != null)
            {
                assemblySparkles.Emit(30);
            }
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

            // Update assembly arm animation
            if (assemblyArmAnimator != null)
            {
                assemblyArmAnimator.SetBool("IsAssembling", isWorking);

                if (isWorking)
                {
                    // Set animation progress based on assembly progress
                    assemblyArmAnimator.SetFloat("AssemblyProgress", assemblyProgress);
                    assemblyArmAnimator.speed = assemblyAnimationSpeed;
                }
                else
                {
                    assemblyArmAnimator.speed = 1f;
                }
            }

            // Update sparkles
            if (assemblySparkles != null)
            {
                if (isWorking)
                {
                    if (!assemblySparkles.isPlaying)
                        assemblySparkles.Play();
                }
                else
                {
                    if (assemblySparkles.isPlaying)
                        assemblySparkles.Stop();
                }
            }

            // Update audio
            UpdateAudio(isWorking);

            // Update component visuals
            UpdateComponentVisuals();
        }

        /// <summary>
        /// Updates audio based on working state.
        /// </summary>
        private void UpdateAudio(bool isWorking)
        {
            if (audioSource == null || assemblySound == null)
                return;

            if (isWorking)
            {
                if (!audioSource.isPlaying || audioSource.clip != assemblySound)
                {
                    audioSource.clip = assemblySound;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else
            {
                if (audioSource.isPlaying && audioSource.clip == assemblySound)
                {
                    audioSource.Stop();
                }
            }
        }

        /// <summary>
        /// Updates visual representations of components in inventory.
        /// </summary>
        private void UpdateComponentVisuals()
        {
            // TODO: Show visual indicators for components in inventory
            // This could display small icons or models around the machine

            if (showDebugInfo && currentRecipe != null)
            {
                // Log inventory status
                foreach (var input in currentRecipe.inputs)
                {
                    int amount = GetInventoryAmount(input.resourceId);
                    if (amount > 0)
                    {
                        // Component is in inventory
                    }
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
                Debug.Log($"[ExampleToyAssembler] {MachineId} state: {oldState} → {newState}");
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
                Debug.Log($"[ExampleToyAssembler] {MachineId} power: {powered}");
            }

            // Update visuals immediately on power change
            UpdateVisuals();
        }

        #endregion

        #region Inventory Management

        /// <summary>
        /// Adds a resource to the inventory.
        /// Override to add custom logic and events.
        /// </summary>
        public override bool AddToInventory(string resourceId, int amount)
        {
            // IMPORTANT: Call base method to add to inventory
            bool success = base.AddToInventory(resourceId, amount);

            if (success)
            {
                // Trigger custom event or visual update
                OnInventoryChanged?.Invoke();

                if (showDebugInfo)
                {
                    Debug.Log($"[ExampleToyAssembler] {MachineId} added: {resourceId} x{amount}");
                    Debug.Log($"[ExampleToyAssembler] {MachineId} inventory: {InventoryCount}/{maxInventorySlots}");
                }

                // Update component visuals
                UpdateComponentVisuals();
            }

            return success;
        }

        /// <summary>
        /// Clears all items from the inventory.
        /// Override to add custom cleanup logic.
        /// </summary>
        public override void ClearInventory()
        {
            // IMPORTANT: Call base method to clear inventory
            base.ClearInventory();

            // Clear component visuals
            foreach (var visual in componentVisuals.Values)
            {
                if (visual != null)
                {
                    Destroy(visual);
                }
            }
            componentVisuals.Clear();

            // Trigger event
            OnInventoryChanged?.Invoke();

            if (showDebugInfo)
            {
                Debug.Log($"[ExampleToyAssembler] {MachineId} inventory cleared");
            }
        }

        /// <summary>
        /// Event fired when inventory changes.
        /// Subscribe to this to update UI or other systems.
        /// </summary>
        public event System.Action OnInventoryChanged;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a summary of the current inventory state.
        /// Useful for UI display.
        /// </summary>
        public string GetInventorySummary()
        {
            if (currentRecipe == null)
                return "No recipe set";

            string summary = $"Recipe: {currentRecipe.recipeName}\n";
            summary += $"Inventory: {InventoryCount}/{maxInventorySlots}\n";
            summary += "Components:\n";

            foreach (var input in currentRecipe.inputs)
            {
                int amount = GetInventoryAmount(input.resourceId);
                summary += $"  {input.resourceId}: {amount}/{input.amount}\n";
            }

            return summary;
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
            // Clear component visuals
            foreach (var visual in componentVisuals.Values)
            {
                if (visual != null)
                {
                    Destroy(visual);
                }
            }
            componentVisuals.Clear();

            // Stop effects
            if (assemblySparkles != null && assemblySparkles.isPlaying)
            {
                assemblySparkles.Stop();
            }

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            // IMPORTANT: Call base method last
            base.OnDestroy();
        }

        #endregion

        #region Debug

        /// <summary>
        /// Draw debug information in editor.
        /// </summary>
        private void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying)
                return;

            // Display inventory summary on screen
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
            if (screenPos.z > 0)
            {
                GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 300, 200), GetInventorySummary());
            }
        }

        #endregion
    }
}
