using System.Numerics;
using System.Text;
using FishUI;
using FishUI.Controls;

namespace UnitTest;

public class RevisionRegressionTests
{
    private sealed class CallbackControl : Control
    {
        public Action? Updating;
        public Action? Blurring;
        public Action? Resizing;
        public int Updates;
        public int Focuses;
        public int Blurs;
        public int Attachments;
        public int Detachments;
        public int Releases;
        protected override void OnFishUIUpdate(FishUI.FishUI ui, float dt, float time) { Updates++; Updating?.Invoke(); }
        protected override void OnFishUIResized(FishUI.FishUI ui, int width, int height) => Resizing?.Invoke();
        protected override void OnAttachedToFishUI(FishUI.FishUI ui) => Attachments++;
        protected override void OnDetachedFromFishUI(FishUI.FishUI ui) => Detachments++;
        public override void HandleFocus() => Focuses++;
        public override void HandleBlur() { Blurs++; if (Blurs == 1) Blurring?.Invoke(); }
        public override void HandleMouseRelease(FishUI.FishUI ui, FishInputState input, FishMouseButton button, Vector2 pos) => Releases++;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RemovingControlDuringUpdateDoesNotSkipSibling(bool nested)
    {
        using var f = new FishUITestFixture();
        var first = new CallbackControl();
        var second = new CallbackControl();
        if (nested)
        {
            var parent = new Panel();
            parent.AddChild(first);
            parent.AddChild(second);
            f.UI.AddControl(parent);
            first.Updating = () => parent.RemoveChild(first);
        }
        else
        {
            f.UI.AddControl(first);
            f.UI.AddControl(second);
            first.Updating = () => f.UI.RemoveControl(first);
        }
        f.UI.TickUpdate(0.1f, 0.1f);
        Assert.Equal(1, second.Updates);
    }

    [Fact]
    public void BlurCanRedirectFocusWithoutRecursiveBlurOrOverwritingNewFocus()
    {
        using var f = new FishUITestFixture();
        var first = new CallbackControl();
        var second = new CallbackControl();
        var third = new CallbackControl();
        f.UI.AddControl(first); f.UI.AddControl(second); f.UI.AddControl(third);
        f.UI.FocusControl(first);
        first.Blurring = () => f.UI.FocusControl(third);
        f.UI.FocusControl(second);
        Assert.Same(third, f.UI.InputActiveControl);
        Assert.Equal(1, first.Blurs);
        Assert.Equal(0, second.Focuses);
        f.UI.FocusControl(third);
        Assert.Equal(1, third.Focuses);
    }

    [Fact]
    public void TextChangeCanClearFocusWhileCharactersRemainQueued()
    {
        using var f = new FishUITestFixture();
        var text = new Textbox { Text = "a" };
        f.UI.AddControl(text);
        f.UI.FocusControl(text);
        text.CursorPosition = 1;
        text.OnTextChanged += (_, _) => f.UI.ClearFocus();
        f.Input.SimulateKeyDown(FishKey.Backspace);
        f.Input.SimulateCharTyped('x');
        f.UI.TickUpdate(0.1f, 0.1f);
        Assert.Null(f.UI.InputActiveControl);
        Assert.Equal("", text.Text);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResizeFailureRollsBackAttachment(bool nested)
    {
        using var f = new FishUITestFixture();
        var control = new CallbackControl { Resizing = () => throw new InvalidOperationException("resize") };
        var parent = new Panel();
        f.UI.AddControl(parent);
        Assert.Throws<InvalidOperationException>(() => { if (nested) parent.AddChild(control); else f.UI.AddControl(control); });
        Assert.Null(control.GetParent());
        Assert.Equal(control.Attachments, control.Detachments);
        control.Resizing = null;
        f.UI.AddControl(control);
        f.UI.TickUpdate(0.1f, 0.1f);
        Assert.Equal(1, control.Updates);
    }

    [Fact]
    public void AnimationRemovalDuringApplyDoesNotUpdateAnotherAnimationTwice()
    {
        var manager = new FishUIAnimationManager();
        manager.Add(new FishUIAnimation { Id = "removed", Duration = 10 });
        var survivor = manager.Add(new FishUIAnimation { Duration = 10, ApplyValue = _ => manager.StopAnimation("removed") });
        manager.Update(0.25f);
        Assert.Equal(0.25f, survivor.ElapsedTime);
    }

    [Fact]
    public void WrappedContentSizeMatchesLaidOutChildren()
    {
        var flow = new FlowLayout { Size = new Vector2(100, 200), LayoutPadding = 0, Spacing = 5, WrapSpacing = 5 };
        for (int i = 0; i < 3; i++) flow.AddChild(new Button { Size = new Vector2(60, 20) });
        flow.UpdateLayout();
        Assert.Equal(new Vector2(60, 70), flow.ContentSize);
    }

    [Theory]
    [InlineData(FlowDirection.RightToLeft)]
    [InlineData(FlowDirection.BottomToTop)]
    public void ReverseFlowStartsWithFirstChildAtFarEdge(FlowDirection direction)
    {
        var flow = new FlowLayout { Size = new Vector2(100), LayoutPadding = 0, Direction = direction };
        var first = new Button { Size = new Vector2(20) };
        flow.AddChild(first); flow.AddChild(new Button { Size = new Vector2(20) });
        flow.UpdateLayout();
        var position = first.GetAbsolutePosition();
        Assert.Equal(80, direction == FlowDirection.RightToLeft ? position.X : position.Y);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpreadsheetEditingPreservesScalarsAndDeletesWholeGraphemes(bool grid)
    {
        using var f = new FishUITestFixture();
        var cell = new SpreadsheetCell();
        var sheet = new SpreadsheetGrid();
        Control editor = grid ? sheet : cell;
        f.UI.AddControl(editor);
        if (grid) sheet.BeginEdit(); else cell.BeginEdit();
        editor.HandleTextInput(f.UI, default, new Rune(0x1f600));
        editor.HandleTextInput(f.UI, default, new Rune('x'));
        editor.HandleKeyPress(f.UI, default, FishKey.Left);
        editor.HandleKeyPress(f.UI, default, FishKey.Backspace);
        if (grid) sheet.CommitEdit(); else cell.CommitEdit();
        Assert.Equal("x", grid ? sheet.GetCell(0, 0) : cell.Value);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(9999, 12)]
    public void CalendarCanRenderBoundaryMonths(int year, int month)
    {
        using var f = new FishUITestFixture();
        var picker = new DatePicker(new DateTime(year, month, 1));
        f.UI.AddControl(picker); picker.Open();
        f.Update();
    }

    [Fact]
    public void DataGridPreparesScrollbarAndRowPickingBeforeDrawing()
    {
        using var f = new FishUITestFixture();
        var grid = new DataGrid { RowHeight = 20, Size = new Vector2(200, 124) };
        grid.AddColumn("Column");
        for (int i = 0; i < 20; i++) grid.AddRow(i.ToString());
        f.UI.AddControl(grid);
        f.Input.SimulateMouseClick(FishMouseButton.Left, new Vector2(10, 35));
        f.UI.TickUpdate(0.1f, 0.1f);
        Assert.Equal(0, grid.SelectedIndex);
        var scrollbar = Assert.Single(grid.Children.OfType<ScrollBarV>());
        scrollbar.ScrollStep = 1;
        scrollbar.ScrollDown();
        grid.HandleMouseMove(f.UI, default, new Vector2(10, 115));
        grid.HandleMousePress(f.UI, default, FishMouseButton.Left, new Vector2(10, 115));
        Assert.Equal(19, grid.SelectedIndex);
    }

    [Fact]
    public void MouseReleaseReachesPressedControlOutsideItsBounds()
    {
        using var f = new FishUITestFixture();
        var control = new CallbackControl { Size = new Vector2(40) };
        f.UI.AddControl(control);
        f.Input.SimulateMouseClick(FishMouseButton.Left, new Vector2(10)); f.Update();
        f.Input.SimulateMouseMove(new Vector2(100));
        f.Input.SimulateMouseUp(FishMouseButton.Left); f.Update();
        Assert.Equal(1, control.Releases);
    }

    [Fact]
    public void HiddenStackChildrenDoNotContributeSpacing()
    {
        var stack = new StackLayout { LayoutPadding = 2, Spacing = 10 };
        stack.AddChild(new Button { Visible = false });
        Assert.Equal(new Vector2(4), stack.ContentSize);
    }

    [Fact]
    public void NonuniformGridMeasuresEachColumnAndRowSeparately()
    {
        var grid = new GridLayout { Columns = 2, UniformCells = false, StretchCells = false, LayoutPadding = 0 };
        grid.AddChild(new Button { Size = new Vector2(20, 10) });
        grid.AddChild(new Button { Size = new Vector2(40, 30) });
        var last = new Button { Size = new Vector2(10, 15) };
        grid.AddChild(last);
        grid.UpdateLayout();
        Assert.Equal(new Vector2(65, 50), grid.ContentSize);
        Assert.Equal(new Vector2(0, 35), last.GetAbsolutePosition());
    }

    [Fact]
    public void DockAxesAreIndependent()
    {
        var parent = new Panel { Size = new Vector2(200) };
        var child = new Button { Size = new Vector2(20), Position = new FishUIPosition(PositionMode.Docked, DockMode.Vertical, new Vector4(7, 10, 9, 15), new Vector2(40, 50)) };
        parent.AddChild(child);
        Assert.Equal(new Vector2(40, 10), child.GetAbsolutePosition());
        Assert.Equal(new Vector2(20, 175), child.GetAbsoluteSize());
    }

    [Fact]
    public void ThemeCanExplicitlyDisableInheritedAtlas()
    {
        using var f = new FishUITestFixture();
        var loader = new FishUIThemeLoader(f.UI);
        f.FileSystem.WriteAllText("parent.yaml", "atlas:\n  enabled: true\n");
        var theme = loader.LoadFromString("theme:\n  inherits: parent.yaml\natlas:\n  enabled: false\n");
        Assert.False(theme.UseAtlas);
    }

    [Fact]
    public void TimelineSynchronizesManualViewOrigin()
    {
        var chart = new LineChart { AutoScroll = false, ViewStart = 20, TimeWindow = 10 };
        var timeline = new Timeline { MaxTime = 100 };
        timeline.SyncFromLineChart(chart);
        Assert.Equal(20, timeline.ViewStart);
        timeline.SetView(40, 50);
        timeline.SyncToLineChart(chart);
        Assert.Equal(40, chart.ViewStart);
    }

    [Fact]
    public void ListScrollbarCannotScrollPastFinalRow()
    {
        using var f = new FishUITestFixture();
        var list = new ListBox { Size = new Vector2(200, 104), CustomItemHeight = 20 };
        for (int i = 0; i < 20; i++) list.AddItem(new ListBoxItem(i.ToString()));
        f.UI.AddControl(list);
        f.UI.TickUpdate(0.1f, 0.1f);
        var bar = Assert.Single(list.Children.OfType<ScrollBarV>());
        bar.ScrollStep = 1; bar.ScrollDown();
        list.HandleMouseClick(f.UI, default, FishMouseButton.Left, new Vector2(10, 95));
        Assert.Equal(19, list.SelectedIndex);
    }

    [Fact]
    public void ItemWidgetsHaveStableScaledLayoutBeforeDrawing()
    {
        using var f = new FishUITestFixture();
        f.Settings.UIScale = 2;
        var list = new ItemListbox { Size = new Vector2(200, 200) };
        var widget = new Button { Size = new Vector2(60, 24) };
        list.AddItem(widget); f.UI.AddControl(list);
        f.UI.TickUpdate(0.1f, 0.1f);
        Assert.True(widget.Visible);
        Assert.Equal(24, widget.Size.Y);
        f.UI.TickUpdate(0.1f, 0.2f);
        Assert.Equal(24, widget.Size.Y);
    }

    [Fact]
    public void SpreadsheetTabCommitsAndKeepsGridFocus()
    {
        using var f = new FishUITestFixture();
        var grid = new SpreadsheetGrid();
        f.UI.AddControl(grid); f.UI.AddControl(new Textbox { Position = new Vector2(700, 500) });
        f.UI.FocusControl(grid); grid.BeginEdit();
        grid.HandleTextInput(f.UI, default, new Rune('x'));
        f.Input.SimulateKeyDown(FishKey.Tab); f.Update();
        Assert.False(grid.IsEditing);
        Assert.Equal("x", grid.GetCell(0, 0));
        Assert.Same(grid, f.UI.InputActiveControl);
    }

    [Fact]
    public void ChartCapacityReductionAndDuplicateTimesRemainWellDefined()
    {
        var series = new LineChartSeries();
        for (int i = 0; i < 10; i++) series.AddPoint(i, i);
        series.MaxPoints = 2; series.AddPoint(10, 10);
        Assert.Equal(2, series.Points.Count);
        series.Clear(); series.AddPoint(1, 2); series.AddPoint(1, 3);
        Assert.Equal(3, series.GetValueAt(1));
    }

    [Fact]
    public void ProgrammaticMultiselectionMatchesSelectedRow()
    {
        var grid = new DataGrid { MultiSelect = true };
        grid.AddRow("first"); grid.AddRow("second");
        grid.SelectRow(1);
        Assert.Equal(new[] { 1 }, grid.GetSelectedIndices());
        grid.SelectRow(-1);
        Assert.Empty(grid.GetSelectedIndices());
    }

    private sealed class ThrowingTextGraphics : UnitTest.Mocks.MockFishUIGfx, IFishUIGfx
    {
        void IFishUIGfx.DrawText(FontRef font, string text, Vector2 pos) => throw new InvalidOperationException("text drawing");
        void IFishUIGfx.DrawTextColor(FontRef font, string text, Vector2 pos, FishColor color) => throw new InvalidOperationException("text drawing");
    }

    [Theory]
    [InlineData("textbox")]
    [InlineData("cell")]
    [InlineData("grid")]
    [InlineData("list")]
    public void TextDrawingFailureRestoresClip(string kind)
    {
        var graphics = new ThrowingTextGraphics();
        using var ui = new FishUI.FishUI(new FishUISettings(), graphics, new UnitTest.Mocks.MockFishUIInput(), new UnitTest.Mocks.MockFishUIEvents(), new UnitTest.Mocks.MockFishUIFileSystem());
        ui.Init();
        Control control = kind switch
        {
            "textbox" => new Textbox { Text = "value" },
            "cell" => new SpreadsheetCell("value"),
            "grid" => new DataGrid(),
            _ => new ListBox()
        };
        if (control is DataGrid grid) grid.AddColumn("Header");
        if (control is ListBox list) list.AddItem("value");
        ui.AddControl(control);
        ui.TickUpdate(0.1f, 0.1f);
        Assert.Throws<InvalidOperationException>(() => ui.TickDraw(0.1f, 0.1f));
        Assert.Equal(graphics.DrawCalls.Count(c => c.StartsWith("PushScissor(")), graphics.DrawCalls.Count(c => c == "PopScissor"));
        Assert.Equal(graphics.BeginDrawingCount, graphics.EndDrawingCount);
    }
}
