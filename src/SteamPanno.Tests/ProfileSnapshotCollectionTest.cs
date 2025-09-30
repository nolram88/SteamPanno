using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamPanno.panno;
using Xunit;
using Shouldly;

namespace SteamPanno
{
	public class ProfileSnapshotCollectionTest
	{
		private ProfileSnapshotCollection collection;

		public ProfileSnapshotCollectionTest()
		{
			collection = new ProfileSnapshotCollection();
		}

		[Fact]
		public void ShouldInitEmptyCollection()
		{
			collection.GetIncrementalSnapshots().Count().ShouldBe(0);
			collection.GetFullSnapshot(100).ShouldBeNull();
		}

		[Fact]
		public void ShouldInitCollectionWithFirstSnapshot()
		{
			var snapshotIn = new ProfileSnapshot()
			{
				Timestamp = 100,
				Games = new PannoGame[]
				{
					NewPannoGame(1, "game1", 99),
				},
			};
			collection.AddFullSnapshot(snapshotIn);

			var incrementalSnapshots = collection.GetIncrementalSnapshots().ToArray();
			incrementalSnapshots.Length.ShouldBe(1);
			incrementalSnapshots.First().ShouldBe(snapshotIn);

			var fullSnapshot = collection.GetFullSnapshot(100);
			fullSnapshot.ShouldBe(snapshotIn);
		}

		[Fact]
		public void ShouldAddSnapshots()
		{
			var snapshots = new decimal[] { 99, 99.1M, 99.2M }.Select((hours, index) => new ProfileSnapshot()
			{
				Timestamp = 100 + index,
				Games = new PannoGame[]
				{
					new PannoGame()
					{
						Id = 1,
						Name = "game1",
						HoursOnRecord = hours,
					}
				},
			}).ToArray();
			foreach (var snapshot in snapshots)
			{
				collection.AddFullSnapshot(snapshot);
			}

			foreach (var snapshot in snapshots)
			{
				collection.GetFullSnapshot(snapshot.Timestamp).ShouldBe(snapshot);
			}
		}

		[Fact]
		public void ShouldKeepIncrementalSnapshots()
		{
			var snapshots = new ProfileSnapshot[]
			{
				new ProfileSnapshot()
				{
					Timestamp = 100,
					Games = new PannoGame[]
					{
						NewPannoGame(1, "game1", 10),
						NewPannoGame(2, "game2", 20),
					}
				},
				new ProfileSnapshot()
				{
					Timestamp = 101,
					Games = new PannoGame[]
					{
						NewPannoGame(1, "game1", 10),
						NewPannoGame(2, "game2", 30),
					}
				},
				new ProfileSnapshot()
				{
					Timestamp = 102,
					Games = new PannoGame[]
					{
						NewPannoGame(1, "game1", 20),
						NewPannoGame(2, "game2", 30),
					}
				},
			};
			foreach (var snapshot in snapshots)
			{
				collection.AddFullSnapshot(snapshot);
			}

			var incrementalSnapshots = collection.GetIncrementalSnapshots().ToArray();
			incrementalSnapshots.Length.ShouldBe(3);
			incrementalSnapshots.First().ShouldBe(snapshots.First());
			incrementalSnapshots.Skip(1).First().ShouldSatisfyAllConditions(
				s => s.Timestamp.ShouldBe(101),
				s => s.Games.Length.ShouldBe(1),
				s => s.Games.First().Id.ShouldBe(2),
				s => s.Games.First().HoursOnRecord.ShouldBe(30));
			incrementalSnapshots.Skip(2).First().ShouldSatisfyAllConditions(
				s => s.Timestamp.ShouldBe(102),
				s => s.Games.Length.ShouldBe(1),
				s => s.Games.First().Id.ShouldBe(1),
				s => s.Games.First().HoursOnRecord.ShouldBe(20));
		}

		[Fact]
		public void ShouldNotSaveEmptyIncrementalSnapshots()
		{
			var snapshots = new decimal[] { 99, 99, 99 }.Select((hours, index) => new ProfileSnapshot()
			{
				Timestamp = 100 + index,
				Games = new PannoGame[]
				{
					NewPannoGame(1, "game1", hours),
				},
			}).ToArray();
			foreach (var snapshot in snapshots)
			{
				collection.AddFullSnapshot(snapshot);
			}

			var incrementalSnapshots = collection.GetIncrementalSnapshots().ToArray();
			incrementalSnapshots.Length.ShouldBe(1);
			incrementalSnapshots.First().ShouldBe(snapshots.First());
		}

		[Fact]
		public void ShouldSaveNotEmptyIncrementalSnapshots()
		{
			var snapshots = new decimal[] { 99, 99, 100 }.Select((hours, index) => new ProfileSnapshot()
			{
				Timestamp = 100 + index,
				Games = new PannoGame[]
				{
					NewPannoGame(1, "game1", hours),
				},
			}).ToArray();
			foreach (var snapshot in snapshots)
			{
				collection.AddFullSnapshot(snapshot);
			}

			var incrementalSnapshots = collection.GetIncrementalSnapshots().ToArray();
			incrementalSnapshots.Length.ShouldBe(2);
			incrementalSnapshots.First().ShouldBe(snapshots.First());
			incrementalSnapshots.Skip(1).First().ShouldSatisfyAllConditions(
				s => s.Timestamp.ShouldBe(102),
				s => s.Games.Length.ShouldBe(1),
				s => s.Games.First().Id.ShouldBe(1),
				s => s.Games.First().HoursOnRecord.ShouldBe(100));
		}

		private PannoGame NewPannoGame(int id, string name, decimal hours)
		{
			return new PannoGame()
			{
				Id = id,
				Name = name,
				HoursOnRecord = hours,
			};
		}
	}
}
