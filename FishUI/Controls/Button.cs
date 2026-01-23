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
	public delegate void ButtonToggledFunc(Button Sender, bool IsToggled);

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

		/// <summary>
		/// If true, this button acts as a toggle (stays pressed when clicked, unpressed when clicked again).
		/// </summary>
		public bool IsToggleButton { get; set; } = false;

		/// <summary>
		/// Gets or sets the toggled state. Only applicable when IsToggleButton is true.
		/// </summary>
		public bool IsToggled { get; set; } = false;

		/// <summary>
		/// If true, this button repeatedly fires OnButtonPressed while held down.
		/// </summary>
		public bool IsRepeatButton { get; set; } = false;

		/// <summary>
		/// Initial delay before repeat starts (in seconds). Only applicable when IsRepeatButton is true.
		/// </summary>
		public float RepeatDelay { get; set; } = 0.4f;

		/// <summary>
		/// Interval between repeat fires (in seconds). Only applicable when IsRepeatButton is true.
		/// </summary>
		public float RepeatInterval { get; set; } = 0.1f;

		// Internal state for repeat timing
		private float _repeatTimer = 0f;
		private bool _repeatStarted = false;

		public event ButtonPressFunc OnButtonPressed;

		/// <summary>
		/// Event fired when the toggle state changes. Only applicable when IsToggleButton is true.
		/// </summary>
		public event ButtonToggledFunc OnToggled;

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

			// Handle toggle mode
			if (IsToggleButton && Btn == FishMouseButton.Left)
			{
				IsToggled = !IsToggled;
				OnToggled?.Invoke(this, IsToggled);
			}

			// Don't fire OnButtonPressed here for repeat buttons - it's handled in DrawControl
			if (!IsRepeatButton && OnButtonPressed != null)
				OnButtonPressed(this, Btn, Pos);
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			// Reset repeat timer on mouse press
			if (IsRepeatButton && Btn == FishMouseButton.Left)
			{
				_repeatTimer = 0f;
				_repeatStarted = false;
				// Fire immediately on first press
				OnButtonPressed?.Invoke(this, Btn, Pos);
			}
		}

		public override void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseRelease(UI, InState, Btn, Pos);

			// Reset repeat state on release
			if (IsRepeatButton)
			{
				_repeatTimer = 0f;
				_repeatStarted = false;
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			// Handle repeat button timing
			if (IsRepeatButton && IsMousePressed && !Disabled)
			{
				_repeatTimer += Dt;

				if (!_repeatStarted)
				{
					// Wait for initial delay
					if (_repeatTimer >= RepeatDelay)
					{
						_repeatStarted = true;
						_repeatTimer = 0f;
						OnButtonPressed?.Invoke(this, FishMouseButton.Left, GetAbsolutePosition() + GetAbsoluteSize() / 2);
					}
				}
				else
				{
					// Fire at repeat interval
					if (_repeatTimer >= RepeatInterval)
					{
						_repeatTimer = 0f;
						OnButtonPressed?.Invoke(this, FishMouseButton.Left, GetAbsolutePosition() + GetAbsoluteSize() / 2);
					}
				}
			}

			//base.Draw(UI, Dt, Time);

			NPatch NNormal = ImgNormal ?? UI.Settings.ImgButtonNormal;
			NPatch NDisabled = ImgDisabled ?? UI.Settings.ImgButtonDisabled;
			NPatch NPressed = ImgPressed ?? UI.Settings.ImgButtonPressed;
			NPatch NHover = ImgHover ?? UI.Settings.ImgButtonHover;

			NPatch Cur = NNormal;

			if (Disabled)
				Cur = NDisabled;
			else if (IsMousePressed || (IsToggleButton && IsToggled))
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
