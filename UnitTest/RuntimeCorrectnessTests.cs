using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest;

public sealed class RuntimeCorrectnessTests
{
    private sealed class ThrowingDrawControl : Control
    {
        public override void DrawControl(FishUI.FishUI ui, float dt, float time) =>
            throw new InvalidOperationException("draw failed");
    }

    [Fact]
    public void ChildClipIsRestoredWhenDrawingThrows()
    {
        using var fixture = new FishUITestFixture();
        var panel = new Panel { Size = new Vector2(100, 100) };
        panel.AddChild(new ThrowingDrawControl { Size = new Vector2(10, 10) });
        fixture.UI.AddControl(panel);

        Assert.Throws<InvalidOperationException>(() => fixture.Update());
        Assert.Equal(fixture.Graphics.DrawCalls.Count(value => value.StartsWith("PushScissor", StringComparison.Ordinal)),
            fixture.Graphics.DrawCalls.Count(value => value == "PopScissor"));
    }
    [Fact]
    public void ChildrenDrawFromBackToFrontAndPickFromFrontToBack()
    {
        using var fixture = new FishUITestFixture();
        var parent = new MarkerControl("parent") { Size = new Vector2(100, 100) };
        var back = new MarkerControl("back") { Size = new Vector2(50, 50), ZDepth = 1 };
        var front = new MarkerControl("front") { Size = new Vector2(50, 50), ZDepth = 2 };
        parent.AddChild(front);
        parent.AddChild(back);
        back.ZDepth = 1;
        front.ZDepth = 2;
        fixture.UI.AddControl(parent);

        fixture.Update();

        string[] markers = fixture.Graphics.DrawCalls.Where(call => call.StartsWith("DrawText(\"", StringComparison.Ordinal)).ToArray();
        Assert.True(Array.FindIndex(markers, value => value.Contains("back")) <
            Array.FindIndex(markers, value => value.Contains("front")));
        Assert.Same(front, fixture.UI.PickControl(new Vector2(10, 10)));
    }

    [Fact]
    public void DisabledFrontControlDoesNotBlockEnabledControlBehindIt()
    {
        using var fixture = new FishUITestFixture();
        var back = new Button { Size = new Vector2(50, 50), ZDepth = 1 };
        var front = new Button { Size = new Vector2(50, 50), ZDepth = 2, Disabled = true };
        fixture.UI.AddControl(back);
        fixture.UI.AddControl(front);
        back.ZDepth = 1;
        front.ZDepth = 2;

        Assert.Same(back, fixture.UI.PickControl(new Vector2(10, 10)));
    }

    [Fact]
    public void DisabledAncestorMakesDescendantIneligibleForPointerAndTabFocus()
    {
        using var fixture = new FishUITestFixture();
        var parent = new Panel { Size = new Vector2(100, 100), Disabled = true };
        var child = new Button { Size = new Vector2(50, 50), Focusable = true, TabIndex = 0 };
        parent.AddChild(child);
        fixture.UI.AddControl(parent);

        Assert.Null(fixture.UI.PickControl(new Vector2(10, 10)));
        fixture.UI.FocusNextControl();
        Assert.Null(fixture.UI.InputActiveControl);
    }

    [Fact]
    public void DetachingFocusedPressedSubtreeClearsUiOwnedInteractionState()
    {
        using var fixture = new FishUITestFixture();
        var parent = new Panel { Size = new Vector2(100, 100) };
        var child = new Textbox { Size = new Vector2(50, 20) };
        parent.AddChild(child);
        fixture.UI.AddControl(parent);
        fixture.UI.FocusControl(child);
        fixture.Input.SimulateMouseClick(FishMouseButton.Left, new Vector2(5, 5));
        fixture.Update();

        parent.RemoveChild(child);

        Assert.Null(fixture.UI.InputActiveControl);
        Assert.NotSame(child, fixture.UI.PickControl(new Vector2(5, 5)));
    }

    [Fact]
    public void PrimaryTouchRoutesAsOneLeftPointerGesture()
    {
        using var fixture = new FishUITestFixture();
        var button = new Button { Size = new Vector2(100, 40) };
        int clicks = 0;
        button.Clicked += (_, _) => clicks++;
        fixture.UI.AddControl(button);

        fixture.Input.SimulateTouchPoints(new FishTouchPoint
        {
            Id = 7,
            Position = new Vector2(10, 10),
            TouchType = FishTouchType.Press
        });
        fixture.Update();
        fixture.Input.SimulateTouchPoints(new FishTouchPoint
        {
            Id = 7,
            Position = new Vector2(10, 10),
            TouchType = FishTouchType.Release
        });
        fixture.Update();

        Assert.Equal(1, clicks);
    }

    [Fact]
    public void TickRequiresSuccessfulInitializationAndInitializationIsIdempotent()
    {
        var graphics = new Mocks.MockFishUIGfx();
        using var ui = new FishUI.FishUI(new FishUISettings(), graphics, new Mocks.MockFishUIInput(),
            new Mocks.MockFishUIEvents(), new Mocks.MockFishUIFileSystem());
        Assert.Throws<InvalidOperationException>(() => ui.TickUpdate(0.016f, 1));

        ui.Init();
        ui.Init();
        Assert.Equal(FishUILifecycleState.Initialized, ui.LifecycleState);
    }

    [Fact]
    public void ScissorScopeValidatesBackendAndDefaultScopeIsSafe()
    {
        Assert.Throws<ArgumentNullException>(() =>
            FishUIGraphicsScopes.PushScissorScope(null!, Vector2.Zero, Vector2.One));
        default(FishUIScissorScope).Dispose();
    }

    [Fact]
    public void DrawDoesNotAdvanceControlStateAndCanRepeatPreparedFrame()
    {
        using var fixture = new FishUITestFixture();
        var control = new UpdatingControl { Size = new Vector2(20, 20) };
        fixture.UI.AddControl(control);

        fixture.UI.TickUpdate(0.016f, 1);
        fixture.UI.TickDraw(0.016f, 1);
        fixture.UI.TickDraw(0.016f, 1);

        Assert.Equal(1, control.UpdateCount);
        Assert.Equal(2, control.DrawCount);
    }

    [Fact]
    public void CrossUiMovementInitializesControlForEachUi()
    {
        using var first = new FishUITestFixture();
        using var second = new FishUITestFixture();
        var control = new InitializingControl { Size = new Vector2(20, 20) };
        first.UI.AddControl(control);
        first.UI.TickUpdate(0.016f, 1);

        second.UI.AddControl(control);
        second.UI.TickUpdate(0.016f, 2);

        Assert.Equal(2, control.InitializationCount);
        Assert.Same(second.UI, control.InitializedUis[1]);
    }

    [Fact]
    public void KeyQueueRoutesEveryPressOnce()
    {
        using var fixture = new FishUITestFixture();
        var control = new KeyRecordingControl { Size = new Vector2(20, 20), Focusable = true };
        fixture.UI.AddControl(control);
        fixture.UI.FocusControl(control);
        fixture.Input.SimulateKeyDown(FishKey.A);
        fixture.Input.SimulateKeyDown(FishKey.B);
        fixture.Input.SimulateKeyDown(FishKey.C);

        fixture.UI.TickUpdate(0.016f, 1);

        Assert.Equal(new[] { FishKey.A, FishKey.B, FishKey.C }, control.PressedKeys);
        Assert.Equal(4, fixture.Input.GetKeyPressedCallCount);
    }

    [Fact]
    public void TouchOwnerDoesNotSwitchWhenAnotherFingerRemains()
    {
        using var fixture = new FishUITestFixture();
        var first = new Button { Size = new Vector2(50, 40) };
        var second = new Button { Position = new Vector2(60, 0), Size = new Vector2(50, 40) };
        int firstClicks = 0;
        int secondClicks = 0;
        first.Clicked += (_, _) => firstClicks++;
        second.Clicked += (_, _) => secondClicks++;
        fixture.UI.AddControl(first);
        fixture.UI.AddControl(second);

        fixture.Input.SimulateTouchPoints(
            new FishTouchPoint { Id = 1, Position = new Vector2(10, 10), TouchType = FishTouchType.Press },
            new FishTouchPoint { Id = 2, Position = new Vector2(70, 10), TouchType = FishTouchType.Press });
        fixture.Update();
        fixture.Input.SimulateTouchPoints(
            new FishTouchPoint { Id = 1, Position = new Vector2(10, 10), TouchType = FishTouchType.Release },
            new FishTouchPoint { Id = 2, Position = new Vector2(70, 10), TouchType = FishTouchType.Motion });
        fixture.Update();

        Assert.Equal(1, firstClicks);
        Assert.Equal(0, secondClicks);

        fixture.Input.SimulateTouchPoints(
            new FishTouchPoint { Id = 2, Position = new Vector2(70, 10), TouchType = FishTouchType.Motion });
        fixture.Update();
        fixture.Input.SimulateTouchPoints(
            new FishTouchPoint { Id = 2, Position = new Vector2(70, 10), TouchType = FishTouchType.Release });
        fixture.Update();

        Assert.Equal(1, secondClicks);
    }

    private sealed class UpdatingControl : Control
    {
        public int UpdateCount { get; private set; }
        public int DrawCount { get; private set; }
        protected override void OnFishUIUpdate(FishUI.FishUI ui, float deltaTime, float time) => UpdateCount++;
        public override void DrawControl(FishUI.FishUI ui, float dt, float time) => DrawCount++;
    }

    private sealed class InitializingControl : Control
    {
        public int InitializationCount { get; private set; }
        public List<FishUI.FishUI> InitializedUis { get; } = new();
        public override void Init(FishUI.FishUI ui)
        {
            InitializationCount++;
            InitializedUis.Add(ui);
        }
    }

    private sealed class KeyRecordingControl : Control
    {
        public List<FishKey> PressedKeys { get; } = new();
        public override void HandleKeyPressed(FishUI.FishUI ui, FishInputState input, FishKey key) => PressedKeys.Add(key);
    }

    private sealed class MarkerControl : Control
    {
        private readonly string _marker;
        public MarkerControl(string marker) => _marker = marker;
        public override void DrawControl(FishUI.FishUI ui, float dt, float time) =>
            ui.Graphics.DrawText(ui.Settings.FontDefault, _marker, GetAbsolutePosition());
    }
}
