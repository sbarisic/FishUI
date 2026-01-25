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
				// Serialize current layout to show YAML output
				SerializeCurrentLayout();
			});

			FUI.EventHandlers.Register("OnLoadClicked", (sender, args) =>
			{
				Log($"Load button clicked! (Control ID: {sender.ID})");
				// Reload layout from YAML file
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

		private void SerializeCurrentLayout()
		{
			// Serialize current controls to YAML
			string yaml = LayoutFormat.Serialize(FUI);
			Log("Layout serialized to YAML:");

			// Show first few lines of YAML in the yaml preview
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

		private void LoadLayoutFromYaml()
		{
			try
			{
				// Store references to controls we don't want to reload
				var logBox = _logBox;
				var statusLabel = _statusLabel;

				Log("Reloading layout from YAML file...");

				// Load the layout - this will deserialize controls with their OnXxxHandler properties
				LayoutFormat.DeserializeFromFile(FUI, "data/layouts/event_demo.yaml");

				// Re-add the static UI elements (title, log, etc.)
				RebuildStaticUI();

				// Populate the ListBox items (not stored in YAML for this demo)
				var themeList = FUI.FindControlByID<ListBox>("themeList");
				if (themeList != null)
				{
					themeList.AddItem("Default");
					themeList.AddItem("Dark");
					themeList.AddItem("Light");
					themeList.AddItem("Blue");
				}

				Log("Layout loaded! Event handlers are automatically connected.");
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
			_statusLabel.Size = new Vector2(450, 20);
			_statusLabel.Alignment = Align.Left;
			FUI.AddControl(_statusLabel);

			_logBox = new MultiLineEditbox();
			_logBox.Position = new Vector2(20, 335);
			_logBox.Size = new Vector2(430, 120);
			_logBox.ReadOnly = true;
			_logBox.ShowLineNumbers = false;
			FUI.AddControl(_logBox);

			// YAML file reference
			Label yamlLabel = new Label("YAML source: data/layouts/event_demo.yaml");
			yamlLabel.Position = new Vector2(20, 460);
			yamlLabel.Size = new Vector2(400, 20);
			yamlLabel.Alignment = Align.Left;
			FUI.AddControl(yamlLabel);
		}

		public void Init()
		{
			// Load the layout from YAML file - controls have OnXxxHandler properties set
			LoadLayoutFromYaml();
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
