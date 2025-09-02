namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeMirrorTest : PannoDrawerGapFillerTest<PannoDrawerResizeMirror>
	{
		protected override PannoDrawerResizeMirror CreateDrawer()
		{
			return new PannoDrawerResizeMirror(dest, processor);
		}
	}
}
