# Implementing a Custom Graphics Backend for FishUI

This guide explains how to create a custom graphics backend for FishUI. FishUI is designed to be renderer-agnostic, allowing you to use it with any graphics library (Raylib, MonoGame, SDL, OpenGL, etc.).

## Table of Contents

1. [Overview](#overview)
2. [Quick Start with SimpleFishUIGfx](#quick-start-with-simplefishuigfx)
3. [Minimum Required Methods](#minimum-required-methods)
4. [Optional Overrides for Performance](#optional-overrides-for-performance)
5. [Complete Example: Raylib Backend](#complete-example-raylib-backend)
6. [Implementing IFishUIGfx Directly](#implementing-ifishuigfx-directly)
7. [Input Backend](#input-backend)
8. [Best Practices](#best-practices)

---

## Overview

FishUI separates rendering from the UI logic through the `IFishUIGfx` interface. To use FishUI with your graphics library, you need to implement this interface.

There are two approaches:

1. **SimpleFishUIGfx** (Recommended) - Extend this abstract class that provides default implementations for many methods. You only need to implement ~10 core methods.

2. **IFishUIGfx** (Direct) - Implement the full interface with 25+ methods for maximum control and performance.

---

## Quick Start with SimpleFishUIGfx

The easiest way to create a backend is to extend `SimpleFishUIGfx`:

```csharp
using FishUI;
using System.Numerics;

public class MyGfx : SimpleFishUIGfx
{
    public override void Init()
    {
        // Initialize your graphics system
    }

    public override int GetWindowWidth() => /* your window width */;
    public override int GetWindowHeight() => /* your window height */;

    public override ImageRef LoadImage(string FileName)
    {
        // Load image and return ImageRef with Width, Height, and Userdata
        return new ImageRef
        {
            Path = FileName,
            Width = /* image width */,
            Height = /* image height */,
            Userdata = /* your native texture/image object */
        };
    }

    public override FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color)
    {
        // Load font and return FontRef
        return new FontRef
        {
            Path = FileName,
            Size = Size,
            Spacing = Spacing,
            Color = Color,
            Userdata = /* your native font object */
        };
    }

    public override void BeginScissor(Vector2 Pos, Vector2 Size)
    {
        // Set scissor/clipping rectangle
    }

    public override void EndScissor()
    {
        // Clear scissor/clipping
    }

    public override void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr)
    {
        // Draw a line
    }

    public override void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color)
    {
        // Draw a filled rectangle
    }

    public override void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color)
    {
        // Draw an image (Img.Userdata contains your native texture)
    }

    public override Vector2 MeasureText(FontRef Fn, string Text)
    {
        // Measure text size and return dimensions
        return new Vector2(/* width */, /* height */);
    }

    public override void DrawTextColorScale(FontRef Fn, string Text, Vector2 Pos, FishColor Color, float Scale)
    {
        // Draw text with color and scale (Fn.Userdata contains your native font)
    }
}
```

---

## Minimum Required Methods

When extending `SimpleFishUIGfx`, you must implement these methods:

| Method | Purpose |
|--------|---------|
| `Init()` | Initialize your graphics system |
| `GetWindowWidth()` | Return current window width |
| `GetWindowHeight()` | Return current window height |
| `LoadImage(string)` | Load an image file into an `ImageRef` |
| `LoadFont(...)` | Load a font file into a `FontRef` |
| `BeginScissor(...)` | Set scissor/clipping rectangle |
| `EndScissor()` | Clear scissor/clipping |
| `DrawLine(...)` | Draw a line between two points |
| `DrawRectangle(...)` | Draw a filled rectangle |
| `DrawImage(...)` | Draw an image with rotation and scale |
| `MeasureText(...)` | Measure text dimensions |
| `DrawTextColorScale(...)` | Draw text with color and scale |

### ImageRef Structure

When loading images, populate these fields:

```csharp
public override ImageRef LoadImage(string FileName)
{
    var texture = YourGraphicsLib.LoadTexture(FileName);
    
    return new ImageRef
    {
        Path = FileName,
        Width = texture.Width,
        Height = texture.Height,
        Userdata = texture,      // Store your native texture here
        Userdata2 = null         // Optional: store CPU-side image data for GetImageColor
    };
}
```

### FontRef Structure

When loading fonts, populate these fields:

```csharp
public override FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color)
{
    var font = YourGraphicsLib.LoadFont(FileName, (int)Size);
    
    return new FontRef
    {
        Path = FileName,
        Size = Size,
        Spacing = Spacing,
        Color = Color,
        Userdata = font,         // Store your native font here
        LineHeight = Size,       // Line height in pixels
        IsMonospaced = false     // Set true for fixed-width fonts
    };
}
```

---

## Optional Overrides for Performance

`SimpleFishUIGfx` provides default implementations for these methods, but you can override them for better performance:

| Method | Default Implementation | Override For |
|--------|----------------------|--------------|
| `BeginDrawing(float)` | Does nothing | Frame begin (clear screen, etc.) |
| `EndDrawing()` | Does nothing | Frame end (swap buffers, etc.) |
| `DrawRectangleOutline(...)` | 4 `DrawLine` calls | Native outline drawing |
| `DrawCircle(...)` | Polygon approximation | Native filled circle |
| `DrawCircleOutline(...)` | Polygon with lines | Native circle outline |
| `DrawNPatch(...)` | 9 `DrawImageRegion` calls | Native 9-slice support |
| `DrawImage(with Size)` | Calculates scale | Proper source rect drawing |
| `DrawText(...)` | Calls `DrawTextColor` | Direct text drawing |
| `DrawTextColor(...)` | Calls `DrawTextColorScale` | Direct colored text |
| `PushScissor(...)` | Stack with intersection | Native scissor stack |
| `PopScissor()` | Stack-based | Native scissor stack |
| `GetFontMetrics(...)` | Estimates from `MeasureText` | Accurate font metrics |
| `SetImageFilter(...)` | Does nothing | Texture filtering |
| `GetImageColor(...)` | Throws | Pixel color reading |

### Example: Overriding DrawNPatch for Raylib

```csharp
public override void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color, float Rotation)
{
    Texture2D tex = (Texture2D)NP.Image.Userdata;
    NPatchInfo info = new NPatchInfo
    {
        Left = NP.Left,
        Right = NP.Right,
        Top = NP.Top,
        Bottom = NP.Bottom,
        Source = new Rectangle(NP.ImagePos, NP.ImageSize),
        Layout = NPatchLayout.NinePatch
    };

    Raylib.DrawTextureNPatch(tex, info, new Rectangle(Pos, Size), Vector2.Zero, Rotation,
        new Color(Color.R, Color.G, Color.B, Color.A));
}
```

---

## Complete Example: Raylib Backend

Here's a complete minimal Raylib backend using `SimpleFishUIGfx`:

```csharp
using FishUI;
using Raylib_cs;
using System;
using System.Numerics;

public class RaylibGfxMinimal : SimpleFishUIGfx
{
    private int _width, _height;
    private string _title;

    public RaylibGfxMinimal(int width, int height, string title)
    {
        _width = width;
        _height = height;
        _title = title;
    }

    #region Required Overrides

    public override void Init()
    {
        Raylib.InitWindow(_width, _height, _title);
        Raylib.SetTargetFPS(60);
    }

    public override int GetWindowWidth() => Raylib.GetScreenWidth();
    public override int GetWindowHeight() => Raylib.GetScreenHeight();

    public override ImageRef LoadImage(string FileName)
    {
        Texture2D tex = Raylib.LoadTexture(FileName);
        return new ImageRef
        {
            Path = FileName,
            Width = tex.Width,
            Height = tex.Height,
            Userdata = tex
        };
    }

    public override FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color)
    {
        Font font = Raylib.LoadFontEx(FileName, (int)Size, null, 250);
        return new FontRef
        {
            Path = FileName,
            Size = Size,
            Spacing = Spacing,
            Color = Color,
            Userdata = font,
            LineHeight = font.BaseSize
        };
    }

    public override void BeginScissor(Vector2 Pos, Vector2 Size)
    {
        Raylib.BeginScissorMode((int)Pos.X, (int)Pos.Y, (int)Size.X, (int)Size.Y);
    }

    public override void EndScissor()
    {
        Raylib.EndScissorMode();
    }

    public override void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr)
    {
        Raylib.DrawLineEx(Pos1, Pos2, Thick, ToColor(Clr));
    }

    public override void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color)
    {
        Raylib.DrawRectangleV(Position, Size, ToColor(Color));
    }

    public override void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color)
    {
        Texture2D tex = (Texture2D)Img.Userdata;
        Raylib.DrawTextureEx(tex, Pos, Rot, Scale, ToColor(Color));
    }

    public override Vector2 MeasureText(FontRef Fn, string Text)
    {
        Font font = (Font)Fn.Userdata;
        return Raylib.MeasureTextEx(font, Text, Fn.Size, Fn.Spacing);
    }

    public override void DrawTextColorScale(FontRef Fn, string Text, Vector2 Pos, FishColor Color, float Scale)
    {
        Font font = (Font)Fn.Userdata;
        Raylib.DrawTextEx(font, Text, Pos, Fn.Size * Scale, Fn.Spacing * Scale, ToColor(Color));
    }

    #endregion

    #region Optional Performance Overrides

    public override void BeginDrawing(float Dt)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);
    }

    public override void EndDrawing()
    {
        Raylib.EndDrawing();
    }

    public override void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color)
    {
        Raylib.DrawRectangleLinesEx(new Rectangle(Position, Size), 1, ToColor(Color));
    }

    public override void DrawCircle(Vector2 Center, float Radius, FishColor Color)
    {
        Raylib.DrawCircleV(Center, Radius, ToColor(Color));
    }

    #endregion

    private static Color ToColor(FishColor c) => new Color(c.R, c.G, c.B, c.A);
}
```

---

## Implementing IFishUIGfx Directly

For maximum control, implement `IFishUIGfx` directly. See `FishUI/IFishUIGfx.cs` for the complete interface definition.

This approach requires implementing all 25+ methods but gives you full control over every aspect of rendering.

---

## Input Backend

You also need to implement `IFishUIInput` for user input. Here's a minimal example:

```csharp
using FishUI;
using System.Numerics;

public class MyInput : IFishUIInput
{
    public FishInputState GetInputState()
    {
        FishInputState state = new FishInputState();
        
        // Mouse position
        state.MousePos = GetMousePosition();
        state.MouseDelta = GetMouseDelta();
        
        // Mouse buttons
        state.LeftDown = IsMouseButtonDown(0);
        state.LeftPressed = IsMouseButtonPressed(0);
        state.LeftReleased = IsMouseButtonReleased(0);
        
        state.RightDown = IsMouseButtonDown(1);
        state.RightPressed = IsMouseButtonPressed(1);
        state.RightReleased = IsMouseButtonReleased(1);
        
        state.MiddleDown = IsMouseButtonDown(2);
        state.MiddlePressed = IsMouseButtonPressed(2);
        state.MiddleReleased = IsMouseButtonReleased(2);
        
        // Mouse wheel
        state.MouseWheel = GetMouseWheelMove();
        
        // Keyboard (implement as needed)
        state.KeyPressed = GetPressedKeys();
        state.KeyDown = GetDownKeys();
        state.TextInput = GetTextInput();
        
        return state;
    }
}
```

---

## Best Practices

### 1. Use Userdata Fields
Store native objects in `ImageRef.Userdata` and `FontRef.Userdata` to avoid dictionary lookups:

```csharp
// Good - direct access
Texture2D tex = (Texture2D)Img.Userdata;

// Avoid - dictionary lookup
Texture2D tex = _textures[Img.Path];
```

### 2. Implement Frame Lifecycle
Override `BeginDrawing` and `EndDrawing` if your graphics library needs frame begin/end calls:

```csharp
public override void BeginDrawing(float Dt)
{
    MyGraphics.BeginFrame();
    MyGraphics.Clear(Color.White);
}

public override void EndDrawing()
{
    MyGraphics.EndFrame();
    MyGraphics.SwapBuffers();
}
```

### 3. Handle Window Resize
Call `FishUI.Resized(width, height)` when the window is resized:

```csharp
if (WindowWasResized())
{
    fishUI.Resized(GetWindowWidth(), GetWindowHeight());
}
```

### 4. Override Performance-Critical Methods
The default 9-patch implementation makes 9 draw calls. If your graphics library has native 9-slice support, override `DrawNPatch` for better performance.

### 5. Test with Theme Atlas
The FishUI theme uses a texture atlas. Make sure your image drawing correctly handles the full image (not sub-regions) as the theme loader creates `NPatch` objects that reference specific atlas regions.

---

## See Also

- [IFishUIGfx.cs](../FishUI/IFishUIGfx.cs) - Full interface definition
- [SimpleFishUIGfx.cs](../FishUI/SimpleFishUIGfx.cs) - Base class with defaults
- [RaylibGfx2.cs](../FishUISample/RaylibGfx2.cs) - Complete Raylib example
- [RaylibInput.cs](../RaylibFishGfx/RaylibInput.cs) - Raylib input example
