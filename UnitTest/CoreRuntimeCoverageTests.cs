using System.Numerics;
using FishUI;
using FishUI.Controls;
using UnitTest.Mocks;

namespace UnitTest;

public sealed class CoreRuntimeCoverageTests
{
    [Fact]
    public void ConstructorRejectsEveryMissingRequiredService()
    {
        var settings = new FishUISettings();
        var graphics = new MockFishUIGfx();
        var input = new MockFishUIInput();
        var events = new MockFishUIEvents();

        Assert.Throws<ArgumentNullException>(() => new FishUI.FishUI(null!, graphics, input, events));
        Assert.Throws<ArgumentNullException>(() => new FishUI.FishUI(settings, null!, input, events));
        Assert.Throws<ArgumentNullException>(() => new FishUI.FishUI(settings, graphics, null!, events));
        Assert.Throws<ArgumentNullException>(() => new FishUI.FishUI(settings, graphics, input, null!));
    }

    [Fact]
    public void RootOrderingUsesDepthAlwaysOnTopAndInsertionOrder()
    {
        using var fixture = new FishUITestFixture();
        var first = new Panel { ID = "first", ZDepth = 5 };
        var back = new Panel { ID = "back", ZDepth = -2 };
        var tie = new Panel { ID = "tie", ZDepth = 5 };
        var top = new Panel { ID = "top", ZDepth = -100, AlwaysOnTop = true };
        fixture.UI.AddControl(first);
        fixture.UI.AddControl(back);
        fixture.UI.AddControl(tie);
        fixture.UI.AddControl(top);
        first.ZDepth = 5;
        back.ZDepth = -2;
        tie.ZDepth = 5;
        top.ZDepth = -100;

        Assert.Equal(new[] { back, first, tie, top }, fixture.UI.GetOrderedControls());
        Assert.Same(first, fixture.UI.FindControlByID("first"));
        Assert.Same(first, fixture.UI.FindControlByID<Panel>("first"));
        Assert.Null(fixture.UI.FindControlByID("missing"));
        Assert.Null(fixture.UI.FindControlByID<Button>("first"));

        back.BringToFront();
        Assert.True(back.ZDepth > first.ZDepth);
        back.SendToBack();
        Assert.True(back.ZDepth < top.ZDepth);
    }

    [Fact]
    public void NestedLookupModalEligibilityAndRemovalFollowHierarchy()
    {
        using var fixture = new FishUITestFixture();
        var root = new Panel { ID = "root", Size = new Vector2(80, 100) };
        var child = new Button { ID = "child", Position = new Vector2(10, 10), Size = new Vector2(40, 20) };
        var outside = new Button { ID = "outside", Position = new Vector2(100, 10), Size = new Vector2(40, 20) };
        root.AddChild(child);
        fixture.UI.AddControl(root);
        fixture.UI.AddControl(outside);

        Assert.Same(child, fixture.UI.FindControlByID("child"));
        fixture.UI.SetModalControl(root);
        Assert.Same(root, fixture.UI.ModalControl);
        Assert.Same(child, fixture.UI.PickControl(new Vector2(15, 15)));
        Assert.Null(fixture.UI.PickControl(new Vector2(110, 15)));
        fixture.UI.SetModalControl(null);

        var detached = new Button { Size = new Vector2(10, 10) };
        Assert.Throws<InvalidOperationException>(() => fixture.UI.SetModalControl(detached));
        Assert.False(fixture.UI.RemoveControl(detached));
        Assert.False(fixture.UI.RemoveControl(null!));

        fixture.UI.RemoveAllControls();
        Assert.Empty(fixture.UI.GetAllControls());
        Assert.Null(fixture.UI.ModalControl);
    }

    [Fact]
    public void AddingAttachedChildAsRootPerformsSameUiReparenting()
    {
        using var fixture = new FishUITestFixture();
        var parent = new Panel { Size = new Vector2(100, 100) };
        var child = new Button { Size = new Vector2(20, 20) };
        parent.AddChild(child);
        fixture.UI.AddControl(parent);

        fixture.UI.AddControl(child);

        Assert.Null(child.GetParent());
        Assert.DoesNotContain(child, parent.Children);
        Assert.Contains(child, fixture.UI.GetAllControls());
    }

    [Fact]
    public void KeyboardCaptureAndDisposeAreIdempotentAndEnforceOwnership()
    {
        var fixture = new FishUITestFixture();
        Assert.Throws<ArgumentNullException>(() => fixture.UI.AcquireKeyboardCapture(null!));
        IDisposable lease = fixture.UI.AcquireKeyboardCapture(new object());
        Assert.True(fixture.UI.WantsKeyboardCapture);
        lease.Dispose();
        lease.Dispose();
        Assert.False(fixture.UI.WantsKeyboardCapture);

        fixture.UI.Dispose();
        fixture.UI.Dispose();
        Assert.Equal(FishUILifecycleState.Disposed, fixture.UI.LifecycleState);
        Assert.Throws<ObjectDisposedException>(() => fixture.UI.AcquireKeyboardCapture(new object()));
        Assert.Throws<ObjectDisposedException>(() => fixture.UI.TickUpdate(0.016f, 1));
    }

    [Fact]
    public void FocusTraversalMovesForwardBackwardAndWraps()
    {
        using var fixture = new FishUITestFixture();
        var first = new Button { Size = new Vector2(20, 20), Focusable = true, TabIndex = 10 };
        var second = new Button { Size = new Vector2(20, 20), Focusable = true, TabIndex = 20 };
        var hidden = new Button { Size = new Vector2(20, 20), Focusable = true, TabIndex = 0, Visible = false };
        fixture.UI.AddControl(second);
        fixture.UI.AddControl(hidden);
        fixture.UI.AddControl(first);
        fixture.UI.TickUpdate(0.016f, 1);

        fixture.UI.FocusNextControl();
        Assert.Same(first, fixture.UI.InputActiveControl);
        fixture.UI.FocusNextControl();
        Assert.Same(second, fixture.UI.InputActiveControl);
        fixture.UI.FocusNextControl();
        Assert.Same(first, fixture.UI.InputActiveControl);
        fixture.UI.FocusNextControl(reverse: true);
        Assert.Same(second, fixture.UI.InputActiveControl);

        fixture.UI.ClearFocus();
        fixture.UI.FocusNextControl(reverse: true);
        Assert.Same(second, fixture.UI.InputActiveControl);
        fixture.UI.ClearFocus();
        hidden.Visible = false;
        first.Visible = false;
        second.Visible = false;
        fixture.UI.TickUpdate(0.016f, 2);
        fixture.UI.FocusNextControl();
        Assert.Null(fixture.UI.InputActiveControl);
    }

    [Fact]
    public void ResizePropagatesAndDrawRequiresPreparedFrame()
    {
        using var fixture = new FishUITestFixture();
        var anchored = new Panel
        {
            Size = new Vector2(20, 20),
            Anchor = FishUIAnchor.Right | FishUIAnchor.Bottom
        };
        fixture.UI.AddControl(anchored);
        fixture.UI.Resized(1024, 768);
        Assert.Equal(1024, fixture.UI.Width);
        Assert.Equal(768, fixture.UI.Height);

        Assert.Throws<InvalidOperationException>(() => fixture.UI.TickDraw(0.016f, 1));
        fixture.UI.TickUpdate(0.016f, 1);
        fixture.UI.TickDraw(0.016f, 1);
    }
}
