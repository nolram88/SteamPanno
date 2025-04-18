using Godot;
using Xunit;
using Shouldly;
using NSubstitute;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeAndMirrorTest
	{
		private readonly PannoDrawerResizeAndMirror drawer;
		private readonly PannoImage dest;

		public PannoDrawerResizeAndMirrorTest()
		{
			dest = Substitute.For<PannoImage>();
			drawer = new PannoDrawerResizeAndMirror()
			{
				Dest = dest,
				Builder = (width, height) =>
				{
					var newImage = Substitute.For<PannoImage>();
					newImage.Size = new Vector2I(width, height);
					return newImage;
				}
			};
		}

		[Theory]
		[InlineData(200, 100)]
		[InlineData(20, 10)]
		public void ShouldResizeImageAndDrawFullAreaWithThreeFragments(int srcWidth, int srcHeight)
		{
			var src = drawer.Builder(srcWidth, srcHeight);

			drawer.Draw(src, new Rect2I(0, 0, 100, 100));

			src.Size.ShouldBe(new Vector2I(100, 50));
			dest.Received(1).Draw(
				Arg.Is<PannoImage>(x => x == src),
				Arg.Is<Rect2I>(x => x == new Rect2I(0, 0, 100, 50)),
				Arg.Is<Vector2I>(x => x == new Vector2I(0, 25)));
			dest.Received(1).Draw(
				Arg.Is<PannoImage>(x => x != src),
				Arg.Is<Rect2I>(x => x == new Rect2I(0, 0, 100, 25)),
				Arg.Is<Vector2I>(x => x == new Vector2I(0, 0)));
			dest.Received(1).Draw(
				Arg.Is<PannoImage>(x => x != src),
				Arg.Is<Rect2I>(x => x == new Rect2I(0, 0, 100, 25)),
				Arg.Is<Vector2I>(x => x == new Vector2I(0, 75)));
		}
	}
}
