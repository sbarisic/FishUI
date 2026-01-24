# FishUI - TODO

A list of planned features, improvements, and new controls for FishUI.

> **CPX (Complexity Points)** - 1 to 5 scale:
> - **1** - Single file control/component
> - **2** - Single file control/component with single function change dependencies
> - **3** - Multi-file control/component or single file with multiple dependencies, no architecture changes
> - **4** - Multi-file control/component with multiple dependencies and significant logic, possible minor architecture changes
> - **5** - Large feature spanning multiple components and subsystems, major architecture changes

> How TODO file should be iterated
- After Active Bugs the rest of the TODO file by priority and complexity (High priority takes precedance, then CPX points) (take one at a time). Consolidate all completed TODO items by combining similar ones and shortening the descriptions where possible
- First handle the Uncategorized section, if any similar issues already are on the TODO list, increase their priority instead of adding duplicates
- When Uncategorized section is empty, start by fixing Active Bugs (take one at a time)

---

## Current Controls (Implemented)

| Control | Status | Theme Support |
|---------|--------|---------------|
| Button | ✅ Complete | ✅ Atlas |
| CheckBox | ✅ Complete | ✅ Atlas |
| RadioButton | ✅ Complete | ✅ Atlas |
| Panel | ✅ Complete | ✅ Atlas |
| Label | ✅ Complete | Colors |
| Textbox | ✅ Complete | ✅ Atlas |
| ListBox | ✅ Complete | ✅ Atlas |
| DropDown (Combobox) | ✅ Complete | ✅ Atlas |
| ScrollBarV | ✅ Complete | ✅ Atlas |
| ScrollBarH | ✅ Complete | ✅ Atlas |
| ProgressBar | ✅ Complete | ✅ Atlas |
| Slider | ✅ Complete | ✅ Atlas |
| ToggleSwitch | ✅ Complete | ✅ Atlas |
| SelectionBox | ✅ Complete | ✅ Atlas |
| Window | ✅ Complete | ✅ Atlas |
| Titlebar | ✅ Complete | ✅ Atlas |
| TabControl | ✅ Complete | ✅ Atlas |
| GroupBox | ✅ Complete | ✅ Atlas |
| TreeView | ✅ Complete | ✅ Atlas |
| NumericUpDown | ✅ Complete | ✅ Atlas |
| Tooltip | ✅ Complete | ✅ Atlas |
| ContextMenu | ✅ Complete | ✅ Atlas |
| MenuItem | ✅ Complete | ✅ Atlas |
| StackLayout | ✅ Complete | N/A |
| ImageBox | ✅ Complete | N/A |
| StaticText | ✅ Complete | N/A |
| BarGauge | ✅ Complete | N/A |
| VUMeter | ✅ Complete | N/A |
| AnimatedImageBox | ✅ Complete | N/A |

---

## New Controls

### High Priority

*All high priority controls have been implemented*

### Medium Priority

- [x] ~~**ContextMenu / PopupMenu**~~ - Right-click context menus with submenu support, keyboard navigation, check marks

- [ ] **MenuBar** (3 CPX)
  - Horizontal menu strip
  - Dropdown submenus
  - Keyboard shortcuts display
  - Separator items
  - *GWEN atlas regions available: Menu.Strip, Menu.Background, Menu.Hover*

- [ ] **ScrollablePane** (3 CPX)
  - Container with automatic scrollbars
  - Virtual scrolling for large content
  - *GWEN atlas regions: Uses existing ScrollBarV/ScrollBarH*

### Lower Priority

- [ ] **MultiColumnList / DataGrid** (4 CPX)
  - Tabular data display with multiple columns
  - Column headers with sorting
  - Column resizing
  - Row selection
  - *GWEN atlas regions available: ListHeaderSegment, ListHeader*

- [ ] **SpreadsheetGrid** (5 CPX)
  - Spreadsheet cell is a separate control, grid is made of cells
  - Excel-like table control with cell editing
  - X-axis (column) and Y-axis (row) headers
  - Cell selection and navigation
  - ~~Configurable cell types (text, number, dropdown)~~ Text only cell type for now
  - *Use case: Data entry forms, configuration editors*

- [ ] **MultiLineEditbox / TextArea** (3 CPX)
  - Multi-line text input
  - Word wrap support
  - Vertical scrollbar integration
  - *GWEN atlas regions: Uses TextBox regions*

- [x] ~~**ImageBox / StaticImage**~~ - Image display with scaling modes (None, Stretch, Fit, Fill) and click events

- [x] ~~**StaticText**~~ - Non-editable text with horizontal/vertical alignment, custom color, optional background

- [x] ~~**VUMeter**~~ - Audio level visualization with peak hold, green/yellow/red zones, continuous or segmented mode; demo in SampleBasicControls

- [x] ~~**AnimatedImageBox**~~ - Frame-based animation with configurable frame rate, loop/reverse/ping-pong modes, play/pause control

- [ ] **DateTimePicker** (4 CPX)
  - Calendar popup for date selection
  - Optional time selection spinner
  - Configurable date formats

- [ ] **ItemListbox** (2 CPX)
  - Listbox with custom item widgets
  - *GWEN WidgetLook: ItemListbox*

- [x] ~~**MenuItem**~~ - Individual menu item with text, shortcut display, checkbox states (implemented with ContextMenu)

- [ ] **RadialGauge** (3 CPX) **HIGH PRIORITY**
  - Circular gauge display (e.g., RPM, speedometer)
  - Configurable min/max values and range angle
  - Needle/pointer rendering
  - Tick marks and labels
  - *Use case: Dashboard-style data visualization*

- [x] ~~**BarGauge**~~ - Linear gauge with min/max values, color zones (temperature/fuel presets), ticks, labels; demo in SampleBasicControls

- [ ] **PropertyGrid** (4 CPX) **HIGH PRIORITY**
  - Windows Forms-like property editor control
  - Displays object properties in categorized/alphabetical list
  - Supports editing of common types (string, int, float, bool, enum, color)
  - Expandable nested objects
  - *Use case: In-game editors, debug panels, configuration UIs*

---

## Control Improvements

### Textbox Enhancements
- [ ] Multi-line mode with character wrap (3 CPX)

### DropDown Enhancements
- [ ] Multi-select mode (checkbox icon per item) (2 CPX)
- [ ] Custom item rendering (2 CPX)
- [ ] Search/filter functionality (2 CPX)

### ListBox Enhancements
- [ ] Multi-select mode (Ctrl+click, Shift+click) (2 CPX)
- [ ] Virtual scrolling for large lists (3 CPX)
- [ ] Custom item templates (2 CPX)
- [ ] Drag and drop reordering (3 CPX)
- [x] ~~Even/odd row alternating colors~~ - Added AlternatingRowColors, EvenRowColor, OddRowColor properties; demo in SampleBasicControls

### Button Enhancements
- [x] ~~Icon support (image + text), add to examples~~ - Added Icon, IconPosition, IconSpacing properties; SampleDefault has icon button demo
- [x] ~~Toggle button mode~~ - Added IsToggleButton, IsToggled properties, OnToggled event; demo in SampleDefault
- [x] ~~Repeat button mode (fires while held)~~ - Added IsRepeatButton, RepeatDelay, RepeatInterval properties; demo in SampleDefault
- [x] ~~ImageButton variant (icon-only button)~~ - Added IsImageButton, ImageButtonHoverTint, ImageButtonPressedTint; demo in SampleDefault

### Panel Enhancements
- [x] ~~Border styles, Panel variants (Normal, Bright, Dark, Highlight), examples~~ - Added PanelVariant enum, BorderStyle enum (None, Solid, Inset, Outset), theme regions, and SampleDefault demo

### ProgressBar Enhancements
- [x] ~~Vertical progress bar mode~~ - Already implemented via Orientation property; added demo in SampleDefault

### Slider Enhancements
- [x] ~~Mouse wheel support~~ - Scroll to adjust value (uses Step or 1% of range)

### ScrollBar Enhancements
- [x] ~~Mouse wheel support for ScrollBarV/ScrollBarH~~ - Both scroll bars now support mouse wheel scrolling

### NumericUpDown Enhancements
- [x] ~~Mouse wheel support~~ - Scroll to increment/decrement value
- [x] ~~Narrower arrow buttons~~ - Reduced ButtonWidth from 20 to 16 for better aspect ratio

### TabControl Enhancements
- [x] ~~Preserve tab names during serialization~~ - Added TabNames property that syncs with TabPages; names restored in OnDeserialized

### BarGauge Enhancements
- [x] ~~Visual styling improvements~~ - Brighter tick marks (white), gray background for unfilled area, color zones only on filled portion

---

## Theme System

### Theme File Improvements
- [ ] Theme inheritance / base themes (3 CPX)
  - Allow theme YAML files to inherit from other themes (e.g., `gwen2.yaml` inherits from `gwen.yaml`)
  - Child themes only need to override specific regions, inheriting all others from parent
- [ ] Per-control color overrides (2 CPX)

### Theme Assets Available in gwen.png (from CEGUI imageset)
The following regions are defined in the CEGUI imageset but may not be fully utilized:

**Windows:**
- `Window.Head.Normal/Inactive` (Left, Middle, Right)
- `Window.Middle.Normal/Inactive` (Left, Middle, Right)
- `Window.Bottom.Normal/Inactive` (Left, Middle, Right)
- `Window.Close`, `Window.Close_Hover`, `Window.Close_Down`, `Window.Close_Disabled`

**Tabs:**
- `Tab.Top.Active/Inactive` (Top, Middle, Bottom sections)
- `Tab.Bottom.Active/Inactive` (Top, Middle, Bottom sections)
- `Tab.Left.Active/Inactive`, `Tab.Right.Active/Inactive`
- `Tab.Control`, `Tab.HeaderBar`

**Menus:**
- `Menu.Strip`, `Menu.BackgroundWithMargin`
- `Menu.Background` (9-slice)
- `Menu.Hover` (9-slice)
- `Menu.Check`, `Menu.RightArrow`, `Menu.LeftArrow`

**Misc:**
- `Tooltip` (9-slice)
- `GroupBox.Normal` (9-slice)
- `Tree` (9-slice) + `Tree.Plus`, `Tree.Minus`
- `StatusBar`
- `CategoryList.Outer`, `CategoryList.Inner`, `CategoryList.Header`
- `Shadow`
- `Selection`

---

## Core Framework Features

### Layout System
- [ ] **FlowLayout** - Automatic horizontal/vertical flow of children (3 CPX)
- [ ] **GridLayout** - Grid-based child positioning (3 CPX)
- [x] ~~**StackLayout**~~ - Vertical/horizontal stacking with spacing and padding
- [x] ~~Anchor system for responsive resizing~~ - Added FishUIAnchor enum; Anchor property on Control with TopLeft/TopRight/BottomLeft/BottomRight/Horizontal/Vertical/All modes; demo in SampleDefault
- [x] ~~Margin and Padding properties on all controls~~ - Added FishUIMargin struct; Margin/Padding properties on Control; demo in SampleDefault
- [ ] Auto-sizing controls based on content (3 CPX)

### Rendering
- [x] ~~Control opacity/transparency~~ - Added Opacity property (0.0-1.0) and EffectiveColor; updated Panel, Button, ImageBox; demo in SampleBasicControls
- [ ] Animation system for transitions (4 CPX)
- [ ] Anti-aliased rendering option (2 CPX)
- [ ] Shadow rendering for windows/popups (2 CPX)
  - *GWEN has: Shadow region*
- [x] ~~Font system refactoring~~ - Added FontStyle enum, FishUIFontMetrics struct, GetFontMetrics method, IsMonospaced detection, LineHeight property
- [x] ~~Rendering images with rotation and scaling~~ - Already supported via DrawImage(Rot, Scale) and DrawNPatch(Rotation) in IFishUIGfx

### Accessibility
- [ ] Keyboard-only navigation (3 CPX)
- [ ] Scalable UI for different DPI (4 CPX)

### Virtual Cursor
- [x] ~~Direct position setting~~ - Added SetPositionFromInput and SyncWithRealMouse methods for touch/mouse mapping
- [x] ~~**Refactor virtual mouse input mapping**~~ - Moved keyboard handling into FishUIVirtualMouse.Update(); added configurable key bindings (MoveUpKeys, MoveDownKeys, MoveLeftKeys, MoveRightKeys, LeftClickKeys, RightClickKeys); UseKeyboardInput flag for hybrid mode
- [x] ~~**Hybrid mode click passthrough**~~ - Added SyncButtonsWithRealMouse() method; SampleVirtualCursor now passes through real mouse clicks in hybrid mode

### Serialization
- [x] ~~LayoutFormat TypeMapping~~ - Added all implemented controls for layout save/load
- [ ] Image reference serialization (3 CPX)
  - Icons/images (Button.Icon, etc.) not preserved across Save/Load Layout
  - Need mechanism to reference images by file path or ID in layout files

---

## Sample Application

- [x] ~~**Split SampleDefault into multiple smaller samples**~~ - Created SampleBasicControls, SampleButtonVariants, SampleLayoutSystem; refocused SampleDefault to Windows/Dialogs only
- [x] ~~**Sample runner loop back to chooser**~~ - Window close returns to sample chooser instead of exiting
- [ ] Examples should be implemented in FishUISample project, using the ISample interface (2 CPX each)
- [x] ~~Implement sample chooser system in Program.cs~~ - Added ChooseSample() with console menu and --sample/-s CLI args; added Name property to ISample
- [x] ~~Theme switcher example~~ (SampleThemeSwitcher.cs)
- [x] ~~Game main menu example~~ (SampleGameMenu.cs) - Buttons + Options window with tabs
- [ ] Car dashboard example with gauges
  - Requires RadialGauge and BarGauge controls first
- [x] ~~ImageBox example~~ - Added to SampleBasicControls with all scaling modes and click event demo
- [ ] **ImageBox example improvements** (2 CPX)
  - Use images from data/images/ folder instead of silk_icons
  - Add pixelated (nearest-neighbor) rendering mode to ImageBox
- [x] ~~BarGauge example interactive controls~~ - Added +/- repeat buttons for all gauges in SampleBasicControls
- [x] ~~Virtual cursor example~~ - SampleVirtualCursor demonstrates keyboard/gamepad cursor navigation
- [x] ~~Virtual cursor custom images~~ - Added cursor image dropdown to SampleVirtualCursor; loads from data/images/cursors/
- [x] ~~Virtual cursor hybrid mode demo~~ - When checkbox unchecked, sync virtual cursor with real mouse; demonstrates SyncWithRealMouse method
- [x] ~~AnimatedImageBox example with stargate images~~ - Added to SampleBasicControls with play/pause, stop, FPS slider, ping-pong toggle
- [x] ~~AnimatedImageBox resizable window example~~ - "Animation Viewer" window with anchored AnimatedImageBox that scales with window resize
- [x] ~~StaticText example~~ - Added to SampleBasicControls with alignment and color demos
- [ ] MultiLine textbox example (1 CPX)
  - Add below existing single-line textbox sample
  - Requires multi-line textbox mode implementation

### SampleDefault Improvements
- [x] ~~Replace main Panel with Window control~~ - Changed `panel1` from Panel to Window with hidden close button
- [x] ~~Add context menu to SampleDefault~~ - Demonstrates ContextMenu with items, separators, checkable items, and submenus
- [x] ~~Expand StackLayout demo~~ - Added nested stacks (horizontal containing 3 vertical stacks with mixed controls)

---

## Documentation

- [ ] Update complete README.md from scratch 
- [ ] API reference documentation
- [ ] Getting started tutorial
- [ ] Custom control creation guide
- [ ] Backend implementation guide (beyond Raylib)
- [ ] Theme creation guide

---

## Code Cleanup & Technical Debt
- [x] ~~ScreenCapture platform compatibility~~ - Added IsSupported check, [SupportedOSPlatform] attributes, TryCaptureActiveWindow safe method; resolves CA1416 warnings
- [x] ~~Audit [YamlIgnore] attributes~~ - Added to FishUIThemeRegion.UsesImageFile and FishUIVirtualMouse read-only properties
- [x] ~~Standardize naming conventions~~ - Converted fields to properties: CheckBox/RadioButton `Checked`→`IsChecked`, Panel `IsTransparent`, ListBox `ShowScrollBar`
- [x] ~~Add XML documentation comments to core public APIs~~ - Documented Control, FishUI, FishUISettings, IFishUIGfx, IFishUIInput
- [x] ~~Add screenshot button to all examples~~ - Added TakeScreenshot to ISample; screenshot buttons with camera icons in all 3 samples
- [x] ~~Extract TODO comments from code~~ - Moved 4 TODOs to proper sections, removed comments from source
- [x] ~~Move DebugLogTooltips flag into FishUIDebug~~ - Now forwards to `FishUIDebug.LogTooltips` like other debug flags

---

## Known Issues / Bugs

### Active Bugs

*No active bugs*

### Fixed Bugs

- [x] ~~DropDown list renders behind other controls~~ - Added overlay rendering: dropdown lists now drawn after all controls via static OpenDropdowns list and DrawDropdownListOverlay method

- [x] ~~DropDown list click-through in SampleVirtualCursor~~ - Modified GetAllChildren to handle AlwaysOnTop like GetOrderedControls; dropdown with AlwaysOnTop=true is now checked first

- [x] ~~DropDown list still doesn't block virtual cursor clicks~~ - Modified PickControl to check children first, before checking parent's IsPointInside; fixes controls that extend beyond parent bounds

- [x] ~~DropDown list doesn't block clicks to controls behind it~~ - Set AlwaysOnTop=true and BringToFront() when dropdown opens, restore on close

- [x] ~~Virtual cursor has wrong pick order~~ - Added ZDepth assignment in Control.AddChild() so children are ordered by insertion order

- [x] ~~DropDown closes when virtual cursor moves to dropdown list~~ - Modified HandleMouseLeave to check if cursor is inside dropdown list before closing

- [x] ~~Virtual cursor renders below controls~~ - Moved VirtualMouse.Draw() inside Draw() method before EndDrawing() so it renders on top

- [x] ~~Virtual cursor not responding to arrow keys~~ - Added automatic arrow key and button input handling in FishUI.Tick() when VirtualMouse.Enabled is true

- [x] ~~ImageBox examples overlap in SampleBasicControls~~ - Moved ImageBox controls 100 units right (X: 500→600, 590→690)

- [x] ~~ScrollBar mouse wheel not working in examples~~ - Mouse wheel events now bubble up to parent controls; child buttons propagate to ScrollBar

- [x] ~~TreeNode.HasChildren deserialization error~~ - Added `[YamlIgnore]` to read-only property
- [x] ~~TabControl.SelectedTab deserialization error~~ - Added `[YamlIgnore]` to read-only property
- [x] ~~DropDown.SelectIndex NullReferenceException~~ - Added null check before `FishUI.Events.Broadcast()`
- [x] ~~Make Invisible/Visible buttons broken~~ - Changed `Panel` cast to `Control` in EvtHandler after Panel→Window conversion
- [x] ~~ContextMenu submenu text overlapping~~ - Added `DrawChildren` override in MenuItem to prevent rendering submenu item data as visual children
- [x] ~~Layout save/load breaks Window and TabControl~~ - Added `OnDeserialized` virtual method to Control, overridden in Window and TabControl to reinitialize internal state

### Uncategorized (Analyze and create TODO entries in above appropriate sections with priority. Do not fix or implement them just yet. Assign complexity points where applicable. Do not delete this section when you are done, just empty it)

*No uncategorized items*

---

## Notes

- Prioritize controls that are commonly needed in game development
- Maintain the "dependency-free" philosophy - keep the core library minimal
- Backend implementations should remain in separate sample projects
- CEGUI theme files in `data/cegui_theme/` provide reference for accurate atlas coordinates
- The GWEN skin atlas (gwen.png) contains 512x512 pixels of UI elements