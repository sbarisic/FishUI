using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates DropDown control variants: basic, searchable, 
	/// multi-select, and custom item rendering.
	/// </summary>
	public class SampleDropDown : ISample
	{
		FishUI.FishUI FUI;

		public string Name => "DropDown";

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
			Label titleLabel = new Label("DropDown Demo");
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
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(GetType().Name);
			FUI.AddControl(screenshotBtn);

			// === Basic DropDown ===
			Label dropLabel = new Label("Basic DropDown");
			dropLabel.Position = new Vector2(20, 60);
			dropLabel.Alignment = Align.Left;
			FUI.AddControl(dropLabel);

			DropDown dropDown = new DropDown();
			dropDown.Position = new Vector2(20, 85);
			dropDown.Size = new Vector2(150, 30);
			dropDown.TooltipText = "Simple dropdown with options";
			FUI.AddControl(dropDown);

			for (int i = 0; i < 10; i++)
				dropDown.AddItem($"Option {i + 1}");

			// === Searchable DropDown ===
			Label searchDropLabel = new Label("Searchable DropDown");
			searchDropLabel.Position = new Vector2(200, 60);
			searchDropLabel.Alignment = Align.Left;
			FUI.AddControl(searchDropLabel);

			DropDown searchDropDown = new DropDown();
			searchDropDown.Position = new Vector2(200, 85);
			searchDropDown.Size = new Vector2(180, 30);
			searchDropDown.Searchable = true;
			searchDropDown.TooltipText = "Click to open, then type to filter";
			FUI.AddControl(searchDropDown);

			// Add various items to search through
			string[] countries = { "Australia", "Austria", "Belgium", "Brazil", "Canada", 
				"China", "Denmark", "Finland", "France", "Germany", "India", "Italy", 
				"Japan", "Mexico", "Netherlands", "Norway", "Poland", "Spain", "Sweden", 
				"Switzerland", "United Kingdom", "United States" };
			foreach (var country in countries)
				searchDropDown.AddItem(country);

			// === Multi-Select DropDown ===
			Label multiDropLabel = new Label("Multi-Select DropDown");
			multiDropLabel.Position = new Vector2(20, 140);
			multiDropLabel.Alignment = Align.Left;
			FUI.AddControl(multiDropLabel);

			Label multiDropInfo = new Label("Selected: 0");
			multiDropInfo.Position = new Vector2(20, 160);
			multiDropInfo.Size = new Vector2(150, 20);
			multiDropInfo.Alignment = Align.Left;
			FUI.AddControl(multiDropInfo);

			DropDown multiDropDown = new DropDown();
			multiDropDown.Position = new Vector2(20, 185);
			multiDropDown.Size = new Vector2(170, 30);
			multiDropDown.MultiSelect = true;
			multiDropDown.TooltipText = "Click items to toggle selection";
			FUI.AddControl(multiDropDown);

			// Add items for multi-select
			string[] options = { "Option A", "Option B", "Option C", "Option D", "Option E", "Option F" };
			foreach (var opt in options)
				multiDropDown.AddItem(opt);

			// Update info label when selection changes
			multiDropDown.OnMultiSelectionChanged += (dd, indices) =>
			{
				multiDropInfo.Text = $"Selected: {indices.Length}";
			};

			// === Custom Rendered DropDown ===
			Label customDropLabel = new Label("Custom Item Rendering");
			customDropLabel.Position = new Vector2(220, 140);
			customDropLabel.Alignment = Align.Left;
			FUI.AddControl(customDropLabel);

			DropDown customDropDown = new DropDown();
			customDropDown.Position = new Vector2(220, 185);
			customDropDown.Size = new Vector2(180, 30);
			customDropDown.CustomItemHeight = 24;
			customDropDown.TooltipText = "Dropdown with custom colored items";
			FUI.AddControl(customDropDown);

			// Add items with color data
			customDropDown.AddItem(new DropDownItem("Error - Red", new FishColor(220, 60, 60, 255)));
			customDropDown.AddItem(new DropDownItem("Warning - Orange", new FishColor(255, 150, 50, 255)));
			customDropDown.AddItem(new DropDownItem("Success - Green", new FishColor(60, 180, 60, 255)));
			customDropDown.AddItem(new DropDownItem("Info - Blue", new FishColor(60, 120, 220, 255)));
			customDropDown.AddItem(new DropDownItem("Debug - Gray", new FishColor(128, 128, 128, 255)));

			// Set custom renderer that draws colored indicators
			customDropDown.CustomItemRenderer = (ui, item, pos, size, isSelected, isHovered) =>
			{
				// Draw color indicator square
				if (item.UserData is FishColor color)
				{
					ui.Graphics.DrawRectangle(pos + new Vector2(2, 4), new Vector2(16, 16), color);
				}
				// Draw text with offset for the color indicator
				FishColor textColor = isSelected || isHovered ? FishColor.Black : FishColor.Black;
				ui.Graphics.DrawTextColor(ui.Settings.FontDefault, item.Text, pos + new Vector2(22, 4), textColor);
			};
		}
	}
}

