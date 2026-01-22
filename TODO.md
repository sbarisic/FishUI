# FishUI - TODO

A list of planned features, improvements, and new controls for FishUI.

---

## New Controls

### High Priority

- [ ] **Slider / TrackBar**
  - Horizontal and vertical variants
  - Min/max value range with configurable step
  - Thumb dragging with value change events
  - Optional value label display

- [ ] **ProgressBar**
  - Horizontal and vertical variants
  - Determinate mode (0-100%)
  - Indeterminate/marquee animation mode
  - Customizable fill color

- [ ] **ScrollBarH** (Horizontal Scrollbar)
  - Assets already exist in `data/sb/` folder
  - Settings already defined in `FishUISettings.cs`
  - Mirror `ScrollBarV` implementation

- [ ] **TabControl / TabPanel**
  - Tab header strip with clickable tabs
  - Content panels that switch based on selected tab
  - Support for closable tabs
  - Tab overflow handling (scroll or dropdown)

- [ ] **Window / Dialog**
  - Draggable title bar
  - Close, minimize, maximize buttons
  - Modal and non-modal modes
  - Resizable borders

### Medium Priority

- [ ] **NumericUpDown / Spinner**
  - Textbox with up/down increment buttons
  - Min/max/step value constraints
  - Keyboard input validation

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
  - Rich content support (multi-line, styled)

### Lower Priority

- [ ] **SplitContainer**
  - Resizable split pane (horizontal/vertical)
  - Draggable splitter bar
  - Minimum size constraints for each pane

- [ ] **GridView / DataGrid**
  - Tabular data display
  - Column headers with sorting
  - Row selection
  - Cell editing support

- [ ] **ImageBox / PictureBox**
  - Image display control
  - Scaling modes (stretch, fit, fill, none)
  - Click events

- [ ] **GroupBox**
  - Labeled container with border
  - Title positioning options

- [ ] **ColorPicker**
  - Color selection dialog/control
  - Hue/saturation/brightness picker
  - Hex/RGB input
  - Alpha channel support

- [ ] **DateTimePicker**
  - Calendar popup for date selection
  - Time selection spinner
  - Configurable date formats

- [ ] **RichTextBox**
  - Multi-line text with formatting
  - Bold, italic, underline support
  - Multiple font sizes/colors

---

## Control Improvements

### Textbox Enhancements
- [ ] Cursor positioning with mouse click
- [ ] Text selection (click and drag)
- [ ] Copy/paste support (Ctrl+C, Ctrl+V)
- [ ] Select all (Ctrl+A)
- [ ] Undo/redo support
- [ ] Multi-line mode with word wrap
- [ ] Password masking mode
- [ ] Placeholder text
- [ ] Max length constraint
- [ ] Read-only mode

### DropDown Enhancements
- [ ] Proper dropdown popup behavior (opens below, closes on outside click)
- [ ] Search/filter functionality
- [ ] Multi-select mode
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
- [ ] Collapsible/expandable mode
- [ ] Border styles
- [ ] Shadow/elevation effects

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
- [ ] Rotation and scale transforms
- [ ] Animation system for transitions
- [ ] Shader effects support
- [ ] Anti-aliased rendering option

### Theming & Styling
- [ ] Theme loading from file (JSON/YAML)
- [ ] Runtime theme switching
- [ ] Per-control style overrides
- [ ] Dark/light theme presets
- [ ] Custom color palettes

### Accessibility
- [ ] Screen reader support
- [ ] High contrast mode
- [ ] Keyboard-only navigation
- [ ] Scalable UI for different DPI

### Performance
- [ ] Dirty rectangle rendering (only redraw changed areas)
- [ ] Control pooling/recycling for virtual lists
- [ ] Lazy initialization of child controls
- [ ] Render caching for static controls

### Serialization
- [ ] JSON layout format support
- [ ] Binary layout format for faster loading
- [ ] Layout editor tool integration

---

## Sample Application

- [ ] Complete demo showcasing all controls
- [ ] Interactive property editor for testing
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

## Backend Implementations

- [ ] **MonoGame** backend sample
- [ ] **SDL2** backend sample
- [ ] **OpenGL** (raw) backend sample
- [ ] **SFML** backend sample

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
