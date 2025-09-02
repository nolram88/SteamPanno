using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeExpand : PannoDrawerGapFiller
	{
		public PannoDrawerResizeExpand(PannoImage dest, IPannoImageProcessor processor)
			: base(dest, processor)
		{
		}

		protected override Task<PannoImage> PrepareExpansion1(
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

			return Task.FromResult(expansion1); 
		}

		protected override Task<PannoImage> PrepareExpansion2(
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

			return Task.FromResult(expansion2);
		}
	}
}
