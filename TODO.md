# FishUI - TODO

A list of planned features, improvements, and new controls for FishUI.

---

## New Controls

### High Priority

- [ ] **Slider / TrackBar** (0 CPX)
  - Horizontal and vertical variants
  - Min/max value range with configurable step
  - Thumb dragging with value change events
  - Optional value label display

- [ ] **ProgressBar** (0 CPX)
  - Horizontal and vertical variants
  - Determinate mode (0-100%)
  - Indeterminate/marquee animation mode

- [ ] **ScrollBarH** (Horizontal Scrollbar) (0 CPX)
  - Assets already exist in `data/sb/` folder
  - Settings already defined in `FishUISettings.cs`
  - Mirror `ScrollBarV` implementation

- [ ] **TabControl / TabPanel**
  - Tab header strip with clickable tabs
  - Content panels that switch based on selected tab
  - Tab overflow handling (scroll with left-right arrows)

- [ ] **Window / Dialog**
  - Draggable title bar
  - Close button only
  - Modal and non-modal modes
  - Resizable and fixed border mode

### Medium Priority

- [ ] **NumericUpDown / Spinner**
  - Textbox with up/down increment buttons
  - Min/max/step value constraints

- [ ] **ToggleSwitch**
  - Modern on/off toggle control
  - Animated transition between states
  - Optional on/off labels

- [ ] **TreeView**
  - Hierarchical node display
  - Expand/collapse functionality
  - Node selection and multi-select
  - Lazy loading support for large trees

- [ ] **ContextMenu / PopupMenu**
  - Right-click context menus
  - Menu item with icons
  - Submenu support
  - Keyboard navigation

- [ ] **MenuBar**
  - Horizontal menu strip
  - Dropdown submenus
  - Keyboard shortcuts display
  - Separator items

- [ ] **Tooltip**
  - Hover-triggered popup text
  - Configurable delay and duration

### Lower Priority

- [ ] **GridView / DataGrid**
  - Tabular data display
  - Column headers
  - Row selection

- [ ] **ImageBox / PictureBox**
  - Image display control
  - Scaling modes (stretch, fit, fill, none)
  - Click events

- [ ] **GroupBox**
  - Labeled container with border
  - Title positioning options

- [ ] **DateTimePicker**
  - Calendar popup for date selection
  - Optional time selection spinner
  - Configurable date formats

---

## Control Improvements

### Textbox Enhancements
- [ ] Cursor positioning with mouse click
- [ ] Text selection (click and drag)
- [ ] Copy/paste support (Ctrl+C, Ctrl+V, exposed as functions on a control)
- [ ] Select all (Ctrl+A, exposed as a function)
- [ ] Multi-line mode with word wrap
- [ ] Password masking mode
- [ ] Placeholder text
- [ ] Max length constraint
- [ ] Read-only mode

### DropDown Enhancements
- [ ] Proper dropdown popup behavior (opens below, closes on outside click)
- [ ] Multi-select mode (checkbox icon per item)
- [ ] Custom item rendering

### ListBox Enhancements
- [ ] Multi-select mode (Ctrl+click, Shift+click)
- [ ] Virtual scrolling for large lists
- [ ] Custom item templates
- [ ] Drag and drop reordering

### Button Enhancements
- [ ] Icon support (image + text)
- [ ] Toggle button mode
- [ ] Repeat button mode (fires while held)

### Panel Enhancements
- [ ] Border styles

---

## Core Framework Features

### Layout System
- [ ] **FlowLayout** - Automatic horizontal/vertical flow of children
- [ ] **GridLayout** - Grid-based child positioning
- [ ] **StackLayout** - Vertical/horizontal stacking
- [ ] Anchor system for responsive resizing
- [ ] Margin and Padding properties on all controls
- [ ] Auto-sizing controls based on content

### Input & Events
- [ ] Mouse wheel scrolling support
- [ ] Double-click event handling
- [ ] Keyboard focus navigation (Tab key)
- [ ] Focus visual indicators
- [ ] Global keyboard shortcuts / hotkeys
- [ ] Gamepad/controller input support

### Rendering
- [ ] Control opacity/transparency
- [ ] Animation system for transitions
- [ ] Shader effects support
- [ ] Anti-aliased rendering option

### Theming & Styling
- [ ] Theme loading from file (YAML)
- [ ] Runtime theme switching
- [ ] Custom color palettes from theme file

### Accessibility
- [ ] Keyboard-only navigation
- [ ] Virtual mouse mode (modifier key + arrow keys/numpad keys)
- [ ] Scalable UI for different DPI

### Serialization
- [ ] YAML layout format support

---

## Sample Application

- [ ] Complete demo showcasing all controls
- [ ] Theme switcher demo
- [ ] Responsive layout examples
- [ ] Game UI example (inventory, HUD, dialogs)

---

## Documentation

- [ ] API reference documentation
- [ ] Getting started tutorial
- [ ] Custom control creation guide
- [ ] Backend implementation guide (beyond Raylib)
- [ ] Best practices and patterns

---

## Known Issues / Bugs

- [ ] DropDown doesn't close on outside click
- [ ] Textbox cursor is underscore style, should be vertical line
- [ ] ScrollBarV thumb position calculation needs refinement
- [ ] Docked positioning may not work correctly without parent

---

## Notes

- Prioritize controls that are commonly needed in game development
- Maintain the "dependency-free" philosophy - keep the core library minimal
- Backend implementations should remain in separate sample projects

```
Complexity points (CPX) estimates are provided for each item to help prioritize development efforts. 1 to 5 scale.

	1 - Single file control/component
	2 - Single file control/component with single function change dependencies (eg. event system, input system...)
	3 - Multi-file control/component or single file with multiple dependencies, no architecture changes, only additions
	4 - Multi-file control/component with multiple dependencies and significant logic, possible minor architecture changes
	5 - Large feature spanning multiple components and subsystems, major architecture changes
```