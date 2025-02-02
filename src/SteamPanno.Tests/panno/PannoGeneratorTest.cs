using System.Linq;
using System.Threading.Tasks;
using Godot;
using Xunit;
using Shouldly;

namespace SteamPanno.panno
{
	public class PannoGeneratorTest
	{
		private readonly PannoGenerator pannoGenerator;
		
		public PannoGeneratorTest()
		{
			pannoGenerator = new PannoGenerator();
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ShouldPutOneImageForOneGame(bool horizontal)
		{
			var game = new PannoGame();
			var games = new PannoGame[] { game };
			var area = new Rect2I(0, 0, 100, 100);

			var panno = await pannoGenerator.Generate(games, area, horizontal);

			panno.Count().ShouldBe(games.Length);
			var node = panno.AllLeaves().First();
			node.Game.ShouldBe(game);
			node.Area.ShouldBe(area);
			node.Horizontal.ShouldBe(horizontal);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ShouldPutAllImageForTwoGames(bool horizontal)
		{
			var game1 = new PannoGame() { HoursOnRecord = 10 };
			var game2 = new PannoGame() { HoursOnRecord = 5 };
			var games = new PannoGame[] { game1, game2 };
			var area = new Rect2I(0, 0, 100, 100);

			var panno = await pannoGenerator.Generate(games, area, horizontal);

			panno.Count().ShouldBe(games.Length);
			var nodes = panno.AllLeaves().ToArray();
			nodes.First().Game.HoursOnRecord.ShouldBe(game1.HoursOnRecord);
			nodes.First().Area.ShouldBe(horizontal ? new Rect2I(0, 0, 50, 100) : new Rect2I(0, 0, 100, 50));
			nodes.First().Horizontal.ShouldBe(!horizontal);
			nodes.Last().Game.HoursOnRecord.ShouldBe(game2.HoursOnRecord);
			nodes.Last().Area.ShouldBe(horizontal ? new Rect2I(50, 0, 50, 100) : new Rect2I(0, 50, 100, 50));
			nodes.Last().Horizontal.ShouldBe(!horizontal);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ShouldSplitAreaCorretlyForOddSize(bool horizontal)
		{
			var game1 = new PannoGame() { HoursOnRecord = 10 };
			var game2 = new PannoGame() { HoursOnRecord = 5 };
			var games = new PannoGame[] { game1, game2 };
			var area = new Rect2I(0, 0, 11, 11);

			var panno = await pannoGenerator.Generate(games, area, horizontal);

			panno.Count().ShouldBe(games.Length);
			var nodes = panno.AllLeaves().ToArray();
			nodes.First().Area.ShouldBe(horizontal ? new Rect2I(0, 0, 6, 11) : new Rect2I(0, 0, 11, 6));
			nodes.Last().Area.ShouldBe(horizontal ? new Rect2I(6, 0, 5, 11) : new Rect2I(0, 6, 11, 5));
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ShouldPutAllImageForThreeGames(bool horizontal)
		{
			var game1 = new PannoGame() { HoursOnRecord = 10 };
			var game2 = new PannoGame() { HoursOnRecord = 5 };
			var game3 = new PannoGame() { HoursOnRecord = 1 };
			var games = new PannoGame[] { game1, game2, game3 };
			var area = new Rect2I(0, 0, 100, 100);

			var panno = await pannoGenerator.Generate(games, area, horizontal);

			panno.Count().ShouldBe(games.Length);
			var nodes = panno.AllLeaves().ToArray();
			nodes.First().Area.Area.ShouldBe(
				nodes.Skip(1).Sum(x => x.Area.Area));
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ShouldSplitInCaseWhenOneGameHasMoreThanHalfOfHours(bool horizontal)
		{
			var game1 = new PannoGame() { HoursOnRecord = 100 };
			var game2 = new PannoGame() { HoursOnRecord = 20 };
			var game3 = new PannoGame() { HoursOnRecord = 10 };
			var games = new PannoGame[] { game1, game2, game3 };
			var area = new Rect2I(0, 0, 100, 100);

			var panno = await pannoGenerator.Generate(games, area, horizontal);

			panno.Count().ShouldBe(games.Length);
			panno.AllLeaves().Sum(x => x.Area.Area).ShouldBe(100 * 100);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ShouldPutAllImageForFiveGames(bool horizontal)
		{
			var game1 = new PannoGame() { HoursOnRecord = 100 };
			var game2 = new PannoGame() { HoursOnRecord = 95 };
			var game3 = new PannoGame() { HoursOnRecord = 90 };
			var game4 = new PannoGame() { HoursOnRecord = 85 };
			var game5 = new PannoGame() { HoursOnRecord = 80 };
			var games = new PannoGame[] { game5, game4, game1, game2, game3 };
			var area = new Rect2I(0, 0, 100, 100);

			var panno = await pannoGenerator.Generate(games, area, horizontal);

			panno.Count().ShouldBe(games.Length);
			var nodes = panno.AllLeaves().ToArray();
			nodes.Sum(x => x.Area.Area).ShouldBe(100 * 100);
			nodes[0].Area.ShouldBe(new Rect2I(0, 0, 50, 50));
			nodes[1].Area.ShouldBe(new Rect2I(horizontal ? 0 : 50, horizontal ? 50 : 0, 50, 50));
			nodes[2].Area.ShouldBe(new Rect2I(horizontal ? 50 : 0, horizontal ? 0 : 50, 50, 50));
			nodes[3].Area.ShouldBe(new Rect2I(50, 50, horizontal ? 25 : 50, horizontal ? 50 : 25));
			nodes[4].Area.ShouldBe(new Rect2I(horizontal ? 75 : 50, horizontal ? 50 : 75, horizontal ? 25 : 50, horizontal ? 50 : 25));
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ShouldSplitAccodringToGameHours(bool horizontal)
		{
			var game1 = new PannoGame() { HoursOnRecord = 1000 };
			var game2 = new PannoGame() { HoursOnRecord = 500 };
			var game3 = new PannoGame() { HoursOnRecord = 300 };
			var game4 = new PannoGame() { HoursOnRecord = 200 };
			var games = new PannoGame[] { game1, game2, game3, game4 };
			var area = new Rect2I(0, 0, 100, 100);

			var panno = await pannoGenerator.Generate(games, area, horizontal);

			panno.Count().ShouldBe(games.Length);
			panno.AllLeaves().Select(x => x.Area.Area).ToArray()
				.ShouldBeEquivalentTo(new int[] { 50 * 100, 50 * 50, 25 * 50, 25 * 50 });
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ShouldSplitAccodringToGameHours2(bool horizontal)
		{
			var games = Enumerable.Repeat(new PannoGame() { HoursOnRecord = 100 }, 16).ToArray();
			var area = new Rect2I(0, 0, 128, 128);

			var panno = await pannoGenerator.Generate(games, area, horizontal);

			panno.Count().ShouldBe(games.Length);
			panno.AllLeaves().Select(x => x.Area.Area)
				.ShouldAllBe(x => x == 32 * 32);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ShouldSplitAccodringToGameHours3(bool horizontal)
		{
			var games = new int[] { 1000, 500, 400, 350, 300, 250, 220, 200, 180, 150, 120, 100 }
				.Select(x => new PannoGame() { HoursOnRecord = x })
				.ToArray();
			var area = new Rect2I(0, 0, 100, 100);

			var panno = await pannoGenerator.Generate(games, area, horizontal);

			panno.Count().ShouldBe(games.Length);
			panno.AllLeaves().Select(x => x.Area.Area)
				.ShouldNotContain(50 * 100);
		}
	}
}
