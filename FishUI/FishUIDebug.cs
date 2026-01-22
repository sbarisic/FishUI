using System;

namespace FishUI
{
	/// <summary>
	/// Centralized debug logging for FishUI framework.
	/// All debug output can be configured globally via FishUISettings.
	/// </summary>
	public static class FishUIDebug
	{
		/// <summary>
		/// Gets or sets whether debug logging is globally enabled.
		/// </summary>
		public static bool Enabled { get; set; } = true;

		/// <summary>
		/// Gets or sets whether control debug messages are logged (mouse enter/leave, click, etc).
		/// </summary>
		public static bool LogControlEvents { get; set; } = true;

		/// <summary>
		/// Gets or sets whether panel debug outlines are drawn.
		/// </summary>
		public static bool DrawPanelOutlines { get; set; } = true;

		/// <summary>
		/// Gets or sets whether ListBox selection changes are logged.
		/// </summary>
		public static bool LogListBoxSelection { get; set; } = true;

		/// <summary>
		/// Logs a debug message if debug logging is enabled.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Log(string message)
		{
			if (Enabled)
				Console.WriteLine($"[FishUI] {message}");
		}

		/// <summary>
		/// Logs a control event message if control event logging is enabled.
		/// </summary>
		/// <param name="controlType">Type name of the control.</param>
		/// <param name="controlId">ID of the control, or null.</param>
		/// <param name="eventName">Name of the event.</param>
		public static void LogControlEvent(string controlType, string controlId, string eventName)
		{
			if (Enabled && LogControlEvents)
				Console.WriteLine($"[FishUI] {controlType}({controlId ?? "null"}) - {eventName}");
		}

		/// <summary>
		/// Logs a control event message with additional info if control event logging is enabled.
		/// </summary>
		/// <param name="controlType">Type name of the control.</param>
		/// <param name="controlId">ID of the control, or null.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="info">Additional information.</param>
		public static void LogControlEvent(string controlType, string controlId, string eventName, string info)
		{
			if (Enabled && LogControlEvents)
				Console.WriteLine($"[FishUI] {controlType}({controlId ?? "null"}) - {eventName} {info}");
		}

		/// <summary>
		/// Logs a ListBox selection change if ListBox selection logging is enabled.
		/// </summary>
		/// <param name="selectedIndex">The newly selected index.</param>
		public static void LogListBoxSelectionChange(int selectedIndex)
		{
			if (Enabled && LogListBoxSelection)
				Console.WriteLine($"[FishUI] ListBox selected index: {selectedIndex}");
		}
	}
}
