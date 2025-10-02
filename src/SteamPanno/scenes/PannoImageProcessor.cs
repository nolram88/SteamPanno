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
			public Sprite2D Sprite { get; init; }
			public ShaderMaterial SpriteMaterial { get; init; }
			public (PannoImage Image, string ShaderPath, Dictionary<string, Variant> ShaderParams) Input { get; set; }
			public Channel<PannoImage> Output { get; init; }
		}

		private Channel<ProcessorLine> freeLines = Channel.CreateUnbounded<ProcessorLine>();
		private Channel<ProcessorLine> workLines = Channel.CreateUnbounded<ProcessorLine>();
		private Queue<ProcessorLine> doneLines = new Queue<ProcessorLine>();

		public override void _Ready()
		{
			for (int i = 0; i < SettingsManager.Instance.Settings.MaxDegreeOfParallelism; i++)
			{
				var subViewport = new SubViewport();
				subViewport.RenderTargetClearMode = SubViewport.ClearMode.Always;
				subViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
				subViewport.ProcessMode = ProcessModeEnum.Always;
				var material = new ShaderMaterial();
				var sprite = new Sprite2D();
				sprite.Centered = false;
				sprite.Material = material;
				subViewport.AddChild(sprite);
				AddChild(subViewport);

				freeLines.Writer.TryWrite(new ProcessorLine()
				{
					SubViewport = subViewport,
					Sprite = sprite,
					SpriteMaterial = material,
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
					(PannoImage image, string shaderPath, Dictionary<string, Variant> shaderParams) = workLine.Input;

					var texture = image.CreateTexture();
					workLine.SubViewport.Size = image.Size;
					workLine.Sprite.Texture = texture;
					workLine.SpriteMaterial.Shader = GD.Load<Shader>(shaderPath);
					workLine.SpriteMaterial.SetShaderParameter("src", texture);
					if (shaderParams != null)
					{
						foreach (var shaderParam in shaderParams)
						{
							workLine.SpriteMaterial.SetShaderParameter(shaderParam.Key, shaderParam.Value);
						}
					}
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

		public async Task<PannoImage> Effect(PannoImage src, string path, Dictionary<string, Variant> parameters = null)
		{
			var line = await freeLines.Reader.ReadAsync();
			line.Input = (src, path, parameters);
			await workLines.Writer.WriteAsync(line);

			return await line.Output.Reader.ReadAsync();
		}
	}
}
