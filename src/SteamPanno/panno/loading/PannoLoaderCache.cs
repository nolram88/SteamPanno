using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SteamPanno.panno.loading
{
	public class PannoLoaderCache : PannoLoader
	{
		private readonly PannoLoader innerLoader;
		private readonly string logoPath;
		private readonly string profilesPath;

		public PannoLoaderCache(PannoLoader innerLoader)
		{
			this.innerLoader = innerLoader;
			logoPath = FileExtensions.GetCachePath();
			profilesPath = FileExtensions.GetProfilesPath();
		}

		public override async Task<PannoGame[]> GetProfileGames(
			string steamId,
			CancellationToken cancellationToken)
		{
			if (!TryGetProfileFromCache(steamId, out var profile) ||
				profile.All(x => x.HoursOnRecordPrivate))
			{
				profile = await innerLoader.GetProfileGames(
					steamId, cancellationToken);
				SaveProfileToCache(steamId, profile);
			}
			
			return profile;
		}

		public override async Task<PannoImage> GetGameLogoV(
			int appId,
			CancellationToken cancellationToken)
		{
			if (!TryGetLogoFromCache(appId, "v", out var image))
			{
				image = await innerLoader.GetGameLogoV(
					appId, cancellationToken);
				SaveLogoToCache(appId, "v", image);
			}

			return image;
		}

		public override async Task<PannoImage> GetGameLogoH(
			int appId,
			CancellationToken cancellationToken)
		{
			if (!TryGetLogoFromCache(appId, "h", out var image))
			{
				image = await innerLoader.GetGameLogoH(
					appId, cancellationToken);
				SaveLogoToCache(appId, "h", image);
			}

			return image;
		}

		private bool TryGetLogoFromCache(int appId, string name, out PannoImage image)
		{
			var fileName = GetLogoFileName(appId, name);
			if (File.Exists(fileName))
			{
				var imageFromCache = PannoImage.Load(fileName);
				if (imageFromCache != null)
				{
					image = imageFromCache;
					return true;
				}
			}

			image = null;
			return false;
		}

		private void SaveLogoToCache(int appId, string name, PannoImage image)
		{
			if (image == null) return;

			var fileName = GetLogoFileName(appId, name);
			if (!File.Exists(fileName))
			{
				image.SaveJpg(fileName);
			}
		}

		private string GetLogoFileName(int appId, string name)
		{
			return Path.Combine(logoPath, $"{appId}{name}.jpg");
		}

		private bool TryGetProfileFromCache(string steamId, out PannoGame[] profile)
		{
			var fileName = GetProfileFileName(steamId);
			if (File.Exists(fileName))
			{
				var json = File.ReadAllText(fileName);
				profile = JsonSerializer.Deserialize<PannoGame[]>(json);
				return true;
			}

			profile = null;
			return false;
		}

		private void SaveProfileToCache(string steamId, PannoGame[] profile)
		{
			if (profile == null || profile.Length == 0)
			{
				return;
			}

			var fileName = GetProfileFileName(steamId);
			var json = JsonSerializer.Serialize(profile);
			File.WriteAllText(fileName, json);
		}

		private string GetProfileFileName(string steamId)
		{
			var dateText = DateTime.Today.ToString("yyyy-MM-dd");
			return Path.Combine(profilesPath, $"{steamId}_{dateText}.json");
		}
	}
}
