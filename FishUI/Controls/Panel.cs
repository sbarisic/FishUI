using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FishUI.Controls
{
	public class Panel : Control
	{
		NPatch ImgNormal;
		NPatch ImgHover;
		NPatch ImgDisabled;
		NPatch ImgPressed;

		public Panel()
		{
		}

		public override void Init(FishUI UI)
		{
			base.Init(UI);

			ImgNormal = new NPatch(UI, "data/panel2_normal.png", 2, 2, 2, 2);
			ImgHover = new NPatch(UI, "data/panel_hover.png", 2, 2, 2, 2);
			ImgDisabled = new NPatch(UI, "data/panel_disabled.png", 2, 2, 2, 2);
			ImgPressed = new NPatch(UI, "data/panel_pressed.png", 2, 2, 2, 2);
		}

		public override void Draw(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch Cur = ImgNormal;

			if (Disabled)
				Cur = ImgDisabled;
			/**else if (IsMousePressed)
				Cur = ImgPressed;
			else if (IsMouseInside)
				Cur = ImgHover;**/

			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			DrawChildren(UI, Dt, Time);
		}
	}
}
