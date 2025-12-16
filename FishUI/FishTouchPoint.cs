using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FishUI
{
	public enum FishTouchType
	{
		Press,
		Release,
		Motion
	}

	public struct FishTouchPoint
	{
		public int Id;
		public float Width;
		public Vector2 Position;
		public Vector2 Delta;

		public FishTouchType TouchType;
	}
}
