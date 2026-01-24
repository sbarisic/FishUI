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

		public Action TakeScreenshot { get; set; }

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			FishUITheme theme = UISettings.LoadTheme("data/themes/gwen.yaml", applyImmediately: true);

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
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke();
			FUI.AddControl(screenshotBtn);

			// === RadialGauge Section ===
			Label radialSectionLabel = new Label("RadialGauge");
			radialSectionLabel.Position = new Vector2(20, 60);
			radialSectionLabel.Size = new Vector2(200, 24);
			radialSectionLabel.Alignment = Align.Left;
			FUI.AddControl(radialSectionLabel);

			// RPM gauge
			Label rpmLabel = new Label("RPM");
			rpmLabel.Position = new Vector2(20, 85);
			rpmLabel.Size = new Vector2(120, 16);
			rpmLabel.Alignment = Align.Center;
			FUI.AddControl(rpmLabel);

			RadialGauge rpmGauge = new RadialGauge(0, 8000);
			rpmGauge.Position = new Vector2(20, 105);
			rpmGauge.Size = new Vector2(120, 120);
			rpmGauge.Value = 3500;
			rpmGauge.SetupRPMZones();
			rpmGauge.UnitSuffix = "RPM";
			rpmGauge.ValueFormat = "F0";
			rpmGauge.TooltipText = "RPM gauge with redline zone";
			FUI.AddControl(rpmGauge);

			// RPM gauge controls
			Button rpmDown = new Button();
			rpmDown.Text = "-";
			rpmDown.Position = new Vector2(50, 230);
			rpmDown.Size = new Vector2(30, 25);
			rpmDown.IsRepeatButton = true;
			rpmDown.RepeatInterval = 0.03f;
			rpmDown.TooltipText = "Decrease RPM";
			rpmDown.OnButtonPressed += (btn, mbtn, pos) => { rpmGauge.Value -= 100; };
			FUI.AddControl(rpmDown);

			Button rpmUp = new Button();
			rpmUp.Text = "+";
			rpmUp.Position = new Vector2(85, 230);
			rpmUp.Size = new Vector2(30, 25);
			rpmUp.IsRepeatButton = true;
			rpmUp.RepeatInterval = 0.03f;
			rpmUp.TooltipText = "Increase RPM";
			rpmUp.OnButtonPressed += (btn, mbtn, pos) => { rpmGauge.Value += 100; };
			FUI.AddControl(rpmUp);

			// Speedometer gauge
			Label speedLabel = new Label("Speed");
			speedLabel.Position = new Vector2(160, 85);
			speedLabel.Size = new Vector2(120, 16);
			speedLabel.Alignment = Align.Center;
			FUI.AddControl(speedLabel);

			RadialGauge speedGauge = new RadialGauge(0, 200);
			speedGauge.Position = new Vector2(160, 105);
			speedGauge.Size = new Vector2(120, 120);
			speedGauge.Value = 85;
			speedGauge.SetupSpeedZones();
			speedGauge.UnitSuffix = "km/h";
			speedGauge.MajorTickCount = 8;
			speedGauge.TooltipText = "Speedometer with color zones";
			FUI.AddControl(speedGauge);

			// Speed gauge controls
			Button speedDown = new Button();
			speedDown.Text = "-";
			speedDown.Position = new Vector2(190, 230);
			speedDown.Size = new Vector2(30, 25);
			speedDown.IsRepeatButton = true;
			speedDown.RepeatInterval = 0.03f;
			speedDown.TooltipText = "Decrease speed";
			speedDown.OnButtonPressed += (btn, mbtn, pos) => { speedGauge.Value -= 5; };
			FUI.AddControl(speedDown);

			Button speedUp = new Button();
			speedUp.Text = "+";
			speedUp.Position = new Vector2(225, 230);
			speedUp.Size = new Vector2(30, 25);
			speedUp.IsRepeatButton = true;
			speedUp.RepeatInterval = 0.03f;
			speedUp.TooltipText = "Increase speed";
			speedUp.OnButtonPressed += (btn, mbtn, pos) => { speedGauge.Value += 5; };
			FUI.AddControl(speedUp);

			// === BarGauge Section ===
			Label barSectionLabel = new Label("BarGauge");
			barSectionLabel.Position = new Vector2(320, 60);
			barSectionLabel.Size = new Vector2(200, 24);
			barSectionLabel.Alignment = Align.Left;
			FUI.AddControl(barSectionLabel);

			// Temperature gauge (green-yellow-red)
			Label tempLabel = new Label("Temperature");
			tempLabel.Position = new Vector2(320, 85);
			tempLabel.Size = new Vector2(150, 16);
			tempLabel.Alignment = Align.Left;
			FUI.AddControl(tempLabel);

			BarGauge tempGauge = new BarGauge(0, 100);
			tempGauge.Position = new Vector2(320, 105);
			tempGauge.Size = new Vector2(150, 25);
			tempGauge.Value = 72;
			tempGauge.SetupTemperatureZones();
			tempGauge.ShowValue = true;
			tempGauge.UnitSuffix = "°C";
			tempGauge.TooltipText = "Temperature gauge with color zones";
			FUI.AddControl(tempGauge);

			// Temperature gauge controls
			Button tempDown = new Button();
			tempDown.Text = "-";
			tempDown.Position = new Vector2(475, 105);
			tempDown.Size = new Vector2(25, 25);
			tempDown.IsRepeatButton = true;
			tempDown.RepeatInterval = 0.05f;
			tempDown.TooltipText = "Decrease temperature";
			tempDown.OnButtonPressed += (btn, mbtn, pos) => { tempGauge.Value -= 2; };
			FUI.AddControl(tempDown);

			Button tempUp = new Button();
			tempUp.Text = "+";
			tempUp.Position = new Vector2(505, 105);
			tempUp.Size = new Vector2(25, 25);
			tempUp.IsRepeatButton = true;
			tempUp.RepeatInterval = 0.05f;
			tempUp.TooltipText = "Increase temperature";
			tempUp.OnButtonPressed += (btn, mbtn, pos) => { tempGauge.Value += 2; };
			FUI.AddControl(tempUp);

			// Simple gauge with ticks
			Label tickLabel = new Label("With Ticks");
			tickLabel.Position = new Vector2(320, 140);
			tickLabel.Size = new Vector2(150, 16);
			tickLabel.Alignment = Align.Left;
			FUI.AddControl(tickLabel);

			BarGauge tickGauge = new BarGauge(0, 50);
			tickGauge.Position = new Vector2(320, 160);
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
			tickDown.Position = new Vector2(475, 160);
			tickDown.Size = new Vector2(25, 20);
			tickDown.IsRepeatButton = true;
			tickDown.RepeatInterval = 0.05f;
			tickDown.TooltipText = "Decrease value";
			tickDown.OnButtonPressed += (btn, mbtn, pos) => { tickGauge.Value -= 1; };
			FUI.AddControl(tickDown);

			Button tickUp = new Button();
			tickUp.Text = "+";
			tickUp.Position = new Vector2(505, 160);
			tickUp.Size = new Vector2(25, 20);
			tickUp.IsRepeatButton = true;
			tickUp.RepeatInterval = 0.05f;
			tickUp.TooltipText = "Increase value";
			tickUp.OnButtonPressed += (btn, mbtn, pos) => { tickGauge.Value += 1; };
			FUI.AddControl(tickUp);

			// Fuel gauge (red-yellow-green, vertical)
			Label fuelLabel = new Label("Fuel (Vertical)");
			fuelLabel.Position = new Vector2(550, 85);
			fuelLabel.Size = new Vector2(80, 16);
			fuelLabel.Alignment = Align.Center;
			FUI.AddControl(fuelLabel);

			BarGauge fuelGauge = new BarGauge(0, 100);
			fuelGauge.Position = new Vector2(570, 105);
			fuelGauge.Size = new Vector2(25, 100);
			fuelGauge.Orientation = BarGaugeOrientation.Vertical;
			fuelGauge.Value = 35;
			fuelGauge.SetupFuelZones();
			fuelGauge.TooltipText = "Fuel gauge (vertical)";
			FUI.AddControl(fuelGauge);

			// Fuel gauge controls
			Button fuelDown = new Button();
			fuelDown.Text = "-";
			fuelDown.Position = new Vector2(600, 165);
			fuelDown.Size = new Vector2(25, 25);
			fuelDown.IsRepeatButton = true;
			fuelDown.RepeatInterval = 0.05f;
			fuelDown.TooltipText = "Decrease fuel";
			fuelDown.OnButtonPressed += (btn, mbtn, pos) => { fuelGauge.Value -= 2; };
			FUI.AddControl(fuelDown);

			Button fuelUp = new Button();
			fuelUp.Text = "+";
			fuelUp.Position = new Vector2(600, 135);
			fuelUp.Size = new Vector2(25, 25);
			fuelUp.IsRepeatButton = true;
			fuelUp.RepeatInterval = 0.05f;
			fuelUp.TooltipText = "Increase fuel";
			fuelUp.OnButtonPressed += (btn, mbtn, pos) => { fuelGauge.Value += 2; };
			FUI.AddControl(fuelUp);

			// === VUMeter Section ===
			Label vuSectionLabel = new Label("VUMeter");
			vuSectionLabel.Position = new Vector2(660, 60);
			vuSectionLabel.Size = new Vector2(200, 24);
			vuSectionLabel.Alignment = Align.Left;
			FUI.AddControl(vuSectionLabel);

			// Continuous VU meter
			Label vuContinuousLabel = new Label("Continuous");
			vuContinuousLabel.Position = new Vector2(660, 85);
			vuContinuousLabel.Size = new Vector2(60, 16);
			vuContinuousLabel.Alignment = Align.Center;
			FUI.AddControl(vuContinuousLabel);

			VUMeter vuMeter1 = new VUMeter();
			vuMeter1.Position = new Vector2(675, 105);
			vuMeter1.Size = new Vector2(25, 100);
			vuMeter1.Orientation = VUMeterOrientation.Vertical;
			vuMeter1.TooltipText = "Continuous VU meter with peak hold";
			FUI.AddControl(vuMeter1);

			// Segmented VU meter
			Label vuSegmentedLabel = new Label("Segmented");
			vuSegmentedLabel.Position = new Vector2(720, 85);
			vuSegmentedLabel.Size = new Vector2(60, 16);
			vuSegmentedLabel.Alignment = Align.Center;
			FUI.AddControl(vuSegmentedLabel);

			VUMeter vuMeter2 = new VUMeter();
			vuMeter2.Position = new Vector2(735, 105);
			vuMeter2.Size = new Vector2(25, 100);
			vuMeter2.Orientation = VUMeterOrientation.Vertical;
			vuMeter2.SegmentCount = 10;
			vuMeter2.TooltipText = "Segmented VU meter (LED style)";
			FUI.AddControl(vuMeter2);

			// Slider to control VU meters
			Label vuSliderLabel = new Label("Level");
			vuSliderLabel.Position = new Vector2(780, 85);
			vuSliderLabel.Size = new Vector2(30, 16);
			vuSliderLabel.Alignment = Align.Center;
			FUI.AddControl(vuSliderLabel);

			Slider vuSlider = new Slider();
			vuSlider.Position = new Vector2(785, 105);
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
			vuHorizontalLabel.Position = new Vector2(660, 220);
			vuHorizontalLabel.Size = new Vector2(200, 16);
			vuHorizontalLabel.Alignment = Align.Left;
			FUI.AddControl(vuHorizontalLabel);

			VUMeter vuHoriz1 = new VUMeter();
			vuHoriz1.Position = new Vector2(660, 240);
			vuHoriz1.Size = new Vector2(150, 20);
			vuHoriz1.Orientation = VUMeterOrientation.Horizontal;
			vuHoriz1.TooltipText = "Horizontal continuous VU meter";
			FUI.AddControl(vuHoriz1);

			VUMeter vuHoriz2 = new VUMeter();
			vuHoriz2.Position = new Vector2(660, 265);
			vuHoriz2.Size = new Vector2(150, 20);
			vuHoriz2.Orientation = VUMeterOrientation.Horizontal;
			vuHoriz2.SegmentCount = 15;
			vuHoriz2.TooltipText = "Horizontal segmented VU meter";
			FUI.AddControl(vuHoriz2);

			// Horizontal slider
			Slider vuHorizSlider = new Slider();
			vuHorizSlider.Position = new Vector2(660, 295);
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

			// === Car Dashboard Preview ===
			Label dashLabel = new Label("Car Dashboard Preview");
			dashLabel.Position = new Vector2(20, 280);
			dashLabel.Size = new Vector2(250, 24);
			dashLabel.Alignment = Align.Left;
			FUI.AddControl(dashLabel);

			Panel dashPanel = new Panel();
			dashPanel.Position = new Vector2(20, 310);
			dashPanel.Size = new Vector2(280, 150);
			dashPanel.TooltipText = "Dashboard preview combining multiple gauges";
			FUI.AddControl(dashPanel);

			// Dashboard RPM gauge
			RadialGauge dashRpm = new RadialGauge(0, 8000);
			dashRpm.Position = new Vector2(10, 10);
			dashRpm.Size = new Vector2(100, 100);
			dashRpm.Value = 2500;
			dashRpm.SetupRPMZones();
			dashRpm.UnitSuffix = "";
			dashRpm.ValueFormat = "F0";
			dashPanel.AddChild(dashRpm);

			// Dashboard Speed gauge
			RadialGauge dashSpeed = new RadialGauge(0, 200);
			dashSpeed.Position = new Vector2(120, 10);
			dashSpeed.Size = new Vector2(100, 100);
			dashSpeed.Value = 60;
			dashSpeed.SetupSpeedZones();
			dashSpeed.UnitSuffix = "";
			dashSpeed.MajorTickCount = 8;
			dashPanel.AddChild(dashSpeed);

			// Dashboard Fuel bar
			BarGauge dashFuel = new BarGauge(0, 100);
			dashFuel.Position = new Vector2(230, 10);
			dashFuel.Size = new Vector2(20, 100);
			dashFuel.Orientation = BarGaugeOrientation.Vertical;
			dashFuel.Value = 75;
			dashFuel.SetupFuelZones();
			dashPanel.AddChild(dashFuel);

			// Dashboard Temp bar
			BarGauge dashTemp = new BarGauge(0, 100);
			dashTemp.Position = new Vector2(10, 120);
			dashTemp.Size = new Vector2(120, 18);
			dashTemp.Value = 45;
			dashTemp.SetupTemperatureZones();
			dashTemp.ShowValue = true;
			dashTemp.UnitSuffix = "°C";
			dashPanel.AddChild(dashTemp);

			// Slider to simulate driving
			Label driveLabel = new Label("Simulate:");
			driveLabel.Position = new Vector2(140, 120);
			driveLabel.Size = new Vector2(50, 16);
			driveLabel.Alignment = Align.Left;
			dashPanel.AddChild(driveLabel);

			Slider driveSlider = new Slider();
			driveSlider.Position = new Vector2(140, 140);
			driveSlider.Size = new Vector2(100, 20);
			driveSlider.MinValue = 0;
			driveSlider.MaxValue = 1;
			driveSlider.Value = 0.3f;
			driveSlider.Step = 0.01f;
			driveSlider.TooltipText = "Simulate throttle position";
			driveSlider.OnValueChanged += (slider, val) =>
			{
				dashRpm.Value = 800 + (val * 7000);
				dashSpeed.Value = val * 180;
			};
			dashPanel.AddChild(driveSlider);
		}
	}
}
