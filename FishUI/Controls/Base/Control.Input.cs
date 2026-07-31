using System.Numerics;

namespace FishUI.Controls
{
	public abstract partial class Control
	{
		/// <summary>
		/// Called when the control is being dragged with the mouse.
		/// </summary>
		public virtual void HandleDrag(FishUI UI, Vector2 StartPos, Vector2 EndPos, FishInputState InState)
		{
			if (Draggable)
			{
				OnDragged?.Invoke(this, InState.MouseDelta);
				Position += InState.MouseDelta;
			}
		}

		/// <summary>
		/// Called when the mouse enters this control's bounds.
		/// </summary>
		public virtual void HandleMouseEnter(FishUI UI, FishInputState InState)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Enter");

			// Fire local event
			var eventArgs = new FishUIMouseEventArgs(UI, this, InState.MousePos);
			MouseEnter?.Invoke(this, eventArgs);

			// Fire interface event
			UI.Events?.OnControlMouseEnter(eventArgs);
		}

		/// <summary>
		/// Called when the mouse moves within this control's bounds.
		/// </summary>
		public virtual void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
		}

		/// <summary>
		/// Called when the mouse leaves this control's bounds.
		/// </summary>
		public virtual void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Leave");

			// Fire local event
			var eventArgs = new FishUIMouseEventArgs(UI, this, InState.MousePos);
			MouseLeave?.Invoke(this, eventArgs);

			// Fire interface event
			UI.Events?.OnControlMouseLeave(eventArgs);
		}

		/// <summary>
		/// Called when a mouse button is pressed on this control.
		/// </summary>
		public virtual void HandleMousePress(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Press", Btn.ToString());
		}

		/// <summary>
		/// Called when a mouse button is released on this control.
		/// </summary>
		public virtual void HandleMouseRelease(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Release", Btn.ToString());
		}

		/// <summary>
		/// Called when the mouse is clicked on this control.
		/// </summary>
		public virtual void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Click", Btn.ToString());

			// Legacy broadcast for backward compatibility
			UI.Events?.Broadcast(UI, this, "mouse_click", null);

			// Fire local event
			var eventArgs = new FishUIClickEventArgs(UI, this, Btn, Pos);
			Clicked?.Invoke(this, eventArgs);

			// Fire interface event
			UI.Events?.OnControlClicked(eventArgs);
		}

		/// <summary>
		/// Called when the mouse is double-clicked on this control.
		/// </summary>
		public virtual void HandleMouseDoubleClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Mouse Double Click", Btn.ToString());

			// Legacy broadcast for backward compatibility
			UI.Events?.Broadcast(UI, this, "mouse_double_click", null);

			// Fire local event
			var eventArgs = new FishUIClickEventArgs(UI, this, Btn, Pos);
			DoubleClicked?.Invoke(this, eventArgs);

			// Fire interface event
			UI.Events?.OnControlDoubleClicked(eventArgs);
		}

		/// <summary>
		/// Called when this control receives input focus.
		/// </summary>
		public virtual void HandleFocus()
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Focus");
		}

		/// <summary>
		/// Called when this control loses input focus.
		/// </summary>
		public virtual void HandleBlur()
		{
			FishUIDebug.LogControlEvent(GetType().Name, ID, "Blur");
		}

		/// <summary>
		/// Called when text is typed while this control has focus.
		/// </summary>
		public virtual void HandleTextInput(FishUI UI, FishInputState InState, char Chr)
		{
		}

		/// <summary>
		/// Called when a key is pressed while this control has focus.
		/// </summary>
		public virtual void HandleKeyPress(FishUI UI, FishInputState InState, FishKey Key)
		{
		}

		/// <summary>
		/// Called when a key is held down while this control has focus.
		/// </summary>
		public virtual void HandleKeyDown(FishUI UI, FishInputState InState, int KeyCode)
		{
		}

		/// <summary>
		/// Called when a key is released while this control has focus.
		/// </summary>
		public virtual void HandleKeyRelease(FishUI UI, FishInputState InState, FishKey Key)
		{
		}

		/// <summary>
		/// Called when the mouse wheel is scrolled over this control.
		/// By default, propagates to parent control.
		/// </summary>
		public virtual void HandleMouseWheel(FishUI UI, FishInputState InState, float WheelDelta)
		{
			// By default, propagate mouse wheel events to parent (bubble up)
			if (Parent != null)
				Parent.HandleMouseWheel(UI, InState, WheelDelta);
		}
	}
}
