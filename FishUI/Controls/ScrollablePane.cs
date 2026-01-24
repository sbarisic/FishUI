using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// A container control with automatic scrollbars for content that exceeds the visible area.
	/// Supports both vertical and horizontal scrolling.
	/// </summary>
	public class ScrollablePane : Control
	{
		/// <summary>
		/// The virtual content size. If larger than the control size, scrollbars appear.
		/// If set to Vector2.Zero, content size is calculated from children bounds.
		/// </summary>
		[YamlMember]
		public Vector2 ContentSize { get; set; } = Vector2.Zero;

		/// <summary>
		/// Whether to automatically calculate content size from children bounds.
		/// </summary>
		[YamlMember]
		public bool AutoContentSize { get; set; } = true;

		/// <summary>
		/// Whether to show the vertical scrollbar when needed.
		/// </summary>
		[YamlMember]
		public bool ShowVerticalScrollBar { get; set; } = true;

		/// <summary>
		/// Whether to show the horizontal scrollbar when needed.
		/// </summary>
		[YamlMember]
		public bool ShowHorizontalScrollBar { get; set; } = true;

		/// <summary>
		/// Width of the vertical scrollbar.
		/// </summary>
		[YamlMember]
		public float ScrollBarWidth { get; set; } = 16f;

		/// <summary>
		/// Height of the horizontal scrollbar.
		/// </summary>
		[YamlMember]
		public float ScrollBarHeight { get; set; } = 16f;

		/// <summary>
		/// Current scroll offset in pixels.
		/// </summary>
		[YamlIgnore]
		public Vector2 ScrollOffset { get; private set; } = Vector2.Zero;

		/// <summary>
		/// Background color of the scrollable area.
		/// </summary>
		[YamlMember]
		public FishColor BackgroundColor { get; set; } = new FishColor(40, 40, 40, 255);

		/// <summary>
		/// Whether to draw a border around the control.
		/// </summary>
		[YamlMember]
		public bool ShowBorder { get; set; } = true;

		/// <summary>
		/// Border color.
		/// </summary>
		[YamlMember]
		public FishColor BorderColor { get; set; } = new FishColor(80, 80, 80, 255);

		[YamlIgnore]
		private ScrollBarV _scrollBarV;

		[YamlIgnore]
		private ScrollBarH _scrollBarH;

		[YamlIgnore]
		private Vector2 _calculatedContentSize = Vector2.Zero;

		[YamlIgnore]
		private bool _scrollBarsCreated = false;

		public ScrollablePane()
		{
			Size = new Vector2(300, 200);
		}

		/// <summary>
		/// Gets the effective content size (either manual or auto-calculated).
		/// </summary>
		private Vector2 GetEffectiveContentSize()
		{
			if (AutoContentSize)
				return _calculatedContentSize;
			return ContentSize;
		}

		/// <summary>
		/// Calculates content size from children bounds.
		/// </summary>
		private void CalculateContentSizeFromChildren()
		{
			if (!AutoContentSize)
				return;

			float maxX = 0;
			float maxY = 0;

			foreach (var child in Children)
			{
				// Skip scrollbars
				if (child == _scrollBarV || child == _scrollBarH)
					continue;

				float childRight = child.Position.X + child.Size.X;
				float childBottom = child.Position.Y + child.Size.Y;

				if (childRight > maxX) maxX = childRight;
				if (childBottom > maxY) maxY = childBottom;
			}

			_calculatedContentSize = new Vector2(maxX, maxY);
		}

		/// <summary>
		/// Creates or updates the scrollbar controls.
		/// </summary>
		private void UpdateScrollBars(FishUI UI)
		{
			Vector2 size = GetAbsoluteSize();
			Vector2 contentSize = GetEffectiveContentSize();

			bool needsVertical = ShowVerticalScrollBar && contentSize.Y > size.Y;
			bool needsHorizontal = ShowHorizontalScrollBar && contentSize.X > size.X;

			// Adjust for scrollbar space
			float availableWidth = size.X - (needsVertical ? ScrollBarWidth : 0);
			float availableHeight = size.Y - (needsHorizontal ? ScrollBarHeight : 0);

			// Recalculate needs after adjusting for scrollbar space
			needsVertical = ShowVerticalScrollBar && contentSize.Y > availableHeight;
			needsHorizontal = ShowHorizontalScrollBar && contentSize.X > availableWidth;

			// Final available space
			availableWidth = size.X - (needsVertical ? ScrollBarWidth : 0);
			availableHeight = size.Y - (needsHorizontal ? ScrollBarHeight : 0);

			// Vertical scrollbar
			if (needsVertical)
			{
				if (_scrollBarV == null)
				{
					_scrollBarV = new ScrollBarV();
					_scrollBarV.OnScrollChanged += (sender, scroll, dir) =>
					{
						UpdateScrollOffset();
					};
					AddChild(_scrollBarV);
				}

				_scrollBarV.Position = new Vector2(availableWidth, 0);
				_scrollBarV.Size = new Vector2(ScrollBarWidth, availableHeight);
				_scrollBarV.Visible = true;

				// Calculate thumb size based on visible/content ratio
				float visibleRatio = availableHeight / contentSize.Y;
				_scrollBarV.ThumbHeight = Math.Clamp(visibleRatio, 0.1f, 1f);
			}
			else if (_scrollBarV != null)
			{
				_scrollBarV.Visible = false;
				_scrollBarV.ThumbPosition = 0;
			}

			// Horizontal scrollbar
			if (needsHorizontal)
			{
				if (_scrollBarH == null)
				{
					_scrollBarH = new ScrollBarH();
					_scrollBarH.OnScrollChanged += (sender, scroll, dir) =>
					{
						UpdateScrollOffset();
					};
					AddChild(_scrollBarH);
				}

				_scrollBarH.Position = new Vector2(0, availableHeight);
				_scrollBarH.Size = new Vector2(availableWidth, ScrollBarHeight);
				_scrollBarH.Visible = true;

				// Calculate thumb size based on visible/content ratio
				float visibleRatio = availableWidth / contentSize.X;
				_scrollBarH.ThumbWidth = Math.Clamp(visibleRatio, 0.1f, 1f);
			}
			else if (_scrollBarH != null)
			{
				_scrollBarH.Visible = false;
				_scrollBarH.ThumbPosition = 0;
			}

			UpdateScrollOffset();
		}

		/// <summary>
		/// Updates the scroll offset based on scrollbar positions.
		/// </summary>
		private void UpdateScrollOffset()
		{
			Vector2 size = GetAbsoluteSize();
			Vector2 contentSize = GetEffectiveContentSize();

			float availableWidth = size.X - (_scrollBarV?.Visible == true ? ScrollBarWidth : 0);
			float availableHeight = size.Y - (_scrollBarH?.Visible == true ? ScrollBarHeight : 0);

			float maxScrollX = Math.Max(0, contentSize.X - availableWidth);
			float maxScrollY = Math.Max(0, contentSize.Y - availableHeight);

			float scrollX = _scrollBarH?.ThumbPosition ?? 0;
			float scrollY = _scrollBarV?.ThumbPosition ?? 0;

			ScrollOffset = new Vector2(scrollX * maxScrollX, scrollY * maxScrollY);
		}

		/// <summary>
		/// Gets the visible area size (excluding scrollbars).
		/// </summary>
		public Vector2 GetVisibleArea()
		{
			Vector2 size = GetAbsoluteSize();
			float width = size.X - (_scrollBarV?.Visible == true ? ScrollBarWidth : 0);
			float height = size.Y - (_scrollBarH?.Visible == true ? ScrollBarHeight : 0);
			return new Vector2(width, height);
		}

		/// <summary>
		/// Scrolls to make a specific position visible.
		/// </summary>
		public void ScrollTo(Vector2 position)
		{
			Vector2 size = GetAbsoluteSize();
			Vector2 contentSize = GetEffectiveContentSize();

			float availableWidth = size.X - (_scrollBarV?.Visible == true ? ScrollBarWidth : 0);
			float availableHeight = size.Y - (_scrollBarH?.Visible == true ? ScrollBarHeight : 0);

			float maxScrollX = Math.Max(0, contentSize.X - availableWidth);
			float maxScrollY = Math.Max(0, contentSize.Y - availableHeight);

			if (maxScrollX > 0 && _scrollBarH != null)
				_scrollBarH.ThumbPosition = Math.Clamp(position.X / maxScrollX, 0, 1);

			if (maxScrollY > 0 && _scrollBarV != null)
				_scrollBarV.ThumbPosition = Math.Clamp(position.Y / maxScrollY, 0, 1);

			UpdateScrollOffset();
		}

		/// <summary>
		/// Scrolls to the top-left corner.
		/// </summary>
		public void ScrollToTop()
		{
			if (_scrollBarV != null)
				_scrollBarV.ThumbPosition = 0;
			if (_scrollBarH != null)
				_scrollBarH.ThumbPosition = 0;
			UpdateScrollOffset();
		}

		/// <summary>
		/// Scrolls to the bottom.
		/// </summary>
		public void ScrollToBottom()
		{
			if (_scrollBarV != null)
				_scrollBarV.ThumbPosition = 1;
			UpdateScrollOffset();
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float WheelDelta)
		{
			// Forward wheel events to vertical scrollbar
			if (_scrollBarV?.Visible == true)
			{
				if (WheelDelta > 0)
					_scrollBarV.ScrollUp();
				else if (WheelDelta < 0)
					_scrollBarV.ScrollDown();
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			CalculateContentSizeFromChildren();
			UpdateScrollBars(UI);

			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();
			Vector2 visibleArea = GetVisibleArea();

			// Draw background
			UI.Graphics.DrawRectangle(absPos, absSize, BackgroundColor);

			// Begin scissor for content area
			UI.Graphics.PushScissor(absPos, visibleArea);

			// Draw children with scroll offset applied
			// Note: Children positions are relative, scroll offset is applied via parent transform
			// The actual offset is handled by child controls checking GetAbsolutePosition()

			UI.Graphics.PopScissor();

			// Draw border
			if (ShowBorder)
			{
				UI.Graphics.DrawRectangleOutline(absPos, absSize, BorderColor);
			}
		}

		/// <summary>
		/// Gets the scroll-adjusted position for child controls.
		/// </summary>
		public Vector2 GetScrollAdjustedChildPosition(Control child)
		{
			Vector2 childRelPos = new Vector2(child.Position.X, child.Position.Y);
			Vector2 basePos = GetAbsolutePosition() + childRelPos;
			return basePos - ScrollOffset;
		}

		/// <summary>
		/// Checks if a child control is visible within the scrollable area.
		/// </summary>
		public bool IsChildVisible(Control child)
		{
			Vector2 childRelPos = new Vector2(child.Position.X, child.Position.Y);
			Vector2 childPos = childRelPos - ScrollOffset;
			Vector2 childSize = child.Size;
			Vector2 visibleArea = GetVisibleArea();

			// Check if child overlaps with visible area
			if (childPos.X + childSize.X < 0) return false;
			if (childPos.Y + childSize.Y < 0) return false;
			if (childPos.X > visibleArea.X) return false;
			if (childPos.Y > visibleArea.Y) return false;

			return true;
		}
	}
}
