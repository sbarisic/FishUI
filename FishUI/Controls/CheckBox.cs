using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public class CheckBox : Control
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

		public CheckBox()
		{
		}

		public CheckBox(string LabelText)
		{
			Label Lbl = new Label();
			Lbl.InitForCheckbox(LabelText);
			AddChild(Lbl);
		}

		public override void Init(FishUI UI)
		{
			base.Init(UI);

			ImgChecked = new NPatch(UI, "data/checkbox_checked.png", 2, 2, 2, 2);
			ImgUnchecked = new NPatch(UI, "data/checkbox_unchecked.png", 2, 2, 2, 2);
			ImgDisabledChecked = new NPatch(UI, "data/checkbox_disabled_checked.png", 2, 2, 2, 2);
			ImgDisabledUnchecked = new NPatch(UI, "data/checkbox_disabled_unchecked.png", 2, 2, 2, 2);
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

			Vector2 Pos = GetAbsolutePosition();
			Vector2 Sz = GetAbsoluteSize();

			//FindChildByType<Label>().Position = new Vector2(Sz.X + 5, 0);
			UI.Graphics.DrawNPatch(Cur, Pos, Sz, Color);

			//DrawChildren(UI, Dt, Time, false);
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
				Checked = !Checked;
		}
	}
}
