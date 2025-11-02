using UnityEngine;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Base class for extractor machines (mining drills, harvesters, etc.).
    /// Extractors gather resources from resource nodes in the world.
    /// </summary>
    public abstract class ExtractorBase : MachineBase
    {
        #region Serialized Fields

        [Header("Extractor Settings")]
        [SerializeField] protected float extractionRate = 1f;
        [SerializeField] protected float extractionRange = 5f;

        #endregion

        #region Protected Fields

        protected ResourceNode targetNode;
        protected float extractionProgress;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current extraction rate (resources per second).
        /// </summary>
        public float ExtractionRate => extractionRate;

        /// <summary>
        /// Gets whether the extractor has a valid target node.
        /// </summary>
        public bool HasTarget => targetNode != null;

        #endregion

        #region Initialization

        public override void Initialize(MachineData data)
        {
            base.Initialize(data);
            FindNearestResourceNode();
        }

        #endregion

        #region Machine Logic

        public override void Tick(float deltaTime)
        {
            if (!isPowered || targetNode == null)
            {
                if (currentState == MachineState.Working)
                {
                    SetState(MachineState.Idle);
                }
                return;
            }

            // Perform extraction
            SetState(MachineState.Working);
            extractionProgress += extractionRate * deltaTime;

            if (extractionProgress >= 1f)
            {
                extractionProgress -= 1f;
                OnResourceExtracted();
            }
        }

        #endregion

        #region Resource Management

        /// <summary>
        /// Finds the nearest resource node within extraction range.
        /// </summary>
        protected virtual void FindNearestResourceNode()
        {
            // TODO: Implement resource node detection
            // This will be implemented when the resource node system is created
            if (showDebugInfo)
            {
                Debug.Log($"[ExtractorBase] {MachineId} searching for resource nodes...");
            }
        }

        /// <summary>
        /// Called when a resource unit has been extracted.
        /// Override in derived classes to handle specific resource types.
        /// </summary>
        protected virtual void OnResourceExtracted()
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtractorBase] {MachineId} extracted resource");
            }

            // TODO: Add resource to output buffer or ResourceManager
        }

        /// <summary>
        /// Sets the target resource node for this extractor.
        /// </summary>
        public virtual void SetTargetNode(ResourceNode node)
        {
            targetNode = node;

            if (showDebugInfo)
            {
                Debug.Log($"[ExtractorBase] {MachineId} target node set: {node != null}");
            }
        }

        #endregion

        #region Debug

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (showDebugInfo)
            {
                // Draw extraction range
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, extractionRange);

                // Draw line to target node
                if (targetNode != null && Application.isPlaying)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, targetNode.transform.position);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Placeholder class for ResourceNode. Will be implemented in a future task.
    /// </summary>
    public class ResourceNode : MonoBehaviour
    {
        // TODO: Implement resource node functionality
    }
}
