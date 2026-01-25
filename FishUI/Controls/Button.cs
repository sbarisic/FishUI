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

		/// <summary>
		/// If true, this button only displays the icon (no background). Use for toolbar-style buttons.
		/// </summary>
		public bool IsImageButton { get; set; } = false;

		/// <summary>
		/// Tint color applied to the icon when the button is hovered. Only used when IsImageButton is true.
		/// </summary>
		public FishColor ImageButtonHoverTint { get; set; } = new FishColor(220, 220, 220, 255);

		/// <summary>
		/// Tint color applied to the icon when the button is pressed. Only used when IsImageButton is true.
		/// </summary>
		public FishColor ImageButtonPressedTint { get; set; } = new FishColor(180, 180, 180, 255);

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

		/// <summary>
		/// Gets the preferred size based on text and icon content.
		/// </summary>
		public override Vector2 GetPreferredSize(FishUI UI)
		{
			if (UI?.Graphics == null)
				return Size;

			float width = 0;
			float height = 0;

			// Measure text if present
			Vector2 textSize = Vector2.Zero;
			if (!string.IsNullOrEmpty(Text))
			{
				textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, Text);
				if (float.IsNaN(textSize.X)) textSize.X = 0;
				if (float.IsNaN(textSize.Y)) textSize.Y = 0;
			}

			// Measure icon if present
			Vector2 iconSize = Vector2.Zero;
			if (Icon != null)
			{
				iconSize = new Vector2(Icon.Width, Icon.Height);
			}

			// Calculate total size based on icon position
			if (Icon != null && !string.IsNullOrEmpty(Text))
			{
				switch (IconPosition)
				{
					case IconPosition.Left:
					case IconPosition.Right:
						width = iconSize.X + IconSpacing + textSize.X;
						height = Math.Max(iconSize.Y, textSize.Y);
						break;
					case IconPosition.Top:
					case IconPosition.Bottom:
						width = Math.Max(iconSize.X, textSize.X);
						height = iconSize.Y + IconSpacing + textSize.Y;
						break;
				}
			}
			else if (Icon != null)
			{
				width = iconSize.X;
				height = iconSize.Y;
			}
			else if (!string.IsNullOrEmpty(Text))
			{
				width = textSize.X;
				height = textSize.Y;
			}

			// Add default button padding (8px horizontal, 4px vertical)
			width += 16;
			height += 8;

			return new Vector2(width, height);
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
			// Update auto-size if enabled
			UpdateAutoSize(UI);

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

			// ImageButton mode - only draw the icon with state-based tinting
			if (IsImageButton && Icon != null)
			{
				FishColor iconTint = FishColor.White;
				if (Disabled)
					iconTint = new FishColor(128, 128, 128, 128);
				else if (IsMousePressed || (IsToggleButton && IsToggled))
					iconTint = ImageButtonPressedTint;
				else if (IsMouseInside)
					iconTint = ImageButtonHoverTint;

				Vector2 iconSize = new Vector2(Icon.Width, Icon.Height);
				Vector2 imgBtnCenter = GetAbsolutePosition() + GetAbsoluteSize() / 2;
				Vector2 iconPos = imgBtnCenter - iconSize / 2;
				UI.Graphics.DrawImage(Icon, iconPos, 0f, 1f, iconTint);
				return;
			}

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

			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), EffectiveColor);

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
				FishColor textColor = GetColorOverride("Text", FishColor.Black);
				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, Txt, textPos, textColor);
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
				FishColor textColor = GetColorOverride("Text", FishColor.Black);
				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, Txt, center - TxtSz / 2, textColor);
			}

			//DrawChildren(UI, Dt, Time);
		}
	}
}
