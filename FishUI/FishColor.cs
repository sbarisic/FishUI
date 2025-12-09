using System;
using System.Collections.Generic;
using System.Text;

namespace FishUI
{
	public struct FishColor
	{
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
