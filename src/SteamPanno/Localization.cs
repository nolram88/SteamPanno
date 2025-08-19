using Godot;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SteamPanno
{
	public static class Localization
	{
		private readonly static Dictionary<string, string> localizationDefault;
		private static Dictionary<string, string> localizationActive;

		static Localization()
		{
			var langDefault = "en";
			var langActive = Settings.Instance.Localization ?? "ru";

			localizationDefault = LoadLocalization(langDefault);
			localizationActive = langDefault != langActive
				? LoadLocalization(langActive)
				: null;
		}

		public static string[] GetLocalizations()
		{
			var files = DirAccess.GetFilesAt($"res://assets/translations");

			return files;
		}

		public static string Localize(string path)
		{
			if (localizationActive != null &&
				localizationActive.TryGetValue(path, out var localizedActive))
			{
				return localizedActive;
			}

			if (localizationDefault.TryGetValue(path, out var localizedDefault))
			{
				return localizedDefault;
			}

			return path;
		}

		private static Dictionary<string, string> LoadLocalization(string language)
		{
			Dictionary<string, string> localization = new Dictionary<string, string>();

			using FileAccess file = FileAccess.Open(
				$"res://assets/translations/{language}.json",
				FileAccess.ModeFlags.Read);
			if (file != null)
			{
				var json = file.GetAsText();
				JsonNode node = JsonNode.Parse(json);
				AppendLocalization(localization, node, string.Empty);
			}

			return localization;
		}

		private static void AppendLocalization(
			Dictionary<string, string> localization,
			JsonNode node,
			string path)
		{
			switch (node.GetValueKind())
			{
				case JsonValueKind.Object:
					foreach (KeyValuePair<string, JsonNode> property in node.AsObject())
					{
						AppendLocalization(
							localization,
							property.Value,
							string.IsNullOrEmpty(path) ? property.Key : path + "/" + property.Key);
					}
					break;
				case JsonValueKind.String:
					localization[path] = node.GetValue<string>();
					break;
				default:
					break;
			}
		}
	}
}
