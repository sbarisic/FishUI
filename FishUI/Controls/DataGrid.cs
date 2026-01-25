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
		private Vector2 _scrollOffset = Vector2.Zero;
		[YamlIgnore]
		private ScrollBarV _scrollBar;
		private float _rowHeight;

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
		/// Height of each data row. If 0, uses font height + padding.
		/// </summary>
		[YamlMember]
		public float RowHeight { get; set; } = 0f;

		/// <summary>
		/// Whether to show the vertical scrollbar.
		/// </summary>
		[YamlMember]
		public bool ShowScrollBar { get; set; } = true;

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
		public FishColor EvenRowColor { get; set; } = new FishColor(255, 255, 255, 20);

		/// <summary>
		/// Background color for odd rows.
		/// </summary>
		[YamlMember]
		public FishColor OddRowColor { get; set; } = new FishColor(0, 0, 0, 20);

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

		public void AddColumn(DataGridColumn column) => _columns.Add(column);

		public void AddColumn(string header, float width = 100f, bool sortable = true)
			=> _columns.Add(new DataGridColumn(header, width, sortable));

		public IReadOnlyList<DataGridColumn> Columns => _columns;

		public void ClearColumns() => _columns.Clear();

		#endregion

		#region Row Management

		public void AddRow(DataGridRow row) => _rows.Add(row);

		public void AddRow(params string[] cells) => _rows.Add(new DataGridRow(cells));

		public IReadOnlyList<DataGridRow> Rows => _rows;

		public void ClearRows()
		{
			_rows.Clear();
			_selectedIndex = -1;
			_selectedIndices.Clear();
			_scrollOffset = Vector2.Zero;
		}

		public DataGridRow GetRow(int index)
			=> index >= 0 && index < _rows.Count ? _rows[index] : null;

		#endregion

		#region Selection

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

		public int[] GetSelectedIndices()
		{
			if (MultiSelect)
				return _selectedIndices.OrderBy(i => i).ToArray();
			else if (_selectedIndex >= 0)
				return new int[] { _selectedIndex };
			else
				return Array.Empty<int>();
		}

		public void ClearSelection()
		{
			_selectedIndex = -1;
			_selectedIndices.Clear();
			_selectionAnchor = -1;
		}

		public bool IsIndexSelected(int index)
		{
			if (MultiSelect)
				return _selectedIndices.Contains(index);
			return index == _selectedIndex;
		}

		#endregion

		#region Sorting

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

			ClearSelection();
			OnColumnSort?.Invoke(this, columnIndex, direction);
		}

		#endregion

		#region ScrollBar

		private void CreateScrollBar(FishUI UI)
		{
			if (_scrollBar != null)
				return;

			RemoveAllChildren();

			_scrollBar = new ScrollBarV();
			_scrollBar.Position = new Vector2(Size.X - 16, Scale(HeaderHeight));
			_scrollBar.Size = new Vector2(16, Size.Y - Scale(HeaderHeight));
			_scrollBar.ThumbHeight = 0.5f;
			_scrollBar.OnScrollChanged += (_, scroll, delta) =>
			{
				float contentHeight = _rows.Count * _rowHeight;
				_scrollOffset = new Vector2(0, -scroll * contentHeight);
			};

			AddChild(_scrollBar);
		}

		#endregion

		#region Rendering

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			// Calculate row height based on font if not set
			_rowHeight = RowHeight > 0 ? Scale(RowHeight) : UI.Settings.FontDefault.Size + 4;

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();
			var font = UI.Settings.FontDefault;
			float headerH = Scale(HeaderHeight);

			// Create/update scrollbar
			if (ShowScrollBar)
			{
				CreateScrollBar(UI);
				if (_scrollBar != null)
				{
					_scrollBar.Position = new Vector2(Size.X - 16, HeaderHeight);
					_scrollBar.Size = new Vector2(16, Size.Y - HeaderHeight);

					float contentHeight = _rows.Count * _rowHeight;
					float viewHeight = size.Y - headerH;
					_scrollBar.Visible = contentHeight > viewHeight;
				}
			}
			else if (_scrollBar != null)
			{
				RemoveChild(_scrollBar);
				_scrollBar = null;
			}

			float scrollBarW = (_scrollBar?.Visible ?? false) ? _scrollBar.GetAbsoluteSize().X : 0;
			float contentWidth = size.X - scrollBarW;

			// Draw background using ListBox theme
			NPatch bgPatch = UI.Settings.ImgListBoxNormal;
			if (bgPatch != null)
			{
				UI.Graphics.DrawNPatch(bgPatch, pos, size, Color);
			}
			else
			{
				UI.Graphics.DrawRectangle(pos, size, new FishColor(255, 255, 255, 255));
				UI.Graphics.DrawRectangleOutline(pos, size, new FishColor(128, 128, 128, 255));
			}

			// Draw header
			DrawHeader(UI, pos, contentWidth, headerH, font);

			// Draw rows with scissoring
			float rowAreaY = pos.Y + headerH;
			float rowAreaHeight = size.Y - headerH;
			UI.Graphics.PushScissor(new Vector2(pos.X + 2, rowAreaY), new Vector2(contentWidth - 4, rowAreaHeight));
			DrawRows(UI, new Vector2(pos.X + 2, rowAreaY), contentWidth - 4, rowAreaHeight, font, scrollBarW);
			UI.Graphics.PopScissor();
		}

		private void DrawHeader(FishUI UI, Vector2 pos, float width, float height, FontRef font)
		{
			float x = pos.X;

			// Draw header background
			NPatch headerBg = UI.Settings.ImgButtonNormal;
			if (headerBg != null)
			{
				UI.Graphics.DrawNPatch(headerBg, new Vector2(pos.X, pos.Y), new Vector2(width, height), Color);
			}
			else
			{
				UI.Graphics.DrawRectangle(new Vector2(pos.X, pos.Y), new Vector2(width, height), new FishColor(220, 220, 220, 255));
			}

			for (int i = 0; i < _columns.Count; i++)
			{
				var col = _columns[i];
				float colW = Scale(col.Width);
				if (x + colW > pos.X + width)
					colW = pos.X + width - x;

				if (colW <= 0) break;

				// Column separator
				if (i > 0)
				{
					UI.Graphics.DrawRectangle(new Vector2(x, pos.Y + 2), new Vector2(1, height - 4), new FishColor(180, 180, 180, 255));
				}

				// Hover highlight
				if (i == _hoveredColumnIndex && _resizingColumnIndex < 0)
				{
					UI.Graphics.DrawRectangle(new Vector2(x, pos.Y), new Vector2(colW, height), new FishColor(0, 0, 0, 30));
				}

				// Header text with sort indicator
				if (font != null)
				{
					string text = col.Header;
					if (col.SortDirection == SortDirection.Ascending)
						text += " ^";
					else if (col.SortDirection == SortDirection.Descending)
						text += " v";

					var textSize = UI.Graphics.MeasureText(font, text);
					float textX = x + 4;
					float textY = pos.Y + (height - textSize.Y) / 2;

					UI.Graphics.PushScissor(new Vector2(x + 2, pos.Y), new Vector2(colW - 4, height));
					UI.Graphics.DrawTextColor(font, text, new Vector2(textX, textY), new FishColor(0, 0, 0, 255));
					UI.Graphics.PopScissor();
				}

				x += colW;
			}

			// Bottom border
			UI.Graphics.DrawRectangle(new Vector2(pos.X, pos.Y + height - 1), new Vector2(width, 1), new FishColor(160, 160, 160, 255));
		}

		private void DrawRows(FishUI UI, Vector2 pos, float width, float height, FontRef font, float scrollBarW)
		{
			for (int i = 0; i < _rows.Count; i++)
			{
				var row = _rows[i];
				float y = pos.Y + i * _rowHeight + _scrollOffset.Y;

				// Skip rows outside visible area
				if (y + _rowHeight < pos.Y) continue;
				if (y > pos.Y + height) break;

				bool isSelected = IsIndexSelected(i);
				bool isHovered = (i == _hoveredRowIndex);

				// Draw alternating row background (like ListBox)
				if (AlternatingRowColors && !isSelected && !isHovered)
				{
					FishColor rowColor = (i % 2 == 0) ? EvenRowColor : OddRowColor;
					UI.Graphics.DrawRectangle(new Vector2(pos.X, y), new Vector2(width, _rowHeight), rowColor);
				}

				// Draw selection/hover using theme (like ListBox)
				NPatch itemPatch = null;
				FishColor textColor = FishColor.Black;

				if (isHovered && isSelected)
				{
					itemPatch = UI.Settings.ImgListBoxItmSelectedHovered;
					textColor = FishColor.White;
				}
				else if (isHovered)
				{
					itemPatch = UI.Settings.ImgListBoxItmHovered;
				}
				else if (isSelected)
				{
					itemPatch = UI.Settings.ImgListBoxItmSelected;
					textColor = FishColor.White;
				}

				if (itemPatch != null)
				{
					UI.Graphics.DrawNPatch(itemPatch, new Vector2(pos.X, y), new Vector2(width, _rowHeight), Color);
				}

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
						float textY = y + (_rowHeight - textSize.Y) / 2;

						UI.Graphics.PushScissor(new Vector2(x + 2, y), new Vector2(colW - 4, _rowHeight));
						UI.Graphics.DrawTextColor(font, cellText, new Vector2(textX, textY), textColor);
						UI.Graphics.PopScissor();
					}

					x += colW;
				}
			}
		}

		#endregion

		#region Input Handling

		private int PickRowFromPosition(Vector2 localPos)
		{
			float headerH = Scale(HeaderHeight);
			if (localPos.Y < headerH) return -1;

			float relY = localPos.Y - headerH - _scrollOffset.Y;
			int rowIdx = (int)(relY / _rowHeight);

			if (rowIdx < 0 || rowIdx >= _rows.Count)
				return -1;

			return rowIdx;
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);

			Vector2 ctrlPos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();
			Vector2 localPos = Pos - ctrlPos;
			float headerH = Scale(HeaderHeight);
			float scrollBarW = (_scrollBar?.Visible ?? false) ? _scrollBar.GetAbsoluteSize().X : 0;
			float contentWidth = size.X - scrollBarW;

			// Handle column resize dragging
			if (_resizingColumnIndex >= 0)
			{
				float delta = Pos.X - _resizeStartX;
				float scale = UI.Settings.UIScale > 0 ? UI.Settings.UIScale : 1f;
				float newWidth = Math.Max(_columns[_resizingColumnIndex].MinWidth, _resizeStartWidth + delta / scale);
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
			if (localPos.Y >= 0 && localPos.Y < headerH && _hoverResizeColumnIndex < 0)
			{
				x = 0;
				for (int i = 0; i < _columns.Count; i++)
				{
					float colW = Scale(_columns[i].Width);
					if (localPos.X >= x && localPos.X < x + colW)
					{
						_hoveredColumnIndex = i;
						break;
					}
					x += colW;
				}
			}

			// Check row hover
			_hoveredRowIndex = -1;
			if (localPos.X >= 0 && localPos.X < contentWidth && localPos.Y >= headerH)
			{
				_hoveredRowIndex = PickRowFromPosition(localPos);
			}
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			if (Btn != FishMouseButton.Left)
				return;

			Vector2 ctrlPos = GetAbsolutePosition();
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
				HandleRowSelection(InState);
			}
		}

		private void HandleRowSelection(FishInputState InState)
		{
			if (_hoveredRowIndex < 0) return;

			if (MultiSelect)
			{
				if (InState.CtrlDown)
				{
					// Toggle selection
					if (_selectedIndices.Contains(_hoveredRowIndex))
						_selectedIndices.Remove(_hoveredRowIndex);
					else
						_selectedIndices.Add(_hoveredRowIndex);
					_selectionAnchor = _hoveredRowIndex;
					_selectedIndex = _hoveredRowIndex;
				}
				else if (InState.ShiftDown && _selectionAnchor >= 0)
				{
					// Range select
					_selectedIndices.Clear();
					int start = Math.Min(_selectionAnchor, _hoveredRowIndex);
					int end = Math.Max(_selectionAnchor, _hoveredRowIndex);
					for (int i = start; i <= end; i++)
						_selectedIndices.Add(i);
					_selectedIndex = _hoveredRowIndex;
				}
				else
				{
					// Single select
					_selectedIndices.Clear();
					_selectedIndices.Add(_hoveredRowIndex);
					_selectionAnchor = _hoveredRowIndex;
					_selectedIndex = _hoveredRowIndex;
				}
				OnRowSelected?.Invoke(this, _hoveredRowIndex, _rows[_hoveredRowIndex]);
			}
			else
			{
				SelectRow(_hoveredRowIndex);
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

			if (_scrollBar != null && _scrollBar.Visible)
			{
				// Delegate to scrollbar
				if (Delta > 0)
					_scrollBar.ScrollUp();
				else if (Delta < 0)
					_scrollBar.ScrollDown();
			}
			else
			{
				// Direct scrolling
				float headerH = Scale(HeaderHeight);
				float viewHeight = GetAbsoluteSize().Y - headerH;
				float contentHeight = _rows.Count * _rowHeight;

				_scrollOffset.Y += Delta * _rowHeight * 3;

				float maxScroll = 0;
				float minScroll = Math.Min(0, viewHeight - contentHeight);
				_scrollOffset.Y = Math.Clamp(_scrollOffset.Y, minScroll, maxScroll);
			}
		}

		public override void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
			base.HandleKeyPress(UI, InState, Key);

			if (_selectedIndex >= 0 && _selectedIndex < _rows.Count)
			{
				if (Key == FishKey.Up)
					SelectRow(_selectedIndex - 1);
				else if (Key == FishKey.Down)
					SelectRow(_selectedIndex + 1);
			}
		}

		#endregion
	}
}
