using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamPanno.panno
{
	public class PannoNodeRoot : PannoNode
	{
		private PannoNode[] leaves;

		public override void Draw(Image image)
		{
			foreach (var leaf in leaves)
			{
				leaf.Draw(image);
			}
		}

		public override int Count()
		{
			return leaves.Sum(x => x.Count());
		}
	}
}
