using System.IO;

namespace SteamPanno.global
{
	public static class Settings
	{
		public static string GetDataPath()
		{
			var dataPath = Path.Combine(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
				nameof(SteamPanno).ToLower());
			
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
	}
}
