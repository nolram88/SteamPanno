using System;

namespace SteamPanno
{
	public static class DateExtensions
	{
		public static long NewTimestamp()
		{
			return ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
		}

		public static DateTime TimestampToLocalDateTime(long timestamp)
		{
			return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
		}
	}
}
