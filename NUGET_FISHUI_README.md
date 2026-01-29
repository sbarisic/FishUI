# FishUI

A dependency-free, immediate-mode-inspired GUI library for .NET applications with backend-agnostic rendering.

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/sbarisic/FishUI/blob/master/LICENSE)

## Features

- **47+ Built-in Controls** - Buttons, textboxes, sliders, data grids, charts, gauges, particles, and more
- **Backend Agnostic** - Implement your own graphics/input via simple interfaces
- **YAML Theming** - Customizable themes with atlas/9-slice support
- **Animation System** - Easing functions, tweens, and particle effects
- **Layout System** - Anchoring, margins, StackLayout, FlowLayout, GridLayout
- **Serialization** - Save/load UI layouts to YAML files
- **Virtual Cursor** - Gamepad/keyboard navigation support

## Installation

```
dotnet add package FishUI
```

For Raylib backend:
```
dotnet add package RaylibFishGfx
```

## Quick Start

```csharp
using FishUI;
using FishUI.Controls;

// 1. Implement IFishUIGfx (graphics) and IFishUIInput (input)
//    Or use RaylibFishGfx package for a ready-to-use Raylib backend

// 2. Create FishUI instance
var settings = new FishUISettings();
var fishUI = new FishUI.FishUI(settings, gfx, input, events);
fishUI.Init();

// 3. Load a theme
settings.LoadTheme("data/themes/gwen_default.yaml", applyImmediately: true);

// 4. Add controls
var button = new Button
{
    Position = new Vector2(100, 100),
    Size = new Vector2(120, 30),
    Text = "Click Me!"
};
button.OnButtonPressed += (btn, mbtn, pos) => Console.WriteLine("Clicked!");
fishUI.AddControl(button);

// 5. Update each frame
fishUI.Tick(deltaTime, totalTime);
```

## Available Controls

| Category | Controls |
|----------|----------|
| **Input** | Button, Textbox, CheckBox, RadioButton, ToggleSwitch, Slider, NumericUpDown, MultiLineEditbox |
| **Selection** | ListBox, DropDown, TreeView, SelectionBox, DatePicker, TimePicker |
| **Display** | Label, StaticText, ImageBox, AnimatedImageBox, ProgressBar, LineChart, Timeline, BigDigitDisplay, ToastNotification |
| **Containers** | Panel, Window, GroupBox, TabControl, ScrollablePane, StackLayout, FlowLayout, GridLayout |
| **Data** | DataGrid, SpreadsheetGrid, PropertyGrid, ItemListbox |
| **Gauges** | RadialGauge, BarGauge, VUMeter |
| **Effects** | ParticleEmitter |
| **Navigation** | ScrollBarV, ScrollBarH, MenuBar, ContextMenu, MenuItem |

## Implementing a Graphics Backend

Extend `SimpleFishUIGfx` for an easier implementation:

```csharp
public class MyGfx : SimpleFishUIGfx
{
    public override void Init() { /* Initialize graphics */ }
    public override int GetWindowWidth() => /* window width */;
    public override int GetWindowHeight() => /* window height */;
    
    public override ImageRef LoadImage(string path) { /* Load texture */ }
    public override FontRef LoadFont(string path, float size, float spacing, FishColor color) { /* Load font */ }
    
    public override void DrawRectangle(Vector2 pos, Vector2 size, FishColor color) { /* Draw rect */ }
    public override void DrawImage(ImageRef img, Vector2 pos, float rot, float scale, FishColor color) { /* Draw image */ }
    public override void DrawTextColorScale(FontRef fn, string text, Vector2 pos, FishColor color, float scale) { /* Draw text */ }
    public override Vector2 MeasureText(FontRef fn, string text) { /* Measure text */ }
    
    public override void BeginScissor(Vector2 pos, Vector2 size) { /* Set clip rect */ }
    public override void EndScissor() { /* Clear clip rect */ }
}
```

## Included Assets

This package includes default assets in the `data` folder:
- **Themes** - GWEN-based default theme (`data/themes/gwen_default.yaml`)
- **Fonts** - Default fonts for UI rendering
- **Images** - UI atlas and icons (Silk Icons set)

## Links

- [GitHub Repository](https://github.com/sbarisic/FishUI)
- [Documentation](https://github.com/sbarisic/FishUI/tree/master/docs)
- [Custom Controls Guide](https://github.com/sbarisic/FishUI/blob/master/docs/CUSTOM_CONTROLS.md)
- [Theming Guide](https://github.com/sbarisic/FishUI/blob/master/docs/THEMING.md)
- [Backend Implementation Guide](https://github.com/sbarisic/FishUI/blob/master/docs/BACKEND_GUIDE.md)

## License

MIT License - see [LICENSE](https://github.com/sbarisic/FishUI/blob/master/LICENSE) for details.
