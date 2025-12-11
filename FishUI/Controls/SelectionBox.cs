using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public class SelectionBox : Control
	{
		[YamlIgnore]
		NPatch ImgNormal;

		public SelectionBox()
		{
		}

		public override void Init(FishUI UI)
		{
			base.Init(UI);

			ImgNormal = new NPatch(UI, "data/selectionbox_normal.png", 2, 2, 2, 2);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch Cur = ImgNormal;
			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			//DrawChildren(UI, Dt, Time);
		}
	}
}
