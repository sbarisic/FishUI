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


		public FishUI(FishUISettings Settings, IFishUIGfx Graphics, IFishUIInput Input, IFishUIEvents Events)
		{
			Controls = new List<Control>();

			this.Settings = Settings;
			this.Graphics = Graphics;
			this.Input = Input;
			this.Events = Events;
		}

		public void Init()
		{
			Graphics.Init();
			Settings.Init(this);
		}

		public void AddControl(Control C)
		{
			C._FishUI = this;
			Controls.Add(C);
		}

		public void RemoveAllControls()
		{
			//Control[] Ctrls = GetOrderedControls();
			Controls.Clear();
		}

		public Control[] GetOrderedControls()
		{
			return Controls.OrderBy(C => C.ZDepth).ToArray();
		}

		public Control[] GetAllControls()
		{
			return Controls.ToArray();
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
					Control CPicked = PickControl(C.GetAllChildren(), GlobalPos);

					if (CPicked != null)
						return CPicked;

					return C;
				}
			}

			return null;
		}

		public Control PickControl(Vector2 GlobalPos)
		{
			return PickControl(GetOrderedControls(), GlobalPos);
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
					ControlUnderMouse.HandleMousePress(this, InState, MBtn, InState.MousePos);
					ClickedControl = ControlUnderMouse;
					FocusControl(ControlUnderMouse);
				}
			}
		}

		// Check for mouse release and clicks
		// Mouse release gets triggered for the first control under the mouse
		// Mouse click gets triggered only after release if the control under the mouse is the same as the one that was pressed
		void CheckMouseRelease(Control ControlUnderMouse, FishInputState InState, bool BtnPressed, FishMouseButton MBtn, ref Control ClickedControl)
		{
			if (BtnPressed)
			{
				if (ControlUnderMouse != null)
					ControlUnderMouse.HandleMouseRelease(this, InState, MBtn, InState.MousePos);

				if (ClickedControl != null && ControlUnderMouse == ClickedControl)
					ClickedControl.HandleMouseClick(this, InState, MBtn, InState.MousePos);

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

		void Update(Control[] Controls, FishInputState InState, FishInputState InLast)
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
			CheckMouseRelease(ControlUnderMouse, InState, InState.MouseLeftReleased, FishMouseButton.Left, ref LeftClickedControl);

			// Right mouse press/release handling
			CheckMousePress(ControlUnderMouse, InState, InState.MouseRightPressed, FishMouseButton.Right, ref RightClickedControl);
			CheckMouseRelease(ControlUnderMouse, InState, InState.MouseRightReleased, FishMouseButton.Right, ref RightClickedControl);

			// Mouse wheel handling
			if (InState.MouseWheelDelta != 0 && ControlUnderMouse != null)
			{
				ControlUnderMouse.HandleMouseWheel(this, InState, InState.MouseWheelDelta);
			}

			// Key press handling
			FishKey Key = Input.GetKeyPressed();
			if (Key != FishKey.None && InputActiveControl != null)
			{
				InputActiveControl.HandleKeyPress(this, InState, Key);
			}

			foreach (Control Ctl in Controls)
				UpdateSingleControl(Ctl, InState, InLast);

			CheckTextInput(InState);
			InLast = InState;
		}

		void Draw(Control[] Controls, float Dt, float Time)
		{
			Graphics.BeginDrawing(Dt);
			foreach (Control Ctl in Controls.Reverse())
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
			InputActiveControl = Ctrl;
			Ctrl.HandleFocus();
		}

		FishInputState InLast;

		public void Tick(float Dt, float Time)
		{
			Vector2 MousePos = Input.GetMousePosition();
			bool MouseLeft = Input.IsMouseDown(FishMouseButton.Left);
			bool MouseRight = Input.IsMouseDown(FishMouseButton.Right);
			float MouseWheel = Input.GetMouseWheelMove();

			FishInputState InState = new FishInputState();
			InState.MousePos = MousePos;
			InState.MouseLeft = MouseLeft;
			InState.MouseRight = MouseRight;
			InState.TouchPoints = Input.GetTouchPoints();
			InState.MouseLeftPressed = Input.IsMousePressed(FishMouseButton.Left);
			InState.MouseLeftReleased = Input.IsMouseReleased(FishMouseButton.Left);
			InState.MouseRightPressed = Input.IsMousePressed(FishMouseButton.Right);
			InState.MouseRightReleased = Input.IsMouseReleased(FishMouseButton.Right);
			InState.MouseDelta = MousePos - InLast.MousePos;
			InState.MouseWheelDelta = MouseWheel;

			Control[] OrderedControls = GetOrderedControls();

			Update(OrderedControls, InState, InLast);
			Draw(OrderedControls, Dt, Time);
			InLast = InState;
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
