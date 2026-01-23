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

		/// <summary>
		/// Action to take a screenshot, set by Program.cs.
		/// </summary>
		public Action TakeScreenshot { get; set; }

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

			// Screenshot button
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");
			Button screenshotBtn = new Button();
			screenshotBtn.ID = "screenshot";
			screenshotBtn.Text = "Screenshot";
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.IconPosition = IconPosition.Left;
			screenshotBtn.Position = new Vector2(430, 280);
			screenshotBtn.Size = new Vector2(150, 50);
			screenshotBtn.ZDepth = 2;
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke();
			FUI.AddControl(screenshotBtn);

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

			// Toggle button demo
			Button toggleBtn = new Button();
			toggleBtn.Text = "Toggle Me";
			toggleBtn.Position = new Vector2(5, 70);
			toggleBtn.Size = new Vector2(100, 30);
			toggleBtn.IsToggleButton = true;
			toggleBtn.TooltipText = "Click to toggle on/off";
			toggleBtn.OnToggled += (btn, toggled) => Console.WriteLine($"Toggle button: {toggled}");
			Pnl.AddChild(toggleBtn);

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

			// === Icon Button Demo ===
			// Load some icons from the silk_icons folder
			ImageRef iconSave = FUI.Graphics.LoadImage("data/silk_icons/disk.png");
			ImageRef iconFolder = FUI.Graphics.LoadImage("data/silk_icons/folder.png");
			ImageRef iconSettings = FUI.Graphics.LoadImage("data/silk_icons/cog.png");
			ImageRef iconHelp = FUI.Graphics.LoadImage("data/silk_icons/help.png");

			// Button with icon on the left (default)
			Button iconBtnLeft = new Button();
			iconBtnLeft.Text = "Save";
			iconBtnLeft.Icon = iconSave;
			iconBtnLeft.IconPosition = IconPosition.Left;
			iconBtnLeft.Position = new Vector2(600, 650);
			iconBtnLeft.Size = new Vector2(100, 30);
			iconBtnLeft.TooltipText = "Icon on left";
			FUI.AddControl(iconBtnLeft);

			// Button with icon on the right
			Button iconBtnRight = new Button();
			iconBtnRight.Text = "Open";
			iconBtnRight.Icon = iconFolder;
			iconBtnRight.IconPosition = IconPosition.Right;
			iconBtnRight.Position = new Vector2(710, 650);
			iconBtnRight.Size = new Vector2(100, 30);
			iconBtnRight.TooltipText = "Icon on right";
			FUI.AddControl(iconBtnRight);

			// Icon-only button (no text)
			Button iconOnlyBtn = new Button();
			iconOnlyBtn.Icon = iconSettings;
			iconOnlyBtn.Position = new Vector2(820, 650);
			iconOnlyBtn.Size = new Vector2(30, 30);
			iconOnlyBtn.TooltipText = "Settings (icon only)";
			FUI.AddControl(iconOnlyBtn);

			// Button with icon on top
			Button iconBtnTop = new Button();
			iconBtnTop.Text = "Help";
			iconBtnTop.Icon = iconHelp;
			iconBtnTop.IconPosition = IconPosition.Top;
			iconBtnTop.Position = new Vector2(860, 640);
			iconBtnTop.Size = new Vector2(60, 50);
			iconBtnTop.TooltipText = "Icon on top";
			FUI.AddControl(iconBtnTop);

			// Label for icon button section
			Label iconBtnLabel = new Label("Icon Buttons Demo");
			iconBtnLabel.Position = new Vector2(600, 625);
			iconBtnLabel.Size = new Vector2(200, 20);
			iconBtnLabel.Alignment = Align.Left;
			FUI.AddControl(iconBtnLabel);

			// === Panel Variants Demo ===
			Label panelVariantsLabel = new Label("Panel Variants & Border Styles");
			panelVariantsLabel.Position = new Vector2(600, 700);
			panelVariantsLabel.Size = new Vector2(300, 20);
			panelVariantsLabel.Alignment = Align.Left;
			FUI.AddControl(panelVariantsLabel);

			// Normal panel
			Panel panelNormal = new Panel();
			panelNormal.Position = new Vector2(600, 720);
			panelNormal.Size = new Vector2(70, 50);
			panelNormal.Variant = PanelVariant.Normal;
			FUI.AddControl(panelNormal);

			Label labelNormal = new Label("Normal");
			labelNormal.Position = new Vector2(5, 15);
			labelNormal.Alignment = Align.Left;
			panelNormal.AddChild(labelNormal);

			// Bright panel
			Panel panelBright = new Panel();
			panelBright.Position = new Vector2(680, 720);
			panelBright.Size = new Vector2(70, 50);
			panelBright.Variant = PanelVariant.Bright;
			FUI.AddControl(panelBright);

			Label labelBright = new Label("Bright");
			labelBright.Position = new Vector2(5, 15);
			labelBright.Alignment = Align.Left;
			panelBright.AddChild(labelBright);

			// Dark panel
			Panel panelDark = new Panel();
			panelDark.Position = new Vector2(760, 720);
			panelDark.Size = new Vector2(70, 50);
			panelDark.Variant = PanelVariant.Dark;
			FUI.AddControl(panelDark);

			Label labelDark = new Label("Dark");
			labelDark.Position = new Vector2(5, 15);
			labelDark.Alignment = Align.Left;
			panelDark.AddChild(labelDark);

			// Highlight panel
			Panel panelHighlight = new Panel();
			panelHighlight.Position = new Vector2(840, 720);
			panelHighlight.Size = new Vector2(80, 50);
			panelHighlight.Variant = PanelVariant.Highlight;
			FUI.AddControl(panelHighlight);

			Label labelHighlight = new Label("Highlight");
			labelHighlight.Position = new Vector2(5, 15);
			labelHighlight.Alignment = Align.Left;
			panelHighlight.AddChild(labelHighlight);

			// Border style demos
			// Solid border
			Panel panelSolidBorder = new Panel();
			panelSolidBorder.Position = new Vector2(600, 780);
			panelSolidBorder.Size = new Vector2(70, 50);
			panelSolidBorder.IsTransparent = true;
			panelSolidBorder.BorderStyle = BorderStyle.Solid;
			panelSolidBorder.BorderColor = new FishColor(100, 100, 100, 255);
			panelSolidBorder.BorderThickness = 2f;
			FUI.AddControl(panelSolidBorder);

			Label labelSolid = new Label("Solid");
			labelSolid.Position = new Vector2(5, 15);
			labelSolid.Alignment = Align.Left;
			panelSolidBorder.AddChild(labelSolid);

			// Inset border
			Panel panelInsetBorder = new Panel();
			panelInsetBorder.Position = new Vector2(680, 780);
			panelInsetBorder.Size = new Vector2(70, 50);
			panelInsetBorder.IsTransparent = true;
			panelInsetBorder.BorderStyle = BorderStyle.Inset;
			panelInsetBorder.BorderColor = new FishColor(150, 150, 150, 255);
			panelInsetBorder.BorderThickness = 2f;
			FUI.AddControl(panelInsetBorder);

			Label labelInset = new Label("Inset");
			labelInset.Position = new Vector2(5, 15);
			labelInset.Alignment = Align.Left;
			panelInsetBorder.AddChild(labelInset);

			// Outset border
			Panel panelOutsetBorder = new Panel();
			panelOutsetBorder.Position = new Vector2(760, 780);
			panelOutsetBorder.Size = new Vector2(70, 50);
			panelOutsetBorder.IsTransparent = true;
			panelOutsetBorder.BorderStyle = BorderStyle.Outset;
			panelOutsetBorder.BorderColor = new FishColor(150, 150, 150, 255);
			panelOutsetBorder.BorderThickness = 2f;
			FUI.AddControl(panelOutsetBorder);

			Label labelOutset = new Label("Outset");
			labelOutset.Position = new Vector2(5, 15);
			labelOutset.Alignment = Align.Left;
			panelOutsetBorder.AddChild(labelOutset);

			// Combined: variant + border
			Panel panelCombined = new Panel();
			panelCombined.Position = new Vector2(840, 780);
			panelCombined.Size = new Vector2(80, 50);
			panelCombined.Variant = PanelVariant.Bright;
			panelCombined.BorderStyle = BorderStyle.Outset;
			panelCombined.BorderColor = new FishColor(120, 120, 180, 255);
			panelCombined.BorderThickness = 2f;
			FUI.AddControl(panelCombined);

			Label labelCombined = new Label("Combined");
			labelCombined.Position = new Vector2(5, 15);
			labelCombined.Alignment = Align.Left;
			panelCombined.AddChild(labelCombined);

			// === Margin & Padding Demo ===
			Label marginPaddingLabel = new Label("Margin & Padding Demo");
			marginPaddingLabel.Position = new Vector2(600, 840);
			marginPaddingLabel.Size = new Vector2(300, 20);
			marginPaddingLabel.Alignment = Align.Left;
			FUI.AddControl(marginPaddingLabel);

			// Panel with padding - children are offset from edges
			Panel paddedPanel = new Panel();
			paddedPanel.Position = new Vector2(600, 860);
			paddedPanel.Size = new Vector2(150, 80);
			paddedPanel.Padding = new FishUIMargin(10); // 10px padding all sides
			paddedPanel.Variant = PanelVariant.Bright;
			FUI.AddControl(paddedPanel);

			// This button will be offset by parent's padding
			Button paddedBtn = new Button();
			paddedBtn.Text = "Padded";
			paddedBtn.Position = new Vector2(0, 0); // Actually placed at (10,10) due to padding
			paddedBtn.Size = new Vector2(80, 25);
			paddedPanel.AddChild(paddedBtn);

			Label paddingDemoLabel = new Label("Padding=10");
			paddingDemoLabel.Position = new Vector2(0, 30);
			paddingDemoLabel.Alignment = Align.Left;
			paddedPanel.AddChild(paddingDemoLabel);

			// Panel with a child that has margin
			Panel marginPanel = new Panel();
			marginPanel.Position = new Vector2(760, 860);
			marginPanel.Size = new Vector2(150, 80);
			marginPanel.Variant = PanelVariant.Normal;
			FUI.AddControl(marginPanel);

			// This button has its own margin
			Button marginBtn = new Button();
			marginBtn.Text = "Margin";
			marginBtn.Position = new Vector2(0, 0);
			marginBtn.Size = new Vector2(80, 25);
			marginBtn.Margin = new FishUIMargin(15, 10, 0, 10); // top, right, bottom, left
			marginPanel.AddChild(marginBtn);

			Label marginDemoLabel = new Label("Margin=10");
			marginDemoLabel.Position = new Vector2(10, 50);
			marginDemoLabel.Alignment = Align.Left;
			marginPanel.AddChild(marginDemoLabel);

			// === Anchor Demo ===
			Label anchorLabel = new Label("Anchor Demo (resize window to test)");
			anchorLabel.Position = new Vector2(920, 840);
			anchorLabel.Size = new Vector2(300, 20);
			anchorLabel.Alignment = Align.Left;
			FUI.AddControl(anchorLabel);

			// Container panel for anchor demos
			Panel anchorContainer = new Panel();
			anchorContainer.Position = new Vector2(920, 860);
			anchorContainer.Size = new Vector2(200, 100);
			anchorContainer.Variant = PanelVariant.Bright;
			anchorContainer.BorderStyle = BorderStyle.Solid;
			anchorContainer.BorderColor = new FishColor(100, 100, 150, 255);
			anchorContainer.BorderThickness = 1f;
			FUI.AddControl(anchorContainer);

			// Button anchored to top-left (default) - stays fixed
			Button anchorTL = new Button();
			anchorTL.Text = "TL";
			anchorTL.Position = new Vector2(5, 5);
			anchorTL.Size = new Vector2(40, 25);
			anchorTL.Anchor = FishUIAnchor.TopLeft;
			anchorTL.TooltipText = "Anchor: TopLeft (default)";
			anchorContainer.AddChild(anchorTL);

			// Button anchored to top-right - moves when parent width changes
			Button anchorTR = new Button();
			anchorTR.Text = "TR";
			anchorTR.Position = new Vector2(155, 5);
			anchorTR.Size = new Vector2(40, 25);
			anchorTR.Anchor = FishUIAnchor.TopRight;
			anchorTR.TooltipText = "Anchor: TopRight";
			anchorContainer.AddChild(anchorTR);

			// Button anchored to bottom-left - moves when parent height changes
			Button anchorBL = new Button();
			anchorBL.Text = "BL";
			anchorBL.Position = new Vector2(5, 70);
			anchorBL.Size = new Vector2(40, 25);
			anchorBL.Anchor = FishUIAnchor.BottomLeft;
			anchorBL.TooltipText = "Anchor: BottomLeft";
			anchorContainer.AddChild(anchorBL);

			// Button anchored to bottom-right - moves when parent size changes
			Button anchorBR = new Button();
			anchorBR.Text = "BR";
			anchorBR.Position = new Vector2(155, 70);
			anchorBR.Size = new Vector2(40, 25);
			anchorBR.Anchor = FishUIAnchor.BottomRight;
			anchorBR.TooltipText = "Anchor: BottomRight";
			anchorContainer.AddChild(anchorBR);

			// Button anchored horizontally - stretches width with parent
			Button anchorH = new Button();
			anchorH.Text = "Horizontal";
			anchorH.Position = new Vector2(50, 35);
			anchorH.Size = new Vector2(100, 25);
			anchorH.Anchor = FishUIAnchor.Horizontal;
			anchorH.TooltipText = "Anchor: Horizontal (stretches)";
			anchorContainer.AddChild(anchorH);

			// === StackLayout Demo ===
			// Vertical stack layout
			StackLayout vStack = new StackLayout();
			vStack.Position = new Vector2(1050, 420);
			vStack.Size = new Vector2(180, 200);
			vStack.Orientation = StackOrientation.Vertical;
			vStack.Spacing = 8;
			vStack.Padding = 10;
			vStack.IsTransparent = false;
			vStack.ZDepth = 5;
			FUI.AddControl(vStack);

			Button stackBtn1 = new Button();
			stackBtn1.Text = "Stack Item 1";
			stackBtn1.Size = new Vector2(150, 30);
			vStack.AddChild(stackBtn1);

			Button stackBtn2 = new Button();
			stackBtn2.Text = "Stack Item 2";
			stackBtn2.Size = new Vector2(150, 30);
			vStack.AddChild(stackBtn2);

			CheckBox stackCheck = new CheckBox("Stack Option");
			stackCheck.Size = new Vector2(15, 15);
			vStack.AddChild(stackCheck);

			ToggleSwitch stackToggle = new ToggleSwitch();
			stackToggle.Size = new Vector2(50, 24);
			vStack.AddChild(stackToggle);

			// Horizontal stack layout
			StackLayout hStack = new StackLayout();
			hStack.Position = new Vector2(1250, 420);
			hStack.Size = new Vector2(300, 50);
			hStack.Orientation = StackOrientation.Horizontal;
			hStack.Spacing = 5;
			hStack.Padding = 5;
			hStack.IsTransparent = false;
			hStack.ZDepth = 5;
			FUI.AddControl(hStack);

			Button hStackBtn1 = new Button();
			hStackBtn1.Text = "A";
			hStackBtn1.Size = new Vector2(40, 35);
			hStack.AddChild(hStackBtn1);

			Button hStackBtn2 = new Button();
			hStackBtn2.Text = "B";
			hStackBtn2.Size = new Vector2(40, 35);
			hStack.AddChild(hStackBtn2);

			Button hStackBtn3 = new Button();
			hStackBtn3.Text = "C";
			hStackBtn3.Size = new Vector2(40, 35);
			hStack.AddChild(hStackBtn3);

			Button hStackBtn4 = new Button();
			hStackBtn4.Text = "D";
			hStackBtn4.Size = new Vector2(40, 35);
			hStack.AddChild(hStackBtn4);

			// Nested/Mixed StackLayout demo - horizontal containing vertical stacks
			StackLayout mixedStack = new StackLayout();
			mixedStack.Position = new Vector2(1250, 490);
			mixedStack.Size = new Vector2(300, 140);
			mixedStack.Orientation = StackOrientation.Horizontal;
			mixedStack.Spacing = 10;
			mixedStack.Padding = 10;
			mixedStack.IsTransparent = false;
			mixedStack.ZDepth = 5;
			FUI.AddControl(mixedStack);

			// First nested vertical stack
			StackLayout nestedV1 = new StackLayout();
			nestedV1.Size = new Vector2(80, 110);
			nestedV1.Orientation = StackOrientation.Vertical;
			nestedV1.Spacing = 5;
			nestedV1.Padding = 5;
			nestedV1.IsTransparent = false;
			mixedStack.AddChild(nestedV1);

			Button nested1Btn1 = new Button();
			nested1Btn1.Text = "1A";
			nested1Btn1.Size = new Vector2(65, 25);
			nestedV1.AddChild(nested1Btn1);

			Button nested1Btn2 = new Button();
			nested1Btn2.Text = "1B";
			nested1Btn2.Size = new Vector2(65, 25);
			nestedV1.AddChild(nested1Btn2);

			Button nested1Btn3 = new Button();
			nested1Btn3.Text = "1C";
			nested1Btn3.Size = new Vector2(65, 25);
			nestedV1.AddChild(nested1Btn3);

			// Second nested vertical stack
			StackLayout nestedV2 = new StackLayout();
			nestedV2.Size = new Vector2(80, 110);
			nestedV2.Orientation = StackOrientation.Vertical;
			nestedV2.Spacing = 5;
			nestedV2.Padding = 5;
			nestedV2.IsTransparent = false;
			mixedStack.AddChild(nestedV2);

			CheckBox nested2Check1 = new CheckBox("X");
			nested2Check1.Size = new Vector2(15, 15);
			nestedV2.AddChild(nested2Check1);

			CheckBox nested2Check2 = new CheckBox("Y");
			nested2Check2.Size = new Vector2(15, 15);
			nestedV2.AddChild(nested2Check2);

			CheckBox nested2Check3 = new CheckBox("Z");
			nested2Check3.Size = new Vector2(15, 15);
			nestedV2.AddChild(nested2Check3);

			// Third nested vertical stack with mixed controls
			StackLayout nestedV3 = new StackLayout();
			nestedV3.Size = new Vector2(80, 110);
			nestedV3.Orientation = StackOrientation.Vertical;
			nestedV3.Spacing = 5;
			nestedV3.Padding = 5;
			nestedV3.IsTransparent = false;
			mixedStack.AddChild(nestedV3);

			Label nestedLabel = new Label("Mix");
			nestedLabel.Size = new Vector2(65, 20);
			nestedLabel.Alignment = Align.Left;
			nestedV3.AddChild(nestedLabel);

			ToggleSwitch nestedToggle = new ToggleSwitch();
			nestedToggle.Size = new Vector2(50, 20);
			nestedV3.AddChild(nestedToggle);

			ProgressBar nestedProgress = new ProgressBar();
			nestedProgress.Size = new Vector2(65, 15);
			nestedProgress.Value = 0.6f;
			nestedV3.AddChild(nestedProgress);
		}
	}
}
