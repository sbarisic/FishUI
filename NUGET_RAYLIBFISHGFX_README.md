# RaylibFishGfx

Raylib graphics and input backend for [FishUI](https://www.nuget.org/packages/FishUI). Provides a complete, production-ready implementation using [Raylib-cs](https://www.nuget.org/packages/Raylib-cs).

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/sbarisic/FishUI/blob/master/LICENSE)

## Features

- **Complete IFishUIGfx Implementation** - All 27+ methods implemented with native Raylib calls
- **RaylibInput** - Full keyboard, mouse, and text input handling
- **Optimized** - Native 9-patch, circles, texture filtering, and scissor clipping
- **Game Loop Integration** - `UseBeginDrawing` option for existing game loops

## Installation

```
dotnet add package FishUI
dotnet add package RaylibFishGfx
```

## Quick Start

```csharp
using FishUI;
using FishUI.Controls;
using RaylibFishGfx;
using Raylib_cs;

// Create Raylib backend
var gfx = new RaylibFishGfx(1280, 720, "My App");
var input = new RaylibInput();
var events = new MyEventHandler(); // Implement IFishUIEvents

// Initialize FishUI
var settings = new FishUISettings();
var fishUI = new FishUI.FishUI(settings, gfx, input, events);
fishUI.Init();

// Load theme
settings.LoadTheme("data/themes/gwen_default.yaml", applyImmediately: true);

// Add controls
var window = new Window
{
    Position = new Vector2(100, 100),
    Size = new Vector2(400, 300),
    Title = "Hello FishUI!"
};
fishUI.AddControl(window);

var button = new Button
{
    Position = new Vector2(20, 50),
    Size = new Vector2(120, 30),
    Text = "Click Me!"
};
button.OnButtonPressed += (btn, mbtn, pos) => Console.WriteLine("Clicked!");
window.AddChild(button);

// Main loop
while (!Raylib.WindowShouldClose())
{
    float dt = Raylib.GetFrameTime();
    fishUI.Tick(dt, (float)Raylib.GetTime());
}

Raylib.CloseWindow();
```

## Integrating with Existing Game Loops

If you already have a Raylib game loop with `BeginDrawing`/`EndDrawing`:

```csharp
var gfx = new RaylibFishGfx(1280, 720, "My Game");
gfx.UseBeginDrawing = false; // Don't call BeginDrawing/EndDrawing

while (!Raylib.WindowShouldClose())
{
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.SkyBlue);
    
    // Draw your game here...
    DrawGame();
    
    // Draw UI on top
    fishUI.Tick(Raylib.GetFrameTime(), (float)Raylib.GetTime());
    
    Raylib.EndDrawing();
}
```

## Classes

### RaylibFishGfx

Complete graphics backend implementing `SimpleFishUIGfx`:

```csharp
public class RaylibFishGfx : SimpleFishUIGfx
{
    // Constructor
    public RaylibFishGfx(int width, int height, string title);
    
    // Set to false when integrating with existing game loop
    public bool UseBeginDrawing { get; set; } = true;
}
```

### RaylibInput

Input backend implementing `IFishUIInput`:

```csharp
public class RaylibInput : IFishUIInput
{
    public FishInputState GetInputState();
}
```

## Implemented Features

| Feature | Implementation |
|---------|----------------|
| Window management | `Init()`, `GetWindowWidth()`, `GetWindowHeight()`, `FocusWindow()` |
| Image loading | Full image, cropped regions, sub-images |
| Font loading | TTF fonts with size, spacing, color, monospace detection |
| Primitives | Lines, rectangles, circles (filled & outline) |
| Images | DrawTexture, DrawTexturePro, NPatch/9-slice |
| Text | MeasureText, DrawText with color/scale |
| Clipping | BeginScissorMode, EndScissorMode, push/pop stack |
| Filtering | Point (pixelated) or Trilinear (smooth) |

## Links

- [FishUI Package](https://www.nuget.org/packages/FishUI)
- [GitHub Repository](https://github.com/sbarisic/FishUI)
- [Backend Implementation Guide](https://github.com/sbarisic/FishUI/blob/master/docs/BACKEND_GUIDE.md)
- [Raylib-cs](https://github.com/ChrisDill/Raylib-cs)

## License

MIT License - see [LICENSE](https://github.com/sbarisic/FishUI/blob/master/LICENSE) for details.
