# FishUI - TODO

A list of planned features, improvements, and new controls for FishUI.

> **CPX (Complexity Points)** - 1 to 5 scale:
> - **1** - Single file control/component
> - **2** - Single file control/component with single function change dependencies
> - **3** - Multi-file control/component or single file with multiple dependencies, no architecture changes
> - **4** - Multi-file control/component with multiple dependencies and significant logic, possible minor architecture changes
> - **5** - Large feature spanning multiple components and subsystems, major architecture changes

> Instructions for the TODO list:
- Move all completed items into separate Completed section
- Consolidate all completed TODO items by combining similar ones and shortening the descriptions where possible

> How TODO file should be iterated:
- First handle the Uncategorized section, if any similar issues already are on the TODO list, increase their priority instead of adding duplicates (take one at a time)
- When Uncategorized section is empty, start by fixing Active Bugs (take one at a time)
- After Active Bugs, handle the rest of the TODO file by priority and complexity (High priority takes precedance, then CPX points) (take one at a time).

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
| RadialGauge | ✅ Complete | N/A |
| PropertyGrid | ✅ Complete | N/A |

---

## New Controls

### High Priority

- [x] ~~**PropertyGrid**~~ - Windows Forms-like property editor with categorization, reflection-based editing, support for string/int/float/bool/enum types; demo in SamplePropertyGrid

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

- [x] ~~**ImageBox, StaticText, VUMeter, AnimatedImageBox**~~ - Display controls with scaling, alignment, animation support

- [ ] **DateTimePicker** (4 CPX)
  - Calendar popup for date selection
  - Optional time selection spinner
  - Configurable date formats

- [ ] **ItemListbox** (2 CPX)
  - Listbox with custom item widgets
  - *GWEN WidgetLook: ItemListbox*

- [x] ~~**MenuItem, RadialGauge, BarGauge**~~ - Menu items and gauge controls with color zones, ticks, labels

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
- [x] ~~Icon, Toggle, Repeat, ImageButton modes~~ - Full button variants with icons, toggle state, repeat firing, image-only modes

### Panel Enhancements
- [x] ~~Border styles, Panel variants (Normal, Bright, Dark, Highlight), examples~~ - Added PanelVariant enum, BorderStyle enum (None, Solid, Inset, Outset), theme regions, and SampleDefault demo

### ProgressBar Enhancements
- [x] ~~Vertical progress bar mode~~ - Already implemented via Orientation property; added demo in SampleDefault

### Input Control Enhancements (Completed)
- [x] ~~Mouse wheel support~~ - Added to Slider, ScrollBarV/H, NumericUpDown
- [x] ~~NumericUpDown narrower buttons~~ - Reduced ButtonWidth for better aspect ratio
- [x] ~~TabControl serialization~~ - Tab names preserved across save/load
- [x] ~~BarGauge styling~~ - Improved tick marks and color zones

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
- [ ] Auto-sizing controls based on content (3 CPX)
- [x] ~~StackLayout, Anchor, Margin/Padding~~ - Layout infrastructure complete

### Rendering
- [ ] Animation system for transitions (4 CPX)
- [ ] Anti-aliased rendering option (2 CPX)
- [ ] Shadow rendering for windows/popups (2 CPX)
  - *GWEN has: Shadow region*
- [x] ~~Opacity, fonts, image rotation/scaling~~ - Rendering features complete

### Accessibility
- [ ] Keyboard-only navigation (3 CPX)
- [ ] Scalable UI for different DPI (4 CPX)

### Virtual Cursor
- [x] ~~Position setting, input mapping, hybrid mode~~ - SetPositionFromInput, SyncWithRealMouse, configurable key bindings, click passthrough

### Serialization
- [ ] Image reference serialization (3 CPX)
  - Icons/images (Button.Icon, etc.) not preserved across Save/Load Layout
  - Need mechanism to reference images by file path or ID in layout files
- [x] ~~LayoutFormat TypeMapping~~ - All controls registered for save/load

---

## Sample Application

- [x] ~~Sample infrastructure~~ - Split samples, runner loop, console chooser with CLI args, theme switcher, game menu
- [x] ~~Control demos~~ - ImageBox, BarGauge interactive, VirtualCursor with custom images/hybrid mode, AnimatedImageBox, StaticText
- [ ] Examples should be implemented in FishUISample project, using the ISample interface (2 CPX each)
- [ ] **GUI-based sample chooser** (3 CPX)
  - Replace console menu with FishUI-based sample selector window
  - Display available samples as clickable buttons or list
  - Return to GUI selector when sample window is closed
  - Self-dogfooding: uses FishUI to demonstrate FishUI
- [ ] **Move gauge controls to dedicated sample page** (2 CPX)
  - Move RadialGauge and BarGauge from SampleBasicControls to new SampleGauges
  - Include VUMeter in the gauges sample
  - Add interactive controls for all gauges
- [ ] Car dashboard example with gauges (2 CPX)
  - RadialGauge and BarGauge controls now available
- [ ] **ImageBox example improvements** (2 CPX)
  - Use images from data/images/ folder instead of silk_icons
  - Add pixelated (nearest-neighbor) rendering mode to ImageBox
- [ ] MultiLine textbox example (1 CPX)
  - Add below existing single-line textbox sample
  - Requires multi-line textbox mode implementation

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
- [x] ~~Completed~~ - ScreenCapture platform compatibility, YamlIgnore audit, naming conventions, XML docs, screenshot buttons, TODO extraction, debug flags refactor

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