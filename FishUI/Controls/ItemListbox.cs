using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Delegate for ItemListbox item selection events.
	/// </summary>
	/// <param name="listBox">The ItemListbox that raised the event.</param>
	/// <param name="index">The index of the selected item.</param>
	/// <param name="item">The selected item.</param>
	public delegate void ItemListboxItemSelectedFunc(ItemListbox listBox, int index, ItemListboxItem item);

	/// <summary>
	/// Represents an item in an ItemListbox that can contain a custom widget control.
	/// </summary>
	public class ItemListboxItem
	{
		/// <summary>
		/// Optional text label for the item. Used when Widget is null.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Custom widget control to display for this item.
		/// When set, this control is rendered instead of the Text property.
		/// </summary>
		public Control Widget { get; set; }

		/// <summary>
		/// User-defined data associated with this item.
		/// </summary>
		public object UserData { get; set; }

		/// <summary>
		/// Height of this item in pixels. If 0, uses the default item height.
		/// </summary>
		public float Height { get; set; }

		public ItemListboxItem()
		{
			Text = "Empty Item";
		}

		public ItemListboxItem(string text, object userData = null)
		{
			Text = text;
			UserData = userData;
		}

		public ItemListboxItem(Control widget, object userData = null)
		{
			Widget = widget;
			UserData = userData;
		}

		public override string ToString()
		{
			if (Widget != null)
				return $"ItemListboxItem [Widget: {Widget.GetType().Name}] <{UserData ?? "null"}>";
			return $"ItemListboxItem '{Text}' <{UserData ?? "null"}>";
		}

		public static implicit operator ItemListboxItem(string text)
		{
			return new ItemListboxItem(text);
		}

		public static implicit operator ItemListboxItem(Control widget)
		{
			return new ItemListboxItem(widget);
		}
	}

	/// <summary>
	/// A listbox control that supports custom widget controls as items.
	/// Each item can contain either plain text or a custom Control widget.
	/// </summary>
	public class ItemListbox : Control
	{
		List<ItemListboxItem> Items = new List<ItemListboxItem>();

		[YamlIgnore]
		Vector2 StartOffset = new Vector2(0, 2);

		Vector2 ScrollOffset = new Vector2(0, 0);

		[YamlMember]
		int SelectedIndex = -1;

		[YamlMember]
		int HoveredIndex = -1;

		[YamlIgnore]
		ScrollBarV ScrollBar;

		[YamlIgnore]
		float DefaultItemHeight;

		/// <summary>
		/// Default height for items that don't specify their own height.
		/// </summary>
		[YamlMember]
		public float ItemHeight { get; set; } = 24f;

		/// <summary>
		/// Whether to show the vertical scrollbar when content exceeds visible area.
		/// </summary>
		[YamlMember]
		public bool ShowScrollBar { get; set; } = true;

		/// <summary>
		/// Whether to display alternating row background colors for even/odd rows.
		/// </summary>
		[YamlMember]
		public bool AlternatingRowColors { get; set; } = false;

		/// <summary>
		/// Background color for even-indexed rows (0, 2, 4, ...) when AlternatingRowColors is enabled.
		/// </summary>
		[YamlMember]
		public FishColor EvenRowColor { get; set; } = new FishColor(255, 255, 255, 20);

		/// <summary>
		/// Background color for odd-indexed rows (1, 3, 5, ...) when AlternatingRowColors is enabled.
		/// </summary>
		[YamlMember]
		public FishColor OddRowColor { get; set; } = new FishColor(0, 0, 0, 20);

		/// <summary>
		/// Event fired when an item is selected.
		/// </summary>
		public event ItemListboxItemSelectedFunc OnItemSelected;

		public ItemListbox()
		{
			Size = new Vector2(200, 150);
		}

		/// <summary>
		/// Adds an item to the listbox.
		/// </summary>
		public void AddItem(ItemListboxItem item)
		{
			Items.Add(item);
			if (item.Widget != null)
			{
				item.Widget.Visible = false;
				AddChild(item.Widget);
			}
		}

		/// <summary>
		/// Adds a text item to the listbox.
		/// </summary>
		public void AddItem(string text, object userData = null)
		{
			AddItem(new ItemListboxItem(text, userData));
		}

		/// <summary>
		/// Adds a widget item to the listbox.
		/// </summary>
		public void AddItem(Control widget, object userData = null)
		{
			AddItem(new ItemListboxItem(widget, userData));
		}

		/// <summary>
		/// Removes an item at the specified index.
		/// </summary>
		public void RemoveItemAt(int index)
		{
			if (index < 0 || index >= Items.Count)
				return;

			var item = Items[index];
			if (item.Widget != null)
				RemoveChild(item.Widget);

			Items.RemoveAt(index);

			if (SelectedIndex >= Items.Count)
				SelectedIndex = Items.Count - 1;
		}

		/// <summary>
		/// Clears all items from the listbox.
		/// </summary>
		public void ClearItems()
		{
			foreach (var item in Items)
			{
				if (item.Widget != null)
					RemoveChild(item.Widget);
			}
			Items.Clear();
			SelectedIndex = -1;
			HoveredIndex = -1;
		}

		/// <summary>
		/// Gets the number of items in the listbox.
		/// </summary>
		public int ItemCount => Items.Count;

		/// <summary>
		/// Gets the item at the specified index.
		/// </summary>
		public ItemListboxItem GetItem(int index)
		{
			if (index < 0 || index >= Items.Count)
				return null;
			return Items[index];
		}

		/// <summary>
		/// Gets or sets the currently selected index. -1 means no selection.
		/// </summary>
		public int GetSelectedIndex() => SelectedIndex;

		/// <summary>
		/// Gets the currently selected item, or null if nothing is selected.
		/// </summary>
		public ItemListboxItem GetSelectedItem()
		{
			if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
				return null;
			return Items[SelectedIndex];
		}

		/// <summary>
		/// Selects the item at the specified index.
		/// </summary>
		public void SelectIndex(int index)
		{
			int lastSelectedIndex = SelectedIndex;

			if (index < 0)
				index = 0;

			if (index >= Items.Count)
				index = Items.Count - 1;

			SelectedIndex = index;

			if (lastSelectedIndex != SelectedIndex && SelectedIndex >= 0)
			{
				FishUI.Events.Broadcast(FishUI, this, "item_selected", new object[] { SelectedIndex, Items[SelectedIndex] });
				OnItemSelected?.Invoke(this, SelectedIndex, Items[SelectedIndex]);
			}
		}

		float GetItemHeight(ItemListboxItem item)
		{
			if (item.Height > 0)
				return item.Height;
			if (item.Widget != null)
				return item.Widget.Size.Y > 0 ? item.Widget.Size.Y : ItemHeight;
			return ItemHeight;
		}

		float GetTotalContentHeight()
		{
			float total = 0;
			foreach (var item in Items)
				total += GetItemHeight(item);
			return total;
		}

		int PickIndexFromPosition(Vector2 localPos)
		{
			float y = localPos.Y - StartOffset.Y - ScrollOffset.Y;
			float accumulatedHeight = 0;

			for (int i = 0; i < Items.Count; i++)
			{
				float itemHeight = GetItemHeight(Items[i]);
				if (y >= accumulatedHeight && y < accumulatedHeight + itemHeight)
					return i;
				accumulatedHeight += itemHeight;
			}

			return -1;
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			Vector2 localPos = GetLocalRelative(Pos);
			HoveredIndex = PickIndexFromPosition(localPos);
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			if (HoveredIndex != -1)
				SelectIndex(HoveredIndex);

			FishUIDebug.LogListBoxSelectionChange(SelectedIndex);
		}

		public override void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
			if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
			{
				if (Key == FishKey.Up)
					SelectIndex(SelectedIndex - 1);
				else if (Key == FishKey.Down)
					SelectIndex(SelectedIndex + 1);
			}
			else if (Items.Count > 0)
			{
				if (Key == FishKey.Up || Key == FishKey.Down)
					SelectIndex(0);
			}
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float WheelDelta)
		{
			if (ScrollBar != null && ScrollBar.Visible)
			{
				if (WheelDelta > 0)
					ScrollBar.ScrollUp();
				else if (WheelDelta < 0)
					ScrollBar.ScrollDown();
			}
			else
			{
				float scrollAmount = ItemHeight;
				ScrollOffset.Y += WheelDelta * scrollAmount;

				float contentHeight = GetTotalContentHeight();
				float visibleHeight = GetAbsoluteSize().Y;
				float maxScroll = 0;
				float minScroll = Math.Min(0, visibleHeight - contentHeight);

				ScrollOffset.Y = Math.Clamp(ScrollOffset.Y, minScroll, maxScroll);
			}
		}

		void CreateScrollBar(FishUI UI)
		{
			if (ScrollBar != null)
				return;

			ScrollBar = new ScrollBarV();
			ScrollBar.Position = new Vector2(GetAbsoluteSize().X - 16, 0);
			ScrollBar.Size = new Vector2(16, GetAbsoluteSize().Y);
			ScrollBar.ThumbHeight = 0.5f;
			ScrollBar.OnScrollChanged += (_, scroll, delta) =>
			{
				float contentHeight = GetTotalContentHeight();
				ScrollOffset = new Vector2(0, -scroll * contentHeight);
			};

			AddChild(ScrollBar);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			if (ShowScrollBar)
				CreateScrollBar(UI);
			else if (ScrollBar != null)
			{
				RemoveChild(ScrollBar);
				ScrollBar = null;
			}

			DefaultItemHeight = ItemHeight;

			NPatch cur = UI.Settings.ImgListBoxNormal;
			UI.Graphics.DrawNPatch(cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			float contentHeight = GetTotalContentHeight();
			bool showSBar = contentHeight > GetAbsoluteSize().Y;

			if (!ShowScrollBar)
				showSBar = false;

			if (ScrollBar != null)
			{
				ScrollBar.Visible = showSBar;
				ScrollBar.Position = new Vector2(GetAbsoluteSize().X - 16, 0);
				ScrollBar.Size = new Vector2(16, GetAbsoluteSize().Y);
			}

			float scrollBarW = (ScrollBar?.Visible ?? false) ? ScrollBar.GetAbsoluteSize().X : 0;

			UI.Graphics.PushScissor(GetAbsolutePosition() + new Vector2(2, 2), GetAbsoluteSize() - new Vector2(4, 4));

			float yOffset = StartOffset.Y;
			for (int i = 0; i < Items.Count; i++)
			{
				var item = Items[i];
				float itemHeight = GetItemHeight(item);
				float itemY = Position.Y + yOffset + ScrollOffset.Y;

				bool isSelected = (i == SelectedIndex);
				bool isHovered = (i == HoveredIndex);

				// Draw alternating row background colors
				if (AlternatingRowColors && !isSelected && !isHovered)
				{
					FishColor rowColor = (i % 2 == 0) ? EvenRowColor : OddRowColor;
					UI.Graphics.DrawRectangle(
						new Vector2(Position.X + 2, itemY),
						new Vector2(GetAbsoluteSize().X - 4 - scrollBarW, itemHeight),
						rowColor);
				}

				NPatch itemPatch = null;
				FishColor txtColor = FishColor.Black;

				if (isHovered && isSelected)
				{
					itemPatch = UI.Settings.ImgListBoxItmSelectedHovered;
					txtColor = FishColor.White;
				}
				else if (isHovered)
				{
					itemPatch = UI.Settings.ImgListBoxItmHovered;
				}
				else if (isSelected)
				{
					itemPatch = UI.Settings.ImgListBoxItmSelected;
					txtColor = FishColor.White;
				}

				if (itemPatch != null)
				{
					UI.Graphics.DrawNPatch(itemPatch,
						new Vector2(Position.X + 2, itemY),
						new Vector2(GetAbsoluteSize().X - 4 - scrollBarW, itemHeight),
						Color);
				}

			// Draw item content
				if (item.Widget != null)
				{
					// Position and show the widget
					item.Widget.Visible = true;
					item.Widget.Position = new FishUIPosition(PositionMode.Relative, new Vector2(4, yOffset + ScrollOffset.Y));
					item.Widget.Size = new Vector2(GetAbsoluteSize().X - 8 - scrollBarW, itemHeight);
				}
				else
				{
					// Draw text
					float textY = itemY + (itemHeight - UI.Settings.FontDefault.Size) / 2;
					UI.Graphics.DrawTextColor(UI.Settings.FontDefault, item.Text ?? "",
						new Vector2(Position.X + 4, textY), txtColor);
				}

				yOffset += itemHeight;
			}

			UI.Graphics.PopScissor();
		}
	}
}
