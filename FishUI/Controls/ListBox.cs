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
				bool IsSelected = (i == SelectedIndex);
				bool IsHovered = (i == HoveredIndex);

				float Y = Position.Y + 2 + i * ListItemHeight;

				if ((Y + ListItemHeight > Position.Y + GetAbsoluteSize().Y) && !ShowSBar)
					ShowSBar = true;

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
					float ScrollBarW = ScrollBar?.GetAbsoluteSize().X ?? 0;
					if (!ScrollBar?.Visible ?? true)
						ScrollBarW = 0;

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