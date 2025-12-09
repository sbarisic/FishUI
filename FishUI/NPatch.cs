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
	}
}
