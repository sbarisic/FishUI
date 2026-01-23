using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
		/// <summary>
		/// Static list of currently open dropdowns for overlay rendering.
		/// </summary>
		internal static List<DropDown> OpenDropdowns = new List<DropDown>();

		List<DropDownItem> Items = new List<DropDownItem>();

		[YamlIgnore]
		Vector2 StartOffset = new Vector2(0, 2);

		Vector2 ScrollOffset = new Vector2(0, 0);

		[YamlMember]
		public int SelectedIndex { get; private set; } = -1;

		[YamlIgnore]
		int HoveredIndex = -1;

		[YamlIgnore]
		float ListItemHeight;

		/// <summary>
		/// Whether the dropdown list is currently open/expanded
		/// </summary>
		[YamlIgnore]
		public bool IsOpen { get; private set; } = false;

		/// <summary>
		/// Maximum number of visible items in the dropdown list (0 = show all)
		/// </summary>
		[YamlMember]
		public int MaxVisibleItems { get; set; } = 8;

		/// <summary>
		/// Height of the dropdown button
		/// </summary>
		[YamlIgnore]
		private float ButtonHeight = 19;

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

		public void ClearItems()
		{
			Items.Clear();
			SelectedIndex = -1;
		}

		public void Open()
		{
			IsOpen = true;
			// Bring dropdown to front and make it always on top while open
			// This ensures the dropdown list appears above other controls
			AlwaysOnTop = true;
			BringToFront();
			// Track this dropdown for overlay rendering
			if (!OpenDropdowns.Contains(this))
				OpenDropdowns.Add(this);
		}

		public void Close()
		{
			IsOpen = false;
			HoveredIndex = -1;
			// Restore normal Z-order behavior
			AlwaysOnTop = false;
			// Remove from overlay tracking
			OpenDropdowns.Remove(this);
		}

		public void Toggle()
		{
			if (IsOpen)
				Close();
			else
				Open();
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
				// Only broadcast event if control is connected to FishUI (has parent or _FishUI set)
				if (FishUI != null)
				{
					FishUI.Events.Broadcast(FishUI, this, "item_selected", new object[] { Items[SelectedIndex] });
				}
				OnItemSelected?.Invoke(this, Items[SelectedIndex]);
			}

			// Close the dropdown after selection
			Close();
		}

		/// <summary>
		/// Gets the selected item, or null if nothing is selected
		/// </summary>
		public DropDownItem GetSelectedItem()
		{
			if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
				return Items[SelectedIndex];
			return null;
		}

		Vector2 GetDropdownListPosition()
		{
			return GetAbsolutePosition() + new Vector2(0, ButtonHeight);
		}

		Vector2 GetDropdownListSize(float itemHeight)
		{
			int visibleCount = MaxVisibleItems > 0 ? Math.Min(Items.Count, MaxVisibleItems) : Items.Count;
			float listHeight = visibleCount * itemHeight + 4;
			return new Vector2(Size.X, listHeight);
		}

		bool IsPointInsideDropdownList(Vector2 GlobalPos, float itemHeight)
		{
			if (!IsOpen)
				return false;

			Vector2 listPos = GetDropdownListPosition();
			Vector2 listSize = GetDropdownListSize(itemHeight);

			return Utils.IsInside(listPos, listSize, GlobalPos);
		}

		int PickIndexFromPosition(Vector2 LocalPos, float ItemHeight)
		{
			int Index = (int)((LocalPos.Y - 2) / ItemHeight);

			if (Index < 0 || Index >= Items.Count)
				return -1;

			return Index;
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			if (IsOpen)
			{
				float itemHeight = UI.Settings.FontDefault.Size + 4;
				Vector2 listPos = GetDropdownListPosition();

				if (IsPointInsideDropdownList(Pos, itemHeight))
				{
					Vector2 localPos = Pos - listPos;
					HoveredIndex = PickIndexFromPosition(localPos - ScrollOffset, itemHeight);
				}
				else
				{
					HoveredIndex = -1;
				}
			}
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			if (Btn != FishMouseButton.Left)
				return;

			float itemHeight = UI.Settings.FontDefault.Size + 4;

			// Check if clicking on the button area
			Vector2 absPos = GetAbsolutePosition();
			Vector2 buttonSize = new Vector2(Size.X, ButtonHeight);

			if (Utils.IsInside(absPos, buttonSize, Pos))
			{
				Toggle();
				return;
			}

			// Check if clicking on the dropdown list
			if (IsOpen && IsPointInsideDropdownList(Pos, itemHeight))
			{
				if (HoveredIndex >= 0 && HoveredIndex < Items.Count)
				{
					SelectIndex(HoveredIndex);
				}
				return;
			}
		}

		public override void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			base.HandleMouseLeave(UI, InState);
			// Only close dropdown if mouse is not inside the dropdown list
			if (IsOpen)
			{
				float itemHeight = ListItemHeight > 0 ? ListItemHeight : 18;
				// Check if cursor moved to the dropdown list (which extends beyond control bounds)
				if (!IsPointInsideDropdownList(InState.MousePos, itemHeight))
				{
					Close();
				}
			}
		}

		public override void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
			if (!IsOpen)
			{
				if (Key == FishKey.Enter || Key == FishKey.Space)
				{
					Open();
				}
				return;
			}

			if (Key == FishKey.Escape)
			{
				Close();
				return;
			}

			if (Key == FishKey.Up)
			{
				if (HoveredIndex > 0)
					HoveredIndex--;
				else if (HoveredIndex == -1 && Items.Count > 0)
					HoveredIndex = Items.Count - 1;
			}
			else if (Key == FishKey.Down)
			{
				if (HoveredIndex < Items.Count - 1)
					HoveredIndex++;
				else if (HoveredIndex == -1 && Items.Count > 0)
					HoveredIndex = 0;
			}
			else if (Key == FishKey.Enter)
			{
				if (HoveredIndex >= 0 && HoveredIndex < Items.Count)
					SelectIndex(HoveredIndex);
			}
		}

		void CreateButton(FishUI UI)
		{
			if (DDButton != null)
				return;

			DDButton = new Button(UI.Settings.ImgDropdownNormal, UI.Settings.ImgDropdownDisabled, UI.Settings.ImgDropdownPressed, UI.Settings.ImgDropdownHover);
			DDButton.Position = Vector2.Zero;
			DDButton.Size = new Vector2(Size.X, ButtonHeight);
			DDButton.OnButtonPressed += (sender, btn, pos) =>
			{
				Toggle();
			};
			AddChild(DDButton);
		}

		public override bool IsPointInside(Vector2 GlobalPt)
		{
			// Check if point is inside the button
			if (base.IsPointInside(GlobalPt))
				return true;

			// Also check if point is inside the open dropdown list
			if (IsOpen)
			{
				float itemHeight = ListItemHeight > 0 ? ListItemHeight : 18;
				return IsPointInsideDropdownList(GlobalPt, itemHeight);
			}

			return false;
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			CreateButton(UI);

			float itemHeight = UI.Settings.FontDefault.Size + 4;
			ListItemHeight = itemHeight;

			Vector2 absPos = GetAbsolutePosition();

			// Update button size to match
			if (DDButton != null)
			{
				DDButton.Size = new Vector2(Size.X, ButtonHeight);

				// Draw selected item text on the button
				string selectedText = "";
				if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
					selectedText = Items[SelectedIndex].Text;

				DDButton.Text = selectedText;
			}

			// NOTE: Dropdown list is drawn separately via DrawDropdownListOverlay
			// to ensure it appears on top of all controls
		}

		/// <summary>
		/// Draws the dropdown list overlay. Called by FishUI after all controls are drawn.
		/// </summary>
		internal void DrawDropdownListOverlay(FishUI UI)
		{
			if (!IsOpen || Items.Count == 0)
				return;

			float itemHeight = UI.Settings.FontDefault.Size + 4;
			Vector2 listPos = GetDropdownListPosition();
			Vector2 listSize = GetDropdownListSize(itemHeight);

			// Draw list background
			NPatch listBg = UI.Settings.ImgListBoxNormal;
			UI.Graphics.DrawNPatch(listBg, listPos, listSize, Color);

			// Draw items
			UI.Graphics.PushScissor(listPos + new Vector2(2, 2), listSize - new Vector2(4, 4));

			int visibleCount = MaxVisibleItems > 0 ? Math.Min(Items.Count, MaxVisibleItems) : Items.Count;

			for (int i = 0; i < visibleCount; i++)
			{
				bool isSelected = (i == SelectedIndex);
				bool isHovered = (i == HoveredIndex);

				float y = listPos.Y + 2 + i * itemHeight;

				NPatch itemBg = null;
				FishColor txtColor = FishColor.Black;

				if (isHovered)
				{
					itemBg = UI.Settings.ImgSelectionBoxNormal;
				}
				else if (isSelected)
				{
					itemBg = UI.Settings.ImgListBoxItmSelected;
				}

				if (itemBg != null)
				{
					UI.Graphics.DrawNPatch(itemBg, new Vector2(listPos.X + 2, y) + ScrollOffset, new Vector2(listSize.X - 4, itemHeight), Color);
				}

				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, Items[i].Text, new Vector2(listPos.X + 4, y) + ScrollOffset + StartOffset, txtColor);
			}

			UI.Graphics.PopScissor();
		}
	}
}