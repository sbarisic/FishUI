using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FishUI
{
    [Flags]
    public enum Anchor
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
    }

    public enum PositionMode
    {
        Absolute,
        Relative,
    }

    public struct Padding
    {
        public float Top;
        public float Left;
        public float Right;
        public float Bottom;
    }

    public class FishUIPosition
    {
        public PositionMode Mode;

        public float X;
        public float Y;

        public float Top;
        public float Left;
        public float Right;
        public float Bottom;

        public FishUIPosition(PositionMode Mode, Vector2 Pos)
        {
            this.Mode = Mode;
            this.X = Pos.X;
            this.Y = Pos.Y;
        }

        public static implicit operator FishUIPosition(Vector2 Pos)
        {
            return new FishUIPosition(PositionMode.Relative, Pos);
        }
    }
}
