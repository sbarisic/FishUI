using FishUI.Controls;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace FishUI
{
	public class FishUI
	{
		public FishUISettings Settings;
		public IFishUIGfx Graphics;
		public IFishUIInput Input;
		public IFishUIEvents Events;

		List<Control> Controls;
		public int Width;
		public int Height;

		public Control InputActiveControl;

		//
		Control HoveredControl;
		Control LeftClickedControl;
		Control RightClickedControl;

		// Double-click detection
		float LastLeftClickTime = -1f;
		float LastRightClickTime = -1f;
		Vector2 LastLeftClickPos;
		Vector2 LastRightClickPos;
		Control LastLeftClickControl;
		Control LastRightClickControl;

		/// <summary>
		/// Maximum time between clicks for a double-click (in seconds).
		/// </summary>
		public float DoubleClickTime { get; set; } = 0.3f;

	/// <summary>
		/// Maximum distance between clicks for a double-click (in pixels).
		/// </summary>
		public float DoubleClickDistance { get; set; } = 5f;

	/// <summary>
		/// Manager for global keyboard hotkeys.
		/// </summary>
		public FishUIHotkeyManager Hotkeys { get; } = new FishUIHotkeyManager();

		/// <summary>
		/// Virtual mouse cursor for keyboard/gamepad input.
		/// </summary>
		public FishUIVirtualMouse VirtualMouse { get; } = new FishUIVirtualMouse();

		/// <summary>
		/// The current modal control (if any). When set, only this control and its children receive input.
		/// </summary>
		public Control ModalControl { get; private set; }

		// Z-order management
		private int _nextZDepth = 0;

		// Tooltip management
		private Controls.Tooltip _activeTooltip;
		private Control _tooltipTargetControl;
		private float _tooltipHoverTime = 0f;

		/// <summary>
		/// Delay in seconds before showing tooltips.
		/// </summary>
		public float TooltipShowDelay { get; set; } = 0.5f;


		public FishUI(FishUISettings Settings, IFishUIGfx Graphics, IFishUIInput Input, IFishUIEvents Events)
		{
		Controls = new List<Control>();

		this.Settings = Settings;
		this.Graphics = Graphics;
		this.Input = Input;
		this.Events = Events;

		// Create the global tooltip
		_activeTooltip = new Controls.Tooltip();
		_activeTooltip._FishUI = this;
		}

		public void Init()
		{
			Graphics.Init();
			Settings.Init(this);
		}

		/// <summary>
		/// Gets the next available Z-depth value for a control.
		/// </summary>
		internal int GetNextZDepth()
		{
			return _nextZDepth++;
		}

		/// <summary>
		/// Gets the highest Z-depth among all controls (excluding AlwaysOnTop controls).
		/// </summary>
		internal int GetHighestZDepth()
		{
			int highest = 0;
			foreach (var c in Controls)
			{
				if (!c.AlwaysOnTop && c.ZDepth > highest)
					highest = c.ZDepth;
			}
			return highest;
		}

		/// <summary>
		/// Gets the lowest Z-depth among all controls.
		/// </summary>
		internal int GetLowestZDepth()
		{
			int lowest = int.MaxValue;
			foreach (var c in Controls)
			{
				if (c.ZDepth < lowest)
					lowest = c.ZDepth;
			}
			return lowest == int.MaxValue ? 0 : lowest;
		}

		/// <summary>
		/// Sets the modal control. Only this control (and its children) will receive input.
		/// Pass null to clear modal mode.
		/// </summary>
		public void SetModalControl(Control control)
		{
			ModalControl = control;
			if (control != null)
			{
				// Bring modal control to front
				control.BringToFront();
			}
		}

		public void AddControl(Control C)
		{
			C._FishUI = this;
			C.ZDepth = GetNextZDepth();
			Controls.Add(C);
	}

		public void RemoveAllControls()
		{
			//Control[] Ctrls = GetOrderedControls();
			Controls.Clear();
			ModalControl = null;
		}

		/// <summary>
		/// Gets controls ordered by Z-depth for rendering (lowest first, AlwaysOnTop last).
		/// </summary>
		public Control[] GetOrderedControls()
		{
			// Sort: normal controls by ZDepth, then AlwaysOnTop controls by ZDepth
			var normal = Controls.Where(c => !c.AlwaysOnTop).OrderBy(c => c.ZDepth);
			var alwaysOnTop = Controls.Where(c => c.AlwaysOnTop).OrderBy(c => c.ZDepth);
			return normal.Concat(alwaysOnTop).ToArray();
		}

		public Control[] GetAllControls()
		{
			return Controls.ToArray();
		}

		/// <summary>
		/// Checks if a control is allowed to receive input (respects modal blocking).
		/// </summary>
		private bool IsControlInputAllowed(Control control)
		{
			if (ModalControl == null)
				return true;

			// Check if control is the modal control or a descendant of it
			Control c = control;
			while (c != null)
			{
				if (c == ModalControl)
					return true;
				c = c.GetParent();
		}
			return false;
	}

		/// <summary>
		/// Gets the root-level control that contains the given control.
		/// </summary>
		private Control GetRootControl(Control control)
		{
			if (control == null)
				return null;

			Control root = control;
			while (root.GetParent() != null)
			{
				root = root.GetParent();
			}
			return root;
		}

		/// <summary>
		/// Brings the root-level parent of a control to the front.
		/// This is called automatically on mouse press.
		/// </summary>
		private void BringControlToFrontOnClick(Control control)
		{
			if (control == null)
				return;

			Control root = GetRootControl(control);
			if (root != null && Controls.Contains(root) && !root.AlwaysOnTop)
			{
				root.BringToFront();
			}
		}

	// Top-down control picking, for mouse events etc
		Control PickControl(Control[] Controls, Vector2 GlobalPos)
		{
			foreach (Control C in Controls)
			{
				if (!C.Visible)
					continue;

				if (C.IsPointInside(GlobalPos))
				{
					// Reverse children order so we check front children (higher Z-depth) first
					Control CPicked = PickControl(C.GetAllChildren().Reverse().ToArray(), GlobalPos);

					if (CPicked != null)
					{
						// Check modal blocking
						if (!IsControlInputAllowed(CPicked))
							return null;
						return CPicked;
					}

					// Check modal blocking
					if (!IsControlInputAllowed(C))
						return null;
					return C;
				}
			}

			return null;
		}

	public Control PickControl(Vector2 GlobalPos)
		{
			// Reverse the order so we check front controls (higher Z-depth) first
			return PickControl(GetOrderedControls().Reverse().ToArray(), GlobalPos);
		}

		Control FindControlByIDEx(Control[] Ctrls, string ID)
		{
			foreach (Control C in Ctrls)
			{
				if (C.ID == ID)
					return C;

				Control Ret = FindControlByIDEx(C.GetAllChildren(), ID);
				if (Ret != null)
					return Ret;
			}

			return null;
		}

		public Control FindControlByID(string ID)
		{
			return FindControlByIDEx(Controls.ToArray(), ID);
		}

		void UpdateSingleControl(Control Ctl, FishInputState InState, FishInputState InLast)
		{
			if (!Ctl.Visible)
				return;

			Ctl.IsMousePressed = LeftClickedControl == Ctl;
			Ctl.IsMouseInside = HoveredControl == Ctl;

			Control[] Children = Ctl.GetAllChildren();

			foreach (Control C in Children)
				UpdateSingleControl(C, InState, InLast);
		}

	// Check for mouse press
		// Mouse press gets triggered for the first control under the mouse
		void CheckMousePress(Control ControlUnderMouse, FishInputState InState, bool BtnPressed, FishMouseButton MBtn, ref Control ClickedControl)
		{
			if (BtnPressed)
			{
				if (ControlUnderMouse != null)
				{
					// Bring the root control to front (for windows, panels, etc.)
					BringControlToFrontOnClick(ControlUnderMouse);

					ControlUnderMouse.HandleMousePress(this, InState, MBtn, InState.MousePos);
					ClickedControl = ControlUnderMouse;
					FocusControl(ControlUnderMouse);
				}
			}
		}

	// Check for mouse release and clicks
		// Mouse release gets triggered for the first control under the mouse
		// Mouse click gets triggered only after release if the control under the mouse is the same as the one that was pressed
		void CheckMouseRelease(Control ControlUnderMouse, FishInputState InState, bool BtnReleased, FishMouseButton MBtn, ref Control ClickedControl, float Time)
		{
			if (BtnReleased)
			{
				if (ControlUnderMouse != null)
					ControlUnderMouse.HandleMouseRelease(this, InState, MBtn, InState.MousePos);

				if (ClickedControl != null && ControlUnderMouse == ClickedControl)
				{
					// Check for double-click
					bool isDoubleClick = false;

					if (MBtn == FishMouseButton.Left)
					{
						float timeSinceLastClick = Time - LastLeftClickTime;
						float distance = Vector2.Distance(InState.MousePos, LastLeftClickPos);

						if (timeSinceLastClick <= DoubleClickTime && distance <= DoubleClickDistance && LastLeftClickControl == ControlUnderMouse)
						{
							isDoubleClick = true;
							LastLeftClickTime = -1f; // Reset to prevent triple-click being detected as double
						}
						else
						{
							LastLeftClickTime = Time;
							LastLeftClickPos = InState.MousePos;
							LastLeftClickControl = ControlUnderMouse;
						}
					}
					else if (MBtn == FishMouseButton.Right)
					{
						float timeSinceLastClick = Time - LastRightClickTime;
						float distance = Vector2.Distance(InState.MousePos, LastRightClickPos);

						if (timeSinceLastClick <= DoubleClickTime && distance <= DoubleClickDistance && LastRightClickControl == ControlUnderMouse)
						{
							isDoubleClick = true;
							LastRightClickTime = -1f;
						}
						else
						{
							LastRightClickTime = Time;
							LastRightClickPos = InState.MousePos;
							LastRightClickControl = ControlUnderMouse;
						}
					}

					if (isDoubleClick)
						ClickedControl.HandleMouseDoubleClick(this, InState, MBtn, InState.MousePos);
					else
						ClickedControl.HandleMouseClick(this, InState, MBtn, InState.MousePos);
				}

				ClickedControl = null;
			}
		}

		void CheckTextInput(FishInputState InState)
		{
			if (InputActiveControl != null)
			{
				if (Input.IsKeyPressed(FishKey.Backspace))
					InputActiveControl.HandleTextInput(this, InState, '\b');

				if (Input.IsKeyPressed(FishKey.Enter) || Input.IsKeyPressed(FishKey.KpEnter))
					InputActiveControl.HandleTextInput(this, InState, '\n');

				int InChr = 0;

				while ((InChr = Input.GetCharPressed()) != 0)
				{
					InputActiveControl.HandleTextInput(this, InState, (char)InChr);
				}
			}
		}

	void Update(Control[] Controls, FishInputState InState, FishInputState InLast, float Time)
		{
			Control ControlUnderMouse = PickControl(InState.MousePos);

			// Mouse drag
			if (LeftClickedControl != null && InState.MouseLeft && InState.MouseDelta != Vector2.Zero)
				LeftClickedControl.HandleDrag(this, InLast.MousePos, InState.MousePos, InState);

			// Mouse move
			if (HoveredControl == ControlUnderMouse && ControlUnderMouse != null && InState.MouseDelta != Vector2.Zero)
				ControlUnderMouse.HandleMouseMove(this, InState, InState.MousePos);

			// Mouse enter/leave handling
			if (HoveredControl != ControlUnderMouse)
			{
				if (HoveredControl != null)
					HoveredControl.HandleMouseLeave(this, InState);

				if (ControlUnderMouse != null)
					ControlUnderMouse.HandleMouseEnter(this, InState);
				HoveredControl = ControlUnderMouse;
			}


			// Left mouse press/release handling
			CheckMousePress(ControlUnderMouse, InState, InState.MouseLeftPressed, FishMouseButton.Left, ref LeftClickedControl);
			CheckMouseRelease(ControlUnderMouse, InState, InState.MouseLeftReleased, FishMouseButton.Left, ref LeftClickedControl, Time);

			// Right mouse press/release handling
			CheckMousePress(ControlUnderMouse, InState, InState.MouseRightPressed, FishMouseButton.Right, ref RightClickedControl);
			CheckMouseRelease(ControlUnderMouse, InState, InState.MouseRightReleased, FishMouseButton.Right, ref RightClickedControl, Time);

			// Mouse wheel handling
			if (InState.MouseWheelDelta != 0 && ControlUnderMouse != null)
			{
				ControlUnderMouse.HandleMouseWheel(this, InState, InState.MouseWheelDelta);
			}

		// Key press handling
		FishKey Key = Input.GetKeyPressed();

		// Process global hotkeys first
		bool hotkeyHandled = Hotkeys.ProcessKeyPress(Key, Input);

		if (!hotkeyHandled)
		{
		// Tab key navigation
		if (Key == FishKey.Tab)
		{
		bool shiftHeld = Input.IsKeyDown(FishKey.LeftShift) || Input.IsKeyDown(FishKey.RightShift);
		FocusNextControl(shiftHeld);
		}
		else if (Key != FishKey.None && InputActiveControl != null)
		{
		InputActiveControl.HandleKeyPress(this, InState, Key);
		InputActiveControl.HandleKeyDown(this, InState, (int)Key);
		}
		}

			foreach (Control Ctl in Controls)
				UpdateSingleControl(Ctl, InState, InLast);

			CheckTextInput(InState);
			InLast = InState;
		}

	void Draw(Control[] Controls, float Dt, float Time)
		{
			Graphics.BeginDrawing(Dt);
			foreach (Control Ctl in Controls)
			{
				if (Ctl.Visible)
				{
					Ctl.DrawControlAndChildren(this, Dt, Time);
				}
			}
			Graphics.EndDrawing();
		}

	public void FocusControl(Control Ctrl)
		{
			Control previousFocus = InputActiveControl;
			
			if (previousFocus != null && previousFocus != Ctrl)
				previousFocus.HandleBlur();

			InputActiveControl = Ctrl;
			
			if (Ctrl != null)
				Ctrl.HandleFocus();
		}

		/// <summary>
		/// Clears the current focus without focusing another control.
		/// </summary>
		public void ClearFocus()
		{
			if (InputActiveControl != null)
			{
				InputActiveControl.HandleBlur();
				InputActiveControl = null;
			}
		}

		/// <summary>
		/// Gets all focusable controls in tab order.
		/// </summary>
		List<Control> GetFocusableControls()
		{
			List<Control> focusable = new List<Control>();
			CollectFocusableControls(Controls.ToArray(), focusable);
			return focusable.OrderBy(c => c.TabIndex).ToList();
		}

		void CollectFocusableControls(Control[] controls, List<Control> result)
		{
			foreach (Control c in controls)
			{
				if (c.Visible && !c.Disabled && c.Focusable)
					result.Add(c);

				CollectFocusableControls(c.GetAllChildren(), result);
			}
		}

		/// <summary>
		/// Focuses the next (or previous if reverse is true) focusable control.
		/// </summary>
		/// <param name="reverse">If true, focus the previous control (Shift+Tab behavior).</param>
		public void FocusNextControl(bool reverse = false)
		{
			List<Control> focusable = GetFocusableControls();
			
			if (focusable.Count == 0)
				return;

			int currentIndex = focusable.IndexOf(InputActiveControl);

			int nextIndex;
			if (currentIndex == -1)
			{
				// No control is focused, focus the first or last
				nextIndex = reverse ? focusable.Count - 1 : 0;
			}
			else
			{
				// Move to next or previous
				if (reverse)
					nextIndex = (currentIndex - 1 + focusable.Count) % focusable.Count;
				else
					nextIndex = (currentIndex + 1) % focusable.Count;
			}

			FocusControl(focusable[nextIndex]);
		}

	FishInputState InLast;

		public void Tick(float Dt, float Time)
		{
			Vector2 MousePos = Input.GetMousePosition();
			bool MouseLeft = Input.IsMouseDown(FishMouseButton.Left);
			bool MouseRight = Input.IsMouseDown(FishMouseButton.Right);
			float MouseWheel = Input.GetMouseWheelMove();

			// Override with virtual mouse if enabled
			if (VirtualMouse.Enabled)
			{
				VirtualMouse.ClampToScreen(Width > 0 ? Width : Graphics.GetWindowWidth(), 
					Height > 0 ? Height : Graphics.GetWindowHeight());
				
				MousePos = VirtualMouse.Position;
				MouseLeft = VirtualMouse.IsLeftDown;
				MouseRight = VirtualMouse.IsRightDown;
			}

			FishInputState InState = new FishInputState();
			InState.MousePos = MousePos;
			InState.MouseLeft = MouseLeft;
			InState.MouseRight = MouseRight;
			InState.TouchPoints = Input.GetTouchPoints();

			if (VirtualMouse.Enabled)
			{
				InState.MouseLeftPressed = VirtualMouse.IsLeftPressed;
				InState.MouseLeftReleased = VirtualMouse.IsLeftReleased;
				InState.MouseRightPressed = VirtualMouse.IsRightPressed;
				InState.MouseRightReleased = VirtualMouse.IsRightReleased;
			}
			else
			{
				InState.MouseLeftPressed = Input.IsMousePressed(FishMouseButton.Left);
				InState.MouseLeftReleased = Input.IsMouseReleased(FishMouseButton.Left);
				InState.MouseRightPressed = Input.IsMousePressed(FishMouseButton.Right);
				InState.MouseRightReleased = Input.IsMouseReleased(FishMouseButton.Right);
			}

			InState.MouseDelta = MousePos - InLast.MousePos;
			InState.MouseWheelDelta = MouseWheel;

			// Modifier keys
			InState.ShiftDown = Input.IsKeyDown(FishKey.LeftShift) || Input.IsKeyDown(FishKey.RightShift);
			InState.CtrlDown = Input.IsKeyDown(FishKey.LeftControl) || Input.IsKeyDown(FishKey.RightControl);
			InState.AltDown = Input.IsKeyDown(FishKey.LeftAlt) || Input.IsKeyDown(FishKey.RightAlt);

			Control[] OrderedControls = GetOrderedControls();

			Update(OrderedControls, InState, InLast, Time);

			// Update tooltip
			UpdateTooltip(Dt, InState.MousePos);

			Draw(OrderedControls, Dt, Time);

			// Draw tooltip on top of everything
			DrawTooltip(Dt, Time);

			// Draw virtual mouse cursor last (on top of everything)
			VirtualMouse.Draw(Graphics);
			VirtualMouse.EndFrame();

			InLast = InState;
			}

	private void UpdateTooltip(float dt, Vector2 mousePos)
		{
			// Find the control under the mouse that has tooltip text
			Control controlWithTooltip = FindControlWithTooltip(HoveredControl);
			
			if (Settings.DebugEnabled)
			{
				if (HoveredControl != null && !string.IsNullOrEmpty(HoveredControl.TooltipText))
				{
					FishUIDebug.Log($"[Tooltip] Hovering control with tooltip: '{HoveredControl.TooltipText}'");
				}
			}

			if (controlWithTooltip != null && !string.IsNullOrEmpty(controlWithTooltip.TooltipText))
			{
				if (_tooltipTargetControl == controlWithTooltip)
				{
					// Still hovering the same control
					_tooltipHoverTime += dt;

					if (!_activeTooltip.IsShowing && _tooltipHoverTime >= TooltipShowDelay)
					{
						if (Settings.DebugEnabled)
						{
							FishUIDebug.Log($"[Tooltip] Showing tooltip: '{controlWithTooltip.TooltipText}' at {mousePos}");
						}
						_activeTooltip.Text = controlWithTooltip.TooltipText;
						_activeTooltip.Show(mousePos);
				}
			}
				else
				{
					// Started hovering a new control
					if (Settings.DebugEnabled)
					{
						FishUIDebug.Log($"[Tooltip] New control hovered, resetting timer");
					}
					_tooltipTargetControl = controlWithTooltip;
					_tooltipHoverTime = 0f;
					_activeTooltip.Hide();
			}
			}
			else
			{
				// Not hovering any control with tooltip
				if (_activeTooltip.IsShowing)
				{
					if (Settings.DebugEnabled)
					{
						FishUIDebug.Log($"[Tooltip] Hiding tooltip - no control with tooltip hovered");
					}
					_activeTooltip.Hide();
				}
				_tooltipTargetControl = null;
				_tooltipHoverTime = 0f;
			}

			// Update tooltip position if showing
			if (_activeTooltip.IsShowing)
			{
			_activeTooltip.UpdatePosition(this, mousePos);
			}
			}

	private Control FindControlWithTooltip(Control control)
		{
			if (control == null)
				return null;

			if (Settings.DebugEnabled)
			{
				FishUIDebug.Log($"[Tooltip] FindControlWithTooltip checking: {control.GetType().Name} ID={control.ID}, TooltipText='{control.TooltipText}'");
			}

			// Check this control first
			if (!string.IsNullOrEmpty(control.TooltipText))
			{
				if (Settings.DebugEnabled)
				{
					FishUIDebug.Log($"[Tooltip] Found control with tooltip: {control.GetType().Name}");
				}
				return control;
			}

			// Check parent chain
			Control parent = control.GetParent();
			while (parent != null)
			{
			if (!string.IsNullOrEmpty(parent.TooltipText))
				return parent;
			parent = parent.GetParent();
			}

			return null;
			}

	private void DrawTooltip(float dt, float time)
		{
			if (_activeTooltip.IsShowing)
			{
				if (Settings.DebugEnabled)
				{
					FishUIDebug.Log($"[Tooltip] Drawing tooltip: '{_activeTooltip.Text}' IsShowing={_activeTooltip.IsShowing}");
				}
				Graphics.BeginDrawing(dt);
				_activeTooltip.DrawControl(this, dt, time);
				Graphics.EndDrawing();
			}
		}

			/// <summary>
			/// Called when the UI container is resized.
		/// Override to handle responsive layout updates.
		/// </summary>
		/// <param name="newWidth">New width of the UI container.</param>
		/// <param name="newHeight">New height of the UI container.</param>
		public void Resized(int newWidth, int newHeight)
		{
			Width = newWidth;
			Height = newHeight;

			// TODO: Notify controls that need to respond to size changes
			// This will be expanded when anchor/docking system is implemented
		}
	}
}
