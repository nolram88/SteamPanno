using Godot;
using SteamPanno.panno;
using System.Linq;
using System.Threading.Tasks;

namespace SteamPanno.scenes
{
	public partial class Panno : Control
	{
		private TextureRect pannoControl;
		private PannoImage pannoImage;

		public override void _Ready()
		{
			pannoControl = GetNode<TextureRect>("./TextureRect");
		}

		public override void _Process(double delta)
		{
			if (pannoImage != null)
			{
				pannoControl.Texture = ImageTexture.CreateFromImage(pannoImage);
				pannoImage = null;
			}
		}

		public async Task Build(PannoNode pannoStructure, PannoLoader loader, PannoDrawer drawer)
		{
			var games = pannoStructure.AllLeaves().ToArray();

			foreach (var game in games)
			{
				var image = game.Horizontal
					? await loader.GetGameLogoH(game.Game.Id)
					: await loader.GetGameLogoV(game.Game.Id);

				if (image != null)
				{
					drawer.Draw(image, game.Area);
				}
				else
				{
					
				}
			}

			pannoImage = drawer.Dest;
		}

		private async Task<PannoImage> Text(string text)
		{
			var view = new SubViewport();
			view.Size = new Vector2I(100, 100);
			view.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
			await ToSignal(GetTree(), "idle_frame");

			return null;
		}
	}
}
