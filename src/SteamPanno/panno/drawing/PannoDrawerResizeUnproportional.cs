using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeUnproportional : PannoDrawer
	{
		public PannoDrawerResizeUnproportional(PannoImage dest, IPannoImageProcessor processor)
			: base(dest, processor)
		{
		}

		public override Task Draw(PannoImage src, Rect2I destArea)
		{
			var position = destArea.Position;
			var size = destArea.Size;

			src.Size = new Vector2I(size.X, size.Y);
			var rect = new Rect2I(Vector2I.Zero, size);

			Dest.Draw(src, rect, position);

			return Task.CompletedTask;
		}
	}
}
