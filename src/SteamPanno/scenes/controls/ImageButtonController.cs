using System;
using Godot;

namespace SteamPanno.scenes.controls
{
	public class ImageButtonController
	{
		public const float alphaMin = 0.3f;
		public const float alphaMax = 0.8f;
		public const float alphaChangePerSecond = 1.5f;
		public const double clickedDelay = 0.150f;

		private readonly ImageButtonView view;

		private float alphaCurrent = 0;
		private float alphaTarget = 0;
		private bool alphaBlink = false;
		private double alphaBlinkDelta = 0;
		private bool clicked = false;
		private double clickedDelta = 0;
		private bool scaledUp = false;

		public Action OnClick { get; set; }

		public ImageButtonController(ImageButtonView view)
		{
			this.view = view;
			view.OnFrame = Frame;
			view.OnHighlight = Highlight;
			view.OnClick = Click;

			alphaCurrent = alphaMin;
			alphaTarget = alphaCurrent;
		}

		public Vector2 Position
		{
			get => view.Position;
			set => view.Position = value;
		}

		public Vector2 Size
		{
			get => view.Size;
			set => view.Size = value;
		}

		public bool Visible
		{
			get => view.Visible;
			set => view.Visible = value;
		}

		public void Blink(bool on)
		{
			alphaBlink = on;
			alphaBlinkDelta = 0;
		}

		private void Frame(double delta)
		{
			if (clicked)
			{
				clickedDelta += delta;
				if (clickedDelta > clickedDelay)
				{
					clicked = false;
					clickedDelta = 0;
					PrimitiveScaleUp();
					OnClick?.Invoke();
				}
			}

			if (alphaBlink)
			{
				alphaCurrent = ((float)Math.Sin(alphaBlinkDelta * Math.PI * 2) * 0.5f + 0.5f) * (alphaMax - alphaMin) + alphaMin;
				alphaBlinkDelta += delta;
			}
			else if (alphaCurrent != alphaTarget)
			{
				alphaCurrent += alphaChangePerSecond * (float)delta * Mathf.Sign(alphaTarget - alphaCurrent);
			}
			alphaCurrent = Mathf.Clamp(alphaCurrent, alphaMin, alphaMax);

			view.Transparency = alphaCurrent;
		}

		private void Highlight(bool active)
		{
			if (active)
			{
				alphaTarget = alphaMax;
				Blink(false);
				if (!clicked)
				{
					PrimitiveScaleUp();
				}
			}
			else
			{
				alphaTarget = alphaMin;
				PrimitiveScaleDown();
			}
		}

		private void Click()
		{
			if (!clicked)
			{
				clicked = true;
				PrimitiveScaleDown();
			}
		}

		private void PrimitiveScaleUp()
		{
			if (!scaledUp)
			{
				PrimitiveScale(Vector2.One * -2);
				scaledUp = true;
			}
		}

		private void PrimitiveScaleDown()
		{
			if (scaledUp)
			{
				PrimitiveScale(Vector2.One * +2);
				scaledUp = false;
			}
		}

		private void PrimitiveScale(Vector2 delta)
		{
			view.Position += delta;
			view.Size -= delta * 2;
		}
	}
}
