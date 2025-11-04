#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using SantasWorkshop.Core;

namespace SantasWorkshop.Testing.Editor
{
    /// <summary>
    /// Editor utility to automatically set up the Resource System test scene.
    /// </summary>
    public static class ResourceSystemTestSceneSetup
    {
        [MenuItem("Santa/Testing/Setup Resource System Test Scene")]
        public static void SetupTestScene()
        {
            // Open or create the test scene
            string scenePath = "Assets/_Project/Scenes/TestScenes/TestScene_ResourceSystem.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Clear existing objects (except camera and light)
            foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (obj.name != "Main Camera" && obj.name != "Directional Light")
                {
                    Object.DestroyImmediate(obj);
                }
            }

            // Create ResourceManager
            GameObject resourceManager = new GameObject("ResourceManager");
            resourceManager.AddComponent<ResourceManager>();
            resourceManager.transform.position = Vector3.zero;

            // Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create EventSystem if it doesn't exist
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Create Status Text
            GameObject statusTextObj = new GameObject("StatusText");
            statusTextObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "Status: Waiting...";
            statusText.fontSize = 24;
            statusText.color = Color.white;
            
            RectTransform statusRect = statusTextObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 1);
            statusRect.anchorMax = new Vector2(0, 1);
            statusRect.pivot = new Vector2(0, 1);
            statusRect.anchoredPosition = new Vector2(50, -50);
            statusRect.sizeDelta = new Vector2(700, 150);

            // Create Resource Counts Text
            GameObject countsTextObj = new GameObject("ResourceCountsText");
            countsTextObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI countsText = countsTextObj.AddComponent<TextMeshProUGUI>();
            countsText.text = "Resource Counts";
            countsText.fontSize = 20;
            countsText.color = Color.white;
            countsText.alignment = TextAlignmentOptions.TopLeft;
            
            RectTransform countsRect = countsTextObj.GetComponent<RectTransform>();
            countsRect.anchorMin = new Vector2(1, 1);
            countsRect.anchorMax = new Vector2(1, 1);
            countsRect.pivot = new Vector2(1, 1);
            countsRect.anchoredPosition = new Vector2(-50, -50);
            countsRect.sizeDelta = new Vector2(500, 800);

            // Create Button Panel
            GameObject panelObj = new GameObject("ButtonPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0);
            panelRect.anchorMax = new Vector2(0.5f, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.anchoredPosition = new Vector2(0, 50);
            panelRect.sizeDelta = new Vector2(1600, 250);

            // Create buttons
            Button addWoodBtn = CreateButton(panelObj.transform, "AddWoodButton", "Add Wood (+10)", new Vector2(-500, 100), new Vector2(200, 60));
            Button addIronBtn = CreateButton(panelObj.transform, "AddIronButton", "Add Iron (+10)", new Vector2(-250, 100), new Vector2(200, 60));
            Button consumeWoodBtn = CreateButton(panelObj.transform, "ConsumeWoodButton", "Consume Wood (-5)", new Vector2(0, 100), new Vector2(200, 60));
            Button consumeIronBtn = CreateButton(panelObj.transform, "ConsumeIronButton", "Consume Iron (-5)", new Vector2(250, 100), new Vector2(200, 60));
            Button queryBtn = CreateButton(panelObj.transform, "QueryResourcesButton", "Query Resources", new Vector2(500, 100), new Vector2(200, 60));
            Button resetBtn = CreateButton(panelObj.transform, "ResetButton", "Reset All", new Vector2(-125, 20), new Vector2(200, 60));
            Button capacityBtn = CreateButton(panelObj.transform, "SetCapacityButton", "Set Capacity (100)", new Vector2(125, 20), new Vector2(200, 60));

            // Tint reset button red
            ColorBlock resetColors = resetBtn.colors;
            resetColors.normalColor = new Color(1f, 0.5f, 0.5f);
            resetBtn.colors = resetColors;

            // Create Test Controller
            GameObject testController = new GameObject("TestController");
            ResourceSystemTester tester = testController.AddComponent<ResourceSystemTester>();

            // Use reflection to set private serialized fields
            var testerType = typeof(ResourceSystemTester);
            
            SetPrivateField(tester, "statusText", statusText);
            SetPrivateField(tester, "resourceCountsText", countsText);
            SetPrivateField(tester, "addWoodButton", addWoodBtn);
            SetPrivateField(tester, "addIronButton", addIronBtn);
            SetPrivateField(tester, "consumeWoodButton", consumeWoodBtn);
            SetPrivateField(tester, "consumeIronButton", consumeIronBtn);
            SetPrivateField(tester, "queryResourcesButton", queryBtn);
            SetPrivateField(tester, "resetButton", resetBtn);
            SetPrivateField(tester, "setCapacityButton", capacityBtn);
            SetPrivateField(tester, "addAmount", 10);
            SetPrivateField(tester, "consumeAmount", 5);
            SetPrivateField(tester, "capacityLimit", 100L);

            // Mark scene as dirty and save
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("Resource System Test Scene setup complete!");
            EditorUtility.DisplayDialog("Setup Complete", 
                "Resource System Test Scene has been set up successfully!\n\n" +
                "The scene includes:\n" +
                "- ResourceManager GameObject\n" +
                "- Test UI with buttons\n" +
                "- Status and resource count displays\n" +
                "- ResourceSystemTester component\n\n" +
                "Press Play to test the Resource System!", 
                "OK");
        }

        private static Button CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = btnObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.6f, 0.9f);

            Button button = btnObj.AddComponent<Button>();

            // Create button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return button;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(obj, value);
                EditorUtility.SetDirty((Object)obj);
            }
            else
            {
                Debug.LogWarning($"Field '{fieldName}' not found on {obj.GetType().Name}");
            }
        }
    }
}
#endif
