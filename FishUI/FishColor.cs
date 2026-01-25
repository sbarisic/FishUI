using System;
using System.Collections.Generic;
using System.Text;

namespace FishUI
{
	public struct FishColor
	{
		public static readonly FishColor White = new FishColor(255, 255, 255);
		public static readonly FishColor Black = new FishColor(0, 0, 0);
		public static readonly FishColor Red = new FishColor(255, 0, 0);
		public static readonly FishColor Green = new FishColor(0, 255, 0);
		public static readonly FishColor Blue = new FishColor(0, 0, 255);
		public static readonly FishColor Cyan = new FishColor(0, 255, 255);
		public static readonly FishColor Yellow = new FishColor(255, 255, 0);
		public static readonly FishColor Teal = new FishColor(255, 0, 255);
		public static readonly FishColor Transparent = new FishColor(0, 0, 0, 0);

		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public FishColor(byte R, byte G, byte B, byte A = 255)
		{
			this.R = R;
			this.G = G;
			this.B = B;
			this.A = A;
		}

		public bool IsEmpty()
		{
			return R == 0 && G == 0 && B == 0 && A == 0;
		}

		/// <summary>
		/// Linearly interpolates between two colors.
		/// </summary>
		/// <param name="a">Start color.</param>
		/// <param name="b">End color.</param>
		/// <param name="t">Interpolation factor (0-1).</param>
		/// <returns>The interpolated color.</returns>
		public static FishColor Lerp(FishColor a, FishColor b, float t)
		{
			t = Math.Clamp(t, 0f, 1f);
			return new FishColor(
				(byte)(a.R + (b.R - a.R) * t),
				(byte)(a.G + (b.G - a.G) * t),
				(byte)(a.B + (b.B - a.B) * t),
				(byte)(a.A + (b.A - a.A) * t)
			);
		}
	}
}
