using System;
using System.Collections.Generic;
using Godot;
using Steamworks;

namespace SteamPanno
{
	public static class Steam
	{
		public static uint appID = 4026140;

		static Steam()
		{
			try
			{
				SteamClient.Init(appID, true);
				GD.Print($"Steam Name: {SteamClient.Name}, Steam Id: {SteamClient.SteamId}");
			}
			catch (Exception e)
			{
				GD.Print(e.Message);
			}
		}

		public static string GetSteamId()
		{
			return SteamClient.IsValid ?
				SteamClient.SteamId.ToString()
				: null;
		}

		public static (string id, string name)[] GetFriends()
		{
			var friends = new List<(string, string)>();

			try
			{
				if (SteamClient.IsValid)
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
				if (SteamClient.IsValid)
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
				if (SteamClient.IsValid)
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
