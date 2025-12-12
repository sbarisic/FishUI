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
	public class ListBoxItem
	{
		public string Text;
		public object UserData;

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
		Vector2 StartOffset = new Vector2(0, 2);

		Vector2 ScrollOffset = new Vector2(0, 0);

		int SelectedIndex = -1;
		int HoveredIndex = -1;

		[YamlIgnore]
		ScrollBarV ScrollBar;

		[YamlIgnore]
		float ListItemHeight;

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
			SelectedIndex = HoveredIndex;
			Console.WriteLine(">> Selected index: " + SelectedIndex);
		}

		public override void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
			if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
			{
				if (Key == FishKey.Up)
				{
					SelectedIndex--;
					if (SelectedIndex < 0)
						SelectedIndex = 0;
				}
				else if (Key == FishKey.Down)
				{
					SelectedIndex++;
					if (SelectedIndex >= Items.Count)
						SelectedIndex = Items.Count - 1;
				}
			}
		}

		void CreateScrollBar(FishUI UI)
		{
			if (ScrollBar != null)
				return;

			ScrollBar = new ScrollBarV();
			ScrollBar.Position = new Vector2(Size.X - 16, 0);
			ScrollBar.Size = new Vector2(16, Size.Y);
			ScrollBar.ThumbHeight = 0.5f;
			ScrollBar.OnScrollChanged += (_, Scroll, Delta) =>
			{
				// Scroll is in range 0..1

				float ContentHeight = Items.Count * ListItemHeight;

				ScrollOffset = new Vector2(0, -Scroll * ContentHeight);
			};

			/*ScrollBar.OnThumbPositionChanged += (s, pos) =>
			{
				float ContentHeight = Items.Count * (UI.Settings.FontDefault.Size + 4) + 4;
				float ViewHeight = Size.Y;
				float MaxScroll = MathF.Max(0, ContentHeight - ViewHeight);
				ScrollOffset.Y = MaxScroll * pos;
			};*/

			AddChild(ScrollBar);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			CreateScrollBar(UI);

			NPatch Cur = UI.Settings.ImgListBoxNormal;
			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			float ItemHeight = UI.Settings.FontDefault.Size + 4;
			ListItemHeight = ItemHeight;

			bool ShowScrollBar = false;

			UI.Graphics.PushScissor(GetAbsolutePosition() + new Vector2(2, 2), GetAbsoluteSize() - new Vector2(4, 4));
			for (int i = 0; i < Items.Count; i++)
			{
				bool IsSelected = (i == SelectedIndex);
				bool IsHovered = (i == HoveredIndex);

				float Y = Position.Y + 2 + i * ItemHeight;

				if ((Y + ItemHeight > Position.Y + Size.Y) && !ShowScrollBar)
					ShowScrollBar = true;

				Cur = null;
				FishColor TxtColor = FishColor.Black;

				if (IsHovered && IsSelected)
				{
					Cur = UI.Settings.ImgListBoxItmSelectedHovered;
					TxtColor = FishColor.White;
				}
				else if (IsHovered)
				{
					//UI.Graphics.DrawRectangle(new Vector2(Position.X + 2, Y) + ScrollOffset, new Vector2(Size.X - 4, ItemHeight), FishColor.Red);
					Cur = UI.Settings.ImgListBoxItmHovered;
				}
				else if (IsSelected)
				{
					//UI.Graphics.DrawRectangle(new Vector2(Position.X + 2, Y) + ScrollOffset, new Vector2(Size.X - 4, ItemHeight), FishColor.Cyan);
					Cur = UI.Settings.ImgListBoxItmSelected;
					TxtColor = FishColor.White;
				}

				if (Cur != null)
					UI.Graphics.DrawNPatch(Cur, new Vector2(Position.X + 2, Y) + ScrollOffset, new Vector2(Size.X - 4, ItemHeight), Color);

				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, Items[i].Text, new Vector2(Position.X + 4, Y) + ScrollOffset + StartOffset, TxtColor);

				ScrollBar.Visible = ShowScrollBar;
			}
			UI.Graphics.PopScissor();
		}
	}
}