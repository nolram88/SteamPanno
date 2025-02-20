using Godot;

namespace SteamPanno
{
	public static class Extensions
	{
		public static bool PreferHorizontal(this Rect2I area)
		{
			return area.Size.X > area.Size.Y;
		}
	}
}
