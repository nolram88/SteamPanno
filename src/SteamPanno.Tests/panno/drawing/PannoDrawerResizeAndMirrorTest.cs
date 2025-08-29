namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeAndMirrorTest : PannoDrawerGapFillerTest<PannoDrawerResizeAndMirror>
	{
		protected override PannoDrawerResizeAndMirror CreateDrawer()
		{
			return new PannoDrawerResizeAndMirror()
			{
				Dest = dest,
				Processor = processor,
			};
		}
	}
}
