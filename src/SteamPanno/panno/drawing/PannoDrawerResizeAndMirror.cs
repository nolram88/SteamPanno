using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeAndMirror : PannoDrawerGapFiller
	{
		protected override async Task<PannoImage> PrepareExpansion1(
			PannoImage src,
			float sizeXRatio,
			float sizeYRatio,
			Vector2I isize,
			Vector2I gapSize)
		{
			var expansion1 = Processor.Create(
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

			return await Processor.Blur(expansion1);
		}

		protected override async Task<PannoImage> PrepareExpansion2(
			PannoImage src,
			float sizeXRatio,
			float sizeYRatio,
			Vector2I isize,
			Vector2I gapSize)
		{
			var expansion2 = Processor.Create(
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

			return await Processor.Blur(expansion2);
		}
	}
}
