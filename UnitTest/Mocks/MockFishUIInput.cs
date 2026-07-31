using System.Numerics;
using FishUI;

namespace UnitTest.Mocks
{
	/// <summary>
	/// Mock input backend for unit testing FishUI without real input devices.
	/// </summary>
	public class MockFishUIInput : IFishUIInput
	{
		// Simulated input state
		public Vector2 MousePosition { get; set; } = Vector2.Zero;
		public float MouseWheel { get; set; } = 0f;
		public string ClipboardContent { get; set; } = "";

		private readonly HashSet<FishKey> _keysDown = new();
		private readonly HashSet<FishKey> _keysPressed = new();
		private readonly HashSet<FishKey> _keysReleased = new();
		private readonly HashSet<FishMouseButton> _mouseDown = new();
		private readonly HashSet<FishMouseButton> _mousePressed = new();
		private readonly HashSet<FishMouseButton> _mouseReleased = new();

		private FishKey _keyPressed = FishKey.None;
		private int _charPressed = 0;
		private FishTouchPoint[] _touchPoints = Array.Empty<FishTouchPoint>();

		// Input simulation methods
		public void SimulateKeyDown(FishKey key)
		{
			if (!_keysDown.Contains(key))
			{
				_keysDown.Add(key);
				_keysPressed.Add(key);
				_keyPressed = key;
			}
		}

		public void SimulateKeyUp(FishKey key)
		{
			_keysDown.Remove(key);
			_keysReleased.Add(key);
		}

		public void SimulateCharTyped(int charCode)
		{
			_charPressed = charCode;
		}

		public void SimulateMouseDown(FishMouseButton button)
		{
			if (!_mouseDown.Contains(button))
			{
				_mouseDown.Add(button);
				_mousePressed.Add(button);
			}
		}

		public void SimulateMouseUp(FishMouseButton button)
		{
			_mouseDown.Remove(button);
			_mouseReleased.Add(button);
		}

		public void SimulateMouseMove(Vector2 position)
		{
			MousePosition = position;
		}

		public void SimulateMouseClick(FishMouseButton button, Vector2 position)
		{
			MousePosition = position;
			SimulateMouseDown(button);
		}

		public void SimulateTouchPoints(params FishTouchPoint[] points)
		{
			_touchPoints = points;
		}

		/// <summary>
		/// Call at the end of each simulated frame to clear per-frame state.
		/// </summary>
		public void EndFrame()
		{
			_keysPressed.Clear();
			_keysReleased.Clear();
			_mousePressed.Clear();
			_mouseReleased.Clear();
			_keyPressed = FishKey.None;
			_charPressed = 0;
			MouseWheel = 0f;
		}

		/// <summary>
		/// Reset all input state.
		/// </summary>
		public void Reset()
		{
			_keysDown.Clear();
			_keysPressed.Clear();
			_keysReleased.Clear();
			_mouseDown.Clear();
			_mousePressed.Clear();
			_mouseReleased.Clear();
			_keyPressed = FishKey.None;
			_charPressed = 0;
			MousePosition = Vector2.Zero;
			MouseWheel = 0f;
			ClipboardContent = "";
			_touchPoints = Array.Empty<FishTouchPoint>();
		}

		// IFishUIInput implementation
		public FishKey GetKeyPressed() => _keyPressed;
		public int GetCharPressed() => _charPressed;
		public bool IsKeyDown(FishKey Key) => _keysDown.Contains(Key);
		public bool IsKeyUp(FishKey Key) => !_keysDown.Contains(Key);
		public bool IsKeyPressed(FishKey Key) => _keysPressed.Contains(Key);
		public bool IsKeyReleased(FishKey Key) => _keysReleased.Contains(Key);
		public Vector2 GetMousePosition() => MousePosition;
		public float GetMouseWheelMove() => MouseWheel;
		public FishTouchPoint[] GetTouchPoints() => _touchPoints;
		public bool IsMouseDown(FishMouseButton Button) => _mouseDown.Contains(Button);
		public bool IsMouseUp(FishMouseButton Button) => !_mouseDown.Contains(Button);
		public bool IsMousePressed(FishMouseButton Button) => _mousePressed.Contains(Button);
		public bool IsMouseReleased(FishMouseButton Button) => _mouseReleased.Contains(Button);
		public string GetClipboardText() => ClipboardContent;
		public void SetClipboardText(string text) => ClipboardContent = text;
	}
}
