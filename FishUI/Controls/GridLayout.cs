using System;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// A layout container that arranges children in a grid with specified rows and columns.
	/// Children are placed sequentially into grid cells from left to right, top to bottom.
	/// </summary>
	public class GridLayout : Control
	{
		/// <summary>
		/// Number of columns in the grid.
		/// </summary>
		[YamlMember]
		public int Columns { get; set; } = 3;

		/// <summary>
		/// Number of rows in the grid. If 0, rows are calculated automatically based on child count.
		/// </summary>
		[YamlMember]
		public int Rows { get; set; } = 0;

		/// <summary>
		/// Horizontal spacing between cells in pixels.
		/// </summary>
		[YamlMember]
		public float HorizontalSpacing { get; set; } = 5f;

		/// <summary>
		/// Vertical spacing between cells in pixels.
		/// </summary>
		[YamlMember]
		public float VerticalSpacing { get; set; } = 5f;

		/// <summary>
		/// Padding from the edges of the container.
		/// </summary>
		[YamlMember]
		public float LayoutPadding { get; set; } = 5f;

		/// <summary>
		/// Whether the grid layout background is transparent (not drawn).
		/// </summary>
		[YamlMember]
		public bool IsTransparent { get; set; } = true;

		/// <summary>
		/// Whether to stretch children to fill their grid cells.
		/// </summary>
		[YamlMember]
		public bool StretchCells { get; set; } = true;

		/// <summary>
		/// Whether to use uniform cell sizes (all cells same size based on available space).
		/// If false, cell sizes are based on the largest child in each row/column.
		/// </summary>
		[YamlMember]
		public bool UniformCells { get; set; } = true;

		public GridLayout()
		{
			Size = new Vector2(300, 200);
		}

		/// <summary>
		/// Gets the actual number of rows based on children count and column setting.
		/// </summary>
		[YamlIgnore]
		public int ActualRows
		{
			get
			{
				if (Rows > 0)
					return Rows;

				int visibleCount = 0;
				foreach (var child in Children)
				{
					if (child.Visible)
						visibleCount++;
				}

				if (visibleCount == 0 || Columns <= 0)
					return 0;

				return (int)Math.Ceiling((double)visibleCount / Columns);
			}
		}

		/// <summary>
		/// Recalculates the positions and sizes of all children based on grid settings.
		/// Call this after adding/removing children or changing properties.
		/// </summary>
		public void UpdateLayout()
		{
			if (Columns <= 0)
				return;

			Vector2 containerSize = GetAbsoluteSize();
			float availableWidth = containerSize.X - LayoutPadding * 2;
			float availableHeight = containerSize.Y - LayoutPadding * 2;

			int actualRows = ActualRows;
			if (actualRows <= 0)
				return;

			// Calculate cell dimensions
			float totalHSpacing = (Columns - 1) * HorizontalSpacing;
			float totalVSpacing = (actualRows - 1) * VerticalSpacing;

			float cellWidth = (availableWidth - totalHSpacing) / Columns;
			float cellHeight = (availableHeight - totalVSpacing) / actualRows;

			// Position visible children
			int index = 0;
			foreach (var child in Children)
			{
				if (!child.Visible)
					continue;

				int row = index / Columns;
				int col = index % Columns;

				// Stop if we've exceeded the row limit
				if (Rows > 0 && row >= Rows)
					break;

				float x = LayoutPadding + col * (cellWidth + HorizontalSpacing);
				float y = LayoutPadding + row * (cellHeight + VerticalSpacing);

				child.Position = new FishUIPosition(PositionMode.Relative, new Vector2(x, y));

				if (StretchCells)
				{
					child.Size = new Vector2(cellWidth, cellHeight);
				}

				index++;
			}
		}

		/// <summary>
		/// Gets the total content size based on children sizes and spacing.
		/// </summary>
		[YamlIgnore]
		public Vector2 ContentSize
		{
			get
			{
				if (Columns <= 0)
					return new Vector2(LayoutPadding * 2, LayoutPadding * 2);

				int actualRows = ActualRows;
				if (actualRows <= 0)
					return new Vector2(LayoutPadding * 2, LayoutPadding * 2);

				// Calculate based on uniform cells
				if (UniformCells || StretchCells)
				{
					Vector2 containerSize = Size;
					float availableWidth = containerSize.X - LayoutPadding * 2;
					float availableHeight = containerSize.Y - LayoutPadding * 2;

					float totalHSpacing = (Columns - 1) * HorizontalSpacing;
					float totalVSpacing = (actualRows - 1) * VerticalSpacing;

					float cellWidth = (availableWidth - totalHSpacing) / Columns;
					float cellHeight = (availableHeight - totalVSpacing) / actualRows;

					float width = Columns * cellWidth + totalHSpacing + LayoutPadding * 2;
					float height = actualRows * cellHeight + totalVSpacing + LayoutPadding * 2;

					return new Vector2(width, height);
				}

				// Calculate based on largest child per row/column
				float maxWidth = 0;
				float maxHeight = 0;

				foreach (var child in Children)
				{
					if (!child.Visible)
						continue;

					maxWidth = Math.Max(maxWidth, child.Size.X);
					maxHeight = Math.Max(maxHeight, child.Size.Y);
				}

				float totalWidth = Columns * maxWidth + (Columns - 1) * HorizontalSpacing + LayoutPadding * 2;
				float totalHeight = actualRows * maxHeight + (actualRows - 1) * VerticalSpacing + LayoutPadding * 2;

				return new Vector2(totalWidth, totalHeight);
			}
		}

		/// <summary>
		/// Resizes the container to fit its content.
		/// </summary>
		public void SizeToContent()
		{
			Size = ContentSize;
		}

		/// <summary>
		/// Gets the grid cell position (row, column) for the child at the specified index.
		/// </summary>
		public (int row, int column) GetCellPosition(int childIndex)
		{
			if (Columns <= 0)
				return (0, 0);

			return (childIndex / Columns, childIndex % Columns);
		}

		/// <summary>
		/// Gets the child index for the specified grid cell position.
		/// </summary>
		public int GetChildIndex(int row, int column)
		{
			return row * Columns + column;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			// Update layout before drawing
			UpdateLayout();

			if (!IsTransparent)
			{
				NPatch Cur = UI.Settings.ImgPanel;

				if (Disabled)
					Cur = UI.Settings.ImgPanelDisabled;

				UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);
			}
		}
	}
}
