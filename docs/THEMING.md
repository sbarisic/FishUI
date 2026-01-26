# Creating Themes in FishUI

This guide explains how to create custom themes for FishUI. Themes control the visual appearance of all controls, including colors, fonts, and sprite atlas regions for styled rendering.

## Table of Contents

1. [Theme File Structure](#theme-file-structure)
2. [Quick Start: Minimal Theme](#quick-start-minimal-theme)
3. [Theme Metadata](#theme-metadata)
4. [Colors](#colors)
5. [Fonts](#fonts)
6. [Atlas and Regions](#atlas-and-regions)
7. [Theme Inheritance](#theme-inheritance)
8. [Loading Themes](#loading-themes)
9. [Runtime Theme Switching](#runtime-theme-switching)
10. [Creating a Sprite Atlas](#creating-a-sprite-atlas)
11. [Complete Theme Example](#complete-theme-example)

---

## Theme File Structure

FishUI themes are defined in YAML files with the following structure:

```yaml
theme:
  name: "My Theme"
  description: "Description of the theme"
  version: "1.0"
  author: "Your Name"
  inherits: "parent_theme.yaml"  # Optional

atlas:
  enabled: true
  path: "data/my_atlas.png"

colors:
  background: "#F0F0F0"
  foreground: "#000000"
  accent: "#4A90D9"
  # ... more colors

fonts:
  defaultpath: "data/fonts/myfont.ttf"
  defaultsize: 14
  # ... more font settings

regions:
  Button:
    Normal:
      x: 0
      y: 0
      width: 32
      height: 32
      # ... 9-slice borders
    Hover:
      # ...
  # ... more control regions
```

---

## Quick Start: Minimal Theme

The simplest theme just overrides colors from a parent theme:

```yaml
theme:
  name: "Dark Mode"
  description: "A dark color scheme"
  inherits: "gwen.yaml"

colors:
  background: "#2D2D30"
  foreground: "#FFFFFF"
  accent: "#0078D4"
  border: "#3F3F46"
```

This inherits all atlas regions and fonts from `gwen.yaml` while only changing colors.

---

## Theme Metadata

The `theme` section contains identifying information:

```yaml
theme:
  name: "My Custom Theme"       # Display name
  description: "A beautiful theme"  # Description
  version: "1.0"                # Version string
  author: "Your Name"           # Author
  inherits: "base_theme.yaml"   # Optional parent theme
```

### Inheritance

When `inherits` is specified, the theme loads the parent first, then applies any overrides. This allows you to:
- Create color variants without duplicating atlas definitions
- Override specific controls while keeping others
- Build theme families from a base theme

---

## Colors

The `colors` section defines the color palette:

```yaml
colors:
  # Core colors (required)
  background: "#F0F0F0"      # Default background
  foreground: "#000000"      # Default text color
  accent: "#4A90D9"          # Primary accent/highlight
  accentsecondary: "#6CB0F0" # Secondary accent
  disabled: "#A0A0A0"        # Disabled control text
  error: "#DC3232"           # Error states
  success: "#32B432"         # Success states
  warning: "#FFB400"         # Warning states
  border: "#B4B4B4"          # Border color
  
  # Custom colors (optional)
  custom:
    buttontext: "#000000"
    highlight: "#E0E8F0"
    linkcolor: "#0066CC"
```

### Color Format

Colors can be specified as:
- Hex with alpha: `#RRGGBBAA` (e.g., `#FF0000FF` for opaque red)
- Hex without alpha: `#RRGGBB` (e.g., `#FF0000` for red, assumes 255 alpha)

### Accessing Colors in Code

```csharp
// Get the color palette
FishUIColorPalette palette = ui.Settings.GetColorPalette();

// Access colors
FishColor bg = palette.Background;
FishColor fg = palette.Foreground;
FishColor accent = palette.Accent;

// Access custom colors
FishColor custom = palette.GetCustom("buttontext");
```

---

## Fonts

The `fonts` section defines typography:

```yaml
fonts:
  defaultpath: "data/fonts/ubuntu_mono.ttf"  # Default font file
  boldpath: "data/fonts/ubuntu_mono_bold.ttf" # Bold variant
  defaultsize: 14                             # Default font size
  labelsize: 14                               # Label font size
  spacing: 0                                  # Character spacing
```

### Font Loading

FishUI loads fonts through the graphics backend. Ensure font files exist at the specified paths relative to your application's working directory.

---

## Atlas and Regions

The atlas system enables 9-slice (NPatch) rendering for scalable control graphics.

### Atlas Configuration

```yaml
atlas:
  enabled: true              # Enable atlas-based rendering
  path: "data/my_atlas.png"  # Path to sprite atlas image
```

### Region Definitions

Regions define where each control state's graphics are located in the atlas:

```yaml
regions:
  Button:
    Normal:
      x: 480        # X position in atlas
      y: 0          # Y position in atlas
      width: 31     # Width of region
      height: 31    # Height of region
      top: 2        # 9-slice top border (pixels that don't stretch)
      bottom: 2     # 9-slice bottom border
      left: 2       # 9-slice left border
      right: 2      # 9-slice right border
    Hover:
      x: 480
      y: 32
      width: 31
      height: 31
      top: 2
      bottom: 2
      left: 2
      right: 2
    Pressed:
      x: 480
      y: 96
      width: 31
      height: 31
      top: 2
      bottom: 2
      left: 2
      right: 2
    Disabled:
      x: 480
      y: 64
      width: 31
      height: 31
      top: 2
      bottom: 2
      left: 2
      right: 2
```

### 9-Slice (NPatch) Rendering

The `top`, `bottom`, `left`, and `right` values define the corners that don't stretch:

```
+-------+---------------+-------+
|  TL   |      Top      |  TR   |  <- top border (fixed)
+-------+---------------+-------+
|       |               |       |
| Left  |    Center     | Right |  <- stretches vertically
|       |    (scales)   |       |
+-------+---------------+-------+
|  BL   |    Bottom     |  BR   |  <- bottom border (fixed)
+-------+---------------+-------+
   ^                        ^
   |                        |
 left                     right
(fixed)                  (fixed)
```

### Supported Control Regions

Each control looks for specific region names:

| Control | States |
|---------|--------|
| Button | Normal, Hover, Pressed, Disabled |
| Panel | Normal, Disabled, Bright, Dark, Highlight |
| Checkbox | Unchecked, Checked, UncheckedHover, CheckedHover, DisabledUnchecked, DisabledChecked |
| RadioButton | Unchecked, Checked, UncheckedHover, CheckedHover, DisabledUnchecked, DisabledChecked |
| Textbox | Normal, Focused, Disabled |
| ListBox | Normal, Disabled |
| DropDown | Normal, Hover, Pressed, Disabled |
| ScrollBarV | Track, Thumb, ThumbHover, ButtonUp, ButtonDown |
| ScrollBarH | Track, Thumb, ThumbHover, ButtonLeft, ButtonRight |
| ProgressBar | Background, Fill |
| Slider | Track, Thumb |
| Window | Background, Titlebar, CloseButton, CloseButtonHover |
| TabControl | Tab, TabActive, TabHover, Background |
| GroupBox | Normal |
| TreeView | Background, NodeExpanded, NodeCollapsed |
| MenuBar | Background, ItemHover |
| ContextMenu | Background, ItemHover, Separator |
| Tooltip | Background |

### Using Individual Images Instead of Atlas

For controls that use separate image files instead of atlas regions:

```yaml
regions:
  Button:
    Normal:
      imagePath: "data/images/button_normal.png"
      top: 4
      bottom: 4
      left: 4
      right: 4
```

---

## Theme Inheritance

Theme inheritance allows you to create variations without duplicating the entire theme:

### Parent Theme (base.yaml)
```yaml
theme:
  name: "Base Theme"

atlas:
  enabled: true
  path: "data/base_atlas.png"

colors:
  background: "#FFFFFF"
  foreground: "#000000"
  accent: "#0078D4"

regions:
  Button:
    Normal:
      x: 0
      y: 0
      width: 32
      height: 32
      # ...
```

### Child Theme (dark.yaml)
```yaml
theme:
  name: "Dark Theme"
  inherits: "base.yaml"

colors:
  background: "#1E1E1E"
  foreground: "#FFFFFF"
  accent: "#0078D4"

# Inherits all regions from base.yaml
# Can override specific regions if needed:
regions:
  Button:
    Normal:
      x: 100  # Override just this one region
      y: 0
      width: 32
      height: 32
```

---

## Loading Themes

### At Initialization

```csharp
FishUISettings settings = new FishUISettings();
FishUI ui = new FishUI(settings, gfx, input, events);
ui.Init();

// Load and apply theme
settings.LoadTheme("data/themes/my_theme.yaml", applyImmediately: true);
```

### Without Immediate Application

```csharp
// Load theme without applying
FishUITheme theme = settings.LoadTheme("data/themes/my_theme.yaml", applyImmediately: false);

// Apply later
settings.ApplyTheme(theme);
```

---

## Runtime Theme Switching

FishUI supports switching themes at runtime:

```csharp
// Load multiple themes
FishUITheme lightTheme = settings.LoadTheme("data/themes/light.yaml");
FishUITheme darkTheme = settings.LoadTheme("data/themes/dark.yaml");

// Switch themes
void SwitchToLight()
{
    settings.ApplyTheme(lightTheme);
}

void SwitchToDark()
{
    settings.ApplyTheme(darkTheme);
}
```

### Theme Switcher Example

See `FishUIDemos/Samples/SampleThemeSwitcher.cs` for a complete example of runtime theme switching with a UI for selecting themes.

---

## Creating a Sprite Atlas

### Atlas Requirements

1. **Single image file**: PNG format recommended
2. **Power of 2 dimensions**: 256x256, 512x512, 1024x1024, etc. (not required but recommended)
3. **Consistent spacing**: Leave a small gap between regions to prevent bleeding

### Layout Tips

1. **Group related states together**: Button Normal, Hover, Pressed, Disabled in a row
2. **Use consistent sizes**: All button states same size makes theme files cleaner
3. **Leave padding**: 1-2px gap between regions prevents texture bleeding

### Reference Atlas

The included `gwen.png` atlas (512x512) provides a complete set of control graphics. Use the CEGUI reference files in `data/cegui_theme/` for exact coordinates:
- `GWEN.imageset.xml` - Region coordinates
- `GWEN.looknfeel.xml` - Control styling reference

---

## Complete Theme Example

Here's a complete theme file for reference:

```yaml
# FishUI Theme File - Custom Dark Theme
theme:
  name: "Custom Dark"
  description: "A modern dark theme"
  version: "1.0"
  author: "Your Name"

atlas:
  enabled: true
  path: "data/custom_dark_atlas.png"

colors:
  background: "#1E1E1E"
  foreground: "#E0E0E0"
  accent: "#0078D4"
  accentsecondary: "#106EBE"
  disabled: "#6D6D6D"
  error: "#F44747"
  success: "#4EC9B0"
  warning: "#CCA700"
  border: "#3C3C3C"
  custom:
    buttontext: "#FFFFFF"
    highlight: "#264F78"
    selection: "#094771"

fonts:
  defaultpath: "data/fonts/segoe_ui.ttf"
  boldpath: "data/fonts/segoe_ui_bold.ttf"
  defaultsize: 13
  labelsize: 13
  spacing: 0

regions:
  Button:
    Normal:
      x: 0
      y: 0
      width: 48
      height: 24
      top: 4
      bottom: 4
      left: 4
      right: 4
    Hover:
      x: 0
      y: 24
      width: 48
      height: 24
      top: 4
      bottom: 4
      left: 4
      right: 4
    Pressed:
      x: 0
      y: 48
      width: 48
      height: 24
      top: 4
      bottom: 4
      left: 4
      right: 4
    Disabled:
      x: 0
      y: 72
      width: 48
      height: 24
      top: 4
      bottom: 4
      left: 4
      right: 4

  Panel:
    Normal:
      x: 48
      y: 0
      width: 32
      height: 32
      top: 4
      bottom: 4
      left: 4
      right: 4

  Textbox:
    Normal:
      x: 80
      y: 0
      width: 32
      height: 24
      top: 2
      bottom: 2
      left: 2
      right: 2
    Focused:
      x: 80
      y: 24
      width: 32
      height: 24
      top: 2
      bottom: 2
      left: 2
      right: 2

  Checkbox:
    Unchecked:
      x: 112
      y: 0
      width: 16
      height: 16
      top: 2
      bottom: 2
      left: 2
      right: 2
    Checked:
      x: 112
      y: 16
      width: 16
      height: 16
      top: 2
      bottom: 2
      left: 2
      right: 2

  # Add more control regions as needed...
```

---

## Tips and Best Practices

1. **Start with inheritance**: Create new themes by inheriting from `gwen.yaml` and overriding only what you need.

2. **Test incrementally**: Load your theme and test each control type to ensure regions are correct.

3. **Use consistent 9-slice borders**: Keep border values consistent within a control for predictable scaling.

4. **Mind the gaps**: When creating atlases, leave small gaps between regions to prevent edge bleeding.

5. **Document your atlas**: Keep a reference image or document showing where each region is located.

6. **Consider accessibility**: Ensure sufficient contrast between foreground and background colors.

7. **Test at different scales**: Verify your theme looks good at various UI scale factors.

---

## See Also

- [README.md](../README.md) - Quick start and theming overview
- [docs/CUSTOM_CONTROLS.md](CUSTOM_CONTROLS.md) - Custom control creation with theme support
- [FishUIDemos/Samples/SampleThemeSwitcher.cs](../FishUIDemos/Samples/SampleThemeSwitcher.cs) - Runtime theme switching example
- [data/themes/gwen.yaml](../FishUI/data/themes/gwen.yaml) - Complete reference theme
- [data/cegui_theme/](../FishUI/data/cegui_theme/) - CEGUI reference files for atlas coordinates
