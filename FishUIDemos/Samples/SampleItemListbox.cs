using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the ItemListbox control with custom widget items.
	/// Shows text items, widget items (buttons, checkboxes, progress bars), and mixed content.
	/// </summary>
	public class SampleItemListbox : ISample
	{
		FishUI.FishUI FUI;

		public string Name => "ItemListbox";

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
			Label titleLabel = new Label("ItemListbox Demo");
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

			// === Description ===
			Label descLabel = new Label("ItemListbox supports text items and custom widget controls as items.");
			descLabel.Position = new Vector2(20, 55);
			descLabel.Size = new Vector2(600, 20);
			descLabel.Alignment = Align.Left;
			FUI.AddControl(descLabel);

			// === Text-Only ItemListbox ===
			Label textListLabel = new Label("Text Items:");
			textListLabel.Position = new Vector2(20, 90);
			textListLabel.Size = new Vector2(150, 20);
			textListLabel.Alignment = Align.Left;
			FUI.AddControl(textListLabel);

			ItemListbox textListbox = new ItemListbox();
			textListbox.Position = new Vector2(20, 115);
			textListbox.Size = new Vector2(180, 200);
			textListbox.AlternatingRowColors = true;

			// Add simple text items
			textListbox.AddItem("First Item");
			textListbox.AddItem("Second Item");
			textListbox.AddItem("Third Item");
			textListbox.AddItem("Fourth Item");
			textListbox.AddItem("Fifth Item");
			textListbox.AddItem("Sixth Item");
			textListbox.AddItem("Seventh Item");
			textListbox.AddItem("Eighth Item");

			textListbox.OnItemSelected += (lb, idx, item) =>
			{
				Console.WriteLine($"Text list selected: {item.Text}");
			};

			FUI.AddControl(textListbox);

			// === Widget ItemListbox ===
			Label widgetListLabel = new Label("Widget Items:");
			widgetListLabel.Position = new Vector2(220, 90);
			widgetListLabel.Size = new Vector2(150, 20);
			widgetListLabel.Alignment = Align.Left;
			FUI.AddControl(widgetListLabel);

			ItemListbox widgetListbox = new ItemListbox();
			widgetListbox.Position = new Vector2(220, 115);
			widgetListbox.Size = new Vector2(220, 200);
			widgetListbox.ItemHeight = 30;

			// Add button widget items
			for (int i = 1; i <= 5; i++)
			{
				Button btn = new Button();
				btn.Text = $"Action Button {i}";
				btn.Size = new Vector2(180, 26);
				int idx = i;
				btn.OnButtonPressed += (b, mb, pos) => Console.WriteLine($"Button {idx} clicked!");
				widgetListbox.AddItem(btn, $"button_{i}");
			}

			widgetListbox.OnItemSelected += (lb, idx, item) =>
			{
				Console.WriteLine($"Widget list selected index: {idx}, UserData: {item.UserData}");
			};

			FUI.AddControl(widgetListbox);

			// === Mixed ItemListbox ===
			Label mixedListLabel = new Label("Mixed Items:");
			mixedListLabel.Position = new Vector2(460, 90);
			mixedListLabel.Size = new Vector2(150, 20);
			mixedListLabel.Alignment = Align.Left;
			FUI.AddControl(mixedListLabel);

			ItemListbox mixedListbox = new ItemListbox();
			mixedListbox.Position = new Vector2(460, 115);
			mixedListbox.Size = new Vector2(250, 250);
			mixedListbox.ItemHeight = 28;
			mixedListbox.AlternatingRowColors = true;

		// Text item
			mixedListbox.AddItem("Header: Settings");

			// Checkbox widget
			CheckBox chk1 = new CheckBox("Enable Feature A");
			chk1.Size = new Vector2(200, 24);
			mixedListbox.AddItem(chk1);

			// Another checkbox
			CheckBox chk2 = new CheckBox("Enable Feature B");
			chk2.Size = new Vector2(200, 24);
			chk2.IsChecked = true;
			mixedListbox.AddItem(chk2);

			// Text separator
			mixedListbox.AddItem("Header: Progress");

			// Progress bar widget
			ProgressBar progress1 = new ProgressBar();
			progress1.Size = new Vector2(200, 20);
			progress1.Value = 0.75f;
			mixedListbox.AddItem(new ItemListboxItem(progress1) { Height = 24 });

			// Another progress bar
			ProgressBar progress2 = new ProgressBar();
			progress2.Size = new Vector2(200, 20);
			progress2.Value = 0.45f;
			mixedListbox.AddItem(new ItemListboxItem(progress2) { Height = 24 });

			// Text separator
			mixedListbox.AddItem("Header: Actions");

			// Button widget
			Button actionBtn = new Button();
			actionBtn.Text = "Apply Settings";
			actionBtn.Size = new Vector2(200, 24);
			actionBtn.OnButtonPressed += (b, mb, pos) => Console.WriteLine("Settings applied!");
			mixedListbox.AddItem(actionBtn);

			// Slider widget
			Slider slider = new Slider();
			slider.Size = new Vector2(200, 20);
			slider.Value = 0.5f;
			mixedListbox.AddItem(new ItemListboxItem(slider) { Height = 24 });

			mixedListbox.OnItemSelected += (lb, idx, item) =>
			{
				string desc = item.Widget != null ? $"Widget: {item.Widget.GetType().Name}" : $"Text: {item.Text}";
				Console.WriteLine($"Mixed list selected index {idx}: {desc}");
			};

			FUI.AddControl(mixedListbox);

			// === Selection Info Label ===
			Label infoLabel = new Label("Click items to select. Check console for selection events.");
			infoLabel.Position = new Vector2(20, 380);
			infoLabel.Size = new Vector2(600, 20);
			infoLabel.Alignment = Align.Left;
			FUI.AddControl(infoLabel);
		}
	}
}

