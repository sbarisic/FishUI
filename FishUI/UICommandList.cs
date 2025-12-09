using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace FishUI
{
	public class UICommandList
	{
		List<UICommand> Commands = new List<UICommand>();
		bool Recording = false;

		public void Begin()
		{
			if (Recording)
				throw new Exception("Mismatched begin-end calls");

			Recording = true;
			Commands.Clear();
		}

		public void End()
		{
			if (!Recording)
				throw new Exception("Mismatched begin-end calls");
			
			Recording = false;
		}

		public void Perform(IFishUIGfx Gfx)
		{
			if (Recording)
				throw new Exception("Cannot perform while recording");

			foreach (UICommand Cmd in Commands)
			{
				Cmd.Perform(Gfx);
			}
		}
	}
}
