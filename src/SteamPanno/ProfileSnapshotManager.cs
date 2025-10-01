using System.Linq;
using System.Collections.Generic;

namespace SteamPanno
{
	public class ProfileSnapshotManager
	{
		public static readonly ProfileSnapshotManager Instance = new ProfileSnapshotManager(new ProfileStorage());

		private ProfileStorage storage;
		private Dictionary<string, ProfileSnapshotCollection> profiles = new Dictionary<string, ProfileSnapshotCollection>();

		public ProfileSnapshotManager(ProfileStorage storage)
		{
			this.storage = storage;
		}

		public string[] GetProfileList()
		{
			return storage.GetProfileList();
		}

		public ProfileSnapshotCollection GetProfile(string profileId)
		{
			if (!profiles.TryGetValue(profileId, out var profile))
			{
				var data = storage.GetProfileData(profileId);
				if (data != null)
				{
					profile = new ProfileSnapshotCollection(data);
					profiles.Add(profileId, profile);
				}
			}

			return profile;
		}

		public void UpdateProfile(string profileId, ProfileSnapshot snapshot)
		{
			var profile = GetProfile(profileId);
			if (profile == null)
			{
				profile = new ProfileSnapshotCollection();
				profiles.Add(profileId, profile);
			}

			if (profile.AddFullSnapshot(snapshot))
			{
				storage.SaveProfile(profileId, profile.GetIncrementalSnapshots().ToArray());
			}
		}
	}
}
