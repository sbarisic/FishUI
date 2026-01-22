using System;
using System.Numerics;

namespace FishUI
{
	/// <summary>
	/// Virtual mouse cursor system for keyboard/gamepad-driven UI navigation.
	/// Allows controlling a software mouse cursor with directional input.
	/// </summary>
	public class FishUIVirtualMouse
	{
		/// <summary>
		/// Whether the virtual mouse is enabled.
		/// </summary>
		public bool Enabled { get; set; } = false;

		/// <summary>
		/// Current position of the virtual mouse cursor.
		/// </summary>
		public Vector2 Position { get; set; }

		/// <summary>
		/// Movement speed in pixels per second.
		/// </summary>
		public float Speed { get; set; } = 300f;

		/// <summary>
		/// Acceleration multiplier when moving continuously.
		/// </summary>
		public float Acceleration { get; set; } = 1.5f;

		/// <summary>
		/// Maximum speed multiplier from acceleration.
		/// </summary>
		public float MaxSpeedMultiplier { get; set; } = 3f;

		/// <summary>
		/// Whether to draw the virtual mouse cursor.
		/// </summary>
		public bool DrawCursor { get; set; } = true;

		/// <summary>
		/// Size of the cursor in pixels.
		/// </summary>
		public float CursorSize { get; set; } = 16f;

		/// <summary>
		/// Color of the virtual mouse cursor.
		/// </summary>
		public FishColor CursorColor { get; set; } = new FishColor(255, 255, 255, 255);

		/// <summary>
		/// Color of the cursor outline.
		/// </summary>
		public FishColor CursorOutlineColor { get; set; } = new FishColor(0, 0, 0, 255);

		/// <summary>
		/// Optional custom cursor image.
		/// </summary>
		public ImageRef CursorImage { get; set; }

		// Internal state
		private Vector2 _velocity;
		private float _accelerationTimer;
		private bool _leftButtonDown;
		private bool _rightButtonDown;
		private bool _leftButtonPressed;
		private bool _rightButtonPressed;
		private bool _leftButtonReleased;
		private bool _rightButtonReleased;

		/// <summary>
		/// Initializes the virtual mouse at the center of the screen.
		/// </summary>
		public void Initialize(int screenWidth, int screenHeight)
		{
			Position = new Vector2(screenWidth / 2f, screenHeight / 2f);
		}

		/// <summary>
		/// Moves the virtual mouse in the specified direction.
		/// </summary>
		/// <param name="direction">Normalized direction vector.</param>
		/// <param name="deltaTime">Time since last frame in seconds.</param>
		public void Move(Vector2 direction, float deltaTime)
		{
			if (direction.LengthSquared() > 0.01f)
			{
				_accelerationTimer += deltaTime;
				float speedMult = Math.Min(1f + (_accelerationTimer * Acceleration), MaxSpeedMultiplier);
				
				if (direction.LengthSquared() > 1f)
					direction = Vector2.Normalize(direction);
				
				_velocity = direction * Speed * speedMult;
			}
			else
			{
				_accelerationTimer = 0f;
				_velocity = Vector2.Zero;
			}

			Position += _velocity * deltaTime;
		}

		/// <summary>
		/// Clamps the cursor position to the screen bounds.
		/// </summary>
		public void ClampToScreen(int screenWidth, int screenHeight)
		{
			Position = new Vector2(
				Math.Clamp(Position.X, 0, screenWidth),
				Math.Clamp(Position.Y, 0, screenHeight)
			);
		}

		/// <summary>
		/// Sets the position of the virtual mouse directly.
		/// </summary>
		public void SetPosition(Vector2 position)
		{
			Position = position;
			_velocity = Vector2.Zero;
			_accelerationTimer = 0f;
		}

		/// <summary>
		/// Simulates pressing the left mouse button.
		/// </summary>
		public void PressLeft()
		{
			if (!_leftButtonDown)
			{
				_leftButtonDown = true;
				_leftButtonPressed = true;
			}
		}

		/// <summary>
		/// Simulates releasing the left mouse button.
		/// </summary>
		public void ReleaseLeft()
		{
			if (_leftButtonDown)
			{
				_leftButtonDown = false;
				_leftButtonReleased = true;
			}
		}

		/// <summary>
		/// Simulates pressing the right mouse button.
		/// </summary>
		public void PressRight()
		{
			if (!_rightButtonDown)
			{
				_rightButtonDown = true;
				_rightButtonPressed = true;
			}
		}

		/// <summary>
		/// Simulates releasing the right mouse button.
		/// </summary>
		public void ReleaseRight()
		{
			if (_rightButtonDown)
			{
				_rightButtonDown = false;
				_rightButtonReleased = true;
			}
		}

		/// <summary>
		/// Simulates a left click (press and release).
		/// </summary>
		public void ClickLeft()
		{
			PressLeft();
			// Release will be called on next frame
		}

		/// <summary>
		/// Simulates a right click (press and release).
		/// </summary>
		public void ClickRight()
		{
			PressRight();
		}

		/// <summary>
		/// Gets whether the left button is currently down.
		/// </summary>
		public bool IsLeftDown => _leftButtonDown;

		/// <summary>
		/// Gets whether the right button is currently down.
		/// </summary>
		public bool IsRightDown => _rightButtonDown;

		/// <summary>
		/// Gets whether the left button was just pressed this frame.
		/// </summary>
		public bool IsLeftPressed => _leftButtonPressed;

		/// <summary>
		/// Gets whether the right button was just pressed this frame.
		/// </summary>
		public bool IsRightPressed => _rightButtonPressed;

		/// <summary>
		/// Gets whether the left button was just released this frame.
		/// </summary>
		public bool IsLeftReleased => _leftButtonReleased;

		/// <summary>
		/// Gets whether the right button was just released this frame.
		/// </summary>
		public bool IsRightReleased => _rightButtonReleased;

		/// <summary>
		/// Called at the end of each frame to clear one-frame states.
		/// </summary>
		internal void EndFrame()
		{
			_leftButtonPressed = false;
			_rightButtonPressed = false;
			_leftButtonReleased = false;
			_rightButtonReleased = false;
		}

		/// <summary>
		/// Draws the virtual mouse cursor.
		/// </summary>
		internal void Draw(IFishUIGfx graphics)
		{
			if (!Enabled || !DrawCursor)
				return;

			if (CursorImage != null)
			{
				graphics.DrawImage(CursorImage, Position, 0, 1, CursorColor);
			}
			else
			{
				// Draw a simple arrow cursor
				Vector2 p1 = Position;
				Vector2 p2 = Position + new Vector2(0, CursorSize);
				Vector2 p3 = Position + new Vector2(CursorSize * 0.4f, CursorSize * 0.7f);
				Vector2 p4 = Position + new Vector2(CursorSize * 0.7f, CursorSize * 0.4f);

				// Draw outline
				graphics.DrawLine(p1, p2, 3, CursorOutlineColor);
				graphics.DrawLine(p2, p3, 3, CursorOutlineColor);
				graphics.DrawLine(p3, p4, 3, CursorOutlineColor);
				graphics.DrawLine(p4, p1, 3, CursorOutlineColor);

				// Draw fill
				graphics.DrawLine(p1, p2, 1, CursorColor);
				graphics.DrawLine(p2, p3, 1, CursorColor);
				graphics.DrawLine(p3, p4, 1, CursorColor);
				graphics.DrawLine(p4, p1, 1, CursorColor);
			}
		}
	}
}
