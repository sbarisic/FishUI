using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public class Panel : Control
	{
		public Panel()
		{
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch Cur = UI.Settings.ImgPanel;

			if (Disabled)
				Cur = UI.Settings.ImgPanelDisabled;
			/**else if (IsMousePressed)
				Cur = ImgPressed;
			else if (IsMouseInside)
				Cur = ImgHover;**/

			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			//DrawChildren(UI, Dt, Time);
		}
	}
}
