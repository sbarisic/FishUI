using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FishUI
{
	public static class Utils
	{
		public static bool IsInside(Vector2 Position, Vector2 Size, Vector2 Point)
		{
			return Point.X >= Position.X && Point.X <= Position.X + Size.X &&
				   Point.Y >= Position.Y && Point.Y <= Position.Y + Size.Y;
		}

		public static bool Union(Vector2 Pos1, Vector2 Size1, Vector2 Pos2, Vector2 Size2, out Vector2 UnPos, out Vector2 UnSize)
		{
			// Calculate union for rectangle Pos1+Size1 and rectangle Pos2+Size2 into UnPos+UnSize
			// If they don't overlap, return false

			// TODO

			UnPos = Vector2.Zero;
			UnSize = Vector2.Zero;
			return false;
		}
	}
}
