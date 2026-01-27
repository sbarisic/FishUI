using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the MenuBar control with dropdown menus, submenus, and separators.
	/// </summary>
	public class SampleMenuBar : ISample
	{
		FishUI.FishUI FUI;
		Label statusLabel;

		public string Name => "MenuBar";

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
			Label titleLabel = new Label("MenuBar Demo");
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

			// === Menu Bar ===
			MenuBar menuBar = new MenuBar();
			menuBar.Position = new Vector2(20, 60);
			menuBar.Size = new Vector2(600, 24);
			FUI.AddControl(menuBar);

			// File menu
			MenuBarItem fileMenu = menuBar.AddMenu("File");
			var newItem = fileMenu.AddItem("New");
			newItem.ShortcutText = "Ctrl+N";
			newItem.OnClicked += (item) => SetStatus("Clicked: New");

			var openItem = fileMenu.AddItem("Open...");
			openItem.ShortcutText = "Ctrl+O";
			openItem.OnClicked += (item) => SetStatus("Clicked: Open");

			var saveItem = fileMenu.AddItem("Save");
			saveItem.ShortcutText = "Ctrl+S";
			saveItem.OnClicked += (item) => SetStatus("Clicked: Save");

			var saveAsItem = fileMenu.AddItem("Save As...");
			saveAsItem.ShortcutText = "Ctrl+Shift+S";
			saveAsItem.OnClicked += (item) => SetStatus("Clicked: Save As");

			fileMenu.AddSeparator();

			// Recent files submenu
			var recentMenu = fileMenu.AddSubmenu("Recent Files");
			var recent1 = recentMenu.AddItem("Document1.txt");
			recent1.OnClicked += (item) => SetStatus("Open: Document1.txt");
			var recent2 = recentMenu.AddItem("Document2.txt");
			recent2.OnClicked += (item) => SetStatus("Open: Document2.txt");
			var recent3 = recentMenu.AddItem("Document3.txt");
			recent3.OnClicked += (item) => SetStatus("Open: Document3.txt");

			fileMenu.AddSeparator();

			var exitItem = fileMenu.AddItem("Exit");
			exitItem.ShortcutText = "Alt+F4";
			exitItem.OnClicked += (item) => SetStatus("Clicked: Exit");

			// Edit menu
			MenuBarItem editMenu = menuBar.AddMenu("Edit");
			var undoItem = editMenu.AddItem("Undo");
			undoItem.ShortcutText = "Ctrl+Z";
			undoItem.OnClicked += (item) => SetStatus("Clicked: Undo");

			var redoItem = editMenu.AddItem("Redo");
			redoItem.ShortcutText = "Ctrl+Y";
			redoItem.OnClicked += (item) => SetStatus("Clicked: Redo");

			editMenu.AddSeparator();

			var cutItem = editMenu.AddItem("Cut");
			cutItem.ShortcutText = "Ctrl+X";
			cutItem.OnClicked += (item) => SetStatus("Clicked: Cut");

			var copyItem = editMenu.AddItem("Copy");
			copyItem.ShortcutText = "Ctrl+C";
			copyItem.OnClicked += (item) => SetStatus("Clicked: Copy");

			var pasteItem = editMenu.AddItem("Paste");
			pasteItem.ShortcutText = "Ctrl+V";
			pasteItem.OnClicked += (item) => SetStatus("Clicked: Paste");

			editMenu.AddSeparator();

			var selectAllItem = editMenu.AddItem("Select All");
			selectAllItem.ShortcutText = "Ctrl+A";
			selectAllItem.OnClicked += (item) => SetStatus("Clicked: Select All");

			// View menu
			MenuBarItem viewMenu = menuBar.AddMenu("View");
			var showToolbar = viewMenu.AddCheckItem("Toolbar", true);
			showToolbar.OnClicked += (item) => SetStatus($"Toolbar: {item.IsChecked}");

			var showStatusBar = viewMenu.AddCheckItem("Status Bar", true);
			showStatusBar.OnClicked += (item) => SetStatus($"Status Bar: {item.IsChecked}");

			viewMenu.AddSeparator();

			var zoomInItem = viewMenu.AddItem("Zoom In");
			zoomInItem.ShortcutText = "Ctrl++";
			zoomInItem.OnClicked += (item) => SetStatus("Clicked: Zoom In");

			var zoomOutItem = viewMenu.AddItem("Zoom Out");
			zoomOutItem.ShortcutText = "Ctrl+-";
			zoomOutItem.OnClicked += (item) => SetStatus("Clicked: Zoom Out");

			// Help menu
			MenuBarItem helpMenu = menuBar.AddMenu("Help");
			var aboutItem = helpMenu.AddItem("About");
			aboutItem.OnClicked += (item) => SetStatus("Clicked: About");

			var docsItem = helpMenu.AddItem("Documentation");
			docsItem.ShortcutText = "F1";
			docsItem.OnClicked += (item) => SetStatus("Clicked: Documentation");

			// === Status Label ===
			statusLabel = new Label("Click a menu item to see its action here");
			statusLabel.Position = new Vector2(20, 120);
			statusLabel.Size = new Vector2(600, 24);
			statusLabel.Alignment = Align.Left;
			FUI.AddControl(statusLabel);

			// === Description Panel ===
			Panel descPanel = new Panel();
			descPanel.Position = new Vector2(20, 160);
			descPanel.Size = new Vector2(600, 200);
			FUI.AddControl(descPanel);

			Label descLabel = new Label(
				"MenuBar Features:\n" +
				"• Horizontal menu strip with dropdown menus\n" +
				"• Hover to switch between open menus\n" +
				"• Click to toggle menu open/closed\n" +
				"• Keyboard shortcut display\n" +
				"• Separator items for grouping\n" +
				"• Nested submenus (see File > Recent Files)\n" +
				"• Checkable menu items (see View menu)\n" +
				"• Uses existing ContextMenu for dropdowns"
			);
			descLabel.Position = new Vector2(10, 10);
			descLabel.Size = new Vector2(580, 180);
			descLabel.Alignment = Align.Left;
			descPanel.AddChild(descLabel);
		}

		private void SetStatus(string message)
		{
			if (statusLabel != null)
			{
				statusLabel.Text = message;
			}
		}

		public void Update(float dt)
		{
		}

		public void Dispose()
		{
		}
	}
}

