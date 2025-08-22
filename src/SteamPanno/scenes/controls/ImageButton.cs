using System;
using Godot;

namespace SteamPanno.scenes.controls
{
	public partial class ImageButton : TextureRect
	{
		private const float alphaMin = 0.3f;
		private const float alphaMax = 0.8f;
		private const float alphaChangePerSecond = 1.5f;
		private const double clickedDelay = 0.2f;

		private float alphaCurrent = 0;
		private float alphaTarget = 0;
		private bool alphaBlink = false;
		private double alphaBlinkDelta = 0;
		private bool clicked = false;
		private double clickedDelta = 0;
		private bool scaledUp = false;

		public Action OnClick { get; set; }

		public void Blink(bool on)
		{
			alphaBlink = on;
			alphaBlinkDelta = 0;
		}

		public override void _Ready()
		{
			alphaCurrent = alphaMin;
			alphaTarget = alphaCurrent;
		}

		public override void _Process(double delta)
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

			Modulate = new Color(1, 1, 1, alphaCurrent);
		}

		public void OnInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseEvent &&
				mouseEvent.ButtonIndex == MouseButton.Left &&
				!mouseEvent.Pressed &&
				!clicked)
			{
				clicked = true;
				PrimitiveScaleDown();
			}
		}

		public void OnMouseEntered()
		{
			alphaTarget = alphaMax;
			Blink(false);
			if (!clicked)
			{
				PrimitiveScaleUp();
			}
		}

		public void OnMouseExited()
		{
			alphaTarget = alphaMin;
			PrimitiveScaleDown();
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
			Position += delta;
			Size -= delta * 2;
		}
	}
}
