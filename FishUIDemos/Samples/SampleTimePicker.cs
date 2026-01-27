using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the TimePicker control with various configurations.
	/// </summary>
	public class SampleTimePicker : ISample
	{
		FishUI.FishUI FUI;
		Label _selectedTimeLabel;
		TimePicker _mainPicker;

		public string Name => "TimePicker";

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
			Label titleLabel = new Label("TimePicker Demo");
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

			// === Basic 24-hour TimePicker ===
			Label basicLabel = new Label("24-Hour Format:");
		basicLabel.Position = new Vector2(20, yPos);
			basicLabel.Size = new Vector2(120, 24);
			basicLabel.Alignment = Align.Left;
			FUI.AddControl(basicLabel);

			_mainPicker = new TimePicker(14, 30, 0);
			_mainPicker.Position = new Vector2(150, yPos);
			_mainPicker.OnValueChanged += OnTimeChanged;
			FUI.AddControl(_mainPicker);

			yPos += 35;

			// Selected time display
			_selectedTimeLabel = new Label($"Selected: {_mainPicker.GetFormattedTime()}");
			_selectedTimeLabel.Position = new Vector2(20, yPos);
			_selectedTimeLabel.Size = new Vector2(300, 20);
			_selectedTimeLabel.Alignment = Align.Left;
			FUI.AddControl(_selectedTimeLabel);

			yPos += 45;

			// === 12-hour format (AM/PM) ===
			Label ampmLabel = new Label("12-Hour (AM/PM):");
			ampmLabel.Position = new Vector2(20, yPos);
			ampmLabel.Size = new Vector2(120, 24);
			ampmLabel.Alignment = Align.Left;
			FUI.AddControl(ampmLabel);

			TimePicker ampmPicker = new TimePicker(9, 30, 0);
			ampmPicker.Position = new Vector2(150, yPos);
			ampmPicker.Use24HourFormat = false;
			ampmPicker.UpdateSize();
			FUI.AddControl(ampmPicker);

			yPos += 35;

			// === With Seconds ===
			Label secondsLabel = new Label("With Seconds:");
			secondsLabel.Position = new Vector2(20, yPos);
			secondsLabel.Size = new Vector2(120, 24);
			secondsLabel.Alignment = Align.Left;
			FUI.AddControl(secondsLabel);

			TimePicker secondsPicker = new TimePicker(12, 45, 30);
			secondsPicker.Position = new Vector2(150, yPos);
			secondsPicker.ShowSeconds = true;
			secondsPicker.UpdateSize();
			FUI.AddControl(secondsPicker);

			yPos += 35;

			// === 12-hour with Seconds ===
			Label fullLabel = new Label("12-Hour + Seconds:");
			fullLabel.Position = new Vector2(20, yPos);
			fullLabel.Size = new Vector2(130, 24);
			fullLabel.Alignment = Align.Left;
			FUI.AddControl(fullLabel);

			TimePicker fullPicker = new TimePicker(23, 59, 59);
			fullPicker.Position = new Vector2(155, yPos);
			fullPicker.Use24HourFormat = false;
			fullPicker.ShowSeconds = true;
			fullPicker.UpdateSize();
			FUI.AddControl(fullPicker);


			yPos += 50;

			// === Quick Set Buttons ===
			Label presetLabel = new Label("Quick Set:");
			presetLabel.Position = new Vector2(20, yPos);
			presetLabel.Alignment = Align.Left;
			FUI.AddControl(presetLabel);

			yPos += 25;

			Button nowBtn = new Button();
			nowBtn.Text = "Now";
			nowBtn.Position = new Vector2(20, yPos);
			nowBtn.Size = new Vector2(60, 24);
			nowBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				var now = DateTime.Now;
				_mainPicker.Value = new TimeSpan(now.Hour, now.Minute, now.Second);
			};
			FUI.AddControl(nowBtn);

			Button noonBtn = new Button();
			noonBtn.Text = "Noon";
			noonBtn.Position = new Vector2(85, yPos);
			noonBtn.Size = new Vector2(60, 24);
			noonBtn.OnButtonPressed += (btn, mbtn, pos) => _mainPicker.Value = new TimeSpan(12, 0, 0);
			FUI.AddControl(noonBtn);

			Button midnightBtn = new Button();
			midnightBtn.Text = "Midnight";
			midnightBtn.Position = new Vector2(150, yPos);
			midnightBtn.Size = new Vector2(70, 24);
			midnightBtn.OnButtonPressed += (btn, mbtn, pos) => _mainPicker.Value = TimeSpan.Zero;
			FUI.AddControl(midnightBtn);

			Button endDayBtn = new Button();
			endDayBtn.Text = "23:59";
			endDayBtn.Position = new Vector2(225, yPos);
			endDayBtn.Size = new Vector2(60, 24);
			endDayBtn.OnButtonPressed += (btn, mbtn, pos) => _mainPicker.Value = new TimeSpan(23, 59, 59);
			FUI.AddControl(endDayBtn);

			yPos += 50;

			// === Instructions ===
			Label instructionsLabel = new Label("Tip: Use mouse wheel over spinners to adjust values");
			instructionsLabel.Position = new Vector2(20, yPos);
			instructionsLabel.Size = new Vector2(350, 20);
			instructionsLabel.Alignment = Align.Left;
			FUI.AddControl(instructionsLabel);
		}

		private void OnTimeChanged(TimePicker sender, TimeSpan value)
		{
			_selectedTimeLabel.Text = $"Selected: {sender.GetFormattedTime()} (TimeSpan: {value})";
		}

		public void Update(float dt)
		{
		}
	}
}

