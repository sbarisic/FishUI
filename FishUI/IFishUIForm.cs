using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace FishUI
{
	// TODO: Base interface for .Designer.cs files for FishUI forms
	public interface IFishUIForm
	{
		// Create FishUI and theme here
		public void Init(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events);

		// Load controls and add them to FUI here
		public void LoadControls(FishUI FUI);

		// Called after controls are loaded
		public void OnLoaded();
	}
}
