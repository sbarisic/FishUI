using System.Numerics;

namespace FishUI.Controls
{
	public abstract partial class Control
	{
		/// <summary>
		/// Gets an additional position offset to apply to child controls.
		/// Override in container controls like ScrollablePane to implement scrolling.
		/// </summary>
		/// <param name="child">The child control requesting the offset.</param>
		/// <returns>The offset to apply to the child's position.</returns>
		public virtual Vector2 GetChildPositionOffset(Control child)
		{
			return Vector2.Zero;
		}

		/// <summary>
		/// Gets the absolute position of this control in screen coordinates.
		/// Accounts for parent Padding, this control's Margin, Anchor settings, and UI scaling.
		/// </summary>
		/// <returns>The absolute position in pixels (scaled by UIScale).</returns>
		public Vector2 GetAbsolutePosition()
		{
			// Calculate parent padding offset (scaled)
			Vector2 parentPaddingOffset = Vector2.Zero;
			Vector2 parentPos = Vector2.Zero;
			Vector2 parentSize = Vector2.Zero;

			if (Parent != null)
			{
				parentPaddingOffset = new Vector2(Scale(Parent.Padding.Left), Scale(Parent.Padding.Top));
				parentPos = Parent.GetAbsolutePosition();
				parentSize = Parent.GetAbsoluteSize();
			}
			else if (FishUI != null)
			{
				parentSize = new Vector2(FishUI.Width, FishUI.Height);
			}

			// This control's margin offset (scaled)
			Vector2 marginOffset = new Vector2(Scale(Margin.Left), Scale(Margin.Top));

			// Calculate base position
			Vector2 basePos;
			if (Position.Mode == PositionMode.Absolute)
			{
				basePos = new Vector2(Scale(Position.X), Scale(Position.Y)) + marginOffset;
			}
			else if (Position.Mode == PositionMode.Relative)
			{
				basePos = parentPos + parentPaddingOffset + new Vector2(Scale(Position.X), Scale(Position.Y)) + marginOffset;
			}
			else if (Position.Mode == PositionMode.Docked)
			{
				Vector2 DockedPos = parentPos + parentPaddingOffset + new Vector2(Scale(Position.X), Scale(Position.Y)) + marginOffset;

				if (Position.Dock.HasFlag(DockMode.Left))
				{
					DockedPos.X = parentPos.X + parentPaddingOffset.X + Scale(Position.Left) + Scale(Margin.Left);
				}

				if (Position.Dock.HasFlag(DockMode.Top))
				{
					DockedPos.Y = parentPos.Y + parentPaddingOffset.Y + Scale(Position.Top) + Scale(Margin.Top);
				}

				basePos = DockedPos;
			}
			else
			{
				// Fallback for unknown position modes - treat as absolute position
				basePos = new Vector2(Scale(Position.X), Scale(Position.Y)) + marginOffset;
			}

			// Apply anchor adjustments if parent exists and anchored to right or bottom
			// Skip if AnchorParentSize is zero (not yet initialized, e.g., during deserialization)
			if (Parent != null && Anchor != FishUIAnchor.None && Anchor != FishUIAnchor.TopLeft && AnchorParentSize != Vector2.Zero)
			{
				Vector2 sizeDelta = parentSize - Scale(AnchorParentSize);

				// Right anchor: adjust X position based on parent width change
				if (Anchor.HasFlag(FishUIAnchor.Right) && !Anchor.HasFlag(FishUIAnchor.Left))
				{
					// Only right anchor - move with right edge
					basePos.X += sizeDelta.X;
				}

				// Bottom anchor: adjust Y position based on parent height change
				if (Anchor.HasFlag(FishUIAnchor.Bottom) && !Anchor.HasFlag(FishUIAnchor.Top))
				{
					// Only bottom anchor - move with bottom edge
					basePos.Y += sizeDelta.Y;
				}
			}

			// Apply parent's child position offset (used for scrolling containers)
			if (Parent != null)
			{
				basePos += Parent.GetChildPositionOffset(this);
			}

			return basePos;
		}

		/// <summary>
		/// Converts a global point to local coordinates relative to this control.
		/// </summary>
		/// <param name="GlobalPt">Point in screen coordinates.</param>
		/// <returns>Point in local coordinates relative to this control's position.</returns>
		public Vector2 GetLocalRelative(Vector2 GlobalPt)
		{
			return GlobalPt - GetAbsolutePosition();
		}

		/// <summary>
		/// Gets the absolute size of this control, accounting for docked positioning, margins, anchor stretching, and UI scaling.
		/// </summary>
		/// <returns>The actual size in pixels (scaled by UIScale).</returns>
		public Vector2 GetAbsoluteSize()
		{
			// Apply UI scaling to the base size
			Vector2 resultSize = Scale(Size);

			// Handle docked positioning
			if (Position.Mode == PositionMode.Docked)
			{
				Vector2 ParentPos;
				Vector2 ParentSize;
				FishUIMargin parentPadding = new FishUIMargin();

				if (Parent != null)
				{
					ParentPos = Parent.GetAbsolutePosition();
					ParentSize = Parent.GetAbsoluteSize();
					parentPadding = Parent.Padding;
				}
				else if (FishUI != null)
				{
					// No parent - dock to screen bounds using FishUI dimensions
					ParentPos = Vector2.Zero;
					ParentSize = new Vector2(FishUI.Width, FishUI.Height);
				}
				else
				{
					// No parent and no FishUI - return scaled size
					return Scale(Size);
				}

				Vector2 MyPos = GetAbsolutePosition();

				float SubX = MyPos.X - ParentPos.X;
				float SubY = MyPos.Y - ParentPos.Y;

				if (Position.Dock.HasFlag(DockMode.Right))
				{
					float FullChildWidth = ParentSize.X - SubX;
					// Account for parent right padding and this control's right margin (scaled)
					resultSize.X = FullChildWidth - Scale(Position.Right) - Scale(parentPadding.Right) - Scale(Margin.Right);
				}

				if (Position.Dock.HasFlag(DockMode.Bottom))
				{
					float FullChildHeight = ParentSize.Y - SubY;
					// Account for parent bottom padding and this control's bottom margin (scaled)
					resultSize.Y = FullChildHeight - Scale(Position.Bottom) - Scale(parentPadding.Bottom) - Scale(Margin.Bottom);
				}
			}

			// Handle anchor stretching (when anchored to both left+right or top+bottom)
			// Skip if AnchorParentSize is zero (not yet initialized, e.g., during deserialization)
			if (Parent != null && Anchor != FishUIAnchor.None && Anchor != FishUIAnchor.TopLeft && AnchorParentSize != Vector2.Zero)
			{
				Vector2 parentSize = Parent.GetAbsoluteSize();
				Vector2 sizeDelta = parentSize - Scale(AnchorParentSize);

				// Horizontal stretching: anchored to both left and right
				if (Anchor.HasFlag(FishUIAnchor.Left) && Anchor.HasFlag(FishUIAnchor.Right))
				{
					resultSize.X = Scale(Size.X) + sizeDelta.X;
				}

				// Vertical stretching: anchored to both top and bottom
				if (Anchor.HasFlag(FishUIAnchor.Top) && Anchor.HasFlag(FishUIAnchor.Bottom))
				{
					resultSize.Y = Scale(Size.Y) + sizeDelta.Y;
				}
			}

			return resultSize;
		}

		/// <summary>
		/// Checks if a point in screen coordinates is inside this control.
		/// </summary>
		/// <param name="GlobalPt">Point in screen coordinates.</param>
		/// <returns>True if the point is inside the control bounds.</returns>
		public virtual bool IsPointInside(Vector2 GlobalPt)
		{
			Vector2 AbsPos = GetAbsolutePosition();
			Vector2 AbsSize = GetAbsoluteSize();
			return Utils.IsInside(AbsPos, AbsSize, GlobalPt);
		}
	}
}
