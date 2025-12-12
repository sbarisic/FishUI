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
		Relative,
		Absolute,
		Docked,
	}

	[Flags]
	public enum DockMode
	{
		Left,
		Top,
		Right,
		Bottom,

		Horizontal = Left | Right,
		Vertical = Top | Bottom,
		Fill = Left | Top | Right | Bottom,
	}

	public struct Padding
	{
		public float Top;
		public float Left;
		public float Right;
		public float Bottom;
	}

	public struct FishUIPosition
	{
		public PositionMode Mode;
		public DockMode Dock;

		public float X;
		public float Y;

		public float Top;
		public float Left;
		public float Right;
		public float Bottom;

		public FishUIPosition()
		{
		}

		public FishUIPosition(PositionMode Mode, Vector2 Pos)
		{
			this.Mode = Mode;
			this.X = Pos.X;
			this.Y = Pos.Y;
		}

		public FishUIPosition(PositionMode Mode, DockMode DMode, Vector4 Dock, Vector2 XY)
		{
			this.Dock = DMode;
			this.Mode = Mode;
			this.Left = Dock.X;
			this.Top = Dock.Y;
			this.Right = Dock.Z;
			this.Bottom = Dock.W;
			this.X = XY.X;
			this.Y = XY.Y;
		}

		public static implicit operator FishUIPosition(Vector2 Pos)
		{
			return new FishUIPosition(PositionMode.Relative, Pos);
		}

		public static implicit operator FishUIPosition(Vector4 Dock)
		{
			FishUIPosition P = new FishUIPosition(PositionMode.Docked, DockMode.Fill, Dock, Vector2.Zero);
			return P;
		}

		public static FishUIPosition operator +(FishUIPosition Pos, Vector2 Mv)
		{
			FishUIPosition NewPos = Pos;
			NewPos.X += Mv.X;
			NewPos.Y += Mv.Y;
			return NewPos;
		}

		public static FishUIPosition operator +(FishUIPosition Pos, Vector4 Mv)
		{
			FishUIPosition NewPos = Pos;
			NewPos.Left += Mv.X;
			NewPos.Top += Mv.Y;
			NewPos.Right += Mv.Z;
			NewPos.Bottom += Mv.W;
			return NewPos;
		}
	}
}