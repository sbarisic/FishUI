using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Represents a data series in a LineChart.
	/// </summary>
	public class LineChartSeries
	{
		/// <summary>
		/// Name of the series (used for legends).
		/// </summary>
		public string Name { get; set; } = "Series";

		/// <summary>
		/// Color of the line for this series.
		/// </summary>
		public FishColor Color { get; set; } = FishColor.Green;

		/// <summary>
		/// Line thickness in pixels.
		/// </summary>
		public float LineThickness { get; set; } = 2f;

		/// <summary>
		/// Data points for this series. Each point is a (time, value) pair.
		/// </summary>
		[YamlIgnore]
		public List<Vector2> Points { get; } = new List<Vector2>();

	/// <summary>
		/// Maximum number of points to retain. Older points are removed when exceeded.
		/// Set to 0 for unlimited points.
		/// </summary>
		public int MaxPoints { get; set; } = 5000;

		public LineChartSeries() { }

		public LineChartSeries(string name, FishColor color)
		{
			Name = name;
			Color = color;
		}

		/// <summary>
		/// Adds a data point to the series.
		/// </summary>
		/// <param name="time">X-axis value (time).</param>
		/// <param name="value">Y-axis value.</param>
		public void AddPoint(float time, float value)
		{
			Points.Add(new Vector2(time, value));

			// Trim old points if MaxPoints is set
			if (MaxPoints > 0 && Points.Count > MaxPoints)
			{
				Points.RemoveAt(0);
			}
		}

		/// <summary>
		/// Clears all data points from the series.
		/// </summary>
		public void Clear()
		{
			Points.Clear();
		}
	}

	/// <summary>
	/// A line chart control for real-time data visualization.
	/// Displays one or more data series as lines over a configurable time window.
	/// </summary>
	public class LineChart : Control
	{
		/// <summary>
		/// Minimum value on the Y-axis.
		/// </summary>
		[YamlMember]
		public float MinValue { get; set; } = 0f;

		/// <summary>
		/// Maximum value on the Y-axis.
		/// </summary>
		[YamlMember]
		public float MaxValue { get; set; } = 100f;

		/// <summary>
		/// Time window to display on the X-axis (in seconds).
		/// Only data within this window from the current time is displayed.
		/// </summary>
		[YamlMember]
		public float TimeWindow { get; set; } = 10f;

		/// <summary>
		/// Current time reference for the chart. Data points with time less than
		/// (CurrentTime - TimeWindow) are not displayed.
		/// </summary>
		[YamlIgnore]
		public float CurrentTime { get; set; } = 0f;

		/// <summary>
		/// Whether to auto-scroll the time window as CurrentTime advances.
		/// </summary>
		[YamlMember]
		public bool AutoScroll { get; set; } = true;

		/// <summary>
		/// Background color of the chart area.
		/// </summary>
		[YamlMember]
		public FishColor BackgroundColor { get; set; } = new FishColor(30, 30, 30, 255);

		/// <summary>
		/// Border color of the chart.
		/// </summary>
		[YamlMember]
		public FishColor BorderColor { get; set; } = new FishColor(80, 80, 80, 255);

		/// <summary>
		/// Color of the grid lines.
		/// </summary>
		[YamlMember]
		public FishColor GridColor { get; set; } = new FishColor(60, 60, 60, 255);

	/// <summary>
		/// Color of the axis labels.
		/// </summary>
		[YamlMember]
		public FishColor LabelColor { get; set; } = new FishColor(0, 0, 0, 255);

		/// <summary>
		/// Whether to show horizontal grid lines.
		/// </summary>
		[YamlMember]
		public bool ShowHorizontalGrid { get; set; } = true;

		/// <summary>
		/// Whether to show vertical grid lines.
		/// </summary>
		[YamlMember]
		public bool ShowVerticalGrid { get; set; } = true;

		/// <summary>
		/// Number of horizontal grid divisions.
		/// </summary>
		[YamlMember]
		public int HorizontalGridDivisions { get; set; } = 5;

		/// <summary>
		/// Number of vertical grid divisions.
		/// </summary>
		[YamlMember]
		public int VerticalGridDivisions { get; set; } = 5;

		/// <summary>
		/// Whether to show Y-axis labels.
		/// </summary>
		[YamlMember]
		public bool ShowYAxisLabels { get; set; } = true;

		/// <summary>
		/// Whether to show X-axis labels.
		/// </summary>
		[YamlMember]
		public bool ShowXAxisLabels { get; set; } = true;

		/// <summary>
		/// Width of the Y-axis label area in pixels.
		/// </summary>
		[YamlMember]
		public float YAxisLabelWidth { get; set; } = 50f;

		/// <summary>
		/// Height of the X-axis label area in pixels.
		/// </summary>
		[YamlMember]
		public float XAxisLabelHeight { get; set; } = 20f;

		/// <summary>
		/// Format string for Y-axis labels.
		/// </summary>
		[YamlMember]
		public string YAxisLabelFormat { get; set; } = "F1";

		/// <summary>
		/// Format string for X-axis labels (time).
		/// </summary>
		[YamlMember]
		public string XAxisLabelFormat { get; set; } = "F1";

		/// <summary>
		/// Data series displayed in this chart.
		/// </summary>
		[YamlIgnore]
		public List<LineChartSeries> Series { get; } = new List<LineChartSeries>();

		public LineChart()
		{
			Size = new Vector2(300, 200);
		}

		/// <summary>
		/// Adds a new data series to the chart.
		/// </summary>
		/// <param name="name">Name of the series.</param>
		/// <param name="color">Color of the series line.</param>
		/// <returns>The created series.</returns>
		public LineChartSeries AddSeries(string name, FishColor color)
		{
			var series = new LineChartSeries(name, color);
			Series.Add(series);
			return series;
		}

		/// <summary>
		/// Removes a data series from the chart.
		/// </summary>
		/// <param name="series">The series to remove.</param>
		public void RemoveSeries(LineChartSeries series)
		{
			Series.Remove(series);
		}

		/// <summary>
		/// Clears all data from all series.
		/// </summary>
		public void ClearAllData()
		{
			foreach (var series in Series)
			{
				series.Clear();
			}
		}

		/// <summary>
		/// Advances the current time and optionally adds a data point to a series.
		/// </summary>
		/// <param name="deltaTime">Time elapsed since last update.</param>
		public void Update(float deltaTime)
		{
			CurrentTime += deltaTime;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			base.DrawControl(UI, Dt, Time);

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = ScaledSize;

			// Calculate chart area (excluding labels)
			float labelLeftOffset = ShowYAxisLabels ? Scale(YAxisLabelWidth) : 0;
			float labelBottomOffset = ShowXAxisLabels ? Scale(XAxisLabelHeight) : 0;

			Vector2 chartPos = new Vector2(pos.X + labelLeftOffset, pos.Y);
			Vector2 chartSize = new Vector2(size.X - labelLeftOffset, size.Y - labelBottomOffset);

			// Draw background
			UI.Graphics.DrawRectangle(chartPos, chartSize, BackgroundColor);

			// Draw grid
			DrawGrid(UI, chartPos, chartSize);

			// Draw data series
			DrawSeries(UI, chartPos, chartSize);

			// Draw border
			UI.Graphics.DrawRectangleOutline(chartPos, chartSize, BorderColor);

			// Draw labels
			if (ShowYAxisLabels)
				DrawYAxisLabels(UI, pos, chartSize, labelLeftOffset);

			if (ShowXAxisLabels)
				DrawXAxisLabels(UI, chartPos, chartSize, labelBottomOffset);
		}

	private void DrawGrid(FishUI UI, Vector2 chartPos, Vector2 chartSize)
		{
			// Horizontal grid lines
			if (ShowHorizontalGrid && HorizontalGridDivisions > 0)
			{
				for (int i = 0; i <= HorizontalGridDivisions; i++)
				{
					float y = chartPos.Y + (chartSize.Y * i / HorizontalGridDivisions);
					UI.Graphics.DrawLine(
						new Vector2(chartPos.X, y),
						new Vector2(chartPos.X + chartSize.X, y),
						1f, GridColor);
				}
			}

			// Vertical grid lines
			if (ShowVerticalGrid && VerticalGridDivisions > 0)
			{
				for (int i = 0; i <= VerticalGridDivisions; i++)
				{
					float x = chartPos.X + (chartSize.X * i / VerticalGridDivisions);
					UI.Graphics.DrawLine(
						new Vector2(x, chartPos.Y),
						new Vector2(x, chartPos.Y + chartSize.Y),
						1f, GridColor);
				}
			}
		}

		private void DrawSeries(FishUI UI, Vector2 chartPos, Vector2 chartSize)
		{
			float timeStart = AutoScroll ? CurrentTime - TimeWindow : 0;
			float timeEnd = AutoScroll ? CurrentTime : TimeWindow;

			foreach (var series in Series)
			{
				if (series.Points.Count < 2)
					continue;

				Vector2? lastScreenPoint = null;

				foreach (var point in series.Points)
				{
					// Skip points outside the time window
					if (point.X < timeStart || point.X > timeEnd)
					{
						lastScreenPoint = null;
						continue;
					}

					// Convert data point to screen coordinates
					float normalizedX = (point.X - timeStart) / (timeEnd - timeStart);
					float normalizedY = 1f - ((point.Y - MinValue) / (MaxValue - MinValue));

					// Clamp Y to chart bounds
					normalizedY = Math.Clamp(normalizedY, 0f, 1f);

					Vector2 screenPoint = new Vector2(
						chartPos.X + normalizedX * chartSize.X,
						chartPos.Y + normalizedY * chartSize.Y);

					// Draw line segment
					if (lastScreenPoint.HasValue)
					{
						UI.Graphics.DrawLine(lastScreenPoint.Value, screenPoint, Scale(series.LineThickness), series.Color);
					}

					lastScreenPoint = screenPoint;
				}
			}
		}

	private void DrawYAxisLabels(FishUI UI, Vector2 pos, Vector2 chartSize, float labelWidth)
		{
			var font = UI.Settings.FontDefault;
			if (font == null) return;

			for (int i = 0; i <= HorizontalGridDivisions; i++)
			{
				float t = 1f - (float)i / HorizontalGridDivisions;
				float value = MinValue + t * (MaxValue - MinValue);
				string label = value.ToString(YAxisLabelFormat);

				float y = pos.Y + (chartSize.Y * i / HorizontalGridDivisions);

				// Right-align the label
				var textSize = UI.Graphics.MeasureText(font, label);
				float x = pos.X + labelWidth - textSize.X - Scale(4);


				UI.Graphics.DrawTextColor(font, label, new Vector2(x, y - textSize.Y / 2), LabelColor);
			}
		}

	private void DrawXAxisLabels(FishUI UI, Vector2 chartPos, Vector2 chartSize, float labelHeight)
		{
			var font = UI.Settings.FontDefault;
			if (font == null) return;

			float timeStart = AutoScroll ? CurrentTime - TimeWindow : 0;
			float timeEnd = AutoScroll ? CurrentTime : TimeWindow;

			for (int i = 0; i <= VerticalGridDivisions; i++)
			{
				float t = (float)i / VerticalGridDivisions;
				float time = timeStart + t * (timeEnd - timeStart);
				string label = time.ToString(XAxisLabelFormat);

				float x = chartPos.X + (chartSize.X * i / VerticalGridDivisions);
				float y = chartPos.Y + chartSize.Y + Scale(2);

				// Center the label
				var textSize = UI.Graphics.MeasureText(font, label);
				UI.Graphics.DrawTextColor(font, label, new Vector2(x - textSize.X / 2, y), LabelColor);
			}
		}
	}
}
