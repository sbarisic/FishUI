using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the LineChart control for real-time data visualization.
	/// Shows sine wave with random noise across multiple data series.
	/// </summary>
	public class SampleLineChart : ISample
	{
		FishUI.FishUI FUI;
		LineChart chart;
		LineChartSeries sineWaveSeries;
		LineChartSeries noisySeries;
		LineChartSeries randomSeries;
		Random random = new Random();
		float time = 0f;

		public string Name => "LineChart";

		public TakeScreenshotFunc TakeScreenshot { get; set; }

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
			Label titleLabel = new Label("LineChart Control Demo");
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

			// === Main Chart ===
			Label chartLabel = new Label("Real-Time Data Visualization");
			chartLabel.Position = new Vector2(20, 60);
			chartLabel.Size = new Vector2(400, 24);
			chartLabel.Alignment = Align.Left;
			FUI.AddControl(chartLabel);

			chart = new LineChart();
			chart.Position = new Vector2(20, 90);
			chart.Size = new Vector2(500, 250);
			chart.MinValue = -1.5f;
			chart.MaxValue = 1.5f;
			chart.TimeWindow = 10f;
			chart.AutoScroll = true;
			chart.HorizontalGridDivisions = 6;
			chart.VerticalGridDivisions = 10;
			chart.YAxisLabelFormat = "F1";
			chart.XAxisLabelFormat = "F0";
			FUI.AddControl(chart);

			// Add data series
			sineWaveSeries = chart.AddSeries("Sine Wave", new FishColor(0, 200, 100, 255));
			sineWaveSeries.LineThickness = 2f;

			noisySeries = chart.AddSeries("Noisy Sine", new FishColor(200, 100, 0, 255));
			noisySeries.LineThickness = 1.5f;

			randomSeries = chart.AddSeries("Random Walk", new FishColor(100, 150, 255, 255));
			randomSeries.LineThickness = 1.5f;

			// === Legend ===
			Label legendLabel = new Label("Legend:");
			legendLabel.Position = new Vector2(540, 90);
			legendLabel.Size = new Vector2(100, 20);
			legendLabel.Alignment = Align.Left;
			FUI.AddControl(legendLabel);

			// Sine wave legend
			Panel sineLegendColor = new Panel();
			sineLegendColor.Position = new Vector2(540, 115);
			sineLegendColor.Size = new Vector2(20, 10);
			sineLegendColor.Color = sineWaveSeries.Color;
			FUI.AddControl(sineLegendColor);

			Label sineLegendLabel = new Label("Sine Wave");
			sineLegendLabel.Position = new Vector2(565, 110);
			sineLegendLabel.Size = new Vector2(100, 20);
			sineLegendLabel.Alignment = Align.Left;
			FUI.AddControl(sineLegendLabel);

			// Noisy sine legend
			Panel noisyLegendColor = new Panel();
			noisyLegendColor.Position = new Vector2(540, 140);
			noisyLegendColor.Size = new Vector2(20, 10);
			noisyLegendColor.Color = noisySeries.Color;
			FUI.AddControl(noisyLegendColor);

			Label noisyLegendLabel = new Label("Noisy Sine");
			noisyLegendLabel.Position = new Vector2(565, 135);
			noisyLegendLabel.Size = new Vector2(100, 20);
			noisyLegendLabel.Alignment = Align.Left;
			FUI.AddControl(noisyLegendLabel);

			// Random walk legend
			Panel randomLegendColor = new Panel();
			randomLegendColor.Position = new Vector2(540, 165);
			randomLegendColor.Size = new Vector2(20, 10);
			randomLegendColor.Color = randomSeries.Color;
			FUI.AddControl(randomLegendColor);

			Label randomLegendLabel = new Label("Random Walk");
			randomLegendLabel.Position = new Vector2(565, 160);
			randomLegendLabel.Size = new Vector2(100, 20);
			randomLegendLabel.Alignment = Align.Left;
			FUI.AddControl(randomLegendLabel);

			// === Controls Section ===
			Label controlsLabel = new Label("Chart Settings:");
			controlsLabel.Position = new Vector2(20, 360);
			controlsLabel.Size = new Vector2(150, 24);
			controlsLabel.Alignment = Align.Left;
			FUI.AddControl(controlsLabel);

			// Time window slider
			Label timeWindowLabel = new Label("Time Window:");
			timeWindowLabel.Position = new Vector2(20, 390);
			timeWindowLabel.Size = new Vector2(100, 20);
			timeWindowLabel.Alignment = Align.Left;
			FUI.AddControl(timeWindowLabel);

			Slider timeWindowSlider = new Slider();
			timeWindowSlider.Position = new Vector2(130, 390);
			timeWindowSlider.Size = new Vector2(150, 20);
			timeWindowSlider.MinValue = 5f;
			timeWindowSlider.MaxValue = 30f;
			timeWindowSlider.Value = chart.TimeWindow;
			timeWindowSlider.OnValueChanged += (slider, val) => chart.TimeWindow = val;
			FUI.AddControl(timeWindowSlider);

		Label timeWindowValueLabel = new Label($"{chart.TimeWindow:F0}s");
			timeWindowValueLabel.Position = new Vector2(290, 390);
			timeWindowValueLabel.Size = new Vector2(50, 20);
			timeWindowValueLabel.Alignment = Align.Left;
			timeWindowSlider.OnValueChanged += (slider, val) => timeWindowValueLabel.Text = $"{val:F0}s";
			FUI.AddControl(timeWindowValueLabel);

			// Pause/Resume button
			Button pauseBtn = new Button();
			pauseBtn.Text = "Pause";
			pauseBtn.Position = new Vector2(360, 385);
			pauseBtn.Size = new Vector2(70, 30);
			pauseBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				chart.IsPaused = !chart.IsPaused;
				pauseBtn.Text = chart.IsPaused ? "Resume" : "Pause";
			};
			FUI.AddControl(pauseBtn);

			// Clear button
			Button clearBtn = new Button();
			clearBtn.Text = "Clear";
			clearBtn.Position = new Vector2(435, 385);
			clearBtn.Size = new Vector2(70, 30);
			clearBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				chart.ClearAllData();
				time = 0f;
				chart.CurrentTime = 0f;
				randomWalkValue = 0f;
			};
			FUI.AddControl(clearBtn);

			// Grid toggle
			CheckBox showGridCheckbox = new CheckBox("Show Grid");
			showGridCheckbox.Position = new Vector2(20, 420);
			showGridCheckbox.Size = new Vector2(15, 15);
			showGridCheckbox.IsChecked = true;
			showGridCheckbox.OnCheckedChanged += (cb, chk) =>
			{
				chart.ShowHorizontalGrid = chk;
				chart.ShowVerticalGrid = chk;
			};
			FUI.AddControl(showGridCheckbox);

			// Labels toggle
			CheckBox showLabelsCheckbox = new CheckBox("Show Labels");
			showLabelsCheckbox.Position = new Vector2(120, 420);
			showLabelsCheckbox.Size = new Vector2(15, 15);
			showLabelsCheckbox.IsChecked = true;
			showLabelsCheckbox.OnCheckedChanged += (cb, chk) =>
			{
				chart.ShowXAxisLabels = chk;
				chart.ShowYAxisLabels = chk;
			};
			FUI.AddControl(showLabelsCheckbox);

			// === Second Chart - Temperature Style ===
			Label tempLabel = new Label("Temperature Monitor Style");
			tempLabel.Position = new Vector2(540, 200);
			tempLabel.Size = new Vector2(200, 24);
			tempLabel.Alignment = Align.Left;
			FUI.AddControl(tempLabel);

			LineChart tempChart = new LineChart();
			tempChart.Position = new Vector2(540, 230);
			tempChart.Size = new Vector2(250, 150);
			tempChart.MinValue = 20f;
			tempChart.MaxValue = 80f;
			tempChart.TimeWindow = 60f;
			tempChart.AutoScroll = true;
			tempChart.HorizontalGridDivisions = 4;
			tempChart.VerticalGridDivisions = 6;
			tempChart.YAxisLabelFormat = "F0";
			tempChart.XAxisLabelFormat = "F0";
			tempChart.BackgroundColor = new FishColor(20, 20, 40, 255);
			tempChart.GridColor = new FishColor(40, 40, 80, 255);
			FUI.AddControl(tempChart);

			var cpuTemp = tempChart.AddSeries("CPU", new FishColor(255, 100, 100, 255));
			var gpuTemp = tempChart.AddSeries("GPU", new FishColor(100, 255, 100, 255));

			// Store reference for update
			this.tempChart = tempChart;
			this.cpuTemp = cpuTemp;
			this.gpuTemp = gpuTemp;
		}

		LineChart tempChart;
		LineChartSeries cpuTemp;
		LineChartSeries gpuTemp;
		float randomWalkValue = 0f;
		float tempTime = 0f;
		float sampleTime = 0f;

		public void Update(float Dt)
		{
			time += Dt;
			tempTime += Dt;
			sampleTime += Dt;


			// Update main chart
			chart.Update(Dt);

			// Add data points at regular intervals (20 samples per second)
			float sampleInterval = 0.05f;

			if (sampleTime >= sampleInterval)
			{
				sampleTime = 0f;

				// Generate sine wave
				float sineValue = (float)Math.Sin(time * 2.0);
				sineWaveSeries.AddPoint(time, sineValue);

				// Generate noisy sine
				float noise = (float)(random.NextDouble() - 0.5) * 0.4f;
				noisySeries.AddPoint(time, sineValue + noise);

				// Generate random walk
				randomWalkValue += (float)(random.NextDouble() - 0.5) * 0.1f;
				randomWalkValue = Math.Clamp(randomWalkValue, -1.2f, 1.2f);
				randomSeries.AddPoint(time, randomWalkValue);
			}

			// Update temperature chart (slower updates)
			tempChart.Update(Dt);

			if (tempTime >= 0.5f) // Update every 0.5 seconds
			{
				tempTime = 0f;

				// Simulate CPU temperature (fluctuates around 50-60)
				float cpu = 55f + (float)(random.NextDouble() - 0.5) * 20f;
				cpuTemp.AddPoint(tempChart.CurrentTime, cpu);

				// Simulate GPU temperature (fluctuates around 60-70)
				float gpu = 65f + (float)(random.NextDouble() - 0.5) * 15f;
				gpuTemp.AddPoint(tempChart.CurrentTime, gpu);
			}
		}

		public void Draw(float Dt, float Time)
		{
		}

		public void Dispose()
		{
		}
	}
}
