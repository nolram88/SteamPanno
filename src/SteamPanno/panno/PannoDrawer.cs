using Godot;

namespace SteamPanno.panno
{
	public abstract class PannoDrawer
	{
		public PannoImage Dest { get; init; }

		public abstract void Draw(PannoImage src, Rect2I destArea);
	}
}
