using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public enum Align
	{
		None,
		Left,
		Center,
		Right
	}

	public class Label : Control
	{
	public string Text;

	/// <summary>
	/// Text alignment within the label bounds. Default is Left to prevent clipping in containers.
	/// </summary>
	public Align Alignment = Align.Left;

	public Label()
		{
		}

		public Label(string Text)
		{
			this.Text = Text;
			Position = new Vector2(18, 0);
			Size = new Vector2(200, 16);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			string Txt = Text;
			if (!string.IsNullOrEmpty(Txt))
			{
				if (Parent is CheckBox || Parent is RadioButton)
				{
					Position.X = Parent.GetAbsoluteSize().X + 4;
					Position.Y = Parent.GetAbsoluteSize().Y / 2 - UI.Settings.FontLabel.Size / 2;
				}


				Vector2 TxtSz = UI.Graphics.MeasureText(UI.Settings.FontLabel, Txt);
				if (float.IsNaN(TxtSz.X))
					TxtSz.X = 0;

				if (float.IsNaN(TxtSz.Y))
					TxtSz.Y = 0;

				Vector2 Pos = Vector2.Zero;

			switch (Alignment)
				{
					case Align.None:
						Pos = GetAbsolutePosition();
						break;

					case Align.Left:
						Pos = GetAbsolutePosition() + new Vector2(0, GetAbsoluteSize().Y / 2) - new Vector2(0, TxtSz.Y / 2);
						break;

					case Align.Center:
						Pos = (GetAbsolutePosition() + GetAbsoluteSize() / 2) - TxtSz / 2;
						break;

					case Align.Right:
						Pos = GetAbsolutePosition() + new Vector2(GetAbsoluteSize().X, GetAbsoluteSize().Y / 2) - new Vector2(TxtSz.X, TxtSz.Y / 2);
						break;

					default:
						throw new NotImplementedException();
				}

				// Use color override if set, otherwise use default black
				FishColor textColor = GetColorOverride("Text", FishColor.Black);
				UI.Graphics.DrawTextColor(UI.Settings.FontLabel, Txt, Pos, textColor);
			}

			//DrawChildren(UI, Dt, Time);
		}
	}
}
