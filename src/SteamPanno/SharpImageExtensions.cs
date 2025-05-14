using SixLabors.ImageSharp;
using System.IO;
using GodotImage = Godot.Image;

namespace SteamPanno
{
	public static class SharpImageExtensions
	{
		public static GodotImage DecodeJpg(this byte[] buffer)
		{
			using (var stream = new MemoryStream())
			{
				var image = Image.Load(buffer);
				image.SaveAsBmp(stream, new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder()
				{
					SupportTransparency = false,
				});
				stream.Seek(0, SeekOrigin.Begin);

				var godotImage = new GodotImage();
				return (godotImage.LoadBmpFromBuffer(stream.GetBuffer()) == Godot.Error.Ok) ? godotImage : null;
			}
		}
	}
}
