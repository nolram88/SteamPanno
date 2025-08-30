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
		private TextureRect textureIn;
		private ShaderMaterial blurMaterial;
		private Channel<PannoImage> imagesIn = Channel.CreateUnbounded<PannoImage>();
		private Queue<Sprite2D> spritesToRemove = new Queue<Sprite2D>();
		private Channel<PannoImage> imagesOut = Channel.CreateUnbounded<PannoImage>();

		public override void _Ready()
		{
			subViewport = GetNode<SubViewport>("./SubViewport");
			textureIn = GetNode<TextureRect>("./SubViewport/TextureIn");
			var blurShader = GD.Load<Shader>("res://assets/shaders/pannoblur.gdshader");
			blurMaterial = new ShaderMaterial();
			blurMaterial.Shader = blurShader;
			textureIn.Material = blurMaterial;
		}

		public override void _Process(double delta)
		{
			while (spritesToRemove.Count > 0)
			{
				var image = subViewport.GetTexture().GetImage();
				image.Convert(Image.Format.Rgb8);
				imagesOut.Writer.TryWrite(PannoImage.Create(image));

				var sprite = spritesToRemove.Dequeue();
				subViewport.RemoveChild(sprite);
			}

			while (imagesIn.Reader.TryRead(out var imageToAdd))
			{
				Texture2D texture = imageToAdd;

				blurMaterial.SetShaderParameter("src", texture);
				var blurSprite = new Sprite2D();
				blurSprite.Texture = texture;
				blurSprite.Material = blurMaterial;
				blurSprite.Centered = false;
				blurSprite.Visible = true;

				textureIn.Size = imageToAdd.Size;
				textureIn.Texture = texture;
				textureIn.Visible = true;

				subViewport.Size = imageToAdd.Size;
				subViewport.AddChild(blurSprite);

				spritesToRemove.Enqueue(blurSprite);
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
