using FishUI;
using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishUIEditor
{
	internal class EvtHandler : IFishUIEvents
	{
		public void Broadcast(FishUI.FishUI FUI, Control Ctrl, string Name, object[] Args)
		{
			if (Ctrl is Button Btn && Name == "mouse_click")
			{
				if (Btn.ID == "visible")
				{
					Control P1 = FUI.FindControlByID("panel1");

					if (P1 != null)
						P1.Visible = true;
				}
				else if (Btn.ID == "invisible")
				{
					Control P1 = FUI.FindControlByID("panel1");

					if (P1 != null)
						P1.Visible = false;
				}
				else if (Btn.ID == "savelayout")
				{
					LayoutFormat.SerializeToFile(FUI, "layout.yaml");
					Console.WriteLine("Wrote file!");
				}
				else if (Btn.ID == "loadlayout")
				{
					LayoutFormat.DeserializeFromFile(FUI, "layout.yaml");
					Console.WriteLine("Loaded from file!");
				}
			}
			else if (Ctrl is ListBox Lb && Name == "item_selected")
			{
				int Idx = (int)Args[0];

				if (Idx == 4)
					Lb.ShowScrollBar = false;
				else if (Idx == 5)
					Lb.ShowScrollBar = true;
			}
		}
	}
}
