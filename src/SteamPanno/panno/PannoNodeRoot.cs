using Godot;
using System.Collections.Generic;

namespace SteamPanno.panno
{
	public class PannoNodeRoot : PannoNode
	{
		private PannoNode first;
		private PannoNode second;

		public PannoNodeRoot(PannoNode first, PannoNode second)
		{
			this.first = first;
			this.second = second;
		}
		
		public override IEnumerable<PannoNodeLeaf> AllLeaves()
		{
			foreach (var leaf in first.AllLeaves())
			{
				yield return leaf;
			}
			foreach (var leaf in second.AllLeaves())
			{
				yield return leaf;
			}
		}

		public override int Count()
		{
			return first.Count() + second.Count();
		}

		public override int Depth()
		{
			return Mathf.Max(first.Depth(), second.Depth()) + 1;
		}
	}
}
