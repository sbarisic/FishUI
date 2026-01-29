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
            // Calculate intersection for rectangle Pos1+Size1 and rectangle Pos2+Size2 into UnPos+UnSize
            // If they don't overlap, return false

            float left = Math.Max(Pos1.X, Pos2.X);
            float top = Math.Max(Pos1.Y, Pos2.Y);
            float right = Math.Min(Pos1.X + Size1.X, Pos2.X + Size2.X);
            float bottom = Math.Min(Pos1.Y + Size1.Y, Pos2.Y + Size2.Y);

            if (left >= right || top >= bottom)
            {
                UnPos = Vector2.Zero;
                UnSize = Vector2.Zero;
                return false;
            }

            UnPos = new Vector2(left, top);
            UnSize = new Vector2(right - left, bottom - top);
            return true;
        }

        public static float Round(this float F)
        {
            return (int)F;
        }

        public static Vector2 Round(this Vector2 V)
        {
            return new Vector2(V.X.Round(), V.Y.Round());
        }
    }
}
