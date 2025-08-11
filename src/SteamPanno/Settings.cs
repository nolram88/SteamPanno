using System.IO;
using System.Text.Json;

namespace SteamPanno
{
	public static class Settings
	{
		public class Dto
		{
			public enum ShowHoursOptions
			{
				OFF = 0,
				BOTTOM = 1,
				BOTTOM_LEFT = 2,
				BOTTOM_RIGHT = 3,
				TOP = 4,
				TOP_LEFT = 5,
				TOP_RIGHT = 6,
			}

			public int AccountIdOption { get; set; }
			public string FriendAccountId { get; set; }
			public string CustomAccountId { get; set; }
			public string CustomResolution { get; set; }
			public bool UseCustomResolution { get; set; }
			public int MinimalHoursOption { get; set; }
			public string CustomMinimalHours { get; set; }
			public int GenerationMethodOption { get; set; }
			public int TileExpansionMethodOption { get; set; }
			public ShowHoursOptions ShowHoursOption { get; set; }

			// hidden settings
			public int MinimalGameAreaSize { get; set; } = 8;
		}

		static Settings()
		{
			Load();
		}

		public static Dto Instance { get; private set; } = new Dto();

		public static void Save()
		{
			var settingsPath = FileExtensions.GetSettingsPath();
			var json = JsonSerializer.Serialize(Instance, options: new JsonSerializerOptions()
			{
				WriteIndented = true,
			});
			File.WriteAllText(settingsPath, json);
		}

		public static void Load()
		{
			var settingsPath = FileExtensions.GetSettingsPath();
			if (File.Exists(settingsPath))
			{
				var json = File.ReadAllText(settingsPath);
				Instance = JsonSerializer.Deserialize<Dto>(json);
			}
		}
	}
}
