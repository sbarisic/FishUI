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
| LineChart | ✅ Complete | N/A |
| Timeline | ✅ Complete | N/A |
| MultiLineEditbox | ✅ Complete | ✅ Atlas |
| DatePicker | ✅ Complete | ✅ Atlas |
| TimePicker | ✅ Complete | ✅ Atlas |
| DataGrid | ✅ Complete | ✅ Atlas |
| SpreadsheetGrid | ✅ Complete | ✅ Atlas |

---

## New Controls

### High Priority

*All high priority controls have been implemented*

### Medium Priority

*All medium priority controls have been implemented*

### Lower Priority

*All lower priority controls have been implemented*

---

## Control Improvements

*All control improvements have been completed - see Completed section*

---

## Theme System

### Theme File Improvements

*Theme file improvements completed - see Completed section*

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

*Scalable UI completed - see Completed section*

### Serialization

*Serializable event handlers completed - see Completed section*

---

## Sample Application

### Sample Improvements (CPX 2) - COMPLETE
- [x] Persist selected theme across samples - save theme preference to file and restore on sample chooser startup

---

## FishUIEditor - Layout Editor Application **HIGH PRIORITY**

A visual layout editor for designing FishUI interfaces. Located in the `FishUIEditor` project.

### Phase 1: Core Infrastructure (CPX 4) - COMPLETE
- [x] Create inert control rendering mode - controls display visually but don't process input events
- [x] Implement EditorCanvas control for displaying the design surface with grid, selection, resize handles
- [x] Set up basic editor window layout with menu bar, toolbox, PropertyGrid panel, status bar

### Phase 2: Selection & Manipulation (CPX 4) - COMPLETE
- [x] Implement control selection (click to select, selection highlight/handles)
- [x] Add drag-and-drop movement for selected controls
- [x] Implement resize handles for selected controls

### Phase 3: Property Editor (CPX 3) - COMPLETE
- [x] Integrate PropertyGrid on right panel for editing selected control properties
- [x] Expose all editable properties (Button.Text, Label.Text, etc.) in PropertyGrid
- [x] Support "reset to default" for property changes (right-click context menu)

### Phase 4: Control Toolbox (CPX 3) - COMPLETE
- [x] Create control palette/toolbox panel listing available controls
- [x] Click toolbox item to add control to design surface
- [x] Drag controls from toolbox to design surface to create new instances
- [x] Support control hierarchy (parenting controls)
- [x] Add Layout Hierarchy TreeView showing all controls with parent/child nesting

### Phase 5: Serialization (CPX 2) - COMPLETE
- [x] Save layouts to YAML files using LayoutFormat
- [x] Load existing YAML layouts for editing
- [x] New/Open/Save/Save As file operations (via MenuBar dropdown MenuItem clicks; default path `data/layouts/editor_layout.yaml`)

### Phase 6: Reparenting & Advanced Manipulation (CPX 3) - COMPLETE
- [x] Drag existing control onto container to reparent
- [x] Drag control out of container to unparent (move to root level)
- [x] Anchor support in editor (controls move/stretch correctly when parent is resized)
- [x] Z-ordering support (controls render and pick in correct depth order)
- [x] Invisible control visualization (pink outline with diagonal lines for Visible=false controls)
- [x] Visual feedback when hovering over valid drop targets (green highlight with label)

### Follow-ups

*All follow-ups completed - see Completed section*

### Phase 7: Collection Property Editing (CPX 3) - COMPLETE
- [x] Add PropertyGrid support for editing collection properties (List<T>, arrays)
- [x] ListBox.Items editing - add/remove/reorder items in PropertyGrid
- [x] DropDown.Items editing - add/remove/reorder items in PropertyGrid
- [x] DataGrid.Columns editing - add/remove/configure columns
- [x] TabControl tabs editing - add/remove/rename tabs

### Phase 8: Editor Enhancements (CPX 2)
- [ ] Container selection mode - when selecting a container control (Window, ListBox, Panel, etc.), child controls should not be directly clickable on canvas; children should only be selectable via the Hierarchy TreeView

---

## Documentation **LOW PRIORITY**

- [x] API reference documentation (XML doc comments and/or generated docs)
- [ ] Backend implementation guide (beyond Raylib)

> **Note:** Getting started tutorial is covered in README.md Quick Start section
> **Note:** Custom control creation guide is in docs/CUSTOM_CONTROLS.md
> **Note:** Theme creation guide is in docs/THEMING.md

---

## Code Cleanup & Technical Debt

### External Dependency Abstraction

*All external dependency abstraction items have been completed - see Completed section*

### Code Refactoring **HIGH PRIORITY**

*All code refactoring items have been completed - see Completed section*

---

## Known Issues / Bugs

### Active Bugs

*No active bugs*

### Uncategorized (Analyze and create TODO entries in above appropriate sections with priority. Do not fix or implement them just yet. Assign complexity points where applicable. Do not delete this section when you are done, just empty it)

- Themes do not get loaded in sample windows that launch after the sample chooser

---

## Notes

- Prioritize controls that are commonly needed in game development
- Maintain the "dependency-free" philosophy - keep the core library minimal
- Backend implementations should remain in separate sample projects
- Do not be afraid to break backwards compatibility if new changes will simplify or improve the project
- CEGUI theme files in `data/cegui_theme/` provide reference for accurate atlas coordinates
- The GWEN skin atlas (gwen.png) contains 512x512 pixels of UI elements
- Do not use powershell commands unless absolutely necessary

---

## Completed

### Controls
- PropertyGrid - Windows Forms-like property editor with categorization, reflection-based editing
- ContextMenu / PopupMenu - Right-click menus with submenus, keyboard navigation
- MenuBar - Horizontal menu strip with dropdown menus, hover switching, submenus
- ImageBox, StaticText, VUMeter, AnimatedImageBox - Display controls
- Timeline - Time range navigation control with draggable view window, pan/resize, LineChart companion
- MultiLineEditbox - Multi-line text editor with line numbers, smooth pixel-based scrolling, ScrollBarV integration, cursor navigation, read-only mode
- MenuItem, RadialGauge, BarGauge - Menu items and gauge controls
- ScrollablePane - Container with automatic scrollbars and virtual scrolling
- ItemListbox - Listbox with custom item widgets
- LineChart - Real-time line chart with multiple data series, configurable time window, grid, and axis labels
- DatePicker - Calendar popup with month/year navigation, date format configuration, min/max date range
- TimePicker - Time selection with hour/minute/second spinners, 12/24-hour format, mouse wheel support
- DataGrid - Multi-column list with sortable headers, resizable columns, single/multi row selection, scrolling
- SpreadsheetGrid - Excel-like grid with editable cells, column headers (A,B,C...), row headers (1,2,3...), keyboard navigation

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
- LineChart: IsPaused property with Pause()/Resume() methods
- LineChart: Vertical cursor with draggable selection, data point interpolation, OnCursorMoved event
- BarGauge: Black tick marks, dashboard styling (larger size, range labels)
- RadialGauge: Black text/ticks, wider draw angle (~260° sweep)
- PropertyGrid: Vector2/Vector3/Vector4 component editors with X/Y/Z/W fields
- PropertyGrid: Reset to default via right-click context menu with default value capture
- Timeline: White label color for dark backgrounds
- Button/ImageBox: IconPath/ImagePath properties for serialization with OnDeserialized image loading
- MultiLineEditbox: Line numbers moved 4px left for better spacing
- MultiLineEditbox: Text selection support with highlighting, Shift+arrow key selection, click/drag to select, Copy/Cut/Paste, visual selection highlighting

### Core Framework
- Scalable UI - UIScale property in FishUISettings (default 1.0), Scale helper methods, all controls respect scaling factor via GetAbsolutePosition/GetAbsoluteSize
- Animation system - Easing functions (14 types), FishUIAnimationManager, tween helpers, Control extensions (FadeIn/Out, SlideIn/Out, AnimatePosition/Size/Opacity, ScaleBounce)
- Auto-sizing controls - AutoSizeMode (None/Width/Height/Both), GetPreferredSize, Min/MaxSize constraints
- GridLayout - Grid-based child positioning with configurable columns, spacing, and stretch options
- FlowLayout - Automatic horizontal/vertical flow with wrapping (4 directions, 3 wrap modes)
- StackLayout, Anchor, Margin/Padding - Layout infrastructure
- Opacity, fonts, image rotation/scaling - Rendering features
- Shadow rendering: Window and ContextMenu shadow support with configurable offset and size
- Virtual cursor: Position setting, input mapping, hybrid mode
- LayoutFormat TypeMapping - All controls registered
- Theme inheritance: Themes can inherit from parent themes via `inherits` property, child themes only override specific values
- Serializable Event Handlers: EventHandlerRegistry for named handlers, OnClickHandler/OnValueChangedHandler/OnSelectionChangedHandler/OnTextChangedHandler/OnCheckedChangedHandler properties on controls, handlers invoked automatically on deserialized layouts

### Samples
- SampleLayoutSystem: Updated to use Window controls for Anchors, StackLayout, FlowLayout, and GridLayout demos
- SampleUIScaling: UI scaling demo with interactive scale slider, preset buttons, various controls
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
- Sample chooser: Replaced button grid with ListBox for better scalability, added theme selector dropdown with persistence
- SampleLineChart: Real-time data visualization with multiple series, sine wave, noise, temperature monitor, 4th slow series (1.5s interval), wider chart layout
- SampleMultiLineEditbox: Multi-line text editor demo with editable editor, read-only log viewer, simple editor examples
- SampleSerialization: Layout serialization demo showing IconPath/ImagePath properties for image preservation
- SampleDatePicker: DatePicker demo with date formats, range restrictions, quick-set buttons
- SampleTimePicker: TimePicker demo with 12/24-hour formats, seconds display, quick-set buttons
- SampleDataGrid: DataGrid demo with employee database, multi-select, sorting, column resize
- SampleSpreadsheetGrid: SpreadsheetGrid demo with sales data, cell editing, navigation, Clear/Fill actions
- SampleEventSerialization: Serializable event handlers demo with named handlers for clicks, value changes, selections, text changes
- SampleEditorLayout: Loads and renders layouts from FishUIEditor (data/layouts/editor_layout.yaml), fallback to instructions if missing
- Theme persistence: ThemePreferences class saves/loads theme path to file, applied on sample chooser startup

### Code Cleanup
- ScreenCapture platform compatibility, YamlIgnore audit, naming conventions, XML docs, screenshot buttons, TODO extraction, debug flags refactor
- CS0108: Renamed Padding to LayoutPadding (FlowLayout, StackLayout) and MenuPadding (ContextMenu)
- CS0219/CS0414: Removed unused ToggleSwitch.useNPatch, ScrollablePane._scrollBarsCreated
- FishUIThemeLoader: Replaced custom YAML parser with YamlDotNet, added DTO classes for deserialization
- Generic FindControlByID<T>() method for strongly-typed control lookup
- Event System Refactoring: IFishUIEvents with specialized methods (OnControlClicked, OnControlValueChanged, OnControlSelectionChanged, etc.), FishUIEventArgs classes, Control events (Clicked, DoubleClicked, MouseEnter, MouseLeave), OnLayoutLoaded event
- Logging Interface Abstraction: IFishUILogger interface, DefaultFishUILogger, NullFishUILogger, FishUIDebug.Logger property for custom logging
- File System Interface Abstraction: IFishUIFileSystem interface, DefaultFishUIFileSystem, FishUI.FileSystem property for virtual file systems

### Documentation
- README.md: Complete rewrite with 35+ controls, code examples, theming, serialization, project structure
- Getting started tutorial: Covered in README.md Quick Start section (interface implementation, initialization, controls, update loop)
- Custom control creation guide: docs/CUSTOM_CONTROLS.md with full examples, input handling, drawing, events, theming
- Theme creation guide: docs/THEMING.md with YAML structure, colors, fonts, atlas regions, 9-slice, inheritance
- API reference documentation: XML doc comments enabled in FishUI.csproj, core classes and interfaces documented (FishUI, Control, IFishUIGfx, IFishUIInput, IFishUIEvents, IFishUIFileSystem, IFishUILogger, LayoutFormat, EventHandlerRegistry, FishUISettings, FishUIAnimation)

### Fixed Bugs
- Button/Label: Text property now editable in PropertyGrid (converted from field to property with [YamlMember])
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
- LineChart: Black labels (was light gray), increased MaxPoints (5000), throttled sample rate in demo, X-axis now uses 2 decimals (F2)
- DatePicker: Fixed click-through and selection issues via IsPointInside override for calendar popup bounds

### FishUIEditor Fixes
- EditorCanvas: Fixed reparenting - RemoveChild now properly clears Parent reference, added ClearParent() method
- EditorCanvas: Fixed nested control selection highlight position for deeply nested controls (Button in Panel in Window)
- EditorCanvas: Fixed anchor offset sync - controls now call UpdateOwnAnchorOffsets() after move/resize
- EditorCanvas: Fixed selection box not updating when parent is resized - now uses GetAnchorAdjustedRelativePosition/Size
- EditorCanvas: Fixed mouse picking for anchor-adjusted controls - hit testing uses anchor-adjusted bounds
- EditorCanvas: Added Z-ordering support - controls render and pick in ZDepth order
- EditorCanvas: Added drop target visual feedback - green highlight with label when hovering over valid containers during drag
- Control: Added GetAnchorAdjustedRelativePosition() for anchor-adjusted position without UI scaling
- Control: Added GetAnchorAdjustedSize() for anchor-adjusted size (stretching when anchored to opposite edges)
- PropertyGrid: Collection property editing for ListBox.Items, DropDown.Items, DataGrid.Columns, TabControl.TabNames (add/remove/reorder/edit), exposed DropDown.Items and DataGrid.Columns as public properties, TabNames syncs to TabPages at runtime