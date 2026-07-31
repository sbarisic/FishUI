using System.Collections.Generic;
using System.Linq;

namespace FishUI.Controls
{
	public abstract partial class Control
	{
		/// <summary>
		/// Removes this control from its parent.
		/// </summary>
		public void Unparent()
		{
			if (Parent != null)
			{
				Parent.RemoveChild(this);
			}
		}

		/// <summary>
		/// Adds a control as a child of this control.
		/// </summary>
		/// <param name="Child">The control to add as a child.</param>
		public void AddChild(Control Child)
		{
			Child.Parent = this;

			// Calculate anchor offsets based on current parent size
			UpdateChildAnchorOffsets(Child);

			// If Children contains Child, skip. It means this function was used to re-parent an existing child on deserialization
			if (Children.Contains(Child))
				return;

			// Assign ZDepth based on insertion order (higher = added later = on top)
			// Use the count of existing children as the ZDepth for proper ordering
			Child.ZDepth = Children.Count;

			Children.Add(Child);
		}

		/// <summary>
		/// Clears the parent reference of this control (used for reparenting).
		/// </summary>
		public void ClearParent()
		{
			Parent = null;
		}

		/// <summary>
		/// Sets the parent reference of this control without adding to parent's Children list.
		/// Used internally for deserialization and special cases.
		/// </summary>
		internal void SetParentInternal(Control parent)
		{
			Parent = parent;
		}

		/// <summary>
		/// Determines if a child control should receive input at the specified point.
		/// Override in container controls like ScrollablePane to restrict input to visible area.
		/// </summary>
		/// <param name="child">The child control to check.</param>
		/// <param name="globalPoint">The point in screen coordinates.</param>
		/// <returns>True if the child should receive input at this point.</returns>
		public virtual bool ShouldChildReceiveInput(Control child, System.Numerics.Vector2 globalPoint)
		{
			return true; // By default, all children can receive input
		}

		/// <summary>
		/// Finds the first child control of the specified type.
		/// </summary>
		/// <typeparam name="T">The type of control to find.</typeparam>
		/// <returns>The first matching child control, or null if not found.</returns>
		public T FindChildByType<T>() where T : Control
		{
			foreach (Control Ch in GetAllChildren())
			{
				if (Ch is T Ret)
					return Ret;
			}

			return null;
		}

		/// <summary>
		/// Gets all child controls.
		/// </summary>
		/// <param name="Order">If true, returns children ordered by ZDepth with AlwaysOnTop controls last.</param>
		/// <returns>Array of child controls.</returns>
		public Control[] GetAllChildren(bool Order = true)
		{
			if (Order)
			{
				// Sort: normal controls by ZDepth, then AlwaysOnTop controls by ZDepth
				// This matches GetOrderedControls behavior so AlwaysOnTop children are picked first when reversed
				var normal = Children.Where(c => !c.AlwaysOnTop).OrderBy(c => c.ZDepth);
				var alwaysOnTop = Children.Where(c => c.AlwaysOnTop).OrderBy(c => c.ZDepth);
				return normal.Concat(alwaysOnTop).ToArray();
			}
			else
				return Children.ToArray();
		}

		/// <summary>
		/// Removes a child control from this control.
		/// </summary>
		/// <param name="Child">The child control to remove.</param>
		public void RemoveChild(Control Child)
		{
			Child.Parent = null;
			Children.Remove(Child);
		}

		/// <summary>
		/// Removes all child controls from this control.
		/// </summary>
		public void RemoveAllChildren()
		{
			Control[] Ch = GetAllChildren(false);

			for (int i = 0; i < Ch.Length; i++)
				RemoveChild(Ch[i]);
		}
	}
}
