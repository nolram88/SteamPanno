using System;
using Godot;

namespace SteamPanno.scenes.controls
{
	public partial class ImageButton : TextureRect, ImageButtonView
	{
		public Action<double> OnFrame { get; set; }
		public Action<bool> OnHighlight { get; set; }
		public Action OnClick { get; set; }

		public float Transparency
		{
			get => Modulate.A;
			set => Modulate = new Color(1, 1, 1, value);
		}

		public override void _Process(double delta)
		{
			OnFrame?.Invoke(delta);
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
			OnHighlight?.Invoke(true);
		}

		public void OnMouseExited()
		{
			OnHighlight?.Invoke(false);
		}
	}
}
