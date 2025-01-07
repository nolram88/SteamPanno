using Godot;

namespace SteamPanno.panno
{
	public class PannoNodeLeaf : PannoNode
	{
		public PannoImage PannoImage { get; init; }

		public override void Draw(Image image)
		{
			image.BlitRect(PannoImage.Image, new Rect2I(Vector2I.Zero, PannoImage.Image.GetSize()), Vector2I.Zero);
		}

		public override int Count()
		{
			return 1;
		}
	}
}
