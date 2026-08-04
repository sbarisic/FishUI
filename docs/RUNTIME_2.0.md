# FishUI 2.0 runtime contracts

## Lifecycle

A FishUI instance starts in the `Created` state. Call `Init()` before frame processing. A successful `Init()` call is idempotent. An initialization failure leaves the instance in `Created`, so the application can retry.

`TickUpdate` and `TickDraw` reject use before initialization. All public runtime access rejects use after disposal. FishUI does not dispose injected graphics, input, event, file-system, or logger services.

Dispose services in this order:

1. Dispose FishUI.
2. Wait for pending diagnostic exports.
3. Dispose the graphics backend.
4. Close the native window.

## Frame processing

`TickUpdate` advances animations and control timers. It prepares layout before input and prepares dirty layout again after input. The update freezes root, child, Z, and overlay order for the prepared frame.

`TickDraw` renders the prepared frame. It does not initialize controls, advance timers, or change layout. Call `TickUpdate` before the first `TickDraw`. You can draw the same prepared frame more than once.

Hierarchy or Z changes during a callback become visible in the next prepared frame. Detached controls lose FishUI-owned interaction state immediately. A detached control cannot receive input from an older frozen snapshot.

## Paint, picking, and overlays

FishUI paints lower `ZDepth` values first. It paints higher values last. It paints `AlwaysOnTop` controls after normal controls. Insertion order resolves equal values.

FishUI picks controls in the exact reverse paint order. Hidden or disabled ancestors make their subtree non-interactive. A disabled control does not block an enabled control below it.

Dropdown and date-picker popups use a per-FishUI overlay registry. FishUI paints overlays after the normal tree. It picks overlays in reverse overlay paint order.

## Clipping

Core controls use `PushScissor` and `PopScissor`. Use `PushScissorScope` in custom controls so exceptions restore the previous clip.

`BeginScissor` and `EndScissor` control the backend direct-scissor state. They do not form a stack. A backend must restore the stacked clip after direct scissoring ends.

## Input

FishUI samples each backend input source once per update. It drains up to 256 key-press records and 1,024 generated characters by default. Diagnostics report queue limits.

Override `HandleKeyPressed`, `HandleKeyHeld`, and `HandleKeyReleased` for keyboard input. Do not poll consumptive backend methods from a control.

Pointer priority is virtual mouse, active touch, then physical mouse. The first active touch owns one left-pointer gesture until release. FishUI does not switch fingers during that gesture.

## Coordinate spaces

Controls store layout, content sizes, scrolling, and scrollbar values in logical FishUI coordinates. Graphics backends convert them to physical pixels when required.

Diagnostics label the FishUI window coordinate space separately from framebuffer dimensions. The framebuffer scale can differ on each axis.

## Layout serialization

Use `FishUILayoutSerializationOptions` to select graph limits and a type registry. Use `FishUILayoutTypeRegistry.BuiltIn.Extend(...)` for application controls.

FishUI validates a complete graph before it changes the current roots. Validation rejects cycles, shared children, invalid roots, excessive depth, and excessive control counts. A failed attachment restores the original roots and interaction state.

Runtime children never enter layout YAML. Composite controls keep their control-specific YAML contracts. `FilePickerDialog` is runtime-only.

## Backend resources

`ImageRef` and `FontRef` are immutable metadata handles. The graphics backend owns all native resources referenced by these handles.

`RaylibFishGfx` caches native images and fonts by normalized resource keys. Its idempotent `Dispose()` method unloads each owned image, texture, and font once.

The Raylib screenshot request runs inside the open Raylib frame. It does not capture the Windows desktop. Diagnostic framebuffer capture also occurs before presentation.

## Automated Raylib checks

FishUISample accepts `--frames N` to close a selected sample after a bounded frame count. Add `--fishui-begin-drawing` to let `RaylibFishGfx` own `BeginDrawing` and `EndDrawing`. Without that switch, the host owns the Raylib frame. Add `--capture-diagnostics` to request and export a framebuffer-backed diagnostic bundle during the run.

For example:

```text
dotnet run --project FishUISample -- --sample 7 --frames 30 --capture-diagnostics
dotnet run --project FishUISample -- --sample 7 --frames 30 --capture-diagnostics --fishui-begin-drawing
```

The sample changes its working directory to `AppContext.BaseDirectory`. Theme, font, image, and diagnostic paths therefore resolve against the deployed output for CLI and IDE launches.
