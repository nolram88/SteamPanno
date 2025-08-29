namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeAndExpandTest : PannoDrawerGapFillerTest<PannoDrawerResizeAndExpand>
	{
		protected override PannoDrawerResizeAndExpand CreateDrawer()
		{
			return new PannoDrawerResizeAndExpand()
			{
				Dest = dest,
				Processor = processor,
			};
		}
	}
}
