using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace FishUI.Controls
{
	public enum Align
	{
		Left, Center, Right
	}

	public class Label : Control
	{
		[YamlIgnore]
		FontRef TxtFnt;

		public string Text;
		public int FontSize;

		public Align Alignment = Align.Center;

		public Label()
		{
		}

		public Label(int FontSize, string Text)
		{
			this.Text = Text;
			this.FontSize = FontSize;

			Size = new Vector2(200, FontSize);
		}

		public void InitForCheckbox(string Text)
		{
			this.Text = Text;
			FontSize = 16;
			Alignment = Align.Left;
			Position = new Vector2(18, 0);
			Size = new Vector2(200, FontSize);
		}

		public override void Init(FishUI UI)
		{
			base.Init(UI);

			TxtFnt = UI.Graphics.LoadFont("data/fonts/ubuntu_mono.ttf", FontSize, 0, FishColor.Black);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			string Txt = Text;
			if (!string.IsNullOrEmpty(Txt))
			{
				if (Parent is CheckBox || Parent is RadioButton)
				{
					Position.X = Parent.Size.X + 4;
					Position.Y = Parent.Size.Y / 2 - FontSize / 2;
				}
				

				Vector2 TxtSz = UI.Graphics.MeasureText(TxtFnt, Txt);
				if (float.IsNaN(TxtSz.X)) 
					TxtSz.X = 0;

				if (float.IsNaN(TxtSz.Y)) 
					TxtSz.Y = 0;

				Vector2 Pos = (GetAbsolutePosition() + GetAbsoluteSize() / 2) - TxtSz / 2;

				if (Alignment == Align.Center)
				{
					Pos = (GetAbsolutePosition() + GetAbsoluteSize() / 2) - TxtSz / 2;
				}
				else if (Alignment == Align.Left)
				{
					Pos = GetAbsolutePosition() + new Vector2(0, GetAbsoluteSize().Y / 2) - new Vector2(0, TxtSz.Y / 2);
				}
				else if (Alignment == Align.Right)
				{
					Pos = GetAbsolutePosition() + new Vector2(GetAbsoluteSize().X, GetAbsoluteSize().Y / 2) - new Vector2(TxtSz.X, TxtSz.Y / 2);
				}
				else
					throw new NotImplementedException();

				UI.Graphics.DrawText(TxtFnt, Txt, Pos);
			}

			//DrawChildren(UI, Dt, Time);
		}
	}
}
