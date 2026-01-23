using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void MenuItemClickFunc(MenuItem Sender);

	/// <summary>
	/// Represents a single item in a ContextMenu or MenuBar.
	/// Can be a regular item, separator, or container for submenus.
	/// </summary>
	public class MenuItem : Control
	{
		/// <summary>
		/// The display text for this menu item.
		/// </summary>
		[YamlMember]
		public string Text { get; set; } = "";

		/// <summary>
		/// Optional shortcut key text to display (e.g., "Ctrl+S").
		/// </summary>
		[YamlMember]
		public string ShortcutText { get; set; }

		/// <summary>
		/// Whether this item is a separator line.
		/// </summary>
		[YamlMember]
		public bool IsSeparator { get; set; } = false;

		/// <summary>
		/// Whether this item shows a checkmark.
		/// </summary>
		[YamlMember]
		public bool IsChecked { get; set; } = false;

		/// <summary>
		/// Whether this item can be toggled (shows check box).
		/// </summary>
		[YamlMember]
		public bool IsCheckable { get; set; } = false;

		/// <summary>
		/// Optional icon image for this menu item.
		/// </summary>
		[YamlIgnore]
		public ImageRef Icon { get; set; }

		/// <summary>
		/// Whether this item has a submenu (has Children).
		/// </summary>
		[YamlIgnore]
		public bool HasSubmenu => Children.Count > 0;

		/// <summary>
		/// User data attached to this menu item.
		/// </summary>
		[YamlIgnore]
		public object UserData { get; set; }

		/// <summary>
		/// Event fired when this menu item is clicked.
		/// </summary>
		public event MenuItemClickFunc OnClicked;

		/// <summary>
		/// Height of a regular menu item.
		/// </summary>
		public const float ItemHeight = 22f;

		/// <summary>
		/// Height of a separator.
		/// </summary>
		public const float SeparatorHeight = 8f;

		/// <summary>
		/// Padding from left edge for text.
		/// </summary>
		public const float LeftPadding = 24f;

		/// <summary>
		/// Padding from right edge.
		/// </summary>
		public const float RightPadding = 16f;

		/// <summary>
		/// Reference to parent menu for coordinating submenu display.
		/// </summary>
		[YamlIgnore]
		internal ContextMenu ParentMenu { get; set; }

		/// <summary>
		/// The submenu control if this item has children.
		/// </summary>
		[YamlIgnore]
		internal ContextMenu Submenu { get; set; }

		public MenuItem()
		{
			Size = new Vector2(150, ItemHeight);
		}

		public MenuItem(string text, object userData = null)
		{
			Text = text;
			UserData = userData;
			Size = new Vector2(150, ItemHeight);
		}

		/// <summary>
		/// Creates a separator menu item.
		/// </summary>
		public static MenuItem CreateSeparator()
		{
			return new MenuItem
			{
				IsSeparator = true,
				Size = new Vector2(150, SeparatorHeight)
			};
		}

		/// <summary>
		/// Adds a submenu item.
		/// </summary>
		public MenuItem AddItem(string text, object userData = null)
		{
			var item = new MenuItem(text, userData);
			AddChild(item);
			return item;
		}

		/// <summary>
		/// Adds a separator to the submenu.
		/// </summary>
		public MenuItem AddSeparator()
		{
			var sep = CreateSeparator();
			AddChild(sep);
			return sep;
		}

		/// <summary>
		/// Gets the height this item occupies.
		/// </summary>
		public float GetItemHeight()
		{
			return IsSeparator ? SeparatorHeight : ItemHeight;
		}

		/// <summary>
		/// Invokes the click event and toggles check state if checkable.
		/// </summary>
		internal void InvokeClick()
		{
			if (Disabled || IsSeparator)
				return;

			if (IsCheckable)
			{
				IsChecked = !IsChecked;
			}

			OnClicked?.Invoke(this);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			if (IsSeparator)
			{
				// Draw separator line
				float lineY = pos.Y + size.Y / 2;
				UI.Graphics.DrawRectangle(
					new Vector2(pos.X + 4, lineY),
					new Vector2(size.X - 8, 1),
					new FishColor(128, 128, 128, 255));
				return;
			}

			// Draw hover background
			if (IsMouseInside && !Disabled)
			{
				NPatch hoverBg = UI.Settings.ImgMenuHover ?? UI.Settings.ImgSelectionBoxNormal;
				if (hoverBg != null)
				{
					UI.Graphics.DrawNPatch(hoverBg, pos, size, Color);
				}
				else
				{
					UI.Graphics.DrawRectangle(pos, size, new FishColor(100, 100, 200, 128));
				}
			}

			// Draw checkmark if checked
			if (IsCheckable || IsChecked)
			{
				if (IsChecked)
				{
					// Draw checkmark - use theme image or fallback to text
					Vector2 checkPos = new Vector2(pos.X + 4, pos.Y + (size.Y - 14) / 2);
					UI.Graphics.DrawText(UI.Settings.FontDefault, "?", checkPos);
				}
			}

			// Draw text
			FishColor textColor = Disabled ? new FishColor(128, 128, 128, 255) : FishColor.Black;
			Vector2 textPos = new Vector2(pos.X + LeftPadding, pos.Y + (size.Y - UI.Settings.FontDefault.Size) / 2);
			UI.Graphics.DrawTextColor(UI.Settings.FontDefault, Text ?? "", textPos, textColor);

			// Draw shortcut text on the right
			if (!string.IsNullOrEmpty(ShortcutText))
			{
				Vector2 shortcutSize = UI.Graphics.MeasureText(UI.Settings.FontDefault, ShortcutText);
				Vector2 shortcutPos = new Vector2(pos.X + size.X - shortcutSize.X - RightPadding, textPos.Y);
				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, ShortcutText, shortcutPos, new FishColor(100, 100, 100, 255));
			}

			// Draw submenu arrow if has children
			if (HasSubmenu)
			{
				NPatch arrow = UI.Settings.ImgMenuRightArrow;
				if (arrow != null)
				{
					Vector2 arrowSize = new Vector2(8, 8);
					Vector2 arrowPos = new Vector2(pos.X + size.X - arrowSize.X - 4, pos.Y + (size.Y - arrowSize.Y) / 2);
					UI.Graphics.DrawNPatch(arrow, arrowPos, arrowSize, Color);
				}
				else
				{
					// Fallback: draw arrow character
					Vector2 arrowPos = new Vector2(pos.X + size.X - 12, pos.Y + (size.Y - UI.Settings.FontDefault.Size) / 2);
					UI.Graphics.DrawText(UI.Settings.FontDefault, "?", arrowPos);
				}
			}
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (Btn == FishMouseButton.Left && !HasSubmenu)
			{
				InvokeClick();
				ParentMenu?.Close();
			}
		}

		public override string ToString()
		{
			if (IsSeparator)
				return "MenuItem [Separator]";
			return $"MenuItem '{Text}'";
		}
	}
}
