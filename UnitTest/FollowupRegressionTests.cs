using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using FishUI;
using FishUI.Controls;

namespace UnitTest;

public class FollowupRegressionTests
{
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static bool Edit(PropertyGrid grid, PropertyGridItem item, string operation, int index = -1, int destination = -1, string? text = null) =>
        (bool)typeof(PropertyGrid).GetMethod("EditCollection", Private)!.Invoke(grid, new object?[] { item, operation, index, destination, text })!;

    [Fact]
    public void CollectionEditsPreserveIdentityAndMetadataWithDuplicateLabels()
    {
        var data = new DataGrid();
        var first = new DataGridColumn("same", 237, false); var second = new DataGridColumn("same", 81);
        data.Columns.AddRange(new[] { first, second });
        var grid = new PropertyGrid { SelectedObject = data };
        var item = grid.Items.Single(i => i.Name == "Columns");
        Assert.True(Edit(grid, item, "rename", 0, text: "Renamed"));
        Assert.Same(first, data.Columns[0]); Assert.Equal(237, first.Width); Assert.False(first.Sortable);
        Assert.Equal("Renamed", first.Header);
        Assert.True(Edit(grid, item, "move", 0, 1)); Assert.Same(first, data.Columns[1]);
        Assert.True(Edit(grid, item, "remove", 0)); Assert.Same(first, Assert.Single(data.Columns));
        Assert.True(Edit(grid, item, "add", text: "new")); Assert.Equal("new", data.Columns[1].Header);
    }

    public class ReadOnlyModel
    {
        [ReadOnly(true)] public string Fixed { get; set; } = "fixed";
        public string PrivateSetter { get; private set; } = "private";
        public readonly string Field = "field";
        [ReadOnly(true)] public List<string> Values { get; set; } = new() { "original" };
    }

    public class FailingItem
    {
        public string Text;
        public FailingItem(string text) { if (text == "new") throw new InvalidOperationException("construction"); Text = text; }
    }
    public class CollectionsModel
    {
        public List<FailingItem> Failing { get; set; } = new() { new("existing") };
        public List<ListBoxItem> Items { get; set; } = new() { new("same", new object()), new("same", new object()) };
        public string[] Names { get; set; } = new[] { "before" };
    }

    [Fact]
    public void FailedConstructionPreservesCollectionAndFieldEditsRetainMetadata()
    {
        var model = new CollectionsModel(); var grid = new PropertyGrid { SelectedObject = model };
        var original = model.Failing[0]; var collection = model.Failing;
        Assert.False(Edit(grid, grid.Items.Single(i => i.Name == "Failing"), "add", text: "new"));
        Assert.Same(collection, model.Failing); Assert.Same(original, Assert.Single(model.Failing));
        var item = model.Items[0]; var data = item.UserData;
        Assert.True(Edit(grid, grid.Items.Single(i => i.Name == "Items"), "rename", 0, text: "renamed"));
        Assert.Same(item, model.Items[0]); Assert.Same(data, item.UserData); Assert.Equal("renamed", item.Text);
        Assert.True(Edit(grid, grid.Items.Single(i => i.Name == "Names"), "rename", 0, text: "after"));
        Assert.Equal("after", Assert.Single(model.Names));
    }

    [Fact]
    public void ReadOnlyMetadataIsEnforcedAtEveryWrite()
    {
        var model = new ReadOnlyModel(); var grid = new PropertyGrid { SelectedObject = model };
        foreach (var item in grid.Items.Where(i => !i.IsCategoryHeader))
        {
            Assert.True(item.IsReadOnly);
            item.IsReadOnly = false; // Direct callers cannot bypass member accessibility.
            Assert.False(item.SetValue("changed"));
        }
        Assert.False(Edit(grid, grid.Items.Single(i => i.Name == "Values"), "add", text: "new"));
        Assert.Equal("original", Assert.Single(model.Values));
    }

    public class BadDetach : Panel
    {
        public override void HandleBlur() => throw new InvalidOperationException("blur");
        protected override void OnDetachedFromFishUI(FishUI.FishUI ui) => throw new InvalidOperationException("detach");
    }
    public class BadInit : Panel
    {
        public override void Init(FishUI.FishUI ui)
        {
            ui.AcquireKeyboardCapture(this);
            ui.Hotkeys.Register(FishKey.A, _ => { }, "incoming");
            ui.FocusControl(this);
            throw new InvalidOperationException("init");
        }
    }
    public class NestedLoad : Panel
    {
        public override void Init(FishUI.FishUI ui) => LayoutFormat.Deserialize(ui, "[]");
    }

    public class ReentrantDetach : Panel
    {
        protected override void OnDetachedFromFishUI(FishUI.FishUI ui) => ui.AddControl(new Panel { ID = "unexpected" });
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void FailedSourceDetachmentDoesNotAttachToDestination(bool sameUI)
    {
        using var source = new FishUITestFixture(); using var other = new FishUITestFixture();
        var parent = new Panel(); var destination = new Panel(); var child = new BadDetach();
        parent.AddChild(child); source.UI.AddControl(parent); (sameUI ? source.UI : other.UI).AddControl(destination);
        Assert.Throws<AggregateException>(() => destination.AddChild(child));
        Assert.Empty(parent.Children); Assert.Empty(destination.Children); Assert.Null(child.GetParent());
    }

    [Fact]
    public void CommittedFileLoadNotifiesDespiteCleanupAndRejectsReentrantAdditions()
    {
        using var f = new FishUITestFixture(); f.UI.AddControl(new ReentrantDetach());
        f.FileSystem.AddFile("layout.yaml", "- !Panel { ID: replacement }");
        Assert.Throws<FishUILayoutCleanupException>(() => LayoutFormat.DeserializeFromFile(f.UI, "layout.yaml"));
        Assert.Equal("replacement", Assert.Single(f.UI.GetAllControls()).ID);
        Assert.Single(f.Events.LayoutLoadedEvents);
    }

    public class ColumnModel
    {
        public List<DataGridColumn> Columns { get; set; } = new() { new("duplicate", 237, false), new("duplicate", 81) };
    }

    [Fact]
    public void PublicCollectionEditorSelectionDoesNotWriteAndFontRefreshPreservesEdit()
    {
        using var f = new FishUITestFixture(); var model = new ColumnModel();
        var grid = new PropertyGrid { SelectedObject = model, Size = new(900, 500) }; f.UI.AddControl(grid); f.Update();
        var visible = (List<PropertyGridItem>)typeof(PropertyGrid).GetField("_visibleItems", Private)!.GetValue(grid)!;
        var row = visible.FindIndex(i => i.Name == "Columns");
        var rowHeight = (float)typeof(PropertyGrid).GetProperty("RowPixels", Private)!.GetValue(grid)!;
        grid.HandleMouseClick(f.UI, new FishInputState(), FishMouseButton.Left, new(500, (row + .5f) * rowHeight));
        var panel = (Panel)typeof(PropertyGrid).GetField("_activeEditor", Private)!.GetValue(grid)!;
        var list = Assert.Single(panel.Children.OfType<ListBox>()); var edit = Assert.Single(panel.Children.OfType<Textbox>());
        int writes = 0; grid.OnPropertyValueChanged += (_, _, _, _) => writes++;
        var first = model.Columns[0]; var second = model.Columns[1];
        list.SelectIndex(0); list.SelectIndex(1); Assert.Equal(0, writes); Assert.Equal(237, first.Width);
        edit.Text = "edited"; Assert.Equal("edited", second.Header); Assert.Same(second, model.Columns[1]);
        var up = panel.Children.OfType<Button>().Single(b => b.Text == "Up");
        up.HandleMouseClick(f.UI, new FishInputState(), FishMouseButton.Left, up.GetAbsolutePosition());
        Assert.Same(second, model.Columns[0]); Assert.Same(first, model.Columns[1]);
        f.Settings.FontDefault = new FontRef("new-font.ttf", size: 30); f.Update();
        Assert.Same(panel, typeof(PropertyGrid).GetField("_activeEditor", Private)!.GetValue(grid));
        Assert.Equal("edited", edit.Text); Assert.Equal(2, writes);
    }

    [Theory]
    [InlineData(1f)] [InlineData(1.25f)] [InlineData(1.5f)] [InlineData(2f)]
    public void NestedGridAndFlowRearrangeAfterScaleAndParentResize(float scale)
    {
        using var f = new FishUITestFixture(); var parent = new Panel { Size = new(400, 200) };
        var grid = new GridLayout { Position = Vector4.Zero, Columns = 2, LayoutPadding = 0, HorizontalSpacing = 0 };
        var flow = new FlowLayout { Position = Vector4.Zero, LayoutPadding = 0, Spacing = 0 };
        var a = new Button { Size = new(90, 20) }; var b = new Button { Size = new(90, 20) };
        flow.AddChild(a); flow.AddChild(b); grid.AddChild(flow); parent.AddChild(grid); f.UI.AddControl(parent);
        f.Settings.UIScale = scale; f.Update(); Assert.Equal(200 * scale, flow.GetAbsoluteSize().X);
        Assert.Equal(a.GetAbsolutePosition().Y, b.GetAbsolutePosition().Y);
        parent.Size = new(200, 200); f.Update(); Assert.Equal(100 * scale, flow.GetAbsoluteSize().X);
        Assert.True(b.GetAbsolutePosition().Y > a.GetAbsolutePosition().Y);
    }

    [Fact]
    public void EqualDepthOrderingAndWindowChildDepthSurviveLoading()
    {
        using var f = new FishUITestFixture(); var window = new Window { ID = "window" };
        var a = new Panel { ID = "a" }; var b = new Panel { ID = "b" };
        window.AddChild(a); window.AddChild(b); a.ZDepth = b.ZDepth = 25;
        var root = new Panel { ID = "last" }; f.UI.AddControl(window); f.UI.AddControl(root); window.ZDepth = root.ZDepth = 50;
        LayoutFormat.Deserialize(f.UI, LayoutFormat.Serialize(f.UI));
        Assert.Equal(new[] { "window", "last" }, f.UI.GetAllControls().Select(c => c.ID));
        var copy = Assert.IsType<Window>(f.UI.GetAllControls()[0]);
        Assert.Equal(new[] { "a", "b" }, copy.UserChildren.Select(c => c.ID));
        Assert.All(copy.UserChildren, c => Assert.Equal(25, c.ZDepth));
        var added = new Panel(); copy.AddChild(added); Assert.True(added.ZDepth > 25);
    }

    [Fact]
    public void RemovingChildCompletesOwnershipAndCaptureCleanupAfterErrors()
    {
        using var f = new FishUITestFixture(); var parent = new Panel(); var child = new BadDetach();
        parent.AddChild(child); f.UI.AddControl(parent); f.UI.FocusControl(child); f.UI.AcquireKeyboardCapture(child);
        Assert.Throws<AggregateException>(() => parent.RemoveChild(child));
        Assert.Empty(parent.Children); Assert.Null(child.GetParent());
        Assert.Null(typeof(Control).GetProperty("AttachedFishUI", Private)!.GetValue(child));
        Assert.Null(f.UI.InputActiveControl); Assert.False(f.UI.WantsKeyboardCapture);
        parent.RemoveChild(child);
    }

    [Theory]
    [InlineData(typeof(BadInit))]
    [InlineData(typeof(NestedLoad))]
    public void PreparationFailurePreservesOriginalLayoutAndFocus(Type type)
    {
        using var f = new FishUITestFixture(); var original = new Panel(); f.UI.AddControl(original); f.UI.FocusControl(original);
        f.UI.SetModalControl(original); f.UI.Hotkeys.Register(FishKey.B, _ => { }, "original");
        var hotkeys = f.UI.Hotkeys.GetAll().ToArray();
        var options = new FishUILayoutSerializationOptions { TypeRegistry = FishUILayoutTypeRegistry.BuiltIn.Extend(new KeyValuePair<string,Type>("!Fail", type)) };
        Assert.ThrowsAny<Exception>(() => LayoutFormat.Deserialize(f.UI, "- !Fail {}", options));
        Assert.Same(original, Assert.Single(f.UI.GetAllControls())); Assert.Same(original, f.UI.InputActiveControl);
        Assert.Same(original, f.UI.ModalControl); Assert.Equal(hotkeys, f.UI.Hotkeys.GetAll());
        Assert.False(f.UI.WantsKeyboardCapture);
        LayoutFormat.Deserialize(f.UI, "- !Panel { ID: good }"); Assert.Equal("good", Assert.Single(f.UI.GetAllControls()).ID);
    }

    [Fact]
    public void CleanupFailureKeepsOnlyCompleteReplacement()
    {
        using var f = new FishUITestFixture(); f.UI.AddControl(new Panel()); f.UI.AddControl(new BadDetach());
        var error = Assert.Throws<FishUILayoutCleanupException>(() => LayoutFormat.Deserialize(f.UI, "- !Panel { ID: new }"));
        Assert.True(error.LayoutCommitted); Assert.Equal("new", Assert.Single(f.UI.GetAllControls()).ID);
    }

    [Fact]
    public void UnattachedAndClearedListSelectionDispatchStablePayloads()
    {
        var list = new ListBox(); list.AddItem("first"); var values = new List<int>();
        list.OnItemSelected += (_, index, _) => values.Add(index);
        list.SelectedIndex = 0; list.SelectedIndex = -1; list.SelectedIndex = -1;
        Assert.Equal(new[] { 0, -1 }, values);
    }

    [Fact]
    public void ListNotificationSurvivesCallbackMutationAndDetachment()
    {
        using var f = new FishUITestFixture(); var list = new ListBox { OnSelectionChangedHandler = "selected" };
        list.AddItem("original"); var original = list.Items[0]; f.UI.AddControl(list);
        list.OnItemSelected += (_, _, _) => { list.Items.Clear(); f.UI.RemoveControl(list); };
        SelectionChangedEventHandlerArgs? received = null;
        f.UI.EventHandlers.Register("selected", (_, args) => received = (SelectionChangedEventHandlerArgs)args);
        list.SelectIndex(0); Assert.NotNull(received); Assert.Equal(0, received.SelectedIndex); Assert.Same(original, received.SelectedItem);
    }

    [Fact]
    public void DataGridMouseAndKeyboardUseTheSameNotificationPath()
    {
        using var f = new FishUITestFixture(); var grid = new DataGrid { OnSelectionChangedHandler = "selected" };
        grid.AddColumn("Name"); grid.AddRow("first"); grid.AddRow("second"); f.UI.AddControl(grid); f.Update();
        var typed = new List<int>(); var named = new List<int>(); grid.OnRowSelected += (_, index, _) => typed.Add(index);
        f.UI.EventHandlers.Register("selected", (_, args) => named.Add(((SelectionChangedEventHandlerArgs)args).SelectedIndex));
        f.Input.SimulateMouseClick(FishMouseButton.Left, new(20, grid.HeaderHeight + 5)); f.Update();
        f.Input.SimulateMouseUp(FishMouseButton.Left); f.Update();
        f.Input.SimulateKeyDown(FishKey.Down); f.Update();
        Assert.Equal(new[] { 0, 1 }, typed); Assert.Equal(typed, named);
    }

    [Fact]
    public void SpreadsheetContentsAreApplicationOwnedAndUnsupportedValuesFailGeneration()
    {
        var grid = new SpreadsheetGrid { RowCount = 2, ColumnCount = 3 }; grid.SetCell(0, 0, "private-cell-data");
        string yaml = LayoutFormat.SerializeControls(new[] { grid });
        string code = new DesignerCodeGenerator().Generate(new[] { grid }, "Generated", "Form");
        Assert.DoesNotContain("private-cell-data", yaml); Assert.DoesNotContain("private-cell-data", code);
        Assert.Equal("", Assert.IsType<SpreadsheetGrid>(Assert.Single(LayoutFormat.DeserializeControls(yaml))).GetCell(0, 0));
        var control = new TreeView(); control.AddNode("node").UserData = new Uri("https://example.test");
        var error = Assert.Throws<InvalidOperationException>(() => new DesignerCodeGenerator().Generate(new[] { control }, "Generated", "Form"));
        Assert.Contains("TreeView.Nodes", error.Message);
    }

    [Fact]
    public void FilePickerSelectsFirstFileAfterEnteringDirectory()
    {
        using var f = new FishUITestFixture(); f.FileSystem.AddFile("C:/root/sub/file.txt", "content");
        var dialog = new FilePickerDialog(FilePickerMode.Open, f.FileSystem, "C:/root"); dialog.Show(f.UI);
        var list = (ListBox)typeof(FilePickerDialog).GetField("_fileListBox", Private)!.GetValue(dialog)!;
        list.SelectIndex(0); Assert.Equal("C:/root/sub", dialog.CurrentDirectory); Assert.Equal(-1, list.SelectedIndex);
        list.SelectIndex(0); Assert.Equal("file.txt", dialog.FileName);
        dialog.CurrentDirectory = "C:/root"; Assert.Equal("", dialog.FileName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MultilineReplacementNotifiesOnceAndNoOpNotAtAll(bool paste)
    {
        using var f = new FishUITestFixture(); var edit = new MultiLineEditbox { Text = "old", OnTextChangedHandler = "changed" };
        f.UI.AddControl(edit); var states = new List<string>(); int named = 0;
        edit.OnTextChanged += (_, value) => states.Add(value); f.UI.EventHandlers.Register("changed", (_, _) => named++);
        edit.SelectAll(); if (paste) edit.Paste("new"); else edit.InsertText("new");
        Assert.Equal(new[] { "new" }, states); Assert.Equal(1, named);
        edit.SelectAll(); edit.Paste("new"); Assert.Single(states); Assert.Equal(1, named);
    }

    [Fact]
    public void DataGridTypedAndNamedCallbacksReceiveCapturedSelectionAndClear()
    {
        using var f = new FishUITestFixture(); var data = new DataGrid { OnSelectionChangedHandler = "selected" };
        data.AddRow("one"); f.UI.AddControl(data); int typed=0; var named=new List<int>();
        data.OnRowSelected += (_, _, _) => typed++;
        f.UI.EventHandlers.Register("selected",(_, args)=>named.Add(((SelectionChangedEventHandlerArgs)args).SelectedIndex));
        data.SelectRow(0); data.SelectRow(0); data.ClearRows();
        Assert.Equal(2,typed); Assert.Equal(new[] { 0, -1 },named);
    }

    [Fact]
    public void RemovingTabsPreservesIdentityAndSkipsDisabledFallbacks()
    {
        var tabs=new TabControl(); tabs.AddTab("A"); var selected=tabs.AddTab("B"); tabs.AddTab("C");
        tabs.SelectedIndex=1; int notifications=0; tabs.OnSelectedIndexChanged+=(_,_,_)=>notifications++;
        tabs.RemoveTabAt(0); Assert.Same(selected,tabs.SelectedTab); Assert.Equal(1,notifications);
        tabs.TabPages[1].Enabled=false; tabs.RemoveTab(selected); Assert.Null(tabs.SelectedTab); Assert.Equal(-1,tabs.SelectedIndex);
        Assert.Equal(2,notifications);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void TreeRemovalClearsOnlyRemovedSelection(int operation)
    {
        var tree=new TreeView(); var root=tree.AddNode("root"); var child=root.AddChild("child");
        tree.SelectNode(child); int notifications=0; tree.OnNodeSelected+=(_,node)=> { Assert.Null(node); notifications++; };
        if(operation==0) tree.RemoveNode(root); else if(operation==1) root.RemoveChild(child); else tree.ClearNodes();
        Assert.Null(tree.SelectedNode); Assert.False(child.IsSelected); Assert.Equal(1,notifications);
    }

    [Theory]
    [InlineData(1f)] [InlineData(1.25f)] [InlineData(1.5f)] [InlineData(2f)]
    public void DynamicAnchorsAndDockedLayoutsUseLogicalDimensions(float scale)
    {
        using var f=new FishUITestFixture(); f.Settings.UIScale=scale;
        var parent=new Panel { Size=new(300,200) }; f.UI.AddControl(parent);
        var anchored=new Button { Position=new Vector2(250,10), Size=new(40,20), Anchor=FishUIAnchor.Right|FishUIAnchor.Top };
        parent.AddChild(anchored); Assert.Equal(250*scale,anchored.GetAbsolutePosition().X);
        parent.Size=new(400,200); Assert.Equal(350*scale,anchored.GetAbsolutePosition().X);
        var stack=new StackLayout { Size=new(100,100), Position=Vector4.Zero, StretchChildren=true };
        var child=new Button(); stack.AddChild(child); parent.AddChild(stack); f.Update();
        Assert.Equal(390*scale,child.GetAbsoluteSize().X);
    }

    [Fact]
    public void NumericPrecisionAndOrderingSurviveRoundTrip()
    {
        using var f=new FishUITestFixture(); var a=new Panel { ID="a" }; var b=new Panel { ID="b" };
        var n=new NumericUpDown { DecimalPlaces=2, Value=1.25f };
        f.UI.AddControl(a); f.UI.AddControl(b); f.UI.AddControl(n); a.BringToFront();
        LayoutFormat.Deserialize(f.UI,LayoutFormat.Serialize(f.UI)); f.Update();
        var roots=f.UI.GetAllControls(); Assert.True(roots.Single(c=>c.ID=="a").ZDepth>roots.Single(c=>c.ID=="b").ZDepth);
        var number=Assert.Single(roots.OfType<NumericUpDown>()); Assert.Equal("1.25",number.InternalTextbox.Text);
        int changes=0; number.OnValueChanged+=(_,_)=>changes++; number.DecimalPlaces=3;
        Assert.Equal("1.250",number.InternalTextbox.Text); Assert.Equal(0,changes);
        var last=new Panel(); f.UI.AddControl(last); Assert.True(last.ZDepth>roots.Max(c=>c.ZDepth));
    }

    [Fact]
    public void PropertyGridFontRefreshKeepsTheSelectedObject()
    {
        using var f=new FishUITestFixture(); var model=new ReadOnlyModel(); var grid=new PropertyGrid { SelectedObject=model };
        f.UI.AddControl(grid); f.Update(); f.Settings.FontDefault=new FontRef("other.ttf",size:28); f.Update();
        Assert.Same(f.Settings.FontDefault,typeof(PropertyGrid).GetField("_font",Private)!.GetValue(grid));
        Assert.Same(model,grid.SelectedObject);
    }
}
