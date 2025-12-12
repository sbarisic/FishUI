using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace FishUI
{
	public interface IFishUIGfx
	{
		public void Init();

		public void BeginDrawing(float Dt);

		public void EndDrawing();

		public void BeginScissor(Vector2 Pos, Vector2 Size);

		public void PushScissor(Vector2 Pos, Vector2 Size);

		public void PopScissor();

		public void EndScissor();

		public int GetWindowWidth();

		public int GetWindowHeight();

		// Loading

		public FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color);

		public ImageRef LoadImage(string FileName);

		public ImageRef LoadImage(string FileName, int X, int Y, int W, int H);

		public ImageRef LoadImage(ImageRef Orig, int X, int Y, int W, int H);

		public FishColor GetImageColor(ImageRef Img, Vector2 Pos);

		public Vector2 MeasureText(FontRef Fn, string Text);

		// Drawing

		public void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr);

		public void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color);

		public void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color);

		public void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color);

		public void DrawImage(ImageRef Img, Vector2 Pos, Vector2 Size, float Rot, float Scale, FishColor Color);

		public void DrawNPatch(NPatch NP, Vector2 Pos, Vector2 Size, FishColor Color);

		public void DrawText(FontRef Fn, string Text, Vector2 Pos);

		public void DrawTextColor(FontRef Fn, string Text, Vector2 Pos, FishColor Color);

	}
}
