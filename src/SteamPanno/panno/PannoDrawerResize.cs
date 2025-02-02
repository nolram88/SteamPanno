using Godot;

namespace SteamPanno.panno
{
	public class PannoDrawerResize : PannoDrawer
	{
		public override void Draw(PannoImage src, Rect2I destArea)
		{
			var position = destArea.Position;
			var size = destArea.Size;

			src.Size = new Vector2I(size.X, size.Y);
			var rect = new Rect2I(Vector2I.Zero, size);

			Dest.Draw(src, rect, position);
		}
	}
}
