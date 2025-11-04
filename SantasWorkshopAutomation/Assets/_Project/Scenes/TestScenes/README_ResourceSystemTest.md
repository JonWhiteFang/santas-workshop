# Resource System Test Scene Setup

This document provides instructions for setting up the Resource System test scene.

## Files Created

1. **TestScene_ResourceSystem.unity** - The test scene
2. **ResourceSystemTester.cs** - Test script with UI controls

## Manual Setup Instructions

Since Unity scene files are complex YAML structures, follow these steps to complete the test scene setup:

### 1. Open the Scene

1. Open Unity Editor
2. Navigate to `Assets/_Project/Scenes/TestScenes/`
3. Open `TestScene_ResourceSystem.unity`

### 2. Add ResourceManager GameObject

1. In the Hierarchy, create an empty GameObject (Right-click → Create Empty)
2. Rename it to "ResourceManager"
3. Add the `ResourceManager` component:
   - Click "Add Component"
   - Search for "Resource Manager"
   - Add the component from `SantasWorkshop.Core`
4. Position at (0, 0, 0)

### 3. Create UI Canvas

1. In the Hierarchy, create a Canvas (Right-click → UI → Canvas)
2. Set Canvas properties:
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920x1080

### 4. Add Status Text Display

1. Under Canvas, create a TextMeshPro text element (Right-click → UI → Text - TextMeshPro)
2. Rename to "StatusText"
3. Configure:
   - Rect Transform: Anchor to top-left
   - Position: X=400, Y=-100
   - Width: 700, Height: 150
   - Font Size: 24
   - Text: "Status: Waiting..."
   - Color: White

### 5. Add Resource Counts Display

1. Under Canvas, create another TextMeshPro text element
2. Rename to "ResourceCountsText"
3. Configure:
   - Rect Transform: Anchor to top-right
   - Position: X=-300, Y=-100
   - Width: 500, Height: 800
   - Font Size: 20
   - Text: "Resource Counts"
   - Color: White
   - Alignment: Top-Left

### 6. Create Button Panel

1. Under Canvas, create a Panel (Right-click → UI → Panel)
2. Rename to "ButtonPanel"
3. Configure:
   - Rect Transform: Anchor to bottom
   - Position: X=0, Y=150
   - Width: 1600, Height: 250
   - Color: Semi-transparent (Alpha: 0.8)

### 7. Add Test Buttons

Under ButtonPanel, create the following buttons with TextMeshPro text:

#### Add Wood Button
- Name: "AddWoodButton"
- Position: X=-500, Y=50
- Size: 200x60
- Text: "Add Wood (+10)"

#### Add Iron Button
- Name: "AddIronButton"
- Position: X=-250, Y=50
- Size: 200x60
- Text: "Add Iron (+10)"

#### Consume Wood Button
- Name: "ConsumeWoodButton"
- Position: X=0, Y=50
- Size: 200x60
- Text: "Consume Wood (-5)"

#### Consume Iron Button
- Name: "ConsumeIronButton"
- Position: X=250, Y=50
- Size: 200x60
- Text: "Consume Iron (-5)"

#### Query Resources Button
- Name: "QueryResourcesButton"
- Position: X=500, Y=50
- Size: 200x60
- Text: "Query Resources"

#### Reset Button
- Name: "ResetButton"
- Position: X=-250, Y=-50
- Size: 200x60
- Text: "Reset All"
- Color: Red tint

#### Set Capacity Button
- Name: "SetCapacityButton"
- Position: X=0, Y=-50
- Size: 200x60
- Text: "Set Capacity (100)"

### 8. Add ResourceSystemTester Component

1. Create an empty GameObject in the Hierarchy
2. Rename to "TestController"
3. Add the `ResourceSystemTester` component:
   - Click "Add Component"
   - Search for "Resource System Tester"
   - Add the component from `SantasWorkshop.Testing`

### 9. Wire Up References

1. Select the "TestController" GameObject
2. In the Inspector, assign the following references:
   - **Status Text**: Drag StatusText from Hierarchy
   - **Resource Counts Text**: Drag ResourceCountsText from Hierarchy
   - **Add Wood Button**: Drag AddWoodButton from Hierarchy
   - **Add Iron Button**: Drag AddIronButton from Hierarchy
   - **Consume Wood Button**: Drag ConsumeWoodButton from Hierarchy
   - **Consume Iron Button**: Drag ConsumeIronButton from Hierarchy
   - **Query Resources Button**: Drag QueryResourcesButton from Hierarchy
   - **Reset Button**: Drag ResetButton from Hierarchy
   - **Set Capacity Button**: Drag SetCapacityButton from Hierarchy

### 10. Configure Test Parameters

In the ResourceSystemTester component, you can adjust:
- **Add Amount**: 10 (default)
- **Consume Amount**: 5 (default)
- **Capacity Limit**: 100 (default)

### 11. Save the Scene

1. File → Save (Ctrl+S)
2. The scene is now ready for testing

## Testing the Scene

### Play Mode Testing

1. Click Play in Unity Editor
2. Wait for "ResourceManager initialized successfully!" message
3. Test the buttons:
   - **Add Wood/Iron**: Adds resources to inventory
   - **Consume Wood/Iron**: Removes resources (fails if insufficient)
   - **Query Resources**: Displays detailed resource information
   - **Reset All**: Resets all resources to zero
   - **Set Capacity**: Sets capacity limit for wood to 100

### Expected Behavior

- Status text updates with each operation
- Resource counts display updates automatically
- Events fire correctly (check Console for logs)
- Capacity limits are enforced
- Consume operations fail gracefully when insufficient resources

## Troubleshooting

### ResourceManager not found
- Ensure ResourceManager GameObject exists in scene
- Ensure ResourceManager component is attached
- Check that ResourceData assets exist in Resources/ResourceDefinitions

### Buttons not working
- Verify all button references are assigned in TestController
- Check that buttons have the Button component
- Ensure EventSystem exists in scene (created automatically with Canvas)

### Resources not displaying
- Ensure ResourceData assets exist with correct resourceIds ("wood", "iron_ore")
- Check that Resources/ResourceDefinitions folder contains assets
- Verify ResourceManager.Initialize() is called

## Next Steps

After testing, you can:
1. Add more test buttons for other operations
2. Test save/load functionality
3. Test capacity management
4. Test atomic multi-resource operations
5. Profile performance with large resource counts

## Requirements Covered

This test scene covers the following requirements from the Testing Strategy:
- ✅ Manual testing of Add/Consume/Query operations
- ✅ UI display for resource counts
- ✅ Event system validation
- ✅ Capacity limit testing
- ✅ ResourceManager initialization testing
