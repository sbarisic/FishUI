using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Orientation for the VUMeter.
	/// </summary>
	public enum VUMeterOrientation
	{
		Horizontal,
		Vertical
	}

	/// <summary>
	/// A VU meter control for visualizing audio levels with peak hold indicator.
	/// </summary>
	public class VUMeter : Control
	{
		/// <summary>
		/// Current level value (0.0 to 1.0).
		/// </summary>
		[YamlMember]
		public float Value
		{
			get => _value;
			set => _value = Math.Clamp(value, 0f, 1f);
		}
		private float _value = 0f;

		/// <summary>
		/// Peak level value (0.0 to 1.0). Automatically decays over time.
		/// </summary>
		[YamlIgnore]
		public float PeakValue
		{
			get => _peakValue;
			set => _peakValue = Math.Clamp(value, 0f, 1f);
		}
		private float _peakValue = 0f;

		/// <summary>
		/// Orientation of the meter (Horizontal or Vertical).
		/// </summary>
		[YamlMember]
		public VUMeterOrientation Orientation { get; set; } = VUMeterOrientation.Vertical;

		/// <summary>
		/// Whether to show the peak hold indicator.
		/// </summary>
		[YamlMember]
		public bool ShowPeak { get; set; } = true;

		/// <summary>
		/// Time in seconds before peak starts to decay.
		/// </summary>
		[YamlMember]
		public float PeakHoldTime { get; set; } = 1.0f;

		/// <summary>
		/// Speed at which the peak decays (units per second).
		/// </summary>
		[YamlMember]
		public float PeakDecaySpeed { get; set; } = 0.5f;

		/// <summary>
		/// Background color of the meter track.
		/// </summary>
		[YamlMember]
		public FishColor BackgroundColor { get; set; } = new FishColor(30, 30, 30, 255);

		/// <summary>
		/// Color for the normal/green level zone (0% to GreenZoneEnd).
		/// </summary>
		[YamlMember]
		public FishColor GreenColor { get; set; } = new FishColor(0, 200, 80, 255);

		/// <summary>
		/// Color for the yellow/warning zone (GreenZoneEnd to YellowZoneEnd).
		/// </summary>
		[YamlMember]
		public FishColor YellowColor { get; set; } = new FishColor(255, 220, 0, 255);

		/// <summary>
		/// Color for the red/clipping zone (YellowZoneEnd to 100%).
		/// </summary>
		[YamlMember]
		public FishColor RedColor { get; set; } = new FishColor(255, 50, 50, 255);

		/// <summary>
		/// End of green zone as percentage (0.0 to 1.0).
		/// </summary>
		[YamlMember]
		public float GreenZoneEnd { get; set; } = 0.6f;

		/// <summary>
		/// End of yellow zone as percentage (0.0 to 1.0).
		/// </summary>
		[YamlMember]
		public float YellowZoneEnd { get; set; } = 0.85f;

		/// <summary>
		/// Color of the peak indicator.
		/// </summary>
		[YamlMember]
		public FishColor PeakColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Thickness of the peak indicator line in pixels.
		/// </summary>
		[YamlMember]
		public float PeakThickness { get; set; } = 2f;

		/// <summary>
		/// Whether to show a border around the meter.
		/// </summary>
		[YamlMember]
		public bool ShowBorder { get; set; } = true;

		/// <summary>
		/// Border color of the meter.
		/// </summary>
		[YamlMember]
		public FishColor BorderColor { get; set; } = new FishColor(80, 80, 80, 255);

		/// <summary>
		/// Number of LED-style segments to display (0 = smooth/continuous).
		/// </summary>
		[YamlMember]
		public int SegmentCount { get; set; } = 0;

		/// <summary>
		/// Gap between LED segments in pixels (only when SegmentCount > 0).
		/// </summary>
		[YamlMember]
		public float SegmentGap { get; set; } = 1f;

		private float _peakHoldTimer = 0f;

		public VUMeter()
		{
			Size = new Vector2(20, 150);
		}

		/// <summary>
		/// Sets the current level and automatically updates the peak if higher.
		/// </summary>
		/// <param name="level">Level value from 0.0 to 1.0</param>
		public void SetLevel(float level)
		{
			Value = level;
			if (level > _peakValue)
			{
				_peakValue = level;
				_peakHoldTimer = PeakHoldTime;
			}
		}

		/// <summary>
		/// Resets the peak value to zero.
		/// </summary>
		public void ResetPeak()
		{
			_peakValue = 0f;
			_peakHoldTimer = 0f;
		}

		/// <summary>
		/// Gets the color at the specified position along the meter.
		/// </summary>
		private FishColor GetColorAtPosition(float position)
		{
			if (position <= GreenZoneEnd)
				return GreenColor;
			else if (position <= YellowZoneEnd)
				return YellowColor;
			else
				return RedColor;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			// Update peak decay
			if (_peakHoldTimer > 0)
			{
				_peakHoldTimer -= Dt;
			}
			else if (_peakValue > Value)
			{
				_peakValue -= PeakDecaySpeed * Dt;

				if (_peakValue < Value)
					_peakValue = Value;
			}

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			// Draw background
			UI.Graphics.DrawRectangle(pos, size, BackgroundColor);

			// Draw level
			if (SegmentCount > 0)
			{
				DrawSegmented(UI, pos, size);
			}
			else
			{
				DrawContinuous(UI, pos, size);
			}

			// Draw peak indicator
			if (ShowPeak && _peakValue > 0)
			{
				DrawPeakIndicator(UI, pos, size);
			}

			// Draw border
			if (ShowBorder)
			{
				UI.Graphics.DrawRectangleOutline(pos, size, BorderColor);
			}
		}

		private void DrawContinuous(FishUI UI, Vector2 pos, Vector2 size)
		{
			if (Value <= 0)
				return;

			if (Orientation == VUMeterOrientation.Horizontal)
			{
				// Draw each color zone up to the current value
				DrawColorZoneHorizontal(UI, pos, size, 0, Math.Min(Value, GreenZoneEnd), GreenColor);

				if (Value > GreenZoneEnd)
					DrawColorZoneHorizontal(UI, pos, size, GreenZoneEnd, Math.Min(Value, YellowZoneEnd), YellowColor);

				if (Value > YellowZoneEnd)
					DrawColorZoneHorizontal(UI, pos, size, YellowZoneEnd, Value, RedColor);
			}
			else
			{
				// Vertical - draw from bottom up
				DrawColorZoneVertical(UI, pos, size, 0, Math.Min(Value, GreenZoneEnd), GreenColor);

				if (Value > GreenZoneEnd)
					DrawColorZoneVertical(UI, pos, size, GreenZoneEnd, Math.Min(Value, YellowZoneEnd), YellowColor);

				if (Value > YellowZoneEnd)
					DrawColorZoneVertical(UI, pos, size, YellowZoneEnd, Value, RedColor);
			}
		}

		private void DrawColorZoneHorizontal(FishUI UI, Vector2 pos, Vector2 size, float start, float end, FishColor color)
		{
			float startX = pos.X + size.X * start;
			float endX = pos.X + size.X * end;

			UI.Graphics.DrawRectangle(
				new Vector2(startX, pos.Y),
				new Vector2(endX - startX, size.Y),
				color);
		}

		private void DrawColorZoneVertical(FishUI UI, Vector2 pos, Vector2 size, float start, float end, FishColor color)
		{
			float startY = pos.Y + size.Y * (1f - end);
			float endY = pos.Y + size.Y * (1f - start);

			UI.Graphics.DrawRectangle(
				new Vector2(pos.X, startY),
				new Vector2(size.X, endY - startY),
				color);
		}

		private void DrawSegmented(FishUI UI, Vector2 pos, Vector2 size)
		{
			if (Orientation == VUMeterOrientation.Horizontal)
			{
				float segmentWidth = (size.X - (SegmentCount - 1) * SegmentGap) / SegmentCount;

				for (int i = 0; i < SegmentCount; i++)
				{
					float segmentPos = (float)i / SegmentCount;
					float segmentEnd = (float)(i + 1) / SegmentCount;

					if (Value >= segmentEnd || (Value > segmentPos && Value < segmentEnd))
					{
						bool lit = Value >= segmentEnd;
						FishColor color = GetColorAtPosition(segmentPos);
						if (!lit) color = new FishColor((byte)(color.R / 3), (byte)(color.G / 3), (byte)(color.B / 3), color.A);

						float x = pos.X + i * (segmentWidth + SegmentGap);
						UI.Graphics.DrawRectangle(
							new Vector2(x, pos.Y),
							new Vector2(segmentWidth, size.Y),
							color);
					}
				}
			}
			else
			{
				float segmentHeight = (size.Y - (SegmentCount - 1) * SegmentGap) / SegmentCount;

				for (int i = 0; i < SegmentCount; i++)
				{
					float segmentPos = (float)i / SegmentCount;
					float segmentEnd = (float)(i + 1) / SegmentCount;

					bool lit = Value >= segmentEnd;
					FishColor color = GetColorAtPosition(segmentPos);

					if (!lit)
						color = new FishColor((byte)(color.R / 4), (byte)(color.G / 4), (byte)(color.B / 4), color.A);

					float y = pos.Y + size.Y - (i + 1) * segmentHeight - i * SegmentGap;

					UI.Graphics.DrawRectangle(
						new Vector2(pos.X, y),
						new Vector2(size.X, segmentHeight),
						color);
				}
			}
		}

		private void DrawPeakIndicator(FishUI UI, Vector2 pos, Vector2 size)
		{
			if (Orientation == VUMeterOrientation.Horizontal)
			{
				float peakX = pos.X + size.X * _peakValue;
				UI.Graphics.DrawRectangle(new Vector2(peakX - PeakThickness / 2, pos.Y), new Vector2(PeakThickness, size.Y), PeakColor);
			}
			else
			{
				float peakY = pos.Y + size.Y * (1f - _peakValue);

				UI.Graphics.DrawRectangle(new Vector2(pos.X, peakY - PeakThickness / 2), new Vector2(size.X, PeakThickness), PeakColor);
			}
		}
	}
}
