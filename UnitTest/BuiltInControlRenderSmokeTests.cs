using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest;

public sealed class BuiltInControlRenderSmokeTests
{
    [Fact]
    public void BuiltInControlsCompletePreparedUpdateAndDraw()
    {
        using var fixture = new FishUITestFixture(1600, 1200);
        fixture.FileSystem.AddDirectory("root");
        Control[] controls =
        {
            new AnimatedImageBox(), new BarGauge(), new BigDigitDisplay(), new Button { Text = "Button" },
            new CheckBox("Check"), new ContextMenu(), new ControlScrollable(), new DataGrid(),
            new DatePicker(), new DropDown(), new FilePickerDialog(FilePickerMode.Open, fixture.FileSystem, "root"),
            new FlowLayout(), new GameConsole(), new GridLayout(), new GroupBox(), new ImageBox(),
            new ItemListbox(), new Label("Label"), new LineChart(), new ListBox(), new MenuBar(),
            new MenuBarItem(), new MenuItem(), new MultiLineEditbox("first\nsecond"), new NumericUpDown(),
            new Panel(), new ParticleEmitter(), new ProgressBar(), new PropertyGrid(), new RadialGauge(),
            new RadioButton("Radio"), new ScrollablePane(), new ScrollBarH(), new ScrollBarV(),
            new SelectionBox(), new Slider(), new SpreadsheetCell(), new SpreadsheetGrid(), new StackLayout(),
            new StaticText("Static"), new TabControl(), new Textbox("Text"), new Timeline(), new TimePicker(),
            new Titlebar(), new ToastNotification(), new ToggleSwitch(), new Tooltip("Tooltip"),
            new TreeView(), new VUMeter(), new Window()
        };

        for (int i = 0; i < controls.Length; i++)
        {
            controls[i].Position = new Vector2((i % 8) * 190, (i / 8) * 150);
            if (controls[i].Size.X <= 0 || controls[i].Size.Y <= 0)
                controls[i].Size = new Vector2(180, 130);
            fixture.UI.AddControl(controls[i]);
        }

        fixture.Update();

        Assert.Equal(controls.Length, fixture.UI.GetAllControls().Length);
        Assert.True(fixture.Graphics.BeginDrawingCount > 0);
        Assert.NotEmpty(fixture.Graphics.DrawCalls);
    }
}
