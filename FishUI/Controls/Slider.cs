using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public enum SliderOrientation
	{
		Horizontal,
		Vertical
	}

	public delegate void SliderValueChangedFunc(Slider Sender, float Value);

	public class Slider : Control
	{
		[YamlMember]
		public SliderOrientation Orientation { get; set; } = SliderOrientation.Horizontal;

		/// <summary>
		/// The current value of the slider
		/// </summary>
		[YamlMember]
		public float Value
		{
			get => _value;
			set
			{
				float newValue = Math.Clamp(value, MinValue, MaxValue);
				if (_value != newValue)
				{
					_value = newValue;
					OnValueChanged?.Invoke(this, _value);
				}
			}
		}
		private float _value = 0f;

		/// <summary>
		/// Minimum value of the slider
		/// </summary>
		[YamlMember]
		public float MinValue { get; set; } = 0f;

		/// <summary>
		/// Maximum value of the slider
		/// </summary>
		[YamlMember]
		public float MaxValue { get; set; } = 100f;

		/// <summary>
		/// Step increment for value changes (0 = continuous)
		/// </summary>
		[YamlMember]
		public float Step { get; set; } = 0f;

		/// <summary>
		/// Background/track color of the slider
		/// </summary>
		[YamlMember]
		public FishColor TrackColor { get; set; } = new FishColor(60, 60, 60, 255);

		/// <summary>
		/// Fill color of the slider track (portion before thumb)
		/// </summary>
		[YamlMember]
		public FishColor FillColor { get; set; } = new FishColor(76, 175, 80, 255);

		/// <summary>
		/// Color of the thumb/knob
		/// </summary>
		[YamlMember]
		public FishColor ThumbColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Border color of the slider
		/// </summary>
		[YamlMember]
		public FishColor BorderColor { get; set; } = new FishColor(100, 100, 100, 255);

		/// <summary>
		/// Whether to draw a border around the track
		/// </summary>
		[YamlMember]
		public bool ShowBorder { get; set; } = true;

		/// <summary>
		/// Whether to show the value label near the slider
		/// </summary>
		[YamlMember]
		public bool ShowValueLabel { get; set; } = false;

		/// <summary>
		/// Format string for the value label (e.g., "0.0", "0", "0.00")
		/// </summary>
		[YamlMember]
		public string ValueLabelFormat { get; set; } = "0";

		/// <summary>
		/// Color of the value label text
		/// </summary>
		[YamlMember]
		public FishColor LabelColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Size of the thumb relative to track height/width
		/// </summary>
		[YamlMember]
		public float ThumbSize { get; set; } = 1.1f;

		/// <summary>
		/// Event fired when the value changes
		/// </summary>
		public event SliderValueChangedFunc OnValueChanged;

		[YamlIgnore]
		private bool _isDragging = false;

		public Slider()
		{
			Size = new Vector2(200, 20);
		}

		/// <summary>
		/// Gets the normalized value (0.0 to 1.0) representing position on the track
		/// </summary>
		private float GetNormalizedValue()
		{
			if (MaxValue == MinValue)
				return 0f;
			return (Value - MinValue) / (MaxValue - MinValue);
		}

		/// <summary>
		/// Sets the value from a normalized position (0.0 to 1.0)
		/// </summary>
		private void SetValueFromNormalized(float normalized)
		{
			normalized = Math.Clamp(normalized, 0f, 1f);
			float newValue = MinValue + normalized * (MaxValue - MinValue);

			// Apply step if configured
			if (Step > 0)
			{
				newValue = (float)Math.Round(newValue / Step) * Step;
			}

			Value = newValue;
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left && !Disabled)
			{
				_isDragging = true;
				UpdateValueFromMousePosition(Pos);
			}
		}

		public override void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseRelease(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				_isDragging = false;
			}
		}

		public override void HandleDrag(FishUI UI, Vector2 StartPos, Vector2 EndPos, FishInputState InState)
		{
			if (_isDragging && !Disabled)
			{
				UpdateValueFromMousePosition(EndPos);
			}
		}

		private void UpdateValueFromMousePosition(Vector2 mousePos)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			float trackThickness = Orientation == SliderOrientation.Horizontal ? size.Y : size.X;
			float thumbDiameter = trackThickness * ThumbSize;
			float thumbRadius = thumbDiameter / 2f;

			float normalized;

			if (Orientation == SliderOrientation.Horizontal)
			{
				float trackStart = pos.X + thumbRadius;
				float trackEnd = pos.X + size.X - thumbRadius;
				float trackLength = trackEnd - trackStart;

				if (trackLength <= 0)
					normalized = 0f;
				else
					normalized = (mousePos.X - trackStart) / trackLength;
			}
			else
			{
				// Vertical: bottom is min, top is max (inverted)
				float trackStart = pos.Y + thumbRadius;
				float trackEnd = pos.Y + size.Y - thumbRadius;
				float trackLength = trackEnd - trackStart;

				if (trackLength <= 0)
					normalized = 0f;
				else
					normalized = 1f - (mousePos.Y - trackStart) / trackLength;
			}

			SetValueFromNormalized(normalized);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			float normalized = GetNormalizedValue();

			if (Orientation == SliderOrientation.Horizontal)
			{
				DrawHorizontal(UI, pos, size, normalized);
			}
			else
			{
				DrawVertical(UI, pos, size, normalized);
			}

			// Draw value label if enabled
			if (ShowValueLabel)
			{
				DrawValueLabel(UI, pos, size);
			}
		}

		private void DrawHorizontal(FishUI UI, Vector2 pos, Vector2 size, float normalized)
		{
			float trackHeight = size.Y * 0.4f;
			float trackY = pos.Y + (size.Y - trackHeight) / 2f;
			Vector2 trackPos = new Vector2(pos.X, trackY);
			Vector2 trackSize = new Vector2(size.X, trackHeight);

			// Draw track background
			UI.Graphics.DrawRectangle(trackPos, trackSize, TrackColor);

			// Draw fill (portion before thumb)
			float fillWidth = size.X * normalized;
			if (fillWidth > 0)
			{
				UI.Graphics.DrawRectangle(trackPos, new Vector2(fillWidth, trackHeight), FillColor);
			}

			// Draw track border
			if (ShowBorder)
			{
				UI.Graphics.DrawRectangleOutline(trackPos, trackSize, BorderColor);
			}

			// Draw thumb
			float thumbDiameter = size.Y * ThumbSize;
			float thumbRadius = thumbDiameter / 2f;
			float thumbX = pos.X + thumbRadius + (size.X - thumbDiameter) * normalized - thumbRadius;
			float thumbY = pos.Y + (size.Y - thumbDiameter) / 2f;

			Vector2 thumbPos = new Vector2(thumbX, thumbY);
			Vector2 thumbDimensions = new Vector2(thumbDiameter, thumbDiameter);

			// Apply hover effect to thumb
			FishColor currentThumbColor = ThumbColor;
			if (IsMouseInside && !Disabled)
			{
				currentThumbColor = new FishColor(
					(byte)Math.Min(255, currentThumbColor.R + 20),
					(byte)Math.Min(255, currentThumbColor.G + 20),
					(byte)Math.Min(255, currentThumbColor.B + 20),
					currentThumbColor.A
				);
			}

			// Apply disabled effect
			if (Disabled)
			{
				currentThumbColor = new FishColor(
					(byte)(currentThumbColor.R / 2),
					(byte)(currentThumbColor.G / 2),
					(byte)(currentThumbColor.B / 2),
					currentThumbColor.A
				);
			}

			UI.Graphics.DrawRectangle(thumbPos, thumbDimensions, currentThumbColor);
			UI.Graphics.DrawRectangleOutline(thumbPos, thumbDimensions, BorderColor);
		}

		private void DrawVertical(FishUI UI, Vector2 pos, Vector2 size, float normalized)
		{
			float trackWidth = size.X * 0.4f;
			float trackX = pos.X + (size.X - trackWidth) / 2f;
			Vector2 trackPos = new Vector2(trackX, pos.Y);
			Vector2 trackSize = new Vector2(trackWidth, size.Y);

			// Draw track background
			UI.Graphics.DrawRectangle(trackPos, trackSize, TrackColor);

			// Draw fill (from bottom up for vertical)
			float fillHeight = size.Y * normalized;
			if (fillHeight > 0)
			{
				Vector2 fillPos = new Vector2(trackX, pos.Y + size.Y - fillHeight);
				UI.Graphics.DrawRectangle(fillPos, new Vector2(trackWidth, fillHeight), FillColor);
			}

			// Draw track border
			if (ShowBorder)
			{
				UI.Graphics.DrawRectangleOutline(trackPos, trackSize, BorderColor);
			}

			// Draw thumb
			float thumbDiameter = size.X * ThumbSize;
			float thumbRadius = thumbDiameter / 2f;
			float thumbX = pos.X + (size.X - thumbDiameter) / 2f;
			// Inverted: high value = top, low value = bottom
			float thumbY = pos.Y + thumbRadius + (size.Y - thumbDiameter) * (1f - normalized) - thumbRadius;

			Vector2 thumbPos = new Vector2(thumbX, thumbY);
			Vector2 thumbDimensions = new Vector2(thumbDiameter, thumbDiameter);

			// Apply hover effect to thumb
			FishColor currentThumbColor = ThumbColor;
			if (IsMouseInside && !Disabled)
			{
				currentThumbColor = new FishColor(
					(byte)Math.Min(255, currentThumbColor.R + 20),
					(byte)Math.Min(255, currentThumbColor.G + 20),
					(byte)Math.Min(255, currentThumbColor.B + 20),
					currentThumbColor.A
				);
			}

			// Apply disabled effect
			if (Disabled)
			{
				currentThumbColor = new FishColor(
					(byte)(currentThumbColor.R / 2),
					(byte)(currentThumbColor.G / 2),
					(byte)(currentThumbColor.B / 2),
					currentThumbColor.A
				);
			}

			UI.Graphics.DrawRectangle(thumbPos, thumbDimensions, currentThumbColor);
			UI.Graphics.DrawRectangleOutline(thumbPos, thumbDimensions, BorderColor);
		}

		private void DrawValueLabel(FishUI UI, Vector2 pos, Vector2 size)
		{
			string label = Value.ToString(ValueLabelFormat);
			Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, label);

			float labelX, labelY;

			if (Orientation == SliderOrientation.Horizontal)
			{
				// Position label to the right of the slider
				labelX = pos.X + size.X + 8;
				labelY = pos.Y + (size.Y - textSize.Y) / 2f;
			}
			else
			{
				// Position label below the slider
				labelX = pos.X + (size.X - textSize.X) / 2f;
				labelY = pos.Y + size.Y + 4;
			}

			UI.Graphics.DrawTextColor(UI.Settings.FontDefault, label, new Vector2(labelX, labelY), LabelColor);
		}
	}
}
