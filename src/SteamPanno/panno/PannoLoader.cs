using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno
{
	public abstract class PannoLoader
	{
		public abstract Task<PannoGame[]> GetProfileGames(string steamId);
		public abstract Task<Image> GetGameLogoV(int appId);
		public abstract Task<Image> GetGameLogoH(int appId);
	}
}
