using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Represents a color zone within a BarGauge.
	/// </summary>
	public class GaugeColorZone
	{
		/// <summary>
		/// Start value of this zone (as a percentage of the gauge range, 0.0 to 1.0).
		/// </summary>
		public float Start { get; set; }

		/// <summary>
		/// End value of this zone (as a percentage of the gauge range, 0.0 to 1.0).
		/// </summary>
		public float End { get; set; }

		/// <summary>
		/// Color of this zone.
		/// </summary>
		public FishColor Color { get; set; }

		public GaugeColorZone() { }

		public GaugeColorZone(float start, float end, FishColor color)
		{
			Start = start;
			End = end;
			Color = color;
		}
	}

	/// <summary>
	/// Orientation for the BarGauge.
	/// </summary>
	public enum BarGaugeOrientation
	{
		Horizontal,
		Vertical
	}

	/// <summary>
	/// A linear gauge control for displaying values with optional color zones and tick marks.
	/// Useful for temperature, fuel level, progress indicators, etc.
	/// </summary>
	public class BarGauge : Control
	{
		/// <summary>
		/// Minimum value of the gauge.
		/// </summary>
		[YamlMember]
		public float MinValue { get; set; } = 0f;

		/// <summary>
		/// Maximum value of the gauge.
		/// </summary>
		[YamlMember]
		public float MaxValue { get; set; } = 100f;

		/// <summary>
		/// Current value of the gauge.
		/// </summary>
		[YamlMember]
		public float Value
		{
			get => _value;
			set => _value = Math.Clamp(value, MinValue, MaxValue);
		}
		private float _value = 0f;

		/// <summary>
		/// Orientation of the gauge (Horizontal or Vertical).
		/// </summary>
		[YamlMember]
		public BarGaugeOrientation Orientation { get; set; } = BarGaugeOrientation.Horizontal;

		/// <summary>
		/// Background color of the gauge track.
		/// </summary>
		[YamlMember]
		public FishColor BackgroundColor { get; set; } = new FishColor(40, 40, 40, 255);

		/// <summary>
		/// Default fill color when no color zones are defined.
		/// </summary>
		[YamlMember]
		public FishColor FillColor { get; set; } = new FishColor(0, 200, 100, 255);

		/// <summary>
		/// Border color of the gauge.
		/// </summary>
		[YamlMember]
		public FishColor BorderColor { get; set; } = new FishColor(80, 80, 80, 255);

		/// <summary>
		/// Whether to show a border around the gauge.
		/// </summary>
		[YamlMember]
		public bool ShowBorder { get; set; } = true;

		/// <summary>
		/// Whether to show tick marks along the gauge.
		/// </summary>
		[YamlMember]
		public bool ShowTicks { get; set; } = false;

		/// <summary>
		/// Number of major tick divisions.
		/// </summary>
		[YamlMember]
		public int TickCount { get; set; } = 5;

		/// <summary>
		/// Color of tick marks.
		/// </summary>
		[YamlMember]
		public FishColor TickColor { get; set; } = new FishColor(200, 200, 200, 255);

		/// <summary>
		/// Length of tick marks in pixels.
		/// </summary>
		[YamlMember]
		public float TickLength { get; set; } = 5f;

		/// <summary>
		/// Whether to show labels at tick positions.
		/// </summary>
		[YamlMember]
		public bool ShowLabels { get; set; } = false;

		/// <summary>
		/// Whether to show the current value as text.
		/// </summary>
		[YamlMember]
		public bool ShowValue { get; set; } = false;

		/// <summary>
		/// Format string for value display (e.g., "F0" for no decimals, "F1" for one decimal).
		/// </summary>
		[YamlMember]
		public string ValueFormat { get; set; } = "F0";

		/// <summary>
		/// Optional unit suffix for value display (e.g., "°C", "%", "L").
		/// </summary>
		[YamlMember]
		public string UnitSuffix { get; set; } = "";

		/// <summary>
		/// Color zones for the gauge. If empty, uses FillColor for the entire range.
		/// </summary>
		[YamlIgnore]
		public List<GaugeColorZone> ColorZones { get; set; } = new List<GaugeColorZone>();

		public BarGauge()
		{
			Size = new Vector2(200, 30);
		}

		/// <summary>
		/// Creates a BarGauge with specified range.
		/// </summary>
		public BarGauge(float minValue, float maxValue) : this()
		{
			MinValue = minValue;
			MaxValue = maxValue;
		}

		/// <summary>
		/// Adds a color zone to the gauge.
		/// </summary>
		/// <param name="startPercent">Start of zone as percentage (0.0 to 1.0).</param>
		/// <param name="endPercent">End of zone as percentage (0.0 to 1.0).</param>
		/// <param name="color">Color for this zone.</param>
		public void AddColorZone(float startPercent, float endPercent, FishColor color)
		{
			ColorZones.Add(new GaugeColorZone(startPercent, endPercent, color));
		}

		/// <summary>
		/// Sets up a classic green-yellow-red temperature-style gauge.
		/// </summary>
		public void SetupTemperatureZones()
		{
			ColorZones.Clear();
			ColorZones.Add(new GaugeColorZone(0f, 0.6f, new FishColor(0, 180, 80, 255)));    // Green
			ColorZones.Add(new GaugeColorZone(0.6f, 0.8f, new FishColor(255, 200, 0, 255))); // Yellow
			ColorZones.Add(new GaugeColorZone(0.8f, 1f, new FishColor(220, 50, 50, 255)));   // Red
		}

		/// <summary>
		/// Sets up a fuel-style gauge (red-yellow-green).
		/// </summary>
		public void SetupFuelZones()
		{
			ColorZones.Clear();
			ColorZones.Add(new GaugeColorZone(0f, 0.2f, new FishColor(220, 50, 50, 255)));   // Red (low)
			ColorZones.Add(new GaugeColorZone(0.2f, 0.4f, new FishColor(255, 200, 0, 255))); // Yellow
			ColorZones.Add(new GaugeColorZone(0.4f, 1f, new FishColor(0, 180, 80, 255)));    // Green
		}

		/// <summary>
		/// Gets the normalized value (0.0 to 1.0) based on current value and range.
		/// </summary>
		private float GetNormalizedValue()
		{
			if (MaxValue <= MinValue)
				return 0f;
			return (Value - MinValue) / (MaxValue - MinValue);
		}

		/// <summary>
		/// Gets the fill color at the current value position.
		/// </summary>
		private FishColor GetFillColorAtValue()
		{
			if (ColorZones.Count == 0)
				return FillColor;

			float normalized = GetNormalizedValue();
			foreach (var zone in ColorZones)
			{
				if (normalized >= zone.Start && normalized <= zone.End)
					return zone.Color;
			}
			return FillColor;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();
			float normalized = GetNormalizedValue();

			// Determine gauge area (leave room for ticks/labels if shown)
			Vector2 gaugePos = pos;
			Vector2 gaugeSize = size;

			if (ShowTicks || ShowLabels)
			{
				float margin = ShowLabels ? 15f : TickLength + 2f;
				if (Orientation == BarGaugeOrientation.Horizontal)
				{
					gaugeSize.Y -= margin;
				}
				else
				{
					gaugeSize.X -= margin;
				}
			}

			// Draw background
			UI.Graphics.DrawRectangle(gaugePos, gaugeSize, BackgroundColor);

			// Draw color zones as background (full width)
			if (ColorZones.Count > 0)
			{
				foreach (var zone in ColorZones)
				{
					DrawZone(UI, gaugePos, gaugeSize, zone);
				}
			}

			// Draw fill overlay (darker portion for unfilled area)
			DrawUnfilledOverlay(UI, gaugePos, gaugeSize, normalized);

			// Draw border
			if (ShowBorder)
			{
				UI.Graphics.DrawRectangleOutline(gaugePos, gaugeSize, BorderColor);
			}

			// Draw ticks
			if (ShowTicks)
			{
				DrawTicks(UI, gaugePos, gaugeSize);
			}

			// Draw labels
			if (ShowLabels)
			{
				DrawLabels(UI, gaugePos, gaugeSize);
			}

			// Draw current value
			if (ShowValue)
			{
				DrawValue(UI, gaugePos, gaugeSize);
			}
		}

		private void DrawZone(FishUI UI, Vector2 pos, Vector2 size, GaugeColorZone zone)
		{
			if (Orientation == BarGaugeOrientation.Horizontal)
			{
				float startX = pos.X + size.X * zone.Start;
				float endX = pos.X + size.X * zone.End;
				UI.Graphics.DrawRectangle(
					new Vector2(startX, pos.Y),
					new Vector2(endX - startX, size.Y),
					zone.Color);
			}
			else
			{
				float startY = pos.Y + size.Y * (1f - zone.End);
				float endY = pos.Y + size.Y * (1f - zone.Start);
				UI.Graphics.DrawRectangle(
					new Vector2(pos.X, startY),
					new Vector2(size.X, endY - startY),
					zone.Color);
			}
		}

		private void DrawUnfilledOverlay(FishUI UI, Vector2 pos, Vector2 size, float normalized)
		{
			// Draw a semi-transparent dark overlay over the unfilled portion
			FishColor overlay = new FishColor(0, 0, 0, 150);

			if (Orientation == BarGaugeOrientation.Horizontal)
			{
				float fillWidth = size.X * normalized;
				float unfillStart = pos.X + fillWidth;
				float unfillWidth = size.X - fillWidth;
				if (unfillWidth > 0)
				{
					UI.Graphics.DrawRectangle(
						new Vector2(unfillStart, pos.Y),
						new Vector2(unfillWidth, size.Y),
						overlay);
				}
			}
			else
			{
				float fillHeight = size.Y * normalized;
				float unfillHeight = size.Y - fillHeight;
				if (unfillHeight > 0)
				{
					UI.Graphics.DrawRectangle(
						pos,
						new Vector2(size.X, unfillHeight),
						overlay);
				}
			}
		}

		private void DrawTicks(FishUI UI, Vector2 pos, Vector2 size)
		{
			for (int i = 0; i <= TickCount; i++)
			{
				float t = (float)i / TickCount;

				if (Orientation == BarGaugeOrientation.Horizontal)
				{
					float x = pos.X + size.X * t;
					UI.Graphics.DrawLine(
						new Vector2(x, pos.Y + size.Y),
						new Vector2(x, pos.Y + size.Y + TickLength),
						1f, TickColor);
				}
				else
				{
					float y = pos.Y + size.Y * (1f - t);
					UI.Graphics.DrawLine(
						new Vector2(pos.X + size.X, y),
						new Vector2(pos.X + size.X + TickLength, y),
						1f, TickColor);
				}
			}
		}

		private void DrawLabels(FishUI UI, Vector2 pos, Vector2 size)
		{
			for (int i = 0; i <= TickCount; i++)
			{
				float t = (float)i / TickCount;
				float labelValue = MinValue + (MaxValue - MinValue) * t;
				string labelText = labelValue.ToString(ValueFormat);

				if (Orientation == BarGaugeOrientation.Horizontal)
				{
					float x = pos.X + size.X * t;
					Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, labelText);
					UI.Graphics.DrawText(UI.Settings.FontDefault, labelText,
						new Vector2(x - textSize.X / 2, pos.Y + size.Y + TickLength + 2));
				}
				else
				{
					float y = pos.Y + size.Y * (1f - t);
					Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, labelText);
					UI.Graphics.DrawText(UI.Settings.FontDefault, labelText,
						new Vector2(pos.X + size.X + TickLength + 2, y - textSize.Y / 2));
				}
			}
		}

		private void DrawValue(FishUI UI, Vector2 pos, Vector2 size)
		{
			string valueText = Value.ToString(ValueFormat) + UnitSuffix;
			Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, valueText);

			// Center the value text on the gauge
			Vector2 textPos = pos + size / 2 - textSize / 2;
			UI.Graphics.DrawTextColor(UI.Settings.FontDefault, valueText, textPos, FishColor.White);
		}
	}
}
