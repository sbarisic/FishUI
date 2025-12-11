using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FishUI.Controls
{
	public class CheckBox : Control
	{
		NPatch ImgChecked;
		NPatch ImgUnchecked;
		NPatch ImgDisabledChecked;
		NPatch ImgDisabledUnchecked;

		public bool Checked;

		public CheckBox()
		{
		}

		public override void Init(FishUI UI)
		{
			base.Init(UI);

			ImgChecked = new NPatch(UI, "data/checkbox_checked.png", 2, 2, 2, 2);
			ImgUnchecked = new NPatch(UI, "data/checkbox_unchecked.png", 2, 2, 2, 2);
			ImgDisabledChecked = new NPatch(UI, "data/checkbox_disabled_checked.png", 2, 2, 2, 2);
			ImgDisabledUnchecked = new NPatch(UI, "data/checkbox_disabled_unchecked.png", 2, 2, 2, 2);
		}

		public override void Draw(FishUI UI, float Dt, float Time)
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

			DrawChildren(UI, Dt, Time);
		}

		public override void HandleInput(FishUI UI, FishInputState InState)
		{
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
				Checked = !Checked;
		}
	}
}
