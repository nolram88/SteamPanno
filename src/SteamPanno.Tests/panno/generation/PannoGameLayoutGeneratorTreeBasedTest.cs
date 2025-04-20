using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Xunit;
using Shouldly;

namespace SteamPanno.panno.generation
{
	public abstract class PannoGameLayoutGeneratorTreeBasedTest
	{
		protected readonly PannoGameLayoutGenerator pannoGenerator;

		public PannoGameLayoutGeneratorTreeBasedTest(PannoGameLayoutGenerator pannoGenerator)
		{
			this.pannoGenerator = pannoGenerator;
		}

		[Theory]
		[InlineData(100, 200)]
		[InlineData(200, 100)]
		public async Task ShouldPutOneImageForOneGame(int width, int height)
		{
			var game = new PannoGame();
			var games = new PannoGame[] { game };
			var area = new Rect2I(0, 0, width, height);

			var layout = await pannoGenerator.Generate(games, area);

			layout.Count().ShouldBe(games.Length);
			var node = layout.First();
			node.Game.ShouldBe(game);
			node.Area.ShouldBe(area);
		}

		[Theory]
		[InlineData(100, 200)]
		[InlineData(200, 100)]
		public async Task ShouldPutAllImageForTwoGames(int width, int height)
		{
			var game1 = new PannoGame() { HoursOnRecord = 10 };
			var game2 = new PannoGame() { HoursOnRecord = 5 };
			var games = new PannoGame[] { game1, game2 };
			var area = new Rect2I(0, 0, width, height);
			var horizontal = area.PreferHorizontal();

			var layout = await pannoGenerator.Generate(games, area);

			layout.Count().ShouldBe(games.Length);
			layout.First().Game.HoursOnRecord.ShouldBe(game1.HoursOnRecord);
			layout.First().Area.ShouldBe(new Rect2I(0, 0, 100, 100));
			layout.Last().Game.HoursOnRecord.ShouldBe(game2.HoursOnRecord);
			layout.Last().Area.ShouldBe(horizontal ? new Rect2I(100, 0, 100, 100) : new Rect2I(0, 100, 100, 100));
		}

		[Theory]
		[InlineData(11, 10)]
		[InlineData(10, 11)]
		public async Task ShouldSplitAreaCorretlyForOddSize(int width, int height)
		{
			var game1 = new PannoGame() { HoursOnRecord = 10 };
			var game2 = new PannoGame() { HoursOnRecord = 5 };
			var games = new PannoGame[] { game1, game2 };
			var area = new Rect2I(0, 0, width, height);
			var horizontal = area.PreferHorizontal();

			var layout = await pannoGenerator.Generate(games, area);

			layout.Count().ShouldBe(games.Length);
			layout.First().Area.ShouldBe(horizontal ? new Rect2I(0, 0, 6, 10) : new Rect2I(0, 0, 10, 6));
			layout.Last().Area.ShouldBe(horizontal ? new Rect2I(6, 0, 5, 10) : new Rect2I(0, 6, 10, 5));
		}

		[Theory]
		[InlineData(100, 200)]
		[InlineData(200, 100)]
		public async Task ShouldPutAllImageForThreeGames(int width, int height)
		{
			var game1 = new PannoGame() { HoursOnRecord = 10 };
			var game2 = new PannoGame() { HoursOnRecord = 5 };
			var game3 = new PannoGame() { HoursOnRecord = 1 };
			var games = new PannoGame[] { game1, game2, game3 };
			var area = new Rect2I(0, 0, width, height);

			var layout = await pannoGenerator.Generate(games, area);

			layout.Count().ShouldBe(games.Length);
			layout.First().Area.Area.ShouldBe(
				layout.Skip(1).Sum(x => x.Area.Area));
		}

		[Theory]
		[InlineData(100, 200)]
		[InlineData(200, 100)]
		public async Task ShouldSplitInCaseWhenOneGameHasMoreThanHalfOfHours(int width, int height)
		{
			var game1 = new PannoGame() { HoursOnRecord = 100 };
			var game2 = new PannoGame() { HoursOnRecord = 20 };
			var game3 = new PannoGame() { HoursOnRecord = 10 };
			var games = new PannoGame[] { game1, game2, game3 };
			var area = new Rect2I(0, 0, width, height);

			var layout = await pannoGenerator.Generate(games, area);

			layout.Count().ShouldBe(games.Length);
			layout.Sum(x => x.Area.Area).ShouldBe(width * height);
		}

		[Theory]
		[InlineData(100, 200)]
		[InlineData(200, 100)]
		public async Task ShouldPutAllImageForFiveGames(int width, int height)
		{
			var game1 = new PannoGame() { HoursOnRecord = 100 };
			var game2 = new PannoGame() { HoursOnRecord = 95 };
			var game3 = new PannoGame() { HoursOnRecord = 90 };
			var game4 = new PannoGame() { HoursOnRecord = 85 };
			var game5 = new PannoGame() { HoursOnRecord = 80 };
			var games = new PannoGame[] { game5, game4, game1, game2, game3 };
			var area = new Rect2I(0, 0, width, height);
			var horizontal = area.PreferHorizontal();

			var layout = await pannoGenerator.Generate(games, area);

			layout.Count().ShouldBe(games.Length);
			layout.Sum(x => x.Area.Area).ShouldBe(width * height);
		}

		[Theory]
		[InlineData(100, 200)]
		[InlineData(200, 100)]
		public async Task ShouldSplitAccodringToGameHours(int width, int height)
		{
			var game1 = new PannoGame() { HoursOnRecord = 1000 };
			var game2 = new PannoGame() { HoursOnRecord = 500 };
			var game3 = new PannoGame() { HoursOnRecord = 300 };
			var game4 = new PannoGame() { HoursOnRecord = 200 };
			var games = new PannoGame[] { game1, game2, game3, game4 };
			var area = new Rect2I(0, 0, width, height);

			var layout = await pannoGenerator.Generate(games, area);

			layout.Count().ShouldBe(games.Length);
			layout.Select(x => x.Area.Area).ToArray()
				.ShouldBeEquivalentTo(new int[] { 100 * 100, 100 * 50, 50 * 50, 50 * 50 });
		}

		[Theory]
		[InlineData(100, 200)]
		[InlineData(200, 100)]
		public async Task ShouldSplitAccodringToGameHours2(int width, int height)
		{
			var games = Enumerable.Repeat(new PannoGame() { HoursOnRecord = 100 }, 16).ToArray();
			var area = new Rect2I(0, 0, width, height);

			var layout = await pannoGenerator.Generate(games, area);

			layout.Count().ShouldBe(games.Length);
			layout.Select(x => x.Area.Area)
				.ShouldAllBe(x => x == 50 * 25);
		}

		[Theory]
		[InlineData(100, 200)]
		[InlineData(200, 100)]
		public async Task ShouldSplitAccodringToGameHours3(int width, int height)
		{
			var games = new int[] { 1000, 500, 400, 350, 300, 250, 220, 200, 180, 150, 120, 100 }
				.Select(x => new PannoGame() { HoursOnRecord = x })
				.ToArray();
			var area = new Rect2I(0, 0, width, height);

			var layout = await pannoGenerator.Generate(games, area);

			layout.Count().ShouldBe(games.Length);
			layout.Select(x => x.Area.Area)
				.ShouldNotContain(100 * 100);
		}

		[Theory]
		[InlineData(100, 10, true)]
		[InlineData(10, 100, false)]
		public async Task ShouldRepeatSameSplittingDependingOnArea(
			int width, int height, bool allHorizontal)
		{
			var games = Enumerable.Repeat(new PannoGame() { HoursOnRecord = 100 }, 8).ToArray();
			var area = new Rect2I(0, 0, width, height);

			var layout = await pannoGenerator.Generate(games, area);

			layout.Count().ShouldBe(8);
			layout.All(x => area.PreferHorizontal())
				.ShouldBe(allHorizontal);
		}

		[Fact]
		public async Task ShouldStopSplittingWhenAreaIsTooSmall()
		{
			var game1 = new PannoGame() { HoursOnRecord = 1000 };
			var game2 = new PannoGame() { HoursOnRecord = 500 };
			var game3 = new PannoGame() { HoursOnRecord = 300 };
			var game4 = new PannoGame() { HoursOnRecord = 200 };
			var games = new PannoGame[] { game1, game2, game3, game4 };
			var area = new Rect2I(0, 0, 8, 8);

			var layout = await pannoGenerator.Generate(games, area);

			layout.Count().ShouldBe(3);
			layout.Select(x => x.Area.Area).ToArray()
				.ShouldBeEquivalentTo(new int[] { 4 * 8, 4 * 4, 4 * 4 });
		}

		[Fact]
		public async Task ShouldKeepSizeGoingSmaller()
		{
			var big = 6;
			var medium = 6;
			var small = 6;
			var games = new List<PannoGame>();
			for (int i = 0; i < big; i++)
			{
				games.Add(new PannoGame() { Name = "100", HoursOnRecord = 100 });
			}
			for (int i = 0; i < medium; i++)
			{
				games.Add(new PannoGame() { Name = "50", HoursOnRecord = 50 });
			}
			for (int i = 0; i < small; i++)
			{
				games.Add(new PannoGame() { Name = "20", HoursOnRecord = 20 });
			}
			var area = new Rect2I(0, 0, 1000, 1000);

			var layout = await pannoGenerator.Generate(games.ToArray(), area);

			layout.Count().ShouldBe(big + medium + small);
			var minArea = int.MaxValue;
			foreach (var leaf in layout)
			{
				var leafArea = leaf.Area.Size.X * leaf.Area.Size.Y;
				if (leafArea <= minArea)
				{
					minArea = leafArea;
				}
				else
				{
					Assert.Fail($"{leaf.Area} {leafArea} > {minArea}");
				}
			}
		}
	}
}
