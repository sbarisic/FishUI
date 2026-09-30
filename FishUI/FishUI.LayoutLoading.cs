using System;
using System.Collections.Generic;
using System.Linq;
using FishUI.Controls;

namespace FishUI
{
    /// <summary>The replacement is active, but one or more old-control cleanup callbacks failed.</summary>
    public sealed class FishUILayoutCleanupException : AggregateException
    {
        public bool LayoutCommitted => true;
        public FishUILayoutCleanupException(IEnumerable<Exception> errors)
            : base("The layout was loaded, but control cleanup failed.", errors) { }
    }

    public partial class FishUI
    {
        private bool _layoutReplacing;
        private bool _layoutPreparing;
        private bool _layoutCleaning;
        private Control[] _layoutOriginal;
        private Control _preparedFocus, _preparedModal;
        private bool _hasPreparedFocus, _hasPreparedModal;
        private readonly HashSet<Control> _detachingControls = new HashSet<Control>();

        internal void CheckHierarchyMutation(Control control)
        {
            if (_layoutCleaning) throw new InvalidOperationException("Cannot modify the hierarchy during layout cleanup.");
            if (IsDetaching(control)) throw new InvalidOperationException("Cannot modify a control during its removal.");
            if (_layoutPreparing && _layoutOriginal.Any(root => IsWithinSubtree(control, root)))
                throw new InvalidOperationException("Cannot modify the original layout while preparing its replacement.");
        }

        private bool IsDetaching(Control control) => _detachingControls.Any(root => IsWithinSubtree(control, root));

        internal void DetachOwnedControl(Control control)
        {
            if (!_detachingControls.Add(control)) return;
            var errors = new List<Exception>();
            Control parent = control.GetParent();
            try
            {
                if (parent != null) parent.Children.Remove(control); else Controls.Remove(control);
                try { PrepareSubtreeDetach(control); } catch (Exception ex) { errors.Add(ex); }
                try { control.DetachSubtree(this); } catch (Exception ex) { errors.Add(ex); }
            }
            finally
            {
                control.SetParentInternal(null);
                control._FishUI = null;
                _detachingControls.Remove(control);
                Diagnostics.NotifyHierarchyChanged();
            }
            if (errors.Count > 0) throw new AggregateException("Control removal completed with callback errors.", errors);
        }

        internal void ReplaceLayout(Func<List<Control>> prepare)
        {
            if (_layoutReplacing) throw new InvalidOperationException("A layout replacement is already in progress.");
            EnsureInitialized();
            _layoutReplacing = _layoutPreparing = true;
            _layoutOriginal = Controls.ToArray();
            var originalHotkeys = Hotkeys.CaptureState();
            var restoreAnimations = Animations.CaptureRestore();
            var restoreHandlers = EventHandlers.CaptureRestore();
            var originalLeases = _keyboardCaptureLeases.ToArray();
            int originalNextDepth = _nextZDepth;
            _hasPreparedFocus = _hasPreparedModal = false;
            try
            {
                try
                {
                    var incoming = prepare();
                    foreach (var root in incoming) root.OnDeserialized(this);
                    foreach (var root in incoming)
                    {
                        int depth = root.ZDepth;
                        AddControl(root);
                        root.ZDepth = depth;
                    }
                    // Initialization may add runtime controls; initialize those before committing too.
                    var initialized = new HashSet<Control>();
                    while (true)
                    {
                        var next = Controls.Where(c => !_layoutOriginal.Contains(c) && initialized.Add(c)).ToArray();
                        if (next.Length == 0) break;
                        foreach (var root in next) root.EnsureInitializedSubtree(this);
                    }
                }
                catch (Exception failure)
                {
                    _layoutCleaning = true;
                    var errors = new List<Exception> { failure };
                    foreach (var root in Controls.Where(c => !_layoutOriginal.Contains(c)).ToArray())
                        try { DetachOwnedControl(root); } catch (Exception ex) { errors.Add(ex); }
                    foreach (var lease in _keyboardCaptureLeases.Except(originalLeases).ToArray()) lease.Dispose();
                    Hotkeys.RestoreState(originalHotkeys);
                    restoreAnimations();
                    restoreHandlers();
                    _nextZDepth = originalNextDepth;
                    if (errors.Count > 1) throw new AggregateException("Layout preparation failed; rollback completed with errors.", errors);
                    throw;
                }

                _layoutPreparing = false;
                _layoutCleaning = true;
                var cleanup = new List<Exception>();
                foreach (var root in _layoutOriginal)
                    try { DetachOwnedControl(root); } catch (Exception ex) { cleanup.Add(ex); }
                _layoutCleaning = false;
                _nextZDepth = Controls.Count == 0 ? 0 : Math.Max(0, Controls.Max(c => c.ZDepth) + 1);
                try { if (_hasPreparedModal) SetModalControl(_preparedModal); } catch (Exception ex) { cleanup.Add(ex); }
                try { if (_hasPreparedFocus) FocusControl(_preparedFocus); } catch (Exception ex) { cleanup.Add(ex); }
                if (cleanup.Count > 0) throw new FishUILayoutCleanupException(cleanup);
            }
            finally
            {
                _layoutPreparing = _layoutReplacing = _layoutCleaning = false;
                _layoutOriginal = null;
                _preparedFocus = _preparedModal = null;
            }
        }
    }
}
