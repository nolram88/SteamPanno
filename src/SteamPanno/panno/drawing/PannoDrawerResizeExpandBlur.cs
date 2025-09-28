using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeExpandBlur : PannoDrawerResizeExpand
	{
		public PannoDrawerResizeExpandBlur(PannoImage dest, IPannoImageProcessor processor)
			: base(dest, processor)
		{
		}

		protected override async Task<PannoImage> PrepareExpansion1(
			PannoImage src,
			bool xFitting,
			Vector2I isize,
			Vector2I gapSize)
		{
			var expansion1 = await base.PrepareExpansion1(
				src, xFitting, isize, gapSize);

			return (gapSize.X > 1 && gapSize.Y > 1)
				? await EdgeBlur(
					expansion1,
					xFitting ? new Vector2(1, 0) : new Vector2(0, 1),
					xFitting ? new Vector2(0, -1) : new Vector2(-1, 0))
				: expansion1;
		}

		protected override async Task<PannoImage> PrepareExpansion2(
			PannoImage src,
			bool xFitting,
			Vector2I isize,
			Vector2I gapSize)
		{
			var expansion2 = await base.PrepareExpansion2(
				src, xFitting, isize, gapSize);

			return (gapSize.X > 1 && gapSize.Y > 1)
				? await EdgeBlur(
					expansion2,
					xFitting ? new Vector2(1, 0) : new Vector2(0, 1),
					xFitting ? new Vector2(0, 1) : new Vector2(1, 0))
				: expansion2;
		}
	}
}
