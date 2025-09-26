using System;
using Godot;

namespace SteamPanno.scenes.controls
{
	public interface ImageButtonView
	{
		Vector2 Position { get; set; }
		Vector2 Size { get; set; }
		bool Visible { get; set; }
		float Transparency { get; set; }

		Action<double> OnFrame { get; set; }
		Action<bool> OnHighlight { get; set; }
		Action OnClick { get; set; }
	}
}
