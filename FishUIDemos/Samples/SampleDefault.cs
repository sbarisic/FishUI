using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// A comprehensive demo showing windows, dialogs, tabs, treeview, context menus, and serialization.
	/// For focused demos, see: SampleBasicControls, SampleButtonVariants, SampleLayoutSystem.
	/// </summary>
	public class SampleDefault : ISample
	{
		FishUI.FishUI FUI;

		public string Name => "Windows & Dialogs";

		public TakeScreenshotFunc TakeScreenshot { get; set; }

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			UISettings.DebugEnabled = true;
			FishUITheme theme = UISettings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);

			return FUI;
		}

		public void Init()
		{
			// === Title & Screenshot ===
			Label titleLabel = new Label("Windows & Dialogs Demo");
			titleLabel.Position = new Vector2(20, 20);
			titleLabel.Size = new Vector2(300, 30);
			titleLabel.Alignment = Align.Left;
			FUI.AddControl(titleLabel);

			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			Button screenshotBtn = new Button();
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.Position = new Vector2(330, 20);
			screenshotBtn.Size = new Vector2(30, 30);
			screenshotBtn.IsImageButton = true;
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(GetType().Name);
			FUI.AddControl(screenshotBtn);

			// === Main Window ===
			Window mainWindow = new Window("Controls Demo");
			mainWindow.Position = new Vector2(20, 60);
			mainWindow.Size = new Vector2(350, 200);
			mainWindow.ShowCloseButton = false;
			FUI.AddControl(mainWindow);

			Button saveBtn = new Button();
			saveBtn.ID = "savelayout";
			saveBtn.Text = "Save Layout";
			saveBtn.Position = new Vector2(10, 10);
			saveBtn.Size = new Vector2(120, 35);
			mainWindow.AddChild(saveBtn);

			Button loadBtn = new Button();
			loadBtn.ID = "loadlayout";
			loadBtn.Text = "Load Layout";
			loadBtn.Position = new Vector2(140, 10);
			loadBtn.Size = new Vector2(120, 35);
			mainWindow.AddChild(loadBtn);

			CheckBox checkBox = new CheckBox("Enable feature");
			checkBox.Position = new Vector2(10, 55);
			mainWindow.AddChild(checkBox);

			ToggleSwitch toggle = new ToggleSwitch();
			toggle.Position = new Vector2(10, 85);
			toggle.Size = new Vector2(60, 24);
			toggle.ShowLabels = true;
			mainWindow.AddChild(toggle);

			// === Draggable Window ===
			Window window1 = new Window("Draggable Window");
			window1.Position = new Vector2(400, 60);
			window1.Size = new Vector2(300, 200);
			window1.ZDepth = 10;
			window1.OnClosing += (sender, e) => Console.WriteLine("Window closing...");
			window1.OnClosed += (w) => Console.WriteLine("Window closed");
			FUI.AddControl(window1);

			Label windowLabel = new Label("This window is draggable and resizable.\nTry moving it around!");
			windowLabel.Position = new Vector2(10, 10);
			windowLabel.Size = new Vector2(280, 50);
			windowLabel.Alignment = Align.Left;
			window1.AddChild(windowLabel);

			Button windowBtn = new Button();
			windowBtn.Text = "Click Me";
			windowBtn.Position = new Vector2(10, 70);
			windowBtn.Size = new Vector2(100, 30);
			windowBtn.OnButtonPressed += (btn, mbtn, pos) => Console.WriteLine("Window button clicked!");
			window1.AddChild(windowBtn);

			// === Modal Dialog ===
			Window dialog = new Window("Confirm");
			dialog.Position = new Vector2(720, 60);
			dialog.Size = new Vector2(220, 130);
			dialog.ZDepth = 20;
			dialog.IsResizable = false;
			dialog.IsModal = true;
			FUI.AddControl(dialog);

			Label dialogLabel = new Label("Are you sure?");
			dialogLabel.Position = new Vector2(15, 15);
			dialogLabel.Alignment = Align.Left;
			dialog.AddChild(dialogLabel);

			Button okBtn = new Button();
			okBtn.Text = "OK";
			okBtn.Position = new Vector2(15, 50);
			okBtn.Size = new Vector2(80, 30);
			okBtn.OnButtonPressed += (btn, mbtn, pos) => dialog.Close();
			dialog.AddChild(okBtn);

			Button cancelBtn = new Button();
			cancelBtn.Text = "Cancel";
			cancelBtn.Position = new Vector2(105, 50);
			cancelBtn.Size = new Vector2(80, 30);
			cancelBtn.OnButtonPressed += (btn, mbtn, pos) => dialog.Close();
			dialog.AddChild(cancelBtn);

			// === TabControl ===
			TabControl tabControl = new TabControl();
			tabControl.Position = new Vector2(20, 280);
			tabControl.Size = new Vector2(400, 200);
			FUI.AddControl(tabControl);

			TabPage tab1 = tabControl.AddTab("General");
			Label tab1Label = new Label("General settings go here");
			tab1Label.Position = new Vector2(10, 10);
			tab1Label.Alignment = Align.Left;
			tab1.Content.AddChild(tab1Label);

			TabPage tab2 = tabControl.AddTab("Settings");
			CheckBox tab2Check = new CheckBox("Enable feature");
			tab2Check.Position = new Vector2(10, 10);
			tab2.Content.AddChild(tab2Check);

			TabPage tab3 = tabControl.AddTab("About");
			Label tab3Label = new Label("FishUI - A lightweight UI framework");
			tab3Label.Position = new Vector2(10, 10);
			tab3Label.Alignment = Align.Left;
			tab3.Content.AddChild(tab3Label);

			// === GroupBox ===
			GroupBox groupBox = new GroupBox("Options");
			groupBox.Position = new Vector2(450, 280);
			groupBox.Size = new Vector2(180, 120);
			FUI.AddControl(groupBox);

			CheckBox opt1 = new CheckBox("Option 1");
			opt1.Position = new Vector2(10, 25);
			groupBox.AddChild(opt1);

			CheckBox opt2 = new CheckBox("Option 2");
			opt2.Position = new Vector2(10, 50);
			opt2.IsChecked = true;
			groupBox.AddChild(opt2);

			CheckBox opt3 = new CheckBox("Option 3");
			opt3.Position = new Vector2(10, 75);
			groupBox.AddChild(opt3);

			// === TreeView ===
			TreeView treeView = new TreeView();
			treeView.Position = new Vector2(650, 280);
			treeView.Size = new Vector2(200, 200);
			FUI.AddControl(treeView);

			TreeNode docs = treeView.AddNode("Documents");
			docs.AddChild("Resume.docx");
			docs.AddChild("Report.pdf");
			TreeNode projects = docs.AddChild("Projects");
			projects.AddChild("FishUI");
			projects.AddChild("GameEngine");
			docs.IsExpanded = true;

			TreeNode pics = treeView.AddNode("Pictures");
			pics.AddChild("Vacation");
			pics.AddChild("Screenshots");

			treeView.OnNodeSelected += (tv, node) => Console.WriteLine($"Selected: {node.Text}");

			// === Context Menu ===
			ContextMenu contextMenu = new ContextMenu();
			FUI.AddControl(contextMenu);

			MenuItem newItem = contextMenu.AddItem("New");
			newItem.ShortcutText = "Ctrl+N";
			newItem.OnClicked += (item) => Console.WriteLine("New clicked");

			MenuItem openItem = contextMenu.AddItem("Open");
			openItem.ShortcutText = "Ctrl+O";

			MenuItem saveItem = contextMenu.AddItem("Save");
			saveItem.ShortcutText = "Ctrl+S";

			contextMenu.AddSeparator();

			MenuItem showGrid = contextMenu.AddCheckItem("Show Grid", true);
			MenuItem snapGrid = contextMenu.AddCheckItem("Snap to Grid", false);

			contextMenu.AddSeparator();

			MenuItem viewSubmenu = contextMenu.AddSubmenu("View");
			viewSubmenu.AddItem("Zoom In");
			viewSubmenu.AddItem("Zoom Out");
			viewSubmenu.AddItem("Reset");

			Button showMenuBtn = new Button();
			showMenuBtn.Text = "Show Context Menu";
			showMenuBtn.Position = new Vector2(20, 500);
			showMenuBtn.Size = new Vector2(150, 30);
			showMenuBtn.OnButtonPressed += (btn, mbtn, pos) => contextMenu.Show(pos + new Vector2(0, 35));
			FUI.AddControl(showMenuBtn);

			Label menuHint = new Label("Right-click anywhere for context menu");
			menuHint.Position = new Vector2(180, 505);
			menuHint.Size = new Vector2(300, 20);
			menuHint.Alignment = Align.Left;
			FUI.AddControl(menuHint);
		}
	}
}

