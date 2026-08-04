using System.ComponentModel;
using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest;

public sealed class RichControlInteractionTests
{
    [Fact]
    public void PopulatedCollectionControlsRenderSelectSortResizeAndScroll()
    {
        using var fixture = new FishUITestFixture(1000, 700);
        var list = new ListBox
        {
            Position = new Vector2(10, 10),
            Size = new Vector2(180, 120),
            MultiSelect = true,
            AlternatingRowColors = true,
            CustomItemHeight = 22,
            CustomItemRenderer = (ui, item, index, position, size, selected, hovered) =>
                ui.Graphics.DrawTextColor(ui.Settings.FontDefault, item.Text, position, selected ? FishColor.White : FishColor.Black)
        };
        for (int i = 0; i < 20; i++) list.AddItem(new ListBoxItem("Item " + i, i));

        var itemList = new ItemListbox
        {
            Position = new Vector2(210, 10),
            Size = new Vector2(180, 120),
            AlternatingRowColors = true
        };
        itemList.AddItem("Text row");
        itemList.AddItem(new Button { Text = "Widget", Size = new Vector2(100, 20) });
        for (int i = 0; i < 15; i++) itemList.AddItem("Entry " + i);

        var grid = new DataGrid
        {
            Position = new Vector2(410, 10),
            Size = new Vector2(400, 180),
            MultiSelect = true,
            AlternatingRowColors = true
        };
        grid.AddColumn("ID", 60, true);
        grid.AddColumn("Name", 150, true);
        grid.AddColumn("Value", 120, true);
        for (int i = 0; i < 20; i++) grid.AddRow(i.ToString(), "Row " + i, (20 - i).ToString());

        fixture.UI.AddControl(list);
        fixture.UI.AddControl(itemList);
        fixture.UI.AddControl(grid);
        list.SelectIndex(1);
        list.SelectAll();
        list.ClearSelection();
        list.SelectIndex(2);
        itemList.SelectIndex(1);
        fixture.Update();

        fixture.Input.SimulateMouseMove(new Vector2(20, 45));
        fixture.Input.SimulateMouseClick(FishMouseButton.Left, new Vector2(20, 45));
        fixture.Update();
        fixture.Input.SimulateMouseUp(FishMouseButton.Left);
        fixture.Update();
        fixture.Input.MouseWheel = -2;
        fixture.Input.SimulateMouseMove(new Vector2(20, 90));
        fixture.Update();

        fixture.Input.SimulateMouseClick(FishMouseButton.Left, new Vector2(430, 20));
        fixture.Update();
        fixture.Input.SimulateMouseUp(FishMouseButton.Left);
        fixture.Update();
        fixture.Input.SimulateMouseMove(new Vector2(520, 80));
        fixture.Input.SimulateMouseClick(FishMouseButton.Left, new Vector2(520, 80));
        fixture.Update();
        fixture.Input.SimulateMouseUp(FishMouseButton.Left);
        fixture.Update();
        fixture.Input.MouseWheel = -3;
        fixture.Update();

        Assert.Equal(20, list.ItemCount);
        Assert.NotNull(itemList.GetSelectedItem());
        Assert.NotEmpty(grid.Rows);
        Assert.True(fixture.Graphics.DrawCalls.Count > 100);
    }

    [Fact]
    public void PopulatedTreeTabsMenusAndTimePickerExerciseOpenStates()
    {
        using var fixture = new FishUITestFixture(1000, 700);
        var tree = new TreeView
        {
            Position = new Vector2(10, 10),
            Size = new Vector2(240, 220),
            ShowLines = true,
            UseThemeColors = false
        };
        TreeNode root = tree.AddNode("Root");
        TreeNode child = root.AddChild("Child");
        child.AddChild("Grandchild");
        TreeNode lazy = tree.AddNode("Lazy");
        lazy.HasChildrenToLoad = true;
        tree.OnLazyLoad += (_, node) => node.AddChild("Loaded");
        tree.SetNodeExpanded(root, true);
        tree.SetNodeExpanded(child, true);
        tree.SetNodeExpanded(lazy, true);
        tree.SelectNode(child);

        var tabs = new TabControl { Position = new Vector2(270, 10), Size = new Vector2(300, 220) };
        tabs.AddTab(new TabPage("First", new Panel()));
        tabs.AddTab(new TabPage("Disabled", new Panel()) { Enabled = false });
        tabs.AddTab(new TabPage("Third", new Panel()));
        tabs.TabPages[0].Content.AddChild(new Label("Tab content"));
        tabs.SelectedIndex = 2;

        var menu = new ContextMenu { Size = new Vector2(180, 100) };
        MenuItem normal = menu.AddItem("Open");
        normal.ShortcutText = "Ctrl+O";
        menu.AddCheckItem("Checked", true);
        menu.AddSeparator();
        MenuItem submenu = menu.AddSubmenu("Recent");
        submenu.AddItem("One");
        submenu.AddSeparator();
        submenu.AddItem("Two");

        var menuBar = new MenuBar { Position = new Vector2(10, 260), Size = new Vector2(500, 24) };
        MenuBarItem file = menuBar.AddMenu("File");
        file.AddItem("New").ShortcutText = "Ctrl+N";
        file.AddCheckItem("Visible", true);
        file.AddSeparator();
        file.AddSubmenu("More").AddItem("Nested");

        var picker = new TimePicker
        {
            Position = new Vector2(600, 10),
            Use24HourFormat = false,
            ShowSeconds = true,
            Value = new TimeSpan(23, 59, 58)
        };

        fixture.UI.AddControl(tree);
        fixture.UI.AddControl(tabs);
        fixture.UI.AddControl(menu);
        fixture.UI.AddControl(menuBar);
        fixture.UI.AddControl(picker);
        menu.Show(new Vector2(580, 260));
        fixture.Update();

        fixture.Input.SimulateMouseMove(new Vector2(595, 275));
        fixture.Update();
        fixture.Input.SimulateMouseMove(new Vector2(20, 272));
        fixture.Input.SimulateMouseClick(FishMouseButton.Left, new Vector2(20, 272));
        fixture.Update();
        fixture.Input.SimulateMouseUp(FishMouseButton.Left);
        fixture.Update();
        fixture.Input.SimulateMouseMove(new Vector2(300, 22));
        fixture.Input.SimulateMouseClick(FishMouseButton.Left, new Vector2(300, 22));
        fixture.Update();
        fixture.Input.SimulateMouseUp(FishMouseButton.Left);
        fixture.Update();

        picker.HandleMouseMove(fixture.UI, new FishInputState(), picker.GetAbsolutePosition() + new Vector2(10, 5));
        picker.HandleMousePress(fixture.UI, new FishInputState(), FishMouseButton.Left,
            picker.GetAbsolutePosition() + new Vector2(picker.Size.X - 5, 5));
        picker.Use24HourFormat = true;
        picker.ShowSeconds = false;
        picker.UpdateSize();
        fixture.Update();

        Assert.True(root.IsExpanded);
        Assert.True(lazy.LazyLoaded);
        Assert.Equal(0, tabs.SelectedIndex);
        Assert.NotEmpty(picker.GetFormattedTime());
        Assert.True(fixture.Graphics.DrawCalls.Count > 100);
    }

    [Fact]
    public void PropertyGridBuildsNestedCollectionAndEditablePropertyKinds()
    {
        using var fixture = new FishUITestFixture(800, 600);
        var model = new PropertyModel();
        var grid = new PropertyGrid
        {
            Position = new Vector2(10, 10),
            Size = new Vector2(420, 500),
            SelectedObject = model,
            GroupByCategory = true,
            SortAlphabetically = true
        };
        fixture.UI.AddControl(grid);
        fixture.Update();

        Assert.NotEmpty(grid.Items);
        foreach (PropertyGridItem item in grid.Items)
        {
            item.CaptureDefaultValue();
            _ = item.CanResetToDefault();
        }
        grid.RefreshValues();
        grid.GroupByCategory = false;
        grid.SortAlphabetically = false;
        grid.RebuildPropertyList();
        fixture.Update();

        for (int y = 20; y < 250; y += 24)
        {
            fixture.Input.SimulateMouseMove(new Vector2(250, y));
            fixture.Input.SimulateMouseClick(FishMouseButton.Left, new Vector2(250, y));
            fixture.Update();
            fixture.Input.SimulateMouseUp(FishMouseButton.Left);
            fixture.Update();
        }

        Assert.True(fixture.Graphics.DrawCalls.Count > 100);
    }

    private sealed class PropertyModel
    {
        [Category("General"), DefaultValue("Fish")]
        public string Name { get; set; } = "Fish";
        [Category("General"), DefaultValue(true)]
        public bool Enabled { get; set; } = true;
        [Category("Values"), DefaultValue(5)]
        public int Count { get; set; } = 5;
        [Category("Values"), DefaultValue(1.5f)]
        public float Speed { get; set; } = 1.5f;
        public Align Alignment { get; set; } = Align.Center;
        public FishColor Color { get; set; } = FishColor.Cyan;
        public Vector2 Position { get; set; } = new Vector2(1, 2);
        public List<int> Values { get; set; } = new() { 1, 2, 3 };
        public NestedPropertyModel Nested { get; set; } = new();
        [ReadOnly(true)]
        public string ReadOnlyValue => "fixed";
    }

    private sealed class NestedPropertyModel
    {
        public int Number { get; set; } = 7;
    }
}
