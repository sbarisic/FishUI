using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void ButtonPressFunc(Button Sender, FishMouseButton Btn, Vector2 Pos);

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

		public string Text;

		public event ButtonPressFunc OnButtonPressed;

	public Button()
		{
			Focusable = true;
		}

		public Button(NPatch Normal, NPatch Disabled, NPatch Pressed, NPatch Hovered)
		{
			ImgNormal = Normal;
			ImgDisabled = Disabled;
			ImgPressed = Pressed;
			ImgHover = Hovered;
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (OnButtonPressed != null)
				OnButtonPressed(this, Btn, Pos);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch NNormal = ImgNormal ?? UI.Settings.ImgButtonNormal;
			NPatch NDisabled = ImgDisabled ?? UI.Settings.ImgButtonDisabled;
			NPatch NPressed = ImgPressed ?? UI.Settings.ImgButtonPressed;
			NPatch NHover = ImgHover ?? UI.Settings.ImgButtonHover;

			NPatch Cur = NNormal;

			if (Disabled)
				Cur = NDisabled;
			else if (IsMousePressed)
				Cur = NPressed;
			else if (IsMouseInside)
				Cur = NHover;

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
