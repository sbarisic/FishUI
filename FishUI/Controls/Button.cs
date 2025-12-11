using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public class Button : Control
	{
		[YamlIgnore]
		NPatch ImgNormal;

		[YamlIgnore]
		NPatch ImgHover;

		[YamlIgnore]
		NPatch ImgDisabled;

		[YamlIgnore]
		NPatch ImgPressed;

		[YamlIgnore]
		FontRef TxtFnt;

		public string Text;

		public Button()
		{
		}

		public override void Init(FishUI UI)
		{
			base.Init(UI);

			ImgNormal = new NPatch(UI, "data/button_normal.png", 2, 2, 2, 2);
			ImgHover = new NPatch(UI, "data/button_hover.png", 2, 2, 2, 2);
			ImgDisabled = new NPatch(UI, "data/button_disabled.png", 2, 2, 2, 2);
			ImgPressed = new NPatch(UI, "data/button_pressed.png", 2, 2, 2, 2);

			TxtFnt = UI.Graphics.LoadFont("data/fonts/ubuntu_mono.ttf", 18, 0, FishColor.Black);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch Cur = ImgNormal;

			if (Disabled)
				Cur = ImgDisabled;
			else if (IsMousePressed)
				Cur = ImgPressed;
			else if (IsMouseInside)
				Cur = ImgHover;

			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			string Txt = Text;

			if (!string.IsNullOrEmpty(Txt))
			{
				Vector2 TxtSz = UI.Graphics.MeasureText(TxtFnt, Txt);
				UI.Graphics.DrawText(TxtFnt, Txt, (GetAbsolutePosition() + GetAbsoluteSize() / 2) - TxtSz / 2);
			}

			//DrawChildren(UI, Dt, Time);
		}
	}
}
