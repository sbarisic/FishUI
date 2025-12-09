using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FishUI.Controls
{
	public abstract class Control
	{
		Control Parent;
		List<Control> Children = new List<Control>();

		public virtual Vector2 Position { get; set; }
		public virtual Vector2 Size { get; set; }
		public virtual int ZDepth { get; set; }

		public virtual bool Disabled { get; set; } = false;

		public virtual bool Visible { get; set; } = true;

		public virtual bool RightClickDragSelect { get; set; } = false;

		public virtual FishColor Color { get; set; } = new FishColor(255, 255, 255, 255);

		public virtual Vector2 GlobalPosition
		{
			get
			{
				if (Parent != null)
					return Parent.GlobalPosition + Position;

				return Position;
			}
		}

		public virtual void Init(FishUI UI)
		{
		}

		bool HasInit = false;
		public void InternalInit(FishUI UI)
		{
			if (HasInit)
				return;

			HasInit = true;
			Init(UI);
		}

		public Vector2 ToLocal(Vector2 GlobalPt)
		{
			return GlobalPt - GlobalPosition;
		}

		public bool IsMouseInside;
		public bool IsMousePressed;

		public void Unparent()
		{
			if (Parent != null)
			{
				Parent.RemoveChild(this);
			}
		}

		public void AddChild(Control Child)
		{
			if (Children.Contains(Child))
				return;

			Child.Parent = this;
			Children.Add(Child);
		}

		public Control[] GetAllChildren(bool Order = true)
		{
			if (Order)
				return Children.OrderBy(C => C.ZDepth).ToArray();
			else
				return Children.ToArray();
		}

		public void RemoveChild(Control Child)
		{
			Child.Parent = null;
			Children.Remove(Child);
		}

		public Control GetChildAt(Vector2 Pos)
		{
			Control[] Cs = GetAllChildren();

			foreach (Control C in Cs)
			{
				if (Utils.IsInside(C.GlobalPosition, C.Size, Pos))
					return C.GetChildAt(Pos);
			}

			return this;
		}

		public virtual void DrawChildren(FishUI UI, float Dt, float Time)
		{
			UI.Graphics.PushScissor(GlobalPosition, Size);
			Control[] Ch = GetAllChildren().Reverse().ToArray();
			foreach (var Child in Ch)
			{
				if (Child.Visible)
				{
					Child.InternalInit(UI);
					Child.Draw(UI, Dt, Time);
				}
			}
			UI.Graphics.PopScissor();
		}

		public virtual void Draw(FishUI UI, float Dt, float Time)
		{
			UI.Graphics.DrawRectangle(Position, Size, Color);

			//UI.Graphics.DrawImage(UI.Skin, Position, Size, 0, 1, new FishColor(255, 255, 255, 255));

			if (IsMouseInside)
			{
				UI.Graphics.DrawRectangleOutline(Position, Size, new FishColor(0, 255, 255));
			}
			else
			{
				UI.Graphics.DrawRectangleOutline(Position, Size, new FishColor(100, 100, 100));
			}

			DrawChildren(UI, Dt, Time);
		}

		bool LeftClickedOn = false;
		bool RightClickedOn = false;

		public void InternalHandleInput(FishUI UI, FishInputState InState, out bool Handled, out Control HandledControl)
		{
			Handled = false;
			HandledControl = null;

			if (IsMouseInside)
			{
				Control[] Ch = GetAllChildren();
				foreach (var Child in Ch)
				{
					Child.InternalHandleInput(UI, InState, out bool ChildHandled, out Control ChildHandledControl);
					if (ChildHandled)
					{
						Handled = true;
						HandledControl = ChildHandledControl;
						break;
					}
				}

				if (!Handled)
				{
					if (InState.MouseLeftPressed)
						LeftClickedOn = true;
					if (InState.MouseRightPressed)
						RightClickedOn = true;

					if (InState.MouseLeftReleased && LeftClickedOn)
					{
						LeftClickedOn = false;
						HandleMouseLeftClick(UI, InState, InState.MousePos);
					}

					if (InState.MouseRightReleased && RightClickedOn)
					{
						RightClickedOn = false;
						HandleMouseRightClick(UI, InState, InState.MousePos);
					}

					// Handle input for this control here
					HandleInput(UI, InState);
					Handled = true;
					HandledControl = this;
				}
			}

			Control Ctl = this;
			bool NewIsInside = Utils.IsInside(GlobalPosition, Size, InState.MousePos);
			if (NewIsInside)
				Ctl = Ctl.GetChildAt(InState.MousePos);

			if (NewIsInside && !Ctl.IsMouseInside)
			{
				Ctl.HandleMouseEnter(UI, InState);
			}
			else if (!NewIsInside && Ctl.IsMouseInside)
			{
				Ctl.HandleMouseLeave(UI, InState);
			}


			Ctl.IsMouseInside = NewIsInside;

			// Drawing flag IsMousePressed
			if (Ctl.IsMouseInside && InState.MouseLeft)
				Ctl.IsMousePressed = true;
			else
				Ctl.IsMousePressed = false;
		}

		public virtual void HandleDrag(FishUI UI, Vector2 StartPos, Vector2 EndPos, FishInputState InState)
		{
			//Position += InState.MouseDelta;
			Console.WriteLine($"{GetType().Name} - Drag");
		}

		public virtual void HandleMouseEnter(FishUI UI, FishInputState InState)
		{
			Console.WriteLine($"{GetType().Name} - Mouse Enter");
		}

		public virtual void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			Console.WriteLine($"{GetType().Name} - Mouse Leave");
		}

		public virtual void HandleMouseLeftClick(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			Console.WriteLine($"{GetType().Name} - Mouse Left Click");
		}

		public virtual void HandleMouseRightClick(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			Console.WriteLine($"{GetType().Name} - Mouse Right Click");
		}

		public virtual void HandleInput(FishUI UI, FishInputState InState)
		{
		}
	}
}
