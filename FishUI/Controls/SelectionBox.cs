using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FishUI.Controls
{
	public class SelectionBox : Control
	{
		NPatch ImgNormal;

		public SelectionBox()
		{
		}

		public override void Init(FishUI UI)
		{
			base.Init(UI);

			ImgNormal = new NPatch(UI, "data/selectionbox_normal.png", 2, 2, 2, 2);
		}

		public override void Draw(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch Cur = ImgNormal;
			UI.Graphics.DrawNPatch(Cur, GlobalPosition, Size, Color);

			DrawChildren(UI, Dt, Time);
		}
	}
}
