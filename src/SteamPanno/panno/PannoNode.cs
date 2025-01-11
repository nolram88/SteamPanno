using System.Collections.Generic;
using Godot;

namespace SteamPanno.panno
{
	public abstract class PannoNode
	{
		public PannoGame Game { get; init; }
		public Rect2I Area { get; init; }
		public bool Horizontal { get; init; }

		public abstract void Draw(Image image, Rect2I area, bool horizontal);
		public abstract IEnumerable<PannoNode> AllNodes();
		public abstract int Count();
	}
}
