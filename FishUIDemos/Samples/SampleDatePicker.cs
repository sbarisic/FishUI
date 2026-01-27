using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the DatePicker control with various configurations.
	/// </summary>
	public class SampleDatePicker : ISample
	{
		FishUI.FishUI FUI;
		Label _selectedDateLabel;
		DatePicker _mainPicker;

		public string Name => "DatePicker";

		public TakeScreenshotFunc TakeScreenshot { get; set; }

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			FishUITheme theme = UISettings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);

			return FUI;
		}

		public void Init()
		{
			// === Title ===
			Label titleLabel = new Label("DatePicker Demo");
			titleLabel.Position = new Vector2(20, 20);
			titleLabel.Size = new Vector2(300, 30);
			titleLabel.Alignment = Align.Left;
			FUI.AddControl(titleLabel);

			// Screenshot button
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			Button screenshotBtn = new Button();
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.Position = new Vector2(330, 20);
			screenshotBtn.Size = new Vector2(30, 30);
			screenshotBtn.IsImageButton = true;
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(GetType().Name);
			FUI.AddControl(screenshotBtn);

			float yPos = 60;

			// === Basic DatePicker ===
			Label basicLabel = new Label("Basic Date Picker:");
			basicLabel.Position = new Vector2(20, yPos);
			basicLabel.Alignment = Align.Left;
			FUI.AddControl(basicLabel);

			_mainPicker = new DatePicker(DateTime.Today);
			_mainPicker.Position = new Vector2(150, yPos);
			_mainPicker.Size = new Vector2(160, 24);
			_mainPicker.OnValueChanged += OnDateChanged;
			FUI.AddControl(_mainPicker);

			yPos += 35;

			// Selected date display
			_selectedDateLabel = new Label($"Selected: {DateTime.Today:D}");
			_selectedDateLabel.Position = new Vector2(20, yPos);
			_selectedDateLabel.Size = new Vector2(300, 20);
			_selectedDateLabel.Alignment = Align.Left;
			FUI.AddControl(_selectedDateLabel);

			yPos += 40;

			// === Different Date Formats ===
			Label formatLabel = new Label("Date Formats:");
			formatLabel.Position = new Vector2(20, yPos);
			formatLabel.Alignment = Align.Left;
			FUI.AddControl(formatLabel);

			yPos += 25;

			// ISO format
			Label isoLabel = new Label("ISO (yyyy-MM-dd):");
			isoLabel.Position = new Vector2(20, yPos);
			isoLabel.Size = new Vector2(130, 24);
			isoLabel.Alignment = Align.Left;
			FUI.AddControl(isoLabel);

			DatePicker isoPicker = new DatePicker(DateTime.Today);
			isoPicker.Position = new Vector2(150, yPos);
			isoPicker.Size = new Vector2(120, 24);
			isoPicker.DateFormat = "yyyy-MM-dd";
			FUI.AddControl(isoPicker);

			yPos += 30;

			// US format
			Label usLabel = new Label("US (MM/dd/yyyy):");
			usLabel.Position = new Vector2(20, yPos);
			usLabel.Size = new Vector2(130, 24);
			usLabel.Alignment = Align.Left;
			FUI.AddControl(usLabel);

			DatePicker usPicker = new DatePicker(DateTime.Today);
			usPicker.Position = new Vector2(150, yPos);
			usPicker.Size = new Vector2(120, 24);
			usPicker.DateFormat = "MM/dd/yyyy";
			FUI.AddControl(usPicker);

			yPos += 30;

			// European format
			Label euLabel = new Label("EU (dd.MM.yyyy):");
			euLabel.Position = new Vector2(20, yPos);
			euLabel.Size = new Vector2(130, 24);
			euLabel.Alignment = Align.Left;
			FUI.AddControl(euLabel);

			DatePicker euPicker = new DatePicker(DateTime.Today);
			euPicker.Position = new Vector2(150, yPos);
			euPicker.Size = new Vector2(120, 24);
			euPicker.DateFormat = "dd.MM.yyyy";
			FUI.AddControl(euPicker);

			yPos += 30;

			// Long format
			Label longLabel = new Label("Long format:");
			longLabel.Position = new Vector2(20, yPos);
			longLabel.Size = new Vector2(130, 24);
			longLabel.Alignment = Align.Left;
			FUI.AddControl(longLabel);

			DatePicker longPicker = new DatePicker(DateTime.Today);
			longPicker.Position = new Vector2(150, yPos);
			longPicker.Size = new Vector2(200, 24);
			longPicker.DateFormat = "MMMM dd, yyyy";
			FUI.AddControl(longPicker);

			yPos += 50;

			// === Date Range Restriction ===
			Label rangeLabel = new Label("Date Range (next 30 days only):");
			rangeLabel.Position = new Vector2(20, yPos);
			rangeLabel.Size = new Vector2(200, 24);
			rangeLabel.Alignment = Align.Left;
			FUI.AddControl(rangeLabel);

			yPos += 25;

			DatePicker rangePicker = new DatePicker(DateTime.Today);
			rangePicker.Position = new Vector2(20, yPos);
			rangePicker.Size = new Vector2(160, 24);
			rangePicker.MinDate = DateTime.Today;
			rangePicker.MaxDate = DateTime.Today.AddDays(30);
			FUI.AddControl(rangePicker);

			yPos += 50;

			// === Preset Date Buttons ===
			Label presetLabel = new Label("Quick Set:");
			presetLabel.Position = new Vector2(20, yPos);
			presetLabel.Alignment = Align.Left;
			FUI.AddControl(presetLabel);

			yPos += 25;

			Button todayBtn = new Button();
			todayBtn.Text = "Today";
			todayBtn.Position = new Vector2(20, yPos);
			todayBtn.Size = new Vector2(70, 24);
			todayBtn.OnButtonPressed += (btn, mbtn, pos) => _mainPicker.Value = DateTime.Today;
			FUI.AddControl(todayBtn);

			Button tomorrowBtn = new Button();
			tomorrowBtn.Text = "Tomorrow";
			tomorrowBtn.Position = new Vector2(95, yPos);
			tomorrowBtn.Size = new Vector2(80, 24);
			tomorrowBtn.OnButtonPressed += (btn, mbtn, pos) => _mainPicker.Value = DateTime.Today.AddDays(1);
			FUI.AddControl(tomorrowBtn);

			Button nextWeekBtn = new Button();
			nextWeekBtn.Text = "Next Week";
			nextWeekBtn.Position = new Vector2(180, yPos);
			nextWeekBtn.Size = new Vector2(85, 24);
			nextWeekBtn.OnButtonPressed += (btn, mbtn, pos) => _mainPicker.Value = DateTime.Today.AddDays(7);
			FUI.AddControl(nextWeekBtn);

			Button nextMonthBtn = new Button();
			nextMonthBtn.Text = "Next Month";
			nextMonthBtn.Position = new Vector2(270, yPos);
			nextMonthBtn.Size = new Vector2(90, 24);
			nextMonthBtn.OnButtonPressed += (btn, mbtn, pos) => _mainPicker.Value = DateTime.Today.AddMonths(1);
			FUI.AddControl(nextMonthBtn);
		}

		private void OnDateChanged(DatePicker sender, DateTime value)
		{
			_selectedDateLabel.Text = $"Selected: {value:D}";
		}

		public void Update(float dt)
		{
		}
	}
}

