using Godot;
using System.Text.RegularExpressions;

namespace SteamPanno
{
	public static class StringExtensions
	{
		public static bool TryParseSteamId(this string input, out string steamId)
		{
			if (!string.IsNullOrEmpty(input))
			{
				var steamIdRegex = new Regex(@"(?:7656119\d{10}|STEAM_[01]:[01]:\d+|\[?[A-Z]:[01]:\d+\]?|U:1:\d+)");
				var match = steamIdRegex.Match(input);

				if (match.Success)
				{
					steamId = match.Groups[0].Value;
					return true;
				}
			}

			steamId = null;
			return false;
		}

		public static bool TryParseResolution(this string input, out Vector2I resolution)
		{
			if (!string.IsNullOrEmpty(input))
			{
				var resRegex = new Regex(@"(\d{2,5})\D+(\d{2,5})");
				var match = resRegex.Match(input);
				if (match.Success)
				{
					var x = int.TryParse(match.Groups[1].Value, out var xv) ? xv : 0;
					var y = int.TryParse(match.Groups[2].Value, out var yv) ? yv : 0;

					resolution = new Vector2I(x, y);
					return true;
				}
			}

			resolution = Vector2I.Zero;
			return false;
		}

		public static bool TryParseProfileSnapshotFileName(
			this string input,
			out string steamId,
			out string date)
		{
			Match match = Regex.Match(input ?? "", @"^(?<id>\d+)_(?<date>\d{4}-\d{2}-\d{2})\.json$");

			steamId = match.Success ? match.Groups["id"].Value : null;
			date = match.Success ? match.Groups["date"].Value : null;

			return match.Success;
		}
	}
}
