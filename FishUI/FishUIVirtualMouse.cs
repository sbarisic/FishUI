using System;
using System.Numerics;
using YamlDotNet.Serialization;

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
		/// Whether to process keyboard input automatically in Update().
		/// Set to false for hybrid mode where you manually control position/buttons.
		/// </summary>
		public bool UseKeyboardInput { get; set; } = true;

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

		#region Configurable Key Bindings

		/// <summary>
		/// Keys that move the cursor up.
		/// </summary>
		public FishKey[] MoveUpKeys { get; set; } = new[] { FishKey.Up };

		/// <summary>
		/// Keys that move the cursor down.
		/// </summary>
		public FishKey[] MoveDownKeys { get; set; } = new[] { FishKey.Down };

		/// <summary>
		/// Keys that move the cursor left.
		/// </summary>
		public FishKey[] MoveLeftKeys { get; set; } = new[] { FishKey.Left };

		/// <summary>
		/// Keys that move the cursor right.
		/// </summary>
		public FishKey[] MoveRightKeys { get; set; } = new[] { FishKey.Right };

		/// <summary>
		/// Keys that trigger a left click.
		/// </summary>
		public FishKey[] LeftClickKeys { get; set; } = new[] { FishKey.Space, FishKey.Enter };

		/// <summary>
		/// Keys that trigger a right click.
		/// </summary>
		public FishKey[] RightClickKeys { get; set; } = new[] { FishKey.RightShift };

		#endregion

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
		/// Updates the virtual mouse state. Call this once per frame when Enabled is true.
		/// Handles keyboard input for movement and clicks based on configured key bindings.
		/// </summary>
		/// <param name="input">The input interface to read keyboard state from.</param>
		/// <param name="deltaTime">Time since last frame in seconds.</param>
		/// <param name="screenWidth">Screen width for clamping cursor position.</param>
		/// <param name="screenHeight">Screen height for clamping cursor position.</param>
		public void Update(IFishUIInput input, float deltaTime, int screenWidth, int screenHeight)
		{
			if (!Enabled)
				return;

			if (UseKeyboardInput)
			{
				// Handle movement based on configured keys
				Vector2 moveDirection = Vector2.Zero;

				foreach (var key in MoveUpKeys)
					if (input.IsKeyDown(key)) { moveDirection.Y -= 1; break; }
				foreach (var key in MoveDownKeys)
					if (input.IsKeyDown(key)) { moveDirection.Y += 1; break; }
				foreach (var key in MoveLeftKeys)
					if (input.IsKeyDown(key)) { moveDirection.X -= 1; break; }
				foreach (var key in MoveRightKeys)
					if (input.IsKeyDown(key)) { moveDirection.X += 1; break; }

				Move(moveDirection, deltaTime);

				// Handle left click based on configured keys
				foreach (var key in LeftClickKeys)
				{
					if (input.IsKeyPressed(key))
					{
						PressLeft();
						break;
					}
				}
				foreach (var key in LeftClickKeys)
				{
					if (input.IsKeyReleased(key))
					{
						ReleaseLeft();
						break;
					}
				}

				// Handle right click based on configured keys
				foreach (var key in RightClickKeys)
				{
					if (input.IsKeyPressed(key))
					{
						PressRight();
						break;
					}
				}
				foreach (var key in RightClickKeys)
				{
					if (input.IsKeyReleased(key))
					{
						ReleaseRight();
						break;
					}
				}
			}

			ClampToScreen(screenWidth, screenHeight);
		}

		/// <summary>
		/// Syncs the virtual mouse button states with the real mouse.
		/// Call this in hybrid mode to pass through real mouse clicks.
		/// </summary>
		/// <param name="input">The input interface to read real mouse state from.</param>
		public void SyncButtonsWithRealMouse(IFishUIInput input)
		{
			if (input == null)
				return;

			bool realLeftDown = input.IsMouseDown(FishMouseButton.Left);
			bool realRightDown = input.IsMouseDown(FishMouseButton.Right);

			// Handle left button
			if (realLeftDown && !_leftButtonDown)
				PressLeft();
			else if (!realLeftDown && _leftButtonDown)
				ReleaseLeft();

			// Handle right button
			if (realRightDown && !_rightButtonDown)
				PressRight();
			else if (!realRightDown && _rightButtonDown)
				ReleaseRight();
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
		/// Sets the position from a touch point or absolute input.
		/// Useful for mapping touch input or real mouse to virtual cursor.
		/// </summary>
		/// <param name="x">X coordinate in screen space.</param>
		/// <param name="y">Y coordinate in screen space.</param>
		public void SetPositionFromInput(float x, float y)
		{
			SetPosition(new Vector2(x, y));
		}

		/// <summary>
		/// Syncs the virtual cursor position with the real mouse position.
		/// Call this in your update loop if you want the virtual cursor to follow the real mouse.
		/// </summary>
		/// <param name="input">The input interface to read real mouse position from.</param>
		public void SyncWithRealMouse(IFishUIInput input)
		{
			if (input != null)
			{
				Position = input.GetMousePosition();
				_velocity = Vector2.Zero;
				_accelerationTimer = 0f;
			}
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
		[YamlIgnore]
		public bool IsLeftDown => _leftButtonDown;

		/// <summary>
		/// Gets whether the right button is currently down.
		/// </summary>
		[YamlIgnore]
		public bool IsRightDown => _rightButtonDown;

		/// <summary>
		/// Gets whether the left button was just pressed this frame.
		/// </summary>
		[YamlIgnore]
		public bool IsLeftPressed => _leftButtonPressed;

		/// <summary>
		/// Gets whether the right button was just pressed this frame.
		/// </summary>
		[YamlIgnore]
		public bool IsRightPressed => _rightButtonPressed;

		/// <summary>
		/// Gets whether the left button was just released this frame.
		/// </summary>
		[YamlIgnore]
		public bool IsLeftReleased => _leftButtonReleased;

		/// <summary>
		/// Gets whether the right button was just released this frame.
		/// </summary>
		[YamlIgnore]
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
