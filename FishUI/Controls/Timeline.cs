using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Event arguments for Timeline view changed events.
	/// </summary>
	public class TimelineViewChangedEventArgs
	{
		/// <summary>
		/// The start time of the visible window.
		/// </summary>
		public float ViewStart { get; set; }

		/// <summary>
		/// The end time of the visible window.
		/// </summary>
		public float ViewEnd { get; set; }

		/// <summary>
		/// The width of the visible window (ViewEnd - ViewStart).
		/// </summary>
		public float ViewWidth => ViewEnd - ViewStart;
	}

	/// <summary>
	/// Delegate for Timeline view changed events.
	/// </summary>
	public delegate void TimelineViewChangedFunc(Timeline sender, TimelineViewChangedEventArgs args);

	/// <summary>
	/// A timeline control for navigating time-based data.
	/// Displays a time range with a selectable view window.
	/// Designed to work as a companion to LineChart for historical data browsing.
	/// </summary>
	public class Timeline : Control
	{
		/// <summary>
		/// Minimum time value of the entire timeline range.
		/// </summary>
		[YamlMember]
		public float MinTime { get; set; } = 0f;

		/// <summary>
		/// Maximum time value of the entire timeline range.
		/// </summary>
		[YamlMember]
		public float MaxTime { get; set; } = 100f;

		/// <summary>
		/// Start time of the currently visible window.
		/// </summary>
		[YamlIgnore]
		public float ViewStart
		{
			get => _viewStart;
			set
			{
				float newValue = Math.Clamp(value, MinTime, MaxTime - MinViewWidth);
				if (_viewStart != newValue)
				{
					_viewStart = newValue;
					FireViewChanged();
				}
			}
		}
		private float _viewStart = 0f;

		/// <summary>
		/// End time of the currently visible window.
		/// </summary>
		[YamlIgnore]
		public float ViewEnd
		{
			get => _viewEnd;
			set
			{
				float newValue = Math.Clamp(value, MinTime + MinViewWidth, MaxTime);
				if (_viewEnd != newValue)
				{
					_viewEnd = newValue;
					FireViewChanged();
				}
			}
		}
		private float _viewEnd = 10f;

		/// <summary>
		/// Minimum width of the view window.
		/// </summary>
		[YamlMember]
		public float MinViewWidth { get; set; } = 1f;

		/// <summary>
		/// Background color of the timeline.
		/// </summary>
		[YamlMember]
		public FishColor BackgroundColor { get; set; } = new FishColor(40, 40, 40, 255);

		/// <summary>
		/// Color of the timeline track.
		/// </summary>
		[YamlMember]
		public FishColor TrackColor { get; set; } = new FishColor(60, 60, 60, 255);

		/// <summary>
		/// Color of the view window outline.
		/// </summary>
		[YamlMember]
		public FishColor ViewWindowColor { get; set; } = new FishColor(80, 150, 255, 255);

		/// <summary>
		/// Color of the view window fill.
		/// </summary>
		[YamlMember]
		public FishColor ViewWindowFillColor { get; set; } = new FishColor(80, 150, 255, 50);

		/// <summary>
		/// Color of the tick marks.
		/// </summary>
		[YamlMember]
		public FishColor TickColor { get; set; } = new FishColor(100, 100, 100, 255);

		/// <summary>
		/// Color of the time labels.
		/// </summary>
		[YamlMember]
		public FishColor LabelColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Number of major tick divisions.
		/// </summary>
		[YamlMember]
		public int MajorTickCount { get; set; } = 10;

		/// <summary>
		/// Format string for time labels.
		/// </summary>
		[YamlMember]
		public string LabelFormat { get; set; } = "F1";

		/// <summary>
		/// Whether to show time labels.
		/// </summary>
		[YamlMember]
		public bool ShowLabels { get; set; } = true;

		/// <summary>
		/// Height of the label area in pixels.
		/// </summary>
		[YamlMember]
		public float LabelHeight { get; set; } = 16f;

		/// <summary>
		/// Width of the resize handles on the view window edges.
		/// </summary>
		[YamlMember]
		public float HandleWidth { get; set; } = 8f;

		/// <summary>
		/// Event fired when the view window changes.
		/// </summary>
		public event TimelineViewChangedFunc OnViewChanged;

		// Dragging state
		private enum DragMode { None, PanWindow, ResizeLeft, ResizeRight }
		private DragMode _dragMode = DragMode.None;
		private float _dragStartMouseX;
		private float _dragStartViewStart;
		private float _dragStartViewEnd;

		// Cached positions
		private Vector2 _trackPos;
		private Vector2 _trackSize;

		public Timeline()
		{
			Size = new Vector2(400, 40);
		}

		/// <summary>
		/// Sets the view window to the specified range.
		/// </summary>
		public void SetView(float start, float end)
		{
			_viewStart = Math.Clamp(start, MinTime, MaxTime - MinViewWidth);
			_viewEnd = Math.Clamp(end, _viewStart + MinViewWidth, MaxTime);
			FireViewChanged();
		}

		/// <summary>
		/// Sets the view window to show the most recent data (right edge at MaxTime).
		/// </summary>
		public void SetViewToEnd(float windowWidth)
		{
			_viewEnd = MaxTime;
			_viewStart = Math.Max(MinTime, MaxTime - windowWidth);
			FireViewChanged();
		}

		/// <summary>
		/// Syncs the timeline with a LineChart's current view.
		/// </summary>
		public void SyncFromLineChart(LineChart chart)
		{
			if (chart.AutoScroll)
			{
				_viewEnd = chart.CurrentTime;
				_viewStart = chart.CurrentTime - chart.TimeWindow;
			}
			else
			{
				_viewStart = 0;
				_viewEnd = chart.TimeWindow;
			}
		}

		/// <summary>
		/// Syncs a LineChart to match this timeline's view.
		/// </summary>
		public void SyncToLineChart(LineChart chart)
		{
			chart.TimeWindow = _viewEnd - _viewStart;
			if (chart.AutoScroll)
			{
				// Can't directly set CurrentTime in auto-scroll mode
				// The chart will need to catch up
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			base.DrawControl(UI, Dt, Time);

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = ScaledSize;

			// Calculate track area
			float labelOffset = ShowLabels ? Scale(LabelHeight) : 0;
			_trackPos = new Vector2(pos.X, pos.Y);
			_trackSize = new Vector2(size.X, size.Y - labelOffset);

			// Draw background
			UI.Graphics.DrawRectangle(pos, size, BackgroundColor);

			// Draw track
			UI.Graphics.DrawRectangle(_trackPos, _trackSize, TrackColor);

			// Draw tick marks
			DrawTicks(UI);

			// Draw view window
			DrawViewWindow(UI);

			// Draw labels
			if (ShowLabels)
				DrawLabels(UI, pos, size, labelOffset);

			// Draw border
			UI.Graphics.DrawRectangleOutline(pos, size, new FishColor(80, 80, 80, 255));
		}

		private void DrawTicks(FishUI UI)
		{
			if (MajorTickCount <= 0) return;

			float timeRange = MaxTime - MinTime;
			if (timeRange <= 0) return;

			for (int i = 0; i <= MajorTickCount; i++)
			{
				float t = (float)i / MajorTickCount;
				float x = _trackPos.X + t * _trackSize.X;

				UI.Graphics.DrawLine(
					new Vector2(x, _trackPos.Y),
					new Vector2(x, _trackPos.Y + _trackSize.Y * 0.3f),
					1f, TickColor);
			}
		}

		private void DrawViewWindow(FishUI UI)
		{
			float timeRange = MaxTime - MinTime;
			if (timeRange <= 0) return;

			// Calculate window position in pixels
			float startNorm = (_viewStart - MinTime) / timeRange;
			float endNorm = (_viewEnd - MinTime) / timeRange;

			float windowX = _trackPos.X + startNorm * _trackSize.X;
			float windowWidth = (endNorm - startNorm) * _trackSize.X;

			Vector2 windowPos = new Vector2(windowX, _trackPos.Y);
			Vector2 windowSize = new Vector2(windowWidth, _trackSize.Y);

			// Draw fill
			UI.Graphics.DrawRectangle(windowPos, windowSize, ViewWindowFillColor);

			// Draw outline
			UI.Graphics.DrawRectangleOutline(windowPos, windowSize, ViewWindowColor);

			// Draw resize handles (thicker lines on edges)
			float handleW = Scale(HandleWidth);
			UI.Graphics.DrawRectangle(windowPos, new Vector2(Math.Min(handleW, windowWidth / 3), windowSize.Y),
				new FishColor(ViewWindowColor.R, ViewWindowColor.G, ViewWindowColor.B, 100));
			UI.Graphics.DrawRectangle(new Vector2(windowPos.X + windowWidth - Math.Min(handleW, windowWidth / 3), windowPos.Y),
				new Vector2(Math.Min(handleW, windowWidth / 3), windowSize.Y),
				new FishColor(ViewWindowColor.R, ViewWindowColor.G, ViewWindowColor.B, 100));
		}

		private void DrawLabels(FishUI UI, Vector2 pos, Vector2 size, float labelHeight)
		{
			var font = UI.Settings.FontDefault;
			if (font == null) return;

			float labelY = pos.Y + size.Y - labelHeight + Scale(2);

			for (int i = 0; i <= MajorTickCount; i++)
			{
				float t = (float)i / MajorTickCount;
				float time = MinTime + t * (MaxTime - MinTime);
				string label = time.ToString(LabelFormat);

				float x = pos.X + t * size.X;
				var textSize = UI.Graphics.MeasureText(font, label);

				// Center label, but clamp to edges
				float labelX = x - textSize.X / 2;
				if (i == 0) labelX = Math.Max(pos.X, labelX);
				if (i == MajorTickCount) labelX = Math.Min(pos.X + size.X - textSize.X, labelX);

				UI.Graphics.DrawTextColor(font, label, new Vector2(labelX, labelY), LabelColor);
			}
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			if (Btn != FishMouseButton.Left) return;

			float timeRange = MaxTime - MinTime;
			if (timeRange <= 0) return;

			// Calculate window position in pixels
			float startNorm = (_viewStart - MinTime) / timeRange;
			float endNorm = (_viewEnd - MinTime) / timeRange;

			float windowX = _trackPos.X + startNorm * _trackSize.X;
			float windowWidth = (endNorm - startNorm) * _trackSize.X;
			float handleW = Scale(HandleWidth);

			// Check if clicking on left handle
			if (Pos.X >= windowX && Pos.X <= windowX + handleW)
			{
				_dragMode = DragMode.ResizeLeft;
			}
			// Check if clicking on right handle
			else if (Pos.X >= windowX + windowWidth - handleW && Pos.X <= windowX + windowWidth)
			{
				_dragMode = DragMode.ResizeRight;
			}
			// Check if clicking inside window (pan)
			else if (Pos.X >= windowX && Pos.X <= windowX + windowWidth)
			{
				_dragMode = DragMode.PanWindow;
			}
			// Clicking outside window - jump to position
			else if (Pos.X >= _trackPos.X && Pos.X <= _trackPos.X + _trackSize.X)
			{
				float clickTime = ScreenXToTime(Pos.X);
				float windowHalfWidth = (_viewEnd - _viewStart) / 2;
				SetView(clickTime - windowHalfWidth, clickTime + windowHalfWidth);
				_dragMode = DragMode.PanWindow;
			}

			_dragStartMouseX = Pos.X;
			_dragStartViewStart = _viewStart;
			_dragStartViewEnd = _viewEnd;
		}

		public override void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseRelease(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				_dragMode = DragMode.None;
			}
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);

			if (_dragMode == DragMode.None) return;

			float deltaX = Pos.X - _dragStartMouseX;
			float timeRange = MaxTime - MinTime;
			float deltaTime = (deltaX / _trackSize.X) * timeRange;

			switch (_dragMode)
			{
				case DragMode.PanWindow:
					float windowWidth = _dragStartViewEnd - _dragStartViewStart;
					float newStart = _dragStartViewStart + deltaTime;
					newStart = Math.Clamp(newStart, MinTime, MaxTime - windowWidth);
					_viewStart = newStart;
					_viewEnd = newStart + windowWidth;
					FireViewChanged();
					break;

				case DragMode.ResizeLeft:
					float newLeft = _dragStartViewStart + deltaTime;
					newLeft = Math.Clamp(newLeft, MinTime, _viewEnd - MinViewWidth);
					_viewStart = newLeft;
					FireViewChanged();
					break;

				case DragMode.ResizeRight:
					float newRight = _dragStartViewEnd + deltaTime;
					newRight = Math.Clamp(newRight, _viewStart + MinViewWidth, MaxTime);
					_viewEnd = newRight;
					FireViewChanged();
					break;
			}
		}

		private float ScreenXToTime(float screenX)
		{
			float norm = (screenX - _trackPos.X) / _trackSize.X;
			norm = Math.Clamp(norm, 0f, 1f);
			return MinTime + norm * (MaxTime - MinTime);
		}

		private void FireViewChanged()
		{
			OnViewChanged?.Invoke(this, new TimelineViewChangedEventArgs
			{
				ViewStart = _viewStart,
				ViewEnd = _viewEnd
			});
		}
	}
}
