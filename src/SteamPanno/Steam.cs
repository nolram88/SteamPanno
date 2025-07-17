#if STEAM

using System;
using System.Collections.Generic;
using Godot;
using Steamworks;

namespace SteamPanno
{
	public static class Steam
	{
		public static uint appID = 480;

		static Steam()
		{
			try
			{
				SteamClient.RestartAppIfNecessary(appID);
				SteamClient.Init(appID, true);
				GD.Print(SteamClient.Name);
				GD.Print(SteamClient.SteamId);
			}
			catch (Exception e)
			{
				GD.Print(e.Message);
			}
		}

		public static string SteamId
		{
			get => SteamClient.SteamId.ToString();
		}

		public static void Update()
		{
			SteamClient.RunCallbacks();
		}

		public static (string id, string name)[] GetFriends()
		{
			var friends = new List<(string, string)>();
			foreach (var friend in SteamFriends.GetFriends())
			{
				friends.Add((friend.Id.ToString(), friend.Name));
			}

			return friends.ToArray();
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

#endif