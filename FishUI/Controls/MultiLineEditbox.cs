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
		private const float LayoutEpsilon = 0.01f;
		private const float CaretRevealMargin = 2f;

		private readonly struct ViewportRect
		{
			public Vector2 Position { get; }
			public Vector2 Size { get; }

			public ViewportRect(Vector2 position, Vector2 size)
			{
				Position = position;
				Size = new Vector2(Math.Max(0, size.X), Math.Max(0, size.Y));
			}
		}

		private readonly struct VisualLine
		{
			public int LogicalRow { get; }
			public int StartColumn { get; }
			public int Length { get; }
			public int EndColumn => StartColumn + Length;

			public VisualLine(int logicalRow, int startColumn, int length)
			{
				LogicalRow = logicalRow;
				StartColumn = startColumn;
				Length = length;
			}
		}

		private sealed class TextViewportLayout
		{
			public ViewportRect TextRect { get; set; }
			public ViewportRect GutterRect { get; set; }
			public ViewportRect HorizontalScrollBarRect { get; set; }
			public ViewportRect VerticalScrollBarRect { get; set; }
			public ViewportRect CornerRect { get; set; }
			public bool HorizontalVisible { get; set; }
			public bool VerticalVisible { get; set; }
			public bool TextOverflowsHorizontally { get; set; }
			public float ContentWidth { get; set; }
			public float ContentHeight { get; set; }
			public float ScrollableContentWidth { get; set; }
			public float MaxHorizontalOffset { get; set; }
			public float MaxVerticalOffset { get; set; }
			public float LineHeight { get; set; }
			public FontRef Font { get; set; }
			public List<VisualLine> VisualLines { get; set; }
		}

		private List<string> _lines = new List<string> { "" };
		private List<VisualLine> _visualLines = new List<VisualLine> { new VisualLine(0, 0, 0) };
		private float _scrollOffsetPixels = 0f;
		private float _horizontalScrollOffsetPixels = 0f;
		private int _cursorRow;
		private int _cursorColumn;
		private bool _wordWrap;
		private float _textPadding = 4f;
		private float _scrollBarWidth = 16f;
		private bool _showLineNumbers;
		private float _lineNumberWidth = 40f;
		private bool _showScrollBar = true;
		private bool _showHorizontalScrollBar = true;
		private float _horizontalScrollBarHeight = 16f;
		private bool _layoutDirty = true;
		private bool _caretVisibilityPending = true;
		private TextViewportLayout _viewportLayout;
		private Vector2 _cachedLayoutSize;
		private FontRef _cachedLayoutFont;
		private float _cachedLayoutScale = -1f;

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
				NormalizeCaret();

				NotifyTextChanged();
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
		public int CursorRow
		{
			get => _cursorRow;
			set
			{
				int newValue = Math.Max(0, value);
				if (_cursorRow == newValue)
					return;

				_cursorRow = newValue;
				_caretVisibilityPending = true;
			}
		}

		/// <summary>
		/// Current cursor column (0-based character index within the line).
		/// </summary>
		[YamlIgnore]
		public int CursorColumn
		{
			get => _cursorColumn;
			set
			{
				int newValue = Math.Max(0, value);
				if (_cursorColumn == newValue)
					return;

				_cursorColumn = newValue;
				_caretVisibilityPending = true;
			}
		}

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
		/// Horizontal scroll offset in scaled pixels.
		/// </summary>
		[YamlIgnore]
		public float HorizontalScrollOffsetPixels
		{
			get => _horizontalScrollOffsetPixels;
			set => _horizontalScrollOffsetPixels = Math.Max(0, value);
		}

		/// <summary>
		/// If true, the text wraps to the next line when it reaches the edge.
		/// </summary>
		[YamlMember]
		public bool WordWrap
		{
			get => _wordWrap;
			set
			{
				if (_wordWrap == value)
					return;

				_wordWrap = value;
				if (_wordWrap)
					_horizontalScrollOffsetPixels = 0;
				MarkLayoutDirty();
			}
		}

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
		public float TextPadding
		{
			get => _textPadding;
			set
			{
				float newValue = Math.Max(0, value);
				if (Math.Abs(_textPadding - newValue) <= LayoutEpsilon)
					return;
				_textPadding = newValue;
				MarkLayoutDirty();
			}
		}

		/// <summary>
		/// Width of the scrollbar.
		/// </summary>
		[YamlMember]
		public float ScrollBarWidth
		{
			get => _scrollBarWidth;
			set
			{
				float newValue = Math.Max(0, value);
				if (Math.Abs(_scrollBarWidth - newValue) <= LayoutEpsilon)
					return;
				_scrollBarWidth = newValue;
				MarkLayoutDirty();
			}
		}

		/// <summary>
		/// Whether to show line numbers.
		/// </summary>
		[YamlMember]
		public bool ShowLineNumbers
		{
			get => _showLineNumbers;
			set
			{
				if (_showLineNumbers == value)
					return;
				_showLineNumbers = value;
				MarkLayoutDirty();
			}
		}

		/// <summary>
		/// Width of the line number gutter.
		/// </summary>
		[YamlMember]
		public float LineNumberWidth
		{
			get => _lineNumberWidth;
			set
			{
				float newValue = Math.Max(0, value);
				if (Math.Abs(_lineNumberWidth - newValue) <= LayoutEpsilon)
					return;
				_lineNumberWidth = newValue;
				MarkLayoutDirty();
			}
		}

		/// <summary>
		/// Color of line numbers.
		/// </summary>
		[YamlMember]
		public FishColor LineNumberColor { get; set; } = new FishColor(128, 128, 128, 255);

		/// <summary>
		/// Whether to show the vertical scrollbar when content exceeds visible area.
		/// </summary>
		[YamlMember]
		public bool ShowScrollBar
		{
			get => _showScrollBar;
			set
			{
				if (_showScrollBar == value)
					return;
				_showScrollBar = value;
				MarkLayoutDirty();
			}
		}

		/// <summary>
		/// Whether to show the horizontal scrollbar for overflowing unwrapped text.
		/// </summary>
		[YamlMember]
		public bool ShowHorizontalScrollBar
		{
			get => _showHorizontalScrollBar;
			set
			{
				if (_showHorizontalScrollBar == value)
					return;
				_showHorizontalScrollBar = value;
				MarkLayoutDirty();
			}
		}

		/// <summary>
		/// Height of the horizontal scrollbar in logical pixels.
		/// </summary>
		[YamlMember]
		public float HorizontalScrollBarHeight
		{
			get => _horizontalScrollBarHeight;
			set
			{
				float newValue = Math.Max(0, value);
				if (Math.Abs(_horizontalScrollBarHeight - newValue) <= LayoutEpsilon)
					return;
				_horizontalScrollBarHeight = newValue;
				MarkLayoutDirty();
			}
		}

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
		[YamlIgnore]
		private ScrollBarH _horizontalScrollBar;

		// Track when scrollbar is driving the scroll (to avoid feedback loop)
		[YamlIgnore]
		private bool _scrollBarDriving = false;
		[YamlIgnore]
		private bool _horizontalScrollBarDriving;

		/// <summary>
		/// Runtime scrollbar children are implementation details and are never serialized.
		/// </summary>
		[YamlIgnore]
		public new List<Control> Children
		{
			get => base.Children;
			set { }
		}

		private void MarkLayoutDirty()
		{
			_layoutDirty = true;
		}

		private void NotifyTextChanged()
		{
			MarkLayoutDirty();
			_caretVisibilityPending = true;
			OnTextChanged?.Invoke(this, Text);
		}

		private void NormalizeCaret()
		{
			if (_lines.Count == 0)
				_lines.Add("");

			_cursorRow = Math.Clamp(_cursorRow, 0, _lines.Count - 1);
			_cursorColumn = Math.Clamp(_cursorColumn, 0, _lines[_cursorRow].Length);
		}

		private void EnsureVisualLayout()
		{
			EnsureViewportLayout(FishUI);
		}

		private TextViewportLayout EnsureViewportLayout(FishUI ui)
		{
			Vector2 scaledSize = GetAbsoluteSize();
			FontRef font = ui?.Settings?.FontDefault;
			float scale = ui?.Settings?.UIScale ?? 1f;
			bool geometryChanged = _viewportLayout != null &&
				(_cachedLayoutSize != scaledSize || !ReferenceEquals(_cachedLayoutFont, font) ||
				 Math.Abs(_cachedLayoutScale - scale) > LayoutEpsilon);
			bool revealAfterRebuild = geometryChanged && IsCaretVisible(_viewportLayout);

			if (!_layoutDirty && !geometryChanged && _viewportLayout != null)
				return _viewportLayout;

			NormalizeCaret();
			float lineHeight = font != null && ui != null ? ui.Graphics.MeasureText(font, "Mg").Y : Scale(16f);
			if (lineHeight <= 0)
				lineHeight = Scale(16f);

			float contentWidth = 0;
			if (font != null && ui != null)
			{
				foreach (string line in _lines)
					contentWidth = Math.Max(contentWidth, ui.Graphics.MeasureText(font, line ?? "").X);
			}

			float padding = Scale(TextPadding);
			float gutterWidth = ShowLineNumbers ? Scale(LineNumberWidth) : 0;
			float verticalWidth = Scale(ScrollBarWidth);
			float horizontalHeight = Scale(HorizontalScrollBarHeight);
			bool verticalVisible = false;
			bool horizontalVisible = false;
			List<VisualLine> visualLines = null;
			float contentHeight = 0;

			for (int iteration = 0; iteration < 4; iteration++)
			{
				float viewportWidth = Math.Max(1f, scaledSize.X - gutterWidth - padding * 2 - (verticalVisible ? verticalWidth : 0));
				float viewportHeight = Math.Max(1f, scaledSize.Y - padding * 2 - (horizontalVisible ? horizontalHeight : 0));
				visualLines = BuildVisualLines(ui, viewportWidth, font);
				contentHeight = visualLines.Count * lineHeight;
				bool nextVertical = ShowScrollBar && contentHeight > viewportHeight + LayoutEpsilon;
				bool nextHorizontal = !WordWrap && ShowHorizontalScrollBar &&
					contentWidth > viewportWidth + LayoutEpsilon;

				if (nextVertical == verticalVisible && nextHorizontal == horizontalVisible)
					break;

				verticalVisible = nextVertical;
				horizontalVisible = nextHorizontal;
			}

			float textWidth = Math.Max(1f, scaledSize.X - gutterWidth - padding * 2 - (verticalVisible ? verticalWidth : 0));
			float textHeight = Math.Max(1f, scaledSize.Y - padding * 2 - (horizontalVisible ? horizontalHeight : 0));
			visualLines = BuildVisualLines(ui, textWidth, font);
			contentHeight = visualLines.Count * lineHeight;
			bool textOverflows = !WordWrap && contentWidth > textWidth + LayoutEpsilon;
			float scrollableContentWidth = contentWidth + (textOverflows ? Scale(CaretRevealMargin) : 0);
			float visibleControlHeight = scaledSize.Y - (horizontalVisible ? horizontalHeight : 0);
			float visibleControlWidth = scaledSize.X - (verticalVisible ? verticalWidth : 0);

			_viewportLayout = new TextViewportLayout
			{
				TextRect = new ViewportRect(new Vector2(gutterWidth + padding, padding), new Vector2(textWidth, textHeight)),
				GutterRect = new ViewportRect(Vector2.Zero, new Vector2(gutterWidth, visibleControlHeight)),
				HorizontalScrollBarRect = new ViewportRect(new Vector2(0, scaledSize.Y - horizontalHeight), new Vector2(visibleControlWidth, horizontalHeight)),
				VerticalScrollBarRect = new ViewportRect(new Vector2(scaledSize.X - verticalWidth, 0), new Vector2(verticalWidth, visibleControlHeight)),
				CornerRect = new ViewportRect(new Vector2(visibleControlWidth, visibleControlHeight), new Vector2(verticalWidth, horizontalHeight)),
				HorizontalVisible = horizontalVisible,
				VerticalVisible = verticalVisible,
				TextOverflowsHorizontally = textOverflows,
				ContentWidth = contentWidth,
				ContentHeight = contentHeight,
				ScrollableContentWidth = scrollableContentWidth,
				MaxHorizontalOffset = WordWrap ? 0 : Math.Max(0, scrollableContentWidth - textWidth),
				MaxVerticalOffset = Math.Max(0, contentHeight - textHeight),
				LineHeight = lineHeight,
				Font = font,
				VisualLines = visualLines
			};

			_visualLines = visualLines;
			_cachedFont = font;
			_lineHeight = lineHeight;
			_cachedLayoutSize = scaledSize;
			_cachedLayoutFont = font;
			_cachedLayoutScale = scale;
			_layoutDirty = false;
			_scrollOffsetPixels = Math.Clamp(_scrollOffsetPixels, 0, _viewportLayout.MaxVerticalOffset);
			_horizontalScrollOffsetPixels = Math.Clamp(_horizontalScrollOffsetPixels, 0, _viewportLayout.MaxHorizontalOffset);

			if (revealAfterRebuild)
				_caretVisibilityPending = true;

			return _viewportLayout;
		}

		private List<VisualLine> BuildVisualLines(FishUI ui, float availableWidth, FontRef font)
		{
			List<VisualLine> result = new List<VisualLine>();

			for (int row = 0; row < _lines.Count; row++)
			{
				string line = _lines[row] ?? "";
				if (!WordWrap || ui == null || font == null || line.Length == 0)
				{
					result.Add(new VisualLine(row, 0, line.Length));
					continue;
				}

				int start = 0;
				while (start < line.Length)
				{
					int remaining = line.Length - start;
					int fit = 0;
					for (int length = 1; length <= remaining; length++)
					{
						float width = ui.Graphics.MeasureText(font, line.Substring(start, length)).X;
						if (width > availableWidth)
							break;
						fit = length;
					}

					if (fit == 0)
						fit = 1;

					if (fit < remaining)
					{
						int whitespaceBreak = -1;
						for (int i = start + fit - 1; i >= start; i--)
						{
							if (char.IsWhiteSpace(line[i]))
							{
								whitespaceBreak = i;
								break;
							}
						}

						if (whitespaceBreak >= start)
							fit = whitespaceBreak - start + 1;
					}

					result.Add(new VisualLine(row, start, fit));
					start += fit;
				}
			}

			if (result.Count == 0)
				result.Add(new VisualLine(0, 0, 0));

			return result;
		}

		private int GetVisualLineIndex(int logicalRow, int column)
		{
			int lastMatch = 0;
			for (int i = 0; i < _visualLines.Count; i++)
			{
				VisualLine visual = _visualLines[i];
				if (visual.LogicalRow != logicalRow)
					continue;

				lastMatch = i;
				bool isLastSegment = i == _visualLines.Count - 1 || _visualLines[i + 1].LogicalRow != logicalRow;
				if (column < visual.EndColumn || (isLastSegment && column <= visual.EndColumn))
					return i;
			}

			return lastMatch;
		}

		private void MoveCursorByVisualLines(int delta)
		{
			EnsureVisualLayout();
			if (_visualLines.Count == 0)
				return;

			int currentIndex = GetVisualLineIndex(CursorRow, CursorColumn);
			VisualLine current = _visualLines[currentIndex];
			int visualColumn = Math.Max(0, CursorColumn - current.StartColumn);
			int targetIndex = Math.Clamp(currentIndex + delta, 0, _visualLines.Count - 1);
			VisualLine target = _visualLines[targetIndex];
			CursorRow = target.LogicalRow;
			CursorColumn = target.StartColumn + Math.Min(visualColumn, target.Length);
		}

		public MultiLineEditbox()
		{
			Size = new Vector2(300, 200);
			Focusable = true;
		}

		public MultiLineEditbox(string text) : this()
		{
			Text = text;
		}

		public override void OnDeserialized(FishUI UI)
		{
			foreach (Control child in base.Children.ToArray())
				RemoveChild(child);

			_scrollBar = null;
			_horizontalScrollBar = null;
			MarkLayoutDirty();
			base.OnDeserialized(UI);
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
			NotifyTextChanged();
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
			NotifyTextChanged();
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

		private void CreateVerticalScrollBar()
		{
			if (_scrollBar != null)
				return;

			_scrollBar = new ScrollBarV { Focusable = false };
			_scrollBar.OnScrollChanged += (_, scroll, delta) =>
			{
				_scrollBarDriving = true;
				TextViewportLayout layout = EnsureViewportLayout(FishUI);
				_scrollOffsetPixels = scroll * layout.MaxVerticalOffset;
			};
			AddChild(_scrollBar);
		}

		private void CreateHorizontalScrollBar()
		{
			if (_horizontalScrollBar != null)
				return;

			_horizontalScrollBar = new ScrollBarH { Focusable = false };
			_horizontalScrollBar.OnScrollChanged += (_, scroll, delta) =>
			{
				_horizontalScrollBarDriving = true;
				TextViewportLayout layout = EnsureViewportLayout(FishUI);
				_horizontalScrollOffsetPixels = scroll * layout.MaxHorizontalOffset;
			};
			AddChild(_horizontalScrollBar);
		}

		private static Vector2 ToLogical(Vector2 scaledPixels, float scale)
		{
			return scaledPixels / Math.Max(scale, float.Epsilon);
		}

		private void UpdateScrollBars(TextViewportLayout layout)
		{
			float scale = Math.Max(UIScale, float.Epsilon);

			if (layout.VerticalVisible)
			{
				CreateVerticalScrollBar();
				_scrollBar.Position = ToLogical(layout.VerticalScrollBarRect.Position, scale);
				_scrollBar.Size = ToLogical(layout.VerticalScrollBarRect.Size, scale);
				_scrollBar.ThumbHeight = Math.Clamp(layout.TextRect.Size.Y / Math.Max(layout.ContentHeight, 1f), 0.1f, 1f);
				if (!_scrollBarDriving)
					_scrollBar.ThumbPosition = layout.MaxVerticalOffset <= LayoutEpsilon ? 0 :
						Math.Clamp(_scrollOffsetPixels / layout.MaxVerticalOffset, 0f, 1f);
				_scrollBar.Visible = true;
			}
			else if (_scrollBar != null)
			{
				_scrollBar.Visible = false;
				_scrollBar.ThumbPosition = 0;
			}

			if (layout.HorizontalVisible)
			{
				CreateHorizontalScrollBar();
				_horizontalScrollBar.Position = ToLogical(layout.HorizontalScrollBarRect.Position, scale);
				_horizontalScrollBar.Size = ToLogical(layout.HorizontalScrollBarRect.Size, scale);
				_horizontalScrollBar.ThumbWidth = Math.Clamp(
					layout.TextRect.Size.X / Math.Max(layout.ScrollableContentWidth, 1f), 0.1f, 1f);
				if (!_horizontalScrollBarDriving)
					_horizontalScrollBar.ThumbPosition = layout.MaxHorizontalOffset <= LayoutEpsilon ? 0 :
						Math.Clamp(_horizontalScrollOffsetPixels / layout.MaxHorizontalOffset, 0f, 1f);
				_horizontalScrollBar.Visible = true;
			}
			else if (_horizontalScrollBar != null)
			{
				_horizontalScrollBar.Visible = false;
				_horizontalScrollBar.ThumbPosition = 0;
			}

			_scrollBarDriving = false;
			_horizontalScrollBarDriving = false;
		}

		private float GetMaxScrollPixels()
		{
			return EnsureViewportLayout(FishUI).MaxVerticalOffset;
		}

		/// <summary>
		/// Gets the number of visible lines that fit in the control.
		/// </summary>
		public int GetVisibleLineCount()
		{
			TextViewportLayout layout = EnsureViewportLayout(FishUI);
			if (layout.LineHeight <= 0)
				return 1;
			return Math.Max(1, (int)(layout.TextRect.Size.Y / layout.LineHeight));
		}

		/// <summary>
		/// Gets the text area rectangle (excluding scrollbar).
		/// </summary>
		private (Vector2 pos, Vector2 size) GetTextAreaBounds()
		{
			TextViewportLayout layout = EnsureViewportLayout(FishUI);
			return (GetAbsolutePosition() + layout.TextRect.Position, layout.TextRect.Size);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			base.DrawControl(UI, Dt, Time);

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

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

			TextViewportLayout layout = EnsureViewportLayout(UI);
			ProcessPendingCaretVisibility(UI);
			var font = layout.Font;

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
			if (ShowLineNumbers && layout.GutterRect.Size.X > 0)
			{
				Vector2 gutterPos = pos + layout.GutterRect.Position;
				UI.Graphics.DrawRectangle(gutterPos, layout.GutterRect.Size, new FishColor(240, 240, 240, 255));
				UI.Graphics.DrawLine(
					new Vector2(gutterPos.X + layout.GutterRect.Size.X, gutterPos.Y),
					new Vector2(gutterPos.X + layout.GutterRect.Size.X, gutterPos.Y + layout.GutterRect.Size.Y),
					1f, new FishColor(200, 200, 200, 255));
			}

			Vector2 textAreaPos = pos + layout.TextRect.Position;
			Vector2 textAreaSize = layout.TextRect.Size;

			UI.Graphics.BeginScissor(textAreaPos, textAreaSize);

			// Get selection range for highlighting
			var (selStart, selEnd) = GetSelectionRange();
			bool hasSelection = HasSelection && HasFocus;

			// Draw all visual rows with pixel offset. Logical text remains unchanged.
			for (int visualIndex = 0; visualIndex < layout.VisualLines.Count; visualIndex++)
			{
				VisualLine visual = layout.VisualLines[visualIndex];
				string logicalLine = _lines[visual.LogicalRow];
				string line = logicalLine.Substring(visual.StartColumn, visual.Length);
				float lineX = textAreaPos.X - _horizontalScrollOffsetPixels;
				float lineY = textAreaPos.Y + visualIndex * layout.LineHeight - _scrollOffsetPixels;

				// Skip lines completely outside visible area
				if (lineY + layout.LineHeight < textAreaPos.Y || lineY > textAreaPos.Y + textAreaSize.Y)
					continue;

				// Draw selection highlight for this line
				if (hasSelection && font != null && visual.LogicalRow >= selStart.Row && visual.LogicalRow <= selEnd.Row)
				{
					int logicalStartCol = visual.LogicalRow == selStart.Row ? selStart.Col : 0;
					int logicalEndCol = visual.LogicalRow == selEnd.Row ? selEnd.Col : logicalLine.Length;
					int startCol = Math.Max(visual.StartColumn, logicalStartCol);
					int endCol = Math.Min(visual.EndColumn, logicalEndCol);

					if (startCol < endCol)
					{
						float selStartX = lineX;
						float selEndX = lineX;

						if (startCol > visual.StartColumn)
							selStartX += UI.Graphics.MeasureText(font, logicalLine.Substring(visual.StartColumn, startCol - visual.StartColumn)).X;
						if (endCol > visual.StartColumn)
							selEndX += UI.Graphics.MeasureText(font, logicalLine.Substring(visual.StartColumn, endCol - visual.StartColumn)).X;

						float selWidth = selEndX - selStartX;
						if (selWidth > 0)
						{
							UI.Graphics.DrawRectangle(
								new Vector2(selStartX, lineY),
								new Vector2(selWidth, layout.LineHeight),
								SelectionColor);
						}
					}
				}

				// Draw text
				if (font != null && !string.IsNullOrEmpty(line))
				{
					UI.Graphics.DrawTextColor(font, line, new Vector2(lineX, lineY), TextColor);
				}

				// Draw cursor on this line
				if (HasFocus && _cursorVisible && CursorRow == visual.LogicalRow &&
					visualIndex == GetVisualLineIndex(CursorRow, CursorColumn))
				{
					int cursorInVisual = Math.Clamp(CursorColumn - visual.StartColumn, 0, visual.Length);
					string textBeforeCursor = line.Substring(0, cursorInVisual);
					float cursorX = lineX;
					if (font != null && textBeforeCursor.Length > 0)
					{
						cursorX += UI.Graphics.MeasureText(font, textBeforeCursor).X;
					}

					UI.Graphics.DrawLine(
						new Vector2(cursorX, lineY),
						new Vector2(cursorX, lineY + layout.LineHeight),
						Scale(1f), CursorColor);
				}
			}

			// End scissor
			UI.Graphics.EndScissor();

			// Draw line numbers (in gutter area with scissoring)
			if (ShowLineNumbers && font != null && layout.GutterRect.Size.X > 0)
			{
				Vector2 gutterPos = pos + layout.GutterRect.Position;
				float gutterW = layout.GutterRect.Size.X;
				UI.Graphics.BeginScissor(gutterPos, layout.GutterRect.Size);

				for (int visualIndex = 0; visualIndex < layout.VisualLines.Count; visualIndex++)
				{
					VisualLine visual = layout.VisualLines[visualIndex];
					if (visual.StartColumn != 0)
						continue;

					float lineY = textAreaPos.Y + visualIndex * layout.LineHeight - _scrollOffsetPixels;
					if (lineY + layout.LineHeight < gutterPos.Y || lineY > gutterPos.Y + layout.GutterRect.Size.Y)
						continue;

					string lineNum = (visual.LogicalRow + 1).ToString();
					var numSize = UI.Graphics.MeasureText(font, lineNum);
					float numX = gutterPos.X + gutterW - numSize.X - Scale(8);
					UI.Graphics.DrawTextColor(font, lineNum, new Vector2(numX, lineY), LineNumberColor);
				}

				UI.Graphics.EndScissor();
			}

			// Draw placeholder inside the same text viewport.
			if (_lines.Count == 1 && string.IsNullOrEmpty(_lines[0]) && !string.IsNullOrEmpty(Placeholder) && font != null)
			{
				UI.Graphics.BeginScissor(textAreaPos, textAreaSize);
				UI.Graphics.DrawTextColor(font, Placeholder, textAreaPos, PlaceholderColor);
				UI.Graphics.EndScissor();
			}

			UpdateScrollBars(layout);
		}

		private void EnsureCursorVisible()
		{
			_caretVisibilityPending = true;
			ProcessPendingCaretVisibility(FishUI);
		}

		private Vector2 GetCaretContentPosition(TextViewportLayout layout)
		{
			NormalizeCaret();
			int visualIndex = GetVisualLineIndex(CursorRow, CursorColumn);
			VisualLine visual = layout.VisualLines[Math.Clamp(visualIndex, 0, layout.VisualLines.Count - 1)];
			int length = Math.Clamp(CursorColumn - visual.StartColumn, 0, visual.Length);
			float cursorX = 0;
			if (layout.Font != null && length > 0 && FishUI != null)
				cursorX = FishUI.Graphics.MeasureText(layout.Font, _lines[CursorRow].Substring(visual.StartColumn, length)).X;
			float cursorY = visualIndex * layout.LineHeight;
			return new Vector2(cursorX, cursorY);
		}

		private bool IsCaretVisible(TextViewportLayout layout)
		{
			if (layout == null || layout.VisualLines == null || layout.VisualLines.Count == 0)
				return false;

			Vector2 caret = GetCaretContentPosition(layout);
			float margin = layout.TextOverflowsHorizontally ? Scale(CaretRevealMargin) : 0;
			return caret.X >= _horizontalScrollOffsetPixels - LayoutEpsilon &&
				caret.X + margin <= _horizontalScrollOffsetPixels + layout.TextRect.Size.X + LayoutEpsilon &&
				caret.Y >= _scrollOffsetPixels - LayoutEpsilon &&
				caret.Y + layout.LineHeight <= _scrollOffsetPixels + layout.TextRect.Size.Y + LayoutEpsilon;
		}

		private void ProcessPendingCaretVisibility(FishUI ui)
		{
			if (!_caretVisibilityPending || ui == null)
				return;

			TextViewportLayout layout = EnsureViewportLayout(ui);
			NormalizeCaret();
			Vector2 caret = GetCaretContentPosition(layout);
			float margin = layout.TextOverflowsHorizontally ? Scale(CaretRevealMargin) : 0;

			if (caret.Y < _scrollOffsetPixels)
				_scrollOffsetPixels = caret.Y;
			else if (caret.Y + layout.LineHeight > _scrollOffsetPixels + layout.TextRect.Size.Y)
				_scrollOffsetPixels = caret.Y + layout.LineHeight - layout.TextRect.Size.Y;

			if (WordWrap)
				_horizontalScrollOffsetPixels = 0;
			else if (caret.X < _horizontalScrollOffsetPixels)
				_horizontalScrollOffsetPixels = caret.X;
			else if (caret.X + margin > _horizontalScrollOffsetPixels + layout.TextRect.Size.X)
				_horizontalScrollOffsetPixels = caret.X + margin - layout.TextRect.Size.X;

			_scrollOffsetPixels = Math.Clamp(_scrollOffsetPixels, 0, layout.MaxVerticalOffset);
			_horizontalScrollOffsetPixels = Math.Clamp(_horizontalScrollOffsetPixels, 0, layout.MaxHorizontalOffset);
			_caretVisibilityPending = false;
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
						if (WordWrap)
							MoveCursorByVisualLines(-1);
						else
							MoveCursorUpInternal();
						ExtendSelection();
					}
					else
					{
						ClearSelection();
						if (WordWrap)
							MoveCursorByVisualLines(-1);
						else
							MoveCursorUpInternal();
					}
					break;
				case FishKey.Down:
					if (InState.ShiftDown)
					{
						StartSelection();
						if (WordWrap)
							MoveCursorByVisualLines(1);
						else
							MoveCursorDownInternal();
						ExtendSelection();
					}
					else
					{
						ClearSelection();
						if (WordWrap)
							MoveCursorByVisualLines(1);
						else
							MoveCursorDownInternal();
					}
					break;
				case FishKey.Home:
					int homeColumn = 0;
					if (WordWrap)
					{
						EnsureVisualLayout();
						homeColumn = _visualLines[GetVisualLineIndex(CursorRow, CursorColumn)].StartColumn;
					}
					if (InState.ShiftDown)
					{
						StartSelection();
						CursorColumn = homeColumn;
						ExtendSelection();
					}
					else
					{
						CursorColumn = homeColumn;
						ClearSelection();
					}
					break;
				case FishKey.End:
					int endColumn = _lines[CursorRow].Length;
					if (WordWrap)
					{
						EnsureVisualLayout();
						endColumn = _visualLines[GetVisualLineIndex(CursorRow, CursorColumn)].EndColumn;
					}
					if (InState.ShiftDown)
					{
						StartSelection();
						CursorColumn = endColumn;
						ExtendSelection();
					}
					else
					{
						CursorColumn = endColumn;
						ClearSelection();
					}
					break;
				case FishKey.PageUp:
					{
						int visibleLines = GetVisibleLineCount();
						if (WordWrap)
						{
							if (InState.ShiftDown)
								StartSelection();
							else
								ClearSelection();
							MoveCursorByVisualLines(-visibleLines);
							if (InState.ShiftDown)
								ExtendSelection();
							break;
						}
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
						if (WordWrap)
						{
							if (InState.ShiftDown)
								StartSelection();
							else
								ClearSelection();
							MoveCursorByVisualLines(visibleLines);
							if (InState.ShiftDown)
								ExtendSelection();
							break;
						}
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
				EnsureCursorVisible();
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

			TextViewportLayout layout = EnsureViewportLayout(UI);
			float scrollAmount = layout.LineHeight * 3f * (Delta > 0 ? -1 : 1);
			_scrollOffsetPixels = Math.Clamp(_scrollOffsetPixels + scrollAmount, 0, layout.MaxVerticalOffset);
		}

		private void PositionCursorFromMouse(FishUI UI, Vector2 mousePos)
		{
			TextViewportLayout layout = EnsureViewportLayout(UI);
			Vector2 localMouse = mousePos - GetAbsolutePosition();

			// Calculate clicked visual row based on pixel position.
			int visualIndex = (int)((localMouse.Y - layout.TextRect.Position.Y + _scrollOffsetPixels) / layout.LineHeight);
			visualIndex = Math.Clamp(visualIndex, 0, layout.VisualLines.Count - 1);
			VisualLine visual = layout.VisualLines[visualIndex];
			CursorRow = visual.LogicalRow;

			// Calculate clicked column
			string line = _lines[CursorRow];
			if (layout.Font != null && visual.Length > 0)
			{
				float relativeX = localMouse.X - layout.TextRect.Position.X + _horizontalScrollOffsetPixels;
				int col = visual.StartColumn;
				float accumulatedWidth = 0f;

				for (int i = visual.StartColumn; i < visual.EndColumn; i++)
				{
					float charWidth = UI.Graphics.MeasureText(layout.Font, line[i].ToString()).X;
					if (accumulatedWidth + charWidth / 2 >= relativeX)
						break;
					accumulatedWidth += charWidth;
					col++;
				}
				CursorColumn = col;
			}
			else
			{
				CursorColumn = visual.StartColumn;
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
			NotifyTextChanged();
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
			NotifyTextChanged();
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
			NotifyTextChanged();
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
			NotifyTextChanged();
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

			NotifyTextChanged();
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
			_horizontalScrollOffsetPixels = 0;
			NotifyTextChanged();
		}

		/// <summary>
		/// Scrolls to the end of the text.
		/// </summary>
		public void ScrollToEnd()
		{
			CursorRow = _lines.Count - 1;
			CursorColumn = _lines[CursorRow].Length;
			EnsureCursorVisible();
		}

		/// <summary>
		/// Scrolls to the beginning of the text.
		/// </summary>
		public void ScrollToStart()
		{
			_scrollOffsetPixels = 0;
			_horizontalScrollOffsetPixels = 0;
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
