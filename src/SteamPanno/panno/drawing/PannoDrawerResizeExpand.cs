using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeExpand : PannoDrawerGapFiller
	{
		protected override async Task<PannoImage> PrepareExpansion1(
			PannoImage src,
			bool xFitting,
			Vector2I isize,
			Vector2I gapSize)
		{
			var expansion1 = Processor.Create(
				xFitting ? isize.X : 1,
				xFitting ? 1 : isize.Y);
			var srcAreaForExpansion1 = new Rect2I(0, 0, expansion1.Size.X, expansion1.Size.Y);
			expansion1.Draw(src, srcAreaForExpansion1, Vector2I.Zero);
			expansion1.Size = gapSize;

			return await EdgeBlur(
				expansion1,
				xFitting ? new Vector2(1, 0) : new Vector2(0, 1),
				xFitting ? new Vector2(0, -1) : new Vector2(-1, 0));
		}

		protected override async Task<PannoImage> PrepareExpansion2(
			PannoImage src,
			bool xFitting,
			Vector2I isize,
			Vector2I gapSize)
		{
			var expansion2 = Processor.Create(
				xFitting ? isize.X : 1,
				xFitting ? 1 : isize.Y);
			var srcAreaForExpansion2 = new Rect2I(
				xFitting ? 0 : isize.X - 1,
				xFitting ? isize.Y - 1 : 0,
				expansion2.Size.X,
				expansion2.Size.Y);
			expansion2.Draw(src, srcAreaForExpansion2, Vector2I.Zero);
			expansion2.Size = gapSize;

			return await EdgeBlur(
				expansion2,
				xFitting ? new Vector2(1, 0) : new Vector2(0, 1),
				xFitting ? new Vector2(0, 1) : new Vector2(1, 0));
		}
	}
}
