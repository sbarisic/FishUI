using FishUI.Controls;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Globalization;

namespace FishUI
{
    public partial class FishUI
    {
        public void FocusControl(Control Ctrl)
        {
            EnsureInitialized();
            if (Ctrl != null && !IsControlEffectivelyInteractive(Ctrl))
                throw new InvalidOperationException("The focused control must be attached, visible, enabled, and modal-eligible.");
            Control previousFocus = InputActiveControl;
            if (previousFocus != null) Diagnostics.EnsureIdentity(previousFocus);
            if (Ctrl != null) Diagnostics.EnsureIdentity(Ctrl);

            if (previousFocus != null && previousFocus != Ctrl)
                previousFocus.HandleBlur();

            InputActiveControl = Ctrl;

            if (Ctrl != null)
                Ctrl.HandleFocus();

            if (Diagnostics.IsEventRecordingEnabled)
                Diagnostics.Record(FishUIDiagnosticEventCategory.Focus, FishUIDiagnosticEventType.FocusChanged, Ctrl,
                    null, focus: new FishUIFocusEventData
                    {
                        FromControlId = previousFocus?.DiagnosticRuntimeId,
                        ToControlId = Ctrl?.DiagnosticRuntimeId,
                        Changed = !ReferenceEquals(previousFocus, Ctrl)
                    });
        }

        /// <summary>
        /// Clears the current focus without focusing another control.
        /// </summary>
        public void ClearFocus()
        {
            Control previous = InputActiveControl;
            if (previous != null) Diagnostics.EnsureIdentity(previous);
            if (InputActiveControl != null)
            {
                InputActiveControl.HandleBlur();
                InputActiveControl = null;
            }
            if (Diagnostics.IsEventRecordingEnabled)
                Diagnostics.Record(FishUIDiagnosticEventCategory.Focus, FishUIDiagnosticEventType.FocusChanged, previous,
                    "cleared", focus: new FishUIFocusEventData { FromControlId = previous?.DiagnosticRuntimeId, ToControlId = null, Changed = previous != null });
        }

        /// <summary>
        /// Gets all focusable controls in tab order.
        /// </summary>
        List<Control> GetFocusableControls()
        {
            List<Control> focusable = new List<Control>();
            CollectFocusableControls(OrderedControls, focusable);
            focusable.Sort(CompareTabOrder);
            return focusable;
        }

        private static int CompareTabOrder(Control left, Control right) => left.TabIndex.CompareTo(right.TabIndex);

        void CollectFocusableControls(Control[] controls, List<Control> result)
        {
            foreach (Control c in controls)
            {
                if (!c.IsHierarchyVisible() || !c.IsHierarchyEnabled())
                    continue;
                if (c.Focusable && IsControlEffectivelyInteractive(c))
                    result.Add(c);

                CollectFocusableControls(c.FrameChildrenPaintOrder, result);
            }
        }

        /// <summary>
        /// Focuses the next (or previous if reverse is true) focusable control.
        /// </summary>
        /// <param name="reverse">If true, focus the previous control (Shift+Tab behavior).</param>
        public void FocusNextControl(bool reverse = false)
        {
            List<Control> focusable = GetFocusableControls();

            if (focusable.Count == 0)
                return;

            int currentIndex = focusable.IndexOf(InputActiveControl);

            int nextIndex;
            if (currentIndex == -1)
            {
                // No control is focused, focus the first or last
                nextIndex = reverse ? focusable.Count - 1 : 0;
            }
            else
            {
                // Move to next or previous
                if (reverse)
                    nextIndex = (currentIndex - 1 + focusable.Count) % focusable.Count;
                else
                    nextIndex = (currentIndex + 1) % focusable.Count;
            }

            FocusControl(focusable[nextIndex]);
        }

        FishInputState InLast;

    }
}
