using UnityEditor;
using UnityEngine;

namespace SantasWorkshop.Editor
{
    public static class CompilationTest
    {
        [MenuItem("Tools/Test Compilation")]
        public static void TestCompilation()
        {
            Debug.Log("=== Compilation Test Started ===");
            
            // Check for compilation errors
            if (EditorUtility.scriptCompilationFailed)
            {
                Debug.LogError("Compilation FAILED! There are script errors.");
            }
            else
            {
                Debug.Log("✓ Compilation SUCCESSFUL! No script errors detected.");
            }
            
            // Check assembly definitions
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            int santasWorkshopAssemblies = 0;
            
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.Contains("SantasWorkshop"))
                {
                    santasWorkshopAssemblies++;
                    Debug.Log($"✓ Found assembly: {assembly.GetName().Name}");
                }
            }
            
            Debug.Log($"=== Found {santasWorkshopAssemblies} Santa's Workshop assemblies ===");
            Debug.Log("=== Compilation Test Complete ===");
        }
    }
}
