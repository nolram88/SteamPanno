using System.Collections.Generic;
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

			public string Language { get; set; }

			public int AccountIdOption { get; set; }
			public string FriendAccountId { get; set; }
			public string CustomAccountId { get; set; }
			public Dictionary<string, string> SelectedDiffSnapshots { get; set; }
			public string CustomResolution { get; set; }
			public bool UseCustomResolution { get; set; }
			public int MinimalHoursOption { get; set; } = 1;
			public string CustomMinimalHours { get; set; }
			public int GenerationMethodOption { get; set; }
			public int OutpaintingMethodOption { get; set; } = 2;
			public ShowHoursOptions ShowHoursOption { get; set; }

			// hidden settings
			public bool ShowConfigOnStart { get; set; } = false;
			public int MinGameAreaSize { get; set; } = 8;
			public int MaxHoursFontSize { get; set; } = 20;
			public int AreaXSizeToHoursFontSizeRatio { get; set; } = 25;
			public int AreaXSizeToTitleFontSizeRatio { get; set; } = 15;
			public int HttpTimeoutSeconds { get; set; } = 20;
			public int MaxDegreeOfParallelism { get; set; } = 8;
		}

		static Settings()
		{
			Load();
		}

		public static Dto Instance { get; private set; } = new Dto();

		public static void Save()
		{
			var settingsPath = FileExtensions.GetSettingsPath();
			var json = JsonSerializer.Serialize(Instance, options: SerializerOptions);
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
		
		public static string GetSteamId()
		{
			var id = Steam.GetSteamId();

			if (id != null)
			{
				if (Settings.Instance.AccountIdOption == 0)
				{
					return id;
				}
				else if (Settings.Instance.AccountIdOption == 1)
				{
					return Settings.Instance.FriendAccountId.TryParseSteamId(out var friendSteamId)
						? friendSteamId : null;
				}
			}

			return Settings.Instance.CustomAccountId.TryParseSteamId(out var customSteamId)
				? customSteamId : null;
		}

		private static JsonSerializerOptions SerializerOptions { get; set; } = new JsonSerializerOptions()
		{
			WriteIndented = true,
		};
	}
}
