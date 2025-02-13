using Godot;
using System.Collections.Generic;

namespace SteamPanno.panno
{
	public class PannoNodeLeaf : PannoNode
	{
		public PannoGame Game { get; init; }
		public Rect2I Area { get; init; }
		public bool Horizontal { get; init; }
		
		public override IEnumerable<PannoNodeLeaf> AllLeaves()
		{
			yield return this;
		}

		public override int Count()
		{
			return 1;
		}

		public override int Depth()
		{
			return 0;
		}
	}
}
