# FishUI - TODO

A list of planned features, improvements, and new controls for FishUI.

> **CPX (Complexity Points)** - 1 to 5 scale:
> - **1** - Single file control/component
> - **2** - Single file control/component with single function change dependencies
> - **3** - Multi-file control/component or single file with multiple dependencies, no architecture changes
> - **4** - Multi-file control/component with multiple dependencies and significant logic, possible minor architecture changes
> - **5** - Large feature spanning multiple components and subsystems, major architecture changes

> How TODO file should be iterated
- First handle the Uncategorized section
- When Uncategorized section is empty, start by fixing Active Bugs (take one at a time)
- After Active Bugs the rest of the TODO file by priority and complexity (High priority takes precedance, then CPX points) (take one at a time)

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

---

## New Controls

### High Priority

*All high priority controls have been implemented*

### Medium Priority

- [ ] **ContextMenu / PopupMenu** (3 CPX)
  - Right-click context menus
  - Menu item with icons
  - Submenu support
  - Keyboard navigation
  - Check mark support for toggleable items
  - *GWEN atlas regions available: Menu.Background, Menu.Hover, Menu.Check, Menu.RightArrow, Menu.LeftArrow*

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

- [ ] **MenuItem** (1 CPX)
  - Individual menu item for menus
  - Icon, text, shortcut display
  - Checkbox/radio states
  - *GWEN WidgetLook: MenuItem*

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

### NumericUpDown Enhancements
- [ ] Mouse wheel support (1 CPX)
  - Scroll up to increment, scroll down to decrement value

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
- [ ] **StackLayout** - Vertical/horizontal stacking (2 CPX)
- [ ] Anchor system for responsive resizing (4 CPX)
- [ ] Margin and Padding properties on all controls (3 CPX)
- [ ] Auto-sizing controls based on content (3 CPX)

### Rendering
- [ ] Control opacity/transparency (2 CPX)
- [ ] Animation system for transitions (4 CPX)
- [ ] Anti-aliased rendering option (2 CPX)
- [ ] Shadow rendering for windows/popups (2 CPX)
  - *GWEN has: Shadow region*
- [ ] Font system refactoring (3 CPX) ** MEDIUM PRIORITY **
  - Support for both monospaced and variable-width fonts
  - Font metrics and proper text measurement
  - Font style variants (regular, bold, italic)

### Accessibility
- [ ] Keyboard-only navigation (3 CPX)
- [ ] Scalable UI for different DPI (4 CPX)

### Serialization
- [ ] Add new controls to LayoutFormat TypeMapping (1 CPX) ** HIGH PRIORITY **
  - Missing: Window, Titlebar, TabControl, GroupBox, TreeView, NumericUpDown, Tooltip
  - Without these mappings, layout save/load will fail for these controls
  - *Blocks: SampleDefault layout save/load functionality*

---

## Sample Application

- [ ] Examples should be implemented in FishUISample project, using the ISample interface (2 CPX each)
- [ ] Theme switcher example ** MEDIUM PRIORITY **
- Demonstrate runtime theme switching between gwen.yaml and gwen2.yaml
- [ ] Responsive layout example
- Show anchor/dock positioning with window resize
- [ ] Game main menu example ** MEDIUM PRIORITY **
- New Game, Options, Quit buttons
- Options shows a window with tabs: Input, Graphics, Gameplay
- [ ] Car dashboard example with gauges
- Requires RadialGauge and BarGauge controls first
- [ ] MultiLine textbox example (1 CPX)
- Add below existing single-line textbox sample
- Requires multi-line textbox mode implementation

### SampleDefault Improvements
- [ ] Replace main Panel with Window control (1 CPX)
  - Change `panel1` from Panel to Window with disabled close button
  - *Depends on: Add new controls to LayoutFormat TypeMapping*

---

## Documentation

- [ ] API reference documentation
- [ ] Getting started tutorial
- [ ] Custom control creation guide
- [ ] Backend implementation guide (beyond Raylib)
- [ ] Theme creation guide
- [ ] CEGUI theme conversion guide

---

## Code Cleanup & Technical Debt
- [ ] Standardize naming conventions across all controls (1 CPX)
- [ ] Add XML documentation comments to public APIs (2 CPX)
- [ ] Add a button to examples to tkae a screenshot with AutoScreenshot code in Program.cs (1 CPX)
  - Forward screenshot method trough ISample interface

---

## Known Issues / Bugs

### Active Bugs
- [ ] **Textbox HasSelection property serialization error** (1 CPX)
  - YamlDotNet.Core.YamlException: 'Property 'HasSelection' not found on type 'FishUI.Controls.Textbox'.'
  - Occurs when serializing/deserializing layouts containing Textbox controls
  - Need to add [YamlIgnore] attribute to HasSelection property or make it serializable

### Uncategorized (Analyze and create TODO entries in above appropriate sections with priority. Do not fix or implement them just yet. Assign complexity points where applicable. Do not delete this section when you are done, just empty it)

---

## Notes

- Prioritize controls that are commonly needed in game development
- Maintain the "dependency-free" philosophy - keep the core library minimal
- Backend implementations should remain in separate sample projects
- CEGUI theme files in `data/cegui_theme/` provide reference for accurate atlas coordinates
- The GWEN skin atlas (gwen.png) contains 512x512 pixels of UI elements