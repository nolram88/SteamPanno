using Godot;
using NSubstitute;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeAndExpandTest : PannoDrawerGapFillerTest<PannoDrawerResizeAndExpand>
	{
		protected override PannoDrawerResizeAndExpand CreateDrawer()
		{
			return new PannoDrawerResizeAndExpand()
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
	}
}
