using Godot;

namespace SteamPanno.panno
{
	public class PannoDrawerResizeProportional : PannoDrawer
	{
		public override void Draw(Rect2I area, Image image)
		{
			var position = area.Position;
			var size = area.Size;

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

			image.Resize(size.X, size.Y, Image.Interpolation.Cubic);
			var rect = new Rect2I(Vector2I.Zero, size);

			Panno.BlitRect(image, rect, position);
		}
	}
}
