using System;
using System.Collections.Generic;

namespace FishUI.Controls
{
    public abstract partial class Control
    {
        private Control[] _frameChildren = Array.Empty<Control>();
        private Control[] _frameChildrenPaintOrder = Array.Empty<Control>();
        private int _frameChildrenSignature;

        [YamlDotNet.Serialization.YamlIgnore]
        internal Control[] FrameChildren => _frameChildren;
        [YamlDotNet.Serialization.YamlIgnore]
        internal Control[] FrameChildrenPaintOrder => _frameChildrenPaintOrder;

        internal void FreezeFrameHierarchy()
        {
            int signature = Children.Count;
            for (int i = 0; i < Children.Count; i++)
            {
                Control child = Children[i];
                signature = HashCode.Combine(signature, child == null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(child),
                    child?.ZDepth ?? 0, child?.AlwaysOnTop ?? false);
            }

            if (signature != _frameChildrenSignature || _frameChildren.Length != Children.Count)
            {
                _frameChildren = Children.ToArray();
                _frameChildrenPaintOrder = (Control[])_frameChildren.Clone();
                for (int i = 1; i < _frameChildrenPaintOrder.Length; i++)
                {
                    Control value = _frameChildrenPaintOrder[i];
                    int j = i - 1;
                    while (j >= 0 && PaintsAfter(_frameChildrenPaintOrder[j], value))
                    {
                        _frameChildrenPaintOrder[j + 1] = _frameChildrenPaintOrder[j];
                        j--;
                    }
                    _frameChildrenPaintOrder[j + 1] = value;
                }
                _frameChildrenSignature = signature;
            }

            for (int i = 0; i < _frameChildren.Length; i++)
                _frameChildren[i]?.FreezeFrameHierarchy();
        }

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
            if (Child == null) throw new ArgumentNullException(nameof(Child));
            if (ReferenceEquals(Child, this) || IsDescendantOf(Child))
                throw new InvalidOperationException("A control cannot be parented to itself or one of its descendants.");
            if (Child.RequiresRootAttachment)
                throw new InvalidOperationException($"{Child.GetType().Name} must be added through FishUI.AddControl.");
            if (Children.Contains(Child) && Child.Parent == null && Child.AttachedFishUI == null)
            {
                Child.Parent = this;
                UpdateChildAnchorOffsets(Child);
                return;
            }
            if (ReferenceEquals(Child.Parent, this) && Children.Contains(Child))
            {
                UpdateChildAnchorOffsets(Child);
                return;
            }

            FishUI oldUi = Child.AttachedFishUI;
            FishUI newUi = AttachedFishUI;
            Control oldParent = Child.Parent;
            int oldIndex = oldParent != null ? oldParent.Children.IndexOf(Child) : oldUi?.IndexOfRoot(Child) ?? -1;
            int oldZDepth = Child.ZDepth;

            if (ReferenceEquals(oldUi, newUi))
            {
                RemoveFromOldOwner(Child, oldParent, oldUi);
                AttachChildReference(Child);
                newUi?.Diagnostics.AttachControl(Child);
                newUi?.Diagnostics.NotifyHierarchyChanged();
                return;
            }

            if (oldUi != null)
            {
                oldUi.PrepareSubtreeDetach(Child);
                Child.DetachSubtree(oldUi);
            }
            RemoveFromOldOwner(Child, oldParent, oldUi);
            AttachChildReference(Child);
            try
            {
                if (newUi != null)
                {
                    newUi.Diagnostics.AttachControl(Child);
                    Child.AttachSubtree(newUi);
                    Child.ResizeSubtree(newUi, newUi.Width, newUi.Height);
                }
            }
            catch (Exception attachFailure)
            {
                Children.Remove(Child);
                Child.Parent = null;
                try
                {
                    RestoreOldOwner(Child, oldParent, oldUi, oldIndex, oldZDepth);
                    if (oldUi != null) Child.AttachSubtree(oldUi);
                }
                catch (Exception rollbackFailure) { throw new AggregateException(attachFailure, rollbackFailure); }
                throw;
            }
            newUi?.Diagnostics.NotifyHierarchyChanged();
        }

        private void AttachChildReference(Control child)
        {
            child.Parent = this;
            child._FishUI = null;
            UpdateChildAnchorOffsets(child);
            child.ZDepth = Children.Count;
            Children.Add(child);
        }

        private static void RemoveFromOldOwner(Control child, Control oldParent, FishUI oldUi)
        {
            if (oldParent != null) oldParent.Children.Remove(child); else oldUi?.RemoveRootReference(child);
            child.Parent = null;
            child._FishUI = null;
        }

        private static void RestoreOldOwner(Control child, Control oldParent, FishUI oldUi, int oldIndex, int oldZDepth)
        {
            child.ZDepth = oldZDepth;
            if (oldParent != null)
            {
                child.Parent = oldParent;
                oldParent.Children.Insert(Math.Max(0, Math.Min(oldIndex, oldParent.Children.Count)), child);
            }
            else if (oldUi != null)
            {
                child._FishUI = oldUi;
                oldUi.InsertRootReference(child, oldIndex);
            }
        }

        /// <summary>
        /// Adds an implementation-created child while retaining the control's existing serialization behavior.
        /// </summary>
        protected void AddRuntimeChild(Control child)
        {
            child.IsRuntimeChild = true;
            AddChild(child);
        }

        /// <summary>
        /// Clears the parent reference of this control (used for reparenting).
        /// </summary>
        public void ClearParent()
        {
            Unparent();
        }

        /// <summary>
        /// Sets the parent reference of this control without adding to parent's Children list.
        /// Used internally for deserialization and special cases.
        /// </summary>
        internal void SetParentInternal(Control parent)
        {
            Parent = parent;
            FishUI?.Diagnostics.NotifyHierarchyChanged();
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
            Control[] result = Children.ToArray();
            if (Order)
            {
                // Stable insertion sort preserves child insertion order for equal paint keys.
                for (int i = 1; i < result.Length; i++)
                {
                    Control value = result[i];
                    int j = i - 1;
                    while (j >= 0 && PaintsAfter(result[j], value))
                    {
                        result[j + 1] = result[j];
                        j--;
                    }
                    result[j + 1] = value;
                }
            }
            return result;
        }

        private static bool PaintsAfter(Control left, Control right)
        {
            if (left.AlwaysOnTop != right.AlwaysOnTop) return left.AlwaysOnTop;
            return left.ZDepth > right.ZDepth;
        }

        /// <summary>
        /// Removes a child control from this control.
        /// </summary>
        /// <param name="Child">The child control to remove.</param>
        public void RemoveChild(Control Child)
        {
            if (Child == null || !Children.Contains(Child)) return;
            FishUI ui = Child.AttachedFishUI;
            if (ui != null)
            {
                ui.PrepareSubtreeDetach(Child);
                Child.DetachSubtree(ui);
            }
            Children.Remove(Child);
            Child.Parent = null;
            Child._FishUI = null;
            ui?.Diagnostics.NotifyHierarchyChanged();
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
