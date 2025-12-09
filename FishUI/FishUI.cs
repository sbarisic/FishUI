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

		public FishUI(IFishUIGfx Graphics, IFishUIInput Input, int Width, int Height)
		{
			Controls = new List<Control>();

			this.Graphics = Graphics;
			this.Input = Input;
		}

		public void Init()
		{
			Graphics.Init();
		}

		Vector2 LastMousePos;
		bool LastMouseLeft;
		bool LastMouseRight;

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
			InState.MouseDelta = LastMousePos - MousePos;

			foreach (Control Ctl in Controls)
			{
				Ctl.IsMouseInside = Utils.IsInside(Ctl.Position, Ctl.Size, MousePos);
				Ctl.Update(this, InState);
			}

			Graphics.BeginDrawing(Dt);
			foreach (Control Ctl in Controls)
			{
				Ctl.Draw(this, Dt, Time);
			}
			Graphics.EndDrawing();

			LastMouseLeft = MouseLeft;
			LastMouseRight = MouseRight;
			LastMousePos = MousePos;
		}
	}
}
