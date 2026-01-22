using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void OnControlDraggedFunc(Control Sender, Vector2 MouseDelta);

	public abstract class Control
	{
		[YamlIgnore]
		internal FishUI _FishUI;

		[YamlIgnore]
		protected FishUI FishUI
		{
			get
			{
				if (Parent != null)
					return Parent.FishUI;
				else
					return _FishUI;
			}
		}

		const bool DebugPrint = true;

		protected Control Parent;

		[YamlMember]
		public List<Control> Children = new List<Control>();

		[YamlMember]
		public FishUIPosition Position;

		//[YamlMember]
		//public Padding Padding;

		[YamlMember]
		public Vector2 Size;

		[YamlMember]
		public string ID;

		public virtual int ZDepth { get; set; }

		public virtual bool Disabled { get; set; } = false;

		public virtual bool Visible { get; set; } = true;

		public virtual FishColor Color { get; set; } = new FishColor(255, 255, 255, 255);

		// If true, this control can be dragged and repositioned with the mouse, handled in the HandleDrag implementation
		public virtual bool Draggable { get; set; } = false;

		public event OnControlDraggedFunc OnDragged;

		public Vector2 GetAbsolutePosition()
		{
			if (Position.Mode == PositionMode.Absolute)
			{
				return new Vector2(Position.X, Position.Y);
			}
			else if (Position.Mode == PositionMode.Relative)
			{
				Vector2 ParentPos = Vector2.Zero;

				if (Parent != null)
					ParentPos = Parent.GetAbsolutePosition();

				return ParentPos + new Vector2(Position.X, Position.Y);
			}
			else if (Position.Mode == PositionMode.Docked)
			{
				if (Parent != null)
				{
					Vector2 ParentPos = Parent.GetAbsolutePosition();
					Vector2 DockedPos = ParentPos + new Vector2(Position.X, Position.Y);

					if (Position.Dock.HasFlag(DockMode.Left))
					{
						DockedPos.X = ParentPos.X + Position.Left;
					}

					if (Position.Dock.HasFlag(DockMode.Top))
					{
						DockedPos.Y = ParentPos.Y + Position.Top;
					}

					return DockedPos;
				}
				else
				{
					// No parent, treat as absolute
					return new Vector2(Position.X, Position.Y);
				}
			}
			else
				throw new NotImplementedException();
		}

		public Vector2 GetLocalRelative(Vector2 GlobalPt)
		{
			return GlobalPt - GetAbsolutePosition();
		}

		public Vector2 GetAbsoluteSize()
		{
			if (Position.Mode == PositionMode.Docked && Parent != null)
			{
				Vector2 ParentPos = Parent.GetAbsolutePosition();
				Vector2 ParentSize = Parent.GetAbsoluteSize();

				Vector2 MyPos = GetAbsolutePosition();
				Vector2 MyNewSize = Size;

				float SubX = MyPos.X - ParentPos.X;
				float SubY = MyPos.Y - ParentPos.Y;

				if (Position.Dock.HasFlag(DockMode.Right))
				{
					float FullChildWidth = ParentSize.X - SubX;
					MyNewSize.X = FullChildWidth - Position.Right;
				}

				if (Position.Dock.HasFlag(DockMode.Bottom))
				{
					float FullChildHeight = ParentSize.Y - SubY;
					MyNewSize.Y = FullChildHeight - Position.Bottom;
				}

				return MyNewSize;
			}

		return Size;
		}

		public virtual bool IsPointInside(Vector2 GlobalPt)
		{
			Vector2 AbsPos = GetAbsolutePosition();
			Vector2 AbsSize = GetAbsoluteSize();
			return Utils.IsInside(AbsPos, AbsSize, GlobalPt);
		}

		[YamlIgnore]
		public bool IsMouseInside;

		[YamlIgnore]
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
			Child.Parent = this;

			// If Children contains Child, skip. It means this function was used to re-parent an existing child on deserialization
			if (Children.Contains(Child))
				return;

			Children.Add(Child);
		}

		public T FindChildByType<T>() where T : Control
		{
			foreach (Control Ch in GetAllChildren())
			{
				if (Ch is T Ret)
					return Ret;
			}

			return null;
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

		public void RemoveAllChildren()
		{
			Control[] Ch = GetAllChildren(false);

			for (int i = 0; i < Ch.Length; i++)
				RemoveChild(Ch[i]);
		}

		// Called once at first draw - for loading resources...
		public virtual void Init(FishUI UI)
		{
		}

		public virtual void DrawChildren(FishUI UI, float Dt, float Time, bool UseScissors = true)
		{
			if (UseScissors)
				UI.Graphics.PushScissor(GetAbsolutePosition(), GetAbsoluteSize());

			Control[] Ch = GetAllChildren().Reverse().ToArray();
			foreach (var Child in Ch)
			{
				if (!Child.Visible)
					continue;

				Child.DrawControlAndChildren(UI, Dt, Time);
			}

			if (UseScissors)
				UI.Graphics.PopScissor();
		}

		public virtual void DrawControl(FishUI UI, float Dt, float Time)
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

			//DrawChildren(UI, Dt, Time);
		}

		[YamlIgnore]
		bool DrawHasInit = false;

		public void DrawControlAndChildren(FishUI UI, float Dt, float Time)
		{
			if (!DrawHasInit)
			{
				DrawHasInit = true;
				Init(UI);
			}

			DrawControl(UI, Dt, Time);
			DrawChildren(UI, Dt, Time);
		}

		public virtual void HandleDrag(FishUI UI, Vector2 StartPos, Vector2 EndPos, FishInputState InState)
		{
			if (Draggable)
			{
				OnDragged?.Invoke(this, InState.MouseDelta);
				Position += InState.MouseDelta;
			}

			//if (DebugPrint)
			//	Console.WriteLine($"{GetType().Name}({ID ?? "null"}) - Drag Control");
		}

		public virtual void HandleMouseEnter(FishUI UI, FishInputState InState)
		{
			if (DebugPrint)
				Console.WriteLine($"{GetType().Name}({ID ?? "null"}) - Mouse Enter");
		}

		public virtual void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			//if (DebugPrint)
			//	Console.WriteLine($"{GetType().Name}({ID ?? "null"}) - Mouse Move");
		}

		public virtual void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			if (DebugPrint)
				Console.WriteLine($"{GetType().Name}({ID ?? "null"}) - Mouse Leave");
		}


		public virtual void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			if (DebugPrint)
				Console.WriteLine($"{GetType().Name}({ID ?? "null"}) - Mouse Press {Btn}");
		}

		public virtual void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			if (DebugPrint)
				Console.WriteLine($"{GetType().Name}({ID ?? "null"}) - Mouse Release {Btn}");
		}

		public virtual void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			if (DebugPrint)
				Console.WriteLine($"{GetType().Name}({ID ?? "null"}) - Mouse Click {Btn}");

			UI.Events.Broadcast(UI, this, "mouse_click", null);
		}

		public virtual void HandleFocus()
		{
		}

		public virtual void HandleTextInput(FishUI UI, FishInputState InState, char Chr)
		{

		}

		public virtual void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
		}

		public virtual void HandleKeyRelease(FishUI UI, FishInputState InState, FishKey Key)
		{
		}

		public virtual void HandleMouseWheel(FishUI UI, FishInputState InState, float WheelDelta)
		{
		}


		/*public virtual void HandleInput(FishUI UI, FishInputState InState)
		{
		}*/
	}
}
