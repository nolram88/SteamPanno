using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Godot;

namespace SteamPanno.panno
{
	public class PannoLoaderOnline : PannoLoader
	{
		private enum LogoType
		{
			LIBRARY = 0,
			CAPSULE = 1,
			HEADER = 2,
		}

		private const string GetProfileUrl = "https://steamcommunity.com/profiles/{0}/games?tab=all&xml=1";
		private const string GetLogoV = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/library_600x900_2x.jpg";
		private const string GetLogoH = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/capsule_616x353.jpg";
		private const string GetLogoHeader = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/header.jpg";

		public override async Task<PannoGame[]> GetProfileGames(string steamId)
		{
			var client = new System.Net.Http.HttpClient();
			var url = string.Format(GetProfileUrl, steamId);
			using (var response = await client.GetAsync(url))
			{
				response.EnsureSuccessStatusCode();
				var responseBody = await response.Content.ReadAsStringAsync();
				var xml = XDocument.Parse(responseBody);
				var games = xml.Root.Element("games").Elements("game");

				return games.Select(x => new PannoGame()
				{
					Id = int.Parse(x.Element("appID").Value),
					Name = x.Element("name").Value,
					HoursOnRecord = float.Parse(x.Element("hoursOnRecord")?.Value ?? "0", CultureInfo.InvariantCulture),
				}).ToArray();
			}
		}

		public override async Task<PannoImage> GetGameLogoV(int appId) =>
			await GetGameLogo(appId, LogoType.LIBRARY);

		public override async Task<PannoImage> GetGameLogoH(int appId) =>
			await GetGameLogo(appId, LogoType.CAPSULE) ?? await GetGameLogo(appId, LogoType.HEADER);

		private async Task<PannoImage> GetGameLogo(int appId, LogoType logo)
		{
			var url = (logo) switch
			{
				LogoType.LIBRARY => GetLogoV,
				LogoType.CAPSULE => GetLogoH,
				LogoType.HEADER => GetLogoHeader,
				_ => null,
			};

			if (!string.IsNullOrEmpty(url) && appId != default)
			{
				url = string.Format(url, appId);

				var client = new System.Net.Http.HttpClient();
				using (var response = await client.GetAsync(url))
				{
					if (response.IsSuccessStatusCode)
					{
						var responseBody = await response.Content.ReadAsByteArrayAsync();

						return PannoImage.Load(responseBody);
					}
				}
			}

			return null;
		}
	}
}
