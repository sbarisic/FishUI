using FishUI;
using FishUI.Controls;
using System;
using System.Numerics;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates the SpreadsheetGrid control with cell editing and navigation.
	/// </summary>
	public class SampleSpreadsheetGrid : ISample
	{
		FishUI.FishUI FUI;
		Label _statusLabel;
		SpreadsheetGrid _mainGrid;
		SpreadsheetGrid _heatMapGrid;
		CheckBox _heatMapToggle;
		SpreadsheetGrid _cursorGrid;
		Slider _cursorXSlider;
		Slider _cursorYSlider;
		Label _cursorPosLabel;

		public string Name => "SpreadsheetGrid";

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
			Label titleLabel = new Label("SpreadsheetGrid Demo");
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

			float yPos = 55;

			// Instructions
			Label instructionsLabel = new Label("Click cell to select, double-click or Enter to edit, Tab to navigate");
			instructionsLabel.Position = new Vector2(20, yPos);
			instructionsLabel.Size = new Vector2(550, 20);
			instructionsLabel.Alignment = Align.Left;
			FUI.AddControl(instructionsLabel);

			yPos += 25;

			// === Main SpreadsheetGrid ===
			_mainGrid = new SpreadsheetGrid();
			_mainGrid.Position = new Vector2(20, yPos);
			_mainGrid.Size = new Vector2(600, 300);
			_mainGrid.RowCount = 20;
			_mainGrid.ColumnCount = 10;
			_mainGrid.CellWidth = 80;
			_mainGrid.CellHeight = 24;

			// Pre-populate sample sales data
			_mainGrid.SetCell(0, 0, "Product");
			_mainGrid.SetCell(0, 1, "Q1");
			_mainGrid.SetCell(0, 2, "Q2");
			_mainGrid.SetCell(0, 3, "Q3");
			_mainGrid.SetCell(0, 4, "Q4");
			_mainGrid.SetCell(0, 5, "Total");
			_mainGrid.SetCell(0, 6, "Avg");

			_mainGrid.SetCell(1, 0, "Widgets");
			_mainGrid.SetCell(1, 1, "1200");
			_mainGrid.SetCell(1, 2, "1350");
			_mainGrid.SetCell(1, 3, "1100");
			_mainGrid.SetCell(1, 4, "1450");
			_mainGrid.SetCell(1, 5, "5100");
			_mainGrid.SetCell(1, 6, "1275");

			_mainGrid.SetCell(2, 0, "Gadgets");
			_mainGrid.SetCell(2, 1, "800");
			_mainGrid.SetCell(2, 2, "950");
			_mainGrid.SetCell(2, 3, "1050");
			_mainGrid.SetCell(2, 4, "900");
			_mainGrid.SetCell(2, 5, "3700");
			_mainGrid.SetCell(2, 6, "925");

			_mainGrid.SetCell(3, 0, "Tools");
			_mainGrid.SetCell(3, 1, "450");
			_mainGrid.SetCell(3, 2, "500");
			_mainGrid.SetCell(3, 3, "475");
			_mainGrid.SetCell(3, 4, "525");
			_mainGrid.SetCell(3, 5, "1950");
			_mainGrid.SetCell(3, 6, "487");

			_mainGrid.SetCell(4, 0, "Parts");
			_mainGrid.SetCell(4, 1, "320");
			_mainGrid.SetCell(4, 2, "380");
			_mainGrid.SetCell(4, 3, "410");
			_mainGrid.SetCell(4, 4, "390");
			_mainGrid.SetCell(4, 5, "1500");
			_mainGrid.SetCell(4, 6, "375");

			_mainGrid.OnSelectionChanged += OnSelectionChanged;
			_mainGrid.OnCellChanged += OnCellChanged;

			FUI.AddControl(_mainGrid);

			yPos += 310;

			// Status label
			_statusLabel = new Label("Selected: A1");
			_statusLabel.Position = new Vector2(20, yPos);
			_statusLabel.Size = new Vector2(600, 20);
			_statusLabel.Alignment = Align.Left;
			FUI.AddControl(_statusLabel);

			yPos += 25;

			// Action buttons
			Button clearBtn = new Button();
			clearBtn.Text = "Clear All";
			clearBtn.Position = new Vector2(20, yPos);
			clearBtn.Size = new Vector2(90, 28);
			clearBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				_mainGrid.ClearAll();
				_statusLabel.Text = "All cells cleared";
			};
			FUI.AddControl(clearBtn);

			Button fillBtn = new Button();
			fillBtn.Text = "Fill Sample";
			fillBtn.Position = new Vector2(120, yPos);
			fillBtn.Size = new Vector2(100, 28);
			fillBtn.OnButtonPressed += (btn, mbtn, pos) =>
			{
				for (int r = 0; r < 6; r++)
				{
					for (int c = 0; c < 6; c++)
					{
						_mainGrid.SetCell(r, c, $"R{r + 1}C{c + 1}");
					}
				}
				_statusLabel.Text = "Sample data filled";
			};
			FUI.AddControl(fillBtn);

			// Keyboard shortcuts help
			Label helpLabel = new Label("Keys: Arrows=Navigate, Enter=Edit, Tab=Next, Esc=Cancel, Del=Clear");
			helpLabel.Position = new Vector2(240, yPos + 4);
			helpLabel.Size = new Vector2(400, 20);
			helpLabel.Alignment = Align.Left;
			FUI.AddControl(helpLabel);

			yPos += 45;

			// === Heat Map Demo ===
			Label heatMapLabel = new Label("Heat Map Mode Demo:");
			heatMapLabel.Position = new Vector2(20, yPos);
			heatMapLabel.Size = new Vector2(200, 20);
			heatMapLabel.Alignment = Align.Left;
			FUI.AddControl(heatMapLabel);

			_heatMapToggle = new CheckBox();
			_heatMapToggle.Position = new Vector2(180, yPos);
			_heatMapToggle.Size = new Vector2(20, 20);
			_heatMapToggle.IsChecked = true;
			_heatMapToggle.OnCheckedChanged += (cb, isChecked) =>
			{
				_heatMapGrid.HeatMapMode = isChecked;
				_statusLabel.Text = isChecked ? "Heat map enabled" : "Heat map disabled";
			};
			FUI.AddControl(_heatMapToggle);

			Label toggleLabel = new Label("Enable");
			toggleLabel.Position = new Vector2(205, yPos);
			toggleLabel.Size = new Vector2(60, 20);
			toggleLabel.Alignment = Align.Left;
			FUI.AddControl(toggleLabel);

			yPos += 25;

			// Heat map grid with linear data (tilted plane visualization)
			_heatMapGrid = new SpreadsheetGrid();
			_heatMapGrid.Position = new Vector2(20, yPos);
			_heatMapGrid.Size = new Vector2(380, 200);
			_heatMapGrid.RowCount = 8;
			_heatMapGrid.ColumnCount = 10;
			_heatMapGrid.CellWidth = 36;
			_heatMapGrid.CellHeight = 24;
			_heatMapGrid.HeatMapMode = true;
			_heatMapGrid.HeatMapMinColor = new FishColor(50, 150, 255, 200);
			_heatMapGrid.HeatMapMaxColor = new FishColor(255, 80, 50, 200);

			// Fill with linear data creating a tilted plane effect
			// Values increase from top-left (low) to bottom-right (high)
			for (int r = 0; r < 8; r++)
			{
				for (int c = 0; c < 10; c++)
				{
					// Linear gradient: combine row and column contributions
					int value = (r * 8) + (c * 6); // Creates a diagonal gradient
					_heatMapGrid.SetCell(r, c, value.ToString());
				}
			}

			FUI.AddControl(_heatMapGrid);

			// Heat map legend
			Label legendLabel = new Label("Tilted plane: Blue (0) at top-left, Red (110) at bottom-right");
			legendLabel.Position = new Vector2(20, yPos + 205);
			legendLabel.Size = new Vector2(380, 20);
			legendLabel.Alignment = Align.Left;
			FUI.AddControl(legendLabel);

			// === Cursor Display Demo (right side) ===
			float cursorDemoX = 420;

			Label cursorLabel = new Label("Cursor Display Mode (ECU Editor Style):");
			cursorLabel.Position = new Vector2(cursorDemoX, yPos);
			cursorLabel.Size = new Vector2(280, 20);
			cursorLabel.Alignment = Align.Left;
			FUI.AddControl(cursorLabel);

			// Cursor grid
			_cursorGrid = new SpreadsheetGrid();
			_cursorGrid.Position = new Vector2(cursorDemoX, yPos + 25);
			_cursorGrid.Size = new Vector2(340, 170);
			_cursorGrid.RowCount = 6;
			_cursorGrid.ColumnCount = 6;
			_cursorGrid.CellWidth = 52;
			_cursorGrid.CellHeight = 26;
			_cursorGrid.CursorMode = true;
			_cursorGrid.CursorX = 0.5f;
			_cursorGrid.CursorY = 0.5f;
			_cursorGrid.CursorColor = new FishColor(255, 100, 0, 220);
			_cursorGrid.CursorRadius = 10f;
			_cursorGrid.CursorLineThickness = 2f;

			// Fill with sample data (tilted plane like heat map)
			for (int r = 0; r < 6; r++)
			{
				for (int c = 0; c < 6; c++)
				{
					int value = (r * 10) + (c * 8);
					_cursorGrid.SetCell(r, c, value.ToString());
				}
			}

			FUI.AddControl(_cursorGrid);

			// X slider
			Label xLabel = new Label("X:");
			xLabel.Position = new Vector2(cursorDemoX, yPos + 200);
			xLabel.Size = new Vector2(20, 20);
			xLabel.Alignment = Align.Left;
			FUI.AddControl(xLabel);

			_cursorXSlider = new Slider();
			_cursorXSlider.Position = new Vector2(cursorDemoX + 25, yPos + 200);
			_cursorXSlider.Size = new Vector2(130, 20);
			_cursorXSlider.Value = 50;
			_cursorXSlider.OnValueChanged += (slider, value) =>
			{
				_cursorGrid.CursorX = value / 100f;
				UpdateCursorPosLabel();
			};
			FUI.AddControl(_cursorXSlider);

			// Y slider
			Label yLabel = new Label("Y:");
			yLabel.Position = new Vector2(cursorDemoX + 165, yPos + 200);
			yLabel.Size = new Vector2(20, 20);
			yLabel.Alignment = Align.Left;
			FUI.AddControl(yLabel);

			_cursorYSlider = new Slider();
			_cursorYSlider.Position = new Vector2(cursorDemoX + 185, yPos + 200);
			_cursorYSlider.Size = new Vector2(130, 20);
			_cursorYSlider.Value = 50;
			_cursorYSlider.OnValueChanged += (slider, value) =>
			{
				_cursorGrid.CursorY = value / 100f;
				UpdateCursorPosLabel();
			};
			FUI.AddControl(_cursorYSlider);

			// Cursor position label
			_cursorPosLabel = new Label("Cursor: (0.50, 0.50)");
			_cursorPosLabel.Position = new Vector2(cursorDemoX, yPos + 225);
			_cursorPosLabel.Size = new Vector2(200, 20);
			_cursorPosLabel.Alignment = Align.Left;
			FUI.AddControl(_cursorPosLabel);
		}

		private void UpdateCursorPosLabel()
		{
			_cursorPosLabel.Text = $"Cursor: ({_cursorGrid.CursorX:F2}, {_cursorGrid.CursorY:F2})";
		}

		private void OnSelectionChanged(SpreadsheetGrid grid, int row, int column)
		{
			string cellRef = SpreadsheetGrid.GetColumnLetter(column) + (row + 1).ToString();
			string value = grid.GetCell(row, column);
			_statusLabel.Text = string.IsNullOrEmpty(value)
				? $"Selected: {cellRef}"
				: $"Selected: {cellRef} = \"{value}\"";
		}

		private void OnCellChanged(SpreadsheetGrid grid, int row, int column, string oldValue, string newValue)
		{
			string cellRef = SpreadsheetGrid.GetColumnLetter(column) + (row + 1).ToString();
			_statusLabel.Text = $"Cell {cellRef} changed: \"{oldValue}\" -> \"{newValue}\"";
		}

		public void Update(float dt)
		{
		}
	}
}

