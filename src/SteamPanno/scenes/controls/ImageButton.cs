using System;
using Godot;

namespace SteamPanno.scenes.controls
{
	public partial class ImageButton : Control
	{
		private TextureRect image;
		private float alphaMin = 0.3f;
		private float alphaMax = 0.7f;
		private float alphaCurrent = 0;
		private float alphaTarget = 0;
		private float alphaChangePerSecond = 1.0f;

		[Export]
		public Texture2D ImageTexture
		{
			get
			{
				return GetImage().Texture;
			}
			set
			{
				GetImage().Texture = value;
			}
		}

		public Action OnClick { get; set; }

		public TextureRect GetImage()
		{
			return GetNode<TextureRect>("./Center/TextureRect");
		}

		public override void _Ready()
		{
			image = GetImage();
			alphaCurrent = alphaMin;
			alphaTarget = alphaCurrent;
		}

		public override void _Process(double delta)
		{
			if (alphaCurrent != alphaTarget)
			{
				alphaCurrent += alphaChangePerSecond * (float)delta * Mathf.Sign(alphaTarget - alphaCurrent);
			}

			alphaCurrent = Mathf.Clamp(alphaCurrent, alphaMin, alphaMax);
			image.Modulate = new Color(1, 1, 1, alphaCurrent);
		}

		public override void _Input(InputEvent @event)
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
		}

		public void OnMouseExited()
		{
			alphaTarget = alphaMin;
		}
	}
}
