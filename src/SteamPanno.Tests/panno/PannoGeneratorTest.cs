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
	}
}
