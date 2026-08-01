using System.Numerics;
using FishUI;

namespace UnitTest;

public sealed class AnimationManagerTests
{
    [Fact]
    public void EveryEasingFunctionHandlesEndpointsAndInteriorValues()
    {
        foreach (Easing easing in Enum.GetValues<Easing>())
        {
            float start = EasingFunctions.Apply(easing, -1f);
            float middle = EasingFunctions.Apply(easing, 0.6f);
            float end = EasingFunctions.Apply(easing, 2f);
            Assert.Equal(0f, start, 4);
            Assert.True(float.IsFinite(middle));
            Assert.Equal(1f, end, 4);
        }

        Assert.True(EasingFunctions.Apply(Easing.EaseOutBounce, 0.2f) < 1f);
        Assert.True(EasingFunctions.Apply(Easing.EaseOutBounce, 0.5f) < 1f);
        Assert.True(EasingFunctions.Apply(Easing.EaseOutBounce, 0.8f) < 1f);
        Assert.True(EasingFunctions.Apply(Easing.EaseOutBounce, 0.95f) < 1f);
    }

    [Fact]
    public void DelayedFloatVectorAndColorAnimationsApplyStartAndEndValues()
    {
        float floatValue = -1f;
        Vector2 vectorValue = new(-1, -1);
        FishColor colorValue = FishColor.Black;
        var scalar = new FishUIAnimation
        {
            StartValue = 2,
            EndValue = 6,
            Delay = 1,
            Duration = 1,
            ApplyValue = value => floatValue = value
        };
        var vector = new FishUIAnimationVector2
        {
            StartValue = Vector2.Zero,
            EndValue = new Vector2(10, 20),
            Delay = 1,
            Duration = 1,
            ApplyValue = value => vectorValue = value
        };
        var color = new FishUIAnimationColor
        {
            StartValue = FishColor.Black,
            EndValue = FishColor.White,
            Delay = 1,
            Duration = 1,
            ApplyValue = value => colorValue = value
        };

        scalar.Update(0.5f);
        vector.Update(0.5f);
        color.Update(0.5f);
        Assert.Equal(2, floatValue);
        Assert.Equal(Vector2.Zero, vectorValue);
        Assert.Equal(FishColor.Black, colorValue);

        scalar.Update(1.5f);
        vector.Update(1.5f);
        color.Update(1.5f);
        Assert.Equal(6, floatValue);
        Assert.Equal(new Vector2(10, 20), vectorValue);
        Assert.Equal(FishColor.White, colorValue);
        Assert.True(scalar.IsComplete);
        Assert.False(scalar.IsRunning);
    }

    [Fact]
    public void ManagerReplacesMatchingAnimationsAndStopsEveryAnimationKind()
    {
        var manager = new FishUIAnimationManager();
        object target = new();
        manager.Add(new FishUIAnimation { Id = "float-1", Target = target, PropertyName = "Value", Duration = 1 });
        manager.Add(new FishUIAnimation { Id = "float-2", Target = target, PropertyName = "Value", Duration = 1 });
        manager.Add(new FishUIAnimationVector2 { Id = "vector", Target = target, PropertyName = "Position", Duration = 1 });
        manager.Add(new FishUIAnimationColor { Id = "color", Target = target, Duration = 1 });
        Assert.Equal(3, manager.ActiveAnimationCount);

        manager.StopAnimation("vector");
        Assert.Equal(2, manager.ActiveAnimationCount);
        manager.StopAnimationsFor(target);
        Assert.Equal(0, manager.ActiveAnimationCount);

        manager.Add(new FishUIAnimation { Id = "float", Duration = 1 });
        manager.Add(new FishUIAnimationVector2 { Id = "vector-2", Duration = 1 });
        manager.Add(new FishUIAnimationColor { Id = "color-2", Duration = 1 });
        manager.StopAll();
        Assert.Equal(0, manager.ActiveAnimationCount);
    }

    [Fact]
    public void ManagerRejectsNullAndCompletesImmediateVectorAndColorAnimations()
    {
        var manager = new FishUIAnimationManager();
        Assert.Throws<ArgumentNullException>(() => manager.Add((FishUIAnimation)null!));
        Assert.Throws<ArgumentNullException>(() => manager.Add((FishUIAnimationVector2)null!));
        Assert.Throws<ArgumentNullException>(() => manager.Add((FishUIAnimationColor)null!));

        Vector2 vector = Vector2.Zero;
        FishColor color = FishColor.Black;
        int completed = 0;
        manager.Add(new FishUIAnimationVector2
        {
            EndValue = Vector2.One,
            ApplyValue = value => vector = value,
            OnComplete = () => completed++
        });
        manager.Add(new FishUIAnimationColor
        {
            EndValue = FishColor.White,
            ApplyValue = value => color = value,
            OnComplete = () => completed++
        });

        Assert.Equal(Vector2.One, vector);
        Assert.Equal(FishColor.White, color);
        Assert.Equal(2, completed);
        Assert.Equal(0, manager.ActiveAnimationCount);
    }
    [Fact]
    public void ZeroDurationAnimationCompletesImmediatelyOnce()
    {
        FishUIAnimationManager manager = new FishUIAnimationManager();
        int applied = 0;
        int completed = 0;

        manager.Add(new FishUIAnimation
        {
            Duration = 0,
            EndValue = 42,
            ApplyValue = value => { Assert.Equal(42, value); applied++; },
            OnComplete = () => completed++
        });

        Assert.Equal(1, applied);
        Assert.Equal(1, completed);
        Assert.Equal(0, manager.ActiveAnimationCount);
        manager.Update(1);
        Assert.Equal(1, completed);
    }

    [Fact]
    public void CompletionCallbackCanAddAndStopAnimations()
    {
        FishUIAnimationManager manager = new FishUIAnimationManager();
        object target = new object();
        int firstCompleted = 0;
        int secondCompleted = 0;

        manager.Add(new FishUIAnimation
        {
            Id = "first",
            Target = target,
            PropertyName = "Value",
            Duration = 0.1f,
            OnComplete = () =>
            {
                firstCompleted++;
                manager.StopAnimationsFor(target);
                manager.Add(new FishUIAnimationVector2
                {
                    Id = "second",
                    Target = target,
                    PropertyName = "Position",
                    StartValue = Vector2.Zero,
                    EndValue = Vector2.One,
                    Duration = 0.1f,
                    OnComplete = () => secondCompleted++
                });
            }
        });

        manager.Update(0.1f);
        Assert.Equal(1, firstCompleted);
        Assert.Equal(0, secondCompleted);
        Assert.Equal(1, manager.ActiveAnimationCount);

        manager.Update(0.1f);
        Assert.Equal(1, firstCompleted);
        Assert.Equal(1, secondCompleted);
        Assert.Equal(0, manager.ActiveAnimationCount);
    }
}
