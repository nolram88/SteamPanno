using Godot;
using System.Collections.Generic;

namespace SteamPanno.panno
{
	public class PannoNodeLeaf : PannoNode
	{
		public PannoGame Game { get; init; }
		public Rect2I Area { get; init; }
		public bool Horizontal { get; init; }
		/*
		public override void Draw(Image image, Rect2I area, bool horizontal)
		{
			var position = area.Position;
			var size = PannoImage.Image.GetSize();
			var sizeXRatio = (area.Size.X / (float)size.X);
			var sizeYRatio = (area.Size.Y / (float)size.Y);

			if (sizeXRatio < 1 || sizeYRatio < 1)
			{
				if (sizeXRatio < sizeYRatio)
				{
					size = new Vector2I((int)(size.X * sizeXRatio), (int)(size.Y * sizeXRatio));
				}
				else
				{
					size = new Vector2I((int)(size.X * sizeYRatio), (int)(size.Y * sizeYRatio));
				}
			}
			else
			{
				var offsetX = area.Size.X - size.X;
				var offsetY = area.Size.Y - size.Y;

				position.X += offsetX;
				position.Y += offsetY;
			}

			PannoImage.Image.Resize(size.X, size.Y, Image.Interpolation.Cubic);
			var rect = new Rect2I(Vector2I.Zero, size);

			image.BlitRect(PannoImage.Image, rect, position);
		}*/

		public override IEnumerable<PannoNodeLeaf> AllLeaves()
		{
			yield return this;
		}

		public override int Count()
		{
			return 1;
		}
	}
}
