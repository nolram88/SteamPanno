using Godot;
using Xunit;
using NSubstitute;
using FluentAssertions;

namespace SteamPanno.panno
{
	public class PannoDrawerResizeTest
	{
		private readonly PannoDrawerResize drawer;
		private readonly PannoImage dest;

		public PannoDrawerResizeTest()
		{
			dest = Substitute.For<PannoImage>();
			drawer = new PannoDrawerResize() { Dest = dest };
		}

		[Theory]
		[InlineData(300, 200)]
		[InlineData(30, 20)]
		public void ShouldResizeImageAndDraw(int srcWidth, int srcHeight)
		{
			dest.Size = new Vector2I(200, 100);
			var src = Substitute.For<PannoImage>();
			src.Size = new Vector2I(srcWidth, srcHeight);

			drawer.Draw(src, new Rect2I(0, 0, 100, 100));

			src.Size.Should().Be(new Vector2I(100, 100));
			dest.Received(1).Draw(
				Arg.Is<PannoImage>(x => x == src),
				Arg.Is<Rect2I>(x => x == new Rect2I(0, 0, 100, 100)),
				Arg.Is<Vector2I>(x => x == Vector2I.Zero));
		}
	}
}
