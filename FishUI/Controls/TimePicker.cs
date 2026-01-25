using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Delegate for TimePicker value changed events.
	/// </summary>
	public delegate void TimePickerValueChangedFunc(TimePicker sender, TimeSpan value);

	/// <summary>
	/// A time picker control with hour, minute, and optional second spinners.
	/// </summary>
	public class TimePicker : Control
	{
		private TimeSpan _value = TimeSpan.Zero;
		private bool _use24HourFormat = true;
		private bool _showSeconds = false;

		// Internal spinner values
		private int _hour = 0;
		private int _minute = 0;
		private int _second = 0;
		private bool _isPM = false;

		// Hover states
		private int _hoveredSpinner = -1; // 0=hour, 1=minute, 2=second, 3=AM/PM
		private bool _hoveredUp = false;
		private bool _hoveredDown = false;

		/// <summary>
		/// Gets or sets the selected time value.
		/// </summary>
		[YamlMember]
		public TimeSpan Value
		{
			get => _value;
			set
			{
				// Clamp to valid range (0 to 23:59:59)
				TimeSpan clamped = value;
				if (clamped < TimeSpan.Zero)
					clamped = TimeSpan.Zero;
				if (clamped >= TimeSpan.FromDays(1))
					clamped = new TimeSpan(23, 59, 59);

				if (_value != clamped)
				{
					_value = clamped;
					UpdateSpinnersFromValue();
					OnValueChanged?.Invoke(this, _value);
				}
			}
		}

		/// <summary>
		/// Gets or sets whether to use 24-hour format (true) or 12-hour AM/PM format (false).
		/// </summary>
		[YamlMember]
		public bool Use24HourFormat
		{
			get => _use24HourFormat;
			set
			{
				if (_use24HourFormat != value)
				{
					_use24HourFormat = value;
					UpdateSpinnersFromValue();
				}
			}
		}

		/// <summary>
		/// Gets or sets whether to show the seconds spinner.
		/// </summary>
		[YamlMember]
		public bool ShowSeconds
		{
			get => _showSeconds;
			set => _showSeconds = value;
		}

		/// <summary>
		/// Width of each spinner section.
		/// </summary>
		[YamlMember]
		public float SpinnerWidth { get; set; } = 45f;

		/// <summary>
		/// Width of the up/down button area within each spinner.
		/// </summary>
		[YamlMember]
		public float ButtonWidth { get; set; } = 16f;

	/// <summary>
		/// Width of the AM/PM toggle button (only used in 12-hour mode).
		/// </summary>
		[YamlMember]
		public float AmPmWidth { get; set; } = 35f;

		/// <summary>
		/// Event raised when the selected time changes.
		/// </summary>
		public event TimePickerValueChangedFunc OnValueChanged;

		public TimePicker()
		{
			Size = new Vector2(GetPreferredWidth(), 24);
			Focusable = true;
			UpdateSpinnersFromValue();
		}

		public TimePicker(TimeSpan value) : this()
		{
			Value = value;
		}

		public TimePicker(int hours, int minutes, int seconds = 0) : this()
		{
			Value = new TimeSpan(hours, minutes, seconds);
		}

		/// <summary>
		/// Calculates the preferred width based on current settings.
		/// </summary>
		private float GetPreferredWidth()
		{
			float width = 0;
			float separatorW = 8f;
			float gap = 4f;

			// Hour spinner
			width += SpinnerWidth;

			// Separator + Minute spinner
			width += separatorW + SpinnerWidth;

			// Seconds (optional)
			if (_showSeconds)
			{
				width += separatorW + SpinnerWidth;
			}

			// AM/PM button (only in 12-hour mode)
			if (!_use24HourFormat)
			{
				width += gap + AmPmWidth;
			}

			return width;
		}

		/// <summary>
		/// Updates the control size based on current settings.
		/// Call this after changing Use24HourFormat or ShowSeconds.
		/// </summary>
		public void UpdateSize()
		{
			Size = new Vector2(GetPreferredWidth(), Size.Y);
		}

		private void UpdateSpinnersFromValue()
		{
			int totalHours = _value.Hours;
			_minute = _value.Minutes;
			_second = _value.Seconds;

			if (_use24HourFormat)
			{
				_hour = totalHours;
				_isPM = false;
			}
			else
			{
				// Convert to 12-hour format
				_isPM = totalHours >= 12;
				_hour = totalHours % 12;
				if (_hour == 0) _hour = 12;
			}
		}

		private void UpdateValueFromSpinners()
		{
			int totalHours;
			if (_use24HourFormat)
			{
				totalHours = _hour;
			}
			else
			{
				// Convert from 12-hour format
				totalHours = _hour % 12;
				if (_isPM) totalHours += 12;
			}

		TimeSpan newValue = new TimeSpan(totalHours, _minute, _second);
			if (_value != newValue)
			{
				_value = newValue;
				OnValueChanged?.Invoke(this, _value);
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			// Don't call base.DrawControl - we don't want a background rectangle
			// The separator areas should be transparent

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = ScaledSize;
			var font = UI.Settings.FontDefault;

			float spinnerW = Scale(SpinnerWidth);
			float btnW = Scale(ButtonWidth);
			float ampmW = Scale(AmPmWidth);
			float separatorW = Scale(8);

			float xOffset = 0;

			// Hour spinner
			DrawSpinner(UI, pos + new Vector2(xOffset, 0), spinnerW, size.Y, btnW,
				_hour.ToString(_use24HourFormat ? "D2" : "D1"), 0, font);
			xOffset += spinnerW;

			// Separator ":"
			if (font != null)
			{
				var sepSize = UI.Graphics.MeasureText(font, ":");
				float sepX = pos.X + xOffset + (separatorW - sepSize.X) / 2;
				float sepY = pos.Y + (size.Y - sepSize.Y) / 2;
				UI.Graphics.DrawTextColor(font, ":", new Vector2(sepX, sepY), new FishColor(0, 0, 0, 255));
			}
			xOffset += separatorW;

			// Minute spinner
			DrawSpinner(UI, pos + new Vector2(xOffset, 0), spinnerW, size.Y, btnW,
				_minute.ToString("D2"), 1, font);
			xOffset += spinnerW;

			// Seconds spinner (optional)
			if (_showSeconds)
			{
				// Separator ":"
				if (font != null)
				{
					var sepSize = UI.Graphics.MeasureText(font, ":");
					float sepX = pos.X + xOffset + (separatorW - sepSize.X) / 2;
					float sepY = pos.Y + (size.Y - sepSize.Y) / 2;
					UI.Graphics.DrawTextColor(font, ":", new Vector2(sepX, sepY), new FishColor(0, 0, 0, 255));
				}
				xOffset += separatorW;

				DrawSpinner(UI, pos + new Vector2(xOffset, 0), spinnerW, size.Y, btnW,
					_second.ToString("D2"), 2, font);
				xOffset += spinnerW;
			}

			// AM/PM toggle (only in 12-hour mode)
			if (!_use24HourFormat)
			{
				xOffset += Scale(4);
				DrawAmPmButton(UI, pos + new Vector2(xOffset, 0), ampmW, size.Y, font);
			}
		}

		private void DrawSpinner(FishUI UI, Vector2 pos, float width, float height, float btnWidth,
			string text, int spinnerIndex, FontRef font)
		{
			float textWidth = width - btnWidth;

			// Draw text background
			NPatch bg = HasFocus ? UI.Settings.ImgTextboxActive : UI.Settings.ImgTextboxNormal;
			if (bg != null)
			{
				UI.Graphics.DrawNPatch(bg, pos, new Vector2(textWidth, height), Color);
			}
			else
			{
				UI.Graphics.DrawRectangle(pos, new Vector2(textWidth, height), new FishColor(255, 255, 255, 255));
				UI.Graphics.DrawRectangleOutline(pos, new Vector2(textWidth, height), new FishColor(128, 128, 128, 255));
			}

			// Draw text centered
			if (font != null)
			{
				var textSize = UI.Graphics.MeasureText(font, text);
				float textX = pos.X + (textWidth - textSize.X) / 2;
				float textY = pos.Y + (height - textSize.Y) / 2;
				UI.Graphics.DrawTextColor(font, text, new Vector2(textX, textY), new FishColor(0, 0, 0, 255));
			}

			// Draw up/down buttons
			float halfHeight = height / 2;
			Vector2 upPos = new Vector2(pos.X + textWidth, pos.Y);
			Vector2 downPos = new Vector2(pos.X + textWidth, pos.Y + halfHeight);
			Vector2 btnSize = new Vector2(btnWidth, halfHeight);

			bool upHovered = _hoveredSpinner == spinnerIndex && _hoveredUp;
			bool downHovered = _hoveredSpinner == spinnerIndex && _hoveredDown;

			// Up button
			NPatch upBg = upHovered ? UI.Settings.ImgButtonHover : UI.Settings.ImgButtonNormal;
			if (upBg != null)
			{
				UI.Graphics.DrawNPatch(upBg, upPos, btnSize, Color);
			}
			else
			{
				FishColor btnColor = upHovered ? new FishColor(200, 200, 200, 255) : new FishColor(230, 230, 230, 255);
				UI.Graphics.DrawRectangle(upPos, btnSize, btnColor);
				UI.Graphics.DrawRectangleOutline(upPos, btnSize, new FishColor(160, 160, 160, 255));
			}

			// Down button
			NPatch downBg = downHovered ? UI.Settings.ImgButtonHover : UI.Settings.ImgButtonNormal;
			if (downBg != null)
			{
				UI.Graphics.DrawNPatch(downBg, downPos, btnSize, Color);
			}
			else
			{
				FishColor btnColor = downHovered ? new FishColor(200, 200, 200, 255) : new FishColor(230, 230, 230, 255);
				UI.Graphics.DrawRectangle(downPos, btnSize, btnColor);
				UI.Graphics.DrawRectangleOutline(downPos, btnSize, new FishColor(160, 160, 160, 255));
			}

		// Draw arrows using simple ASCII characters
			if (font != null)
			{
				var upArrow = "+";
				var downArrow = "-";
				var upSize = UI.Graphics.MeasureText(font, upArrow);
				var downSize = UI.Graphics.MeasureText(font, downArrow);

				float upX = upPos.X + (btnWidth - upSize.X) / 2;
				float upY = upPos.Y + (halfHeight - upSize.Y) / 2;
				UI.Graphics.DrawTextColor(font, upArrow, new Vector2(upX, upY), new FishColor(60, 60, 60, 255));

				float downX = downPos.X + (btnWidth - downSize.X) / 2;
				float downY = downPos.Y + (halfHeight - downSize.Y) / 2;
				UI.Graphics.DrawTextColor(font, downArrow, new Vector2(downX, downY), new FishColor(60, 60, 60, 255));
			}
		}

		private void DrawAmPmButton(FishUI UI, Vector2 pos, float width, float height, FontRef font)
		{
			bool hovered = _hoveredSpinner == 3;

			NPatch bg = hovered ? UI.Settings.ImgButtonHover : UI.Settings.ImgButtonNormal;
			if (bg != null)
			{
				UI.Graphics.DrawNPatch(bg, pos, new Vector2(width, height), Color);
			}
			else
			{
				FishColor btnColor = hovered ? new FishColor(200, 200, 200, 255) : new FishColor(230, 230, 230, 255);
				UI.Graphics.DrawRectangle(pos, new Vector2(width, height), btnColor);
				UI.Graphics.DrawRectangleOutline(pos, new Vector2(width, height), new FishColor(160, 160, 160, 255));
			}

			if (font != null)
			{
				string text = _isPM ? "PM" : "AM";
				var textSize = UI.Graphics.MeasureText(font, text);
				float textX = pos.X + (width - textSize.X) / 2;
				float textY = pos.Y + (height - textSize.Y) / 2;
				UI.Graphics.DrawTextColor(font, text, new Vector2(textX, textY), new FishColor(0, 0, 0, 255));
			}
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);
			UpdateHoverState(Pos);
		}

		private void UpdateHoverState(Vector2 mousePos)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = ScaledSize;

			float spinnerW = Scale(SpinnerWidth);
			float btnW = Scale(ButtonWidth);
			float separatorW = Scale(8);
			float ampmW = Scale(AmPmWidth);

			_hoveredSpinner = -1;
			_hoveredUp = false;
			_hoveredDown = false;

			float xOffset = 0;
			float halfHeight = size.Y / 2;

			// Check hour spinner
			if (CheckSpinnerHover(mousePos, pos + new Vector2(xOffset, 0), spinnerW, size.Y, btnW, 0))
				return;
			xOffset += spinnerW + separatorW;

			// Check minute spinner
			if (CheckSpinnerHover(mousePos, pos + new Vector2(xOffset, 0), spinnerW, size.Y, btnW, 1))
				return;
			xOffset += spinnerW;

			// Check seconds spinner
			if (_showSeconds)
			{
				xOffset += separatorW;
				if (CheckSpinnerHover(mousePos, pos + new Vector2(xOffset, 0), spinnerW, size.Y, btnW, 2))
					return;
				xOffset += spinnerW;
			}

			// Check AM/PM button
			if (!_use24HourFormat)
			{
				xOffset += Scale(4);
				Vector2 ampmPos = pos + new Vector2(xOffset, 0);
				if (mousePos.X >= ampmPos.X && mousePos.X < ampmPos.X + ampmW &&
					mousePos.Y >= ampmPos.Y && mousePos.Y < ampmPos.Y + size.Y)
				{
					_hoveredSpinner = 3;
				}
			}
		}

		private bool CheckSpinnerHover(Vector2 mousePos, Vector2 spinnerPos, float width, float height, float btnWidth, int spinnerIndex)
		{
			float textWidth = width - btnWidth;
			float halfHeight = height / 2;

			// Check button area
			Vector2 btnAreaPos = new Vector2(spinnerPos.X + textWidth, spinnerPos.Y);
			if (mousePos.X >= btnAreaPos.X && mousePos.X < btnAreaPos.X + btnWidth &&
				mousePos.Y >= btnAreaPos.Y && mousePos.Y < btnAreaPos.Y + height)
			{
				_hoveredSpinner = spinnerIndex;
				_hoveredUp = mousePos.Y < btnAreaPos.Y + halfHeight;
				_hoveredDown = !_hoveredUp;
				return true;
			}

			return false;
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			if (Btn != FishMouseButton.Left)
				return;

			UpdateHoverState(Pos);

			if (_hoveredSpinner >= 0 && _hoveredSpinner <= 2)
			{
				int delta = _hoveredUp ? 1 : (_hoveredDown ? -1 : 0);
				if (delta != 0)
				{
					AdjustSpinner(_hoveredSpinner, delta);
				}
			}
			else if (_hoveredSpinner == 3)
			{
				// Toggle AM/PM
				_isPM = !_isPM;
				UpdateValueFromSpinners();
			}
		}

		private void AdjustSpinner(int spinnerIndex, int delta)
		{
			switch (spinnerIndex)
			{
				case 0: // Hour
					if (_use24HourFormat)
					{
						_hour = (_hour + delta + 24) % 24;
					}
					else
					{
						_hour = _hour + delta;
						if (_hour > 12) _hour = 1;
						if (_hour < 1) _hour = 12;
					}
					break;
				case 1: // Minute
					_minute = (_minute + delta + 60) % 60;
					break;
				case 2: // Second
					_second = (_second + delta + 60) % 60;
					break;
			}

			UpdateValueFromSpinners();
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float Delta)
		{
			base.HandleMouseWheel(UI, InState, Delta);

			if (_hoveredSpinner >= 0 && _hoveredSpinner <= 2)
			{
				int delta = Delta > 0 ? 1 : -1;
				AdjustSpinner(_hoveredSpinner, delta);
			}
			else if (_hoveredSpinner == 3)
			{
				// Toggle AM/PM with wheel
				_isPM = !_isPM;
				UpdateValueFromSpinners();
			}
		}

		public override void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
			base.HandleKeyPress(UI, InState, Key);

			if (!HasFocus)
				return;

			switch (Key)
			{
				case FishKey.Up:
					if (_hoveredSpinner >= 0 && _hoveredSpinner <= 2)
						AdjustSpinner(_hoveredSpinner, 1);
					break;
				case FishKey.Down:
					if (_hoveredSpinner >= 0 && _hoveredSpinner <= 2)
						AdjustSpinner(_hoveredSpinner, -1);
					break;
			}
		}

		/// <summary>
		/// Gets the formatted time string.
		/// </summary>
		public string GetFormattedTime()
		{
			if (_use24HourFormat)
			{
				if (_showSeconds)
					return _value.ToString(@"hh\:mm\:ss");
				else
					return _value.ToString(@"hh\:mm");
			}
			else
			{
				string ampm = _isPM ? "PM" : "AM";
				if (_showSeconds)
					return $"{_hour}:{_minute:D2}:{_second:D2} {ampm}";
				else
					return $"{_hour}:{_minute:D2} {ampm}";
			}
		}
	}
}
