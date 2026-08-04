# Diagnostic control coverage

This inventory tracks controls whose runtime state is not visible from generic hierarchy, geometry, focus, input, and render records. Providers are bounded and do not copy application objects, delegates, credentials, console contents, or unclassified text.

## Implemented

- `Textbox` and `MultiLineEditbox`: text metrics, selection, caret, viewport, scrolling, and text privacy.
- `ScrollBarH`, `ScrollBarV`, and `ScrollablePane`: scroll position, thumb metrics, viewport, and content offsets.
- `Window`: window-specific layout, resize, and interaction state.
- `DatePicker` and `DropDown`: bounded calendar/popup geometry, structural selection and filtering, scrolling, and transitions without item data.
- `SpreadsheetGrid`: bounded dimensions and occupancy, selection, editing, visible ranges, scrolling, geometry, modes, and transitions without cell values.
- `CheckBox` and `Slider`: current value, visual state, and transitions.
- `DataGrid`: bounded selection, row/column and visible-range counts, hover/anchor, sort and resize state, scrolling, geometry, scrollbar identity, and structural transitions. Row values and `UserData` are excluded.
- `ListBox` and `ItemListbox`: bounded selection, item/visible/widget counts, hover/anchor, height and scroll state, and scrollbar identity. Item text, widgets, and `UserData` are excluded.
- `TreeView`: weak per-owner node identities, bounded traversal, expanded/lazy/visible counts, selection/hover, scrolling, and cycle/duplicate warnings. Optional selected labels are text-classified; tags are excluded.
- `TabControl`: tab/availability counts, selected/hover indices, header/content geometry, selected content identity, and count/selection transitions. The selected title is text-classified; `Tag` is excluded.
- `PropertyGrid`: structural item/category/visibility counts, selection/editor/context-menu state, scrolling, and reset/editor transitions. Capture reads metadata only and never invokes application getters, formatters, or `ToString()`.
- `GameConsole`: open/closing and animation state, geometry, bounded counts, dropped writes, completion/history state, input metrics, child identities, and content-free command outcomes. Command names, lines, arguments, input, output, delegates, and logger state are excluded.
- `ContextMenu`, `MenuBar`, `MenuBarItem`, and `MenuItem`: open/popup state, submenu chain and ownership by runtime ID, hover state, check/invocation outcomes, and text-classified labels/shortcuts. Actions, icon paths, and `UserData` are excluded.
- `TimePicker` and `FilePickerDialog`: time/spinner modes and transitions; cached file/directory counts, selection, navigation capability, child identities, and content-free outcomes. File capture makes no filesystem calls; paths, filenames, and filters are text-classified.
- `NumericUpDown`, `ToggleSwitch`, and `RadioButton`: numeric range/step/precision/parse/button state and value transitions; toggle/radio state and animation/visual modes. Labels are text-classified and no synthetic radio group is created.
- `Timeline` and `LineChart`: view/data ranges, geometry, drag/cursor and pause/auto-scroll state, bounded series counts, and discrete drag/pause transitions. Point values and high-frequency motion remain snapshot-only; series names and formats are text-classified.
- `ProgressBar`, `BarGauge`, `RadialGauge`, `VUMeter`, and `BigDigitDisplay`: value/range/normalization, orientation/mode, ticks/zones/peak/alignment, and mode transitions. Formats, units, labels, and display text are text-classified.
- `AnimatedImageBox`: bounded frame assets, playback modes/timing, sequence count changes, play/pause/stop, and completion. Asset identifiers are text-classified; per-frame advancement stays out of rolling history.
- `ToastNotification`: bounded severity counts, current lifetime/alpha, queue transitions, and expiration without titles or message bodies.
- `ParticleEmitter`: aggregate capacity/emission/configuration state and start/stop/burst/clear transitions without particle enumeration or per-frame count events.

All providers use the shared writer limits. Provider strings are explicitly either closed structural tokens or privacy-classified text, and projected arrays/dictionaries are independent copies for each request. Collection, viewport, chart, menu, picker, gauge, animation, toast, and particle controls also publish explicit render semantics where ownership is unambiguous.

## Generic coverage is sufficient

`ControlScrollable` and `SelectionBox` are empty marker controls, so generic diagnostics are sufficient. `Button`, `Label`, `StaticText`, `ImageBox`, `Panel`, `GroupBox`, `Titlebar`, `Tooltip`, `FlowLayout`, `GridLayout`, and `StackLayout` currently have no additional high-value state that justifies a dedicated provider. Their hierarchy, geometry, visibility, focus, pointer state, render ownership, clips, and resource calls are already captured. Revisit this only when one gains state that cannot be inferred from the generic snapshot.
