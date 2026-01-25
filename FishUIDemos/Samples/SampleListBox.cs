using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates ListBox control variants: basic with alternating colors,
	/// multi-select mode, and custom item rendering.
	/// </summary>
	public class SampleListBox : ISample
	{
		FishUI.FishUI FUI;

		public string Name => "ListBox";

		public TakeScreenshotFunc TakeScreenshot { get; set; }

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
			Label titleLabel = new Label("ListBox Demo");
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

			// === Basic ListBox with Alternating Colors ===
			Label listLabel = new Label("ListBox (Alternating Colors)");
			listLabel.Position = new Vector2(20, 60);
			listLabel.Alignment = Align.Left;
			FUI.AddControl(listLabel);

			ListBox listBox = new ListBox();
			listBox.Position = new Vector2(20, 85);
			listBox.Size = new Vector2(150, 180);
			listBox.AlternatingRowColors = true;
			listBox.EvenRowColor = new FishColor(200, 220, 255, 40);
			listBox.OddRowColor = new FishColor(255, 255, 255, 10);
			listBox.TooltipText = "ListBox with alternating row colors";
			FUI.AddControl(listBox);

			for (int i = 0; i < 15; i++)
				listBox.AddItem($"Item {i + 1}");

			// === Multi-Select ListBox ===
			Label multiSelectLabel = new Label("Multi-Select ListBox");
			multiSelectLabel.Position = new Vector2(200, 60);
			multiSelectLabel.Alignment = Align.Left;
			FUI.AddControl(multiSelectLabel);

			Label multiSelectHint = new Label("Ctrl+click: toggle, Shift+click: range");
			multiSelectHint.Position = new Vector2(200, 80);
			multiSelectHint.Size = new Vector2(180, 16);
			multiSelectHint.Alignment = Align.Left;
			FUI.AddControl(multiSelectHint);

			Label multiSelectInfo = new Label("Selected: 0 items");
			multiSelectInfo.Position = new Vector2(200, 270);
			multiSelectInfo.Size = new Vector2(150, 20);
			multiSelectInfo.Alignment = Align.Left;
			FUI.AddControl(multiSelectInfo);

			ListBox multiSelectListBox = new ListBox();
			multiSelectListBox.Position = new Vector2(200, 100);
			multiSelectListBox.Size = new Vector2(150, 165);
			multiSelectListBox.MultiSelect = true;
			multiSelectListBox.AlternatingRowColors = true;
			multiSelectListBox.TooltipText = "Multi-select enabled";
			FUI.AddControl(multiSelectListBox);

			for (int i = 0; i < 10; i++)
				multiSelectListBox.AddItem($"Entry {i + 1}");

			// Update selection count label when selection changes
			multiSelectListBox.OnItemSelected += (lb, idx, item) =>
			{
				int count = multiSelectListBox.GetSelectedIndices().Length;
				multiSelectInfo.Text = $"Selected: {count} item{(count != 1 ? "s" : "")}";
			};

			// === Custom Rendered ListBox ===
			Label customListLabel = new Label("Custom ListBox Items");
			customListLabel.Position = new Vector2(380, 60);
			customListLabel.Alignment = Align.Left;
			FUI.AddControl(customListLabel);

			ListBox customListBox = new ListBox();
			customListBox.Position = new Vector2(380, 85);
			customListBox.Size = new Vector2(180, 180);
			customListBox.CustomItemHeight = 28;
			customListBox.ShowScrollBar = true;
			customListBox.TooltipText = "ListBox with custom item rendering";
			FUI.AddControl(customListBox);

			// Add items with priority data
			customListBox.AddItem(new ListBoxItem("Critical Issue", 1));
			customListBox.AddItem(new ListBoxItem("High Priority", 2));
			customListBox.AddItem(new ListBoxItem("Medium Task", 3));
			customListBox.AddItem(new ListBoxItem("Low Priority", 4));
			customListBox.AddItem(new ListBoxItem("Backlog Item", 5));

			// Set custom renderer that draws priority indicators
			customListBox.CustomItemRenderer = (ui, item, index, pos, size, isSelected, isHovered) =>
			{
				// Draw priority indicator based on UserData
				FishColor priorityColor = new FishColor(128, 128, 128, 255);
				if (item.UserData is int priority)
				{
					priorityColor = priority switch
					{
						1 => new FishColor(220, 50, 50, 255),   // Critical - Red
						2 => new FishColor(255, 150, 50, 255),  // High - Orange
						3 => new FishColor(220, 200, 50, 255),  // Medium - Yellow
						4 => new FishColor(100, 180, 100, 255), // Low - Green
						_ => new FishColor(128, 128, 128, 255)  // Backlog - Gray
					};
				}
				ui.Graphics.DrawRectangle(pos + new Vector2(2, 6), new Vector2(12, 12), priorityColor);

				// Draw text with offset
				FishColor textColor = isSelected ? FishColor.White : FishColor.Black;
				ui.Graphics.DrawTextColor(ui.Settings.FontDefault, item.Text, pos + new Vector2(18, 6), textColor);
			};
		}
	}
}
