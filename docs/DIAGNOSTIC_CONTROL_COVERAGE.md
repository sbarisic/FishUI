# Diagnostic control coverage

This inventory tracks controls whose important runtime state is not visible from the generic hierarchy, geometry, focus, input, and render records. A provider must remain bounded and must not copy item text, application objects, delegates, credentials, console contents, file paths, or other arbitrary values by default.

## Implemented

- `Textbox` and `MultiLineEditbox`: text metrics, selection, caret, viewport, scrolling, and text privacy.
- `ScrollBarH`, `ScrollBarV`, and `ScrollablePane`: scroll position, thumb metrics, viewport, and content offsets.
- `Window`: window-specific layout and interaction state.
- `DatePicker`: selected/displayed dates, popup geometry, hover, and calendar transitions.
- `DropDown`: bounded selection indices, filtering counts, hover, scrolling, popup geometry, and transitions without item data.
- `SpreadsheetGrid`: bounded dimensions and occupancy, selection, editing, visible ranges, scrolling, cell-area geometry, modes, and transitions without cell values.
- `CheckBox` and `Slider`: current value and state transitions. These are included because they are companion controls in the SpreadsheetGrid diagnostic sample.

## High-priority gaps

- `DataGrid`: row/column counts, selected indices, hovered row, sort columns/directions, scroll position, and visible ranges. Never copy row values or row `UserData`.
- `ListBox` and `ItemListbox`: bounded selected indices, hover, scroll position, item/visible counts, and runtime scrollbar identity. Never copy item text, widgets, or `UserData`.
- `TreeView`: selected-node structural ID, expanded/visible node counts, hover, scroll position, and visible range. Node labels and tags need text/value privacy.
- `TabControl`: selected and hovered indices, enabled-tab count, content bounds, and selection transitions. Tab titles should be omitted or text-redacted.
- `PropertyGrid`: selected/editor state, expanded-category counts, visible range, and scroll state. Reflected objects and property values must never enter diagnostics by default.
- `GameConsole`: open/closing state, animation progress, dimensions, history/output/pending-write counts, and command execution outcomes. Command lines, input, output, delegates, and logger objects are sensitive and require explicit redaction rules. The current console implementation is otherwise untouched.
- `ContextMenu`, `MenuBar`, `MenuBarItem`, and `MenuItem`: open submenu chain, hovered/selected structural indices, popup geometry, and modal/input ownership.
- `TimePicker`: selected time, active field, popup/edit state, and transitions, following the privacy rules used by `DatePicker`.
- `FilePickerDialog`: navigation and selection state with path-specific privacy. Raw paths and filesystem objects must be excluded unless explicitly permitted.

## Medium-priority gaps

- `NumericUpDown`: aggregate numeric value/range/step, parse validity, pressed button, and value transitions. Its runtime textbox already supplies text-input diagnostics.
- `ToggleSwitch` and `RadioButton`: checked state and transitions. Radio-group ownership should be structural rather than an arbitrary object reference.
- `ControlScrollable`: generic content offset, viewport, and scrollbar state for subclasses that do not use `ScrollablePane`.
- `Timeline`: view range, cursor/current time, drag mode, and zoom/pan transitions.
- `LineChart`: series/point counts, visible time/value ranges, cursor mode, and selected series without labels or point values by default.
- `SelectionBox`: selection rectangle, active drag phase, and selected-control runtime IDs.
- `ProgressBar`, `BarGauge`, `RadialGauge`, `VUMeter`, and `BigDigitDisplay`: normalized value/range, orientation, mode, and animation state. Treat telemetry values as value-redactable.
- `AnimatedImageBox`: frame index/count, playback state, timing, and resource identity under asset-path privacy.
- `ToastNotification`: bounded queue count, current severity/lifetime, and transition state without message text.
- `ParticleEmitter`: active-particle count, emission state, and configured bounds without enumerating particles.

## Generic coverage is sufficient

`Button`, `Label`, `StaticText`, `ImageBox`, `Panel`, `GroupBox`, `Titlebar`, `Tooltip`, `FlowLayout`, `GridLayout`, and `StackLayout` currently have no additional high-value state that justifies a dedicated provider. Their hierarchy, geometry, visibility, focus, pointer state, render ownership, clips, and resource calls are already captured. Revisit this only when one of these controls gains state that cannot be inferred from the generic snapshot.
