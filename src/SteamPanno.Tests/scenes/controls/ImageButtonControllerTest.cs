using Xunit;
using Shouldly;
using NSubstitute;

namespace SteamPanno.scenes.controls
{
	public class ImageButtonControllerTest
	{
		private readonly ImageButtonView view;
		private readonly ImageButtonController controller;

		public ImageButtonControllerTest()
		{
			view = Substitute.For<ImageButtonView>();
			controller = new ImageButtonController(view);
		}

		[Fact]
		public void ShouldChangeTransparencyDependingOnHighlightState()
		{
			view.OnFrame(1);
			var state1 = view.Transparency;
			view.OnHighlight(true);
			view.OnFrame(1);
			var state2 = view.Transparency;
			view.OnHighlight(false);
			view.OnFrame(1);
			var state3 = view.Transparency;

			(state1 < state2).ShouldBeTrue();
			(state2 > state3).ShouldBeTrue();
			(state1 == state3).ShouldBeTrue();
		}

		[Fact]
		public void ShouldChangeSizeDependingOnHighlightState()
		{
			view.OnFrame(1);
			var state1 = view.Size;
			view.OnHighlight(true);
			view.OnFrame(1);
			var state2 = view.Size;
			view.OnHighlight(false);
			view.OnFrame(1);
			var state3 = view.Size;

			(state1.LengthSquared() < state2.LengthSquared()).ShouldBeTrue();
			(state2.LengthSquared() > state3.LengthSquared()).ShouldBeTrue();
			(state1.LengthSquared() == state3.LengthSquared()).ShouldBeTrue();
		}

		[Fact]
		public void ShouldCallClickWithDelay()
		{
			var clicks = 0;
			controller.OnClick = () => clicks++;

			view.OnFrame(1);
			view.OnClick();
			var state1 = clicks;
			view.OnFrame(1);
			var state2 = clicks;

			state1.ShouldBe(0);
			state2.ShouldBe(1);
		}
	}
}
