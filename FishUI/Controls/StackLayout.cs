using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Orientation for stack layout.
	/// </summary>
	public enum StackOrientation
	{
		/// <summary>
		/// Children are stacked vertically from top to bottom.
		/// </summary>
		Vertical,

		/// <summary>
		/// Children are stacked horizontally from left to right.
		/// </summary>
		Horizontal
	}

	/// <summary>
	/// A layout container that arranges children in a vertical or horizontal stack.
	/// Children are automatically positioned based on orientation and spacing.
	/// </summary>
	public class StackLayout : Control
	{
		/// <summary>
		/// The orientation of the stack (Vertical or Horizontal).
		/// </summary>
		[YamlMember]
		public StackOrientation Orientation { get; set; } = StackOrientation.Vertical;

		/// <summary>
		/// Spacing between children in pixels.
		/// </summary>
		[YamlMember]
		public float Spacing { get; set; } = 5f;

		/// <summary>
		/// Padding from the edges of the container.
		/// </summary>
		[YamlMember]
		public float Padding { get; set; } = 5f;

		/// <summary>
		/// Whether the stack layout background is transparent (not drawn).
		/// </summary>
		[YamlMember]
		public bool IsTransparent { get; set; } = true;

		/// <summary>
		/// Whether to automatically resize children to fill the cross-axis.
		/// For Vertical orientation, this stretches children to fill width.
		/// For Horizontal orientation, this stretches children to fill height.
		/// </summary>
		[YamlMember]
		public bool StretchChildren { get; set; } = false;

		public StackLayout()
		{
			Size = new Vector2(200, 200);
		}

		/// <summary>
		/// Recalculates the positions of all children based on orientation and spacing.
		/// Call this after adding/removing children or changing properties.
		/// </summary>
		public void UpdateLayout()
		{
			float currentPos = Padding;
			Vector2 containerSize = GetAbsoluteSize();

			foreach (var child in Children)
			{
				if (!child.Visible)
					continue;

				if (Orientation == StackOrientation.Vertical)
				{
					// Position child vertically
					child.Position = new FishUIPosition(PositionMode.Relative, new Vector2(Padding, currentPos));

					// Optionally stretch to fill width
					if (StretchChildren)
					{
						child.Size = new Vector2(containerSize.X - Padding * 2, child.Size.Y);
					}

					currentPos += child.Size.Y + Spacing;
				}
				else // Horizontal
				{
					// Position child horizontally
					child.Position = new FishUIPosition(PositionMode.Relative, new Vector2(currentPos, Padding));

					// Optionally stretch to fill height
					if (StretchChildren)
					{
						child.Size = new Vector2(child.Size.X, containerSize.Y - Padding * 2);
					}

					currentPos += child.Size.X + Spacing;
				}
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
				float mainAxis = Padding;
				float crossAxis = 0;

				foreach (var child in Children)
				{
					if (!child.Visible)
						continue;

					if (Orientation == StackOrientation.Vertical)
					{
						mainAxis += child.Size.Y + Spacing;
						crossAxis = Math.Max(crossAxis, child.Size.X);
					}
					else
					{
						mainAxis += child.Size.X + Spacing;
						crossAxis = Math.Max(crossAxis, child.Size.Y);
					}
				}

				// Remove last spacing and add end padding
				if (Children.Count > 0)
					mainAxis = mainAxis - Spacing + Padding;
				else
					mainAxis = Padding * 2;

				crossAxis += Padding * 2;

				return Orientation == StackOrientation.Vertical
					? new Vector2(crossAxis, mainAxis)
					: new Vector2(mainAxis, crossAxis);
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
