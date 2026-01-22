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
- [ ] Mouse wheel scrolling support (2 CPX)
- [ ] Double-click event handling (2 CPX)
- [ ] Keyboard focus navigation (Tab key) (3 CPX)
- [ ] Focus visual indicators (2 CPX)
- [ ] Global keyboard shortcuts / hotkeys (3 CPX)
- [ ] Gamepad/controller input support (4 CPX)

### Rendering
- [ ] Control opacity/transparency (2 CPX)
- [ ] Animation system for transitions (4 CPX)
- [ ] Shader effects support (3 CPX)
- [ ] Anti-aliased rendering option (2 CPX)

### Theming & Styling
- [ ] Theme loading from file (YAML) (3 CPX)
- [ ] Runtime theme switching (3 CPX)
- [ ] Custom color palettes from theme file (2 CPX)

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
- [ ] Implement `FishUI.Resized()` method - currently empty with placeholder comment (2 CPX)
- [ ] Implement or remove `Padding` property in `Control.cs` - currently commented out (2 CPX)
- [ ] Implement or remove `UICommands.cs` and `UICommandList.cs` - unused command buffer scaffolding (1 CPX)
- [ ] Complete `Panel` hover/pressed states - currently commented out in `Panel.cs` (1 CPX)

### Commented-Out Code to Review
- [ ] `FishUI.cs`: Remove or implement `PressedLeftControl`, `HeldLeftControl`, `PressedRightControl`, `HeldRightControl` fields (1 CPX)
- [ ] `Control.cs`: Remove commented `HandleInput` method or implement it (1 CPX)
- [ ] `ScrollBarV.cs`: Remove old manual drawing code block (already replaced by child buttons) (1 CPX)

### Debug Code to Address
- [ ] `Control.cs`: Make `DebugPrint` configurable (1 CPX)
- [ ] `Panel.cs`: Make `Debug` flag configurable (1 CPX)
- [ ] `ListBox.cs`: Make debug `Console.WriteLine` for selected index configurable (1 CPX)
- [ ] Centrailize debug logging mechanism (2 CPX)

### Code Quality
- [ ] Remove unused `using System.ComponentModel.Design` in `FishUI.cs` (1 CPX)
- [ ] Remove unused `using System.Runtime.ConstrainedExecution` in `ListBox.cs` (1 CPX)
- [ ] Remove unused `using static System.Net.Mime.MediaTypeNames` in `Textbox.cs` and `Label.cs` (1 CPX)

---

## Known Issues / Bugs

- [ ] DropDown doesn't close on outside click (1 CPX)
- [ ] Textbox cursor is underscore style, should be vertical line (1 CPX)
- [ ] ScrollBarV thumb position calculation needs refinement (1 CPX)
- [ ] Docked positioning may not work correctly without parent (2 CPX)
- [ ] `Control.GetAbsolutePosition()` throws `NotImplementedException` for unknown position modes (1 CPX)

---

## Notes

- Prioritize controls that are commonly needed in game development
- Maintain the "dependency-free" philosophy - keep the core library minimal
- Backend implementations should remain in separate sample projects