using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FishUI.Controls
{
	public class Button : Control
	{
		public override Vector2 Position
		{
			get => base.Position;
			set => base.Position = value;
		}

		public override Vector2 Size
		{
			get => base.Size;
			set => base.Size = value;
		}
	}
}
