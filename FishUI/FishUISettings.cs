using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishUI
{
	public class FishUISettings
	{
		// Public settings
		public int FontSpacing { get; set; } = 0;
		public int FontSize { get; set; } = 14;
		public int FontSizeLabel { get; set; } = 14;


		// Fonts
		public FontRef FontDefault { get; set; }
		public FontRef FontDefaultBold { get; set; }
		public FontRef FontLabel { get; set; }
		public FontRef FontTextboxDefault { get; set; }

		// Button
		public NPatch ImgButtonNormal { get; set; }
		public NPatch ImgButtonHover { get; set; }
		public NPatch ImgButtonDisabled { get; set; }
		public NPatch ImgButtonPressed { get; set; }

		// Checkbox
		public NPatch ImgCheckboxChecked { get; set; }
		public NPatch ImgCheckboxUnchecked { get; set; }
		public NPatch ImgCheckboxDisabledChecked { get; set; }
		public NPatch ImgCheckboxDisabledUnchecked { get; set; }
		public NPatch ImgCheckboxCheckedHover { get; set; }
		public NPatch ImgCheckboxUncheckedHover { get; set; }

		// Panel
		public NPatch ImgPanel { get; set; }
		public NPatch ImgPanelDisabled { get; set; }

		// Radio button
		public NPatch ImgRadioButtonChecked { get; set; }
		public NPatch ImgRadioButtonUnchecked { get; set; }
		public NPatch ImgRadioButtonDisabledChecked { get; set; }
		public NPatch ImgRadioButtonDisabledUnchecked { get; set; }
		public NPatch ImgRadioButtonCheckedHover { get; set; }
		public NPatch ImgRadioButtonUncheckedHover { get; set; }

		// Selection box
		public NPatch ImgSelectionBoxNormal { get; set; }

		// Selection box
		public NPatch ImgTextboxNormal { get; set; }
		public NPatch ImgTextboxActive { get; set; }
		public NPatch ImgTextboxDisabled { get; set; }

		// ListBox
		public NPatch ImgListBoxNormal { get; set; }
		public NPatch ImgListBoxItmSelected { get; set; }
		public NPatch ImgListBoxItmSelectedHovered { get; set; }
		public NPatch ImgListBoxItmHovered { get; set; }

		// Scroll bar vertical
		public NPatch ImgSBVBarNormal { get; set; }
		public NPatch ImgSBVBarPressed { get; set; }
		public NPatch ImgSBVBarHover { get; set; }
		public NPatch ImgSBVBarDisabled { get; set; }
		public NPatch ImgSBVBarBackground { get; set; }
		public NPatch ImgSBVBtnDownNormal { get; set; }
		public NPatch ImgSBVBtnDownPressed { get; set; }
		public NPatch ImgSBVBtnDownHover { get; set; }
		public NPatch ImgSBVBtnDownDisabled { get; set; }
		public NPatch ImgSBVBtnUpNormal { get; set; }
		public NPatch ImgSBVBtnUpPressed { get; set; }
		public NPatch ImgSBVBtnUpHover { get; set; }
		public NPatch ImgSBVBtnUpDisabled { get; set; }

		// Scroll bar horizontal
		public NPatch ImgSBHBarNormal { get; set; }
		public NPatch ImgSBHBarPressed { get; set; }
		public NPatch ImgSBHBarHover { get; set; }
		public NPatch ImgSBHBarDisabled { get; set; }
		public NPatch ImgSBHBarBackground { get; set; }
		public NPatch ImgSBHBtnLeftNormal { get; set; }
		public NPatch ImgSBHBtnLeftPressed { get; set; }
		public NPatch ImgSBHBtnLeftHover { get; set; }
		public NPatch ImgSBHBtnLeftDisabled { get; set; }
		public NPatch ImgSBHBtnRightNormal { get; set; }
		public NPatch ImgSBHBtnRightPressed { get; set; }
		public NPatch ImgSBHBtnRightHover { get; set; }
		public NPatch ImgSBHBtnRightDisabled { get; set; }


		public FishUISettings()
		{
		}

		public void Init(FishUI UI)
		{
			string DataFolder = "data/";
			string FontFolder = "data/fonts/";
			string SBFolder = "data/sb/";

			// Fonts
			FontDefault = UI.Graphics.LoadFont(FontFolder + "ubuntu_mono.ttf", FontSize, FontSpacing, FishColor.Black);
			FontDefaultBold = UI.Graphics.LoadFont(FontFolder + "ubuntu_mono_bold.ttf", FontSize, FontSpacing, FishColor.Black);

			FontTextboxDefault = UI.Graphics.LoadFont(FontFolder + "ubuntu_mono.ttf", FontSize, FontSpacing, FishColor.Black);

			FontLabel = UI.Graphics.LoadFont(FontFolder + "ubuntu_mono.ttf", FontSizeLabel, FontSpacing, FishColor.Black);

			//
			ImgButtonNormal = new NPatch(UI, DataFolder + "button_normal.png", 2, 2, 2, 2);
			ImgButtonHover = new NPatch(UI, DataFolder + "button_hover.png", 2, 2, 2, 2);
			ImgButtonDisabled = new NPatch(UI, DataFolder + "button_disabled.png", 2, 2, 2, 2);
			ImgButtonPressed = new NPatch(UI, DataFolder + "button_pressed.png", 2, 2, 2, 2);

			// Panel
			ImgPanel = new NPatch(UI, DataFolder + "panel2_normal.png", 2, 2, 2, 2);
			ImgPanelDisabled = new NPatch(UI, DataFolder + "panel2_disabled.png", 2, 2, 2, 2);

			// SelectionBox
			ImgSelectionBoxNormal = new NPatch(UI, DataFolder + "selectionbox_normal.png", 2, 2, 2, 2);

			// Textbox
			ImgTextboxNormal = new NPatch(UI, DataFolder + "textbox_normal.png", 1, 1, 1, 1);
			ImgTextboxActive = new NPatch(UI, DataFolder + "textbox_active.png", 1, 1, 1, 1);
			ImgTextboxDisabled = new NPatch(UI, DataFolder + "textbox_disabled.png", 1, 1, 1, 1);

			// Checkbox
			ImgCheckboxChecked = new NPatch(UI, DataFolder + "checkbox_checked.png", 2, 2, 2, 2);
			ImgCheckboxUnchecked = new NPatch(UI, DataFolder + "checkbox_unchecked.png", 2, 2, 2, 2);
			ImgCheckboxDisabledChecked = new NPatch(UI, DataFolder + "checkbox_disabled_checked.png", 2, 2, 2, 2);
			ImgCheckboxDisabledUnchecked = new NPatch(UI, DataFolder + "checkbox_disabled_unchecked.png", 2, 2, 2, 2);
			ImgCheckboxCheckedHover = new NPatch(UI, DataFolder + "checkbox_checked_hover.png", 2, 2, 2, 2);
			ImgCheckboxUncheckedHover = new NPatch(UI, DataFolder + "checkbox_unchecked_hover.png", 2, 2, 2, 2);

			// Radio button
			ImgRadioButtonChecked = new NPatch(UI, DataFolder + "radiobutton_checked.png", 2, 2, 2, 2);
			ImgRadioButtonUnchecked = new NPatch(UI, DataFolder + "radiobutton_unchecked.png", 2, 2, 2, 2);
			ImgRadioButtonDisabledChecked = new NPatch(UI, DataFolder + "radiobutton_disabled_checked.png", 2, 2, 2, 2);
			ImgRadioButtonDisabledUnchecked = new NPatch(UI, DataFolder + "radiobutton_disabled_unchecked.png", 2, 2, 2, 2);
			ImgRadioButtonCheckedHover = new NPatch(UI, DataFolder + "radiobutton_checked_hover.png", 2, 2, 2, 2);
			ImgRadioButtonUncheckedHover = new NPatch(UI, DataFolder + "radiobutton_unchecked_hover.png", 2, 2, 2, 2);

			// ListBox
			ImgListBoxNormal = new NPatch(UI, DataFolder + "listbox_normal.png", 2, 2, 2, 2);
			ImgListBoxItmSelected = new NPatch(UI, DataFolder + "listbox_itm_selected.png", 2, 2, 2, 2);
			ImgListBoxItmSelectedHovered = new NPatch(UI, DataFolder + "listbox_itm_selected_hovered.png", 2, 2, 2, 2);
			ImgListBoxItmHovered = new NPatch(UI, DataFolder + "listbox_itm_hovered.png", 2, 2, 2, 2);


			// Scroll bar vertical
			ImgSBVBarNormal = new NPatch(UI, SBFolder + "v_bar_normal.png", 2, 2, 2, 2);
			ImgSBVBarPressed = new NPatch(UI, SBFolder + "v_bar_pressed.png", 2, 2, 2, 2);
			ImgSBVBarHover = new NPatch(UI, SBFolder + "v_bar_hover.png", 2, 2, 2, 2);
			ImgSBVBarDisabled = new NPatch(UI, SBFolder + "v_bar_disabled.png", 2, 2, 2, 2);
			ImgSBVBarBackground = new NPatch(UI, SBFolder + "v_bar_background.png", 2, 2, 2, 2);

			ImgSBVBtnDownNormal = new NPatch(UI, SBFolder + "v_down_normal.png", 2, 2, 2, 2);
			ImgSBVBtnDownPressed = new NPatch(UI, SBFolder + "v_down_pressed.png", 2, 2, 2, 2);
			ImgSBVBtnDownHover = new NPatch(UI, SBFolder + "v_down_hover.png", 2, 2, 2, 2);
			ImgSBVBtnDownDisabled = new NPatch(UI, SBFolder + "v_down_disabled.png", 2, 2, 2, 2);

			ImgSBVBtnUpNormal = new NPatch(UI, SBFolder + "v_up_normal.png", 2, 2, 2, 2);
			ImgSBVBtnUpPressed = new NPatch(UI, SBFolder + "v_up_pressed.png", 2, 2, 2, 2);
			ImgSBVBtnUpHover = new NPatch(UI, SBFolder + "v_up_hover.png", 2, 2, 2, 2);
			ImgSBVBtnUpDisabled = new NPatch(UI, SBFolder + "v_up_disabled.png", 2, 2, 2, 2);

			// Scroll bar horizontal
			ImgSBHBarNormal = new NPatch(UI, SBFolder + "h_bar_normal.png", 2, 2, 2, 2);
			ImgSBHBarPressed = new NPatch(UI, SBFolder + "h_bar_pressed.png", 2, 2, 2, 2);
			ImgSBHBarHover = new NPatch(UI, SBFolder + "h_bar_hover.png", 2, 2, 2, 2);
			ImgSBHBarDisabled = new NPatch(UI, SBFolder + "h_bar_disabled.png", 2, 2, 2, 2);
			ImgSBHBarBackground = new NPatch(UI, SBFolder + "h_bar_background.png", 2, 2, 2, 2);

			ImgSBHBtnLeftNormal = new NPatch(UI, SBFolder + "h_left_normal.png", 2, 2, 2, 2);
			ImgSBHBtnLeftPressed = new NPatch(UI, SBFolder + "h_left_pressed.png", 2, 2, 2, 2);
			ImgSBHBtnLeftHover = new NPatch(UI, SBFolder + "h_left_hover.png", 2, 2, 2, 2);
			ImgSBHBtnLeftDisabled = new NPatch(UI, SBFolder + "h_left_disabled.png", 2, 2, 2, 2);

			ImgSBHBtnRightNormal = new NPatch(UI, SBFolder + "h_right_normal.png", 2, 2, 2, 2);
			ImgSBHBtnRightPressed = new NPatch(UI, SBFolder + "h_right_pressed.png", 2, 2, 2, 2);
			ImgSBHBtnRightHover = new NPatch(UI, SBFolder + "h_right_hover.png", 2, 2, 2, 2);
			ImgSBHBtnRightDisabled = new NPatch(UI, SBFolder + "h_right_disabled.png", 2, 2, 2, 2);

		}
	}
}
