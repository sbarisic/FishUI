# FishUI - Completed Items

Consolidated list of completed features, improvements, and controls.

---

## Implemented Controls

| Control | Theme Support | Available in Editor |
|---------|---------------|---------------------|
| Button | ? Atlas | ? |
| CheckBox | ? Atlas | ? |
| RadioButton | ? Atlas | ? |
| Panel | ? Atlas | ? |
| Label | Colors | ? |
| Textbox | ? Atlas | ? |
| ListBox | ? Atlas | ? |
| DropDown (Combobox) | ? Atlas | ? |
| ScrollBarV | ? Atlas | ? |
| ScrollBarH | ? Atlas | ? |
| ProgressBar | ? Atlas | ? |
| Slider | ? Atlas | ? |
| ToggleSwitch | ? Atlas | ? |
| SelectionBox | ? Atlas | ? |
| Window | ? Atlas | ? |
| Titlebar | ? Atlas | ? |
| TabControl | ? Atlas | ? |
| GroupBox | ? Atlas | ? |
| TreeView | ? Atlas | ? |
| NumericUpDown | ? Atlas | ? |
| Tooltip | ? Atlas | ? |
| ContextMenu | ? Atlas | ? |
| MenuItem | ? Atlas | ? |
| StackLayout | N/A | ? |
| ImageBox | N/A | ? |
| StaticText | N/A | ? |
| BarGauge | N/A | ? |
| VUMeter | N/A | ? |
| AnimatedImageBox | N/A | ? |
| RadialGauge | N/A | ? |
| PropertyGrid | N/A | ? |
| MenuBar | ? Atlas | ? |
| ScrollablePane | N/A | ? |
| ItemListbox | ? Atlas | ? |
| FlowLayout | N/A | ? |
| GridLayout | N/A | ? |
| LineChart | N/A | ? |
| Timeline | N/A | ? |
| MultiLineEditbox | ? Atlas | ? |
| DatePicker | ? Atlas | ? |
| TimePicker | ? Atlas | ? |
| DataGrid | ? Atlas | ? |
| SpreadsheetGrid | ? Atlas | ? |
| BigDigitDisplay | N/A | ? |
| ToastNotification | ? Atlas | ? |
| FilePickerDialog | ? Atlas | ? |

---

## Control Features
- PropertyGrid, ContextMenu, MenuBar, MenuItem - Property editing and menu controls
- ImageBox, StaticText, VUMeter, AnimatedImageBox, RadialGauge, BarGauge - Display/gauge controls
- Timeline, LineChart - Time-based visualization controls
- MultiLineEditbox - Multi-line text editor with line numbers, selection, scrolling
- ScrollablePane, ItemListbox - Container and list controls
- DatePicker, TimePicker - Date/time selection controls
- DataGrid, SpreadsheetGrid - Grid/table controls
- BigDigitDisplay - Large text display for digital speedometer/RPM readouts
- ToastNotification - Auto-dismissing notifications with stacking, types, and titles
- FilePickerDialog - File browser dialog for Open/Save operations

## Control Improvements
- Button: Icon, Toggle, Repeat, ImageButton modes; IconPath serialization
- Panel: Border styles (Normal, Bright, Dark, Highlight)
- ListBox/DropDown: Multi-select, custom rendering, search/filter, alternating colors
- ImageBox: Pixelated filter mode, ImagePath serialization
- Input controls: Mouse wheel support (Slider, ScrollBars, NumericUpDown)
- LineChart: Pause/resume, vertical cursor with drag selection
- PropertyGrid: Vector2/3/4 editors, reset to default, collection editing
- MultiLineEditbox: Text selection, copy/cut/paste
- Gauges: Dashboard styling improvements
- SpreadsheetGrid: Heat map mode, cursor display with crosshair lines (ECU editor style)

## Core Framework
- Scalable UI via UIScale property
- Animation system with easing functions, tweens, control extensions
- Auto-sizing controls with Min/MaxSize constraints
- Layout controls: GridLayout, FlowLayout, StackLayout, Anchor, Margin/Padding
- Rendering: Opacity, fonts, image rotation/scaling, shadows
- Virtual cursor support
- Theme inheritance
- Serializable event handlers via EventHandlerRegistry
- RaylibGfx: UseBeginDrawing option for integration with existing game loops
- FishCSharpWriter utility class for C# code generation
- DesignerCodeGenerator for exporting layouts as .Designer.cs files implementing IFishUIForm
- SimpleFishUIGfx: Simplified base class for graphics backends with default implementations (reduces required overrides from 25+ to ~10)
- RaylibGfx2: Minimal Raylib backend example using SimpleFishUIGfx

## Samples
- Sample infrastructure: Runner loop, GUI chooser, theme switcher, auto-discovery
- Control demos: BasicControls, ImageBox, Gauges, DropDown, ListBox, LineChart, BigDigitDisplay, ToastNotification
- Feature demos: LayoutSystem, UIScaling, Animations, Serialization, EventSerialization
- Data controls: DatePicker, TimePicker, DataGrid, SpreadsheetGrid, MultiLineEditbox
- EditorLayout sample for testing FishUIEditor output
- DesignerForm sample demonstrating IFishUIForm designer-generated forms
- Theme persistence across samples
- Demo layout fixes: BasicControls, ImageBox, ListBox, SpreadsheetGrid, GameMenu (TabControl anchoring)

## FishUIEditor
- Phases 1-10: Core infrastructure through C# code generation
- PropertyGrid integration with all editable properties
- Control toolbox with drag-drop, hierarchy TreeView
- YAML serialization (New/Open/Save/Save As)
- Reparenting, anchors, Z-ordering, invisible control visualization
- Collection property editing (ListBox.Items, DropDown.Items, DataGrid.Columns, TabControl)
- Drop target feedback, nested control selection fixes
- Container selection mode, resize handle fixes, Window child positioning
- FilePickerDialog control for Open/Save As operations
- C# code export with .Designer.cs generation implementing IFishUIForm
- DrawControlEditor/DrawChildrenEditor for container and complex controls

## Code Cleanup
- Interface abstractions: IFishUIEvents, IFishUILogger, IFishUIFileSystem
- FishUIThemeLoader refactored to use YamlDotNet
- ScrollBar cleanup: Removed unused hover state fields and empty methods
- Code style fixes: Fixed indentation in LayoutFormat.cs, ScrollBarH.cs, ScrollBarV.cs
- Dead code removal: Program.cs unused import and commented-out variable
- Utils.Round() renamed to Truncate() for accuracy
- ScreenCapture: GetDesktopWindow made private for consistency
- Naming conventions, XML docs, warning fixes (CS0108, CS0219, CS0414)
- Generic FindControlByID<T>() method
- FishUITween API refactor: Animation methods moved to Control base class. Old extension methods marked obsolete.
- Control.cs refactored into partial classes: Control.Animation.cs, Control.Anchoring.cs, Control.AutoSize.cs, Control.Children.cs, Control.Drawing.cs, Control.Input.cs, Control.Position.cs
- SimpleFishUIGfx enhanced with default implementations: DrawRectangleOutline (4 lines), DrawCircle/DrawCircleOutline (polygon), DrawNPatch (9-slice from DrawImageRegion), PushScissor/PopScissor (stack-based), DrawText chain, LoadImage sub-regions, GetFontMetrics estimate

## Documentation
- README.md rewrite with examples, table of contents, updated control list
- docs/CUSTOM_CONTROLS.md and docs/THEMING.md guides
- docs/BACKEND_GUIDE.md - Custom graphics backend implementation guide with SimpleFishUIGfx examples
- XML doc comments on core classes

## Fixed Bugs
- Button/Label Text editable in PropertyGrid
- DropDown overlay rendering, click-through fixes
- Virtual cursor pick/render order
- PropertyGrid inline editors, indentation fixes
- MenuBar dropdown rendering
- ScrollablePane scrolling and input bounds
- Window resize handles, anchor recalculation
- Anchored control position preserved across save/load
- LineChart labels, DatePicker click-through
- ListBox scrollbar visibility on multi-select Ctrl+click
- GameMenu TabControl size corrected for window client area
- ListBox crash on empty list selection and event handler modifications
- FilePickerDialog centering
- Window child serialization with proper parent linking
- PropertyGrid enum dropdown showing all values
- DesignerCodeGenerator variable name consistency

