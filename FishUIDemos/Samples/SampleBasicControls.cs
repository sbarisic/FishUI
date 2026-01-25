using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates basic input controls: Textbox, ScrollBars, ProgressBars, Sliders,
	/// NumericUpDown, ToggleSwitch, CheckBox, RadioButton, StaticText, and visual features
	/// like Opacity and Color Overrides.
	/// </summary>
	public class SampleBasicControls : ISample
	{
		FishUI.FishUI FUI;

		public string Name => "Basic Controls";

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
			Label titleLabel = new Label("Basic Controls Demo");
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

			// === Text Input ===
			Label textLabel = new Label("Text Input");
			textLabel.Position = new Vector2(20, 60);
			textLabel.Alignment = Align.Left;
			FUI.AddControl(textLabel);

			Textbox textbox = new Textbox("Type here...");
			textbox.Position = new Vector2(20, 85);
			textbox.Size = new Vector2(200, 30);
			FUI.AddControl(textbox);


			// === ScrollBars ===
			Label scrollLabel = new Label("ScrollBars");
			scrollLabel.Position = new Vector2(20, 280);
			scrollLabel.Alignment = Align.Left;
			FUI.AddControl(scrollLabel);

			ScrollBarV scrollV = new ScrollBarV();
			scrollV.Position = new Vector2(20, 305);
			scrollV.Size = new Vector2(20, 150);
			FUI.AddControl(scrollV);

			ScrollBarH scrollH = new ScrollBarH();
			scrollH.Position = new Vector2(50, 305);
			scrollH.Size = new Vector2(150, 20);
			FUI.AddControl(scrollH);

			// === Progress Bars ===
			Label progressLabel = new Label("Progress Bars");
			progressLabel.Position = new Vector2(250, 280);
			progressLabel.Alignment = Align.Left;
			FUI.AddControl(progressLabel);

			// Determinate
			ProgressBar progressBar = new ProgressBar();
			progressBar.Position = new Vector2(250, 305);
			progressBar.Size = new Vector2(200, 20);
			progressBar.Value = 0.75f;
			progressBar.TooltipText = "75% complete";
			FUI.AddControl(progressBar);

			// Indeterminate
			ProgressBar loadingBar = new ProgressBar();
			loadingBar.Position = new Vector2(250, 335);
			loadingBar.Size = new Vector2(200, 20);
			loadingBar.IsIndeterminate = true;
			loadingBar.TooltipText = "Loading...";
			FUI.AddControl(loadingBar);

			// Vertical
			ProgressBar vProgress = new ProgressBar();
			vProgress.Position = new Vector2(460, 305);
			vProgress.Size = new Vector2(20, 100);
			vProgress.Orientation = ProgressBarOrientation.Vertical;
			vProgress.Value = 0.6f;
			vProgress.TooltipText = "Vertical progress bar";
			FUI.AddControl(vProgress);

			// === Toggle Switches ===
			Label toggleLabel = new Label("Toggle Switches");
			toggleLabel.Position = new Vector2(20, 470);
			toggleLabel.Alignment = Align.Left;
			FUI.AddControl(toggleLabel);

			ToggleSwitch toggle1 = new ToggleSwitch();
			toggle1.Position = new Vector2(20, 495);
			toggle1.Size = new Vector2(50, 24);
			toggle1.IsOn = false;
			toggle1.TooltipText = "Simple toggle";
			FUI.AddControl(toggle1);

			ToggleSwitch toggle2 = new ToggleSwitch();
			toggle2.Position = new Vector2(80, 495);
			toggle2.Size = new Vector2(70, 24);
			toggle2.IsOn = true;
			toggle2.ShowLabels = true;
			toggle2.TooltipText = "Toggle with labels";
			FUI.AddControl(toggle2);

			// === Sliders ===
			Label sliderLabel = new Label("Sliders");
			sliderLabel.Position = new Vector2(180, 470);
			sliderLabel.Alignment = Align.Left;
			FUI.AddControl(sliderLabel);

			Slider slider1 = new Slider();
			slider1.Position = new Vector2(180, 495);
			slider1.Size = new Vector2(200, 24);
			slider1.MinValue = 0;
			slider1.MaxValue = 100;
			slider1.Value = 50;
			slider1.ShowValueLabel = true;
			slider1.TooltipText = "Horizontal slider";
			FUI.AddControl(slider1);

			Slider slider2 = new Slider();
			slider2.Position = new Vector2(390, 470);
			slider2.Size = new Vector2(24, 100);
			slider2.Orientation = SliderOrientation.Vertical;
			slider2.MinValue = 0;
			slider2.MaxValue = 100;
			slider2.Step = 10;
			slider2.Value = 30;
			slider2.TooltipText = "Vertical slider (step=10)";
			FUI.AddControl(slider2);

			// === NumericUpDown ===
			Label numericLabel = new Label("NumericUpDown");
			numericLabel.Position = new Vector2(20, 540);
			numericLabel.Alignment = Align.Left;
			FUI.AddControl(numericLabel);

			NumericUpDown numUpDown1 = new NumericUpDown(50, 0, 100, 1);
			numUpDown1.Position = new Vector2(20, 565);
			numUpDown1.Size = new Vector2(120, 24);
			numUpDown1.TooltipText = "Integer (0-100)";
			FUI.AddControl(numUpDown1);

			NumericUpDown numUpDown2 = new NumericUpDown(0.5f, 0f, 1f, 0.1f);
			numUpDown2.Position = new Vector2(150, 565);
			numUpDown2.Size = new Vector2(120, 24);
			numUpDown2.DecimalPlaces = 1;
			numUpDown2.TooltipText = "Decimal (0.0-1.0)";
			FUI.AddControl(numUpDown2);

			// === Opacity Demo ===
			Label opacityLabel = new Label("Opacity:");
			opacityLabel.Position = new Vector2(780, 60);
			opacityLabel.Size = new Vector2(100, 16);
			opacityLabel.Alignment = Align.Left;
			FUI.AddControl(opacityLabel);

			// Three panels with different opacity levels
			Panel panel100 = new Panel();
			panel100.Position = new Vector2(780, 85);
			panel100.Size = new Vector2(60, 60);
			panel100.Opacity = 1.0f;
			panel100.TooltipText = "Opacity: 100%";
			FUI.AddControl(panel100);

			Label lbl100 = new Label("100%");
			lbl100.Position = new Vector2(5, 22);
			lbl100.Size = new Vector2(50, 16);
			lbl100.Alignment = Align.Center;
			panel100.AddChild(lbl100);

			Panel panel50 = new Panel();
			panel50.Position = new Vector2(850, 85);
			panel50.Size = new Vector2(60, 60);
			panel50.Opacity = 0.5f;
			panel50.TooltipText = "Opacity: 50%";
			FUI.AddControl(panel50);

			Label lbl50 = new Label("50%");
			lbl50.Position = new Vector2(5, 22);
			lbl50.Size = new Vector2(50, 16);
			lbl50.Alignment = Align.Center;
			panel50.AddChild(lbl50);

			Panel panel25 = new Panel();
			panel25.Position = new Vector2(920, 85);
			panel25.Size = new Vector2(60, 60);
			panel25.Opacity = 0.25f;
			panel25.TooltipText = "Opacity: 25%";
			FUI.AddControl(panel25);

			Label lbl25 = new Label("25%");
			lbl25.Position = new Vector2(5, 22);
			lbl25.Size = new Vector2(50, 16);
			lbl25.Alignment = Align.Center;
			panel25.AddChild(lbl25);

			// Opacity slider to control a button
			Label opacitySliderLabel = new Label("Adjust:");
			opacitySliderLabel.Position = new Vector2(780, 155);
			opacitySliderLabel.Size = new Vector2(60, 16);
			opacitySliderLabel.Alignment = Align.Left;
			FUI.AddControl(opacitySliderLabel);

			Button opacityButton = new Button();
			opacityButton.Text = "Fade Me";
			opacityButton.Position = new Vector2(780, 175);
			opacityButton.Size = new Vector2(100, 30);
			opacityButton.TooltipText = "Opacity controlled by slider";
			FUI.AddControl(opacityButton);

			Slider opacitySlider = new Slider();
			opacitySlider.Position = new Vector2(890, 175);
			opacitySlider.Size = new Vector2(100, 24);
			opacitySlider.MinValue = 0f;
			opacitySlider.MaxValue = 1f;
			opacitySlider.Value = 1f;
			opacitySlider.Step = 0.05f;
			opacitySlider.ShowValueLabel = true;
			opacitySlider.TooltipText = "Adjust button opacity";
			opacitySlider.OnValueChanged += (slider, val) =>
		{
				opacityButton.Opacity = val;
			};
			FUI.AddControl(opacitySlider);

			// === Color Overrides Demo ===
			Label colorOverrideLabel = new Label("Color Overrides:");
			colorOverrideLabel.Position = new Vector2(780, 310);
			colorOverrideLabel.Size = new Vector2(150, 16);
			colorOverrideLabel.Alignment = Align.Left;
			FUI.AddControl(colorOverrideLabel);

			// Labels with different text colors
			Label redLabel = new Label("Red Text");
			redLabel.Position = new Vector2(780, 335);
			redLabel.Size = new Vector2(80, 20);
			redLabel.SetColorOverride("Text", new FishColor(220, 50, 50, 255));
			redLabel.TooltipText = "SetColorOverride(\"Text\", red)";
			FUI.AddControl(redLabel);

			Label greenLabel = new Label("Green Text");
			greenLabel.Position = new Vector2(870, 335);
			greenLabel.Size = new Vector2(80, 20);
			greenLabel.SetColorOverride("Text", new FishColor(50, 180, 50, 255));
			greenLabel.TooltipText = "SetColorOverride(\"Text\", green)";
			FUI.AddControl(greenLabel);

			Label blueLabel = new Label("Blue Text");
			blueLabel.Position = new Vector2(780, 360);
			blueLabel.Size = new Vector2(80, 20);
			blueLabel.SetColorOverride("Text", new FishColor(50, 100, 220, 255));
			blueLabel.TooltipText = "SetColorOverride(\"Text\", blue)";
			FUI.AddControl(blueLabel);

			// Buttons with custom text colors
			Button colorBtn1 = new Button();
			colorBtn1.Text = "Custom";
			colorBtn1.Position = new Vector2(870, 360);
			colorBtn1.Size = new Vector2(80, 25);
			colorBtn1.SetColorOverride("Text", new FishColor(150, 50, 200, 255));
			colorBtn1.TooltipText = "Button with purple text override";
			FUI.AddControl(colorBtn1);

			// === CheckBox and RadioButton ===
			Label checkLabel = new Label("CheckBox & RadioButton");
			checkLabel.Position = new Vector2(300, 540);
			checkLabel.Alignment = Align.Left;
			FUI.AddControl(checkLabel);

			CheckBox check1 = new CheckBox("Option A");
			check1.Position = new Vector2(300, 565);
			check1.Size = new Vector2(15, 15);
			FUI.AddControl(check1);

			CheckBox check2 = new CheckBox("Option B");
			check2.Position = new Vector2(300, 590);
			check2.Size = new Vector2(15, 15);
			check2.IsChecked = true;
			FUI.AddControl(check2);

			RadioButton radio1 = new RadioButton("Choice 1");
			radio1.Position = new Vector2(420, 565);
			radio1.Size = new Vector2(15, 15);
			radio1.IsChecked = true;
			FUI.AddControl(radio1);

			RadioButton radio2 = new RadioButton("Choice 2");
			radio2.Position = new Vector2(420, 590);
			radio2.Size = new Vector2(15, 15);
			FUI.AddControl(radio2);

			// === StaticText ===
			Label staticTextLabel = new Label("StaticText Alignment & Colors");
			staticTextLabel.Position = new Vector2(600, 400);
			staticTextLabel.Size = new Vector2(200, 20);
			staticTextLabel.Alignment = Align.Left;
			FUI.AddControl(staticTextLabel);

			// Top-Left aligned
			StaticText stTopLeft = new StaticText("Top-Left", Align.Left, VerticalAlign.Top);
			stTopLeft.Position = new Vector2(600, 425);
			stTopLeft.Size = new Vector2(80, 50);
			stTopLeft.ShowBackground = true;
			stTopLeft.BackgroundColor = new FishColor(60, 60, 80, 200);
			stTopLeft.TooltipText = "HorizontalAlign.Left, VerticalAlign.Top";
			FUI.AddControl(stTopLeft);

			// Center aligned
			StaticText stCenter = new StaticText("Center", Align.Center, VerticalAlign.Middle);
			stCenter.Position = new Vector2(690, 425);
			stCenter.Size = new Vector2(80, 50);
			stCenter.ShowBackground = true;
			stCenter.BackgroundColor = new FishColor(60, 80, 60, 200);
			stCenter.TooltipText = "HorizontalAlign.Center, VerticalAlign.Middle";
			FUI.AddControl(stCenter);

			// Bottom-Right aligned
			StaticText stBottomRight = new StaticText("Bot-Right", Align.Right, VerticalAlign.Bottom);
			stBottomRight.Position = new Vector2(600, 485);
			stBottomRight.Size = new Vector2(80, 50);
			stBottomRight.ShowBackground = true;
			stBottomRight.BackgroundColor = new FishColor(80, 60, 60, 200);
			stBottomRight.TooltipText = "HorizontalAlign.Right, VerticalAlign.Bottom";
			FUI.AddControl(stBottomRight);

			// Custom text color
			StaticText stColored = new StaticText("Colored!");
			stColored.Position = new Vector2(690, 485);
			stColored.Size = new Vector2(80, 50);
			stColored.HorizontalAlignment = Align.Center;
			stColored.VerticalAlignment = VerticalAlign.Middle;
			stColored.TextColor = new FishColor(100, 200, 255, 255);
			stColored.ShowBackground = true;
			stColored.BackgroundColor = new FishColor(40, 40, 60, 220);
			stColored.TooltipText = "Custom TextColor (cyan)";
			FUI.AddControl(stColored);

		}
	}
}
