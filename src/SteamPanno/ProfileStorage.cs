using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace SteamPanno
{
	public class ProfileStorage
	{
		public virtual string[] GetProfileList()
		{
			return Directory
				.GetFiles(FileExtensions.GetProfilesPath(), "*.json")
				.Select(x => Path.GetFileNameWithoutExtension(x))
				.ToArray();
		}

		public virtual List<ProfileSnapshot> GetProfileData(string profileId)
		{
			var snapshotPath = Path.Combine(FileExtensions.GetProfilesPath(), profileId + ".json");

			if (File.Exists(snapshotPath))
			{
				var snapshotData = File.ReadAllText(snapshotPath);
				var snapshots = JsonSerializer.Deserialize<List<ProfileSnapshot>>(snapshotData);
				return snapshots;
			}

			return null;
		}

		public virtual void SaveProfile(string profileId, ProfileSnapshot[] snapshots)
		{
			if (snapshots == null || snapshots.Length == 0)
			{
				return;
			}

			var fileName = Path.Combine(FileExtensions.GetProfilesPath(), $"{profileId}.json");
			var json = JsonSerializer.Serialize(snapshots);
			File.WriteAllText(fileName, json);
		}
	}
}
