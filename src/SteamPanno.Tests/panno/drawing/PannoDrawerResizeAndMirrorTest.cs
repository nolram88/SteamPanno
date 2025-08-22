using Godot;
using NSubstitute;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeAndMirrorTest : PannoDrawerGapFillerTest<PannoDrawerResizeAndMirror>
	{
		protected override PannoDrawerResizeAndMirror CreateDrawer()
		{
			return new PannoDrawerResizeAndMirror()
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
