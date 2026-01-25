using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace FishUI
{
	/// <summary>
	/// Interface for the input backend. Implement this to use FishUI with different input systems.
	/// </summary>
	public interface IFishUIInput
	{
		/// <summary>
		/// Gets the key that was just pressed this frame.
		/// </summary>
		public FishKey GetKeyPressed();

		/// <summary>
		/// Gets the character that was typed this frame (for text input).
		/// </summary>
		public int GetCharPressed();

		/// <summary>
		/// Returns true if the specified key is currently held down.
		/// </summary>
		public bool IsKeyDown(FishKey Key);

		/// <summary>
		/// Returns true if the specified key is currently up.
		/// </summary>
		public bool IsKeyUp(FishKey Key);

		/// <summary>
		/// Returns true if the specified key was just pressed this frame.
		/// </summary>
		public bool IsKeyPressed(FishKey Key);

		/// <summary>
		/// Returns true if the specified key was just released this frame.
		/// </summary>
		public bool IsKeyReleased(FishKey Key);

		/// <summary>
		/// Gets the current mouse position in screen coordinates.
		/// </summary>
		public Vector2 GetMousePosition();

		/// <summary>
		/// Gets the mouse wheel movement this frame.
		/// </summary>
		public float GetMouseWheelMove();

		/// <summary>
		/// Gets the current touch points for touch input.
		/// </summary>
		public FishTouchPoint[] GetTouchPoints();

		/// <summary>
		/// Returns true if the specified mouse button is currently held down.
		/// </summary>
		public bool IsMouseDown(FishMouseButton Button);

		/// <summary>
		/// Returns true if the specified mouse button is currently up.
		/// </summary>
		public bool IsMouseUp(FishMouseButton Button);

		/// <summary>
		/// Returns true if the specified mouse button was just pressed this frame.
		/// </summary>
		public bool IsMousePressed(FishMouseButton Button);

		/// <summary>
		/// Returns true if the specified mouse button was just released this frame.
		/// </summary>
		public bool IsMouseReleased(FishMouseButton Button);

		/// <summary>
		/// Gets text from the system clipboard. Returns empty string if clipboard is empty or unavailable.
		/// </summary>
		public string GetClipboardText() => "";

		/// <summary>
		/// Sets text to the system clipboard.
		/// </summary>
		public void SetClipboardText(string text) { }
	}
}
