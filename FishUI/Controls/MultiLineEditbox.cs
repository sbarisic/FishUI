using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Delegate for MultiLineEditbox text changed events.
	/// </summary>
	public delegate void MultiLineEditboxTextChangedFunc(MultiLineEditbox sender, string text);

	/// <summary>
	/// A multi-line text editor control with word wrap and scrolling support.
	/// </summary>
	public class MultiLineEditbox : Control
	{
		private List<string> _lines = new List<string> { "" };
		private int _scrollOffset = 0;

		/// <summary>
		/// Gets or sets the full text content with line breaks.
		/// </summary>
		[YamlMember]
		public string Text
		{
			get => string.Join("\n", _lines);
			set
			{
				string newValue = value ?? "";
				_lines = new List<string>(newValue.Split('\n'));
				if (_lines.Count == 0)
					_lines.Add("");

				// Clamp cursor to valid range
				CursorRow = Math.Clamp(CursorRow, 0, _lines.Count - 1);
				CursorColumn = Math.Clamp(CursorColumn, 0, _lines[CursorRow].Length);

				OnTextChanged?.Invoke(this, Text);
			}
		}

		/// <summary>
		/// Gets the lines of text.
		/// </summary>
		[YamlIgnore]
		public IReadOnlyList<string> Lines => _lines;

		/// <summary>
		/// Current cursor row (0-based line index).
		/// </summary>
		[YamlIgnore]
		public int CursorRow { get; set; } = 0;

		/// <summary>
		/// Current cursor column (0-based character index within the line).
		/// </summary>
		[YamlIgnore]
		public int CursorColumn { get; set; } = 0;

		/// <summary>
		/// Vertical scroll offset in lines.
		/// </summary>
		[YamlIgnore]
		public int ScrollOffset
		{
			get => _scrollOffset;
			set => _scrollOffset = Math.Max(0, value);
		}

		/// <summary>
		/// If true, the text wraps to the next line when it reaches the edge.
		/// </summary>
		[YamlMember]
		public bool WordWrap { get; set; } = false;

		/// <summary>
		/// If true, the control cannot be edited.
		/// </summary>
		[YamlMember]
		public bool ReadOnly { get; set; } = false;

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
		/// Background color of the text area.
		/// </summary>
		[YamlMember]
		public FishColor BackgroundColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Text color.
		/// </summary>
		[YamlMember]
		public FishColor TextColor { get; set; } = new FishColor(0, 0, 0, 255);

		/// <summary>
		/// Cursor color.
		/// </summary>
		[YamlMember]
		public FishColor CursorColor { get; set; } = new FishColor(0, 0, 0, 255);

		/// <summary>
		/// Padding inside the text area.
		/// </summary>
		[YamlMember]
		public float TextPadding { get; set; } = 4f;

		/// <summary>
		/// Width of the scrollbar.
		/// </summary>
		[YamlMember]
		public float ScrollBarWidth { get; set; } = 16f;

		/// <summary>
		/// Whether to show line numbers.
		/// </summary>
		[YamlMember]
		public bool ShowLineNumbers { get; set; } = false;

		/// <summary>
		/// Width of the line number gutter.
		/// </summary>
		[YamlMember]
		public float LineNumberWidth { get; set; } = 40f;

		/// <summary>
		/// Color of line numbers.
		/// </summary>
		[YamlMember]
		public FishColor LineNumberColor { get; set; } = new FishColor(128, 128, 128, 255);

		/// <summary>
		/// Event fired when text changes.
		/// </summary>
		public event MultiLineEditboxTextChangedFunc OnTextChanged;

		// Cursor blink timer
		private float _cursorBlinkTimer = 0f;
		private bool _cursorVisible = true;

		// Cached font metrics
		private float _lineHeight = 0f;
		private FontRef _cachedFont;

		public MultiLineEditbox()
		{
			Size = new Vector2(300, 200);
			Focusable = true;
		}

		public MultiLineEditbox(string text) : this()
		{
			Text = text;
		}

		/// <summary>
		/// Gets the number of visible lines that fit in the control.
		/// </summary>
		public int GetVisibleLineCount()
		{
			if (_lineHeight <= 0) return 1;
			float textAreaHeight = ScaledSize.Y - Scale(TextPadding) * 2;
			return Math.Max(1, (int)(textAreaHeight / _lineHeight));
		}

		/// <summary>
		/// Gets the text area rectangle (excluding scrollbar).
		/// </summary>
		private (Vector2 pos, Vector2 size) GetTextAreaBounds()
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = ScaledSize;

			float leftOffset = ShowLineNumbers ? Scale(LineNumberWidth) : 0;
			float rightOffset = Scale(ScrollBarWidth);

			return (
				new Vector2(pos.X + leftOffset + Scale(TextPadding), pos.Y + Scale(TextPadding)),
				new Vector2(size.X - leftOffset - rightOffset - Scale(TextPadding) * 2, size.Y - Scale(TextPadding) * 2)
			);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			base.DrawControl(UI, Dt, Time);

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = ScaledSize;

			// Update cursor blink
			if (HasFocus)
			{
				_cursorBlinkTimer += Dt;
				if (_cursorBlinkTimer >= 0.5f)
				{
					_cursorBlinkTimer = 0f;
					_cursorVisible = !_cursorVisible;
				}
			}
			else
			{
				_cursorVisible = false;
			}

			// Get font and calculate line height
			var font = UI.Settings.FontDefault;
			_cachedFont = font;
			if (font != null)
			{
				var textSize = UI.Graphics.MeasureText(font, "Mg");
				_lineHeight = textSize.Y;
			}
			else
			{
				_lineHeight = 16f;
			}

			// Draw background using textbox NPatch
			NPatch bg = HasFocus ? UI.Settings.ImgTextboxActive : UI.Settings.ImgTextboxNormal;
			if (bg != null)
			{
				UI.Graphics.DrawNPatch(bg, pos, size, Color);
			}
			else
			{
				UI.Graphics.DrawRectangle(pos, size, BackgroundColor);
				UI.Graphics.DrawRectangleOutline(pos, size, new FishColor(128, 128, 128, 255));
			}

			// Draw line numbers gutter
			if (ShowLineNumbers)
			{
				float gutterWidth = Scale(LineNumberWidth);
				UI.Graphics.DrawRectangle(pos, new Vector2(gutterWidth, size.Y), new FishColor(240, 240, 240, 255));
				UI.Graphics.DrawLine(
					new Vector2(pos.X + gutterWidth, pos.Y),
					new Vector2(pos.X + gutterWidth, pos.Y + size.Y),
					1f, new FishColor(200, 200, 200, 255));
			}

			// Calculate text area
			var (textAreaPos, textAreaSize) = GetTextAreaBounds();
			int visibleLines = GetVisibleLineCount();

			// Ensure cursor is visible (auto-scroll)
			EnsureCursorVisible(visibleLines);

			// Draw visible lines
			for (int i = 0; i < visibleLines && i + _scrollOffset < _lines.Count; i++)
			{
				int lineIndex = i + _scrollOffset;
				string line = _lines[lineIndex];
				float lineY = textAreaPos.Y + i * _lineHeight;

				// Draw line number
				if (ShowLineNumbers && font != null)
				{
					string lineNum = (lineIndex + 1).ToString();
					var numSize = UI.Graphics.MeasureText(font, lineNum);
					float numX = pos.X + Scale(LineNumberWidth) - numSize.X - Scale(4);
					UI.Graphics.DrawTextColor(font, lineNum, new Vector2(numX, lineY), LineNumberColor);
				}

				// Draw text
				if (font != null && !string.IsNullOrEmpty(line))
				{
					UI.Graphics.DrawTextColor(font, line, new Vector2(textAreaPos.X, lineY), TextColor);
				}

				// Draw cursor on this line
				if (HasFocus && _cursorVisible && CursorRow == lineIndex)
				{
					string textBeforeCursor = line.Substring(0, Math.Min(CursorColumn, line.Length));
					float cursorX = textAreaPos.X;
					if (font != null && textBeforeCursor.Length > 0)
					{
						cursorX += UI.Graphics.MeasureText(font, textBeforeCursor).X;
					}

					UI.Graphics.DrawLine(
						new Vector2(cursorX, lineY),
						new Vector2(cursorX, lineY + _lineHeight),
						Scale(1f), CursorColor);
				}
			}

			// Draw placeholder if empty
			if (_lines.Count == 1 && string.IsNullOrEmpty(_lines[0]) && !string.IsNullOrEmpty(Placeholder) && font != null)
			{
				UI.Graphics.DrawTextColor(font, Placeholder, textAreaPos, PlaceholderColor);
			}

			// Draw scrollbar if needed
			if (_lines.Count > visibleLines)
			{
				DrawScrollBar(UI, pos, size, visibleLines);
			}
		}

		private void DrawScrollBar(FishUI UI, Vector2 pos, Vector2 size, int visibleLines)
		{
			float scrollBarX = pos.X + size.X - Scale(ScrollBarWidth);
			float scrollBarHeight = size.Y;

			// Track
			UI.Graphics.DrawRectangle(
				new Vector2(scrollBarX, pos.Y),
				new Vector2(Scale(ScrollBarWidth), scrollBarHeight),
				new FishColor(220, 220, 220, 255));

			// Thumb
			float thumbRatio = (float)visibleLines / _lines.Count;
			float thumbHeight = Math.Max(Scale(20f), scrollBarHeight * thumbRatio);
			float thumbY = pos.Y + ((float)_scrollOffset / Math.Max(1, _lines.Count - visibleLines)) * (scrollBarHeight - thumbHeight);

			UI.Graphics.DrawRectangle(
				new Vector2(scrollBarX + Scale(2), thumbY),
				new Vector2(Scale(ScrollBarWidth) - Scale(4), thumbHeight),
				new FishColor(180, 180, 180, 255));
		}

		private void EnsureCursorVisible(int visibleLines)
		{
			// Scroll up if cursor is above visible area
			if (CursorRow < _scrollOffset)
			{
				_scrollOffset = CursorRow;
			}
			// Scroll down if cursor is below visible area
			else if (CursorRow >= _scrollOffset + visibleLines)
			{
				_scrollOffset = CursorRow - visibleLines + 1;
			}
		}

		public override void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
			base.HandleKeyPress(UI, InState, Key);

			if (ReadOnly && Key != FishKey.Up && Key != FishKey.Down && Key != FishKey.Left && Key != FishKey.Right &&
				Key != FishKey.Home && Key != FishKey.End && Key != FishKey.PageUp && Key != FishKey.PageDown)
				return;

			switch (Key)
			{
				case FishKey.Left:
					MoveCursorLeft();
					break;
				case FishKey.Right:
					MoveCursorRight();
					break;
				case FishKey.Up:
					MoveCursorUp();
					break;
				case FishKey.Down:
					MoveCursorDown();
					break;
				case FishKey.Home:
					CursorColumn = 0;
					break;
				case FishKey.End:
					CursorColumn = _lines[CursorRow].Length;
					break;
				case FishKey.PageUp:
					{
						int visibleLines = GetVisibleLineCount();
						CursorRow = Math.Max(0, CursorRow - visibleLines);
						CursorColumn = Math.Min(CursorColumn, _lines[CursorRow].Length);
					}
					break;
				case FishKey.PageDown:
					{
						int visibleLines = GetVisibleLineCount();
						CursorRow = Math.Min(_lines.Count - 1, CursorRow + visibleLines);
						CursorColumn = Math.Min(CursorColumn, _lines[CursorRow].Length);
					}
					break;
				case FishKey.Enter:
					if (!ReadOnly)
						InsertNewLine();
					break;
				case FishKey.Backspace:
					if (!ReadOnly)
						HandleBackspace();
					break;
				case FishKey.Delete:
					if (!ReadOnly)
						HandleDelete();
					break;
				case FishKey.Tab:
					if (!ReadOnly)
						InsertText("\t");
					break;
			}

			ResetCursorBlink();
		}

		public override void HandleTextInput(FishUI UI, FishInputState InState, char Chr)
		{
			base.HandleTextInput(UI, InState, Chr);

			if (ReadOnly)
				return;

			// Filter control characters
			if (char.IsControl(Chr) && Chr != '\t')
				return;

			InsertText(Chr.ToString());
			ResetCursorBlink();
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				PositionCursorFromMouse(UI, Pos);
				ResetCursorBlink();
			}
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float Delta)
		{
			base.HandleMouseWheel(UI, InState, Delta);

			int scrollAmount = Delta > 0 ? -3 : 3;
			_scrollOffset = Math.Clamp(_scrollOffset + scrollAmount, 0, Math.Max(0, _lines.Count - GetVisibleLineCount()));
		}

		private void PositionCursorFromMouse(FishUI UI, Vector2 mousePos)
		{
			var (textAreaPos, textAreaSize) = GetTextAreaBounds();

			// Calculate clicked row
			int clickedRow = (int)((mousePos.Y - textAreaPos.Y) / _lineHeight) + _scrollOffset;
			clickedRow = Math.Clamp(clickedRow, 0, _lines.Count - 1);
			CursorRow = clickedRow;

			// Calculate clicked column
			string line = _lines[CursorRow];
			if (_cachedFont != null && line.Length > 0)
			{
				float relativeX = mousePos.X - textAreaPos.X;
				int col = 0;
				float accumulatedWidth = 0f;

				for (int i = 0; i < line.Length; i++)
				{
					float charWidth = UI.Graphics.MeasureText(_cachedFont, line[i].ToString()).X;
					if (accumulatedWidth + charWidth / 2 >= relativeX)
						break;
					accumulatedWidth += charWidth;
					col++;
				}
				CursorColumn = col;
			}
			else
			{
				CursorColumn = 0;
			}
		}

		private void MoveCursorLeft()
		{
			if (CursorColumn > 0)
			{
				CursorColumn--;
			}
			else if (CursorRow > 0)
			{
				CursorRow--;
				CursorColumn = _lines[CursorRow].Length;
			}
		}

		private void MoveCursorRight()
		{
			if (CursorColumn < _lines[CursorRow].Length)
			{
				CursorColumn++;
			}
			else if (CursorRow < _lines.Count - 1)
			{
				CursorRow++;
				CursorColumn = 0;
			}
		}

		private void MoveCursorUp()
		{
			if (CursorRow > 0)
			{
				CursorRow--;
				CursorColumn = Math.Min(CursorColumn, _lines[CursorRow].Length);
			}
		}

		private void MoveCursorDown()
		{
			if (CursorRow < _lines.Count - 1)
			{
				CursorRow++;
				CursorColumn = Math.Min(CursorColumn, _lines[CursorRow].Length);
			}
		}

		private void InsertText(string text)
		{
			string currentLine = _lines[CursorRow];
			_lines[CursorRow] = currentLine.Insert(CursorColumn, text);
			CursorColumn += text.Length;
			OnTextChanged?.Invoke(this, Text);
		}

		private void InsertNewLine()
		{
			string currentLine = _lines[CursorRow];
			string beforeCursor = currentLine.Substring(0, CursorColumn);
			string afterCursor = currentLine.Substring(CursorColumn);

			_lines[CursorRow] = beforeCursor;
			_lines.Insert(CursorRow + 1, afterCursor);

			CursorRow++;
			CursorColumn = 0;
			OnTextChanged?.Invoke(this, Text);
		}

		private void HandleBackspace()
		{
			if (CursorColumn > 0)
			{
				string line = _lines[CursorRow];
				_lines[CursorRow] = line.Remove(CursorColumn - 1, 1);
				CursorColumn--;
			}
			else if (CursorRow > 0)
			{
				// Merge with previous line
				string currentLine = _lines[CursorRow];
				_lines.RemoveAt(CursorRow);
				CursorRow--;
				CursorColumn = _lines[CursorRow].Length;
				_lines[CursorRow] += currentLine;
			}
			OnTextChanged?.Invoke(this, Text);
		}

		private void HandleDelete()
		{
			string line = _lines[CursorRow];
			if (CursorColumn < line.Length)
			{
				_lines[CursorRow] = line.Remove(CursorColumn, 1);
			}
			else if (CursorRow < _lines.Count - 1)
			{
				// Merge with next line
				_lines[CursorRow] += _lines[CursorRow + 1];
				_lines.RemoveAt(CursorRow + 1);
			}
			OnTextChanged?.Invoke(this, Text);
		}

		private void ResetCursorBlink()
		{
			_cursorBlinkTimer = 0f;
			_cursorVisible = true;
		}

		/// <summary>
		/// Appends text to the end of the content.
		/// </summary>
		public void AppendText(string text)
		{
			if (string.IsNullOrEmpty(text))
				return;

			string[] newLines = text.Split('\n');

			// Append first part to last existing line
			_lines[_lines.Count - 1] += newLines[0];

			// Add remaining lines
			for (int i = 1; i < newLines.Length; i++)
			{
				_lines.Add(newLines[i]);
			}

			OnTextChanged?.Invoke(this, Text);
		}

		/// <summary>
		/// Clears all text.
		/// </summary>
		public void Clear()
		{
			_lines.Clear();
			_lines.Add("");
			CursorRow = 0;
			CursorColumn = 0;
			_scrollOffset = 0;
			OnTextChanged?.Invoke(this, Text);
		}

		/// <summary>
		/// Scrolls to the end of the text.
		/// </summary>
		public void ScrollToEnd()
		{
			int visibleLines = GetVisibleLineCount();
			_scrollOffset = Math.Max(0, _lines.Count - visibleLines);
			CursorRow = _lines.Count - 1;
			CursorColumn = _lines[CursorRow].Length;
		}

		/// <summary>
		/// Scrolls to the beginning of the text.
		/// </summary>
		public void ScrollToStart()
		{
			_scrollOffset = 0;
			CursorRow = 0;
			CursorColumn = 0;
		}

		/// <summary>
		/// Gets the line at the specified index.
		/// </summary>
		public string GetLine(int index)
		{
			if (index >= 0 && index < _lines.Count)
				return _lines[index];
			return "";
		}

		/// <summary>
		/// Gets the total number of lines.
		/// </summary>
		[YamlIgnore]
		public int LineCount => _lines.Count;
	}
}
