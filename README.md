# FishUI

A dependency-free, simple GUI library for .NET applications.

## Overview

FishUI is a flexible, graphics-backend-agnostic GUI framework that provides common UI controls and supports custom rendering implementations. The library separates UI logic from rendering, allowing you to integrate it with any graphics library.

## Features

- **Backend Agnostic**: Implement your own graphics and input handlers via interfaces
- **Built-in Controls**: Button, CheckBox, RadioButton, Panel, Label, Textbox, ListBox, DropDown, ScrollBarV, SelectionBox
- **Layout System**: Support for Absolute, Relative, and Docked positioning modes
- **Draggable Controls**: Built-in drag support for repositioning controls
- **YAML Layout Serialization**: Save and load UI layouts using YAML format
- **NPatch/9-Slice Support**: Scalable UI graphics with 9-slice rendering
- **Touch Support**: Built-in touch point handling for mobile/touch devices

## Projects

| Project | Description |
|---------|-------------|
| **FishUI** | Core library containing all UI controls and interfaces |
| **FishUISample** | Sample application demonstrating FishUI with Raylib |

## Getting Started

### 1. Implement Required Interfaces

FishUI requires you to implement three interfaces for your graphics backend:

```csharp
// Graphics rendering
public interface IFishUIGfx
{
    void Init();
    void BeginDrawing(float Dt);
    void EndDrawing();
    void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color);
    void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color);
    void DrawText(FontRef Fn, string Text, Vector2 Pos);
    // ... and more
}

// Input handling
public interface IFishUIInput
{
    Vector2 GetMousePosition();
    bool IsMouseDown(FishMouseButton Button);
    bool IsKeyPressed(FishKey Key);
    FishTouchPoint[] GetTouchPoints();
    // ... and more
}

// Event broadcasting
public interface IFishUIEvents
{
    void Broadcast(FishUI UI, Control Sender, string EventName, object Data);
}
```

### 2. Initialize FishUI

```csharp
FishUISettings UISettings = new FishUISettings();
IFishUIGfx Gfx = new YourGfxImplementation();
IFishUIInput Input = new YourInputImplementation();
IFishUIEvents Events = new YourEventHandler();

FishUI.FishUI UI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
UI.Init();
```

### 3. Add Controls

```csharp
// Create a button
Button btn = new Button();
btn.ID = "myButton";
btn.Text = "Click Me";
btn.Position = new Vector2(100, 100);
btn.Size = new Vector2(150, 50);
UI.AddControl(btn);

// Create a panel with children
Panel panel = new Panel();
panel.Position = new Vector2(10, 10);
panel.Size = new Vector2(400, 300);
panel.Draggable = true;
UI.AddControl(panel);

CheckBox checkbox = new CheckBox("Enable Feature");
checkbox.Position = new Vector2(5, 10);
panel.AddChild(checkbox);
```

### 4. Run the Update Loop

```csharp
while (running)
{
    float deltaTime = GetDeltaTime();
    float totalTime = GetTotalTime();
    
    UI.Tick(deltaTime, totalTime);
}
```

## Available Controls

| Control | Description |
|---------|-------------|
| `Button` | Clickable button with text |
| `CheckBox` | Toggle checkbox with label |
| `RadioButton` | Radio button for exclusive selection |
| `Panel` | Container for grouping controls |
| `Label` | Static text display |
| `Textbox` | Text input field |
| `ListBox` | Scrollable list of selectable items |
| `DropDown` | Dropdown selection menu |
| `ScrollBarV` | Vertical scrollbar |
| `SelectionBox` | Selection box control |

## Positioning Modes

FishUI supports three positioning modes:

- **Absolute**: Fixed position on screen
- **Relative**: Position relative to parent control
- **Docked**: Position and size relative to parent with docking (Left, Top, Right, Bottom)

```csharp
// Docked positioning example
button.Position = new FishUIPosition(
    PositionMode.Docked, 
    DockMode.Horizontal, 
    new Vector4(15, 0, 15, 0),  // Left, Top, Right, Bottom margins
    new Vector2(100, 100)
);
```

## Layout Serialization

Save and load UI layouts using YAML:

```csharp
// Save layout
LayoutFormat.SerializeToFile(UI, "layout.yaml");

// Load layout
LayoutFormat.DeserializeFromFile(UI, "layout.yaml");
```

## Sample Application

The `FishUISample` project demonstrates FishUI with [Raylib-cs](https://github.com/ChrisDill/Raylib-cs) as the graphics backend. See the sample for complete implementation examples of `IFishUIGfx` and `IFishUIInput`.

## Requirements

- .NET 9.0
- [YamlDotNet](https://github.com/aaubry/YamlDotNet) (for layout serialization)

## License

See repository for license information.

