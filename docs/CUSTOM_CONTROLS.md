# Creating Custom Controls in FishUI

This guide explains how to create custom controls for FishUI. Whether you need a specialized widget or want to extend the library with new functionality, this document covers everything you need to know.

## Table of Contents

1. [Basic Control Structure](#basic-control-structure)
2. [Simple Example: ColorBox](#simple-example-colorbox)
3. [Properties and Serialization](#properties-and-serialization)
4. [Input Handling](#input-handling)
5. [Drawing Your Control](#drawing-your-control)
6. [Events and Callbacks](#events-and-callbacks)
7. [Theme Support](#theme-support)
8. [Advanced Topics](#advanced-topics)
9. [Complete Example: RatingControl](#complete-example-ratingcontrol)

---

## Basic Control Structure

All FishUI controls inherit from the `Control` base class. The minimum implementation requires:

```csharp
using FishUI.Controls;
using System.Numerics;

namespace MyApp.Controls
{
    public class MyControl : Control
    {
        public MyControl()
        {
            // Set default size
            Size = new Vector2(100, 30);
        }

        public override void DrawControl(FishUI.FishUI UI, float Dt, float Time)
        {
            Vector2 pos = GetAbsolutePosition();
            Vector2 size = GetAbsoluteSize();
            
            // Draw your control here
            UI.Graphics.DrawRectangle(pos, size, FishColor.Gray);
        }
    }
}
```

### Key Base Class Features

The `Control` base class provides:

| Property/Method | Purpose |
|----------------|---------|
| `Position` | Control position (relative to parent) |
| `Size` | Control size in logical pixels |
| `GetAbsolutePosition()` | Screen position including parent offsets and scaling |
| `GetAbsoluteSize()` | Scaled size based on UIScale |
| `Parent` | Reference to parent control |
| `Children` | List of child controls |
| `Visible` | Whether control is rendered |
| `Disabled` | Whether control accepts input |
| `Opacity` | Transparency (0.0 - 1.0) |
| `FishUI` | Reference to the FishUI instance |

---

## Simple Example: ColorBox

Let's create a simple control that displays a solid color:

```csharp
using FishUI;
using FishUI.Controls;
using System.Numerics;
using YamlDotNet.Serialization;

namespace MyApp.Controls
{
    public class ColorBox : Control
    {
        /// <summary>
        /// The color to display.
        /// </summary>
        [YamlMember]
        public FishColor BoxColor { get; set; } = new FishColor(100, 150, 200, 255);

        /// <summary>
        /// Whether to draw a border.
        /// </summary>
        [YamlMember]
        public bool ShowBorder { get; set; } = true;

        /// <summary>
        /// Border color.
        /// </summary>
        [YamlMember]
        public FishColor BorderColor { get; set; } = FishColor.Black;

        public ColorBox()
        {
            Size = new Vector2(50, 50);
        }

        public override void DrawControl(FishUI.FishUI UI, float Dt, float Time)
        {
            Vector2 pos = GetAbsolutePosition();
            Vector2 size = GetAbsoluteSize();

            // Draw filled rectangle
            UI.Graphics.DrawRectangle(pos, size, BoxColor);

            // Draw border if enabled
            if (ShowBorder)
            {
                UI.Graphics.DrawRectangleOutline(pos, size, BorderColor);
            }
        }
    }
}
```

**Usage:**
```csharp
ColorBox colorBox = new ColorBox();
colorBox.Position = new Vector2(10, 10);
colorBox.BoxColor = new FishColor(255, 0, 0, 255); // Red
ui.AddControl(colorBox);
```

---

## Properties and Serialization

### YamlMember Attribute

Use `[YamlMember]` to make properties serializable to YAML layout files:

```csharp
[YamlMember]
public float Value { get; set; } = 0f;

[YamlMember]
public string Label { get; set; } = "Default";

[YamlMember]
public FishColor HighlightColor { get; set; } = FishColor.Yellow;
```

### YamlIgnore Attribute

Use `[YamlIgnore]` for runtime-only state that shouldn't be serialized:

```csharp
[YamlIgnore]
private bool _isHovered = false;

[YamlIgnore]
private float _animationProgress = 0f;
```

### Property Change Events

For properties that should trigger events when changed:

```csharp
[YamlMember]
public float Value
{
    get => _value;
    set
    {
        float newValue = Math.Clamp(value, MinValue, MaxValue);
        if (_value != newValue)
        {
            float oldValue = _value;
            _value = newValue;
            OnValueChanged?.Invoke(this, _value);
        }
    }
}
private float _value = 0f;
```

---

## Input Handling

Override these methods to handle user input:

### Mouse Events

```csharp
// Called when mouse enters the control bounds
public override void HandleMouseEnter(FishUI.FishUI UI, FishInputState InState)
{
    base.HandleMouseEnter(UI, InState);
    _isHovered = true;
}

// Called when mouse leaves the control bounds
public override void HandleMouseLeave(FishUI.FishUI UI, FishInputState InState)
{
    base.HandleMouseLeave(UI, InState);
    _isHovered = false;
}

// Called when a mouse button is pressed
public override void HandleMousePress(FishUI.FishUI UI, FishInputState InState, 
    FishMouseButton Btn, Vector2 Pos)
{
    base.HandleMousePress(UI, InState, Btn, Pos);
    
    if (Btn == FishMouseButton.Left && !Disabled)
    {
        // Handle left click
        _isPressed = true;
    }
}

// Called when a mouse button is released
public override void HandleMouseRelease(FishUI.FishUI UI, FishInputState InState, 
    FishMouseButton Btn, Vector2 Pos)
{
    base.HandleMouseRelease(UI, InState, Btn, Pos);
    
    if (Btn == FishMouseButton.Left)
    {
        if (_isPressed && IsPointInside(Pos))
        {
            // Click completed
            OnClicked?.Invoke(this);
        }
        _isPressed = false;
    }
}

// Called when mouse is dragged over the control
public override void HandleDrag(FishUI.FishUI UI, Vector2 StartPos, Vector2 EndPos, 
    FishInputState InState)
{
    base.HandleDrag(UI, StartPos, EndPos, InState);
    // Handle drag gesture
}

// Called when mouse wheel is scrolled
public override void HandleMouseWheel(FishUI.FishUI UI, FishInputState InState, float WheelDelta)
{
    base.HandleMouseWheel(UI, InState, WheelDelta);
    // WheelDelta > 0 = scroll up, < 0 = scroll down
}
```

### Keyboard Events

```csharp
// Called when a key is pressed
public override void HandleKeyPress(FishUI.FishUI UI, FishInputState InState, FishKey Key)
{
    base.HandleKeyPress(UI, InState, Key);
    
    if (Key == FishKey.Enter)
    {
        // Handle Enter key
    }
}

// Called when a key is released
public override void HandleKeyRelease(FishUI.FishUI UI, FishInputState InState, FishKey Key)
{
    base.HandleKeyRelease(UI, InState, Key);
}

// Called for text input (typed characters)
public override void HandleTextInput(FishUI.FishUI UI, FishInputState InState, string Text)
{
    base.HandleTextInput(UI, InState, Text);
    // Text contains the typed character(s)
}
```

---

## Drawing Your Control

### Graphics API

The `UI.Graphics` interface provides drawing methods:

```csharp
public override void DrawControl(FishUI.FishUI UI, float Dt, float Time)
{
    Vector2 pos = GetAbsolutePosition();
    Vector2 size = GetAbsoluteSize();
    IFishUIGfx gfx = UI.Graphics;

    // Rectangles
    gfx.DrawRectangle(pos, size, FishColor.Blue);
    gfx.DrawRectangleOutline(pos, size, FishColor.White);

    // Lines
    gfx.DrawLine(pos, pos + size, 2f, FishColor.Red);

    // Text
    gfx.DrawText(UI.Settings.FontDefault, "Hello", pos);
    gfx.DrawTextColor(UI.Settings.FontDefault, "Colored", pos, FishColor.Green);

    // Images (if loaded)
    ImageRef img = gfx.LoadImage("path/to/image.png");
    gfx.DrawImage(img, pos, 0f, 1f, FishColor.White);

    // NPatch (9-slice) for themed controls
    if (UI.Settings.ImgButton != null)
    {
        gfx.DrawNPatch(UI.Settings.ImgButton, pos, size, FishColor.White);
    }
}
```

### Using UI Scale

Always use `GetAbsolutePosition()` and `GetAbsoluteSize()` for proper scaling:

```csharp
// These account for UIScale and parent positioning
Vector2 pos = GetAbsolutePosition();
Vector2 size = GetAbsoluteSize();

// For internal calculations that need scaling
float paddingScaled = Scale(10f); // Scales 10 by UIScale
Vector2 offsetScaled = Scale(new Vector2(5, 5));
```

### Scissoring (Clipping)

For controls with scrollable content:

```csharp
public override void DrawControl(FishUI.FishUI UI, float Dt, float Time)
{
    Vector2 pos = GetAbsolutePosition();
    Vector2 size = GetAbsoluteSize();

    // Draw background
    UI.Graphics.DrawRectangle(pos, size, BackgroundColor);

    // Enable clipping to control bounds
    UI.Graphics.BeginScissor(pos, size);

    // Draw content (will be clipped)
    DrawContent(UI);

    // Disable clipping
    UI.Graphics.EndScissor();
}
```

---

## Events and Callbacks

### Defining Events

```csharp
// Define delegate type
public delegate void MyControlClickedFunc(MyControl sender);
public delegate void MyControlValueChangedFunc(MyControl sender, float oldValue, float newValue);

// Declare event
public event MyControlClickedFunc OnClicked;
public event MyControlValueChangedFunc OnValueChanged;
```

### Invoking Events

```csharp
// In your input handler or property setter
OnClicked?.Invoke(this);
OnValueChanged?.Invoke(this, oldValue, newValue);
```

### Serializable Event Handlers

For events that can be defined in YAML layouts:

```csharp
// Handler name (registered with EventHandlerRegistry)
[YamlMember]
public string OnClickHandler { get; set; }

// Override to support deserialized handlers
public override void HandleMouseClick(FishUI.FishUI UI, FishInputState InState, 
    FishMouseButton Btn, Vector2 Pos)
{
    base.HandleMouseClick(UI, InState, Btn, Pos);
    
    if (Btn == FishMouseButton.Left && !Disabled)
    {
        // Invoke the serialized handler
        InvokeHandler(OnClickHandler, new ClickEventHandlerArgs(UI, Btn, Pos));
    }
}
```

---

## Theme Support

### Using Theme Colors

```csharp
private FishColor GetBackgroundColor(FishUI.FishUI UI)
{
    if (UseThemeColors && UI.Settings.CurrentTheme != null)
    {
        return UI.Settings.GetColorPalette().Background;
    }
    return _backgroundColor;
}

private FishColor GetTextColor(FishUI.FishUI UI)
{
    if (UseThemeColors && UI.Settings.CurrentTheme != null)
    {
        return UI.Settings.GetColorPalette().Foreground;
    }
    return _textColor;
}
```

### Using Theme Images (NPatch)

```csharp
public override void DrawControl(FishUI.FishUI UI, float Dt, float Time)
{
    Vector2 pos = GetAbsolutePosition();
    Vector2 size = GetAbsoluteSize();

    // Use themed button image if available
    NPatch buttonImage = UI.Settings.ImgButton;
    if (_isHovered)
        buttonImage = UI.Settings.ImgButtonHover ?? buttonImage;
    if (_isPressed)
        buttonImage = UI.Settings.ImgButtonDown ?? buttonImage;

    if (buttonImage != null)
    {
        UI.Graphics.DrawNPatch(buttonImage, pos, size, Color);
    }
    else
    {
        // Fallback to solid color
        UI.Graphics.DrawRectangle(pos, size, BackgroundColor);
    }
}
```

---

## Advanced Topics

### Adding Child Controls

```csharp
public class MyContainer : Control
{
    private Button _innerButton;

    public MyContainer()
    {
        Size = new Vector2(200, 100);
        
        // Create and add child control
        _innerButton = new Button();
        _innerButton.Text = "Inner Button";
        _innerButton.Position = new Vector2(10, 10);
        _innerButton.Size = new Vector2(80, 30);
        AddChild(_innerButton);
    }
}
```

### OnDeserialized Hook

For initialization after loading from YAML:

```csharp
public override void OnDeserialized(FishUI.FishUI UI)
{
    base.OnDeserialized(UI);
    
    // Load images from paths
    if (!string.IsNullOrEmpty(ImagePath))
    {
        _image = UI.Graphics.LoadImage(ImagePath);
    }
    
    // Rebuild internal state
    RebuildLayout();
}
```

### Custom Hit Testing

```csharp
public override bool IsPointInside(Vector2 GlobalPt)
{
    // Custom shape (e.g., circle)
    Vector2 center = GetAbsolutePosition() + GetAbsoluteSize() / 2f;
    float radius = Math.Min(GetAbsoluteSize().X, GetAbsoluteSize().Y) / 2f;
    
    float distance = Vector2.Distance(GlobalPt, center);
    return distance <= radius;
}
```

### Registering for Layout Serialization

Add your control to `LayoutFormat.TypeMapping` for YAML serialization support:

```csharp
// In your initialization code
LayoutFormat.RegisterType("!MyControl", typeof(MyControl));
```

---

## Complete Example: RatingControl

A 5-star rating control demonstrating all concepts:

```csharp
using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace MyApp.Controls
{
    public delegate void RatingChangedFunc(RatingControl sender, int rating);

    public class RatingControl : Control
    {
        // === Serializable Properties ===
        
        [YamlMember]
        public int Rating
        {
            get => _rating;
            set
            {
                int newValue = Math.Clamp(value, 0, MaxRating);
                if (_rating != newValue)
                {
                    int oldValue = _rating;
                    _rating = newValue;
                    OnRatingChanged?.Invoke(this, _rating);
                }
            }
        }
        private int _rating = 0;

        [YamlMember]
        public int MaxRating { get; set; } = 5;

        [YamlMember]
        public FishColor FilledColor { get; set; } = new FishColor(255, 200, 0, 255);

        [YamlMember]
        public FishColor EmptyColor { get; set; } = new FishColor(100, 100, 100, 255);

        [YamlMember]
        public FishColor HoverColor { get; set; } = new FishColor(255, 230, 100, 255);

        [YamlMember]
        public float StarSpacing { get; set; } = 5f;

        // === Runtime State ===
        
        [YamlIgnore]
        private int _hoveredStar = -1;

        // === Events ===
        
        public event RatingChangedFunc OnRatingChanged;

        // === Constructor ===
        
        public RatingControl()
        {
            Size = new Vector2(150, 30);
        }

        // === Input Handling ===

        public override void HandleMouseMove(FishUI.FishUI UI, FishInputState InState, Vector2 Pos)
        {
            base.HandleMouseMove(UI, InState, Pos);
            _hoveredStar = GetStarAtPosition(Pos);
        }

        public override void HandleMouseLeave(FishUI.FishUI UI, FishInputState InState)
        {
            base.HandleMouseLeave(UI, InState);
            _hoveredStar = -1;
        }

        public override void HandleMouseClick(FishUI.FishUI UI, FishInputState InState, 
            FishMouseButton Btn, Vector2 Pos)
        {
            base.HandleMouseClick(UI, InState, Btn, Pos);
            
            if (Btn == FishMouseButton.Left && !Disabled)
            {
                int clickedStar = GetStarAtPosition(Pos);
                if (clickedStar >= 0)
                {
                    Rating = clickedStar + 1;
                }
            }
        }

        // === Helper Methods ===

        private int GetStarAtPosition(Vector2 pos)
        {
            Vector2 absPos = GetAbsolutePosition();
            Vector2 absSize = GetAbsoluteSize();
            
            float starSize = absSize.Y;
            float spacing = Scale(StarSpacing);
            
            float localX = pos.X - absPos.X;
            
            for (int i = 0; i < MaxRating; i++)
            {
                float starX = i * (starSize + spacing);
                if (localX >= starX && localX <= starX + starSize)
                {
                    return i;
                }
            }
            
            return -1;
        }

        // === Drawing ===

        public override void DrawControl(FishUI.FishUI UI, float Dt, float Time)
        {
            Vector2 pos = GetAbsolutePosition();
            Vector2 size = GetAbsoluteSize();
            
            float starSize = size.Y;
            float spacing = Scale(StarSpacing);
            
            for (int i = 0; i < MaxRating; i++)
            {
                Vector2 starPos = new Vector2(
                    pos.X + i * (starSize + spacing),
                    pos.Y
                );
                
                FishColor color;
                
                if (i < Rating)
                {
                    color = FilledColor;
                }
                else if (i == _hoveredStar)
                {
                    color = HoverColor;
                }
                else
                {
                    color = EmptyColor;
                }
                
                // Draw star (simplified as a square - use DrawImage for real stars)
                UI.Graphics.DrawRectangle(starPos, new Vector2(starSize, starSize), color);
                UI.Graphics.DrawRectangleOutline(starPos, new Vector2(starSize, starSize), FishColor.Black);
            }
        }
    }
}
```

**Usage:**
```csharp
RatingControl rating = new RatingControl();
rating.Position = new Vector2(10, 10);
rating.Rating = 3;
rating.MaxRating = 5;
rating.OnRatingChanged += (sender, value) => Console.WriteLine($"Rating: {value}");
ui.AddControl(rating);
```

---

## Tips and Best Practices

1. **Always use absolute positions for drawing** - Call `GetAbsolutePosition()` and `GetAbsoluteSize()` to account for parent offsets and UI scaling.

2. **Call base class methods** - When overriding input handlers, call `base.HandleXxx()` first to maintain proper event propagation.

3. **Use YamlMember/YamlIgnore correctly** - Serializable state uses `[YamlMember]`, runtime-only state uses `[YamlIgnore]`.

4. **Support disabled state** - Check `Disabled` property before processing input and render differently when disabled.

5. **Use theme colors when appropriate** - Access `UI.Settings.GetColorPalette()` for consistent theming.

6. **Consider touch/gamepad** - Your control may receive virtual cursor input from gamepad navigation.

7. **Handle keyboard navigation** - For interactive controls, support `FishKey.Enter` as an alternative to mouse click.

8. **Clean up resources** - If your control loads images or other resources, consider cleanup in a destructor or Dispose pattern.

---

## See Also

- [README.md](../README.md) - Quick start guide and control examples
- [FishUI/Controls/](../FishUI/Controls/) - Built-in control implementations for reference
- [FishUIDemos/Samples/](../FishUIDemos/Samples/) - Sample applications showing control usage
