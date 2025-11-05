using UnityEngine;
using System.Collections.Generic;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Base class for assembler machines.
    /// Assemblers combine multiple input resources to create complex products.
    /// </summary>
    public abstract class AssemblerBase : MachineBase
    {
        #region Serialized Fields

        [Header("Assembler Settings")]
        [SerializeField] protected int maxInventorySlots = 20;

        #endregion

        #region Protected Fields

        protected Recipe currentRecipe;
        protected Dictionary<string, int> inputInventory;
        protected float assemblyProgress;
        protected bool hasRequiredInputs;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current recipe being assembled.
        /// </summary>
        public Recipe CurrentRecipe => currentRecipe;

        /// <summary>
        /// Gets whether the assembler has a recipe set.
        /// </summary>
        public bool HasRecipe => currentRecipe != null;

        /// <summary>
        /// Gets the current assembly progress (0-1).
        /// </summary>
        public float AssemblyProgress => assemblyProgress;

        /// <summary>
        /// Gets the total number of items in the input inventory.
        /// </summary>
        public int InventoryCount
        {
            get
            {
                int count = 0;
                foreach (var kvp in inputInventory)
                {
                    count += kvp.Value;
                }
                return count;
            }
        }

        /// <summary>
        /// Gets whether the inventory has space for more items.
        /// </summary>
        public bool HasInventorySpace => InventoryCount < maxInventorySlots;

        #endregion

        #region Initialization

        protected override void Awake()
        {
            base.Awake();
            inputInventory = new Dictionary<string, int>();
        }

        #endregion

        #region Recipe Management

        /// <summary>
        /// Sets the current recipe for this assembler.
        /// </summary>
        public virtual void SetRecipe(Recipe recipe)
        {
            currentRecipe = recipe;
            assemblyProgress = 0f;
            hasRequiredInputs = false;
            SetActiveRecipe(recipe);

            Debug.Log($"[AssemblerBase] {MachineId} recipe set: {recipe?.recipeId ?? "none"}");
        }

        /// <summary>
        /// Checks if the inventory contains all required inputs for the current recipe.
        /// </summary>
        protected virtual void CheckRequiredInputs()
        {
            if (currentRecipe == null || currentRecipe.inputs == null)
            {
                hasRequiredInputs = false;
                return;
            }

            hasRequiredInputs = true;

            foreach (var input in currentRecipe.inputs)
            {
                if (!inputInventory.TryGetValue(input.resourceId, out int count) || count < input.amount)
                {
                    hasRequiredInputs = false;
                    break;
                }
            }
        }

        /// <summary>
        /// Completes the current assembly cycle and produces the output.
        /// </summary>
        protected virtual void CompleteAssembly()
        {
            if (currentRecipe == null)
                return;

            // Consume inputs
            ConsumeInputsFromInventory();

            // Produce output
            ProduceOutput();

            Debug.Log($"[AssemblerBase] {MachineId} completed assembly: {currentRecipe.recipeId}");

            // Check if we can continue assembling
            CheckRequiredInputs();
        }

        /// <summary>
        /// Consumes the required inputs from the inventory.
        /// </summary>
        protected virtual void ConsumeInputsFromInventory()
        {
            if (currentRecipe == null || currentRecipe.inputs == null)
                return;

            foreach (var input in currentRecipe.inputs)
            {
                if (inputInventory.ContainsKey(input.resourceId))
                {
                    inputInventory[input.resourceId] -= input.amount;

                    if (inputInventory[input.resourceId] <= 0)
                    {
                        inputInventory.Remove(input.resourceId);
                    }
                }
            }
        }

        /// <summary>
        /// Produces the recipe output.
        /// Override in derived classes to handle output delivery.
        /// </summary>
        protected virtual void ProduceOutput()
        {
            if (currentRecipe == null || currentRecipe.outputs == null)
                return;

            // TODO: Implement output delivery to logistics system or ResourceManager
            foreach (var output in currentRecipe.outputs)
            {
                Debug.Log($"[AssemblerBase] {MachineId} produced: {output.resourceId} x{output.amount}");
            }
        }

        #endregion

        #region Inventory Management

        /// <summary>
        /// Adds a resource to the input inventory.
        /// </summary>
        public virtual bool AddToInventory(string resourceId, int amount)
        {
            if (!HasInventorySpace)
                return false;

            if (inputInventory.ContainsKey(resourceId))
            {
                inputInventory[resourceId] += amount;
            }
            else
            {
                inputInventory[resourceId] = amount;
            }

            Debug.Log($"[AssemblerBase] {MachineId} added to inventory: {resourceId} x{amount}");

            return true;
        }

        /// <summary>
        /// Gets the amount of a specific resource in the inventory.
        /// </summary>
        public virtual int GetInventoryAmount(string resourceId)
        {
            return inputInventory.TryGetValue(resourceId, out int amount) ? amount : 0;
        }

        /// <summary>
        /// Clears all items from the inventory.
        /// </summary>
        public virtual void ClearInventory()
        {
            inputInventory.Clear();
            hasRequiredInputs = false;

            Debug.Log($"[AssemblerBase] {MachineId} inventory cleared");
        }

        #endregion
    }
}
