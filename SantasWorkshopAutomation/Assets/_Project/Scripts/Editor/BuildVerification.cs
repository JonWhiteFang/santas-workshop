using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.Linq;

namespace SantasWorkshop.Editor
{
    /// <summary>
    /// Build verification and validation tools
    /// </summary>
    public static class BuildVerification
    {
        [MenuItem("Tools/Verify Project Build")]
        public static void VerifyProjectBuild()
        {
            Debug.Log("=== BUILD VERIFICATION STARTED ===\n");
            
            bool allChecksPassed = true;
            
            // 1. Check for compilation errors
            allChecksPassed &= CheckCompilation();
            
            // 2. Check assembly definitions
            allChecksPassed &= CheckAssemblies();
            
            // 3. Check scenes in build settings
            allChecksPassed &= CheckBuildScenes();
            
            // 4. Check required packages
            allChecksPassed &= CheckPackages();
            
            // 5. Check project settings
            allChecksPassed &= CheckProjectSettings();
            
            // Final result
            Debug.Log("\n=== BUILD VERIFICATION COMPLETE ===");
            if (allChecksPassed)
            {
                Debug.Log("<color=green><b>✓ ALL CHECKS PASSED - Project is ready to build!</b></color>");
            }
            else
            {
                Debug.LogWarning("<color=yellow><b>⚠ SOME CHECKS FAILED - Review warnings above</b></color>");
            }
        }
        
        private static bool CheckCompilation()
        {
            Debug.Log("\n--- Checking Compilation ---");
            
            if (EditorUtility.scriptCompilationFailed)
            {
                Debug.LogError("✗ Compilation FAILED! There are script errors.");
                return false;
            }
            
            Debug.Log("✓ Compilation successful - No script errors");
            return true;
        }
        
        private static bool CheckAssemblies()
        {
            Debug.Log("\n--- Checking Assemblies ---");
            
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            var requiredAssemblies = new[]
            {
                "SantasWorkshop.Core",
                "SantasWorkshop.Data",
                "SantasWorkshop.Machines",
                "SantasWorkshop.Utilities",
                "SantasWorkshop.Editor"
            };
            
            bool allFound = true;
            foreach (var required in requiredAssemblies)
            {
                var found = assemblies.Any(a => a.GetName().Name == required);
                if (found)
                {
                    Debug.Log($"✓ Found assembly: {required}");
                }
                else
                {
                    Debug.LogWarning($"⚠ Missing assembly: {required}");
                    allFound = false;
                }
            }
            
            return allFound;
        }
        
        private static bool CheckBuildScenes()
        {
            Debug.Log("\n--- Checking Build Scenes ---");
            
            var scenes = EditorBuildSettings.scenes;
            
            if (scenes.Length == 0)
            {
                Debug.LogWarning("⚠ No scenes in build settings!");
                return false;
            }
            
            Debug.Log($"Found {scenes.Length} scene(s) in build settings:");
            bool allValid = true;
            
            foreach (var scene in scenes)
            {
                if (scene.enabled)
                {
                    if (System.IO.File.Exists(scene.path))
                    {
                        Debug.Log($"✓ {scene.path}");
                    }
                    else
                    {
                        Debug.LogError($"✗ Scene not found: {scene.path}");
                        allValid = false;
                    }
                }
                else
                {
                    Debug.Log($"○ {scene.path} (disabled)");
                }
            }
            
            return allValid;
        }
        
        private static bool CheckPackages()
        {
            Debug.Log("\n--- Checking Required Packages ---");
            
            var requiredPackages = new[]
            {
                "com.unity.render-pipelines.universal",
                "com.unity.inputsystem",
                "com.unity.textmeshpro",
                "com.unity.burst",
                "com.unity.collections",
                "com.unity.mathematics"
            };
            
            bool allFound = true;
            
            foreach (var package in requiredPackages)
            {
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath($"Packages/{package}");
                if (packageInfo != null)
                {
                    Debug.Log($"✓ {package} ({packageInfo.version})");
                }
                else
                {
                    Debug.LogWarning($"⚠ Package not found: {package}");
                    allFound = false;
                }
            }
            
            return allFound;
        }
        
        private static bool CheckProjectSettings()
        {
            Debug.Log("\n--- Checking Project Settings ---");
            
            bool allValid = true;
            
            // Check company name
            if (string.IsNullOrEmpty(PlayerSettings.companyName))
            {
                Debug.LogWarning("⚠ Company name not set");
                allValid = false;
            }
            else
            {
                Debug.Log($"✓ Company: {PlayerSettings.companyName}");
            }
            
            // Check product name
            if (string.IsNullOrEmpty(PlayerSettings.productName))
            {
                Debug.LogWarning("⚠ Product name not set");
                allValid = false;
            }
            else
            {
                Debug.Log($"✓ Product: {PlayerSettings.productName}");
            }
            
            // Check version
            Debug.Log($"✓ Version: {PlayerSettings.bundleVersion}");
            
            // Check scripting backend
            var backend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone);
            Debug.Log($"✓ Scripting Backend: {backend}");
            
            // Check API compatibility
            var apiLevel = PlayerSettings.GetApiCompatibilityLevel(NamedBuildTarget.Standalone);
            Debug.Log($"✓ API Compatibility: {apiLevel}");
            
            return allValid;
        }
        
        [MenuItem("Tools/Quick Build Test (Development)")]
        public static void QuickBuildTest()
        {
            Debug.Log("=== QUICK BUILD TEST ===");
            
            // Verify first
            VerifyProjectBuild();
            
            Debug.Log("\nNote: To perform an actual build, use File > Build Settings > Build");
            Debug.Log("Or use the BuildConfiguration.BuildDevelopment() method");
        }
    }
}
