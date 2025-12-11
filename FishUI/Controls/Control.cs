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



		public FishUIPosition Position;
		public Padding Padding;
		public Vector2 Size;

		public virtual int ZDepth { get; set; }

		public virtual bool Disabled { get; set; } = false;

		public virtual bool Visible { get; set; } = true;

		public virtual FishColor Color { get; set; } = new FishColor(255, 255, 255, 255);

		public Vector2 GetAbsolutePosition()
		{
			if (Position.Mode == PositionMode.Absolute)
			{
				return new Vector2(Position.X, Position.Y);
			}
			else
			{
				Vector2 ParentPos = Vector2.Zero;

				if (Parent != null)
					ParentPos = Parent.GetAbsolutePosition();

				return ParentPos + new Vector2(Position.X, Position.Y);
			}
		}

		public Vector2 GetLocalRelative(Vector2 GlobalPt)
		{
			return GlobalPt - GetAbsolutePosition();
		}

		public Vector2 GetAbsoluteSize()
		{
			return Size;
		}

		public bool IsPointInside(Vector2 GlobalPt)
		{
			Vector2 AbsPos = GetAbsolutePosition();
			Vector2 AbsSize = GetAbsoluteSize();
			return Utils.IsInside(AbsPos, AbsSize, GlobalPt);
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

		// Called once at first draw - for loading resources...
		public virtual void Init(FishUI UI)
		{
		}

		public virtual void DrawChildren(FishUI UI, float Dt, float Time)
		{
			UI.Graphics.PushScissor(GetAbsolutePosition(), GetAbsoluteSize());
			Control[] Ch = GetAllChildren().Reverse().ToArray();
			foreach (var Child in Ch)
			{
				if (Child.Visible)
				{
					Child.DrawControlAndChildren(UI, Dt, Time);
				}
			}
			UI.Graphics.PopScissor();
		}

		public virtual void Draw(FishUI UI, float Dt, float Time)
		{
			UI.Graphics.DrawRectangle(GetAbsolutePosition(), GetAbsoluteSize(), Color);

			//UI.Graphics.DrawImage(UI.Skin, Position, Size, 0, 1, new FishColor(255, 255, 255, 255));

			if (IsMouseInside)
			{
				UI.Graphics.DrawRectangleOutline(GetAbsolutePosition(), GetAbsoluteSize(), new FishColor(0, 255, 255));
			}
			else
			{
				UI.Graphics.DrawRectangleOutline(GetAbsolutePosition(), GetAbsoluteSize(), new FishColor(100, 100, 100));
			}

			DrawChildren(UI, Dt, Time);
		}


		bool DrawHasInit = false;

		public void DrawControlAndChildren(FishUI UI, float Dt, float Time)
		{
			if (!DrawHasInit)
			{
				DrawHasInit = true;
				Init(UI);
			}

			Draw(UI, Dt, Time);
			DrawChildren(UI, Dt, Time);
		}

		public virtual void HandleDrag(FishUI UI, Vector2 StartPos, Vector2 EndPos, FishInputState InState)
		{
			//Position += InState.MouseDelta;
			//Console.WriteLine($"{GetType().Name} - Drag");
		}

		public virtual void HandleMouseEnter(FishUI UI, FishInputState InState)
		{
			Console.WriteLine($"{GetType().Name} - Mouse Enter");
		}

		public virtual void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			Console.WriteLine($"{GetType().Name} - Mouse Leave");
		}


		public virtual void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			Console.WriteLine($"{GetType().Name} - Mouse Press {Btn}");
		}

		public virtual void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			Console.WriteLine($"{GetType().Name} - Mouse Release {Btn}");
		}

		public virtual void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			Console.WriteLine($"{GetType().Name} - Mouse Click {Btn}");
		}

		public virtual void HandleFocus()
		{
		}

		public virtual void HandleTextInput(FishUI UI, FishInputState InState, char Chr)
		{

		}

		/*public virtual void HandleInput(FishUI UI, FishInputState InState)
		{
		}*/
	}
}
