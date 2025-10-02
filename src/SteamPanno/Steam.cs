using System;
using System.Collections.Generic;
using Godot;
using Steamworks;

namespace SteamPanno
{
	public static class Steam
	{
		private static uint appID = 4026140;
		private static bool init = false;

		static Steam()
		{
			try
			{
				SteamClient.Init(appID, true);
				init = true;
				GD.Print($"Steam Name: {SteamClient.Name}, Id: {SteamClient.SteamId}, Lang: {SteamApps.GameLanguage}");
			}
			catch (Exception e)
			{
				GD.Print(e.Message);
			}
		}

		public static bool IsReady()
		{
			return init && SteamClient.IsValid;
		}

		public static string GetSteamId()
		{
			return IsReady() ? SteamClient.SteamId.ToString() : null;
		}

		public static string Language
		{
			get => IsReady() ? SteamApps.GameLanguage : null;
		}

		public static (string id, string name)[] GetFriends()
		{
			var friends = new List<(string, string)>();

			try
			{
				if (IsReady())
				{
					foreach (var friend in SteamFriends.GetFriends())
					{
						friends.Add((friend.Id.ToString(), friend.Name));
					}
				}
			}
			catch (Exception e)
			{
				GD.Print(e.Message);
			}
			
			return friends.ToArray();
		}

		public static void SaveScreenshot(byte[] data, int width, int height)
		{
			try
			{
				if (IsReady())
				{
					SteamScreenshots.WriteScreenshot(data, width, height);
				}
			}
			catch (Exception e)
			{
				GD.Print(e.Message);
			}
		}
		
		public static void Shutdown()
		{
			try
			{
				if (IsReady())
				{
					SteamClient.Shutdown();
				}
			}
			catch (Exception e)
			{
				GD.Print(e.Message);
			}
		}
	}
}
