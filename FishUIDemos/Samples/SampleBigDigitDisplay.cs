using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the BigDigitDisplay control with various configurations
	/// for speedometer, RPM gauge, and counter-style readouts.
	/// </summary>
	public class SampleBigDigitDisplay : ISample
	{
		FishUI.FishUI FUI;
		BigDigitDisplay _speedDisplay;
		BigDigitDisplay _rpmDisplay;
		BigDigitDisplay _tempDisplay;
		BigDigitDisplay _counterDisplay;
		float _speed = 0f;
		float _rpm = 0f;
		float _temp = 20f;
		int _counter = 0;
		float _animTime = 0f;

		public string Name => "BigDigitDisplay";

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
			Label titleLabel = new Label("BigDigitDisplay Demo");
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
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(Name);
			FUI.AddControl(screenshotBtn);

			// ============ ROW 1: Speedometer Style ============

			Label speedLabel = new Label("Speedometer (Green)");
			speedLabel.Position = new Vector2(20, 60);
			speedLabel.Size = new Vector2(200, 20);
			speedLabel.Alignment = Align.Left;
			FUI.AddControl(speedLabel);

			_speedDisplay = new BigDigitDisplay();
			_speedDisplay.Position = new Vector2(20, 85);
			_speedDisplay.Size = new Vector2(200, 80);
			_speedDisplay.Text = "0";
			_speedDisplay.UnitLabel = "km/h";
			_speedDisplay.TextColor = new FishColor(0, 255, 0, 255);
			_speedDisplay.Alignment = DigitAlignment.Right;
			_speedDisplay.TooltipText = "Digital speedometer display";
			FUI.AddControl(_speedDisplay);

			// Speed slider
			Slider speedSlider = new Slider();
			speedSlider.Position = new Vector2(20, 175);
			speedSlider.Size = new Vector2(200, 24);
			speedSlider.MinValue = 0;
			speedSlider.MaxValue = 280;
			speedSlider.Value = 0;
			speedSlider.Step = 1;
			speedSlider.ShowValueLabel = true;
			speedSlider.OnValueChanged += (s, v) =>
			{
				_speed = v;
				_speedDisplay.Value = v;
			};
			FUI.AddControl(speedSlider);

			// ============ ROW 1: RPM Style ============

			Label rpmLabel = new Label("RPM Gauge (Red)");
			rpmLabel.Position = new Vector2(240, 60);
			rpmLabel.Size = new Vector2(200, 20);
			rpmLabel.Alignment = Align.Left;
			FUI.AddControl(rpmLabel);

			_rpmDisplay = new BigDigitDisplay();
			_rpmDisplay.Position = new Vector2(240, 85);
			_rpmDisplay.Size = new Vector2(200, 80);
			_rpmDisplay.Text = "0";
			_rpmDisplay.UnitLabel = "RPM";
			_rpmDisplay.TextColor = new FishColor(255, 60, 60, 255);
			_rpmDisplay.BackgroundColor = new FishColor(30, 10, 10, 255);
			_rpmDisplay.Alignment = DigitAlignment.Right;
			_rpmDisplay.TooltipText = "Engine RPM display";
			FUI.AddControl(_rpmDisplay);

			// RPM slider
			Slider rpmSlider = new Slider();
			rpmSlider.Position = new Vector2(240, 175);
			rpmSlider.Size = new Vector2(200, 24);
			rpmSlider.MinValue = 0;
			rpmSlider.MaxValue = 8000;
			rpmSlider.Value = 0;
			rpmSlider.Step = 100;
			rpmSlider.ShowValueLabel = true;
			rpmSlider.OnValueChanged += (s, v) =>
			{
				_rpm = v;
				_rpmDisplay.Value = v;
			};
			FUI.AddControl(rpmSlider);

			// ============ ROW 1: Temperature Style ============

			Label tempLabel = new Label("Temperature (Cyan)");
			tempLabel.Position = new Vector2(460, 60);
			tempLabel.Size = new Vector2(200, 20);
			tempLabel.Alignment = Align.Left;
			FUI.AddControl(tempLabel);

			_tempDisplay = new BigDigitDisplay();
			_tempDisplay.Position = new Vector2(460, 85);
			_tempDisplay.Size = new Vector2(160, 80);
			_tempDisplay.ValueFormat = "F1";
			_tempDisplay.Value = 20.0f;
			_tempDisplay.UnitLabel = "°C";
			_tempDisplay.TextColor = new FishColor(0, 200, 255, 255);
			_tempDisplay.BackgroundColor = new FishColor(10, 20, 30, 255);
			_tempDisplay.Alignment = DigitAlignment.Right;
			_tempDisplay.TooltipText = "Temperature display with decimal";
			FUI.AddControl(_tempDisplay);

			// Temperature slider
			Slider tempSlider = new Slider();
			tempSlider.Position = new Vector2(460, 175);
			tempSlider.Size = new Vector2(160, 24);
			tempSlider.MinValue = -40;
			tempSlider.MaxValue = 120;
			tempSlider.Value = 20;
			tempSlider.Step = 0.5f;
			tempSlider.ShowValueLabel = true;
			tempSlider.OnValueChanged += (s, v) =>
			{
				_temp = v;
				_tempDisplay.Value = v;
			};
			FUI.AddControl(tempSlider);

			// ============ ROW 2: Counter/Timer Style ============

			Label counterLabel = new Label("Counter (Orange, Center Aligned)");
			counterLabel.Position = new Vector2(20, 220);
			counterLabel.Size = new Vector2(250, 20);
			counterLabel.Alignment = Align.Left;
			FUI.AddControl(counterLabel);

			_counterDisplay = new BigDigitDisplay();
			_counterDisplay.Position = new Vector2(20, 245);
			_counterDisplay.Size = new Vector2(180, 70);
			_counterDisplay.Text = "00000";
			_counterDisplay.TextColor = new FishColor(255, 180, 0, 255);
			_counterDisplay.BackgroundColor = new FishColor(30, 20, 0, 255);
			_counterDisplay.Alignment = DigitAlignment.Center;
			_counterDisplay.BorderThickness = 3;
			_counterDisplay.TooltipText = "Counter display (center aligned)";
			FUI.AddControl(_counterDisplay);

			// Counter buttons
			Button incBtn = new Button();
			incBtn.Text = "+1";
			incBtn.Position = new Vector2(20, 325);
			incBtn.Size = new Vector2(50, 28);
			incBtn.OnButtonPressed += (b, m, p) =>
			{
				_counter++;
				_counterDisplay.Text = _counter.ToString("D5");
			};
			FUI.AddControl(incBtn);

			Button inc10Btn = new Button();
			inc10Btn.Text = "+10";
			inc10Btn.Position = new Vector2(75, 325);
			inc10Btn.Size = new Vector2(50, 28);
			inc10Btn.OnButtonPressed += (b, m, p) =>
			{
				_counter += 10;
				_counterDisplay.Text = _counter.ToString("D5");
			};
			FUI.AddControl(inc10Btn);

			Button resetBtn = new Button();
			resetBtn.Text = "Reset";
			resetBtn.Position = new Vector2(130, 325);
			resetBtn.Size = new Vector2(70, 28);
			resetBtn.OnButtonPressed += (b, m, p) =>
			{
				_counter = 0;
				_counterDisplay.Text = "00000";
			};
			FUI.AddControl(resetBtn);

			// ============ ROW 2: No Border / Minimal Style ============

			Label minimalLabel = new Label("Minimal (No Border)");
			minimalLabel.Position = new Vector2(220, 220);
			minimalLabel.Size = new Vector2(200, 20);
			minimalLabel.Alignment = Align.Left;
			FUI.AddControl(minimalLabel);

			BigDigitDisplay minimalDisplay = new BigDigitDisplay();
			minimalDisplay.Position = new Vector2(220, 245);
			minimalDisplay.Size = new Vector2(150, 70);
			minimalDisplay.Text = "1234";
			minimalDisplay.TextColor = new FishColor(255, 255, 255, 255);
			minimalDisplay.BackgroundColor = new FishColor(40, 40, 40, 255);
			minimalDisplay.BorderThickness = 0;
			minimalDisplay.Alignment = DigitAlignment.Center;
			minimalDisplay.TooltipText = "Minimal style without border";
			FUI.AddControl(minimalDisplay);

			// ============ ROW 2: Left Aligned Style ============

			Label leftLabel = new Label("Left Aligned (Yellow)");
			leftLabel.Position = new Vector2(390, 220);
			leftLabel.Size = new Vector2(200, 20);
			leftLabel.Alignment = Align.Left;
			FUI.AddControl(leftLabel);

			BigDigitDisplay leftDisplay = new BigDigitDisplay();
			leftDisplay.Position = new Vector2(390, 245);
			leftDisplay.Size = new Vector2(180, 70);
			leftDisplay.Text = "88.8";
			leftDisplay.UnitLabel = "%";
			leftDisplay.TextColor = new FishColor(255, 255, 0, 255);
			leftDisplay.Alignment = DigitAlignment.Left;
			leftDisplay.TooltipText = "Left aligned display";
			FUI.AddControl(leftDisplay);

			// ============ Info Section ============

			Label infoLabel = new Label("BigDigitDisplay features: customizable colors, alignment, unit labels, font scaling, and border styles.");
			infoLabel.Position = new Vector2(20, 370);
			infoLabel.Size = new Vector2(600, 20);
			infoLabel.Alignment = Align.Left;
			FUI.AddControl(infoLabel);
		}

		public void Update(float dt)
		{
			// Animate displays for visual effect
			_animTime += dt;

			// Subtle fluctuation on temperature
			if (_tempDisplay != null)
			{
				float fluctuation = MathF.Sin(_animTime * 2f) * 0.2f;
				_tempDisplay.Value = _temp + fluctuation;
			}
		}
	}
}
