using SteamPanno.panno;

namespace SteamPanno
{
	public class ProfileSnapshot
	{
		public long Timestamp { get; set; }
		public PannoGame[] Games { get; set; }
	}
}
