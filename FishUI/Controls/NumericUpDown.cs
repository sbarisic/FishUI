using System;
using System.Globalization;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void NumericValueChangedFunc(NumericUpDown Sender, float Value);

	/// <summary>
	/// A numeric input control with up/down increment buttons.
	/// </summary>
	public class NumericUpDown : Control
	{
		/// <summary>
		/// The current numeric value.
		/// </summary>
		[YamlMember]
		public float Value
		{
			get => _value;
			set
			{
				float newValue = Math.Clamp(value, MinValue, MaxValue);
				if (_value != newValue)
				{
					_value = newValue;
					UpdateTextFromValue();
					OnValueChanged?.Invoke(this, _value);
				}
			}
		}
		private float _value = 0f;

		/// <summary>
		/// Minimum allowed value.
		/// </summary>
		[YamlMember]
		public float MinValue { get; set; } = 0f;

		/// <summary>
		/// Maximum allowed value.
		/// </summary>
		[YamlMember]
		public float MaxValue { get; set; } = 100f;

		/// <summary>
		/// Step increment for up/down buttons and arrow keys.
		/// </summary>
		[YamlMember]
		public float Step { get; set; } = 1f;

		/// <summary>
		/// Number of decimal places to display.
		/// </summary>
		[YamlMember]
		public int DecimalPlaces { get; set; } = 0;

		/// <summary>
		/// Width of the up/down button area.
		/// </summary>
		[YamlMember]
		public float ButtonWidth { get; set; } = 16f;

		/// <summary>
		/// Event raised when the value changes.
		/// </summary>
		public event NumericValueChangedFunc OnValueChanged;

		private Textbox _textbox;
		private bool _upButtonHovered = false;
		private bool _upButtonPressed = false;
		private bool _downButtonHovered = false;
		private bool _downButtonPressed = false;

		public NumericUpDown()
		{
			Size = new Vector2(120, 24);
			Focusable = true;
			CreateInternalControls();
		}

		public NumericUpDown(float value, float minValue = 0f, float maxValue = 100f, float step = 1f) : this()
		{
			MinValue = minValue;
			MaxValue = maxValue;
			Step = step;
			Value = value;
		}

		private void CreateInternalControls()
		{
			_textbox = new Textbox()
			{
				Position = Vector2.Zero,
				Size = new Vector2(Size.X - ButtonWidth, Size.Y),
				Text = FormatValue(_value)
			};

			_textbox.OnTextChanged += OnTextboxTextChanged;
			base.AddChild(_textbox);
		}

		private void OnTextboxTextChanged(Textbox sender, string text)
		{
			if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
			{
				float clamped = Math.Clamp(parsed, MinValue, MaxValue);
				if (_value != clamped)
				{
					_value = clamped;
					OnValueChanged?.Invoke(this, _value);
				}
			}
		}

		private void UpdateTextFromValue()
		{
			if (_textbox != null)
			{
				_textbox.Text = FormatValue(_value);
			}
		}

		private string FormatValue(float value)
		{
			return DecimalPlaces > 0
				? value.ToString($"F{DecimalPlaces}", CultureInfo.InvariantCulture)
				: ((int)value).ToString(CultureInfo.InvariantCulture);
		}

		private void UpdateInternalSizes()
		{
			if (_textbox != null)
			{
				_textbox.Size = new Vector2(Size.X - ButtonWidth, Size.Y);
			}
		}

		/// <summary>
		/// Increment the value by Step.
		/// </summary>
		public void Increment()
		{
			Value += Step;
		}

		/// <summary>
		/// Decrement the value by Step.
		/// </summary>
		public void Decrement()
		{
			Value -= Step;
		}

		private Vector2 GetUpButtonPosition()
		{
			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();
			return new Vector2(absPos.X + absSize.X - ButtonWidth, absPos.Y);
		}

		private Vector2 GetDownButtonPosition()
		{
			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();
			return new Vector2(absPos.X + absSize.X - ButtonWidth, absPos.Y + absSize.Y / 2);
		}

		private Vector2 GetButtonSize()
		{
			return new Vector2(ButtonWidth, GetAbsoluteSize().Y / 2);
		}

		private bool IsPointInUpButton(Vector2 point)
		{
			Vector2 pos = GetUpButtonPosition();
			Vector2 size = GetButtonSize();
			return Utils.IsInside(pos, size, point);
		}

		private bool IsPointInDownButton(Vector2 point)
		{
			Vector2 pos = GetDownButtonPosition();
			Vector2 size = GetButtonSize();
			return Utils.IsInside(pos, size, point);
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);
			_upButtonHovered = IsPointInUpButton(Pos);
			_downButtonHovered = IsPointInDownButton(Pos);
		}

		public override void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			base.HandleMouseLeave(UI, InState);
			_upButtonHovered = false;
			_downButtonHovered = false;
			_upButtonPressed = false;
			_downButtonPressed = false;
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				if (IsPointInUpButton(Pos))
				{
					_upButtonPressed = true;
				}
				else if (IsPointInDownButton(Pos))
				{
					_downButtonPressed = true;
				}
			}
		}

		public override void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseRelease(UI, InState, Btn, Pos);
			_upButtonPressed = false;
			_downButtonPressed = false;
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				if (IsPointInUpButton(Pos))
				{
					Increment();
				}
				else if (IsPointInDownButton(Pos))
				{
					Decrement();
				}
			}
		}

		public override void HandleKeyDown(FishUI UI, FishInputState InState, int KeyCode)
		{
			base.HandleKeyDown(UI, InState, KeyCode);

			FishKey key = (FishKey)KeyCode;
			if (key == FishKey.Up)
			{
				Increment();
			}
			else if (key == FishKey.Down)
			{
				Decrement();
			}
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float WheelDelta)
		{
			base.HandleMouseWheel(UI, InState, WheelDelta);

			// Scroll up to increment, scroll down to decrement
			if (WheelDelta > 0)
			{
				Increment();
			}
			else if (WheelDelta < 0)
			{
				Decrement();
			}
		}

		public override void HandleBlur()
		{
			base.HandleBlur();
			// Ensure value is properly formatted when focus leaves
			UpdateTextFromValue();
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			UpdateInternalSizes();

			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();

			Vector2 upPos = GetUpButtonPosition();
			Vector2 downPos = GetDownButtonPosition();
			Vector2 btnSize = GetButtonSize();

			// Draw up button
			NPatch upImg;
			if (Disabled)
				upImg = UI.Settings.ImgNumericUpDownUpDisabled;
			else if (_upButtonPressed)
				upImg = UI.Settings.ImgNumericUpDownUpPressed;
			else if (_upButtonHovered)
				upImg = UI.Settings.ImgNumericUpDownUpHover;
			else
				upImg = UI.Settings.ImgNumericUpDownUpNormal;

			if (upImg != null)
			{
				UI.Graphics.DrawNPatch(upImg, upPos, btnSize, Color);
			}
			else
			{
				// Fallback
				FishColor btnColor = _upButtonHovered ? new FishColor(80, 80, 80) : new FishColor(60, 60, 60);
				UI.Graphics.DrawRectangle(upPos, btnSize, btnColor);
				UI.Graphics.DrawText(UI.Settings.FontDefault, "?", upPos + new Vector2(btnSize.X / 2 - 4, 1));
			}

			// Draw down button
			NPatch downImg;
			if (Disabled)
				downImg = UI.Settings.ImgNumericUpDownDownDisabled;
			else if (_downButtonPressed)
				downImg = UI.Settings.ImgNumericUpDownDownPressed;
			else if (_downButtonHovered)
				downImg = UI.Settings.ImgNumericUpDownDownHover;
			else
				downImg = UI.Settings.ImgNumericUpDownDownNormal;

			if (downImg != null)
			{
				UI.Graphics.DrawNPatch(downImg, downPos, btnSize, Color);
			}
			else
			{
				// Fallback
				FishColor btnColor = _downButtonHovered ? new FishColor(80, 80, 80) : new FishColor(60, 60, 60);
				UI.Graphics.DrawRectangle(downPos, btnSize, btnColor);
				UI.Graphics.DrawText(UI.Settings.FontDefault, "?", downPos + new Vector2(btnSize.X / 2 - 4, 1));
			}
		}
	}
}
