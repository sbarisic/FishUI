using System;
using System.Collections.Generic;
using System.Numerics;

namespace FishUI
{
    /// <summary>
    /// Easing functions for animations.
    /// </summary>
    public enum Easing
    {
        /// <summary>Linear interpolation (constant speed).</summary>
        Linear,

        /// <summary>Starts slow, accelerates.</summary>
        EaseIn,

        /// <summary>Starts fast, decelerates.</summary>
        EaseOut,

        /// <summary>Starts slow, speeds up, then slows down.</summary>
        EaseInOut,

        /// <summary>Quadratic ease in.</summary>
        EaseInQuad,

        /// <summary>Quadratic ease out.</summary>
        EaseOutQuad,

        /// <summary>Quadratic ease in and out.</summary>
        EaseInOutQuad,

        /// <summary>Cubic ease in.</summary>
        EaseInCubic,

        /// <summary>Cubic ease out.</summary>
        EaseOutCubic,

        /// <summary>Cubic ease in and out.</summary>
        EaseInOutCubic,

        /// <summary>Elastic bounce at the end.</summary>
        EaseOutElastic,

        /// <summary>Overshoot and bounce back.</summary>
        EaseOutBack,

        /// <summary>Bounce effect at the end.</summary>
        EaseOutBounce
    }

    /// <summary>
    /// Static class providing easing function calculations.
    /// </summary>
    public static class EasingFunctions
    {
        /// <summary>
        /// Applies the specified easing function to a normalized time value (0-1).
        /// </summary>
        /// <param name="easing">The easing type to apply.</param>
        /// <param name="t">Normalized time (0 to 1).</param>
        /// <returns>The eased value (0 to 1).</returns>
        public static float Apply(Easing easing, float t)
        {
            t = Math.Clamp(t, 0f, 1f);

            return easing switch
            {
                Easing.Linear => t,
                Easing.EaseIn => t * t,
                Easing.EaseOut => 1f - (1f - t) * (1f - t),
                Easing.EaseInOut => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f,
                Easing.EaseInQuad => t * t,
                Easing.EaseOutQuad => 1f - (1f - t) * (1f - t),
                Easing.EaseInOutQuad => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f,
                Easing.EaseInCubic => t * t * t,
                Easing.EaseOutCubic => 1f - MathF.Pow(1f - t, 3f),
                Easing.EaseInOutCubic => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f,
                Easing.EaseOutElastic => EaseOutElastic(t),
                Easing.EaseOutBack => EaseOutBack(t),
                Easing.EaseOutBounce => EaseOutBounce(t),
                _ => t
            };
        }

        private static float EaseOutElastic(float t)
        {
            const float c4 = (2f * MathF.PI) / 3f;
            return t == 0f ? 0f : t == 1f ? 1f : MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * c4) + 1f;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
        }

        private static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
                return n1 * t * t;
            else if (t < 2f / d1)
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            else if (t < 2.5f / d1)
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            else
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
    }

    /// <summary>
    /// Represents a single property animation.
    /// </summary>
    public class FishUIAnimation
    {
        /// <summary>
        /// Unique identifier for this animation.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The target object being animated.
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// Name of the property being animated.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Starting value of the animation.
        /// </summary>
        public float StartValue { get; set; }

        /// <summary>
        /// Ending value of the animation.
        /// </summary>
        public float EndValue { get; set; }

        /// <summary>
        /// Duration of the animation in seconds.
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// Current elapsed time in seconds.
        /// </summary>
        public float ElapsedTime { get; set; }

        /// <summary>
        /// Easing function to apply.
        /// </summary>
        public Easing Easing { get; set; } = Easing.Linear;

        /// <summary>
        /// Delay before the animation starts (in seconds).
        /// </summary>
        public float Delay { get; set; } = 0f;

        /// <summary>
        /// Whether the animation has completed.
        /// </summary>
        public bool IsComplete => ElapsedTime >= Duration + Delay;

        /// <summary>
        /// Whether the animation is currently running.
        /// </summary>
        public bool IsRunning => ElapsedTime >= Delay && !IsComplete;

        /// <summary>
        /// Action to call to apply the current value.
        /// </summary>
        public Action<float> ApplyValue { get; set; }

        /// <summary>
        /// Callback when animation completes.
        /// </summary>
        public Action OnComplete { get; set; }

        /// <summary>
        /// Gets the current interpolated value.
        /// </summary>
        public float CurrentValue
        {
            get
            {
                if (ElapsedTime < Delay)
                    return StartValue;

                float t = Math.Clamp((ElapsedTime - Delay) / Duration, 0f, 1f);
                float easedT = EasingFunctions.Apply(Easing, t);
                return StartValue + (EndValue - StartValue) * easedT;
            }
        }

        /// <summary>
        /// Updates the animation by the specified delta time.
        /// </summary>
        /// <param name="dt">Delta time in seconds.</param>
        public void Update(float dt)
        {
            ElapsedTime += dt;
            ApplyValue?.Invoke(CurrentValue);

            if (IsComplete)
            {
                ApplyValue?.Invoke(EndValue);
                OnComplete?.Invoke();
            }
        }
    }

    /// <summary>
    /// Represents a Vector2 animation.
    /// </summary>
    public class FishUIAnimationVector2
    {
        public string Id { get; set; }
        public object Target { get; set; }
        public string PropertyName { get; set; }
        public Vector2 StartValue { get; set; }
        public Vector2 EndValue { get; set; }
        public float Duration { get; set; }
        public float ElapsedTime { get; set; }
        public Easing Easing { get; set; } = Easing.Linear;
        public float Delay { get; set; } = 0f;
        public bool IsComplete => ElapsedTime >= Duration + Delay;
        public Action<Vector2> ApplyValue { get; set; }
        public Action OnComplete { get; set; }

        public Vector2 CurrentValue
        {
            get
            {
                if (ElapsedTime < Delay)
                    return StartValue;

                float t = Math.Clamp((ElapsedTime - Delay) / Duration, 0f, 1f);
                float easedT = EasingFunctions.Apply(Easing, t);
                return Vector2.Lerp(StartValue, EndValue, easedT);
            }
        }

        public void Update(float dt)
        {
            ElapsedTime += dt;
            ApplyValue?.Invoke(CurrentValue);

            if (IsComplete)
            {
                ApplyValue?.Invoke(EndValue);
                OnComplete?.Invoke();
            }
        }
    }

    /// <summary>
    /// Represents a color animation.
    /// </summary>
    public class FishUIAnimationColor
    {
        public string Id { get; set; }
        public object Target { get; set; }
        public FishColor StartValue { get; set; }
        public FishColor EndValue { get; set; }
        public float Duration { get; set; }
        public float ElapsedTime { get; set; }
        public Easing Easing { get; set; } = Easing.Linear;
        public float Delay { get; set; } = 0f;
        public bool IsComplete => ElapsedTime >= Duration + Delay;
        public Action<FishColor> ApplyValue { get; set; }
        public Action OnComplete { get; set; }

        public FishColor CurrentValue
        {
            get
            {
                if (ElapsedTime < Delay)
                    return StartValue;

                float t = Math.Clamp((ElapsedTime - Delay) / Duration, 0f, 1f);
                float easedT = EasingFunctions.Apply(Easing, t);
                return FishColor.Lerp(StartValue, EndValue, easedT);
            }
        }

        public void Update(float dt)
        {
            ElapsedTime += dt;
            ApplyValue?.Invoke(CurrentValue);

            if (IsComplete)
            {
                ApplyValue?.Invoke(EndValue);
                OnComplete?.Invoke();
            }
        }
    }

    /// <summary>
    /// Manages all active animations and updates them each frame.
    /// </summary>
    public class FishUIAnimationManager
    {
        private readonly List<FishUIAnimation> _floatAnimations = new();
        private readonly List<FishUIAnimationVector2> _vector2Animations = new();
        private readonly List<FishUIAnimationColor> _colorAnimations = new();

        /// <summary>
        /// Gets the count of active animations.
        /// </summary>
        public int ActiveAnimationCount => _floatAnimations.Count + _vector2Animations.Count + _colorAnimations.Count;

        /// <summary>
        /// Adds a float animation.
        /// </summary>
        public FishUIAnimation Add(FishUIAnimation animation)
        {
            // Remove any existing animation for the same target and property
            if (animation.Target != null && !string.IsNullOrEmpty(animation.PropertyName))
            {
                _floatAnimations.RemoveAll(a => a.Target == animation.Target && a.PropertyName == animation.PropertyName);
            }
            _floatAnimations.Add(animation);
            return animation;
        }

        /// <summary>
        /// Adds a Vector2 animation.
        /// </summary>
        public FishUIAnimationVector2 Add(FishUIAnimationVector2 animation)
        {
            if (animation.Target != null && !string.IsNullOrEmpty(animation.PropertyName))
            {
                _vector2Animations.RemoveAll(a => a.Target == animation.Target && a.PropertyName == animation.PropertyName);
            }
            _vector2Animations.Add(animation);
            return animation;
        }

        /// <summary>
        /// Adds a color animation.
        /// </summary>
        public FishUIAnimationColor Add(FishUIAnimationColor animation)
        {
            if (animation.Target != null)
            {
                _colorAnimations.RemoveAll(a => a.Target == animation.Target && a.Id == animation.Id);
            }
            _colorAnimations.Add(animation);
            return animation;
        }

        /// <summary>
        /// Stops all animations for a specific target.
        /// </summary>
        public void StopAnimationsFor(object target)
        {
            _floatAnimations.RemoveAll(a => a.Target == target);
            _vector2Animations.RemoveAll(a => a.Target == target);
            _colorAnimations.RemoveAll(a => a.Target == target);
        }

        /// <summary>
        /// Stops a specific animation by ID.
        /// </summary>
        public void StopAnimation(string id)
        {
            _floatAnimations.RemoveAll(a => a.Id == id);
            _vector2Animations.RemoveAll(a => a.Id == id);
            _colorAnimations.RemoveAll(a => a.Id == id);
        }

        /// <summary>
        /// Stops all animations.
        /// </summary>
        public void StopAll()
        {
            _floatAnimations.Clear();
            _vector2Animations.Clear();
            _colorAnimations.Clear();
        }

        /// <summary>
        /// Updates all animations. Call this once per frame.
        /// </summary>
        /// <param name="dt">Delta time in seconds.</param>
        public void Update(float dt)
        {
            // Update float animations
            for (int i = _floatAnimations.Count - 1; i >= 0; i--)
            {
                _floatAnimations[i].Update(dt);
                if (_floatAnimations[i].IsComplete)
                    _floatAnimations.RemoveAt(i);
            }

            // Update Vector2 animations
            for (int i = _vector2Animations.Count - 1; i >= 0; i--)
            {
                _vector2Animations[i].Update(dt);
                if (_vector2Animations[i].IsComplete)
                    _vector2Animations.RemoveAt(i);
            }

            // Update color animations
            for (int i = _colorAnimations.Count - 1; i >= 0; i--)
            {
                _colorAnimations[i].Update(dt);
                if (_colorAnimations[i].IsComplete)
                    _colorAnimations.RemoveAt(i);
            }
        }
    }
}
