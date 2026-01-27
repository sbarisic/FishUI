using FishUI;
using FishUI.Controls;
using System;
using System.ComponentModel;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Sample object with various property types for PropertyGrid testing.
	/// </summary>
	public class SampleObject
	{
		// Basic types
		[Category("Basic")]
		[DisplayName("Name")]
		[Description("The name of this object")]
		public string Name { get; set; } = "Sample Object";

		[Category("Basic")]
		[DisplayName("Enabled")]
		[Description("Whether this object is enabled")]
		public bool Enabled { get; set; } = true;

		[Category("Basic")]
		[DisplayName("Count")]
		[Description("Number of items")]
		public int Count { get; set; } = 42;

		// Numeric types
		[Category("Numeric")]
		[DisplayName("Speed")]
		[Description("Movement speed in units per second")]
		public float Speed { get; set; } = 10.5f;

		[Category("Numeric")]
		[DisplayName("Scale Factor")]
		[Description("Scale multiplier")]
		public double ScaleFactor { get; set; } = 1.0;

		[Category("Numeric")]
		[DisplayName("Health")]
		public int Health { get; set; } = 100;

		// Enum type
		[Category("Appearance")]
		[DisplayName("Alignment")]
		[Description("Text alignment")]
		public Align TextAlignment { get; set; } = Align.Center;

		[Category("Appearance")]
		[DisplayName("Visible")]
		public bool Visible { get; set; } = true;

		// Vector types
		[Category("Transform")]
		[DisplayName("Position")]
		[Description("Position in 2D space")]
		public Vector2 Position { get; set; } = new Vector2(100, 200);

		// Read-only property
		[Category("Info")]
		[DisplayName("Type Name")]
		[Description("Read-only type information")]
		public string TypeName => GetType().Name;
	}

	/// <summary>
	/// Demonstrates the PropertyGrid control with various property types.
	/// </summary>
	public class SamplePropertyGrid : ISample
	{
		FishUI.FishUI FUI;
		PropertyGrid propertyGrid;
		SampleObject sampleObject;
		Label statusLabel;
		Label nameValueLabel;
		Label enabledValueLabel;
		Label countValueLabel;
		Label speedValueLabel;
		Label alignValueLabel;

		public string Name => "PropertyGrid";

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
			Label titleLabel = new Label("PropertyGrid Demo");
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

			// === Create sample object ===
			sampleObject = new SampleObject();

			// === PropertyGrid ===
			Label propLabel = new Label("Object Properties:");
			propLabel.Position = new Vector2(20, 60);
			propLabel.Alignment = Align.Left;
			FUI.AddControl(propLabel);

			propertyGrid = new PropertyGrid();
			propertyGrid.Position = new Vector2(20, 85);
			propertyGrid.Size = new Vector2(350, 400);
			propertyGrid.SelectedObject = sampleObject;
			propertyGrid.OnPropertyValueChanged += PropertyGrid_OnPropertyValueChanged;
			FUI.AddControl(propertyGrid);

			// === Status label ===
			statusLabel = new Label("Click a property to edit its value");
			statusLabel.Position = new Vector2(20, 500);
			statusLabel.Size = new Vector2(350, 25);
			statusLabel.Alignment = Align.Left;
			FUI.AddControl(statusLabel);

			// === Options panel ===
			Label optionsLabel = new Label("Options:");
			optionsLabel.Position = new Vector2(400, 60);
			optionsLabel.Alignment = Align.Left;
			FUI.AddControl(optionsLabel);

			// Group by category checkbox
			CheckBox groupByCategoryCheck = new CheckBox("Group by Category");
			groupByCategoryCheck.Position = new Vector2(400, 85);
			groupByCategoryCheck.IsChecked = propertyGrid.GroupByCategory;
			FUI.AddControl(groupByCategoryCheck);

			// Sort alphabetically checkbox
			CheckBox sortAlphaCheck = new CheckBox("Sort Alphabetically");
			sortAlphaCheck.Position = new Vector2(400, 115);
			sortAlphaCheck.IsChecked = propertyGrid.SortAlphabetically;
			FUI.AddControl(sortAlphaCheck);

			// Refresh button
			Button refreshBtn = new Button();
			refreshBtn.Text = "Refresh";
			refreshBtn.Position = new Vector2(400, 150);
			refreshBtn.Size = new Vector2(120, 30);
			refreshBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				propertyGrid.GroupByCategory = groupByCategoryCheck.IsChecked;
				propertyGrid.SortAlphabetically = sortAlphaCheck.IsChecked;
				propertyGrid.RebuildPropertyList();
				statusLabel.Text = "Property list refreshed";
			};
			FUI.AddControl(refreshBtn);

			// === Current values display ===
			Label valuesLabel = new Label("Current Values:");
			valuesLabel.Position = new Vector2(400, 200);
			valuesLabel.Alignment = Align.Left;
			FUI.AddControl(valuesLabel);

			// Values will be shown in a panel
			Panel valuesPanel = new Panel();
			valuesPanel.Position = new Vector2(400, 225);
			valuesPanel.Size = new Vector2(200, 260);
			valuesPanel.Variant = PanelVariant.Dark;
			FUI.AddControl(valuesPanel);

			// Add labels for some key values
			nameValueLabel = new Label($"Name: {sampleObject.Name}");
			nameValueLabel.Position = new Vector2(10, 10);
			nameValueLabel.Alignment = Align.Left;
			valuesPanel.AddChild(nameValueLabel);

			enabledValueLabel = new Label($"Enabled: {sampleObject.Enabled}");
			enabledValueLabel.Position = new Vector2(10, 35);
			enabledValueLabel.Alignment = Align.Left;
			valuesPanel.AddChild(enabledValueLabel);

			countValueLabel = new Label($"Count: {sampleObject.Count}");
			countValueLabel.Position = new Vector2(10, 60);
			countValueLabel.Alignment = Align.Left;
			valuesPanel.AddChild(countValueLabel);

			speedValueLabel = new Label($"Speed: {sampleObject.Speed:F2}");
			speedValueLabel.Position = new Vector2(10, 85);
			speedValueLabel.Alignment = Align.Left;
			valuesPanel.AddChild(speedValueLabel);

			alignValueLabel = new Label($"Alignment: {sampleObject.TextAlignment}");
			alignValueLabel.Position = new Vector2(10, 110);
			alignValueLabel.Alignment = Align.Left;
			valuesPanel.AddChild(alignValueLabel);
		}

		private void PropertyGrid_OnPropertyValueChanged(PropertyGrid sender, PropertyGridItem item, object oldValue, object newValue)
		{
			statusLabel.Text = $"Changed '{item.Name}': {oldValue} -> {newValue}";

			// Update the values display
			UpdateValuesDisplay();
		}

		private void UpdateValuesDisplay()
		{
			if (nameValueLabel != null)
				nameValueLabel.Text = $"Name: {sampleObject.Name}";

			if (enabledValueLabel != null)
				enabledValueLabel.Text = $"Enabled: {sampleObject.Enabled}";

			if (countValueLabel != null)
				countValueLabel.Text = $"Count: {sampleObject.Count}";

			if (speedValueLabel != null)
				speedValueLabel.Text = $"Speed: {sampleObject.Speed:F2}";

			if (alignValueLabel != null)
				alignValueLabel.Text = $"Alignment: {sampleObject.TextAlignment}";
		}

		public void Update(float dt)
		{
		}
	}
}

