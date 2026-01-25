using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void ContextMenuClosedFunc(ContextMenu Sender);

	/// <summary>
	/// A popup context menu that displays a list of menu items.
	/// Can be shown at any position and supports nested submenus.
	/// </summary>
	public class ContextMenu : Control
	{
		/// <summary>
		/// Whether the context menu is currently visible.
		/// </summary>
		[YamlIgnore]
		public bool IsOpen { get; private set; } = false;

		/// <summary>
		/// Whether to draw a shadow behind the menu.
		/// </summary>
		public bool ShowShadow { get; set; } = true;

		/// <summary>
		/// Offset of the shadow from the menu (in pixels).
		/// </summary>
		public Vector2 ShadowOffset { get; set; } = new Vector2(4, 4);

		/// <summary>
		/// Extra size added to the shadow beyond the menu bounds (in pixels).
		/// </summary>
		public float ShadowSize { get; set; } = 4;

		/// <summary>
		/// Currently hovered item index.
		/// </summary>
		[YamlIgnore]
		private int HoveredIndex = -1;

		/// <summary>
		/// Currently open submenu (if any).
		/// </summary>
		[YamlIgnore]
		private ContextMenu OpenSubmenu;

		/// <summary>
		/// Parent menu if this is a submenu.
		/// </summary>
		[YamlIgnore]
		internal ContextMenu ParentContextMenu { get; set; }

		/// <summary>
		/// Padding inside the menu.
		/// </summary>
		private const float MenuPadding = 2f;

		/// <summary>
		/// Minimum width of the menu.
		/// </summary>
		[YamlMember]
		public float MinWidth { get; set; } = 120f;

		/// <summary>
		/// Event fired when the menu is closed.
		/// </summary>
		public event ContextMenuClosedFunc OnClosed;

		public ContextMenu()
		{
			AlwaysOnTop = true;
			Visible = false;
		}

		/// <summary>
		/// Adds a menu item with the specified text.
		/// </summary>
		public MenuItem AddItem(string text, object userData = null)
		{
			var item = new MenuItem(text, userData);
			item.ParentMenu = this;
			AddChild(item);
			RecalculateSize();
			return item;
		}

		/// <summary>
		/// Adds a checkable menu item.
		/// </summary>
		public MenuItem AddCheckItem(string text, bool isChecked = false, object userData = null)
		{
			var item = new MenuItem(text, userData)
			{
				IsCheckable = true,
				IsChecked = isChecked
			};
			item.ParentMenu = this;
			AddChild(item);
			RecalculateSize();
			return item;
		}

		/// <summary>
		/// Adds a separator line.
		/// </summary>
		public MenuItem AddSeparator()
		{
			var sep = MenuItem.CreateSeparator();
			sep.ParentMenu = this;
			AddChild(sep);
			RecalculateSize();
			return sep;
		}

		/// <summary>
		/// Adds a submenu item that will show child items when hovered.
		/// </summary>
		public MenuItem AddSubmenu(string text)
		{
			var item = new MenuItem(text);
			item.ParentMenu = this;
			AddChild(item);
			RecalculateSize();
			return item;
		}

		/// <summary>
		/// Clears all menu items.
		/// </summary>
		public void ClearItems()
		{
			RemoveAllChildren();
			RecalculateSize();
		}

		/// <summary>
		/// Shows the context menu at the specified screen position.
		/// </summary>
		public void Show(Vector2 position)
		{
			Position = new FishUIPosition(PositionMode.Absolute, position);
			RecalculateSize();
			IsOpen = true;
			Visible = true;
			HoveredIndex = -1;
			BringToFront();
		}

		/// <summary>
		/// Shows the context menu at the current mouse position.
		/// </summary>
		public void ShowAtMouse(FishUI UI)
		{
			Vector2 mousePos = UI.Input.GetMousePosition();
			Show(mousePos);
		}

		/// <summary>
		/// Closes the context menu and any open submenus.
		/// </summary>
		public void Close()
		{
			CloseSubmenu();
			IsOpen = false;
			Visible = false;
			HoveredIndex = -1;
			OnClosed?.Invoke(this);

			// Close parent menus too (for closing entire menu tree)
			ParentContextMenu?.Close();
		}

		/// <summary>
		/// Closes only this menu without closing parent menus.
		/// </summary>
		public void CloseThis()
		{
			CloseSubmenu();
			IsOpen = false;
			Visible = false;
			HoveredIndex = -1;
			OnClosed?.Invoke(this);
		}

		/// <summary>
		/// Closes any open submenu.
		/// </summary>
		private void CloseSubmenu()
		{
			if (OpenSubmenu != null)
			{
				OpenSubmenu.CloseThis();
				OpenSubmenu = null;
			}
		}

		/// <summary>
		/// Recalculates the menu size based on its items.
		/// </summary>
		private void RecalculateSize()
		{
			float totalHeight = MenuPadding * 2;
			float maxWidth = MinWidth;

			foreach (var child in Children)
			{
				if (child is MenuItem item)
				{
					totalHeight += item.GetItemHeight();

					// Calculate required width for this item
					float itemWidth = MenuItem.LeftPadding + MenuItem.RightPadding;
					// Approximate text width (will be more accurate when drawn)
					itemWidth += (item.Text?.Length ?? 0) * 8;
					if (!string.IsNullOrEmpty(item.ShortcutText))
						itemWidth += item.ShortcutText.Length * 8 + 16;
					if (item.HasSubmenu)
						itemWidth += 16;

					if (itemWidth > maxWidth)
						maxWidth = itemWidth;
				}
			}

			Size = new Vector2(maxWidth, totalHeight);

			// Update child item sizes and positions
			float y = MenuPadding;
			foreach (var child in Children)
			{
				if (child is MenuItem item)
				{
					item.Position = new FishUIPosition(PositionMode.Relative, new Vector2(MenuPadding, y));
					item.Size = new Vector2(maxWidth - MenuPadding * 2, item.GetItemHeight());
					y += item.GetItemHeight();
				}
			}
		}

		/// <summary>
		/// Gets the menu item at the specified index.
		/// </summary>
		public MenuItem GetItem(int index)
		{
			int i = 0;
			foreach (var child in Children)
			{
				if (child is MenuItem item)
				{
					if (i == index)
						return item;
					i++;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the total number of menu items.
		/// </summary>
		[YamlIgnore]
		public int ItemCount
		{
			get
			{
				int count = 0;
				foreach (var child in Children)
				{
					if (child is MenuItem)
						count++;
				}
				return count;
			}
		}

		private int GetItemIndexAtPosition(Vector2 localPos)
		{
			float y = MenuPadding;
			int index = 0;

			foreach (var child in Children)
			{
				if (child is MenuItem item)
				{
					float itemHeight = item.GetItemHeight();
					if (localPos.Y >= y && localPos.Y < y + itemHeight)
						return index;
					y += itemHeight;
					index++;
				}
			}
			return -1;
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);

			Vector2 localPos = GetLocalRelative(Pos);
			int newIndex = GetItemIndexAtPosition(localPos);

			if (newIndex != HoveredIndex)
			{
				HoveredIndex = newIndex;

				// Close current submenu if hovering different item
				CloseSubmenu();

				// Open submenu if hovered item has one
				var hoveredItem = GetItem(HoveredIndex);
				if (hoveredItem != null && hoveredItem.HasSubmenu && !hoveredItem.Disabled)
				{
					OpenSubmenuFor(hoveredItem);
				}
			}

			// Update IsMouseInside for children
			foreach (var child in Children)
			{
				if (child is MenuItem item)
				{
					item.IsMouseInside = child.IsPointInside(Pos);
				}
			}
		}

		private void OpenSubmenuFor(MenuItem item)
		{
			if (item.Submenu == null)
			{
				// Create submenu from item's children
				item.Submenu = new ContextMenu();
				item.Submenu.ParentContextMenu = this;
				item.Submenu._FishUI = FishUI;

				foreach (var child in item.Children)
				{
					if (child is MenuItem subItem)
					{
						var newItem = new MenuItem(subItem.Text, subItem.UserData)
						{
							ShortcutText = subItem.ShortcutText,
							IsSeparator = subItem.IsSeparator,
							IsChecked = subItem.IsChecked,
							IsCheckable = subItem.IsCheckable,
							Disabled = subItem.Disabled
						};
						newItem.ParentMenu = item.Submenu;
						item.Submenu.AddChild(newItem);
					}
				}
				item.Submenu.RecalculateSize();
			}

			// Position submenu to the right of this item
			Vector2 itemPos = item.GetAbsolutePosition();
			Vector2 itemSize = item.GetAbsoluteSize();
			Vector2 submenuPos = new Vector2(itemPos.X + itemSize.X - 2, itemPos.Y);

			item.Submenu.Show(submenuPos);
			OpenSubmenu = item.Submenu;
		}

		public override void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			base.HandleMouseLeave(UI, InState);

			// Don't close if mouse is in submenu
			if (OpenSubmenu != null && OpenSubmenu.IsOpen)
				return;

			// Clear hover state
			foreach (var child in Children)
			{
				if (child is MenuItem item)
				{
					item.IsMouseInside = false;
				}
			}
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			if (Btn != FishMouseButton.Left)
			{
				if (Btn == FishMouseButton.Right)
				{
					// Right-click closes menu
					Close();
				}
				return;
			}

			Vector2 localPos = GetLocalRelative(Pos);
			int index = GetItemIndexAtPosition(localPos);
			var item = GetItem(index);

			if (item != null && !item.Disabled && !item.IsSeparator)
			{
				if (item.HasSubmenu)
				{
					// Open submenu instead of closing
					OpenSubmenuFor(item);
				}
				else
				{
					item.InvokeClick();
					Close();
				}
			}
		}

		public override void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
			base.HandleKeyPress(UI, InState, Key);

			if (!IsOpen)
				return;

			switch (Key)
			{
				case FishKey.Escape:
					Close();
					break;

				case FishKey.Up:
					NavigateUp();
					break;

				case FishKey.Down:
					NavigateDown();
					break;

				case FishKey.Right:
					// Open submenu if available
					var item = GetItem(HoveredIndex);
					if (item != null && item.HasSubmenu)
					{
						OpenSubmenuFor(item);
						if (OpenSubmenu != null)
							OpenSubmenu.HoveredIndex = 0;
					}
					break;

				case FishKey.Left:
					// Close this submenu and go back to parent
					if (ParentContextMenu != null)
					{
						CloseThis();
					}
					break;

				case FishKey.Enter:
					var selectedItem = GetItem(HoveredIndex);
					if (selectedItem != null && !selectedItem.Disabled && !selectedItem.IsSeparator)
					{
						if (selectedItem.HasSubmenu)
						{
							OpenSubmenuFor(selectedItem);
						}
						else
						{
							selectedItem.InvokeClick();
							Close();
						}
					}
					break;
			}
		}

		private void NavigateUp()
		{
			int count = ItemCount;
			if (count == 0) return;

			int startIndex = HoveredIndex;
			int newIndex = startIndex <= 0 ? count - 1 : startIndex - 1;

			// Skip separators
			int attempts = 0;
			while (attempts < count)
			{
				var item = GetItem(newIndex);
				if (item != null && !item.IsSeparator)
				{
					HoveredIndex = newIndex;
					UpdateHoverState();
					return;
				}
				newIndex = newIndex <= 0 ? count - 1 : newIndex - 1;
				attempts++;
			}
		}

		private void NavigateDown()
		{
			int count = ItemCount;
			if (count == 0) return;

			int startIndex = HoveredIndex;
			int newIndex = startIndex < 0 || startIndex >= count - 1 ? 0 : startIndex + 1;

			// Skip separators
			int attempts = 0;
			while (attempts < count)
			{
				var item = GetItem(newIndex);
				if (item != null && !item.IsSeparator)
				{
					HoveredIndex = newIndex;
					UpdateHoverState();
					return;
				}
				newIndex = newIndex >= count - 1 ? 0 : newIndex + 1;
				attempts++;
			}
		}

		private void UpdateHoverState()
		{
			int index = 0;
			foreach (var child in Children)
			{
				if (child is MenuItem item)
				{
					item.IsMouseInside = (index == HoveredIndex);
					index++;
				}
			}
		}

		public override bool IsPointInside(Vector2 GlobalPt)
		{
			// Check if inside main menu
			if (base.IsPointInside(GlobalPt))
				return true;

			// Check if inside open submenu
			if (OpenSubmenu != null && OpenSubmenu.IsOpen && OpenSubmenu.IsPointInside(GlobalPt))
				return true;

			return false;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			if (!IsOpen)
				return;

			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			// Draw shadow behind the menu
			if (ShowShadow && UI.Settings.ImgShadow != null)
			{
				Vector2 shadowPos = pos + ShadowOffset - new Vector2(ShadowSize, ShadowSize);
				Vector2 shadowSize = size + new Vector2(ShadowSize * 2, ShadowSize * 2);
				UI.Graphics.DrawNPatch(UI.Settings.ImgShadow, shadowPos, shadowSize, FishColor.White);
			}

			// Draw menu background
			NPatch bgPatch = UI.Settings.ImgMenuBackground ?? UI.Settings.ImgPanel;
			if (bgPatch != null)
			{
				UI.Graphics.DrawNPatch(bgPatch, pos, size, Color);
			}
			else
			{
				UI.Graphics.DrawRectangle(pos, size, new FishColor(240, 240, 240, 255));
				UI.Graphics.DrawRectangleOutline(pos, size, new FishColor(160, 160, 160, 255));
			}
		}

		public override void DrawChildren(FishUI UI, float Dt, float Time, bool UseScissors = true)
		{
			if (!IsOpen)
				return;

			base.DrawChildren(UI, Dt, Time, UseScissors);

			// Draw open submenu
			if (OpenSubmenu != null && OpenSubmenu.IsOpen)
			{
				OpenSubmenu.DrawControlAndChildren(UI, Dt, Time);
			}
		}
	}
}
