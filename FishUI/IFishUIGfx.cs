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

		public void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color);

		public void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color);
	}
}
