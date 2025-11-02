using UnityEditor;
using UnityEngine;

namespace SantasWorkshop.Editor
{
    /// <summary>
    /// Build configuration utility for Santa's Workshop Automation.
    /// Provides methods to configure Development and Release build settings.
    /// </summary>
    public static class BuildConfiguration
    {
        private const string BuildsDirectory = "Builds";
        
        /// <summary>
        /// Configure build settings for Windows x86_64 platform.
        /// This sets up the base platform configuration.
        /// </summary>
        [MenuItem("Santa's Workshop/Build/Configure Windows Platform")]
        public static void ConfigureWindowsPlatform()
        {
            // Switch to Windows Standalone platform
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Standalone, 
                BuildTarget.StandaloneWindows64
            );
            
            // Set architecture to x86_64
            PlayerSettings.SetArchitecture(
                BuildTargetGroup.Standalone, 
                (int)ScriptingImplementation.Mono2x
            );
            
            Debug.Log("✓ Windows x86_64 platform configured");
        }
        
        /// <summary>
        /// Configure Development build settings.
        /// Enables debugging, profiling, and development features.
        /// </summary>
        [MenuItem("Santa's Workshop/Build/Configure Development Settings")]
        public static void ConfigureDevelopmentBuild()
        {
            // Enable Development Build
            EditorUserBuildSettings.development = true;
            
            // Enable Script Debugging
            EditorUserBuildSettings.allowDebugging = true;
            
            // Enable Profiler connection
            EditorUserBuildSettings.connectProfiler = true;
            
            // Enable Deep Profiling Support (optional, can be toggled)
            EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
            
            // Use Mono scripting backend for faster iteration
            PlayerSettings.SetScriptingBackend(
                BuildTargetGroup.Standalone, 
                ScriptingImplementation.Mono2x
            );
            
            // Set stripping level to Disabled for development
            PlayerSettings.SetManagedStrippingLevel(
                BuildTargetGroup.Standalone,
                ManagedStrippingLevel.Disabled
            );
            
            Debug.Log("✓ Development build settings configured:");
            Debug.Log("  - Development Build: Enabled");
            Debug.Log("  - Script Debugging: Enabled");
            Debug.Log("  - Profiler Connection: Enabled");
            Debug.Log("  - Scripting Backend: Mono");
            Debug.Log("  - Code Stripping: Disabled");
        }
        
        /// <summary>
        /// Configure Release build settings.
        /// Optimizes for performance and build size.
        /// </summary>
        [MenuItem("Santa's Workshop/Build/Configure Release Settings")]
        public static void ConfigureReleaseBuild()
        {
            // Disable Development Build
            EditorUserBuildSettings.development = false;
            
            // Disable Script Debugging
            EditorUserBuildSettings.allowDebugging = false;
            
            // Disable Profiler connection
            EditorUserBuildSettings.connectProfiler = false;
            
            // Disable Deep Profiling Support
            EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
            
            // Use IL2CPP scripting backend for better performance
            PlayerSettings.SetScriptingBackend(
                BuildTargetGroup.Standalone, 
                ScriptingImplementation.IL2CPP
            );
            
            // Set stripping level to High for smaller build size
            PlayerSettings.SetManagedStrippingLevel(
                BuildTargetGroup.Standalone,
                ManagedStrippingLevel.High
            );
            
            // Enable code optimization
            PlayerSettings.SetIl2CppCompilerConfiguration(
                BuildTargetGroup.Standalone,
                Il2CppCompilerConfiguration.Release
            );
            
            Debug.Log("✓ Release build settings configured:");
            Debug.Log("  - Development Build: Disabled");
            Debug.Log("  - Script Debugging: Disabled");
            Debug.Log("  - Profiler Connection: Disabled");
            Debug.Log("  - Scripting Backend: IL2CPP");
            Debug.Log("  - Code Stripping: High");
            Debug.Log("  - IL2CPP Configuration: Release");
        }
        
        /// <summary>
        /// Build Development version for Windows.
        /// </summary>
        [MenuItem("Santa's Workshop/Build/Build Development (Windows)")]
        public static void BuildDevelopment()
        {
            ConfigureWindowsPlatform();
            ConfigureDevelopmentBuild();
            
            string buildPath = $"{BuildsDirectory}/Dev/SantasWorkshop.exe";
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler
            };
            
            BuildPipeline.BuildPlayer(buildOptions);
            Debug.Log($"✓ Development build completed: {buildPath}");
        }
        
        /// <summary>
        /// Build Release version for Windows.
        /// </summary>
        [MenuItem("Santa's Workshop/Build/Build Release (Windows)")]
        public static void BuildRelease()
        {
            ConfigureWindowsPlatform();
            ConfigureReleaseBuild();
            
            string buildPath = $"{BuildsDirectory}/Release/SantasWorkshop.exe";
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            
            BuildPipeline.BuildPlayer(buildOptions);
            Debug.Log($"✓ Release build completed: {buildPath}");
        }
        
        /// <summary>
        /// Get all enabled scene paths from build settings.
        /// </summary>
        private static string[] GetScenePaths()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }
            return scenes.ToArray();
        }
    }
}
