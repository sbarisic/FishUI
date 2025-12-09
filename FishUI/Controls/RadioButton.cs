using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FishUI.Controls
{
	public class RadioButton : Control
	{
		NPatch ImgChecked;
		NPatch ImgUnchecked;
		NPatch ImgDisabledChecked;
		NPatch ImgDisabledUnchecked;

		public bool Checked;

		public RadioButton()
		{
		}

		public override void Init(FishUI UI)
		{
			base.Init(UI);

			ImgChecked = new NPatch(UI, "data/radiobutton_checked.png", 2, 2, 2, 2);
			ImgUnchecked = new NPatch(UI, "data/radiobutton_unchecked.png", 2, 2, 2, 2);
			ImgDisabledChecked = new NPatch(UI, "data/radiobutton_disabled_checked.png", 2, 2, 2, 2);
			ImgDisabledUnchecked = new NPatch(UI, "data/radiobutton_disabled_unchecked.png", 2, 2, 2, 2);
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

			UI.Graphics.DrawNPatch(Cur, GlobalPosition, Size, Color);

			DrawChildren(UI, Dt, Time);
		}

		public override void HandleInput(FishUI UI, FishInputState InState)
		{
		}

        public override void HandleMouseLeftClick(FishUI UI, FishInputState InState, Vector2 Pos)
        {
			Checked = !Checked;
            //base.HandleMouseLeftClick(UI, InState, Pos);
        }
	}
}
