using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public abstract partial class Control
	{
		/// <summary>
		/// Specifies which dimensions of this control should auto-size based on content.
		/// Default is None (manual sizing).
		/// </summary>
		[YamlMember]
		public virtual AutoSizeMode AutoSize { get; set; } = AutoSizeMode.None;

		/// <summary>
		/// Minimum size constraint for auto-sizing. Set to Vector2.Zero to disable.
		/// </summary>
		[YamlMember]
		public virtual Vector2 MinSize { get; set; } = Vector2.Zero;

		/// <summary>
		/// Maximum size constraint for auto-sizing. Set to Vector2.Zero to disable (unlimited).
		/// </summary>
		[YamlMember]
		public virtual Vector2 MaxSize { get; set; } = Vector2.Zero;

		/// <summary>
		/// Extra padding added to the preferred size when auto-sizing.
		/// Useful for adding breathing room around text content.
		/// </summary>
		[YamlMember]
		public virtual Vector2 AutoSizePadding { get; set; } = Vector2.Zero;

		/// <summary>
		/// Gets the preferred size of this control based on its content.
		/// Override in derived classes to provide content-based sizing.
		/// </summary>
		/// <param name="UI">The FishUI instance for measuring text, etc.</param>
		/// <returns>The preferred size in pixels, or Vector2.Zero if not applicable.</returns>
		public virtual Vector2 GetPreferredSize(FishUI UI)
		{
			// Base implementation returns current size (no auto-sizing)
			return Size;
		}

		/// <summary>
		/// Updates the control's size based on AutoSize mode and preferred size.
		/// Call this before drawing or after content changes.
		/// </summary>
		/// <param name="UI">The FishUI instance for measuring text, etc.</param>
		public void UpdateAutoSize(FishUI UI)
		{
			if (AutoSize == AutoSizeMode.None)
				return;

			Vector2 preferred = GetPreferredSize(UI) + AutoSizePadding;

			// Apply min/max constraints
			if (MinSize.X > 0) preferred.X = Math.Max(preferred.X, MinSize.X);
			if (MinSize.Y > 0) preferred.Y = Math.Max(preferred.Y, MinSize.Y);
			if (MaxSize.X > 0) preferred.X = Math.Min(preferred.X, MaxSize.X);
			if (MaxSize.Y > 0) preferred.Y = Math.Min(preferred.Y, MaxSize.Y);

			// Apply based on mode
			switch (AutoSize)
			{
				case AutoSizeMode.Width:
					Size.X = preferred.X;
					break;
				case AutoSizeMode.Height:
					Size.Y = preferred.Y;
					break;
				case AutoSizeMode.Both:
					Size = preferred;
					break;
			}
		}
	}
}
