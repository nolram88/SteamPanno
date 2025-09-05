using System.Threading;
using System.Threading.Tasks;

namespace SteamPanno.panno
{
	public abstract class PannoLoader
	{
		public abstract Task<PannoGame[]> GetProfileGames(
			string steamId,
			CancellationToken cancellationToken);
		public abstract Task<PannoImage> GetGameLogoV(
			int appId,
			CancellationToken cancellationToken);
		public abstract Task<PannoImage> GetGameLogoH(
			int appId,
			CancellationToken cancellationToken);
	}
}
