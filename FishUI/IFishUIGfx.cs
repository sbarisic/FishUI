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

		public void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color);
	}
}
