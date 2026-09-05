# Diagnostic snapshots

See [Unicode, layout, and update hardening](HARDENING.md) for the current Rune input, numeric-range, catch-up, and text-metrics contracts.

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

Per-request `RedactText`, `RedactValues`, and `IncludeControlData` settings can remove more data from that projection. They cannot weaken the session privacy floor. The authoritative control tree is captured after drawing; `preDrawGeometry`, `createdDuringDraw`, and `removedDuringDraw` identify changes made by lazy drawing or layout work. Provider data is collected only once, after the final identity/path traversal, so lazy draw-time state is visible and expensive models are not scanned twice.

Control providers share bounded collection and scan contracts. The default limits are 256 emitted entries per collection, 100,000 scanned entries per provider, 500,000 scanned entries across the final capture pass, and 512 characters for any collected text value. `controlScanEntries`, `controlScanBudget`, and `controlScanLimitReached` report capture-wide use. Truncated collections include source/emitted counts and emit stable warnings. Invalid third-party field keys are skipped without discarding valid fields from that provider.

Date pickers add bounded calendar state to their control record: whether the popup is open, its pixel bounds, the displayed month, the selected date, and the hovered date. Their open/close, month-navigation, and selection changes also appear as state-change events when event history is active. Date values remain subject to session and request value redaction.

Drop-down controls record bounded structural state: popup and search-box bounds, selected indices, selection count, hovered and filtered indices, item counts, and scrolling metrics. Open/close, selection, and search-length changes appear in the event timeline. Item text, custom-renderer delegates, and item `UserData` are never copied into control data; the selected-index list is capped at 256 entries and reports truncation.

Spreadsheet grids record selection, edit state, scroll position, visible row and column ranges, cell geometry, cursor and heat-map modes, and bounded occupancy counts. Cell contents are never copied. Cell-change events contain coordinates and old/new lengths only. Check boxes and sliders also record their current values and state transitions. See [Diagnostic control coverage](DIAGNOSTIC_CONTROL_COVERAGE.md) for the complete provider inventory.

`Ctrl+Shift+F12` queues the default capture when diagnostics and the diagnostic hotkey are enabled. In Debug builds, rolling event history is enabled by default; in Release builds it is disabled until the application enables it. The hotkey capture includes up to the configured rolling-history duration before the trigger plus events from the captured frame. Enabling only the hotkey does not enable continuous recording.

```csharp
ui.Diagnostics.RollingEventHistoryEnabled = true;
ui.Diagnostics.RollingEventHistoryDuration = TimeSpan.FromSeconds(10);
ui.Diagnostics.MaximumRollingHistoryEvents = 20_000;
```

History is bounded by both time and capacity. A high-volume burst can retain less than the requested duration; snapshot metadata reports the actual duration, projected event count, and capacity truncation. Disabling rolling history clears it as soon as no capture is active or pending. Enabling it later records only future activity.

Unchanged raw pointer state is not repeated every frame. Actual mouse movement remains in `recent-events.json`, while the human interaction summary reports its sample count instead of listing every movement. Diagnostic drag events begin only after `DragStartThresholdPixels` is exceeded; this does not change control drag behavior.

## Thread and lifetime rules

Request enqueueing and cancellation are thread-safe. Capture batching, UI traversal, event routing, hit-test inspection, and rendering occur on the FishUI thread. Other diagnostic inspection APIs are not thread-safe unless their API says otherwise.

Dispose `FishUI` to cancel outstanding requests. Disposal is idempotent and does not dispose injected graphics, input, event, or filesystem services.

## Privacy

Text is redacted by default. Password text, clipboard contents, delegates, `UserData`, and arbitrary application objects are never recorded. Framebuffers are disabled by default because pixels can expose text that structured redaction hides.

When text redaction is active, printable `KeyPressed` identities and backend key codes are also removed. This prevents the key event from revealing a character that the corresponding text-input event redacted. Navigation, function, modifier, and hotkey identities remain available.

```csharp
ui.Diagnostics.PrivacyPolicy.AllowFramebufferCapture = true;
ui.Diagnostics.ResetEventRecorder(); // commits a weaker policy for future data
```

Strengthening privacy clears buffered data that could violate the new policy. Weakening a policy does not reveal old data and takes effect only after `ResetEventRecorder`. Password protection cannot be weakened by request settings.

Provider privacy has four modes. `Default` collects normal bounded data. `RedactText` keeps structural and numeric fields but omits classified text. `RedactValues` and `ExcludeControlData` omit the provider dictionary. Request projection can remove more data but cannot recover text discarded by the session or control policy. Paths, control names and IDs, theme names, render assets, exception and artifact messages, warning/event details, and provider dictionary text use the same text policy. Warning codes, event types, stages, enum tokens, and control type names remain structural.

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
