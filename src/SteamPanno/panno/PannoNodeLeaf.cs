using Godot;

namespace SteamPanno.panno
{
	public class PannoNodeLeaf : PannoNode
	{
		public PannoImage PannoImage { get; init; }

		public override void Draw(Image image)
		{
			var size = PannoImage.Image.GetSize();
			var rect = new Rect2I(Vector2I.Zero, size);

			image.BlitRect(PannoImage.Image, rect, Vector2I.Zero);
		}

		public override int Count()
		{
			return 1;
		}
	}
}
