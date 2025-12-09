using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace FishUI.Controls
{
	public class Textbox : Control
	{
		FontRef TxtFnt;

		NPatch ImgNormal;
		NPatch ImgActive;
		NPatch ImgDisabled;

		public string Text;
		int FontSize;


		public Textbox(int FontSize, string Text)
		{
			this.Text = Text;
			this.FontSize = FontSize;
		}

		public override void Init(FishUI UI)
		{
			base.Init(UI);

			ImgNormal = new NPatch(UI, "data/textbox_normal.png", 1, 1, 1, 1);
			ImgActive = new NPatch(UI, "data/textbox_active.png", 1, 1, 1, 1);
			ImgDisabled = new NPatch(UI, "data/textbox_disabled.png", 1, 1, 1, 1);


			TxtFnt = UI.Graphics.LoadFont("data/fonts/ubuntu_mono.ttf", FontSize, 0, FishColor.Black);
		}

		public Vector2 MeasureText(FishUI UI)
		{
			InternalInit(UI);
			return UI.Graphics.MeasureText(TxtFnt, Text);
		}

		public override void Draw(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch Cur = ImgNormal;

			if (Disabled)
				Cur = ImgDisabled;
			else if (UI.InputActiveControl == this)
				Cur = ImgActive;

			UI.Graphics.DrawNPatch(Cur, GlobalPosition, Size, Color);

			UI.Graphics.PushScissor(GlobalPosition, Size);
			string Txt = Text;
			Vector2 TxtSz = UI.Graphics.MeasureText(TxtFnt, Txt);

			Vector2 TxtPos = (GlobalPosition + new Vector2(Cur.Left + 4, Size.Y / 2)) - new Vector2(0, TxtSz.Y / 2);
			UI.Graphics.DrawText(TxtFnt, Txt, TxtPos);
			UI.Graphics.PopScissor();

			bool DrawCursor = false;

			if (UI.InputActiveControl == this)
				DrawCursor = MathF.Cos(Time * 10) > 0;

			if (DrawCursor)
			{
				Vector2 LineStart = TxtPos + TxtSz + new Vector2(-1, -2);
				UI.Graphics.DrawLine(LineStart, LineStart + new Vector2(8, 0), 2, FishColor.Black);
			}

			DrawChildren(UI, Dt, Time);
		}

		public override void HandleInput(FishUI UI, FishInputState InState)
		{
			if (UI.InputActiveControl != this)
				return;

			if (UI.Input.IsKeyPressed(FishKey.Backspace))
			{
				if (Text.Length > 0)
					Text = Text.Substring(0, Text.Length - 1);
			}
			else
			{
				int Cr = UI.Input.GetCharPressed();
				if (Cr != 0)
				{
					char Char = (char)Cr;

					Text += Char;
				}
			}

		}
	}
}
