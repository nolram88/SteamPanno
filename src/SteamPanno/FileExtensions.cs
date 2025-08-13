using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SteamPanno
{
	public static class FileExtensions
	{
		public static string GetDataPath()
		{
			var dataPath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				nameof(SteamPanno).ToLower());
			if (!Directory.Exists(dataPath))
			{
				Directory.CreateDirectory(dataPath);
			}

			return dataPath;
		}
		
		public static string GetGenerationPath()
		{
			var generationPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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

		public static string GetProfilesPath()
		{
			var cachePath = Path.Combine(GetDataPath(), "profiles");
			if (!Directory.Exists(cachePath))
			{
				Directory.CreateDirectory(cachePath);
			}

			return cachePath;
		}

		public static Dictionary<string, Dictionary<string, string>> GetProfileSnapshots()
		{
			var result = new Dictionary<string, Dictionary<string, string>>();
			var files = Directory
				.GetFiles(GetProfilesPath(), "*.json")
				.Select(x => Path.GetFileName(x))
				.ToArray();

			foreach (var file in files)
			{
				if (file.TryParseProfileSnapshotFileName(out var steamId, out var date))
				{
					if (DateTime.Parse(date) >= DateTime.Today)
					{
						continue;
					}

					if (result.TryGetValue(steamId, out var profileSnapshots))
					{
						profileSnapshots.Add(date, file);
					}
					else
					{
						result[steamId] = new Dictionary<string, string>()
						{
							{ date, file }
						};
					}
				}
			}

			return result;
		}

		public static string GetSettingsPath()
		{
			var settingsPath = Path.Combine(GetDataPath(), "settings.json");
			
			return settingsPath;
		}
	}
}
