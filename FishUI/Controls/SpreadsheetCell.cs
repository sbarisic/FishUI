using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Represents a single editable cell in a SpreadsheetGrid.
	/// </summary>
	public class SpreadsheetCell : Control
	{
		private string _value = "";
		private string _editValue = "";
		private bool _isEditing = false;
		private int _cursorPos = 0;

		/// <summary>
		/// Gets or sets the cell's text value.
		/// </summary>
		[YamlMember]
		public string Value
		{
			get => _value;
			set
			{
				if (_value != value)
				{
					_value = value ?? "";
					OnValueChanged?.Invoke(this, _value);
				}
			}
		}

		/// <summary>
		/// Gets or sets whether this cell is currently selected.
		/// </summary>
		[YamlIgnore]
		public bool IsSelected { get; set; } = false;

		/// <summary>
		/// Gets whether this cell is currently in edit mode.
		/// </summary>
		[YamlIgnore]
		public bool IsEditing => _isEditing;

		/// <summary>
		/// Row index of this cell in the grid.
		/// </summary>
		[YamlIgnore]
		public int Row { get; set; } = 0;

		/// <summary>
		/// Column index of this cell in the grid.
		/// </summary>
		[YamlIgnore]
		public int Column { get; set; } = 0;

		/// <summary>
		/// Background color for selected cells.
		/// </summary>
		[YamlMember]
		public FishColor SelectedColor { get; set; } = new FishColor(51, 153, 255, 80);

		/// <summary>
		/// Background color when editing.
		/// </summary>
		[YamlMember]
		public FishColor EditingColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Event raised when the cell value changes.
		/// </summary>
		public event Action<SpreadsheetCell, string> OnValueChanged;

		/// <summary>
		/// Event raised when editing is completed.
		/// </summary>
		public event Action<SpreadsheetCell, bool> OnEditComplete; // bool = committed (true) or cancelled (false)

		public SpreadsheetCell()
		{
			Size = new Vector2(80, 24);
			Focusable = true;
		}

		public SpreadsheetCell(string value) : this()
		{
			_value = value ?? "";
		}

		/// <summary>
		/// Begins editing this cell.
		/// </summary>
		public void BeginEdit()
		{
			if (_isEditing) return;
			_isEditing = true;
			_editValue = _value;
			_cursorPos = _editValue.Length;
		}

		/// <summary>
		/// Commits the current edit.
		/// </summary>
		public void CommitEdit()
		{
			if (!_isEditing) return;
			_isEditing = false;
			Value = _editValue;
			OnEditComplete?.Invoke(this, true);
		}

		/// <summary>
		/// Cancels the current edit.
		/// </summary>
		public void CancelEdit()
		{
			if (!_isEditing) return;
			_isEditing = false;
			_editValue = _value;
			OnEditComplete?.Invoke(this, false);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();
			var font = UI.Settings.FontDefault;

			// Background
			if (_isEditing)
			{
				// Editing background
				NPatch textboxBg = UI.Settings.ImgTextboxActive;
				if (textboxBg != null)
					UI.Graphics.DrawNPatch(textboxBg, pos, size, Color);
				else
					UI.Graphics.DrawRectangle(pos, size, EditingColor);
			}
			else if (IsSelected)
			{
				// Selected background
				UI.Graphics.DrawRectangle(pos, size, SelectedColor);
			}

			// Border
			UI.Graphics.DrawRectangleOutline(pos, size, new FishColor(200, 200, 200, 255));

			// Text
			if (font != null)
			{
				string displayText = _isEditing ? _editValue : _value;
				if (!string.IsNullOrEmpty(displayText))
				{
					var textSize = UI.Graphics.MeasureText(font, displayText);
					float textX = pos.X + 3;
					float textY = pos.Y + (size.Y - textSize.Y) / 2;

					UI.Graphics.PushScissor(pos + new Vector2(2, 0), size - new Vector2(4, 0));
					UI.Graphics.DrawTextColor(font, displayText, new Vector2(textX, textY), FishColor.Black);
					UI.Graphics.PopScissor();
				}

				// Draw cursor when editing
				if (_isEditing)
				{
					string beforeCursor = _editValue.Substring(0, Math.Min(_cursorPos, _editValue.Length));
					float cursorX = pos.X + 3 + UI.Graphics.MeasureText(font, beforeCursor).X;
					float cursorY1 = pos.Y + 3;
					float cursorY2 = pos.Y + size.Y - 3;

					// Blinking cursor
					if ((int)(Time * 2) % 2 == 0)
					{
						UI.Graphics.DrawLine(new Vector2(cursorX, cursorY1), new Vector2(cursorX, cursorY2), 1f, FishColor.Black);
					}
				}
			}

			// Selection border (thicker)
			if (IsSelected && !_isEditing)
			{
				UI.Graphics.DrawRectangleOutline(pos, size, new FishColor(51, 153, 255, 255));
				UI.Graphics.DrawRectangleOutline(pos + new Vector2(1, 1), size - new Vector2(2, 2), new FishColor(51, 153, 255, 255));
			}
		}

		public override void HandleTextInput(FishUI UI, FishInputState InState, char Character)
		{
			if (!_isEditing) return;

			if (!char.IsControl(Character))
			{
				_editValue = _editValue.Insert(_cursorPos, Character.ToString());
				_cursorPos++;
			}
		}

		public override void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
			if (!_isEditing) return;

			switch (Key)
			{
				case FishKey.Enter:
					CommitEdit();
					break;
				case FishKey.Escape:
					CancelEdit();
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

		public override void HandleMouseDoubleClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			if (Btn == FishMouseButton.Left && IsSelected)
			{
				BeginEdit();
			}
		}
	}
}
