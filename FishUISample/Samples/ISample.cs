using FishUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishUISample.Samples
{
	internal interface ISample
	{
		/// <summary>
		/// Display name of the sample.
		/// </summary>
		public string Name { get; }

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events);

		public void Init();

		/// <summary>
		/// Sets the action to be called when the sample wants to take a screenshot.
		/// </summary>
		public Action TakeScreenshot { get; set; }
	}
}
