using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public class CheckBox : Control
	{
		/// <summary>
		/// Whether the checkbox is currently IsChecked.
		/// </summary>
		[YamlMember]
		public bool IsChecked { get; set; }

		/// <summary>
		/// CheckBox disables child scissor so labels can extend beyond the checkbox icon bounds.
		/// </summary>
		public override bool DisableChildScissor { get; set; } = true;

		public CheckBox()
		{
		}

		public CheckBox(string LabelText)
		{
			Label Lbl = new Label(LabelText);
			Lbl.Alignment = Align.Left;
			AddChild(Lbl);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch Cur = UI.Settings.ImgCheckboxUnchecked;

			if (Disabled)
			{
				if (IsChecked)
					Cur = UI.Settings.ImgCheckboxDisabledChecked;
				else
					Cur = UI.Settings.ImgCheckboxDisabledUnchecked;
			}
			else
			{
				if (IsChecked)
					Cur = IsMouseInside ? UI.Settings.ImgCheckboxCheckedHover : UI.Settings.ImgCheckboxChecked;
				else
					Cur = IsMouseInside ? UI.Settings.ImgCheckboxUncheckedHover : UI.Settings.ImgCheckboxUnchecked;
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
				IsChecked = !IsChecked;
		}
	}
}
