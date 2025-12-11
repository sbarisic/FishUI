using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public class RadioButton : Control
	{
		[YamlIgnore]
		NPatch ImgChecked;

		[YamlIgnore]
		NPatch ImgUnchecked;

		[YamlIgnore]
		NPatch ImgDisabledChecked;

		[YamlIgnore]
		NPatch ImgDisabledUnchecked;

		public bool Checked;

		public RadioButton()
		{
		}

		public RadioButton(string LabelText)
		{
			Label Lbl = new Label(16, LabelText); 
			Lbl.InitForCheckbox(LabelText);
			AddChild(Lbl);

		}
		public override void Init(FishUI UI)
		{
			base.Init(UI);

			ImgChecked = new NPatch(UI, "data/radiobutton_checked.png", 2, 2, 2, 2);
			ImgUnchecked = new NPatch(UI, "data/radiobutton_unchecked.png", 2, 2, 2, 2);
			ImgDisabledChecked = new NPatch(UI, "data/radiobutton_disabled_checked.png", 2, 2, 2, 2);
			ImgDisabledUnchecked = new NPatch(UI, "data/radiobutton_disabled_unchecked.png", 2, 2, 2, 2);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch Cur = ImgUnchecked;

			if (Disabled)
			{
				if (Checked)
					Cur = ImgDisabledChecked;
				else
					Cur = ImgDisabledUnchecked;
			}
			else
			{
				if (Checked)
					Cur = ImgChecked;
				else
					Cur = ImgUnchecked;
			}

			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			//DrawChildren(UI, Dt, Time);
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			if (Btn == FishMouseButton.Left)
				Checked = !Checked;
		}

	}
}
