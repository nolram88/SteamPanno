using Godot;
using System.Collections.Generic;
using System.Linq;

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

		public override void Draw(Image image, Rect2I area, bool horizontal)
		{
			var firstArea = new Rect2I(
				area.Position.X,
				area.Position.Y,
				area.End.X / (horizontal ? 2 : 1),
				area.End.Y / (!horizontal ? 2 : 1));
			var secondArea = new Rect2I(
				area.Position.X + (horizontal ? area.End.X / 2 : 0),
				area.Position.Y + (!horizontal ? area.End.Y / 2 : 0),
				area.End.X / (horizontal ? 2 : 1),
				area.End.Y / (!horizontal ? 2 : 1));

			first.Draw(image, firstArea, !horizontal);
			second.Draw(image, secondArea, !horizontal);
		}

		public override IEnumerable<PannoNode> AllNodes()
		{
			foreach (var leaf in first.AllNodes())
			{
				yield return leaf;
			}
			foreach (var leaf in second.AllNodes())
			{
				yield return leaf;
			}
		}

		public override int Count()
		{
			return first.Count() + second.Count();
		}
	}
}
