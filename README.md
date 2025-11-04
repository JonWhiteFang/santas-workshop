# 🎅 Santa's Workshop Automation

> Build and optimize Santa's automated workshop to produce high-quality toys before Christmas!

[![Unity Version](https://img.shields.io/badge/Unity-2022%20LTS-blue.svg)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-PC-lightgrey.svg)](https://store.steampowered.com/)
[![License](https://img.shields.io/badge/License-Proprietary-red.svg)]()

## 🎁 About

**Santa's Workshop Automation** is a factory automation and management simulation game where players design, build, and optimize Santa's toy production facility. Balance traditional craftsmanship with modern automation and magical enhancements to meet Christmas demand while maintaining toy quality.

### Key Features

- **🏭 Deep Factory Building**: Design complex production chains from raw materials to finished toys
- **📅 Seasonal Cycle**: Manage production through an in-game year culminating in the Christmas Rush
- **🔬 Branching Research**: 8 tech trees including Automation, Energy, Materials, Toys, Logistics, Elf Management, Magic, and Aesthetics
- **⭐ Quality System**: Produce S/A/B/C grade toys based on speed, materials, energy efficiency, and magic infusion
- **🎯 Mission-Based Progression**: Campaign tutorials leading to infinite sandbox mode
- **🔄 Prestige System**: Seasonal resets with meta-upgrades and persistent unlocks
- **❄️ Cozy Atmosphere**: No fail states—underperformance yields coaching, not game over

## 🎮 Genre & Target Audience

**Genre**: Factory Automation / Management Simulation  
**Target Audience**: Fans of Factorio, Satisfactory, and cozy management games who enjoy optimization puzzles with a festive, low-stress atmosphere

## 🛠️ Technical Stack

- **Engine**: Unity 2022+ LTS
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Architecture**: ECS (Entity Component System) with Burst Compiler and Job System
- **UI Framework**: UI Toolkit
- **Platform**: PC (Windows primary, Linux/Mac secondary)
- **Distribution**: Steam

### Performance Targets

- 60 FPS minimum on mid-range hardware
- Support for 1000+ active machines
- Save/load time <5 seconds for typical factories
- Memory usage <4GB RAM for large factories

## 📁 Project Structure

```
SantasWorkshopAutomation/
├── Assets/
│   ├── _Project/              # Main project assets
│   │   ├── Scripts/           # All C# code
│   │   │   ├── Core/          # Core systems (ResourceManager, PowerGrid, etc.)
│   │   │   ├── Machines/      # Machine framework and implementations
│   │   │   ├── Logistics/     # Conveyors, pipes, routing
│   │   │   ├── Research/      # Tech tree system
│   │   │   ├── Missions/      # Campaign and objectives
│   │   │   └── UI/            # UI controllers
│   │   ├── Prefabs/           # Reusable game objects
│   │   ├── Scenes/            # Unity scenes
│   │   └── ScriptableObjects/ # Data-driven configuration
│   ├── Art/                   # Models, textures, animations, VFX
│   ├── Audio/                 # Music, SFX, voice-over
│   └── UI/                    # UI Toolkit assets (UXML, USS)
├── Packages/                  # Unity package dependencies
├── ProjectSettings/           # Unity project configuration
└── .kiro/                     # AI assistant configuration
    └── steering/              # Project documentation
```

## 🚀 Getting Started

### Prerequisites

- **Unity Hub**: Latest version
- **Unity 2022 LTS**: Install via Unity Hub (2022.3.x or later recommended)
- **Git**: For version control
- **Git LFS**: For large binary assets

### Required Unity Packages

The following packages are required and should be installed via the Unity Package Manager:

- **Universal RP** (com.unity.render-pipelines.universal)
- **Burst Compiler** (com.unity.burst)
- **Collections** (com.unity.collections)
- **Mathematics** (com.unity.mathematics)
- **Input System** (com.unity.inputsystem)
- **Cinemachine** (com.unity.cinemachine)
- **TextMeshPro** (com.unity.textmeshpro)
- **UI Toolkit** (built-in)

These packages are automatically included in the project manifest and will be installed when you open the project.

### Third-Party Assets

Currently, no third-party assets are required for the project. All functionality is implemented using Unity's built-in systems and the packages listed above.

**Optional Third-Party Assets** (for future consideration):
- **DOTween**: Animation tweening library (not currently used)
- **Odin Inspector**: Enhanced editor tools (not currently used)
- **Addressables**: Asset management system (not currently used)

If third-party assets are added in the future, they will be documented here with installation instructions.

### Installation

1. Clone the repository:
```powershell
git clone <repository-url>
cd SantasWorkshopAutomation
```

2. Open in Unity Hub:
   - Open Unity Hub
   - Click "Add" → Select project folder
   - Open project with Unity 2022 LTS

3. Wait for Unity to import assets (first import may take several minutes)

### Running the Game

1. Open the main scene: `Assets/_Project/Scenes/Workshop.unity`
2. Click the Play button in Unity Editor
3. Use WASD to move camera, mouse to interact

## 🔧 Development

### Implemented Systems

#### Resource Management System ✅ (Completed: Nov 4, 2025)

The Resource System provides centralized management of all game resources including raw materials, refined goods, components, and finished toys.

**Key Features**:
- **Global Inventory**: Centralized resource tracking via singleton ResourceManager
- **8 Resource Categories**: RawMaterial, Refined, Component, ToyPart, FinishedToy, Magic, Energy, Special
- **Event System**: Real-time notifications when resources change
- **Capacity Management**: Per-resource storage limits with overflow protection
- **Save/Load Support**: Full persistence of resource state
- **High Performance**: 1000+ operations per second, minimal memory footprint

**Usage Example**:
```csharp
// Add resources
ResourceManager.Instance.AddResource("wood", 50);

// Consume resources
if (ResourceManager.Instance.TryConsumeResource("wood", 10))
{
    Debug.Log("Consumed 10 wood");
}

// Check availability
if (ResourceManager.Instance.HasResource("iron_ore", 5))
{
    Debug.Log("Have enough iron ore");
}

// Subscribe to changes
ResourceManager.Instance.OnResourceChanged += (resourceId, amount) =>
{
    Debug.Log($"{resourceId} changed to {amount}");
};
```

**Test Results**: 28/28 unit tests passing, all integration tests passing

**Documentation**:
- [Resource System Completion Summary](SantasWorkshopAutomation/RESOURCE_SYSTEM_COMPLETION_SUMMARY.md)
- [Integration Test Report](SantasWorkshopAutomation/INTEGRATION_TEST_REPORT.md)
- [Resource System Spec](.kiro/specs/1.2-resource-system/)

**Sample Resources**: 8 sample resources included (Wood, Iron Ore, Coal, Wood Plank, Iron Ingot, Iron Gear, Paint, Wooden Train)

**Test Scene**: `Assets/_Project/Scenes/TestScenes/TestScene_ResourceSystem.unity`

---

### Building

#### Quick Build (Using the Unity Editor Menu)

The project includes custom build scripts accessible from the Unity Editor menu:

1. **Configure Platform**: `Santa's Workshop → Build → Configure Windows Platform`
   - Sets Windows x86_64 as the target platform

2. **Development Build**: `Santa's Workshop → Build → Build Development (Windows)`
   - Automatically configures and builds a development version
   - Output: `Builds/Dev/SantasWorkshop.exe`

3. **Release Build**: `Santa's Workshop → Build → Build Release (Windows)`
   - Automatically configures and builds an optimized release version
   - Output: `Builds/Release/SantasWorkshop.exe`

#### Manual Build Configuration

##### Development Build Settings

To manually configure development build settings:

1. **From the Unity Editor Menu**: `Santa's Workshop → Build → Configure Development Settings`

2. **Manual Configuration**:
   - File → Build Settings → Select "Windows, Mac, Linux" platform
   - Check "Development Build"
   - Check "Script Debugging"
   - Check "Profiler Connection"
   - Edit → Project Settings → Player → Other Settings:
     - Scripting Backend: **Mono**
     - Managed Stripping Level: **Disabled**

3. **Command-line development build**:
```powershell
"C:\Program Files\Unity\Hub\Editor\2022.x.xxf1\Editor\Unity.exe" `
  -quit -batchmode -projectPath . `
  -buildWindows64Player "Builds/Dev/SantasWorkshop.exe" `
  -development
```

**Development Build Configuration**:
- ✅ Development Build: **Enabled**
- ✅ Script Debugging: **Enabled**
- ✅ Profiler Connection: **Enabled**
- ✅ Scripting Backend: **Mono** (faster iteration)
- ✅ Code Stripping: **Disabled** (full debugging)
- 📦 Build Size: Larger (~500MB-1GB)
- ⚡ Performance: Moderate (not optimized)
- 🐛 Debugging: Full support with symbols

##### Release Build Settings

To manually configure release build settings:

1. **From the Unity Editor Menu**: `Santa's Workshop → Build → Configure Release Settings`

2. **Manual Configuration**:
   - File → Build Settings → Select "Windows, Mac, Linux" platform
   - Uncheck "Development Build"
   - Uncheck "Script Debugging"
   - Uncheck "Profiler Connection"
   - Edit → Project Settings → Player → Other Settings:
     - Scripting Backend: **IL2CPP**
     - Managed Stripping Level: **High**
     - IL2CPP Code Generation: **Release**

3. **Command-line release build**:
```powershell
"C:\Program Files\Unity\Hub\Editor\2022.x.xxf1\Editor\Unity.exe" `
  -quit -batchmode -projectPath . `
  -buildWindows64Player "Builds/Release/SantasWorkshop.exe"
```

**Release Build Configuration**:
- ❌ Development Build: **Disabled**
- ❌ Script Debugging: **Disabled**
- ❌ Profiler Connection: **Disabled**
- ✅ Scripting Backend: **IL2CPP** (better performance)
- ✅ Code Stripping: **High** (smaller build size)
- ✅ IL2CPP Configuration: **Release** (optimized)
- 📦 Build Size: Smaller (~200-400MB)
- ⚡ Performance: Optimized for production
- 🐛 Debugging: Limited (no symbols)

#### Build Output Structure

```
Builds/
├── Dev/                         # Development builds
│   ├── SantasWorkshop.exe
│   ├── SantasWorkshop_Data/
│   ├── MonoBleedingEdge/        # Mono runtime
│   └── UnityPlayer.dll
└── Release/                     # Release builds
    ├── SantasWorkshop.exe
    ├── SantasWorkshop_Data/
    ├── GameAssembly.dll         # IL2CPP compiled code
    └── UnityPlayer.dll
```

**Note**: The `Builds/` directory is excluded from version control (.gitignore).

### Testing

```powershell
# Run tests in Unity Test Runner
# Window → General → Test Runner → Run All

# Command-line tests
"C:\Program Files\Unity\Hub\Editor\2022.x.xxf1\Editor\Unity.exe" `
  -runTests -batchmode -projectPath . `
  -testResults TestResults.xml
```

### Performance Profiling

```powershell
# Unity Profiler
# Window → Analysis → Profiler (Ctrl+7)
```

## 📚 Documentation

- **[Product Overview](.kiro/steering/product.md)**: Game concept, features, and target audience
- **[Technical Stack](.kiro/steering/tech.md)**: Technologies, architecture, and dependencies
- **[Project Structure](.kiro/steering/structure.md)**: Directory layout and organization
- **[Unity Development Guidelines](.kiro/steering/unity-csharp-development.md)**: C# best practices and Unity patterns
- **[Full Game Design Document](Santas_Workshop_Automation_GDD_FULL.txt)**: Complete design specification

## 🎨 Art & Audio Guidelines

### Models
- **Format**: FBX preferred
- **Scale**: 1 Unity unit = 1 meter
- **LOD**: 3 levels (High, Medium, Low) for machines

### Textures
- **Resolution**: 1024x1024 for machines, 2048x2048 for environment
- **Format**: PNG for UI, TGA/PSD for 3D assets

### Audio
- **Format**: WAV for SFX, OGG for music
- **Sample Rate**: 44.1kHz, 16-bit

## 🤝 Contributing

This is a proprietary project. Contribution guidelines will be provided to team members separately.

### Code Style

- Follow C# naming conventions (PascalCase for public, _camelCase for private)
- Use XML documentation for public APIs
- Organize code with `#region` directives
- See [Unity Development Guidelines](.kiro/steering/unity-csharp-development.md) for details

## 📋 Roadmap

### Phase 1: Core Systems (Current)
- ✅ Project setup and architecture
- ✅ Resource management system (COMPLETE - Nov 4, 2025)
- 🔄 Machine framework
- 🔄 Power grid system
- ⏳ Basic logistics (conveyors)

### Phase 2: Gameplay Loop
- ⏳ Research and tech tree
- ⏳ Mission system
- ⏳ Toy production chains
- ⏳ Quality grading system

### Phase 3: Polish & Content
- ⏳ UI/UX refinement
- ⏳ Visual effects and animations
- ⏳ Audio implementation
- ⏳ Campaign missions

### Phase 4: Launch Preparation
- ⏳ Performance optimization
- ⏳ Save/load system
- ⏳ Steam integration
- ⏳ Playtesting and balancing

## 🎯 Success Metrics

- Steam rating ≥85%
- Player retention >25% at 30 days
- Average session length 45–90 minutes
- Christmas Spirit Score as primary engagement metric

## 📄 License

Proprietary - All rights reserved

## 🎄 Credits

**Genre**: Factory Automation / Management Simulation  
**Platform**: PC (Steam)  
**Pricing**: Premium single purchase (£25–£35), no microtransactions

---

*Built with ❤️ and festive cheer*
