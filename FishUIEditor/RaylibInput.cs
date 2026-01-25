using FishUI;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FishUIEditor
{
	class RaylibInput : IFishUIInput
	{
		public FishKey GetKeyPressed()
		{
			int K = Raylib.GetKeyPressed();
			if (K == 0)
				return FishKey.None;

			return (FishKey)K;
		}

		public int GetCharPressed()
		{
			return Raylib.GetCharPressed();
		}

		public Vector2 GetMousePosition()
		{
			return Raylib.GetMousePosition();
		}

		public float GetMouseWheelMove()
		{
			return Raylib.GetMouseWheelMove();
		}

		public FishTouchPoint[] GetTouchPoints()
		{
			return new FishTouchPoint[0];
		}

		public bool IsKeyDown(FishKey Key)
		{
			return Raylib.IsKeyDown((KeyboardKey)Key);
		}

		public bool IsKeyPressed(FishKey Key)
		{
			return Raylib.IsKeyPressed((KeyboardKey)Key);
		}

		public bool IsKeyReleased(FishKey Key)
		{
			return Raylib.IsKeyReleased((KeyboardKey)Key);
		}

		public bool IsKeyUp(FishKey Key)
		{
			return Raylib.IsKeyUp((KeyboardKey)Key);
		}

		public bool IsMouseDown(FishMouseButton Button)
		{
			return Raylib.IsMouseButtonDown((MouseButton)Button);
		}

		public bool IsMousePressed(FishMouseButton Button)
		{
			return Raylib.IsMouseButtonPressed((MouseButton)Button);
		}

		public bool IsMouseReleased(FishMouseButton Button)
		{
			return Raylib.IsMouseButtonReleased((MouseButton)Button);
		}

		public bool IsMouseUp(FishMouseButton Button)
		{
			return Raylib.IsMouseButtonUp((MouseButton)Button);
		}

		public string GetClipboardText()
		{
			return Raylib.GetClipboardText_();
		}

		public void SetClipboardText(string text)
		{
			Raylib.SetClipboardText(text ?? "");
		}
	}
}
