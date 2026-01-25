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
| ScrollablePane | ✅ Complete | N/A |
| ItemListbox | ✅ Complete | ✅ Atlas |
| FlowLayout | ✅ Complete | N/A |
| GridLayout | ✅ Complete | N/A |

---

## New Controls

### High Priority

*All high priority controls have been implemented*

### Medium Priority

- [ ] **MultiLineEditbox / TextArea** (3 CPX)
  - Multi-line text input
  - Word wrap support
  - Vertical scrollbar integration
  - *GWEN atlas regions: Uses TextBox regions*

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

- [ ] **DateTimePicker** (4 CPX)
  - Calendar popup for date selection
  - Optional time selection spinner
  - Configurable date formats

---

## Control Improvements

### Textbox Enhancements
- [ ] Multi-line mode with character wrap (3 CPX)


---

## Theme System

### Theme File Improvements
- [ ] Theme inheritance / base themes (3 CPX) **HIGH PRIORITY**
  - Allow theme YAML files to inherit from other themes (e.g., `gwen2.yaml` inherits from `gwen.yaml`)
  - Child themes only need to override specific regions, inheriting all others from parent

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

### Accessibility
- [ ] Scalable UI for different DPI (4 CPX)

### Serialization
- [ ] Figure out how to handle events in serialization, extend the Event system?
- [ ] Image reference serialization (3 CPX)
  - Icons/images (Button.Icon, etc.) not preserved across Save/Load Layout
  - Need mechanism to reference images by file path or ID in layout files

---

## Sample Application

- [ ] Examples should be implemented in FishUISample project, using the ISample interface (2 CPX each)
- [ ] MultiLine textbox example (separate demo)
  - Add single line and multi line 
  - Requires multi-line textbox mode implementation
- [ ] SampleLayoutSystem: Use Window controls instead of Panels for demos (1 CPX)

---

## Documentation **LOW PRIORITY**

- [ ] API reference documentation
- [ ] Getting started tutorial
- [ ] Custom control creation guide
- [ ] Backend implementation guide (beyond Raylib)
- [ ] Theme creation guide

---

## Code Cleanup & Technical Debt

### External Dependency Abstraction

- [ ] **Logging Interface Abstraction** (2 CPX)
  - `FishUIDebug.cs` uses `Console.WriteLine` directly (lines 69, 81, 94, 104)
  - Should introduce `IFishUILogger` interface with methods like `Log(string message)`
  - Allow users to provide custom logging implementations (file, game console, etc.)

- [ ] **File System Interface Abstraction** (3 CPX)
  - `FishUIThemeLoader.cs` uses `File.Exists`, `File.ReadAllText` (lines 26, 29)
  - `LayoutFormat.cs` uses `File.WriteAllText`, `File.ReadAllText` (lines 22, 27)
  - Should introduce `IFishUIFileSystem` interface for file operations
  - Enable virtual file systems, embedded resources, or game engine asset systems

### Code Refactoring

*All code refactoring items have been completed - see Completed section*

---

## Known Issues / Bugs

### Active Bugs

*No active bugs*

### Uncategorized (Analyze and create TODO entries in above appropriate sections with priority. Do not fix or implement them just yet. Assign complexity points where applicable. Do not delete this section when you are done, just empty it)

- FishUI.cs:342 - Categorize TODO comment
- IFishUIEvents.cs - Categorize TODO comment

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
- ScrollablePane - Container with automatic scrollbars and virtual scrolling
- ItemListbox - Listbox with custom item widgets

### Control Improvements
- Button: Icon, Toggle, Repeat, ImageButton modes
- Panel: Border styles and variants (Normal, Bright, Dark, Highlight)
- ProgressBar: Vertical orientation mode
- ListBox: Alternating row colors, multi-select mode (Ctrl+click, Shift+click)
- ListBox: Public SelectedIndex property for easy get/set access
- DropDown: Search/filter functionality with type-to-filter support
- Per-control color overrides: ColorOverrides dictionary with GetColorOverride/SetColorOverride methods
- DropDown: Custom item rendering via CustomItemRenderer delegate
- ListBox: Custom item rendering via CustomItemRenderer delegate
- DropDown: Multi-select mode with checkbox icons per item
- ImageBox: Pixelated (nearest-neighbor) filter mode for pixel art
- Input controls: Mouse wheel support for Slider, ScrollBars, NumericUpDown
- NumericUpDown: Narrower buttons, TabControl serialization, BarGauge styling
- BarGauge: Black tick marks, dashboard styling (larger size, range labels)
- RadialGauge: Black text/ticks, wider draw angle (~260° sweep)
- PropertyGrid: Vector2/Vector3/Vector4 component editors with X/Y/Z/W fields

### Core Framework
- Animation system - Easing functions (14 types), FishUIAnimationManager, tween helpers, Control extensions (FadeIn/Out, SlideIn/Out, AnimatePosition/Size/Opacity, ScaleBounce)
- Auto-sizing controls - AutoSizeMode (None/Width/Height/Both), GetPreferredSize, Min/MaxSize constraints
- GridLayout - Grid-based child positioning with configurable columns, spacing, and stretch options
- FlowLayout - Automatic horizontal/vertical flow with wrapping (4 directions, 3 wrap modes)
- StackLayout, Anchor, Margin/Padding - Layout infrastructure
- Opacity, fonts, image rotation/scaling - Rendering features
- Shadow rendering: Window and ContextMenu shadow support with configurable offset and size
- Virtual cursor: Position setting, input mapping, hybrid mode
- LayoutFormat TypeMapping - All controls registered

### Samples
- SampleAnimations: Animation system demo with fade, slide, scale, easing comparisons
- Sample infrastructure: Split samples, runner loop, GUI-based chooser, theme switcher, game menu
- GUI sample chooser: FishUI-based sample selector replacing console menu (self-dogfooding)
- Control demos: ImageBox, BarGauge, VirtualCursor, AnimatedImageBox, StaticText
- SampleGauges: Dedicated gauge demo with RadialGauge, BarGauge, VUMeter, car dashboard preview
- Sample auto-discovery via reflection (SampleDiscovery class)
- SampleImageBox: Dedicated ImageBox/AnimatedImageBox demo split from SampleBasicControls
- SampleDropDown: Dedicated DropDown demo (basic, searchable, multi-select, custom rendering)
- SampleListBox: Dedicated ListBox demo (alternating colors, multi-select, custom rendering)
- SampleBasicControls refactored: Focused on input controls (Textbox, Sliders, NumericUpDown, etc.)
- Sample chooser: Replaced button grid with ListBox for better scalability

### Code Cleanup
- ScreenCapture platform compatibility, YamlIgnore audit, naming conventions, XML docs, screenshot buttons, TODO extraction, debug flags refactor
- CS0108: Renamed Padding to LayoutPadding (FlowLayout, StackLayout) and MenuPadding (ContextMenu)
- CS0219/CS0414: Removed unused ToggleSwitch.useNPatch, ScrollablePane._scrollBarsCreated
- FishUIThemeLoader: Replaced custom YAML parser with YamlDotNet, added DTO classes for deserialization

### Documentation
- README.md: Complete rewrite with 35+ controls, code examples, theming, serialization, project structure

### Fixed Bugs
- DropDown: Overlay rendering, click-through fixes, AlwaysOnTop handling, mouse leave behavior
- Virtual cursor: Pick order, rendering order, arrow key handling
- Serialization: TreeNode/TabControl YamlIgnore, Window/TabControl OnDeserialized
- PropertyGrid: Inline editors sync values, auto-focus, fixed indentation and int conversion
- MenuBar: Dropdown menus now properly added to FishUI root for correct rendering
- ScrollablePane: Content scrolling via GetChildPositionOffset, DrawChildren override for proper scissoring
- ScrollBarV/H: Button positions now updated on resize in DrawControl
- Window: Increased resize handle size, content panel recalculates child anchors on size change
- ScrollablePane: Input restricted to visible area intersection with parent bounds
- Misc: ImageBox overlap, ScrollBar mouse wheel, ContextMenu submenu overlap