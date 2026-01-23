using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUISample.Samples
{
	/// <summary>
	/// Demonstrates basic input controls: Textbox, ListBox, DropDown, ScrollBars, 
	/// ProgressBars, Sliders, NumericUpDown, ToggleSwitch.
	/// </summary>
	internal class SampleBasicControls : ISample
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

			// === ListBox ===
			Label listLabel = new Label("ListBox");
			listLabel.Position = new Vector2(250, 60);
			listLabel.Alignment = Align.Left;
			FUI.AddControl(listLabel);

			ListBox listBox = new ListBox();
			listBox.Position = new Vector2(250, 85);
			listBox.Size = new Vector2(150, 180);
			FUI.AddControl(listBox);

			for (int i = 0; i < 15; i++)
				listBox.AddItem($"Item {i + 1}");

			// === DropDown ===
			Label dropLabel = new Label("DropDown");
			dropLabel.Position = new Vector2(420, 60);
			dropLabel.Alignment = Align.Left;
			FUI.AddControl(dropLabel);

			DropDown dropDown = new DropDown();
			dropDown.Position = new Vector2(420, 85);
			dropDown.Size = new Vector2(150, 30);
			FUI.AddControl(dropDown);

			for (int i = 0; i < 10; i++)
				dropDown.AddItem($"Option {i + 1}");

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

			// === ImageBox ===
			Label imageBoxLabel = new Label("ImageBox Scaling Modes");
			imageBoxLabel.Position = new Vector2(500, 60);
			imageBoxLabel.Size = new Vector2(200, 20);
			imageBoxLabel.Alignment = Align.Left;
			FUI.AddControl(imageBoxLabel);

			// Load a sample image for demonstration
			ImageRef sampleImage = FUI.Graphics.LoadImage("data/silk_icons/picture.png");

			// None - original size, centered
			Label noneLabel = new Label("None");
			noneLabel.Position = new Vector2(500, 85);
			noneLabel.Size = new Vector2(80, 16);
			noneLabel.Alignment = Align.Left;
			FUI.AddControl(noneLabel);

			ImageBox imgNone = new ImageBox(sampleImage);
			imgNone.Position = new Vector2(500, 105);
			imgNone.Size = new Vector2(80, 80);
			imgNone.ScaleMode = ImageScaleMode.None;
			imgNone.TooltipText = "ScaleMode.None - Original size, centered";
			FUI.AddControl(imgNone);

			// Stretch - fills bounds, may distort
			Label stretchLabel = new Label("Stretch");
			stretchLabel.Position = new Vector2(590, 85);
			stretchLabel.Size = new Vector2(80, 16);
			stretchLabel.Alignment = Align.Left;
			FUI.AddControl(stretchLabel);

			ImageBox imgStretch = new ImageBox(sampleImage);
			imgStretch.Position = new Vector2(590, 105);
			imgStretch.Size = new Vector2(80, 80);
			imgStretch.ScaleMode = ImageScaleMode.Stretch;
			imgStretch.TooltipText = "ScaleMode.Stretch - Fills bounds (may distort)";
			FUI.AddControl(imgStretch);

			// Fit - maintains aspect ratio, fits within bounds
			Label fitLabel = new Label("Fit");
			fitLabel.Position = new Vector2(500, 195);
			fitLabel.Size = new Vector2(80, 16);
			fitLabel.Alignment = Align.Left;
			FUI.AddControl(fitLabel);

			ImageBox imgFit = new ImageBox(sampleImage);
			imgFit.Position = new Vector2(500, 215);
			imgFit.Size = new Vector2(80, 50);
			imgFit.ScaleMode = ImageScaleMode.Fit;
			imgFit.TooltipText = "ScaleMode.Fit - Fits within bounds (maintains aspect)";
			FUI.AddControl(imgFit);

			// Fill - maintains aspect ratio, fills bounds (may crop)
			Label fillLabel = new Label("Fill");
			fillLabel.Position = new Vector2(590, 195);
			fillLabel.Size = new Vector2(80, 16);
			fillLabel.Alignment = Align.Left;
			FUI.AddControl(fillLabel);

			ImageBox imgFill = new ImageBox(sampleImage);
			imgFill.Position = new Vector2(590, 215);
			imgFill.Size = new Vector2(80, 50);
			imgFill.ScaleMode = ImageScaleMode.Fill;
			imgFill.TooltipText = "ScaleMode.Fill - Fills bounds (may crop, maintains aspect)";
			FUI.AddControl(imgFill);

			// Clickable ImageBox demo
			Label clickLabel = new Label("Clickable:");
			clickLabel.Position = new Vector2(500, 280);
			clickLabel.Size = new Vector2(80, 16);
			clickLabel.Alignment = Align.Left;
			FUI.AddControl(clickLabel);

			Label clickCountLabel = new Label("Clicks: 0");
			clickCountLabel.Position = new Vector2(590, 305);
			clickCountLabel.Size = new Vector2(80, 20);
			clickCountLabel.Alignment = Align.Left;
			FUI.AddControl(clickCountLabel);

			int clickCount = 0;
			ImageBox imgClickable = new ImageBox(sampleImage);
			imgClickable.Position = new Vector2(500, 305);
			imgClickable.Size = new Vector2(80, 80);
			imgClickable.ScaleMode = ImageScaleMode.Stretch;
			imgClickable.TooltipText = "Click me!";
			imgClickable.OnClick += (sender, btn, pos) =>
			{
				clickCount++;
				clickCountLabel.Text = $"Clicks: {clickCount}";
			};
			FUI.AddControl(imgClickable);

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
		}
	}
}
