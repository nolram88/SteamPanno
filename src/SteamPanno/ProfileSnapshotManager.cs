using System;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace SteamPanno
{
	public class ProfileSnapshotManager
	{
		public static readonly ProfileSnapshotManager Instance = new ProfileSnapshotManager();

		public ProfileSnapshotManager()
		{
			ProfileListResolver = () =>
			{
				var profiles = Directory
					.GetFiles(FileExtensions.GetProfilesPath(), "*.json")
					.Select(x => Path.GetFileNameWithoutExtension(x))
					.ToArray();

				return profiles;
			};

			ProfileDataResolver = (profileId) =>
			{
				var snapshotPath = Path.Combine(FileExtensions.GetProfilesPath(), profileId + ".json");

				if (File.Exists(snapshotPath))
				{
					var snapshotData = File.ReadAllText(snapshotPath);
					var snapshots = JsonSerializer.Deserialize<ProfileSnapshot[]>(snapshotData);
					return snapshots;
				}

				return null;
			};

			SaveProfile = (profileId, snapshots) =>
			{
				if (snapshots == null || snapshots.Length == 0)
				{
					return;
				}

				var fileName = Path.Combine(FileExtensions.GetProfilesPath(), $"{profileId}.json");
				var json = JsonSerializer.Serialize(snapshots);
				File.WriteAllText(fileName, json);
			};
		}

		public Func<string[]> ProfileListResolver { get; set; }
		public Func<string, ProfileSnapshot[]> ProfileDataResolver { get; set; }
		public Action<string, ProfileSnapshot[]> SaveProfile { get; set; }

		public string[] GetProfiles()
		{
			return ProfileListResolver();
		}

		public ProfileSnapshotCollection GetProfile(string profileId)
		{
			
		}
	}
}
