using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void ToggleSwitchChangedFunc(ToggleSwitch Sender, bool IsOn);

	public class ToggleSwitch : Control
	{
		/// <summary>
		/// Whether the toggle switch is currently on
		/// </summary>
		[YamlMember]
		public bool IsOn
		{
			get => _isOn;
			set
			{
				if (_isOn != value)
				{
					_isOn = value;
					OnToggleChanged?.Invoke(this, _isOn);
				}
			}
		}
		private bool _isOn = false;

		/// <summary>
		/// Background color when the switch is off
		/// </summary>
		[YamlMember]
		public FishColor OffColor { get; set; } = new FishColor(120, 120, 120, 255);

		/// <summary>
		/// Background color when the switch is on
		/// </summary>
		[YamlMember]
		public FishColor OnColor { get; set; } = new FishColor(76, 175, 80, 255);

		/// <summary>
		/// Color of the thumb/knob
		/// </summary>
		[YamlMember]
		public FishColor ThumbColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Border color of the switch
		/// </summary>
		[YamlMember]
		public FishColor BorderColor { get; set; } = new FishColor(80, 80, 80, 255);

		/// <summary>
		/// Whether to show the optional on/off labels
		/// </summary>
		[YamlMember]
		public bool ShowLabels { get; set; } = false;

		/// <summary>
		/// Text displayed when the switch is on
		/// </summary>
		[YamlMember]
		public string OnLabel { get; set; } = "ON";

		/// <summary>
		/// Text displayed when the switch is off
		/// </summary>
		[YamlMember]
		public string OffLabel { get; set; } = "OFF";

		/// <summary>
		/// Label text color
		/// </summary>
		[YamlMember]
		public FishColor LabelColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Animation speed for the toggle transition (0 = instant, higher = slower)
		/// </summary>
		[YamlMember]
		public float AnimationSpeed { get; set; } = 8.0f;

		/// <summary>
		/// When true, uses colors from the current theme's color palette instead of the control's color properties.
		/// </summary>
		[YamlMember]
		public bool UseThemeColors { get; set; } = true;

		/// <summary>
		/// Event fired when the toggle state changes
		/// </summary>
		public event ToggleSwitchChangedFunc OnToggleChanged;

		[YamlIgnore]
		private float _animationPosition = 0f;

		public ToggleSwitch()
		{
			Size = new Vector2(50, 24);
		}

		private FishColor GetOnColor(FishUI UI)
		{
			if (UseThemeColors && UI.Settings.CurrentTheme != null)
				return UI.Settings.GetColorPalette().Success;
			return OnColor;
		}

		private FishColor GetOffColor(FishUI UI)
		{
			if (UseThemeColors && UI.Settings.CurrentTheme != null)
				return UI.Settings.GetColorPalette().Disabled;
			return OffColor;
		}

		private FishColor GetThumbColor(FishUI UI)
		{
			if (UseThemeColors && UI.Settings.CurrentTheme != null)
				return FishColor.White;
			return ThumbColor;
		}

		private FishColor GetBorderColor(FishUI UI)
		{
			if (UseThemeColors && UI.Settings.CurrentTheme != null)
				return UI.Settings.GetColorPalette().Border;
			return BorderColor;
		}

		private FishColor GetLabelColor(FishUI UI)
		{
			if (UseThemeColors && UI.Settings.CurrentTheme != null)
				return FishColor.White;
			return LabelColor;
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left && !Disabled)
			{
				IsOn = !IsOn;
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			// Animate the thumb position
			float targetPosition = IsOn ? 1f : 0f;
			if (AnimationSpeed > 0)
			{
				float diff = targetPosition - _animationPosition;
				_animationPosition += diff * Math.Min(1f, Dt * AnimationSpeed);
			}
			else
			{
				_animationPosition = targetPosition;
			}

			// Draw background track using NPatch if available
			bool useNPatch = false;
			if (IsOn && UI.Settings.ImgToggleSwitchTrackOn != null)
			{
				UI.Graphics.DrawNPatch(UI.Settings.ImgToggleSwitchTrackOn, pos, size, FishColor.White);
				useNPatch = true;
			}
			else if (!IsOn && UI.Settings.ImgToggleSwitchTrackOff != null)
			{
				UI.Graphics.DrawNPatch(UI.Settings.ImgToggleSwitchTrackOff, pos, size, FishColor.White);
				useNPatch = true;
			}
			else
			{
				// Calculate colors with interpolation for smooth transition
				FishColor currentBgColor = LerpColor(GetOffColor(UI), GetOnColor(UI), _animationPosition);

				// Apply disabled state
				if (Disabled)
				{
					currentBgColor = new FishColor(
						(byte)(currentBgColor.R / 2),
						(byte)(currentBgColor.G / 2),
						(byte)(currentBgColor.B / 2),
						currentBgColor.A
					);
				}

				UI.Graphics.DrawRectangle(pos, size, currentBgColor);
				UI.Graphics.DrawRectangleOutline(pos, size, GetBorderColor(UI));
			}

			// Calculate thumb dimensions and position
			float thumbPadding = 2f;
			float thumbSize = size.Y - (thumbPadding * 2);
			float thumbTravel = size.X - thumbSize - (thumbPadding * 2);
			float thumbX = pos.X + thumbPadding + (thumbTravel * _animationPosition);
			float thumbY = pos.Y + thumbPadding;

			Vector2 thumbPos = new Vector2(thumbX, thumbY);
			Vector2 thumbDimensions = new Vector2(thumbSize, thumbSize);

			// Draw thumb using NPatch if available
			if (UI.Settings.ImgToggleSwitchThumb != null)
			{
				UI.Graphics.DrawNPatch(UI.Settings.ImgToggleSwitchThumb, thumbPos, thumbDimensions, FishColor.White);
			}
			else
			{
				// Draw thumb with hover effect
				FishColor currentThumbColor = GetThumbColor(UI);
				if (IsMouseInside && !Disabled)
				{
					currentThumbColor = new FishColor(
						(byte)Math.Min(255, currentThumbColor.R + 20),
						(byte)Math.Min(255, currentThumbColor.G + 20),
						(byte)Math.Min(255, currentThumbColor.B + 20),
						currentThumbColor.A
					);
				}

				UI.Graphics.DrawRectangle(thumbPos, thumbDimensions, currentThumbColor);
			}

			// Draw optional labels
			if (ShowLabels)
			{
				string label = IsOn ? OnLabel : OffLabel;
				Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, label);

				// Position label on the opposite side of the thumb
				float labelX;
				if (IsOn)
				{
					// Label on the left side when on
					labelX = pos.X + (size.X - thumbSize - thumbPadding * 2 - textSize.X) / 2;
				}
				else
				{
					// Label on the right side when off
					labelX = pos.X + thumbSize + thumbPadding * 2 + (size.X - thumbSize - thumbPadding * 2 - textSize.X) / 2;
				}

				float labelY = pos.Y + (size.Y - textSize.Y) / 2;
				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, label, new Vector2(labelX, labelY), GetLabelColor(UI));
			}
		}

		private static FishColor LerpColor(FishColor a, FishColor b, float t)
		{
			return new FishColor(
				(byte)(a.R + (b.R - a.R) * t),
				(byte)(a.G + (b.G - a.G) * t),
				(byte)(a.B + (b.B - a.B) * t),
				(byte)(a.A + (b.A - a.A) * t)
			);
		}
	}
}
