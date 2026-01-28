# Copilot Instructions

## Project Guidelines

### TODO.md Management
- Move all completed TODO items into the Completed section, consolidating similar ones and shortening descriptions
- When handling Uncategorized items: categorize all at once, increase priority of similar existing items instead of adding duplicates
- Work order: Uncategorized → Active Bugs → High priority items → Lower CPX items

### Code Style
- Avoid PowerShell commands - they get stuck; use tools and file operations instead
- Maintain "dependency-free" philosophy - keep core library minimal
- Backend implementations should remain in separate sample projects
- Breaking backwards compatibility is acceptable if it simplifies or improves the project

### Project Structure
- FishUI: Core library (no external dependencies)
- FishUIDemos: Sample applications with Raylib backend
- FishUIEditor: Visual layout editor for designing interfaces
- Theme files: CEGUI files in `data/cegui_theme/` for atlas coordinates reference
- GWEN skin atlas (gwen.png): 512x512 pixels of UI elements