using FishUI;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FishUISample
{
	class RaylibGfx : IFishUIGfx
	{
		public void Init()
		{
		}

		public void BeginDrawing(float Dt)
		{
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.Black);
		}

		public void EndDrawing()
		{
			Raylib.EndDrawing();
		}

		public void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color)
		{
			Raylib.DrawRectangleV(Position, Size, new Color(Color.R, Color.G, Color.B, Color.A));
		}

		public void BeginScissor(Vector2 Pos, Vector2 Size)
		{
			Raylib.BeginScissorMode((int)Pos.X, (int)Pos.Y, (int)Size.X, (int)Size.Y);
		}

		public void EndScissor()
		{
			Raylib.EndScissorMode();
		}
	}
}
