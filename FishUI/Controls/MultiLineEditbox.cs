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
	/// A multi-line text editor control with smooth scrolling support.
	/// </summary>
	public class MultiLineEditbox : Control
	{
		private List<string> _lines = new List<string> { "" };
		private float _scrollOffsetPixels = 0f;

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
		/// Vertical scroll offset in pixels.
		/// </summary>
		[YamlIgnore]
		public float ScrollOffsetPixels
		{
			get => _scrollOffsetPixels;
			set => _scrollOffsetPixels = Math.Max(0, value);
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
		/// Whether to show the vertical scrollbar when content exceeds visible area.
		/// </summary>
		[YamlMember]
		public bool ShowScrollBar { get; set; } = true;

		/// <summary>
		/// Color of the selection highlight.
		/// </summary>
		[YamlMember]
		public FishColor SelectionColor { get; set; } = new FishColor(51, 153, 255, 128);

		/// <summary>
		/// Selection start row (0-based line index).
		/// </summary>
		[YamlIgnore]
		public int SelectionStartRow { get; set; } = 0;

		/// <summary>
		/// Selection start column (0-based character index).
		/// </summary>
		[YamlIgnore]
		public int SelectionStartColumn { get; set; } = 0;

		/// <summary>
		/// Selection end row (0-based line index).
		/// </summary>
		[YamlIgnore]
		public int SelectionEndRow { get; set; } = 0;

		/// <summary>
		/// Selection end column (0-based character index).
		/// </summary>
		[YamlIgnore]
		public int SelectionEndColumn { get; set; } = 0;

		/// <summary>
		/// Returns true if there is any text selected.
		/// </summary>
		[YamlIgnore]
		public bool HasSelection => SelectionStartRow != SelectionEndRow || SelectionStartColumn != SelectionEndColumn;

		/// <summary>
		/// Event fired when text changes.
		/// </summary>
		public event MultiLineEditboxTextChangedFunc OnTextChanged;

		// Cursor blink timer
		private float _cursorBlinkTimer = 0f;
		private bool _cursorVisible = true;

		// For drag selection
		[YamlIgnore]
		private bool _isSelecting = false;
		[YamlIgnore]
		private int _selectionAnchorRow = 0;
		[YamlIgnore]
		private int _selectionAnchorColumn = 0;

		// Cached font metrics
		private float _lineHeight = 0f;
		private FontRef _cachedFont;

		// Scrollbar
		[YamlIgnore]
		private ScrollBarV _scrollBar;

		// Track when scrollbar is driving the scroll (to avoid feedback loop)
		[YamlIgnore]
		private bool _scrollBarDriving = false;

		public MultiLineEditbox()
		{
			Size = new Vector2(300, 200);
			Focusable = true;
		}

		public MultiLineEditbox(string text) : this()
		{
			Text = text;
		}

		#region Selection Methods

		/// <summary>
		/// Gets the normalized selection range with start always before end.
		/// Returns ((startRow, startCol), (endRow, endCol)).
		/// </summary>
		public ((int Row, int Col) Start, (int Row, int Col) End) GetSelectionRange()
		{
			int startRow = SelectionStartRow;
			int startCol = SelectionStartColumn;
			int endRow = SelectionEndRow;
			int endCol = SelectionEndColumn;

			// Normalize: ensure start is before end
			if (startRow > endRow || (startRow == endRow && startCol > endCol))
			{
				(startRow, endRow) = (endRow, startRow);
				(startCol, endCol) = (endCol, startCol);
			}

			// Clamp to valid ranges
			startRow = Math.Clamp(startRow, 0, _lines.Count - 1);
			endRow = Math.Clamp(endRow, 0, _lines.Count - 1);
			startCol = Math.Clamp(startCol, 0, _lines[startRow].Length);
			endCol = Math.Clamp(endCol, 0, _lines[endRow].Length);

			return ((startRow, startCol), (endRow, endCol));
		}

		/// <summary>
		/// Gets the currently selected text.
		/// </summary>
		public string GetSelectedText()
		{
			if (!HasSelection)
				return "";

			var (start, end) = GetSelectionRange();

			if (start.Row == end.Row)
			{
				// Single line selection
				return _lines[start.Row].Substring(start.Col, end.Col - start.Col);
			}

			// Multi-line selection
			StringBuilder sb = new StringBuilder();

			// First line (from start column to end)
			sb.Append(_lines[start.Row].Substring(start.Col));

			// Middle lines (full lines)
			for (int row = start.Row + 1; row < end.Row; row++)
			{
				sb.Append('\n');
				sb.Append(_lines[row]);
			}

			// Last line (from start to end column)
			sb.Append('\n');
			sb.Append(_lines[end.Row].Substring(0, end.Col));

			return sb.ToString();
		}

		/// <summary>
		/// Selects all text in the editbox.
		/// </summary>
		public void SelectAll()
		{
			if (_lines.Count > 0)
			{
				SelectionStartRow = 0;
				SelectionStartColumn = 0;
				SelectionEndRow = _lines.Count - 1;
				SelectionEndColumn = _lines[_lines.Count - 1].Length;
				CursorRow = SelectionEndRow;
				CursorColumn = SelectionEndColumn;
			}
		}

		/// <summary>
		/// Clears the current selection.
		/// </summary>
		public void ClearSelection()
		{
			SelectionStartRow = CursorRow;
			SelectionStartColumn = CursorColumn;
			SelectionEndRow = CursorRow;
			SelectionEndColumn = CursorColumn;
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

			// Insert the text (may contain newlines)
			string[] linesToInsert = text.Split('\n');

			if (linesToInsert.Length == 1)
			{
				// Single line paste
				InsertTextInternal(linesToInsert[0]);
			}
			else
			{
				// Multi-line paste
				string currentLine = _lines[CursorRow];
				string beforeCursor = currentLine.Substring(0, CursorColumn);
				string afterCursor = currentLine.Substring(CursorColumn);

				// First line: append to current line
				_lines[CursorRow] = beforeCursor + linesToInsert[0];

				// Middle lines: insert new lines
				for (int i = 1; i < linesToInsert.Length - 1; i++)
				{
					_lines.Insert(CursorRow + i, linesToInsert[i]);
				}

				// Last line: append with remainder
				int lastIndex = linesToInsert.Length - 1;
				_lines.Insert(CursorRow + lastIndex, linesToInsert[lastIndex] + afterCursor);

				CursorRow += lastIndex;
				CursorColumn = linesToInsert[lastIndex].Length;
			}

			ClearSelection();
			OnTextChanged?.Invoke(this, Text);
		}

		/// <summary>
		/// Deletes the currently selected text.
		/// </summary>
		private void DeleteSelection()
		{
			if (!HasSelection)
				return;

			var (start, end) = GetSelectionRange();

			if (start.Row == end.Row)
			{
				// Single line deletion
				string line = _lines[start.Row];
				_lines[start.Row] = line.Remove(start.Col, end.Col - start.Col);
			}
			else
			{
				// Multi-line deletion
				string startLine = _lines[start.Row].Substring(0, start.Col);
				string endLine = _lines[end.Row].Substring(end.Col);

				// Remove lines in between
				for (int row = end.Row; row > start.Row; row--)
				{
					_lines.RemoveAt(row);
				}

				// Merge remaining
				_lines[start.Row] = startLine + endLine;
			}

			// Move cursor to start of selection
			CursorRow = start.Row;
			CursorColumn = start.Col;
			ClearSelection();
			OnTextChanged?.Invoke(this, Text);
		}

		/// <summary>
		/// Starts a selection at the current cursor position.
		/// </summary>
		private void StartSelection()
		{
			if (!HasSelection)
			{
				SelectionStartRow = CursorRow;
				SelectionStartColumn = CursorColumn;
				SelectionEndRow = CursorRow;
				SelectionEndColumn = CursorColumn;
			}
		}

		/// <summary>
		/// Extends the selection to the current cursor position.
		/// </summary>
		private void ExtendSelection()
		{
			SelectionEndRow = CursorRow;
			SelectionEndColumn = CursorColumn;
		}

		#endregion

		private void CreateScrollBar()
		{
			if (_scrollBar != null)
				return;

			_scrollBar = new ScrollBarV();
			_scrollBar.Size = new Vector2(Scale(ScrollBarWidth), ScaledSize.Y);
			_scrollBar.Position = new Vector2(Size.X - ScrollBarWidth, 0);
			_scrollBar.OnScrollChanged += (_, scroll, delta) =>
			{
				_scrollBarDriving = true;
				float maxScrollPixels = GetMaxScrollPixels();
				_scrollOffsetPixels = scroll * maxScrollPixels;
			};

			AddChild(_scrollBar);
		}

		/// <summary>
		/// Gets the maximum scroll offset in pixels.
		/// </summary>
		private float GetMaxScrollPixels()
		{
			float contentHeight = _lines.Count * _lineHeight;
			float viewHeight = GetTextAreaHeight();
			return Math.Max(0, contentHeight - viewHeight);
		}

		/// <summary>
		/// Gets the text area height (excluding padding).
		/// </summary>
		private float GetTextAreaHeight()
		{
			return ScaledSize.Y - Scale(TextPadding) * 2;
		}

		private void UpdateScrollBar()
		{
			if (_scrollBar == null) return;

			float contentHeight = _lines.Count * _lineHeight;
			float viewHeight = GetTextAreaHeight();

			// Update scrollbar position and size
			_scrollBar.Position = new Vector2(Size.X - ScrollBarWidth, 0);
			_scrollBar.Size = new Vector2(ScrollBarWidth, Size.Y);

			if (contentHeight <= viewHeight)
			{
				_scrollBar.ThumbHeight = 1f;
				_scrollBar.ThumbPosition = 0f;
			}
			else
			{
				_scrollBar.ThumbHeight = Math.Clamp(viewHeight / contentHeight, 0.1f, 1f);

				// Only update thumb position if scrollbar is NOT driving the scroll
				// This prevents fighting with the user's drag input
				if (!_scrollBarDriving)
				{
					float maxScroll = contentHeight - viewHeight;
					_scrollBar.ThumbPosition = Math.Clamp(_scrollOffsetPixels / maxScroll, 0f, 1f);
				}
			}

			// Reset the flag after processing
			_scrollBarDriving = false;
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
			float contentHeight = _lines.Count * _lineHeight;
			float viewHeight = GetTextAreaHeight();
			float rightOffset = (ShowScrollBar && contentHeight > viewHeight) ? Scale(ScrollBarWidth) : 0;

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

			// Draw line numbers gutter background
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
			float contentHeight = _lines.Count * _lineHeight;
			float viewHeight = GetTextAreaHeight();

			// Clamp scroll offset
			float maxScroll = GetMaxScrollPixels();
			_scrollOffsetPixels = Math.Clamp(_scrollOffsetPixels, 0, maxScroll);

			// Begin scissor for text area clipping
			UI.Graphics.BeginScissor(new Vector2(textAreaPos.X, pos.Y), new Vector2(textAreaSize.X, size.Y));

			// Get selection range for highlighting
			var (selStart, selEnd) = GetSelectionRange();
			bool hasSelection = HasSelection && HasFocus;

			// Draw all lines with pixel offset
			for (int lineIndex = 0; lineIndex < _lines.Count; lineIndex++)
			{
				string line = _lines[lineIndex];
				float lineY = textAreaPos.Y + lineIndex * _lineHeight - _scrollOffsetPixels;

				// Skip lines completely outside visible area
				if (lineY + _lineHeight < textAreaPos.Y || lineY > textAreaPos.Y + viewHeight)
					continue;

				// Draw selection highlight for this line
				if (hasSelection && font != null && lineIndex >= selStart.Row && lineIndex <= selEnd.Row)
				{
					int startCol = 0;
					int endCol = line.Length;

					if (lineIndex == selStart.Row)
						startCol = selStart.Col;
					if (lineIndex == selEnd.Row)
						endCol = selEnd.Col;

					if (startCol < endCol || (lineIndex > selStart.Row && lineIndex < selEnd.Row))
					{
						float selStartX = textAreaPos.X;
						float selEndX = textAreaPos.X;

						if (line.Length > 0)
						{
							if (startCol > 0)
								selStartX += UI.Graphics.MeasureText(font, line.Substring(0, Math.Min(startCol, line.Length))).X;
							if (endCol > 0)
								selEndX += UI.Graphics.MeasureText(font, line.Substring(0, Math.Min(endCol, line.Length))).X;
						}

						// For full line selections (middle lines), extend to a minimum width
						if (lineIndex > selStart.Row && lineIndex < selEnd.Row && line.Length == 0)
						{
							selEndX = selStartX + UI.Graphics.MeasureText(font, " ").X;
						}

						float selWidth = selEndX - selStartX;
						if (selWidth > 0)
						{
							UI.Graphics.DrawRectangle(
								new Vector2(selStartX, lineY),
								new Vector2(selWidth, _lineHeight),
								SelectionColor);
						}
					}
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

			// End scissor
			UI.Graphics.EndScissor();

			// Draw line numbers (in gutter area with scissoring)
			if (ShowLineNumbers && font != null)
			{
				float gutterX = pos.X;
				float gutterW = Scale(LineNumberWidth);
				UI.Graphics.BeginScissor(new Vector2(gutterX, pos.Y), new Vector2(gutterW, size.Y));

				for (int lineIndex = 0; lineIndex < _lines.Count; lineIndex++)
				{
					float lineY = textAreaPos.Y + lineIndex * _lineHeight - _scrollOffsetPixels;
					if (lineY + _lineHeight < pos.Y || lineY > pos.Y + size.Y)
						continue;

					string lineNum = (lineIndex + 1).ToString();
					var numSize = UI.Graphics.MeasureText(font, lineNum);
					float numX = gutterX + gutterW - numSize.X - Scale(8);
					UI.Graphics.DrawTextColor(font, lineNum, new Vector2(numX, lineY), LineNumberColor);
				}

				UI.Graphics.EndScissor();
			}

			// Draw placeholder if empty
			if (_lines.Count == 1 && string.IsNullOrEmpty(_lines[0]) && !string.IsNullOrEmpty(Placeholder) && font != null)
			{
				UI.Graphics.DrawTextColor(font, Placeholder, textAreaPos, PlaceholderColor);
			}

			// Handle scrollbar
			if (ShowScrollBar && contentHeight > viewHeight)
			{
				CreateScrollBar();
				UpdateScrollBar();
				_scrollBar.Visible = true;
			}
			else if (_scrollBar != null)
			{
				_scrollBar.Visible = false;
			}
		}

		private void EnsureCursorVisible()
		{
			if (_lineHeight <= 0) return;

			float cursorY = CursorRow * _lineHeight;
			float viewHeight = GetTextAreaHeight();

			// Scroll up if cursor is above visible area
			if (cursorY < _scrollOffsetPixels)
			{
				_scrollOffsetPixels = cursorY;
			}
			// Scroll down if cursor is below visible area
			else if (cursorY + _lineHeight > _scrollOffsetPixels + viewHeight)
			{
				_scrollOffsetPixels = cursorY + _lineHeight - viewHeight;
			}
		}

		public override void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
			base.HandleKeyPress(UI, InState, Key);

			// Handle Ctrl key combinations for clipboard
			if (InState.CtrlDown)
			{
				switch (Key)
				{
					case FishKey.A: // Select All
						SelectAll();
						ResetCursorBlink();
						return;
					case FishKey.C: // Copy
						{
							string text = Copy();
							if (!string.IsNullOrEmpty(text))
								UI.Input?.SetClipboardText(text);
						}
						return;
					case FishKey.V: // Paste
						if (!ReadOnly)
						{
							string text = UI.Input?.GetClipboardText() ?? "";
							if (!string.IsNullOrEmpty(text))
								Paste(text);
							ResetCursorBlink();
							EnsureCursorVisible();
						}
						return;
					case FishKey.X: // Cut
						if (!ReadOnly)
						{
							string text = Cut();
							if (!string.IsNullOrEmpty(text))
								UI.Input?.SetClipboardText(text);
							ResetCursorBlink();
							EnsureCursorVisible();
						}
						return;
				}
			}

			if (ReadOnly && Key != FishKey.Up && Key != FishKey.Down && Key != FishKey.Left && Key != FishKey.Right &&
				Key != FishKey.Home && Key != FishKey.End && Key != FishKey.PageUp && Key != FishKey.PageDown)
				return;

			switch (Key)
			{
				case FishKey.Left:
					if (InState.ShiftDown)
					{
						StartSelection();
						MoveCursorLeftInternal();
						ExtendSelection();
					}
					else
					{
						if (HasSelection)
						{
							var (start, _) = GetSelectionRange();
							CursorRow = start.Row;
							CursorColumn = start.Col;
							ClearSelection();
						}
						else
						{
							MoveCursorLeftInternal();
						}
					}
					break;
				case FishKey.Right:
					if (InState.ShiftDown)
					{
						StartSelection();
						MoveCursorRightInternal();
						ExtendSelection();
					}
					else
					{
						if (HasSelection)
						{
							var (_, end) = GetSelectionRange();
							CursorRow = end.Row;
							CursorColumn = end.Col;
							ClearSelection();
						}
						else
						{
							MoveCursorRightInternal();
						}
					}
					break;
				case FishKey.Up:
					if (InState.ShiftDown)
					{
						StartSelection();
						MoveCursorUpInternal();
						ExtendSelection();
					}
					else
					{
						ClearSelection();
						MoveCursorUpInternal();
					}
					break;
				case FishKey.Down:
					if (InState.ShiftDown)
					{
						StartSelection();
						MoveCursorDownInternal();
						ExtendSelection();
					}
					else
					{
						ClearSelection();
						MoveCursorDownInternal();
					}
					break;
				case FishKey.Home:
					if (InState.ShiftDown)
					{
						StartSelection();
						CursorColumn = 0;
						ExtendSelection();
					}
					else
					{
						CursorColumn = 0;
						ClearSelection();
					}
					break;
				case FishKey.End:
					if (InState.ShiftDown)
					{
						StartSelection();
						CursorColumn = _lines[CursorRow].Length;
						ExtendSelection();
					}
					else
					{
						CursorColumn = _lines[CursorRow].Length;
						ClearSelection();
					}
					break;
				case FishKey.PageUp:
					{
						int visibleLines = GetVisibleLineCount();
						if (InState.ShiftDown)
						{
							StartSelection();
							CursorRow = Math.Max(0, CursorRow - visibleLines);
							CursorColumn = Math.Min(CursorColumn, _lines[CursorRow].Length);
							ExtendSelection();
						}
						else
						{
							CursorRow = Math.Max(0, CursorRow - visibleLines);
							CursorColumn = Math.Min(CursorColumn, _lines[CursorRow].Length);
							ClearSelection();
						}
					}
					break;
				case FishKey.PageDown:
					{
						int visibleLines = GetVisibleLineCount();
						if (InState.ShiftDown)
						{
							StartSelection();
							CursorRow = Math.Min(_lines.Count - 1, CursorRow + visibleLines);
							CursorColumn = Math.Min(CursorColumn, _lines[CursorRow].Length);
							ExtendSelection();
						}
						else
						{
							CursorRow = Math.Min(_lines.Count - 1, CursorRow + visibleLines);
							CursorColumn = Math.Min(CursorColumn, _lines[CursorRow].Length);
							ClearSelection();
						}
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
			EnsureCursorVisible();
		}

		public override void HandleTextInput(FishUI UI, FishInputState InState, char Chr)
		{
			base.HandleTextInput(UI, InState, Chr);

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

			// Filter control characters
			if (char.IsControl(Chr) && Chr != '\t')
				return;

			// Delete selection first if any
			if (HasSelection)
				DeleteSelection();

			InsertTextInternal(Chr.ToString());
			ResetCursorBlink();
			EnsureCursorVisible();
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				PositionCursorFromMouse(UI, Pos);
				_selectionAnchorRow = CursorRow;
				_selectionAnchorColumn = CursorColumn;
				_isSelecting = true;
				ClearSelection();
				ResetCursorBlink();
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
			base.HandleDrag(UI, StartPos, EndPos, InState);

			if (_isSelecting && HasFocus)
			{
				PositionCursorFromMouse(UI, EndPos);
				SelectionStartRow = _selectionAnchorRow;
				SelectionStartColumn = _selectionAnchorColumn;
				SelectionEndRow = CursorRow;
				SelectionEndColumn = CursorColumn;
			}
		}

		public override void HandleMouseDoubleClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseDoubleClick(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left && HasFocus)
			{
				// Double-click selects all text
				SelectAll();
			}
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				PositionCursorFromMouse(UI, Pos);
				ResetCursorBlink();
				EnsureCursorVisible();
			}
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float Delta)
		{
			base.HandleMouseWheel(UI, InState, Delta);

			// Scroll by 3 lines worth of pixels
			float scrollAmount = _lineHeight * 3f * (Delta > 0 ? -1 : 1);
			_scrollOffsetPixels = Math.Clamp(_scrollOffsetPixels + scrollAmount, 0, GetMaxScrollPixels());
		}

		private void PositionCursorFromMouse(FishUI UI, Vector2 mousePos)
		{
			var (textAreaPos, textAreaSize) = GetTextAreaBounds();

			// Calculate clicked row based on pixel position
			int clickedRow = (int)((mousePos.Y - textAreaPos.Y + _scrollOffsetPixels) / _lineHeight);
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

		private void MoveCursorLeftInternal()
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

		private void MoveCursorRightInternal()
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

		private void MoveCursorUpInternal()
		{
			if (CursorRow > 0)
			{
				CursorRow--;
				CursorColumn = Math.Min(CursorColumn, _lines[CursorRow].Length);
			}
		}

		private void MoveCursorDownInternal()
		{
			if (CursorRow < _lines.Count - 1)
			{
				CursorRow++;
				CursorColumn = Math.Min(CursorColumn, _lines[CursorRow].Length);
			}
		}

		private void InsertTextInternal(string text)
		{
			string currentLine = _lines[CursorRow];
			_lines[CursorRow] = currentLine.Insert(CursorColumn, text);
			CursorColumn += text.Length;
			OnTextChanged?.Invoke(this, Text);
		}

		/// <summary>
		/// Inserts text at the current cursor position, replacing any selection.
		/// </summary>
		public void InsertText(string text)
		{
			if (ReadOnly || string.IsNullOrEmpty(text))
				return;

			// Delete selection first if any
			if (HasSelection)
				DeleteSelection();

			InsertTextInternal(text);
		}

		private void InsertNewLine()
		{
			// Delete selection first if any
			if (HasSelection)
				DeleteSelection();

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
			// Delete selection first if any
			if (HasSelection)
			{
				DeleteSelection();
				return;
			}

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
			// Delete selection first if any
			if (HasSelection)
			{
				DeleteSelection();
				return;
			}

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
			_scrollOffsetPixels = 0;
			OnTextChanged?.Invoke(this, Text);
		}

		/// <summary>
		/// Scrolls to the end of the text.
		/// </summary>
		public void ScrollToEnd()
		{
			_scrollOffsetPixels = GetMaxScrollPixels();
			CursorRow = _lines.Count - 1;
			CursorColumn = _lines[CursorRow].Length;
		}

		/// <summary>
		/// Scrolls to the beginning of the text.
		/// </summary>
		public void ScrollToStart()
		{
			_scrollOffsetPixels = 0;
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
