using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void ListBoxItemSelectedFunc(ListBox ListBox, int Idx, ListBoxItem Itm);

	public class ListBoxItem
	{
		public string Text;
		public object UserData;

		public ListBoxItem()
		{
			Text = "Empty Item";
		}

		public ListBoxItem(string Text, object UserData = null)
		{
			this.Text = Text;
			this.UserData = UserData;
		}

		public override string ToString()
		{
			return $"ListBoxItem '{Text}' <{UserData ?? "null"}>";
		}

		public static implicit operator ListBoxItem(string Text)
		{
			return new ListBoxItem(Text);
		}
	}

	public class ListBox : Control
	{
		List<ListBoxItem> Items = new List<ListBoxItem>();

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
		float ListItemHeight;

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
		/// Whether to allow multiple item selection using Ctrl+click and Shift+click.
		/// </summary>
		[YamlMember]
		public bool MultiSelect { get; set; } = false;

		/// <summary>
		/// Set of currently selected indices when MultiSelect is enabled.
		/// </summary>
		[YamlIgnore]
		HashSet<int> SelectedIndices = new HashSet<int>();

		/// <summary>
		/// Anchor index for Shift+click range selection.
		/// </summary>
		[YamlIgnore]
		int SelectionAnchor = -1;

		public event ListBoxItemSelectedFunc OnItemSelected;

		public ListBox()
		{
			Size = new Vector2(140, 120);
		}

		public void AddItem(ListBoxItem Itm)
		{
			Items.Add(Itm);
		}

		public void SelectIndex(int Idx)
		{
			int LastSelectedIndex = SelectedIndex;

			if (Idx < 0)
				Idx = 0;

			if (Idx >= Items.Count)
				Idx = Items.Count - 1;

			SelectedIndex = Idx;

			if (LastSelectedIndex != SelectedIndex)
			{
				FishUI.Events.Broadcast(FishUI, this, "item_selected", new object[] { SelectedIndex, Items[SelectedIndex] });
			OnItemSelected?.Invoke(this, SelectedIndex, Items[SelectedIndex]);
			}
		}

		/// <summary>
		/// Gets all currently selected indices. In single-select mode, returns only the SelectedIndex.
		/// </summary>
		public int[] GetSelectedIndices()
		{
			if (MultiSelect)
				return SelectedIndices.OrderBy(i => i).ToArray();
			else if (SelectedIndex >= 0)
				return new int[] { SelectedIndex };
			else
				return Array.Empty<int>();
		}

		/// <summary>
		/// Gets all currently selected items.
		/// </summary>
		public ListBoxItem[] GetSelectedItems()
		{
			return GetSelectedIndices().Where(i => i >= 0 && i < Items.Count).Select(i => Items[i]).ToArray();
		}

		/// <summary>
		/// Clears all selections.
		/// </summary>
		public void ClearSelection()
		{
			SelectedIndices.Clear();
			SelectedIndex = -1;
			SelectionAnchor = -1;
		}

		/// <summary>
		/// Selects all items (only works when MultiSelect is enabled).
		/// </summary>
		public void SelectAll()
		{
			if (!MultiSelect)
				return;

			SelectedIndices.Clear();
			for (int i = 0; i < Items.Count; i++)
				SelectedIndices.Add(i);

			if (Items.Count > 0)
				SelectedIndex = 0;
		}

		/// <summary>
		/// Returns true if the given index is selected.
		/// </summary>
		public bool IsIndexSelected(int index)
		{
			if (MultiSelect)
				return SelectedIndices.Contains(index);
			else
				return index == SelectedIndex;
		}

		int PickIndexFromPosition(FishUI UI, Vector2 LocalPos, float ItemHeight)
		{
			int Index = (int)((LocalPos.Y - StartOffset.Y) / ItemHeight);

			if (Index < 0 || Index >= Items.Count)
				return -1;

			return Index;
		}

		int PickIndexFromPosition2(FishUI UI, Vector2 LocalPos, float ItemHeight)
		{
			return PickIndexFromPosition(UI, LocalPos - ScrollOffset, ItemHeight);
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			Vector2 LocalPos = GetLocalRelative(Pos);
			HoveredIndex = PickIndexFromPosition2(UI, LocalPos, UI.Settings.FontDefault.Size + 4);
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
		if (HoveredIndex == -1)
		return;

		if (MultiSelect)
		{
		if (InState.CtrlDown)
		{
		// Ctrl+click: Toggle selection of clicked item
		if (SelectedIndices.Contains(HoveredIndex))
			SelectedIndices.Remove(HoveredIndex);
		else
			SelectedIndices.Add(HoveredIndex);

		SelectionAnchor = HoveredIndex;
		SelectedIndex = HoveredIndex;
		}
		else if (InState.ShiftDown && SelectionAnchor >= 0)
		{
		// Shift+click: Select range from anchor to clicked item
		SelectedIndices.Clear();
		int start = Math.Min(SelectionAnchor, HoveredIndex);
		int end = Math.Max(SelectionAnchor, HoveredIndex);
		for (int i = start; i <= end; i++)
			SelectedIndices.Add(i);

		SelectedIndex = HoveredIndex;
		}
		else
		{
		// Normal click: Clear selection and select only clicked item
		SelectedIndices.Clear();
		SelectedIndices.Add(HoveredIndex);
		SelectionAnchor = HoveredIndex;
		SelectIndex(HoveredIndex);
		}

		FishUI.Events.Broadcast(FishUI, this, "selection_changed", new object[] { GetSelectedIndices() });
		}
		else
		{
		SelectIndex(HoveredIndex);
		}

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
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float WheelDelta)
		{
			if (ScrollBar != null)
			{
				// Delegate to scrollbar if it exists
				if (WheelDelta > 0)
					ScrollBar.ScrollUp();
				else if (WheelDelta < 0)
					ScrollBar.ScrollDown();
			}
			else
			{
				// Direct scrolling when no scrollbar
				float scrollAmount = ListItemHeight > 0 ? ListItemHeight : 18f;
				ScrollOffset.Y += WheelDelta * scrollAmount;

				// Clamp scroll offset
				float contentHeight = Items.Count * (ListItemHeight > 0 ? ListItemHeight : 18f);
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
			RemoveAllChildren();

			ScrollBar = new ScrollBarV();
			ScrollBar.Position = new Vector2(GetAbsoluteSize().X - 16, 0);
			ScrollBar.Size = new Vector2(16, GetAbsoluteSize().Y);
			ScrollBar.ThumbHeight = 0.5f;
			ScrollBar.OnScrollChanged += (_, Scroll, Delta) =>
			{
				float ContentHeight = Items.Count * ListItemHeight;
				ScrollOffset = new Vector2(0, -Scroll * ContentHeight);
			};

			AddChild(ScrollBar);
		}

		public void AutoResizeHeight()
		{
			if (ListItemHeight == 0)
			{
				Size.Y = 0;
				return;
			}
			Size = new Vector2(Size.X, Items.Count * ListItemHeight + 4);
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

			float ItemHeight = UI.Settings.FontDefault.Size + 4;
			ListItemHeight = ItemHeight;

			if (Size.Y == 0)
				AutoResizeHeight();

			NPatch Cur = UI.Settings.ImgListBoxNormal;
			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);



			bool ShowSBar = false;

		UI.Graphics.PushScissor(GetAbsolutePosition() + new Vector2(2, 2), GetAbsoluteSize() - new Vector2(4, 4));
			for (int i = 0; i < Items.Count; i++)
			{
				bool IsSelected = IsIndexSelected(i);
				bool IsHovered = (i == HoveredIndex);

				float Y = Position.Y + 2 + i * ListItemHeight;

				if ((Y + ListItemHeight > Position.Y + GetAbsoluteSize().Y) && !ShowSBar)
					ShowSBar = true;

				// Calculate scrollbar width for row rendering
				float ScrollBarW = ScrollBar?.GetAbsoluteSize().X ?? 0;
				if (!ScrollBar?.Visible ?? true)
					ScrollBarW = 0;

				// Draw alternating row background colors
				if (AlternatingRowColors && !IsSelected && !IsHovered)
				{
					FishColor rowColor = (i % 2 == 0) ? EvenRowColor : OddRowColor;
					UI.Graphics.DrawRectangle(
						new Vector2(Position.X + 2, Y) + ScrollOffset,
						new Vector2(GetAbsoluteSize().X - 4 - ScrollBarW, ListItemHeight),
						rowColor);
				}

				Cur = null;
				FishColor TxtColor = FishColor.Black;

				if (IsHovered && IsSelected)
				{
					Cur = UI.Settings.ImgListBoxItmSelectedHovered;
					TxtColor = FishColor.White;
				}
				else if (IsHovered)
				{
					Cur = UI.Settings.ImgListBoxItmHovered;
				}
				else if (IsSelected)
				{
					Cur = UI.Settings.ImgListBoxItmSelected;
					TxtColor = FishColor.White;
				}

				if (Cur != null)
				{
					UI.Graphics.DrawNPatch(Cur, new Vector2(Position.X + 2, Y) + ScrollOffset, new Vector2(GetAbsoluteSize().X - 4 - (ScrollBarW), ListItemHeight), Color);
				}

				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, Items[i].Text, new Vector2(Position.X + 4, Y) + ScrollOffset + StartOffset, TxtColor);

				if (!ShowScrollBar)
					ShowSBar = false;

				if (ScrollBar != null)
					ScrollBar.Visible = ShowSBar;
			}

			UI.Graphics.PopScissor();
		}
	}
}