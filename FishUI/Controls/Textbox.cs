using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void TextboxTextChangedFunc(Textbox Sender, string Text);

	public class Textbox : Control
	{
		private string _text = "";

		/// <summary>
		/// The text content of the textbox.
		/// </summary>
		[YamlMember]
		public string Text
		{
			get => _text ?? "";
			set
			{
				string newValue = value ?? "";
				if (MaxLength > 0 && newValue.Length > MaxLength)
					newValue = newValue.Substring(0, MaxLength);
				if (_text != newValue)
				{
					string oldValue = _text;
					_text = newValue;
					// Clamp cursor position to valid range
					CursorPosition = Math.Clamp(CursorPosition, 0, _text.Length);
					ClearSelection();
					OnTextChanged?.Invoke(this, _text);

					// Invoke serialized text changed handler
					InvokeHandler(OnTextChangedHandler, new TextChangedEventHandlerArgs(FishUI, oldValue, _text));
				}
			}
		}

		/// <summary>
		/// Current cursor position in the text (0 = before first character).
		/// </summary>
		[YamlIgnore]
		public int CursorPosition { get; set; } = 0;

		/// <summary>
		/// Start index of the text selection.
		/// </summary>
		[YamlIgnore]
		public int SelectionStart { get; set; } = 0;

		/// <summary>
		/// Length of the text selection (can be negative for backward selection).
		/// </summary>
		[YamlIgnore]
		public int SelectionLength { get; set; } = 0;

		/// <summary>
		/// If true, displays asterisks instead of actual characters.
		/// </summary>
		[YamlMember]
		public bool PasswordMode { get; set; } = false;

		/// <summary>
		/// The character used to mask password text.
		/// </summary>
		[YamlMember]
		public char PasswordChar { get; set; } = '*';

		/// <summary>
		/// Placeholder text displayed when the textbox is empty.
		/// </summary>
		[YamlMember]
		public string Placeholder { get; set; } = "";

		/// <summary>
		/// Color of the placeholder text.
		/// </summary>
		[YamlMember]
		public FishColor PlaceholderColor { get; set; } = new FishColor(128, 128, 128, 255);

		/// <summary>
		/// Maximum number of characters allowed. 0 = unlimited.
		/// </summary>
		[YamlMember]
		public int MaxLength { get; set; } = 0;

		/// <summary>
		/// If true, the textbox cannot be edited.
		/// </summary>
		[YamlMember]
		public bool ReadOnly { get; set; } = false;

		/// <summary>
		/// Color of the selection highlight.
		/// </summary>
		[YamlMember]
		public FishColor SelectionColor { get; set; } = new FishColor(51, 153, 255, 128);

		/// <summary>
		/// Event fired when the text changes.
		/// </summary>
		public event TextboxTextChangedFunc OnTextChanged;

		// For drag selection
		[YamlIgnore]
		private bool _isSelecting = false;
		[YamlIgnore]
		private int _selectionAnchor = 0;

		// Cached for mouse position calculations
		[YamlIgnore]
		private Vector2 _lastTextPos;
		[YamlIgnore]
		private FontRef _lastFont;

		public Textbox()
		{
			Size = new Vector2(200, 19);
			Focusable = true;
		}

		public Textbox(string Text) : this()
		{
			this.Text = Text;
		}

		/// <summary>
		/// Returns true if there is any text selected.
		/// </summary>
		[YamlIgnore]
		public bool HasSelection => SelectionLength != 0;

		/// <summary>
		/// Gets the normalized selection range (start, end) where start is always less than end.
		/// </summary>
		public (int Start, int End) GetSelectionRange()
		{
			int start = SelectionStart;
			int end = SelectionStart + SelectionLength;
			if (start > end)
				(start, end) = (end, start);
			return (Math.Max(0, start), Math.Min(Text.Length, end));
		}

		/// <summary>
		/// Gets the currently selected text.
		/// </summary>
		public string GetSelectedText()
		{
			if (!HasSelection)
				return "";
			var (start, end) = GetSelectionRange();
			return Text.Substring(start, end - start);
		}

		/// <summary>
		/// Selects all text in the textbox.
		/// </summary>
		public void SelectAll()
		{
			if (Text.Length > 0)
			{
				SelectionStart = 0;
				SelectionLength = Text.Length;
				CursorPosition = Text.Length;
			}
		}

		/// <summary>
		/// Clears the current selection.
		/// </summary>
		public void ClearSelection()
		{
			SelectionStart = CursorPosition;
			SelectionLength = 0;
		}

		/// <summary>
		/// Copies the selected text to the clipboard (returns the text for external clipboard handling).
		/// </summary>
		public string Copy()
		{
			return GetSelectedText();
		}

		/// <summary>
		/// Cuts the selected text (returns the text for external clipboard handling).
		/// </summary>
		public string Cut()
		{
			if (ReadOnly || !HasSelection)
				return "";

			string selectedText = GetSelectedText();
			DeleteSelection();
			return selectedText;
		}

		/// <summary>
		/// Pastes text at the current cursor position, replacing any selection.
		/// </summary>
		public void Paste(string text)
		{
			if (ReadOnly || string.IsNullOrEmpty(text))
				return;

			// Delete selection first if any
			if (HasSelection)
				DeleteSelection();

			// Apply max length constraint
			if (MaxLength > 0)
			{
				int availableSpace = MaxLength - Text.Length;
				if (availableSpace <= 0)
					return;
				if (text.Length > availableSpace)
					text = text.Substring(0, availableSpace);
			}

			// Insert text at cursor position
			_text = Text.Insert(CursorPosition, text);
			CursorPosition += text.Length;
			ClearSelection();
			OnTextChanged?.Invoke(this, _text);
		}

		/// <summary>
		/// Deletes the currently selected text.
		/// </summary>
		private void DeleteSelection()
		{
			if (!HasSelection)
				return;

			var (start, end) = GetSelectionRange();
			_text = Text.Remove(start, end - start);
			CursorPosition = start;
			ClearSelection();
			OnTextChanged?.Invoke(this, _text);
		}

		/// <summary>
		/// Gets the display text (with password masking if enabled).
		/// </summary>
		private string GetDisplayText()
		{
			if (PasswordMode && !string.IsNullOrEmpty(Text))
				return new string(PasswordChar, Text.Length);
			return Text;
		}

		/// <summary>
		/// Calculates the cursor position from a mouse X coordinate.
		/// </summary>
		private int GetCursorPositionFromX(FishUI UI, float mouseX)
		{
			if (_lastFont == null)
				return 0;

			string displayText = GetDisplayText();
			float textStartX = _lastTextPos.X;

			// If click is before text start, return 0
			if (mouseX <= textStartX)
				return 0;

			// Find the closest character position
			for (int i = 0; i <= displayText.Length; i++)
			{
				string substring = displayText.Substring(0, i);
				float charX = textStartX + UI.Graphics.MeasureText(_lastFont, substring).X;

				if (mouseX < charX)
				{
					// Check if we're closer to this position or the previous one
					if (i > 0)
					{
						string prevSubstring = displayText.Substring(0, i - 1);
						float prevCharX = textStartX + UI.Graphics.MeasureText(_lastFont, prevSubstring).X;
						if (mouseX - prevCharX < charX - mouseX)
							return i - 1;
					}
					return i;
				}
			}

			return displayText.Length;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			NPatch Cur = UI.Settings.ImgTextboxNormal;

			if (Disabled)
				Cur = UI.Settings.ImgTextboxDisabled;
			else if (UI.InputActiveControl == this)
				Cur = UI.Settings.ImgTextboxActive;

			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			UI.Graphics.PushScissor(GetAbsolutePosition(), GetAbsoluteSize());

			string displayText = GetDisplayText();
			bool showPlaceholder = string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Placeholder);
			string textToDraw = showPlaceholder ? Placeholder : displayText;

			FontRef font = UI.Settings.FontTextboxDefault;
			_lastFont = font;
			Vector2 txtSz = UI.Graphics.MeasureText(font, textToDraw);
			Vector2 txtPos = (GetAbsolutePosition() + new Vector2(Cur.Left + 4, GetAbsoluteSize().Y / 2)) - new Vector2(0, txtSz.Y / 2);
			_lastTextPos = txtPos;

			// Draw selection highlight
			if (HasSelection && UI.InputActiveControl == this && !showPlaceholder)
			{
				var (selStart, selEnd) = GetSelectionRange();
				string beforeSel = displayText.Substring(0, selStart);
				string selText = displayText.Substring(selStart, selEnd - selStart);

				float selStartX = txtPos.X + UI.Graphics.MeasureText(font, beforeSel).X;
				float selWidth = UI.Graphics.MeasureText(font, selText).X;

				UI.Graphics.DrawRectangle(
					new Vector2(selStartX, txtPos.Y),
					new Vector2(selWidth, txtSz.Y),
					SelectionColor
				);
			}

			// Draw text
			if (showPlaceholder)
				UI.Graphics.DrawTextColor(font, textToDraw, txtPos, PlaceholderColor);
			else
				UI.Graphics.DrawText(font, textToDraw, txtPos);

			UI.Graphics.PopScissor();

			// Draw cursor
			bool drawCursor = false;
			if (UI.InputActiveControl == this && !showPlaceholder)
				drawCursor = MathF.Sin(Time * 5) > 0;

			if (drawCursor || (UI.InputActiveControl == this && showPlaceholder))
			{
				// Calculate cursor X position
				string textBeforeCursor = displayText.Substring(0, Math.Min(CursorPosition, displayText.Length));
				float cursorX = txtPos.X + UI.Graphics.MeasureText(font, textBeforeCursor).X;

				float cursorHeight = txtSz.Y > 0 ? txtSz.Y : GetAbsoluteSize().Y - 4;
				Vector2 cursorStart = new Vector2(cursorX, txtPos.Y);
				Vector2 cursorEnd = new Vector2(cursorX, txtPos.Y + cursorHeight);

				if (drawCursor || showPlaceholder)
					UI.Graphics.DrawLine(cursorStart, cursorEnd, 1, FishColor.Black);
			}
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				// Set cursor position based on click location
				CursorPosition = GetCursorPositionFromX(UI, Pos.X);
				_selectionAnchor = CursorPosition;
				_isSelecting = true;
				ClearSelection();
			}
		}

		public override void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseRelease(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				_isSelecting = false;
			}
		}

		public override void HandleDrag(FishUI UI, Vector2 StartPos, Vector2 EndPos, FishInputState InState)
		{
			if (_isSelecting && UI.InputActiveControl == this)
			{
				int newPos = GetCursorPositionFromX(UI, EndPos.X);
				CursorPosition = newPos;
				SelectionStart = _selectionAnchor;
				SelectionLength = newPos - _selectionAnchor;
			}
		}

		public override void HandleMouseDoubleClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseDoubleClick(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left && UI.InputActiveControl == this)
			{
				// Double-click selects all text
				SelectAll();
			}
		}

		public override void HandleTextInput(FishUI UI, FishInputState InState, char Chr)
		{
			if (UI.InputActiveControl != this)
				return;

			// Handle Ctrl key combinations
			if (InState.CtrlDown)
			{
				switch (char.ToLower(Chr))
				{
					case 'a': // Select All
						SelectAll();
						return;
					case 'c': // Copy (handled by clipboard, we just provide the text)
							  // Copy() returns the text - actual clipboard integration is external
						return;
					case 'v': // Paste (handled externally, text comes through normal input or Paste method)
						return;
					case 'x': // Cut
						if (!ReadOnly)
							Cut();
						return;
				}
			}

			if (ReadOnly)
				return;

			if (Chr == '\b') // Backspace
			{
				if (HasSelection)
				{
					DeleteSelection();
				}
				else if (CursorPosition > 0)
				{
					_text = Text.Remove(CursorPosition - 1, 1);
					CursorPosition--;
					OnTextChanged?.Invoke(this, _text);
				}
			}
			else if (Chr == 127) // Delete key (sometimes sent as 127)
			{
				if (HasSelection)
				{
					DeleteSelection();
				}
				else if (CursorPosition < Text.Length)
				{
					_text = Text.Remove(CursorPosition, 1);
					OnTextChanged?.Invoke(this, _text);
				}
			}
			else if (!char.IsControl(Chr) || Chr == '\t')
			{
				// Delete selection first if any
				if (HasSelection)
					DeleteSelection();

				// Check max length
				if (MaxLength > 0 && Text.Length >= MaxLength)
					return;

				// Insert character at cursor position
				_text = Text.Insert(CursorPosition, Chr.ToString());
				CursorPosition++;
				OnTextChanged?.Invoke(this, _text);
			}
		}

		public override void HandleKeyDown(FishUI UI, FishInputState InState, int KeyCode)
		{
			base.HandleKeyDown(UI, InState, KeyCode);

			if (UI.InputActiveControl != this)
				return;

			bool shift = InState.ShiftDown;

			// Arrow key handling (key codes may vary by backend)
			// Common key codes: Left=263, Right=262, Home=268, End=269, Delete=261
			switch (KeyCode)
			{
				case 263: // Left Arrow
					if (CursorPosition > 0)
					{
						if (shift)
						{
							if (!HasSelection)
							{
								SelectionStart = CursorPosition;
							}
							CursorPosition--;
							SelectionLength = CursorPosition - SelectionStart;
						}
						else
						{
							if (HasSelection)
							{
								var (start, _) = GetSelectionRange();
								CursorPosition = start;
								ClearSelection();
							}
							else
							{
								CursorPosition--;
							}
						}
					}
					break;

				case 262: // Right Arrow
					if (CursorPosition < Text.Length)
					{
						if (shift)
						{
							if (!HasSelection)
							{
								SelectionStart = CursorPosition;
							}
							CursorPosition++;
							SelectionLength = CursorPosition - SelectionStart;
						}
						else
						{
							if (HasSelection)
							{
								var (_, end) = GetSelectionRange();
								CursorPosition = end;
								ClearSelection();
							}
							else
							{
								CursorPosition++;
							}
						}
					}
					break;

				case 268: // Home
					if (shift)
					{
						if (!HasSelection)
							SelectionStart = CursorPosition;
						SelectionLength = -SelectionStart;
						CursorPosition = 0;
						SelectionLength = CursorPosition - SelectionStart;
					}
					else
					{
						CursorPosition = 0;
						ClearSelection();
					}
					break;

				case 269: // End
					if (shift)
					{
						if (!HasSelection)
							SelectionStart = CursorPosition;
						CursorPosition = Text.Length;
						SelectionLength = CursorPosition - SelectionStart;
					}
					else
					{
						CursorPosition = Text.Length;
						ClearSelection();
					}
					break;

				case 261: // Delete
					if (!ReadOnly)
					{
						if (HasSelection)
						{
							DeleteSelection();
						}
						else if (CursorPosition < Text.Length)
						{
							_text = Text.Remove(CursorPosition, 1);
							OnTextChanged?.Invoke(this, _text);
						}
					}
					break;
			}
		}
	}
}
