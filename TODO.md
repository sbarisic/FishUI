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
- First handle the Uncategorized section, if any similar issues already are on the TODO list, increase their priority instead of adding duplicates (categorize all at once)
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
| MenuBar | ✅ Complete | ✅ Atlas |

---

## New Controls

### High Priority

*All high priority controls have been implemented*

### Medium Priority

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
  - Text only cell type for now
  - *Use case: Data entry forms, configuration editors*

- [ ] **MultiLineEditbox / TextArea** (3 CPX)
  - Multi-line text input
  - Word wrap support
  - Vertical scrollbar integration
  - *GWEN atlas regions: Uses TextBox regions*

- [ ] **DateTimePicker** (4 CPX)
  - Calendar popup for date selection
  - Optional time selection spinner
  - Configurable date formats

- [ ] **ItemListbox** (2 CPX)
  - Listbox with custom item widgets
  - *GWEN WidgetLook: ItemListbox*

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

### PropertyGrid Enhancements
- [ ] Vector2/Vector3/Vector4 component editors (2 CPX)
  - Display vectors as separate editable X, Y, Z fields
  - Currently vectors fall through to text editor which doesn't work

### BarGauge Enhancements
- [ ] Black tick marks for better visibility (1 CPX)
- [ ] Car dashboard styling improvements (2 CPX)
  - Larger default size
  - Range step labels (RPM, speed, etc.)
  - More authentic dashboard appearance

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

### Rendering
- [ ] Animation system for transitions (4 CPX)
- [ ] Anti-aliased rendering option (2 CPX)
- [ ] Shadow rendering for windows/popups (2 CPX)
  - *GWEN has: Shadow region*

### Accessibility
- [ ] Keyboard-only navigation (3 CPX)
- [ ] Scalable UI for different DPI (4 CPX)

### Serialization
- [ ] Image reference serialization (3 CPX)
  - Icons/images (Button.Icon, etc.) not preserved across Save/Load Layout
  - Need mechanism to reference images by file path or ID in layout files

---

## Sample Application

- [ ] Examples should be implemented in FishUISample project, using the ISample interface (2 CPX each)
- [ ] **ImageBox example improvements** (2 CPX)
  - Use images from data/images/ folder instead of silk_icons
  - Add pixelated (nearest-neighbor) rendering mode to ImageBox
- [ ] MultiLine textbox example (separate demo)
  - Add single line and multi line 
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

*All code cleanup items have been completed - see Completed section*

---

## Known Issues / Bugs

### Active Bugs

*No active bugs*

### Uncategorized (Analyze and create TODO entries in above appropriate sections with priority. Do not fix or implement them just yet. Assign complexity points where applicable. Do not delete this section when you are done, just empty it)

*No uncategorized items*

---

## Notes

- Prioritize controls that are commonly needed in game development
- Maintain the "dependency-free" philosophy - keep the core library minimal
- Backend implementations should remain in separate sample projects
- CEGUI theme files in `data/cegui_theme/` provide reference for accurate atlas coordinates
- The GWEN skin atlas (gwen.png) contains 512x512 pixels of UI elements

---

## Completed

### Controls
- PropertyGrid - Windows Forms-like property editor with categorization, reflection-based editing
- ContextMenu / PopupMenu - Right-click menus with submenus, keyboard navigation
- MenuBar - Horizontal menu strip with dropdown menus, hover switching, submenus
- ImageBox, StaticText, VUMeter, AnimatedImageBox - Display controls
- MenuItem, RadialGauge, BarGauge - Menu items and gauge controls

### Control Improvements
- Button: Icon, Toggle, Repeat, ImageButton modes
- Panel: Border styles and variants (Normal, Bright, Dark, Highlight)
- ProgressBar: Vertical orientation mode
- ListBox: Alternating row colors
- Input controls: Mouse wheel support for Slider, ScrollBars, NumericUpDown
- NumericUpDown: Narrower buttons, TabControl serialization, BarGauge styling

### Core Framework
- StackLayout, Anchor, Margin/Padding - Layout infrastructure
- Opacity, fonts, image rotation/scaling - Rendering features
- Virtual cursor: Position setting, input mapping, hybrid mode
- LayoutFormat TypeMapping - All controls registered

### Samples
- Sample infrastructure: Split samples, runner loop, GUI-based chooser, theme switcher, game menu
- GUI sample chooser: FishUI-based sample selector replacing console menu (self-dogfooding)
- Control demos: ImageBox, BarGauge, VirtualCursor, AnimatedImageBox, StaticText
- SampleGauges: Dedicated gauge demo with RadialGauge, BarGauge, VUMeter, car dashboard preview

### Code Cleanup
- ScreenCapture platform compatibility, YamlIgnore audit, naming conventions, XML docs, screenshot buttons, TODO extraction, debug flags refactor

### Fixed Bugs
- DropDown: Overlay rendering, click-through fixes, AlwaysOnTop handling, mouse leave behavior
- Virtual cursor: Pick order, rendering order, arrow key handling
- Serialization: TreeNode/TabControl YamlIgnore, Window/TabControl OnDeserialized
- PropertyGrid: Inline editors sync values, auto-focus, fixed indentation and int conversion
- MenuBar: Dropdown menus now properly added to FishUI root for correct rendering
- Misc: ImageBox overlap, ScrollBar mouse wheel, ContextMenu submenu overlap