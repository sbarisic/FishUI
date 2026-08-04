using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest;

public sealed class TabControlRenderingTests
{
    [Fact]
    public void SelectedTabOverlapsContentBorderWhileInactiveTabDoesNot()
    {
        using var fixture = new FishUITestFixture();
        ConfigureTabTheme(fixture);
        var tabs = new TabControl
        {
            Position = Vector2.Zero,
            Size = new Vector2(300, 200),
            TabHeaderHeight = 24,
            ContentPadding = 0
        };
        tabs.AddTab(new TabPage("First", new Panel()));
        tabs.AddTab("Second");
        fixture.UI.AddControl(tabs);

        fixture.Update();

        Assert.Contains("DrawNPatch(<0, 0>, <300, 24>)", fixture.Graphics.DrawCalls);
        Assert.Contains("DrawNPatch(<0, 24>, <300, 176>)", fixture.Graphics.DrawCalls);
        Assert.Contains("DrawNPatch(<5, 0>, <60, 32>)", fixture.Graphics.DrawCalls);
        Assert.Contains("DrawNPatch(<64, 0>, <68, 24>)", fixture.Graphics.DrawCalls);
        int selectedTabDraw = fixture.Graphics.DrawCalls.FindLastIndex(call => call == "DrawNPatch(<5, 0>, <60, 32>)");
        int selectedPageDraw = fixture.Graphics.DrawCalls.FindLastIndex(call => call == "DrawNPatch(<0, 24>, <300, 176>)");
        Assert.True(selectedTabDraw > selectedPageDraw, "The selected tab overlap must be painted over the selected page.");
    }

    [Fact]
    public void HeaderInsetIsNotPartOfTheFirstTabHitArea()
    {
        using var fixture = new FishUITestFixture();
        ConfigureTabTheme(fixture);
        var tabs = new TabControl { Size = new Vector2(300, 200) };
        tabs.AddTab("First");
        tabs.AddTab("Second");
        tabs.SelectedIndex = 1;
        fixture.UI.AddControl(tabs);
        fixture.UI.TickUpdate(0.016f, 1);

        tabs.HandleMouseClick(fixture.UI, new FishInputState(), FishMouseButton.Left, new Vector2(2, 10));
        Assert.Equal(1, tabs.SelectedIndex);

        tabs.HandleMouseClick(fixture.UI, new FishInputState(), FishMouseButton.Left, new Vector2(6, 10));
        Assert.Equal(0, tabs.SelectedIndex);
    }

    [Fact]
    public void TabContentLayoutIsPreparedBeforeDrawingAndDrawIsStateFree()
    {
        using var fixture = new FishUITestFixture();
        ConfigureTabTheme(fixture);
        var tabs = new TabControl { Size = new Vector2(300, 200), ContentPadding = 4 };
        TabPage page = tabs.AddTab("First");
        fixture.UI.AddControl(tabs);

        fixture.UI.TickUpdate(0.016f, 1);
        Assert.Equal(new Vector2(4, 28), new Vector2(page.Content.Position.X, page.Content.Position.Y));
        Assert.Equal(new Vector2(292, 168), page.Content.Size);

        page.Content.Position = new Vector2(9, 10);
        page.Content.Size = new Vector2(11, 12);
        tabs.DrawControl(fixture.UI, 0.016f, 1);

        Assert.Equal(new Vector2(9, 10), new Vector2(page.Content.Position.X, page.Content.Position.Y));
        Assert.Equal(new Vector2(11, 12), page.Content.Size);
    }

    [Fact]
    public void HeaderAndOverlapScaleWithUiScale()
    {
        using var fixture = new FishUITestFixture();
        fixture.Settings.UIScale = 2;
        ConfigureTabTheme(fixture);
        var tabs = new TabControl
        {
            Size = new Vector2(150, 100),
            TabHeaderHeight = 24
        };
        tabs.AddTab("First");
        fixture.UI.AddControl(tabs);

        fixture.Update();

        Assert.Contains("DrawNPatch(<0, 48>, <300, 152>)", fixture.Graphics.DrawCalls);
        Assert.Contains("DrawNPatch(<10, 0>, <120, 64>)", fixture.Graphics.DrawCalls);
    }

    private static void ConfigureTabTheme(FishUITestFixture fixture)
    {
        var image = new ImageRef("tabs", 128, 128);
        fixture.Settings.ImgPanel = new NPatch(image, 0, 0, 127, 127, 3, 3, 3, 3);
        fixture.Settings.ImgTabHeaderBar = new NPatch(image, 0, 0, 127, 31, 2, 2, 2, 2);
        fixture.Settings.ImgTabControlBackground = new NPatch(image, 0, 0, 127, 127, 3, 3, 3, 3);
        fixture.Settings.ImgTabTopActive = new NPatch(image, 0, 0, 63, 31, 3, 3, 3, 3);
        fixture.Settings.ImgTabTopInactive = new NPatch(image, 0, 0, 63, 31, 3, 3, 3, 3);
    }
}
