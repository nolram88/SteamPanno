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

			return await Processor.Effect(expansion1, "res://assets/shaders/blur.gdshader");
		}

		protected override async Task<PannoImage> PrepareExpansion2(
			PannoImage src,
			bool xFitting,
			Vector2I isize,
			Vector2I gapSize)
		{
			var expansion2 = await base.PrepareExpansion2(
				src, xFitting, isize, gapSize);

			return await Processor.Effect(expansion2, "res://assets/shaders/blur.gdshader");
		}
	}
}
