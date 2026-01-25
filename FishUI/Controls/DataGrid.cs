using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Specifies the sort direction for a column.
	/// </summary>
	public enum SortDirection
	{
		None,
		Ascending,
		Descending
	}

	/// <summary>
	/// Represents a column definition in a DataGrid.
	/// </summary>
	public class DataGridColumn
	{
		/// <summary>
		/// Header text displayed at the top of the column.
		/// </summary>
		public string Header { get; set; } = "Column";

		/// <summary>
		/// Width of the column in pixels.
		/// </summary>
		public float Width { get; set; } = 100f;

		/// <summary>
		/// Minimum width when resizing.
		/// </summary>
		public float MinWidth { get; set; } = 30f;

		/// <summary>
		/// Whether this column can be sorted by clicking the header.
		/// </summary>
		public bool Sortable { get; set; } = true;

		/// <summary>
		/// Whether this column can be resized by dragging the header border.
		/// </summary>
		public bool Resizable { get; set; } = true;

		/// <summary>
		/// Current sort direction for this column.
		/// </summary>
		public SortDirection SortDirection { get; set; } = SortDirection.None;

		public DataGridColumn() { }

		public DataGridColumn(string header, float width = 100f, bool sortable = true)
		{
			Header = header;
			Width = width;
			Sortable = sortable;
		}
	}

	/// <summary>
	/// Represents a row of data in a DataGrid.
	/// </summary>
	public class DataGridRow
	{
		/// <summary>
		/// Cell values for each column.
		/// </summary>
		public List<string> Cells { get; set; } = new List<string>();

		/// <summary>
		/// User data associated with this row.
		/// </summary>
		public object UserData { get; set; }

		public DataGridRow() { }

		public DataGridRow(params string[] cells)
		{
			Cells = new List<string>(cells);
		}

		public string this[int index]
		{
			get => index >= 0 && index < Cells.Count ? Cells[index] : "";
			set
			{
				while (Cells.Count <= index)
					Cells.Add("");
				Cells[index] = value;
			}
		}
	}

	/// <summary>
	/// Delegate for row selection events.
	/// </summary>
	public delegate void DataGridRowSelectedFunc(DataGrid grid, int rowIndex, DataGridRow row);

	/// <summary>
	/// Delegate for column sort events.
	/// </summary>
	public delegate void DataGridColumnSortFunc(DataGrid grid, int columnIndex, SortDirection direction);

	/// <summary>
	/// A multi-column list control with sortable headers, resizable columns, and row selection.
	/// </summary>
	public class DataGrid : Control
	{
		private List<DataGridColumn> _columns = new List<DataGridColumn>();
		private List<DataGridRow> _rows = new List<DataGridRow>();
		private int _selectedIndex = -1;
		private int _hoveredRowIndex = -1;
		private int _hoveredColumnIndex = -1;
		private HashSet<int> _selectedIndices = new HashSet<int>();
		private int _selectionAnchor = -1;

		// Scrolling
		private float _scrollOffset = 0;
		private ScrollBarV _scrollBar;

		// Column resize
		private int _resizingColumnIndex = -1;
		private float _resizeStartX = 0;
		private float _resizeStartWidth = 0;
		private int _hoverResizeColumnIndex = -1;

		/// <summary>
		/// Height of the header row.
		/// </summary>
		[YamlMember]
		public float HeaderHeight { get; set; } = 24f;

		/// <summary>
		/// Height of each data row.
		/// </summary>
		[YamlMember]
		public float RowHeight { get; set; } = 22f;

		/// <summary>
		/// Whether to show the vertical scrollbar.
		/// </summary>
		[YamlMember]
		public bool ShowScrollBar { get; set; } = true;

		/// <summary>
		/// Width of the scrollbar.
		/// </summary>
		[YamlMember]
		public float ScrollBarWidth { get; set; } = 16f;

		/// <summary>
		/// Whether to allow multiple row selection.
		/// </summary>
		[YamlMember]
		public bool MultiSelect { get; set; } = false;

		/// <summary>
		/// Whether to display alternating row colors.
		/// </summary>
		[YamlMember]
		public bool AlternatingRowColors { get; set; } = true;

		/// <summary>
		/// Background color for even rows.
		/// </summary>
		[YamlMember]
		public FishColor EvenRowColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Background color for odd rows.
		/// </summary>
		[YamlMember]
		public FishColor OddRowColor { get; set; } = new FishColor(240, 240, 240, 255);

		/// <summary>
		/// Background color for selected rows.
		/// </summary>
		[YamlMember]
		public FishColor SelectedRowColor { get; set; } = new FishColor(51, 153, 255, 255);

		/// <summary>
		/// Background color for hovered rows.
		/// </summary>
		[YamlMember]
		public FishColor HoveredRowColor { get; set; } = new FishColor(200, 220, 255, 255);

		/// <summary>
		/// Background color for column headers.
		/// </summary>
		[YamlMember]
		public FishColor HeaderColor { get; set; } = new FishColor(220, 220, 220, 255);

		/// <summary>
		/// Background color for hovered column header.
		/// </summary>
		[YamlMember]
		public FishColor HeaderHoverColor { get; set; } = new FishColor(200, 200, 200, 255);

		/// <summary>
		/// Gets or sets the selected row index.
		/// </summary>
		[YamlIgnore]
		public int SelectedIndex
		{
			get => _selectedIndex;
			set => SelectRow(value);
		}

		/// <summary>
		/// Event raised when a row is selected.
		/// </summary>
		public event DataGridRowSelectedFunc OnRowSelected;

		/// <summary>
		/// Event raised when a column is sorted.
		/// </summary>
		public event DataGridColumnSortFunc OnColumnSort;

		public DataGrid()
		{
			Size = new Vector2(400, 200);
			Focusable = true;
		}

		#region Column Management

		/// <summary>
		/// Adds a column to the grid.
		/// </summary>
		public void AddColumn(DataGridColumn column)
		{
			_columns.Add(column);
		}

		/// <summary>
		/// Adds a column with the specified header and width.
		/// </summary>
		public void AddColumn(string header, float width = 100f, bool sortable = true)
		{
			_columns.Add(new DataGridColumn(header, width, sortable));
		}

		/// <summary>
		/// Gets all columns.
		/// </summary>
		public IReadOnlyList<DataGridColumn> Columns => _columns;

		/// <summary>
		/// Clears all columns.
		/// </summary>
		public void ClearColumns()
		{
			_columns.Clear();
		}

		#endregion

		#region Row Management

		/// <summary>
		/// Adds a row to the grid.
		/// </summary>
		public void AddRow(DataGridRow row)
		{
			_rows.Add(row);
		}

		/// <summary>
		/// Adds a row with the specified cell values.
		/// </summary>
		public void AddRow(params string[] cells)
		{
			_rows.Add(new DataGridRow(cells));
		}

		/// <summary>
		/// Gets all rows.
		/// </summary>
		public IReadOnlyList<DataGridRow> Rows => _rows;

		/// <summary>
		/// Clears all rows.
		/// </summary>
		public void ClearRows()
		{
			_rows.Clear();
			_selectedIndex = -1;
			_selectedIndices.Clear();
			_scrollOffset = 0;
		}

		/// <summary>
		/// Gets the row at the specified index.
		/// </summary>
		public DataGridRow GetRow(int index)
		{
			return index >= 0 && index < _rows.Count ? _rows[index] : null;
		}

		#endregion

		#region Selection

		/// <summary>
		/// Selects a row by index.
		/// </summary>
		public void SelectRow(int index)
		{
			if (index < 0) index = -1;
			if (index >= _rows.Count) index = _rows.Count - 1;

			int lastIndex = _selectedIndex;
			_selectedIndex = index;

			if (lastIndex != _selectedIndex && _selectedIndex >= 0)
			{
				OnRowSelected?.Invoke(this, _selectedIndex, _rows[_selectedIndex]);
			}
		}

		/// <summary>
		/// Gets all selected row indices.
		/// </summary>
		public int[] GetSelectedIndices()
		{
			if (MultiSelect)
				return _selectedIndices.OrderBy(i => i).ToArray();
			else if (_selectedIndex >= 0)
				return new int[] { _selectedIndex };
			else
				return Array.Empty<int>();
		}

		/// <summary>
		/// Clears all selections.
		/// </summary>
		public void ClearSelection()
		{
			_selectedIndex = -1;
			_selectedIndices.Clear();
			_selectionAnchor = -1;
		}

		#endregion

		#region Sorting

		/// <summary>
		/// Sorts the grid by the specified column.
		/// </summary>
		public void SortByColumn(int columnIndex, SortDirection direction)
		{
			if (columnIndex < 0 || columnIndex >= _columns.Count)
				return;

			// Clear sort on other columns
			for (int i = 0; i < _columns.Count; i++)
			{
				if (i != columnIndex)
					_columns[i].SortDirection = SortDirection.None;
			}

			_columns[columnIndex].SortDirection = direction;

			if (direction != SortDirection.None)
			{
				_rows = direction == SortDirection.Ascending
					? _rows.OrderBy(r => r[columnIndex], StringComparer.OrdinalIgnoreCase).ToList()
					: _rows.OrderByDescending(r => r[columnIndex], StringComparer.OrdinalIgnoreCase).ToList();
			}

			// Clear selection after sort
			ClearSelection();

			OnColumnSort?.Invoke(this, columnIndex, direction);
		}

		#endregion

		#region Rendering

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = ScaledSize;
			var font = UI.Settings.FontDefault;

			float headerH = Scale(HeaderHeight);
			float rowH = Scale(RowHeight);
			float scrollBarW = ShowScrollBar ? Scale(ScrollBarWidth) : 0;
			float contentWidth = size.X - scrollBarW;

			// Draw background
			UI.Graphics.DrawRectangle(pos, size, new FishColor(255, 255, 255, 255));
			UI.Graphics.DrawRectangleOutline(pos, size, new FishColor(128, 128, 128, 255));

			// Draw header
			DrawHeader(UI, pos, contentWidth, headerH, font);

			// Draw rows
			float rowAreaY = pos.Y + headerH;
			float rowAreaHeight = size.Y - headerH;
			UI.Graphics.BeginScissor(new Vector2(pos.X, rowAreaY), new Vector2(contentWidth, rowAreaHeight));
			DrawRows(UI, new Vector2(pos.X, rowAreaY), contentWidth, rowAreaHeight, rowH, font);
			UI.Graphics.EndScissor();

			// Draw scrollbar
			if (ShowScrollBar)
			{
				DrawScrollBar(UI, pos, size, headerH, rowH);
			}
		}

		private void DrawHeader(FishUI UI, Vector2 pos, float width, float height, FontRef font)
		{
			float x = pos.X;

			for (int i = 0; i < _columns.Count; i++)
			{
				var col = _columns[i];
				float colW = Scale(col.Width);
				if (x + colW > pos.X + width)
					colW = pos.X + width - x;

				if (colW <= 0) break;

				// Header background
				FishColor bgColor = (i == _hoveredColumnIndex && _resizingColumnIndex < 0)
					? HeaderHoverColor : HeaderColor;
				UI.Graphics.DrawRectangle(new Vector2(x, pos.Y), new Vector2(colW, height), bgColor);
				UI.Graphics.DrawRectangleOutline(new Vector2(x, pos.Y), new Vector2(colW, height), new FishColor(180, 180, 180, 255));

				// Header text
				if (font != null)
				{
					string text = col.Header;
					// Add sort indicator
					if (col.SortDirection == SortDirection.Ascending)
						text += " ^";
					else if (col.SortDirection == SortDirection.Descending)
						text += " v";

					var textSize = UI.Graphics.MeasureText(font, text);
					float textX = x + 4;
					float textY = pos.Y + (height - textSize.Y) / 2;
					UI.Graphics.BeginScissor(new Vector2(x + 2, pos.Y), new Vector2(colW - 4, height));
					UI.Graphics.DrawTextColor(font, text, new Vector2(textX, textY), new FishColor(0, 0, 0, 255));
					UI.Graphics.EndScissor();
				}

				x += colW;
			}
		}

		private void DrawRows(FishUI UI, Vector2 pos, float width, float height, float rowH, FontRef font)
		{
			int startRow = (int)(_scrollOffset / rowH);
			int visibleRows = (int)(height / rowH) + 2;

			float y = pos.Y - (_scrollOffset % rowH);

			for (int i = startRow; i < Math.Min(startRow + visibleRows, _rows.Count); i++)
			{
				if (y + rowH < pos.Y) { y += rowH; continue; }
				if (y > pos.Y + height) break;

				var row = _rows[i];
				bool isSelected = MultiSelect ? _selectedIndices.Contains(i) : (i == _selectedIndex);
				bool isHovered = i == _hoveredRowIndex && !isSelected;

				// Row background
				FishColor bgColor;
				if (isSelected)
					bgColor = SelectedRowColor;
				else if (isHovered)
					bgColor = HoveredRowColor;
				else if (AlternatingRowColors)
					bgColor = (i % 2 == 0) ? EvenRowColor : OddRowColor;
				else
					bgColor = EvenRowColor;

				UI.Graphics.DrawRectangle(new Vector2(pos.X, y), new Vector2(width, rowH), bgColor);

				// Draw cells
				float x = pos.X;
				for (int c = 0; c < _columns.Count; c++)
				{
					float colW = Scale(_columns[c].Width);
					if (x + colW > pos.X + width)
						colW = pos.X + width - x;

					if (colW <= 0) break;

					string cellText = row[c];
					if (font != null && !string.IsNullOrEmpty(cellText))
					{
						var textSize = UI.Graphics.MeasureText(font, cellText);
						float textX = x + 4;
						float textY = y + (rowH - textSize.Y) / 2;

						UI.Graphics.BeginScissor(new Vector2(x + 2, y), new Vector2(colW - 4, rowH));
						FishColor textColor = isSelected ? new FishColor(255, 255, 255, 255) : new FishColor(0, 0, 0, 255);
						UI.Graphics.DrawTextColor(font, cellText, new Vector2(textX, textY), textColor);
						UI.Graphics.EndScissor();
					}

					x += colW;
				}

				// Row separator
				UI.Graphics.DrawLine(new Vector2(pos.X, y + rowH), new Vector2(pos.X + width, y + rowH), 1f, new FishColor(220, 220, 220, 255));

				y += rowH;
			}
		}

		private void DrawScrollBar(FishUI UI, Vector2 pos, Vector2 size, float headerH, float rowH)
		{
			float totalHeight = _rows.Count * rowH;
			float viewHeight = size.Y - headerH;
			float scrollBarW = Scale(ScrollBarWidth);

			if (totalHeight <= viewHeight)
				return;

			Vector2 sbPos = new Vector2(pos.X + size.X - scrollBarW, pos.Y + headerH);
			Vector2 sbSize = new Vector2(scrollBarW, viewHeight);

			// Track
			UI.Graphics.DrawRectangle(sbPos, sbSize, new FishColor(230, 230, 230, 255));

			// Thumb
			float thumbRatio = viewHeight / totalHeight;
			float thumbHeight = Math.Max(20, viewHeight * thumbRatio);
			float thumbY = sbPos.Y + (_scrollOffset / totalHeight) * (viewHeight - thumbHeight);

			UI.Graphics.DrawRectangle(new Vector2(sbPos.X + 2, thumbY), new Vector2(scrollBarW - 4, thumbHeight), new FishColor(180, 180, 180, 255));
		}

		#endregion

		#region Input Handling

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);

			Vector2 ctrlPos = GetAbsolutePosition();
			Vector2 size = ScaledSize;
			float headerH = Scale(HeaderHeight);
			float rowH = Scale(RowHeight);
			float scrollBarW = ShowScrollBar ? Scale(ScrollBarWidth) : 0;
			float contentWidth = size.X - scrollBarW;

			// Handle column resize dragging
			if (_resizingColumnIndex >= 0)
			{
				float delta = Pos.X - _resizeStartX;
				float newWidth = Math.Max(_columns[_resizingColumnIndex].MinWidth, _resizeStartWidth + delta / (UI.Settings.UIScale > 0 ? UI.Settings.UIScale : 1f));
				_columns[_resizingColumnIndex].Width = newWidth;
				return;
			}

			// Check for resize handle hover
			_hoverResizeColumnIndex = -1;
			float x = ctrlPos.X;
			for (int i = 0; i < _columns.Count; i++)
			{
				x += Scale(_columns[i].Width);
				if (Math.Abs(Pos.X - x) < 4 && Pos.Y >= ctrlPos.Y && Pos.Y < ctrlPos.Y + headerH)
				{
					if (_columns[i].Resizable)
						_hoverResizeColumnIndex = i;
					break;
				}
			}

			// Check header hover
			_hoveredColumnIndex = -1;
			if (Pos.Y >= ctrlPos.Y && Pos.Y < ctrlPos.Y + headerH && _hoverResizeColumnIndex < 0)
			{
				x = ctrlPos.X;
				for (int i = 0; i < _columns.Count; i++)
				{
					float colW = Scale(_columns[i].Width);
					if (Pos.X >= x && Pos.X < x + colW)
					{
						_hoveredColumnIndex = i;
						break;
					}
					x += colW;
				}
			}

			// Check row hover
			_hoveredRowIndex = -1;
			if (Pos.Y >= ctrlPos.Y + headerH && Pos.X >= ctrlPos.X && Pos.X < ctrlPos.X + contentWidth)
			{
				float relY = Pos.Y - (ctrlPos.Y + headerH) + _scrollOffset;
				int rowIdx = (int)(relY / rowH);
				if (rowIdx >= 0 && rowIdx < _rows.Count)
					_hoveredRowIndex = rowIdx;
			}
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			if (Btn != FishMouseButton.Left)
				return;

			Vector2 ctrlPos = GetAbsolutePosition();
			Vector2 size = ScaledSize;
			float headerH = Scale(HeaderHeight);

			// Start column resize
			if (_hoverResizeColumnIndex >= 0)
			{
				_resizingColumnIndex = _hoverResizeColumnIndex;
				_resizeStartX = Pos.X;
				_resizeStartWidth = _columns[_resizingColumnIndex].Width;
				return;
			}

			// Header click - sort
			if (_hoveredColumnIndex >= 0 && Pos.Y >= ctrlPos.Y && Pos.Y < ctrlPos.Y + headerH)
			{
				var col = _columns[_hoveredColumnIndex];
				if (col.Sortable)
				{
					SortDirection newDir = col.SortDirection == SortDirection.Ascending
						? SortDirection.Descending
						: SortDirection.Ascending;
					SortByColumn(_hoveredColumnIndex, newDir);
				}
				return;
			}

			// Row click - select
			if (_hoveredRowIndex >= 0)
			{
				bool ctrl = InState.CtrlDown;
				bool shift = InState.ShiftDown;

				if (MultiSelect)
				{
					if (shift && _selectionAnchor >= 0)
					{
						// Range select
						_selectedIndices.Clear();
						int start = Math.Min(_selectionAnchor, _hoveredRowIndex);
						int end = Math.Max(_selectionAnchor, _hoveredRowIndex);
						for (int i = start; i <= end; i++)
							_selectedIndices.Add(i);
					}
					else if (ctrl)
					{
						// Toggle select
						if (_selectedIndices.Contains(_hoveredRowIndex))
							_selectedIndices.Remove(_hoveredRowIndex);
						else
							_selectedIndices.Add(_hoveredRowIndex);
						_selectionAnchor = _hoveredRowIndex;
					}
					else
					{
						// Single select
						_selectedIndices.Clear();
						_selectedIndices.Add(_hoveredRowIndex);
						_selectionAnchor = _hoveredRowIndex;
					}
					_selectedIndex = _hoveredRowIndex;
					OnRowSelected?.Invoke(this, _hoveredRowIndex, _rows[_hoveredRowIndex]);
				}
				else
				{
					SelectRow(_hoveredRowIndex);
				}
			}
		}

		public override void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseRelease(UI, InState, Btn, Pos);
			_resizingColumnIndex = -1;
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float Delta)
		{
			base.HandleMouseWheel(UI, InState, Delta);

			float rowH = Scale(RowHeight);
			float headerH = Scale(HeaderHeight);
			float viewHeight = ScaledSize.Y - headerH;
			float totalHeight = _rows.Count * rowH;

			_scrollOffset -= Delta * rowH * 3;
			_scrollOffset = Math.Max(0, Math.Min(_scrollOffset, Math.Max(0, totalHeight - viewHeight)));
		}

		#endregion
	}
}
