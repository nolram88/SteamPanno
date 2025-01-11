using System;
using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno
{
	public class PannoGame
	{
		public int Id { get; init; }
		public string Name { get; init; }
		public float HoursOnRecord { get; init; }

		public PannoImage LogoV { get; protected set; }
		public PannoImage LogoH { get; protected set; }

		public async Task<PannoImage> LoadLogoV(PannoLoader loader)
		{
			LogoV = await loader.GetGameLogoV(Id);

			return LogoV;
		}

		public async Task<PannoImage> LoadLogoH(PannoLoader loader)
		{
			LogoH = await loader.GetGameLogoH(Id);

			return LogoH;
		}
	}
}
