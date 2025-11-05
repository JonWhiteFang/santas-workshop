using UnityEngine;
using System.Collections.Generic;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines.Examples
{
    /// <summary>
    /// Example implementation of a toy assembler that combines multiple components.
    /// This demonstrates proper inheritance from AssemblerBase.
    /// </summary>
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

        protected override void Awake()
        {
            base.Awake();

            // Initialize assembly point
            if (assemblyPoint == null)
            {
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
        }

        protected override void Start()
        {
            base.Start();

            // Set default recipe if available
            if (machineData != null && machineData.availableRecipes != null && machineData.availableRecipes.Count > 0)
            {
                SetRecipe(machineData.availableRecipes[0]);
                Debug.Log($"[ExampleToyAssembler] {MachineId} default recipe: {machineData.availableRecipes[0].recipeName}");
            }
        }

        #endregion

        #region Assembly Logic

        /// <summary>
        /// Called when assembly completes.
        /// </summary>
        protected override void CompleteAssembly()
        {
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

            Debug.Log($"[ExampleToyAssembler] {MachineId} completed: {currentRecipe?.recipeName ?? "unknown"}");
        }

        /// <summary>
        /// Spawns a visual representation of the completed toy.
        /// </summary>
        private void SpawnToyVisual(string toyId, Vector3 position)
        {
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

            // Add physics
            Rigidbody rb = toy.AddComponent<Rigidbody>();
            rb.mass = 0.1f;

            // Destroy after a few seconds
            Destroy(toy, 5f);

            Debug.Log($"[ExampleToyAssembler] {MachineId} spawned toy: {toyId}");
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
        private void UpdateVisualEffects()
        {
            bool isWorking = CurrentState == MachineState.Processing && IsPowered;

            // Update assembly arm animation
            if (assemblyArmAnimator != null)
            {
                assemblyArmAnimator.SetBool("IsAssembling", isWorking);

                if (isWorking)
                {
                    assemblyArmAnimator.SetFloat("AssemblyProgress", AssemblyProgress);
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

        #endregion

        #region Inventory Management

        /// <summary>
        /// Adds a resource to the inventory.
        /// </summary>
        public override bool AddToInventory(string resourceId, int amount)
        {
            bool success = base.AddToInventory(resourceId, amount);

            if (success)
            {
                OnInventoryChanged?.Invoke();
                Debug.Log($"[ExampleToyAssembler] {MachineId} added: {resourceId} x{amount}");
                Debug.Log($"[ExampleToyAssembler] {MachineId} inventory: {InventoryCount}/{maxInventorySlots}");
            }

            return success;
        }

        /// <summary>
        /// Clears all items from the inventory.
        /// </summary>
        public override void ClearInventory()
        {
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

            OnInventoryChanged?.Invoke();
            Debug.Log($"[ExampleToyAssembler] {MachineId} inventory cleared");
        }

        /// <summary>
        /// Event fired when inventory changes.
        /// </summary>
        public event System.Action OnInventoryChanged;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a summary of the current inventory state.
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

            base.OnDestroy();
        }

        #endregion
    }
}
