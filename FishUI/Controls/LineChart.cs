using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Event arguments for LineChart cursor events.
	/// </summary>
	public class LineChartCursorEventArgs
	{
		/// <summary>
		/// The time value at the cursor position.
		/// </summary>
		public float Time { get; set; }

		/// <summary>
		/// Data values for each series at the cursor time.
		/// Key is the series, value is the interpolated Y value (or null if no data).
		/// </summary>
		public Dictionary<LineChartSeries, float?> Values { get; set; } = new Dictionary<LineChartSeries, float?>();
	}

	/// <summary>
	/// Delegate for LineChart cursor moved events.
	/// </summary>
	public delegate void LineChartCursorMovedFunc(LineChart sender, LineChartCursorEventArgs args);

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

		/// <summary>
		/// Gets the interpolated value at the specified time.
		/// Returns null if no data is available at that time.
		/// </summary>
		/// <param name="time">The time to get the value at.</param>
		/// <returns>The interpolated value, or null if not available.</returns>
		public float? GetValueAt(float time)
		{
			if (Points.Count == 0) return null;
			if (Points.Count == 1) return Points[0].Y;

			// Find the two points surrounding the time
			for (int i = 0; i < Points.Count - 1; i++)
			{
				if (time >= Points[i].X && time <= Points[i + 1].X)
				{
					// Linear interpolation
					float t = (time - Points[i].X) / (Points[i + 1].X - Points[i].X);
					return Points[i].Y + t * (Points[i + 1].Y - Points[i].Y);
				}
			}

			// Time is outside the data range
			if (time < Points[0].X) return null;
			if (time > Points[Points.Count - 1].X) return null;

			return null;
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
		public string XAxisLabelFormat { get; set; } = "F2";

		/// <summary>
		/// Whether the chart is paused. When paused, CurrentTime does not advance
		/// and the chart display is frozen, but data can still be added to series.
		/// </summary>
		[YamlMember]
		public bool IsPaused { get; set; } = false;

		/// <summary>
		/// Data series displayed in this chart.
		/// </summary>
		[YamlIgnore]
		public List<LineChartSeries> Series { get; } = new List<LineChartSeries>();

		// Cursor properties

		/// <summary>
		/// Whether to show the vertical cursor.
		/// </summary>
		[YamlMember]
		public bool ShowCursor { get; set; } = false;

		/// <summary>
		/// The time position of the cursor.
		/// </summary>
		[YamlIgnore]
		public float CursorTime { get; set; } = 0f;

		/// <summary>
		/// Color of the cursor line.
		/// </summary>
		[YamlMember]
		public FishColor CursorColor { get; set; } = new FishColor(255, 100, 100, 200);

		/// <summary>
		/// Size of the data point markers on the cursor.
		/// </summary>
		[YamlMember]
		public float CursorMarkerSize { get; set; } = 6f;

		/// <summary>
		/// Whether the cursor is currently being dragged.
		/// </summary>
		[YamlIgnore]
		public bool IsDraggingCursor { get; private set; } = false;

		/// <summary>
		/// Event fired when the cursor is moved.
		/// </summary>
		public event LineChartCursorMovedFunc OnCursorMoved;



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
		/// Advances the current time if the chart is not paused.
		/// </summary>
		/// <param name="deltaTime">Time elapsed since last update.</param>
		public void Update(float deltaTime)
		{
			if (!IsPaused)
			{
				CurrentTime += deltaTime;
			}
		}

		/// <summary>
		/// Pauses the chart, freezing the display at the current time.
		/// Data can still be added to series while paused.
		/// </summary>
		public void Pause()
		{
			IsPaused = true;
		}

		/// <summary>
		/// Resumes the chart, allowing CurrentTime to advance again.
		/// </summary>
		public void Resume()
		{
			IsPaused = false;
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			if (ShowCursor && Btn == FishMouseButton.Left)
			{
				// Check if click is within chart area
				if (Pos.X >= _chartPos.X && Pos.X <= _chartPos.X + _chartSize.X &&
					Pos.Y >= _chartPos.Y && Pos.Y <= _chartPos.Y + _chartSize.Y)
				{
					IsDraggingCursor = true;
					CursorTime = ScreenXToTime(Pos.X);
					FireCursorMoved();
				}
			}
		}

		public override void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseRelease(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				IsDraggingCursor = false;
			}
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);

			if (ShowCursor && IsDraggingCursor)
			{
				CursorTime = ScreenXToTime(Pos.X);
				FireCursorMoved();
			}
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

			// Store chart area for mouse interaction
			_chartPos = chartPos;
			_chartSize = chartSize;

			// Draw background
			UI.Graphics.DrawRectangle(chartPos, chartSize, BackgroundColor);

			// Draw grid
			DrawGrid(UI, chartPos, chartSize);

			// Draw data series
			DrawSeries(UI, chartPos, chartSize);

			// Draw cursor
			if (ShowCursor)
				DrawCursor(UI, chartPos, chartSize);

			// Draw border
			UI.Graphics.DrawRectangleOutline(chartPos, chartSize, BorderColor);

			// Draw labels
			if (ShowYAxisLabels)
				DrawYAxisLabels(UI, pos, chartSize, labelLeftOffset);

			if (ShowXAxisLabels)
				DrawXAxisLabels(UI, chartPos, chartSize, labelBottomOffset);
		}

		// Cached chart area for mouse interaction
		private Vector2 _chartPos;
		private Vector2 _chartSize;

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

		private void DrawCursor(FishUI UI, Vector2 chartPos, Vector2 chartSize)
		{
			float timeStart = AutoScroll ? CurrentTime - TimeWindow : 0;
			float timeEnd = AutoScroll ? CurrentTime : TimeWindow;

			// Check if cursor is within visible range
			if (CursorTime < timeStart || CursorTime > timeEnd)
				return;

			// Calculate cursor X position
			float normalizedX = (CursorTime - timeStart) / (timeEnd - timeStart);
			float cursorX = chartPos.X + normalizedX * chartSize.X;

			// Draw vertical cursor line
			UI.Graphics.DrawLine(
				new Vector2(cursorX, chartPos.Y),
				new Vector2(cursorX, chartPos.Y + chartSize.Y),
				Scale(2f), CursorColor);

			// Draw data point markers for each series
			float markerSize = Scale(CursorMarkerSize);
			foreach (var series in Series)
			{
				float? value = series.GetValueAt(CursorTime);
				if (value.HasValue)
				{
					float normalizedY = 1f - ((value.Value - MinValue) / (MaxValue - MinValue));
					normalizedY = Math.Clamp(normalizedY, 0f, 1f);

					Vector2 markerPos = new Vector2(
						cursorX - markerSize / 2,
						chartPos.Y + normalizedY * chartSize.Y - markerSize / 2);

					// Draw filled circle as marker (using rectangle for simplicity)
					UI.Graphics.DrawRectangle(markerPos, new Vector2(markerSize, markerSize), series.Color);
				}
			}
		}

		/// <summary>
		/// Converts a screen X position to a time value.
		/// </summary>
		private float ScreenXToTime(float screenX)
		{
			float timeStart = AutoScroll ? CurrentTime - TimeWindow : 0;
			float timeEnd = AutoScroll ? CurrentTime : TimeWindow;

			float normalizedX = (screenX - _chartPos.X) / _chartSize.X;
			normalizedX = Math.Clamp(normalizedX, 0f, 1f);

			return timeStart + normalizedX * (timeEnd - timeStart);
		}

		/// <summary>
		/// Fires the cursor moved event with current cursor data.
		/// </summary>
		private void FireCursorMoved()
		{
			if (OnCursorMoved == null) return;

			var args = new LineChartCursorEventArgs { Time = CursorTime };
			foreach (var series in Series)
			{
				args.Values[series] = series.GetValueAt(CursorTime);
			}
			OnCursorMoved?.Invoke(this, args);
		}
	}
}
