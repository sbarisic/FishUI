using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Represents a single tab in a TabControl.
	/// </summary>
	public class TabPage
	{
		/// <summary>
		/// The text displayed on the tab header.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// The content panel for this tab.
		/// </summary>
		public Panel Content { get; set; }

		/// <summary>
		/// Whether this tab is enabled.
		/// </summary>
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// Optional user data associated with this tab.
		/// </summary>
		public object Tag { get; set; }

		public TabPage()
		{
			Content = new Panel { IsTransparent = true };
		}

		public TabPage(string text) : this()
		{
			Text = text;
		}

		public TabPage(string text, Panel content)
		{
			Text = text;
			Content = content ?? new Panel { IsTransparent = true };
		}
	}

	/// <summary>
	/// A tab control that displays multiple tabs with switchable content panels.
	/// </summary>
	public class TabControl : Control
	{
		/// <summary>
		/// The list of tab pages in this control.
		/// </summary>
		[YamlIgnore]
		public List<TabPage> TabPages { get; } = new List<TabPage>();

		/// <summary>
		/// Tab names for serialization and PropertyGrid editing. 
		/// Setting this property updates existing tabs, adds new tabs, or removes excess tabs.
		/// </summary>
		[YamlMember]
		public List<string> TabNames
		{
			get
			{
				// Populate from TabPages when serializing
				return TabPages.Select(p => p.Text ?? "").ToList();
			}
			set
			{
				if (value == null)
				{
					_tabNamesForDeserialization = null;
					return;
				}

				// Store for use during deserialization (when Children haven't been populated yet)
				_tabNamesForDeserialization = value;

				// Also update TabPages at runtime if they already exist
				if (TabPages.Count > 0)
				{
					SyncTabNamesToTabPages(value);
				}
			}
		}

		/// <summary>
		/// Syncs the TabNames list to the TabPages list.
		/// Updates existing tab names, adds new tabs, or removes excess tabs.
		/// </summary>
		private void SyncTabNamesToTabPages(List<string> names)
		{
			// Update existing tabs
			for (int i = 0; i < names.Count; i++)
			{
				if (i < TabPages.Count)
				{
					// Update existing tab's name
					TabPages[i].Text = names[i];
				}
				else
				{
					// Add new tab
					AddTab(names[i]);
				}
			}

			// Remove excess tabs (from the end)
			while (TabPages.Count > names.Count)
			{
				RemoveTabAt(TabPages.Count - 1);
			}
		}

		// Temporary storage for tab names during deserialization
		private List<string> _tabNamesForDeserialization;

		/// <summary>
		/// The index of the currently selected tab.
		/// </summary>
		public int SelectedIndex
		{
			get => _selectedIndex;
			set
			{
				if (value >= 0 && value < TabPages.Count && value != _selectedIndex)
				{
					int oldIndex = _selectedIndex;
					_selectedIndex = value;
					UpdateContentVisibility();
					OnSelectedIndexChanged?.Invoke(this, oldIndex, _selectedIndex);
				}
			}
		}
		private int _selectedIndex = 0;

		/// <summary>
		/// Gets the currently selected tab page, or null if none selected.
		/// </summary>
		[YamlIgnore]
		public TabPage SelectedTab => _selectedIndex >= 0 && _selectedIndex < TabPages.Count 
			? TabPages[_selectedIndex] 
			: null;

		/// <summary>
		/// Height of the tab header strip.
		/// </summary>
		public float TabHeaderHeight { get; set; } = 24;

		/// <summary>
		/// Minimum width of each tab.
		/// </summary>
		public float MinTabWidth { get; set; } = 60;

		/// <summary>
		/// Maximum width of each tab. Set to 0 for no limit.
		/// </summary>
		public float MaxTabWidth { get; set; } = 150;

		/// <summary>
		/// Padding inside the content area.
		/// </summary>
		public float ContentPadding { get; set; } = 4;

		/// <summary>
		/// Event raised when the selected tab changes.
		/// </summary>
		public event Action<TabControl, int, int> OnSelectedIndexChanged;

		private int _hoveredTabIndex = -1;

		public TabControl()
		{
			Size = new Vector2(300, 200);
		}

		/// <summary>
		/// Adds a new tab page to the control.
		/// </summary>
		public TabPage AddTab(string text)
		{
			var page = new TabPage(text);
			AddTab(page);
			return page;
		}

		/// <summary>
		/// Adds a tab page to the control.
		/// </summary>
		public void AddTab(TabPage page)
		{
			TabPages.Add(page);
			
			// Setup content panel positioning
			page.Content.Position = new Vector2(ContentPadding, TabHeaderHeight + ContentPadding);
			page.Content.Size = GetContentSize() - new Vector2(ContentPadding * 2, ContentPadding * 2);
			
			AddChild(page.Content);
			
			// If this is the first tab, select it
			if (TabPages.Count == 1)
			{
				_selectedIndex = 0;
			}
			
			UpdateContentVisibility();
		}

		/// <summary>
		/// Removes a tab page from the control.
		/// </summary>
		public void RemoveTab(TabPage page)
		{
			int index = TabPages.IndexOf(page);
			if (index >= 0)
			{
				RemoveTabAt(index);
			}
		}

		/// <summary>
		/// Removes a tab at the specified index.
		/// </summary>
		public void RemoveTabAt(int index)
		{
			if (index >= 0 && index < TabPages.Count)
			{
				var page = TabPages[index];
				RemoveChild(page.Content);
				TabPages.RemoveAt(index);

				if (_selectedIndex >= TabPages.Count)
				{
					_selectedIndex = Math.Max(0, TabPages.Count - 1);
				}
				
				UpdateContentVisibility();
			}
		}

		/// <summary>
		/// Gets the size of the content area.
		/// </summary>
		public Vector2 GetContentSize()
		{
			return new Vector2(Size.X, Size.Y - TabHeaderHeight);
		}

		private void UpdateContentVisibility()
		{
			for (int i = 0; i < TabPages.Count; i++)
			{
				TabPages[i].Content.Visible = (i == _selectedIndex);
			}
		}

		private float GetTabWidth(FishUI UI, TabPage page)
		{
			if (UI?.Settings?.FontDefault == null)
				return MinTabWidth;

			Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, page.Text ?? "");
			float width = textSize.X + 20; // padding

			if (width < MinTabWidth) width = MinTabWidth;
			if (MaxTabWidth > 0 && width > MaxTabWidth) width = MaxTabWidth;

			return width;
		}

		private int GetTabAtPosition(FishUI UI, Vector2 pos)
		{
			Vector2 absPos = GetAbsolutePosition();
			
			// Check if in header area
			if (pos.Y < absPos.Y || pos.Y > absPos.Y + TabHeaderHeight)
				return -1;

			float x = absPos.X;
			for (int i = 0; i < TabPages.Count; i++)
			{
				float tabWidth = GetTabWidth(UI, TabPages[i]);
				if (pos.X >= x && pos.X < x + tabWidth)
				{
					return i;
				}
				x += tabWidth;
			}

			return -1;
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);
			_hoveredTabIndex = GetTabAtPosition(UI, Pos);
		}

		public override void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			base.HandleMouseLeave(UI, InState);
			_hoveredTabIndex = -1;
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				int tabIndex = GetTabAtPosition(UI, Pos);
				if (tabIndex >= 0 && TabPages[tabIndex].Enabled)
				{
					SelectedIndex = tabIndex;
				}
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();

			// Update content panel sizes
			Vector2 contentSize = GetContentSize() - new Vector2(ContentPadding * 2, ContentPadding * 2);
			foreach (var page in TabPages)
			{
			page.Content.Position = new Vector2(ContentPadding, TabHeaderHeight + ContentPadding);
			page.Content.Size = contentSize;
			}

		// Draw tab control background (content area)
			NPatch bgImg = UI.Settings.ImgTabControlBackground;
			Vector2 contentPos = new Vector2(absPos.X, absPos.Y + TabHeaderHeight);
			Vector2 bgSize = new Vector2(absSize.X, absSize.Y - TabHeaderHeight);

			if (bgImg != null)
			{
				UI.Graphics.DrawNPatch(bgImg, contentPos, bgSize, Color);
			}
			else
			{
				// Fallback
				UI.Graphics.DrawRectangle(contentPos, bgSize, new FishColor(240, 240, 240));
				UI.Graphics.DrawRectangleOutline(contentPos, bgSize, new FishColor(180, 180, 180));
			}

			// Calculate total tab width first
			float totalTabWidth = 0;
			foreach (var page in TabPages)
			{
				totalTabWidth += GetTabWidth(UI, page);
			}

			// Draw a neutral background for the empty header area (after the tabs)
			// Use the inactive tab background or a simple gray fill
			if (totalTabWidth < absSize.X)
			{
				Vector2 headerPos = new Vector2(absPos.X + totalTabWidth, absPos.Y);
				Vector2 headerSize = new Vector2(absSize.X - totalTabWidth, TabHeaderHeight);
				
				NPatch inactiveTabImg = UI.Settings.ImgTabTopInactive;
				if (inactiveTabImg != null)
				{
					UI.Graphics.DrawNPatch(inactiveTabImg, headerPos, headerSize, Color);
				}
				else
				{
					// Fallback: draw a neutral gray
					UI.Graphics.DrawRectangle(headerPos, headerSize, new FishColor(200, 200, 200));
				}
			}

			// Draw tabs
			float x = absPos.X;
			for (int i = 0; i < TabPages.Count; i++)
			{
				var page = TabPages[i];
				float tabWidth = GetTabWidth(UI, page);
				bool isSelected = (i == _selectedIndex);
				bool isHovered = (i == _hoveredTabIndex);

				Vector2 tabPos = new Vector2(x, absPos.Y);
				Vector2 tabSize = new Vector2(tabWidth, TabHeaderHeight);

				// Draw tab background
				NPatch tabImg = isSelected 
					? UI.Settings.ImgTabTopActive 
					: UI.Settings.ImgTabTopInactive;

				if (tabImg != null)
				{
					FishColor tabColor = page.Enabled ? Color : new FishColor(180, 180, 180);
					UI.Graphics.DrawNPatch(tabImg, tabPos, tabSize, tabColor);
				}
				else
				{
					// Fallback
					FishColor bgColor = isSelected 
						? new FishColor(240, 240, 240) 
						: (isHovered ? new FishColor(220, 220, 220) : new FishColor(200, 200, 200));
					UI.Graphics.DrawRectangle(tabPos, tabSize, bgColor);
					UI.Graphics.DrawRectangleOutline(tabPos, tabSize, new FishColor(150, 150, 150));
				}

				// Draw tab text
				if (!string.IsNullOrEmpty(page.Text))
				{
					Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, page.Text);
					float textX = x + (tabWidth - textSize.X) / 2;
					float textY = absPos.Y + (TabHeaderHeight - textSize.Y) / 2;
					UI.Graphics.DrawText(UI.Settings.FontDefault, page.Text, new Vector2(textX, textY));
				}

				x += tabWidth;
			}
		}

		/// <summary>
		/// Called after deserialization to rebuild the TabPages list from children.
		/// Uses stored TabNames to preserve tab names across serialization.
		/// </summary>
		public override void OnDeserialized(FishUI UI)
		{
			// Clear and rebuild TabPages from child panels
			TabPages.Clear();
			int tabIndex = 0;

			foreach (var child in Children)
			{
				if (child is Panel panel)
				{
					// Use stored tab name if available, otherwise fallback to generic name
					string tabName = (_tabNamesForDeserialization != null && tabIndex < _tabNamesForDeserialization.Count)
						? _tabNamesForDeserialization[tabIndex]
						: $"Tab {tabIndex + 1}";

					var page = new TabPage(tabName, panel);
					TabPages.Add(page);
					tabIndex++;
				}
			}

			// Clear the temporary storage
			_tabNamesForDeserialization = null;

			// Reset selected index if needed
			if (_selectedIndex >= TabPages.Count)
			{
				_selectedIndex = TabPages.Count > 0 ? 0 : -1;
			}

			UpdateContentVisibility();

			// Call base to handle children recursively
			base.OnDeserialized(UI);
		}

		/// <summary>
		/// Draws the TabControl in editor mode with tab panels and header visualization.
		/// Shows all tab content panels with different colored outlines for easy identification.
		/// </summary>
		public override void DrawControlEditor(FishUI UI, float Dt, float Time, Vector2 canvasOffset)
		{
			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();

			// Draw the control background (simplified - just the frame)
			NPatch bgImg = UI.Settings.ImgTabControlBackground;
			Vector2 contentPos = new Vector2(absPos.X, absPos.Y + TabHeaderHeight);
			Vector2 bgSize = new Vector2(absSize.X, absSize.Y - TabHeaderHeight);

			if (bgImg != null)
			{
				UI.Graphics.DrawNPatch(bgImg, contentPos, bgSize, Color);
			}
			else
			{
				UI.Graphics.DrawRectangle(contentPos, bgSize, new FishColor(240, 240, 240));
				UI.Graphics.DrawRectangleOutline(contentPos, bgSize, new FishColor(180, 180, 180));
			}

			// Draw tab header background
			NPatch headerImg = UI.Settings.ImgTabTopInactive;
			Vector2 headerPos = absPos;
			Vector2 headerSize = new Vector2(absSize.X, TabHeaderHeight);

			if (headerImg != null)
			{
				UI.Graphics.DrawNPatch(headerImg, headerPos, headerSize, Color);
			}
			else
			{
				UI.Graphics.DrawRectangle(headerPos, headerSize, new FishColor(200, 200, 200));
			}

			// Draw tab labels in header
			float x = absPos.X;
			FishColor[] tabColors = new FishColor[]
			{
				new FishColor(100, 150, 255, 180), // Blue
				new FishColor(100, 200, 100, 180), // Green
				new FishColor(200, 150, 100, 180), // Orange
				new FishColor(180, 100, 180, 180), // Purple
				new FishColor(200, 200, 100, 180), // Yellow
			};

			for (int i = 0; i < TabPages.Count; i++)
			{
				var page = TabPages[i];
				float tabWidth = GetTabWidth(UI, page);
				bool isSelected = (i == _selectedIndex);

				Vector2 tabPos = new Vector2(x, absPos.Y);
				Vector2 tabSize = new Vector2(tabWidth, TabHeaderHeight);

				// Draw tab background with color coding
				FishColor tabColor = tabColors[i % tabColors.Length];
				if (isSelected)
				{
					tabColor = new FishColor(tabColor.R, tabColor.G, tabColor.B, 255);
				}

				UI.Graphics.DrawRectangle(tabPos, tabSize, tabColor);
				UI.Graphics.DrawRectangleOutline(tabPos, tabSize, new FishColor(60, 60, 60));

				// Draw tab text
				if (!string.IsNullOrEmpty(page.Text) && UI.Settings.FontDefault != null)
				{
					Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, page.Text);
					float textX = x + (tabWidth - textSize.X) / 2;
					float textY = absPos.Y + (TabHeaderHeight - textSize.Y) / 2;
					UI.Graphics.DrawText(UI.Settings.FontDefault, page.Text, new Vector2(textX, textY));
				}

				// Draw content panel outline with matching color
				if (page.Content != null)
				{
					Vector2 panelPos = page.Content.GetAbsolutePosition();
					Vector2 panelSize = page.Content.GetAbsoluteSize();
					FishColor outlineColor = new FishColor(tabColor.R, tabColor.G, tabColor.B, (byte)(isSelected ? 200 : 100));
					UI.Graphics.DrawRectangleOutline(panelPos, panelSize, outlineColor);

					// Draw tab index label in the content area corner
					if (UI.Settings.FontDefault != null)
					{
						string label = $"Tab {i + 1}";
						UI.Graphics.DrawTextColor(UI.Settings.FontDefault, label, new Vector2(panelPos.X + 4, panelPos.Y + 2), outlineColor);
					}
				}

				x += tabWidth;
			}

			// Draw container outline for the whole control
			FishColor containerColor = new FishColor(100, 150, 255, 150);
			UI.Graphics.DrawRectangleOutline(absPos, absSize, containerColor);

			// Draw anchor visualization
			DrawAnchorVisualization(UI);
		}

		/// <summary>
		/// Draws children in editor mode. For TabControl, draws all tab content panels
		/// regardless of which tab is selected, so all children are visible in the editor.
		/// </summary>
		public override void DrawChildrenEditor(FishUI UI, float Dt, float Time)
		{
			// Draw all tab page content panels (not just the selected one)
			foreach (var page in TabPages)
			{
				if (page.Content != null)
				{
					// Draw the content panel's children
					foreach (var child in page.Content.Children.OrderBy(c => c.ZDepth))
					{
						child.DrawControlEditor(UI, Dt, Time, Vector2.Zero);
						child.DrawChildrenEditor(UI, Dt, Time);
					}
				}
			}
		}
	}
}
