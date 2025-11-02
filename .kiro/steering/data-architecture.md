# Data Architecture - Santa's Workshop Automation

**Last Updated**: November 2, 2025  
**Purpose**: Data structures, ScriptableObject schemas, and serialization patterns

This document defines the data architecture for Santa's Workshop Automation, including ScriptableObject schemas, save file formats, and data relationships between game systems.

---

## Core Data Principles

1. **Data-Driven Design**: Game balance and content defined in ScriptableObjects, not code
2. **Separation of Data and Logic**: Data structures are pure data, logic lives in systems
3. **Versioned Schemas**: All serialized data includes version numbers for migration
4. **Immutable Configuration**: ScriptableObjects are read-only at runtime
5. **Efficient Serialization**: Minimize save file size while maintaining readability

---

## ScriptableObject Schemas

### 1. Resource Data

Defines all resources in the game (raw materials, refined goods, components, toys).

```csharp
[CreateAssetMenu(fileName = "NewResource", menuName = "Game/Resource")]
public class ResourceData : ScriptableObject
{
    [Header("Basic Info")]
    public string resourceName;
    public string description;
    public Sprite icon;
    public ResourceCategory category;

    [Header("Properties")]
    public ResourceType type;
    public int stackSize = 100;
    public float weight = 1f; // For logistics calculations

    [Header("Visual")]
    public GameObject itemPrefab; // 3D model for conveyor items
    public Color itemColor = Color.white;

    [Header("Economy")]
    public int baseValue = 10; // For future trading/economy
    public bool canBeStored = true;
    public bool canBeTransported = true;
}

public enum ResourceCategory
{
    RawMaterial,    // Iron Ore, Wood, Coal
    RefinedGood,    // Iron Ingot, Planks, Steel
    Component,      // Gears, Circuits, Paint
    ToyPart,        // Wheels, Arms, Bodies
    FinishedToy,    // Complete toys
    Magic,          // Magic Dust, Enchantments
    Energy          // Coal, Magic Crystals (for power)
}

public enum ResourceType
{
    // Raw Materials
    IronOre,
    CopperOre,
    Wood,
    Coal,
    Sand,
    Clay,
    
    // Refined Goods
    IronIngot,
    CopperIngot,
    SteelIngot,
    Planks,
    Glass,
    Bricks,
    
    // Components
    IronGear,
    CopperWire,
    Circuit,
    Paint_Red,
    Paint_Blue,
    Paint_Green,
    
    // Toy Parts
    WoodenWheel,
    PlasticBody,
    MetalArm,
    
    // Finished Toys
    WoodenTrain,
    TeddyBear,
    RobotToy,
    DollHouse,
    
    // Magic
    MagicDust,
    EnchantedCrystal,
    
    // Energy
    CoalFuel,
    MagicCrystalFuel
}
```

**Resource Relationships**:
```
IronOre → [Smelter] → IronIngot → [Assembler] → IronGear → [ToyAssembler] → RobotToy
Wood → [Sawmill] → Planks → [Assembler] → WoodenWheel → [ToyAssembler] → WoodenTrain
```

---

### 2. Recipe Data

Defines input/output relationships for machines.

```csharp
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Game/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Basic Info")]
    public string recipeName;
    public string description;
    public Sprite icon;

    [Header("Inputs")]
    public ResourceAmount[] inputs;

    [Header("Outputs")]
    public ResourceAmount[] outputs;

    [Header("Processing")]
    public float processingTime = 1f; // Seconds
    public float powerConsumption = 10f; // Watts

    [Header("Requirements")]
    public MachineType requiredMachineType;
    public int requiredMachineTier = 1;
    public ResearchNode requiredResearch; // Optional unlock requirement

    [Header("Quality Modifiers")]
    public float qualityMultiplier = 1f; // Affects toy quality
    public bool allowsMagicInfusion = false;

    // Validation
    private void OnValidate()
    {
        if (inputs.Length == 0)
            Debug.LogWarning($"Recipe {recipeName} has no inputs!");
        
        if (outputs.Length == 0)
            Debug.LogWarning($"Recipe {recipeName} has no outputs!");
    }
}

[System.Serializable]
public struct ResourceAmount
{
    public ResourceData resource;
    public int amount;
}
```

**Recipe Examples**:

```csharp
// Iron Smelting Recipe
inputs: [IronOre x1]
outputs: [IronIngot x1]
processingTime: 2s
powerConsumption: 20W
requiredMachineType: Smelter
requiredMachineTier: 1

// Gear Crafting Recipe
inputs: [IronIngot x2]
outputs: [IronGear x1]
processingTime: 3s
powerConsumption: 30W
requiredMachineType: Assembler
requiredMachineTier: 1

// Robot Toy Assembly Recipe
inputs: [MetalArm x2, Circuit x1, IronGear x4, Paint_Red x1]
outputs: [RobotToy x1]
processingTime: 10s
powerConsumption: 80W
requiredMachineType: ToyAssembler
requiredMachineTier: 2
allowsMagicInfusion: true
```

---

### 3. Machine Data

Defines all machine types and their properties.

```csharp
[CreateAssetMenu(fileName = "NewMachine", menuName = "Game/Machine")]
public class MachineData : ScriptableObject
{
    [Header("Basic Info")]
    public string machineName;
    public string description;
    public Sprite icon;
    public MachineType type;
    public int tier = 1;

    [Header("Visual")]
    public GameObject prefab;
    public Vector2Int gridSize = new Vector2Int(1, 1); // Cells occupied

    [Header("Stats")]
    public float productionSpeed = 1f; // Multiplier
    public float powerConsumption = 10f; // Watts
    public int inputSlots = 1;
    public int outputSlots = 1;

    [Header("Recipes")]
    public RecipeData[] availableRecipes;

    [Header("Construction")]
    public ResourceAmount[] buildCost;
    public float buildTime = 5f; // Seconds

    [Header("Requirements")]
    public ResearchNode requiredResearch;
    public MachineData[] requiredMachines; // Prerequisites

    [Header("Upgrades")]
    public MachineData nextTier; // Upgrade path
    public ResourceAmount[] upgradeCost;

    // Helper methods
    public bool CanCraftRecipe(RecipeData recipe)
    {
        return availableRecipes.Contains(recipe) && recipe.requiredMachineTier <= tier;
    }
}

public enum MachineType
{
    // Extractors
    MiningDrill,
    WoodHarvester,
    
    // Processors
    Smelter,
    Sawmill,
    Refinery,
    
    // Assemblers
    BasicAssembler,
    AdvancedAssembler,
    ToyAssembler,
    
    // Utility
    StorageChest,
    PowerGenerator,
    MagicInfuser,
    
    // Logistics
    ConveyorBelt,
    Splitter,
    Merger,
    UndergroundBelt
}
```

**Machine Tier Progression**:
```
Tier 1: Basic machines (slow, low power)
Tier 2: Improved machines (faster, moderate power)
Tier 3: Advanced machines (fast, high power, multi-recipe)
Tier 4: Magical machines (very fast, magic-powered, quality bonuses)
```

---

### 4. Research Node Data

Defines the tech tree structure.

```csharp
[CreateAssetMenu(fileName = "NewResearchNode", menuName = "Game/Research Node")]
public class ResearchNodeData : ScriptableObject
{
    [Header("Basic Info")]
    public string nodeName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public ResearchBranch branch;

    [Header("Requirements")]
    public ResearchNodeData[] prerequisites;
    public ResourceAmount[] researchCost;
    public float researchTime = 30f; // Seconds

    [Header("Unlocks")]
    public MachineData[] unlockedMachines;
    public RecipeData[] unlockedRecipes;
    public ResearchNodeData[] unlockedNodes;

    [Header("Bonuses")]
    public ResearchBonus[] bonuses;

    [Header("Visual")]
    public Vector2 treePosition; // Position in tech tree UI
    public Color nodeColor = Color.white;

    // Helper methods
    public bool CanResearch(HashSet<ResearchNodeData> completedResearch)
    {
        foreach (var prereq in prerequisites)
        {
            if (!completedResearch.Contains(prereq))
                return false;
        }
        return true;
    }
}

public enum ResearchBranch
{
    Automation,      // Machine speed, efficiency
    Energy,          // Power generation, distribution
    Materials,       // Resource processing, refinement
    Toys,            // Toy types, quality
    Logistics,       // Conveyors, storage, routing
    ElfManagement,   // Elf helpers, automation
    Magic,           // Enchantments, magic systems
    Aesthetics       // Decorations, workshop appearance
}

[System.Serializable]
public struct ResearchBonus
{
    public BonusType type;
    public float value;
}

public enum BonusType
{
    ProductionSpeed,     // +10% production speed
    PowerEfficiency,     // -10% power consumption
    ResourceYield,       // +20% resource output
    ToyQuality,          // +5% toy quality
    MagicPotency,        // +15% magic effectiveness
    StorageCapacity,     // +50% storage capacity
    ConveyorSpeed        // +25% conveyor speed
}
```

**Research Tree Structure**:
```
Automation Branch:
├── Basic Automation (Tier 1 machines)
├── Advanced Automation (Tier 2 machines)
│   └── Industrial Automation (Tier 3 machines)
└── Production Optimization (+10% speed)

Energy Branch:
├── Coal Power (Coal generators)
├── Advanced Power (Efficient generators)
│   └── Magic Power (Magic crystal generators)
└── Power Distribution (Power poles, substations)

Materials Branch:
├── Basic Smelting (Iron, Copper)
├── Advanced Smelting (Steel, Alloys)
│   └── Exotic Materials (Rare resources)
└── Resource Efficiency (+20% yield)

Toys Branch:
├── Wooden Toys (Trains, blocks)
├── Mechanical Toys (Robots, cars)
│   └── Electronic Toys (RC toys, gadgets)
└── Toy Quality (+5% quality)

Logistics Branch:
├── Basic Conveyors (Tier 1 belts)
├── Fast Conveyors (Tier 2 belts)
│   └── Express Conveyors (Tier 3 belts)
└── Smart Routing (Splitters, mergers, filters)

Magic Branch:
├── Basic Enchantments (Simple magic)
├── Advanced Enchantments (Powerful magic)
│   └── Master Enchantments (Legendary magic)
└── Magic Efficiency (+15% potency)
```

---

### 5. Mission Data

Defines campaign missions and objectives.

```csharp
[CreateAssetMenu(fileName = "NewMission", menuName = "Game/Mission")]
public class MissionData : ScriptableObject
{
    [Header("Basic Info")]
    public string missionName;
    [TextArea(3, 10)]
    public string description;
    public Sprite icon;
    public int missionOrder; // Sequence in campaign

    [Header("Objectives")]
    public ObjectiveData[] objectives;

    [Header("Requirements")]
    public MissionData[] prerequisiteMissions;
    public ResearchNodeData[] requiredResearch;

    [Header("Rewards")]
    public ResourceAmount[] resourceRewards;
    public ResearchNodeData[] unlockedResearch;
    public int christmasSpiritReward = 100;

    [Header("Time Limit")]
    public bool hasTimeLimit = false;
    public float timeLimitSeconds = 600f; // 10 minutes

    [Header("Dialogue")]
    public DialogueData introDialogue;
    public DialogueData completionDialogue;

    // Helper methods
    public bool IsComplete(MissionProgress progress)
    {
        foreach (var objective in objectives)
        {
            if (!progress.IsObjectiveComplete(objective))
                return false;
        }
        return true;
    }
}

[System.Serializable]
public class ObjectiveData
{
    public string objectiveName;
    public ObjectiveType type;
    public int targetValue;
    public ResourceData targetResource; // For production objectives
    public MachineData targetMachine; // For building objectives
    public bool isOptional = false;
}

public enum ObjectiveType
{
    ProduceResource,     // Produce X units of resource
    BuildMachine,        // Build X machines of type
    ResearchTech,        // Complete research node
    ReachQuality,        // Produce toy with quality grade
    GeneratePower,       // Generate X watts of power
    StoreResources,      // Store X resources
    CompleteOrder        // Fulfill toy order
}
```

**Mission Examples**:

```csharp
// Mission 1: First Steps
objectives:
  - Build 1 Mining Drill
  - Produce 10 Iron Ore
  - Build 1 Smelter
  - Produce 5 Iron Ingots
rewards:
  - 50 Wood
  - 20 Coal
  - Unlock: Basic Automation research

// Mission 5: Toy Production
objectives:
  - Build 1 Toy Assembler
  - Produce 10 Wooden Trains (Grade B or higher)
  - Research: Wooden Toys
rewards:
  - 100 Christmas Spirit
  - Unlock: Mechanical Toys research
timeLimit: 15 minutes

// Mission 10: Christmas Rush
objectives:
  - Produce 100 toys (any type, Grade A or higher)
  - Maintain 500W power generation
  - Complete 5 toy orders
rewards:
  - 500 Christmas Spirit
  - Unlock: Prestige System
timeLimit: 30 minutes
```

---

### 6. Toy Data

Defines finished toy products with quality grading.

```csharp
[CreateAssetMenu(fileName = "NewToy", menuName = "Game/Toy")]
public class ToyData : ScriptableObject
{
    [Header("Basic Info")]
    public string toyName;
    public string description;
    public Sprite icon;
    public ToyCategory category;

    [Header("Visual")]
    public GameObject prefab;
    public AnimationClip idleAnimation;

    [Header("Production")]
    public RecipeData recipe;
    public float baseProductionTime = 10f;

    [Header("Quality Grading")]
    public QualityThresholds qualityThresholds;

    [Header("Value")]
    public int baseChristmasSpirit = 10;
    public Dictionary<ToyGrade, float> gradeMultipliers = new()
    {
        { ToyGrade.C, 0.5f },
        { ToyGrade.B, 1.0f },
        { ToyGrade.A, 1.5f },
        { ToyGrade.S, 2.0f }
    };

    [Header("Magic")]
    public bool canBeEnchanted = true;
    public MagicEnchantment[] availableEnchantments;

    // Calculate toy grade based on production metrics
    public ToyGrade CalculateGrade(float productionSpeed, float powerEfficiency, float magicLevel)
    {
        float score = 0f;
        
        // Speed component (40%)
        score += Mathf.Clamp01(productionSpeed / qualityThresholds.speedTarget) * 0.4f;
        
        // Efficiency component (30%)
        score += Mathf.Clamp01(powerEfficiency / qualityThresholds.efficiencyTarget) * 0.3f;
        
        // Magic component (30%)
        score += Mathf.Clamp01(magicLevel / qualityThresholds.magicTarget) * 0.3f;

        if (score >= qualityThresholds.sGradeThreshold) return ToyGrade.S;
        if (score >= qualityThresholds.aGradeThreshold) return ToyGrade.A;
        if (score >= qualityThresholds.bGradeThreshold) return ToyGrade.B;
        return ToyGrade.C;
    }
}

public enum ToyCategory
{
    Wooden,      // Trains, blocks, puzzles
    Mechanical,  // Robots, cars, wind-up toys
    Electronic,  // RC toys, gadgets, lights
    Plush,       // Teddy bears, dolls
    Educational, // Science kits, building sets
    Magical      // Enchanted toys, special items
}

public enum ToyGrade
{
    C,  // Basic quality
    B,  // Good quality
    A,  // Excellent quality
    S   // Perfect quality
}

[System.Serializable]
public struct QualityThresholds
{
    public float speedTarget;       // Target production speed
    public float efficiencyTarget;  // Target power efficiency
    public float magicTarget;       // Target magic level
    
    public float sGradeThreshold;   // 0.9+ = S grade
    public float aGradeThreshold;   // 0.75+ = A grade
    public float bGradeThreshold;   // 0.5+ = B grade
}

[System.Serializable]
public struct MagicEnchantment
{
    public string enchantmentName;
    public ResourceAmount[] cost;
    public float qualityBonus;
    public float christmasSpiritBonus;
}
```

**Toy Quality Examples**:
```
Wooden Train (Grade C):
- Production: 12s (slow)
- Power: 100W (inefficient)
- Magic: 0% (none)
- Christmas Spirit: 5

Wooden Train (Grade S):
- Production: 8s (fast)
- Power: 60W (efficient)
- Magic: 50% (enchanted)
- Christmas Spirit: 20
```

---

## Save File Format

### Save Data Structure

```csharp
[System.Serializable]
public class SaveData
{
    // Metadata
    public int version = 1;
    public string saveName;
    public DateTime saveTime;
    public float playTime; // Total seconds played

    // Game State
    public ResourceSaveData resources;
    public MachineSaveData machines;
    public LogisticsSaveData logistics;
    public PowerGridSaveData powerGrid;
    public ResearchSaveData research;
    public MissionSaveData missions;
    public PrestigeSaveData prestige;

    // World State
    public int currentDay; // 1-365 (in-game year)
    public SeasonalPhase currentPhase;
    public float christmasSpirit;
}

[System.Serializable]
public class ResourceSaveData
{
    public ResourceEntry[] resources;
}

[System.Serializable]
public struct ResourceEntry
{
    public string resourceId; // ResourceType as string
    public int amount;
    public int capacity;
}

[System.Serializable]
public class MachineSaveData
{
    public MachineEntry[] machines;
}

[System.Serializable]
public struct MachineEntry
{
    public string machineId; // GUID
    public string machineType;
    public int tier;
    public Vector3Int gridPosition;
    public string currentRecipeId;
    public float productionProgress;
    public bool isActive;
    public ResourceEntry[] inputBuffer;
    public ResourceEntry[] outputBuffer;
}

[System.Serializable]
public class LogisticsSaveData
{
    public ConveyorEntry[] conveyors;
    public ItemEntry[] items;
}

[System.Serializable]
public struct ConveyorEntry
{
    public string conveyorId;
    public Vector3Int gridPosition;
    public ConveyorDirection direction;
    public int tier;
}

[System.Serializable]
public struct ItemEntry
{
    public string resourceId;
    public string conveyorId;
    public float progress; // 0-1 along conveyor
}

[System.Serializable]
public class PowerGridSaveData
{
    public PowerGeneratorEntry[] generators;
    public PowerConsumerEntry[] consumers;
    public float totalGeneration;
    public float totalConsumption;
}

[System.Serializable]
public struct PowerGeneratorEntry
{
    public string generatorId;
    public Vector3Int gridPosition;
    public float outputWatts;
    public bool isActive;
}

[System.Serializable]
public struct PowerConsumerEntry
{
    public string consumerId;
    public float consumptionWatts;
    public bool isPowered;
}

[System.Serializable]
public class ResearchSaveData
{
    public string[] completedResearch; // ResearchNode IDs
    public string currentResearch;
    public float researchProgress;
}

[System.Serializable]
public class MissionSaveData
{
    public string[] completedMissions;
    public string currentMission;
    public ObjectiveProgress[] objectiveProgress;
}

[System.Serializable]
public struct ObjectiveProgress
{
    public string objectiveId;
    public int currentValue;
    public bool isComplete;
}

[System.Serializable]
public class PrestigeSaveData
{
    public int prestigeLevel;
    public int totalChristmasSpirit;
    public string[] permanentUnlocks;
    public Dictionary<string, float> prestigeBonuses;
}
```

### Save File Example (JSON)

```json
{
  "version": 1,
  "saveName": "My Workshop",
  "saveTime": "2025-11-02T14:30:00Z",
  "playTime": 3600.5,
  "resources": {
    "resources": [
      { "resourceId": "IronOre", "amount": 150, "capacity": 500 },
      { "resourceId": "IronIngot", "amount": 75, "capacity": 500 },
      { "resourceId": "Wood", "amount": 200, "capacity": 1000 }
    ]
  },
  "machines": {
    "machines": [
      {
        "machineId": "abc123",
        "machineType": "MiningDrill",
        "tier": 1,
        "gridPosition": { "x": 5, "y": 0, "z": 10 },
        "currentRecipeId": "ExtractIronOre",
        "productionProgress": 0.5,
        "isActive": true,
        "inputBuffer": [],
        "outputBuffer": [
          { "resourceId": "IronOre", "amount": 2, "capacity": 10 }
        ]
      }
    ]
  },
  "currentDay": 45,
  "currentPhase": "Production",
  "christmasSpirit": 250.0
}
```

---

## Data Relationships

### Resource Flow Diagram

```
[Mining Drill] → IronOre → [Conveyor] → [Smelter] → IronIngot → [Conveyor] → [Assembler] → IronGear → [Conveyor] → [Toy Assembler] → RobotToy
                                            ↑
                                         [Coal]
                                            ↑
                                    [Power Generator]
```

### Research Dependencies

```
Basic Automation
├── Advanced Automation
│   ├── Industrial Automation
│   └── Production Optimization
└── Basic Smelting
    └── Advanced Smelting
        └── Exotic Materials
```

### Machine Upgrade Paths

```
Mining Drill Tier 1 → Mining Drill Tier 2 → Mining Drill Tier 3
Smelter Tier 1 → Smelter Tier 2 → Smelter Tier 3
Conveyor Tier 1 → Conveyor Tier 2 → Conveyor Tier 3
```

---

## Database Organization

### ScriptableObject File Structure

```
Assets/_Project/ScriptableObjects/
├── Resources/
│   ├── RawMaterials/
│   │   ├── IronOre.asset
│   │   ├── CopperOre.asset
│   │   └── Wood.asset
│   ├── RefinedGoods/
│   │   ├── IronIngot.asset
│   │   ├── SteelIngot.asset
│   │   └── Planks.asset
│   └── Components/
│       ├── IronGear.asset
│       └── Circuit.asset
├── Recipes/
│   ├── Smelting/
│   │   ├── IronSmelting.asset
│   │   └── SteelSmelting.asset
│   ├── Assembly/
│   │   ├── GearCrafting.asset
│   │   └── CircuitCrafting.asset
│   └── ToyProduction/
│       ├── WoodenTrainAssembly.asset
│       └── RobotToyAssembly.asset
├── Machines/
│   ├── Extractors/
│   │   ├── MiningDrill_T1.asset
│   │   ├── MiningDrill_T2.asset
│   │   └── WoodHarvester_T1.asset
│   ├── Processors/
│   │   ├── Smelter_T1.asset
│   │   └── Sawmill_T1.asset
│   └── Assemblers/
│       ├── BasicAssembler_T1.asset
│       └── ToyAssembler_T1.asset
├── Research/
│   ├── Automation/
│   │   ├── BasicAutomation.asset
│   │   └── AdvancedAutomation.asset
│   ├── Energy/
│   │   ├── CoalPower.asset
│   │   └── MagicPower.asset
│   └── Materials/
│       ├── BasicSmelting.asset
│       └── AdvancedSmelting.asset
├── Missions/
│   ├── Tutorial/
│   │   ├── Mission01_FirstSteps.asset
│   │   └── Mission02_PowerUp.asset
│   └── Campaign/
│       ├── Mission05_ToyProduction.asset
│       └── Mission10_ChristmasRush.asset
└── Toys/
    ├── Wooden/
    │   ├── WoodenTrain.asset
    │   └── WoodenBlocks.asset
    └── Mechanical/
        ├── RobotToy.asset
        └── WindUpCar.asset
```

---

## Data Validation

### Editor Validation Scripts

```csharp
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(RecipeData))]
public class RecipeDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RecipeData recipe = (RecipeData)target;

        // Validate inputs
        if (recipe.inputs.Length == 0)
        {
            EditorGUILayout.HelpBox("Recipe has no inputs!", MessageType.Error);
        }

        // Validate outputs
        if (recipe.outputs.Length == 0)
        {
            EditorGUILayout.HelpBox("Recipe has no outputs!", MessageType.Error);
        }

        // Validate processing time
        if (recipe.processingTime <= 0)
        {
            EditorGUILayout.HelpBox("Processing time must be positive!", MessageType.Error);
        }

        // Validate power consumption
        if (recipe.powerConsumption < 0)
        {
            EditorGUILayout.HelpBox("Power consumption cannot be negative!", MessageType.Warning);
        }
    }
}
#endif
```

---

## Performance Considerations

### Data Loading Strategy

1. **Startup**: Load all ScriptableObjects into memory (small footprint)
2. **Runtime**: Reference ScriptableObjects by ID, not direct reference
3. **Save/Load**: Serialize IDs, resolve references on load
4. **Pooling**: Reuse data structures for frequently created/destroyed objects

### Memory Optimization

```csharp
// ❌ BAD: Storing full ScriptableObject references in save data
[System.Serializable]
public class BadMachineEntry
{
    public MachineData machineData; // Don't serialize ScriptableObjects!
}

// ✅ GOOD: Store IDs, resolve on load
[System.Serializable]
public class GoodMachineEntry
{
    public string machineTypeId; // "MiningDrill_T1"
    
    public MachineData ResolveMachineData()
    {
        return MachineDatabase.Instance.GetMachineById(machineTypeId);
    }
}
```

---

## Summary

This data architecture provides:

1. **Clear Schemas**: Well-defined ScriptableObject structures
2. **Flexible Recipes**: Data-driven production chains
3. **Versioned Saves**: Forward-compatible save files
4. **Efficient Serialization**: Minimal save file size
5. **Editor Validation**: Catch errors early
6. **Performance**: Optimized data loading and referencing

Follow these patterns for consistent, maintainable data management throughout the project.
