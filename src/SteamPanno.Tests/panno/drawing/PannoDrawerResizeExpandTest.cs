namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeExpandTest : PannoDrawerGapFillerTest<PannoDrawerResizeExpand>
	{
		protected override PannoDrawerResizeExpand CreateDrawer()
		{
			return new PannoDrawerResizeExpand(dest, processor);
		}
	}
}
