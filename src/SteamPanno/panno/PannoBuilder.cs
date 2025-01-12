using System.Linq;
using System.Threading.Tasks;

namespace SteamPanno.panno
{
	public class PannoBuilder
	{
		private PannoLoader loader;
		private PannoDrawer drawer;

		public PannoBuilder(
			PannoLoader loader,
			PannoDrawer drawer)
		{
			this.loader = loader;
			this.drawer = drawer;
		}

		public async Task Build(PannoNode panno)
		{
			var games = panno.AllLeaves().ToArray();

			foreach (var game in games)
			{
				var image = game.Horizontal
					? await loader.GetGameLogoH(game.Game.Id)
					: await loader.GetGameLogoV(game.Game.Id);

				if (image != null)
				{
					drawer.Draw(game.Area, image);
				}
			}
		}
	}
}
