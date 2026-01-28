using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Horizontal alignment for the digit display text.
	/// </summary>
	public enum DigitAlignment
	{
		Left,
		Center,
		Right
	}

	/// <summary>
	/// A large text display control designed for digital readouts like speedometers, 
	/// RPM gauges, counters, or any application requiring prominent numeric/text display.
	/// </summary>
	public class BigDigitDisplay : Control
	{
		/// <summary>
		/// The text/value to display. Can be numeric or any string.
		/// </summary>
		[YamlMember]
		public string Text { get; set; } = "0";

		/// <summary>
		/// Numeric value for convenience. Setting this updates Text with the formatted value.
		/// </summary>
		[YamlIgnore]
		public float Value
		{
			get => _value;
			set
			{
				_value = value;
				Text = value.ToString(ValueFormat);
			}
		}
		private float _value = 0f;

		/// <summary>
		/// Format string for numeric values (e.g., "F0" for integers, "F1" for one decimal).
		/// </summary>
		[YamlMember]
		public string ValueFormat { get; set; } = "F0";

		/// <summary>
		/// Scale multiplier for the font size relative to control height.
		/// Default is 0.7 (70% of control height).
		/// </summary>
		[YamlMember]
		public float FontScale { get; set; } = 0.7f;

		/// <summary>
		/// Text color for the digits.
		/// </summary>
		[YamlMember]
		public FishColor TextColor { get; set; } = new FishColor(0, 255, 0, 255); // Green by default (classic digital look)

		/// <summary>
		/// Background color of the display.
		/// </summary>
		[YamlMember]
		public FishColor BackgroundColor { get; set; } = new FishColor(20, 20, 20, 255); // Dark background

		/// <summary>
		/// Border color around the display.
		/// </summary>
		[YamlMember]
		public FishColor BorderColor { get; set; } = new FishColor(60, 60, 60, 255);

		/// <summary>
		/// Border thickness in pixels. Set to 0 to disable border.
		/// </summary>
		[YamlMember]
		public int BorderThickness { get; set; } = 2;

		/// <summary>
		/// Horizontal alignment of the text within the display.
		/// </summary>
		[YamlMember]
		public DigitAlignment Alignment { get; set; } = DigitAlignment.Right;

		/// <summary>
		/// Whether to show a background behind the digits.
		/// </summary>
		[YamlMember]
		public bool ShowBackground { get; set; } = true;

		/// <summary>
		/// Padding inside the display border.
		/// </summary>
		[YamlMember]
		public new int Padding { get; set; } = 8;

		/// <summary>
		/// Optional unit label to display after the value (e.g., "km/h", "RPM", "°C").
		/// </summary>
		[YamlMember]
		public string UnitLabel { get; set; } = "";

		/// <summary>
		/// Color for the unit label. If null, uses TextColor with reduced opacity.
		/// </summary>
		[YamlMember]
		public FishColor? UnitLabelColor { get; set; } = null;

		/// <summary>
		/// Scale of the unit label relative to the main digit size (default 0.4 = 40%).
		/// </summary>
		[YamlMember]
		public float UnitLabelScale { get; set; } = 0.4f;

		public BigDigitDisplay()
		{
			Size = new Vector2(150, 60);
		}

		public BigDigitDisplay(string text) : this()
		{
			Text = text;
		}

		public BigDigitDisplay(float value, string format = "F0") : this()
		{
			ValueFormat = format;
			Value = value;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();

			// Draw background
			if (ShowBackground)
			{
				UI.Graphics.DrawRectangle(absPos, absSize, BackgroundColor);
			}

			// Draw border
			if (BorderThickness > 0)
			{
				// Top
				UI.Graphics.DrawRectangle(absPos, new Vector2(absSize.X, BorderThickness), BorderColor);
				// Bottom
				UI.Graphics.DrawRectangle(absPos + new Vector2(0, absSize.Y - BorderThickness), new Vector2(absSize.X, BorderThickness), BorderColor);
				// Left
				UI.Graphics.DrawRectangle(absPos, new Vector2(BorderThickness, absSize.Y), BorderColor);
				// Right
				UI.Graphics.DrawRectangle(absPos + new Vector2(absSize.X - BorderThickness, 0), new Vector2(BorderThickness, absSize.Y), BorderColor);
			}

			// Calculate available area for text
			float innerX = absPos.X + Padding + BorderThickness;
			float innerY = absPos.Y + Padding + BorderThickness;
			float innerWidth = absSize.X - (Padding + BorderThickness) * 2;
			float innerHeight = absSize.Y - (Padding + BorderThickness) * 2;

			if (innerWidth <= 0 || innerHeight <= 0)
				return;

			// Calculate font size based on control height
			float fontSize = innerHeight * FontScale;
			if (fontSize < 8) fontSize = 8;

			// Get text to display
			string displayText = Text ?? "";

			// Measure main text (approximate using font metrics)
			float charWidth = fontSize * 0.6f; // Approximate character width
			float textWidth = displayText.Length * charWidth;

			// Calculate unit label dimensions
			float unitWidth = 0;
			float unitFontSize = fontSize * UnitLabelScale;
			if (!string.IsNullOrEmpty(UnitLabel))
			{
				unitWidth = UnitLabel.Length * unitFontSize * 0.6f + 4; // +4 for spacing
			}

			// Calculate text position based on alignment
			float textX;
			switch (Alignment)
			{
				case DigitAlignment.Left:
					textX = innerX;
					break;
				case DigitAlignment.Center:
					textX = innerX + (innerWidth - textWidth - unitWidth) / 2;
					break;
				case DigitAlignment.Right:
				default:
					textX = innerX + innerWidth - textWidth - unitWidth;
					break;
			}

			// Center vertically
			float textY = innerY + (innerHeight - fontSize) / 2;

			// Draw main text using scaled font
			// Note: FishUI doesn't support dynamic font sizing, so we use DrawTextEx if available
			// For now, we'll draw at the default font size but position appropriately
			Vector2 textPos = new Vector2(textX, textY);

			// Use the default font but try to scale
			FontRef font = UI.Settings.FontDefault;
			float scale = fontSize / font.Size;

			// Draw the main digits
			UI.Graphics.DrawTextColorScale(font, displayText, textPos, TextColor, scale);

			// Draw unit label if present
			if (!string.IsNullOrEmpty(UnitLabel))
			{
				float unitScale = unitFontSize / font.Size;
				float unitX = textX + textWidth + 4;
				float unitY = textY + fontSize - unitFontSize; // Align to baseline

				FishColor unitColor = UnitLabelColor ?? new FishColor(TextColor.R, TextColor.G, TextColor.B, (byte)(TextColor.A * 0.7f));
				UI.Graphics.DrawTextColorScale(font, UnitLabel, new Vector2(unitX, unitY), unitColor, unitScale);
			}
		}
	}
}
