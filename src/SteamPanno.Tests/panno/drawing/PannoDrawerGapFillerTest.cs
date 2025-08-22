using Godot;
using System.Linq;
using Xunit;
using Shouldly;
using NSubstitute;

namespace SteamPanno.panno.drawing
{
	public abstract class PannoDrawerGapFillerTest<T>
		where T : PannoDrawerGapFiller
	{
		protected readonly T drawer;
		protected readonly PannoImage dest;

		public PannoDrawerGapFillerTest()
		{
			dest = Substitute.For<PannoImage>();
			drawer = CreateDrawer();
		}

		protected abstract T CreateDrawer();

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

		[Theory]
		[InlineData(200, 100)]
		public void ShouldDrawSingleFragmentWhenSizeIsExactMatch(int srcWidth, int srcHeight)
		{
			var src = drawer.Builder(srcWidth, srcHeight);

			drawer.Draw(src, new Rect2I(0, 0, 200, 100));

			src.Size.ShouldBe(new Vector2I(200, 100));
			dest.Received(1).Draw(
				Arg.Is<PannoImage>(x => x == src),
				Arg.Is<Rect2I>(x => x == new Rect2I(0, 0, 200, 100)),
				Arg.Is<Vector2I>(x => x == new Vector2I(0, 0)));
			dest.ReceivedCalls().Count().ShouldBe(1);
		}

		[Theory]
		[InlineData(200, 100)]
		public void ShouldDrawGapsOnlyWhenNecessary(int srcWidth, int srcHeight)
		{
			var src = drawer.Builder(srcWidth, srcHeight);

			drawer.Draw(src, new Rect2I(0, 0, 2, 2));

			src.Size.ShouldBe(new Vector2I(2, 1));
			dest.Received(1).Draw(
				Arg.Is<PannoImage>(x => x == src),
				Arg.Is<Rect2I>(x => x == new Rect2I(0, 0, 2, 1)),
				Arg.Is<Vector2I>(x => x == new Vector2I(0, 0)));
			dest.Received(1).Draw(
				Arg.Is<PannoImage>(x => x != src),
				Arg.Is<Rect2I>(x => x == new Rect2I(0, 0, 2, 1)),
				Arg.Is<Vector2I>(x => x == new Vector2I(0, 1)));
			dest.ReceivedCalls().Count().ShouldBe(2);
		}
	}
}
