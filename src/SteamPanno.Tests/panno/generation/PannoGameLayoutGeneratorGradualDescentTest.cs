using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Shouldly;

namespace SteamPanno.panno.generation
{
	public class PannoGameLayoutGeneratorGradualDescentTest : PannoGameLayoutGeneratorTreeBasedTest
	{
		public PannoGameLayoutGeneratorGradualDescentTest()
			: base(new PannoGameLayoutGeneratorGradualDescent())
		{
		}

		[Fact]
		public async Task ShouldKeepSizeGoingSmaller()
		{
			var big = 10;
			var medium = 10;
			var small = 10;
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
