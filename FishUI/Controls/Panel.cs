using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public class Panel : Control
	{
		/// <summary>
		/// Whether the panel background is transparent (not drawn).
		/// </summary>
		[YamlMember]
		public bool IsTransparent { get; set; } = false;

		public Panel()
		{
		}

	public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			if (IsTransparent)
				return;

		NPatch Cur = UI.Settings.ImgPanel;

			if (Disabled)
				Cur = UI.Settings.ImgPanelDisabled;

			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			//DrawChildren(UI, Dt, Time);
		}
	}
}
