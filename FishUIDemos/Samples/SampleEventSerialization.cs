using FishUI;
using FishUI.Controls;
using System;
using System.Collections.Generic;
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

			FishUITheme theme = UISettings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);

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
				SerializeCurrentLayout();
			});

			FUI.EventHandlers.Register("OnLoadClicked", (sender, args) =>
			{
				Log($"Load button clicked! (Control ID: {sender.ID})");
				LoadLayoutFromYaml();
			});

			FUI.EventHandlers.Register("OnDeleteClicked", (sender, args) =>
			{
				Log($"Delete button clicked! (Control ID: {sender.ID})");
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
			// Try to load the layout from YAML file
			bool loadedFromYaml = false;
			try
			{
				// Load the layout - this clears all controls and loads from YAML
				LayoutFormat.DeserializeFromFile(FUI, "data/layouts/event_demo.yaml");

				// Populate the ListBox items only if empty (items may be serialized)
				var themeList = FUI.FindControlByID<ListBox>("themeList");
				if (themeList != null && themeList.ItemCount == 0)
				{
					themeList.AddItem("Default");
					themeList.AddItem("Dark");
					themeList.AddItem("Light");
					themeList.AddItem("Blue");
				}

				loadedFromYaml = true;
			}
			catch (Exception)
			{
				// YAML loading failed, create controls in code
				CreateControlsInCode();
			}

			// Build the static UI (title, log, labels) - always after loading
			RebuildStaticUI();

			if (loadedFromYaml)
				Log("Layout loaded from YAML! Event handlers are connected.");
			else
				Log("Created controls in code (YAML file not found).");
		}

		private void CreateControlsInCode()
		{
			// Fallback: create controls in code if YAML loading fails
			Button saveBtn = new Button();
			saveBtn.ID = "saveButton";
			saveBtn.Text = "Save";
			saveBtn.Position = new Vector2(20, 85);
			saveBtn.Size = new Vector2(80, 28);
			saveBtn.OnClickHandler = "OnSaveClicked";
			FUI.AddControl(saveBtn);

			Button loadBtn = new Button();
			loadBtn.ID = "loadButton";
			loadBtn.Text = "Load";
			loadBtn.Position = new Vector2(110, 85);
			loadBtn.Size = new Vector2(80, 28);
			loadBtn.OnClickHandler = "OnLoadClicked";
			FUI.AddControl(loadBtn);

			Button deleteBtn = new Button();
			deleteBtn.ID = "deleteButton";
			deleteBtn.Text = "Delete";
			deleteBtn.Position = new Vector2(200, 85);
			deleteBtn.Size = new Vector2(80, 28);
			deleteBtn.OnClickHandler = "OnDeleteClicked";
			FUI.AddControl(deleteBtn);

			Slider volumeSlider = new Slider();
			volumeSlider.ID = "volumeSlider";
			volumeSlider.Position = new Vector2(85, 125);
			volumeSlider.Size = new Vector2(150, 24);
			volumeSlider.MinValue = 0;
			volumeSlider.MaxValue = 100;
			volumeSlider.Value = 50;
			volumeSlider.OnValueChangedHandler = "OnSliderChanged";
			FUI.AddControl(volumeSlider);

			CheckBox enableSoundCb = new CheckBox();
			enableSoundCb.ID = "enableSound";
			enableSoundCb.Position = new Vector2(20, 160);
			enableSoundCb.Size = new Vector2(20, 20);
			enableSoundCb.OnCheckedChangedHandler = "OnCheckboxToggled";
			FUI.AddControl(enableSoundCb);

			CheckBox enableMusicCb = new CheckBox();
			enableMusicCb.ID = "enableMusic";
			enableMusicCb.Position = new Vector2(160, 160);
			enableMusicCb.Size = new Vector2(20, 20);
			enableMusicCb.IsChecked = true;
			enableMusicCb.OnCheckedChangedHandler = "OnCheckboxToggled";
			FUI.AddControl(enableMusicCb);

			ListBox themeList = new ListBox();
			themeList.ID = "themeList";
			themeList.Position = new Vector2(20, 217);
			themeList.Size = new Vector2(150, 80);
			themeList.AddItem("Default");
			themeList.AddItem("Dark");
			themeList.AddItem("Light");
			themeList.AddItem("Blue");
			themeList.OnSelectionChangedHandler = "OnItemSelected";
			FUI.AddControl(themeList);

			Textbox usernameBox = new Textbox();
			usernameBox.ID = "usernameBox";
			usernameBox.Position = new Vector2(190, 217);
			usernameBox.Size = new Vector2(150, 24);
			usernameBox.Placeholder = "Enter username";
			usernameBox.OnTextChangedHandler = "OnTextEdited";
			FUI.AddControl(usernameBox);
		}

		private const string SavedLayoutPath = "data/layouts/event_demo_saved.yaml";

		private void SerializeCurrentLayout()
		{
			try
			{
				// Only serialize controls that have IDs (the dynamic controls from YAML)
				// Exclude static UI elements like labels, log box, etc.
				var dynamicControls = new List<Control>();
				string[] dynamicIDs = { "saveButton", "loadButton", "deleteButton", "volumeSlider",
		"enableSound", "enableMusic", "themeList", "usernameBox" };

				foreach (var id in dynamicIDs)
				{
					var ctrl = FUI.FindControlByID<Control>(id);
					if (ctrl != null)
						dynamicControls.Add(ctrl);
				}

				// Serialize only the dynamic controls using LayoutFormat
				string yaml = LayoutFormat.SerializeControls(dynamicControls);

				// Save to file
				FUI.FileSystem.WriteAllText(SavedLayoutPath, yaml);
				Log($"Layout saved to: {SavedLayoutPath}");

				// Show a preview of what was saved
				string[] lines = yaml.Split('\n');
				int linesToShow = Math.Min(8, lines.Length);
				for (int i = 0; i < linesToShow; i++)
				{
					if (!string.IsNullOrWhiteSpace(lines[i]))
						Log($"  {lines[i].TrimEnd()}");
				}
				if (lines.Length > linesToShow)
					Log($"  ... ({lines.Length - linesToShow} more lines)");
			}
			catch (Exception ex)
			{
				Log($"Error saving: {ex.Message}");
			}
		}

		private void LoadLayoutFromYaml()
		{
			try
			{
				// Try to load from saved file first, fall back to original
				string pathToLoad = System.IO.File.Exists(SavedLayoutPath)
				? SavedLayoutPath
				: "data/layouts/event_demo.yaml";

				Log($"Loading layout from: {pathToLoad}");

				// Load the layout - this clears all controls and loads from YAML
				LayoutFormat.DeserializeFromFile(FUI, pathToLoad);

				// Populate the ListBox items only if empty (items may be serialized)
				var themeList = FUI.FindControlByID<ListBox>("themeList");
				if (themeList != null && themeList.ItemCount == 0)
				{
					themeList.AddItem("Default");
					themeList.AddItem("Dark");
					themeList.AddItem("Light");
					themeList.AddItem("Blue");
				}

				// Re-add the static UI elements
				RebuildStaticUI();

				Log("Layout reloaded! Event handlers are automatically connected.");
			}
			catch (Exception ex)
			{
				Log($"Error loading layout: {ex.Message}");
			}
		}

		private void RebuildStaticUI()
		{
			// Rebuild static UI elements that aren't part of the serialized layout

			// Title
			Label titleLabel = new Label("Event Serialization Demo");
			titleLabel.Position = new Vector2(20, 20);
			titleLabel.Size = new Vector2(350, 30);
			titleLabel.Alignment = Align.Left;
			FUI.AddControl(titleLabel);

			// Screenshot button
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			Button screenshotBtn = new Button();
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.Position = new Vector2(420, 20);
			screenshotBtn.Size = new Vector2(30, 30);
			screenshotBtn.IsImageButton = true;
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(GetType().Name);
			FUI.AddControl(screenshotBtn);

			// Instructions
			Label instructionsLabel = new Label("Controls loaded from YAML with serializable event handlers:");
			instructionsLabel.Position = new Vector2(20, 55);
			instructionsLabel.Size = new Vector2(450, 20);
			instructionsLabel.Alignment = Align.Left;
			FUI.AddControl(instructionsLabel);

			// Labels for controls
			Label sliderLabel = new Label("Volume:");
			sliderLabel.Position = new Vector2(20, 125);
			sliderLabel.Size = new Vector2(60, 24);
			sliderLabel.Alignment = Align.Left;
			FUI.AddControl(sliderLabel);

			Label cbLabel1 = new Label("Enable Sound");
			cbLabel1.Position = new Vector2(45, 160);
			cbLabel1.Size = new Vector2(100, 20);
			cbLabel1.Alignment = Align.Left;
			FUI.AddControl(cbLabel1);

			Label cbLabel2 = new Label("Enable Music");
			cbLabel2.Position = new Vector2(185, 160);
			cbLabel2.Size = new Vector2(100, 20);
			cbLabel2.Alignment = Align.Left;
			FUI.AddControl(cbLabel2);

			Label listLabel = new Label("Select theme:");
			listLabel.Position = new Vector2(20, 195);
			listLabel.Size = new Vector2(100, 20);
			listLabel.Alignment = Align.Left;
			FUI.AddControl(listLabel);

			Label textLabel = new Label("Username:");
			textLabel.Position = new Vector2(190, 195);
			textLabel.Size = new Vector2(80, 20);
			textLabel.Alignment = Align.Left;
			FUI.AddControl(textLabel);

			// Status and log
			_statusLabel = new Label("Interact with controls to see events. Click 'Save' to serialize, 'Load' to reload from YAML.");
			_statusLabel.Position = new Vector2(20, 310);
			_statusLabel.Size = new Vector2(750, 20);
			_statusLabel.Alignment = Align.Left;
			FUI.AddControl(_statusLabel);

			_logBox = new MultiLineEditbox();
			_logBox.Position = new Vector2(20, 335);
			_logBox.Size = new Vector2(750, 180);
			_logBox.ReadOnly = true;
			_logBox.ShowLineNumbers = false;
			FUI.AddControl(_logBox);

			// YAML file reference
			Label yamlLabel = new Label("YAML source: data/layouts/event_demo.yaml");
			yamlLabel.Position = new Vector2(20, 520);
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

