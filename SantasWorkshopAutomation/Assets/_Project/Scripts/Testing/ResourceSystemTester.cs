using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SantasWorkshop.Core;
using SantasWorkshop.Data;

namespace SantasWorkshop.Testing
{
    /// <summary>
    /// Test script for the Resource System.
    /// Provides UI buttons to test ResourceManager methods and displays resource counts.
    /// </summary>
    public class ResourceSystemTester : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI resourceCountsText;
        [SerializeField] private Button addWoodButton;
        [SerializeField] private Button addIronButton;
        [SerializeField] private Button consumeWoodButton;
        [SerializeField] private Button consumeIronButton;
        [SerializeField] private Button queryResourcesButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button setCapacityButton;

        [Header("Test Parameters")]
        [SerializeField] private int addAmount = 10;
        [SerializeField] private int consumeAmount = 5;
        [SerializeField] private long capacityLimit = 100;

        private void Start()
        {
            // Subscribe to ResourceManager events
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceSystemInitialized += OnResourceSystemInitialized;
                ResourceManager.Instance.OnResourceChanged += OnResourceChanged;
            }

            // Setup button listeners
            if (addWoodButton != null)
                addWoodButton.onClick.AddListener(() => TestAddResource("wood"));

            if (addIronButton != null)
                addIronButton.onClick.AddListener(() => TestAddResource("iron_ore"));

            if (consumeWoodButton != null)
                consumeWoodButton.onClick.AddListener(() => TestConsumeResource("wood"));

            if (consumeIronButton != null)
                consumeIronButton.onClick.AddListener(() => TestConsumeResource("iron_ore"));

            if (queryResourcesButton != null)
                queryResourcesButton.onClick.AddListener(TestQueryResources);

            if (resetButton != null)
                resetButton.onClick.AddListener(TestResetResources);

            if (setCapacityButton != null)
                setCapacityButton.onClick.AddListener(() => TestSetCapacity("wood"));

            UpdateStatusText("Resource System Tester initialized. Waiting for ResourceManager...");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceSystemInitialized -= OnResourceSystemInitialized;
                ResourceManager.Instance.OnResourceChanged -= OnResourceChanged;
            }
        }

        #region Event Handlers

        private void OnResourceSystemInitialized()
        {
            UpdateStatusText("ResourceManager initialized successfully!");
            UpdateResourceCountsDisplay();
        }

        private void OnResourceChanged(string resourceId, long newAmount)
        {
            UpdateStatusText($"Resource changed: {resourceId} = {newAmount}");
            UpdateResourceCountsDisplay();
        }

        #endregion

        #region Test Methods

        /// <summary>
        /// Tests adding a resource to the global inventory.
        /// </summary>
        private void TestAddResource(string resourceId)
        {
            if (ResourceManager.Instance == null)
            {
                UpdateStatusText("ERROR: ResourceManager not found!");
                return;
            }

            bool success = ResourceManager.Instance.AddResource(resourceId, addAmount);
            
            if (success)
            {
                UpdateStatusText($"✓ Added {addAmount} {resourceId}");
            }
            else
            {
                UpdateStatusText($"✗ Failed to add {addAmount} {resourceId}");
            }
        }

        /// <summary>
        /// Tests consuming a resource from the global inventory.
        /// </summary>
        private void TestConsumeResource(string resourceId)
        {
            if (ResourceManager.Instance == null)
            {
                UpdateStatusText("ERROR: ResourceManager not found!");
                return;
            }

            bool success = ResourceManager.Instance.TryConsumeResource(resourceId, consumeAmount);
            
            if (success)
            {
                UpdateStatusText($"✓ Consumed {consumeAmount} {resourceId}");
            }
            else
            {
                UpdateStatusText($"✗ Failed to consume {consumeAmount} {resourceId} (insufficient resources)");
            }
        }

        /// <summary>
        /// Tests querying resource information.
        /// </summary>
        private void TestQueryResources()
        {
            if (ResourceManager.Instance == null)
            {
                UpdateStatusText("ERROR: ResourceManager not found!");
                return;
            }

            // Test HasResource
            bool hasWood = ResourceManager.Instance.HasResource("wood", 1);
            bool hasIron = ResourceManager.Instance.HasResource("iron_ore", 1);

            // Test GetResourceCount
            long woodCount = ResourceManager.Instance.GetResourceCount("wood");
            long ironCount = ResourceManager.Instance.GetResourceCount("iron_ore");

            // Test GetResourceData
            ResourceData woodData = ResourceManager.Instance.GetResourceData("wood");
            ResourceData ironData = ResourceManager.Instance.GetResourceData("iron_ore");

            string result = $"Query Results:\n";
            result += $"Wood: {woodCount} (Has: {hasWood}, Data: {(woodData != null ? woodData.displayName : "null")})\n";
            result += $"Iron Ore: {ironCount} (Has: {hasIron}, Data: {(ironData != null ? ironData.displayName : "null")})";

            UpdateStatusText(result);
        }

        /// <summary>
        /// Tests resetting all resources to zero.
        /// </summary>
        private void TestResetResources()
        {
            if (ResourceManager.Instance == null)
            {
                UpdateStatusText("ERROR: ResourceManager not found!");
                return;
            }

            ResourceManager.Instance.ResetResources();
            UpdateStatusText("✓ All resources reset to zero");
        }

        /// <summary>
        /// Tests setting a capacity limit for a resource.
        /// </summary>
        private void TestSetCapacity(string resourceId)
        {
            if (ResourceManager.Instance == null)
            {
                UpdateStatusText("ERROR: ResourceManager not found!");
                return;
            }

            ResourceManager.Instance.SetResourceCapacity(resourceId, capacityLimit);
            long capacity = ResourceManager.Instance.GetResourceCapacity(resourceId);
            UpdateStatusText($"✓ Set capacity for {resourceId} to {capacity}");
        }

        #endregion

        #region UI Updates

        /// <summary>
        /// Updates the status text display.
        /// </summary>
        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }

            Debug.Log($"[ResourceSystemTester] {message}");
        }

        /// <summary>
        /// Updates the resource counts display.
        /// </summary>
        private void UpdateResourceCountsDisplay()
        {
            if (resourceCountsText == null || ResourceManager.Instance == null)
                return;

            string display = "=== Resource Counts ===\n\n";

            // Get all resources and display their counts
            var allResources = ResourceManager.Instance.GetAllResources();
            
            foreach (var resource in allResources)
            {
                long count = ResourceManager.Instance.GetResourceCount(resource.resourceId);
                long capacity = ResourceManager.Instance.GetResourceCapacity(resource.resourceId);
                
                string capacityStr = capacity > 0 ? $" / {capacity}" : "";
                display += $"{resource.displayName}: {count}{capacityStr}\n";
            }

            resourceCountsText.text = display;
        }

        #endregion
    }
}
