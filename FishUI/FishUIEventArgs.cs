using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace FishUI
{
	/// <summary>
	/// Base class for all FishUI event arguments.
	/// </summary>
	public class FishUIEventArgs : EventArgs
	{
		/// <summary>
		/// The FishUI instance that raised the event.
		/// </summary>
		public FishUI UI { get; }

		/// <summary>
		/// The control that raised the event.
		/// </summary>
		public Control Control { get; }

		/// <summary>
		/// Creates a new FishUIEventArgs instance.
		/// </summary>
		/// <param name="ui">The FishUI instance.</param>
		/// <param name="control">The control that raised the event.</param>
		public FishUIEventArgs(FishUI ui, Control control)
		{
			UI = ui;
			Control = control;
		}
	}

	/// <summary>
	/// Event arguments for mouse click events.
	/// </summary>
	public class FishUIClickEventArgs : FishUIEventArgs
	{
		/// <summary>
		/// The mouse button that was clicked.
		/// </summary>
		public FishMouseButton Button { get; }

		/// <summary>
		/// The position of the click in screen coordinates.
		/// </summary>
		public Vector2 Position { get; }

		/// <summary>
		/// Creates a new FishUIClickEventArgs instance.
		/// </summary>
		public FishUIClickEventArgs(FishUI ui, Control control, FishMouseButton button, Vector2 position)
			: base(ui, control)
		{
			Button = button;
			Position = position;
		}
	}

	/// <summary>
	/// Event arguments for value changed events on controls like Slider, NumericUpDown, ProgressBar.
	/// </summary>
	public class FishUIValueChangedEventArgs : FishUIEventArgs
	{
		/// <summary>
		/// The previous value before the change.
		/// </summary>
		public object OldValue { get; }

		/// <summary>
		/// The new value after the change.
		/// </summary>
		public object NewValue { get; }

		/// <summary>
		/// Creates a new FishUIValueChangedEventArgs instance.
		/// </summary>
		public FishUIValueChangedEventArgs(FishUI ui, Control control, object oldValue, object newValue)
			: base(ui, control)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	/// <summary>
	/// Event arguments for selection changed events on controls like ListBox, DropDown, TreeView.
	/// </summary>
	public class FishUISelectionChangedEventArgs : FishUIEventArgs
	{
		/// <summary>
		/// The index of the selected item, or -1 if no selection.
		/// </summary>
		public int SelectedIndex { get; }

		/// <summary>
		/// The selected item object, or null if no selection.
		/// </summary>
		public object SelectedItem { get; }

		/// <summary>
		/// For multi-select controls, the indices of all selected items.
		/// </summary>
		public int[] SelectedIndices { get; }

		/// <summary>
		/// Creates a new FishUISelectionChangedEventArgs instance.
		/// </summary>
		public FishUISelectionChangedEventArgs(FishUI ui, Control control, int selectedIndex, object selectedItem)
			: base(ui, control)
		{
			SelectedIndex = selectedIndex;
			SelectedItem = selectedItem;
			SelectedIndices = selectedIndex >= 0 ? new[] { selectedIndex } : Array.Empty<int>();
		}

		/// <summary>
		/// Creates a new FishUISelectionChangedEventArgs instance for multi-select.
		/// </summary>
		public FishUISelectionChangedEventArgs(FishUI ui, Control control, int[] selectedIndices)
			: base(ui, control)
		{
			SelectedIndices = selectedIndices ?? Array.Empty<int>();
			SelectedIndex = selectedIndices?.Length > 0 ? selectedIndices[0] : -1;
			SelectedItem = null;
		}
	}

	/// <summary>
	/// Event arguments for text changed events on controls like Textbox.
	/// </summary>
	public class FishUITextChangedEventArgs : FishUIEventArgs
	{
		/// <summary>
		/// The previous text before the change.
		/// </summary>
		public string OldText { get; }

		/// <summary>
		/// The new text after the change.
		/// </summary>
		public string NewText { get; }

		/// <summary>
		/// Creates a new FishUITextChangedEventArgs instance.
		/// </summary>
		public FishUITextChangedEventArgs(FishUI ui, Control control, string oldText, string newText)
			: base(ui, control)
		{
			OldText = oldText;
			NewText = newText;
		}
	}

	/// <summary>
	/// Event arguments for checked/toggle state changed events.
	/// </summary>
	public class FishUICheckedChangedEventArgs : FishUIEventArgs
	{
		/// <summary>
		/// The new checked state.
		/// </summary>
		public bool IsChecked { get; }

		/// <summary>
		/// Creates a new FishUICheckedChangedEventArgs instance.
		/// </summary>
		public FishUICheckedChangedEventArgs(FishUI ui, Control control, bool isChecked)
			: base(ui, control)
		{
			IsChecked = isChecked;
		}
	}

	/// <summary>
	/// Event arguments for layout loaded events.
	/// </summary>
	public class FishUILayoutLoadedEventArgs : EventArgs
	{
		/// <summary>
		/// The FishUI instance where the layout was loaded.
		/// </summary>
		public FishUI UI { get; }

		/// <summary>
		/// The file path of the loaded layout, if applicable.
		/// </summary>
		public string FilePath { get; }

		/// <summary>
		/// Creates a new FishUILayoutLoadedEventArgs instance.
		/// </summary>
		public FishUILayoutLoadedEventArgs(FishUI ui, string filePath = null)
		{
			UI = ui;
			FilePath = filePath;
		}
	}

	/// <summary>
	/// Event arguments for mouse hover events.
	/// </summary>
	public class FishUIMouseEventArgs : FishUIEventArgs
	{
		/// <summary>
		/// The current mouse position in screen coordinates.
		/// </summary>
		public Vector2 Position { get; }

		/// <summary>
		/// Creates a new FishUIMouseEventArgs instance.
		/// </summary>
		public FishUIMouseEventArgs(FishUI ui, Control control, Vector2 position)
			: base(ui, control)
		{
			Position = position;
		}
	}
}
