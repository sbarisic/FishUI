using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishUI
{
	public interface IFishUIEvents
	{
		public void Broadcast(FishUI FUI, Control Ctrl, string Name, object[] Args);
	}
}
