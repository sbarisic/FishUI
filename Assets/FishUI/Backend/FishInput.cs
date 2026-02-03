using FishUI;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

using Vector2 = System.Numerics.Vector2;
using UVector2 = UnityEngine.Vector2;

public class FishInput : IFishUIInput
{
	private Keyboard keyboard;
	private Mouse mouse;
	private Touchscreen touchscreen;
	
	private List<char> textInputBuffer = new List<char>();
	private int textInputIndex = 0;
	private List<Key> pressedKeysThisFrame = new List<Key>();

	public FishInput()
	{
		keyboard = Keyboard.current;
		mouse = Mouse.current;
		touchscreen = Touchscreen.current;

		if (keyboard != null)
		{
			keyboard.onTextInput += OnTextInput;
		}
	}

	private void OnTextInput(char c)
	{
		textInputBuffer.Add(c);
	}

	public void Update()
	{
		// Refresh device references in case they changed
		keyboard = Keyboard.current;
		mouse = Mouse.current;
		touchscreen = Touchscreen.current;

		// Remove consumed text input characters and reset index
		if (textInputIndex > 0)
		{
			textInputBuffer.RemoveRange(0, textInputIndex);
		}
		textInputIndex = 0;

		// Cache pressed keys this frame
		pressedKeysThisFrame.Clear();
		if (keyboard != null)
		{
			foreach (KeyControl key in keyboard.allKeys)
			{
				if (key.wasPressedThisFrame)
				{
					pressedKeysThisFrame.Add(key.keyCode);
				}
			}
		}
	}

	public int GetCharPressed()
	{
		if (textInputIndex < textInputBuffer.Count)
		{
			return textInputBuffer[textInputIndex++];
		}
		return 0;
	}

	public FishKey GetKeyPressed()
	{
		if (pressedKeysThisFrame.Count > 0)
		{
			for (int i = 0; i < pressedKeysThisFrame.Count; i++)
			{
				FishKey fishKey = KeyToFishKey(pressedKeysThisFrame[i]);
				if (fishKey != FishKey.None)
					return fishKey;
			}
		}
		return FishKey.None;
	}

	public Vector2 GetMousePosition()
	{
		if (mouse == null) return Vector2.Zero;
		UVector2 pos = mouse.position.ReadValue();
		return new Vector2(pos.x, Screen.height - pos.y);
	}

	public float GetMouseWheelMove()
	{
		if (mouse == null) return 0f;
		return mouse.scroll.ReadValue().y / 120f;
	}

	public FishTouchPoint[] GetTouchPoints()
	{
		if (touchscreen == null) return new FishTouchPoint[0];

		var touches = touchscreen.touches;
		List<FishTouchPoint> points = new List<FishTouchPoint>();

		foreach (var touch in touches)
		{
			if (touch.isInProgress)
			{
				UVector2 pos = touch.position.ReadValue();
				UVector2 delta = touch.delta.ReadValue();

				points.Add(new FishTouchPoint
				{
					Id = touch.touchId.ReadValue(),
					Position = new Vector2(pos.x, Screen.height - pos.y),
					Delta = new Vector2(delta.x, -delta.y),
					Width = touch.radius.ReadValue().x * 2f,
					TouchType = GetTouchType(touch.phase.ReadValue())
				});
			}
		}

		return points.ToArray();
	}

	private FishTouchType GetTouchType(UnityEngine.InputSystem.TouchPhase phase)
	{
		switch (phase)
		{
			case UnityEngine.InputSystem.TouchPhase.Began: return FishTouchType.Press;
			case UnityEngine.InputSystem.TouchPhase.Moved: return FishTouchType.Motion;
			case UnityEngine.InputSystem.TouchPhase.Stationary: return FishTouchType.Motion;
			case UnityEngine.InputSystem.TouchPhase.Ended: return FishTouchType.Release;
			case UnityEngine.InputSystem.TouchPhase.Canceled: return FishTouchType.Release;
			default: return FishTouchType.Press;
		}
	}

	public bool IsKeyDown(FishKey Key)
	{
		if (keyboard == null) return false;
		Key key = FishKeyToKey(Key);
		if (key == UnityEngine.InputSystem.Key.None) return false;
		return keyboard[key].isPressed;
	}

	public bool IsKeyPressed(FishKey Key)
	{
		if (keyboard == null) return false;
		Key key = FishKeyToKey(Key);
		if (key == UnityEngine.InputSystem.Key.None) return false;
		return keyboard[key].wasPressedThisFrame;
	}

	public bool IsKeyReleased(FishKey Key)
	{
		if (keyboard == null) return false;
		Key key = FishKeyToKey(Key);
		if (key == UnityEngine.InputSystem.Key.None) return false;
		return keyboard[key].wasReleasedThisFrame;
	}

	public bool IsKeyUp(FishKey Key)
	{
		if (keyboard == null) return true;
		Key key = FishKeyToKey(Key);
		if (key == UnityEngine.InputSystem.Key.None) return true;
		return !keyboard[key].isPressed;
	}

	public bool IsMouseDown(FishMouseButton Button)
	{
		if (mouse == null) return false;
		return GetMouseButton(Button)?.isPressed ?? false;
	}

	public bool IsMousePressed(FishMouseButton Button)
	{
		if (mouse == null) return false;
		return GetMouseButton(Button)?.wasPressedThisFrame ?? false;
	}

	public bool IsMouseReleased(FishMouseButton Button)
	{
		if (mouse == null) return false;
		return GetMouseButton(Button)?.wasReleasedThisFrame ?? false;
	}

	public bool IsMouseUp(FishMouseButton Button)
	{
		if (mouse == null) return true;
		return !(GetMouseButton(Button)?.isPressed ?? false);
	}

	private ButtonControl GetMouseButton(FishMouseButton button)
	{
		switch (button)
		{
			case FishMouseButton.Left: return mouse.leftButton;
			case FishMouseButton.Right: return mouse.rightButton;
			case FishMouseButton.Middle: return mouse.middleButton;
			default: return mouse.leftButton;
		}
	}

	private Key FishKeyToKey(FishKey key)
	{
		switch (key)
		{
			case FishKey.A: return Key.A;
			case FishKey.B: return Key.B;
			case FishKey.C: return Key.C;
			case FishKey.D: return Key.D;
			case FishKey.E: return Key.E;
			case FishKey.F: return Key.F;
			case FishKey.G: return Key.G;
			case FishKey.H: return Key.H;
			case FishKey.I: return Key.I;
			case FishKey.J: return Key.J;
			case FishKey.K: return Key.K;
			case FishKey.L: return Key.L;
			case FishKey.M: return Key.M;
			case FishKey.N: return Key.N;
			case FishKey.O: return Key.O;
			case FishKey.P: return Key.P;
			case FishKey.Q: return Key.Q;
			case FishKey.R: return Key.R;
			case FishKey.S: return Key.S;
			case FishKey.T: return Key.T;
			case FishKey.U: return Key.U;
			case FishKey.V: return Key.V;
			case FishKey.W: return Key.W;
			case FishKey.X: return Key.X;
			case FishKey.Y: return Key.Y;
			case FishKey.Z: return Key.Z;
			case FishKey.Zero: return Key.Digit0;
			case FishKey.One: return Key.Digit1;
			case FishKey.Two: return Key.Digit2;
			case FishKey.Three: return Key.Digit3;
			case FishKey.Four: return Key.Digit4;
			case FishKey.Five: return Key.Digit5;
			case FishKey.Six: return Key.Digit6;
			case FishKey.Seven: return Key.Digit7;
			case FishKey.Eight: return Key.Digit8;
			case FishKey.Nine: return Key.Digit9;
			case FishKey.Space: return Key.Space;
			case FishKey.Enter: return Key.Enter;
			case FishKey.Escape: return Key.Escape;
			case FishKey.Backspace: return Key.Backspace;
			case FishKey.Tab: return Key.Tab;
			case FishKey.Left: return Key.LeftArrow;
			case FishKey.Right: return Key.RightArrow;
			case FishKey.Up: return Key.UpArrow;
			case FishKey.Down: return Key.DownArrow;
			case FishKey.LeftShift: return Key.LeftShift;
			case FishKey.RightShift: return Key.RightShift;
			case FishKey.LeftControl: return Key.LeftCtrl;
			case FishKey.RightControl: return Key.RightCtrl;
			case FishKey.LeftAlt: return Key.LeftAlt;
			case FishKey.RightAlt: return Key.RightAlt;
			case FishKey.Delete: return Key.Delete;
			case FishKey.Home: return Key.Home;
			case FishKey.End: return Key.End;
			case FishKey.PageUp: return Key.PageUp;
			case FishKey.PageDown: return Key.PageDown;
			case FishKey.F1: return Key.F1;
			case FishKey.F2: return Key.F2;
			case FishKey.F3: return Key.F3;
			case FishKey.F4: return Key.F4;
			case FishKey.F5: return Key.F5;
			case FishKey.F6: return Key.F6;
			case FishKey.F7: return Key.F7;
			case FishKey.F8: return Key.F8;
			case FishKey.F9: return Key.F9;
			case FishKey.F10: return Key.F10;
			case FishKey.F11: return Key.F11;
			case FishKey.F12: return Key.F12;
			case FishKey.Insert: return Key.Insert;
			case FishKey.CapsLock: return Key.CapsLock;
			case FishKey.ScrollLock: return Key.ScrollLock;
			case FishKey.NumLock: return Key.NumLock;
			case FishKey.PrintScreen: return Key.PrintScreen;
			case FishKey.Pause: return Key.Pause;
			case FishKey.Kp0: return Key.Numpad0;
			case FishKey.Kp1: return Key.Numpad1;
			case FishKey.Kp2: return Key.Numpad2;
			case FishKey.Kp3: return Key.Numpad3;
			case FishKey.Kp4: return Key.Numpad4;
			case FishKey.Kp5: return Key.Numpad5;
			case FishKey.Kp6: return Key.Numpad6;
			case FishKey.Kp7: return Key.Numpad7;
			case FishKey.Kp8: return Key.Numpad8;
			case FishKey.Kp9: return Key.Numpad9;
			case FishKey.KpDecimal: return Key.NumpadPeriod;
			case FishKey.KpDivide: return Key.NumpadDivide;
			case FishKey.KpMultiply: return Key.NumpadMultiply;
			case FishKey.KpSubtract: return Key.NumpadMinus;
			case FishKey.KpAdd: return Key.NumpadPlus;
			case FishKey.KpEnter: return Key.NumpadEnter;
			case FishKey.KpEqual: return Key.NumpadEquals;
			case FishKey.Comma: return Key.Comma;
			case FishKey.Period: return Key.Period;
			case FishKey.Slash: return Key.Slash;
			case FishKey.Backslash: return Key.Backslash;
			case FishKey.LeftBracket: return Key.LeftBracket;
			case FishKey.RightBracket: return Key.RightBracket;
			case FishKey.Semicolon: return Key.Semicolon;
			case FishKey.Apostrophe: return Key.Quote;
			case FishKey.Grave: return Key.Backquote;
			case FishKey.Minus: return Key.Minus;
			case FishKey.Equal: return Key.Equals;
			default: return Key.None;
		}
	}

	private FishKey KeyToFishKey(Key key)
	{
		switch (key)
		{
			case Key.A: return FishKey.A;
			case Key.B: return FishKey.B;
			case Key.C: return FishKey.C;
			case Key.D: return FishKey.D;
			case Key.E: return FishKey.E;
			case Key.F: return FishKey.F;
			case Key.G: return FishKey.G;
			case Key.H: return FishKey.H;
			case Key.I: return FishKey.I;
			case Key.J: return FishKey.J;
			case Key.K: return FishKey.K;
			case Key.L: return FishKey.L;
			case Key.M: return FishKey.M;
			case Key.N: return FishKey.N;
			case Key.O: return FishKey.O;
			case Key.P: return FishKey.P;
			case Key.Q: return FishKey.Q;
			case Key.R: return FishKey.R;
			case Key.S: return FishKey.S;
			case Key.T: return FishKey.T;
			case Key.U: return FishKey.U;
			case Key.V: return FishKey.V;
			case Key.W: return FishKey.W;
			case Key.X: return FishKey.X;
			case Key.Y: return FishKey.Y;
			case Key.Z: return FishKey.Z;
			case Key.Digit0: return FishKey.Zero;
			case Key.Digit1: return FishKey.One;
			case Key.Digit2: return FishKey.Two;
			case Key.Digit3: return FishKey.Three;
			case Key.Digit4: return FishKey.Four;
			case Key.Digit5: return FishKey.Five;
			case Key.Digit6: return FishKey.Six;
			case Key.Digit7: return FishKey.Seven;
			case Key.Digit8: return FishKey.Eight;
			case Key.Digit9: return FishKey.Nine;
			case Key.Space: return FishKey.Space;
			case Key.Enter: return FishKey.Enter;
			case Key.Escape: return FishKey.Escape;
			case Key.Backspace: return FishKey.Backspace;
			case Key.Tab: return FishKey.Tab;
			case Key.LeftArrow: return FishKey.Left;
			case Key.RightArrow: return FishKey.Right;
			case Key.UpArrow: return FishKey.Up;
			case Key.DownArrow: return FishKey.Down;
			case Key.LeftShift: return FishKey.LeftShift;
			case Key.RightShift: return FishKey.RightShift;
			case Key.LeftCtrl: return FishKey.LeftControl;
			case Key.RightCtrl: return FishKey.RightControl;
			case Key.LeftAlt: return FishKey.LeftAlt;
			case Key.RightAlt: return FishKey.RightAlt;
			case Key.Delete: return FishKey.Delete;
			case Key.Home: return FishKey.Home;
			case Key.End: return FishKey.End;
			case Key.PageUp: return FishKey.PageUp;
			case Key.PageDown: return FishKey.PageDown;
			case Key.F1: return FishKey.F1;
			case Key.F2: return FishKey.F2;
			case Key.F3: return FishKey.F3;
			case Key.F4: return FishKey.F4;
			case Key.F5: return FishKey.F5;
			case Key.F6: return FishKey.F6;
			case Key.F7: return FishKey.F7;
			case Key.F8: return FishKey.F8;
			case Key.F9: return FishKey.F9;
			case Key.F10: return FishKey.F10;
			case Key.F11: return FishKey.F11;
			case Key.F12: return FishKey.F12;
			case Key.Insert: return FishKey.Insert;
			case Key.CapsLock: return FishKey.CapsLock;
			case Key.ScrollLock: return FishKey.ScrollLock;
			case Key.NumLock: return FishKey.NumLock;
			case Key.PrintScreen: return FishKey.PrintScreen;
			case Key.Pause: return FishKey.Pause;
			case Key.Numpad0: return FishKey.Kp0;
			case Key.Numpad1: return FishKey.Kp1;
			case Key.Numpad2: return FishKey.Kp2;
			case Key.Numpad3: return FishKey.Kp3;
			case Key.Numpad4: return FishKey.Kp4;
			case Key.Numpad5: return FishKey.Kp5;
			case Key.Numpad6: return FishKey.Kp6;
			case Key.Numpad7: return FishKey.Kp7;
			case Key.Numpad8: return FishKey.Kp8;
			case Key.Numpad9: return FishKey.Kp9;
			case Key.NumpadPeriod: return FishKey.KpDecimal;
			case Key.NumpadDivide: return FishKey.KpDivide;
			case Key.NumpadMultiply: return FishKey.KpMultiply;
			case Key.NumpadMinus: return FishKey.KpSubtract;
			case Key.NumpadPlus: return FishKey.KpAdd;
			case Key.NumpadEnter: return FishKey.KpEnter;
			case Key.NumpadEquals: return FishKey.KpEqual;
			case Key.Comma: return FishKey.Comma;
			case Key.Period: return FishKey.Period;
			case Key.Slash: return FishKey.Slash;
			case Key.Backslash: return FishKey.Backslash;
			case Key.LeftBracket: return FishKey.LeftBracket;
			case Key.RightBracket: return FishKey.RightBracket;
			case Key.Semicolon: return FishKey.Semicolon;
			case Key.Quote: return FishKey.Apostrophe;
			case Key.Backquote: return FishKey.Grave;
			case Key.Minus: return FishKey.Minus;
			case Key.Equals: return FishKey.Equal;
			default: return FishKey.None;
		}
	}
}