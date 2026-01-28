# FishUI - TODO

A list of planned features, improvements, and new controls for FishUI.

> **CPX (Complexity Points)** - 1 to 5 scale:
> - **1** - Single file control/component
> - **2** - Single file control/component with single function change dependencies
> - **3** - Multi-file control/component or single file with multiple dependencies, no architecture changes
> - **4** - Multi-file control/component with multiple dependencies and significant logic, possible minor architecture changes
> - **5** - Large feature spanning multiple components and subsystems, major architecture changes

> Instructions for the TODO list:
- Move all completed TODO items into a separate Completed section and simplify by consolidating/combining similar ones and shortening the descriptions where possible

> How TODO file should be iterated:
- First handle the Uncategorized section, if any similar issues already are on the TODO list, increase their priority instead of adding duplicates (categorize all at once)
- When Uncategorized section is empty, start by fixing Active Bugs (take one at a time)
- After Active Bugs, handle the rest of the TODO file by priority and complexity (High priority takes precedance, then CPX points) (take one at a time).

---

## Current Controls (Implemented)

| Control | Status | Theme Support | Available in Editor | Confirmed Working |
|---------|--------|---------------|---------------------|-------------------|
| Button | ✅ Complete | ✅ Atlas | ✅ | |
| CheckBox | ✅ Complete | ✅ Atlas | ✅ | |
| RadioButton | ✅ Complete | ✅ Atlas | ✅ | |
| Panel | ✅ Complete | ✅ Atlas | ✅ | |
| Label | ✅ Complete | Colors | ✅ | |
| Textbox | ✅ Complete | ✅ Atlas | ✅ | |
| ListBox | ✅ Complete | ✅ Atlas | ✅ | |
| DropDown (Combobox) | ✅ Complete | ✅ Atlas | ✅ | |
| ScrollBarV | ✅ Complete | ✅ Atlas | ❌ | |
| ScrollBarH | ✅ Complete | ✅ Atlas | ❌ | |
| ProgressBar | ✅ Complete | ✅ Atlas | ✅ | |
| Slider | ✅ Complete | ✅ Atlas | ✅ | |
| ToggleSwitch | ✅ Complete | ✅ Atlas | ✅ | |
| SelectionBox | ✅ Complete | ✅ Atlas | ❌ | |
| Window | ✅ Complete | ✅ Atlas | ✅ | |
| Titlebar | ✅ Complete | ✅ Atlas | ❌ | |
| TabControl | ✅ Complete | ✅ Atlas | ✅ | |
| GroupBox | ✅ Complete | ✅ Atlas | ✅ | |
| TreeView | ✅ Complete | ✅ Atlas | ✅ | |
| NumericUpDown | ✅ Complete | ✅ Atlas | ✅ | |
| Tooltip | ✅ Complete | ✅ Atlas | ❌ | |
| ContextMenu | ✅ Complete | ✅ Atlas | ❌ | |
| MenuItem | ✅ Complete | ✅ Atlas | ❌ | |
| StackLayout | ✅ Complete | N/A | ❌ | |
| ImageBox | ✅ Complete | N/A | ✅ | |
| StaticText | ✅ Complete | N/A | ✅ | |
| BarGauge | ✅ Complete | N/A | ✅ | |
| VUMeter | ✅ Complete | N/A | ✅ | |
| AnimatedImageBox | ✅ Complete | N/A | ❌ | |
| RadialGauge | ✅ Complete | N/A | ✅ | |
| PropertyGrid | ✅ Complete | N/A | ❌ | |
| MenuBar | ✅ Complete | ✅ Atlas | ❌ | |
| ScrollablePane | ✅ Complete | N/A | ✅ | |
| ItemListbox | ✅ Complete | ✅ Atlas | ❌ | |
| FlowLayout | ✅ Complete | N/A | ❌ | |
| GridLayout | ✅ Complete | N/A | ❌ | |
| LineChart | ✅ Complete | N/A | ❌ | |
| Timeline | ✅ Complete | N/A | ❌ | |
| MultiLineEditbox | ✅ Complete | ✅ Atlas | ❌ | |
| DatePicker | ✅ Complete | ✅ Atlas | ✅ | |
| TimePicker | ✅ Complete | ✅ Atlas | ✅ | |
| DataGrid | ✅ Complete | ✅ Atlas | ✅ | |
| SpreadsheetGrid | ✅ Complete | ✅ Atlas | ✅ | |
| BigDigitDisplay | ✅ Complete | N/A | ❌ | |
| ToastNotification | ✅ Complete | ✅ Atlas | ❌ | |

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

### SpreadsheetGrid Enhancements (CPX 2)
- [ ] Heat map mode - fixed size grid with color-coded cells based on values
- [ ] Cursor display - circle with vertical/horizontal crosshair lines (ECU editor style)

---

## Theme System

*All theme improvements completed - see Completed section*

---

## Core Framework Features

*All core framework features completed - see Completed section*

---

## Sample Application

*All sample demos completed - see Completed section*

---

## FishUIEditor - Layout Editor Application **HIGH PRIORITY**

A visual layout editor for designing FishUI interfaces. Located in the `FishUIEditor` project.

### Completed Phases

*Phases 1-7 completed - see Completed section (core infrastructure, selection/manipulation, property editor, control toolbox, serialization, reparenting, collection editing)*

### Phase 8: Editor Enhancements (CPX 2)
- [ ] File picker dialog - replace hardcoded paths with proper file picker dialogs for Open/Save As operations

### Phase 9: Editor Rendering Refactor (CPX 3)
- [ ] Implement `Control.DrawControlEditor` method in all supported controls - each control draws its own editor representation (e.g., Panel/Window draw child container rectangles)
- [ ] Refactor EditorCanvas "draw inert" to use `Control.DrawControlEditor` method instead of custom inert rendering logic

---

## Documentation **LOW PRIORITY**

- [ ] Backend implementation guide (beyond Raylib)

> **Note:** Getting started tutorial is covered in README.md Quick Start section
> **Note:** Custom control creation guide is in docs/CUSTOM_CONTROLS.md
> **Note:** Theme creation guide is in docs/THEMING.md

---

## Code Cleanup & Technical Debt

*All code cleanup and refactoring completed - see Completed section*

---

## Known Issues / Bugs

### Active Bugs

*No active bugs*

### Uncategorized (Analyze and create TODO entries in above appropriate sections with priority. Do not fix or implement them just yet. Assign complexity points where applicable. Do not delete this section when you are done, just empty it)

*No uncategorized items*

---

## Notes

- Try to edit files and use tools WITHOUT POWERSHELL where possible, shell scripts get stuck and then manually terminate
- Prioritize controls that are commonly needed in game development
- Maintain the "dependency-free" philosophy - keep the core library minimal
- Backend implementations should remain in separate sample projects
- Do not be afraid to break backwards compatibility if new changes will simplify or improve the project
- CEGUI theme files in `data/cegui_theme/` provide reference for accurate atlas coordinates
- The GWEN skin atlas (gwen.png) contains 512x512 pixels of UI elements
- Do not use powershell commands unless absolutely necessary

### Available Theme Assets in gwen.png (Reference)
- Windows: `Window.Head/Middle/Bottom.Normal/Inactive` (9-slice), Close button states
- Tabs: `Tab.Top/Bottom/Left/Right.Active/Inactive`, `Tab.Control`, `Tab.HeaderBar`
- Menus: `Menu.Strip`, `Menu.Background/Hover` (9-slice), `Menu.Check`, arrows
- Misc: `Tooltip`, `GroupBox.Normal`, `Tree` + Plus/Minus, `StatusBar`, `CategoryList`, `Shadow`, `Selection`

---

## Completed

### Controls
- PropertyGrid, ContextMenu, MenuBar, MenuItem - Property editing and menu controls
- ImageBox, StaticText, VUMeter, AnimatedImageBox, RadialGauge, BarGauge - Display/gauge controls
- Timeline, LineChart - Time-based visualization controls
- MultiLineEditbox - Multi-line text editor with line numbers, selection, scrolling
- ScrollablePane, ItemListbox - Container and list controls
- DatePicker, TimePicker - Date/time selection controls
- DataGrid, SpreadsheetGrid - Grid/table controls
- BigDigitDisplay - Large text display for digital speedometer/RPM readouts
- ToastNotification - Auto-dismissing notifications with stacking, types, and titles

### Control Improvements
- Button: Icon, Toggle, Repeat, ImageButton modes; IconPath serialization
- Panel: Border styles (Normal, Bright, Dark, Highlight)
- ListBox/DropDown: Multi-select, custom rendering, search/filter, alternating colors
- ImageBox: Pixelated filter mode, ImagePath serialization
- Input controls: Mouse wheel support (Slider, ScrollBars, NumericUpDown)
- LineChart: Pause/resume, vertical cursor with drag selection
- PropertyGrid: Vector2/3/4 editors, reset to default, collection editing
- MultiLineEditbox: Text selection, copy/cut/paste
- Gauges: Dashboard styling improvements

### Core Framework
- Scalable UI via UIScale property
- Animation system with easing functions, tweens, control extensions
- Auto-sizing controls with Min/MaxSize constraints
- Layout controls: GridLayout, FlowLayout, StackLayout, Anchor, Margin/Padding
- Rendering: Opacity, fonts, image rotation/scaling, shadows
- Virtual cursor support
- Theme inheritance
- Serializable event handlers via EventHandlerRegistry
- RaylibGfx: UseBeginDrawing option for integration with existing game loops

### Samples
- Sample infrastructure: Runner loop, GUI chooser, theme switcher, auto-discovery
- Control demos: BasicControls, ImageBox, Gauges, DropDown, ListBox, LineChart, BigDigitDisplay, ToastNotification
- Feature demos: LayoutSystem, UIScaling, Animations, Serialization, EventSerialization
- Data controls: DatePicker, TimePicker, DataGrid, SpreadsheetGrid, MultiLineEditbox
- EditorLayout sample for testing FishUIEditor output
- Theme persistence across samples
- Demo layout fixes: BasicControls, ImageBox, ListBox, SpreadsheetGrid, GameMenu (TabControl anchoring)

### FishUIEditor
- Phases 1-7: Core infrastructure, EditorCanvas, selection/manipulation, resize handles
- PropertyGrid integration with all editable properties
- Control toolbox with drag-drop, hierarchy TreeView
- YAML serialization (New/Open/Save/Save As)
- Reparenting, anchors, Z-ordering, invisible control visualization
- Collection property editing (ListBox.Items, DropDown.Items, DataGrid.Columns, TabControl)
- Drop target feedback, nested control selection fixes
- Container selection mode: Window/TabControl block internal children, Panel/GroupBox allow child selection
- Resize handle fix for nested controls (parent offset calculation)
- Window child positioning fix (content panel offset)

### Code Cleanup
- Interface abstractions: IFishUIEvents, IFishUILogger, IFishUIFileSystem
- FishUIThemeLoader refactored to use YamlDotNet
- Naming conventions, XML docs, warning fixes (CS0108, CS0219, CS0414)
- Generic FindControlByID<T>() method

### Documentation
- README.md rewrite with examples, table of contents, updated control list
- docs/CUSTOM_CONTROLS.md and docs/THEMING.md guides
- XML doc comments on core classes

### Fixed Bugs
- Button/Label Text editable in PropertyGrid
- DropDown overlay rendering, click-through fixes
- Virtual cursor pick/render order
- PropertyGrid inline editors, indentation fixes
- MenuBar dropdown rendering
- ScrollablePane scrolling and input bounds
- Window resize handles, anchor recalculation
- LineChart labels, DatePicker click-through
- ListBox scrollbar visibility on multi-select Ctrl+click
- GameMenu TabControl size corrected for window client area