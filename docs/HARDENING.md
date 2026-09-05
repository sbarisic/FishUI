# Unicode, layout, and update hardening

This revision fixes the September 2026 audit findings. It retains the .NET 9 runtime, backend ownership, YAML control tags, and update/draw split. It does not restore the removed GitHub Actions workflow.

## Input and editing

Custom controls override `HandleTextInput(FishUI, FishInputState, System.Text.Rune)`. Text filters implement the corresponding Rune signature. The char overload remains a convenience for callers; overrides must migrate to Rune. Backend `GetCharPressed` returns a Unicode scalar as an integer. Invalid scalars or malformed UTF-16 text become U+FFFD.

The linked `UnityFishUI` .NET Standard 2.1 build uses `FishUI.Compatibility.UnicodeScalar` for the same input contract because that reference framework has no public `System.Text.Rune`. Its scalar conversion and replacement rules match the modern runtime. Grapheme segmentation uses the host's `StringInfo`; Unity runtime Unicode tables can differ from .NET 9. The solution build verifies source compatibility; Unity editor/player execution remains a separate platform check.

Textbox and multiline editor cursor positions remain UTF-16 offsets. Movement, deletion, selection consumption, length truncation, and wrapping use Unicode grapheme boundaries. `TextElements` provides the shared operations. Complex shaping, bidirectional layout, and IME composition are not implemented by this change.

Equal tab indices retain hierarchy paint traversal order. Checkbox and radio labels prepare their positions before input; drawing no longer changes their layout.

## Numeric controls and time

Slider, NumericUpDown, BarGauge, and RadialGauge implement `IFishUINumericRange`. Call `SetRange(minimum, maximum)` when changing both bounds. Bounds and values must be finite; minimum must not exceed maximum. A range change clamps the current value and emits at most one existing value-change notification. Slider steps are relative to the minimum; zero selects continuous movement.

The layout loader stages numeric properties while parsing and validates the complete range before attachment. YAML property order does not affect the resulting value. Invalid ranges leave the current UI graph unchanged.

Frame deltas and timestamps must be finite. Animation frame rates must be finite and positive. Particle emission rates must be finite and nonnegative; zero disables automatic emission. Particle capacity must be nonnegative, and zero disables particle creation.

AnimatedImageBox stops at completion, including completion callbacks that modify playback. Each update replays at most 256 frame transitions, then advances excess time arithmetically and coalesces the final frame notification. Completion fires once per completed playback. Particle updates create at most the configured capacity; older emissions that would immediately be discarded are skipped.

## Backend text metrics

`IFishUIGfx.TryMeasureTextAdvances` optionally fills two buffers of `text.Length + 1` floats: prefix advances by UTF-16 offset, and the kerning/spacing immediately before each scalar. The layout engine subtracts the leading adjustment when beginning a wrapped line. Backends that return false use a grapheme-safe binary search over `MeasureText`.

`GetTextMetricsVersion` invalidates cached layout when backend metrics change. The default is zero for stable metrics. Diagnostic wrappers forward both methods. Font handles, text, spacing, size, viewport dimensions, and UI scale participate in layout invalidation.

The FishGfx backend supplies prefix metrics without rasterizing glyphs. The Raylib backend continues to use its native text measurement through the fallback contract. No new native resource ownership moves into FishUI.

## Validation

Run `dotnet test UnitTest -c Debug` and then `dotnet test UnitTest -c Release`. `AuditRegressionTests` covers scalar input, grapheme deletion, malformed input, YAML range order, tab order, completion, and bounded catch-up. Existing lifecycle, forms, diagnostics, serialization, and rendering tests remain required. Run `pwsh -NoProfile -File scripts/Test-Documentation.ps1` for local links.

The 2.0 coverage figures in CODEBASE_AUDIT.md describe the historical audit. This revision does not claim a new coverage percentage or Linux/high-DPI acceptance from Windows unit tests.

The September implementation run passed 245 tests in both Debug and Release. Set `FISHUI_CAPTURE_ROOT` to an absolute directory before running `FishUISample --sample 7 --frames 30 --capture-diagnostics` to keep sample screenshots outside tracked content. Diagnostic bundles remain under the application's output directory.

## Documentation audit

All 16 existing first-party Markdown files were reviewed. README, package README descriptions, migration/runtime/backend/custom-control guides, diagnostics contracts, TODO/DONE, the historical audit, and contributor metadata were reconciled where affected. FORMS_GUIDE and THEMING retain their existing contracts; DIAGNOSTIC_CONTROL_COVERAGE retains its provider inventory. TODO_EMPTY is a reusable template, not the live backlog. Historical accomplishments and package usage examples do not certify that this source revision has been published to NuGet.

Both Debug and Release solution builds passed without warnings. The Raylib diagnostic sample completed with screenshot and overlay capture in both application-owned and backend-owned frame modes. The package vulnerability audit reported no vulnerable packages from its configured sources. These are Windows checks; Unity editor/player, Linux, macOS, IME and a multi-monitor DPI matrix were not exercised.
