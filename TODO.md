# FishUI - TODO

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

### High Priority

*All high priority controls have been implemented*

### Medium Priority

*All medium priority controls have been implemented*

### Lower Priority

- [ ] **Particle System** (CPX 4) - Animated particle effects for UI controls
  - Support blending modes and rectangular image particles
  - Integration with tween/animation system for control particle emission

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

## Known Issues / Bugs

### Active Bugs

*No active bugs*

### Uncategorized (Analyze and create TODO entries in above appropriate sections with priority. Do not fix or implement them just yet. Assign complexity points where applicable. Do not delete this section when you are done, just empty it)

*No uncategorized items*

---

## Notes

- Try to edit files and use tools WITHOUT POWERSHELL where possible, shell scripts get stuck and then manually terminate
- Prioritize controls that are commonly needed in game development
- Maintain the "dependency-free" philosophy - keep the core library minimal
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
