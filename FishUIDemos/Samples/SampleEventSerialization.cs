using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates serializable event handlers that can be referenced in YAML layout files.
	/// </summary>
	public class SampleEventSerialization : ISample
	{
		FishUI.FishUI FUI;
		Label _statusLabel;
		MultiLineEditbox _logBox;

		public string Name => "Event Serialization";

		public TakeScreenshotFunc TakeScreenshot { get; set; }

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			FishUITheme theme = UISettings.LoadTheme("data/themes/gwen.yaml", applyImmediately: true);

			// Register event handlers BEFORE loading layout
			RegisterEventHandlers();

			return FUI;
		}

		private void RegisterEventHandlers()
		{
			// Register named event handlers that can be referenced from YAML
			FUI.EventHandlers.Register("OnSaveClicked", (sender, args) =>
			{
				Log($"Save button clicked! (Control ID: {sender.ID})");
			});

			FUI.EventHandlers.Register("OnLoadClicked", (sender, args) =>
			{
				Log($"Load button clicked! (Control ID: {sender.ID})");
			});

			FUI.EventHandlers.Register("OnSliderChanged", (sender, args) =>
			{
				if (args is ValueChangedEventHandlerArgs valueArgs)
				{
					Log($"Slider value: {valueArgs.OldValue:F1} -> {valueArgs.NewValue:F1}");
				}
			});

			FUI.EventHandlers.Register("OnCheckboxToggled", (sender, args) =>
			{
				if (args is CheckedChangedEventHandlerArgs checkArgs)
				{
					Log($"Checkbox '{sender.ID}': {(checkArgs.IsChecked ? "Checked" : "Unchecked")}");
				}
			});

			FUI.EventHandlers.Register("OnItemSelected", (sender, args) =>
			{
				if (args is SelectionChangedEventHandlerArgs selArgs)
				{
					Log($"ListBox selection: index {selArgs.SelectedIndex}, item: {selArgs.SelectedItem}");
				}
			});

			FUI.EventHandlers.Register("OnTextEdited", (sender, args) =>
			{
				if (args is TextChangedEventHandlerArgs textArgs)
				{
					Log($"Text changed: \"{textArgs.OldText}\" -> \"{textArgs.NewText}\"");
				}
			});
		}

		public void Init()
		{
			// === Title ===
			Label titleLabel = new Label("Event Serialization Demo");
			titleLabel.Position = new Vector2(20, 20);
			titleLabel.Size = new Vector2(350, 30);
			titleLabel.Alignment = Align.Left;
			FUI.AddControl(titleLabel);

			// Screenshot button
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			Button screenshotBtn = new Button();
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.Position = new Vector2(380, 20);
			screenshotBtn.Size = new Vector2(30, 30);
			screenshotBtn.IsImageButton = true;
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(GetType().Name);
			FUI.AddControl(screenshotBtn);

			float yPos = 55;

			// Instructions
			Label instructionsLabel = new Label("Controls below use serializable OnXxxHandler properties:");
			instructionsLabel.Position = new Vector2(20, yPos);
			instructionsLabel.Size = new Vector2(400, 20);
			instructionsLabel.Alignment = Align.Left;
			FUI.AddControl(instructionsLabel);

			yPos += 30;

			// === Buttons with OnClickHandler ===
			Button saveBtn = new Button();
			saveBtn.ID = "saveButton";
			saveBtn.Text = "Save";
			saveBtn.Position = new Vector2(20, yPos);
			saveBtn.Size = new Vector2(80, 28);
			saveBtn.OnClickHandler = "OnSaveClicked"; // Serializable handler reference
			FUI.AddControl(saveBtn);

			Button loadBtn = new Button();
			loadBtn.ID = "loadButton";
			loadBtn.Text = "Load";
			loadBtn.Position = new Vector2(110, yPos);
			loadBtn.Size = new Vector2(80, 28);
			loadBtn.OnClickHandler = "OnLoadClicked";
			FUI.AddControl(loadBtn);

			yPos += 40;

			// === Slider with OnValueChangedHandler ===
			Label sliderLabel = new Label("Volume:");
			sliderLabel.Position = new Vector2(20, yPos);
			sliderLabel.Size = new Vector2(60, 24);
			sliderLabel.Alignment = Align.Left;
			FUI.AddControl(sliderLabel);

			Slider volumeSlider = new Slider();
			volumeSlider.ID = "volumeSlider";
			volumeSlider.Position = new Vector2(85, yPos);
			volumeSlider.Size = new Vector2(150, 24);
			volumeSlider.MinValue = 0;
			volumeSlider.MaxValue = 100;
			volumeSlider.Value = 50;
			volumeSlider.OnValueChangedHandler = "OnSliderChanged";
			FUI.AddControl(volumeSlider);

			yPos += 35;

			// === Checkboxes with OnCheckedChangedHandler ===
			CheckBox enableSoundCb = new CheckBox("Enable Sound");
			enableSoundCb.ID = "enableSound";
			enableSoundCb.Position = new Vector2(20, yPos);
			enableSoundCb.Size = new Vector2(20, 20);
			enableSoundCb.OnCheckedChangedHandler = "OnCheckboxToggled";
			FUI.AddControl(enableSoundCb);

			Label cbLabel1 = new Label("Enable Sound");
			cbLabel1.Position = new Vector2(45, yPos);
			cbLabel1.Size = new Vector2(100, 20);
			cbLabel1.Alignment = Align.Left;
			FUI.AddControl(cbLabel1);

			CheckBox enableMusicCb = new CheckBox("Enable Music");
			enableMusicCb.ID = "enableMusic";
			enableMusicCb.Position = new Vector2(160, yPos);
			enableMusicCb.Size = new Vector2(20, 20);
			enableMusicCb.IsChecked = true;
			enableMusicCb.OnCheckedChangedHandler = "OnCheckboxToggled";
			FUI.AddControl(enableMusicCb);

			Label cbLabel2 = new Label("Enable Music");
			cbLabel2.Position = new Vector2(185, yPos);
			cbLabel2.Size = new Vector2(100, 20);
			cbLabel2.Alignment = Align.Left;
			FUI.AddControl(cbLabel2);

			yPos += 35;

			// === ListBox with OnSelectionChangedHandler ===
			Label listLabel = new Label("Select theme:");
			listLabel.Position = new Vector2(20, yPos);
			listLabel.Size = new Vector2(100, 20);
			listLabel.Alignment = Align.Left;
			FUI.AddControl(listLabel);

			yPos += 22;

			ListBox themeList = new ListBox();
			themeList.ID = "themeList";
			themeList.Position = new Vector2(20, yPos);
			themeList.Size = new Vector2(150, 80);
			themeList.AddItem("Default");
			themeList.AddItem("Dark");
			themeList.AddItem("Light");
			themeList.AddItem("Blue");
			themeList.OnSelectionChangedHandler = "OnItemSelected";
			FUI.AddControl(themeList);

			// === Textbox with OnTextChangedHandler ===
			Label textLabel = new Label("Username:");
			textLabel.Position = new Vector2(190, yPos - 22);
			textLabel.Size = new Vector2(80, 20);
			textLabel.Alignment = Align.Left;
			FUI.AddControl(textLabel);

			Textbox usernameBox = new Textbox();
			usernameBox.ID = "usernameBox";
			usernameBox.Position = new Vector2(190, yPos);
			usernameBox.Size = new Vector2(150, 24);
			usernameBox.Placeholder = "Enter username";
			usernameBox.OnTextChangedHandler = "OnTextEdited";
			FUI.AddControl(usernameBox);

			yPos += 95;

			// Status label
			_statusLabel = new Label("Interact with controls to see events logged below:");
			_statusLabel.Position = new Vector2(20, yPos);
			_statusLabel.Size = new Vector2(400, 20);
			_statusLabel.Alignment = Align.Left;
			FUI.AddControl(_statusLabel);

			yPos += 25;

			// Event log
			_logBox = new MultiLineEditbox();
			_logBox.Position = new Vector2(20, yPos);
			_logBox.Size = new Vector2(380, 100);
			_logBox.ReadOnly = true;
			_logBox.ShowLineNumbers = false;
			FUI.AddControl(_logBox);

			yPos += 110;

			// YAML example
			Label yamlLabel = new Label("YAML Example: OnClickHandler: \"OnSaveClicked\"");
			yamlLabel.Position = new Vector2(20, yPos);
			yamlLabel.Size = new Vector2(400, 20);
			yamlLabel.Alignment = Align.Left;
			FUI.AddControl(yamlLabel);
		}

		private void Log(string message)
		{
			string timestamp = DateTime.Now.ToString("HH:mm:ss");
			string logLine = $"[{timestamp}] {message}";

			if (_logBox != null)
			{
				if (!string.IsNullOrEmpty(_logBox.Text))
					_logBox.Text += "\n";
				_logBox.Text += logLine;
			}
		}

		public void Update(float dt)
		{
		}
	}
}
