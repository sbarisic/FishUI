# FishUI - TODO

A list of planned features, improvements, and new controls for FishUI.

> **CPX (Complexity Points)** - 1 to 5 scale:
> - **1** - Single file control/component
> - **2** - Single file control/component with single function change dependencies
> - **3** - Multi-file control/component or single file with multiple dependencies, no architecture changes
> - **4** - Multi-file control/component with multiple dependencies and significant logic, possible minor architecture changes
> - **5** - Large feature spanning multiple components and subsystems, major architecture changes

How TODO file should be iterated: First handle the Uncategorized section, when this section is empty, start by fixing Active Bugs (take one at a time). After that the rest of the TODO file by priority (take one at a time).

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

---

## New Controls

### High Priority

- [x] **TabControl / TabPanel** (3 CPX) ✅ Implemented
  - Tab header strip with clickable tabs
  - Content panels that switch based on selected tab
  - Tab overflow handling (scroll with left-right arrows) - *TODO*
  - Top/bottom tab positioning - *TODO*
  - *GWEN atlas regions available: Tab.Top.Active, Tab.Top.Inactive, Tab.Bottom.Active, Tab.Bottom.Inactive, Tab.Control, Tab.HeaderBar*

- [x] **Window / FrameWindow / Dialog** (4 CPX) ✅ Implemented
  - Draggable title bar
  - Close button (optional minimize/maximize) - minimize/maximize *TODO*
  - Modal and non-modal modes - modal blocking *TODO*
  - Resizable and fixed border mode
  - *GWEN atlas regions available: Window.Head.Normal, Window.Head.Inactive, Window.Middle, Window.Bottom, Window.Close*

- [x] **Titlebar** (2 CPX) ✅ Implemented
  - Standalone titlebar component for windows
  - Text display with optional icon - icon *TODO*
  - *GWEN atlas regions available: Window.Head.Normal, Window.Head.Inactive*

### Medium Priority

- [ ] **NumericUpDown / Spinner** (2 CPX)
  - Textbox with up/down increment buttons
  - Min/max/step value constraints
  - *GWEN atlas regions available: Input.UpDown.Up, Input.UpDown.Down*

- [x] **TreeView / Tree** (4 CPX) ✅ Implemented
  - Hierarchical node display
  - Expand/collapse functionality
  - Node selection with keyboard navigation
  - Lazy loading support for large trees
  - *GWEN atlas regions available: Tree.Top, Tree.Middle, Tree.Bottom, Tree.Plus, Tree.Minus*

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

- [ ] **Tooltip** (2 CPX)
  - Hover-triggered popup text
  - Configurable delay and duration
  - Multi-line support
  - *GWEN atlas regions available: Tooltip.Top, Tooltip.Middle, Tooltip.Bottom*

- [x] **GroupBox** (1 CPX) ✅ Implemented
- Labeled container with border
- Title positioning options
- *GWEN atlas regions available: GroupBox.Normal*

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
- [x] Cursor positioning with mouse click (2 CPX) ✅ Implemented
- [x] Text selection (click and drag) (2 CPX) ✅ Implemented
- [x] Copy/paste support (Ctrl+C, Ctrl+V, exposed as functions on a control) (2 CPX) ✅ Implemented
- [x] Select all (Ctrl+A, exposed as a function) (1 CPX) ✅ Implemented
- [ ] Multi-line mode with character wrap (3 CPX)
- [x] Password masking mode (1 CPX) ✅ Implemented
- [x] Placeholder text (1 CPX) ✅ Implemented
- [x] Max length constraint (1 CPX) ✅ Implemented
- [x] Read-only mode (1 CPX) ✅ Implemented

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

---

## Theme System

### Current State
- [x] Theme loading from YAML files
- [x] Sprite atlas support (gwen.png)
- [x] NPatch/9-slice rendering
- [x] Region coordinates from CEGUI imageset format
- [x] Runtime theme switching
- [x] Color palette support

### Theme File Improvements
- [x] Add missing GWEN atlas regions to gwen.yaml (2 CPX) ✅ Completed
  - Window/FrameWindow regions (HeadNormal, HeadInactive, MiddleNormal, MiddleInactive, BottomNormal, BottomInactive, Close buttons)
  - Tab regions (HeaderBar, ControlBackground, TopActive, TopInactive)
  - Menu regions (Strip, Background, Hover, RightArrow, LeftArrow)
  - Tooltip regions
  - GroupBox regions
  - Tree regions (Background, Plus, Minus)
  - NumericUpDown regions (Up/Down button states)
- [ ] Theme inheritance / base themes (3 CPX)
- [ ] Per-control color overrides (2 CPX)
- [ ] Support CEGUI imageset.xml format directly (3 CPX)

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

### Core system
- [x] Z-order management for overlapping controls (3 CPX) ✅ Implemented
	- BringToFront/SendToBack methods on Control base class
	- AlwaysOnTop property for controls that should always render on top
	- Modal windows blocking input to background controls via SetModalControl/ShowModal
	- Auto-incrementing Z-depth assignment on AddControl
	- Mouse click automatically brings root control to front

### Layout System
- [ ] **FlowLayout** - Automatic horizontal/vertical flow of children (3 CPX)
- [ ] **GridLayout** - Grid-based child positioning (3 CPX)
- [ ] **StackLayout** - Vertical/horizontal stacking (2 CPX)
- [ ] Anchor system for responsive resizing (4 CPX)
- [ ] Margin and Padding properties on all controls (3 CPX)
- [ ] Auto-sizing controls based on content (3 CPX)

### Input & Events
- [x] Mouse wheel scrolling support
- [x] Double-click event handling
- [x] Keyboard focus navigation (Tab key)
- [x] Focus visual indicators
- [x] Global keyboard shortcuts / hotkeys
- [x] Virtual mouse cursor mode

### Rendering
- [ ] Control opacity/transparency (2 CPX)
- [ ] Animation system for transitions (4 CPX)
- [ ] Anti-aliased rendering option (2 CPX)
- [ ] Shadow rendering for windows/popups (2 CPX)
  - *GWEN has: Shadow region*
- [ ] Font system refactoring (3 CPX)
  - Support for both monospaced and variable-width fonts
  - Font metrics and proper text measurement
  - Font style variants (regular, bold, italic)

### Accessibility
- [ ] Keyboard-only navigation (3 CPX)
- [ ] Scalable UI for different DPI (4 CPX)

### Serialization
- [ ] YAML layout format support (2 CPX)

---

## Sample Application

- [x] Complete demo showcasing all controls (2 CPX) - *Partially complete, includes new Window/Tab/GroupBox controls*
- [ ] Examples should be implemented in FishUISample project, using the ISample interface
	- [ ] Theme switcher example
	- [ ] Responsive layout example
	- [ ] Game main menu example, New Game, Options, Edit. Options shows a window with tabs Input, Graphics, Gameplay. 
	- [ ] Car dashboard example with gauges
	- [ ] MultiLine textbox example (1 CPX)
	  - Add below existing single-line textbox sample

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

### Completed Cleanup
- [x] Implement `FishUI.Resized()` method
- [x] Implement or remove `Padding` property in `Control.cs`
- [x] Remove unused `UICommands.cs` and `UICommandList.cs`
- [x] Complete `Panel` hover/pressed states
- [x] Remove old commented code in `FishUI.cs`, `Control.cs`, `ScrollBarV.cs`
- [x] Centralize debug logging mechanism (`FishUIDebug`)
- [x] Implement debug outline drawing for all controls
- [x] Remove unused `using` statements
- [x] Fix `Utils.Union` scissor intersection calculation (was commented out)
- [x] Fix `RaylibGfx` scissor stack management

### Remaining Cleanup
- [ ] Standardize naming conventions across all controls (1 CPX)
- [ ] Add XML documentation comments to public APIs (2 CPX)

---

## Known Issues / Bugs

### Active Bugs
*No active bugs*

### Resolved Bugs
- [x] **Z-order drawing is inverted** (1 CPX) ✅ Fixed
  - `Draw()` in `FishUI.cs` was reversing the ordered controls array, causing lower Z-depth controls to render on top
  - Fixed by removing `.Reverse()` call in `Draw()` method
  - Now higher Z-depth controls are drawn last (on top), matching the logical Z-order

- [x] **Drag/interaction events sent to wrong control** (2 CPX) ✅ Fixed
  - Issue was in `PickControl()` recursive call for children - it wasn't reversing child order
  - Fixed by adding `.Reverse().ToArray()` to `C.GetAllChildren()` call in `PickControl`
  - Now child controls are also checked front-to-back (higher Z-depth first), matching root control behavior

- [x] **Mouse click bring-to-front is inverted** (1 CPX) ✅ Fixed
  - `PickControl()` now reverses the ordered controls array before iterating
  - Front controls (higher Z-depth) are now checked first for mouse picking
  - Clicking on overlapping controls now correctly brings the clicked item to front

- [x] **TreeView scrollbar position incorrect on expand** (2 CPX) ✅ Fixed
  - Added scroll position recalculation when content height changes in `DrawControl`
  - Scroll offset is now clamped to valid range and `ThumbPosition` is updated to match
  - Scrollbar now maintains correct position when nodes are expanded/collapsed

- [x] **Window resize only works on bottom border** (1 CPX) ✅ Fixed
  - Added `SideBorderWidth` property and adjusted content panel to leave space for left/right resize handles
  - Window can now be resized from all edges and corners

- [x] **First click on text box positions cursor at start** (1 CPX) ✅ Fixed
  - Removed `InputActiveControl` check in `HandleMousePress` that prevented cursor positioning on first click
  - Cursor now correctly positions where clicked, even on the first click

- [x] **Tab control tabs positioned too high** (1 CPX) ✅ Fixed
  - Reduced default `TabHeaderHeight` from 31 to 24 to match GWEN skin visual design
  - Active tabs now connect seamlessly with the content area

- [x] **CheckBox/RadioButton labels clipped by scissor** (2 CPX) ✅ Fixed
  - Added `DisableChildScissor` property to Control base class
  - CheckBox and RadioButton set this to true so labels can extend beyond icon bounds

- [x] **Label default Alignment causes clipping in containers** (1 CPX) ✅ Fixed
  - Changed Label default Alignment from `Align.Center` to `Align.Left`
  - Prevents text from extending past the left edge and getting scissor-clipped

- [x] DropDown doesn't close on outside click
- [x] Textbox cursor styling and blinking
- [x] ScrollBarV thumb position calculation
- [x] Docked positioning without parent
- [x] `Control.GetAbsolutePosition()` for unknown position modes
- [x] `LayoutFormat.Deserialize()` with abstract Control type
- [x] Theme system for newer controls
- [x] Theme atlas coordinate mismatch (case sensitivity in region lookup)
- [x] `Utils.Union` was commented out, breaking scissor intersection calculation
- [x] `RaylibGfx.PopScissor` didn't restore previous scissor state correctly
- [x] Window children positioned relative to window frame instead of content area

### Uncategorized (Analyze and create TODO entries in above appropriate sections with priority. Do not fix or implement them just yet. Assign complexity points where applicable. Do not delete this section when you are done, just empty it)
*No uncategorized items*

---

## Notes

- Prioritize controls that are commonly needed in game development
- Maintain the "dependency-free" philosophy - keep the core library minimal
- Backend implementations should remain in separate sample projects
- CEGUI theme files in `data/cegui_theme/` provide reference for accurate atlas coordinates
- The GWEN skin atlas (gwen.png) contains 512x512 pixels of UI elements