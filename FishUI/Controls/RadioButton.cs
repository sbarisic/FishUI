using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public class RadioButton : Control
	{
		/// <summary>
		/// Whether the radio button is currently IsChecked.
		/// </summary>
		[YamlMember]
		public bool IsChecked { get; set; }

		/// <summary>
		/// RadioButton disables child scissor so labels can extend beyond the radio button icon bounds.
		/// </summary>
		public override bool DisableChildScissor { get; set; } = true;

		public RadioButton()
		{
		}

		public RadioButton(string LabelText)
		{
			Label Lbl = new Label(LabelText);
			Lbl.Alignment = Align.Left;
			AddChild(Lbl);

		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch Cur = UI.Settings.ImgRadioButtonUnchecked;

			if (Disabled)
			{
				if (IsChecked)
					Cur = UI.Settings.ImgRadioButtonDisabledChecked;
				else
					Cur = UI.Settings.ImgRadioButtonDisabledUnchecked;
			}
			else
			{
				if (IsChecked)
					Cur = IsMouseInside ? UI.Settings.ImgRadioButtonCheckedHover : UI.Settings.ImgRadioButtonChecked;
				else
					Cur = IsMouseInside ? UI.Settings.ImgRadioButtonUncheckedHover : UI.Settings.ImgRadioButtonUnchecked;
			}

			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			//DrawChildren(UI, Dt, Time);
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			if (Btn == FishMouseButton.Left)
				IsChecked = !IsChecked;
		}

	}
}
