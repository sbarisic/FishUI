using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public enum IconPosition
	{
		Left,
		Right,
		Top,
		Bottom
	}

	public delegate void ButtonPressFunc(Button Sender, FishMouseButton Btn, Vector2 Pos);

	public class Button : Control
	{
		[YamlIgnore]
		NPatch ImgNormal;

		[YamlIgnore]
		NPatch ImgHover;

		[YamlIgnore]
		NPatch ImgDisabled;

		[YamlIgnore]
		NPatch ImgPressed;

		public string Text;

		/// <summary>
		/// Icon image to display on the button. When set, the icon is rendered alongside or instead of text.
		/// </summary>
		[YamlIgnore]
		public ImageRef Icon;

		/// <summary>
		/// Position of the icon relative to the text.
		/// </summary>
		public IconPosition IconPosition = IconPosition.Left;

		/// <summary>
		/// Spacing between the icon and text in pixels.
		/// </summary>
		public float IconSpacing = 4f;

		public event ButtonPressFunc OnButtonPressed;

	public Button()
		{
			Focusable = true;
		}

		public Button(NPatch Normal, NPatch Disabled, NPatch Pressed, NPatch Hovered)
		{
			ImgNormal = Normal;
			ImgDisabled = Disabled;
			ImgPressed = Pressed;
			ImgHover = Hovered;
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (OnButtonPressed != null)
				OnButtonPressed(this, Btn, Pos);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			//base.Draw(UI, Dt, Time);

			NPatch NNormal = ImgNormal ?? UI.Settings.ImgButtonNormal;
			NPatch NDisabled = ImgDisabled ?? UI.Settings.ImgButtonDisabled;
			NPatch NPressed = ImgPressed ?? UI.Settings.ImgButtonPressed;
			NPatch NHover = ImgHover ?? UI.Settings.ImgButtonHover;

			NPatch Cur = NNormal;

			if (Disabled)
				Cur = NDisabled;
			else if (IsMousePressed)
				Cur = NPressed;
			else if (IsMouseInside)
				Cur = NHover;

			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			string Txt = Text;
			bool hasText = !string.IsNullOrEmpty(Txt);
			bool hasIcon = Icon != null;

			Vector2 center = GetAbsolutePosition() + GetAbsoluteSize() / 2;

			if (hasIcon && hasText)
			{
				// Icon + Text mode
				Vector2 iconSize = new Vector2(Icon.Width, Icon.Height);
				Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, Txt);

				Vector2 iconPos;
				Vector2 textPos;

				switch (IconPosition)
				{
					case IconPosition.Left:
						float totalWidth = iconSize.X + IconSpacing + textSize.X;
						iconPos = new Vector2(center.X - totalWidth / 2, center.Y - iconSize.Y / 2);
						textPos = new Vector2(iconPos.X + iconSize.X + IconSpacing, center.Y - textSize.Y / 2);
						break;

					case IconPosition.Right:
						totalWidth = textSize.X + IconSpacing + iconSize.X;
						textPos = new Vector2(center.X - totalWidth / 2, center.Y - textSize.Y / 2);
						iconPos = new Vector2(textPos.X + textSize.X + IconSpacing, center.Y - iconSize.Y / 2);
						break;

					case IconPosition.Top:
						float totalHeight = iconSize.Y + IconSpacing + textSize.Y;
						iconPos = new Vector2(center.X - iconSize.X / 2, center.Y - totalHeight / 2);
						textPos = new Vector2(center.X - textSize.X / 2, iconPos.Y + iconSize.Y + IconSpacing);
						break;

					case IconPosition.Bottom:
					default:
						totalHeight = textSize.Y + IconSpacing + iconSize.Y;
						textPos = new Vector2(center.X - textSize.X / 2, center.Y - totalHeight / 2);
						iconPos = new Vector2(center.X - iconSize.X / 2, textPos.Y + textSize.Y + IconSpacing);
						break;
				}

				UI.Graphics.DrawImage(Icon, iconPos, 0f, 1f, FishColor.White);
				UI.Graphics.DrawText(UI.Settings.FontDefault, Txt, textPos);
			}
			else if (hasIcon)
			{
				// Icon-only mode - center the icon
				Vector2 iconSize = new Vector2(Icon.Width, Icon.Height);
				Vector2 iconPos = center - iconSize / 2;
				UI.Graphics.DrawImage(Icon, iconPos, 0f, 1f, FishColor.White);
			}
			else if (hasText)
			{
				// Text-only mode (original behavior)
				Vector2 TxtSz = UI.Graphics.MeasureText(UI.Settings.FontDefault, Txt);
				UI.Graphics.DrawText(UI.Settings.FontDefault, Txt, center - TxtSz / 2);
			}

			//DrawChildren(UI, Dt, Time);
		}
	}
}
