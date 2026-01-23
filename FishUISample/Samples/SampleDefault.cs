using FishUI;
using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FishUISample.Samples
{
	internal class SampleDefault : ISample
	{
		FishUI.FishUI FUI;

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			// Enable debug logging to diagnose theme loading
			UISettings.DebugEnabled = true;

			FishUITheme theme = UISettings.LoadTheme("data/themes/gwen.yaml", applyImmediately: false);
			UISettings.ApplyTheme(theme);
			UISettings.OnThemeChanged += (theme) => { /* handle theme change */ };
			FishUIColorPalette colors = UISettings.GetColorPalette();

			return FUI;
		}

        public void Init()
        {
			Textbox Lbl = new Textbox("The quick");
			Lbl.Position = new Vector2(100, 400);
			Lbl.ZDepth = 2;
			FUI.AddControl(Lbl);

			ListBox Lb = new ListBox();
			Lb.Position = new Vector2(320, 350);
			Lb.ZDepth = 2;
			Lb.ID = "listbox1";
			FUI.AddControl(Lb);

			for (int i = 0; i < 10; i++)
			{
				Lb.AddItem("Item " + i);
			}

			//Lb.AutoResizeHeight();

			DropDown DD = new DropDown();
			DD.Position = new Vector2(550, 350);
			DD.ZDepth = 2;
			DD.ID = "dropdown1";
			FUI.AddControl(DD);

			for (int i = 0; i < 10; i++)
			{
				DD.AddItem("Option " + i);
			}

			ScrollBarV Sbv = new ScrollBarV();
			Sbv.Position = new Vector2(480, 340);
			Sbv.ZDepth = 2;
			FUI.AddControl(Sbv);

			ScrollBarH Sbh = new ScrollBarH();
			Sbh.Position = new Vector2(260, 540);
			Sbh.ZDepth = 2;
			FUI.AddControl(Sbh);

			// Determinate progress bar
			ProgressBar progressBar = new ProgressBar();
			progressBar.Position = new Vector2(260, 580);
			progressBar.Size = new Vector2(200, 20);
			progressBar.Value = 0.75f; // 75% complete
			FUI.AddControl(progressBar);

			// Indeterminate/marquee progress bar
			ProgressBar loadingBar = new ProgressBar();
			loadingBar.Position = new Vector2(260, 620);
			loadingBar.IsIndeterminate = true;
			FUI.AddControl(loadingBar);

			// Toggle switch
			ToggleSwitch toggle1 = new ToggleSwitch();
			toggle1.Position = new Vector2(260, 660);
			toggle1.Size = new Vector2(50, 24);
			toggle1.IsOn = false;
			FUI.AddControl(toggle1);

			// Toggle switch with labels
			ToggleSwitch toggle2 = new ToggleSwitch();
			toggle2.Position = new Vector2(320, 660);
			toggle2.Size = new Vector2(70, 24);
			toggle2.IsOn = true;
			toggle2.ShowLabels = true;
			FUI.AddControl(toggle2);

			// Horizontal slider
			Slider slider1 = new Slider();
			slider1.Position = new Vector2(260, 700);
			slider1.Size = new Vector2(200, 24);
			slider1.MinValue = 0;
			slider1.MaxValue = 100;
			slider1.Value = 50;
			slider1.ShowValueLabel = true;
			FUI.AddControl(slider1);

			// Vertical slider
			Slider slider2 = new Slider();
			slider2.Position = new Vector2(500, 540);
			slider2.Size = new Vector2(24, 150);
			slider2.Orientation = SliderOrientation.Vertical;
			slider2.MinValue = 0;
			slider2.MaxValue = 100;
			slider2.Step = 10; // Snap to 10s
			slider2.Value = 30;
			FUI.AddControl(slider2);

			// NumericUpDown / Spinner
			NumericUpDown numUpDown1 = new NumericUpDown(50, 0, 100, 1);
			numUpDown1.Position = new Vector2(260, 740);
			numUpDown1.Size = new Vector2(120, 24);
			FUI.AddControl(numUpDown1);

			// NumericUpDown with decimal places
			NumericUpDown numUpDown2 = new NumericUpDown(0.5f, 0f, 1f, 0.1f);
			numUpDown2.Position = new Vector2(400, 740);
			numUpDown2.Size = new Vector2(120, 24);
			numUpDown2.DecimalPlaces = 1;
			FUI.AddControl(numUpDown2);


			Button Btn0 = new Button();
			Btn0.ID = "visible";
			Btn0.Text = "Make Visible";
			Btn0.Position = new Vector2(430, 100);
			Btn0.Size = new Vector2(150, 50);
			Btn0.ZDepth = 2;
			FUI.AddControl(Btn0);

			Button Btn1 = new Button();
			Btn1.ID = "savelayout";
			Btn1.Text = "Save Layout";
			Btn1.Position = new Vector2(430, 160);
			Btn1.Size = new Vector2(150, 50);
			Btn1.ZDepth = 2;
			FUI.AddControl(Btn1);

			Button Btn2 = new Button();
			Btn2.ID = "loadlayout";
			Btn2.Text = "Load Layout";
			Btn2.Position = new Vector2(430, 220);
			Btn2.Size = new Vector2(150, 50);
			Btn2.ZDepth = 2;
			FUI.AddControl(Btn2);

			// Main Window (replaces Panel) ------------------
			Window Pnl = new Window("Controls Demo");
			Pnl.ID = "panel1";
			Pnl.Position = new Vector2(10, 10);
			Pnl.Size = new Vector2(400, 350);
			Pnl.ZDepth = 1;
			Pnl.ShowCloseButton = false; // Disable close button
			FUI.AddControl(Pnl);

			Button Btn = new Button();
			Btn.ID = "invisible";
			Btn.Text = "Make Invisible";
			//Btn.Position = new Vector2(100, 100);
			Btn.Position = new FishUIPosition(PositionMode.Docked, DockMode.Horizontal, new Vector4(15, 0, 15, 0), new Vector2(100, 100));
			Btn.Size = new Vector2(150, 50);
			Btn.TooltipText = "Click to hide the panel";
			Pnl.AddChild(Btn);

			CheckBox CBox = new CheckBox("Checkbox");
			CBox.Position = new Vector2(5, 10);
			CBox.Size = new Vector2(15, 15);
			CBox.TooltipText = "Toggle this option";
			Pnl.AddChild(CBox);

			RadioButton RBut = new RadioButton("Radio button");
			RBut.Position = new Vector2(5, 40);
			RBut.Size = new Vector2(15, 15);
			Pnl.AddChild(RBut);

			// === NEW CONTROLS DEMO ===

			// Window / Dialog demo
			Window window1 = new Window("Sample Window");
			window1.Position = new Vector2(600, 100);
			window1.Size = new Vector2(350, 250);
			window1.ZDepth = 10;
			window1.OnClosing += (sender, e) =>
			{
				Console.WriteLine("Window closing...");
				// Set e.Cancel = true to prevent closing
			};
			window1.OnClosed += (w) => Console.WriteLine("Window closed");
			FUI.AddControl(window1);

			// Add a button inside the window
			Button windowBtn = new Button();
			windowBtn.Text = "Window Button";
			windowBtn.Position = new Vector2(10, 10);
			windowBtn.Size = new Vector2(120, 30);
			window1.AddChild(windowBtn);

			// Add a label inside the window
			Label windowLabel = new Label("This is a draggable, resizable window!");
			windowLabel.Position = new Vector2(10, 50);
			windowLabel.Alignment = Align.Left;
			window1.AddChild(windowLabel);

			// TabControl demo
			TabControl tabControl = new TabControl();
			tabControl.Position = new Vector2(600, 380);
			tabControl.Size = new Vector2(400, 250);
			tabControl.ZDepth = 5;
			FUI.AddControl(tabControl);

			// Add tabs
			TabPage tab1 = tabControl.AddTab("General");
			TabPage tab2 = tabControl.AddTab("Settings");
			TabPage tab3 = tabControl.AddTab("About");

			// Add content to first tab
			Button tab1Btn = new Button();
			tab1Btn.Text = "Tab 1 Button";
			tab1Btn.Position = new Vector2(10, 10);
			tab1Btn.Size = new Vector2(100, 30);
			tab1.Content.AddChild(tab1Btn);

			Label tab1Label = new Label("Content of the General tab");
			tab1Label.Position = new Vector2(10, 50);
			tab1Label.Alignment = Align.Left;
			tab1.Content.AddChild(tab1Label);

			// Add content to second tab
			CheckBox tab2Check = new CheckBox("Enable feature");
			tab2Check.Position = new Vector2(10, 10);
			tab2.Content.AddChild(tab2Check);

			ToggleSwitch tab2Toggle = new ToggleSwitch();
			tab2Toggle.Position = new Vector2(10, 40);
			tab2Toggle.Size = new Vector2(50, 24);
			tab2.Content.AddChild(tab2Toggle);

			// Add content to third tab
			Label tab3Label = new Label("FishUI - A lightweight UI framework\nVersion 1.0");
			tab3Label.Position = new Vector2(10, 10);
			tab3Label.Alignment = Align.Left;
			tab3Label.Size = new Vector2(380, 50);
			tab3.Content.AddChild(tab3Label);

			// GroupBox demo
			GroupBox groupBox = new GroupBox("Options");
			groupBox.Position = new Vector2(1050, 100);
			groupBox.Size = new Vector2(200, 120);
			groupBox.ZDepth = 5;
			FUI.AddControl(groupBox);

			CheckBox groupCheck1 = new CheckBox("Option 1");
			groupCheck1.Position = new Vector2(10, 25);
			groupBox.AddChild(groupCheck1);

			CheckBox groupCheck2 = new CheckBox("Option 2");
			groupCheck2.Position = new Vector2(10, 50);
			groupBox.AddChild(groupCheck2);

			// TreeView demo
			TreeView treeView = new TreeView();
			treeView.Position = new Vector2(1300, 100);
			treeView.Size = new Vector2(250, 300);
			treeView.ZDepth = 5;
			FUI.AddControl(treeView);

			// Build a sample tree structure
			TreeNode documentsNode = treeView.AddNode("Documents");
			documentsNode.AddChild("Resume.docx");
			documentsNode.AddChild("Report.pdf");
			TreeNode projectsNode = documentsNode.AddChild("Projects");
			projectsNode.AddChild("FishUI");
			projectsNode.AddChild("GameEngine");
			projectsNode.AddChild("WebApp");
			documentsNode.IsExpanded = true;

			TreeNode picturesNode = treeView.AddNode("Pictures");
			picturesNode.AddChild("Vacation");
			picturesNode.AddChild("Family");
			picturesNode.AddChild("Screenshots");

			TreeNode musicNode = treeView.AddNode("Music");
			TreeNode rockNode = musicNode.AddChild("Rock");
			rockNode.AddChild("Album 1");
			rockNode.AddChild("Album 2");
			musicNode.AddChild("Jazz");
			musicNode.AddChild("Classical");

			TreeNode settingsNode = treeView.AddNode("Settings");
			settingsNode.AddChild("Display");
			settingsNode.AddChild("Audio");
			settingsNode.AddChild("Network");

			// Handle selection events
			treeView.OnNodeSelected += (tv, node) =>
			{
				Console.WriteLine($"Selected: {node.Text}");
			};

			treeView.OnNodeExpandedChanged += (tv, node, expanded) =>
			{
				Console.WriteLine($"{node.Text} {(expanded ? "expanded" : "collapsed")}");
			};

			CheckBox groupCheck3 = new CheckBox("Option 3");
			groupCheck3.Position = new Vector2(10, 75);
			groupBox.AddChild(groupCheck3);

			// Second Window (modal example - not blocking in demo)
			Window window2 = new Window("Dialog");
			window2.Position = new Vector2(1050, 250);
			window2.Size = new Vector2(250, 150);
			window2.ZDepth = 15;
			window2.IsResizable = false; // Fixed size dialog
			window2.IsModal = true;
			FUI.AddControl(window2);

			Label dialogLabel = new Label("Are you sure?");
			dialogLabel.Position = new Vector2(20, 20);
			dialogLabel.Alignment = Align.Left;
			window2.AddChild(dialogLabel);

			Button dialogOk = new Button();
			dialogOk.Text = "OK";
			dialogOk.Position = new Vector2(20, 60);
			dialogOk.Size = new Vector2(80, 30);
			dialogOk.OnButtonPressed += (btn, mbtn, pos) => window2.Close();
			window2.AddChild(dialogOk);

			Button dialogCancel = new Button();
			dialogCancel.Text = "Cancel";
			dialogCancel.Position = new Vector2(110, 60);
			dialogCancel.Size = new Vector2(80, 30);
			dialogCancel.OnButtonPressed += (btn, mbtn, pos) => window2.Close();
			window2.AddChild(dialogCancel);

			// === ContextMenu Demo ===
			ContextMenu contextMenu = new ContextMenu();
			FUI.AddControl(contextMenu);

			// Add menu items
			MenuItem newItem = contextMenu.AddItem("New");
			newItem.ShortcutText = "Ctrl+N";
			newItem.OnClicked += (item) => Console.WriteLine("New clicked");

			MenuItem openItem = contextMenu.AddItem("Open");
			openItem.ShortcutText = "Ctrl+O";
			openItem.OnClicked += (item) => Console.WriteLine("Open clicked");

			MenuItem saveItem = contextMenu.AddItem("Save");
			saveItem.ShortcutText = "Ctrl+S";
			saveItem.OnClicked += (item) => Console.WriteLine("Save clicked");

			contextMenu.AddSeparator();

			// Checkable menu item
			MenuItem showGridItem = contextMenu.AddCheckItem("Show Grid", true);
			showGridItem.OnClicked += (item) => Console.WriteLine($"Show Grid: {item.IsChecked}");

			MenuItem snapToGridItem = contextMenu.AddCheckItem("Snap to Grid", false);
			snapToGridItem.OnClicked += (item) => Console.WriteLine($"Snap to Grid: {item.IsChecked}");

			contextMenu.AddSeparator();

			// Submenu
			// TODO: This seems like a bug? Multiple levels of submenu draw all the text overlapping
			MenuItem viewSubmenu = contextMenu.AddSubmenu("View");
			viewSubmenu.AddItem("Zoom In").OnClicked += (item) => Console.WriteLine("Zoom In");
			viewSubmenu.AddItem("Zoom Out").OnClicked += (item) => Console.WriteLine("Zoom Out");
			viewSubmenu.AddItem("Reset Zoom").OnClicked += (item) => Console.WriteLine("Reset Zoom");

			contextMenu.AddSeparator();

			MenuItem exitItem = contextMenu.AddItem("Exit");
			exitItem.OnClicked += (item) => Console.WriteLine("Exit clicked");

			// Show context menu on panel right-click
			Pnl.OnDragged += (sender, delta) => { }; // Keep existing behavior
			// Hook into panel to show context menu on right-click
			// Note: Right-click on the panel area (10,10 to 410,360) will show context menu

			// Create a label to instruct users
			Label contextMenuLabel = new Label("Right-click on panel for context menu");
			contextMenuLabel.Position = new Vector2(10, 360);
			contextMenuLabel.Size = new Vector2(400, 20);
			contextMenuLabel.Alignment = Align.Left;
			FUI.AddControl(contextMenuLabel);

			// Handle right-click via button (as demonstration)
			Button showMenuBtn = new Button();
			showMenuBtn.ID = "showcontextmenu";
			showMenuBtn.Text = "Show Context Menu";
			showMenuBtn.Position = new Vector2(260, 780);
			showMenuBtn.Size = new Vector2(150, 30);
			showMenuBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				contextMenu.Show(pos + new Vector2(0, 30));
			};
			FUI.AddControl(showMenuBtn);
		}
    }
}
