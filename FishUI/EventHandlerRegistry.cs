using FishUI.Controls;
using System;
using System.Collections.Generic;

namespace FishUI
{
	/// <summary>
	/// Delegate for generic control event handlers used with serialization.
	/// </summary>
	/// <param name="sender">The control that raised the event.</param>
	/// <param name="args">Event arguments containing event-specific data.</param>
	public delegate void ControlEventHandler(Control sender, EventHandlerArgs args);

	/// <summary>
	/// Base class for event handler arguments.
	/// </summary>
	public class EventHandlerArgs
	{
		/// <summary>
		/// The FishUI instance.
		/// </summary>
		public FishUI UI { get; set; }

		/// <summary>
		/// The name of the event that was raised.
		/// </summary>
		public string EventName { get; set; }

		public EventHandlerArgs(FishUI ui, string eventName)
		{
			UI = ui;
			EventName = eventName;
		}
	}

	/// <summary>
	/// Event arguments for click events.
	/// </summary>
	public class ClickEventHandlerArgs : EventHandlerArgs
	{
		public FishMouseButton Button { get; set; }

		public ClickEventHandlerArgs(FishUI ui, FishMouseButton button)
			: base(ui, "Click")
		{
			Button = button;
		}
	}

	/// <summary>
	/// Event arguments for value changed events.
	/// </summary>
	public class ValueChangedEventHandlerArgs : EventHandlerArgs
	{
		public object OldValue { get; set; }
		public object NewValue { get; set; }

		public ValueChangedEventHandlerArgs(FishUI ui, object oldValue, object newValue)
			: base(ui, "ValueChanged")
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	/// <summary>
	/// Event arguments for selection changed events.
	/// </summary>
	public class SelectionChangedEventHandlerArgs : EventHandlerArgs
	{
		public int SelectedIndex { get; set; }
		public object SelectedItem { get; set; }

		public SelectionChangedEventHandlerArgs(FishUI ui, int selectedIndex, object selectedItem)
			: base(ui, "SelectionChanged")
		{
			SelectedIndex = selectedIndex;
			SelectedItem = selectedItem;
		}
	}

	/// <summary>
	/// Event arguments for text changed events.
	/// </summary>
	public class TextChangedEventHandlerArgs : EventHandlerArgs
	{
		public string OldText { get; set; }
		public string NewText { get; set; }

		public TextChangedEventHandlerArgs(FishUI ui, string oldText, string newText)
			: base(ui, "TextChanged")
		{
			OldText = oldText;
			NewText = newText;
		}
	}

	/// <summary>
	/// Event arguments for toggle/checked changed events.
	/// </summary>
	public class CheckedChangedEventHandlerArgs : EventHandlerArgs
	{
		public bool IsChecked { get; set; }

		public CheckedChangedEventHandlerArgs(FishUI ui, bool isChecked)
			: base(ui, "CheckedChanged")
		{
			IsChecked = isChecked;
		}
	}

	/// <summary>
	/// Registry for named event handlers that can be referenced from serialized layouts.
	/// Register handlers by name, then reference them in YAML layout files.
	/// </summary>
	/// <example>
	/// // Register a handler in code:
	/// FishUI.EventHandlers.Register("SaveButton_Click", (sender, args) => {
	///     Console.WriteLine("Save clicked!");
	/// });
	/// 
	/// // Reference in YAML:
	/// - !Button
	///   ID: saveButton
	///   Text: Save
	///   OnClickHandler: SaveButton_Click
	/// </example>
	public class EventHandlerRegistry
	{
		private readonly Dictionary<string, ControlEventHandler> _handlers = new Dictionary<string, ControlEventHandler>();

		/// <summary>
		/// Registers an event handler with the specified name.
		/// </summary>
		/// <param name="name">Unique name for the handler.</param>
		/// <param name="handler">The handler delegate.</param>
		public void Register(string name, ControlEventHandler handler)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			_handlers[name] = handler;
		}

		/// <summary>
		/// Unregisters an event handler.
		/// </summary>
		/// <param name="name">Name of the handler to remove.</param>
		/// <returns>True if the handler was found and removed.</returns>
		public bool Unregister(string name)
		{
			return _handlers.Remove(name);
		}

		/// <summary>
		/// Gets a registered event handler by name.
		/// </summary>
		/// <param name="name">Name of the handler.</param>
		/// <returns>The handler delegate, or null if not found.</returns>
		public ControlEventHandler Get(string name)
		{
			if (string.IsNullOrEmpty(name))
				return null;

			_handlers.TryGetValue(name, out var handler);
			return handler;
		}

		/// <summary>
		/// Checks if a handler with the specified name is registered.
		/// </summary>
		/// <param name="name">Name of the handler.</param>
		/// <returns>True if the handler exists.</returns>
		public bool Contains(string name)
		{
			return !string.IsNullOrEmpty(name) && _handlers.ContainsKey(name);
		}

		/// <summary>
		/// Invokes a handler by name if it exists.
		/// </summary>
		/// <param name="name">Name of the handler.</param>
		/// <param name="sender">The control raising the event.</param>
		/// <param name="args">Event arguments.</param>
		/// <returns>True if the handler was found and invoked.</returns>
		public bool Invoke(string name, Control sender, EventHandlerArgs args)
		{
			var handler = Get(name);
			if (handler != null)
			{
				handler(sender, args);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Clears all registered handlers.
		/// </summary>
		public void Clear()
		{
			_handlers.Clear();
		}

		/// <summary>
		/// Gets the names of all registered handlers.
		/// </summary>
		public IEnumerable<string> GetRegisteredNames()
		{
			return _handlers.Keys;
		}
	}
}
