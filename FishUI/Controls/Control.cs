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

		public virtual Vector2 GlobalPosition
		{
			get
			{
				if (Parent != null)
					return Parent.GlobalPosition + Position;

				return Position;
			}
		}

		public bool IsMouseInside;

		public void AddChild(Control Child)
		{
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

		public virtual void Draw(FishUI UI, float Dt, float Time)
		{
			UI.Graphics.DrawRectangle(Position, Size, new FishColor(255, 0, 0));

			Control[] Ch = GetAllChildren();
			foreach (var Child in Ch)
			{
				Child.Draw(UI, Dt, Time);
			}
		}

		public void Update(FishUI UI, FishInputState InState)
		{

		}
	}
}
