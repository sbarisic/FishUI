using System;

namespace FishUI
{
	/// <summary>
	/// Centralized debug logging for FishUI framework.
	/// All debug output can be configured globally via FishUISettings.
	/// </summary>
	public static class FishUIDebug
	{
		private static IFishUILogger _logger = new DefaultFishUILogger();

		/// <summary>
		/// Gets or sets the logger implementation used for all FishUI debug output.
		/// Set to a custom IFishUILogger implementation to redirect logs to file, game console, etc.
		/// Set to NullFishUILogger to disable all logging.
		/// </summary>
		public static IFishUILogger Logger
		{
			get => _logger;
			set => _logger = value ?? new NullFishUILogger();
		}

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
		[Obsolete("Use DrawControlOutlines instead for all controls.")]
		public static bool DrawPanelOutlines
		{
			get => DrawControlOutlines;
			set => DrawControlOutlines = value;
		}

		/// <summary>
		/// Gets or sets whether debug outlines are drawn around all controls.
		/// </summary>
		public static bool DrawControlOutlines { get; set; } = false;

		/// <summary>
		/// Gets or sets the color used for debug outlines. Default is Teal.
		/// </summary>
		public static FishColor OutlineColor { get; set; } = FishColor.Teal;

		/// <summary>
		/// Gets or sets whether focus indicators are drawn around focused controls.
		/// </summary>
		public static bool DrawFocusIndicators { get; set; } = false;

		/// <summary>
		/// Gets or sets the color used for focus indicators. Default is a bright blue.
		/// </summary>
		public static FishColor FocusIndicatorColor { get; set; } = new FishColor(0, 120, 215, 255);

		/// <summary>
		/// Gets or sets whether ListBox selection changes are logged.
		/// </summary>
		public static bool LogListBoxSelection { get; set; } = true;

		/// <summary>
		/// Gets or sets whether tooltip-related events are logged.
		/// Tooltips update every frame and can produce excessive log output.
		/// </summary>
		public static bool LogTooltips { get; set; } = false;

		/// <summary>
		/// Logs a debug message if debug logging is enabled.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Log(string message)
		{
			if (Enabled)
				_logger.Log(message);
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
				_logger.LogControlEvent(controlType, controlId, eventName);
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
				_logger.LogControlEvent(controlType, controlId, eventName, info);
		}

		/// <summary>
		/// Logs a ListBox selection change if ListBox selection logging is enabled.
		/// </summary>
		/// <param name="selectedIndex">The newly selected index.</param>
		public static void LogListBoxSelectionChange(int selectedIndex)
		{
			if (Enabled && LogListBoxSelection)
				_logger.Log($"ListBox selected index: {selectedIndex}");
		}
	}
}
