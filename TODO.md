# FishUI - TODO

A list of planned features, improvements, and new controls for FishUI.

> **CPX (Complexity Points)** - 1 to 5 scale:
> - **1** - Single file control/component
> - **2** - Single file control/component with single function change dependencies
> - **3** - Multi-file control/component or single file with multiple dependencies, no architecture changes
> - **4** - Multi-file control/component with multiple dependencies and significant logic, possible minor architecture changes
> - **5** - Large feature spanning multiple components and subsystems, major architecture changes

> How TODO file should be iterated
- First handle the Uncategorized section, if any similar issues already are on the TODO list, increase their priority instead of adding duplicates
- When Uncategorized section is empty, start by fixing Active Bugs (take one at a time)
- After Active Bugs the rest of the TODO file by priority and complexity (High priority takes precedance, then CPX points) (take one at a time)
- Consolidate all completed TODO items by combining similar ones and shortening the descriptions where possible

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

- [ ] **MultiLineEditbox / TextArea** (3 CPX)
  - Multi-line text input
  - Word wrap support
  - Vertical scrollbar integration
  - *GWEN atlas regions: Uses TextBox regions*

- [ ] **ImageBox / StaticImage** (1 CPX)
  - Image display control
  - Scaling modes (stretch, fit, fill, none)
  - Click events
  - *GWEN WidgetLook: StaticImage*

- [ ] **StaticText** (1 CPX)
  - Non-editable formatted text display
  - Text alignment options
  - *GWEN WidgetLook: StaticText*

- [ ] **VUMeter** (2 CPX)
  - Audio level visualization
  - Horizontal/vertical variants
  - Peak hold indicator
  - *GWEN WidgetLook: VUMeter (uses ProgressBar renderer)*

- [ ] **AnimatedImageBox** (2 CPX)
  - Frame sequences are stored as an array of images
  - Configurable frame rate
  - Ability to pause animation display static frame
  - Play/pause control

- [ ] **DateTimePicker** (4 CPX)
  - Calendar popup for date selection
  - Optional time selection spinner
  - Configurable date formats

- [ ] **ItemListbox** (2 CPX)
  - Listbox with custom item widgets
  - *GWEN WidgetLook: ItemListbox*

- [x] ~~**MenuItem**~~ - Individual menu item with text, shortcut display, checkbox states (implemented with ContextMenu)

- [ ] **RadialGauge** (3 CPX)
  - Circular gauge display (e.g., RPM, speedometer)
  - Configurable min/max values and range angle
  - Needle/pointer rendering
  - Tick marks and labels
  - *Use case: Dashboard-style data visualization*

- [ ] **BarGauge** (2 CPX)
  - Linear gauge display (horizontal or vertical)
  - Configurable min/max values
  - Color zones (e.g., green/yellow/red for temperature)
  - Tick marks and labels
  - *Use case: Temperature, fuel level, progress indicators*

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
- [ ] Even/odd row alternating colors (1 CPX)
  - *GWEN has: Input.ListBox.EvenLine, Input.ListBox.OddLine, Input.ListBox.EvenLineSelected, Input.ListBox.OddLineSelected*

### Button Enhancements
- [ ] Icon support (image + text) (1 CPX)
- [ ] Toggle button mode (1 CPX)
- [ ] Repeat button mode (fires while held) (1 CPX)
- [ ] ImageButton variant (icon-only button) (1 CPX)
  - *GWEN WidgetLook: ImageButton*

### Panel Enhancements
- [ ] Border styles (1 CPX)
- [ ] Panel variants (Normal, Bright, Dark, Highlight) (1 CPX)
  - *GWEN has: Panel.Normal, Panel.Bright, Panel.Dark, Panel.Highlight*

### ProgressBar Enhancements
- [ ] Vertical progress bar mode (1 CPX)
  - *GWEN has: VProgressBar regions for vertical variant*

### Slider Enhancements
- [x] ~~Mouse wheel support~~ - Scroll to adjust value (uses Step or 1% of range)

### ScrollBar Enhancements
- [ ] Mouse wheel support for ScrollBar buttons (1 CPX)
  - Allow scrolling with mouse wheel over ScrollBarV/ScrollBarH button areas

### NumericUpDown Enhancements
- [x] ~~Mouse wheel support~~ - Scroll to increment/decrement value

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
- [ ] **StackLayout** - Vertical/horizontal stacking (2 CPX) **HIGH PRIORITY**
- [ ] Anchor system for responsive resizing (4 CPX) **HIGH PRIORITY**
- [ ] Margin and Padding properties on all controls (3 CPX) **HIGH PRIORITY**
- [ ] Auto-sizing controls based on content (3 CPX)

### Rendering
- [ ] Control opacity/transparency (2 CPX)
- [ ] Animation system for transitions (4 CPX)
- [ ] Anti-aliased rendering option (2 CPX)
- [ ] Shadow rendering for windows/popups (2 CPX)
  - *GWEN has: Shadow region*
- [ ] Font system refactoring (3 CPX) **HIGH PRIORITY**
  - Support for both monospaced and variable-width fonts
  - Font metrics and proper text measurement
  - Font style variants (regular, bold, italic)
- [ ] Rendering images with rotation and scaling (useful for gauges) (TODO: estimate CPX after analysis) **HIGH PRIORITY**

### Accessibility
- [ ] Keyboard-only navigation (3 CPX)
- [ ] Scalable UI for different DPI (4 CPX)

### Serialization
- [x] ~~LayoutFormat TypeMapping~~ - Added all implemented controls for layout save/load

---

## Sample Application

- [ ] Examples should be implemented in FishUISample project, using the ISample interface (2 CPX each)
- [ ] Implement sample chooser system in Program.cs (2 CPX)
  - Console-based or UI-based selection to choose which sample to run
  - Currently hardcoded to `Samples[2]` in Program.cs:54
- [x] ~~Theme switcher example~~ (SampleThemeSwitcher.cs)
- [x] ~~Game main menu example~~ (SampleGameMenu.cs) - Buttons + Options window with tabs
- [ ] Car dashboard example with gauges
- Requires RadialGauge and BarGauge controls first
- [ ] MultiLine textbox example (1 CPX)
- Add below existing single-line textbox sample
- Requires multi-line textbox mode implementation

### SampleDefault Improvements
- [x] ~~Replace main Panel with Window control~~ - Changed `panel1` from Panel to Window with hidden close button
- [x] ~~Add context menu to SampleDefault~~ - Demonstrates ContextMenu with items, separators, checkable items, and submenus

---

## Documentation

- [ ] Update complete README.md from scratch 
- [ ] API reference documentation
- [ ] Getting started tutorial
- [ ] Custom control creation guide
- [ ] Backend implementation guide (beyond Raylib)
- [ ] Theme creation guide

---

## Code Cleanup & Technical Debt **HIGH PRIORITY**
- [ ] Audit all controls for missing `[YamlIgnore]` attributes (2 CPX)
  - Check read-only properties that could cause deserialization errors
  - Similar to fixed bugs: TreeNode.HasChildren, TabControl.SelectedTab
- [x] ~~Standardize naming conventions~~ - Converted fields to properties: CheckBox/RadioButton `Checked`→`IsChecked`, Panel `IsTransparent`, ListBox `ShowScrollBar`
- [ ] Add XML documentation comments to public APIs (2 CPX)
- [ ] Add screenshot button to all examples (2 CPX)
- Add a button to each sample that triggers screenshot capture
- Forward `AutoScreenshot` method from Program.cs through ISample interface **HIGH PRIORITY**
- Update SampleThemeSwitcher, SampleGameMenu, and SampleDefault with screenshot buttons
- [x] ~~Extract TODO comments from code~~ - Moved 4 TODOs to proper sections, removed comments from source
- [ ] Move DebugLogTooltips flag into FishUIDebug class (1 CPX)
  - Currently in FishUISettings, should be in FishUIDebug like other debug flags

---

## Known Issues / Bugs

### Active Bugs

- [ ] **ContextMenu submenu text overlapping** (2 CPX)
  - Multiple levels of submenu draw all the text overlapping
  - Found in SampleDefault.cs context menu demo

### Fixed Bugs

- [x] ~~TreeNode.HasChildren deserialization error~~ - Added `[YamlIgnore]` to read-only property
- [x] ~~TabControl.SelectedTab deserialization error~~ - Added `[YamlIgnore]` to read-only property
- [x] ~~DropDown.SelectIndex NullReferenceException~~ - Added null check before `FishUI.Events.Broadcast()`
- [x] ~~Make Invisible/Visible buttons broken~~ - Changed `Panel` cast to `Control` in EvtHandler after Panel→Window conversion

### Uncategorized (Analyze and create TODO entries in above appropriate sections with priority. Do not fix or implement them just yet. Assign complexity points where applicable. Do not delete this section when you are done, just empty it)

*No uncategorized items*

---

## Notes

- Prioritize controls that are commonly needed in game development
- Maintain the "dependency-free" philosophy - keep the core library minimal
- Backend implementations should remain in separate sample projects
- CEGUI theme files in `data/cegui_theme/` provide reference for accurate atlas coordinates
- The GWEN skin atlas (gwen.png) contains 512x512 pixels of UI elements