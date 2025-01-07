using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamPanno.panno
{
	public class PannoGenerator
	{
		private readonly PannoLoader pannoLoader;

		public PannoGenerator(PannoLoader pannoLoader)
		{
			this.pannoLoader = pannoLoader;
		}

		public async Task<PannoNode> Generate(PannoGame[] games, bool horizontal)
		{
			if (games.Length == 1)
			{
				var game = games.First();
				var pannoImage = horizontal
					? (game.LogoH ?? await game.LoadLogoH(pannoLoader))
					: (game.LogoV ?? await game.LoadLogoV(pannoLoader));
				return new PannoNodeLeaf() { PannoImage = pannoImage };
			}

			var hours = games.Sum(x => x.HoursOnRecord);

			await Task.CompletedTask;
			return null;
		}
	}
}
