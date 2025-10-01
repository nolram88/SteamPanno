using SteamPanno.panno;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamPanno
{
	public class ProfileSnapshotCollection
	{
		private List<ProfileSnapshot> incrementalSnapshots;
		private Dictionary<long, ProfileSnapshot> fullSnapshots;

		public ProfileSnapshotCollection(
			List<ProfileSnapshot> incrementalSnapshots = null)
		{
			this.incrementalSnapshots = incrementalSnapshots ?? new List<ProfileSnapshot>();
			this.fullSnapshots = new Dictionary<long, ProfileSnapshot>();
			var firstSnapshot = this.incrementalSnapshots.FirstOrDefault();
			if (firstSnapshot != null)
			{
				fullSnapshots.Add(firstSnapshot.Timestamp, firstSnapshot);
			}
		}

		public long? GetLastSnapshotDate()
		{
			return incrementalSnapshots.Count > 0
				? incrementalSnapshots.Last().Timestamp
				: null;
		}

		public ProfileSnapshot GetLastSnapshot()
		{
			return incrementalSnapshots.Count > 0
				? incrementalSnapshots.Last()
				: null;
		}

		public IEnumerable<ProfileSnapshot> GetIncrementalSnapshots()
		{
			return incrementalSnapshots;
		}

		public ProfileSnapshot GetFullSnapshot(long timestamp)
		{
			if (incrementalSnapshots.Count() == 0)
			{
				return null;
			}
			if (fullSnapshots.TryGetValue(timestamp, out ProfileSnapshot fullSnapshot))
			{
				return fullSnapshot;
			}

			var fullSnapshotGames = new List<PannoGame>();
			foreach (var incrementalSnapshot in incrementalSnapshots)
			{
				if (incrementalSnapshot.Timestamp > timestamp)
				{
					break;
				}

				IncrementGames(fullSnapshotGames, incrementalSnapshot.Games);
			}

			fullSnapshot = new ProfileSnapshot()
			{
				Timestamp = timestamp,
				Games = fullSnapshotGames.ToArray(),
			};
			fullSnapshots.Add(timestamp, fullSnapshot);

			return fullSnapshot;
		}

		public bool AddFullSnapshot(ProfileSnapshot fullSnapshot)
		{
			if (incrementalSnapshots.Count == 0)
			{
				incrementalSnapshots.Add(fullSnapshot);
				fullSnapshots.Add(fullSnapshot.Timestamp, fullSnapshot);
				return true;
			}

			var lastIncrementalSnapshot = incrementalSnapshots.Last();
			var lastIncrementalSnapshotTimestamp = lastIncrementalSnapshot.Timestamp;
			var lastFullSnapshot = GetFullSnapshot(lastIncrementalSnapshotTimestamp);
			var newIncrementalSnapshot = CreateIncrementalSnapshot(lastFullSnapshot, fullSnapshot);

			if (newIncrementalSnapshot.Games.Length > 0)
			{
				fullSnapshots.Add(fullSnapshot.Timestamp, fullSnapshot);
				incrementalSnapshots.Add(newIncrementalSnapshot);
				return true;
			}

			return false;
		}

		private void IncrementGames(List<PannoGame> full, PannoGame[] incremental)
		{
			if (incremental.Length == 0)
			{
				return;
			}

			foreach (var incGame in incremental)
			{
				var fullGame = full.FirstOrDefault(x => x.Id == incGame.Id);
				if (fullGame != null)
				{
					fullGame.HoursOnRecord = incGame.HoursOnRecord;
				}
				else
				{
					full.Add(new PannoGame()
					{
						Id = incGame.Id,
						Name = incGame.Name,
						HoursOnRecord = incGame.HoursOnRecord,
					});
				}
			}
		}

		private ProfileSnapshot CreateIncrementalSnapshot(ProfileSnapshot prev, ProfileSnapshot next)
		{
			var incrementalSnapshotGames = next.Games
				.Select(nextGame =>
				{
					var prevGame = prev.Games.SingleOrDefault(x => x.Id == nextGame.Id);
					if (prevGame == null ||
						prevGame.HoursOnRecord < nextGame.HoursOnRecord)
					{
						return new PannoGame()
						{
							Id = nextGame.Id,
							Name = nextGame.Name,
							HoursOnRecord = nextGame.HoursOnRecord,
						};
					}

					return null;
				}).Where(x => x != null).ToArray();

			var incrementalSnapshot = new ProfileSnapshot()
			{
				Timestamp = next.Timestamp,
				Games = incrementalSnapshotGames,
			};

			return incrementalSnapshot;
		}
	}
}
