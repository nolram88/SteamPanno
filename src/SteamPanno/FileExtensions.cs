using System.IO;

namespace SteamPanno
{
	public static class FileExtensions
	{
		public static string GetDataPath()
		{
			var dataPath = Path.Combine(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
				nameof(SteamPanno).ToLower());
			if (!Directory.Exists(dataPath))
			{
				Directory.CreateDirectory(dataPath);
			}

			return dataPath;
		}
		
		public static string GetGenerationPath()
		{
			var generationPath = Path.Combine(GetDataPath(), "generated");
			if (!Directory.Exists(generationPath))
			{
				Directory.CreateDirectory(generationPath);
			}

			return generationPath;
		}

		public static string GetCachePath()
		{
			var cachePath = Path.Combine(GetDataPath(), "cache");
			if (!Directory.Exists(cachePath))
			{
				Directory.CreateDirectory(cachePath);
			}

			return cachePath;
		}
		
		public static string GetSettingsPath()
		{
			var settingsPath = Path.Combine(GetDataPath(), "settings.json");
			
			return settingsPath;
		}
	}
}
