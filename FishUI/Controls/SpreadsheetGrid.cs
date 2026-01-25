using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Delegate for cell value changed events.
	/// </summary>
	public delegate void SpreadsheetCellChangedFunc(SpreadsheetGrid grid, int row, int column, string oldValue, string newValue);

	/// <summary>
	/// Delegate for cell selection changed events.
	/// </summary>
	public delegate void SpreadsheetSelectionChangedFunc(SpreadsheetGrid grid, int row, int column);

	/// <summary>
	/// A spreadsheet-like grid control with editable cells, row/column headers, and navigation.
	/// </summary>
	public class SpreadsheetGrid : Control
	{
		private int _rowCount = 10;
		private int _columnCount = 5;
		private List<List<string>> _cellData = new List<List<string>>();
		private int _selectedRow = 0;
		private int _selectedCol = 0;
		private int _editingRow = -1;
		private int _editingCol = -1;
		private string _editValue = "";
		private int _cursorPos = 0;

		// Scrolling
		private Vector2 _scrollOffset = Vector2.Zero;
		[YamlIgnore]
		private ScrollBarV _scrollBarV;
		[YamlIgnore]
		private ScrollBarH _scrollBarH;

		// Hover state
		private int _hoveredRow = -1;
		private int _hoveredCol = -1;

		/// <summary>
		/// Number of rows in the grid.
		/// </summary>
		[YamlMember]
		public int RowCount
		{
			get => _rowCount;
			set
			{
				_rowCount = Math.Max(1, value);
				EnsureDataSize();
			}
		}

		/// <summary>
		/// Number of columns in the grid.
		/// </summary>
		[YamlMember]
		public int ColumnCount
		{
			get => _columnCount;
			set
			{
				_columnCount = Math.Max(1, value);
				EnsureDataSize();
			}
		}

		/// <summary>
		/// Width of each cell.
		/// </summary>
		[YamlMember]
		public float CellWidth { get; set; } = 80f;

		/// <summary>
		/// Height of each cell.
		/// </summary>
		[YamlMember]
		public float CellHeight { get; set; } = 24f;

		/// <summary>
		/// Width of the row header (showing row numbers).
		/// </summary>
		[YamlMember]
		public float RowHeaderWidth { get; set; } = 40f;

		/// <summary>
		/// Height of the column header (showing A, B, C...).
		/// </summary>
		[YamlMember]
		public float ColumnHeaderHeight { get; set; } = 24f;

		/// <summary>
		/// Color for header cells.
		/// </summary>
		[YamlMember]
		public FishColor HeaderColor { get; set; } = new FishColor(230, 230, 230, 255);

		/// <summary>
		/// Color for selected cell highlight.
		/// </summary>
		[YamlMember]
		public FishColor SelectedCellColor { get; set; } = new FishColor(51, 153, 255, 80);

		/// <summary>
		/// Color for hovered cell.
		/// </summary>
		[YamlMember]
		public FishColor HoveredCellColor { get; set; } = new FishColor(200, 220, 255, 100);

		/// <summary>
		/// Gets the currently selected row.
		/// </summary>
		[YamlIgnore]
		public int SelectedRow => _selectedRow;

		/// <summary>
		/// Gets the currently selected column.
		/// </summary>
		[YamlIgnore]
		public int SelectedColumn => _selectedCol;

		/// <summary>
		/// Gets whether a cell is currently being edited.
		/// </summary>
		[YamlIgnore]
		public bool IsEditing => _editingRow >= 0 && _editingCol >= 0;

		/// <summary>
		/// Event raised when a cell value changes.
		/// </summary>
		public event SpreadsheetCellChangedFunc OnCellChanged;

		/// <summary>
		/// Event raised when selection changes.
		/// </summary>
		public event SpreadsheetSelectionChangedFunc OnSelectionChanged;

		public SpreadsheetGrid()
		{
			Size = new Vector2(450, 280);
			Focusable = true;
			EnsureDataSize();
		}

		private void CreateScrollBars()
		{
			if (_scrollBarV == null)
			{
				_scrollBarV = new ScrollBarV();
				_scrollBarV.OnScrollChanged += (_, scroll, delta) =>
				{
					float cellH = Scale(CellHeight);
					float contentHeight = _rowCount * cellH;
					_scrollOffset.Y = -scroll * contentHeight;
				};
				AddChild(_scrollBarV);
			}

			if (_scrollBarH == null)
			{
				_scrollBarH = new ScrollBarH();
				_scrollBarH.OnScrollChanged += (_, scroll, delta) =>
				{
					float cellW = Scale(CellWidth);
					float contentWidth = _columnCount * cellW;
					_scrollOffset.X = -scroll * contentWidth;
				};
				AddChild(_scrollBarH);
			}
		}

		private void UpdateScrollBars(FishUI UI)
		{
			float rowHeaderW = Scale(RowHeaderWidth);
			float colHeaderH = Scale(ColumnHeaderHeight);
			float cellW = Scale(CellWidth);
			float cellH = Scale(CellHeight);
			Vector2 size = GetAbsoluteSize();

			float contentWidth = _columnCount * cellW;
			float contentHeight = _rowCount * cellH;
			float viewWidth = size.X - rowHeaderW - 16;
			float viewHeight = size.Y - colHeaderH - 16;

			// Update vertical scrollbar
			_scrollBarV.Position = new Vector2(Size.X - 16, ColumnHeaderHeight);
			_scrollBarV.Size = new Vector2(16, Size.Y - ColumnHeaderHeight - 16);
			_scrollBarV.Visible = contentHeight > viewHeight;
			if (_scrollBarV.Visible)
			{
				_scrollBarV.ThumbHeight = Math.Clamp(viewHeight / contentHeight, 0.1f, 1f);
			}

			// Update horizontal scrollbar
			_scrollBarH.Position = new Vector2(RowHeaderWidth, Size.Y - 16);
			_scrollBarH.Size = new Vector2(Size.X - RowHeaderWidth - 16, 16);
			_scrollBarH.Visible = contentWidth > viewWidth;
			if (_scrollBarH.Visible)
			{
				_scrollBarH.ThumbWidth = Math.Clamp(viewWidth / contentWidth, 0.1f, 1f);
			}
		}

		private void EnsureDataSize()
		{
			// Ensure we have enough rows
			while (_cellData.Count < _rowCount)
				_cellData.Add(new List<string>());

			// Ensure each row has enough columns
			for (int r = 0; r < _rowCount; r++)
			{
				while (_cellData[r].Count < _columnCount)
					_cellData[r].Add("");
			}
		}

		#region Cell Data Access

		/// <summary>
		/// Gets the value of a cell.
		/// </summary>
		public string GetCell(int row, int column)
		{
			if (row < 0 || row >= _rowCount || column < 0 || column >= _columnCount)
				return "";
			EnsureDataSize();
			return _cellData[row][column];
		}

		/// <summary>
		/// Sets the value of a cell.
		/// </summary>
		public void SetCell(int row, int column, string value)
		{
			if (row < 0 || row >= _rowCount || column < 0 || column >= _columnCount)
				return;
			EnsureDataSize();
			string oldValue = _cellData[row][column];
			_cellData[row][column] = value ?? "";
			if (oldValue != _cellData[row][column])
			{
				OnCellChanged?.Invoke(this, row, column, oldValue, _cellData[row][column]);
			}
		}

		/// <summary>
		/// Clears all cell data.
		/// </summary>
		public void ClearAll()
		{
			for (int r = 0; r < _cellData.Count; r++)
			{
				for (int c = 0; c < _cellData[r].Count; c++)
				{
					_cellData[r][c] = "";
				}
			}
		}

		/// <summary>
		/// Gets the column letter (A, B, C... AA, AB...).
		/// </summary>
		public static string GetColumnLetter(int column)
		{
			string result = "";
			column++;
			while (column > 0)
			{
				column--;
				result = (char)('A' + column % 26) + result;
				column /= 26;
			}
			return result;
		}

		#endregion

		#region Selection & Editing

		/// <summary>
		/// Selects a cell.
		/// </summary>
		public void SelectCell(int row, int column)
		{
			// Commit any pending edit
			if (IsEditing)
				CommitEdit();

			row = Math.Clamp(row, 0, _rowCount - 1);
			column = Math.Clamp(column, 0, _columnCount - 1);

			if (_selectedRow != row || _selectedCol != column)
			{
				_selectedRow = row;
				_selectedCol = column;
				OnSelectionChanged?.Invoke(this, row, column);
				EnsureCellVisible(row, column);
			}
		}

		/// <summary>
		/// Begins editing the selected cell.
		/// </summary>
		public void BeginEdit()
		{
			if (IsEditing) return;
			_editingRow = _selectedRow;
			_editingCol = _selectedCol;
			_editValue = GetCell(_editingRow, _editingCol);
			_cursorPos = _editValue.Length;
		}

		/// <summary>
		/// Commits the current edit.
		/// </summary>
		public void CommitEdit()
		{
			if (!IsEditing) return;
			SetCell(_editingRow, _editingCol, _editValue);
			_editingRow = -1;
			_editingCol = -1;
		}

		/// <summary>
		/// Cancels the current edit.
		/// </summary>
		public void CancelEdit()
		{
			if (!IsEditing) return;
			_editingRow = -1;
			_editingCol = -1;
		}

		private void EnsureCellVisible(int row, int column)
		{
			float cellW = Scale(CellWidth);
			float cellH = Scale(CellHeight);
			float rowHeaderW = Scale(RowHeaderWidth);
			float colHeaderH = Scale(ColumnHeaderHeight);
			Vector2 size = GetAbsoluteSize();

			float viewWidth = size.X - rowHeaderW - 16; // scrollbar
			float viewHeight = size.Y - colHeaderH - 16;

			float cellX = column * cellW;
			float cellY = row * cellH;

			// Horizontal scroll
			if (cellX + _scrollOffset.X < 0)
				_scrollOffset.X = -cellX;
			else if (cellX + cellW + _scrollOffset.X > viewWidth)
				_scrollOffset.X = viewWidth - cellX - cellW;

			// Vertical scroll
			if (cellY + _scrollOffset.Y < 0)
				_scrollOffset.Y = -cellY;
			else if (cellY + cellH + _scrollOffset.Y > viewHeight)
				_scrollOffset.Y = viewHeight - cellY - cellH;

			// Sync scrollbar positions
			SyncScrollBarsFromOffset();
		}

		private void SyncScrollBarsFromOffset()
		{
			float cellW = Scale(CellWidth);
			float cellH = Scale(CellHeight);
			float contentWidth = _columnCount * cellW;
			float contentHeight = _rowCount * cellH;

			if (_scrollBarV != null && contentHeight > 0)
			{
				_scrollBarV.ThumbPosition = -_scrollOffset.Y / contentHeight;
			}
			if (_scrollBarH != null && contentWidth > 0)
			{
				_scrollBarH.ThumbPosition = -_scrollOffset.X / contentWidth;
			}
		}

		#endregion

		#region Rendering

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			// Create and update scrollbars
			CreateScrollBars();
			UpdateScrollBars(UI);

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();
			var font = UI.Settings.FontDefault;

			float cellW = Scale(CellWidth);
			float cellH = Scale(CellHeight);
			float rowHeaderW = Scale(RowHeaderWidth);
			float colHeaderH = Scale(ColumnHeaderHeight);


			// Background
			NPatch bgPatch = UI.Settings.ImgListBoxNormal;
			if (bgPatch != null)
				UI.Graphics.DrawNPatch(bgPatch, pos, size, Color);
			else
			{
				UI.Graphics.DrawRectangle(pos, size, new FishColor(255, 255, 255, 255));
				UI.Graphics.DrawRectangleOutline(pos, size, new FishColor(128, 128, 128, 255));
			}

			// Draw corner header cell
			UI.Graphics.DrawRectangle(pos, new Vector2(rowHeaderW, colHeaderH), HeaderColor);
			UI.Graphics.DrawRectangleOutline(pos, new Vector2(rowHeaderW, colHeaderH), new FishColor(180, 180, 180, 255));

			// Draw column headers
			DrawColumnHeaders(UI, pos, size, cellW, colHeaderH, rowHeaderW, font);

			// Draw row headers
			DrawRowHeaders(UI, pos, size, cellH, colHeaderH, rowHeaderW, font);

			// Draw cells with scissoring
			Vector2 cellAreaPos = pos + new Vector2(rowHeaderW, colHeaderH);
			Vector2 cellAreaSize = size - new Vector2(rowHeaderW + 16, colHeaderH + 16);
			UI.Graphics.PushScissor(cellAreaPos, cellAreaSize);
			DrawCells(UI, cellAreaPos, cellAreaSize, cellW, cellH, font, Time);
			UI.Graphics.PopScissor();
		}

		private void DrawColumnHeaders(FishUI UI, Vector2 pos, Vector2 size, float cellW, float headerH, float rowHeaderW, FontRef font)
		{
			float startX = pos.X + rowHeaderW + _scrollOffset.X;
			float viewWidth = size.X - rowHeaderW - 16;

			UI.Graphics.PushScissor(new Vector2(pos.X + rowHeaderW, pos.Y), new Vector2(viewWidth, headerH));

			for (int c = 0; c < _columnCount; c++)
			{
				float x = startX + c * cellW;
				if (x + cellW < pos.X + rowHeaderW) continue;
				if (x > pos.X + size.X) break;

				// Header background
				FishColor bgColor = (c == _selectedCol) ? new FishColor(200, 200, 200, 255) : HeaderColor;
				UI.Graphics.DrawRectangle(new Vector2(x, pos.Y), new Vector2(cellW, headerH), bgColor);
				UI.Graphics.DrawRectangleOutline(new Vector2(x, pos.Y), new Vector2(cellW, headerH), new FishColor(180, 180, 180, 255));

				// Header text (A, B, C...)
				if (font != null)
				{
					string text = GetColumnLetter(c);
					var textSize = UI.Graphics.MeasureText(font, text);
					float textX = x + (cellW - textSize.X) / 2;
					float textY = pos.Y + (headerH - textSize.Y) / 2;
					UI.Graphics.DrawTextColor(font, text, new Vector2(textX, textY), FishColor.Black);
				}
			}

			UI.Graphics.PopScissor();
		}

		private void DrawRowHeaders(FishUI UI, Vector2 pos, Vector2 size, float cellH, float colHeaderH, float headerW, FontRef font)
		{
			float startY = pos.Y + colHeaderH + _scrollOffset.Y;
			float viewHeight = size.Y - colHeaderH - 16;

			UI.Graphics.PushScissor(new Vector2(pos.X, pos.Y + colHeaderH), new Vector2(headerW, viewHeight));

			for (int r = 0; r < _rowCount; r++)
			{
				float y = startY + r * cellH;
				if (y + cellH < pos.Y + colHeaderH) continue;
				if (y > pos.Y + size.Y) break;

				// Header background
				FishColor bgColor = (r == _selectedRow) ? new FishColor(200, 200, 200, 255) : HeaderColor;
				UI.Graphics.DrawRectangle(new Vector2(pos.X, y), new Vector2(headerW, cellH), bgColor);
				UI.Graphics.DrawRectangleOutline(new Vector2(pos.X, y), new Vector2(headerW, cellH), new FishColor(180, 180, 180, 255));

				// Header text (1, 2, 3...)
				if (font != null)
				{
					string text = (r + 1).ToString();
					var textSize = UI.Graphics.MeasureText(font, text);
					float textX = pos.X + (headerW - textSize.X) / 2;
					float textY = y + (cellH - textSize.Y) / 2;
					UI.Graphics.DrawTextColor(font, text, new Vector2(textX, textY), FishColor.Black);
				}
			}

			UI.Graphics.PopScissor();
		}

		private void DrawCells(FishUI UI, Vector2 areaPos, Vector2 areaSize, float cellW, float cellH, FontRef font, float time)
		{
			Vector2 startPos = areaPos + _scrollOffset;

			for (int r = 0; r < _rowCount; r++)
			{
				for (int c = 0; c < _columnCount; c++)
				{
					float x = startPos.X + c * cellW;
					float y = startPos.Y + r * cellH;

					// Skip cells outside visible area
					if (x + cellW < areaPos.X || x > areaPos.X + areaSize.X) continue;
					if (y + cellH < areaPos.Y || y > areaPos.Y + areaSize.Y) continue;

					Vector2 cellPos = new Vector2(x, y);
					Vector2 cellSize = new Vector2(cellW, cellH);

					bool isSelected = (r == _selectedRow && c == _selectedCol);
					bool isEditing = (r == _editingRow && c == _editingCol);
					bool isHovered = (r == _hoveredRow && c == _hoveredCol);

					// Cell background
					if (isEditing)
					{
						UI.Graphics.DrawRectangle(cellPos, cellSize, new FishColor(255, 255, 255, 255));
					}
					else if (isSelected)
					{
						UI.Graphics.DrawRectangle(cellPos, cellSize, SelectedCellColor);
					}
					else if (isHovered)
					{
						UI.Graphics.DrawRectangle(cellPos, cellSize, HoveredCellColor);
					}

					// Cell border
					UI.Graphics.DrawRectangleOutline(cellPos, cellSize, new FishColor(220, 220, 220, 255));

					// Cell text
					string text = isEditing ? _editValue : GetCell(r, c);
					if (font != null && !string.IsNullOrEmpty(text))
					{
						var textSize = UI.Graphics.MeasureText(font, text);
						float textX = x + 3;
						float textY = y + (cellH - textSize.Y) / 2;

						UI.Graphics.PushScissor(cellPos + new Vector2(2, 0), cellSize - new Vector2(4, 0));
						UI.Graphics.DrawTextColor(font, text, new Vector2(textX, textY), FishColor.Black);
						UI.Graphics.PopScissor();
					}

					// Cursor when editing
					if (isEditing && font != null)
					{
						string beforeCursor = _editValue.Substring(0, Math.Min(_cursorPos, _editValue.Length));
						float cursorX = x + 3 + UI.Graphics.MeasureText(font, beforeCursor).X;

						if ((int)(time * 2) % 2 == 0)
						{
							UI.Graphics.DrawLine(new Vector2(cursorX, y + 3), new Vector2(cursorX, y + cellH - 3), 1f, FishColor.Black);
						}
					}

					// Selection border (thicker for selected cell)
					if (isSelected && !isEditing)
					{
						UI.Graphics.DrawRectangleOutline(cellPos, cellSize, new FishColor(51, 153, 255, 255));
						UI.Graphics.DrawRectangleOutline(cellPos + new Vector2(1, 1), cellSize - new Vector2(2, 2), new FishColor(51, 153, 255, 255));
					}
				}
			}
		}

		#endregion

		#region Input Handling

		private (int row, int col) GetCellFromPosition(Vector2 pos)
		{
			Vector2 ctrlPos = GetAbsolutePosition();
			float rowHeaderW = Scale(RowHeaderWidth);
			float colHeaderH = Scale(ColumnHeaderHeight);
			float cellW = Scale(CellWidth);
			float cellH = Scale(CellHeight);

			float localX = pos.X - ctrlPos.X - rowHeaderW - _scrollOffset.X;
			float localY = pos.Y - ctrlPos.Y - colHeaderH - _scrollOffset.Y;

			int col = (int)(localX / cellW);
			int row = (int)(localY / cellH);

			if (row < 0 || row >= _rowCount || col < 0 || col >= _columnCount)
				return (-1, -1);

			return (row, col);
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);

			var (row, col) = GetCellFromPosition(Pos);
			_hoveredRow = row;
			_hoveredCol = col;
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			if (Btn != FishMouseButton.Left) return;

			var (row, col) = GetCellFromPosition(Pos);
			if (row >= 0 && col >= 0)
			{
				SelectCell(row, col);
			}
		}

		public override void HandleMouseDoubleClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			if (Btn != FishMouseButton.Left) return;

			var (row, col) = GetCellFromPosition(Pos);
			if (row >= 0 && col >= 0 && row == _selectedRow && col == _selectedCol)
			{
				BeginEdit();
			}
		}

		public override void HandleTextInput(FishUI UI, FishInputState InState, char Character)
		{
			if (!IsEditing)
			{
				// Start editing on any printable character
				if (!char.IsControl(Character))
				{
					BeginEdit();
					_editValue = "";
					_cursorPos = 0;
				}
				else return;
			}

			if (!char.IsControl(Character))
			{
				_editValue = _editValue.Insert(_cursorPos, Character.ToString());
				_cursorPos++;
			}
		}

		public override void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
			if (IsEditing)
			{
				HandleEditingKeyPress(Key, InState);
			}
			else
			{
				HandleNavigationKeyPress(Key, InState);
			}
		}

		private void HandleEditingKeyPress(FishKey Key, FishInputState InState)
		{
			switch (Key)
			{
				case FishKey.Enter:
					CommitEdit();
					// Move down after enter
					if (_selectedRow < _rowCount - 1)
						SelectCell(_selectedRow + 1, _selectedCol);
					break;
				case FishKey.Escape:
					CancelEdit();
					break;
				case FishKey.Tab:
					CommitEdit();
					// Move right (or wrap to next row)
					if (_selectedCol < _columnCount - 1)
						SelectCell(_selectedRow, _selectedCol + 1);
					else if (_selectedRow < _rowCount - 1)
						SelectCell(_selectedRow + 1, 0);
					break;
				case FishKey.Backspace:
					if (_cursorPos > 0 && _editValue.Length > 0)
					{
						_editValue = _editValue.Remove(_cursorPos - 1, 1);
						_cursorPos--;
					}
					break;
				case FishKey.Delete:
					if (_cursorPos < _editValue.Length)
					{
						_editValue = _editValue.Remove(_cursorPos, 1);
					}
					break;
				case FishKey.Left:
					if (_cursorPos > 0) _cursorPos--;
					break;
				case FishKey.Right:
					if (_cursorPos < _editValue.Length) _cursorPos++;
					break;
				case FishKey.Home:
					_cursorPos = 0;
					break;
				case FishKey.End:
					_cursorPos = _editValue.Length;
					break;
			}
		}

		private void HandleNavigationKeyPress(FishKey Key, FishInputState InState)
		{
			switch (Key)
			{
				case FishKey.Up:
					if (_selectedRow > 0)
						SelectCell(_selectedRow - 1, _selectedCol);
					break;
				case FishKey.Down:
					if (_selectedRow < _rowCount - 1)
						SelectCell(_selectedRow + 1, _selectedCol);
					break;
				case FishKey.Left:
					if (_selectedCol > 0)
						SelectCell(_selectedRow, _selectedCol - 1);
					break;
				case FishKey.Right:
					if (_selectedCol < _columnCount - 1)
						SelectCell(_selectedRow, _selectedCol + 1);
					break;
				case FishKey.Tab:
					if (InState.ShiftDown)
					{
						// Move left (or wrap to previous row)
						if (_selectedCol > 0)
							SelectCell(_selectedRow, _selectedCol - 1);
						else if (_selectedRow > 0)
							SelectCell(_selectedRow - 1, _columnCount - 1);
					}
					else
					{
						// Move right (or wrap to next row)
						if (_selectedCol < _columnCount - 1)
							SelectCell(_selectedRow, _selectedCol + 1);
						else if (_selectedRow < _rowCount - 1)
							SelectCell(_selectedRow + 1, 0);
					}
					break;
				case FishKey.Enter:
					BeginEdit();
					break;
				case FishKey.Delete:
					// Clear selected cell
					SetCell(_selectedRow, _selectedCol, "");
					break;
				case FishKey.Home:
					if (InState.CtrlDown)
						SelectCell(0, 0);
					else
						SelectCell(_selectedRow, 0);
					break;
				case FishKey.End:
					if (InState.CtrlDown)
						SelectCell(_rowCount - 1, _columnCount - 1);
					else
						SelectCell(_selectedRow, _columnCount - 1);
					break;
			}
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float Delta)
		{
			float cellH = Scale(CellHeight);
			float colHeaderH = Scale(ColumnHeaderHeight);
			float viewHeight = GetAbsoluteSize().Y - colHeaderH - 16;
			float contentHeight = _rowCount * cellH;


			_scrollOffset.Y += Delta * cellH * 3;

			float maxScroll = 0;
			float minScroll = Math.Min(0, viewHeight - contentHeight);
			_scrollOffset.Y = Math.Clamp(_scrollOffset.Y, minScroll, maxScroll);

			// Sync scrollbar position
			SyncScrollBarsFromOffset();
		}

		#endregion
	}
}
