using System;
using System.Collections.Generic;
using System.Text;

namespace FishUI
{
	public class ImageRef
	{
		public string Path;
		public int Width;
		public int Height;

		public object Userdata;
		public object Userdata2;

		/// <summary>
		/// When true, this ImageRef represents a sub-region of an atlas image.
		/// </summary>
		public bool IsAtlasRegion;

		/// <summary>
		/// Source X coordinate within the atlas (when IsAtlasRegion is true).
		/// </summary>
		public int SourceX;

		/// <summary>
		/// Source Y coordinate within the atlas (when IsAtlasRegion is true).
		/// </summary>
		public int SourceY;

		/// <summary>
		/// Source width within the atlas (when IsAtlasRegion is true).
		/// </summary>
		public int SourceW;

		/// <summary>
		/// Source height within the atlas (when IsAtlasRegion is true).
		/// </summary>
		public int SourceH;

		/// <summary>
		/// Reference to the parent atlas ImageRef (when IsAtlasRegion is true).
		/// </summary>
		public ImageRef AtlasParent;

		/// <summary>
		/// Creates a sub-region ImageRef from an atlas.
		/// </summary>
		public static ImageRef FromAtlasRegion(ImageRef atlas, int x, int y, int width, int height)
		{
			return new ImageRef
			{
				Path = atlas.Path,
				Width = width,
				Height = height,
				Userdata = atlas.Userdata,
				Userdata2 = atlas.Userdata2,
				IsAtlasRegion = true,
				SourceX = x,
				SourceY = y,
				SourceW = width,
				SourceH = height,
				AtlasParent = atlas
			};
		}

		/// <summary>
		/// Creates a sub-region ImageRef from an atlas using a theme region.
		/// </summary>
		public static ImageRef FromAtlasRegion(ImageRef atlas, FishUIThemeRegion region)
		{
			return FromAtlasRegion(atlas, region.X, region.Y, region.Width, region.Height);
		}
	}
}
