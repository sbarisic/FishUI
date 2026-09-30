using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
    public partial class RadioButton : Control
    {
        /// <summary>
        /// Whether the radio button is currently IsChecked.
        /// </summary>
        [YamlMember]
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value) return;
                bool previous = _isChecked;
                _isChecked = value;
                RecordDiagnosticTransition("isChecked", previous, value);
                InvokeHandler(OnCheckedChangedHandler, new CheckedChangedEventHandlerArgs(FishUI, value));
            }
        }
        private bool _isChecked;

        /// <summary>
        /// RadioButton disables child scissor so labels can extend beyond the radio button icon bounds.
        /// </summary>
        public override bool DisableChildScissor { get; set; } = true;

        public RadioButton()
        {
        }

        public RadioButton(string LabelText)
        {
            Label Lbl = new Label(LabelText);
            Lbl.Alignment = Align.Left;
            AddChild(Lbl);

        }

        public override void DrawControl(FishUI UI, float Dt, float Time)
        {
            using FishUIDebugRenderScope semantic = UI.Diagnostics.EnterRenderSemantic(FishUIRenderSemantic.ControlBounds);
            //base.Draw(UI, Dt, Time);

            NPatch Cur = UI.Settings.ImgRadioButtonUnchecked;

            if (Disabled)
            {
                if (IsChecked)
                    Cur = UI.Settings.ImgRadioButtonDisabledChecked;
                else
                    Cur = UI.Settings.ImgRadioButtonDisabledUnchecked;
            }
            else
            {
                if (IsChecked)
                    Cur = IsMouseInside ? UI.Settings.ImgRadioButtonCheckedHover : UI.Settings.ImgRadioButtonChecked;
                else
                    Cur = IsMouseInside ? UI.Settings.ImgRadioButtonUncheckedHover : UI.Settings.ImgRadioButtonUnchecked;
            }

            UI.Graphics.DrawNPatch(Cur, GetAbsolutePosition(), GetAbsoluteSize(), ApplyOpacity(Color));

            //DrawChildren(UI, Dt, Time);
        }

        public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
        {
            base.HandleMouseClick(UI, InState, Btn, Pos);
            if (Btn == FishMouseButton.Left)
            {
                IsChecked = !IsChecked;
            }
        }

    }
}
