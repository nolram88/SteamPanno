using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace SteamPanno
{
	public class ProfileStorage
	{
		private HashSet<string> broken = new HashSet<string>();

		public virtual string[] GetProfileList()
		{
			return Directory
				.GetFiles(FileExtensions.GetProfilesPath(), "*.json")
				.Select(x => Path.GetFileNameWithoutExtension(x))
				.ToArray();
		}

		public virtual List<ProfileSnapshot> GetProfileData(string profileId)
		{
			var profileFileName = ProfileFileName(profileId);
			if (File.Exists(profileFileName))
			{
				try
				{
					var snapshotData = File.ReadAllText(profileFileName);
					var snapshots = JsonSerializer.Deserialize<List<ProfileSnapshot>>(snapshotData);
					return snapshots;
				}
				catch
				{
					broken.Add(profileFileName);
				}
			}

			var profileBackupFileName = ProfileBackupFileName(profileId);
			if (File.Exists(profileBackupFileName))
			{
				try
				{
					var snapshotData = File.ReadAllText(profileBackupFileName);
					var snapshots = JsonSerializer.Deserialize<List<ProfileSnapshot>>(snapshotData);
					return snapshots;
				}
				catch
				{
				}
			}

			return null;
		}

		public virtual void SaveProfile(string profileId, ProfileSnapshot[] snapshots)
		{
			if (snapshots == null || snapshots.Length == 0)
			{
				return;
			}

			var profileFileName = ProfileFileName(profileId);
			if (File.Exists(profileFileName) && !broken.Contains(profileFileName))
			{
				var profileBackupFileName = ProfileBackupFileName(profileId);
				File.Copy(profileFileName, profileBackupFileName, true);
			}

			var json = JsonSerializer.Serialize(snapshots);
			File.WriteAllText(profileFileName, json);
		}

		private string ProfileFileName(string profileId)
		{
			return Path.Combine(FileExtensions.GetProfilesPath(), $"{profileId}.json");
		}

		private string ProfileBackupFileName(string profileId)
		{
			return ProfileFileName(profileId) + ".bak";
		}
	}
}
