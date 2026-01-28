using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates loading and rendering a layout created with the FishUIEditor.
	/// Loads from data/layouts/editor_layout.yaml if it exists, otherwise shows a blank panel.
	/// </summary>
	public class SampleEditorLayout : ISample
	{
		FishUI.FishUI FUI;
		Label _statusLabel;
		Panel _layoutContainer;

		public string Name => "Editor Layout";

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
			Label titleLabel = new Label("Editor Layout Demo");
			titleLabel.Position = new Vector2(20, 20);
			titleLabel.Size = new Vector2(350, 30);
			titleLabel.Alignment = Align.Left;
			FUI.AddControl(titleLabel);

			// Screenshot button
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			Button screenshotBtn = new Button();
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.IconPath = "data/silk_icons/camera.png";
			screenshotBtn.Position = new Vector2(380, 20);
			screenshotBtn.Size = new Vector2(30, 30);
			screenshotBtn.IsImageButton = true;
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(Name);
			FUI.AddControl(screenshotBtn);

			// Reload button
			Button reloadBtn = new Button();
			reloadBtn.Text = "Reload Layout";
			reloadBtn.Position = new Vector2(420, 20);
			reloadBtn.Size = new Vector2(120, 30);
			reloadBtn.TooltipText = "Reload the layout from data/layouts/editor_layout.yaml";
			reloadBtn.OnButtonPressed += (btn, mbtn, pos) => ReloadLayout();
			FUI.AddControl(reloadBtn);

			// Status label
			_statusLabel = new Label("");
			_statusLabel.Position = new Vector2(550, 20);
			_statusLabel.Size = new Vector2(400, 30);
			_statusLabel.Alignment = Align.Left;
			FUI.AddControl(_statusLabel);

			// Instructions
			Label instructionsLabel = new Label("This sample loads layouts created with FishUIEditor from data/layouts/editor_layout.yaml");
			instructionsLabel.Position = new Vector2(20, 55);
			instructionsLabel.Size = new Vector2(800, 20);
			instructionsLabel.Alignment = Align.Left;
			FUI.AddControl(instructionsLabel);

			// Layout container panel
			_layoutContainer = new Panel();
			_layoutContainer.Position = new Vector2(20, 85);
			_layoutContainer.Size = new Vector2(760, 480);
			_layoutContainer.Variant = PanelVariant.Dark;
			FUI.AddControl(_layoutContainer);

			// Try to load the layout
			LoadLayout();
		}

		private void LoadLayout()
		{
			const string layoutPath = "data/layouts/editor_layout.yaml";

			try
			{
				// Check if file exists
				if (!FUI.FileSystem.Exists(layoutPath))
				{
					ShowEmptyState($"Layout file not found: {layoutPath}");
					return;
				}

				// Read and deserialize the layout
				string yaml = FUI.FileSystem.ReadAllText(layoutPath);
				var controls = LayoutFormat.DeserializeControls(yaml);

				// Clear existing controls in container
				_layoutContainer.RemoveAllChildren();

				// Add loaded controls to the container
				foreach (var control in controls)
				{
					control.OnDeserialized(FUI);
					_layoutContainer.AddChild(control);
				}

				SetStatus($"Loaded {controls.Count} control(s) from {layoutPath}");
			}
			catch (Exception ex)
			{
				ShowEmptyState($"Failed to load layout: {ex.Message}");
			}
		}

		private void ReloadLayout()
		{
			// Clear container
			_layoutContainer.RemoveAllChildren();

			// Reload
			LoadLayout();
		}

		private void ShowEmptyState(string message)
		{
			SetStatus(message);

			// Show instructions for creating a layout
			Label emptyLabel = new Label("No layout loaded");
			emptyLabel.Position = new Vector2(20, 20);
			emptyLabel.Size = new Vector2(300, 30);
			emptyLabel.Alignment = Align.Left;
			_layoutContainer.AddChild(emptyLabel);

			Label instructionLabel = new Label("To create a layout:");
			instructionLabel.Position = new Vector2(20, 60);
			instructionLabel.Size = new Vector2(300, 20);
			instructionLabel.Alignment = Align.Left;
			_layoutContainer.AddChild(instructionLabel);

			Label step1 = new Label("1. Run the FishUIEditor project");
			step1.Position = new Vector2(40, 85);
			step1.Size = new Vector2(400, 20);
			step1.Alignment = Align.Left;
			_layoutContainer.AddChild(step1);

			Label step2 = new Label("2. Design your UI by dragging controls from the toolbox");
			step2.Position = new Vector2(40, 105);
			step2.Size = new Vector2(400, 20);
			step2.Alignment = Align.Left;
			_layoutContainer.AddChild(step2);

			Label step3 = new Label("3. Save your layout to data/layouts/editor_layout.yaml");
			step3.Position = new Vector2(40, 125);
			step3.Size = new Vector2(400, 20);
			step3.Alignment = Align.Left;
			_layoutContainer.AddChild(step3);

			Label step4 = new Label("4. Click 'Reload Layout' in this sample to see your creation!");
			step4.Position = new Vector2(40, 145);
			step4.Size = new Vector2(400, 20);
			step4.Alignment = Align.Left;
			_layoutContainer.AddChild(step4);
		}

		private void SetStatus(string message)
		{
			if (_statusLabel != null)
				_statusLabel.Text = message;
		}
	}
}

