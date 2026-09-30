using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
    public abstract partial class Control
    {
        [YamlIgnore]
        internal FishUI AttachedFishUI { get; private set; }

        [YamlIgnore]
        private FishUI _initializedFishUI;

        [YamlIgnore]
        public Control MouseFocusTarget { get; set; }

        public virtual bool PreviewKeyPress(FishUI ui, FishInputState input, FishKey key)
        {
            return false;
        }

        protected virtual void OnAttachedToFishUI(FishUI ui) { }
        protected virtual void OnDetachedFromFishUI(FishUI ui) { }
        protected virtual void OnFishUIUpdate(FishUI ui, float deltaTime, float time) { }
        protected virtual void OnFishUIPostInputUpdate(FishUI ui, float deltaTime, float time) { }
        protected virtual void OnFishUIResized(FishUI ui, int width, int height) { }
        protected virtual void PrepareLayout(FishUI ui) { }

        [YamlIgnore]
        protected internal virtual bool RequiresRootAttachment => false;

        internal void AttachSubtree(FishUI ui)
        {
            if (ui == null)
                throw new ArgumentNullException(nameof(ui));

            List<Control> completed = new List<Control>();
            List<Control> assigned = new List<Control>();
            try
            {
                AttachSubtreeCore(ui, completed, assigned);
            }
            catch
            {
                for (int i = completed.Count - 1; i >= 0; i--)
                {
                    Control control = completed[i];
                    try { control.OnDetachedFromFishUI(ui); }
                    catch { }
                    control.AttachedFishUI = null;
                    if (control.Parent == null)
                        control._FishUI = null;
                }
                for (int i = 0; i < assigned.Count; i++)
                {
                    assigned[i].AttachedFishUI = null;
                    if (assigned[i].Parent == null)
                        assigned[i]._FishUI = null;
                }
                throw;
            }
        }

        private void AttachSubtreeCore(FishUI ui, List<Control> completed, List<Control> assigned)
        {
            if (RequiresRootAttachment && Parent != null)
                throw new InvalidOperationException($"{GetType().Name} must be added as a FishUI root control.");
            if (AttachedFishUI != null && AttachedFishUI != ui)
                throw new InvalidOperationException("The control is already attached to another FishUI instance.");

            AttachedFishUI = ui;
            assigned.Add(this);
            if (Parent == null)
                _FishUI = ui;
            OnAttachedToFishUI(ui);
            completed.Add(this);

            Control[] children = GetAllChildren(false);
            for (int i = 0; i < children.Length; i++)
                children[i].AttachSubtreeCore(ui, completed, assigned);
        }

        internal void DetachSubtree(FishUI ui)
        {
            List<Exception> errors = new List<Exception>();
            Control[] children = GetAllChildren(false);
            for (int i = children.Length - 1; i >= 0; i--)
                try { children[i].DetachSubtree(ui); } catch (Exception ex) { errors.Add(ex); }

            if (AttachedFishUI == ui)
            {
                try { OnDetachedFromFishUI(ui); } catch (Exception ex) { errors.Add(ex); }
                AttachedFishUI = null;
                _initializedFishUI = null;
            }
            if (Parent == null)
                _FishUI = null;
            if (errors.Count > 0) throw new AggregateException("Control detach failed after cleanup.", errors);
        }

        internal void UpdateSubtree(FishUI ui, float deltaTime, float time)
        {
            if (AttachedFishUI != ui)
                return;
            OnFishUIUpdate(ui, deltaTime, time);
            Control[] children = GetTraversalChildren();
            for (int i = 0; i < children.Length; i++)
                if (children[i].Parent == this) children[i].UpdateSubtree(ui, deltaTime, time);
        }

        internal void PostInputUpdateSubtree(FishUI ui, float deltaTime, float time)
        {
            if (AttachedFishUI != ui)
                return;
            OnFishUIPostInputUpdate(ui, deltaTime, time);
            Control[] children = FrameChildren;
            for (int i = 0; i < children.Length; i++)
                children[i].PostInputUpdateSubtree(ui, deltaTime, time);
        }

        internal void EnsureInitializedSubtree(FishUI ui)
        {
            if (AttachedFishUI != ui)
                return;
            if (!ReferenceEquals(_initializedFishUI, ui))
            {
                Init(ui);
                _initializedFishUI = ui;
            }
            Control[] children = GetTraversalChildren();
            for (int i = 0; i < children.Length; i++)
                if (children[i].Parent == this) children[i].EnsureInitializedSubtree(ui);
        }

        internal void PrepareLayoutSubtree(FishUI ui)
        {
            if (AttachedFishUI != ui)
                return;
            PrepareLayout(ui);
            Control[] children = GetTraversalChildren();
            for (int i = 0; i < children.Length; i++)
                if (children[i].Parent == this) children[i].PrepareLayoutSubtree(ui);
        }

        internal void ResizeSubtree(FishUI ui, int width, int height)
        {
            if (AttachedFishUI != ui)
                return;
            OnFishUIResized(ui, width, height);
            Control[] children = GetAllChildren(false);
            for (int i = 0; i < children.Length; i++)
                children[i].ResizeSubtree(ui, width, height);
        }

        internal bool IsHierarchyVisible()
        {
            for (Control control = this; control != null; control = control.Parent)
                if (!control.Visible) return false;
            return true;
        }

        internal bool IsHierarchyEnabled()
        {
            for (Control control = this; control != null; control = control.Parent)
                if (control.Disabled) return false;
            return true;
        }

        internal bool IsDescendantOf(Control ancestor)
        {
            for (Control control = Parent; control != null; control = control.Parent)
                if (ReferenceEquals(control, ancestor)) return true;
            return false;
        }
    }
}
