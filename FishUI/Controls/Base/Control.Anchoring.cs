using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public abstract partial class Control
	{
		/// <summary>
		/// Anchor settings for responsive resizing. Determines how this control repositions/resizes
		/// when the parent resizes. Default is TopLeft (fixed position relative to parent origin).
		/// </summary>
		[YamlMember]
		public FishUIAnchor Anchor { get; set; } = FishUIAnchor.TopLeft;

		/// <summary>
		/// Stores the initial distance from the right edge of parent. Used for right anchor calculations.
		/// Set automatically when the control is added to a parent.
		/// </summary>
		[YamlIgnore]
		internal float AnchorRight { get; set; }

		/// <summary>
		/// Stores the initial distance from the bottom edge of parent. Used for bottom anchor calculations.
		/// Set automatically when the control is added to a parent.
		/// </summary>
		[YamlIgnore]
		internal float AnchorBottom { get; set; }

		/// <summary>
		/// Stores the initial parent size when anchor offsets were calculated.
		/// Serialized so anchor adjustments work correctly after layout reload.
		/// </summary>
		[YamlMember]
		public Vector2 AnchorParentSize { get; set; }

		/// <summary>
		/// Updates the anchor offset values for a child control based on current parent size.
		/// </summary>
		private void UpdateChildAnchorOffsets(Control Child)
		{
			Vector2 parentSize = GetAbsoluteSize();

			// Only set AnchorParentSize if not already set (e.g., from deserialization)
			// This preserves the original parent size so anchor adjustments work correctly after reload
			if (Child.AnchorParentSize == Vector2.Zero)
			{
				Child.AnchorParentSize = parentSize;
			}

			// Calculate distances from right and bottom edges
			float childRight = Child.Position.X + Child.Size.X;
			float childBottom = Child.Position.Y + Child.Size.Y;

			Child.AnchorRight = parentSize.X - childRight;
			Child.AnchorBottom = parentSize.Y - childBottom;
		}

		/// <summary>
		/// Recalculates anchor offsets for all children. Call after parent is resized.
		/// </summary>
		public void RecalculateChildAnchors()
		{
			foreach (var child in Children)
			{
				UpdateChildAnchorOffsets(child);
			}
		}

		/// <summary>
		/// Updates this control's anchor offsets based on its current position and parent size.
		/// Call after manually changing a control's position (e.g., in an editor) to keep anchoring in sync.
		/// </summary>
		public void UpdateOwnAnchorOffsets()
		{
			Control parent = GetParent();
			if (parent == null)
				return;

			Vector2 parentSize = parent.GetAbsoluteSize();
			AnchorParentSize = parentSize;

			// Calculate distances from right and bottom edges
			float childRight = Position.X + Size.X;
			float childBottom = Position.Y + Size.Y;

			AnchorRight = parentSize.X - childRight;
			AnchorBottom = parentSize.Y - childBottom;
		}

		/// <summary>
		/// Gets this control's position with anchor adjustments applied.
		/// Returns the relative position (to parent) adjusted for anchor settings based on parent size changes.
		/// </summary>
		/// <returns>The anchor-adjusted relative position.</returns>
		public Vector2 GetAnchorAdjustedRelativePosition()
		{
			Vector2 pos = new Vector2(Position.X, Position.Y);

			// Apply anchor adjustments if this control has a parent and uses anchoring
			// Skip if AnchorParentSize is zero (not yet initialized, e.g., during deserialization)
			Control parent = GetParent();
			if (parent != null && Anchor != FishUIAnchor.None && Anchor != FishUIAnchor.TopLeft && AnchorParentSize != Vector2.Zero)
			{
				Vector2 currentParentSize = parent.Size;
				Vector2 sizeDelta = currentParentSize - AnchorParentSize;

				// Right anchor: adjust X position based on parent width change
				if (Anchor.HasFlag(FishUIAnchor.Right) && !Anchor.HasFlag(FishUIAnchor.Left))
				{
					pos.X += sizeDelta.X;
				}

				// Bottom anchor: adjust Y position based on parent height change
				if (Anchor.HasFlag(FishUIAnchor.Bottom) && !Anchor.HasFlag(FishUIAnchor.Top))
				{
					pos.Y += sizeDelta.Y;
				}
			}

			return pos;
		}

		/// <summary>
		/// Gets this control's size with anchor stretching applied.
		/// Returns the size adjusted for anchor settings when anchored to opposite edges (stretch behavior).
		/// </summary>
		/// <returns>The anchor-adjusted size.</returns>
		public Vector2 GetAnchorAdjustedSize()
		{
			Vector2 size = Size;

			// Apply anchor stretching if this control has a parent and uses stretching anchors
			// Skip if AnchorParentSize is zero (not yet initialized, e.g., during deserialization)
			Control parent = GetParent();
			if (parent != null && Anchor != FishUIAnchor.None && Anchor != FishUIAnchor.TopLeft && AnchorParentSize != Vector2.Zero)
			{
				Vector2 currentParentSize = parent.Size;
				Vector2 sizeDelta = currentParentSize - AnchorParentSize;

				// Horizontal stretching: anchored to both left and right
				if (Anchor.HasFlag(FishUIAnchor.Left) && Anchor.HasFlag(FishUIAnchor.Right))
				{
					size.X += sizeDelta.X;
				}

				// Vertical stretching: anchored to both top and bottom
				if (Anchor.HasFlag(FishUIAnchor.Top) && Anchor.HasFlag(FishUIAnchor.Bottom))
				{
					size.Y += sizeDelta.Y;
				}
			}

			return size;
		}

		/// <summary>
		/// Draws yellow lines to visualize anchor connections to parent control.
		/// </summary>
		protected void DrawAnchorVisualization(FishUI UI)
		{
			Control parent = GetParent();
			if (parent == null || Anchor == FishUIAnchor.None || Anchor == FishUIAnchor.TopLeft)
				return;

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();
			Vector2 parentPos = parent.GetAbsolutePosition();
			Vector2 parentSize = parent.GetAbsoluteSize();

			FishColor anchorColor = new FishColor(255, 200, 0, 180); // Yellow
			float lineThickness = 1.5f;

			// Draw line from control's left edge to parent's left edge
			if (Anchor.HasFlag(FishUIAnchor.Left))
			{
				float midY = pos.Y + size.Y / 2;
				UI.Graphics.DrawLine(new Vector2(parentPos.X, midY), new Vector2(pos.X, midY), lineThickness, anchorColor);
			}

			// Draw line from control's right edge to parent's right edge
			if (Anchor.HasFlag(FishUIAnchor.Right))
			{
				float midY = pos.Y + size.Y / 2;
				UI.Graphics.DrawLine(new Vector2(pos.X + size.X, midY), new Vector2(parentPos.X + parentSize.X, midY), lineThickness, anchorColor);
			}

			// Draw line from control's top edge to parent's top edge
			if (Anchor.HasFlag(FishUIAnchor.Top))
			{
				float midX = pos.X + size.X / 2;
				UI.Graphics.DrawLine(new Vector2(midX, parentPos.Y), new Vector2(midX, pos.Y), lineThickness, anchorColor);
			}

			// Draw line from control's bottom edge to parent's bottom edge
			if (Anchor.HasFlag(FishUIAnchor.Bottom))
			{
				float midX = pos.X + size.X / 2;
				UI.Graphics.DrawLine(new Vector2(midX, pos.Y + size.Y), new Vector2(midX, parentPos.Y + parentSize.Y), lineThickness, anchorColor);
			}
		}
	}
}
