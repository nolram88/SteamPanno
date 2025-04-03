using Godot;

namespace SteamPanno.panno
{
	public class PannoImage
	{
		protected Image Dest { get; init; }

		protected PannoImage()
		{
		}

		public static PannoImage Create(int width, int height)
		{
			return new PannoImage()
			{
				Dest = Image.CreateEmpty(width, height, false, Image.Format.Rgb8),
			};
		}

		public static PannoImage Create(Image image)
		{
			return new PannoImage()
			{
				Dest = image,
			};
		}

		public static PannoImage Load(string file)
		{
			var image = new Image();
			return (image.Load(file) == Error.Ok) ? PannoImage.Create(image) : null;
		}

		public static PannoImage Load(byte[] buffer)
		{
			var image = new Image();
			return (image.LoadJpgFromBuffer(buffer) == Error.Ok) ? PannoImage.Create(image) : null;
		}

		public static implicit operator Image(PannoImage image)
		{
			return image.Dest;
		}

		public virtual Vector2I Size
		{
			get => Dest.GetSize();
			set => Dest.Resize(value.X, value.Y, Image.Interpolation.Cubic);
		}

		public virtual void MirrorX()
		{
			this.Dest.FlipX();
		}

		public virtual void MirrorY()
		{
			this.Dest.FlipY();
		}

		public virtual void Draw(PannoImage src, Rect2I srcArea, Vector2I dstPosition)
		{
			this.Dest.BlitRect(src, srcArea, dstPosition);
		}

		public virtual bool SaveJpg(string file)
		{
			return Dest.SaveJpg(file) == Error.Ok;
		}

		public virtual bool SavePng(string file)
		{
			return Dest.SavePng(file) == Error.Ok;
		}
	}
}
