# FishUI - TODO

A list of planned features, improvements, and new controls for FishUI.

> **CPX (Complexity Points)** - 1 to 5 scale:
> - **1** - Single file control/component
> - **2** - Single file control/component with single function change dependencies
> - **3** - Multi-file control/component or single file with multiple dependencies, no architecture changes
> - **4** - Multi-file control/component with multiple dependencies and significant logic, possible minor architecture changes
> - **5** - Large feature spanning multiple components and subsystems, major architecture changes

---

## New Controls

### High Priority

- [x] **Slider / TrackBar** (2 CPX)
- Horizontal and vertical variants
- Min/max value range with configurable step
- Thumb dragging with value change events
- Optional value label display

- [x] **ProgressBar** (1 CPX)
- Horizontal and vertical variants
- Determinate mode (0-100%)
- Indeterminate/marquee animation mode

- [x] **ScrollBarH** (Horizontal Scrollbar) (1 CPX)
- Assets already exist in `data/sb/` folder
- Settings already defined in `FishUISettings.cs`
- Mirror `ScrollBarV` implementation

- [ ] **TabControl / TabPanel** (3 CPX)
  - Tab header strip with clickable tabs
  - Content panels that switch based on selected tab
  - Tab overflow handling (scroll with left-right arrows)

- [ ] **Window / Dialog** (4 CPX)
  - Draggable title bar
  - Close button only
  - Modal and non-modal modes
  - Resizable and fixed border mode

### Medium Priority

- [ ] **NumericUpDown / Spinner** (2 CPX)
  - Textbox with up/down increment buttons
  - Min/max/step value constraints

- [x] **ToggleSwitch** (1 CPX)
  - Modern on/off toggle control
  - Animated transition between states
  - Optional on/off labels

- [ ] **TreeView** (4 CPX)
  - Hierarchical node display
  - Expand/collapse functionality
  - Node selection and multi-select
  - Lazy loading support for large trees

- [ ] **ContextMenu / PopupMenu** (3 CPX)
  - Right-click context menus
  - Menu item with icons
  - Submenu support
  - Keyboard navigation

- [ ] **MenuBar** (3 CPX)
  - Horizontal menu strip
  - Dropdown submenus
  - Keyboard shortcuts display
  - Separator items

- [ ] **Tooltip** (2 CPX)
  - Hover-triggered popup text
  - Configurable delay and duration

### Lower Priority

- [ ] **GridView / DataGrid** (4 CPX)
  - Tabular data display
  - Column headers
  - Row selection

- [ ] **ImageBox / PictureBox** (1 CPX)
  - Image display control
  - Scaling modes (stretch, fit, fill, none)
  - Click events

- [ ] **AnimatedImageBox** (2 CPX)
  - Frame sequences are stored as an array of images
  - Configurable frame rate
  - Ability to pause animation display static frame
  - Play/pause control

- [ ] **GroupBox** (1 CPX)
  - Labeled container with border
  - Title positioning options

- [ ] **DateTimePicker** (4 CPX)
  - Calendar popup for date selection
  - Optional time selection spinner
  - Configurable date formats

---

## Control Improvements

### Textbox Enhancements
- [ ] Cursor positioning with mouse click (2 CPX)
- [ ] Text selection (click and drag) (2 CPX)
- [ ] Copy/paste support (Ctrl+C, Ctrl+V, exposed as functions on a control) (2 CPX)
- [ ] Select all (Ctrl+A, exposed as a function) (1 CPX)
- [ ] Multi-line mode with word wrap (3 CPX)
- [ ] Password masking mode (1 CPX)
- [ ] Placeholder text (1 CPX)
- [ ] Max length constraint (1 CPX)
- [ ] Read-only mode (1 CPX)

### DropDown Enhancements
- [ ] Proper dropdown popup behavior (opens below, closes on outside click) (2 CPX)
- [ ] Multi-select mode (checkbox icon per item) (2 CPX)
- [ ] Custom item rendering (2 CPX)

### ListBox Enhancements
- [ ] Multi-select mode (Ctrl+click, Shift+click) (2 CPX)
- [ ] Virtual scrolling for large lists (3 CPX)
- [ ] Custom item templates (2 CPX)
- [ ] Drag and drop reordering (3 CPX)

### Button Enhancements
- [ ] Icon support (image + text) (1 CPX)
- [ ] Toggle button mode (1 CPX)
- [ ] Repeat button mode (fires while held) (1 CPX)

### Panel Enhancements
- [ ] Border styles (1 CPX)

---

## Core Framework Features

### Layout System
- [ ] **FlowLayout** - Automatic horizontal/vertical flow of children (3 CPX)
- [ ] **GridLayout** - Grid-based child positioning (3 CPX)
- [ ] **StackLayout** - Vertical/horizontal stacking (2 CPX)
- [ ] Anchor system for responsive resizing (4 CPX)
- [ ] Margin and Padding properties on all controls (3 CPX)
- [ ] Auto-sizing controls based on content (3 CPX)

### Input & Events
- [x] Mouse wheel scrolling support (2 CPX)
- [x] Double-click event handling (2 CPX)
- [x] Keyboard focus navigation (Tab key) (3 CPX)
- [x] Focus visual indicators (2 CPX)
- [x] Global keyboard shortcuts / hotkeys (3 CPX)
- [x] Virtual mouse cursor mode, custom mouse pointer drawing, input functions to move and click the virtual mouse, i.e. joystick to mouse input (4 CPX)

### Rendering
- [ ] Control opacity/transparency (2 CPX)
- [ ] Animation system for transitions (4 CPX)
- [ ] Anti-aliased rendering option (2 CPX)

### Theming & Styling
- [x] Theme loading from file (YAML) (3 CPX)
- [x] Runtime theme switching (3 CPX)
- [x] Custom color palettes from theme file (2 CPX)
- [x] **Sprite Atlas Theme Support** (4 CPX)
  - Support loading a single theme image (sprite atlas) as alternative to individual image files
  - Use `gwen.png` as the base/reference theme atlas
  - Define texture region coordinates for each control skin (buttons, panels, scrollbars, checkboxes, etc.)
  - Support both atlas-based and individual file-based theming modes
  - Extract control states from atlas (normal, hover, pressed, disabled)

### Accessibility
- [ ] Keyboard-only navigation (3 CPX)
- [ ] Virtual mouse mode (modifier key + arrow keys/numpad keys) (3 CPX)
- [ ] Scalable UI for different DPI (4 CPX)

### Serialization
- [ ] YAML layout format support (2 CPX)

---

## Sample Application

- [ ] Complete demo showcasing all controls (2 CPX)
- [ ] Theme switcher demo (2 CPX)
- [ ] Responsive layout examples (2 CPX)
- [ ] Game UI example (inventory, HUD, dialogs) (3 CPX)

---

## Documentation

- [ ] API reference documentation
- [ ] Getting started tutorial
- [ ] Custom control creation guide
- [ ] Backend implementation guide (beyond Raylib)
- [ ] Best practices and patterns

---

## Code Cleanup & Technical Debt

### Incomplete Implementations
- [x] Implement `FishUI.Resized()` method - currently empty with placeholder comment (2 CPX)
- [x] Implement or remove `Padding` property in `Control.cs` - currently commented out (2 CPX)
- [x] Implement or remove `UICommands.cs` and `UICommandList.cs` - unused command buffer scaffolding (1 CPX)
- [x] Complete `Panel` hover/pressed states - currently commented out in `Panel.cs` (1 CPX)

### Commented-Out Code to Review
- [x] `FishUI.cs`: Remove or implement `PressedLeftControl`, `HeldLeftControl`, `PressedRightControl`, `HeldRightControl` fields (1 CPX)
- [x] `Control.cs`: Remove commented `HandleInput` method or implement it (1 CPX)
- [x] `ScrollBarV.cs`: Remove old manual drawing code block (already replaced by child buttons) (1 CPX)

### Debug Code to Address
- [x] `Control.cs`: Make `DebugPrint` configurable (1 CPX)
- [x] `Panel.cs`: Make `Debug` flag configurable (1 CPX)
- [x] `ListBox.cs`: Make debug `Console.WriteLine` for selected index configurable (1 CPX)
- [x] Centrailize debug logging mechanism (2 CPX)
- [x] Implement debug outline drawing for all controls when debug mode is enabled (3 CPX)

### Code Quality
- [x] Remove unused `using System.ComponentModel.Design` in `FishUI.cs` (1 CPX)
- [x] Remove unused `using System.Runtime.ConstrainedExecution` in `ListBox.cs` (1 CPX)
- [x] Remove unused `using static System.Net.Mime.MediaTypeNames` in `Textbox.cs` and `Label.cs` (1 CPX)

---

## Known Issues / Bugs

- [x] DropDown doesn't close on outside click (1 CPX)
- [x] Textbox cursor is underscore style, should be vertical line, it should blink when control is in focus (1 CPX)
- [x] ScrollBarV thumb position calculation needs refinement (1 CPX)
- [x] Docked positioning may not work correctly without parent (2 CPX)
- [x] `Control.GetAbsolutePosition()` throws `NotImplementedException` for unknown position modes (1 CPX)
- [ ] `LayoutFormat.Deserialize()` fails with abstract `Control` type - YamlDotNet cannot instantiate abstract class (2 CPX)
  - Exception at line 83: `MissingMethodException: Cannot dynamically create an instance of type 'FishUI.Controls.Control'`
  - Need to configure YamlDotNet to use concrete types via tag mapping or custom type resolver
- [ ] **Theme system incomplete for newer controls** (3 CPX)
  - ProgressBar, Slider, ToggleSwitch use hardcoded `FishColor` values instead of theme color palette
  - These controls don't have theme region mappings in `FishUISettings.ApplyThemeRegions()`
  - Need to add region definitions for these controls in theme YAML files
  - Controls should use `Settings.GetColorPalette()` for colors instead of hardcoded values

---

## Notes

- Prioritize controls that are commonly needed in game development
- Maintain the "dependency-free" philosophy - keep the core library minimal
- Backend implementations should remain in separate sample projects