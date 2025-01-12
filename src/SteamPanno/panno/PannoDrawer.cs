using Godot;

namespace SteamPanno.panno
{
	public class PannoDrawer
	{
		public Image Panno { get; init; }

		public void Draw(PannoNodeLeaf game, PannoImage image)
		{
			var position = game.Area.Position;
			var area = game.Area;
			var size = image.Image.GetSize();
			var sizeXRatio = (area.Size.X / (float)size.X);
			var sizeYRatio = (area.Size.Y / (float)size.Y);

			if (sizeXRatio < 1 || sizeYRatio < 1)
			{
				if (sizeXRatio < sizeYRatio)
				{
					size = new Vector2I((int)(size.X * sizeXRatio), (int)(size.Y * sizeXRatio));
				}
				else
				{
					size = new Vector2I((int)(size.X * sizeYRatio), (int)(size.Y * sizeYRatio));
				}
			}
			else
			{
				var offsetX = area.Size.X - size.X;
				var offsetY = area.Size.Y - size.Y;

				position.X += offsetX;
				position.Y += offsetY;
			}

			image.Image.Resize(size.X, size.Y, Image.Interpolation.Cubic);
			var rect = new Rect2I(Vector2I.Zero, size);

			Panno.BlitRect(image.Image, rect, position);
		}
	}
}
