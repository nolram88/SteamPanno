using System.Collections.Generic;
using SteamPanno.panno;

namespace SteamPanno
{
	public class ProfileSnapshot
	{
		public long Timestamp { get; init; }
		public IReadOnlyList<PannoGame> Games { get; init; }
	}
}
