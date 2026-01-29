using System;
using System.Numerics;
using FishUI.Controls;

namespace FishUI
{
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
}
