using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the UI scaling feature with interactive scale adjustment.
	/// </summary>
	public class SampleUIScaling : ISample
	{
		FishUI.FishUI FUI;
		FishUISettings _settings;
		IFishUIGfx _gfx;
		IFishUIInput _input;
		IFishUIEvents _events;

		public string Name => "UI Scaling";

		public TakeScreenshotFunc TakeScreenshot { get; set; }

		// Controls that display current scale info
		Label scaleValueLabel;
		Slider scaleSlider;

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			_settings = UISettings;
			_gfx = Gfx;
			_input = Input;
			_events = Events;

			// Start with default scale
			UISettings.UIScale = 1.0f;

			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			FishUITheme theme = UISettings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);

			return FUI;
		}

		public void Init()
		{
			CreateDemoUI();
		}

		void CreateDemoUI()
		{
			// === Title ===
			Label titleLabel = new Label("UI Scaling Demo");
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

			// === Scale Control Section ===
			Label scaleLabel = new Label("UI Scale Factor");
			scaleLabel.Position = new Vector2(20, 60);
			scaleLabel.Alignment = Align.Left;
			FUI.AddControl(scaleLabel);

			scaleValueLabel = new Label($"Current Scale: {_settings.UIScale:F2}x");
			scaleValueLabel.Position = new Vector2(150, 60);
			scaleValueLabel.Size = new Vector2(150, 20);
			scaleValueLabel.Alignment = Align.Left;
			FUI.AddControl(scaleValueLabel);

			scaleSlider = new Slider();
			scaleSlider.Position = new Vector2(20, 85);
			scaleSlider.Size = new Vector2(200, 20);
			scaleSlider.MinValue = 0.5f;
			scaleSlider.MaxValue = 2.0f;
			scaleSlider.Value = _settings.UIScale;
			scaleSlider.OnValueChanged += (slider, value) =>
			{
				scaleValueLabel.Text = $"Current Scale: {value:F2}x";
			};
			FUI.AddControl(scaleSlider);

			Button applyScaleBtn = new Button();
			applyScaleBtn.Text = "Apply Scale";
			applyScaleBtn.Position = new Vector2(230, 85);
			applyScaleBtn.Size = new Vector2(100, 25);
			applyScaleBtn.TooltipText = "Apply the new scale (rebuilds UI)";
			applyScaleBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				ApplyNewScale(scaleSlider.Value);
			};
			FUI.AddControl(applyScaleBtn);

			// Preset scale buttons
			Label presetsLabel = new Label("Presets:");
			presetsLabel.Position = new Vector2(20, 120);
			presetsLabel.Alignment = Align.Left;
			FUI.AddControl(presetsLabel);

			float[] presets = { 0.75f, 1.0f, 1.25f, 1.5f, 2.0f };
			for (int i = 0; i < presets.Length; i++)
			{
				float scale = presets[i];
				Button presetBtn = new Button();
				presetBtn.Text = $"{scale:F2}x";
				presetBtn.Position = new Vector2(80 + i * 55, 115);
				presetBtn.Size = new Vector2(50, 25);
				presetBtn.OnButtonPressed += (btn, mbtn, pos) =>
				{
					scaleSlider.Value = scale;
					ApplyNewScale(scale);
				};
				FUI.AddControl(presetBtn);
			}

			// === Demo Controls Section ===
			Label demoLabel = new Label("Sample Controls (affected by scaling)");
			demoLabel.Position = new Vector2(20, 160);
			demoLabel.Alignment = Align.Left;
			FUI.AddControl(demoLabel);

			// Panel with various controls to demonstrate scaling
			Panel demoPanel = new Panel();
			demoPanel.Position = new Vector2(20, 185);
			demoPanel.Size = new Vector2(350, 200);
			FUI.AddControl(demoPanel);

			// Button inside panel
			Button demoButton = new Button();
			demoButton.Text = "Sample Button";
			demoButton.Position = new Vector2(10, 10);
			demoButton.Size = new Vector2(120, 30);
			demoPanel.AddChild(demoButton);

			// Checkbox
			CheckBox demoCheckbox = new CheckBox("Enable Feature");
			demoCheckbox.Position = new Vector2(140, 10);
			demoCheckbox.Size = new Vector2(20, 20);
			demoPanel.AddChild(demoCheckbox);

			// Textbox
			Textbox demoTextbox = new Textbox("Sample text...");
			demoTextbox.Position = new Vector2(10, 50);
			demoTextbox.Size = new Vector2(200, 25);
			demoPanel.AddChild(demoTextbox);

			// ProgressBar
			ProgressBar demoProgress = new ProgressBar();
			demoProgress.Position = new Vector2(10, 85);
			demoProgress.Size = new Vector2(200, 18);
			demoProgress.Value = 0.65f;
			demoPanel.AddChild(demoProgress);

			// Slider
			Slider demoSlider = new Slider();
			demoSlider.Position = new Vector2(10, 115);
			demoSlider.Size = new Vector2(200, 18);
			demoSlider.Value = 0.5f;
			demoPanel.AddChild(demoSlider);

			// NumericUpDown
			NumericUpDown demoNumeric = new NumericUpDown();
			demoNumeric.Position = new Vector2(220, 50);
			demoNumeric.Size = new Vector2(100, 25);
			demoNumeric.Value = 42;
			demoPanel.AddChild(demoNumeric);

			// ToggleSwitch
			ToggleSwitch demoToggle = new ToggleSwitch();
			demoToggle.Position = new Vector2(220, 85);
			demoToggle.Size = new Vector2(50, 25);
			demoPanel.AddChild(demoToggle);

			// ListBox with items
			ListBox demoListBox = new ListBox();
			demoListBox.Position = new Vector2(10, 145);
			demoListBox.Size = new Vector2(150, 50);
			demoListBox.AddItem("Item One");
			demoListBox.AddItem("Item Two");
			demoListBox.AddItem("Item Three");
			demoPanel.AddChild(demoListBox);

			// DropDown
			DropDown demoDropDown = new DropDown();
			demoDropDown.Position = new Vector2(170, 145);
			demoDropDown.Size = new Vector2(150, 25);
			demoDropDown.AddItem("Option A");
			demoDropDown.AddItem("Option B");
			demoDropDown.AddItem("Option C");
			demoPanel.AddChild(demoDropDown);

			// === Info Section ===
			Label infoLabel = new Label("Note: Scaling affects positions, sizes, margins, padding, and fonts.");
			infoLabel.Position = new Vector2(20, 395);
			infoLabel.Size = new Vector2(400, 20);
			infoLabel.Alignment = Align.Left;
			FUI.AddControl(infoLabel);
		}

		void ApplyNewScale(float newScale)
		{
			// Store the new scale
			_settings.UIScale = newScale;

			// Remove all controls and reinitialize
			FUI.RemoveAllControls();


			// Reinitialize the settings to reload fonts at new scale
			_settings.Init(FUI);

			// Reload theme at new scale (using saved preference)
			_settings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);

			// Recreate the demo UI
			CreateDemoUI();

			// Update the slider to reflect current value
			scaleSlider.Value = newScale;
		}
	}
}

