# Machine Designer Guide

**Last Updated**: November 5, 2025  
**Version**: 1.0  
**Audience**: Game Designers, Content Creators

This guide explains how to create and configure machines and recipes using Unity's ScriptableObject system. No programming knowledge required!

---

## Table of Contents

1. [Introduction](#introduction)
2. [Creating Recipes](#creating-recipes)
3. [Creating Machine Data](#creating-machine-data)
4. [Machine Tiers](#machine-tiers)
5. [Balancing Guidelines](#balancing-guidelines)
6. [Testing Your Machines](#testing-your-machines)
7. [Common Mistakes](#common-mistakes)
8. [Quick Reference](#quick-reference)

---

## Introduction

Machines in Santa's Workshop Automation are configured using **ScriptableObjects** - special Unity assets that store data. This system allows you to create and balance machines without writing code.

**What You'll Create**:
- **Recipes**: Define what resources go in and what comes out
- **Machine Data**: Configure machine properties, costs, and available recipes

**Tools You'll Use**:
- Unity Editor (no coding required!)
- Project window (for creating assets)
- Inspector window (for editing properties)

---

## Creating Recipes

### Step 1: Create a New Recipe

1. In Unity, navigate to `Assets/_Project/ScriptableObjects/Recipes/`
2. Right-click in the folder
3. Select **Create → Game → Recipe**
4. Name your recipe (e.g., `IronSmelting_Recipe`)

### Step 2: Configure Basic Info

In the Inspector, fill out the **Basic Info** section:

| Field | Description | Example |
|-------|-------------|---------|
| **Recipe Id** | Unique identifier (lowercase, underscores) | `recipe_iron_smelting` |
| **Recipe Name** | Display name (user-friendly) | `Iron Smelting` |
| **Description** | What the recipe does | `Smelt iron ore into iron ingots` |
| **Icon** | Recipe icon (drag from Assets) | `IronIngot_Icon` |

### Step 3: Configure Inputs

Inputs are the resources required to craft this recipe.

1. Set **Inputs → Size** to the number of input types
2. For each input:
   - **Resource Id**: The resource identifier (e.g., `iron_ore`)
   - **Amount**: How many units required (e.g., `1`)

**Example - Iron Smelting**:
```
Inputs (Size: 1)
  Element 0:
    Resource Id: iron_ore
    Amount: 1
```

**Example - Gear Crafting**:
```
Inputs (Size: 1)
  Element 0:
    Resource Id: iron_ingot
    Amount: 2
```

**Example - Toy Assembly** (multiple inputs):
```
Inputs (Size: 4)
  Element 0:
    Resource Id: iron_gear
    Amount: 2
  Element 1:
    Resource Id: copper_wire
    Amount: 4
  Element 2:
    Resource Id: circuit
    Amount: 1
  Element 3:
    Resource Id: paint_red
    Amount: 1
```

### Step 4: Configure Outputs

Outputs are the resources produced by this recipe.

1. Set **Outputs → Size** to the number of output types
2. For each output:
   - **Resource Id**: The resource identifier (e.g., `iron_ingot`)
   - **Amount**: How many units produced (e.g., `1`)

**Example - Iron Smelting**:
```
Outputs (Size: 1)
  Element 0:
    Resource Id: iron_ingot
    Amount: 1
```

**Example - Advanced Smelting** (multiple outputs):
```
Outputs (Size: 2)
  Element 0:
    Resource Id: steel_ingot
    Amount: 1
  Element 1:
    Resource Id: slag
    Amount: 1
```

### Step 5: Configure Processing

Set how long the recipe takes and how much power it uses:

| Field | Description | Typical Values |
|-------|-------------|----------------|
| **Processing Time** | Seconds to complete | 1-10 seconds |
| **Power Consumption** | Watts consumed | 10-100 watts |
| **Required Tier** | Minimum machine tier | 1, 2, or 3 |

**Balancing Tips**:
- Simple recipes: 1-3 seconds, 10-30 watts
- Complex recipes: 5-10 seconds, 50-100 watts
- Advanced recipes: 10+ seconds, 100+ watts

**Example Values**:
```
Processing Time: 2.0 seconds
Power Consumption: 20.0 watts
Required Tier: 1
```

### Step 6: Validation

Unity will automatically validate your recipe:

- ⚠️ **Warning**: "Recipe has no inputs!" - Add at least one input
- ⚠️ **Warning**: "Recipe has no outputs!" - Add at least one output
- ⚠️ **Warning**: "Invalid processing time!" - Must be greater than 0

Fix any warnings before using the recipe!

---

## Creating Machine Data

### Step 1: Create a New Machine Data

1. Navigate to `Assets/_Project/ScriptableObjects/Machines/`
2. Right-click in the appropriate subfolder:
   - `Extractors/` for mining drills, harvesters
   - `Processors/` for smelters, sawmills
   - `Assemblers/` for toy assemblers
3. Select **Create → Game → Machine Data**
4. Name your machine (e.g., `Smelter_T1`)

### Step 2: Configure Basic Info

| Field | Description | Example |
|-------|-------------|---------|
| **Machine Name** | Display name | `Basic Smelter` |
| **Description** | What the machine does | `Smelts ore into ingots` |
| **Icon** | Machine icon | `Smelter_Icon` |
| **Grid Size** | Cells occupied (X, Y) | `(2, 2)` for 2x2 |
| **Tier** | Machine tier (1-3) | `1` |

**Grid Size Guidelines**:
- Small machines: `(1, 1)` - single cell
- Medium machines: `(2, 2)` - 4 cells
- Large machines: `(3, 3)` - 9 cells
- Special machines: `(2, 3)` or custom

### Step 3: Configure Stats

| Field | Description | Typical Values |
|-------|-------------|----------------|
| **Base Processing Speed** | Speed multiplier | `1.0` (normal speed) |
| **Base Power Consumption** | Watts consumed | `10-100` watts |
| **Input Port Count** | Number of input ports | `1-3` |
| **Output Port Count** | Number of output ports | `1-2` |
| **Buffer Capacity** | Items per port | `5-20` |

**Tier Bonuses** (automatic):
- Tier 1: 1.0x speed, 1.0x efficiency
- Tier 2: 1.2x speed, 0.9x efficiency (10% less power)
- Tier 3: 1.5x speed, 0.8x efficiency (20% less power)

**Example - Tier 1 Smelter**:
```
Base Processing Speed: 1.0
Base Power Consumption: 20.0
Input Port Count: 1
Output Port Count: 1
Buffer Capacity: 10
```

**Example - Tier 2 Smelter**:
```
Base Processing Speed: 1.0 (becomes 1.2x with tier bonus)
Base Power Consumption: 20.0 (becomes 18.0 with tier bonus)
Input Port Count: 2
Output Port Count: 1
Buffer Capacity: 15
```

### Step 4: Assign Recipes

1. Set **Available Recipes → Size** to the number of recipes
2. Drag recipe assets from the Project window into the array

**Example - Basic Smelter**:
```
Available Recipes (Size: 2)
  Element 0: IronSmelting_Recipe
  Element 1: CopperSmelting_Recipe
```

**Example - Advanced Smelter**:
```
Available Recipes (Size: 4)
  Element 0: IronSmelting_Recipe
  Element 1: CopperSmelting_Recipe
  Element 2: SteelSmelting_Recipe
  Element 3: AlloyProduction_Recipe
```

### Step 5: Assign Prefab

1. Create or locate the machine prefab in `Assets/_Project/Prefabs/Machines/`
2. Drag the prefab into the **Prefab** field

**Prefab Requirements**:
- Must have the appropriate machine script (e.g., `Smelter.cs`)
- Should have visual model, colliders, and effects
- Should be configured for the correct grid size

### Step 6: Port Configuration (Advanced)

Port positions are automatically initialized, but you can customize them:

**Input Port Positions**:
- Array size matches `Input Port Count`
- Each element is a Vector3 (local position)
- Example: `(-1, 0.5, 0)` for left side

**Output Port Positions**:
- Array size matches `Output Port Count`
- Each element is a Vector3 (local position)
- Example: `(1, 0.5, 0)` for right side

**Typical Port Layouts**:

**1x1 Machine**:
```
Input Port 0: (-0.5, 0.5, 0)  [Left]
Output Port 0: (0.5, 0.5, 0)  [Right]
```

**2x2 Machine**:
```
Input Port 0: (-1, 0.5, 0)    [Left]
Output Port 0: (1, 0.5, 0)    [Right]
```

**2x2 Machine with Multiple Ports**:
```
Input Port 0: (-1, 0.5, -0.5) [Left-Front]
Input Port 1: (-1, 0.5, 0.5)  [Left-Back]
Output Port 0: (1, 0.5, 0)    [Right]
```

---

## Machine Tiers

### Tier 1: Basic Machines

**Characteristics**:
- Slow processing speed (1.0x)
- Standard power consumption (1.0x)
- Limited recipes (1-2)
- Small buffers (5-10 capacity)
- Cheap to build

**Examples**:
- Basic Smelter
- Simple Assembler
- Mining Drill Mk1

**Balancing**:
```
Processing Speed: 1.0x
Power Consumption: 20-40 watts
Buffer Capacity: 5-10
Available Recipes: 1-2
```

### Tier 2: Improved Machines

**Characteristics**:
- Faster processing (1.2x)
- More efficient (0.9x power)
- More recipes (2-4)
- Larger buffers (10-15 capacity)
- Moderate cost

**Examples**:
- Advanced Smelter
- Fast Assembler
- Mining Drill Mk2

**Balancing**:
```
Processing Speed: 1.2x (automatic)
Power Consumption: 30-60 watts (becomes 27-54 with efficiency)
Buffer Capacity: 10-15
Available Recipes: 2-4
```

### Tier 3: Advanced Machines

**Characteristics**:
- Very fast processing (1.5x)
- Highly efficient (0.8x power)
- Many recipes (4-6)
- Large buffers (15-20 capacity)
- Expensive to build

**Examples**:
- Industrial Smelter
- Automated Assembler
- Mining Drill Mk3

**Balancing**:
```
Processing Speed: 1.5x (automatic)
Power Consumption: 50-100 watts (becomes 40-80 with efficiency)
Buffer Capacity: 15-20
Available Recipes: 4-6
```

---

## Balancing Guidelines

### Recipe Complexity

**Simple Recipes** (1 input → 1 output):
```
Processing Time: 1-3 seconds
Power Consumption: 10-30 watts
Example: Iron Ore → Iron Ingot
```

**Medium Recipes** (2-3 inputs → 1-2 outputs):
```
Processing Time: 3-7 seconds
Power Consumption: 30-60 watts
Example: Iron Ingot + Coal → Steel Ingot
```

**Complex Recipes** (4+ inputs → 1-2 outputs):
```
Processing Time: 7-15 seconds
Power Consumption: 60-100 watts
Example: Multiple components → Finished Toy
```

### Resource Flow

**Early Game** (Tier 1):
- 1 resource per 2-3 seconds
- ~20-30 resources per minute
- Low power consumption (10-30 watts)

**Mid Game** (Tier 2):
- 1 resource per 1-2 seconds
- ~30-60 resources per minute
- Moderate power (30-60 watts)

**Late Game** (Tier 3):
- 1 resource per 0.5-1 second
- ~60-120 resources per minute
- High power (60-100 watts)

### Power Budget

**Total Power Guidelines**:
- Early game: 100-500 watts total
- Mid game: 500-2000 watts total
- Late game: 2000-10000 watts total

**Per Machine**:
- Extractors: 10-30 watts
- Processors: 20-60 watts
- Assemblers: 40-100 watts
- Utilities: 5-20 watts

---

## Testing Your Machines

### Step 1: Create Test Scene

1. Open `Assets/_Project/Scenes/TestScenes/MachineFrameworkTest.unity`
2. Or create a new test scene

### Step 2: Place Machine

1. Drag machine prefab into scene
2. Position at grid-aligned location
3. Ensure GridManager is in scene

### Step 3: Configure Machine

1. Select machine in Hierarchy
2. In Inspector, assign **Machine Data** to your new asset
3. Enable **Show Debug Info** for detailed logging

### Step 4: Test Functionality

**Test Checklist**:
- ✅ Machine initializes without errors
- ✅ Machine transitions to correct states
- ✅ Recipe processing works correctly
- ✅ Input/output buffers function
- ✅ Power consumption is correct
- ✅ Visual feedback works (animations, effects)
- ✅ Save/load preserves state

**Console Output**:
```
[MachineBase] machine_001 initialized
[MachineBase] machine_001 state changed: Idle → Working
[Smelter] machine_001 completed processing: recipe_iron_smelting
```

### Step 5: Balance Testing

**Questions to Ask**:
1. Does the processing time feel right?
2. Is the power consumption balanced?
3. Are the buffer sizes appropriate?
4. Does the machine fit the intended tier?
5. Is the recipe complexity appropriate?

**Adjust Values**:
- Too slow? Reduce processing time or increase speed multiplier
- Too fast? Increase processing time
- Too powerful? Increase power consumption
- Too weak? Decrease power consumption

---

## Common Mistakes

### Mistake 1: Empty Inputs/Outputs

**Problem**: Recipe has no inputs or outputs

**Solution**: Always add at least one input and one output

```
❌ BAD:
Inputs (Size: 0)
Outputs (Size: 0)

✅ GOOD:
Inputs (Size: 1)
  Element 0: iron_ore, Amount: 1
Outputs (Size: 1)
  Element 0: iron_ingot, Amount: 1
```

### Mistake 2: Invalid Processing Time

**Problem**: Processing time is 0 or negative

**Solution**: Set processing time to a positive value (typically 1-10 seconds)

```
❌ BAD:
Processing Time: 0

✅ GOOD:
Processing Time: 2.0
```

### Mistake 3: Mismatched Recipe Tier

**Problem**: Recipe requires Tier 2, but assigned to Tier 1 machine

**Solution**: Ensure recipe tier ≤ machine tier

```
❌ BAD:
Machine Tier: 1
Recipe Required Tier: 2

✅ GOOD:
Machine Tier: 2
Recipe Required Tier: 2
```

### Mistake 4: No Recipes Assigned

**Problem**: Machine has no available recipes

**Solution**: Assign at least one recipe to the machine

```
❌ BAD:
Available Recipes (Size: 0)

✅ GOOD:
Available Recipes (Size: 1)
  Element 0: IronSmelting_Recipe
```

### Mistake 5: Wrong Grid Size

**Problem**: Machine prefab doesn't match grid size in data

**Solution**: Ensure prefab visual size matches grid size

```
❌ BAD:
Grid Size: (2, 2)
Prefab: 1x1 cube

✅ GOOD:
Grid Size: (2, 2)
Prefab: 2x2 model
```

### Mistake 6: Missing Prefab

**Problem**: Machine Data has no prefab assigned

**Solution**: Always assign a prefab reference

```
❌ BAD:
Prefab: None

✅ GOOD:
Prefab: Smelter_T1_Prefab
```

---

## Quick Reference

### Recipe Template

```
Recipe Id: recipe_[name]
Recipe Name: [Display Name]
Description: [What it does]

Inputs:
  - Resource Id: [resource_id]
    Amount: [number]

Outputs:
  - Resource Id: [resource_id]
    Amount: [number]

Processing Time: [1-10] seconds
Power Consumption: [10-100] watts
Required Tier: [1-3]
```

### Machine Data Template

```
Machine Name: [Display Name]
Description: [What it does]
Grid Size: ([1-3], [1-3])
Tier: [1-3]

Base Processing Speed: 1.0
Base Power Consumption: [10-100]
Input Port Count: [1-3]
Output Port Count: [1-2]
Buffer Capacity: [5-20]

Available Recipes:
  - [Recipe 1]
  - [Recipe 2]

Prefab: [Machine Prefab]
```

### Typical Values by Tier

| Property | Tier 1 | Tier 2 | Tier 3 |
|----------|--------|--------|--------|
| Speed Multiplier | 1.0x | 1.2x | 1.5x |
| Efficiency | 1.0x | 0.9x | 0.8x |
| Power (Extractor) | 10-20W | 15-30W | 20-40W |
| Power (Processor) | 20-40W | 30-60W | 40-80W |
| Power (Assembler) | 40-80W | 60-100W | 80-120W |
| Buffer Capacity | 5-10 | 10-15 | 15-20 |
| Recipe Count | 1-2 | 2-4 | 4-6 |

### Resource Flow Rates

| Tier | Items/Minute | Processing Time | Power Budget |
|------|--------------|-----------------|--------------|
| 1 | 20-30 | 2-3s | 100-500W |
| 2 | 30-60 | 1-2s | 500-2000W |
| 3 | 60-120 | 0.5-1s | 2000-10000W |

---

## Summary

Creating machines and recipes is straightforward:

1. **Create Recipe**: Define inputs, outputs, time, and power
2. **Create Machine Data**: Configure stats, ports, and recipes
3. **Assign Prefab**: Link to visual representation
4. **Test**: Verify functionality and balance
5. **Iterate**: Adjust values based on testing

**Remember**:
- Always validate recipes (no empty inputs/outputs)
- Match recipe tier to machine tier
- Balance processing time and power consumption
- Test in-game before finalizing
- Use tier bonuses to your advantage

**Need Help?**  
Check the example recipes and machines in `Assets/_Project/ScriptableObjects/` for reference!

---

**Happy Designing!** 🎄
