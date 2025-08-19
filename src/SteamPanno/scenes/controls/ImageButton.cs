using System;
using Godot;

namespace SteamPanno.scenes.controls
{
	public partial class ImageButton : TextureRect
	{
		private float alphaMin = 0.3f;
		private float alphaMax = 0.8f;
		private float alphaCurrent = 0;
		private float alphaTarget = 0;
		private float alphaChangePerSecond = 1.5f;

		private double alphaBlinkDelta = 0;
		private bool alphaBlink = false;

		public Action OnClick { get; set; }

		public void Blink(bool on)
		{
			alphaBlinkDelta = 0;
			alphaBlink = on;
		}

		public override void _Ready()
		{
			alphaCurrent = alphaMin;
			alphaTarget = alphaCurrent;
		}

		public override void _Process(double delta)
		{
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
				!mouseEvent.Pressed)
			{
				OnClick?.Invoke();
			}
		}

		public void OnMouseEntered()
		{
			alphaTarget = alphaMax;
			Blink(false);
		}

		public void OnMouseExited()
		{
			alphaTarget = alphaMin;
		}
	}
}
