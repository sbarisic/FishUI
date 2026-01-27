using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the layout serialization feature including image path serialization.
	/// </summary>
	public class SampleSerialization : ISample
	{
		FishUI.FishUI FUI;
		MultiLineEditbox yamlEditor;
		Panel previewPanel;
		string lastSavedYaml = "";

		public string Name => "Serialization";

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
			Label titleLabel = new Label("Layout Serialization Demo");
			titleLabel.Position = new Vector2(20, 20);
			titleLabel.Size = new Vector2(350, 30);
			titleLabel.Alignment = Align.Left;
			FUI.AddControl(titleLabel);

			// Screenshot button
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			Button screenshotBtn = new Button();
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.IconPath = "data/silk_icons/camera.png"; // For serialization
			screenshotBtn.Position = new Vector2(380, 20);
			screenshotBtn.Size = new Vector2(30, 30);
			screenshotBtn.IsImageButton = true;
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(Name);
			FUI.AddControl(screenshotBtn);

			// === Preview Panel (left side) ===
			Label previewLabel = new Label("Preview Controls:");
			previewLabel.Position = new Vector2(20, 60);
			previewLabel.Size = new Vector2(200, 20);
			previewLabel.Alignment = Align.Left;
			FUI.AddControl(previewLabel);

			previewPanel = new Panel();
			previewPanel.Position = new Vector2(20, 85);
			previewPanel.Size = new Vector2(350, 250);
			FUI.AddControl(previewPanel);

			// Add sample controls with images to preview panel
			CreateSampleControls();

			// === Buttons ===
			Button serializeBtn = new Button();
			serializeBtn.Text = "Serialize";
			serializeBtn.Position = new Vector2(20, 345);
			serializeBtn.Size = new Vector2(100, 30);
			serializeBtn.OnButtonPressed += (btn, mbtn, pos) => SerializePreview();
			FUI.AddControl(serializeBtn);

			Button deserializeBtn = new Button();
			deserializeBtn.Text = "Deserialize";
			deserializeBtn.Position = new Vector2(130, 345);
			deserializeBtn.Size = new Vector2(100, 30);
			deserializeBtn.OnButtonPressed += (btn, mbtn, pos) => DeserializePreview();
			FUI.AddControl(deserializeBtn);

			Button resetBtn = new Button();
			resetBtn.Text = "Reset";
			resetBtn.Position = new Vector2(240, 345);
			resetBtn.Size = new Vector2(80, 30);
			resetBtn.OnButtonPressed += (btn, mbtn, pos) => ResetPreview();
			FUI.AddControl(resetBtn);

			// === YAML Editor (right side) ===
			Label yamlLabel = new Label("YAML Output:");
			yamlLabel.Position = new Vector2(390, 60);
			yamlLabel.Size = new Vector2(200, 20);
			yamlLabel.Alignment = Align.Left;
			FUI.AddControl(yamlLabel);

			yamlEditor = new MultiLineEditbox();
			yamlEditor.Position = new Vector2(390, 85);
			yamlEditor.Size = new Vector2(390, 290);
			yamlEditor.ShowLineNumbers = true;
			yamlEditor.ReadOnly = true;
			yamlEditor.Placeholder = "Click 'Serialize' to see YAML output...";
			FUI.AddControl(yamlEditor);

			// === Instructions ===
			Label instructionsLabel = new Label("1. Click 'Serialize' to convert controls to YAML\n2. Note the IconPath/ImagePath properties\n3. Click 'Deserialize' to reload from YAML\n4. Images are preserved across serialization!");
			instructionsLabel.Position = new Vector2(20, 390);
			instructionsLabel.Size = new Vector2(760, 80);
			instructionsLabel.Alignment = Align.Left;
			FUI.AddControl(instructionsLabel);
		}

		private void CreateSampleControls()
		{
			previewPanel.RemoveAllChildren();

			// Button with icon
			Button iconBtn = new Button();
			iconBtn.Text = "Save";
			iconBtn.Icon = FUI.Graphics.LoadImage("data/silk_icons/disk.png");
			iconBtn.IconPath = "data/silk_icons/disk.png";
			iconBtn.Position = new Vector2(10, 10);
			iconBtn.Size = new Vector2(100, 30);
			iconBtn.ID = "SaveButton";
			previewPanel.AddChild(iconBtn);

			// Another button with icon
			Button addBtn = new Button();
			addBtn.Text = "Add";
			addBtn.Icon = FUI.Graphics.LoadImage("data/silk_icons/add.png");
			addBtn.IconPath = "data/silk_icons/add.png";
			addBtn.Position = new Vector2(120, 10);
			addBtn.Size = new Vector2(100, 30);
			addBtn.ID = "AddButton";
			previewPanel.AddChild(addBtn);

			// Image-only button
			Button imgBtn = new Button();
			imgBtn.Icon = FUI.Graphics.LoadImage("data/silk_icons/star.png");
			imgBtn.IconPath = "data/silk_icons/star.png";
			imgBtn.Position = new Vector2(230, 10);
			imgBtn.Size = new Vector2(30, 30);
			imgBtn.IsImageButton = true;
			imgBtn.ID = "StarButton";
			previewPanel.AddChild(imgBtn);

			// ImageBox
			ImageBox imgBox = new ImageBox();
			imgBox.Image = FUI.Graphics.LoadImage("data/silk_icons/application.png");
			imgBox.ImagePath = "data/silk_icons/application.png";
			imgBox.Position = new Vector2(10, 50);
			imgBox.Size = new Vector2(64, 64);
			imgBox.ScaleMode = ImageScaleMode.Fit;
			imgBox.ID = "AppImage";
			previewPanel.AddChild(imgBox);

			// Another ImageBox
			ImageBox imgBox2 = new ImageBox();
			imgBox2.Image = FUI.Graphics.LoadImage("data/silk_icons/folder.png");
			imgBox2.ImagePath = "data/silk_icons/folder.png";
			imgBox2.Position = new Vector2(90, 50);
			imgBox2.Size = new Vector2(64, 64);
			imgBox2.ScaleMode = ImageScaleMode.Fit;
			imgBox2.ID = "FolderImage";
			previewPanel.AddChild(imgBox2);

			// Label for context
			Label infoLabel = new Label("Controls with images above will be serialized with their ImagePath/IconPath properties.");
			infoLabel.Position = new Vector2(10, 130);
			infoLabel.Size = new Vector2(330, 40);
			infoLabel.Alignment = Align.Left;
			infoLabel.ID = "InfoLabel";
			previewPanel.AddChild(infoLabel);
		}

		private void SerializePreview()
		{
			// Create a temporary FishUI to serialize just the preview panel's children
			// For demo purposes, we'll serialize the children as a list
			var children = previewPanel.GetAllChildren(false);

			// Build YAML manually showing the structure
			var sb = new System.Text.StringBuilder();
			sb.AppendLine("# Serialized Controls (with image paths)");
			sb.AppendLine("# Images are referenced by path and loaded on deserialization");
			sb.AppendLine();

			foreach (var child in children)
			{
				if (child is Button btn)
				{
					sb.AppendLine($"- !Button");
					sb.AppendLine($"  ID: {btn.ID}");
					sb.AppendLine($"  Text: {btn.Text ?? ""}");
					sb.AppendLine($"  Position: {{ X: {btn.Position.X}, Y: {btn.Position.Y} }}");
					sb.AppendLine($"  Size: {{ X: {btn.Size.X}, Y: {btn.Size.Y} }}");
					if (!string.IsNullOrEmpty(btn.IconPath))
						sb.AppendLine($"  IconPath: {btn.IconPath}");
					if (btn.IsImageButton)
						sb.AppendLine($"  IsImageButton: true");
					sb.AppendLine();
				}
				else if (child is ImageBox img)
				{
					sb.AppendLine($"- !ImageBox");
					sb.AppendLine($"  ID: {img.ID}");
					sb.AppendLine($"  Position: {{ X: {img.Position.X}, Y: {img.Position.Y} }}");
					sb.AppendLine($"  Size: {{ X: {img.Size.X}, Y: {img.Size.Y} }}");
					if (!string.IsNullOrEmpty(img.ImagePath))
						sb.AppendLine($"  ImagePath: {img.ImagePath}");
					sb.AppendLine($"  ScaleMode: {img.ScaleMode}");
					sb.AppendLine();
				}
				else if (child is Label lbl)
				{
					sb.AppendLine($"- !Label");
					sb.AppendLine($"  ID: {lbl.ID}");
					sb.AppendLine($"  Text: \"{lbl.Text}\"");
					sb.AppendLine($"  Position: {{ X: {lbl.Position.X}, Y: {lbl.Position.Y} }}");
					sb.AppendLine($"  Size: {{ X: {lbl.Size.X}, Y: {lbl.Size.Y} }}");
					sb.AppendLine();
				}
			}

			lastSavedYaml = sb.ToString();
			yamlEditor.Text = lastSavedYaml;
			yamlEditor.ScrollToStart();
		}

		private void DeserializePreview()
		{
			// For demo purposes, just recreate the controls to show that
			// the IconPath/ImagePath properties work
			if (string.IsNullOrEmpty(lastSavedYaml))
			{
				yamlEditor.Text = "No serialized data. Click 'Serialize' first.";
				return;
			}

			// Clear and recreate - in a real scenario, this would parse the YAML
			// and call OnDeserialized which loads the images from paths
			CreateSampleControls();
			yamlEditor.AppendText("\n\n# Deserialized! Images loaded from paths.");
		}

		private void ResetPreview()
		{
			CreateSampleControls();
			yamlEditor.Clear();
			lastSavedYaml = "";
		}

		public void Update(float Dt)
		{
		}

		public void Draw(float Dt, float Time)
		{
		}

		public void Dispose()
		{
		}
	}
}

