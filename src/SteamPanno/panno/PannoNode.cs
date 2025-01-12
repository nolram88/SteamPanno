using System.Collections.Generic;

namespace SteamPanno.panno
{
	public abstract class PannoNode
	{
		public abstract IEnumerable<PannoNodeLeaf> AllLeaves();
		public abstract int Count();
	}
}
