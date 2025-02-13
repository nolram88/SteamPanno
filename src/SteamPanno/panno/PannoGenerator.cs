using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno
{
	public abstract class PannoGenerator
	{
		public abstract Task<PannoNode> Generate(PannoGame[] games, Rect2I area, bool horizontal);
		
		protected virtual Rect2I GetFirstArea(Rect2I area, bool horizontal)
		{
			return new Rect2I(
				area.Position.X,
				area.Position.Y,
				(int)Math.Ceiling((decimal)area.Size.X / (horizontal ? 2 : 1)),
				(int)Math.Ceiling((decimal)area.Size.Y / (!horizontal ? 2 : 1)));
		}

		protected virtual Rect2I GetSecondArea(Rect2I area, bool horizontal)
		{
			return new Rect2I(
				area.Position.X + (horizontal ? (int)Math.Ceiling((decimal)area.Size.X / 2) : 0),
				area.Position.Y + (!horizontal ? (int)Math.Ceiling((decimal)area.Size.Y / 2) : 0),
				(int)Math.Floor((decimal)area.Size.X / (horizontal ? 2 : 1)),
				(int)Math.Floor((decimal)area.Size.Y / (!horizontal ? 2 : 1)));
		}
	}
}
