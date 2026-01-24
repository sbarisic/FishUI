using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	/// <summary>
	/// Delegate for property value changed events.
	/// </summary>
	public delegate void PropertyValueChangedFunc(PropertyGrid sender, PropertyGridItem item, object oldValue, object newValue);

	/// <summary>
	/// Represents a single property in the PropertyGrid.
	/// </summary>
	public class PropertyGridItem
	{
		/// <summary>
		/// Display name of the property.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Category this property belongs to.
		/// </summary>
		public string Category { get; set; } = "Misc";

		/// <summary>
		/// Description of the property (shown in tooltip).
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// The PropertyInfo for reflection-based access.
		/// </summary>
		[YamlIgnore]
		public PropertyInfo PropertyInfo { get; set; }

		/// <summary>
		/// The object instance containing this property.
		/// </summary>
		[YamlIgnore]
		public object Instance { get; set; }

		/// <summary>
		/// Type of the property.
		/// </summary>
		[YamlIgnore]
		public Type PropertyType => PropertyInfo?.PropertyType;

		/// <summary>
		/// Whether this property can be edited.
		/// </summary>
		public bool IsReadOnly { get; set; }

		/// <summary>
		/// Whether this item is a category header (not an actual property).
		/// </summary>
		public bool IsCategoryHeader { get; set; }

		/// <summary>
		/// Whether the category is expanded.
		/// </summary>
		public bool IsExpanded { get; set; } = true;

		/// <summary>
		/// Depth level for nested objects (0 = root).
		/// </summary>
		public int Depth { get; set; } = 0;

		/// <summary>
		/// Parent item for nested properties.
		/// </summary>
		[YamlIgnore]
		public PropertyGridItem Parent { get; set; }

		/// <summary>
		/// Child items for expandable/nested objects.
		/// </summary>
		[YamlIgnore]
		public List<PropertyGridItem> Children { get; set; } = new List<PropertyGridItem>();

		/// <summary>
		/// Whether this item has children (expandable).
		/// </summary>
		[YamlIgnore]
		public bool HasChildren => Children.Count > 0;

		/// <summary>
		/// Gets the current value of the property.
		/// </summary>
		public object GetValue()
		{
			if (PropertyInfo == null || Instance == null)
				return null;
			try
			{
				return PropertyInfo.GetValue(Instance);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Sets the value of the property.
		/// </summary>
		public bool SetValue(object value)
		{
			if (PropertyInfo == null || Instance == null || IsReadOnly)
				return false;
			try
			{
				PropertyInfo.SetValue(Instance, value);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public override string ToString()
		{
			return IsCategoryHeader ? $"[Category: {Name}]" : $"{Name} = {GetValue()}";
		}
	}

	/// <summary>
	/// A Windows Forms-like property editor control that displays and edits object properties.
	/// Supports categorization, common types (string, int, float, bool, enum), and nested objects.
	/// </summary>
	public class PropertyGrid : Control
	{
		/// <summary>
		/// The object being edited.
		/// </summary>
		[YamlIgnore]
		public object SelectedObject
		{
			get => _selectedObject;
			set
			{
				_selectedObject = value;
				RebuildPropertyList();
			}
		}
		private object _selectedObject;

		/// <summary>
		/// All property items (including category headers).
		/// </summary>
		[YamlIgnore]
		public List<PropertyGridItem> Items { get; private set; } = new List<PropertyGridItem>();

		/// <summary>
		/// Height of each property row.
		/// </summary>
		[YamlMember]
		public float RowHeight { get; set; } = 22f;

		/// <summary>
		/// Width ratio of the name column (0.0 to 1.0).
		/// </summary>
		[YamlMember]
		public float NameColumnRatio { get; set; } = 0.4f;

		/// <summary>
		/// Indentation per depth level in pixels.
		/// </summary>
		[YamlMember]
		public float IndentWidth { get; set; } = 16f;

		/// <summary>
		/// Whether to group properties by category.
		/// </summary>
		[YamlMember]
		public bool GroupByCategory { get; set; } = true;

		/// <summary>
		/// Whether to sort properties alphabetically within categories.
		/// </summary>
		[YamlMember]
		public bool SortAlphabetically { get; set; } = true;

		/// <summary>
		/// Background color of the property grid.
		/// </summary>
		[YamlMember]
		public FishColor BackgroundColor { get; set; } = new FishColor(45, 45, 48, 255);

		/// <summary>
		/// Color of the name column background.
		/// </summary>
		[YamlMember]
		public FishColor NameColumnColor { get; set; } = new FishColor(37, 37, 38, 255);

		/// <summary>
		/// Color of category headers.
		/// </summary>
		[YamlMember]
		public FishColor CategoryColor { get; set; } = new FishColor(60, 60, 65, 255);

		/// <summary>
		/// Color of the separator line between name and value.
		/// </summary>
		[YamlMember]
		public FishColor SeparatorColor { get; set; } = new FishColor(80, 80, 85, 255);

		/// <summary>
		/// Color of alternating rows (even rows).
		/// </summary>
		[YamlMember]
		public FishColor EvenRowColor { get; set; } = new FishColor(45, 45, 48, 255);

		/// <summary>
		/// Color of alternating rows (odd rows).
		/// </summary>
		[YamlMember]
		public FishColor OddRowColor { get; set; } = new FishColor(50, 50, 53, 255);

		/// <summary>
		/// Color of selected row.
		/// </summary>
		[YamlMember]
		public FishColor SelectionColor { get; set; } = new FishColor(51, 153, 255, 255);

		/// <summary>
		/// Color of hovered row.
		/// </summary>
		[YamlMember]
		public FishColor HoverColor { get; set; } = new FishColor(62, 62, 66, 255);

		/// <summary>
		/// Text color for property names.
		/// </summary>
		[YamlMember]
		public FishColor NameTextColor { get; set; } = new FishColor(200, 200, 200, 255);

		/// <summary>
		/// Text color for property values.
		/// </summary>
		[YamlMember]
		public FishColor ValueTextColor { get; set; } = new FishColor(220, 220, 220, 255);

		/// <summary>
		/// Text color for category headers.
		/// </summary>
		[YamlMember]
		public FishColor CategoryTextColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Event fired when a property value changes.
		/// </summary>
		public event PropertyValueChangedFunc OnPropertyValueChanged;

		[YamlIgnore]
		private ScrollBarV _scrollBar;

		[YamlIgnore]
		private float _scrollOffset = 0f;

		[YamlIgnore]
		private PropertyGridItem _selectedItem;

		[YamlIgnore]
		private PropertyGridItem _hoveredItem;

		[YamlIgnore]
		private Control _activeEditor;

		[YamlIgnore]
		private List<PropertyGridItem> _visibleItems = new List<PropertyGridItem>();

		[YamlIgnore]
		private FontRef _font;

		public PropertyGrid()
		{
			Size = new Vector2(300, 400);
		}

		/// <summary>
		/// Rebuilds the property list from the selected object.
		/// </summary>
		public void RebuildPropertyList()
		{
			Items.Clear();
			_selectedItem = null;
			_hoveredItem = null;
			DestroyActiveEditor();

			if (_selectedObject == null)
				return;

			var properties = _selectedObject.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CanRead && IsSupportedType(p.PropertyType))
				.ToList();

			if (GroupByCategory)
			{
				var grouped = properties
					.GroupBy(p => GetCategory(p))
					.OrderBy(g => g.Key);

				foreach (var group in grouped)
				{
					// Add category header
					var categoryItem = new PropertyGridItem
					{
						Name = group.Key,
						IsCategoryHeader = true,
						IsExpanded = true
					};
					Items.Add(categoryItem);

					// Add properties in category
					var propsInCategory = SortAlphabetically
						? group.OrderBy(p => GetDisplayName(p))
						: group.AsEnumerable();

					foreach (var prop in propsInCategory)
					{
						var item = CreatePropertyItem(prop, _selectedObject, categoryItem, 0);
						categoryItem.Children.Add(item);
						Items.Add(item);
					}
				}
			}
			else
			{
				var sortedProps = SortAlphabetically
					? properties.OrderBy(p => GetDisplayName(p))
					: properties.AsEnumerable();

				foreach (var prop in sortedProps)
				{
					var item = CreatePropertyItem(prop, _selectedObject, null, 0);
					Items.Add(item);
				}
			}
		}

		private PropertyGridItem CreatePropertyItem(PropertyInfo prop, object instance, PropertyGridItem parent, int depth)
		{
			var item = new PropertyGridItem
			{
				Name = GetDisplayName(prop),
				Category = GetCategory(prop),
				Description = GetDescription(prop),
				PropertyInfo = prop,
				Instance = instance,
				IsReadOnly = !prop.CanWrite,
				Parent = parent,
				Depth = depth
			};
			return item;
		}

		private bool IsSupportedType(Type type)
		{
			if (type == typeof(string)) return true;
			if (type == typeof(int) || type == typeof(float) || type == typeof(double)) return true;
			if (type == typeof(bool)) return true;
			if (type.IsEnum) return true;
			if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4)) return true;
			return false;
		}

		private string GetCategory(PropertyInfo prop)
		{
			var attr = prop.GetCustomAttribute<CategoryAttribute>();
			return attr?.Category ?? "Misc";
		}

		private string GetDisplayName(PropertyInfo prop)
		{
			var attr = prop.GetCustomAttribute<DisplayNameAttribute>();
			return attr?.DisplayName ?? prop.Name;
		}

		private string GetDescription(PropertyInfo prop)
		{
			var attr = prop.GetCustomAttribute<DescriptionAttribute>();
			return attr?.Description;
		}

		/// <summary>
		/// Refreshes the display without rebuilding the property list.
		/// </summary>
		public void RefreshValues()
		{
			// Values are read dynamically, so just invalidate display
		}

		private void DestroyActiveEditor()
		{
			if (_activeEditor != null)
			{
				RemoveChild(_activeEditor);
				_activeEditor = null;
			}
		}

		private void CreateEditorForItem(FishUI UI, PropertyGridItem item, float x, float y, float width, float height)
		{
			DestroyActiveEditor();

			if (item == null || item.IsCategoryHeader || item.IsReadOnly)
				return;

			var propType = item.PropertyType;
			var currentValue = item.GetValue();

			if (propType == typeof(bool))
			{
				var checkBox = new CheckBox();
				checkBox.IsChecked = currentValue is bool b && b;
				checkBox.Position = new Vector2(x + 2, y + 2);
				checkBox.Size = new Vector2(height - 4, height - 4);
				checkBox.HandleMouseClick(UI, default, FishMouseButton.Left, Vector2.Zero); // Toggle to test
				// Note: CheckBox toggles on click, we'll handle value sync via polling
				AddChild(checkBox);
				_activeEditor = checkBox;
			}
			else if (propType.IsEnum)
			{
				var dropdown = new DropDown();
				dropdown.Position = new Vector2(x, y);
				dropdown.Size = new Vector2(width, height);
				var values = Enum.GetValues(propType);
				foreach (var val in values)
					dropdown.AddItem(val.ToString());
				if (currentValue != null)
					dropdown.SelectIndex(Array.IndexOf(values, currentValue));
				dropdown.OnItemSelected += (sender, ddItem) =>
				{
					var oldValue = item.GetValue();
					int idx = Array.FindIndex(Enum.GetNames(propType), n => n == ddItem.Text);
					if (idx >= 0)
					{
						var newValue = values.GetValue(idx);
						if (item.SetValue(newValue))
							OnPropertyValueChanged?.Invoke(this, item, oldValue, newValue);
					}
				};
				AddChild(dropdown);
				_activeEditor = dropdown;
			}
			else if (propType == typeof(int) || propType == typeof(float) || propType == typeof(double))
			{
				var numericUpDown = new NumericUpDown();
				numericUpDown.Position = new Vector2(x, y);
				numericUpDown.Size = new Vector2(width, height);
				numericUpDown.DecimalPlaces = propType == typeof(int) ? 0 : 2;
				numericUpDown.MinValue = float.MinValue;
				numericUpDown.MaxValue = float.MaxValue;
				if (currentValue != null)
					numericUpDown.Value = Convert.ToSingle(currentValue);
				numericUpDown.OnValueChanged += (sender, value) =>
				{
					var oldValue = item.GetValue();
					object newValue = propType == typeof(int) ? (int)value :
									  propType == typeof(float) ? value :
									  (double)value;
					if (item.SetValue(newValue))
						OnPropertyValueChanged?.Invoke(this, item, oldValue, newValue);
				};
				AddChild(numericUpDown);
				_activeEditor = numericUpDown;
			}
			else // string and other types
			{
				var textbox = new Textbox();
				textbox.Position = new Vector2(x, y);
				textbox.Size = new Vector2(width, height);
				textbox.Text = currentValue?.ToString() ?? "";
				textbox.OnTextChanged += (sender, text) =>
				{
					var oldValue = item.GetValue();
					if (item.SetValue(text))
						OnPropertyValueChanged?.Invoke(this, item, oldValue, text);
				};
				AddChild(textbox);
				_activeEditor = textbox;
			}
		}

		private void UpdateVisibleItems()
		{
			_visibleItems.Clear();

			foreach (var item in Items)
			{
				if (item.IsCategoryHeader)
				{
					_visibleItems.Add(item);
				}
				else if (item.Parent == null || (item.Parent.IsCategoryHeader && item.Parent.IsExpanded))
				{
					_visibleItems.Add(item);
				}
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 absPos = GetAbsolutePosition();
			Vector2 absSize = GetAbsoluteSize();
			IFishUIGfx gfx = UI.Graphics;

			// Get font if not cached
			if (_font == null)
			_font = UI.Settings.FontDefault;

			// Draw background
			gfx.DrawRectangle(absPos, absSize, BackgroundColor);

			UpdateVisibleItems();

			float contentHeight = _visibleItems.Count * RowHeight;
			float visibleHeight = absSize.Y;
			float scrollBarWidth = contentHeight > visibleHeight ? 16 : 0;
			float contentWidth = absSize.X - scrollBarWidth;
			float nameColumnWidth = contentWidth * NameColumnRatio;

			// Draw items
			float y = absPos.Y - _scrollOffset;
			int visibleIndex = 0;

			foreach (var item in _visibleItems)
			{
				float itemY = y + visibleIndex * RowHeight;

				// Skip items outside visible area
				if (itemY + RowHeight < absPos.Y || itemY > absPos.Y + absSize.Y)
				{
					visibleIndex++;
					continue;
				}

				if (item.IsCategoryHeader)
				{
					// Draw category header
					gfx.DrawRectangle(new Vector2(absPos.X, itemY), new Vector2(contentWidth, RowHeight), CategoryColor);
					
					// Draw expand/collapse indicator and text
					string indicator = item.IsExpanded ? "- " : "+ ";
					gfx.DrawTextColor(_font, indicator + item.Name, new Vector2(absPos.X + 4, itemY + 3), CategoryTextColor);
				}
				else
				{
					// Draw property row
					FishColor rowColor = visibleIndex % 2 == 0 ? EvenRowColor : OddRowColor;
					
					if (item == _selectedItem)
						rowColor = SelectionColor;
					else if (item == _hoveredItem)
						rowColor = HoverColor;

					float indent = item.Depth * IndentWidth;

					// Name column background
					gfx.DrawRectangle(new Vector2(absPos.X, itemY), new Vector2(nameColumnWidth, RowHeight), NameColumnColor);
					
					// Value column background
					gfx.DrawRectangle(new Vector2(absPos.X + nameColumnWidth, itemY), new Vector2(contentWidth - nameColumnWidth, RowHeight), rowColor);

					// Separator line
					gfx.DrawRectangle(new Vector2(absPos.X + nameColumnWidth - 1, itemY), new Vector2(1, RowHeight), SeparatorColor);

					// Draw name
					gfx.DrawTextColor(_font, item.Name, new Vector2(absPos.X + 4 + indent, itemY + 3), NameTextColor);

					// Draw value (if no active editor for this item)
					if (_activeEditor == null || _selectedItem != item)
					{
						string valueText = FormatValue(item.GetValue());
						gfx.DrawTextColor(_font, valueText, new Vector2(absPos.X + nameColumnWidth + 4, itemY + 3), ValueTextColor);
					}
				}

				// Draw row separator
				gfx.DrawRectangle(new Vector2(absPos.X, itemY + RowHeight - 1), new Vector2(contentWidth, 1), SeparatorColor);

				visibleIndex++;
			}

			// Draw scrollbar if needed
			if (contentHeight > visibleHeight)
			{
				if (_scrollBar == null)
				{
					_scrollBar = new ScrollBarV();
					_scrollBar.Position = new Vector2(absSize.X - 16, 0);
					_scrollBar.Size = new Vector2(16, absSize.Y);
					_scrollBar.OnScrollChanged += (sender, scroll, dir) => _scrollOffset = scroll * (contentHeight - visibleHeight);
					AddChild(_scrollBar);
				}

				_scrollBar.Visible = true;
				_scrollBar.Size = new Vector2(16, absSize.Y);
			}
			else if (_scrollBar != null)
			{
				_scrollBar.Visible = false;
				_scrollOffset = 0;
			}

			// Draw border
			gfx.DrawRectangleOutline(absPos, absSize, SeparatorColor);
		}

		private string FormatValue(object value)
		{
			if (value == null)
				return "(null)";
			if (value is bool b)
				return b ? "True" : "False";
			if (value is float f)
				return f.ToString("F2");
			if (value is double d)
				return d.ToString("F2");
			if (value is Vector2 v2)
				return $"{v2.X:F2}, {v2.Y:F2}";
			if (value is Vector3 v3)
				return $"{v3.X:F2}, {v3.Y:F2}, {v3.Z:F2}";
			return value.ToString();
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (Btn != FishMouseButton.Left)
				return;

			Vector2 absPos = GetAbsolutePosition();
			float localY = Pos.Y - absPos.Y + _scrollOffset;
			int index = (int)(localY / RowHeight);

			if (index >= 0 && index < _visibleItems.Count)
			{
				var item = _visibleItems[index];

				if (item.IsCategoryHeader)
				{
					// Toggle category expansion
					item.IsExpanded = !item.IsExpanded;
					DestroyActiveEditor();
				}
				else
				{
					// Select property
					_selectedItem = item;

					// Create editor in value column
					float scrollBarWidth = _scrollBar?.Visible == true ? 16 : 0;
					float contentWidth = GetAbsoluteSize().X - scrollBarWidth;
					float nameColumnWidth = contentWidth * NameColumnRatio;
					float valueWidth = contentWidth - nameColumnWidth;
					float itemY = index * RowHeight - _scrollOffset;

					CreateEditorForItem(UI, item, nameColumnWidth, itemY, valueWidth, RowHeight);
				}
			}
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);

			Vector2 absPos = GetAbsolutePosition();
			float localY = Pos.Y - absPos.Y + _scrollOffset;
			int index = (int)(localY / RowHeight);

			if (index >= 0 && index < _visibleItems.Count)
				_hoveredItem = _visibleItems[index];
			else
				_hoveredItem = null;
		}

		public override void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			base.HandleMouseLeave(UI, InState);
			_hoveredItem = null;
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float WheelDelta)
		{
			float contentHeight = _visibleItems.Count * RowHeight;
			float visibleHeight = GetAbsoluteSize().Y;
			float maxScroll = Math.Max(0, contentHeight - visibleHeight);

			_scrollOffset -= WheelDelta * RowHeight * 3;
			_scrollOffset = Math.Clamp(_scrollOffset, 0, maxScroll);

			if (_scrollBar != null && _scrollBar.Visible && maxScroll > 0)
			{
				_scrollBar.ThumbPosition = _scrollOffset / maxScroll;
			}
		}
	}
}
