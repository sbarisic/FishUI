using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FishUI
{
	public struct FishInputState
	{
		public Vector2 MousePos;
		public Vector2 MouseDelta;
		public float MouseWheelDelta;

		public FishTouchPoint[] TouchPoints;

		public bool MouseLeft;
		public bool MouseLeftPressed;
		public bool MouseLeftReleased;

		public bool MouseRight;
		public bool MouseRightPressed;
		public bool MouseRightReleased;

		// Double-click detection
		public bool MouseLeftDoubleClick;
		public bool MouseRightDoubleClick;
	}
}
