using System.IO;
using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno
{
	public class PannoLoaderCache : PannoLoader
	{
		private readonly PannoLoader innerLoader;
		private readonly string cachePath;

		public PannoLoaderCache(PannoLoader innerLoader)
		{
			this.innerLoader = innerLoader;
			var appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
			this.cachePath = Path.Combine(appDataPath, nameof(SteamPanno).ToLower(), "cache");
			GD.Print(cachePath);
			if (!Directory.Exists(cachePath))
			{
				Directory.CreateDirectory(cachePath);
			}
		}

		public override async Task<PannoGame[]> GetProfileGames(string steamId)
		{
			return await innerLoader.GetProfileGames(steamId);
		}

		public override async Task<Image> GetGameLogoV(int appId)
		{
			if (!TryGetFromCache(appId, "v", out var image))
			{
				image = await innerLoader.GetGameLogoV(appId);
				TrySaveToCache(appId, "v", image);
			}

			return image;
		}

		public override async Task<Image> GetGameLogoH(int appId)
		{
			if (!TryGetFromCache(appId, "h", out var image))
			{
				image = await innerLoader.GetGameLogoH(appId);
				TrySaveToCache(appId, "h", image);
			}

			return image;
		}

		private bool TryGetFromCache(int appId, string name, out Image image)
		{
			image = null;
			var fileName = GetFileName(appId, name);
			if (File.Exists(fileName))
			{
				var imageFromCache = new Image();
				if (imageFromCache.Load(fileName) == Error.Ok)
				{
					image = imageFromCache;
					return true;
				}
			}

			return false;
		}

		private void TrySaveToCache(int appId, string name, Image image)
		{
			if (image == null) return;

			var fileName = GetFileName(appId, name);
			if (!File.Exists(fileName))
			{
				image.SaveJpg(fileName);
			}
		}

		private string GetFileName(int appId, string name)
		{
			return Path.Combine(cachePath, $"{appId}{name}.jpg");
		}
	}
}
