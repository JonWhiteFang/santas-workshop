# Manual Setup Alternative - Resource System Test Scene

If the automated menu item isn't appearing, you can set up the test scene manually. This takes about 5-10 minutes.

## Prerequisites

Ensure these ResourceData assets exist in `Assets/_Project/Resources/ResourceDefinitions/`:
- Wood resource with `resourceId = "wood"`
- Iron Ore resource with `resourceId = "iron_ore"`

## Step-by-Step Manual Setup

### 1. Open the Test Scene

1. In Unity Project window, navigate to: `Assets/_Project/Scenes/TestScenes/`
2. Double-click `TestScene_ResourceSystem.unity` to open it

### 2. Add ResourceManager

1. In Hierarchy, right-click → Create Empty
2. Rename to "ResourceManager"
3. With ResourceManager selected, click "Add Component"
4. Search for "Resource Manager" and add it
5. Set Transform position to (0, 0, 0)

### 3. Create Canvas

1. In Hierarchy, right-click → UI → Canvas
2. Select the Canvas, in Inspector:
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler → UI Scale Mode: Scale With Screen Size
   - Canvas Scaler → Reference Resolution: 1920 x 1080

### 4. Create Status Text

1. Right-click Canvas → UI → Text - TextMeshPro
   - If prompted to import TMP Essentials, click "Import TMP Essentials"
2. Rename to "StatusText"
3. In Inspector:
   - Rect Transform:
     - Anchor Presets: Top-Left (hold Alt+Shift and click)
     - Pos X: 400, Pos Y: -100
     - Width: 700, Height: 150
   - TextMeshPro:
     - Text: "Status: Waiting..."
     - Font Size: 24
     - Color: White
     - Alignment: Left

### 5. Create Resource Counts Text

1. Right-click Canvas → UI → Text - TextMeshPro
2. Rename to "ResourceCountsText"
3. In Inspector:
   - Rect Transform:
     - Anchor Presets: Top-Right (hold Alt+Shift and click)
     - Pos X: -300, Pos Y: -100
     - Width: 500, Height: 800
   - TextMeshPro:
     - Text: "Resource Counts"
     - Font Size: 20
     - Color: White
     - Alignment: Top-Left

### 6. Create Button Panel

1. Right-click Canvas → UI → Panel
2. Rename to "ButtonPanel"
3. In Inspector:
   - Rect Transform:
     - Anchor Presets: Bottom-Center (hold Alt+Shift and click)
     - Pos X: 0, Pos Y: 150
     - Width: 1600, Height: 250
   - Image:
     - Color: RGBA (51, 51, 51, 204) - dark gray, semi-transparent

### 7. Create Buttons

For each button below, follow these steps:
1. Right-click ButtonPanel → UI → Button - TextMeshPro
2. Rename as specified
3. Set Rect Transform position and size
4. Change button text

#### Button 1: Add Wood
- Name: "AddWoodButton"
- Rect Transform:
  - Anchor: Center
  - Pos X: -500, Pos Y: 50
  - Width: 200, Height: 60
- Text (child object): "Add Wood (+10)"

#### Button 2: Add Iron
- Name: "AddIronButton"
- Rect Transform:
  - Anchor: Center
  - Pos X: -250, Pos Y: 50
  - Width: 200, Height: 60
- Text: "Add Iron (+10)"

#### Button 3: Consume Wood
- Name: "ConsumeWoodButton"
- Rect Transform:
  - Anchor: Center
  - Pos X: 0, Pos Y: 50
  - Width: 200, Height: 60
- Text: "Consume Wood (-5)"

#### Button 4: Consume Iron
- Name: "ConsumeIronButton"
- Rect Transform:
  - Anchor: Center
  - Pos X: 250, Pos Y: 50
  - Width: 200, Height: 60
- Text: "Consume Iron (-5)"

#### Button 5: Query Resources
- Name: "QueryResourcesButton"
- Rect Transform:
  - Anchor: Center
  - Pos X: 500, Pos Y: 50
  - Width: 200, Height: 60
- Text: "Query Resources"

#### Button 6: Reset All
- Name: "ResetButton"
- Rect Transform:
  - Anchor: Center
  - Pos X: -125, Pos Y: -50
  - Width: 200, Height: 60
- Text: "Reset All"
- Button → Colors → Normal Color: Light Red (255, 128, 128)

#### Button 7: Set Capacity
- Name: "SetCapacityButton"
- Rect Transform:
  - Anchor: Center
  - Pos X: 125, Pos Y: -50
  - Width: 200, Height: 60
- Text: "Set Capacity (100)"

### 8. Create Test Controller

1. In Hierarchy, right-click → Create Empty
2. Rename to "TestController"
3. Click "Add Component"
4. Search for "Resource System Tester" and add it

### 9. Wire Up References

1. Select "TestController" in Hierarchy
2. In Inspector, find the ResourceSystemTester component
3. Drag and drop the following from Hierarchy to the corresponding fields:

   - **Status Text** ← StatusText
   - **Resource Counts Text** ← ResourceCountsText
   - **Add Wood Button** ← AddWoodButton
   - **Add Iron Button** ← AddIronButton
   - **Consume Wood Button** ← ConsumeWoodButton
   - **Consume Iron Button** ← ConsumeIronButton
   - **Query Resources Button** ← QueryResourcesButton
   - **Reset Button** ← ResetButton
   - **Set Capacity Button** ← SetCapacityButton

4. Set test parameters (optional):
   - Add Amount: 10
   - Consume Amount: 5
   - Capacity Limit: 100

### 10. Save the Scene

1. File → Save (Ctrl+S)
2. The scene is now ready!

## Testing

1. Press Play
2. Wait for "ResourceManager initialized successfully!"
3. Test the buttons
4. Check Console for logs
5. Verify resource counts update in real-time

## Troubleshooting

### EventSystem Missing
If buttons don't respond:
1. Right-click Hierarchy → UI → Event System
2. This should be created automatically with Canvas, but add it if missing

### TextMeshPro Not Imported
If you see pink text or errors:
1. Window → TextMeshPro → Import TMP Essential Resources
2. Click "Import"

### ResourceManager Not Initializing
Check Console for errors. Ensure:
- ResourceData assets exist in `Resources/ResourceDefinitions/`
- ResourceData assets have correct `resourceId` values ("wood", "iron_ore")

### Buttons Not Wired Up
If clicking buttons does nothing:
1. Select TestController
2. Verify all button references are assigned (not "None")
3. Check Console for "ResourceManager not found!" errors

## Quick Verification Checklist

Before testing, verify:
- ✅ ResourceManager GameObject exists with ResourceManager component
- ✅ Canvas exists with proper settings
- ✅ StatusText and ResourceCountsText exist
- ✅ ButtonPanel with 7 buttons exists
- ✅ TestController exists with ResourceSystemTester component
- ✅ All references in TestController are assigned
- ✅ EventSystem exists in scene
- ✅ Scene is saved

## Next Steps

Once setup is complete:
1. Press Play to test
2. Follow testing procedures in QUICK_START.md
3. Report any issues or bugs found

---

**Estimated Setup Time**: 5-10 minutes  
**Difficulty**: Beginner-friendly  
**Alternative**: Wait for Unity to compile and use the automated setup menu
