# Diagnostic snapshots

FishUI diagnostics record structured input, hierarchy, hit-test, layout, and rendering information for one `FishUI` instance. Image capture is optional. A graphics backend supports images only when it implements `IFishUIFramebufferProvider`.

## Enable recording and request a capture

```csharp
ui.Diagnostics.Enabled = true;

Task<FishUIDebugSnapshot> pending = FishUIDiagnostics.CaptureAsync(
    ui,
    new FishUIDebugSnapshotOptions
    {
        IncludeScreenshot = false,
        IncludeAnnotatedOverlay = false
    });

// A later TickDraw completes the request.
FishUIDebugSnapshot snapshot = await pending;
```

A request queued before `TickDraw` starts participates in that draw. A request queued during drawing waits for the next draw. Do not synchronously wait for a capture on the UI thread: it remains pending when the application does not call `TickDraw`.

Several callers can share one draw recording pass. Each caller receives a distinct `RequestId` and independently projected snapshot; those snapshots share a `CaptureId`. `UiSessionId` scopes both IDs and all runtime control IDs. `LastCapture` is the last completed per-request projection.

Per-request `RedactText`, `RedactValues`, and `IncludeControlData` settings can remove more data from that projection. They cannot weaken the session privacy floor. The authoritative control tree is captured after drawing; `preDrawGeometry`, `createdDuringDraw`, and `removedDuringDraw` identify changes made by lazy drawing or layout work.

Date pickers add bounded calendar state to their control record: whether the popup is open, its pixel bounds, the displayed month, the selected date, and the hovered date. Their open/close, month-navigation, and selection changes also appear as state-change events when event history is active. Date values remain subject to session and request value redaction.

Drop-down controls record bounded structural state: popup and search-box bounds, selected indices, selection count, hovered and filtered indices, item counts, and scrolling metrics. Open/close, selection, and search-length changes appear in the event timeline. Item text, custom-renderer delegates, and item `UserData` are never copied into control data; the selected-index list is capped at 256 entries and reports truncation.

`Ctrl+Shift+F12` queues the default capture when diagnostics and the diagnostic hotkey are enabled. In Debug builds, rolling event history is enabled by default; in Release builds it is disabled until the application enables it. The hotkey capture includes up to the configured rolling-history duration before the trigger plus events from the captured frame. Enabling only the hotkey does not enable continuous recording.

```csharp
ui.Diagnostics.RollingEventHistoryEnabled = true;
ui.Diagnostics.RollingEventHistoryDuration = TimeSpan.FromSeconds(10);
ui.Diagnostics.MaximumRollingHistoryEvents = 20_000;
```

History is bounded by both time and capacity. A high-volume burst can retain less than the requested duration; snapshot metadata reports the actual duration, projected event count, and capacity truncation. Disabling rolling history clears it as soon as no capture is active or pending. Enabling it later records only future activity.

## Thread and lifetime rules

Request enqueueing and cancellation are thread-safe. Capture batching, UI traversal, event routing, hit-test inspection, and rendering occur on the FishUI thread. Other diagnostic inspection APIs are not thread-safe unless their API says otherwise.

Dispose `FishUI` to cancel outstanding requests. Disposal is idempotent and does not dispose injected graphics, input, event, or filesystem services.

## Privacy

Text is redacted by default. Password text, clipboard contents, delegates, `UserData`, and arbitrary application objects are never recorded. Framebuffers are disabled by default because pixels can expose text that structured redaction hides.

```csharp
ui.Diagnostics.PrivacyPolicy.AllowFramebufferCapture = true;
ui.Diagnostics.ResetEventRecorder(); // commits a weaker policy for future data
```

Strengthening privacy clears buffered data that could violate the new policy. Weakening a policy does not reveal old data and takes effect only after `ResetEventRecorder`. Password protection cannot be weakened by request settings.

Exception stacks are excluded unless both the request and session privacy policy allow them.

## Framebuffer timing and ownership

FishUI asks the optional framebuffer provider after controls, dropdown overlays, the tooltip, and the virtual mouse are drawn, immediately before `EndDrawing`. Returning a `FishUIFramebuffer` transfers ownership to FishUI. FishUI validates, copies, normalizes, and disposes it exactly once.

The default limits are 16,384 pixels per dimension and 256 MiB of decoded RGBA data. Accepted pixels become tightly packed, top-left, non-premultiplied RGBA before PNG encoding.

`windowWidthPixels` and `windowHeightPixels` are the FishUI coordinate space frozen when the capture draw begins. `framebufferWidthPixels` and `framebufferHeightPixels` are the physical image dimensions. `framebufferScaleX` and `framebufferScaleY` map the former to the latter independently, so annotated overlays remain aligned on non-uniform and High-DPI displays. Labels and outline thickness remain fixed in physical pixels.

The Raylib backend flushes its active render batch and captures before presentation. Raylib 5.5 can return a DPI-sized readback with its logical viewport bottom-aligned inside it; the provider expands that viewport to the physical image dimensions so the scale metadata and overlays describe the displayed window. With the sample runner's host-owned frame, the image includes host drawing done before `FUI.Tick`, FishUI controls, dropdowns, tooltips, and the virtual mouse. It excludes the FPS counter and other host drawing performed after `FUI.Tick`.

## Partial captures and artifacts

A draw or presentation exception produces a `partial` snapshot when useful diagnostic state can still be finalized, then FishUI rethrows the original exception. An image captured before an `EndDrawing` failure remains valid. Screenshot and overlay status are independent; overlay generation failure does not discard a valid screenshot.

The artifact status is one of `excluded`, `unsupported`, `blockedByPrivacy`, `unavailable`, `available`, or `failed`.

## Export

```csharp
snapshot.SaveDirectory("diagnostics/capture-1");
snapshot.SaveZip("diagnostics/capture-1.zip");
```

These methods perform blocking filesystem and compression work. Do not normally call them from the rendering thread. They refuse existing destinations unless `overwrite: true` is supplied and publish through a temporary sibling.

Set `ui.Diagnostics.AutoExportAsync` for asynchronous automatic export. Capture completion does not wait for it. Export failures raise `ExportFailed` and never change a valid in-memory capture into a failed or partial capture.

`FishUISample` enables ten seconds of rolling history and framebuffer permission for every selected demo. It writes bundles to `diagnostics/{snapshot.DefaultExportName}` under `AppContext.BaseDirectory`. It drains already-started export work during shutdown before closing Raylib. The Raylib backend package remains at local version `1.0.11`; bump it before publishing these backend changes.

The directory bundle contains `snapshot.json`, `recent-events.json`, and `interaction-summary.txt` when requested. It also contains `screenshot.png` and `overlay.png` when those artifacts are available.

## Rendering and scissor records

Rendering records keep one total order and classify rendering, scissor, measurement, resource, and graphics-state calls. Control ownership is automatic. Built-in controls can add selection, caret, text, viewport, background, or scrollbar semantics. Custom controls do not need semantic scopes; missing semantics are not warnings.

`PushScissor` saves the effective clip and applies an intersection. `PopScissor` restores the saved state. `BeginScissor` replaces the direct backend scissor state, while `EndScissor` ends it; they are not modeled as a second push/pop pair. Invalid or unbalanced use produces diagnostic warnings without changing backend calls.

The **Diagnostic Snapshot** demo uses the committed multiline Notepad scrolling flow and shows request completion identities.
