using System;

namespace FishUI
{
	/// <summary>
	/// Anchor flags that determine how a control is positioned relative to its parent when the parent resizes.
	/// Use combinations to anchor to multiple edges. When anchored to opposite edges, the control stretches.
	/// </summary>
	[Flags]
	public enum FishUIAnchor
	{
		/// <summary>
		/// No anchoring - control position is fixed.
		/// </summary>
		None = 0,

		/// <summary>
		/// Anchor to the left edge of the parent.
		/// </summary>
		Left = 1,

		/// <summary>
		/// Anchor to the top edge of the parent.
		/// </summary>
		Top = 2,

		/// <summary>
		/// Anchor to the right edge of the parent.
		/// </summary>
		Right = 4,

		/// <summary>
		/// Anchor to the bottom edge of the parent.
		/// </summary>
		Bottom = 8,

		/// <summary>
		/// Anchor to top-left (default behavior, position fixed relative to parent origin).
		/// </summary>
		TopLeft = Top | Left,

		/// <summary>
		/// Anchor to top-right (control moves when parent width changes).
		/// </summary>
		TopRight = Top | Right,

		/// <summary>
		/// Anchor to bottom-left (control moves when parent height changes).
		/// </summary>
		BottomLeft = Bottom | Left,

		/// <summary>
		/// Anchor to bottom-right (control moves when parent size changes).
		/// </summary>
		BottomRight = Bottom | Right,

		/// <summary>
		/// Anchor to all edges (control stretches with parent).
		/// </summary>
		All = Left | Top | Right | Bottom,

		/// <summary>
		/// Anchor horizontally (left and right) - control stretches horizontally.
		/// </summary>
		Horizontal = Left | Right,

		/// <summary>
		/// Anchor vertically (top and bottom) - control stretches vertically.
		/// </summary>
		Vertical = Top | Bottom
	}
}
