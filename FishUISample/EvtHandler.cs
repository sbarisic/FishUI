using FishUI;
using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishUISample
{
	internal class EvtHandler : IFishUIEvents
	{
		public void Broadcast(FishUI.FishUI FUI, Control Ctrl, string Name, object[] Args)
		{
			if (Ctrl is Button Btn && Name == "mouse_click")
			{
				if (Btn.ID == "visible")
				{
					Panel P1 = FUI.FindControlByID("panel1") as Panel;

					if (P1 != null)
						P1.Visible = true;
				}
				else if (Btn.ID == "invisible")
				{
					Panel P1 = FUI.FindControlByID("panel1") as Panel;

					if (P1 != null)
						P1.Visible = false;
				}
			}
		}
	}
}
