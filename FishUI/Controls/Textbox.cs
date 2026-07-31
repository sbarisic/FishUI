using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void TextboxTextChangedFunc(Textbox Sender, string Text);

	public class Textbox : Control, IFishUIDebugSnapshotProvider, IFishUIDebugPrivacyProvider
	{
		private const float LayoutEpsilon = 0.01f;
		private const float CaretRevealMargin = 2f;

		private readonly struct TextboxViewport
		{
			public NPatch Patch { get; }
			public FontRef Font { get; }
			public string DisplayText { get; }
			public string TextToDraw { get; }
			public bool ShowPlaceholder { get; }
			public Vector2 Position { get; }
			public Vector2 Size { get; }
			public Vector2 TextPosition { get; }
			public Vector2 TextSize { get; }

			public TextboxViewport(NPatch patch, FontRef font, string displayText, string textToDraw,
				bool showPlaceholder, Vector2 position, Vector2 size, Vector2 textPosition, Vector2 textSize)
			{
				Patch = patch;
				Font = font;
				DisplayText = displayText;
				TextToDraw = textToDraw;
				ShowPlaceholder = showPlaceholder;
				Position = position;
				Size = size;
				TextPosition = textPosition;
				TextSize = textSize;
			}
		}

		private string _text = "";
		private int _cursorPosition;
		private float _horizontalScrollOffsetPixels;
		private bool _caretVisibilityPending = true;

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
					if (FishUI?.Diagnostics.Events.Options.Enabled == true)
						FishUI.Diagnostics.Record(FishUIDiagnosticEventCategory.StateChange,
							FishUIDiagnosticEventType.StateChanged, this, null,
							state: new FishUIStateEventData { Name = "textLength", OldValue = oldValue?.Length.ToString(), NewValue = _text.Length.ToString() });
					// Clamp cursor position to valid range
					CursorPosition = Math.Clamp(CursorPosition, 0, _text.Length);
					_caretVisibilityPending = true;
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
		public int CursorPosition
		{
			get => _cursorPosition;
			set
			{
				int newValue = Math.Clamp(value, 0, Text.Length);
				if (_cursorPosition == newValue)
					return;

				int oldValue = _cursorPosition;
				_cursorPosition = newValue;
				_caretVisibilityPending = true;
				if (FishUI?.Diagnostics.Events.Options.Enabled == true)
					FishUI.Diagnostics.Record(FishUIDiagnosticEventCategory.StateChange,
						FishUIDiagnosticEventType.StateChanged, this, null,
						state: new FishUIStateEventData { Name = "cursorPosition", OldValue = oldValue.ToString(), NewValue = newValue.ToString() });
			}
		}

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

		[YamlMember]
		public FishColor? TextColorOverride { get; set; }

		[YamlMember]
		public FishColor? CursorColorOverride { get; set; }

		/// <summary>
		/// Event fired when the text changes.
		/// </summary>
		public event TextboxTextChangedFunc OnTextChanged;

		// For drag selection
		[YamlIgnore]
		private bool _isSelecting = false;
		[YamlIgnore]
		private int _selectionAnchor = 0;

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
			TextboxViewport viewport = CalculateViewport(UI, false);
			if (viewport.Font == null)
				return 0;

			string displayText = viewport.DisplayText;
			float textStartX = viewport.TextPosition.X;

			// If click is before text start, return 0
			if (mouseX <= textStartX)
				return 0;

			// Find the closest character position
			for (int i = 0; i <= displayText.Length; i++)
			{
				string substring = displayText.Substring(0, i);
				float charX = textStartX + UI.Graphics.MeasureText(viewport.Font, substring).X;

				if (mouseX < charX)
				{
					// Check if we're closer to this position or the previous one
					if (i > 0)
					{
						string prevSubstring = displayText.Substring(0, i - 1);
						float prevCharX = textStartX + UI.Graphics.MeasureText(viewport.Font, prevSubstring).X;
						if (mouseX - prevCharX < charX - mouseX)
							return i - 1;
					}
					return i;
				}
			}

			return displayText.Length;
		}

		private NPatch GetCurrentPatch(FishUI UI)
		{
			if (Disabled)
				return UI.Settings.ImgTextboxDisabled;
			if (UI.InputActiveControl == this)
				return UI.Settings.ImgTextboxActive;
			return UI.Settings.ImgTextboxNormal;
		}

		private TextboxViewport CalculateViewport(FishUI UI, bool revealCaret)
		{
			NPatch patch = GetCurrentPatch(UI);
			FontRef font = UI.Settings.FontTextboxDefault;
			Vector2 absolutePosition = GetAbsolutePosition();
			Vector2 absoluteSize = GetAbsoluteSize();
			float leftInset = Scale((patch?.Left ?? 0) + 4f);
			float rightInset = Scale((patch?.Right ?? 0) + 4f);
			Vector2 viewportPosition = new Vector2(absolutePosition.X + leftInset, absolutePosition.Y);
			Vector2 viewportSize = new Vector2(Math.Max(1, absoluteSize.X - leftInset - rightInset), absoluteSize.Y);
			string displayText = GetDisplayText();
			bool showPlaceholder = string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Placeholder);
			string textToDraw = showPlaceholder ? Placeholder : displayText;
			Vector2 displaySize = font != null ? UI.Graphics.MeasureText(font, displayText) : Vector2.Zero;
			Vector2 textSize = font != null ? UI.Graphics.MeasureText(font, textToDraw) : Vector2.Zero;
			bool textOverflows = displaySize.X > viewportSize.X + LayoutEpsilon;
			float margin = textOverflows ? Scale(CaretRevealMargin) : 0;
			float maxOffset = textOverflows ? Math.Max(0, displaySize.X + margin - viewportSize.X) : 0;
			_horizontalScrollOffsetPixels = Math.Clamp(_horizontalScrollOffsetPixels, 0, maxOffset);

			if (showPlaceholder)
			{
				_horizontalScrollOffsetPixels = 0;
			}
			else if (revealCaret || _caretVisibilityPending)
			{
				int cursor = Math.Clamp(CursorPosition, 0, displayText.Length);
				float cursorX = font != null && cursor > 0 ?
					UI.Graphics.MeasureText(font, displayText.Substring(0, cursor)).X : 0;
				if (cursorX < _horizontalScrollOffsetPixels)
					_horizontalScrollOffsetPixels = cursorX;
				else if (cursorX + margin > _horizontalScrollOffsetPixels + viewportSize.X)
					_horizontalScrollOffsetPixels = cursorX + margin - viewportSize.X;

				_horizontalScrollOffsetPixels = Math.Clamp(_horizontalScrollOffsetPixels, 0, maxOffset);
				_caretVisibilityPending = false;
			}

			Vector2 textPosition = new Vector2(
				viewportPosition.X - _horizontalScrollOffsetPixels,
				absolutePosition.Y + absoluteSize.Y / 2 - textSize.Y / 2);
			return new TextboxViewport(patch, font, displayText, textToDraw, showPlaceholder,
				viewportPosition, viewportSize, textPosition, textSize);
		}

		public FishUIDebugPrivacyMode GetDebugPrivacyMode() => PasswordMode ? FishUIDebugPrivacyMode.RedactText : FishUIDebugPrivacyMode.Default;

		public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
		{
			TextboxViewport viewport = CalculateViewport(FishUI, false);
			float maximumOffset = Math.Max(0, viewport.TextSize.X + Scale(CaretRevealMargin) - viewport.Size.X);
			writer.Write("textLength", Text.Length);
			writer.Write("displayTextLength", viewport.DisplayText.Length);
			writer.Write("displayTextWidthPixels", viewport.TextSize.X);
			writer.Write("passwordMode", PasswordMode);
			writer.Write("viewportPixels", new FishUIDebugRect(viewport.Position.X, viewport.Position.Y, viewport.Size.X, viewport.Size.Y));
			writer.Write("horizontalOffsetPixels", _horizontalScrollOffsetPixels);
			writer.Write("maximumHorizontalOffsetPixels", maximumOffset);
			writer.Write("cursorPosition", CursorPosition);
			writer.Write("selectionStart", SelectionStart);
			writer.Write("selectionLength", SelectionLength);
			writer.Write("font", viewport.Font?.Path);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			TextboxViewport viewport = CalculateViewport(UI, true);
			using (UI.Diagnostics.EnterRenderSemantic(FishUIRenderSemantic.ControlBounds))
				UI.Graphics.DrawNPatch(viewport.Patch, GetAbsolutePosition(), GetAbsoluteSize(), Color);
			UI.Graphics.PushScissor(viewport.Position, viewport.Size);

			// Draw selection highlight
			if (HasSelection && UI.InputActiveControl == this && !viewport.ShowPlaceholder)
			{
				var (selStart, selEnd) = GetSelectionRange();
				string beforeSel = viewport.DisplayText.Substring(0, selStart);
				string selText = viewport.DisplayText.Substring(selStart, selEnd - selStart);

				float selStartX = viewport.TextPosition.X + UI.Graphics.MeasureText(viewport.Font, beforeSel).X;
				float selWidth = UI.Graphics.MeasureText(viewport.Font, selText).X;

				using (UI.Diagnostics.EnterRenderSemantic(FishUIRenderSemantic.Selection))
					UI.Graphics.DrawRectangle(
						new Vector2(selStartX, viewport.TextPosition.Y),
						new Vector2(selWidth, viewport.TextSize.Y),
						SelectionColor
					);
			}

			// Draw text
			using (UI.Diagnostics.EnterRenderSemantic(FishUIRenderSemantic.Text))
			{
				if (viewport.ShowPlaceholder)
					UI.Graphics.DrawTextColor(viewport.Font, viewport.TextToDraw, viewport.TextPosition, PlaceholderColor);
				else if (TextColorOverride.HasValue)
					UI.Graphics.DrawTextColor(viewport.Font, viewport.TextToDraw, viewport.TextPosition, TextColorOverride.Value);
				else
					UI.Graphics.DrawText(viewport.Font, viewport.TextToDraw, viewport.TextPosition);
			}

			// Draw cursor
			bool drawCursor = false;
			if (UI.InputActiveControl == this && !viewport.ShowPlaceholder)
				drawCursor = MathF.Sin(Time * 5) > 0;

			if (drawCursor || (UI.InputActiveControl == this && viewport.ShowPlaceholder))
			{
				string textBeforeCursor = viewport.DisplayText.Substring(0, Math.Min(CursorPosition, viewport.DisplayText.Length));
				float cursorX = viewport.TextPosition.X + UI.Graphics.MeasureText(viewport.Font, textBeforeCursor).X;

				float cursorHeight = viewport.TextSize.Y > 0 ? viewport.TextSize.Y : GetAbsoluteSize().Y - Scale(4);
				Vector2 cursorStart = new Vector2(cursorX, viewport.TextPosition.Y);
				Vector2 cursorEnd = new Vector2(cursorX, viewport.TextPosition.Y + cursorHeight);

				if (drawCursor || viewport.ShowPlaceholder)
					using (UI.Diagnostics.EnterRenderSemantic(FishUIRenderSemantic.Caret))
						UI.Graphics.DrawLine(cursorStart, cursorEnd, 1, CursorColorOverride ?? FishColor.Black);
			}

			UI.Graphics.PopScissor();
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
					case 'c': // Copy
						{
							string text = Copy();
							if (!string.IsNullOrEmpty(text))
								UI.Input?.SetClipboardText(text);
						}
						return;
					case 'v': // Paste
						{
							string text = UI.Input?.GetClipboardText() ?? "";
							if (!string.IsNullOrEmpty(text))
								Paste(text);
						}
						return;
					case 'x': // Cut
						if (!ReadOnly)
						{
							string text = Cut();
							if (!string.IsNullOrEmpty(text))
								UI.Input?.SetClipboardText(text);
						}
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

			// Handle Ctrl key combinations for clipboard
			// Key codes: A=65, C=67, V=86, X=88
			if (InState.CtrlDown)
			{
				switch (KeyCode)
				{
					case 65: // A - Select All
						SelectAll();
						return;
					case 67: // C - Copy
						{
							string text = Copy();
							if (!string.IsNullOrEmpty(text))
								UI.Input?.SetClipboardText(text);
						}
						return;
					case 86: // V - Paste
						if (!ReadOnly)
						{
							string text = UI.Input?.GetClipboardText() ?? "";
							if (!string.IsNullOrEmpty(text))
								Paste(text);
						}
						return;
					case 88: // X - Cut
						if (!ReadOnly)
						{
							string text = Cut();
							if (!string.IsNullOrEmpty(text))
								UI.Input?.SetClipboardText(text);
						}
						return;
				}
			}

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
