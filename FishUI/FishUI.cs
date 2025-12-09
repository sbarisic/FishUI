using FishUI.Controls;
using System.Numerics;

namespace FishUI
{
	public class FishUI
	{
		public IFishUIGfx Graphics;
		public IFishUIInput Input;

		public List<Control> Controls;
		public int Width;
		public int Height;

		public Control PressedControl;
		public Control HeldControl;

		public FishUI(IFishUIGfx Graphics, IFishUIInput Input, int Width, int Height)
		{
			Controls = new List<Control>();

			this.Graphics = Graphics;
			this.Input = Input;
		}

		internal ImageRef Skin;

		public void Init()
		{
			Graphics.Init();

			Skin = Graphics.LoadImage("data/gwen.png");
		}

		Control[] GetOrderedControls()
		{
			return Controls.OrderBy(C => C.ZDepth).ToArray();
		}

		Control GetControlAt(Vector2 Pos)
		{
			Control[] Cs = Controls.OrderBy(C => C.ZDepth).ToArray();

			foreach (Control C in Cs)
			{
				if (Utils.IsInside(C.Position, C.Size, Pos))
				{
					Control Picked = C.GetChildAt(Pos);
					return Picked;
				}
			}

			return null;
		}

		Vector2 LastMousePos;
		bool LastMouseLeft;
		bool LastMouseRight;

		Vector2? MouseLeftClickPos;
		Vector2? MouseRightClickPos;

		public void Tick(float Dt, float Time)
		{
			Vector2 MousePos = Input.GetMousePosition();
			bool MouseLeft = Input.IsMouseDown(FishMouseButton.Left);
			bool MouseRight = Input.IsMouseDown(FishMouseButton.Right);

			FishInputState InState = new FishInputState();
			InState.MousePos = MousePos;
			InState.MouseLeft = MouseLeft;
			InState.MouseRight = MouseRight;
			InState.MouseLeftPressed = MouseLeft && !LastMouseLeft;
			InState.MouseLeftReleased = !MouseLeft && LastMouseLeft;
			InState.MouseRightPressed = MouseRight && !LastMouseRight;
			InState.MouseRightReleased = !MouseRight && LastMouseRight;
			InState.MouseDelta = MousePos - LastMousePos;

			if (InState.MouseLeftPressed)
			{
				PressedControl = GetControlAt(MousePos);
				MouseLeftClickPos = MousePos;
				HeldControl = PressedControl;
			}

			if (InState.MouseLeftReleased)
			{
				MouseLeftClickPos = null;
				HeldControl = null;
			}

			if (HeldControl != null && InState.MouseDelta != Vector2.Zero)
			{
				HeldControl.HandleDrag(this, MouseLeftClickPos ?? Vector2.Zero, MousePos, InState);
			}

			foreach (Control Ctlr in GetOrderedControls())
			{
				Ctlr.InternalHandleInput(this, InState, out bool Handled, out Control HandledControl);
				Control Ctl = Ctlr;
				//Ctl = HandledControl;

				/*bool NewIsInside = Utils.IsInside(Ctl.GlobalPosition, Ctl.Size, MousePos);
				if (NewIsInside)
					Ctl = Ctl.GetChildAt(MousePos);

				if (NewIsInside && !Ctl.IsMouseInside)
				{
					Ctl.HandleMouseEnter(this, InState);
				}
				else if (!NewIsInside && Ctl.IsMouseInside)
				{
					Ctl.HandleMouseLeave(this, InState);
				}


				Ctl.IsMouseInside = NewIsInside;

				// Drawing flag IsMousePressed
				if (Ctl.IsMouseInside && InState.MouseLeft)
					Ctl.IsMousePressed = true;
				else
					Ctl.IsMousePressed = false;*/


				if (Ctl.IsMouseInside && !Handled)
				{
					Control Ctl2 = Ctl.GetChildAt(InState.MousePos);
					Ctl2.InternalHandleInput(this, InState, out bool Handled2, out Control HandledControl2);

					if (Handled2)
						Handled = true;
				}

				if (Handled)
					break;
			}

			Graphics.BeginDrawing(Dt);
			foreach (Control Ctl in Controls)
			{
				Ctl.InternalInit(this);
				Ctl.Draw(this, Dt, Time);
			}
			Graphics.EndDrawing();

			LastMouseLeft = MouseLeft;
			LastMouseRight = MouseRight;
			LastMousePos = MousePos;
		}
	}
}
