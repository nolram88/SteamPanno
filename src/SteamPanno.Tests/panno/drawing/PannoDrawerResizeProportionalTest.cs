using Godot;
using Xunit;
using Shouldly;
using NSubstitute;
using System.Threading.Tasks;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeProportionalTest
	{
		private readonly PannoDrawerResizeProportional drawer;
		private readonly PannoImage dest;

		public PannoDrawerResizeProportionalTest()
		{
			dest = Substitute.For<PannoImage>();
			drawer = new PannoDrawerResizeProportional(dest, null);
		}

		[Theory]
		[InlineData(200, 100)]
		[InlineData(20, 10)]
		public async Task ShouldResizeImageAndDrawWithOffset(int srcWidth, int srcHeight)
		{
			var src = Substitute.For<PannoImage>();
			src.Size = new Vector2I(srcWidth, srcHeight);

			await drawer.Draw(src, new Rect2I(0, 0, 100, 100));

			src.Size.ShouldBe(new Vector2I(100, 50));
			dest.Received(1).Draw(
				Arg.Is<PannoImage>(x => x == src),
				Arg.Is<Rect2I>(x => x == new Rect2I(0, 0, 100, 50)),
				Arg.Is<Vector2I>(x => x == new Vector2I(0, 25)));
		}
	}
}
