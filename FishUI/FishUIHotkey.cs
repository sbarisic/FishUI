using System;
using System.Collections.Generic;

namespace FishUI
{
	/// <summary>
	/// Represents a keyboard modifier key combination.
	/// </summary>
	[Flags]
	public enum FishKeyModifiers
	{
		None = 0,
		Shift = 1,
		Control = 2,
		Alt = 4
	}

	/// <summary>
	/// Represents a global keyboard hotkey binding.
	/// </summary>
	public class FishUIHotkey
	{
		/// <summary>
		/// The primary key for this hotkey.
		/// </summary>
		public FishKey Key { get; set; }

		/// <summary>
		/// Required modifier keys (Shift, Control, Alt).
		/// </summary>
		public FishKeyModifiers Modifiers { get; set; }

		/// <summary>
		/// Optional identifier for the hotkey.
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// Action to execute when the hotkey is triggered.
		/// </summary>
		public Action<FishUIHotkey> Action { get; set; }

		/// <summary>
		/// If true, the hotkey is active and will be processed.
		/// </summary>
		public bool Enabled { get; set; } = true;

		public FishUIHotkey()
		{
		}

		public FishUIHotkey(FishKey key, FishKeyModifiers modifiers, Action<FishUIHotkey> action, string id = null)
		{
			Key = key;
			Modifiers = modifiers;
			Action = action;
			ID = id;
		}

		public override string ToString()
		{
			string result = "";
			
			if (Modifiers.HasFlag(FishKeyModifiers.Control))
				result += "Ctrl+";
			if (Modifiers.HasFlag(FishKeyModifiers.Shift))
				result += "Shift+";
			if (Modifiers.HasFlag(FishKeyModifiers.Alt))
				result += "Alt+";
			
			result += Key.ToString();
			
			return result;
		}
	}

	/// <summary>
	/// Manages global keyboard hotkeys for the UI.
	/// </summary>
	public class FishUIHotkeyManager
	{
		private List<FishUIHotkey> _hotkeys = new List<FishUIHotkey>();

		/// <summary>
		/// Registers a new hotkey.
		/// </summary>
		public FishUIHotkey Register(FishKey key, FishKeyModifiers modifiers, Action<FishUIHotkey> action, string id = null)
		{
			var hotkey = new FishUIHotkey(key, modifiers, action, id);
			_hotkeys.Add(hotkey);
			return hotkey;
		}

		/// <summary>
		/// Registers a simple hotkey without modifiers.
		/// </summary>
		public FishUIHotkey Register(FishKey key, Action<FishUIHotkey> action, string id = null)
		{
			return Register(key, FishKeyModifiers.None, action, id);
		}

		/// <summary>
		/// Unregisters a hotkey by reference.
		/// </summary>
		public bool Unregister(FishUIHotkey hotkey)
		{
			return _hotkeys.Remove(hotkey);
		}

		/// <summary>
		/// Unregisters a hotkey by ID.
		/// </summary>
		public bool Unregister(string id)
		{
			return _hotkeys.RemoveAll(h => h.ID == id) > 0;
		}

		/// <summary>
		/// Clears all registered hotkeys.
		/// </summary>
		public void Clear()
		{
			_hotkeys.Clear();
		}

		/// <summary>
		/// Gets a hotkey by ID.
		/// </summary>
		public FishUIHotkey GetByID(string id)
		{
			return _hotkeys.Find(h => h.ID == id);
		}

		/// <summary>
		/// Checks if a hotkey matches the current key and modifier state.
		/// </summary>
		internal bool CheckHotkey(FishUIHotkey hotkey, FishKey keyPressed, IFishUIInput input)
		{
			if (!hotkey.Enabled || hotkey.Key != keyPressed)
				return false;

			bool shiftRequired = hotkey.Modifiers.HasFlag(FishKeyModifiers.Shift);
			bool ctrlRequired = hotkey.Modifiers.HasFlag(FishKeyModifiers.Control);
			bool altRequired = hotkey.Modifiers.HasFlag(FishKeyModifiers.Alt);

			bool shiftDown = input.IsKeyDown(FishKey.LeftShift) || input.IsKeyDown(FishKey.RightShift);
			bool ctrlDown = input.IsKeyDown(FishKey.LeftControl) || input.IsKeyDown(FishKey.RightControl);
			bool altDown = input.IsKeyDown(FishKey.LeftAlt) || input.IsKeyDown(FishKey.RightAlt);

			return shiftRequired == shiftDown && ctrlRequired == ctrlDown && altRequired == altDown;
		}

		/// <summary>
		/// Processes a key press and triggers any matching hotkeys.
		/// Returns true if a hotkey was triggered.
		/// </summary>
		internal bool ProcessKeyPress(FishKey keyPressed, IFishUIInput input)
		{
			if (keyPressed == FishKey.None)
				return false;

			foreach (var hotkey in _hotkeys)
			{
				if (CheckHotkey(hotkey, keyPressed, input))
				{
					hotkey.Action?.Invoke(hotkey);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets all registered hotkeys.
		/// </summary>
		public IReadOnlyList<FishUIHotkey> GetAll()
		{
			return _hotkeys.AsReadOnly();
		}
	}
}
