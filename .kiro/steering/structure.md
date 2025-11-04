# Santa's Workshop Automation - Project Structure

**Last Updated**: November 4, 2025

## Related Documentation

- **[Product Overview](product.md)** - Game concept, features, and design pillars
- **[Technical Stack](tech.md)** - Technologies, architecture, and current implementation status
- **[Game Design Patterns](game-design-patterns.md)** - Architectural patterns and best practices
- **[Data Architecture](data-architecture.md)** - ScriptableObject schemas and data structures
- **[Unity Development Guidelines](unity-csharp-development.md)** - C# best practices for Unity 6

## Implementation Status

**Completed Systems** ✅:
- Resource Management System (Nov 4, 2025)
- Grid & Placement System (Nov 4, 2025)
- Assembly definition structure
- Custom build scripts
- Test framework setup

**In Progress** 🔄:
- Machine Framework (29% complete)

**Pending** ⏳:
- Power Grid System
- Logistics System
- Research System
- Mission System
- UI System

## Root Directory Layout

```
SantasWorkshopAutomation/
├── Assets/                      # All game assets and code
├── Packages/                    # Unity package dependencies
├── ProjectSettings/             # Unity project configuration
├── UserSettings/                # Local user preferences (gitignored)
├── Library/                     # Unity cache (gitignored)
├── Logs/                        # Unity logs (gitignored)
├── Temp/                        # Temporary build files (gitignored)
├── Builds/                      # Build output directory
└── .kiro/                       # Kiro AI assistant configuration
    └── steering/                # Project steering documents
```

## Assets Directory Structure

```
Assets/
├── _Project/                    # Main project assets (underscore for top sorting)
│   ├── Scripts/                 # All C# scripts
│   │   ├── Core/                # Core systems and managers
│   │   │   ├── ResourceManager.cs
│   │   │   ├── PowerGrid.cs
│   │   │   ├── SaveLoadSystem.cs
│   │   │   └── GameManager.cs
│   │   ├── Machines/            # Machine framework and implementations
│   │   │   ├── Base/            # Base classes and interfaces
│   │   │   ├── Extractors/      # Mining drills, harvesters
│   │   │   ├── Processors/      # Smelters, sawmills, refineries
│   │   │   ├── Assemblers/      # Assembly machines
│   │   │   └── Utility/         # Storage, power generators
│   │   ├── Logistics/           # Transport and routing
│   │   │   ├── Conveyors/
│   │   │   ├── Pipes/
│   │   │   └── Routing/
│   │   ├── Research/            # Tech tree and progression
│   │   ├── Missions/            # Campaign and objectives
│   │   ├── UI/                  # UI controllers and views
│   │   ├── Data/                # ScriptableObjects and data structures
│   │   └── Utilities/           # Helper classes and extensions
│   ├── Prefabs/                 # Reusable game objects
│   │   ├── Machines/
│   │   ├── UI/
│   │   ├── Effects/
│   │   └── Environment/
│   ├── Scenes/                  # Unity scenes
│   │   ├── MainMenu.unity
│   │   ├── Workshop.unity       # Main gameplay scene
│   │   └── TestScenes/          # Development test scenes
│   ├── ScriptableObjects/       # Data assets
│   │   ├── Resources/           # Resource definitions
│   │   ├── Recipes/             # Crafting recipes
│   │   ├── Machines/            # Machine configurations
│   │   ├── Research/            # Tech tree nodes
│   │   └── Toys/                # Toy definitions
│   ├── Materials/               # Unity materials
│   ├── Shaders/                 # Custom shaders
│   └── Settings/                # URP and project settings
├── Art/                         # Visual assets
│   ├── Models/                  # 3D models (.fbx, .blend)
│   │   ├── Machines/
│   │   ├── Buildings/
│   │   ├── Environment/
│   │   └── Toys/
│   ├── Textures/                # Texture maps
│   │   ├── Machines/
│   │   ├── Environment/
│   │   └── UI/
│   ├── Animations/              # Animation clips and controllers
│   └── VFX/                     # Visual effects (particles, shaders)
├── Audio/                       # Sound assets
│   ├── Music/                   # Background music tracks
│   ├── SFX/                     # Sound effects
│   │   ├── Machines/
│   │   ├── UI/
│   │   └── Ambient/
│   └── VO/                      # Voice-over clips (elf stingers)
├── UI/                          # UI Toolkit assets
│   ├── UXML/                    # UI layout documents
│   ├── USS/                     # UI stylesheets
│   └── Assets/                  # UI sprites and icons
└── ThirdParty/                  # Third-party assets and plugins
```

## Key Organizational Principles

### 1. Separation of Concerns
- **Scripts**: Organized by system (Core, Machines, Logistics, etc.)
- **Data**: ScriptableObjects separate from code
- **Visuals**: Art assets separate from logic

### 2. Naming Conventions
- **Folders**: PascalCase (e.g., `Machines/`, `ScriptableObjects/`)
- **Scripts**: PascalCase matching class name (e.g., `ResourceManager.cs`)
- **Prefabs**: PascalCase with descriptive names (e.g., `ConveyorBelt_Tier1.prefab`)
- **Scenes**: PascalCase (e.g., `Workshop.unity`)
- **ScriptableObjects**: Descriptive names (e.g., `IronOre_Resource.asset`)

### 3. Machine Framework Hierarchy
All machines inherit from base classes:
```
IMachine (interface)
└── MachineBase (abstract)
    ├── ExtractorBase
    │   ├── MiningDrill
    │   └── WoodHarvester
    ├── ProcessorBase
    │   ├── Smelter
    │   └── Sawmill
    ├── AssemblerBase
    │   ├── BasicAssembler
    │   └── AdvancedAssembler
    └── UtilityBase
        ├── PowerGenerator
        └── StorageContainer
```

### 4. ScriptableObject Data Architecture
- **Resources**: Define raw materials, refined goods, components
- **Recipes**: Input/output mappings for machines
- **Machines**: Stats, costs, unlock requirements
- **Research**: Tech tree nodes with dependencies
- **Toys**: Final product definitions with quality tiers

### 5. Scene Organization
- **MainMenu**: Entry point, settings, save selection
- **Workshop**: Main gameplay scene (persistent)
- **TestScenes**: Isolated testing environments (not in builds)

### 6. Prefab Structure
- **Machines**: Fully configured with components, colliders, visuals
- **UI**: Reusable UI panels and widgets
- **Effects**: Particle systems, VFX prefabs
- **Environment**: Decorative and structural elements

## Special Directories

### _Project/
Underscore prefix ensures it sorts to the top in Unity's Project window, making it easy to find core project assets.

### ScriptableObjects/
All data-driven configuration lives here. Designers can modify game balance without touching code.

### TestScenes/
Development-only scenes for testing specific systems. Excluded from release builds via build settings.

## Build Output

```
Builds/
├── Windows/
│   ├── SantasWorkshop.exe
│   ├── SantasWorkshop_Data/
│   └── UnityPlayer.dll
├── Linux/
└── Mac/
```

## Asset Guidelines

### Models
- **Format**: FBX preferred, Blender source files in separate repository
- **Scale**: 1 Unity unit = 1 meter
- **Pivot**: Bottom-center for buildings, center for items
- **LOD**: 3 levels (High, Medium, Low) for machines

### Textures
- **Resolution**: 1024x1024 for machines, 2048x2048 for environment
- **Format**: PNG for UI, TGA/PSD for 3D assets
- **Naming**: `MachineName_BaseColor.png`, `MachineName_Normal.png`

### Audio
- **Format**: WAV for SFX, OGG for music
- **Sample Rate**: 44.1kHz
- **Bit Depth**: 16-bit

### Scripts
- **Namespace**: `SantasWorkshop.<System>` (e.g., `SantasWorkshop.Machines`, `SantasWorkshop.Core`)
- **Regions**: Use `#region` to organize large classes
- **Comments**: XML documentation for public APIs
- **File Organization**: One class per file, file name matches class name

## Version Control Exclusions

### .gitignore includes:
- `Library/` - Unity cache (regenerated)
- `Temp/` - Temporary files
- `Logs/` - Log files
- `UserSettings/` - Local user preferences
- `*.csproj`, `*.sln` - IDE files (regenerated)
- `Builds/` - Build output (too large)

### Git LFS tracks:
- `*.fbx`, `*.blend` - 3D models
- `*.png`, `*.tga`, `*.psd` - Textures
- `*.wav`, `*.ogg`, `*.mp3` - Audio files
- `*.mp4`, `*.mov` - Video files

## Finding Things Quickly

### Common Locations
- **Game entry point**: `Assets/_Project/Scripts/Core/GameManager.cs`
- **Resource Manager**: `Assets/_Project/Scripts/Core/ResourceManager.cs` ✅
- **Grid Manager**: `Assets/_Project/Scripts/Core/GridManager.cs` ✅
- **Placement Controller**: `Assets/_Project/Scripts/Core/PlacementController.cs` ✅
- **Machine base classes**: `Assets/_Project/Scripts/Machines/` (🔄 In Progress)
- **Resource definitions**: `Assets/_Project/Resources/ResourceDefinitions/` ✅
- **Machine data**: `Assets/_Project/ScriptableObjects/` (Pending)
- **Recipe data**: `Assets/_Project/ScriptableObjects/` (Pending)
- **Main scene**: `Assets/_Project/Scenes/Workshop.unity`
- **Test scenes**: `Assets/_Project/Scenes/TestScenes/`
- **UI layouts**: `Assets/UI/UXML/` (Pending)
- **Research tree**: `Assets/_Project/ScriptableObjects/Research/` (Pending)

### Search Tips
- Use Unity's search bar with `t:` prefix (e.g., `t:ScriptableObject` finds all SOs)
- Search by component: `t:MonoBehaviour` finds all scripts
- Search in folder: Right-click folder → "Find References In Scene"

## Quick Reference

### Namespace Structure
```csharp
SantasWorkshop.Core          // GameManager, ResourceManager, SaveLoadSystem
SantasWorkshop.Machines      // All machine implementations
SantasWorkshop.Logistics     // Conveyors, pipes, routing
SantasWorkshop.Research      // Tech tree system
SantasWorkshop.Missions      // Campaign and objectives
SantasWorkshop.UI            // UI controllers
SantasWorkshop.Data          // Data structures and ScriptableObjects
SantasWorkshop.Utilities     // Helper classes and extensions
```

### Common File Patterns
```
MachineBase.cs               // Abstract base for all machines
ExtractorBase.cs             // Base for resource extractors
IronOre_Resource.asset       // ScriptableObject for iron ore
BasicSmelter_Recipe.asset    // Recipe for basic smelter
Workshop.unity               // Main gameplay scene
```
