#if STEAM

using System;
using Godot;
using Steamworks;

namespace SteamPanno.global
{
	public static class Steam
	{
		public static uint appID = 480;

		static Steam()
		{
			SteamClient.RestartAppIfNecessary(appID);
		}

		public static string SteamId
		{
			get => SteamClient.SteamId.ToString();
		}

		public static void Init()
		{
			try
			{
				SteamClient.Init(appID, true);
				GD.Print(SteamClient.Name);
				GD.Print(SteamClient.SteamId);
			}
			catch (Exception e)
			{
				GD.Print(e.Message);
			}
		}
		
		public static void Update()
		{
			SteamClient.RunCallbacks();
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