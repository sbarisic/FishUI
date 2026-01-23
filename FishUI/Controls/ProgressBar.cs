using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public enum ProgressBarOrientation
	{
		Horizontal,
		Vertical
	}

	public class ProgressBar : Control
	{
		[YamlMember]
		public ProgressBarOrientation Orientation { get; set; } = ProgressBarOrientation.Horizontal;

		/// <summary>
		/// The current value of the progress bar (0.0 to 1.0)
		/// </summary>
		[YamlMember]
		public float Value
		{
			get => _value;
			set => _value = Math.Clamp(value, 0f, 1f);
		}
		private float _value = 0f;

		/// <summary>
		/// If true, the progress bar will display an indeterminate/marquee animation
		/// </summary>
		[YamlMember]
		public bool IsIndeterminate { get; set; } = false;

		/// <summary>
		/// The speed of the indeterminate animation (cycles per second)
		/// </summary>
		[YamlMember]
		public float IndeterminateSpeed { get; set; } = 1.0f;

		/// <summary>
		/// The size of the indeterminate indicator as a fraction of the total bar (0.0 to 1.0)
		/// </summary>
		[YamlMember]
		public float IndeterminateSize { get; set; } = 0.3f;

		/// <summary>
		/// Background color of the progress bar
		/// </summary>
		[YamlMember]
		public FishColor BackgroundColor { get; set; } = new FishColor(60, 60, 60, 255);

		/// <summary>
		/// Fill color of the progress bar
		/// </summary>
		[YamlMember]
		public FishColor FillColor { get; set; } = new FishColor(76, 175, 80, 255);

		/// <summary>
		/// Border color of the progress bar
		/// </summary>
		[YamlMember]
		public FishColor BorderColor { get; set; } = new FishColor(100, 100, 100, 255);

		/// <summary>
		/// Whether to draw a border around the progress bar
		/// </summary>
		[YamlMember]
		public bool ShowBorder { get; set; } = true;

		/// <summary>
		/// When true, uses colors from the current theme's color palette instead of the control's color properties.
		/// </summary>
		[YamlMember]
		public bool UseThemeColors { get; set; } = true;

		[YamlIgnore]
		private float _animationTime = 0f;

		public ProgressBar()
		{
			Size = new Vector2(200, 20);
		}

		private FishColor GetBackgroundColor(FishUI UI)
		{
			if (UseThemeColors && UI.Settings.CurrentTheme != null)
				return UI.Settings.GetColorPalette().Background;
			return BackgroundColor;
		}

		private FishColor GetFillColor(FishUI UI)
		{
			if (UseThemeColors && UI.Settings.CurrentTheme != null)
				return UI.Settings.GetColorPalette().Accent;
			return FillColor;
		}

		private FishColor GetBorderColor(FishUI UI)
		{
			if (UseThemeColors && UI.Settings.CurrentTheme != null)
				return UI.Settings.GetColorPalette().Border;
			return BorderColor;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			// Draw background using NPatch if available, otherwise use color
			if (UI.Settings.ImgProgressBarTrack != null)
			{
				UI.Graphics.DrawNPatch(UI.Settings.ImgProgressBarTrack, pos, size, FishColor.White);
			}
			else
			{
				UI.Graphics.DrawRectangle(pos, size, GetBackgroundColor(UI));
			}

			if (IsIndeterminate)
			{
				DrawIndeterminate(UI, Dt, pos, size);
			}
			else
			{
				DrawDeterminate(UI, pos, size);
			}

			// Draw border if no NPatch is used
			if (ShowBorder && UI.Settings.ImgProgressBarTrack == null)
			{
				UI.Graphics.DrawRectangleOutline(pos, size, GetBorderColor(UI));
			}
		}

		private void DrawDeterminate(FishUI UI, Vector2 pos, Vector2 size)
		{
			if (Value <= 0f)
				return;

			Vector2 fillSize;
			Vector2 fillPos = pos;

			if (Orientation == ProgressBarOrientation.Horizontal)
			{
				fillSize = new Vector2(size.X * Value, size.Y);
			}
			else
			{
				// Vertical: fill from bottom to top
				float fillHeight = size.Y * Value;
				fillPos = new Vector2(pos.X, pos.Y + size.Y - fillHeight);
				fillSize = new Vector2(size.X, fillHeight);
			}

			// Draw fill using NPatch if available, otherwise use color
			if (UI.Settings.ImgProgressBarFill != null)
			{
				UI.Graphics.DrawNPatch(UI.Settings.ImgProgressBarFill, fillPos, fillSize, FishColor.White);
			}
			else
			{
				UI.Graphics.DrawRectangle(fillPos, fillSize, GetFillColor(UI));
			}
		}

		private void DrawIndeterminate(FishUI UI, float Dt, Vector2 pos, Vector2 size)
		{
			_animationTime += Dt * IndeterminateSpeed;
			if (_animationTime > 1f)
				_animationTime -= 1f;

			// Use a sine-based easing for smoother animation
			float easedPosition = (float)(Math.Sin(_animationTime * Math.PI * 2 - Math.PI / 2) + 1) / 2;

			Vector2 fillPos;
			Vector2 fillSize;

			if (Orientation == ProgressBarOrientation.Horizontal)
			{
				float indicatorWidth = size.X * IndeterminateSize;
				float maxOffset = size.X - indicatorWidth;
				float offset = maxOffset * easedPosition;

				fillPos = new Vector2(pos.X + offset, pos.Y);
				fillSize = new Vector2(indicatorWidth, size.Y);
			}
			else
			{
				float indicatorHeight = size.Y * IndeterminateSize;
				float maxOffset = size.Y - indicatorHeight;
				float offset = maxOffset * easedPosition;

				fillPos = new Vector2(pos.X, pos.Y + offset);
				fillSize = new Vector2(size.X, indicatorHeight);
			}

			// Draw fill using NPatch if available, otherwise use color
			if (UI.Settings.ImgProgressBarFill != null)
			{
				UI.Graphics.DrawNPatch(UI.Settings.ImgProgressBarFill, fillPos, fillSize, FishColor.White);
			}
			else
			{
				UI.Graphics.DrawRectangle(fillPos, fillSize, GetFillColor(UI));
			}
		}
	}
}
