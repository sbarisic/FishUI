using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

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

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			NPatch Cur = UI.Settings.ImgListBoxNormal;
			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			float ItemHeight = UI.Settings.FontDefault.Size + 4;

			UI.Graphics.PushScissor(GetAbsolutePosition() + new Vector2(2, 2), GetAbsoluteSize() - new Vector2(4, 4));
			for (int i = 0; i < Items.Count; i++)
			{
				bool IsSelected = (i == SelectedIndex);
				bool IsHovered = (i == HoveredIndex);

				float Y = Position.Y + 2 + i * ItemHeight;

				//if (Y + ItemHeight > Position.Y + Size.Y)
				//	break;

				if (IsHovered)
				{
					UI.Graphics.DrawRectangle(new Vector2(Position.X + 2, Y) + ScrollOffset, new Vector2(Size.X - 4, ItemHeight), FishColor.Red);
				}
				else if (IsSelected)
				{
					UI.Graphics.DrawRectangle(new Vector2(Position.X + 2, Y) + ScrollOffset, new Vector2(Size.X - 4, ItemHeight), FishColor.Cyan);
				}

				UI.Graphics.DrawText(UI.Settings.FontDefault, Items[i].Text, new Vector2(Position.X + 4, Y) + ScrollOffset + StartOffset);
			}
			UI.Graphics.PopScissor();
		}
	}
}