using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace FishUI.Controls
{
	public class Textbox : Control
	{
		public string Text;
		//public int FontSize;

		public Textbox()
		{
			Size = new Vector2(200, 19);
		}

		public Textbox(string Text) : this()
		{
			this.Text = Text;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch Cur = UI.Settings.ImgTextboxNormal;

			if (Disabled)
				Cur = UI.Settings.ImgTextboxDisabled;
			else if (UI.InputActiveControl == this)
				Cur = UI.Settings.ImgTextboxActive;

			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			UI.Graphics.PushScissor(GetAbsolutePosition(), GetAbsoluteSize());
			string Txt = Text;
			Vector2 TxtSz = UI.Graphics.MeasureText(UI.Settings.FontTextboxDefault, Txt);

			Vector2 TxtPos = (GetAbsolutePosition() + new Vector2(Cur.Left + 4, Size.Y / 2)) - new Vector2(0, TxtSz.Y / 2);
			UI.Graphics.DrawText(UI.Settings.FontTextboxDefault, Txt, TxtPos);
			UI.Graphics.PopScissor();

			bool DrawCursor = false;

			if (UI.InputActiveControl == this)
				DrawCursor = MathF.Cos(Time * 10) > 0;

			if (DrawCursor)
			{
				Vector2 LineStart = TxtPos + TxtSz + new Vector2(-1, -2);
				UI.Graphics.DrawLine(LineStart, LineStart + new Vector2(8, 0), 2, FishColor.Black);
			}

			//DrawChildren(UI, Dt, Time);
		}

		public override void HandleTextInput(FishUI UI, FishInputState InState, char Chr)
		{
			if (UI.InputActiveControl != this)
				return;

			if (Chr == '\b')
			{
				if (Text.Length > 0)
					Text = Text.Substring(0, Text.Length - 1);
			}
			else
			{
				Text += Chr;
			}
		}
	}
}
