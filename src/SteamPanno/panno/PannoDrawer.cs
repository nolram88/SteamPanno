using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno
{
	public abstract class PannoDrawer
	{
		public PannoDrawer(PannoImage dest, IPannoImageProcessor processor)
		{
			Dest = dest;
			Processor = processor;
		}

		public PannoImage Dest { get; init; }
		public IPannoImageProcessor Processor { get; init; }

		public abstract Task Draw(PannoImage src, Rect2I destArea);
	}
}
