using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno
{
	public class PannoGenerator
	{
		private readonly PannoLoader pannoLoader;

		public PannoGenerator(PannoLoader pannoLoader)
		{
			this.pannoLoader = pannoLoader;
		}

		public async Task<PannoNode> Generate(PannoGame[] games, Rect2I area, bool horizontal)
		{
			games = games.OrderBy(x => x.HoursOnRecord).ToArray();

			return await GenerateInner(games, area, horizontal);
		}

		private async Task<PannoNode> GenerateInner(PannoGame[] games, Rect2I area, bool horizontal)
		{
			if (games.Length == 0)
			{
				return null;
			}
			else if (games.Length == 1)
			{
				var game = games.First();
				var pannoImage = horizontal
					? (game.LogoH ?? await game.LoadLogoH(pannoLoader))
					: (game.LogoV ?? await game.LoadLogoV(pannoLoader));
				return new PannoNodeLeaf()
				{
					Game = game,
					Area = area,
					Horizontal = horizontal,
					//PannoImage = pannoImage,
				};
			}
			else
			{
				var gamesFirst = (PannoGame[])null;
				var gamesSecond = (PannoGame[])null;

				if (games.Length > 2)
				{
					var halfHours = games.Sum(x => x.HoursOnRecord) / 2;
					var gamesSecondHours = 0.0f;
					var gamesSecondCounter = 0;

					while ((gamesSecondHours += games[gamesSecondCounter].HoursOnRecord) < halfHours && gamesSecondCounter < games.Length - 1)
					{
						gamesSecondCounter++;
					}

					gamesSecond = games.Take(gamesSecondCounter).ToArray();
					gamesFirst = games.Skip(gamesSecondCounter).ToArray();
				}
				else
				{
					gamesSecond = new PannoGame[] { games.First() };
					gamesFirst = new PannoGame[] { games.Last() };
				}

				var areaFirst = new Rect2I(
					area.Position.X,
					area.Position.Y,
					(int)Math.Ceiling((decimal)area.Size.X / (horizontal ? 2 : 1)),
					(int)Math.Ceiling((decimal)area.Size.Y / (!horizontal ? 2 : 1)));
				var areaSecond = new Rect2I(
					area.Position.X + (horizontal ? (int)Math.Ceiling((decimal)area.End.X / 2) : 0),
					area.Position.Y + (!horizontal ? (int)Math.Ceiling((decimal)area.End.Y / 2) : 0),
					(int)Math.Floor((decimal)area.Size.X / (horizontal ? 2 : 1)),
					(int)Math.Floor((decimal)area.Size.Y / (!horizontal ? 2 : 1)));
				var nodeFirst = await GenerateInner(gamesFirst, areaFirst, !horizontal);
				var nodeSecond = await GenerateInner(gamesSecond, areaSecond, !horizontal);
				return new PannoNodeRoot(nodeFirst, nodeSecond);
			}
		}
	}
}
