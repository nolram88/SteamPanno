using Godot;
using SteamPanno.panno;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SteamPanno.scenes
{
	public partial class PannoImageProcessor : Control, IPannoImageProcessor
	{
		private SubViewport subViewport;
		private Sprite2D spriteIn;
		private ShaderMaterial spriteInMaterial;
		private Channel<PannoImage> imagesIn = Channel.CreateUnbounded<PannoImage>();
		private Queue<SubViewport> subViewportOut = new Queue<SubViewport>();
		private Channel<PannoImage> imagesOut = Channel.CreateUnbounded<PannoImage>();

		public override void _Ready()
		{
			subViewport = GetNode<SubViewport>("./SubViewport");
			
			var blurShader = GD.Load<Shader>("res://assets/shaders/pannoblur.gdshader");
			spriteInMaterial = new ShaderMaterial();
			spriteInMaterial.Shader = blurShader;

			spriteIn = new Sprite2D();
			spriteIn.Centered = false;
			spriteIn.Material = spriteInMaterial;
			subViewport.AddChild(spriteIn);
		}

		public override void _Process(double delta)
		{
			while (subViewportOut.Count > 0)
			{
				var subViewport = subViewportOut.Dequeue();
				var image = subViewport.GetTexture().GetImage();
				image.Convert(Image.Format.Rgb8);
				imagesOut.Writer.TryWrite(PannoImage.Create(image));
			}

			while (imagesIn.Reader.TryRead(out var imageToAdd))
			{
				Texture2D texture = imageToAdd;

				spriteInMaterial.SetShaderParameter("src", texture);
				spriteIn.Texture = texture;
				subViewport.Size = imageToAdd.Size;
				subViewportOut.Enqueue(subViewport);
			}
		}

		public PannoImage Create(int x, int y)
		{
			return PannoImage.Create(x, y);
		}

		public async Task<PannoImage> Blur(PannoImage src)
		{
			return await BlurOnGPU(src);
		}

		private async Task<PannoImage> BlurOnGPU(PannoImage src)
		{
			await imagesIn.Writer.WriteAsync(src);
			return await imagesOut.Reader.ReadAsync();
		}
	}
}
