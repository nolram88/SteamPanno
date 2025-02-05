using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno
{
	public class PannoGenerator
	{
		public async Task<PannoNode> Generate(PannoGame[] games, Rect2I area, bool horizontal)
		{
			games = games.OrderByDescending(x => x.HoursOnRecord).ToArray();

			return await DivideAndConquer(games, area, horizontal);
		}

		private async Task<PannoNode> DivideAndConquer(PannoGame[] games, Rect2I area, bool horizontal)
		{
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
					Horizontal = horizontal,
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

				if ((horizontal ? area.Size.X : area.Size.Y) >= 8)
				{
					var areaFirst = GetFirstArea(area, horizontal);
					var areaSecond = GetSecondArea(area, horizontal);
					var nodeFirst = await DivideAndConquer(gamesFirst, areaFirst, !horizontal);
					var nodeSecond = await DivideAndConquer(gamesSecond, areaSecond, !horizontal);
					return new PannoNodeRoot(nodeFirst, nodeSecond);
				}
				
				return new PannoNodeLeaf()
				{
					Game = gamesFirst.First(),
					Area = area,
					Horizontal = horizontal,
				};
			}
		}

		private Rect2I GetFirstArea(Rect2I area, bool horizontal)
		{
			return new Rect2I(
				area.Position.X,
				area.Position.Y,
				(int)Math.Ceiling((decimal)area.Size.X / (horizontal ? 2 : 1)),
				(int)Math.Ceiling((decimal)area.Size.Y / (!horizontal ? 2 : 1)));
		}

		private Rect2I GetSecondArea(Rect2I area, bool horizontal)
		{
			return new Rect2I(
				area.Position.X + (horizontal ? (int)Math.Ceiling((decimal)area.Size.X / 2) : 0),
				area.Position.Y + (!horizontal ? (int)Math.Ceiling((decimal)area.Size.Y / 2) : 0),
				(int)Math.Floor((decimal)area.Size.X / (horizontal ? 2 : 1)),
				(int)Math.Floor((decimal)area.Size.Y / (!horizontal ? 2 : 1)));
		}
	}
}
