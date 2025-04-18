using Godot;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeAndMirror : PannoDrawer
	{
		public override void Draw(PannoImage src, Rect2I destArea)
		{
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

			src.Size = new Vector2I(isize.X, isize.Y);

			var gapSize = new Vector2I(
				sizeXRatio < sizeYRatio ? isize.X : (size.X - isize.X) / 2,
				sizeXRatio < sizeYRatio ? (size.Y - isize.Y) / 2 : isize.Y);

			var expansion1 = Builder(
				sizeXRatio < sizeYRatio ? isize.X : gapSize.X,
				sizeXRatio < sizeYRatio ? gapSize.Y : isize.Y);
			var srcAreaForExpansion1 = new Rect2I(0, 0, expansion1.Size.X, expansion1.Size.Y);
			expansion1.Draw(src, srcAreaForExpansion1, Vector2I.Zero);
			if (sizeXRatio < sizeYRatio)
			{
				expansion1.MirrorY();
			}
			else
			{
				expansion1.MirrorX();
			}

			var expansion2 = Builder(
				sizeXRatio < sizeYRatio ? isize.X : gapSize.X,
				sizeXRatio < sizeYRatio ? gapSize.Y : isize.Y);
			var srcAreaForExpansion2 = new Rect2I(
					sizeXRatio < sizeYRatio ? 0 : isize.X - gapSize.X,
					sizeXRatio < sizeYRatio ? isize.Y - gapSize.Y : 0,
					expansion2.Size.X,
					expansion2.Size.Y);
			expansion2.Draw(src, srcAreaForExpansion2, Vector2I.Zero);
			if (sizeXRatio < sizeYRatio)
			{
				expansion2.MirrorY();
			}
			else
			{
				expansion2.MirrorX();
			}

			var srcArea = new Rect2I(Vector2I.Zero, isize);
			var srcPosition = destArea.Position + gapSize * (sizeXRatio < sizeYRatio ? new Vector2I(0, 1) : new Vector2I(1, 0));
			Dest.Draw(src, srcArea, srcPosition);

			var expansion1Area = new Rect2I(Vector2I.Zero, expansion1.Size);
			var expansion1Position = destArea.Position;
			Dest.Draw(expansion1, expansion1Area, expansion1Position);

			var expansion2Area = new Rect2I(Vector2I.Zero, expansion2.Size);
			var expansion2Position = destArea.Position + new Vector2I(
				sizeXRatio < sizeYRatio ? 0 : gapSize.X + isize.X,
				sizeXRatio < sizeYRatio ? gapSize.Y + isize.Y : 0);
			Dest.Draw(expansion2, expansion2Area, expansion2Position);
		}
	}
}
