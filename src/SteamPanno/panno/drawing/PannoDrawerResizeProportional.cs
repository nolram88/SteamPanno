using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeProportional : PannoDrawer
	{
		public PannoDrawerResizeProportional(PannoImage dest, IPannoImageProcessor processor)
			: base(dest, processor)
		{
		}

		public override Task Draw(PannoImage src, Rect2I destArea)
		{
			var position = destArea.Position;
			var size = destArea.Size;
			var isize = src.Size;

			var sizeXRatio = size.X / (float)isize.X;
			var sizeYRatio = size.Y / (float)isize.Y;
			if (sizeXRatio != 1 || sizeYRatio != 1)
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

			position.X += (size.X - isize.X) / 2;
			position.Y += (size.Y - isize.Y) / 2;

			src.Size = new Vector2I(isize.X, isize.Y);
			var rect = new Rect2I(Vector2I.Zero, isize);

			Dest.Draw(src, rect, position);

			return Task.CompletedTask;
		}
	}
}
