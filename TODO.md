# FishUI - TODO

A list of planned features, improvements, and new controls for FishUI.

> **CPX (Complexity Points)** - 1 to 5 scale:
> - **1** - Single file control/component
> - **2** - Single file control/component with single function change dependencies
> - **3** - Multi-file control/component or single file with multiple dependencies, no architecture changes
> - **4** - Multi-file control/component with multiple dependencies and significant logic, possible minor architecture changes
> - **5** - Large feature spanning multiple components and subsystems, major architecture changes

> How TODO file should be iterated
- After Active Bugs the rest of the TODO file by priority and complexity (High priority takes precedance, then CPX points) (take one at a time). Consolidate all completed TODO items by combining similar ones and shortening the descriptions where possible
- First handle the Uncategorized section, if any similar issues already are on the TODO list, increase their priority instead of adding duplicates
- When Uncategorized section is empty, start by fixing Active Bugs (take one at a time)

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
- [x] ~~Icon support (image + text), add to examples~~ - Added Icon, IconPosition, IconSpacing properties; SampleDefault has icon button demo
- [x] ~~Toggle button mode~~ - Added IsToggleButton, IsToggled properties, OnToggled event; demo in SampleDefault
- [x] ~~Repeat button mode (fires while held)~~ - Added IsRepeatButton, RepeatDelay, RepeatInterval properties; demo in SampleDefault
- [x] ~~ImageButton variant (icon-only button)~~ - Added IsImageButton, ImageButtonHoverTint, ImageButtonPressedTint; demo in SampleDefault

### Panel Enhancements
- [x] ~~Border styles, Panel variants (Normal, Bright, Dark, Highlight), examples~~ - Added PanelVariant enum, BorderStyle enum (None, Solid, Inset, Outset), theme regions, and SampleDefault demo

### ProgressBar Enhancements
- [ ] Vertical progress bar mode (1 CPX)
  - *GWEN has: VProgressBar regions for vertical variant*

### Slider Enhancements
- [x] ~~Mouse wheel support~~ - Scroll to adjust value (uses Step or 1% of range)

### ScrollBar Enhancements
- [x] ~~Mouse wheel support for ScrollBarV/ScrollBarH~~ - Both scroll bars now support mouse wheel scrolling

### NumericUpDown Enhancements
- [x] ~~Mouse wheel support~~ - Scroll to increment/decrement value

### TabControl Enhancements
- [x] ~~Preserve tab names during serialization~~ - Added TabNames property that syncs with TabPages; names restored in OnDeserialized

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
- [x] ~~**StackLayout**~~ - Vertical/horizontal stacking with spacing and padding
- [x] ~~Anchor system for responsive resizing~~ - Added FishUIAnchor enum; Anchor property on Control with TopLeft/TopRight/BottomLeft/BottomRight/Horizontal/Vertical/All modes; demo in SampleDefault
- [x] ~~Margin and Padding properties on all controls~~ - Added FishUIMargin struct; Margin/Padding properties on Control; demo in SampleDefault
- [ ] Auto-sizing controls based on content (3 CPX)

### Rendering
- [ ] Control opacity/transparency (2 CPX)
- [ ] Animation system for transitions (4 CPX)
- [ ] Anti-aliased rendering option (2 CPX)
- [ ] Shadow rendering for windows/popups (2 CPX)
  - *GWEN has: Shadow region*
- [x] ~~Font system refactoring~~ - Added FontStyle enum, FishUIFontMetrics struct, GetFontMetrics method, IsMonospaced detection, LineHeight property
- [x] ~~Rendering images with rotation and scaling~~ - Already supported via DrawImage(Rot, Scale) and DrawNPatch(Rotation) in IFishUIGfx

### Accessibility
- [ ] Keyboard-only navigation (3 CPX)
- [ ] Scalable UI for different DPI (4 CPX)

### Serialization
- [x] ~~LayoutFormat TypeMapping~~ - Added all implemented controls for layout save/load
- [ ] Image reference serialization (3 CPX)
  - Icons/images (Button.Icon, etc.) not preserved across Save/Load Layout
  - Need mechanism to reference images by file path or ID in layout files

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
- [x] ~~Expand StackLayout demo~~ - Added nested stacks (horizontal containing 3 vertical stacks with mixed controls)

---

## Documentation

- [ ] Update complete README.md from scratch 
- [ ] API reference documentation
- [ ] Getting started tutorial
- [ ] Custom control creation guide
- [ ] Backend implementation guide (beyond Raylib)
- [ ] Theme creation guide

---

## Code Cleanup & Technical Debt
- [x] ~~Audit [YamlIgnore] attributes~~ - Added to FishUIThemeRegion.UsesImageFile and FishUIVirtualMouse read-only properties
- [x] ~~Standardize naming conventions~~ - Converted fields to properties: CheckBox/RadioButton `Checked`→`IsChecked`, Panel `IsTransparent`, ListBox `ShowScrollBar`
- [x] ~~Add XML documentation comments to core public APIs~~ - Documented Control, FishUI, FishUISettings, IFishUIGfx, IFishUIInput
- [x] ~~Add screenshot button to all examples~~ - Added TakeScreenshot to ISample; screenshot buttons with camera icons in all 3 samples
- [x] ~~Extract TODO comments from code~~ - Moved 4 TODOs to proper sections, removed comments from source
- [x] ~~Move DebugLogTooltips flag into FishUIDebug~~ - Now forwards to `FishUIDebug.LogTooltips` like other debug flags

---

## Known Issues / Bugs

### Active Bugs

*No active bugs*

### Fixed Bugs

- [x] ~~ScrollBar mouse wheel not working in examples~~ - Mouse wheel events now bubble up to parent controls; child buttons propagate to ScrollBar

- [x] ~~TreeNode.HasChildren deserialization error~~ - Added `[YamlIgnore]` to read-only property
- [x] ~~TabControl.SelectedTab deserialization error~~ - Added `[YamlIgnore]` to read-only property
- [x] ~~DropDown.SelectIndex NullReferenceException~~ - Added null check before `FishUI.Events.Broadcast()`
- [x] ~~Make Invisible/Visible buttons broken~~ - Changed `Panel` cast to `Control` in EvtHandler after Panel→Window conversion
- [x] ~~ContextMenu submenu text overlapping~~ - Added `DrawChildren` override in MenuItem to prevent rendering submenu item data as visual children
- [x] ~~Layout save/load breaks Window and TabControl~~ - Added `OnDeserialized` virtual method to Control, overridden in Window and TabControl to reinitialize internal state

### Uncategorized (Analyze and create TODO entries in above appropriate sections with priority. Do not fix or implement them just yet. Assign complexity points where applicable. Do not delete this section when you are done, just empty it)

*No uncategorized items*

---

## Notes

- Prioritize controls that are commonly needed in game development
- Maintain the "dependency-free" philosophy - keep the core library minimal
- Backend implementations should remain in separate sample projects
- CEGUI theme files in `data/cegui_theme/` provide reference for accurate atlas coordinates
- The GWEN skin atlas (gwen.png) contains 512x512 pixels of UI elements