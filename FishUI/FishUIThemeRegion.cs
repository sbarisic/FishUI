using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI
{
	/// <summary>
	/// Represents a region within a sprite atlas for a control skin.
	/// Contains coordinates and NPatch border values for 9-slice scaling.
	/// </summary>
	public class FishUIThemeRegion
	{
		/// <summary>
		/// X coordinate in the atlas (pixels from left).
		/// </summary>
		public int X { get; set; }

		/// <summary>
		/// Y coordinate in the atlas (pixels from top).
		/// </summary>
		public int Y { get; set; }

		/// <summary>
		/// Width of the region in pixels.
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Height of the region in pixels.
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// NPatch top border (pixels from top edge for 9-slice).
		/// </summary>
		public int Top { get; set; } = 2;

		/// <summary>
		/// NPatch bottom border (pixels from bottom edge for 9-slice).
		/// </summary>
		public int Bottom { get; set; } = 2;

		/// <summary>
		/// NPatch left border (pixels from left edge for 9-slice).
		/// </summary>
		public int Left { get; set; } = 2;

		/// <summary>
		/// NPatch right border (pixels from right edge for 9-slice).
		/// </summary>
		public int Right { get; set; } = 2;

		/// <summary>
		/// Optional path to individual image file (when not using atlas).
		/// </summary>
		public string ImagePath { get; set; }

		/// <summary>
		/// Creates an empty theme region.
		/// </summary>
		public FishUIThemeRegion()
		{
		}

		/// <summary>
		/// Creates a theme region with atlas coordinates.
		/// </summary>
		public FishUIThemeRegion(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Creates a theme region with atlas coordinates and NPatch borders.
		/// </summary>
		public FishUIThemeRegion(int x, int y, int width, int height, int top, int bottom, int left, int right)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
			Top = top;
			Bottom = bottom;
			Left = left;
			Right = right;
		}

		/// <summary>
		/// Creates a theme region from an individual image file path.
		/// </summary>
		public FishUIThemeRegion(string imagePath, int top = 2, int bottom = 2, int left = 2, int right = 2)
		{
			ImagePath = imagePath;
			Top = top;
			Bottom = bottom;
			Left = left;
			Right = right;
		}

		/// <summary>
		/// Gets the position as a Vector2.
		/// </summary>
		public Vector2 GetPosition()
		{
			return new Vector2(X, Y);
		}

		/// <summary>
		/// Gets the size as a Vector2.
		/// </summary>
		public Vector2 GetSize()
		{
			return new Vector2(Width, Height);
		}

		/// <summary>
		/// Returns true if this region uses an individual image file instead of atlas coordinates.
		/// </summary>
		[YamlIgnore]
		public bool UsesImageFile => !string.IsNullOrEmpty(ImagePath);
	}
}
