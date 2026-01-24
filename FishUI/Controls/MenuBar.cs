using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Delegate for menu bar item click events.
	/// </summary>
	public delegate void MenuBarItemClickFunc(MenuBar menuBar, MenuBarItem item);

	/// <summary>
	/// A horizontal menu bar control typically placed at the top of an application window.
	/// Contains MenuBarItems which can open dropdown menus when clicked.
	/// </summary>
	public class MenuBar : Control
	{
		/// <summary>
		/// Height of the menu bar.
		/// </summary>
		[YamlMember]
		public float BarHeight { get; set; } = 24f;

		/// <summary>
		/// Padding between menu items.
		/// </summary>
		[YamlMember]
		public float ItemPadding { get; set; } = 8f;

		/// <summary>
		/// Currently open menu item (if any).
		/// </summary>
		[YamlIgnore]
		public MenuBarItem OpenItem { get; private set; }

		/// <summary>
		/// Currently hovered menu item.
		/// </summary>
		[YamlIgnore]
		private MenuBarItem _hoveredItem;

		/// <summary>
		/// Whether any menu is currently open.
		/// </summary>
		[YamlIgnore]
		public bool IsMenuOpen => OpenItem != null;

		/// <summary>
		/// Event fired when a menu item is clicked.
		/// </summary>
		public event MenuBarItemClickFunc OnItemClicked;

		public MenuBar()
		{
			Size = new Vector2(400, 24);
		}

		/// <summary>
		/// Adds a top-level menu item to the menu bar.
		/// </summary>
		public MenuBarItem AddMenu(string text)
		{
			var item = new MenuBarItem(text);
			item.ParentMenuBar = this;
			AddChild(item);
			RecalculateItemPositions();
			return item;
		}

		/// <summary>
		/// Clears all menu items.
		/// </summary>
		public void ClearMenus()
		{
			CloseOpenMenu();
			RemoveAllChildren();
		}

		/// <summary>
		/// Closes any currently open menu.
		/// </summary>
		public void CloseOpenMenu()
		{
			if (OpenItem != null)
			{
				OpenItem.CloseDropdown();
				OpenItem = null;
			}
		}

		/// <summary>
		/// Opens the dropdown for the specified menu item.
		/// </summary>
		internal void OpenMenu(MenuBarItem item)
		{
			if (OpenItem == item)
				return;

			CloseOpenMenu();
			OpenItem = item;
			item.OpenDropdown();
		}

		/// <summary>
		/// Called when a menu item is hovered while a menu is open.
		/// Automatically switches to the new menu.
		/// </summary>
		internal void OnItemHovered(MenuBarItem item)
		{
			_hoveredItem = item;
			if (IsMenuOpen && item != OpenItem)
			{
				OpenMenu(item);
			}
		}

		/// <summary>
		/// Called when a menu item's dropdown is closed.
		/// </summary>
		internal void OnDropdownClosed(MenuBarItem item)
		{
			if (OpenItem == item)
			{
				OpenItem = null;
			}
		}

		/// <summary>
		/// Fires the item clicked event.
		/// </summary>
		internal void FireItemClicked(MenuBarItem item)
		{
			OnItemClicked?.Invoke(this, item);
		}

		/// <summary>
		/// Recalculates the positions of all menu items.
		/// </summary>
		private void RecalculateItemPositions()
		{
			float x = ItemPadding;

			foreach (var child in Children)
			{
				if (child is MenuBarItem item)
				{
					item.Position = new Vector2(x, 0);
					item.Size = new Vector2(item.CalculateWidth(), BarHeight);
					x += item.Size.X + ItemPadding;
				}
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();

			// Draw menu bar background
			if (UI.Settings.ImgMenuStrip != null)
			{
				UI.Graphics.DrawNPatch(UI.Settings.ImgMenuStrip, absPos, absSize, Color);
			}
			else
			{
				// Fallback
				UI.Graphics.DrawRectangle(absPos, absSize, new FishColor(60, 60, 60, 255));
			}
		}

		public override void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			base.HandleMouseLeave(UI, InState);
			_hoveredItem = null;
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			// Close menu if clicking on the bar background (not on an item)
			if (Btn == FishMouseButton.Left && IsMenuOpen && _hoveredItem == null)
			{
				CloseOpenMenu();
			}
		}
	}
}
