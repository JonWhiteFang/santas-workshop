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
- **Unity 2022 LTS**: Install via Unity Hub
- **Git**: For version control
- **Git LFS**: For large binary assets

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

### Building

```powershell
# Build from Unity Editor
# File → Build Settings → Select Platform → Build

# Command-line build (Windows)
"C:\Program Files\Unity\Hub\Editor\2022.x.xxf1\Editor\Unity.exe" `
  -quit -batchmode -projectPath . `
  -buildWindows64Player "Builds/SantasWorkshop.exe"
```

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
- 🔄 Resource management system
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
