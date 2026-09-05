using System.Numerics;
using FishUI.Controls;

namespace UnitTest;

public sealed class AuditRegressionTests
{
    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    public void MultilinePartialSelectionNormalizesBeforeExpandingGraphemes(int start, int end)
    {
        MultiLineEditbox text = new() { Text = "A\U0001F600B", SelectionStartColumn = start, SelectionEndColumn = end };
        Assert.Equal("\U0001F600", text.GetSelectedText());
        text.Paste("x");
        Assert.Equal("AxB", text.Text);
    }

    [Fact]
    public void LabelLayoutIsReadyBeforeFirstDrawAndDrawDoesNotMoveIt()
    {
        using FishUITestFixture fixture = new();
        CheckBox box = new("Label") { Size = new Vector2(30, 40) };
        fixture.UI.AddControl(box);
        fixture.Update();
        Label label = box.FindChildByType<Label>();
        var prepared = label.Position;
        Assert.Equal(34, prepared.X);
        fixture.UI.TickDraw(0, 0);
        Assert.Equal(prepared, label.Position);
    }

    [Fact]
    public void PasteAndPartialSelectionConsumeWholeGraphemes()
    {
        Textbox text = new() { Text = "A\U0001F600B", SelectionStart = 2, SelectionLength = 1 };
        Assert.Equal("\U0001F600", text.GetSelectedText());
        text.Paste("e\u0301");
        Assert.Equal("Ae\u0301B", text.Text);
        Assert.Equal(3, text.CursorPosition);
        text.CursorPosition = 2;
        Assert.Equal(1, text.CursorPosition);
    }

    [Fact]
    public void CompletionCallbackRestartDoesNotConsumeOldElapsedTime()
    {
        using FishUITestFixture fixture = new();
        AnimatedImageBox animation = new() { Loop = false, FrameRate = 10 };
        animation.Frames.Add(null!); animation.Frames.Add(null!);
        int completed = 0;
        animation.OnAnimationComplete += a => { completed++; a.Play(); };
        fixture.UI.AddControl(animation);
        fixture.UI.TickUpdate(100, 100);
        Assert.Equal(1, completed);
    }

    [Theory]
    [InlineData("\U0001F600")]
    [InlineData("e\u0301")]
    [InlineData("\U0001F469\u200D\U0001F4BB")]
    public void BackspaceRemovesOneGrapheme(string grapheme)
    {
        using FishUITestFixture fixture = new();
        Textbox text = new() { Text = "A" + grapheme };
        fixture.UI.AddControl(text);
        fixture.UI.FocusControl(text);
        text.CursorPosition = text.Text.Length;
        text.HandleTextInput(fixture.UI, new FishUI.FishInputState(), '\b');
        Assert.Equal("A", text.Text);
        Assert.Equal(1, text.CursorPosition);
    }

    [Fact]
    public void InvalidUtf16AndLengthLimitsNeverLeaveHalfAScalar()
    {
        Textbox text = new() { Text = "\uD800x" };
        Assert.Equal("\uFFFDx", text.Text);
        text.MaxLength = 1;
        text.Text = "\U0001F600";
        Assert.Equal("", text.Text);
    }

    [Fact]
    public void SerializedRangesDoNotDependOnPropertyOrder()
    {
        var controls = FishUI.LayoutFormat.DeserializeControls("- !Slider\n  Value: 400\n  MinValue: 200\n  MaxValue: 300\n");
        Slider slider = Assert.IsType<Slider>(Assert.Single(controls));
        Assert.Equal(300, slider.Value);
        int changes = 0;
        slider.OnValueChanged += (_, _) => changes++;
        slider.SetRange(-20, -10);
        Assert.Equal(-10, slider.Value);
        Assert.Equal(1, changes);
    }

    [Fact]
    public void CatchUpAndEmissionAreBounded()
    {
        using FishUITestFixture fixture = new();
        AnimatedImageBox animation = new() { FrameRate = 1000000 };
        animation.Frames.Add(null!); animation.Frames.Add(null!);
        int transitions = 0;
        animation.OnFrameChanged += (_, _) => transitions++;
        ParticleEmitter particles = new() { MaxParticles = 8, EmissionRate = float.MaxValue };
        fixture.UI.AddControl(animation); fixture.UI.AddControl(particles);
        fixture.UI.TickUpdate(1, 1);
        Assert.InRange(transitions, 0, 257);
        Assert.InRange(particles.ParticleCount, 0, 8);
        Assert.Throws<ArgumentOutOfRangeException>(() => animation.FrameRate = float.PositiveInfinity);
        Assert.Throws<ArgumentOutOfRangeException>(() => fixture.UI.TickUpdate(float.NaN, 0));
    }

    [Fact]
    public void SupplementaryInputIsPreserved()
    {
        using FishUITestFixture fixture = new();
        Textbox text = new();
        fixture.UI.AddControl(text);
        fixture.UI.FocusControl(text);
        fixture.Input.SimulateCharTyped(0x1F600);
        fixture.Update();
        Assert.Equal("\U0001F600", text.Text);
    }

    [Fact]
    public void EqualTabIndicesPreserveHierarchyOrder()
    {
        using FishUITestFixture fixture = new();
        Button[] buttons = Enumerable.Range(0, 20).Select(_ => new Button { Focusable = true }).ToArray();
        foreach (Button button in buttons) fixture.UI.AddControl(button);
        foreach (Button button in buttons)
        {
            fixture.UI.FocusNextControl();
            Assert.Same(button, fixture.UI.InputActiveControl);
        }
        for (int index = buttons.Length - 2; index >= 0; index--)
        {
            fixture.UI.FocusNextControl(reverse: true);
            Assert.Same(buttons[index], fixture.UI.InputActiveControl);
        }
    }

    [Fact]
    public void RangeChangesClampValuesAndRejectNaN()
    {
        Slider slider = new() { Value = 50 };
        slider.MaxValue = 10;
        Assert.Equal(10, slider.Value);
        Assert.Throws<ArgumentOutOfRangeException>(() => slider.Value = float.NaN);
    }

    [Fact]
    public void AnimationCompletesOnceDuringLongUpdate()
    {
        using FishUITestFixture fixture = new();
        AnimatedImageBox animation = new() { Loop = false, FrameRate = 10 };
        animation.Frames.Add(null!);
        animation.Frames.Add(null!);
        int completions = 0;
        animation.OnAnimationComplete += _ => completions++;
        fixture.UI.AddControl(animation);
        fixture.UI.TickUpdate(1, 1);
        Assert.Equal(1, completions);
        Assert.False(animation.IsPlaying);
    }
}
