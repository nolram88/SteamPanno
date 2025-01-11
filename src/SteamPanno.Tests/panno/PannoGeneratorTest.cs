using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		[Fact]
		public async Task ShouldPutOneImageForOneGame()
		{
			var games = new PannoGame[]
			{
				new PannoGame()
			};

			var panno = await pannoGenerator.Generate(games, true);

			panno.Count().Should().Be(1);
		}

		[Fact]
		public async Task ShouldPutAllImageForTwoGame()
		{
			var games = new PannoGame[]
			{
				new PannoGame()
				{
					HoursOnRecord = 10,
				},
				new PannoGame()
				{
					HoursOnRecord = 10,
				}
			};

			var panno = await pannoGenerator.Generate(games, true);

			panno.Count().Should().Be(2);
		}

		[Fact]
		public async Task ShouldPutAllImageForThreeGame()
		{
			var games = new PannoGame[]
			{
				new PannoGame()
				{
					HoursOnRecord = 10,
				},
				new PannoGame()
				{
					HoursOnRecord = 10,
				},
				new PannoGame()
				{
					HoursOnRecord = 10,
				}
			};

			var panno = await pannoGenerator.Generate(games, true);

			panno.Count().Should().Be(3);
		}

		[Fact]
		public async Task ShouldSplitInCaseWhenOneGameHasMoreThanHalfOfHours()
		{
			var games = new PannoGame[]
			{
				new PannoGame()
				{
					HoursOnRecord = 100,
				},
				new PannoGame()
				{
					HoursOnRecord = 20,
				},
				new PannoGame()
				{
					HoursOnRecord = 10,
				}
			};

			var panno = await pannoGenerator.Generate(games, true);

			panno.Count().Should().Be(3);
		}
	}
}
