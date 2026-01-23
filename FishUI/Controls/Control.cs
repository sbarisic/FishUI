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

		protected Control Parent;

		[YamlMember]
		public List<Control> Children = new List<Control>();

	[YamlMember]
		public FishUIPosition Position;

		[YamlMember]
		public Vector2 Size;

		[YamlMember]
	public string ID;

		public virtual int ZDepth { get; set; }

		/// <summary>
		/// If true, this control is always rendered on top of non-AlwaysOnTop controls.
		/// </summary>
		public virtual bool AlwaysOnTop { get; set; } = false;

		public virtual bool Disabled { get; set; } = false;

		public virtual bool Visible { get; set; } = true;

		public virtual FishColor Color { get; set; } = new FishColor(255, 255, 255, 255);

		// If true, this control can be dragged and repositioned with the mouse, handled in the HandleDrag implementation
		public virtual bool Draggable { get; set; } = false;

		/// <summary>
		/// If true, this control can receive keyboard focus via Tab key navigation.
		/// </summary>
		public virtual bool Focusable { get; set; } = false;

		/// <summary>
		/// The tab order index for keyboard navigation. Lower values are focused first.
		/// Controls with the same TabIndex are focused in the order they were added.
		/// </summary>
		public virtual int TabIndex { get; set; } = 0;

		/// <summary>
		/// Returns true if this control currently has input focus.
		/// </summary>
		[YamlIgnore]
		public bool HasFocus => FishUI?.InputActiveControl == this;

		/// <summary>
		/// If true, children are drawn without scissor clipping to parent bounds.
		/// Useful for controls like CheckBox/RadioButton where labels extend beyond the control.
		/// </summary>
		public virtual bool DisableChildScissor { get; set; } = false;

		/// <summary>
		/// Tooltip text to display when hovering over this control.
		/// Set to null or empty to disable tooltip.
		/// </summary>
		[YamlMember]
		public virtual string TooltipText { get; set; }

		public event OnControlDraggedFunc OnDragged;

		/// <summary>
		/// Gets the parent control.
		/// </summary>
		public Control GetParent()
		{
			return Parent;
		}

		/// <summary>
		/// Brings this control to the front of all sibling controls.
		/// For root-level controls, this brings it in front of all other root controls.
		/// </summary>
		public virtual void BringToFront()
		{
			if (FishUI != null)
			{
				ZDepth = FishUI.GetHighestZDepth() + 1;
			}
			else if (Parent != null)
			{
				// For child controls, bring to front among siblings
				int maxDepth = 0;
				foreach (var sibling in Parent.Children)
				{
					if (sibling != this && sibling.ZDepth > maxDepth)
						maxDepth = sibling.ZDepth;
				}
				ZDepth = maxDepth + 1;
			}
		}

		/// <summary>
		/// Sends this control to the back of all sibling controls.
		/// </summary>
		public virtual void SendToBack()
		{
			if (FishUI != null)
			{
				ZDepth = FishUI.GetLowestZDepth() - 1;
			}
			else if (Parent != null)
			{
				// For child controls, send to back among siblings
				int minDepth = int.MaxValue;
				foreach (var sibling in Parent.Children)
				{
					if (sibling != this && sibling.ZDepth < minDepth)
						minDepth = sibling.ZDepth;
				}
				ZDepth = minDepth == int.MaxValue ? 0 : minDepth - 1;
			}
		}

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
				Vector2 ParentPos;
				Vector2 ParentSize;

				if (Parent != null)
				{
					ParentPos = Parent.GetAbsolutePosition();
					ParentSize = Parent.GetAbsoluteSize();
				}
				else
				{
					// No parent - dock to screen bounds using FishUI dimensions
					ParentPos = Vector2.Zero;
					ParentSize = FishUI != null ? new Vector2(FishUI.Width, FishUI.Height) : Size;
				}

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
				// Fallback for unknown position modes - treat as absolute position
				return new Vector2(Position.X, Position.Y);
			}
		}

		public Vector2 GetLocalRelative(Vector2 GlobalPt)
		{
			return GlobalPt - GetAbsolutePosition();
		}

		public Vector2 GetAbsoluteSize()
		{
			if (Position.Mode == PositionMode.Docked)
			{
				Vector2 ParentPos;
				Vector2 ParentSize;

				if (Parent != null)
				{
					ParentPos = Parent.GetAbsolutePosition();
					ParentSize = Parent.GetAbsoluteSize();
				}
				else if (FishUI != null)
				{
					// No parent - dock to screen bounds using FishUI dimensions
					ParentPos = Vector2.Zero;
					ParentSize = new Vector2(FishUI.Width, FishUI.Height);
				}
				else
				{
					// No parent and no FishUI - return default size
					return Size;
				}

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

		/// <summary>
		/// Called after this control and its children have been deserialized from a layout file.
		/// Override this to reinitialize internal state that depends on child controls.
		/// </summary>
		public virtual void OnDeserialized()
		{
			// Call OnDeserialized on all children
			foreach (var child in Children)
			{
				child.OnDeserialized();
			}
		}

		public virtual void DrawChildren(FishUI UI, float Dt, float Time, bool UseScissors = true)
		{
		// Respect DisableChildScissor property - controls like CheckBox/RadioButton need labels to extend beyond bounds
		bool applyScissor = UseScissors && !DisableChildScissor;

		if (applyScissor)
		UI.Graphics.PushScissor(GetAbsolutePosition(), GetAbsoluteSize());

			Control[] Ch = GetAllChildren().Reverse().ToArray();
			foreach (var Child in Ch)
			{
				if (!Child.Visible)
					continue;

				Child.DrawControlAndChildren(UI, Dt, Time);
			}

			if (applyScissor)
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

	// Draw debug outline if enabled
	if (FishUIDebug.DrawControlOutlines)
	UI.Graphics.DrawRectangleOutline(GetAbsolutePosition(), GetAbsoluteSize(), FishUIDebug.OutlineColor);

	// Draw focus indicator if this control has focus
	if (FishUIDebug.DrawFocusIndicators && HasFocus && Focusable)
	UI.Graphics.DrawRectangleOutline(GetAbsolutePosition() - new Vector2(2, 2), GetAbsoluteSize() + new Vector2(4, 4), FishUIDebug.FocusIndicatorColor);

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
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Enter");
		}

		public virtual void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			//if (DebugPrint)
			//	Console.WriteLine($"{GetType().Name}({ID ?? "null"}) - Mouse Move");
		}

		public virtual void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Leave");
		}


		public virtual void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Press", Btn.ToString());
		}

		public virtual void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Release", Btn.ToString());
		}

	public virtual void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Click", Btn.ToString());

			UI.Events.Broadcast(UI, this, "mouse_click", null);
		}

		public virtual void HandleMouseDoubleClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Double Click", Btn.ToString());

			UI.Events.Broadcast(UI, this, "mouse_double_click", null);
		}

	public virtual void HandleFocus()
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Focus");
		}

		public virtual void HandleBlur()
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Blur");
		}

		public virtual void HandleTextInput(FishUI UI, FishInputState InState, char Chr)
		{

		}

		public virtual void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
		}

		public virtual void HandleKeyDown(FishUI UI, FishInputState InState, int KeyCode)
		{
		}

		public virtual void HandleKeyRelease(FishUI UI, FishInputState InState, FishKey Key)
		{
		}

	public virtual void HandleMouseWheel(FishUI UI, FishInputState InState, float WheelDelta)
		{
		}
	}
}
