using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SteamPanno
{
	public static class Localization
	{
		private const string LanguageProperty = "Language";
		private const string LanguageDefault = "english";
		private const string TranslationsPath = "res://assets/translations";
		private const string TranslationsPathAlt = "./custom-assets/translations";

		private readonly static Dictionary<string, Dictionary<string, string>> localizations;
		private static string localizationActive;

		static Localization()
		{
			localizations = new Dictionary<string, Dictionary<string, string>>();
			var filesFromRes = DirAccess.GetFilesAt(TranslationsPath);
			var filesFromDir = System.IO.Directory.Exists(TranslationsPathAlt)
				? DirAccess.GetFilesAt(TranslationsPathAlt)
				: Array.Empty<string>();
			var filesAll = filesFromRes.Concat(filesFromDir).Distinct().ToArray();
			
			foreach (var file in filesAll)
			{
				var filePath = filesFromDir.Contains(file)
					? $"{TranslationsPathAlt}/{file}"
					: $"{TranslationsPath}/{file}";
				var fileLocalization = LoadLocalization(filePath);
				if (fileLocalization.Count > 0 && fileLocalization.TryGetValue(LanguageProperty, out var fileLanguage))
				{
					localizations[System.IO.Path.GetFileNameWithoutExtension(file)] = fileLocalization;
				}
			}

			if (!string.IsNullOrEmpty(SettingsManager.Instance.Settings.Language) &&
				localizations.ContainsKey(SettingsManager.Instance.Settings.Language))
			{
				SetLocalization(SettingsManager.Instance.Settings.Language);
			}
			else if (!string.IsNullOrEmpty(Steam.Language) &&
				localizations.ContainsKey(Steam.Language))
			{
				SetLocalization(Steam.Language);
			}
			else
			{
				SetLocalization(LanguageDefault);
			}
		}

		public static (string Invariant, string Native)[] GetLocalizations()
		{
			return localizations.Select(x => (x.Key, x.Value[LanguageProperty])).ToArray();
		}

		public static string GetLocalization()
		{
			return localizationActive;
		}

		public static void SetLocalization(string lang)
		{
			localizationActive = lang;

			if (SettingsManager.Instance.Settings.Language != localizationActive)
			{
				SettingsManager.Instance.Settings.Language = 
					Steam.Language != localizationActive
						? localizationActive : null;
				SettingsManager.Instance.Save();
			}
		}

		public static void SetLocalization(int index)
		{
			var lang = localizations.Keys.Skip(index).FirstOrDefault();
			if (lang != null)
			{
				SetLocalization(lang);
			}
		}

		public static string Localize(string phrase, params string[] args)
		{
			if (localizations.TryGetValue(localizationActive, out var localization) &&
				localization.TryGetValue(phrase, out var phraseLocalized))
			{
				return args.Length == 0
					? phraseLocalized
					: string.Format(phraseLocalized, args);
			}

			if (localizations.TryGetValue(LanguageDefault, out var localizationDefault) &&
				localizationDefault.TryGetValue(phrase, out var phraseLocalizedDefault))
			{
				return args.Length == 0
					? phraseLocalizedDefault
					: string.Format(phraseLocalizedDefault, args);
			}

			return phrase;
		}

		private static Dictionary<string, string> LoadLocalization(string filePath)
		{
			Dictionary<string, string> localization = new Dictionary<string, string>();

			using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
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
