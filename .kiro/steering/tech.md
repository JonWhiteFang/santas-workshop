# Santa's Workshop Automation - Technical Stack

**Last Updated**: November 2, 2025

## Engine & Platform

- **Engine**: Unity 2022+ LTS (Long Term Support)
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Target Platform**: PC (Windows primary, Linux/Mac secondary)
- **Distribution**: Steam

## Core Technologies

### Unity Systems
- **ECS (Entity Component System)**: For high-performance simulation
- **Burst Compiler**: Optimizing conveyor/item updates
- **Job System**: Parallel processing for logistics and resource management
- **UI Toolkit**: Modern UI framework for all interface elements
- **URP**: Lighting, post-processing, and visual effects

### Performance Optimization
- **Chunking & Spatial Partitioning**: Update only visible factory sections
- **GPU Instancing**: Efficient rendering of repeated objects (conveyors, machines)
- **LOD (Level of Detail)**: Distance-based model complexity
- **Baked Lighting**: Pre-computed global illumination for static elements
- **Async Simulation Snapshots**: UI reads from cached state to avoid blocking

## Architecture Layers

```
┌─────────────────────────────────────┐
│       Render Layer                  │  Visuals, lighting, particles, animation
├─────────────────────────────────────┤
│       Simulation Layer              │  Machines, power, logistics, recipes, world state
├─────────────────────────────────────┤
│       UI Layer                      │  UI Toolkit with async reads from sim snapshots
└─────────────────────────────────────┘
```

## Core Systems

1. **Resource Manager**: Tracks all resources, extraction rates, and consumption
2. **Machine Framework**: Base classes for all factory buildings (extractors, processors, assemblers)
3. **Power Grid**: Electricity generation, distribution, and consumption tracking
4. **Logistics System**: Conveyor belts, pipes, carts, item routing
5. **Research & Tech Tree**: Branching progression with 8 major branches
6. **Mission System**: Campaign missions and sandbox challenges
7. **Save/Load System**: Versioned schema for factory persistence

## Common Commands

### Project Setup
```powershell
# Clone repository
git clone <repository-url>
cd SantasWorkshopAutomation

# Open in Unity Hub (Unity 2022 LTS required)
# File → Open Project → Select project folder
```

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

# Deep Profile for detailed analysis
# Profiler → Deep Profile (warning: significant overhead)
```

## Dependencies & Packages

### Unity Packages (Package Manager)
- Universal RP
- UI Toolkit
- Burst Compiler
- Collections (for ECS)
- Mathematics (for Jobs)
- Input System (new input system)
- Cinemachine (camera control)
- TextMeshPro (text rendering)

### Third-Party Assets (if applicable)
- DOTween (animation tweening)
- Odin Inspector (enhanced editor tools)
- Addressables (asset management)

## Build Configuration

### Development Build
- Debug symbols enabled
- Profiler connection enabled
- Script debugging enabled
- Faster iteration time

### Release Build
- IL2CPP scripting backend
- Code stripping (High)
- Optimizations enabled
- Compressed assets

## Version Control

- **Git**: Primary VCS
- **Git LFS**: For large binary assets (models, textures, audio)
- **.gitignore**: Excludes Library/, Temp/, Logs/, UserSettings/

## Performance Targets

- **Frame Rate**: 60 FPS minimum on mid-range hardware
- **Factory Scale**: Support 1000+ active machines without degradation
- **Save/Load Time**: <5 seconds for typical factory
- **Memory Usage**: <4GB RAM for large factories

## Known Limitations

- **Single-threaded simulation**: Main simulation loop runs on one thread (Jobs used for sub-systems)
- **Save file size**: Large factories may produce 10-50MB save files
- **GPU requirements**: URP requires DirectX 11+ / Vulkan / Metal
