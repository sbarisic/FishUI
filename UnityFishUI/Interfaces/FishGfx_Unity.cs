using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using FishUI;

namespace UnityFishUI.Interfaces
{
	public class FishGfx_Unity : SimpleFishUIGfx
	{		
		public override void Init()
		{
		}

		public override void FocusWindow()
		{
		}

		public override int GetWindowHeight()
		{
			return 0;
		}

		public override int GetWindowWidth()
		{
			return 0;
		}

		public override void BeginScissor(Vector2 Pos, Vector2 Size)
		{
			throw new NotImplementedException();
		}

		public override void EndScissor()
		{
			throw new NotImplementedException();
		}

		public override FontRef LoadFont(string FileName, float Size, float Spacing, FishColor Color)
		{
			throw new NotImplementedException();
		}

		public override ImageRef LoadImage(string FileName)
		{
			throw new NotImplementedException();
		}

		public override FishColor GetImageColor(ImageRef Img, Vector2 Pos)
		{
			throw new NotImplementedException();
		}

		public override Vector2 MeasureText(FontRef Fn, string Text)
		{
			throw new NotImplementedException();
		}

		public override void DrawTextColorScale(FontRef Fn, string Text, Vector2 Pos, FishColor Color, float Scale)
		{
			throw new NotImplementedException();
		}

		public override void DrawLine(Vector2 Pos1, Vector2 Pos2, float Thick, FishColor Clr)
		{
			throw new NotImplementedException();
		}

		public override void DrawRectangle(Vector2 Position, Vector2 Size, FishColor Color)
		{
			throw new NotImplementedException();
		}

		public override void DrawImage(ImageRef Img, Vector2 Pos, float Rot, float Scale, FishColor Color)
		{
			throw new NotImplementedException();
		}
	}
}
