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
	}
}
