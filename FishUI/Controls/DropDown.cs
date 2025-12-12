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
	public delegate void DropDownItemSelectedFunc(DropDown DD, DropDownItem Itm);

	public class DropDownItem
	{
		public string Text;
		public object UserData;

		public DropDownItem()
		{
			Text = "Empty Item";
		}

		public DropDownItem(string Text, object UserData = null)
		{
			this.Text = Text;
			this.UserData = UserData;
		}

		public override string ToString()
		{
			return $"DropDownItem '{Text}' <{UserData ?? "null"}>";
		}

		public static implicit operator DropDownItem(string Text)
		{
			return new DropDownItem(Text);
		}
	}

	public class DropDown : Control
	{
		List<DropDownItem> Items = new List<DropDownItem>();

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

		public event DropDownItemSelectedFunc OnItemSelected;

		[YamlIgnore]
		Button DDButton;

		public DropDown()
		{
			Size = new Vector2(200, 19);
		}

		public void AddItem(DropDownItem Itm)
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
				FishUI.Events.Broadcast(FishUI, this, "item_selected", new object[] { Items[SelectedIndex] });
				OnItemSelected?.Invoke(this, Items[SelectedIndex]);
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

		public void AutoResizeHeight()
		{
			Size = new Vector2(GetAbsoluteSize().X, Items.Count * ListItemHeight + 4);
		}

		void CreateButton(FishUI UI)
		{
			if (DDButton != null)
				return;

			DDButton = new Button(UI.Settings.ImgDropdownNormal, UI.Settings.ImgDropdownDisabled, UI.Settings.ImgDropdownPressed, UI.Settings.ImgDropdownHover);
			DDButton.Position = Vector2.Zero;
			DDButton.Size = new Vector2(Size.X, 19);
			AddChild(DDButton);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			CreateButton(UI);

			float ItemHeight = UI.Settings.FontDefault.Size + 4;
			ListItemHeight = ItemHeight;

			AutoResizeHeight();

			NPatch Cur = UI.Settings.ImgListBoxNormal;
			UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), Color);

			UI.Graphics.PushScissor(GetAbsolutePosition() + new Vector2(2, 2), GetAbsoluteSize() - new Vector2(4, 4));
			for (int i = 0; i < Items.Count; i++)
			{
				bool IsSelected = (i == SelectedIndex);
				bool IsHovered = (i == HoveredIndex);

				float Y = Position.Y + 2 + i * ItemHeight;

				Cur = null;
				FishColor TxtColor = FishColor.Black;

				if (IsHovered)
				{
					Cur = UI.Settings.ImgSelectionBoxNormal;
				}

				if (Cur != null)
					UI.Graphics.DrawNPatch(Cur, new Vector2(Position.X + 2, Y) + ScrollOffset, new Vector2(GetAbsoluteSize().X - 4, ItemHeight), Color);

				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, Items[i].Text, new Vector2(Position.X + 4, Y) + ScrollOffset + StartOffset, TxtColor);
			}

			UI.Graphics.PopScissor();
		}
	}
}