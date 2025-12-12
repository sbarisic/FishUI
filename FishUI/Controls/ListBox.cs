using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
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
		public List<ListBoxItem> Items = new List<ListBoxItem>();

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

		[YamlMember]
		public bool ShowScrollBar = true;

		public event ListBoxItemSelectedFunc OnItemSelected;

		public ListBox()
		{
			Size = new Vector2(140, 120);
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

			//Console.WriteLine(">> Selected index: " + SelectedIndex);
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

		void CreateScrollBar(FishUI UI)
		{
			if (ScrollBar != null)
				return;
			RemoveAllChildren();

			ScrollBar = new ScrollBarV();
			ScrollBar.Position = new Vector2(Size.X - 16, 0);
			ScrollBar.Size = new Vector2(16, Size.Y);
			ScrollBar.ThumbHeight = 0.5f;
			ScrollBar.OnScrollChanged += (_, Scroll, Delta) =>
			{
				float ContentHeight = Items.Count * ListItemHeight;
				ScrollOffset = new Vector2(0, -Scroll * ContentHeight);
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



			NPatch Cur = UI.Settings.ImgListBoxNormal;
			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			float ItemHeight = UI.Settings.FontDefault.Size + 4;
			ListItemHeight = ItemHeight;

			bool ShowSBar = false;

			UI.Graphics.PushScissor(GetAbsolutePosition() + new Vector2(2, 2), GetAbsoluteSize() - new Vector2(4, 4));
			for (int i = 0; i < Items.Count; i++)
			{
				bool IsSelected = (i == SelectedIndex);
				bool IsHovered = (i == HoveredIndex);

				float Y = Position.Y + 2 + i * ItemHeight;

				if ((Y + ItemHeight > Position.Y + Size.Y) && !ShowSBar)
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
					UI.Graphics.DrawNPatch(Cur, new Vector2(Position.X + 2, Y) + ScrollOffset, new Vector2(Size.X - 4 - (ScrollBar?.Size.X ?? 0), ItemHeight), Color);

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