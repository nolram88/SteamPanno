using System.Linq;
using System.Threading.Tasks;
using Godot;
using Xunit;
using FluentAssertions;
using NSubstitute;

namespace SteamPanno.panno
{
	public class PannoGeneratorTest
	{
		private readonly PannoGenerator pannoGenerator;
		private readonly PannoLoader pannoLoader;

		public PannoGeneratorTest()
		{
			pannoLoader = Substitute.For<PannoLoader>();
			pannoLoader.GetGameLogoV(Arg.Any<int>()).Returns(new PannoImage());
			pannoLoader.GetGameLogoH(Arg.Any<int>()).Returns(new PannoImage());
			pannoGenerator = new PannoGenerator(pannoLoader);
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

			panno.Count().Should().Be(1);
			var node = panno.AllLeaves().First();
			node.Game.Should().Be(game);
			node.Area.Should().Be(area);
			node.Horizontal.Should().Be(horizontal);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ShouldPutAllImageForTwoGame(bool horizontal)
		{
			var game1 = new PannoGame() { HoursOnRecord = 10 };
			var game2 = new PannoGame() { HoursOnRecord = 5 };
			var games = new PannoGame[] { game1, game2 };
			var area = new Rect2I(0, 0, 100, 100);

			var panno = await pannoGenerator.Generate(games, area, horizontal);

			panno.Count().Should().Be(2);
			var nodes = panno.AllLeaves();
			nodes.First().Game.HoursOnRecord.Should().Be(game1.HoursOnRecord);
			nodes.First().Area.Should().Be(horizontal ? new Rect2I(0, 0, 50, 100) : new Rect2I(0, 0, 100, 50));
			nodes.First().Horizontal.Should().Be(!horizontal);
			nodes.Last().Game.HoursOnRecord.Should().Be(game2.HoursOnRecord);
			nodes.Last().Area.Should().Be(horizontal ? new Rect2I(50, 0, 50, 100) : new Rect2I(0, 50, 100, 50));
			nodes.Last().Horizontal.Should().Be(!horizontal);
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

			panno.Count().Should().Be(2);
			var nodes = panno.AllLeaves();
			nodes.First().Area.Should().Be(horizontal ? new Rect2I(0, 0, 6, 11) : new Rect2I(0, 0, 11, 6));
			nodes.Last().Area.Should().Be(horizontal ? new Rect2I(6, 0, 5, 11) : new Rect2I(0, 6, 11, 5));
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ShouldPutAllImageForThreeGame(bool horizontal)
		{
			var game1 = new PannoGame() { HoursOnRecord = 10 };
			var game2 = new PannoGame() { HoursOnRecord = 5 };
			var game3 = new PannoGame() { HoursOnRecord = 1 };
			var games = new PannoGame[] { game1, game2, game3 };
			var area = new Rect2I(0, 0, 100, 100);

			var panno = await pannoGenerator.Generate(games, area, horizontal);

			panno.Count().Should().Be(3);
			var nodes = panno.AllLeaves();
			nodes.First().Area.Area.Should().Be(
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

			panno.Count().Should().Be(3);
			panno.AllLeaves().Sum(x => x.Area.Area).Should().Be(100 * 100);
		}
	}
}
