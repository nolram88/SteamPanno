using Godot;

namespace SteamPanno.panno
{
	public abstract class PannoDrawer
	{
		public Image Panno { get; init; }

		public abstract void Draw(Rect2I area, Image image);
	}
}
