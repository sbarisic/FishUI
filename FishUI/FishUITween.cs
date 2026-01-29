using System;
using System.Numerics;
using FishUI.Controls;

namespace FishUI
{
    // TODO: Does this make sense to be a fluent API, shouldn't these extension methods be implemented in the base Control class?

    public static class FishUITween
    {
        /// <summary>
        /// Creates a float animation.
        /// </summary>
        /// <param name="manager">The animation manager.</param>
        /// <param name="target">Target object (for tracking).</param>
        /// <param name="propertyName">Property name (for tracking).</param>
        /// <param name="from">Start value.</param>
        /// <param name="to">End value.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="easing">Easing function.</param>
        /// <param name="apply">Action to apply the current value.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        /// <returns>The created animation.</returns>
        public static FishUIAnimation Float(
            FishUIAnimationManager manager,
            object target,
            string propertyName,
            float from,
            float to,
            float duration,
            Easing easing,
            Action<float> apply,
            Action onComplete = null)
        {
            var animation = new FishUIAnimation
            {
                Target = target,
                PropertyName = propertyName,
                StartValue = from,
                EndValue = to,
                Duration = duration,
                Easing = easing,
                ApplyValue = apply,
                OnComplete = onComplete
            };
            manager.Add(animation);
            return animation;
        }

        /// <summary>
        /// Creates a Vector2 animation.
        /// </summary>
        public static FishUIAnimationVector2 Vector2(
            FishUIAnimationManager manager,
            object target,
            string propertyName,
            Vector2 from,
            Vector2 to,
            float duration,
            Easing easing,
            Action<Vector2> apply,
            Action onComplete = null)
        {
            var animation = new FishUIAnimationVector2
            {
                Target = target,
                PropertyName = propertyName,
                StartValue = from,
                EndValue = to,
                Duration = duration,
                Easing = easing,
                ApplyValue = apply,
                OnComplete = onComplete
            };
            manager.Add(animation);
            return animation;
        }

        /// <summary>
        /// Creates a color animation.
        /// </summary>
        public static FishUIAnimationColor Color(
            FishUIAnimationManager manager,
            object target,
            string id,
            FishColor from,
            FishColor to,
            float duration,
            Easing easing,
            Action<FishColor> apply,
            Action onComplete = null)
        {
            var animation = new FishUIAnimationColor
            {
                Target = target,
                Id = id,
                StartValue = from,
                EndValue = to,
                Duration = duration,
                Easing = easing,
                ApplyValue = apply,
                OnComplete = onComplete
            };
            manager.Add(animation);
            return animation;
        }
    }

    /// <summary>
    /// Extension methods for animating Control properties.
    /// </summary>
    public static class FishUIControlAnimationExtensions
    {
        /// <summary>
        /// Animates the control's position.
        /// </summary>
        /// <param name="control">The control to animate.</param>
        /// <param name="manager">The animation manager.</param>
        /// <param name="to">Target position.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="easing">Easing function.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        public static void AnimatePosition(this Control control, FishUIAnimationManager manager, Vector2 to, float duration, Easing easing = Easing.EaseOutQuad, Action onComplete = null)
        {
            var from = new Vector2(control.Position.X, control.Position.Y);
            FishUITween.Vector2(manager, control, "Position", from, to, duration, easing,
                v => control.Position = new FishUIPosition(control.Position.Mode, v),
                onComplete);
        }

        /// <summary>
        /// Animates the control's size.
        /// </summary>
        /// <param name="control">The control to animate.</param>
        /// <param name="manager">The animation manager.</param>
        /// <param name="to">Target size.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="easing">Easing function.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        public static void AnimateSize(
            this Control control,
            FishUIAnimationManager manager,
            Vector2 to,
            float duration,
            Easing easing = Easing.EaseOutQuad,
            Action onComplete = null)
        {
            var from = control.Size;
            FishUITween.Vector2(manager, control, "Size", from, to, duration, easing,
                v => control.Size = v,
                onComplete);
        }

        /// <summary>
        /// Animates the control's opacity.
        /// </summary>
        /// <param name="control">The control to animate.</param>
        /// <param name="manager">The animation manager.</param>
        /// <param name="to">Target opacity (0-1).</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="easing">Easing function.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        public static void AnimateOpacity(
            this Control control,
            FishUIAnimationManager manager,
            float to,
            float duration,
            Easing easing = Easing.EaseOutQuad,
            Action onComplete = null)
        {
            var from = control.Opacity;
            FishUITween.Float(manager, control, "Opacity", from, to, duration, easing,
                v => control.Opacity = v,
                onComplete);
        }

        /// <summary>
        /// Fades in the control (sets visible and animates opacity from 0 to 1).
        /// </summary>
        /// <param name="control">The control to fade in.</param>
        /// <param name="manager">The animation manager.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="easing">Easing function.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        public static void FadeIn(
            this Control control,
            FishUIAnimationManager manager,
            float duration = 0.3f,
            Easing easing = Easing.EaseOutQuad,
            Action onComplete = null)
        {
            control.Opacity = 0f;
            control.Visible = true;
            control.AnimateOpacity(manager, 1f, duration, easing, onComplete);
        }

        /// <summary>
        /// Fades out the control (animates opacity to 0, then sets invisible).
        /// </summary>
        /// <param name="control">The control to fade out.</param>
        /// <param name="manager">The animation manager.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="easing">Easing function.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        public static void FadeOut(
            this Control control,
            FishUIAnimationManager manager,
            float duration = 0.3f,
            Easing easing = Easing.EaseOutQuad,
            Action onComplete = null)
        {
            control.AnimateOpacity(manager, 0f, duration, easing, () =>
            {
                control.Visible = false;
                control.Opacity = 1f; // Reset for next show
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// Slides the control in from a direction.
        /// </summary>
        /// <param name="control">The control to animate.</param>
        /// <param name="manager">The animation manager.</param>
        /// <param name="fromOffset">Offset to slide from (e.g., new Vector2(-100, 0) for slide from left).</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="easing">Easing function.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        public static void SlideIn(
            this Control control,
            FishUIAnimationManager manager,
            Vector2 fromOffset,
            float duration = 0.3f,
            Easing easing = Easing.EaseOutQuad,
            Action onComplete = null)
        {
            var targetPos = new Vector2(control.Position.X, control.Position.Y);
            var startPos = targetPos + fromOffset;

            control.Position = new FishUIPosition(control.Position.Mode, startPos);
            control.Visible = true;
            control.AnimatePosition(manager, targetPos, duration, easing, onComplete);
        }

        /// <summary>
        /// Slides the control out to a direction.
        /// </summary>
        /// <param name="control">The control to animate.</param>
        /// <param name="manager">The animation manager.</param>
        /// <param name="toOffset">Offset to slide to (e.g., new Vector2(100, 0) for slide to right).</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="easing">Easing function.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        public static void SlideOut(
            this Control control,
            FishUIAnimationManager manager,
            Vector2 toOffset,
            float duration = 0.3f,
            Easing easing = Easing.EaseInQuad,
            Action onComplete = null)
        {
            var startPos = new Vector2(control.Position.X, control.Position.Y);
            var targetPos = startPos + toOffset;

            control.AnimatePosition(manager, targetPos, duration, easing, () =>
            {
                control.Visible = false;
                control.Position = new FishUIPosition(control.Position.Mode, startPos); // Reset position
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// Scales the control with a bounce effect.
        /// </summary>
        /// <param name="control">The control to animate.</param>
        /// <param name="manager">The animation manager.</param>
        /// <param name="scale">Scale factor (1.0 = normal, 1.2 = 20% larger).</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        public static void ScaleBounce(
            this Control control,
            FishUIAnimationManager manager,
            float scale = 1.1f,
            float duration = 0.2f,
            Action onComplete = null)
        {
            var originalSize = control.Size;
            var scaledSize = originalSize * scale;

            // Scale up
            control.AnimateSize(manager, scaledSize, duration / 2, Easing.EaseOutQuad, () =>
            {
                // Then scale back
                control.AnimateSize(manager, originalSize, duration / 2, Easing.EaseInQuad, onComplete);
            });
        }
    }
}
