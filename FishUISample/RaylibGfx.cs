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
		class Scissor
		{
			public Vector2 Pos;
			public Vector2 Size;

			public Scissor(Vector2 Pos, Vector2 Size)
			{
				this.Pos = Pos;
				this.Size = Size;
			}
		}

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

		public void DrawRectangleOutline(Vector2 Position, Vector2 Size, FishColor Color)
		{
			Color C = new Color(Color.R, Color.G, Color.B, Color.A);
			Raylib.DrawRectangleLinesEx(new Rectangle(Position, Size), 1, C);
		}

		public void BeginScissor(Vector2 Pos, Vector2 Size)
		{
			Raylib.BeginScissorMode((int)Pos.X, (int)Pos.Y, (int)Size.X, (int)Size.Y);
		}

		public void EndScissor()
		{
			Raylib.EndScissorMode();
		}

		Scissor CurScissor;
		Stack<Scissor> ScissorStack = new Stack<Scissor>();

		public void PushScissor(Vector2 Pos, Vector2 Size)
		{
			ScissorStack.Push(new Scissor(Pos, Size));
			bool HasScissor = false;

			if (ScissorStack.Count == 1)
			{
				HasScissor = true;
				CurScissor = ScissorStack.Peek();
			}
			else if (ScissorStack.Count > 1)
			{
				HasScissor = true;
				Scissor Tp = ScissorStack.Peek();
				if (Utils.Union(CurScissor.Pos, CurScissor.Size, Tp.Pos, Tp.Size, out Vector2 NewPos, out Vector2 NewSize))
				{
					CurScissor = new Scissor(NewPos, NewSize);
				}
			}

			if (HasScissor)
				BeginScissor(CurScissor.Pos, CurScissor.Size);
		}

		public void PopScissor()
		{
			ScissorStack.Pop();

			if (ScissorStack.Count == 0)
				EndScissor();
		}
	}
}
