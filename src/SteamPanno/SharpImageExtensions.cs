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
				image.SaveAsBmp(stream);
				stream.Seek(0, SeekOrigin.Begin);

				var godotImage = new GodotImage();
				var result = godotImage.LoadBmpFromBuffer(stream.GetBuffer());

				if (result == Godot.Error.Ok)
				{
					if (godotImage.GetFormat() != GodotImage.Format.Rgb8)
					{
						godotImage.Convert(GodotImage.Format.Rgb8);
					}

					return godotImage;
				}

				return null;
			}
		}
	}
}
