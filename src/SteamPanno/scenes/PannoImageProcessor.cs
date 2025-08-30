using Godot;
using SteamPanno.panno;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SteamPanno.scenes
{
	public partial class PannoImageProcessor : Control, IPannoImageProcessor
	{
		private record ProcessorLine
		{
			public SubViewport SubViewport { get; init; }
			public Sprite2D SpriteIn { get; init; }
			public ShaderMaterial SpriteInMaterial { get; init; }
			public PannoImage Input { get; set; }
			public Channel<PannoImage> Output { get; init; }
		}

		private Channel<ProcessorLine> freeLines = Channel.CreateUnbounded<ProcessorLine>();
		private Channel<ProcessorLine> workLines = Channel.CreateUnbounded<ProcessorLine>();
		private Queue<ProcessorLine> doneLines = new Queue<ProcessorLine>();

		public override void _Ready()
		{
			var blurShader = GD.Load<Shader>("res://assets/shaders/pannoblur.gdshader");

			for (int i = 0; i < Settings.Instance.MaxDegreeOfParallelism; i++)
			{
				var subViewport = new SubViewport();
				subViewport.RenderTargetClearMode = SubViewport.ClearMode.Always;
				subViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
				subViewport.ProcessMode = ProcessModeEnum.Always;
				var material = new ShaderMaterial();
				material.Shader = blurShader;
				var spriteIn = new Sprite2D();
				spriteIn.Centered = false;
				spriteIn.Material = material;
				subViewport.AddChild(spriteIn);
				AddChild(subViewport);

				freeLines.Writer.TryWrite(new ProcessorLine()
				{
					SubViewport = subViewport,
					SpriteIn = spriteIn,
					SpriteInMaterial = material,
					Input = null,
					Output = Channel.CreateUnbounded<PannoImage>(),
				});
			}
		}

		public override void _Process(double delta)
		{
			while (doneLines.Count > 0)
			{
				var doneLine = doneLines.Dequeue();
				try
				{
					var image = doneLine.SubViewport.GetTexture().GetImage();
					image.Convert(Image.Format.Rgb8);
					doneLine.Output.Writer.TryWrite(PannoImage.Create(image));
				}
				finally
				{
					freeLines.Writer.TryWrite(doneLine);
				}
			}

			while (workLines.Reader.TryRead(out var workLine))
			{
				try
				{
					Texture2D texture = workLine.Input;
					workLine.SpriteInMaterial.SetShaderParameter("src", texture);
					workLine.SpriteIn.Texture = texture;
					workLine.SubViewport.Size = workLine.Input.Size;
				}
				finally
				{
					doneLines.Enqueue(workLine);
				}
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
			var line = await freeLines.Reader.ReadAsync();
			line.Input = src;
			await workLines.Writer.WriteAsync(line);

			return await line.Output.Reader.ReadAsync();
		}
	}
}
