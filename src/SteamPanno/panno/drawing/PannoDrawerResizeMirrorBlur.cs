using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno.drawing
{
	public class PannoDrawerResizeMirrorBlur : PannoDrawerResizeMirror
	{
		protected override async Task<PannoImage> PrepareExpansion1(
			PannoImage src,
			bool xFitting,
			Vector2I isize,
			Vector2I gapSize)
		{
			var expansion1 = await base.PrepareExpansion1(
				src, xFitting, isize, gapSize);

			return await EdgeBlur(expansion1);
		}

		protected override async Task<PannoImage> PrepareExpansion2(
			PannoImage src,
			bool xFitting,
			Vector2I isize,
			Vector2I gapSize)
		{
			var expansion2 = await base.PrepareExpansion2(
				src, xFitting, isize, gapSize);

			return await EdgeBlur(expansion2);
		}

		protected async Task<PannoImage> EdgeBlur(PannoImage src)
		{
			return await Processor.Effect(
				src,
				"res://assets/shaders/edgeblur.gdshader",
				new Dictionary<string, Variant>()
				{
					{ "radiusMinX", 0 },
					{ "radiusMaxX", 5 },
					{ "radiusMinY", 0 },
					{ "radiusMaxY", 5 },
				});
		}
	}
}
