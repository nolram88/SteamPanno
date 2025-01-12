using Godot;

namespace SteamPanno.panno
{
	public class PannoDrawerResize : PannoDrawer
	{
		public override void Draw(Rect2I area, Image image)
		{
			var position = area.Position;
			var size = area.Size;

			image.Resize(size.X, size.Y, Image.Interpolation.Cubic);
			var rect = new Rect2I(Vector2I.Zero, size);

			Panno.BlitRect(image, rect, position);
		}
	}
}
