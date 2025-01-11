using System.Collections.Generic;
using Godot;

namespace SteamPanno.panno
{
	public abstract class PannoNode
	{
		public abstract void Draw(Image image, Rect2I area, bool horizontal);
		public abstract IEnumerable<PannoNode> AllNodes();
		public abstract int Count();
	}
}
