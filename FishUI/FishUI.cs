using FishUI.Controls;
using System.ComponentModel.Design;
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
		public Control PressedRightControl;
		public Control HeldRightControl;

		SelectionBox SelBox;

		public FishUI(IFishUIGfx Graphics, IFishUIInput Input, int Width, int Height)
		{
			Controls = new List<Control>();

			this.Graphics = Graphics;
			this.Input = Input;

			SelBox = new SelectionBox();
			SelBox.ZDepth = -100;
			//Controls.Add(SelBox);
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
				if (!C.Visible)
					continue;

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

			if (InState.MouseRightPressed)
			{
				PressedRightControl = GetControlAt(MousePos);
				MouseRightClickPos = MousePos;
				HeldRightControl = PressedRightControl;
			}

			if (InState.MouseLeftReleased)
			{
				MouseLeftClickPos = null;
				HeldControl = null;
			}

			if (InState.MouseRightReleased)
			{
				MouseRightClickPos = null;
				HeldRightControl = null;
			}

			if (HeldRightControl != null && MouseRightClickPos != null)
			{
				Vector2 Sz = MousePos - MouseRightClickPos.Value;
				SelBox.Visible = true;
				HeldRightControl.AddChild(SelBox);

				if (Sz.X < 0 && Sz.Y < 0)
				{
					SelBox.Position = HeldRightControl.ToLocal(MousePos.Round());
					SelBox.Size = new Vector2(-Sz.X, -Sz.Y).Round();
				}
				else if (Sz.X < 0)
				{
					SelBox.Position = HeldRightControl.ToLocal(new Vector2(MousePos.X, MouseRightClickPos.Value.Y).Round());
					SelBox.Size = new Vector2(-Sz.X, Sz.Y).Round();
				}
				else if (Sz.Y < 0)
				{
					SelBox.Position = HeldRightControl.ToLocal(new Vector2(MouseRightClickPos.Value.X, MousePos.Y).Round());
					SelBox.Size = new Vector2(Sz.X, -Sz.Y).Round();
				}
				else
				{
					SelBox.Position = HeldRightControl.ToLocal(MouseRightClickPos.Value.Round());
					SelBox.Size = Sz.Round();
				}
			}
			else
			{
				SelBox.Visible = false;
				SelBox.Unparent();
			}

			if (HeldControl != null && InState.MouseDelta != Vector2.Zero)
			{
				if (HeldControl.Visible)
					HeldControl.HandleDrag(this, MouseLeftClickPos ?? Vector2.Zero, MousePos, InState);
			}

			foreach (Control Ctlr in GetOrderedControls())
			{
				if (!Ctlr.Visible)
					continue;

				Ctlr.InternalHandleInput(this, InState, out bool Handled, out Control HandledControl);
				Control Ctl = Ctlr;

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
			foreach (Control Ctl in GetOrderedControls().Reverse())
			{
				if (Ctl.Visible)
				{
					Ctl.InternalInit(this);
					Ctl.Draw(this, Dt, Time);
				}
			}
			Graphics.EndDrawing();

			LastMouseLeft = MouseLeft;
			LastMouseRight = MouseRight;
			LastMousePos = MousePos;
		}
	}
}
