using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FishUI
{
	/// <summary>
	/// Interface for handling FishUI events. Implement this interface to receive
	/// notifications about control interactions and state changes.
	/// </summary>
	public interface IFishUIEvents
	{
		/// <summary>
		/// Legacy broadcast method for backward compatibility.
		/// Consider using the specialized event methods instead.
		/// </summary>
		/// <param name="FUI">The FishUI instance.</param>
		/// <param name="Ctrl">The control that raised the event.</param>
		/// <param name="Name">The event name (e.g., "mouse_click", "item_selected").</param>
		/// <param name="Args">Optional event arguments.</param>
		void Broadcast(FishUI FUI, Control Ctrl, string Name, object[] Args);

		/// <summary>
		/// Called when a control is clicked.
		/// </summary>
		void OnControlClicked(FishUIClickEventArgs e) { }

		/// <summary>
		/// Called when a control is double-clicked.
		/// </summary>
		void OnControlDoubleClicked(FishUIClickEventArgs e) { }

		/// <summary>
		/// Called when the mouse enters a control's bounds.
		/// </summary>
		void OnControlMouseEnter(FishUIMouseEventArgs e) { }

		/// <summary>
		/// Called when the mouse leaves a control's bounds.
		/// </summary>
		void OnControlMouseLeave(FishUIMouseEventArgs e) { }

		/// <summary>
		/// Called when a control's value changes (Slider, NumericUpDown, ProgressBar, etc.).
		/// </summary>
		void OnControlValueChanged(FishUIValueChangedEventArgs e) { }

		/// <summary>
		/// Called when a control's selection changes (ListBox, DropDown, TreeView, etc.).
		/// </summary>
		void OnControlSelectionChanged(FishUISelectionChangedEventArgs e) { }

		/// <summary>
		/// Called when a text control's content changes (Textbox).
		/// </summary>
		void OnControlTextChanged(FishUITextChangedEventArgs e) { }

		/// <summary>
		/// Called when a toggle/checkbox control's checked state changes.
		/// </summary>
		void OnControlCheckedChanged(FishUICheckedChangedEventArgs e) { }

		/// <summary>
		/// Called after a layout has been loaded/deserialized.
		/// </summary>
		void OnLayoutLoaded(FishUILayoutLoadedEventArgs e) { }
	}
}
