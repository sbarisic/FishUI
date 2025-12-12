using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public class Panel : Control
	{
		const bool Debug = true;
		public bool IsTransparent = false;

		public Panel()
		{
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			if (Debug)
				UI.Graphics.DrawRectangleOutline(GetAbsolutePosition(), GetAbsoluteSize(), FishColor.Teal);

			if (IsTransparent)
				return;

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
