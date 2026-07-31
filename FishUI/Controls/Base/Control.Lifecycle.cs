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
		public Control MouseFocusTarget { get; set; }

		public virtual bool PreviewKeyPress(FishUI ui, FishInputState input, FishKey key)
		{
			return false;
		}

		protected virtual void OnAttachedToFishUI(FishUI ui) { }
		protected virtual void OnDetachedFromFishUI(FishUI ui) { }
		protected virtual void OnFishUIUpdate(FishUI ui, float deltaTime, float time) { }
		protected virtual void OnFishUIResized(FishUI ui, int width, int height) { }

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
			Control[] children = GetAllChildren(false);
			for (int i = children.Length - 1; i >= 0; i--)
				children[i].DetachSubtree(ui);

			if (AttachedFishUI == ui)
			{
				OnDetachedFromFishUI(ui);
				AttachedFishUI = null;
			}
			if (Parent == null)
				_FishUI = null;
		}

		internal void UpdateSubtree(FishUI ui, float deltaTime, float time)
		{
			if (AttachedFishUI != ui)
				return;
			OnFishUIUpdate(ui, deltaTime, time);
			Control[] children = GetAllChildren(false);
			for (int i = 0; i < children.Length; i++)
				children[i].UpdateSubtree(ui, deltaTime, time);
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

		internal bool IsDescendantOf(Control ancestor)
		{
			for (Control control = Parent; control != null; control = control.Parent)
				if (ReferenceEquals(control, ancestor)) return true;
			return false;
		}
	}
}
