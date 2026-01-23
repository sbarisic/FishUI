using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Vertical text alignment options.
	/// </summary>
	public enum VerticalAlign
	{
		Top,
		Middle,
		Bottom
	}

	/// <summary>
	/// A non-editable text display control with alignment and color options.
	/// Unlike Label, StaticText supports both horizontal and vertical alignment,
	/// custom text colors, and optional background rendering.
	/// </summary>
	public class StaticText : Control
	{
		/// <summary>
		/// The text to display.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Horizontal text alignment within the control bounds.
		/// </summary>
		public Align HorizontalAlignment { get; set; } = Align.Left;

		/// <summary>
		/// Vertical text alignment within the control bounds.
		/// </summary>
		public VerticalAlign VerticalAlignment { get; set; } = VerticalAlign.Middle;

		/// <summary>
		/// Custom text color. If null, uses the default font color.
		/// </summary>
		public FishColor? TextColor { get; set; } = null;

		/// <summary>
		/// If true, draws a background behind the text using the control's Color property.
		/// </summary>
		public bool ShowBackground { get; set; } = false;

		/// <summary>
		/// Background color when ShowBackground is true.
		/// </summary>
		public FishColor BackgroundColor { get; set; } = new FishColor(40, 40, 40, 200);

		public StaticText()
		{
			Size = new Vector2(200, 24);
		}

		public StaticText(string text) : this()
		{
			Text = text;
		}

		public StaticText(string text, Align horizontalAlign, VerticalAlign verticalAlign) : this(text)
		{
			HorizontalAlignment = horizontalAlign;
			VerticalAlignment = verticalAlign;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			// Draw background if enabled
			if (ShowBackground)
			{
				UI.Graphics.DrawRectangle(pos, size, BackgroundColor);
			}

			// Draw text
			if (!string.IsNullOrEmpty(Text))
			{
				Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontLabel, Text);
				
				// Handle NaN values
				if (float.IsNaN(textSize.X)) textSize.X = 0;
				if (float.IsNaN(textSize.Y)) textSize.Y = 0;

				// Calculate horizontal position
				float x = pos.X;
				switch (HorizontalAlignment)
				{
					case Align.None:
					case Align.Left:
						x = pos.X;
						break;
					case Align.Center:
						x = pos.X + (size.X - textSize.X) / 2;
						break;
					case Align.Right:
						x = pos.X + size.X - textSize.X;
						break;
				}

				// Calculate vertical position
				float y = pos.Y;
				switch (VerticalAlignment)
				{
					case VerticalAlign.Top:
						y = pos.Y;
						break;
					case VerticalAlign.Middle:
						y = pos.Y + (size.Y - textSize.Y) / 2;
						break;
					case VerticalAlign.Bottom:
						y = pos.Y + size.Y - textSize.Y;
						break;
				}

				Vector2 textPos = new Vector2(x, y);

				// Draw with custom color or default
				if (TextColor.HasValue)
				{
					UI.Graphics.DrawTextColor(UI.Settings.FontLabel, Text, textPos, TextColor.Value);
				}
				else
				{
					UI.Graphics.DrawText(UI.Settings.FontLabel, Text, textPos);
				}
			}
		}
	}
}
