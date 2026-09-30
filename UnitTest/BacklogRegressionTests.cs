using System.Numerics;
using FishUI;
using FishUI.Controls;
using FishUIEditor.Controls;

namespace UnitTest;

public class BacklogRegressionTests
{
    [Fact]
    public void HeatmapValidatesStorageOnceAndPreservesRange()
    {
        var grid = new SpreadsheetGrid { RowCount = 4000, ColumnCount = 5 };
        grid.SetCell(0, 0, "-12"); grid.SetCell(3999, 4, "97"); grid.SetCell(100, 2, "text");
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var counter = typeof(SpreadsheetGrid).GetProperty("StorageValidationCount", flags)!;
        long before = (long)counter.GetValue(grid)!;
        var scan = typeof(SpreadsheetGrid).GetMethod("CalculateHeatMapRange", flags)!;
        Assert.Equal((-12f, 97f), ((float, float))scan.Invoke(grid, null)!);
        Assert.Equal(before + 1, (long)counter.GetValue(grid)!);
    }

    [Fact]
    public void HidingRepeatButtonCancelsHeldGestureUntilNextPress()
    {
        using var f = new FishUITestFixture(); var button = new Button { Size = new(50, 30), IsRepeatButton = true, RepeatDelay = .05f };
        f.UI.AddControl(button); int calls = 0; button.OnButtonPressed += (_, _, _) => calls++;
        f.Input.SimulateMouseClick(FishMouseButton.Left, new(5, 5)); f.Update();
        button.Visible = false; f.Update(.1f); button.Visible = true; f.Update(.1f);
        Assert.Equal(1, calls);
        f.Input.SimulateMouseUp(FishMouseButton.Left); f.Update();
        f.Input.SimulateMouseDown(FishMouseButton.Left); f.Update();
        Assert.Equal(2, calls);
    }

    [Fact]
    public void RootTreeNodeCannotHaveMultipleOwners()
    {
        var first = new TreeView(); var second = new TreeView(); var node = first.AddNode("root");
        Assert.Throws<InvalidOperationException>(() => second.AddNode(node));
        Assert.Throws<InvalidOperationException>(() => new TreeNode("parent").AddChild(node));
        Assert.True(first.RemoveNode(node)); second.AddNode(node); Assert.Single(second.Nodes);
    }

    [Fact]
    public void BuiltinDrawingAppliesOpacityExactlyOnce()
    {
        using var f = new FishUITestFixture();
        foreach (Type type in FishUILayoutTypeRegistry.BuiltIn.Mappings.Values.Distinct())
        {
            if (!typeof(Control).IsAssignableFrom(type)) continue;
            var control = (Control)Activator.CreateInstance(type)!;
            if (control is Label label) label.Text = "Opacity";
            if (control is Textbox text) text.Text = "Opacity";
            if (control is ImageBox image) image.Image = new ImageRef("image", 16, 16);
            f.UI.AddControl(control); f.Update();
            f.Graphics.Reset(); control.Opacity = 1; control.DrawControl(f.UI, 0, 0);
            var opaque = f.Graphics.DrawColors.ToArray();
            f.Graphics.Reset(); control.Opacity = .5f; control.DrawControl(f.UI, 0, 0);
            var half = f.Graphics.DrawColors.ToArray();
            Assert.True(opaque.Length == half.Length, type.Name);
            for (int i = 0; i < opaque.Length; i++)
                Assert.True(half[i].A == (byte)(opaque[i].A * .5f), $"{type.Name}: draw {i} has alpha {half[i].A}, expected {(byte)(opaque[i].A * .5f)}");
            f.UI.RemoveControl(control);
        }
    }

    [Fact]
    public void NamedValueSelectionAndCheckedHandlersFireOncePerChange()
    {
        using var f = new FishUITestFixture(); int calls = 0;
        f.UI.EventHandlers.Register("changed", (_, _) => calls++);
        var number = new NumericUpDown { OnValueChangedHandler = "changed" };
        var toggle = new ToggleSwitch { OnCheckedChangedHandler = "changed" };
        var radio = new RadioButton { OnCheckedChangedHandler = "changed" };
        var drop = new DropDown { OnSelectionChangedHandler = "changed" }; drop.AddItem("one"); drop.AddItem("two");
        f.UI.AddControl(number); f.UI.AddControl(toggle); f.UI.AddControl(radio); f.UI.AddControl(drop);
        number.Value = 3; number.Value = 3; toggle.IsOn = true; toggle.IsOn = true; radio.IsChecked = true; radio.IsChecked = true;
        drop.SelectIndex(1); drop.SelectIndex(1);
        Assert.Equal(4, calls);
    }

    private sealed class ThrowingDetach : Control
    {
        public int Detachments;
        protected override void OnDetachedFromFishUI(FishUI.FishUI ui) { Detachments++; throw new InvalidOperationException("detach"); }
    }

    [Fact]
    public void DisposalFinishesAllCleanupDespiteCallbackFailures()
    {
        var f = new FishUITestFixture();
        var first = new ThrowingDetach(); var second = new ThrowingDetach();
        first.AddChild(second); f.UI.AddControl(first);
        var capture = f.UI.Diagnostics.CaptureAsync();
        Assert.Throws<AggregateException>(() => f.UI.Dispose());
        Assert.Empty(f.UI.GetAllControls());
        Assert.True(capture.IsCompleted);
        Assert.Equal(1, first.Detachments); Assert.Equal(1, second.Detachments);
        f.UI.Dispose();
    }

    [Fact]
    public void ClipAndSiblingOrderingAgreeWithDrawing()
    {
        using var f = new FishUITestFixture();
        var parent = new Panel { Size = new(20, 20) };
        var a = new Button { Position = new Vector2(50, 0), Size = new(20, 20) };
        var b = new Button { ZDepth = 100 };
        parent.AddChild(a); parent.AddChild(b); f.UI.AddControl(parent); f.Update();
        Assert.NotSame(a, f.UI.PickControl(new(55, 5)));
        parent.DisableChildScissor = true; f.Update();
        b.Visible = false; f.Update();
        Assert.Same(a, f.UI.PickControl(new(55, 5)));
        a.BringToFront(); Assert.True(a.ZDepth > b.ZDepth);
        b.ZDepth = -100; a.SendToBack(); Assert.True(a.ZDepth < b.ZDepth);
    }

    [Theory]
    [InlineData(1f)]
    [InlineData(2f)]
    public void DragAndCheckboxLabelUseLogicalCoordinates(float scale)
    {
        using var f = new FishUITestFixture(); f.Settings.UIScale = scale;
        var panel = new Panel { Draggable = true }; f.UI.AddControl(panel);
        var before = panel.GetAbsolutePosition();
        panel.HandleDrag(f.UI, Vector2.Zero, new(10, 0), new FishInputState { MouseDelta = new(10, 0) });
        Assert.Equal(10, (panel.GetAbsolutePosition() - before).X);
        var check = new CheckBox("text") { Size = new(16, 16) }; f.UI.AddControl(check); f.Update();
        Assert.Equal(20 * scale, check.Children.OfType<Label>().Single().GetAbsolutePosition().X - check.GetAbsolutePosition().X);
    }

    [Fact]
    public void TreeRestoresParentsAndReusesScrollbar()
    {
        using var f = new FishUITestFixture();
        var tree = new TreeView(); tree.AddNode("root").AddChild("child");
        LayoutFormat.Deserialize(f.UI, LayoutFormat.SerializeControls(new[] { tree }));
        tree = Assert.IsType<TreeView>(Assert.Single(f.UI.GetAllControls())); f.Update();
        Assert.Equal(1, tree.Nodes[0].Children[0].Depth);
        var bar = Assert.Single(tree.Children.OfType<ScrollBarV>());
        f.UI.RemoveControl(tree); f.UI.AddControl(tree); f.Update();
        Assert.Same(bar, Assert.Single(tree.Children.OfType<ScrollBarV>()));
        Assert.True(tree.Focusable); Assert.True(new ListBox().Focusable);
        var a = new TreeNode("a"); var b = new TreeNode("b"); a.AddChild(b); a.AddChild(b);
        Assert.Single(a.Children);
        Assert.Throws<InvalidOperationException>(() => b.AddChild(a));
        Assert.Throws<InvalidOperationException>(() => a.AddChild(a));
        Assert.Throws<InvalidOperationException>(() => new TreeNode("other").AddChild(b));
    }

    [Fact]
    public void NumericEditorRoutesKeysNormalizesOnBlurAndPreservesLargeValues()
    {
        using var f = new FishUITestFixture(); var number = new NumericUpDown(5); f.UI.AddControl(number);
        f.UI.FocusControl(number.InternalTextbox);
        f.Input.SimulateKeyDown(FishKey.Up); f.Update();
        Assert.Equal(6, number.Value);
        number.InternalTextbox.SelectAll(); number.InternalTextbox.Paste("999"); f.UI.ClearFocus();
        Assert.Equal(100, number.Value); Assert.Equal("100", number.InternalTextbox.Text);
        var large = new NumericUpDown(3_000_000_000f, 0, 4_000_000_000f);
        Assert.Equal(3_000_000_000f, large.Value); Assert.Equal("3000000000", large.InternalTextbox.Text);
        large.DecimalPlaces = 0; large.Value = 1.5f;
        Assert.Equal(1.5f, large.Value);
    }

    [Fact]
    public void NamedTextHandlerMatchesTypedHandlerForTypingReplacementAndPaste()
    {
        using var f = new FishUITestFixture(); int named = 0, typed = 0;
        f.UI.EventHandlers.Register("change", (_, _) => named++);
        var text = new Textbox { OnTextChangedHandler = "change" }; f.UI.AddControl(text); f.UI.FocusControl(text);
        text.OnTextChanged += (_, _) => typed++;
        f.Input.SimulateCharTyped('x'); f.Update(); text.Paste("y");
        text.SelectAll(); text.Paste("z");
        Assert.Equal("z", text.Text); Assert.Equal(3, named); Assert.Equal(named, typed);
        text.Text = "z"; Assert.Equal(3, named);
    }

    [Fact]
    public void TabStateAndWindowGraphLimitsSurviveSerialization()
    {
        using var f = new FishUITestFixture(); var tabs = new TabControl();
        tabs.AddTab("one"); tabs.AddTab("two").Enabled = false; tabs.SelectedIndex = 1;
        LayoutFormat.Deserialize(f.UI, LayoutFormat.SerializeControls(new[] { tabs }));
        var copy = Assert.IsType<TabControl>(Assert.Single(f.UI.GetAllControls()));
        Assert.Equal(1, copy.SelectedIndex); Assert.False(copy.TabPages[1].Enabled);
        var window = new Window(); window.AddChild(new Button());
        Assert.Throws<InvalidOperationException>(() => LayoutFormat.DeserializeControls(LayoutFormat.SerializeControls(new[] { window }), new() { MaximumControls = 1 }));
    }

    [Fact]
    public void EditorDeletesNestedSelectionAndExposesFields()
    {
        var canvas = new EditorCanvas(); var parent = new Panel(); var child = new Button(); parent.AddChild(child);
        canvas.AddEditedControl(parent); canvas.SelectControl(child); canvas.RemoveEditedControl(child);
        Assert.Empty(parent.Children); Assert.Null(canvas.SelectedControl);
        var grid = new PropertyGrid { SelectedObject = child };
        var size = Assert.Single(grid.Items, i => i.Name == "Size");
        Assert.True(size.SetValue(new Vector2(123, 45))); Assert.Equal(new(123, 45), child.Size);
        Assert.Contains(grid.Items, i => i.Name == "Position"); Assert.Contains(grid.Items, i => i.Name == "ID");
    }

    [Fact]
    public void PropertyGridScrollbarUsesCurrentDimensions()
    {
        using var f = new FishUITestFixture();
        var grid = new PropertyGrid { Size = new(300, 80), GroupByCategory = false, SelectedObject = new Button() };
        f.UI.AddControl(grid); f.Update(); f.Update();
        var bar = Assert.Single(grid.Children.OfType<ScrollBarV>());
        grid.Size = new(400, 160); f.Update(); bar.ScrollDown();
        Assert.Equal(384, bar.Position.X);
        float offset = (float)typeof(PropertyGrid).GetField("_scrollOffset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(grid)!;
        Assert.Equal(bar.ThumbPosition * (grid.Items.Count * grid.RowHeight - 160), offset, .001f);
    }

    [Fact]
    public void ThemeSpacingDistinguishesOmittedAndExplicitZero()
    {
        using var f = new FishUITestFixture(); f.FileSystem.AddFile("base.yaml", "theme:\n  name: Base\nfonts:\n  spacing: 4\n");
        var loader = new FishUIThemeLoader(f.UI);
        Assert.Equal(4, loader.LoadFromString("theme:\n  inherits: base.yaml\nfonts:\n  labelSize: 20\n").Fonts.Spacing);
        Assert.Equal(0, loader.LoadFromString("theme:\n  inherits: base.yaml\nfonts:\n  spacing: 0\n").Fonts.Spacing);
    }

    [Fact]
    public void TimelineCannotInvertItsVisibleRange()
    {
        var timeline = new Timeline(); timeline.SetView(0, 10); timeline.ViewStart = 50;
        Assert.True(timeline.ViewEnd - timeline.ViewStart >= timeline.MinViewWidth);
        timeline.ViewEnd = -10; Assert.True(timeline.ViewEnd >= timeline.ViewStart);
        int events = 0; timeline.OnViewChanged += (_, _) => events++;
        timeline.SetView(20, 30); Assert.Equal(1, events);
        timeline.SetView(20, 30); Assert.Equal(1, events);
        timeline.MaxTime = 0; Assert.Equal(timeline.ViewStart, timeline.ViewEnd);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MouseLeaveCanRemoveItsOwnSubtree(bool nested)
    {
        using var f = new FishUITestFixture();
        var parent = new Panel { Size = new(100, 100) };
        var button = new Button { Size = new(50, 30) };
        var focus = new Textbox { Position = new Vector2(200, 0) };
        if (nested) { parent.AddChild(button); f.UI.AddControl(parent); }
        else f.UI.AddControl(button);
        f.UI.AddControl(focus);
        int leaves = 0;
        button.MouseLeave += (_, _) =>
        {
            if (++leaves > 1) throw new InvalidOperationException("Recursive leave");
            f.UI.RemoveControl(nested ? parent : button);
            f.UI.FocusControl(focus);
        };
        f.Input.MousePosition = new(10, 10);
        f.Update();
        f.UI.RemoveControl(nested ? parent : button);
        Assert.Equal(1, leaves);
        Assert.Same(focus, f.UI.InputActiveControl);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HiddenRepeatButtonStopsAndDoesNotResumeAfterRelease(bool ancestor)
    {
        using var f = new FishUITestFixture();
        var parent = new Panel { Size = new(100, 100) };
        var button = new Button { Size = new(50, 30), IsRepeatButton = true, RepeatDelay = .01f };
        parent.AddChild(button); f.UI.AddControl(parent);
        int calls = 0;
        button.OnButtonPressed += (_, _, _) => calls++;
        f.Input.SimulateMouseClick(FishMouseButton.Left, new(10, 10)); f.Update();
        int initial = calls;
        (ancestor ? (Control)parent : button).Visible = false;
        f.Update(.2f);
        f.Input.SimulateMouseUp(FishMouseButton.Left); f.Update();
        (ancestor ? (Control)parent : button).Visible = true;
        f.Update(.2f);
        Assert.Equal(initial, calls);
    }

    [Fact]
    public void RepeatCallbackDetachmentStopsCatchUpLoop()
    {
        using var f = new FishUITestFixture();
        var button = new Button { Size = new(50, 30), IsRepeatButton = true, RepeatDelay = .01f, RepeatInterval = .01f };
        f.UI.AddControl(button);
        int calls = 0;
        button.OnButtonPressed += (_, _, _) => { if (++calls == 2) f.UI.RemoveControl(button); };
        f.Input.SimulateMouseClick(FishMouseButton.Left, new(10, 10)); f.Update();
        f.Update(1);
        Assert.Equal(2, calls);
    }

    [Fact]
    public void YamlRetainsExplicitFalseAndZero()
    {
        var source = new Button { Visible = false, Focusable = false, Opacity = 0, Size = Vector2.Zero };
        var copy = Assert.IsType<Button>(LayoutFormat.DeserializeControls(LayoutFormat.SerializeControls(new[] { source }))[0]);
        Assert.False(copy.Visible); Assert.False(copy.Focusable); Assert.Equal(0, copy.Opacity); Assert.Equal(Vector2.Zero, copy.Size);
    }

    [Fact]
    public void EveryRegisteredBuiltinRoundTrips()
    {
        var registry = new FishUILayoutSerializationOptions().TypeRegistry;
        foreach (Type type in registry.Mappings.Values.Distinct())
        {
            if (!typeof(Control).IsAssignableFrom(type)) continue;
            var source = (Control)Activator.CreateInstance(type)!;
            string yaml = LayoutFormat.SerializeControls(new[] { source });
            var copy = LayoutFormat.DeserializeControls(yaml);
            Assert.Equal(type, Assert.Single(copy).GetType());
        }
        Assert.ThrowsAny<Exception>(() => LayoutFormat.DeserializeControls("- !Button\n  UnknownProperty: true\n"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedEditorLoadPreservesDocumentAndSelection(bool resourceFailure)
    {
        using var f = new FishUITestFixture();
        var canvas = new EditorCanvas();
        var original = new Button { Text = "keep me" };
        canvas.AddEditedControl(original); canvas.SelectControl(original);
        f.Graphics.ImageLoadFailure = _ => new IOException("Missing image");
        string yaml = resourceFailure
            ? LayoutFormat.SerializeControls(new[] { new ImageBox { ImagePath = "missing.png" } })
            : "- !UnknownControl {}";
        Assert.ThrowsAny<Exception>(() => canvas.LoadLayout(f.UI, yaml));
        Assert.Same(original, Assert.Single(canvas.GetEditedControls()));
        Assert.Same(original, canvas.SelectedControl);
    }

    [Fact]
    public void EditorInitializesNestedTabsOnlyOnce()
    {
        using var f = new FishUITestFixture();
        var parent = new Panel(); var tabs = new TabControl(); tabs.AddTab("Preserved name"); parent.AddChild(tabs);
        var canvas = new EditorCanvas(); canvas.LoadLayout(f.UI, LayoutFormat.SerializeControls(new[] { parent }));
        var copy = Assert.IsType<TabControl>(Assert.Single(canvas.GetEditedControls()).Children[0]);
        Assert.Equal("Preserved name", Assert.Single(copy.TabPages).Text);
    }
}
