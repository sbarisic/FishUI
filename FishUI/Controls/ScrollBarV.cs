using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void OnScrollChangedFunc(ScrollBarV Sender, float Scroll, int Direction);

	public class ScrollBarV : Control
	{
		public float ThumbHeight = 0.5f; // 0..1
		public float ThumbPosition = 0.1f; // 0..1

		public float ScrollStep = 0.05f;

		public event OnScrollChangedFunc OnScrollChanged;

		[YamlIgnore]
		Vector2 ButtonSize = new Vector2(15, 15);

		[YamlIgnore]
		bool ThumbHovered = false;

		[YamlIgnore]
		bool BtnUpHovered = false;

		[YamlIgnore]
		bool BtnDownHovered = false;

		[YamlIgnore]
		Button BtnThumb;

		[YamlIgnore]
		Button BtnUp;

		[YamlIgnore]
		Button BtnDown;

		bool IsInsideThumb(FishUI UI, Vector2 Pos)
		{
			CalculateThumb(out Vector2 ThumbSize, out Vector2 ThumbPos);

			if (Utils.IsInside(ThumbPos, ThumbSize, Pos))
				return true;

			return false;
		}

		bool IsInsideBtnUp(FishUI UI, Vector2 Pos)
		{
			Vector2 BtnPos = GetAbsolutePosition();

			if (Utils.IsInside(BtnPos, ButtonSize, Pos))
				return true;

			return false;
		}

		bool IsInsideBtnDown(FishUI UI, Vector2 Pos)
		{
			Vector2 BtnPos = GetAbsolutePosition() + new Vector2(0, Size.Y - ButtonSize.Y);

			if (Utils.IsInside(BtnPos, ButtonSize, Pos))
				return true;

			return false;
		}

		void CalculateThumb(out Vector2 ThumbSize, out Vector2 ThumbPos)
		{
			Vector2 BtnSize = ButtonSize;
			Vector2 thumbScrollSize = GetAbsoluteSize() - new Vector2(0, BtnSize.Y + BtnSize.Y);
			Vector2 thumbScrollPos = GetAbsolutePosition() + new Vector2(0, BtnSize.Y);

			float thumbH = thumbScrollSize.Y * ThumbHeight;
			float thumbY = (thumbScrollSize.Y - thumbH) * ThumbPosition;

			ThumbSize = new Vector2(thumbScrollSize.X, thumbH);
			ThumbPos = thumbScrollPos + new Vector2(0, thumbY);
		}


		public override void HandleDrag(FishUI UI, Vector2 StartPos, Vector2 EndPos, FishInputState InState)
		{

		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			ThumbHovered = IsInsideThumb(UI, Pos);
			BtnUpHovered = IsInsideBtnUp(UI, Pos);
			BtnDownHovered = IsInsideBtnDown(UI, Pos);
		}

		void CreateChildControls(FishUI UI)
		{
			BtnDown = new Button(UI.Settings.ImgSBVBtnUpNormal, UI.Settings.ImgSBVBtnUpDisabled, UI.Settings.ImgSBVBtnUpPressed, UI.Settings.ImgSBVBtnUpHover);
			BtnDown.Position = new Vector2(0, 0);
			BtnDown.Size = ButtonSize;
			BtnDown.OnButtonPressed += (sender, btn, pos) =>
			{
				ScrollUp();
			};
			AddChild(BtnDown);

			BtnUp = new Button(UI.Settings.ImgSBVBtnDownNormal, UI.Settings.ImgSBVBtnDownDisabled, UI.Settings.ImgSBVBtnDownPressed, UI.Settings.ImgSBVBtnDownHover);
			BtnUp.Size = ButtonSize;
			BtnUp.Position = new Vector2(0, Size.Y - ButtonSize.Y);
			BtnUp.OnButtonPressed += (sender, btn, pos) =>
			{
				ScrollDown();
			};
			AddChild(BtnUp);

			BtnThumb = new Button(UI.Settings.ImgSBVBarNormal, UI.Settings.ImgSBVBarDisabled, UI.Settings.ImgSBVBarPressed, UI.Settings.ImgSBVBarHover);
			BtnThumb.Draggable = true;
			BtnThumb.OnDragged += (_, Delta) =>
			{
				Vector2 BtnSize = ButtonSize;
				Vector2 thumbScrollSize = GetAbsoluteSize() - new Vector2(0, BtnSize.Y + BtnSize.Y);

				float newThumbY = ThumbPosition * (thumbScrollSize.Y - (thumbScrollSize.Y * ThumbHeight)) + Delta.Y;

				float OldThumbPosition = ThumbPosition;
				ThumbPosition = newThumbY / (thumbScrollSize.Y - (thumbScrollSize.Y * ThumbHeight));

				float Dt = ThumbPosition - OldThumbPosition;
				int Dir = Dt > 0 ? 1 : -1;

				if (ThumbPosition < 0)
					ThumbPosition = 0;
				if (ThumbPosition > 1)
					ThumbPosition = 1;

				OnScrollChanged?.Invoke(this, ThumbPosition, Dir);
			};
			AddChild(BtnThumb);

		}

		public void ScrollUp()
		{
			ThumbPosition -= ScrollStep;

			if (ThumbPosition < 0)
				ThumbPosition = 0;

			OnScrollChanged?.Invoke(this, ThumbPosition, -1);
		}

		public void ScrollDown()
		{
			ThumbPosition += ScrollStep;

			if (ThumbPosition > 1)
				ThumbPosition = 1;

			OnScrollChanged?.Invoke(this, ThumbPosition, 1);
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			if (BtnDown == null)
				CreateChildControls(UI);

			Vector2 GlobalPos = GetAbsolutePosition();

			// Draw background
			UI.Graphics.DrawNPatch(UI.Settings.ImgSBVBarBackground, GlobalPos, GetAbsoluteSize(), Color);

			CalculateThumb(out Vector2 thumbSize, out Vector2 thumbPos);
			if (BtnThumb != null)
			{
				BtnThumb.Position = GetLocalRelative(thumbPos);
				BtnThumb.Size = thumbSize;
			}

			/*// Draw up button
			if (BtnUpHovered)
				UI.Graphics.DrawNPatch(UI.Settings.ImgSBVBtnUpHover, GlobalPos, ButtonSize, Color);
			else
				UI.Graphics.DrawNPatch(UI.Settings.ImgSBVBtnUpNormal, GlobalPos, ButtonSize, Color);

			// Draw down button
			if (BtnDownHovered)
				UI.Graphics.DrawNPatch(UI.Settings.ImgSBVBtnDownHover, GlobalPos + new Vector2(0, Size.Y - ButtonSize.Y), ButtonSize, Color);
			else
				UI.Graphics.DrawNPatch(UI.Settings.ImgSBVBtnDownNormal, GlobalPos + new Vector2(0, Size.Y - ButtonSize.Y), ButtonSize, Color);

			CalculateThumb(out Vector2 thumbSize, out Vector2 thumbPos);

			// Draw thumb
			if (ThumbHovered)
				UI.Graphics.DrawNPatch(UI.Settings.ImgSBVBarHover, thumbPos, thumbSize, Color);
			else
				UI.Graphics.DrawNPatch(UI.Settings.ImgSBVBarNormal, thumbPos, thumbSize, Color);*/
		}
	}
}
