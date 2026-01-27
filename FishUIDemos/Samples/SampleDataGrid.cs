using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the DataGrid control with various configurations.
	/// </summary>
	public class SampleDataGrid : ISample
	{
		FishUI.FishUI FUI;
		Label _statusLabel;
		DataGrid _mainGrid;

		public string Name => "DataGrid";

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
			Label titleLabel = new Label("DataGrid Demo");
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

			float yPos = 60;

			// === Basic DataGrid ===
			Label basicLabel = new Label("Employee Database (click headers to sort, drag borders to resize):");
			basicLabel.Position = new Vector2(20, yPos);
			basicLabel.Size = new Vector2(400, 20);
			basicLabel.Alignment = Align.Left;
			FUI.AddControl(basicLabel);

			yPos += 25;

			_mainGrid = new DataGrid();
			_mainGrid.Position = new Vector2(20, yPos);
			_mainGrid.Size = new Vector2(450, 180);
			_mainGrid.AlternatingRowColors = true;

			// Add columns
			_mainGrid.AddColumn("ID", 40, true);
			_mainGrid.AddColumn("Name", 120, true);
			_mainGrid.AddColumn("Department", 100, true);
			_mainGrid.AddColumn("Position", 100, true);
			_mainGrid.AddColumn("Salary", 80, true);

			// Add sample data
			_mainGrid.AddRow("001", "Alice Johnson", "Engineering", "Senior Dev", "$95,000");
			_mainGrid.AddRow("002", "Bob Smith", "Marketing", "Manager", "$78,000");
			_mainGrid.AddRow("003", "Carol White", "Engineering", "Lead Dev", "$110,000");
			_mainGrid.AddRow("004", "David Brown", "Sales", "Rep", "$55,000");
			_mainGrid.AddRow("005", "Eve Davis", "HR", "Director", "$92,000");
			_mainGrid.AddRow("006", "Frank Miller", "Engineering", "Junior Dev", "$65,000");
			_mainGrid.AddRow("007", "Grace Lee", "Finance", "Analyst", "$72,000");
			_mainGrid.AddRow("008", "Henry Wilson", "Sales", "Manager", "$85,000");
			_mainGrid.AddRow("009", "Ivy Chen", "Engineering", "Architect", "$125,000");
			_mainGrid.AddRow("010", "Jack Taylor", "Marketing", "Designer", "$68,000");

			_mainGrid.OnRowSelected += OnRowSelected;
			_mainGrid.OnColumnSort += OnColumnSort;

			FUI.AddControl(_mainGrid);

			yPos += 190;

			// Status label
			_statusLabel = new Label("Click a row to select, Ctrl+click for multi-select");
			_statusLabel.Position = new Vector2(20, yPos);
			_statusLabel.Size = new Vector2(450, 20);
			_statusLabel.Alignment = Align.Left;
			FUI.AddControl(_statusLabel);

			yPos += 35;

			// === Multi-Select Grid ===
			Label multiLabel = new Label("Multi-Select Grid (Ctrl+click, Shift+click):");
			multiLabel.Position = new Vector2(20, yPos);
			multiLabel.Size = new Vector2(300, 20);
			multiLabel.Alignment = Align.Left;
			FUI.AddControl(multiLabel);

			yPos += 25;

			DataGrid multiGrid = new DataGrid();
			multiGrid.Position = new Vector2(20, yPos);
			multiGrid.Size = new Vector2(300, 120);
			multiGrid.MultiSelect = true;

			multiGrid.AddColumn("Item", 150, true);
			multiGrid.AddColumn("Quantity", 70, true);
			multiGrid.AddColumn("Price", 70, true);

			multiGrid.AddRow("Widget A", "10", "$5.99");
			multiGrid.AddRow("Widget B", "25", "$3.49");
			multiGrid.AddRow("Gadget X", "5", "$12.99");
			multiGrid.AddRow("Gadget Y", "15", "$8.99");
			multiGrid.AddRow("Tool Z", "8", "$24.99");

			FUI.AddControl(multiGrid);

			yPos += 130;

			// === Instructions ===
			Label instructionsLabel = new Label("Tip: Use mouse wheel to scroll, drag column borders to resize");
			instructionsLabel.Position = new Vector2(20, yPos);
			instructionsLabel.Size = new Vector2(450, 20);
			instructionsLabel.Alignment = Align.Left;
			FUI.AddControl(instructionsLabel);
		}

		private void OnRowSelected(DataGrid grid, int rowIndex, DataGridRow row)
		{
			_statusLabel.Text = $"Selected: Row {rowIndex} - {row[1]} ({row[2]})";
		}

		private void OnColumnSort(DataGrid grid, int columnIndex, SortDirection direction)
		{
			string dirStr = direction == SortDirection.Ascending ? "ascending" : "descending";
			_statusLabel.Text = $"Sorted by column {columnIndex} ({grid.Columns[columnIndex].Header}) {dirStr}";
		}

		public void Update(float dt)
		{
		}
	}
}

