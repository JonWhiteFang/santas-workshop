# Resource Definitions

This folder contains ScriptableObject assets that define all resources in Santa's Workshop Automation.

## Created Sample Resources

### Raw Materials (Category: RawMaterial)
1. **Wood** (`wood`)
   - Stack Size: 200
   - Weight: 2 kg
   - Base Value: 5
   - Description: Raw wood harvested from trees

2. **Iron Ore** (`iron_ore`)
   - Stack Size: 100
   - Weight: 5 kg
   - Base Value: 8
   - Description: Raw iron ore extracted from mines

3. **Coal** (`coal`)
   - Stack Size: 150
   - Weight: 3 kg
   - Base Value: 6
   - Description: Fossil fuel for power generation

### Refined Goods (Category: Refined)
4. **Wood Plank** (`wood_plank`)
   - Stack Size: 150
   - Weight: 1.5 kg
   - Base Value: 12
   - Description: Processed wood planks from sawmill

5. **Iron Ingot** (`iron_ingot`)
   - Stack Size: 100
   - Weight: 4 kg
   - Base Value: 20
   - Description: Refined iron from smelter

### Components (Category: Component)
6. **Iron Gear** (`iron_gear`)
   - Stack Size: 50
   - Weight: 0.5 kg
   - Base Value: 35
   - Description: Precision-crafted gear for mechanical toys

7. **Paint** (`paint`)
   - Stack Size: 100
   - Weight: 0.3 kg
   - Base Value: 15
   - Description: Colorful paint for toy decoration

### Finished Toys (Category: FinishedToy)
8. **Wooden Train** (`wooden_train`)
   - Stack Size: 20
   - Weight: 2 kg
   - Base Value: 100
   - Description: Classic wooden toy train

## Resource Flow Example

```
Wood → [Sawmill] → Wood Plank → [Toy Assembler] → Wooden Train
Iron Ore → [Smelter] → Iron Ingot → [Assembler] → Iron Gear → [Toy Assembler] → Mechanical Toys
```

## Notes

- All resources have `icon` and `itemPrefab` set to null (will be added in art phase)
- All resources are configured with appropriate `itemColor` for visual identification
- All resources can be stored and transported (default behavior)
- Resource IDs use snake_case format for consistency
