using Godot;
using System;

namespace SteamPanno
{
	public static class GodotExtensions
	{
		public static bool PreferHorizontal(this Rect2I area)
		{
			// hard optimized
			return (float)area.Size.X / area.Size.Y >= 1.080;

			// same, but fully calculated
			/*
			var vSize = new Vector2(600, 900);
			var hSize = new Vector2(616, 353);

			var vRatio = vSize / new Vector2(area.Size.X, area.Size.Y);
			var hRatio = hSize / new Vector2(area.Size.X, area.Size.Y);

			var vFitted = vSize / Math.Max(vRatio.X, vRatio.Y);
			var hFitted = hSize / Math.Max(hRatio.X, hRatio.Y);

			var areaV = vFitted.X * vFitted.Y;
			var areaH = hFitted.X * hFitted.Y;

			return areaH > areaV;
			*/
		}
	}
}
