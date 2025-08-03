using System.IO;
using System.Text.Json;

namespace SteamPanno
{
	public static class Settings
	{
		public class Dto
		{
			public int AccountIdOption { get; set; }
			public string FriendAccountId { get; set; }
			public string CustomAccountId { get; set; }
			public bool UseNativeResolution { get; set; }
			public string CustomResolution { get; set; }
			public int MinimalHoursOption { get; set; }
			public string CustomMinimalHours { get; set; }
			public int GenerationMethodOption { get; set; }
			public int TileExpansionMethodOption { get; set; }
		}

		static Settings()
		{
			Load();
		}

		public static Dto Instance { get; private set; } = new Dto();

		public static void Save()
		{
			var settingsPath = GetSettingsPath();
			var json = JsonSerializer.Serialize(Instance, options: new JsonSerializerOptions()
			{
				WriteIndented = true,
			});
			File.WriteAllText(settingsPath, json);
		}

		public static void Load()
		{
			var settingsPath = GetSettingsPath();
			if (File.Exists(settingsPath))
			{
				var json = File.ReadAllText(settingsPath);
				Instance = JsonSerializer.Deserialize<Dto>(json);
			}
		}

		private static string GetSettingsPath()
		{
			var dataPath = FileExtensions.GetDataPath();
			var settingsPath = Path.Combine(dataPath, "settings.json");

			return settingsPath;
		}
	}
}
