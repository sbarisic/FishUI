using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Type of toast notification, affecting its appearance.
	/// </summary>
	public enum ToastType
	{
		Info,
		Success,
		Warning,
		Error
	}

	/// <summary>
	/// Represents a single toast notification message.
	/// </summary>
	public class ToastMessage
	{
		public string Title { get; set; }
		public string Message { get; set; }
		public ToastType Type { get; set; }
		public float Duration { get; set; }
		public float ElapsedTime { get; set; }
		public float Alpha { get; set; } = 1f;
		public bool IsExpired => ElapsedTime >= Duration;

		public ToastMessage(string message, ToastType type = ToastType.Info, float duration = 3f)
		{
			Title = "";
			Message = message;
			Type = type;
			Duration = duration;
			ElapsedTime = 0f;
		}

		public ToastMessage(string title, string message, ToastType type = ToastType.Info, float duration = 3f)
		{
			Title = title;
			Message = message;
			Type = type;
			Duration = duration;
			ElapsedTime = 0f;
		}
	}

	/// <summary>
	/// A toast notification system that displays temporary messages.
	/// Toasts stack in the top-right corner and auto-dismiss after a timeout.
	/// Add this control once to your UI to enable toast notifications.
	/// </summary>
	public class ToastNotification : Control
	{
		/// <summary>
		/// List of active toast messages.
		/// </summary>
		[YamlIgnore]
		private List<ToastMessage> _toasts = new List<ToastMessage>();

		/// <summary>
		/// Maximum number of toasts to display at once.
		/// </summary>
		[YamlMember]
		public int MaxToasts { get; set; } = 5;

		/// <summary>
		/// Width of each toast notification.
		/// </summary>
		[YamlMember]
		public float ToastWidth { get; set; } = 300f;

		/// <summary>
		/// Height of each toast notification.
		/// </summary>
		[YamlMember]
		public float ToastHeight { get; set; } = 60f;

		/// <summary>
		/// Spacing between toasts.
		/// </summary>
		[YamlMember]
		public float ToastSpacing { get; set; } = 8f;

		/// <summary>
		/// Margin from the screen edge.
		/// </summary>
		[YamlMember]
		public float ScreenMargin { get; set; } = 20f;

		/// <summary>
		/// Duration of fade-out animation in seconds.
		/// </summary>
		[YamlMember]
		public float FadeOutDuration { get; set; } = 0.5f;

		/// <summary>
		/// Default duration for toasts in seconds.
		/// </summary>
		[YamlMember]
		public float DefaultDuration { get; set; } = 3f;

		/// <summary>
		/// Padding inside toast.
		/// </summary>
		[YamlMember]
		public new float Padding { get; set; } = 10f;

		/// <summary>
		/// Color for Info type toasts.
		/// </summary>
		[YamlMember]
		public FishColor InfoColor { get; set; } = new FishColor(60, 120, 200, 255);

		/// <summary>
		/// Color for Success type toasts.
		/// </summary>
		[YamlMember]
		public FishColor SuccessColor { get; set; } = new FishColor(60, 180, 80, 255);

		/// <summary>
		/// Color for Warning type toasts.
		/// </summary>
		[YamlMember]
		public FishColor WarningColor { get; set; } = new FishColor(220, 160, 40, 255);

		/// <summary>
		/// Color for Error type toasts.
		/// </summary>
		[YamlMember]
		public FishColor ErrorColor { get; set; } = new FishColor(200, 60, 60, 255);

		/// <summary>
		/// Background color of the toast.
		/// </summary>
		[YamlMember]
		public FishColor BackgroundColor { get; set; } = new FishColor(40, 40, 40, 240);

		/// <summary>
		/// Text color for toast messages.
		/// </summary>
		[YamlMember]
		public FishColor TextColor { get; set; } = new FishColor(255, 255, 255, 255);

		public ToastNotification()
		{
			// This control doesn't have a fixed position/size - it renders in the corner
			AlwaysOnTop = true;
			Visible = true;
		}

		/// <summary>
		/// Shows a toast notification with a message.
		/// </summary>
		public void Show(string message, ToastType type = ToastType.Info)
		{
			Show(message, type, DefaultDuration);
		}

		/// <summary>
		/// Shows a toast notification with a message and custom duration.
		/// </summary>
		public void Show(string message, ToastType type, float duration)
		{
			var toast = new ToastMessage(message, type, duration);
			AddToast(toast);
		}

		/// <summary>
		/// Shows a toast notification with a title and message.
		/// </summary>
		public void Show(string title, string message, ToastType type = ToastType.Info)
		{
			Show(title, message, type, DefaultDuration);
		}

		/// <summary>
		/// Shows a toast notification with a title, message, and custom duration.
		/// </summary>
		public void Show(string title, string message, ToastType type, float duration)
		{
			var toast = new ToastMessage(title, message, type, duration);
			AddToast(toast);
		}

		/// <summary>
		/// Convenience method to show an info toast.
		/// </summary>
		public void ShowInfo(string message) => Show(message, ToastType.Info);

		/// <summary>
		/// Convenience method to show a success toast.
		/// </summary>
		public void ShowSuccess(string message) => Show(message, ToastType.Success);

		/// <summary>
		/// Convenience method to show a warning toast.
		/// </summary>
		public void ShowWarning(string message) => Show(message, ToastType.Warning);

		/// <summary>
		/// Convenience method to show an error toast.
		/// </summary>
		public void ShowError(string message) => Show(message, ToastType.Error);

		/// <summary>
		/// Clears all active toasts.
		/// </summary>
		public void ClearAll()
		{
			_toasts.Clear();
		}

		/// <summary>
		/// Gets the number of active toasts.
		/// </summary>
		[YamlIgnore]
		public int ActiveCount => _toasts.Count;

		private void AddToast(ToastMessage toast)
		{
			_toasts.Insert(0, toast);

			// Remove oldest if we exceed max
			while (_toasts.Count > MaxToasts)
			{
				_toasts.RemoveAt(_toasts.Count - 1);
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			if (_toasts.Count == 0)
				return;

			float screenWidth = UI.Graphics.GetWindowWidth();
			float startX = screenWidth - ToastWidth - ScreenMargin;
			float startY = ScreenMargin;

			// Update and remove expired toasts
			for (int i = _toasts.Count - 1; i >= 0; i--)
			{
				_toasts[i].ElapsedTime += Dt;

				// Calculate fade-out alpha
				float timeRemaining = _toasts[i].Duration - _toasts[i].ElapsedTime;
				if (timeRemaining < FadeOutDuration)
				{
					_toasts[i].Alpha = Math.Max(0, timeRemaining / FadeOutDuration);
				}

				if (_toasts[i].IsExpired)
				{
					_toasts.RemoveAt(i);
				}
			}

			// Draw toasts from top to bottom
			float currentY = startY;
			for (int i = 0; i < _toasts.Count; i++)
			{
				var toast = _toasts[i];
				DrawToast(UI, toast, startX, currentY);
				currentY += ToastHeight + ToastSpacing;
			}
		}

		private void DrawToast(FishUI UI, ToastMessage toast, float x, float y)
		{
			Vector2 pos = new Vector2(x, y);
			Vector2 size = new Vector2(ToastWidth, ToastHeight);

			byte alpha = (byte)(toast.Alpha * 255);

			// Get type color
			FishColor typeColor = toast.Type switch
			{
				ToastType.Success => SuccessColor,
				ToastType.Warning => WarningColor,
				ToastType.Error => ErrorColor,
				_ => InfoColor
			};
			typeColor = new FishColor(typeColor.R, typeColor.G, typeColor.B, alpha);

			// Draw background using tooltip texture if available, otherwise fallback
			FishColor bgColor = new FishColor(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, alpha);
			NPatch bgImg = UI.Settings.ImgTooltipNormal;
			if (bgImg != null)
			{
				UI.Graphics.DrawNPatch(bgImg, pos, size, bgColor);
			}
			else
			{
				UI.Graphics.DrawRectangle(pos, size, bgColor);
			}

			// Draw type indicator bar on the left
			UI.Graphics.DrawRectangle(pos, new Vector2(4, size.Y), typeColor);

			// Draw title if present
			FishColor textColor = new FishColor(TextColor.R, TextColor.G, TextColor.B, alpha);
			float textX = pos.X + Padding + 6; // +6 for indicator bar
			float textY = pos.Y + Padding;

			if (!string.IsNullOrEmpty(toast.Title))
			{
				// Draw title in type color
				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, toast.Title, new Vector2(textX, textY), typeColor);
				textY += UI.Settings.FontDefault.Size + 4;
			}

			// Draw message
			UI.Graphics.DrawTextColor(UI.Settings.FontDefault, toast.Message, new Vector2(textX, textY), textColor);
		}
	}
}
