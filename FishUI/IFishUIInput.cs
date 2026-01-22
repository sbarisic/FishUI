using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace FishUI
{
	public interface IFishUIInput
	{
		public FishKey GetKeyPressed();
		public int GetCharPressed();

		public bool IsKeyDown(FishKey Key);
		public bool IsKeyUp(FishKey Key);
		public bool IsKeyPressed(FishKey Key);
		public bool IsKeyReleased(FishKey Key);

		public Vector2 GetMousePosition();
		public float GetMouseWheelMove();

		public FishTouchPoint[] GetTouchPoints();


		public bool IsMouseDown(FishMouseButton Button);
		public bool IsMouseUp(FishMouseButton Button);
		public bool IsMousePressed(FishMouseButton Button);
		public bool IsMouseReleased(FishMouseButton Button);
	}
}
