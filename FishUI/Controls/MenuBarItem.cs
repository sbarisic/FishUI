using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Delegate for menu bar item events.
	/// </summary>
	public delegate void MenuBarItemEventFunc(MenuBarItem item);

	/// <summary>
	/// A top-level item in a MenuBar that can display a dropdown menu when clicked.
	/// </summary>
	public class MenuBarItem : Control
	{
		/// <summary>
		/// The display text for this menu item.
		/// </summary>
		[YamlMember]
		public string Text { get; set; } = "";

		/// <summary>
		/// Horizontal padding inside the item.
		/// </summary>
		[YamlMember]
		public float HorizontalPadding { get; set; } = 12f;

		/// <summary>
		/// Whether the dropdown menu is currently open.
		/// </summary>
		[YamlIgnore]
		public bool IsOpen { get; private set; } = false;

		/// <summary>
		/// Reference to the parent menu bar.
		/// </summary>
		[YamlIgnore]
		internal MenuBar ParentMenuBar { get; set; }

		/// <summary>
		/// The dropdown context menu for this item.
		/// </summary>
		[YamlIgnore]
		private ContextMenu _dropdownMenu;

		/// <summary>
		/// Event fired when the dropdown is opened.
		/// </summary>
		public event MenuBarItemEventFunc OnOpened;

		/// <summary>
		/// Event fired when the dropdown is closed.
		/// </summary>
		public event MenuBarItemEventFunc OnClosed;

		public MenuBarItem()
		{
		}

		public MenuBarItem(string text)
		{
			Text = text;
		}

		/// <summary>
		/// Adds a menu item to this item's dropdown.
		/// </summary>
		public MenuItem AddItem(string text, object userData = null)
		{
			EnsureDropdownMenu();
			return _dropdownMenu.AddItem(text, userData);
		}

		/// <summary>
		/// Adds a checkable menu item to this item's dropdown.
		/// </summary>
		public MenuItem AddCheckItem(string text, bool isChecked = false, object userData = null)
		{
			EnsureDropdownMenu();
			return _dropdownMenu.AddCheckItem(text, isChecked, userData);
		}

		/// <summary>
		/// Adds a separator to this item's dropdown.
		/// </summary>
		public MenuItem AddSeparator()
		{
			EnsureDropdownMenu();
			return _dropdownMenu.AddSeparator();
		}

		/// <summary>
		/// Adds a submenu item that will show child items when hovered.
		/// </summary>
		public MenuItem AddSubmenu(string text)
		{
			EnsureDropdownMenu();
			return _dropdownMenu.AddSubmenu(text);
		}

		/// <summary>
		/// Clears all items from the dropdown.
		/// </summary>
		public void ClearItems()
		{
			_dropdownMenu?.ClearItems();
		}

		/// <summary>
		/// Ensures the dropdown menu exists.
		/// </summary>
		private void EnsureDropdownMenu()
		{
			if (_dropdownMenu == null)
			{
				_dropdownMenu = new ContextMenu();
				_dropdownMenu.OnClosed += OnDropdownMenuClosed;
			}
		}

		/// <summary>
		/// Opens the dropdown menu.
		/// </summary>
		internal void OpenDropdown()
		{
		if (_dropdownMenu == null || _dropdownMenu.Children.Count == 0)
		return;

		if (FishUI == null)
		return;

		IsOpen = true;

		// Position dropdown below this item
		Vector2 absPos = GetAbsolutePosition();
		Vector2 absSize = GetAbsoluteSize();
		Vector2 dropdownPos = new Vector2(absPos.X, absPos.Y + absSize.Y);

		// Add dropdown to FishUI root for proper rendering (not as child)
		if (!_dropdownAddedToRoot)
		{
		FishUI.AddControl(_dropdownMenu);
		_dropdownAddedToRoot = true;
		}

		_dropdownMenu.Show(dropdownPos);
		OnOpened?.Invoke(this);
		}

		/// <summary>
		/// Whether the dropdown has been added to the FishUI root.
		/// </summary>
		private bool _dropdownAddedToRoot = false;

		/// <summary>
		/// Closes the dropdown menu.
		/// </summary>
		internal void CloseDropdown()
		{
		if (_dropdownMenu != null && IsOpen)
		{
		_dropdownMenu.CloseThis();
		IsOpen = false;
		OnClosed?.Invoke(this);
		}
		}

		/// <summary>
		/// Called when the dropdown menu is closed.
		/// </summary>
		private void OnDropdownMenuClosed(ContextMenu sender)
		{
			IsOpen = false;
			ParentMenuBar?.OnDropdownClosed(this);
			OnClosed?.Invoke(this);
		}

		/// <summary>
		/// Calculates the required width for this item based on text.
		/// </summary>
		internal float CalculateWidth()
		{
			// Approximate width based on text length
			// This will be more accurate when we have font metrics
			float textWidth = Text.Length * 8f; // Rough estimate
			return textWidth + HorizontalPadding * 2;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();

			// Draw hover/selected background
			if (IsOpen || IsMouseInside)
			{
				if (UI.Settings.ImgMenuHover != null)
				{
					UI.Graphics.DrawNPatch(UI.Settings.ImgMenuHover, absPos, absSize, Color);
				}
				else
				{
					// Fallback
					UI.Graphics.DrawRectangle(absPos, absSize, new FishColor(80, 80, 80, 255));
				}
			}

			// Draw text centered
			if (!string.IsNullOrEmpty(Text))
			{
				var font = UI.Settings.FontDefault;
				Vector2 textSize = UI.Graphics.MeasureText(font, Text);
				Vector2 textPos = new Vector2(
					absPos.X + (absSize.X - textSize.X) / 2,
					absPos.Y + (absSize.Y - textSize.Y) / 2
				);
				UI.Graphics.DrawText(font, Text, textPos);
			}
		}

		public override void HandleMouseEnter(FishUI UI, FishInputState InState)
		{
			base.HandleMouseEnter(UI, InState);
			ParentMenuBar?.OnItemHovered(this);
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left)
			{
				if (IsOpen)
				{
					CloseDropdown();
					ParentMenuBar?.OnDropdownClosed(this);
				}
				else
				{
					ParentMenuBar?.OpenMenu(this);
				}
				ParentMenuBar?.FireItemClicked(this);
			}
		}
	}
}
