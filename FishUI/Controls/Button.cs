using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public class Button : Control
	{
		/*[YamlIgnore]
		NPatch ImgNormal;

		[YamlIgnore]
		NPatch ImgHover;

		[YamlIgnore]
		NPatch ImgDisabled;

		[YamlIgnore]
		NPatch ImgPressed;

		[YamlIgnore]
		FontRef TxtFnt;*/

		public string Text;

		public Button()
		{
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch Cur = UI.Settings.ImgButtonNormal;

			if (Disabled)
				Cur = UI.Settings.ImgButtonDisabled;
			else if (IsMousePressed)
				Cur = UI.Settings.ImgButtonPressed;
			else if (IsMouseInside)
				Cur = UI.Settings.ImgButtonHover;

			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			string Txt = Text;

			if (!string.IsNullOrEmpty(Txt))
			{
				Vector2 TxtSz = UI.Graphics.MeasureText(UI.Settings.FontDefault, Txt);
				UI.Graphics.DrawText(UI.Settings.FontDefault, Txt, (GetAbsolutePosition() + GetAbsoluteSize() / 2) - TxtSz / 2);
			}

			//DrawChildren(UI, Dt, Time);
		}
	}
}
