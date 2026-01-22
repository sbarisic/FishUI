using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FishUI
{
	public class NPatch
	{
		public int Left;
		public int Right;
		public int Top;
		public int Bottom;

		public ImageRef Image;
		public Vector2 ImagePos;
		public Vector2 ImageSize;

		/// <summary>
		/// Creates an NPatch from an image file.
		/// </summary>
		public NPatch(FishUI UI, string FileName, int Top, int Bottom, int Left, int Right)
		{
			Image = UI.Graphics.LoadImage(FileName);
			this.Top = Top;
			this.Bottom = Bottom;
			this.Left = Left;
			this.Right = Right;

			ImagePos = Vector2.Zero;
			ImageSize = new Vector2(Image.Width, Image.Height);
		}

		/// <summary>
		/// Creates an NPatch from a sprite atlas using a theme region.
		/// </summary>
		public NPatch(ImageRef atlasImage, FishUIThemeRegion region)
		{
			Image = atlasImage;
			this.Top = region.Top;
			this.Bottom = region.Bottom;
			this.Left = region.Left;
			this.Right = region.Right;

			ImagePos = region.GetPosition();
			ImageSize = region.GetSize();
		}

		/// <summary>
		/// Creates an NPatch from a sprite atlas with explicit coordinates.
		/// </summary>
		public NPatch(ImageRef atlasImage, int x, int y, int width, int height, int top, int bottom, int left, int right)
		{
			Image = atlasImage;
			this.Top = top;
			this.Bottom = bottom;
			this.Left = left;
			this.Right = right;

			ImagePos = new Vector2(x, y);
			ImageSize = new Vector2(width, height);
		}
	}
}
