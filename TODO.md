# FishUI - TODO

## Follow-up review

F01–F16 and their validation evidence are recorded in [DONE.md](DONE.md#2026-09-30-follow-up-fixes). The third review of `55a431d` confirmed 16 further findings, T01–T16, listed under [Active Bugs](#active-bugs); these reopen the affected completion claims. Spreadsheet cell data remains application-owned. R32 (Undo/Redo) remains deferred; R34's remaining physical-DPI checks remain open below.

## Text and rendering follow-ups

- [ ] Complex text shaping, bidirectional layout, and IME composition.
- [ ] Complete the physical DPI matrix in R34 and run the runtime suites on Linux. Actual 125% Windows rendering has been checked.

A list of planned features, improvements, and new controls for FishUI.

> **CPX (Complexity Points)** - 1 to 5 scale:
> - **1** - Single file control/component
> - **2** - Single file control/component with single function change dependencies
> - **3** - Multi-file control/component or single file with multiple dependencies, no architecture changes
> - **4** - Multi-file control/component with multiple dependencies and significant logic, possible minor architecture changes
> - **5** - Large feature spanning multiple components and subsystems, major architecture changes

> Instructions for the TODO list:
- Move all completed TODO items into a separate Completed document (DONE.md) and simplify by consolidating/combining similar ones and shortening the descriptions where possible

> How TODO file should be iterated:
- First handle the Uncategorized section, if any similar issues already are on the TODO list, increase their priority instead of adding duplicates (categorize all at once)
- When Uncategorized section is empty, start by fixing Active Bugs (take one at a time)
- After Active Bugs, handle the rest of the TODO file by priority and complexity (High priority takes precedance, then CPX points) (take one at a time).

---

## New Controls

*All controls completed - see DONE.md*

---

## Control Improvements

*All control improvements completed - see Completed section*

---

## Theme System

*All theme improvements completed - see Completed section*

---

## Core Framework Features

*All core framework features completed - see Completed section*

---

## Sample Application

*All sample demos completed - see Completed section*

---

## FishUIEditor - Layout Editor Application **HIGH PRIORITY**

A visual layout editor for designing FishUI interfaces. Located in the `FishUIEditor` project.

### Completed Phases

*Phases 1-7 completed - see Completed section (core infrastructure, selection/manipulation, property editor, control toolbox, serialization, reparenting, collection editing)*

### Phase 8: Editor Enhancements (CPX 2)

*Completed - File picker dialog implemented*

### Phase 9: Editor Rendering Refactor (CPX 3)

*Completed - DrawControlEditor/DrawChildrenEditor virtual methods, EditorCanvas refactored, container and complex control overrides (Panel, Window, GroupBox, TabControl, DataGrid)*

### Phase 10: C# Code Generation (CPX 4)

*Completed - Export layouts as C# .Designer.cs files implementing IFishUIForm*

---

## Documentation **LOW PRIORITY**

*All documentation completed - see DONE.md*

> **Note:** Getting started tutorial is covered in README.md Quick Start section
> **Note:** Custom control creation guide is in docs/CUSTOM_CONTROLS.md
> **Note:** Theme creation guide is in docs/THEMING.md
> **Note:** Backend implementation guide is in docs/BACKEND_GUIDE.md
> **Note:** Designer forms guide is in docs/FORMS_GUIDE.md

---

## Code Cleanup & Technical Debt

*All cleanup items completed - see DONE.md*

---

## Unit Testing **MEDIUM PRIORITY**

The original 78-test milestone is historical. Current regression commands and scope are recorded in [hardening notes](docs/HARDENING.md).

> Note: Tests located in UnitTest project with mock implementations for all core interfaces

---

## Known Issues / Bugs

### Active Bugs

The 2026-09-30 implementation and its validation of R01-R31 and R33 are recorded in [DONE.md](DONE.md) and the [validation notes](docs/HARDENING.md#2026-09-30-backlog-fixes). Completion evidence is historical; the open findings below identify remaining failures.

Third review baseline: `55a431d`. All 338 existing Debug tests passed, but isolated reproductions confirmed T01–T16. Local reproduction sources and results are in the ignored `artifacts/review-55a431d` directory; the generated-C# compilation reproduction is in `artifacts/review-55a431d-generated`. These findings are open and have not been fixed. No new native visual validation was performed in this review.

- [ ] **T01 — Invalid initialization requests can destroy the original layout** (P1, CPX 4). Replacement preparation defers focus and modal requests before validating them. An incoming control whose `Init` focuses an unattached textbox fails only after old roots have been removed, producing a committed-cleanup exception instead of preserving the original layout. Validate deferred requests before commit. Sources: [layout loading](FishUI/FishUI.LayoutLoading.cs), [focus handling](FishUI/FishUI.Focus.cs), and [modal handling](FishUI/FishUI.cs).
- [ ] **T02 — Reentrant tab removal deletes another tab** (P1, CPX 3). `RemoveTabAt` detaches content before removing its page entry. A focused textbox's blur callback can call `RemoveTabAt` again at the same index; a two-tab reproduction ends with no pages and one orphaned content child. Make removal reentrancy-safe and preserve page/content ownership. Source: [TabControl](FishUI/Controls/TabControl.cs).
- [ ] **T03 — Failed layout replacement loses the original modal state** (P2, CPX 3). If incoming initialization calls `RemoveAllControls`, the transaction guard rejects removal of original roots, but bulk removal still clears `ModalControl`. Rollback preserves the original root with its modal state lost. Preserve modal state on preparation failure. Sources: [bulk removal](FishUI/FishUI.cs) and [layout loading](FishUI/FishUI.LayoutLoading.cs).
- [ ] **T04 — Children added during attachment receive attachment hooks twice** (P2, CPX 3). A parent's attachment hook can add a child, immediately attaching it; the subsequent subtree traversal attaches the same child again. The reproduction records two attachment callbacks, risking duplicate subscriptions and registrations. Source: [control lifecycle](FishUI/Controls/Base/Control.Lifecycle.cs).
- [ ] **T05 — Failed tab attachment leaves a phantom page** (P2, CPX 3). Adding a tab inserts its page before attaching its content. If content attachment throws, the page remains; the reproduction has two pages but only one content child. Roll back the page insertion when attachment fails. Source: [TabControl](FishUI/Controls/TabControl.cs).
- [ ] **T06 — PropertyGrid tab-name collection edits silently do nothing** (P2, CPX 3). `TabNames` returns a copied list, while collection editing mutates that list without assigning it back. Rename reports success but the tab label remains unchanged; add and remove operations are also lost. Support collection properties that require setter write-back. Sources: [PropertyGrid](FishUI/Controls/PropertyGrid.cs) and [TabControl](FishUI/Controls/TabControl.cs).
- [ ] **T07 — Collection removal leaves stale inline editor text** (P2, CPX 2). Removing B from `[A, B, C]` keeps selected index 1, now C, without a selection-change callback. The editor still displays B and can apply that stale text to C. Refresh the editor when selected object identity changes even if its index does not. Source: [PropertyGrid](FishUI/Controls/PropertyGrid.cs).
- [ ] **T08 — Temporary PropertyGrid editors are persisted as layout children** (P2, CPX 2). Collection and vector editor panels are added as ordinary children. Saving and loading a grid restores temporary editor controls as permanent children although `SelectedObject` is excluded; the reproduction reloads a non-runtime child and persists the Remove button. Mark generated editor controls as runtime-only, including the other inline editor paths. Source: [PropertyGrid](FishUI/Controls/PropertyGrid.cs).
- [ ] **T09 — Valid Unicode text can produce uncompilable generated C#** (P2, CPX 2). String literal escaping leaves U+2028 as a literal line separator. Generating a button whose text contains `first\u2028second` fails compilation with CS1010, "Newline in constant". Escape C# line separators and other characters requiring literal-safe emission. Source: [FishCSharpWriter](FishUI/FishCSharpWriter.cs).
- [ ] **T10 — NumericUpDown geometry ignores docking and UI scale** (P2, CPX 3). A docked control with effective dimensions 600 × 32 retains an internal textbox of 104 × 24 rather than 584 × 32. At UI scale 2, arrow buttons remain 16 pixels wide instead of 32. Derive child geometry from effective logical dimensions and apply scale once. Source: [NumericUpDown](FishUI/Controls/NumericUpDown.cs).
- [ ] **T11 — Tab, tree, and scroll containers use stored rather than effective dimensions** (P2, CPX 3). A docked 600 × 400 TabControl retains 292-pixel-wide content instead of 592; a TreeView retains an unnecessary scrollbar at x = 184 despite sufficient effective space; a ScrollablePane retains a 284 × 184 viewport and two unnecessary scrollbars when its child fits the effective 600 × 400 area. Use effective logical dimensions for content and scrollbar layout. Sources: [TabControl](FishUI/Controls/TabControl.cs), [TreeView](FishUI/Controls/TreeView.cs), and [ScrollablePane](FishUI/Controls/ScrollablePane.cs).
- [ ] **T12 — Multi-select ListBox keyboard navigation leaves selection inconsistent** (P2, CPX 2). With `MultiSelect = true`, clicking item 0 then pressing Down changes `SelectedIndex` to 1 but leaves the selected-index set and highlight on item 0. Keep keyboard selection, selected indices, and displayed selection consistent. Source: [ListBox](FishUI/Controls/ListBox.cs).
- [ ] **T13 — DropDown loses its named selection callback when the typed callback detaches it** (P2, CPX 2). A typed `OnItemSelected` handler can remove the dropdown, clearing its UI reference before named-handler dispatch. The named handler then runs zero times instead of once. Capture the dispatch context and event payload before invoking callbacks. Source: [DropDown](FishUI/Controls/DropDown.cs).
- [ ] **T14 — Public multiline insertion breaks logical line structure** (P2, CPX 2). `MultiLineEditbox.InsertText("first\nsecond")` inserts the newline into one internal line, leaving one line and cursor `(0, 12)` instead of two lines and cursor `(1, 6)`. Use newline-aware insertion for the public API as well as paste, preserving one edit notification. Source: [MultiLineEditbox](FishUI/Controls/MultiLineEditbox.cs).
- [ ] **T15 — Menu persistence loses commands and parent ownership** (P2, CPX 3). Commands added through `AddMenu("File").AddItem("OpenFile")` are omitted from YAML and generated forms because the dropdown is private runtime state. Loaded menu items also lack `ParentMenuBar`; even adding a new command after loading does not make the dropdown open. Preserve menu command definitions and restore runtime ownership during reconstruction. Sources: [MenuBarItem](FishUI/Controls/MenuBarItem.cs) and [MenuBar](FishUI/Controls/MenuBar.cs).
- [ ] **T16 — ItemListbox entries disappear on save/load** (P2, CPX 2). The private `Items` field has no persisted-member declaration. A list containing one item reloads with zero items. Persist supported item definitions and restore them through the established initialization path. The separate application-owned spreadsheet-cell contract remains unchanged. Source: [ItemListbox](FishUI/Controls/ItemListbox.cs).

- [ ] **R34 — Finish physical DPI validation for rough text** (P2, CPX 3). Font atlases now use framebuffer resolution, bilinear sampling, and DPI-aware cache lookup while keeping logical metrics. The chooser was reproduced and visually checked before/after at actual 125% Windows scale (800 × 600 logical, 1000 × 750 framebuffer). Gwen/Gwen2, native editor rendering, and a Basic Controls diagnostic capture were checked. Physical 100%, 150%, 200%, and live monitor/DPI transitions remain unverified; keep this finding open until those checks pass. Local captures are in the ignored `artifacts/render-validation` directory.
- [ ] **R32 — Editor Undo and Redo are no-ops** (P3, CPX 4; deferred by request). Leave the commands and implementation unchanged for this revision.

### Uncategorized (Analyze and create TODO entries in above appropriate sections with priority. Do not fix or implement them just yet. Assign complexity points where applicable. Do not delete this section when you are done, just empty it)

*No uncategorized items*

> Note: Input control testing with simulated mouse clicks is already covered in EventSystemTests (Button_OnButtonPressed_EventFires, CheckBox_OnCheckedChanged_EventFires, etc.)

---

## Notes

- Try to edit files and use tools WITHOUT POWERSHELL where possible, shell scripts get stuck and then manually terminate
- Prioritize controls that are commonly needed in game development
- Keep the core library small; YamlDotNet remains the serialization dependency
- Backend implementations should remain in separate sample projects
- Do not be afraid to break backwards compatibility if new changes will simplify or improve the project
- CEGUI theme files in `data/cegui_theme/` provide reference for accurate atlas coordinates
- The GWEN skin atlas (gwen.png) contains 512x512 pixels of UI elements
- Do not use powershell commands unless absolutely necessary

### Available Theme Assets in gwen.png (Reference)
- Windows: `Window.Head/Middle/Bottom.Normal/Inactive` (9-slice), Close button states
- Tabs: `Tab.Top/Bottom/Left/Right.Active/Inactive`, `Tab.Control`, `Tab.HeaderBar`
- Menus: `Menu.Strip`, `Menu.Background/Hover` (9-slice), `Menu.Check`, arrows
- Misc: `Tooltip`, `GroupBox.Normal`, `Tree` + Plus/Minus, `StatusBar`, `CategoryList`, `Shadow`, `Selection`
