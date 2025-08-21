using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SteamPanno
{
	public static class Localization
	{
		private const string LanguageProperty = "Language";
		private const string LanguageDefault = "English";
		private const string TranslationsPath = "res://assets/translations";

		private readonly static Dictionary<string, Dictionary<string, string>> localizations;
		
		static Localization()
		{
			localizations = new Dictionary<string, Dictionary<string, string>>();
			var files = DirAccess.GetFilesAt(TranslationsPath);
			foreach (var file in files)
			{
				var filePath = $"{TranslationsPath}/{file}";
				var fileLocalization = LoadLocalization(filePath);
				if (fileLocalization.Count > 0 && fileLocalization.TryGetValue(LanguageProperty, out var fileLanguage))
				{
					localizations[fileLanguage] = fileLocalization;
				}
			}

			if (string.IsNullOrEmpty(Settings.Instance.Language) ||
				!localizations.ContainsKey(Settings.Instance.Language))
			{
				SetLocalization(LanguageDefault);
			}
		}

		public static string[] GetLocalizations()
		{
			return localizations.Select(x => x.Value[LanguageProperty]).ToArray();
		}

		public static string GetLocalization()
		{
			return Settings.Instance.Language;
		}

		public static void SetLocalization(string lang)
		{
			Settings.Instance.Language = lang;
			Settings.Save();
		}

		public static void SetLocalization(int index)
		{
			var localization = localizations.Keys.Skip(index).FirstOrDefault();
			if (localization != null)
			{
				SetLocalization(localizations[localization][LanguageProperty]);
			}
		}

		public static string Localize(string phrase)
		{
			if (localizations.TryGetValue(Settings.Instance.Language, out var localization) &&
				localization.TryGetValue(phrase, out var phraseLocalized))
			{
				return phraseLocalized;
			}

			if (localizations.TryGetValue(LanguageDefault, out var localizationDefault) &&
				localizationDefault.TryGetValue(phrase, out var phraseLocalizedDefault))
			{
				return phraseLocalizedDefault;
			}

			return phrase;
		}

		private static Dictionary<string, string> LoadLocalization(string filePath)
		{
			Dictionary<string, string> localization = new Dictionary<string, string>();

			using FileAccess file = FileAccess.Open(filePath,FileAccess.ModeFlags.Read);
			{
				if (file != null)
				{
					var json = file.GetAsText();
					JsonNode node = JsonNode.Parse(json);
					AppendLocalization(localization, node, string.Empty);
				}
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
