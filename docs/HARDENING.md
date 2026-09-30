# Unicode, layout, and update hardening

## 2026-09-30 follow-up fixes

The second review's F01–F16 fixes cover collection editing, removal, layout replacement, generated state, selection, and logical sizing. R32 remains deferred. R34 still needs physical 100%, 150%, 200%, and live monitor/DPI transition checks.

- PropertyGrid edits existing collection objects and preserves metadata and identity. Selection does not write back. Public setters, readonly fields, `ReadOnly`, `Browsable`, and YAML exclusion metadata govern editing and reset. Font/theme changes and resizing retain the selected object and current editor text.
- Root, child, bulk removal, and reparenting finish framework cleanup before reporting callback errors as `AggregateException`. A failed source detachment does not attach to the destination. Hooks whose attachment fails also receive a cleanup attempt. Capture or hierarchy changes within a subtree being removed are rejected.
- Runtime layout loading validates, runs recursive `OnDeserialized` once per root, attaches, and runs normal initialization before committing. Preparation failures remove incoming controls and registrations and retain the original layout, order, focus, and modal state. Nested replacements and hierarchy mutations during transaction cleanup are rejected. After commit, all old roots are removed even when cleanup fails.
- `FishUILayoutCleanupException : AggregateException` reports failures after replacement commits; `LayoutCommitted` is always `true`. `DeserializeFromFile` still sends the layout-loaded notification. Treat this exception as a successful load with cleanup errors: retain the new document/path and report the errors separately. Other preparation exceptions leave the old document active. Arbitrary external side effects in application callbacks cannot be rolled back.
- Generated forms retain direct C# initialization, protected fields, and `IFishUIForm`. YAML and C# generation share persisted-member rules. Collections and hierarchy precede dependent selection; resources use recursive deserialization initialization once. Unsupported persisted values fail generation with their member path. Spreadsheet dimensions/settings persist; cell contents remain application-owned and must be restored separately.
- Root and child `ZDepth` survive restoration, with insertion order retained for equal depths. Later public additions allocate beyond existing depths. Numeric precision changes refresh text without changing the value or notifying value handlers; supported precision is 0–99 decimal places.
- ListBox supports selection before attachment and `SelectedIndex = -1`. ListBox and DataGrid cleared-selection payloads use index `-1` and a null item/row. TreeView removal can notify with a null node; tab removal can leave index `-1`. Subscribers must handle clearing. Event payloads are captured before callbacks; multiline replacement sends one final change and no event for a no-op.
- Anchor baselines and layout-container dimensions use effective logical sizes, dividing UI scale once. Valid serialized baselines remain intact. Old ambiguous baselines are not guessed or migrated; applications can explicitly recalculate them. Stack, grid, and flow arrangements respond to effective resizing and logical UI scales independently of display DPI.

Validation evidence for this batch is recorded in [DONE.md](../DONE.md). Native captures are retained locally in the ignored `artifacts/followup-native` directory: chooser, editor save/load and nested removal, collection editing before/after theme/font/size changes, and file navigation. Actual Windows scale was 125% (800 × 600 logical / 1000 × 750 framebuffer; editor 1600 × 900 / 2000 × 1125). These checks do not close R34's remaining matrix or establish Unity player/Linux/macOS behavior.

## 2026-09-30 backlog fixes

The follow-up implementation closes R01-R31 and R33 from the code review. R32 (Undo/Redo) is deferred. R34 has a font-rendering fix verified at actual 125% Windows display scale, but remains open for physical 100%, 150%, 200%, and live DPI/monitor-transition checks. Package-consumer validation used local packages; it does not establish NuGet publication.

Compatibility and behavior changes:

- TreeView and ListBox now default to keyboard focusable. Explicit `Focusable = false` remains supported and survives YAML round trips.
- Layout serialization retains explicit zero/false values and excludes computed properties. YAML output is larger. Existing valid layouts remain readable; unknown properties and unregistered control types remain errors. `TabEnabled` is optional metadata; omitted enabled states default to true.
- TreeNode additions reject self-links, ancestor cycles, and existing ownership. Remove a node from its parent/tree before moving it. Repeated addition to the same owner is idempotent.
- Drag callbacks receive logical movement. Window resize and titlebar movement use the same conversion. Fonts loaded during theme changes respect the configured UI scale.
- Disposal attempts cleanup before throwing an `AggregateException` for callback failures. A second disposal is a no-op. Injected graphics/input services remain caller-owned.
- Built-in drawing applies each control's opacity once. Parent opacity is not inherited by child controls. Custom drawing code can use the protected `ApplyOpacity` helper.
- The Raylib backend keeps logical font sizes and line heights while resolving physical font atlases for the current framebuffer scale. Cached font handles remain usable when the scale changes. Latin Extended A/B and General Punctuation are requested explicitly; unsupported font characters still use fallback glyphs. Complex shaping, bidi, and IME remain separate work.
- Editor layout replacement prepares the new controls before changing the document; generated forms preserve common control state and named handlers. PropertyGrid supports public fields in addition to properties, respecting read-only and exclusion metadata.

Validation commands (run builds/tests sequentially):

```text
dotnet build FishUI.sln -c Debug --no-restore -v minimal
dotnet test FishUI.sln -c Debug --no-build --no-restore -v minimal
dotnet build FishUI.sln -c Release --no-restore -v minimal
dotnet test FishUI.sln -c Release --no-build --no-restore -v minimal
pwsh -NoProfile -File scripts/Test-Documentation.ps1
pwsh -NoProfile -File scripts/Test-UnitySourceLinks.ps1
pwsh -NoProfile -File scripts/Test-LocalPackages.ps1 -Configuration Release
```

Debug and Release each passed 304 tests (303 in UnitTest and 1 in FishUI_UnitTest); solution builds reported zero warnings/errors. BacklogRegressionTests supplies 27 cases and DesignerExecutionTests compiles and instantiates an exported form. The package check uses an isolated package cache and fresh output directories to verify all asset-copy modes. The heatmap regression checks one storage validation per scan rather than relying on timing thresholds.

Native Windows validation reproduced the rough chooser text with an 800 × 600 logical window and a 1000 × 750 framebuffer, then inspected the corrected rendering. Gwen/Gwen2 theme changes, editor rendering and save/load, Croatian glyph lookup, and a Basic Controls diagnostic screenshot/overlay completed. Local evidence is under the ignored `artifacts/render-validation` and sample diagnostic output directories. These checks do not establish other display scales, live DPI transitions, Unity player behavior, Linux, or macOS runtime correctness.

The subsequent [codebase revision](REVISION_2026_09_09.md) records further fixes, validation, and the `DockMode` numeric compatibility change.

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
