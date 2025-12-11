using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace FishUI.Controls
{
	public class Label : Control
	{
		FontRef TxtFnt;

		public string Text;
		int FontSize;

		public Label(int FontSize, string Text)
		{
			this.Text = Text;
			this.FontSize = FontSize;
		}

		public override void Init(FishUI UI)
		{
			base.Init(UI);

			TxtFnt = UI.Graphics.LoadFont("data/fonts/ubuntu_mono.ttf", FontSize, 0, FishColor.Black);
		}

		public override void Draw(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			string Txt = Text;
			Vector2 TxtSz = UI.Graphics.MeasureText(TxtFnt, Txt);
			UI.Graphics.DrawText(TxtFnt, Txt, (GetAbsolutePosition() + GetAbsoluteSize() / 2) - TxtSz / 2);

			DrawChildren(UI, Dt, Time);
		}
	}
}
