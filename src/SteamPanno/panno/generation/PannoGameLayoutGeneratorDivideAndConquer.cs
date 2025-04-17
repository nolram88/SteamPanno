using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno.generation
{
	public class PannoGameLayoutGeneratorDivideAndConquer : PannoGameLayoutGeneratorTreeBased
	{
		private int maxDepth;

		public override async Task<PannoGameLayout[]> Generate(PannoGame[] games, Rect2I area)
		{
			games = games.OrderByDescending(x => x.HoursOnRecord).ToArray();
			maxDepth = 1;
			var root = await GenerateInner(games, area, 1);

			return root.AllLeaves()
				.Select(l => new PannoGameLayout() { Game = l.Game, Area = l.Area })
				.ToArray();
		}

		private async Task<PannoNode> GenerateInner(PannoGame[] games, Rect2I area, int depth)
		{
			if (depth > maxDepth)
			{
				maxDepth = depth;
			}

			if (games.Length == 0)
			{
				return null;
			}
			else if (games.Length == 1)
			{
				var game = games.First();
				return new PannoNodeLeaf()
				{
					Game = game,
					Area = area,
				};
			}
			else
			{
				var gamesFirst = (PannoGame[])null;
				var gamesSecond = (PannoGame[])null;

				if (games.Length > 2)
				{
					var gamesHalfHours = games.Sum(x => x.HoursOnRecord) / 2;
					var gamesFirstCounter = 1;
					var gamesFirstHours = games[gamesFirstCounter - 1].HoursOnRecord;
					var gamesSecondCounter = 1;
					var gamesSecondHours = games[games.Length - gamesSecondCounter].HoursOnRecord;

					while (gamesFirstCounter + gamesSecondCounter < games.Length)
					{
						if (gamesSecondCounter <= gamesFirstCounter || gamesSecondHours < gamesHalfHours)
						{
							gamesSecondCounter++;
							gamesSecondHours += games[games.Length - gamesSecondCounter].HoursOnRecord;
						}
						else
						{
							gamesFirstCounter++;
							gamesFirstHours += games[gamesFirstCounter - 1].HoursOnRecord;
						}
					}

					gamesFirst = games.Take(gamesFirstCounter).ToArray();
					gamesSecond = games.Skip(gamesFirstCounter).ToArray();
				}
				else
				{
					gamesFirst = new PannoGame[] { games.First() };
					gamesSecond = new PannoGame[] { games.Last() };
				}

				if ((area.PreferHorizontal() ? area.Size.X : area.Size.Y) >= 8)
				{
					var areaFirst = GetFirstArea(area);
					var areaSecond = GetSecondArea(area);
					var nodeFirst = await GenerateInner(gamesFirst, areaFirst, depth + 1);
					var nodeSecond = await GenerateInner(gamesSecond, areaSecond, depth + 1);
					return new PannoNodeRoot(nodeFirst, nodeSecond);
				}

				return new PannoNodeLeaf()
				{
					Game = gamesFirst.First(),
					Area = area,
				};
			}
		}
	}
}
