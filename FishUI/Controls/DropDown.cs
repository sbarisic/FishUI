using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void DropDownItemSelectedFunc(DropDown DD, DropDownItem Itm);

	/// <summary>
	/// Delegate for custom item rendering in DropDown.
	/// </summary>
	/// <param name="ui">The FishUI instance</param>
	/// <param name="item">The item being rendered</param>
	/// <param name="position">Position to render at</param>
	/// <param name="size">Size of the item area</param>
	/// <param name="isSelected">Whether item is currently selected</param>
	/// <param name="isHovered">Whether item is currently hovered</param>
	public delegate void DropDownItemRenderFunc(FishUI ui, DropDownItem item, Vector2 position, Vector2 size, bool isSelected, bool isHovered);

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

	public class DropDown : Control, IFishUIDebugSnapshotProvider
	{
		private const int MaximumDiagnosticSelectionIndices = 256;
		private readonly struct DiagnosticSelection
		{
			internal int Count { get; }
			internal int[] Indices { get; }

			internal DiagnosticSelection(int count, int[] indices)
			{
				Count = count;
				Indices = indices;
			}
		}

		/// <summary>
		/// Static list of currently open dropdowns for overlay rendering.
		/// </summary>
		internal static List<DropDown> OpenDropdowns = new List<DropDown>();

		/// <summary>
		/// The list of items in the DropDown. Can be serialized to/from YAML.
		/// </summary>
		[YamlMember]
		public List<DropDownItem> Items { get; set; } = new List<DropDownItem>();

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
		/// Whether to show a search/filter textbox when the dropdown is open.
		/// </summary>
		[YamlMember]
		public bool Searchable { get; set; } = false;

		/// <summary>
		/// Height of the search textbox when Searchable is enabled.
		/// </summary>
		[YamlIgnore]
		private float SearchBoxHeight = 24;

		/// <summary>
		/// Current search/filter text.
		/// </summary>
		[YamlIgnore]
		private string SearchText = "";

		/// <summary>
		/// Filtered items based on search text.
		/// </summary>
		[YamlIgnore]
		private List<int> FilteredIndices = new List<int>();

		/// <summary>
		/// Height of the dropdown button
		/// </summary>
		[YamlIgnore]
		private float ButtonHeight = 19;

		public event DropDownItemSelectedFunc OnItemSelected;

		/// <summary>
		/// Custom item renderer. When set, this delegate is called to render each dropdown item
		/// instead of the default text rendering. Allows icons, colors, or complex layouts per item.
		/// </summary>
		[YamlIgnore]
		public DropDownItemRenderFunc CustomItemRenderer { get; set; }

		/// <summary>
		/// Custom height for items when using CustomItemRenderer. Set to 0 to use default font-based height.
		/// </summary>
		[YamlMember]
		public float CustomItemHeight { get; set; } = 0;

		/// <summary>
		/// Whether to allow multiple item selection with checkboxes.
		/// When enabled, clicking an item toggles its selection instead of closing the dropdown.
		/// </summary>
		[YamlMember]
		public bool MultiSelect { get; set; } = false;

		/// <summary>
		/// Set of currently selected indices when MultiSelect is enabled.
		/// </summary>
		[YamlIgnore]
		private HashSet<int> SelectedIndices = new HashSet<int>();

		/// <summary>
		/// Event fired when selection changes in MultiSelect mode.
		/// </summary>
		public event Action<DropDown, int[]> OnMultiSelectionChanged;

		[YamlIgnore]
		Button DDButton;

		public DropDown()
		{
			Size = new Vector2(200, 19);
		}

		/// <summary>
		/// Writes bounded structural selection and popup state. Item text and user data are intentionally
		/// excluded; selected indices remain subject to value redaction during snapshot projection.
		/// </summary>
		public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
		{
			float itemHeight = GetEffectiveItemHeight();
			int hoveredItemIndex = HoveredIndex < 0 ? -1 : FilteredIndexToItemIndex(HoveredIndex);
			int displayedItemCount = GetDisplayedItemCount();
			DiagnosticSelection selection = GetDiagnosticSelection(writer);
			writer.Write("isOpen", IsOpen);
			writer.Write("multiSelect", MultiSelect);
			writer.Write("searchable", Searchable);
			writer.Write("itemCount", Items.Count);
			writer.Write("selectedIndex", SelectedIndex);
			writer.Write("selectedIndices", selection.Indices, selection.Count);
			writer.Write("selectedCount", selection.Count);
			writer.Write("selectedIndicesTruncated", selection.Count > selection.Indices.Length);
			writer.Write("hoveredDisplayIndex", HoveredIndex);
			writer.Write("hoveredItemIndex", hoveredItemIndex);
			writer.Write("searchTextLength", SearchText.Length);
			writer.Write("filteredItemCount", GetFilteredItemCountForDiagnostics());
			writer.Write("displayedItemCount", displayedItemCount);
			writer.Write("visibleItemCount", MaxVisibleItems > 0 ? Math.Min(displayedItemCount, MaxVisibleItems) : displayedItemCount);
			writer.Write("buttonHeightPixels", ButtonHeight);
			writer.Write("itemHeightPixels", itemHeight);
			writer.Write("scrollOffsetPixels", new FishUIDebugPoint(ScrollOffset.X, ScrollOffset.Y));

			if (!IsOpen)
				return;

			Vector2 listPosition = GetDropdownListPosition();
			Vector2 listSize = GetDropdownListSize(itemHeight);
			writer.Write("popupPixels", new FishUIDebugRect(
				listPosition.X, listPosition.Y, listSize.X, listSize.Y));
			if (Searchable)
			{
				writer.Write("searchBoxPixels", new FishUIDebugRect(
					listPosition.X + 2,
					listPosition.Y + 2,
					listSize.X - 4,
					SearchBoxHeight - 4));
			}
		}

		public void AddItem(DropDownItem Itm)
		{
			int oldCount = Items.Count;
			Items.Add(Itm);
			RecordDiagnosticState("itemCount", FormatInt(oldCount), FormatInt(Items.Count));
		}

		public void ClearItems()
		{
			int oldCount = Items.Count;
			int oldSelectedIndex = SelectedIndex;
			DiagnosticSelection? oldSelectedIndices = GetSelectionForDiagnosticEvent();
			Items.Clear();
			SelectedIndex = -1;
			SelectedIndices.Clear();
			RecordDiagnosticState("itemCount", FormatInt(oldCount), FormatInt(Items.Count));
			if (oldSelectedIndex != SelectedIndex)
				RecordDiagnosticState("selectedIndex", FormatInt(oldSelectedIndex), FormatInt(SelectedIndex));
			RecordSelectedIndicesChange(oldSelectedIndices);
		}

		/// <summary>
		/// Gets all currently selected indices in MultiSelect mode.
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
		/// Gets all currently selected items in MultiSelect mode.
		/// </summary>
		public DropDownItem[] GetSelectedItems()
		{
			return GetSelectedIndices().Where(i => i >= 0 && i < Items.Count).Select(i => Items[i]).ToArray();
		}

		/// <summary>
		/// Checks if an item at the given index is selected (works for both single and multi-select).
		/// </summary>
		public bool IsItemSelected(int index)
		{
			if (MultiSelect)
				return SelectedIndices.Contains(index);
			else
				return index == SelectedIndex;
		}

		/// <summary>
		/// Toggles the selection state of an item in MultiSelect mode.
		/// </summary>
		public void ToggleItemSelection(int index)
		{
			if (!MultiSelect || index < 0 || index >= Items.Count)
				return;

			DiagnosticSelection? oldIndices = GetSelectionForDiagnosticEvent();
			if (SelectedIndices.Contains(index))
				SelectedIndices.Remove(index);
			else
				SelectedIndices.Add(index);

			RecordSelectedIndicesChange(oldIndices);
			OnMultiSelectionChanged?.Invoke(this, GetSelectedIndices());
		}

		/// <summary>
		/// Selects all items in MultiSelect mode.
		/// </summary>
		public void SelectAll()
		{
			if (!MultiSelect)
				return;

			DiagnosticSelection? oldIndices = GetSelectionForDiagnosticEvent();
			for (int i = 0; i < Items.Count; i++)
				SelectedIndices.Add(i);

			RecordSelectedIndicesChange(oldIndices);
			OnMultiSelectionChanged?.Invoke(this, GetSelectedIndices());
		}

		/// <summary>
		/// Clears all selections in MultiSelect mode.
		/// </summary>
		public void ClearSelection()
		{
			DiagnosticSelection? oldIndices = GetSelectionForDiagnosticEvent();
			SelectedIndices.Clear();
			RecordSelectedIndicesChange(oldIndices);
			if (MultiSelect)
				OnMultiSelectionChanged?.Invoke(this, GetSelectedIndices());
		}

		public void Open()
		{
			bool wasOpen = IsOpen;
			int oldSearchLength = SearchText.Length;
			int oldFilteredCount = FilteredIndices.Count;
			IsOpen = true;
			// Reset search filter when opening
			SearchText = "";
			UpdateFilteredItems();
			RecordSearchChange(oldSearchLength, oldFilteredCount);
			// Bring dropdown to front and make it always on top while open
			// This ensures the dropdown list appears above other controls
			AlwaysOnTop = true;
			BringToFront();
			// Track this dropdown for overlay rendering
			if (!OpenDropdowns.Contains(this))
				OpenDropdowns.Add(this);
			if (!wasOpen)
				RecordDiagnosticState("dropdownOpen", bool.FalseString, bool.TrueString);
		}

		public void Close()
		{
			bool wasOpen = IsOpen;
			int oldSearchLength = SearchText.Length;
			IsOpen = false;
			HoveredIndex = -1;
			// Clear search filter when closing
			SearchText = "";
			FilteredIndices.Clear();
			// Restore normal Z-order behavior
			AlwaysOnTop = false;
			// Remove from overlay tracking
			OpenDropdowns.Remove(this);
			if (oldSearchLength != 0)
				RecordDiagnosticState("searchTextLength", FormatInt(oldSearchLength), "0");
			if (wasOpen)
				RecordDiagnosticState("dropdownOpen", bool.TrueString, bool.FalseString);
		}

		internal bool BelongsTo(FishUI ui)
		{
			return FishUI == ui;
		}

		internal new bool IsHierarchyVisible()
		{
			Control control = this;
			while (control != null)
			{
				if (!control.Visible)
					return false;
				control = control.GetParent();
			}

			return true;
		}

		/// <summary>
		/// Updates the filtered item indices based on the current search text.
		/// </summary>
		private void UpdateFilteredItems()
		{
			FilteredIndices.Clear();

			if (string.IsNullOrEmpty(SearchText))
			{
				// No filter - show all items
				for (int i = 0; i < Items.Count; i++)
					FilteredIndices.Add(i);
			}
			else
			{
				// Filter items by search text (case-insensitive contains)
				string searchLower = SearchText.ToLowerInvariant();
				for (int i = 0; i < Items.Count; i++)
				{
					if (Items[i].Text.ToLowerInvariant().Contains(searchLower))
						FilteredIndices.Add(i);
				}
			}

			// Reset hovered index when filter changes
			HoveredIndex = -1;
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
				RecordDiagnosticState("selectedIndex", FormatInt(LastSelectedIndex), FormatInt(SelectedIndex));
				// Only broadcast event if control is connected to FishUI (has parent or _FishUI set)
				if (FishUI != null)
				{
					// Legacy broadcast for backward compatibility
					FishUI.Events?.Broadcast(FishUI, this, "item_selected", new object[] { Items[SelectedIndex] });

					// Fire new interface event
					var eventArgs = new FishUISelectionChangedEventArgs(FishUI, this, SelectedIndex, Items[SelectedIndex]);
					FishUI.Events?.OnControlSelectionChanged(eventArgs);
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
			int itemCount = FilteredIndices.Count > 0 ? FilteredIndices.Count : Items.Count;
			int visibleCount = MaxVisibleItems > 0 ? Math.Min(itemCount, MaxVisibleItems) : itemCount;
			float listHeight = visibleCount * itemHeight + 4;
			// Add space for search box if searchable
			if (Searchable)
				listHeight += SearchBoxHeight;
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
			// Adjust for search box height
			float yOffset = Searchable ? SearchBoxHeight : 0;
			int Index = (int)((LocalPos.Y - 2 - yOffset) / ItemHeight);

			int itemCount = FilteredIndices.Count > 0 ? FilteredIndices.Count : Items.Count;
			int visibleCount = MaxVisibleItems > 0 ? Math.Min(itemCount, MaxVisibleItems) : itemCount;
			if (Index < 0 || Index >= visibleCount)
				return -1;

			return Index;
		}

		/// <summary>
		/// Converts a filtered display index to the actual item index.
		/// </summary>
		private int FilteredIndexToItemIndex(int filteredIndex)
		{
			if (filteredIndex < 0)
				return -1;
			if (FilteredIndices.Count > 0 && filteredIndex < FilteredIndices.Count)
				return FilteredIndices[filteredIndex];
			if (filteredIndex < Items.Count)
				return filteredIndex;
			return -1;
		}

		/// <summary>
		/// Checks if a global position is inside the search box area.
		/// </summary>
		private bool IsPointInsideSearchBox(Vector2 GlobalPos)
		{
			if (!Searchable || !IsOpen)
				return false;

			Vector2 listPos = GetDropdownListPosition();
			Vector2 searchPos = listPos + new Vector2(2, 2);
			Vector2 searchSize = new Vector2(Size.X - 4, SearchBoxHeight - 4);

			return Utils.IsInside(searchPos, searchSize, GlobalPos);
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			if (IsOpen)
			{
				float itemHeight = CustomItemHeight > 0 ? CustomItemHeight : UI.Settings.FontDefault.Size + 4;
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

			float itemHeight = CustomItemHeight > 0 ? CustomItemHeight : UI.Settings.FontDefault.Size + 4;

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
				// Check if clicking in search box area - don't close/select
				if (Searchable && IsPointInsideSearchBox(Pos))
				{
					// Just absorb the click, keep dropdown open for typing
					return;
				}

				int itemCount = FilteredIndices.Count > 0 ? FilteredIndices.Count : Items.Count;
				int visibleCount = MaxVisibleItems > 0 ? Math.Min(itemCount, MaxVisibleItems) : itemCount;
				if (HoveredIndex >= 0 && HoveredIndex < visibleCount)
				{
					int actualIndex = FilteredIndexToItemIndex(HoveredIndex);
					if (actualIndex >= 0)
					{
						if (MultiSelect)
						{
							// Toggle selection, don't close dropdown
							ToggleItemSelection(actualIndex);
						}
						else
						{
							SelectIndex(actualIndex);
						}
					}
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
				float itemHeight = CustomItemHeight > 0 ? CustomItemHeight : (ListItemHeight > 0 ? ListItemHeight : 18);
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
			else if (Key == FishKey.Backspace && Searchable)
			{
				// Handle backspace for search
				if (SearchText.Length > 0)
				{
					int oldLength = SearchText.Length;
					int oldFilteredCount = FilteredIndices.Count;
					SearchText = SearchText.Substring(0, SearchText.Length - 1);
					UpdateFilteredItems();
					RecordSearchChange(oldLength, oldFilteredCount);
				}
			}
		}

		public override void HandleTextInput(FishUI UI, FishInputState InState, char Character)
		{
			// Handle text input for search when dropdown is open and searchable
			if (IsOpen && Searchable && !char.IsControl(Character))
			{
				int oldLength = SearchText.Length;
				int oldFilteredCount = FilteredIndices.Count;
				SearchText += Character;
				UpdateFilteredItems();
				RecordSearchChange(oldLength, oldFilteredCount);
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
			AddRuntimeChild(DDButton);
		}

		public override bool IsPointInside(Vector2 GlobalPt)
		{
			// Check if point is inside the button
			if (base.IsPointInside(GlobalPt))
				return true;

			// Also check if point is inside the open dropdown list
			if (IsOpen)
			{
				float itemHeight = CustomItemHeight > 0 ? CustomItemHeight : (ListItemHeight > 0 ? ListItemHeight : 18);
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
				if (MultiSelect)
				{
					// Show count of selected items in multi-select mode
					int count = SelectedIndices.Count;
					if (count == 0)
						selectedText = "";
					else if (count == 1)
						selectedText = Items[SelectedIndices.First()].Text;
					else
						selectedText = $"{count} items selected";
				}
				else if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
				{
					selectedText = Items[SelectedIndex].Text;
				}

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

			// Use custom item height if set, otherwise use font-based height
			float itemHeight = CustomItemHeight > 0 ? CustomItemHeight : UI.Settings.FontDefault.Size + 4;
			Vector2 listPos = GetDropdownListPosition();
			Vector2 listSize = GetDropdownListSize(itemHeight);

			// Draw list background
			NPatch listBg = UI.Settings.ImgListBoxNormal;
			UI.Graphics.DrawNPatch(listBg, listPos, listSize, Color);

			float yOffset = 0;

			// Draw search box if searchable
			if (Searchable)
			{
				// Draw search box background
				NPatch searchBg = UI.Settings.ImgTextboxNormal;
				Vector2 searchPos = listPos + new Vector2(2, 2);
				Vector2 searchSize = new Vector2(listSize.X - 4, SearchBoxHeight - 4);
				UI.Graphics.DrawNPatch(searchBg, searchPos, searchSize, Color);

				// Draw search text or placeholder
				string displayText = string.IsNullOrEmpty(SearchText) ? "Type to filter..." : SearchText;
				FishColor textColor = string.IsNullOrEmpty(SearchText) ? new FishColor(128, 128, 128, 255) : FishColor.Black;
				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, displayText, searchPos + new Vector2(4, 2), textColor);

				yOffset = SearchBoxHeight;
			}

			// Get items to display (filtered or all)
			List<int> displayIndices = FilteredIndices.Count > 0 ? FilteredIndices : Enumerable.Range(0, Items.Count).ToList();
			int itemCount = displayIndices.Count;
			int visibleCount = MaxVisibleItems > 0 ? Math.Min(itemCount, MaxVisibleItems) : itemCount;

			// Draw items
			UI.Graphics.PushScissor(listPos + new Vector2(2, 2 + yOffset), listSize - new Vector2(4, 4 + yOffset));

			for (int i = 0; i < visibleCount; i++)
			{
				int actualIndex = displayIndices[i];
				bool isSelected = MultiSelect ? SelectedIndices.Contains(actualIndex) : (actualIndex == SelectedIndex);
				bool isHovered = (i == HoveredIndex);

				float y = listPos.Y + 2 + yOffset + i * itemHeight;
				Vector2 itemPos = new Vector2(listPos.X + 2, y) + ScrollOffset;
				Vector2 itemSize = new Vector2(listSize.X - 4, itemHeight);

				// Draw selection/hover background
				NPatch itemBg = null;
				if (isHovered)
					itemBg = UI.Settings.ImgSelectionBoxNormal;
				else if (isSelected && !MultiSelect)
					itemBg = UI.Settings.ImgListBoxItmSelected;

				if (itemBg != null)
					UI.Graphics.DrawNPatch(itemBg, itemPos, itemSize, Color);

				// Calculate text offset (checkbox takes space in multi-select mode)
				float textXOffset = MultiSelect ? 20 : 2;

				// Draw checkbox in multi-select mode
				if (MultiSelect)
				{
					float checkSize = 14;
					float checkY = itemPos.Y + (itemHeight - checkSize) / 2;
					Vector2 checkPos = new Vector2(itemPos.X + 2, checkY);

					// Draw checkbox background
					NPatch checkBg = isSelected ? UI.Settings.ImgCheckboxChecked : UI.Settings.ImgCheckboxUnchecked;
					if (checkBg != null)
						UI.Graphics.DrawNPatch(checkBg, checkPos, new Vector2(checkSize, checkSize), Color);
					else
					{
						// Fallback: draw simple checkbox
						UI.Graphics.DrawRectangle(checkPos, new Vector2(checkSize, checkSize), new FishColor(200, 200, 200, 255));
						if (isSelected)
							UI.Graphics.DrawRectangle(checkPos + new Vector2(3, 3), new Vector2(checkSize - 6, checkSize - 6), new FishColor(50, 120, 200, 255));
					}
				}

				// Use custom renderer if set, otherwise default text rendering
				if (CustomItemRenderer != null)
				{
					CustomItemRenderer(UI, Items[actualIndex], itemPos + new Vector2(textXOffset, 0), itemSize - new Vector2(textXOffset + 2, 0), isSelected, isHovered);
				}
				else
				{
					FishColor txtColor = FishColor.Black;
					UI.Graphics.DrawTextColor(UI.Settings.FontDefault, Items[actualIndex].Text, itemPos + new Vector2(textXOffset, 0) + StartOffset, txtColor);
				}
			}

			UI.Graphics.PopScissor();

			// Show "no results" message if filter has no matches
			if (Searchable && !string.IsNullOrEmpty(SearchText) && displayIndices.Count == 0)
			{
				float y = listPos.Y + 2 + yOffset;
				UI.Graphics.DrawTextColor(UI.Settings.FontDefault, "No matching items", new Vector2(listPos.X + 4, y) + StartOffset, new FishColor(128, 128, 128, 255));
			}
		}

		private float GetEffectiveItemHeight()
		{
			if (CustomItemHeight > 0)
				return CustomItemHeight;
			if (ListItemHeight > 0)
				return ListItemHeight;
			if (FishUI?.Settings?.FontDefault != null)
				return FishUI.Settings.FontDefault.Size + 4;
			return 18;
		}

		private int GetDisplayedItemCount()
		{
			return FilteredIndices.Count > 0 ? FilteredIndices.Count : Items.Count;
		}

		private int GetFilteredItemCountForDiagnostics()
		{
			if (!Searchable || string.IsNullOrEmpty(SearchText))
				return Items.Count;
			return FilteredIndices.Count;
		}

		private void RecordSelectedIndicesChange(DiagnosticSelection? oldSelection)
		{
			if (!MultiSelect || !oldSelection.HasValue)
				return;

			DiagnosticSelection newSelection = GetDiagnosticSelection();
			DiagnosticSelection previous = oldSelection.Value;
			if (previous.Count == newSelection.Count && previous.Indices.SequenceEqual(newSelection.Indices))
				return;
			RecordDiagnosticState("selectedIndices", FormatSelection(previous), FormatSelection(newSelection));
		}

		private DiagnosticSelection? GetSelectionForDiagnosticEvent()
		{
			return FishUI?.Diagnostics.IsEventRecordingEnabled == true ? GetDiagnosticSelection() : (DiagnosticSelection?)null;
		}

		private DiagnosticSelection GetDiagnosticSelection()
		{
			if (MultiSelect)
			{
				return new DiagnosticSelection(
					SelectedIndices.Count,
					SelectedIndices.OrderBy(index => index).Take(MaximumDiagnosticSelectionIndices).ToArray());
			}
			if (SelectedIndex >= 0)
				return new DiagnosticSelection(1, new[] { SelectedIndex });
			return new DiagnosticSelection(0, Array.Empty<int>());
		}

		private DiagnosticSelection GetDiagnosticSelection(FishUIDebugSnapshotWriter writer)
		{
			if (!MultiSelect)
				return SelectedIndex >= 0
					? new DiagnosticSelection(1, new[] { SelectedIndex })
					: new DiagnosticSelection(0, Array.Empty<int>());
			var indices = new List<int>();
			foreach (int index in SelectedIndices)
			{
				if (!writer.TryConsumeScanEntry()) break;
				if (indices.Count < writer.MaximumCollectionEntries) indices.Add(index);
			}
			indices.Sort();
			return new DiagnosticSelection(SelectedIndices.Count, indices.ToArray());
		}

		private void RecordSearchChange(int oldLength, int oldFilteredCount)
		{
			if (oldLength != SearchText.Length)
				RecordDiagnosticState("searchTextLength", FormatInt(oldLength), FormatInt(SearchText.Length));
			if (Searchable && (oldLength > 0 || SearchText.Length > 0) && oldFilteredCount != FilteredIndices.Count)
				RecordDiagnosticState("filteredItemCount", FormatInt(oldFilteredCount), FormatInt(FilteredIndices.Count));
		}

		private void RecordDiagnosticState(string name, string oldValue, string newValue)
		{
			if (FishUI?.Diagnostics.IsEventRecordingEnabled != true)
				return;

			FishUI.Diagnostics.Record(
				FishUIDiagnosticEventCategory.StateChange,
				FishUIDiagnosticEventType.StateChanged,
				this,
				null,
				state: new FishUIStateEventData
				{
					Name = name,
					OldValue = oldValue,
					NewValue = newValue
				});
		}

		private static string FormatInt(int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		private static string FormatSelection(DiagnosticSelection selection)
		{
			string result = string.Join(",", selection.Indices);
			return selection.Count > selection.Indices.Length ? result + ",..." : result;
		}
	}
}
