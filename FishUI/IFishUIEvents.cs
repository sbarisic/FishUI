using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishUI
{
	// TODO: Refactor and extend events to be more powerful
	// Make multiple specialized functions, instead of a single Broadcast function
	// Add events to all controls (e.g. OnClick, OnHover, OnValueChanged, etc.)
	public interface IFishUIEvents
	{
		public void Broadcast(FishUI FUI, Control Ctrl, string Name, object[] Args);
	}
}
