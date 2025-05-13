using Godot;
using SixLabors.ImageSharp;
using System.IO;
using SharpImage = SixLabors.ImageSharp.Image;

namespace SteamPanno
{
	public static class Extensions
	{
		public static bool PreferHorizontal(this Rect2I area)
		{
			return area.Size.X > area.Size.Y;
		}

		// godot jpg decoder has problems with some files
		// so we use alternative decoder
		public static MemoryStream ToBmpStream(this byte[] buffer)
		{
			var stream = new MemoryStream();
			var image = SharpImage.Load(buffer);
			image.SaveAsBmp(stream, new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder()
			{
				SupportTransparency = false,
			});
			stream.Seek(0, SeekOrigin.Begin);

			return stream;
		}
	}
}
