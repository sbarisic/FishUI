using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Direction of flow for child elements.
	/// </summary>
	public enum FlowDirection
	{
		/// <summary>
		/// Children flow from left to right, wrapping to the next row.
		/// </summary>
		LeftToRight,

		/// <summary>
		/// Children flow from right to left, wrapping to the next row.
		/// </summary>
		RightToLeft,

		/// <summary>
		/// Children flow from top to bottom, wrapping to the next column.
		/// </summary>
		TopToBottom,

		/// <summary>
		/// Children flow from bottom to top, wrapping to the next column.
		/// </summary>
		BottomToTop
	}

	/// <summary>
	/// Wrapping behavior for flow layout.
	/// </summary>
	public enum FlowWrap
	{
		/// <summary>
		/// Children do not wrap (single line/column).
		/// </summary>
		NoWrap,

		/// <summary>
		/// Children wrap to the next line/column when they exceed container bounds.
		/// </summary>
		Wrap,

		/// <summary>
		/// Children wrap in reverse order.
		/// </summary>
		WrapReverse
	}

	/// <summary>
	/// A layout container that arranges children in a flowing manner, wrapping to new rows/columns as needed.
	/// Similar to CSS flexbox with wrap enabled.
	/// </summary>
	public class FlowLayout : Control
	{
		/// <summary>
		/// The direction of the flow (LeftToRight, RightToLeft, TopToBottom, BottomToTop).
		/// </summary>
		[YamlMember]
		public FlowDirection Direction { get; set; } = FlowDirection.LeftToRight;

		/// <summary>
		/// The wrapping behavior (NoWrap, Wrap, WrapReverse).
		/// </summary>
		[YamlMember]
		public FlowWrap Wrap { get; set; } = FlowWrap.Wrap;

		/// <summary>
		/// Spacing between children along the main axis in pixels.
		/// </summary>
		[YamlMember]
		public float Spacing { get; set; } = 5f;

		/// <summary>
		/// Spacing between rows/columns (cross-axis) when wrapping in pixels.
		/// </summary>
		[YamlMember]
		public float WrapSpacing { get; set; } = 5f;

		/// <summary>
		/// Padding from the edges of the container.
		/// </summary>
		[YamlMember]
		public float Padding { get; set; } = 5f;

		/// <summary>
		/// Whether the flow layout background is transparent (not drawn).
		/// </summary>
		[YamlMember]
		public bool IsTransparent { get; set; } = true;

		public FlowLayout()
		{
			Size = new Vector2(200, 200);
		}

		/// <summary>
		/// Determines if the flow is horizontal (LeftToRight or RightToLeft).
		/// </summary>
		private bool IsHorizontalFlow => Direction == FlowDirection.LeftToRight || Direction == FlowDirection.RightToLeft;

		/// <summary>
		/// Recalculates the positions of all children based on direction, spacing, and wrapping.
		/// Call this after adding/removing children or changing properties.
		/// </summary>
		public void UpdateLayout()
		{
			Vector2 containerSize = GetAbsoluteSize();
			float availableMainAxis = IsHorizontalFlow
				? containerSize.X - Padding * 2
				: containerSize.Y - Padding * 2;

			// Collect visible children
			var visibleChildren = new System.Collections.Generic.List<Control>();
			foreach (var child in Children)
			{
				if (child.Visible)
					visibleChildren.Add(child);
			}

			if (visibleChildren.Count == 0)
				return;

			// Group children into rows/columns
			var lines = new System.Collections.Generic.List<System.Collections.Generic.List<Control>>();
			var currentLine = new System.Collections.Generic.List<Control>();
			float currentLineSize = 0;

			foreach (var child in visibleChildren)
			{
				float childMainSize = IsHorizontalFlow ? child.Size.X : child.Size.Y;

				// Check if child fits in current line
				bool fitsInLine = currentLine.Count == 0 ||
					(Wrap != FlowWrap.NoWrap && currentLineSize + Spacing + childMainSize <= availableMainAxis) ||
					(Wrap == FlowWrap.NoWrap);

				if (!fitsInLine && Wrap != FlowWrap.NoWrap)
				{
					// Start new line
					lines.Add(currentLine);
					currentLine = new System.Collections.Generic.List<Control>();
					currentLineSize = 0;
				}

				currentLine.Add(child);
				currentLineSize += (currentLine.Count > 1 ? Spacing : 0) + childMainSize;
			}

			if (currentLine.Count > 0)
				lines.Add(currentLine);

			// Handle WrapReverse
			if (Wrap == FlowWrap.WrapReverse)
				lines.Reverse();

			// Position children
			float crossAxisPos = Padding;

			foreach (var line in lines)
			{
				float mainAxisPos = Padding;
				float lineCrossSize = 0;

				// Calculate line cross size (max height for horizontal, max width for vertical)
				foreach (var child in line)
				{
					float childCrossSize = IsHorizontalFlow ? child.Size.Y : child.Size.X;
					lineCrossSize = Math.Max(lineCrossSize, childCrossSize);
				}

				// Handle reverse direction
				if (Direction == FlowDirection.RightToLeft)
				{
					mainAxisPos = containerSize.X - Padding;
					line.Reverse();
				}
				else if (Direction == FlowDirection.BottomToTop)
				{
					mainAxisPos = containerSize.Y - Padding;
					line.Reverse();
				}

				foreach (var child in line)
				{
					float childMainSize = IsHorizontalFlow ? child.Size.X : child.Size.Y;

					Vector2 childPos;
					if (IsHorizontalFlow)
					{
						if (Direction == FlowDirection.RightToLeft)
						{
							mainAxisPos -= childMainSize;
							childPos = new Vector2(mainAxisPos, crossAxisPos);
							mainAxisPos -= Spacing;
						}
						else
						{
							childPos = new Vector2(mainAxisPos, crossAxisPos);
							mainAxisPos += childMainSize + Spacing;
						}
					}
					else
					{
						if (Direction == FlowDirection.BottomToTop)
						{
							mainAxisPos -= IsHorizontalFlow ? child.Size.X : child.Size.Y;
							childPos = new Vector2(crossAxisPos, mainAxisPos);
							mainAxisPos -= Spacing;
						}
						else
						{
							childPos = new Vector2(crossAxisPos, mainAxisPos);
							mainAxisPos += childMainSize + Spacing;
						}
					}

					child.Position = new FishUIPosition(PositionMode.Relative, childPos);
				}

				crossAxisPos += lineCrossSize + WrapSpacing;
			}
		}

		/// <summary>
		/// Gets the total content size based on children sizes and spacing.
		/// </summary>
		[YamlIgnore]
		public Vector2 ContentSize
		{
			get
			{
				Vector2 containerSize = Size;
				float availableMainAxis = IsHorizontalFlow
					? containerSize.X - Padding * 2
					: containerSize.Y - Padding * 2;

				// Collect visible children
				var visibleChildren = new System.Collections.Generic.List<Control>();
				foreach (var child in Children)
				{
					if (child.Visible)
						visibleChildren.Add(child);
				}

				if (visibleChildren.Count == 0)
					return new Vector2(Padding * 2, Padding * 2);

				// Calculate lines
				float totalCrossSize = Padding;
				float maxMainSize = 0;
				float currentLineMainSize = 0;
				float currentLineCrossSize = 0;
				bool firstInLine = true;

				foreach (var child in visibleChildren)
				{
					float childMainSize = IsHorizontalFlow ? child.Size.X : child.Size.Y;
					float childCrossSize = IsHorizontalFlow ? child.Size.Y : child.Size.X;

					float proposedSize = currentLineMainSize + (firstInLine ? 0 : Spacing) + childMainSize;

					if (!firstInLine && Wrap != FlowWrap.NoWrap && proposedSize > availableMainAxis)
					{
						// Finish current line
						maxMainSize = Math.Max(maxMainSize, currentLineMainSize);
						totalCrossSize += currentLineCrossSize + WrapSpacing;

						// Start new line
						currentLineMainSize = childMainSize;
						currentLineCrossSize = childCrossSize;
						firstInLine = true;
					}
					else
					{
						currentLineMainSize += (firstInLine ? 0 : Spacing) + childMainSize;
						currentLineCrossSize = Math.Max(currentLineCrossSize, childCrossSize);
						firstInLine = false;
					}
				}

				// Add last line
				maxMainSize = Math.Max(maxMainSize, currentLineMainSize);
				totalCrossSize += currentLineCrossSize + Padding;

				return IsHorizontalFlow
					? new Vector2(maxMainSize + Padding * 2, totalCrossSize)
					: new Vector2(totalCrossSize, maxMainSize + Padding * 2);
			}
		}

		/// <summary>
		/// Resizes the container to fit its content.
		/// </summary>
		public void SizeToContent()
		{
			Size = ContentSize;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			// Update layout before drawing
			UpdateLayout();

			if (!IsTransparent)
			{
				NPatch Cur = UI.Settings.ImgPanel;

				if (Disabled)
					Cur = UI.Settings.ImgPanelDisabled;

				UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);
			}
		}
	}
}
