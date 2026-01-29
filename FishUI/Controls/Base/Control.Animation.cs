using System;
using System.Numerics;

namespace FishUI.Controls
{
	public abstract partial class Control
	{
		/// <summary>
		/// Gets the animation manager for this control.
		/// </summary>
		protected FishUIAnimationManager Animations => FishUI?.Animations;

		/// <summary>
		/// Animates the control's position.
		/// </summary>
		/// <param name="to">Target position.</param>
		/// <param name="duration">Duration in seconds.</param>
		/// <param name="easing">Easing function.</param>
		/// <param name="onComplete">Callback when animation completes.</param>
		public void AnimatePosition(Vector2 to, float duration, Easing easing = Easing.EaseOutQuad, Action onComplete = null)
		{
			if (Animations == null) return;
			var from = new Vector2(Position.X, Position.Y);
			FishUITween.Vector2(Animations, this, "Position", from, to, duration, easing,
				v => Position = new FishUIPosition(Position.Mode, v),
				onComplete);
		}

		/// <summary>
		/// Animates the control's size.
		/// </summary>
		/// <param name="to">Target size.</param>
		/// <param name="duration">Duration in seconds.</param>
		/// <param name="easing">Easing function.</param>
		/// <param name="onComplete">Callback when animation completes.</param>
		public void AnimateSize(Vector2 to, float duration, Easing easing = Easing.EaseOutQuad, Action onComplete = null)
		{
			if (Animations == null) return;
			var from = Size;
			FishUITween.Vector2(Animations, this, "Size", from, to, duration, easing,
				v => Size = v,
				onComplete);
		}

		/// <summary>
		/// Animates the control's opacity.
		/// </summary>
		/// <param name="to">Target opacity (0-1).</param>
		/// <param name="duration">Duration in seconds.</param>
		/// <param name="easing">Easing function.</param>
		/// <param name="onComplete">Callback when animation completes.</param>
		public void AnimateOpacity(float to, float duration, Easing easing = Easing.EaseOutQuad, Action onComplete = null)
		{
			if (Animations == null) return;
			var from = Opacity;
			FishUITween.Float(Animations, this, "Opacity", from, to, duration, easing,
				v => Opacity = v,
				onComplete);
		}

		/// <summary>
		/// Fades in the control (sets visible and animates opacity from 0 to 1).
		/// </summary>
		/// <param name="duration">Duration in seconds.</param>
		/// <param name="easing">Easing function.</param>
		/// <param name="onComplete">Callback when animation completes.</param>
		public void FadeIn(float duration = 0.3f, Easing easing = Easing.EaseOutQuad, Action onComplete = null)
		{
			Opacity = 0f;
			Visible = true;
			AnimateOpacity(1f, duration, easing, onComplete);
		}

		/// <summary>
		/// Fades out the control (animates opacity to 0, then sets invisible).
		/// </summary>
		/// <param name="duration">Duration in seconds.</param>
		/// <param name="easing">Easing function.</param>
		/// <param name="onComplete">Callback when animation completes.</param>
		public void FadeOut(float duration = 0.3f, Easing easing = Easing.EaseOutQuad, Action onComplete = null)
		{
			AnimateOpacity(0f, duration, easing, () =>
			{
				Visible = false;
				Opacity = 1f; // Reset for next show
				onComplete?.Invoke();
			});
		}

		/// <summary>
		/// Slides the control in from a direction.
		/// </summary>
		/// <param name="fromOffset">Offset to slide from (e.g., new Vector2(-100, 0) for slide from left).</param>
		/// <param name="duration">Duration in seconds.</param>
		/// <param name="easing">Easing function.</param>
		/// <param name="onComplete">Callback when animation completes.</param>
		public void SlideIn(Vector2 fromOffset, float duration = 0.3f, Easing easing = Easing.EaseOutQuad, Action onComplete = null)
		{
			var targetPos = new Vector2(Position.X, Position.Y);
			var startPos = targetPos + fromOffset;

			Position = new FishUIPosition(Position.Mode, startPos);
			Visible = true;
			AnimatePosition(targetPos, duration, easing, onComplete);
		}

		/// <summary>
		/// Slides the control out to a direction.
		/// </summary>
		/// <param name="toOffset">Offset to slide to (e.g., new Vector2(100, 0) for slide to right).</param>
		/// <param name="duration">Duration in seconds.</param>
		/// <param name="easing">Easing function.</param>
		/// <param name="onComplete">Callback when animation completes.</param>
		public void SlideOut(Vector2 toOffset, float duration = 0.3f, Easing easing = Easing.EaseInQuad, Action onComplete = null)
		{
			var startPos = new Vector2(Position.X, Position.Y);
			var targetPos = startPos + toOffset;

			AnimatePosition(targetPos, duration, easing, () =>
			{
				Visible = false;
				Position = new FishUIPosition(Position.Mode, startPos); // Reset position
				onComplete?.Invoke();
			});
		}

		/// <summary>
		/// Scales the control with a bounce effect.
		/// </summary>
		/// <param name="scale">Scale factor (1.0 = normal, 1.2 = 20% larger).</param>
		/// <param name="duration">Duration in seconds.</param>
		/// <param name="onComplete">Callback when animation completes.</param>
		public void ScaleBounce(float scale = 1.1f, float duration = 0.2f, Action onComplete = null)
		{
			var originalSize = Size;
			var scaledSize = originalSize * scale;

			// Scale up
			AnimateSize(scaledSize, duration / 2, Easing.EaseOutQuad, () =>
			{
				// Then scale back
				AnimateSize(originalSize, duration / 2, Easing.EaseInQuad, onComplete);
			});
		}
	}
}
