using UnityEngine;
using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Abstract base class for all machines in the game.
    /// Provides common functionality for power management, state tracking, and lifecycle.
    /// </summary>
    public abstract class MachineBase : MonoBehaviour, IMachine
    {
        #region Serialized Fields

        [Header("Machine Configuration")]
        [SerializeField] protected MachineData machineData;

        [Header("Debug")]
        [SerializeField] protected bool showDebugInfo = false;

        #endregion

        #region Protected Fields

        protected MachineState currentState = MachineState.Offline;
        protected float powerConsumption;
        protected bool isPowered;
        protected Transform cachedTransform;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique identifier for this machine type.
        /// </summary>
        public string MachineId => machineData != null ? machineData.machineId : "unknown";

        /// <summary>
        /// Gets the current operational state of the machine.
        /// </summary>
        public MachineState State => currentState;

        /// <summary>
        /// Gets whether the machine is currently powered.
        /// </summary>
        public bool IsPowered => isPowered;

        /// <summary>
        /// Gets the machine's power consumption rate.
        /// </summary>
        public float PowerConsumption => powerConsumption;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            cachedTransform = transform;
        }

        protected virtual void Start()
        {
            if (machineData != null)
            {
                Initialize(machineData);
            }
            else
            {
                Debug.LogWarning($"[MachineBase] {gameObject.name} has no MachineData assigned!");
            }
        }

        protected virtual void Update()
        {
            if (currentState != MachineState.Offline)
            {
                Tick(Time.deltaTime);
            }
        }

        protected virtual void OnDestroy()
        {
            Shutdown();
        }

        #endregion

        #region IMachine Implementation

        /// <summary>
        /// Initializes the machine with configuration data.
        /// </summary>
        public virtual void Initialize(MachineData data)
        {
            machineData = data;
            powerConsumption = data != null ? data.powerConsumption : 0f;
            currentState = MachineState.Idle;

            if (showDebugInfo)
            {
                Debug.Log($"[MachineBase] {MachineId} initialized");
            }
        }

        /// <summary>
        /// Updates the machine's logic. Override in derived classes for specific behavior.
        /// </summary>
        public abstract void Tick(float deltaTime);

        /// <summary>
        /// Shuts down the machine and cleans up resources.
        /// </summary>
        public virtual void Shutdown()
        {
            currentState = MachineState.Offline;
            isPowered = false;

            if (showDebugInfo)
            {
                Debug.Log($"[MachineBase] {MachineId} shut down");
            }
        }

        #endregion

        #region Power Management

        /// <summary>
        /// Sets the powered state of the machine.
        /// </summary>
        /// <param name="powered">Whether the machine should be powered</param>
        public virtual void SetPowered(bool powered)
        {
            if (isPowered != powered)
            {
                isPowered = powered;
                OnPowerChanged(powered);

                if (showDebugInfo)
                {
                    Debug.Log($"[MachineBase] {MachineId} power state changed to: {powered}");
                }
            }
        }

        /// <summary>
        /// Called when the machine's power state changes.
        /// Override in derived classes to handle power state changes.
        /// </summary>
        protected virtual void OnPowerChanged(bool powered)
        {
            if (!powered && currentState == MachineState.Working)
            {
                currentState = MachineState.Idle;
            }
        }

        #endregion

        #region State Management

        /// <summary>
        /// Changes the machine's operational state.
        /// </summary>
        protected virtual void SetState(MachineState newState)
        {
            if (currentState != newState)
            {
                MachineState previousState = currentState;
                currentState = newState;

                if (showDebugInfo)
                {
                    Debug.Log($"[MachineBase] {MachineId} state changed: {previousState} -> {newState}");
                }

                OnStateChanged(previousState, newState);
            }
        }

        /// <summary>
        /// Called when the machine's state changes.
        /// Override in derived classes to handle state transitions.
        /// </summary>
        protected virtual void OnStateChanged(MachineState previousState, MachineState newState)
        {
            // Override in derived classes
        }

        #endregion

        #region Debug

        protected virtual void OnDrawGizmos()
        {
            if (showDebugInfo && Application.isPlaying)
            {
                // Draw state indicator
                Gizmos.color = GetStateColor();
                Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
            }
        }

        private Color GetStateColor()
        {
            return currentState switch
            {
                MachineState.Offline => Color.gray,
                MachineState.Idle => Color.yellow,
                MachineState.Working => Color.green,
                MachineState.Blocked => Color.red,
                MachineState.Error => Color.magenta,
                _ => Color.white
            };
        }

        #endregion
    }
}
