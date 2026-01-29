using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace FishUI
{
	/// <summary>
	/// Interface for the graphics backend. Implement this to use FishUI with different rendering libraries.
	/// </summary>
	public abstract class SimpleFishUIGfx : IFishUIGfx
	{
		public virtual void BeginDrawing(float Dt)
		{
			// Does nothing by default, some graphics systems do not need it
		}

		public virtual void BeginScissor(Vector2 Pos, Vector2 Size)
		{
			throw new NotImplementedException();
		}

		public virtual void DrawCircle(Vector2 Center, float Radius, FishColor Color)
		{
			throw new NotImplementedException();
		}

		public virtual void DrawCircleOutline(Vector2 Center, float Radius, FishColor Color, float Thickness = 1)
		{
			throw new NotImplementedException();
		}

		public virtual void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color)
		{
			throw new NotImplementedException();
		}

		public virtual void DrawImage(ImageRef Img, Vector2 Pos, Vector2 Size, float Rot, float Scale, FishColor Color)
		{
			throw new NotImplementedException();
		}

		public virtual void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr)
		{
			throw new NotImplementedException();
		}

		public virtual void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color)
		{
			// TODO: Implement yourself using the other DrawNPatch overload
		}

		public virtual void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color, float Rotation)
		{
			// TODO: Implement yourself using images and rectangles
		}

		public virtual void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color)
		{
			throw new NotImplementedException();
		}

		public virtual void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color)
		{
			throw new NotImplementedException();
		}

		public virtual void DrawText(FontRef Fn, string Text, Vector2 Pos)
		{
			throw new NotImplementedException();
		}

		public virtual void DrawTextColor(FontRef Fn, string Text, Vector2 Pos, FishColor Color)
		{
			throw new NotImplementedException();
		}

		public virtual void DrawTextColorScale(FontRef Fn, string Text, Vector2 Pos, FishColor Color, float Scale)
		{
			throw new NotImplementedException();
		}

		public virtual void EndDrawing()
		{
			throw new NotImplementedException();
		}

		public virtual void EndScissor()
		{
			throw new NotImplementedException();
		}

		public virtual FishUIFontMetrics GetFontMetrics(FontRef Fn)
		{
			throw new NotImplementedException();
		}

		public virtual FishColor GetImageColor(ImageRef Img, Vector2 Pos)
		{
			throw new NotImplementedException();
		}

		public abstract int GetWindowHeight();

		public abstract int GetWindowWidth();

		public abstract void Init();

		public virtual FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color)
		{
			throw new NotImplementedException();
		}

		public virtual FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color, FontStyle Style)
		{
			throw new NotImplementedException();
		}

		public virtual ImageRef LoadImage(string FileName)
		{
			throw new NotImplementedException();
		}

		public virtual ImageRef LoadImage(string FileName, int X, int Y, int W, int H)
		{
			throw new NotImplementedException();
		}

		public virtual ImageRef LoadImage(ImageRef Orig, int X, int Y, int W, int H)
		{
			throw new NotImplementedException();
		}

		public virtual Vector2 MeasureText(FontRef Fn, string Text)
		{
			throw new NotImplementedException();
		}

		public virtual void PopScissor()
		{
			throw new NotImplementedException();
		}

		public virtual void PushScissor(Vector2 Pos, Vector2 Size)
		{
			throw new NotImplementedException();
		}

		public virtual void SetImageFilter(ImageRef Img, bool pixelated)
		{
			throw new NotImplementedException();
		}
	}
}
