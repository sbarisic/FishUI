using FishUI;
using FishUI.Controls;
using FishUIEditor.Controls;
using Raylib_cs;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace FishUIEditor
{
	internal class Program
	{
		static FishUI.FishUI FUI;
		static EditorCanvas _canvas;
		static PropertyGrid _propertyGrid;
		static ListBox _toolbox;
		static Label _statusLabel;
		static MenuBar _menuBar;
		static Panel _leftPanel;
		static Panel _rightPanel;
		static Panel _statusBar;
		static Label _toolboxLabel;
		static Label _propsLabel;
		static Label _hierarchyLabel;
		static TreeView _hierarchyTree;

		// Drag-from-toolbox state
		static bool _isDraggingFromToolbox = false;
		static string _draggedControlType = null;
		static Vector2 _dragPreviewPosition;
		static DragPreviewOverlay _dragOverlay;

		private const float MenuBarHeight = 24f;
		private const float StatusBarHeight = 24f;
		private const float PanelPadding = 5f;
		private const float LeftPanelWidth = 220f;
		private const float RightPanelWidth = 520f;
		private const float HeaderHeight = 20f;
		private const float HeaderSpacing = 5f;

		private static string _currentLayoutPath;
		private const string DefaultLayoutPath = "data/layouts/editor_layout.yaml";

		static void Main(string[] args)
		{
			FishUISettings UISettings = new FishUISettings();
			UISettings.UIScale = 1.0f;

			RaylibGfx Gfx = new RaylibGfx(1600, 900, "FishUI Layout Editor");
			IFishUIInput Input = new RaylibInput();
			IFishUIEvents Events = new EvtHandler();

			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			UISettings.LoadTheme("data/themes/gwen.yaml", applyImmediately: true);

			CreateEditorLayout();
			AddSampleControls();

			Stopwatch SWatch = Stopwatch.StartNew();
			Stopwatch RuntimeWatch = Stopwatch.StartNew();

			while (!Raylib.WindowShouldClose())
			{
				float Dt = SWatch.ElapsedMilliseconds / 1000.0f;
				SWatch.Restart();

				if (Raylib.IsWindowResized())
				{
					FUI.Resized(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
					UpdateLayout();
				}

				FUI.Tick(Dt, (float)RuntimeWatch.Elapsed.TotalSeconds);

				// Handle toolbox drag-and-drop (after FUI.Tick so selection is updated)
				HandleToolboxDrag();

				// Update drag overlay visibility and position
				UpdateDragOverlay();
			}

			Raylib.CloseWindow();
		}

		static void HandleToolboxDrag()
		{
			Vector2 mousePos = Raylib.GetMousePosition();
			bool mousePressed = Raylib.IsMouseButtonPressed(MouseButton.Left);
			bool mouseReleased = Raylib.IsMouseButtonReleased(MouseButton.Left);
			bool mouseDown = Raylib.IsMouseButtonDown(MouseButton.Left);

			// Start drag when mouse pressed over toolbox with a selection
			if (mousePressed && !_isDraggingFromToolbox)
			{
				Vector2 toolboxAbsPos = _toolbox.GetAbsolutePosition();
				Vector2 toolboxSize = _toolbox.Size;

				if (mousePos.X >= toolboxAbsPos.X && mousePos.X <= toolboxAbsPos.X + toolboxSize.X &&
				mousePos.Y >= toolboxAbsPos.Y && mousePos.Y <= toolboxAbsPos.Y + toolboxSize.Y)
				{
					if (_toolbox.SelectedIndex >= 0 && _toolbox.SelectedIndex < _toolbox.Items.Count)
					{
						_isDraggingFromToolbox = true;
						_draggedControlType = _toolbox.Items[_toolbox.SelectedIndex].Text;
						_dragPreviewPosition = mousePos;
						SetStatus($"Dragging: {_draggedControlType}");
					}
				}
			}

			// Update drag position
			if (_isDraggingFromToolbox && mouseDown)
			{
				_dragPreviewPosition = mousePos;

				// Update hovered drop target for visual feedback
				Vector2 canvasAbsPos = _canvas.GetAbsolutePosition();
				Vector2 canvasSize = _canvas.Size;
				if (mousePos.X >= canvasAbsPos.X && mousePos.X <= canvasAbsPos.X + canvasSize.X &&
					mousePos.Y >= canvasAbsPos.Y && mousePos.Y <= canvasAbsPos.Y + canvasSize.Y)
				{
					Vector2 canvasLocalPos = mousePos - canvasAbsPos;
					_canvas.HoveredDropTarget = _canvas.FindContainerAtPosition(canvasLocalPos);
				}
				else
				{
					_canvas.HoveredDropTarget = null;
				}
			}

			// Handle drop on mouse release
			if (_isDraggingFromToolbox && mouseReleased)
			{
				Vector2 canvasAbsPos = _canvas.GetAbsolutePosition();
				Vector2 canvasSize = _canvas.Size;

				// Check if dropped on canvas
				if (mousePos.X >= canvasAbsPos.X && mousePos.X <= canvasAbsPos.X + canvasSize.X &&
				mousePos.Y >= canvasAbsPos.Y && mousePos.Y <= canvasAbsPos.Y + canvasSize.Y)
				{
					Control newControl = CreateControlFromType(_draggedControlType);
					if (newControl != null)
					{
						// Calculate drop position relative to canvas
						Vector2 canvasLocalPos = mousePos - canvasAbsPos;

						// Check if dropping onto a container control
						Control containerAtDrop = _canvas.FindContainerAtPosition(canvasLocalPos);

						if (containerAtDrop != null)
						{
							// Calculate position relative to the container
							Vector2 dropPos = canvasLocalPos - new Vector2(containerAtDrop.Position.X, containerAtDrop.Position.Y);
							dropPos -= new Vector2(newControl.Size.X / 2, newControl.Size.Y / 2);

							// For Window, account for content panel offset since AddChild adds to content panel
							if (containerAtDrop is Window window)
							{
								float sideMargin = Math.Max(window.SideBorderWidth, window.ResizeHandleSize);
								dropPos.X -= sideMargin;
								dropPos.Y -= window.TitlebarHeight;
							}

							// Snap to grid if enabled
							if (_canvas.ShowGrid)
							{
								dropPos.X = (float)Math.Round(dropPos.X / _canvas.GridSize) * _canvas.GridSize;
								dropPos.Y = (float)Math.Round(dropPos.Y / _canvas.GridSize) * _canvas.GridSize;
							}

							newControl.Position = dropPos;

							// Must cast to Window to call the correct AddChild (Window.AddChild is 'new', not override)
							if (containerAtDrop is Window windowContainer)
								windowContainer.AddChild(newControl);
							else
								containerAtDrop.AddChild(newControl);

							_canvas.SelectControl(newControl);
							SetStatus($"Added {_draggedControlType} as child of {containerAtDrop.GetType().Name} at ({dropPos.X}, {dropPos.Y})");
							RefreshHierarchyTree();
						}
						else
						{
							// Add to canvas root level
							Vector2 dropPos = canvasLocalPos - new Vector2(newControl.Size.X / 2, newControl.Size.Y / 2);

							// Snap to grid if enabled
							if (_canvas.ShowGrid)
							{
								dropPos.X = (float)Math.Round(dropPos.X / _canvas.GridSize) * _canvas.GridSize;
								dropPos.Y = (float)Math.Round(dropPos.Y / _canvas.GridSize) * _canvas.GridSize;
							}

							newControl.Position = dropPos;
							_canvas.AddEditedControl(newControl);
							_canvas.SelectControl(newControl);
							SetStatus($"Added {_draggedControlType} at ({dropPos.X}, {dropPos.Y})");
							RefreshHierarchyTree();
						}
					}
				}
				else
				{
					SetStatus("Drag cancelled");
				}

				// End drag
				_isDraggingFromToolbox = false;
				_draggedControlType = null;
				_canvas.HoveredDropTarget = null;
			}
		}

		static void UpdateDragOverlay()
		{
			if (_dragOverlay == null)
				return;

			if (_isDraggingFromToolbox && _draggedControlType != null)
			{
				Control preview = CreateControlFromType(_draggedControlType);
				if (preview != null)
				{
					// Center preview on mouse cursor
					Vector2 previewPos = _dragPreviewPosition - new Vector2(preview.Size.X / 2, preview.Size.Y / 2);
					_dragOverlay.Position = previewPos;
					_dragOverlay.Size = preview.Size;
					_dragOverlay.ControlTypeName = _draggedControlType;
					_dragOverlay.Visible = true;
				}
			}
			else
			{
				_dragOverlay.Visible = false;
			}
		}

		static void CreateEditorLayout()
		{
			int screenW = Raylib.GetScreenWidth();
			int screenH = Raylib.GetScreenHeight();

			// Menu Bar
			_menuBar = new MenuBar();
			_menuBar.Position = new Vector2(0, 0);
			_menuBar.Size = new Vector2(screenW, MenuBarHeight);

			// File menu
			var fileMenu = _menuBar.AddMenu("File");
			fileMenu.AddItem("New Layout").OnClicked += _ => NewLayout();
			fileMenu.AddItem("Open...").OnClicked += _ => OpenLayout();
			fileMenu.AddItem("Save").OnClicked += _ => SaveLayout();
			fileMenu.AddItem("Save As...").OnClicked += _ => SaveLayoutAs();
			fileMenu.AddSeparator();
			fileMenu.AddItem("Export as C#...").OnClicked += _ => ExportAsDesigner();
			fileMenu.AddSeparator();
			fileMenu.AddItem("Exit").OnClicked += _ => Environment.Exit(0);

			// Edit menu
			var editMenu = _menuBar.AddMenu("Edit");
			editMenu.AddItem("Undo").OnClicked += _ => { };
			editMenu.AddItem("Redo").OnClicked += _ => { };
			editMenu.AddSeparator();
			editMenu.AddItem("Delete").OnClicked += _ => DeleteSelectedControl();

			// View menu
			var viewMenu = _menuBar.AddMenu("View");
			viewMenu.AddItem("Toggle Grid").OnClicked += _ => _canvas.ShowGrid = !_canvas.ShowGrid;
			viewMenu.AddItem("Snap to Grid").OnClicked += _ => { };

			FUI.AddControl(_menuBar);

			// Left Panel - Control Toolbox and Hierarchy
			_leftPanel = new Panel();
			_leftPanel.Position = new Vector2(0, MenuBarHeight + 4);
			_leftPanel.Size = new Vector2(LeftPanelWidth, screenH - (MenuBarHeight + StatusBarHeight + 4));
			_leftPanel.BorderStyle = BorderStyle.Solid;
			FUI.AddControl(_leftPanel);

			float leftPanelContentHeight = _leftPanel.Size.Y - PanelPadding * 2;
			float toolboxHeight = leftPanelContentHeight * 0.4f; // 40% for toolbox
			float hierarchyHeight = leftPanelContentHeight * 0.6f - HeaderHeight - HeaderSpacing; // 60% for hierarchy

			// Toolbox section
			_toolboxLabel = new Label("Controls");
			_toolboxLabel.Position = new Vector2(PanelPadding, PanelPadding);
			_toolboxLabel.Size = new Vector2(LeftPanelWidth - PanelPadding * 2, HeaderHeight);
			_toolboxLabel.Alignment = Align.Left;
			_leftPanel.AddChild(_toolboxLabel);

			_toolbox = new ListBox();
			_toolbox.Position = new Vector2(PanelPadding, PanelPadding + HeaderHeight + HeaderSpacing);
			_toolbox.Size = new Vector2(LeftPanelWidth - PanelPadding * 2, toolboxHeight - HeaderHeight - HeaderSpacing);
			// Basic Controls
			_toolbox.AddItem("Button");
			_toolbox.AddItem("Label");
			_toolbox.AddItem("Textbox");
			_toolbox.AddItem("CheckBox");
			_toolbox.AddItem("RadioButton");
			_toolbox.AddItem("ToggleSwitch");
			// Value Controls
			_toolbox.AddItem("Slider");
			_toolbox.AddItem("ProgressBar");
			_toolbox.AddItem("NumericUpDown");
			// List Controls
			_toolbox.AddItem("ListBox");
			_toolbox.AddItem("DropDown");
			_toolbox.AddItem("TreeView");
			// Container Controls
			_toolbox.AddItem("Panel");
			_toolbox.AddItem("GroupBox");
			_toolbox.AddItem("Window");
			_toolbox.AddItem("TabControl");
			_toolbox.AddItem("ScrollablePane");
			// Display Controls
			_toolbox.AddItem("ImageBox");
			_toolbox.AddItem("StaticText");
			// Date/Time Controls
			_toolbox.AddItem("DatePicker");
			_toolbox.AddItem("TimePicker");
			// Data Controls
			_toolbox.AddItem("DataGrid");
			_toolbox.AddItem("SpreadsheetGrid");
			// Gauges
			_toolbox.AddItem("BarGauge");
			_toolbox.AddItem("RadialGauge");
			_toolbox.AddItem("VUMeter");
			_toolbox.OnItemSelected += OnToolboxItemSelected;
			_leftPanel.AddChild(_toolbox);

			// Hierarchy section
			float hierarchyY = PanelPadding + toolboxHeight + HeaderSpacing;
			_hierarchyLabel = new Label("Layout Hierarchy");
			_hierarchyLabel.Position = new Vector2(PanelPadding, hierarchyY);
			_hierarchyLabel.Size = new Vector2(LeftPanelWidth - PanelPadding * 2, HeaderHeight);
			_hierarchyLabel.Alignment = Align.Left;
			_leftPanel.AddChild(_hierarchyLabel);

			_hierarchyTree = new TreeView();
			_hierarchyTree.Position = new Vector2(PanelPadding, hierarchyY + HeaderHeight + HeaderSpacing);
			_hierarchyTree.Size = new Vector2(LeftPanelWidth - PanelPadding * 2, hierarchyHeight);
			_hierarchyTree.OnNodeSelected += OnHierarchyNodeSelected;
			_leftPanel.AddChild(_hierarchyTree);

			// Right Panel - Property Grid
			_rightPanel = new Panel();
			_rightPanel.Position = new Vector2(screenW - RightPanelWidth, MenuBarHeight + 4);
			_rightPanel.Size = new Vector2(RightPanelWidth, screenH - (MenuBarHeight + StatusBarHeight + 4));
			_rightPanel.BorderStyle = BorderStyle.Solid;
			FUI.AddControl(_rightPanel);

			_propsLabel = new Label("Properties");
			_propsLabel.Position = new Vector2(PanelPadding, PanelPadding);
			_propsLabel.Size = new Vector2(RightPanelWidth - PanelPadding * 2, HeaderHeight);
			_propsLabel.Alignment = Align.Left;
			_rightPanel.AddChild(_propsLabel);

			_propertyGrid = new PropertyGrid();
			_propertyGrid.Position = new Vector2(PanelPadding, PanelPadding + HeaderHeight + HeaderSpacing);
			_propertyGrid.Size = new Vector2(RightPanelWidth - PanelPadding * 2, _rightPanel.Size.Y - (PanelPadding * 2 + HeaderHeight + HeaderSpacing));
			_rightPanel.AddChild(_propertyGrid);

			// Center - Editor Canvas
			_canvas = new EditorCanvas();
			_canvas.Position = new Vector2(LeftPanelWidth + 8, MenuBarHeight + 4);
			_canvas.Size = new Vector2(screenW - (LeftPanelWidth + RightPanelWidth + 16), screenH - (MenuBarHeight + StatusBarHeight + 4 + 4));
			_canvas.OnControlSelected += OnCanvasControlSelected;
			_canvas.OnControlModified += OnCanvasControlModified;
			_canvas.OnControlReparented += OnCanvasControlReparented;
			FUI.AddControl(_canvas);

			// Status Bar
			_statusBar = new Panel();
			_statusBar.Position = new Vector2(0, screenH - StatusBarHeight);
			_statusBar.Size = new Vector2(screenW, StatusBarHeight);
			_statusBar.Variant = PanelVariant.Dark;
			FUI.AddControl(_statusBar);

			_statusLabel = new Label("Ready");
			_statusLabel.Position = new Vector2(PanelPadding, 2);
			_statusLabel.Size = new Vector2(screenW - PanelPadding * 2, 20);
			_statusLabel.Alignment = Align.Left;
			_statusBar.AddChild(_statusLabel);

			// Drag preview overlay (added last so it renders on top)
			_dragOverlay = new DragPreviewOverlay();
			_dragOverlay.Visible = false;
			_dragOverlay.AlwaysOnTop = true;
			FUI.AddControl(_dragOverlay);
		}

		static void UpdateLayout()
		{
			int screenW = Raylib.GetScreenWidth();
			int screenH = Raylib.GetScreenHeight();

			if (_menuBar != null)
			{
				_menuBar.Size = new Vector2(screenW, MenuBarHeight);
			}

			if (_leftPanel != null)
			{
				_leftPanel.Position = new Vector2(0, MenuBarHeight + 4);
				_leftPanel.Size = new Vector2(LeftPanelWidth, screenH - (MenuBarHeight + StatusBarHeight + 4));
			}

			if (_toolboxLabel != null)
			{
				_toolboxLabel.Size = new Vector2(LeftPanelWidth - PanelPadding * 2, HeaderHeight);
			}

			if (_toolbox != null && _leftPanel != null)
			{
				float leftPanelContentHeight = _leftPanel.Size.Y - PanelPadding * 2;
				float toolboxHeight = leftPanelContentHeight * 0.4f;
				_toolbox.Size = new Vector2(LeftPanelWidth - PanelPadding * 2, toolboxHeight - HeaderHeight - HeaderSpacing);
			}

			if (_hierarchyLabel != null && _leftPanel != null)
			{
				float leftPanelContentHeight = _leftPanel.Size.Y - PanelPadding * 2;
				float toolboxHeight = leftPanelContentHeight * 0.4f;
				float hierarchyY = PanelPadding + toolboxHeight + HeaderSpacing;
				_hierarchyLabel.Position = new Vector2(PanelPadding, hierarchyY);
			}

			if (_hierarchyTree != null && _leftPanel != null)
			{
				float leftPanelContentHeight = _leftPanel.Size.Y - PanelPadding * 2;
				float toolboxHeight = leftPanelContentHeight * 0.4f;
				float hierarchyHeight = leftPanelContentHeight * 0.6f - HeaderHeight - HeaderSpacing;
				float hierarchyY = PanelPadding + toolboxHeight + HeaderSpacing;
				_hierarchyTree.Position = new Vector2(PanelPadding, hierarchyY + HeaderHeight + HeaderSpacing);
				_hierarchyTree.Size = new Vector2(LeftPanelWidth - PanelPadding * 2, hierarchyHeight);
			}

			if (_rightPanel != null)
			{
				_rightPanel.Position = new Vector2(screenW - RightPanelWidth, MenuBarHeight + 4);
				_rightPanel.Size = new Vector2(RightPanelWidth, screenH - (MenuBarHeight + StatusBarHeight + 4));
			}

			if (_propsLabel != null)
			{
				_propsLabel.Size = new Vector2(RightPanelWidth - PanelPadding * 2, HeaderHeight);
			}

			if (_propertyGrid != null && _rightPanel != null)
			{
				_propertyGrid.Size = new Vector2(RightPanelWidth - PanelPadding * 2, _rightPanel.Size.Y - (PanelPadding * 2 + HeaderHeight + HeaderSpacing));
			}

			if (_canvas != null)
			{
				_canvas.Position = new Vector2(LeftPanelWidth + 8, MenuBarHeight + 4);
				_canvas.Size = new Vector2(screenW - (LeftPanelWidth + RightPanelWidth + 16), screenH - (MenuBarHeight + StatusBarHeight + 4 + 4));
			}

			if (_statusBar != null)
			{
				_statusBar.Position = new Vector2(0, screenH - StatusBarHeight);
				_statusBar.Size = new Vector2(screenW, StatusBarHeight);
			}

			if (_statusLabel != null)
			{
				_statusLabel.Size = new Vector2(screenW - PanelPadding * 2, 20);
			}
		}

		static void AddSampleControls()
		{
			// Add some sample controls to the canvas for testing
			Button btn = new Button();
			btn.Text = "Sample Button";
			btn.Position = new Vector2(50, 50);
			btn.Size = new Vector2(120, 32);
			_canvas.AddEditedControl(btn);

			Label lbl = new Label("Sample Label");
			lbl.Position = new Vector2(50, 100);
			lbl.Size = new Vector2(120, 24);
			_canvas.AddEditedControl(lbl);

			Textbox txt = new Textbox();
			txt.Placeholder = "Enter text...";
			txt.Position = new Vector2(50, 140);
			txt.Size = new Vector2(200, 28);
			_canvas.AddEditedControl(txt);

			CheckBox chk = new CheckBox();
			chk.Position = new Vector2(50, 180);
			chk.Size = new Vector2(20, 20);
			_canvas.AddEditedControl(chk);

			SetStatus($"Loaded {_canvas.GetEditedControls().Count} sample controls");
			RefreshHierarchyTree();
		}

		static void OnToolboxItemSelected(ListBox listBox, int index, ListBoxItem item)
		{
			// Selection only - drag to canvas to create, or double-click for quick add
			// Note: Dragging is handled by HandleToolboxDrag()
			SetStatus($"Selected: {item.Text} - Drag to canvas to add");
		}

		static void OnHierarchyNodeSelected(TreeView tree, TreeNode node)
		{
			// Select the corresponding control on the canvas
			if (node?.UserData is Control control)
			{
				_canvas.SelectControl(control);
				_propertyGrid.SelectedObject = control;
				SetStatus($"Selected: {control.GetType().Name} from hierarchy");
			}
		}

		static void RefreshHierarchyTree()
		{
			if (_hierarchyTree == null)
				return;

			_hierarchyTree.ClearNodes();

			var editedControls = _canvas.GetEditedControls();
			foreach (var control in editedControls)
			{
				var node = AddControlToHierarchy(control);
				_hierarchyTree.AddNode(node);
			}
		}

		static TreeNode AddControlToHierarchy(Control control)
		{
			string nodeName = GetControlDisplayName(control);
			var node = new TreeNode(nodeName);
			node.UserData = control;

			// Add children recursively
			// For Window, use ContentChildren to show user-added children only (not internal Titlebar/Panel)
			var children = control is Window window ? window.ContentChildren : control.Children;
			foreach (var child in children)
			{
				var childNode = AddControlToHierarchy(child);
				node.AddChild(childNode);
			}

			return node;
		}

		static string GetControlDisplayName(Control control)
		{
			string typeName = control.GetType().Name;

			// Priority: DesignerName > ID > text content > type name only
			if (!string.IsNullOrEmpty(control.DesignerName))
				return $"{typeName} ({control.DesignerName})";

			if (!string.IsNullOrEmpty(control.ID))
				return $"{typeName} ({control.ID})";

			// For controls with text, show a preview
			if (control is Button btn && !string.IsNullOrEmpty(btn.Text))
				return $"{typeName}: \"{btn.Text}\"";
			if (control is Label lbl && !string.IsNullOrEmpty(lbl.Text))
				return $"{typeName}: \"{lbl.Text}\"";
			if (control is GroupBox grp)
				return $"{typeName}: \"{grp.Text}\"";

			return typeName;
		}

		static Control CreateControlFromType(string typeName)
		{
			return typeName switch
			{
				// Basic Controls
				"Button" => new Button { Text = "Button", Size = new Vector2(100, 28) },
				"Label" => new Label("Label") { Size = new Vector2(100, 24) },
				"Textbox" => new Textbox { Size = new Vector2(150, 28) },
				"CheckBox" => new CheckBox { Size = new Vector2(20, 20) },
				"RadioButton" => new RadioButton { Size = new Vector2(20, 20) },
				"ToggleSwitch" => new ToggleSwitch { Size = new Vector2(50, 24) },
				// Value Controls
				"Slider" => new Slider { Size = new Vector2(150, 24) },
				"ProgressBar" => new ProgressBar { Size = new Vector2(150, 24), Value = 50 },
				"NumericUpDown" => new NumericUpDown { Size = new Vector2(100, 28) },
				// List Controls
				"ListBox" => new ListBox { Size = new Vector2(150, 100) },
				"DropDown" => new DropDown { Size = new Vector2(150, 28) },
				"TreeView" => new TreeView { Size = new Vector2(150, 120) },
				// Container Controls
				"Panel" => new Panel { Size = new Vector2(200, 150) },
				"GroupBox" => new GroupBox("Group") { Size = new Vector2(200, 150) },
				"Window" => new Window { Size = new Vector2(300, 200) },
				"TabControl" => CreateDefaultTabControl(),
				"ScrollablePane" => new ScrollablePane { Size = new Vector2(200, 150) },
				// Display Controls
				"ImageBox" => new ImageBox { Size = new Vector2(100, 100) },
				"StaticText" => new StaticText("Static Text") { Size = new Vector2(100, 24) },
				// Date/Time Controls
				"DatePicker" => new DatePicker { Size = new Vector2(150, 28) },
				"TimePicker" => new TimePicker { Size = new Vector2(120, 28) },
				// Data Controls
				"DataGrid" => CreateDefaultDataGrid(),
				"SpreadsheetGrid" => new SpreadsheetGrid { Size = new Vector2(300, 200) },
				// Gauges
				"BarGauge" => new BarGauge { Size = new Vector2(200, 40), Value = 50 },
				"RadialGauge" => new RadialGauge { Size = new Vector2(120, 120), Value = 50 },
				"VUMeter" => new VUMeter { Size = new Vector2(30, 100) },
				_ => null
			};
		}

		static TabControl CreateDefaultTabControl()
		{
			var tabControl = new TabControl { Size = new Vector2(250, 180) };
			tabControl.AddTab("Tab 1");
			tabControl.AddTab("Tab 2");
			return tabControl;
		}

		static DataGrid CreateDefaultDataGrid()
		{
			var dataGrid = new DataGrid { Size = new Vector2(300, 150) };
			dataGrid.AddColumn("Column 1", 100);
			dataGrid.AddColumn("Column 2", 100);
			return dataGrid;
		}

		static void OnCanvasControlSelected(Control control)
		{
			if (control != null)
			{
				_propertyGrid.SelectedObject = control;
				SetStatus($"Selected: {control.GetType().Name} at ({control.Position.X}, {control.Position.Y})");
			}
			else
			{
				_propertyGrid.SelectedObject = null;
				SetStatus("No selection");
			}
		}

		static void OnCanvasControlModified(Control control)
		{
			// Refresh property grid when control is moved/resized
			_propertyGrid.SelectedObject = control;
			SetStatus($"Modified: {control.GetType().Name} - Pos: ({control.Position.X}, {control.Position.Y}) Size: ({control.Size.X}, {control.Size.Y})");
		}

		static void OnCanvasControlReparented(Control control)
		{
			// Refresh hierarchy and property grid when control is reparented
			var newParent = control.GetParent();
			string parentName = newParent != null ? newParent.GetType().Name : "Root";
			SetStatus($"Reparented: {control.GetType().Name} -> {parentName}");
			RefreshHierarchyTree();
			_propertyGrid.SelectedObject = control;
		}

		static void DeleteSelectedControl()
		{
			if (_canvas.SelectedControl != null)
			{
				_canvas.RemoveEditedControl(_canvas.SelectedControl);
				_propertyGrid.SelectedObject = null;
				SetStatus("Control deleted");
				RefreshHierarchyTree();
			}
		}

		static void NewLayout()
		{
			_canvas.ClearEditedControls();
			_propertyGrid.SelectedObject = null;
			_currentLayoutPath = null;
			SetStatus("New layout created");
			RefreshHierarchyTree();
		}

		static void OpenLayout()
		{
			// Get initial directory from current path or default
			string initialDir = string.IsNullOrWhiteSpace(_currentLayoutPath)
				? FUI.FileSystem.GetFullPath("data/layouts")
				: FUI.FileSystem.GetDirectoryName(_currentLayoutPath);

			var dialog = new FilePickerDialog(FilePickerMode.Open, FUI.FileSystem, initialDir, "*.yaml");
			dialog.OnFileConfirmed += (dlg, path) =>
			{
				LoadLayoutFromPath(path);
			};
			dialog.OnDialogCancelled += (dlg) =>
			{
				SetStatus("Open cancelled");
			};
			dialog.Show(FUI);
		}

		static void SaveLayout()
		{
			if (string.IsNullOrWhiteSpace(_currentLayoutPath))
			{
				SaveLayoutAs();
				return;
			}
			SaveLayoutToPath(_currentLayoutPath);
		}

		static void SaveLayoutAs()
		{
			// Get initial directory from current path or default
			string initialDir = string.IsNullOrWhiteSpace(_currentLayoutPath)
				? FUI.FileSystem.GetFullPath("data/layouts")
				: FUI.FileSystem.GetDirectoryName(_currentLayoutPath);

			var dialog = new FilePickerDialog(FilePickerMode.Save, FUI.FileSystem, initialDir, "*.yaml");

			// Pre-fill filename if we have one
			if (!string.IsNullOrWhiteSpace(_currentLayoutPath))
			{
				dialog.FileName = FUI.FileSystem.GetFileName(_currentLayoutPath);
			}
			else
			{
				dialog.FileName = "layout.yaml";
			}

			dialog.OnFileConfirmed += (dlg, path) =>
			{
				// Ensure .yaml extension
				if (!path.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
				{
					path += ".yaml";
				}
				SaveLayoutToPath(path);
			};
			dialog.OnDialogCancelled += (dlg) =>
			{
				SetStatus("Save cancelled");
			};
			dialog.Show(FUI);
		}

		static void SaveLayoutToPath(string path)
		{
			try
			{
				if (_canvas == null)
					return;

				var ctrls = _canvas.GetEditedControls();
				string yaml = LayoutFormat.SerializeControls(ctrls);
				FUI.FileSystem.WriteAllText(path, yaml);
				_currentLayoutPath = path;
				SetStatus($"Saved layout: {path}");
			}
			catch (Exception ex)
			{
				SetStatus($"Save failed: {ex.Message}");
			}
		}

		static void LoadLayoutFromPath(string path)
		{
			try
			{
				if (_canvas == null)
					return;

				string yaml = FUI.FileSystem.ReadAllText(path);
				var ctrls = LayoutFormat.DeserializeControls(yaml);

				_canvas.ClearEditedControls();
				foreach (var c in ctrls)
				{
					OnDeserializedRecursive(c, FUI);
					_canvas.AddEditedControl(c);
				}

				_currentLayoutPath = path;
				_propertyGrid.SelectedObject = null;
				SetStatus($"Loaded layout: {path}");
				RefreshHierarchyTree();
			}
			catch (Exception ex)
			{
				// Log full error details to file for debugging
				string errorDetails = $"[{DateTime.Now}] Load failed for: {path}\n{ex}\n\n";
				try
				{
					File.AppendAllText("out.txt", errorDetails);
				}
				catch { }

				SetStatus($"Load failed: {ex.Message}");
			}
		}

		static void OnDeserializedRecursive(Control control, FishUI.FishUI ui)
		{
			control.OnDeserialized(ui);

			// For Window, iterate content children
			if (control is Window window)
			{
				foreach (var child in window.ContentChildren)
				{
					OnDeserializedRecursive(child, ui);
				}
			}
			else
			{
				foreach (var child in control.Children)
				{
					OnDeserializedRecursive(child, ui);
				}
			}
		}

		static void ExportAsDesigner()
		{
			if (_canvas == null || _canvas.GetEditedControls().Count == 0)
			{
				SetStatus("No controls to export");
				return;
			}

			// Create a dialog window for namespace and class name input
			var dialog = new Window("Export as C# Designer");
			dialog.Size = new Vector2(400, 200);
			dialog.IsResizable = false;

			var namespaceLabel = new Label("Namespace:");
			namespaceLabel.Position = new Vector2(20, 20);
			namespaceLabel.Size = new Vector2(100, 24);
			dialog.AddChild(namespaceLabel);

			var namespaceTextbox = new Textbox();
			namespaceTextbox.Position = new Vector2(130, 20);
			namespaceTextbox.Size = new Vector2(230, 28);
			namespaceTextbox.Text = "MyApp.Forms";
			dialog.AddChild(namespaceTextbox);

			var classLabel = new Label("Class Name:");
			classLabel.Position = new Vector2(20, 60);
			classLabel.Size = new Vector2(100, 24);
			dialog.AddChild(classLabel);

			var classTextbox = new Textbox();
			classTextbox.Position = new Vector2(130, 60);
			classTextbox.Size = new Vector2(230, 28);
			classTextbox.Text = "MainForm";
			dialog.AddChild(classTextbox);

			var exportButton = new Button();
			exportButton.Text = "Export...";
			exportButton.Position = new Vector2(130, 110);
			exportButton.Size = new Vector2(100, 32);
			dialog.AddChild(exportButton);

			var cancelButton = new Button();
			cancelButton.Text = "Cancel";
			cancelButton.Position = new Vector2(240, 110);
			cancelButton.Size = new Vector2(100, 32);
			dialog.AddChild(cancelButton);

			exportButton.OnButtonPressed += (btn, mouseBtn, pos) =>
			{
				string namespaceName = namespaceTextbox.Text.Trim();
				string className = classTextbox.Text.Trim();

				if (string.IsNullOrEmpty(namespaceName) || string.IsNullOrEmpty(className))
				{
					SetStatus("Namespace and class name are required");
					return;
				}

				dialog.Close();
				ShowExportFileDialog(namespaceName, className);
			};

			cancelButton.OnButtonPressed += (btn, mouseBtn, pos) =>
			{
				dialog.Close();
				SetStatus("Export cancelled");
			};

			// Add to FUI first, then center and show
			FUI.AddControl(dialog);

			// Center using actual screen dimensions (like FilePickerDialog)
			int screenWidth = FUI.Graphics.GetWindowWidth();
			int screenHeight = FUI.Graphics.GetWindowHeight();
			dialog.Position = new Vector2(
				(screenWidth - dialog.Size.X) / 2,
				(screenHeight - dialog.Size.Y) / 2
			);

			dialog.ShowModal();
		}

		static void ShowExportFileDialog(string namespaceName, string className)
		{
			string initialDir = string.IsNullOrWhiteSpace(_currentLayoutPath)
				? FUI.FileSystem.GetFullPath(".")
				: FUI.FileSystem.GetDirectoryName(_currentLayoutPath);

			var dialog = new FilePickerDialog(FilePickerMode.Save, FUI.FileSystem, initialDir, "*.cs");
			dialog.FileName = $"{className}.Designer.cs";

			dialog.OnFileConfirmed += (dlg, path) =>
			{
				// Ensure .cs extension
				if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
				{
					path += ".cs";
				}
				ExportDesignerToPath(path, namespaceName, className);
			};
			dialog.OnDialogCancelled += (dlg) =>
			{
				SetStatus("Export cancelled");
			};
			dialog.Show(FUI);
		}

		static void ExportDesignerToPath(string path, string namespaceName, string className)
		{
			try
			{
				if (_canvas == null)
					return;

				var ctrls = _canvas.GetEditedControls();
				var generator = new DesignerCodeGenerator();
				string code = generator.Generate(ctrls, namespaceName, className);

				FUI.FileSystem.WriteAllText(path, code);
				SetStatus($"Exported C# designer: {path}");
			}
			catch (Exception ex)
			{
				SetStatus($"Export failed: {ex.Message}");
			}
		}

		static void SetStatus(string message)
		{
			if (_statusLabel != null)
				_statusLabel.Text = message;
		}
	}
}
