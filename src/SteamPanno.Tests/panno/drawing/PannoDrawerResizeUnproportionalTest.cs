using Godot;
using Xunit;
using Shouldly;
using NSubstitute;
using System.Threading.Tasks;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeUnproportionalTest
	{
		private readonly PannoDrawerResizeUnproportional drawer;
		private readonly PannoImage dest;

		public PannoDrawerResizeUnproportionalTest()
		{
			dest = Substitute.For<PannoImage>();
			drawer = new PannoDrawerResizeUnproportional() { Dest = dest };
		}

		[Theory]
		[InlineData(300, 200)]
		[InlineData(30, 20)]
		public async Task ShouldResizeImageAndDraw(int srcWidth, int srcHeight)
		{
			var src = Substitute.For<PannoImage>();
			src.Size = new Vector2I(srcWidth, srcHeight);

			await drawer.Draw(src, new Rect2I(0, 0, 100, 100));

			src.Size.ShouldBe(new Vector2I(100, 100));
			dest.Received(1).Draw(
				Arg.Is<PannoImage>(x => x == src),
				Arg.Is<Rect2I>(x => x == new Rect2I(0, 0, 100, 100)),
				Arg.Is<Vector2I>(x => x == Vector2I.Zero));
		}
	}
}
