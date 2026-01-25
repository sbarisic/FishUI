using System;

namespace FishUI
{
	/// <summary>
	/// Interface for FishUI logging. Implement this interface to provide
	/// custom logging behavior (file logging, game console, external systems, etc.).
	/// </summary>
	public interface IFishUILogger
	{
		/// <summary>
		/// Logs a general debug message.
		/// </summary>
		/// <param name="message">The message to log.</param>
		void Log(string message);

		/// <summary>
		/// Logs a control event (mouse enter/leave, click, focus, etc.).
		/// </summary>
		/// <param name="controlType">Type name of the control.</param>
		/// <param name="controlId">ID of the control, or null.</param>
		/// <param name="eventName">Name of the event.</param>
		void LogControlEvent(string controlType, string controlId, string eventName);

		/// <summary>
		/// Logs a control event with additional information.
		/// </summary>
		/// <param name="controlType">Type name of the control.</param>
		/// <param name="controlId">ID of the control, or null.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="info">Additional information about the event.</param>
		void LogControlEvent(string controlType, string controlId, string eventName, string info);
	}

	/// <summary>
	/// Default logger implementation that writes to the console.
	/// </summary>
	public class DefaultFishUILogger : IFishUILogger
	{
		/// <summary>
		/// Prefix used for all log messages. Default is "[FishUI]".
		/// </summary>
		public string Prefix { get; set; } = "[FishUI]";

		/// <inheritdoc/>
		public void Log(string message)
		{
			Console.WriteLine($"{Prefix} {message}");
		}

		/// <inheritdoc/>
		public void LogControlEvent(string controlType, string controlId, string eventName)
		{
			Console.WriteLine($"{Prefix} {controlType}({controlId ?? "null"}) - {eventName}");
		}

		/// <inheritdoc/>
		public void LogControlEvent(string controlType, string controlId, string eventName, string info)
		{
			Console.WriteLine($"{Prefix} {controlType}({controlId ?? "null"}) - {eventName} {info}");
		}
	}

	/// <summary>
	/// A null logger that discards all log messages. Useful for disabling logging entirely.
	/// </summary>
	public class NullFishUILogger : IFishUILogger
	{
		/// <inheritdoc/>
		public void Log(string message) { }

		/// <inheritdoc/>
		public void LogControlEvent(string controlType, string controlId, string eventName) { }

		/// <inheritdoc/>
		public void LogControlEvent(string controlType, string controlId, string eventName, string info) { }
	}
}
