using UnityEngine;
using System.Collections.Generic;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Base class for processor machines (smelters, sawmills, refineries, etc.).
    /// Processors convert input resources into output resources using recipes.
    /// </summary>
    public abstract class ProcessorBase : MachineBase
    {
        #region Serialized Fields

        [Header("Processor Settings")]
        [SerializeField] protected int inputBufferSize = 10;
        [SerializeField] protected int outputBufferSize = 10;

        #endregion

        #region Protected Fields

        protected RecipeData currentRecipe;
        protected Queue<ResourceStack> inputBuffer;
        protected Queue<ResourceStack> outputBuffer;
        protected float processingProgress;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current recipe being processed.
        /// </summary>
        public RecipeData CurrentRecipe => currentRecipe;

        /// <summary>
        /// Gets whether the processor has a recipe set.
        /// </summary>
        public bool HasRecipe => currentRecipe != null;

        /// <summary>
        /// Gets the current processing progress (0-1).
        /// </summary>
        public float ProcessingProgress => processingProgress;

        /// <summary>
        /// Gets whether the input buffer has space.
        /// </summary>
        public bool CanAcceptInput => inputBuffer.Count < inputBufferSize;

        /// <summary>
        /// Gets whether the output buffer has items.
        /// </summary>
        public bool HasOutput => outputBuffer.Count > 0;

        #endregion

        #region Initialization

        protected override void Awake()
        {
            base.Awake();
            inputBuffer = new Queue<ResourceStack>(inputBufferSize);
            outputBuffer = new Queue<ResourceStack>(outputBufferSize);
        }

        public override void Initialize(MachineData data)
        {
            base.Initialize(data);
            processingProgress = 0f;
        }

        #endregion

        #region Machine Logic

        public override void Tick(float deltaTime)
        {
            if (!isPowered || currentRecipe == null)
            {
                if (currentState == MachineState.Working)
                {
                    SetState(MachineState.Idle);
                }
                return;
            }

            // Check if we can process
            if (!CanProcess())
            {
                SetState(MachineState.Blocked);
                return;
            }

            // Process recipe
            SetState(MachineState.Working);
            processingProgress += deltaTime / currentRecipe.processingTime;

            if (processingProgress >= 1f)
            {
                processingProgress = 0f;
                CompleteProcessing();
            }
        }

        #endregion

        #region Recipe Management

        /// <summary>
        /// Sets the current recipe for this processor.
        /// </summary>
        public virtual void SetRecipe(RecipeData recipe)
        {
            currentRecipe = recipe;
            processingProgress = 0f;

            if (showDebugInfo)
            {
                Debug.Log($"[ProcessorBase] {MachineId} recipe set: {recipe?.recipeId ?? "none"}");
            }
        }

        /// <summary>
        /// Checks if the processor can currently process (has inputs, has space for outputs).
        /// </summary>
        protected virtual bool CanProcess()
        {
            if (currentRecipe == null)
                return false;

            // Check if we have required inputs
            if (!HasRequiredInputs())
                return false;

            // Check if output buffer has space
            if (outputBuffer.Count >= outputBufferSize)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if the input buffer contains all required inputs for the current recipe.
        /// </summary>
        protected virtual bool HasRequiredInputs()
        {
            // TODO: Implement proper input checking when ResourceStack is fully implemented
            return inputBuffer.Count > 0;
        }

        /// <summary>
        /// Completes the current processing cycle and produces outputs.
        /// </summary>
        protected virtual void CompleteProcessing()
        {
            if (currentRecipe == null)
                return;

            // Consume inputs
            ConsumeInputs();

            // Produce outputs
            ProduceOutputs();

            if (showDebugInfo)
            {
                Debug.Log($"[ProcessorBase] {MachineId} completed processing: {currentRecipe.recipeId}");
            }
        }

        /// <summary>
        /// Consumes the required inputs from the input buffer.
        /// </summary>
        protected virtual void ConsumeInputs()
        {
            // TODO: Implement proper input consumption when ResourceStack is fully implemented
            if (inputBuffer.Count > 0)
            {
                inputBuffer.Dequeue();
            }
        }

        /// <summary>
        /// Produces the recipe outputs and adds them to the output buffer.
        /// </summary>
        protected virtual void ProduceOutputs()
        {
            if (currentRecipe == null || currentRecipe.outputs == null)
                return;

            foreach (var output in currentRecipe.outputs)
            {
                if (outputBuffer.Count < outputBufferSize)
                {
                    outputBuffer.Enqueue(output);
                }
            }
        }

        #endregion

        #region Buffer Management

        /// <summary>
        /// Adds a resource to the input buffer.
        /// </summary>
        public virtual bool AddInput(ResourceStack resource)
        {
            if (!CanAcceptInput)
                return false;

            inputBuffer.Enqueue(resource);
            return true;
        }

        /// <summary>
        /// Removes and returns a resource from the output buffer.
        /// </summary>
        public virtual ResourceStack TakeOutput()
        {
            if (outputBuffer.Count > 0)
            {
                return outputBuffer.Dequeue();
            }

            return default;
        }

        #endregion
    }
}
