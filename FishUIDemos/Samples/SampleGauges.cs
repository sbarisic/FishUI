using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates gauge controls: RadialGauge, BarGauge, and VUMeter.
	/// Showcases various configurations and interactive controls.
	/// </summary>
	public class SampleGauges : ISample
	{
		FishUI.FishUI FUI;

		public string Name => "Gauges";

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
			Label titleLabel = new Label("Gauge Controls Demo");
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

			// === RadialGauge Section ===
			Label radialSectionLabel = new Label("RadialGauge - Car Dashboard Style");
			radialSectionLabel.Position = new Vector2(20, 60);
			radialSectionLabel.Size = new Vector2(600, 24);
			radialSectionLabel.Alignment = Align.Left;
			FUI.AddControl(radialSectionLabel);

			// RPM gauge - large car dashboard style
			Label rpmLabel = new Label("RPM x1000");
			rpmLabel.Position = new Vector2(20, 85);
			rpmLabel.Size = new Vector2(280, 20);
			rpmLabel.Alignment = Align.Center;
			FUI.AddControl(rpmLabel);

			RadialGauge rpmGauge = new RadialGauge(0, 8000);
			rpmGauge.Position = new Vector2(20, 110);
			rpmGauge.Size = new Vector2(280, 280);
			rpmGauge.Value = 3500;
			rpmGauge.SetupRPMZones();
			rpmGauge.UnitSuffix = "RPM";
			rpmGauge.ValueFormat = "F0";
			rpmGauge.TooltipText = "RPM gauge with redline zone";
			FUI.AddControl(rpmGauge);

			// RPM gauge controls
			Button rpmDown = new Button();
			rpmDown.Text = "-";
			rpmDown.Position = new Vector2(110, 395);
			rpmDown.Size = new Vector2(40, 30);
			rpmDown.IsRepeatButton = true;
			rpmDown.RepeatInterval = 0.03f;
			rpmDown.TooltipText = "Decrease RPM";
			rpmDown.OnButtonPressed += (btn, mbtn, pos) => { rpmGauge.Value -= 100; };
			FUI.AddControl(rpmDown);

			Button rpmUp = new Button();
			rpmUp.Text = "+";
			rpmUp.Position = new Vector2(155, 395);
			rpmUp.Size = new Vector2(40, 30);
			rpmUp.IsRepeatButton = true;
			rpmUp.RepeatInterval = 0.03f;
			rpmUp.TooltipText = "Increase RPM";
			rpmUp.OnButtonPressed += (btn, mbtn, pos) => { rpmGauge.Value += 100; };
			FUI.AddControl(rpmUp);

			// Speedometer gauge - large car dashboard style
			Label speedLabel = new Label("km/h");
			speedLabel.Position = new Vector2(320, 85);
			speedLabel.Size = new Vector2(280, 20);
			speedLabel.Alignment = Align.Center;
			FUI.AddControl(speedLabel);

			RadialGauge speedGauge = new RadialGauge(0, 200);
			speedGauge.Position = new Vector2(320, 110);
			speedGauge.Size = new Vector2(280, 280);
			speedGauge.Value = 85;
			speedGauge.SetupSpeedZones();
			speedGauge.UnitSuffix = "km/h";
			speedGauge.MajorTickCount = 8;
			speedGauge.TooltipText = "Speedometer with color zones";
			FUI.AddControl(speedGauge);

			// Speed gauge controls
			Button speedDown = new Button();
			speedDown.Text = "-";
			speedDown.Position = new Vector2(410, 395);
			speedDown.Size = new Vector2(40, 30);
			speedDown.IsRepeatButton = true;
			speedDown.RepeatInterval = 0.03f;
			speedDown.TooltipText = "Decrease speed";
			speedDown.OnButtonPressed += (btn, mbtn, pos) => { speedGauge.Value -= 5; };
			FUI.AddControl(speedDown);

			Button speedUp = new Button();
			speedUp.Text = "+";
			speedUp.Position = new Vector2(455, 395);
			speedUp.Size = new Vector2(40, 30);
			speedUp.IsRepeatButton = true;
			speedUp.RepeatInterval = 0.03f;
			speedUp.TooltipText = "Increase speed";
			speedUp.OnButtonPressed += (btn, mbtn, pos) => { speedGauge.Value += 5; };
			FUI.AddControl(speedUp);

			// === BarGauge Section ===
			Label barSectionLabel = new Label("BarGauge");
			barSectionLabel.Position = new Vector2(620, 60);
			barSectionLabel.Size = new Vector2(200, 24);
			barSectionLabel.Alignment = Align.Left;
			FUI.AddControl(barSectionLabel);

			// Temperature gauge (green-yellow-red)
			Label tempLabel = new Label("Temperature");
			tempLabel.Position = new Vector2(640, 85);
			tempLabel.Size = new Vector2(150, 16);
			tempLabel.Alignment = Align.Left;
			FUI.AddControl(tempLabel);

			BarGauge tempGauge = new BarGauge(0, 100);
			tempGauge.Position = new Vector2(640, 105);
			tempGauge.Size = new Vector2(150, 28);
			tempGauge.Value = 72;
			tempGauge.SetupTemperatureZones();
			tempGauge.ShowValue = true;
			tempGauge.UnitSuffix = "°C";
			tempGauge.ShowRangeLabels = true;
			tempGauge.MinLabel = "C";
			tempGauge.MaxLabel = "H";
			tempGauge.TooltipText = "Temperature gauge with C/H range labels";
			FUI.AddControl(tempGauge);

			// Temperature gauge controls
			Button tempDown = new Button();
			tempDown.Text = "-";
			tempDown.Position = new Vector2(810, 105);
			tempDown.Size = new Vector2(25, 28);
			tempDown.IsRepeatButton = true;
			tempDown.RepeatInterval = 0.05f;
			tempDown.TooltipText = "Decrease temperature";
			tempDown.OnButtonPressed += (btn, mbtn, pos) => { tempGauge.Value -= 2; };
			FUI.AddControl(tempDown);

			Button tempUp = new Button();
			tempUp.Text = "+";
			tempUp.Position = new Vector2(840, 105);
			tempUp.Size = new Vector2(25, 28);
			tempUp.IsRepeatButton = true;
			tempUp.RepeatInterval = 0.05f;
			tempUp.TooltipText = "Increase temperature";
			tempUp.OnButtonPressed += (btn, mbtn, pos) => { tempGauge.Value += 2; };
			FUI.AddControl(tempUp);

			// Simple gauge with ticks
			Label tickLabel = new Label("With Ticks");
			tickLabel.Position = new Vector2(620, 145);
			tickLabel.Size = new Vector2(150, 16);
			tickLabel.Alignment = Align.Left;
			FUI.AddControl(tickLabel);

			BarGauge tickGauge = new BarGauge(0, 50);
			tickGauge.Position = new Vector2(640, 165);
			tickGauge.Size = new Vector2(150, 20);
			tickGauge.Value = 32;
			tickGauge.ShowTicks = true;
			tickGauge.TickCount = 5;
			tickGauge.FillColor = new FishColor(100, 150, 255, 255);
			tickGauge.TooltipText = "Gauge with tick marks";
			FUI.AddControl(tickGauge);

			// Tick gauge controls
			Button tickDown = new Button();
			tickDown.Text = "-";
			tickDown.Position = new Vector2(795, 165);
			tickDown.Size = new Vector2(25, 20);
			tickDown.IsRepeatButton = true;
			tickDown.RepeatInterval = 0.05f;
			tickDown.TooltipText = "Decrease value";
			tickDown.OnButtonPressed += (btn, mbtn, pos) => { tickGauge.Value -= 1; };
			FUI.AddControl(tickDown);

			Button tickUp = new Button();
			tickUp.Text = "+";
			tickUp.Position = new Vector2(825, 165);
			tickUp.Size = new Vector2(25, 20);
			tickUp.IsRepeatButton = true;
			tickUp.RepeatInterval = 0.05f;
			tickUp.TooltipText = "Increase value";
			tickUp.OnButtonPressed += (btn, mbtn, pos) => { tickGauge.Value += 1; };
			FUI.AddControl(tickUp);


			// Fuel gauge (red-yellow-green, vertical)
			Label fuelLabel = new Label("Fuel (Vertical)");
			fuelLabel.Position = new Vector2(880, 85);
			fuelLabel.Size = new Vector2(80, 16);
			fuelLabel.Alignment = Align.Center;
			FUI.AddControl(fuelLabel);

			BarGauge fuelGauge = new BarGauge(0, 100);
			fuelGauge.Position = new Vector2(900, 125);
			fuelGauge.Size = new Vector2(25, 80);
			fuelGauge.Orientation = BarGaugeOrientation.Vertical;
			fuelGauge.Value = 35;
			fuelGauge.SetupFuelZones();
			fuelGauge.ShowRangeLabels = true;
			fuelGauge.MinLabel = "E";
			fuelGauge.MaxLabel = "F";
			fuelGauge.TooltipText = "Fuel gauge with E/F range labels";
			FUI.AddControl(fuelGauge);

			// Fuel gauge controls
			Button fuelDown = new Button();
			fuelDown.Text = "-";
			fuelDown.Position = new Vector2(930, 165);
			fuelDown.Size = new Vector2(25, 25);
			fuelDown.IsRepeatButton = true;
			fuelDown.RepeatInterval = 0.05f;
			fuelDown.TooltipText = "Decrease fuel";
			fuelDown.OnButtonPressed += (btn, mbtn, pos) => { fuelGauge.Value -= 2; };
			FUI.AddControl(fuelDown);

			Button fuelUp = new Button();
			fuelUp.Text = "+";
			fuelUp.Position = new Vector2(930, 135);
			fuelUp.Size = new Vector2(25, 25);
			fuelUp.IsRepeatButton = true;
			fuelUp.RepeatInterval = 0.05f;
			fuelUp.TooltipText = "Increase fuel";
			fuelUp.OnButtonPressed += (btn, mbtn, pos) => { fuelGauge.Value += 2; };
			FUI.AddControl(fuelUp);

			// === VUMeter Section ===
			Label vuSectionLabel = new Label("VUMeter");
			vuSectionLabel.Position = new Vector2(620, 200);
			vuSectionLabel.Size = new Vector2(200, 24);
			vuSectionLabel.Alignment = Align.Left;
			FUI.AddControl(vuSectionLabel);

			// Continuous VU meter
			Label vuContinuousLabel = new Label("Continuous");
			vuContinuousLabel.Position = new Vector2(620, 225);
			vuContinuousLabel.Size = new Vector2(60, 16);
			vuContinuousLabel.Alignment = Align.Center;
			FUI.AddControl(vuContinuousLabel);

			VUMeter vuMeter1 = new VUMeter();
			vuMeter1.Position = new Vector2(635, 245);
			vuMeter1.Size = new Vector2(25, 100);
			vuMeter1.Orientation = VUMeterOrientation.Vertical;
			vuMeter1.TooltipText = "Continuous VU meter with peak hold";
			FUI.AddControl(vuMeter1);

			// Segmented VU meter
			Label vuSegmentedLabel = new Label("Segmented");
			vuSegmentedLabel.Position = new Vector2(680, 225);
			vuSegmentedLabel.Size = new Vector2(60, 16);
			vuSegmentedLabel.Alignment = Align.Center;
			FUI.AddControl(vuSegmentedLabel);

			VUMeter vuMeter2 = new VUMeter();
			vuMeter2.Position = new Vector2(695, 245);
			vuMeter2.Size = new Vector2(25, 100);
			vuMeter2.Orientation = VUMeterOrientation.Vertical;
			vuMeter2.SegmentCount = 10;
			vuMeter2.TooltipText = "Segmented VU meter (LED style)";
			FUI.AddControl(vuMeter2);

			// Slider to control VU meters
			Label vuSliderLabel = new Label("Level");
			vuSliderLabel.Position = new Vector2(740, 225);
			vuSliderLabel.Size = new Vector2(30, 16);
			vuSliderLabel.Alignment = Align.Center;
			FUI.AddControl(vuSliderLabel);

			Slider vuSlider = new Slider();
			vuSlider.Position = new Vector2(745, 245);
			vuSlider.Size = new Vector2(24, 100);
			vuSlider.Orientation = SliderOrientation.Vertical;
			vuSlider.MinValue = 0;
			vuSlider.MaxValue = 1;
			vuSlider.Value = 0.5f;
			vuSlider.Step = 0.01f;
			vuSlider.TooltipText = "Adjust VU level";
			vuSlider.OnValueChanged += (slider, val) =>
			{
				vuMeter1.SetLevel(val);
				vuMeter2.SetLevel(val);
			};
			FUI.AddControl(vuSlider);

			// Initialize VU meters
			vuMeter1.SetLevel(0.5f);
			vuMeter2.SetLevel(0.5f);

			// Horizontal VU meters
			Label vuHorizontalLabel = new Label("Horizontal VU Meters");
			vuHorizontalLabel.Position = new Vector2(800, 225);
			vuHorizontalLabel.Size = new Vector2(200, 16);
			vuHorizontalLabel.Alignment = Align.Left;
			FUI.AddControl(vuHorizontalLabel);

			VUMeter vuHoriz1 = new VUMeter();
			vuHoriz1.Position = new Vector2(800, 250);
			vuHoriz1.Size = new Vector2(150, 20);
			vuHoriz1.Orientation = VUMeterOrientation.Horizontal;
			vuHoriz1.TooltipText = "Horizontal continuous VU meter";
			FUI.AddControl(vuHoriz1);

			VUMeter vuHoriz2 = new VUMeter();
			vuHoriz2.Position = new Vector2(800, 275);
			vuHoriz2.Size = new Vector2(150, 20);
			vuHoriz2.Orientation = VUMeterOrientation.Horizontal;
			vuHoriz2.SegmentCount = 15;
			vuHoriz2.TooltipText = "Horizontal segmented VU meter";
			FUI.AddControl(vuHoriz2);

			// Horizontal slider
			Slider vuHorizSlider = new Slider();
			vuHorizSlider.Position = new Vector2(800, 305);
			vuHorizSlider.Size = new Vector2(150, 20);
			vuHorizSlider.MinValue = 0;
			vuHorizSlider.MaxValue = 1;
			vuHorizSlider.Value = 0.6f;
			vuHorizSlider.Step = 0.01f;
			vuHorizSlider.ShowValueLabel = true;
			vuHorizSlider.TooltipText = "Adjust horizontal VU level";
			vuHorizSlider.OnValueChanged += (slider, val) =>
			{
				vuHoriz1.SetLevel(val);
				vuHoriz2.SetLevel(val);
			};
			FUI.AddControl(vuHorizSlider);

			// Initialize horizontal VU meters
			vuHoriz1.SetLevel(0.6f);
			vuHoriz2.SetLevel(0.6f);


			// === Car Dashboard Preview === (removed - main gauges are now large enough)
			// The main RPM and Speed gauges above now serve as the dashboard demo

			// Throttle simulator for the large gauges
			Label throttleLabel = new Label("Throttle Simulator");
			throttleLabel.Position = new Vector2(20, 440);
			throttleLabel.Size = new Vector2(200, 20);
			throttleLabel.Alignment = Align.Left;
			FUI.AddControl(throttleLabel);

			Slider throttleSlider = new Slider();
			throttleSlider.Position = new Vector2(20, 465);
			throttleSlider.Size = new Vector2(580, 24);
			throttleSlider.MinValue = 0;
			throttleSlider.MaxValue = 1;
			throttleSlider.Value = 0.4f;
			throttleSlider.Step = 0.01f;
			throttleSlider.ShowValueLabel = true;
			throttleSlider.TooltipText = "Simulate throttle position - controls RPM and Speed gauges";
			throttleSlider.OnValueChanged += (slider, val) =>
			{
				rpmGauge.Value = 800 + (val * 7000);
				speedGauge.Value = val * 180;
			};
			FUI.AddControl(throttleSlider);

			// Initialize gauges to match throttle
			rpmGauge.Value = 800 + (0.4f * 7000);
			speedGauge.Value = 0.4f * 180;
		}
	}
}

