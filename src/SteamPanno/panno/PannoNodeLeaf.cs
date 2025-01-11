using Godot;
using System.Collections.Generic;

namespace SteamPanno.panno
{
	public class PannoNodeLeaf : PannoNode
	{
		public PannoImage PannoImage { get; init; }

		public override void Draw(Image image, Rect2I area, bool horizontal)
		{
			var size = PannoImage.Image.GetSize();
			var rect = new Rect2I(Vector2I.Zero, size);

			image.BlitRect(PannoImage.Image, rect, area.Position);
		}

		public override IEnumerable<PannoNode> AllNodes()
		{
			yield return this;
		}

		public override int Count()
		{
			return 1;
		}
	}
}
