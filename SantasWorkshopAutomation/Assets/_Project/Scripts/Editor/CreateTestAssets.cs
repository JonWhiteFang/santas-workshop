using UnityEngine;
using UnityEditor;
using SantasWorkshop.Data;

namespace SantasWorkshop.Editor
{
    /// <summary>
    /// Editor utility to create test ScriptableObject assets for the Machine Framework.
    /// </summary>
    public static class CreateTestAssets
    {
        private const string TestAssetsPath = "Assets/_Project/ScriptableObjects/TestAssets";
        private const string RecipesPath = TestAssetsPath + "/Recipes";
        private const string MachinesPath = TestAssetsPath + "/Machines";

        [MenuItem("Santa/Create Test Assets/All Test Assets")]
        public static void CreateAllTestAssets()
        {
            CreateTestRecipes();
            CreateTestMachineData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("All test assets created successfully!");
        }

        [MenuItem("Santa/Create Test Assets/Test Recipes")]
        public static void CreateTestRecipes()
        {
            // Ensure directories exist
            if (!AssetDatabase.IsValidFolder(TestAssetsPath))
            {
                AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "TestAssets");
            }
            if (!AssetDatabase.IsValidFolder(RecipesPath))
            {
                AssetDatabase.CreateFolder(TestAssetsPath, "Recipes");
            }

            // Create Test Extractor Recipe (no inputs, produces iron ore)
            Recipe extractorRecipe = ScriptableObject.CreateInstance<Recipe>();
            extractorRecipe.recipeId = "test_extractor_recipe";
            extractorRecipe.recipeName = "Test Extractor Recipe";
            extractorRecipe.inputs = new ResourceStack[0]; // No inputs for extractor
            extractorRecipe.outputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "iron_ore", amount = 1 }
            };
            extractorRecipe.processingTime = 2f;
            extractorRecipe.powerConsumption = 10f;
            extractorRecipe.requiredTier = 1;

            AssetDatabase.CreateAsset(extractorRecipe, $"{RecipesPath}/TestExtractorRecipe.asset");
            Debug.Log("Created TestExtractorRecipe.asset");

            // Create Test Processor Recipe (iron ore -> iron ingot)
            Recipe processorRecipe = ScriptableObject.CreateInstance<Recipe>();
            processorRecipe.recipeId = "test_processor_recipe";
            processorRecipe.recipeName = "Test Processor Recipe";
            processorRecipe.inputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "iron_ore", amount = 1 }
            };
            processorRecipe.outputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "iron_ingot", amount = 1 }
            };
            processorRecipe.processingTime = 3f;
            processorRecipe.powerConsumption = 20f;
            processorRecipe.requiredTier = 1;

            AssetDatabase.CreateAsset(processorRecipe, $"{RecipesPath}/TestProcessorRecipe.asset");
            Debug.Log("Created TestProcessorRecipe.asset");
        }

        [MenuItem("Santa/Create Test Assets/Test Machine Data")]
        public static void CreateTestMachineData()
        {
            // Ensure directories exist
            if (!AssetDatabase.IsValidFolder(TestAssetsPath))
            {
                AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "TestAssets");
            }
            if (!AssetDatabase.IsValidFolder(MachinesPath))
            {
                AssetDatabase.CreateFolder(TestAssetsPath, "Machines");
            }

            // Load test recipes
            Recipe extractorRecipe = AssetDatabase.LoadAssetAtPath<Recipe>($"{RecipesPath}/TestExtractorRecipe.asset");
            Recipe processorRecipe = AssetDatabase.LoadAssetAtPath<Recipe>($"{RecipesPath}/TestProcessorRecipe.asset");

            // Create Test Extractor Machine Data
            MachineData extractorData = ScriptableObject.CreateInstance<MachineData>();
            extractorData.machineName = "Test Extractor";
            extractorData.description = "A test extractor machine that produces iron ore from nothing. Used for testing the Machine Framework.";
            extractorData.gridSize = new Vector2Int(1, 1);
            extractorData.tier = 1;
            extractorData.baseProcessingSpeed = 1f;
            extractorData.basePowerConsumption = 10f;
            
            // Extractor has no input ports, only output
            extractorData.inputPortCount = 0;
            extractorData.outputPortCount = 1;
            extractorData.inputPortPositions = new Vector3[0];
            extractorData.outputPortPositions = new Vector3[]
            {
                new Vector3(0.5f, 0.5f, 0f) // Right side
            };
            
            extractorData.bufferCapacity = 10;
            
            if (extractorRecipe != null)
            {
                extractorData.availableRecipes.Add(extractorRecipe);
            }
            else
            {
                Debug.LogWarning("TestExtractorRecipe not found. Create recipes first.");
            }

            AssetDatabase.CreateAsset(extractorData, $"{MachinesPath}/TestExtractorData.asset");
            Debug.Log("Created TestExtractorData.asset");

            // Create Test Processor Machine Data
            MachineData processorData = ScriptableObject.CreateInstance<MachineData>();
            processorData.machineName = "Test Processor";
            processorData.description = "A test processor machine that converts iron ore into iron ingots. Used for testing the Machine Framework.";
            processorData.gridSize = new Vector2Int(1, 1);
            processorData.tier = 1;
            processorData.baseProcessingSpeed = 1f;
            processorData.basePowerConsumption = 20f;
            
            // Processor has both input and output ports
            processorData.inputPortCount = 1;
            processorData.outputPortCount = 1;
            processorData.inputPortPositions = new Vector3[]
            {
                new Vector3(-0.5f, 0.5f, 0f) // Left side
            };
            processorData.outputPortPositions = new Vector3[]
            {
                new Vector3(0.5f, 0.5f, 0f) // Right side
            };
            
            processorData.bufferCapacity = 10;
            
            if (processorRecipe != null)
            {
                processorData.availableRecipes.Add(processorRecipe);
            }
            else
            {
                Debug.LogWarning("TestProcessorRecipe not found. Create recipes first.");
            }

            AssetDatabase.CreateAsset(processorData, $"{MachinesPath}/TestProcessorData.asset");
            Debug.Log("Created TestProcessorData.asset");
        }

        [MenuItem("Santa/Create Test Assets/Clean Test Assets")]
        public static void CleanTestAssets()
        {
            if (AssetDatabase.IsValidFolder(TestAssetsPath))
            {
                AssetDatabase.DeleteAsset(TestAssetsPath);
                AssetDatabase.Refresh();
                Debug.Log("Test assets cleaned successfully!");
            }
            else
            {
                Debug.Log("No test assets to clean.");
            }
        }
    }
}
