using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// A circular/radial gauge control for displaying values like speedometers, RPM gauges, etc.
	/// Supports configurable angle range, tick marks, labels, color zones, and needle rendering.
	/// </summary>
	public class RadialGauge : Control
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
		/// Start angle of the gauge in degrees (0 = right, 90 = down, 180 = left, 270 = up).
		/// Default is 225 degrees (lower-left).
		/// </summary>
		[YamlMember]
		public float StartAngle { get; set; } = 225f;

		/// <summary>
		/// End angle of the gauge in degrees.
		/// Default is 315 degrees (-45 or lower-right), giving a 270-degree sweep.
		/// </summary>
		[YamlMember]
		public float EndAngle { get; set; } = -45f;

		/// <summary>
		/// Background color of the gauge arc.
		/// </summary>
		[YamlMember]
		public FishColor BackgroundColor { get; set; } = new FishColor(60, 60, 60, 255);

		/// <summary>
		/// Color of the gauge arc when no color zones are defined.
		/// </summary>
		[YamlMember]
		public FishColor ArcColor { get; set; } = new FishColor(100, 100, 100, 255);

		/// <summary>
		/// Color of the needle/pointer.
		/// </summary>
		[YamlMember]
		public FishColor NeedleColor { get; set; } = new FishColor(255, 80, 80, 255);

		/// <summary>
		/// Color of the center hub/pivot.
		/// </summary>
		[YamlMember]
		public FishColor HubColor { get; set; } = new FishColor(80, 80, 80, 255);

		/// <summary>
		/// Thickness of the gauge arc in pixels.
		/// </summary>
		[YamlMember]
		public float ArcThickness { get; set; } = 15f;

		/// <summary>
		/// Width of the needle at its base.
		/// </summary>
		[YamlMember]
		public float NeedleWidth { get; set; } = 4f;

		/// <summary>
		/// Length of the needle as a fraction of the radius (0.0 to 1.0).
		/// </summary>
		[YamlMember]
		public float NeedleLength { get; set; } = 0.85f;

		/// <summary>
		/// Radius of the center hub as a fraction of the gauge radius.
		/// </summary>
		[YamlMember]
		public float HubRadius { get; set; } = 0.1f;

		/// <summary>
		/// Whether to show tick marks along the gauge arc.
		/// </summary>
		[YamlMember]
		public bool ShowTicks { get; set; } = true;

		/// <summary>
		/// Number of major tick divisions.
		/// </summary>
		[YamlMember]
		public int MajorTickCount { get; set; } = 10;

		/// <summary>
		/// Number of minor ticks between major ticks.
		/// </summary>
		[YamlMember]
		public int MinorTickCount { get; set; } = 4;

		/// <summary>
		/// Length of major tick marks in pixels.
		/// </summary>
		[YamlMember]
		public float MajorTickLength { get; set; } = 12f;

		/// <summary>
		/// Length of minor tick marks in pixels.
		/// </summary>
		[YamlMember]
		public float MinorTickLength { get; set; } = 6f;

		/// <summary>
		/// Color of tick marks.
		/// </summary>
		[YamlMember]
		public FishColor TickColor { get; set; } = new FishColor(200, 200, 200, 255);

		/// <summary>
		/// Whether to show labels at major tick positions.
		/// </summary>
		[YamlMember]
		public bool ShowLabels { get; set; } = true;

		/// <summary>
		/// Color of the labels.
		/// </summary>
		[YamlMember]
		public FishColor LabelColor { get; set; } = new FishColor(220, 220, 220, 255);

		/// <summary>
		/// Whether to show the current value in the center.
		/// </summary>
		[YamlMember]
		public bool ShowValue { get; set; } = true;

		/// <summary>
		/// Format string for value display (e.g., "F0" for no decimals).
		/// </summary>
		[YamlMember]
		public string ValueFormat { get; set; } = "F0";

		/// <summary>
		/// Optional unit suffix for value display (e.g., "RPM", "km/h").
		/// </summary>
		[YamlMember]
		public string UnitSuffix { get; set; } = "";

		/// <summary>
		/// Color zones for the gauge arc. If empty, uses ArcColor for the entire range.
		/// </summary>
		[YamlIgnore]
		public List<GaugeColorZone> ColorZones { get; set; } = new List<GaugeColorZone>();

		public RadialGauge()
		{
			Size = new Vector2(150, 150);
		}

		/// <summary>
		/// Creates a RadialGauge with specified range.
		/// </summary>
		public RadialGauge(float minValue, float maxValue) : this()
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
		/// Sets up an RPM-style gauge with red zone at high values.
		/// </summary>
		public void SetupRPMZones()
		{
			ColorZones.Clear();
			ColorZones.Add(new GaugeColorZone(0f, 0.7f, new FishColor(80, 80, 80, 255)));    // Normal (gray)
			ColorZones.Add(new GaugeColorZone(0.7f, 0.85f, new FishColor(255, 200, 0, 255))); // Warning (yellow)
			ColorZones.Add(new GaugeColorZone(0.85f, 1f, new FishColor(220, 50, 50, 255)));   // Redline
		}

		/// <summary>
		/// Sets up a speedometer-style gauge with green-yellow-red zones.
		/// </summary>
		public void SetupSpeedZones()
		{
			ColorZones.Clear();
			ColorZones.Add(new GaugeColorZone(0f, 0.5f, new FishColor(0, 180, 80, 255)));    // Green (safe)
			ColorZones.Add(new GaugeColorZone(0.5f, 0.75f, new FishColor(255, 200, 0, 255))); // Yellow (caution)
			ColorZones.Add(new GaugeColorZone(0.75f, 1f, new FishColor(220, 50, 50, 255)));   // Red (danger)
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
		/// Converts a normalized value (0-1) to an angle in radians.
		/// </summary>
		private float NormalizedToAngle(float normalized)
		{
			float startRad = StartAngle * MathF.PI / 180f;
			float endRad = EndAngle * MathF.PI / 180f;

			// Handle wrap-around (e.g., 225 to -45 degrees)
			float sweep = endRad - startRad;
			if (EndAngle < StartAngle)
				sweep = (360f * MathF.PI / 180f) + sweep;

			return startRad + sweep * normalized;
		}

		/// <summary>
		/// Gets the sweep angle in degrees.
		/// </summary>
		private float GetSweepAngle()
		{
			float sweep = EndAngle - StartAngle;
			if (EndAngle < StartAngle)
				sweep = 360f + sweep;
			return sweep;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			// Calculate center and radius
			Vector2 center = pos + size / 2f;
			float radius = Math.Min(size.X, size.Y) / 2f - 5f; // 5px margin

			// Draw background circle
			DrawBackgroundArc(UI, center, radius);

			// Draw color zones or arc
			if (ColorZones.Count > 0)
			{
				foreach (var zone in ColorZones)
				{
					DrawArcSegment(UI, center, radius, zone.Start, zone.End, zone.Color);
				}
			}
			else
			{
				DrawArcSegment(UI, center, radius, 0f, 1f, ArcColor);
			}

			// Draw ticks
			if (ShowTicks)
			{
				DrawTicks(UI, center, radius);
			}

			// Draw labels
			if (ShowLabels)
			{
				DrawLabels(UI, center, radius);
			}

			// Draw needle
			DrawNeedle(UI, center, radius);

			// Draw center hub
			DrawHub(UI, center, radius);

			// Draw current value
			if (ShowValue)
			{
				DrawValue(UI, center, radius);
			}
		}

		private void DrawBackgroundArc(FishUI UI, Vector2 center, float radius)
		{
			// Draw arc segments to approximate the background
			int segments = 32;
			float outerRadius = radius;
			float innerRadius = radius - ArcThickness;

			for (int i = 0; i < segments; i++)
			{
				float t1 = (float)i / segments;
				float t2 = (float)(i + 1) / segments;
				float angle1 = NormalizedToAngle(t1);
				float angle2 = NormalizedToAngle(t2);

				DrawArcQuad(UI, center, innerRadius, outerRadius, angle1, angle2, BackgroundColor);
			}
		}

		private void DrawArcSegment(FishUI UI, Vector2 center, float radius, float startNorm, float endNorm, FishColor color)
		{
			int segments = (int)(32 * (endNorm - startNorm));
			segments = Math.Max(segments, 4);

			float outerRadius = radius;
			float innerRadius = radius - ArcThickness;

			for (int i = 0; i < segments; i++)
			{
				float t1 = startNorm + (endNorm - startNorm) * ((float)i / segments);
				float t2 = startNorm + (endNorm - startNorm) * ((float)(i + 1) / segments);
				float angle1 = NormalizedToAngle(t1);
				float angle2 = NormalizedToAngle(t2);

				DrawArcQuad(UI, center, innerRadius, outerRadius, angle1, angle2, color);
			}
		}

		private void DrawArcQuad(FishUI UI, Vector2 center, float innerRadius, float outerRadius, float angle1, float angle2, FishColor color)
		{
			// Calculate the four corners of the arc segment
			Vector2 inner1 = center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * innerRadius;
			Vector2 outer1 = center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * outerRadius;
			Vector2 inner2 = center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * innerRadius;
			Vector2 outer2 = center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * outerRadius;

			// Draw as two triangles using lines (approximation)
			UI.Graphics.DrawLine(inner1, outer1, ArcThickness * 0.5f, color);
			UI.Graphics.DrawLine(inner2, outer2, ArcThickness * 0.5f, color);
			UI.Graphics.DrawLine(outer1, outer2, 2f, color);
			UI.Graphics.DrawLine(inner1, inner2, 2f, color);
		}

		private void DrawTicks(FishUI UI, Vector2 center, float radius)
		{
			int totalTicks = MajorTickCount * (MinorTickCount + 1) + 1;

			for (int i = 0; i <= MajorTickCount * (MinorTickCount + 1); i++)
			{
				float t = (float)i / (MajorTickCount * (MinorTickCount + 1));
				float angle = NormalizedToAngle(t);

				bool isMajor = (i % (MinorTickCount + 1)) == 0;
				float tickLen = isMajor ? MajorTickLength : MinorTickLength;
				float tickWidth = isMajor ? 2f : 1f;

				float innerRadius = radius - ArcThickness - 2f;
				float outerRadius = innerRadius - tickLen;

				Vector2 inner = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * innerRadius;
				Vector2 outer = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * outerRadius;

				UI.Graphics.DrawLine(outer, inner, tickWidth, TickColor);
			}
		}

		private void DrawLabels(FishUI UI, Vector2 center, float radius)
		{
			float labelRadius = radius - ArcThickness - MajorTickLength - 12f;

			for (int i = 0; i <= MajorTickCount; i++)
			{
				float t = (float)i / MajorTickCount;
				float angle = NormalizedToAngle(t);
				float labelValue = MinValue + (MaxValue - MinValue) * t;
				string labelText = labelValue.ToString(ValueFormat);

				Vector2 labelPos = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * labelRadius;
				Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, labelText);

				// Center the label on the position
				labelPos -= textSize / 2f;

				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, labelText, labelPos, LabelColor);
			}
		}

		private void DrawNeedle(FishUI UI, Vector2 center, float radius)
		{
			float normalized = GetNormalizedValue();
			float angle = NormalizedToAngle(normalized);

			float needleLen = radius * NeedleLength;
			Vector2 tip = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * needleLen;

			// Draw needle as a thick line
			UI.Graphics.DrawLine(center, tip, NeedleWidth, NeedleColor);

			// Draw a small circle at the tip for visibility
			float tipRadius = NeedleWidth * 0.75f;
			DrawFilledCircle(UI, tip, tipRadius, NeedleColor);
		}

		private void DrawHub(FishUI UI, Vector2 center, float radius)
		{
			float hubRad = radius * HubRadius;
			DrawFilledCircle(UI, center, hubRad, HubColor);
		}

		private void DrawFilledCircle(FishUI UI, Vector2 center, float radius, FishColor color)
		{
			// Approximate circle with line segments
			int segments = 16;
			for (int i = 0; i < segments; i++)
			{
				float angle1 = (float)i / segments * MathF.PI * 2f;
				float angle2 = (float)(i + 1) / segments * MathF.PI * 2f;

				Vector2 p1 = center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * radius;
				Vector2 p2 = center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * radius;

				UI.Graphics.DrawLine(center, p1, radius, color);
				UI.Graphics.DrawLine(center, p2, radius, color);
			}
		}

		private void DrawValue(FishUI UI, Vector2 center, float radius)
		{
			string valueText = Value.ToString(ValueFormat);
			Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, valueText);

			// Position value below center
			Vector2 valuePos = center + new Vector2(-textSize.X / 2f, radius * 0.25f);
			UI.Graphics.DrawTextColor(UI.Settings.FontDefault, valueText, valuePos, LabelColor);

			// Draw unit suffix below value
			if (!string.IsNullOrEmpty(UnitSuffix))
			{
				Vector2 unitSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, UnitSuffix);
				Vector2 unitPos = center + new Vector2(-unitSize.X / 2f, radius * 0.25f + textSize.Y + 2f);
				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, UnitSuffix, unitPos, new FishColor(150, 150, 150, 255));
			}
		}
	}
}
