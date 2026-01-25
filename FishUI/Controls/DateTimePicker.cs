using System;
using System.Globalization;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Delegate for DateTimePicker value changed events.
	/// </summary>
	public delegate void DateTimePickerValueChangedFunc(DateTimePicker sender, DateTime value);

	/// <summary>
	/// A date picker control with calendar popup for date selection.
	/// </summary>
	public class DateTimePicker : Control
	{
		/// <summary>
		/// Static list of currently open date pickers for overlay rendering.
		/// </summary>
		internal static List<DateTimePicker> OpenPickers = new List<DateTimePicker>();

		private DateTime _value = DateTime.Today;
		private DateTime _displayMonth;
		private bool _isOpen = false;
		private int _hoveredDay = -1;
		private bool _dropdownButtonHovered = false;
		private bool _prevMonthHovered = false;
		private bool _nextMonthHovered = false;
		private bool _prevYearHovered = false;
		private bool _nextYearHovered = false;

		/// <summary>
		/// Gets or sets the selected date value.
		/// </summary>
		[YamlMember]
		public DateTime Value
		{
			get => _value;
			set
			{
				if (_value != value)
				{
					_value = value;
					_displayMonth = new DateTime(value.Year, value.Month, 1);
					OnValueChanged?.Invoke(this, _value);
				}
			}
		}

		/// <summary>
		/// Gets or sets the date format string for display.
		/// </summary>
		[YamlMember]
		public string DateFormat { get; set; } = "yyyy-MM-dd";

		/// <summary>
		/// Gets or sets the minimum selectable date.
		/// </summary>
		[YamlMember]
		public DateTime MinDate { get; set; } = DateTime.MinValue;

		/// <summary>
		/// Gets or sets the maximum selectable date.
		/// </summary>
		[YamlMember]
		public DateTime MaxDate { get; set; } = DateTime.MaxValue;

		/// <summary>
		/// Width of the dropdown button.
		/// </summary>
		[YamlMember]
		public float DropdownButtonWidth { get; set; } = 24f;

		/// <summary>
		/// Size of the calendar popup.
		/// </summary>
		[YamlMember]
		public Vector2 CalendarSize { get; set; } = new Vector2(220, 200);

		/// <summary>
		/// Whether the calendar popup is currently open.
		/// </summary>
		[YamlIgnore]
		public bool IsOpen => _isOpen;

		/// <summary>
		/// Background color of the calendar popup.
		/// </summary>
		[YamlMember]
		public FishColor CalendarBackgroundColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Color for the selected day.
		/// </summary>
		[YamlMember]
		public FishColor SelectedDayColor { get; set; } = new FishColor(51, 153, 255, 255);

		/// <summary>
		/// Color for hovered day.
		/// </summary>
		[YamlMember]
		public FishColor HoveredDayColor { get; set; } = new FishColor(200, 220, 255, 255);

		/// <summary>
		/// Color for today's date indicator.
		/// </summary>
		[YamlMember]
		public FishColor TodayColor { get; set; } = new FishColor(255, 200, 200, 255);

		/// <summary>
		/// Color for days outside the current month.
		/// </summary>
		[YamlMember]
		public FishColor OutsideMonthColor { get; set; } = new FishColor(180, 180, 180, 255);

		/// <summary>
		/// Color for weekend days.
		/// </summary>
		[YamlMember]
		public FishColor WeekendColor { get; set; } = new FishColor(200, 50, 50, 255);

		/// <summary>
		/// Event raised when the selected date changes.
		/// </summary>
		public event DateTimePickerValueChangedFunc OnValueChanged;

		// Calendar layout constants
		private const float HeaderHeight = 28f;
		private const float DayOfWeekHeight = 20f;
		private const int DaysInWeek = 7;
		private const int MaxWeeksDisplayed = 6;

		public DateTimePicker()
		{
			Size = new Vector2(150, 24);
			Focusable = true;
			_displayMonth = new DateTime(_value.Year, _value.Month, 1);
		}

		public DateTimePicker(DateTime value) : this()
		{
			Value = value;
		}

		/// <summary>
		/// Opens the calendar popup.
		/// </summary>
		public void Open()
		{
			_isOpen = true;
			_displayMonth = new DateTime(_value.Year, _value.Month, 1);
			AlwaysOnTop = true;
			BringToFront();
			if (!OpenPickers.Contains(this))
				OpenPickers.Add(this);
		}

		/// <summary>
		/// Closes the calendar popup.
		/// </summary>
		public void Close()
		{
			_isOpen = false;
			_hoveredDay = -1;
			AlwaysOnTop = false;
			OpenPickers.Remove(this);
		}

		/// <summary>
		/// Toggles the calendar popup.
		/// </summary>
		public void Toggle()
		{
			if (_isOpen)
				Close();
			else
				Open();
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			base.DrawControl(UI, Dt, Time);

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = ScaledSize;
			float buttonWidth = Scale(DropdownButtonWidth);

			// Draw textbox background
			NPatch bg = HasFocus ? UI.Settings.ImgTextboxActive : UI.Settings.ImgTextboxNormal;
			Vector2 textboxSize = new Vector2(size.X - buttonWidth, size.Y);
			if (bg != null)
			{
				UI.Graphics.DrawNPatch(bg, pos, textboxSize, Color);
			}
			else
			{
				UI.Graphics.DrawRectangle(pos, textboxSize, new FishColor(255, 255, 255, 255));
				UI.Graphics.DrawRectangleOutline(pos, textboxSize, new FishColor(128, 128, 128, 255));
			}

			// Draw date text
			var font = UI.Settings.FontDefault;
			if (font != null)
			{
				string dateText = _value.ToString(DateFormat);
				float textY = pos.Y + (size.Y - UI.Graphics.MeasureText(font, dateText).Y) / 2;
				UI.Graphics.BeginScissor(pos + new Vector2(4, 0), new Vector2(textboxSize.X - 8, size.Y));
				UI.Graphics.DrawTextColor(font, dateText, new Vector2(pos.X + 4, textY), new FishColor(0, 0, 0, 255));
				UI.Graphics.EndScissor();
			}

			// Draw dropdown button
			Vector2 buttonPos = new Vector2(pos.X + size.X - buttonWidth, pos.Y);
			NPatch buttonBg = _dropdownButtonHovered
				? UI.Settings.ImgButtonHover
				: UI.Settings.ImgButtonNormal;
			if (buttonBg != null)
			{
				UI.Graphics.DrawNPatch(buttonBg, buttonPos, new Vector2(buttonWidth, size.Y), Color);
			}
			else
			{
				FishColor btnColor = _dropdownButtonHovered
					? new FishColor(200, 200, 200, 255)
					: new FishColor(230, 230, 230, 255);
				UI.Graphics.DrawRectangle(buttonPos, new Vector2(buttonWidth, size.Y), btnColor);
			}

			// Draw dropdown arrow
			if (font != null)
			{
				string arrow = _isOpen ? "▲" : "▼";
				var arrowSize = UI.Graphics.MeasureText(font, arrow);
				float arrowX = buttonPos.X + (buttonWidth - arrowSize.X) / 2;
				float arrowY = buttonPos.Y + (size.Y - arrowSize.Y) / 2;
				UI.Graphics.DrawTextColor(font, arrow, new Vector2(arrowX, arrowY), new FishColor(60, 60, 60, 255));
			}

			// Draw calendar popup if open
			if (_isOpen)
			{
				DrawCalendarPopup(UI, pos, size, font);
			}
		}

		private void DrawCalendarPopup(FishUI UI, Vector2 controlPos, Vector2 controlSize, FontRef font)
		{
			Vector2 calSize = Scale(CalendarSize);
			Vector2 calPos = new Vector2(controlPos.X, controlPos.Y + controlSize.Y + 2);

			// Draw shadow
			UI.Graphics.DrawRectangle(calPos + new Vector2(3, 3), calSize, new FishColor(0, 0, 0, 80));

			// Draw background
			NPatch menuBg = UI.Settings.ImgMenuBackground;
			if (menuBg != null)
			{
				UI.Graphics.DrawNPatch(menuBg, calPos, calSize, Color);
			}
			else
			{
				UI.Graphics.DrawRectangle(calPos, calSize, CalendarBackgroundColor);
				UI.Graphics.DrawRectangleOutline(calPos, calSize, new FishColor(128, 128, 128, 255));
			}

			if (font == null) return;

			float headerH = Scale(HeaderHeight);
			float dowH = Scale(DayOfWeekHeight);
			float padding = Scale(4);

			// Draw header with month/year and navigation
			DrawCalendarHeader(UI, calPos, calSize.X, headerH, font);

			// Draw day-of-week labels
			float dayAreaTop = calPos.Y + headerH;
			DrawDayOfWeekLabels(UI, calPos.X + padding, dayAreaTop, calSize.X - padding * 2, dowH, font);

			// Draw day grid
			float gridTop = dayAreaTop + dowH;
			float gridHeight = calSize.Y - headerH - dowH - padding;
			DrawDayGrid(UI, calPos.X + padding, gridTop, calSize.X - padding * 2, gridHeight, font);
		}

		private void DrawCalendarHeader(FishUI UI, Vector2 calPos, float calWidth, float headerH, FontRef font)
		{
			float btnWidth = Scale(24);
			float padding = Scale(4);

			// Previous year button
			Vector2 prevYearPos = new Vector2(calPos.X + padding, calPos.Y + padding);
			Vector2 btnSize = new Vector2(btnWidth, headerH - padding * 2);
			DrawNavButton(UI, prevYearPos, btnSize, "«", _prevYearHovered, font);

			// Previous month button
			Vector2 prevMonthPos = new Vector2(prevYearPos.X + btnWidth + 2, prevYearPos.Y);
			DrawNavButton(UI, prevMonthPos, btnSize, "‹", _prevMonthHovered, font);

			// Next year button
			Vector2 nextYearPos = new Vector2(calPos.X + calWidth - padding - btnWidth, calPos.Y + padding);
			DrawNavButton(UI, nextYearPos, btnSize, "»", _nextYearHovered, font);

			// Next month button
			Vector2 nextMonthPos = new Vector2(nextYearPos.X - btnWidth - 2, nextYearPos.Y);
			DrawNavButton(UI, nextMonthPos, btnSize, "›", _nextMonthHovered, font);

			// Month/Year text centered
			string monthYear = _displayMonth.ToString("MMMM yyyy");
			var textSize = UI.Graphics.MeasureText(font, monthYear);
			float textX = calPos.X + (calWidth - textSize.X) / 2;
			float textY = calPos.Y + (headerH - textSize.Y) / 2;
			UI.Graphics.DrawTextColor(font, monthYear, new Vector2(textX, textY), new FishColor(0, 0, 0, 255));
		}

		private void DrawNavButton(FishUI UI, Vector2 pos, Vector2 size, string text, bool hovered, FontRef font)
		{
			FishColor bgColor = hovered
				? new FishColor(200, 200, 200, 255)
				: new FishColor(240, 240, 240, 255);
			UI.Graphics.DrawRectangle(pos, size, bgColor);
			UI.Graphics.DrawRectangleOutline(pos, size, new FishColor(180, 180, 180, 255));

			var textSize = UI.Graphics.MeasureText(font, text);
			float textX = pos.X + (size.X - textSize.X) / 2;
			float textY = pos.Y + (size.Y - textSize.Y) / 2;
			UI.Graphics.DrawTextColor(font, text, new Vector2(textX, textY), new FishColor(60, 60, 60, 255));
		}

		private void DrawDayOfWeekLabels(FishUI UI, float x, float y, float width, float height, FontRef font)
		{
			float cellWidth = width / DaysInWeek;
			string[] dayNames = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };

			for (int i = 0; i < DaysInWeek; i++)
			{
				var textSize = UI.Graphics.MeasureText(font, dayNames[i]);
				float textX = x + i * cellWidth + (cellWidth - textSize.X) / 2;
				float textY = y + (height - textSize.Y) / 2;

				FishColor color = (i == 0 || i == 6) ? WeekendColor : new FishColor(100, 100, 100, 255);
				UI.Graphics.DrawTextColor(font, dayNames[i], new Vector2(textX, textY), color);
			}
		}

		private void DrawDayGrid(FishUI UI, float x, float y, float width, float height, FontRef font)
		{
			float cellWidth = width / DaysInWeek;
			float cellHeight = height / MaxWeeksDisplayed;

			// Get first day of month and days in month
			int daysInMonth = DateTime.DaysInMonth(_displayMonth.Year, _displayMonth.Month);
			int firstDayOfWeek = (int)_displayMonth.DayOfWeek;

			// Get previous month days to show
			DateTime prevMonth = _displayMonth.AddMonths(-1);
			int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

			int dayIndex = 0;
			for (int week = 0; week < MaxWeeksDisplayed; week++)
			{
				for (int dow = 0; dow < DaysInWeek; dow++)
				{
					float cellX = x + dow * cellWidth;
					float cellY = y + week * cellHeight;

					int displayDay;
					bool isCurrentMonth;
					DateTime cellDate;

					if (dayIndex < firstDayOfWeek)
					{
						// Previous month
						displayDay = daysInPrevMonth - firstDayOfWeek + dayIndex + 1;
						isCurrentMonth = false;
						cellDate = new DateTime(prevMonth.Year, prevMonth.Month, displayDay);
					}
					else if (dayIndex < firstDayOfWeek + daysInMonth)
					{
						// Current month
						displayDay = dayIndex - firstDayOfWeek + 1;
						isCurrentMonth = true;
						cellDate = new DateTime(_displayMonth.Year, _displayMonth.Month, displayDay);
					}
					else
					{
						// Next month
						displayDay = dayIndex - firstDayOfWeek - daysInMonth + 1;
						isCurrentMonth = false;
						DateTime nextMonth = _displayMonth.AddMonths(1);
						cellDate = new DateTime(nextMonth.Year, nextMonth.Month, displayDay);
					}

					// Draw cell background
					bool isSelected = cellDate.Date == _value.Date;
					bool isToday = cellDate.Date == DateTime.Today;
					bool isHovered = dayIndex == _hoveredDay;
					bool isWeekend = dow == 0 || dow == 6;

					if (isSelected)
					{
						UI.Graphics.DrawRectangle(new Vector2(cellX, cellY), new Vector2(cellWidth, cellHeight), SelectedDayColor);
					}
					else if (isHovered && isCurrentMonth)
					{
						UI.Graphics.DrawRectangle(new Vector2(cellX, cellY), new Vector2(cellWidth, cellHeight), HoveredDayColor);
					}
					else if (isToday)
					{
						UI.Graphics.DrawRectangle(new Vector2(cellX, cellY), new Vector2(cellWidth, cellHeight), TodayColor);
					}

					// Draw day number
					string dayText = displayDay.ToString();
					var textSize = UI.Graphics.MeasureText(font, dayText);
					float textX = cellX + (cellWidth - textSize.X) / 2;
					float textY = cellY + (cellHeight - textSize.Y) / 2;

					FishColor textColor;
					if (isSelected)
						textColor = new FishColor(255, 255, 255, 255);
					else if (!isCurrentMonth)
						textColor = OutsideMonthColor;
					else if (isWeekend)
						textColor = WeekendColor;
					else
						textColor = new FishColor(0, 0, 0, 255);

					UI.Graphics.DrawTextColor(font, dayText, new Vector2(textX, textY), textColor);

					dayIndex++;
				}
			}
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);

			Vector2 controlPos = GetAbsolutePosition();
			Vector2 size = ScaledSize;
			float buttonWidth = Scale(DropdownButtonWidth);

			// Check dropdown button hover
			Vector2 buttonPos = new Vector2(controlPos.X + size.X - buttonWidth, controlPos.Y);
			_dropdownButtonHovered = IsPointInRect(Pos, buttonPos, new Vector2(buttonWidth, size.Y));

			if (_isOpen)
			{
				UpdateCalendarHover(UI, Pos, controlPos, size);
			}
		}

		private void UpdateCalendarHover(FishUI UI, Vector2 mousePos, Vector2 controlPos, Vector2 controlSize)
		{
			Vector2 calSize = Scale(CalendarSize);
			Vector2 calPos = new Vector2(controlPos.X, controlPos.Y + controlSize.Y + 2);

			float headerH = Scale(HeaderHeight);
			float dowH = Scale(DayOfWeekHeight);
			float padding = Scale(4);
			float btnWidth = Scale(24);

			// Check navigation button hovers
			Vector2 btnSize = new Vector2(btnWidth, headerH - padding * 2);

			Vector2 prevYearPos = new Vector2(calPos.X + padding, calPos.Y + padding);
			_prevYearHovered = IsPointInRect(mousePos, prevYearPos, btnSize);

			Vector2 prevMonthPos = new Vector2(prevYearPos.X + btnWidth + 2, prevYearPos.Y);
			_prevMonthHovered = IsPointInRect(mousePos, prevMonthPos, btnSize);

			Vector2 nextYearPos = new Vector2(calPos.X + calSize.X - padding - btnWidth, calPos.Y + padding);
			_nextYearHovered = IsPointInRect(mousePos, nextYearPos, btnSize);

			Vector2 nextMonthPos = new Vector2(nextYearPos.X - btnWidth - 2, nextYearPos.Y);
			_nextMonthHovered = IsPointInRect(mousePos, nextMonthPos, btnSize);

			// Check day cell hover
			float gridTop = calPos.Y + headerH + dowH;
			float gridHeight = calSize.Y - headerH - dowH - padding;
			float gridWidth = calSize.X - padding * 2;
			float cellWidth = gridWidth / DaysInWeek;
			float cellHeight = gridHeight / MaxWeeksDisplayed;

			float gridX = calPos.X + padding;
			_hoveredDay = -1;

			if (mousePos.X >= gridX && mousePos.X < gridX + gridWidth &&
				mousePos.Y >= gridTop && mousePos.Y < gridTop + gridHeight)
			{
				int col = (int)((mousePos.X - gridX) / cellWidth);
				int row = (int)((mousePos.Y - gridTop) / cellHeight);
				_hoveredDay = row * DaysInWeek + col;
			}
		}

		public override void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMousePress(UI, InState, Btn, Pos);

			if (Btn != FishMouseButton.Left)
				return;

			Vector2 controlPos = GetAbsolutePosition();
			Vector2 size = ScaledSize;
			float buttonWidth = Scale(DropdownButtonWidth);

			// Check dropdown button click
			Vector2 buttonPos = new Vector2(controlPos.X + size.X - buttonWidth, controlPos.Y);
			if (IsPointInRect(Pos, buttonPos, new Vector2(buttonWidth, size.Y)))
			{
				Toggle();
				return;
			}

			// Check textbox click (also opens calendar)
			if (IsPointInRect(Pos, controlPos, new Vector2(size.X - buttonWidth, size.Y)))
			{
				if (!_isOpen)
					Open();
				return;
			}

			if (_isOpen)
			{
				HandleCalendarClick(UI, Pos, controlPos, size);
			}
		}

		private void HandleCalendarClick(FishUI UI, Vector2 mousePos, Vector2 controlPos, Vector2 controlSize)
		{
			Vector2 calSize = Scale(CalendarSize);
			Vector2 calPos = new Vector2(controlPos.X, controlPos.Y + controlSize.Y + 2);

			// Check if click is outside calendar
			if (!IsPointInRect(mousePos, calPos, calSize))
			{
				Close();
				return;
			}

			float headerH = Scale(HeaderHeight);
			float dowH = Scale(DayOfWeekHeight);
			float padding = Scale(4);
			float btnWidth = Scale(24);
			Vector2 btnSize = new Vector2(btnWidth, headerH - padding * 2);

			// Check navigation buttons
			Vector2 prevYearPos = new Vector2(calPos.X + padding, calPos.Y + padding);
			if (IsPointInRect(mousePos, prevYearPos, btnSize))
			{
				_displayMonth = _displayMonth.AddYears(-1);
				return;
			}

			Vector2 prevMonthPos = new Vector2(prevYearPos.X + btnWidth + 2, prevYearPos.Y);
			if (IsPointInRect(mousePos, prevMonthPos, btnSize))
			{
				_displayMonth = _displayMonth.AddMonths(-1);
				return;
			}

			Vector2 nextYearPos = new Vector2(calPos.X + calSize.X - padding - btnWidth, calPos.Y + padding);
			if (IsPointInRect(mousePos, nextYearPos, btnSize))
			{
				_displayMonth = _displayMonth.AddYears(1);
				return;
			}

			Vector2 nextMonthPos = new Vector2(nextYearPos.X - btnWidth - 2, nextYearPos.Y);
			if (IsPointInRect(mousePos, nextMonthPos, btnSize))
			{
				_displayMonth = _displayMonth.AddMonths(1);
				return;
			}

			// Check day selection
			if (_hoveredDay >= 0)
			{
				DateTime? selectedDate = GetDateFromDayIndex(_hoveredDay);
				if (selectedDate.HasValue && selectedDate.Value >= MinDate && selectedDate.Value <= MaxDate)
				{
					Value = selectedDate.Value;
					Close();
				}
			}
		}

		private DateTime? GetDateFromDayIndex(int dayIndex)
		{
			int daysInMonth = DateTime.DaysInMonth(_displayMonth.Year, _displayMonth.Month);
			int firstDayOfWeek = (int)_displayMonth.DayOfWeek;

			if (dayIndex < firstDayOfWeek)
			{
				// Previous month
				DateTime prevMonth = _displayMonth.AddMonths(-1);
				int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
				int day = daysInPrevMonth - firstDayOfWeek + dayIndex + 1;
				return new DateTime(prevMonth.Year, prevMonth.Month, day);
			}
			else if (dayIndex < firstDayOfWeek + daysInMonth)
			{
				// Current month
				int day = dayIndex - firstDayOfWeek + 1;
				return new DateTime(_displayMonth.Year, _displayMonth.Month, day);
			}
			else
			{
				// Next month
				DateTime nextMonth = _displayMonth.AddMonths(1);
				int day = dayIndex - firstDayOfWeek - daysInMonth + 1;
				if (day > DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month))
					return null;
				return new DateTime(nextMonth.Year, nextMonth.Month, day);
			}
		}

		private bool IsPointInRect(Vector2 point, Vector2 rectPos, Vector2 rectSize)
		{
			return point.X >= rectPos.X && point.X < rectPos.X + rectSize.X &&
				   point.Y >= rectPos.Y && point.Y < rectPos.Y + rectSize.Y;
		}
	}
}
