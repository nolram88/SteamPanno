using Godot;

namespace SteamPanno.panno
{
	public class PannoDrawerResizeProportional : PannoDrawer
	{
		public override void Draw(PannoImage src, Rect2I destArea)
		{
			var position = destArea.Position;
			var size = destArea.Size;
			var isize = src.Size;

			var sizeXRatio = (size.X / (float)isize.X);
			var sizeYRatio = (size.Y / (float)isize.Y);
			if (sizeXRatio < 1 || sizeYRatio < 1)
			{
				if (sizeXRatio < sizeYRatio)
				{
					isize = new Vector2I((int)(isize.X * sizeXRatio), (int)(isize.Y * sizeXRatio));
				}
				else
				{
					isize = new Vector2I((int)(isize.X * sizeYRatio), (int)(isize.Y * sizeYRatio));
				}
			}
			else
			{
				var offsetX = (size.X - isize.X) / 2;
				var offsetY = (size.Y - isize.Y) / 2;

				position.X += offsetX;
				position.Y += offsetY;
			}

			src.Size = new Vector2I(isize.X, isize.Y);
			var rect = new Rect2I(Vector2I.Zero, isize);

			Dest.Draw(src, rect, position);
		}
	}
}
