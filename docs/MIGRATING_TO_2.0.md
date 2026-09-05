# Migrating to FishUI 2.0

See [Unicode, layout, and update hardening](HARDENING.md) for the current Rune input, numeric-range, catch-up, and text-metrics contracts.

FishUI 2.0 makes lifecycle, input, serialization, and backend ownership explicit.

Call `FishUI.Init()` before any tick. A successful call is idempotent. Ticks before initialization and all use after disposal throw. Dispose FishUI before disposing its injected graphics backend; FishUI never disposes injected services.

Custom controls update timers and mutable state in `OnFishUIUpdate`. They prepare geometry and runtime children in `PrepareLayout`. Keep `DrawControl` rendering-only. Use `PushScissorScope` for exception-safe clipping. Child controls paint from low to high Z depth. `AlwaysOnTop` children paint last. Picking uses the exact reverse order.

Call `TickUpdate` before the first `TickDraw`. You can draw the same prepared frame more than once. A later update prepares a new immutable frame.

Keyboard overrides should move to `HandleKeyPressed`, `HandleKeyHeld`, and `HandleKeyReleased`. The legacy callbacks remain as compatibility bridges during the 2.0 transition. Input backends must return stable touch IDs and press, motion, and release records.

`ImageRef` and `FontRef` are immutable handles. Construct them explicitly in custom backends. The backend owns the native resources referenced by those handles. `RaylibFishGfx` is disposable. Dispose FishUI first. Then dispose `RaylibFishGfx` before `Raylib.CloseWindow()`.

Layout customization now uses `FishUILayoutSerializationOptions.TypeRegistry`. Start with `FishUILayoutTypeRegistry.BuiltIn.Extend(...)`; do not mutate global tag state. `FilePickerDialog` remains runtime-only. Deserialization validates the complete graph and applies it transactionally.

FishUI and RaylibFishGfx packages now use version 2.0.0. The supported package set is centralized in `Directory.Packages.props`.
