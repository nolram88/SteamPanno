using System.Linq;
using System.Threading.Tasks;
using Godot;
using Xunit;
using Shouldly;

namespace SteamPanno.panno
{
	public class PannoGeneratorDivideAndConquerTest
	{
		private readonly PannoGeneratorDivideAndConquer pannoGenerator;

		public PannoGeneratorDivideAndConquerTest()
		{
			pannoGenerator = new PannoGeneratorDivideAndConquer();
		}

		[Theory]
		[InlineData(100, 200)]
		[InlineData(200, 100)]
		public async Task ShouldPutOneImageForOneGame(int width, int height)
		{
			var game = new PannoGame();
			var games = new PannoGame[] { game };
			var area = new Rect2I(0, 0, width, height);

			var panno = await pannoGenerator.Generate(games, area);

			panno.Count().ShouldBe(games.Length);
			var node = panno.AllLeaves().First();
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

			var panno = await pannoGenerator.Generate(games, area);

			panno.Count().ShouldBe(games.Length);
			var nodes = panno.AllLeaves().ToArray();
			nodes.First().Game.HoursOnRecord.ShouldBe(game1.HoursOnRecord);
			nodes.First().Area.ShouldBe(new Rect2I(0, 0, 100, 100));
			nodes.Last().Game.HoursOnRecord.ShouldBe(game2.HoursOnRecord);
			nodes.Last().Area.ShouldBe(horizontal ? new Rect2I(100, 0, 100, 100) : new Rect2I(0, 100, 100, 100));
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

			var panno = await pannoGenerator.Generate(games, area);

			panno.Count().ShouldBe(games.Length);
			var nodes = panno.AllLeaves().ToArray();
			nodes.First().Area.ShouldBe(horizontal ? new Rect2I(0, 0, 6, 10) : new Rect2I(0, 0, 10, 6));
			nodes.Last().Area.ShouldBe(horizontal ? new Rect2I(6, 0, 5, 10) : new Rect2I(0, 6, 10, 5));
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

			var panno = await pannoGenerator.Generate(games, area);

			panno.Count().ShouldBe(games.Length);
			var nodes = panno.AllLeaves().ToArray();
			nodes.First().Area.Area.ShouldBe(
				nodes.Skip(1).Sum(x => x.Area.Area));
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

			var panno = await pannoGenerator.Generate(games, area);

			panno.Count().ShouldBe(games.Length);
			panno.AllLeaves().Sum(x => x.Area.Area).ShouldBe(width * height);
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

			var panno = await pannoGenerator.Generate(games, area);

			panno.Count().ShouldBe(games.Length);
			var nodes = panno.AllLeaves().ToArray();
			nodes.Sum(x => x.Area.Area).ShouldBe(width * height);
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

			var panno = await pannoGenerator.Generate(games, area);

			panno.Count().ShouldBe(games.Length);
			panno.AllLeaves().Select(x => x.Area.Area).ToArray()
				.ShouldBeEquivalentTo(new int[] { 100 * 100, 100 * 50, 50 * 50, 50 * 50 });
		}

		[Theory]
		[InlineData(100, 200)]
		[InlineData(200, 100)]
		public async Task ShouldSplitAccodringToGameHours2(int width, int height)
		{
			var games = Enumerable.Repeat(new PannoGame() { HoursOnRecord = 100 }, 16).ToArray();
			var area = new Rect2I(0, 0, width, height);

			var panno = await pannoGenerator.Generate(games, area);

			panno.Count().ShouldBe(games.Length);
			panno.AllLeaves().Select(x => x.Area.Area)
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

			var panno = await pannoGenerator.Generate(games, area);

			panno.Count().ShouldBe(games.Length);
			panno.AllLeaves().Select(x => x.Area.Area)
				.ShouldNotContain(100 * 100);
		}

		[Theory]
		[InlineData(100, 10)]
		[InlineData(10, 100)]
		public async Task ShouldRepeatSameSplittingDependingOnArea(int width, int height)
		{
			var games = Enumerable.Repeat(new PannoGame() { HoursOnRecord = 100 }, 8).ToArray();
			var area = new Rect2I(0, 0, width, height);

			var panno = await pannoGenerator.Generate(games, area);

			panno.Count().ShouldBe(8);
			panno.AllLeaves().Select(x => x.Area.PreferHorizontal()).All(x => area.PreferHorizontal());
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

			var panno = await pannoGenerator.Generate(games, area);

			panno.Count().ShouldBe(3);
			panno.AllLeaves().Select(x => x.Area.Area).ToArray()
				.ShouldBeEquivalentTo(new int[] { 4 * 8, 4 * 4, 4 * 4 });
		}
	}
}
